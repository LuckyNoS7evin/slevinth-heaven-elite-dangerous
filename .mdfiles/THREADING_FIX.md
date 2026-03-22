# Threading Issues Fix - ObservableCollection and INotifyPropertyChanged

## Problem: COMException on FSDJump and Body Events

### Root Cause
When game events (FSDJump, Scan, FSSBodySignals) are processed, they occur on **background threads** (FileListener thread). The services were:
1. Modifying `VisitedSystemCard` properties directly (e.g., `LastVisitTimestamp`)
2. Adding bodies with `existingCard.AddBody(bodyCard)`

Since these cards were already in UI-bound `ObservableCollection<VisitedSystemCard>`, any property changes or collection modifications triggered UI updates from the background thread, causing:
```
Exception thrown: 'System.Runtime.InteropServices.COMException' in System.Private.CoreLib.dll
```

---

## The Threading Problem

### What Was Happening (BAD) ❌
```
Background Thread (FileListener)
    ↓
Service.HandleEvent(FSDJumpEvent)
    ↓
existingCard.LastVisitTimestamp = evt.Timestamp  ← Fires INotifyPropertyChanged
    ↓
UI tries to update binding from background thread
    ↓
COMException!
```

### Also Happening (BAD) ❌
```
Background Thread (FileListener)
    ↓
Service.HandleEvent(ScanEvent)
    ↓
existingCard.AddBody(bodyCard)  ← Modifies ObservableCollection<BodyCard>
    ↓
ObservableCollection.CollectionChanged event fires
    ↓
UI tries to update from background thread
    ↓
COMException!
```

---

## The Solution

### Principle
**NEVER modify UI-bound data from background threads.**

All modifications to:
- Properties that fire `INotifyPropertyChanged`
- `ObservableCollection<T>` items

Must happen on the **UI thread** via `DispatcherQueue`.

---

## Fixes Applied

### Fix 1: Body Addition (Scan and FSSBodySignals Events)

**Before:**
```csharp
// In Service (background thread)
var bodyCard = new BodyCard { /* ... */ };
existingCard.AddBody(bodyCard);  // ❌ Modifies ObservableCollection on background thread
BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, bodyCard));
```

**After:**
```csharp
// In Service (background thread)
var bodyCard = new BodyCard { /* ... */ };
// Don't add here - just signal the ViewModel
BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, bodyCard, isNewBody: true));

// In ViewModel (UI thread)
private void OnBodyUIUpdateRequested(object? sender, BodyUIUpdateEventArgs e)
{
    _dispatcherQueue.TryEnqueue(() =>  // ✅ Ensures UI thread
    {
        if (e.IsNewBody && e.Body != null)
        {
            e.System.AddBody(e.Body);  // Now safe!
        }
    });
}
```

### Fix 2: Timestamp Update (FSDJump Events)

**Before:**
```csharp
// In Service (background thread)
existingCard.LastVisitTimestamp = evt.Timestamp;  // ❌ Fires INotifyPropertyChanged on background thread
SystemUIUpdateRequested?.Invoke(this, new SystemUIUpdateEventArgs(existingCard, false));
```

**After:**
```csharp
// In Service (background thread)
// Don't update property here - pass timestamp to ViewModel
SystemUIUpdateRequested?.Invoke(this, new SystemUIUpdateEventArgs(existingCard, false, evt.Timestamp));

// In ViewModel (UI thread)
private void OnSystemUIUpdateRequested(object? sender, SystemUIUpdateEventArgs e)
{
    _dispatcherQueue.TryEnqueue(() =>  // ✅ Ensures UI thread
    {
        if (!e.IsNew && e.NewLastVisitTimestamp.HasValue)
        {
            e.System.LastVisitTimestamp = e.NewLastVisitTimestamp.Value;  // Now safe!
        }
        // ... move in collection
    });
}
```

---

## Updated Event Args

### BodyUIUpdateEventArgs
```csharp
public class BodyUIUpdateEventArgs : EventArgs
{
    public VisitedSystemCard System { get; }
    public BodyCard? Body { get; }
    public bool IsNewBody { get; }  // ← Added

    public BodyUIUpdateEventArgs(VisitedSystemCard system, BodyCard? body, bool isNewBody = false)
    {
        System = system;
        Body = body;
        IsNewBody = isNewBody;
    }
}
```

