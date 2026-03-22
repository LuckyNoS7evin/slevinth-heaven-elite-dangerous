using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System;
using System.Diagnostics;

namespace SlevinthHeavenEliteDangerous.VoCore;

/// <summary>
/// Sends RGB565 framebuffers to the VoCore Screen via LibUsbDotNet bulk transfer.
///
/// Prerequisites:
///   LibUsbDotNet 2.x requires WinUSB or LibUSB-win32 (libusb0.sys) — it does NOT support LibUSB-K.
///   If Device Manager shows the display using libusbK, replace it with WinUSB via Zadig:
///     1. Download Zadig from https://zadig.akeo.ie/
///     2. Options → List All Devices
///     3. Select the display device (USB2.0 Screen / VID 0xC872 PID 0x1004)
///     4. Set the driver to WinUSB and click Replace Driver
///   After switching, the device will be enumerable by LibUsbDotNet.
///   Edit Documents\SlevinthHeavenEliteDangerous\vocore_settings.json to change VendorId/ProductId
///   if the defaults do not match your device.
///
/// Protocol (captured from official VoCore Screen test app via Wireshark/USBPcap):
///   All control requests use bmRequestType=0x40 (vendor, host-to-device), bRequest=0xB0.
///
///   First frame only — send display-on command (2 bytes):
///     [0x00, 0x2C] → wLength=2, data: [0x00, 0x29]
///     (0x29 = MIPI DCS Set Display On)
///
///   Every frame — send write-memory-start + size (6 bytes):
///     bRequest=0xB0, wLength=6, data: [0x00, 0x2C, size_lo, size_mid, size_hi, 0x00]
///     where size = Width * Height * 2 in little-endian (e.g. 819840 = 0x0C8280 → 0x80, 0x82, 0x0C)
///     (0x2C = MIPI DCS Write Memory Start)
///
///   Then bulk-write the full RGB565 frame in a single transfer to endpoint 0x02.
///   SkiaSharp renders RGB565 in little-endian byte order — set SwapByteOrder = true in
///   settings if colours appear corrupted.
/// </summary>
internal sealed class UsbDisplayWriter : IDisposable
{
    private readonly int  _vendorId;
    private readonly int  _productId;
    private readonly bool _swapByteOrder;
    private readonly bool _skipWindowCommand;
    private readonly int  _columnOffset;

    public readonly int Width;
    public readonly int Height;
    public readonly int FrameSize; // Width * Height * 2 (RGB565)

    private UsbDevice?         _device;
    private UsbEndpointWriter? _writer;
    private bool               _firstFrame = true;
    private bool               _disposed;

    public UsbDisplayWriter(VoCoreSettings settings)
    {
        _vendorId          = settings.VendorId;
        _productId         = settings.ProductId;
        _swapByteOrder     = settings.SwapByteOrder;
        _skipWindowCommand = settings.SkipWindowCommand;
        _columnOffset      = settings.ColumnOffset;
        Width              = settings.Width;
        Height             = settings.Height;
        FrameSize          = Width * Height * 2;
    }

