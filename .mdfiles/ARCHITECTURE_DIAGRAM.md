# Refactored Architecture - Quick Reference

## Layer Structure

```
┌─────────────────────────────────────────────────────┐
│                   View Layer                        │
│  (XAML + minimal code-behind)                       │
│                                                     │
│  ExoBioControl.xaml / .xaml.cs                     │
│  - Binds to ViewModel via DataContext              │
│  - Pure UI presentation                            │
└──────────────────┬──────────────────────────────────┘
                   │ Data Binding
┌──────────────────▼──────────────────────────────────┐
│                ViewModel Layer                      │
│  (UI State Management)                              │
│                                                     │
│  ExoBioViewModel                                    │
│  - ObservableCollection<ExoBioCardViewModel>       │
│  - Subscribes to Service events                    │
│  - Manages UI state                                │
│  - INotifyPropertyChanged                          │
└──────────────────┬──────────────────────────────────┘
                   │ Service Events
┌──────────────────▼──────────────────────────────────┐
│                Service Layer                        │
│  (Business Logic & Data Management)                 │
│                                                     │
│  ExoBioService (IEventHandler)                     │
│  - Processes game events                           │
│  - Business logic                                  │
│  - Raises events: DiscoveryAdded,                  │
│    DiscoveryUpdated, DiscoveriesSubmitted          │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│                 Data Layer                          │
│  (Persistence)                                      │
│                                                     │
│  ExoBioDataService                                  │
│  - SaveDataAsync() / LoadDataAsync()               │
│  - File I/O operations                             │
└─────────────────────────────────────────────────────┘
```

## Event Flow Example: ScanOrganicEvent

```
1. Journal File Update
   ↓
2. FileListener detects change
   ↓
3. EventParser parses → ScanOrganicEvent
   ↓
4. JournalEventService.OnEventReceived(ScanOrganicEvent)
   ├─ Routes to ALL registered services
   └─ Raises GameEventReceived event
   ↓
5. ExoBioService.HandleEvent(ScanOrganicEvent)
   │
   ├─ Business Logic:
   │  - Check if duplicate
   │  - Calculate values
   │  - Update internal state
   │
   └─ Raise Event: DiscoveryAdded
      ↓
6. ExoBioViewModel.OnDiscoveryAdded(EventArgs)
   │
   ├─ UI Logic:
   │  - Create ExoBioCardViewModel
   │  - Add to ObservableCollection
   │  - Trigger property changes
   │
   └─ Schedule Save
      ↓
7. UI updates automatically via data binding
```

## Event Flow Example: FSDJumpEvent (Multiple Services)

```
1. Journal File Update
   ↓
2. FileListener detects change
   ↓
3. EventParser parses → FSDJumpEvent
   ↓
4. JournalEventService.OnEventReceived(FSDJumpEvent)
   │
   ├─ Routes to ALL services (in parallel):
   │  │
   │  ├─→ VisitedSystemsService.HandleEvent(FSDJumpEvent)
   │  │   └─ Updates system, raises SystemUIUpdateRequested
   │  │
   │  └─→ FSDService.HandleEvent(FSDJumpEvent)
   │      └─ Calculates timing stats, raises TimingUpdated
   │
   └─ Each service processes independently
      ↓
5. ViewModels receive their respective events
   ├─ VisitedSystemsViewModel updates systems list
   └─ GeneralViewModel/FSDTimingViewModel updates stats
      ↓
6. UI updates automatically via data binding
```

## Key Files by Layer

### Services
- ✅ `Services/ExoBioService.cs` - ExoBio business logic
- ✅ `Services/VisitedSystemsService.cs` - VisitedSystems event processing
- ✅ `Services/VisitedSystemsManager.cs` - Visited systems data manager
- ✅ `Services/FSDService.cs` - FSD (Frame Shift Drive) statistics and target tracking

### ViewModels
- ✅ `ViewModels/ExoBioViewModel.cs` - ExoBio UI state
- ✅ `ViewModels/ExoBioCardViewModel.cs` - Individual card UI state
- ✅ `ViewModels/VisitedSystemsViewModel.cs` - VisitedSystems UI state
- ✅ `ViewModels/GeneralViewModel.cs` - General control main ViewModel
- ✅ `ViewModels/FSDTimingViewModel.cs` - FSD timing statistics UI state
- ✅ `ViewModels/FSDTargetViewModel.cs` - FSD target information UI state

### Models
- ✅ `Models/ExoBioDiscoveryModel.cs` - Business model
- ✅ `Models/ExoBioStateModel.cs` - State model
- ✅ `Models/VisitedSystemCard.cs` - System data model
- ✅ `Models/BodyCard.cs` - Body data model

### Controls
- ✅ `Controls/ExoBioControl.xaml[.cs]` - ExoBio UI
- ✅ `Controls/VisitedSystemsControl.xaml[.cs]` - Systems UI
- ✅ `Controls/GeneralControl.xaml[.cs]` - General UI (container)
- ✅ `Controls/FSDTimingCard.xaml[.cs]` - FSD timing statistics UI
- ✅ `Controls/FSDTargetCard.xaml[.cs]` - FSD target information UI

### Data
- `Data/ExoBioDataService.cs` - Persistence
- `Data/ExoBioData.cs` - Serialization model
- `Data/ExoBioCardData.cs` - Card serialization

## Benefits

✅ **Testability** - Services can be unit tested without UI
✅ **Separation** - Business logic separate from presentation
✅ **Reusability** - Services can be shared across controls
✅ **Maintainability** - Clear responsibility for each layer
✅ **Event-Driven** - Loose coupling through events
