# JournalEventService - Event Routing Service

## Overview

The `JournalEventService` is a central service that manages journal file monitoring and routes game events to all registered services. This completes the separation of concerns by removing event infrastructure from the MainWindow.

---

## Architecture

### Before
```
MainWindow
├─ Creates EventParser
├─ Creates FileListener
├─ Manages List<IEventHandler>
├─ Subscribes to FileListener events
├─ Routes events to handlers
├─ Handles diagnostics
└─ Manages FileListener lifecycle
```

### After
```
JournalEventService (Singleton)
├─ Creates EventParser
├─ Creates FileListener
├─ Manages List<IEventHandler>
├─ Routes events to handlers
├─ Handles diagnostics
└─ Manages FileListener lifecycle
    ↓
MainWindow
├─ Subscribes to JournalEventService.UnknownEventReceived
├─ Manages window lifecycle
└─ Coordinates control initialization
```

---

## Key Features

### 1. Singleton Pattern
```csharp
public static JournalEventService Instance { get; }
```
Single instance ensures all services receive the same events.

### 2. Service Registration
```csharp
public void RegisterEventHandler(IEventHandler handler)
public void UnregisterEventHandler(IEventHandler handler)
```
Services register themselves to receive game events.

### 3. Lifecycle Management
```csharp
public void Start()  // Start monitoring journal files
public void Stop()   // Stop monitoring
public void Dispose() // Clean up resources
```

### 4. Diagnostics
```csharp
public async Task RunDiagnosticsAsync()
```
Scans all journal files for event analysis.

### 5. Event Propagation
```csharp
// Service events
public event EventHandler<GameEventReceivedEventArgs>? GameEventReceived;
public event EventHandler<UnknownEventReceivedEventArgs>? UnknownEventReceived;
public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
```

---

## Event Flow

```
1. Journal File Changes
   ↓
2. FileListener detects change
   ↓
3. EventParser parses lines
   ↓
4. JournalEventService.OnEventReceived(EventBase)
   │
   ├─ Log event
   │
   ├─ Route to ALL registered IEventHandlers:
   │  ├─→ ExoBioService.HandleEvent(evt)
   │  ├─→ VisitedSystemsService.HandleEvent(evt)
   │  └─→ FSDService.HandleEvent(evt)
   │
   └─ Raise GameEventReceived (for UI or other subscribers)
```

---

## Usage in MainWindow

### Before (90+ lines)
```csharp
private readonly EventParser _eventParser;
private readonly FileListener _fileListener;
private readonly List<IEventHandler> _eventHandlers = [];

public MainWindow()
{
    // Register handlers
    _eventHandlers.Add(service1);
    _eventHandlers.Add(service2);
    
    // Create infrastructure
    _eventParser = new EventParser();
    _fileListener = new FileListener(_eventParser);
    
    // Subscribe to events
    _fileListener.EventReceived += OnEventReceived;
    _fileListener.UnknownEventReceived += OnUnknownEventReceived;
    
    // Start listening
    _fileListener.Start();
}

private void OnEventReceived(EventBase evt)
{
    foreach (var handler in _eventHandlers)
    {
        handler.HandleEvent(evt);
    }
}
```

### After (~50 lines)
```csharp
private readonly JournalEventService _journalEventService = JournalEventService.Instance;

public MainWindow()
{
    // Register services
    _journalEventService.RegisterEventHandler(ExoBioService.Instance);
    _journalEventService.RegisterEventHandler(VisitedSystemsService.Instance);
    _journalEventService.RegisterEventHandler(FSDService.Instance);
    
    // Subscribe for UI updates only
    _journalEventService.UnknownEventReceived += OnUnknownEventReceived;
    
    // Start
    _journalEventService.Start();
}
```

---

## Benefits

### ✅ Single Responsibility
- MainWindow: UI and window lifecycle only
- JournalEventService: Event infrastructure only

### ✅ Testability
- JournalEventService can be tested independently
- Mock event service for MainWindow tests
- Services can be tested without FileListener

### ✅ Reusability
- Event routing logic can be used by other windows/components
- Services can be dynamically registered/unregistered

### ✅ Error Isolation
- Event handler errors don't crash the app
- Each handler wrapped in try-catch
- Errors reported via ErrorOccurred event

### ✅ Diagnostics
- Built-in diagnostics support
- Can be called independently
- No UI dependencies

---

## Registration Pattern

Services register themselves with the JournalEventService:

```csharp
// In MainWindow or App startup
var journalService = JournalEventService.Instance;

// Register domain services
journalService.RegisterEventHandler(ExoBioService.Instance);
journalService.RegisterEventHandler(VisitedSystemsService.Instance);
journalService.RegisterEventHandler(FSDService.Instance);

// Start monitoring
journalService.Start();

// Later, on shutdown
journalService.Stop();
journalService.Dispose();
```

---

## Error Handling

The service provides robust error handling:

1. **Event Handler Errors**: Wrapped in try-catch, reported via ErrorOccurred
2. **FileListener Errors**: Caught and reported
3. **Diagnostics Errors**: Logged and reported
4. **Startup Errors**: Thrown to caller for handling

---

## Thread Safety

- Singleton uses double-check locking
- FileListener events handled on background thread
- ViewModels use DispatcherQueue for UI updates
- No direct UI access from service

---

## Impact

### MainWindow Simplified
- **From**: Event infrastructure + routing + lifecycle (~170 lines)
- **To**: Service coordination + window lifecycle (~95 lines)
- **Reduction**: ~44% less code

### Concerns Separated
- **JournalEventService**: Event infrastructure
- **Services**: Domain logic
- **ViewModels**: UI state
- **Controls**: Presentation
- **MainWindow**: Coordination only

---

## Complete Architecture

```
Application Startup
    ↓
MainWindow.Constructor
    ├─ JournalEventService.Instance
    │   ├─ Creates EventParser
    │   ├─ Creates FileListener
    │   └─ Registers event handlers:
    │       ├─ ExoBioService.Instance
    │       ├─ VisitedSystemsService.Instance
    │       └─ FSDService.Instance
    │
    ├─ Subscribes to UnknownEventReceived
    ├─ Subscribes to ErrorOccurred
    └─ JournalEventService.Start()
        ↓
    FileListener monitors journal files
        ↓
    Game events → Services → ViewModels → Controls
```

This is a **clean, maintainable, enterprise-grade architecture**! 🎉
