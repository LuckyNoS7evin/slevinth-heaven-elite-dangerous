# Architecture Refactoring - Separation of Concerns

## Overview

This document explains the refactored architecture that separates data retrieval, business logic, and UI presentation.

## Architecture Layers

### 0. **Event Infrastructure Layer** (`Services/JournalEventService`)
- **Purpose**: Manage journal file monitoring and event routing
- **Responsibilities**:
  - Create and manage FileListener lifecycle
  - Route game events to registered IEventHandlers
  - Provide diagnostics functionality
  - Raise events for unknown events and errors
- **Examples**: `JournalEventService`

### 1. **Services Layer** (`Services/`)
- **Purpose**: Handle data retrieval, business logic, and event processing
- **Responsibilities**:
  - Process game events (from journal files)
  - Manage business state and rules
  - Persist data through DataServices
  - Raise domain events for UI updates
- **Examples**: `ExoBioService`, `VisitedSystemsManager`

### 2. **ViewModels Layer** (`ViewModels/`)
- **Purpose**: Manage UI state and respond to service events
- **Responsibilities**:
  - Subscribe to service events
  - Transform data models to presentation models
  - Handle UI-specific logic (sorting, filtering, formatting)
  - Implement INotifyPropertyChanged for data binding
- **Examples**: `ExoBioViewModel`, `ExoBioCardViewModel`

### 3. **Models Layer** (`Models/`)
- **Purpose**: Data structures for business logic and presentation
- **Types**:
  - Business Models: Domain objects (`VisitedSystemCard`, `BodyCard`)
  - Presentation Models: UI-specific data (`ExoBioDiscoveryModel`, `ExoBioStateModel`)
- **Examples**: `ExoBioDiscoveryModel`, `ExoBioStateModel`

### 4. **Controls Layer** (`Controls/`)
- **Purpose**: Pure UI components
- **Responsibilities**:
  - Minimal code-behind (mostly initialization)
  - Bind to ViewModels via DataContext
  - Handle lifecycle (Loaded/Unloaded)
- **Examples**: `ExoBioControl`

### 5. **Data Layer** (`Data/`)
- **Purpose**: Data persistence
- **Responsibilities**:
  - Serialize/deserialize data
  - File I/O operations
- **Examples**: `ExoBioDataService`, `ExoBioData`, `ExoBioCardData`

## Example: ExoBio Feature Flow

### Event Processing Flow
```
Game Event (Journal File)
    ↓
FileListener → EventParser
    ↓
JournalEventService.OnEventReceived()     [Event Infrastructure]
    ↓
Routes to registered IEventHandlers:
    ├─→ ExoBioService.HandleEvent()       [Service Layer - Business Logic]
    ├─→ VisitedSystemsService.HandleEvent()
    └─→ FSDService.HandleEvent()
    ↓
ExoBioService raises events:
    - DiscoveryAdded
    - DiscoveryUpdated
    - DiscoveriesSubmitted
    ↓
ExoBioViewModel (subscribed)              [ViewModel Layer - UI State]
    ↓
Updates ObservableCollection<ExoBioCardViewModel>
    ↓
UI Updates via Data Binding               [View Layer]
```

### Key Components

#### ExoBioService
```csharp
public sealed class ExoBioService : IEventHandler, IDisposable
{
    // Singleton pattern for shared state
    public static ExoBioService Instance { get; }
    
    // Service events
    public event EventHandler<ExoBioDiscoveryEventArgs>? DiscoveryAdded;
    public event EventHandler<ExoBioDiscoveryEventArgs>? DiscoveryUpdated;
    public event EventHandler<ExoBioSubmittedEventArgs>? DiscoveriesSubmitted;
    public event EventHandler<ExoBioDataLoadedEventArgs>? DataLoaded;
    
    // Handle game events
    public void HandleEvent(EventBase evt)
    
    // Load/save operations
    public Task<ExoBioStateModel> LoadDataAsync()
    public void ScheduleSave(ExoBioStateModel state)
}
```

