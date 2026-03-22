# Data Integrity Fix - Threading and Save Issues

## Problems Identified

### Issue 1: Missing Data in Saves
Bodies were not being saved because:
1. Service created body and raised event
2. Service called `_manager.ScheduleSave()` **immediately**
3. ViewModel received event and queued body addition on dispatcher
4. Save executed **before** body was actually added
5. Saved data was missing bodies!

### Issue 2: UI Not Updating
Properties weren't updating correctly because:
1. FSDJump set `LastVisitTimestamp` on background thread
2. PropertyChanged fired on background thread
3. WinUI binding system received notification on wrong thread
4. UI didn't update or threw COMException

---

## Solution: Two-Phase Update Pattern

### Phase 1: Data Integrity (Background Thread - Service)
- Update internal data structures immediately
- Register bodies in dictionary (`RegisterBody`)
- Update timestamps silently (`UpdateLastVisitTimestampSilent`)
- Schedule save with complete data

### Phase 2: UI Notification (UI Thread - ViewModel)
- Add bodies to ObservableCollection (`AddBodyToUI`)
- Trigger PropertyChanged notifications (`NotifyLastVisitChanged`)
- Move items in collections

---

## Implementation Details

### VisitedSystemCard Changes

```csharp
// NEW: Register body in dictionary only (thread-safe)
public void RegisterBody(BodyCard body)
{
    _bodiesDict[body.BodyID] = body;
}

// NEW: Add to ObservableCollection only (must be on UI thread)
public void AddBodyToUI(BodyCard body)
{
    Bodies.Add(body);
}

// NEW: Update timestamp without PropertyChanged (thread-safe)
public void UpdateLastVisitTimestampSilent(DateTime timestamp)
{
    _lastVisitTimestamp = timestamp;
}

// NEW: Trigger PropertyChanged manually (must be on UI thread)
public void NotifyLastVisitChanged()
{
    OnPropertyChanged(nameof(LastVisitTimestamp));
    OnPropertyChanged(nameof(LastVisitFormatted));
}

// EXISTING: Combined method (must be on UI thread)
public void AddBody(BodyCard body)
{
    Bodies.Add(body);
    _bodiesDict[body.BodyID] = body;
}
```

### Service Pattern (Background Thread)

```csharp
// For new bodies
var bodyCard = new BodyCard { /* ... */ };

// Phase 1: Register immediately for data integrity
existingCard.RegisterBody(bodyCard);

// Signal UI to add to collection
BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, bodyCard, isNewBody: true));

// Save now has complete data!
_manager.ScheduleSave();
```

```csharp
// For FSDJump timestamp updates
// Phase 1: Update timestamp silently for data integrity
existingCard.UpdateLastVisitTimestampSilent(evt.Timestamp);

// Signal UI to notify property changed
SystemUIUpdateRequested?.Invoke(this, new SystemUIUpdateEventArgs(existingCard, false, needsTimestampNotification: true));

// Save now has correct timestamp!
_manager.ScheduleSave();
```

### ViewModel Pattern (UI Thread)

```csharp
private void OnBodyUIUpdateRequested(object? sender, BodyUIUpdateEventArgs e)
{
    _dispatcherQueue.TryEnqueue(() =>  // ← UI thread
    {
        if (e.IsNewBody && e.Body != null)
        {
            // Phase 2: Add to UI collection
            e.System.AddBodyToUI(e.Body);
        }
    });
}

private void OnSystemUIUpdateRequested(object? sender, SystemUIUpdateEventArgs e)
{
    _dispatcherQueue.TryEnqueue(() =>  // ← UI thread
    {
        if (!e.IsNew && e.NeedsTimestampNotification)
        {
            // Phase 2: Trigger PropertyChanged
            e.System.NotifyLastVisitChanged();
        }
        
        // Move in collection
        if (displayIndex >= 0)
        {
            VisitedSystems.Move(displayIndex, 0);
        }
    });
}
```

---

## Event Args Updates

### BodyUIUpdateEventArgs
```csharp
public class BodyUIUpdateEventArgs : EventArgs
{
    public VisitedSystemCard System { get; }
    public BodyCard? Body { get; }
    public bool IsNewBody { get; }  // Tells ViewModel to add to UI collection
}
```

### SystemUIUpdateEventArgs
```csharp
public class SystemUIUpdateEventArgs : EventArgs
{
    public VisitedSystemCard System { get; }
    public bool IsNew { get; }
    public bool NeedsTimestampNotification { get; }  // Tells ViewModel to trigger PropertyChanged
}
```

---

## Data Flow Diagrams

### New Body Addition Flow
```
Background Thread (Service):
1. Create BodyCard
2. existingCard.RegisterBody(bodyCard)      ← Data saved immediately
3. Raise BodyUIUpdateRequested(isNewBody: true)
4. _manager.ScheduleSave()                  ← Body is in data!
5. Save to disk                             ← Complete data ✅

UI Thread (ViewModel):
6. Receive event on background thread
7. Queue on DispatcherQueue
8. Execute on UI thread:
   - e.System.AddBodyToUI(e.Body)           ← UI updates ✅
```

### FSDJump Timestamp Update Flow
```
Background Thread (Service):
1. existingCard.UpdateLastVisitTimestampSilent(timestamp)  ← Data updated immediately
2. Raise SystemUIUpdateRequested(needsTimestampNotification: true)
3. _manager.ScheduleSave()                                 ← Timestamp is in data!
4. Save to disk                                            ← Complete data ✅

UI Thread (ViewModel):
5. Receive event on background thread
6. Queue on DispatcherQueue
7. Execute on UI thread:
   - e.System.NotifyLastVisitChanged()                     ← UI updates ✅
   - VisitedSystems.Move(...)                              ← Collection updates ✅
```

---

## Fixed Issues

✅ **Data Integrity**: Bodies and timestamps are registered/updated before save  
✅ **UI Updates**: All PropertyChanged notifications happen on UI thread  
✅ **Thread Safety**: No COMExceptions from cross-thread access  
✅ **Performance**: UI updates happen async without blocking service  

---

## Testing Checklist

- [x] Scan new body → Body appears in UI and is saved
- [x] Scan existing body → Properties update in UI and are saved
- [x] FSSBodySignals new body → Body with signals appears in UI and is saved
- [x] FSSBodySignals existing body → Signals update in UI and are saved
- [x] FSDJump to new system → System appears in UI and is saved
- [x] FSDJump to existing system → Timestamp updates in UI and is saved, system moves to top
- [x] No COMExceptions during any operations
- [x] All data persists across app restarts

---

## Key Principles

1. **Data First**: Always update internal data structures immediately (phase 1)
2. **UI Second**: Queue UI updates on DispatcherQueue (phase 2)
3. **Save After Data**: Schedule saves only after data is complete
4. **Separate Concerns**: Service handles data, ViewModel handles UI

This ensures both data integrity AND thread-safe UI updates! 🎯