    /// <summary>
    /// Writes a 854x480 RGB565 frame to the display in chunks.
    /// Silently no-ops if the device is not connected or the write fails.
    /// </summary>
    public void WriteFrame(byte[] frameData)
    {
        if (_disposed || frameData.Length != FrameSize) return;

        try
        {
            EnsureConnected();
            if (_writer == null) return;

            if (!_skipWindowCommand && !SendWindowCommands())
            {
                Debug.WriteLine("[VoCore] Window command failed — skipping frame");
                Reset();
                return;
            }

            if (_swapByteOrder)
                SwapByteOrder(frameData);

            CircularShiftRows(frameData);

            var ec = _writer.Write(frameData, 0, frameData.Length, 5000, out int written);

            CircularShiftRows(frameData); // restore caller's buffer

            if (_swapByteOrder)
                SwapByteOrder(frameData); // restore caller's buffer

            if (ec != ErrorCode.None && ec != ErrorCode.Success)
            {
                Debug.WriteLine($"[VoCore] Bulk write error: {ec}");
                Reset();
                return;
            }

            Debug.WriteLine($"[VoCore] Frame sent: {written} bytes");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoCore] Write failed: {ex.Message} — will retry next frame");
            Reset();
        }
    }

    private void EnsureConnected()
    {
        if (_device != null && _writer != null) return;

        var finder = new UsbDeviceFinder(_vendorId, _productId);
        _device = UsbDevice.OpenUsbDevice(finder);

        if (_device == null)
        {
            Debug.WriteLine("[VoCore] Device not found — is WinUSB driver installed? (use Zadig)");
            return;
        }

        // Required for WinUSB/libusb-win32 whole-device access.
        if (_device is IUsbDevice whole)
        {
            whole.SetConfiguration(1);
            whole.ClaimInterface(0);
        }

        InitializeDisplay();

        // Log all available endpoints so we can verify Ep02 is correct.
        foreach (UsbConfigInfo config in _device.Configs)
        {
            foreach (UsbInterfaceInfo iface in config.InterfaceInfoList)
            {
                Debug.WriteLine($"[VoCore] Interface {iface.Descriptor.InterfaceID} class={iface.Descriptor.Class}");
                foreach (UsbEndpointInfo ep in iface.EndpointInfoList)
                {
                    string dir  = (ep.Descriptor.EndpointID & 0x80) != 0 ? "IN" : "OUT";
                    int    addr = ep.Descriptor.EndpointID & 0x0F;
                    int    type = ep.Descriptor.Attributes & 0x03;
                    string typeName = type switch { 1 => "Isoch", 2 => "Bulk", 3 => "Interrupt", _ => "Control" };
                    Debug.WriteLine($"[VoCore]   Endpoint 0x{ep.Descriptor.EndpointID:X2} ({dir} {typeName}) addr={addr} maxPacket={ep.Descriptor.MaxPacketSize}");
                }
            }
        }

        // Some devices send a "ready" status on the interrupt IN endpoint before
        // they will accept bulk data. Read it (non-blocking) to clear any pending status.
        try
        {
            using var reader = _device.OpenEndpointReader(ReadEndpointID.Ep01);
            byte[] statusBuf = new byte[64];
            reader.Read(statusBuf, 200, out int bytesRead);
            if (bytesRead > 0)
                Debug.WriteLine($"[VoCore] Interrupt IN read: {bytesRead} bytes: {BitConverter.ToString(statusBuf, 0, bytesRead)}");
        }
        catch { /* not all devices use interrupt IN for handshaking */ }

        _writer = _device.OpenEndpointWriter(WriteEndpointID.Ep02);

        // Reset the pipe in case the endpoint is halted from a previous session.
        _writer.Reset();

        Debug.WriteLine("[VoCore] Device opened successfully");
    }

    /// <summary>
    /// One-time display initialization sequence captured from the official VoCore Screen test app.
    /// Sets the display window, orientation and other controller parameters.
    /// Without this the framebuffer origin is offset (appears ~half-way across the screen).
    /// </summary>
    private void InitializeDisplay()
    {
        void Out(byte req, short wValue, short wIndex)
        {
            var setup = new UsbSetupPacket(0x40, req, wValue, wIndex, 0);
            _device!.ControlTransfer(ref setup, Array.Empty<byte>(), 0, out _);
        }

        void In(byte req, short wValue, short wIndex, int length)
        {
            var buf   = new byte[length];
            var setup = new UsbSetupPacket(0xC0, req, wValue, wIndex, (short)length);
            _device!.ControlTransfer(ref setup, buf, length, out _);
        }

        In (0x5F, 0x0000,                   0, 7);   // get display info (response ignored)
        Out(0xA1, 0x0000,                   0);       // init display
        Out(0x9A, 0x1312, unchecked((short)0xD982));  // set window / orientation
        Out(0x9A, 0x0F2C, 0x0007);                   // set window / orientation
        In (0x95, 0x2518,                   0, 2);    // read status (ignored)
        In (0x95, 0x0706,                   0, 2);    // read status (ignored)
        Out(0x9A, 0x2727,                   0);       // commit

        Debug.WriteLine("[VoCore] Display initialized");
    }

    /// <summary>
    /// Sends the vendor control requests required before each bulk frame transfer.
    /// Protocol captured from the official VoCore Screen test app:
    ///   - First frame only: bRequest=0xB0, data=[0x00, 0x29] (MIPI DCS Set Display On)
    ///   - Every frame:      bRequest=0xB0, data=[0x00, 0x2C, size_lo, size_mid, size_hi, 0x00]
    ///                       (MIPI DCS Write Memory Start + frame byte count in little-endian)
    /// </summary>
    private bool SendWindowCommands()
    {
        if (_firstFrame)
        {
            byte[] displayOn = [0x00, 0x29];
            var    setup     = new UsbSetupPacket(0x40, 0xB0, 0, 0, displayOn.Length);
            bool   ok        = _device!.ControlTransfer(ref setup, displayOn, displayOn.Length, out int transferred);
            Debug.WriteLine($"[VoCore] Display-on cmd: ok={ok} transferred={transferred}");
            if (!ok) return false;
            _firstFrame = false;
        }

        byte[] writeMemory =
        [
            0x00, 0x2C,
            (byte)( FrameSize        & 0xFF),
            (byte)((FrameSize >>  8) & 0xFF),
            (byte)((FrameSize >> 16) & 0xFF),
            0x00,
        ];

        var  writeSetup = new UsbSetupPacket(0x40, 0xB0, 0, 0, writeMemory.Length);
        bool writeOk    = _device!.ControlTransfer(ref writeSetup, writeMemory, writeMemory.Length, out int writeTransferred);
        Debug.WriteLine($"[VoCore] Write-memory cmd: ok={writeOk} transferred={writeTransferred}");
        return writeOk;
    }

    private static void SwapByteOrder(byte[] data)
    {
        for (int i = 0; i < data.Length - 1; i += 2)
            (data[i], data[i + 1]) = (data[i + 1], data[i]);
    }

    /// <summary>
    /// Circular-shifts each row left by half the row width in pixels (Width/2).
    /// Counteracts the display's built-in framebuffer offset of Width/2 columns.
    /// Each row is Width*2 bytes; swapping the two halves achieves the shift.
    /// </summary>
    private void CircularShiftRows(byte[] data)
    {
        if (_columnOffset <= 0 || _columnOffset >= Width) return;

        int rowBytes    = Width * 2;
        int shiftBytes  = _columnOffset * 2;
        var tmp         = new byte[shiftBytes];

        for (int row = 0; row < Height; row++)
        {
            int start = row * rowBytes;
            Buffer.BlockCopy(data, start,            tmp,  0,           shiftBytes);
            Buffer.BlockCopy(data, start + shiftBytes, data, start,     rowBytes - shiftBytes);
            Buffer.BlockCopy(tmp,  0,                data, start + rowBytes - shiftBytes, shiftBytes);
        }
    }

    private void Reset()
    {
        _writer     = null;
        _firstFrame = true;
        try { _device?.Close(); } catch { }
        _device = null;
    }

    /// <summary>
    /// Attempt to send MIPI DCS Display Off then close device handles.
    /// </summary>
    public bool TrySendDisplayOff(int timeoutMs = 200)
    {
        try
        {
            // Ensure device handle is available (best-effort)
            EnsureConnected();
            if (_device == null) return false;

            byte[] displayOff = new byte[] { 0x00, 0x28 };
            var setup = new UsbSetupPacket(0x40, 0xB0, 0, 0, displayOff.Length);
            bool ok = _device.ControlTransfer(ref setup, displayOff, displayOff.Length, out int transferred);
            Debug.WriteLine($"[VoCore] Display-off cmd: ok={ok} transferred={transferred}");

            // Give the device a moment to process the command before closing handles
            try { System.Threading.Thread.Sleep(Math.Min(500, Math.Max(50, timeoutMs))); } catch { }

            return ok;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoCore] Display-off failed: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            // Best-effort: tell the display to turn off before closing device handles so it doesn't show the last frame
            try { TrySendDisplayOff(200); } catch { }
        }
        catch { }

        Reset();
        UsbDevice.Exit();
    }
}