### SystemUIUpdateEventArgs
```csharp
public class SystemUIUpdateEventArgs : EventArgs
{
    public VisitedSystemCard System { get; }
    public bool IsNew { get; }
    public DateTime? NewLastVisitTimestamp { get; }  // ← Added

    public SystemUIUpdateEventArgs(VisitedSystemCard system, bool isNew, DateTime? newLastVisitTimestamp = null)
    {
        System = system;
        IsNew = isNew;
        NewLastVisitTimestamp = newLastVisitTimestamp;
    }
}
```

---

## Thread Safety Rules

### ✅ SAFE: Service Layer (Background Thread)
- Read model properties
- Create new model objects
- Modify internal collections (List, Dictionary)
- Call methods without side effects
- Raise events

### ❌ UNSAFE: Service Layer (Background Thread)
- Set properties that fire INotifyPropertyChanged on UI-bound objects
- Modify ObservableCollection<T>
- Call methods that modify ObservableCollection<T>
- Direct UI access

### ✅ SAFE: ViewModel Layer (Via DispatcherQueue)
```csharp
_dispatcherQueue.TryEnqueue(() =>
{
    // Set properties on UI-bound models
    card.LastVisitTimestamp = newValue;
    
    // Modify ObservableCollections
    ObservableCollection.Add(item);
    ObservableCollection.Remove(item);
    ObservableCollection.Move(oldIndex, newIndex);
});
```

---

## Pattern Summary

### Service Responsibilities
1. Process events (on background thread)
2. Create/update non-UI-bound data
3. Raise domain events with necessary data
4. **NEVER touch UI-bound properties/collections**

### ViewModel Responsibilities
1. Subscribe to service events
2. Receive updates via event args
3. Apply updates on UI thread via DispatcherQueue
4. Modify UI-bound properties and collections

---

## Example: Complete Flow

```
1. FSDJumpEvent received (background thread)
   ↓
2. VisitedSystemsService.HandleFSDJumpEvent()
   - DON'T: existingCard.LastVisitTimestamp = evt.Timestamp ❌
   - DO: Raise event with timestamp in args ✅
   ↓
3. SystemUIUpdateRequested event fired
   ↓
4. VisitedSystemsViewModel.OnSystemUIUpdateRequested()
   - Receives event (still on background thread)
   ↓
5. _dispatcherQueue.TryEnqueue(() => { ... })
   - Switches to UI thread ✅
   ↓
6. Inside TryEnqueue lambda (UI thread):
   - existingCard.LastVisitTimestamp = e.NewLastVisitTimestamp ✅
   - VisitedSystems.Move(oldIndex, 0) ✅
   ↓
7. UI updates safely
```

---

## Files Modified

1. **Services/VisitedSystemsService.cs**
   - `HandleFSDJumpEvent`: Don't set `LastVisitTimestamp` directly, pass in event args
   - `HandleScanEvent`: Don't call `AddBody` directly, signal ViewModel with `isNewBody: true`
   - `HandleFSSBodySignalsEvent`: Don't call `AddBody` directly, signal ViewModel with `isNewBody: true`
   - Updated `SystemUIUpdateEventArgs` to include `NewLastVisitTimestamp`
   - Updated `BodyUIUpdateEventArgs` to include `IsNewBody` flag

2. **ViewModels/VisitedSystemsViewModel.cs**
   - `OnSystemUIUpdateRequested`: Apply timestamp update on UI thread
   - `OnBodyUIUpdateRequested`: Handle new body addition on UI thread

---

## Testing Checklist

✅ FSDJump to new system - No COMException  
✅ FSDJump to existing system - No COMException, timestamp updates  
✅ Scan new body - No COMException, body added  
✅ Scan existing body - No COMException, properties update  
✅ FSSBodySignals for new body - No COMException, body added with signals  
✅ FSSBodySignals for existing body - No COMException, signals update  

---

## Key Takeaway

In MVVM with background event processing:

**Golden Rule**: 
> Services provide data via events.  
> ViewModels apply data to UI-bound objects on the UI thread.

This ensures thread safety and prevents COMExceptions! 🎯
