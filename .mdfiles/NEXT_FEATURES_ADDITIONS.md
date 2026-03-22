Additional Next Features Additions
=================================

This file contains additions requested to be merged into `NEXT_FEATURES.md`.

- VoCore linking
  - Integrate the `SlevinthHeavenEliteDangerous.LibusbK` wrapper as a fallback for VoCore screens when WinUSB is not available or libusbK is required.
  - Implement native bindings for control transfers (display on/off) and bulk frame writes, expose configuration to prefer libusbK or WinUSB, and add robust device diagnostics.

- System Tray Icon
  - Provide a minimize-to-tray workflow for the desktop app. Use a native `Shell_NotifyIcon` helper (message-only window) rather than bringing in WinForms to keep the WinUI project clean.
  - Tray menu items: `Open` (restore window) and `Exit` (graceful shutdown). Make the behaviour configurable.

- Visited-systems: when bodies == stars
  - Note: simply comparing `Bodies.Count` to an expected star/body count can be misleading. Update processing to take into account:
    - `FSSDiscoveryScan` events (`BodyCount`/`NonBodyCount`) which report discovery results from the FSS scanner and may indicate discovered bodies without per-body `Scan` events.
    - `Scan` events where `Type == "AutoScan"` (automatic FSS/AutoScan results) — these should count as discovered bodies.
  - Implementation hints: extend `JournalLineProcessor` to record `FSSDiscoveryScan` totals per system and to treat `Scan(Type=="AutoScan")` as a scanned body. Use these sources when deciding whether a system is "discovered" or fully scanned.

Merge notes
-----------
To avoid conflicts, copy the relevant bullets from this file into `NEXT_FEATURES.md` under the "Quick Wins" or appropriate section. Alternatively I can re-run and directly update `NEXT_FEATURES.md` if you want me to.
