# General/FSD Feature Refactoring Summary

## What Changed

The GeneralControl and its sub-controls (FSDTimingCard, FSDTargetCard) have been refactored to follow the clean architecture pattern:

### Before
```
GeneralControl (implements IEventHandler)
├─ Direct event handling (FSDJump, FSDTarget)
├─ Data persistence logic
└─ Sub-controls with business logic:
    ├─ FSDTimingCard
    │   ├─ Event handling (HandleFSDJump)
    │   ├─ Business logic (CalculateStatistics)
    │   └─ UI properties
    └─ FSDTargetCard
        ├─ Event handling (HandleFSDTarget)
        └─ UI properties
```

### After
```
FSDService (implements IEventHandler)
├─ Event handling (FSDJump, FSDTarget)
├─ Business logic (timing calculations)
├─ Data persistence
└─ Raises UI events: TimingUpdated, TargetUpdated
    ↓
GeneralViewModel
├─ Subscribes to service events
├─ Coordinates sub-ViewModels:
│   ├─ FSDTimingViewModel (timing statistics)
│   └─ FSDTargetViewModel (target information)
└─ UI state management
    ↓
GeneralControl (container)
├─ Sets DataContext for sub-controls
├─ FSDTimingCard (pure UI) → binds to FSDTimingViewModel
└─ FSDTargetCard (pure UI) → binds to FSDTargetViewModel
```

## New Files Created

### Models
1. **Models/FSDTimingModel.cs** - Business model for timing statistics
2. **Models/FSDTargetModel.cs** - Business model for target information
3. **Models/GeneralStateModel.cs** - Complete state model

### Service
4. **Services/FSDService.cs**
   - Processes game events: FSDJumpEvent, FSDTargetEvent
   - Calculates timing statistics (avg, shortest, longest)
   - Manages jump history
   - Raises UI events for ViewModel consumption
   - Handles data persistence

### ViewModels
5. **ViewModels/GeneralViewModel.cs**
   - Main ViewModel coordinating sub-ViewModels
   - Subscribes to FSDService events
   - Manages lifecycle of FSDTimingViewModel and FSDTargetViewModel

6. **ViewModels/FSDTimingViewModel.cs**
   - UI state for timing statistics
   - Formatting logic for display
   - INotifyPropertyChanged implementation

7. **ViewModels/FSDTargetViewModel.cs**
   - UI state for target information
   - Formatting logic for display
   - INotifyPropertyChanged implementation

## Files Modified

### Controls
1. **Controls/GeneralControl.xaml.cs**
   - Removed IEventHandler implementation
   - Removed IDisposable implementation
   - Removed all event handling
   - Removed all data persistence logic
   - Creates GeneralViewModel and sets DataContext for sub-controls
   - Now only ~45 lines (was ~163 lines)

2. **Controls/FSDTimingCard.xaml.cs**
   - Removed INotifyPropertyChanged implementation
   - Removed all properties and logic
   - Pure UI component (only constructor)
   - Now only ~12 lines (was ~162 lines)

3. **Controls/FSDTimingCard.xaml**
   - Changed all `x:Bind` to `Binding` for ViewModel binding

4. **Controls/FSDTargetCard.xaml.cs**
   - Removed INotifyPropertyChanged implementation
   - Removed all properties and event handling
   - Pure UI component (only constructor)
   - Now only ~12 lines (was ~80 lines)

5. **Controls/FSDTargetCard.xaml**
   - Changed all `x:Bind` to `Binding` for ViewModel binding

### MainWindow
6. **MainWindow.xaml.cs**
   - Added FSDService to event handlers
   - Removed GeneralControl from event handlers

## Key Benefits

✅ **Composite ViewModel Pattern**: GeneralViewModel coordinates multiple sub-ViewModels
✅ **Separation of Concerns**: Business logic in service, statistics calculations isolated
✅ **Reusability**: FSDService can be used by multiple controls
✅ **Testability**: Service and ViewModels can be tested independently
✅ **Maintainability**: Each component has a single, clear responsibility

## Event Flow

### FSDJumpEvent
```
FSDJumpEvent
    ↓
FSDService.HandleFSDJumpEvent()
    - Add timestamp to _jumpTimes
    - Calculate statistics
    - Raise TimingUpdated event
    ↓
GeneralViewModel.OnTimingUpdated()
    - Update FSDTimingViewModel
    - Schedule save
    ↓
FSDTimingCard UI updates via Binding
```

### FSDTargetEvent
```
FSDTargetEvent
    ↓
FSDService.HandleFSDTargetEvent()
    - Create FSDTargetModel
    - Raise TargetUpdated event
    ↓
GeneralViewModel.OnTargetUpdated()
    - Update FSDTargetViewModel
    ↓
FSDTargetCard UI updates via Binding
```

## Architecture Pattern

The GeneralControl demonstrates a **composite ViewModel pattern**:
- One main ViewModel (`GeneralViewModel`)
- Multiple sub-ViewModels (`FSDTimingViewModel`, `FSDTargetViewModel`)
- Sub-controls receive their specific ViewModel as DataContext
- Main control coordinates initialization and lifecycle

This pattern is ideal for complex controls with multiple sections that need separate UI state management.

## Notes

- **Jump History Preserved**: The service maintains the complete jump history in memory
- **Statistics Calculated On-Demand**: Timing stats are recalculated on each jump
- **Minimal Persistence**: Only the shortest time is persisted (as per original design)
- **No Target Persistence**: FSD target is session-only (not persisted)
