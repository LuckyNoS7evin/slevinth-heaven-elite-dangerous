using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace SlevinthHeavenEliteDangerous.VoCore;

public static class UsbDeviceDetector
{
    public static bool IsDevicePresent()
    {
        try
        {
            var settings = VoCoreSettings.Load();
            var finder = new UsbDeviceFinder(settings.VendorId, settings.ProductId);
            var device = UsbDevice.OpenUsbDevice(finder);
            if (device != null)
            {
                try
                {
                    device.Close();
                }
                catch { }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VoCore] Device detection error: {ex.Message}");
            return false;
        }
    }
}