#### ExoBioViewModel
```csharp
public sealed class ExoBioViewModel : INotifyPropertyChanged, IDisposable
{
    // UI-bindable collections
    public ObservableCollection<ExoBioCardViewModel> ExoBioCards { get; }
    
    // UI-bindable properties
    public long SubmittedTotal { get; set; }
    public string SubmittedTotalFormatted { get; }
    
    // Constructor subscribes to service events
    public ExoBioViewModel(DispatcherQueue dispatcherQueue)
    {
        _service.DiscoveryAdded += OnDiscoveryAdded;
        _service.DiscoveryUpdated += OnDiscoveryUpdated;
        // ...
    }
    
    // Event handlers update UI state
    private void OnDiscoveryAdded(object? sender, ExoBioDiscoveryEventArgs e)
}
```

#### ExoBioControl
```csharp
public sealed partial class ExoBioControl : UserControl
{
    private ExoBioViewModel? _viewModel;
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = new ExoBioViewModel(DispatcherQueue);
        this.DataContext = _viewModel;
    }
    
    public async Task InitializeAsync()
    {
        await _service.LoadDataAsync();
    }
}
```

## Benefits of This Architecture

1. **Separation of Concerns**: Each layer has a single responsibility
2. **Testability**: Services and ViewModels can be tested independently of UI
3. **Reusability**: Services can be shared across multiple UI components
4. **Maintainability**: Changes to business logic don't affect UI and vice versa
5. **Event-Driven**: Loose coupling through events instead of direct dependencies

## Migration Path

The ExoBio feature has been fully refactored. To migrate other features:

1. **Create Service**:
   - Move event handling logic from Control to Service
   - Add events for state changes
   - Implement IEventHandler interface
   - Use singleton pattern if state needs to be shared

2. **Create Models**:
   - Extract business/presentation models from Control
   - Keep them simple POCOs (Plain Old CLR Objects)

3. **Create ViewModel**:
   - Subscribe to Service events
   - Manage ObservableCollections for UI binding
   - Transform Models to ViewModels as needed
   - Implement INotifyPropertyChanged

4. **Update Control**:
   - Remove business logic and event handling
   - Create ViewModel in Loaded event
   - Set DataContext to ViewModel
   - Keep only UI-specific code

5. **Update XAML**:
   - Change `x:Bind` to `Binding` for DataContext properties
   - Update DataTemplate types to ViewModels
   - Add ViewModel namespace

6. **Register in MainWindow**:
   - Add Service to _eventHandlers list
   - Remove Control from _eventHandlers if it no longer implements IEventHandler

## Current Status

### ✅ Refactored
- **ExoBio Feature**: Fully refactored with Service → ViewModel → Control pattern
  - `ExoBioService` - Processes ScanOrganic and SellOrganicData events
  - `ExoBioViewModel` - Manages UI state for ExoBio cards
  - `ExoBioControl` - Pure UI with data binding

- **VisitedSystems Feature**: Fully refactored with Service → ViewModel → Control pattern
  - `VisitedSystemsService` - Processes FSDJump, Scan, FSSBodySignals, and SAAScanComplete events
  - `VisitedSystemsViewModel` - Manages UI state for visited systems
  - `VisitedSystemsManager` - Shared data manager (provides IEventHandler via VisitedSystemsService)
  - `VisitedSystemsControl` - Pure UI with data binding

- **General Feature**: Fully refactored with Service → ViewModel → Control pattern
  - `FSDService` - Processes FSDJump and FSDTarget events
  - `GeneralViewModel` - Manages UI state, coordinates FSD sub-ViewModels
  - `FSDTimingViewModel` - Manages FSD timing statistics UI state
  - `FSDTargetViewModel` - Manages FSD target information UI state
  - `GeneralControl` - Pure UI container
  - `FSDTimingCard` - Pure UI component
  - `FSDTargetCard` - Pure UI component

### 🎉 All Controls Refactored!
All features now follow the clean Service → ViewModel → Control architecture pattern.

## Next Steps

Apply the same refactoring pattern to:
1. VisitedSystemsControl
2. GeneralControl
3. Any other controls that implement IEventHandler

Each feature should follow the Service → ViewModel → Control pattern established by ExoBio.
