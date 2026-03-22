# Dependency Injection Implementation

## 🎯 Enterprise-Grade DI

The application now implements proper **Dependency Injection** using `Microsoft.Extensions.DependencyInjection`, replacing the anti-pattern singleton approach with proper IoC (Inversion of Control).

---

## What Changed

### Before: Singleton Anti-Pattern
```csharp
// Services had static Instance properties
public sealed class ExoBioService
{
    private static ExoBioService? _instance;
    private static readonly object _lock = new();
    
    public static ExoBioService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ExoBioService();
                }
            }
            return _instance;
        }
    }
    
    private ExoBioService() { }
}

// Consumers accessed services via static Instance
var service = ExoBioService.Instance;
```

### After: Dependency Injection
```csharp
// Services have public constructors with dependencies
public sealed class ExoBioService : IEventHandler, IDisposable
{
    private readonly VisitedSystemsManager _systemsManager;
    
    public ExoBioService(VisitedSystemsManager systemsManager)
    {
        _systemsManager = systemsManager ?? throw new ArgumentNullException(nameof(systemsManager));
    }
}

// Consumers receive services via constructor injection
public sealed class MainWindow : Window
{
    private readonly JournalEventService _journalEventService;
    private readonly ExoBioService _exoBioService;
    
    public MainWindow(
        JournalEventService journalEventService,
        ExoBioService exoBioService,
        ...)
    {
        _journalEventService = journalEventService ?? throw new ArgumentNullException(...);
        _exoBioService = exoBioService ?? throw new ArgumentNullException(...);
    }
}
```

---

## Architecture Components

### 1. Service Configuration (`ServiceConfiguration.cs`)

Centralizes all DI registrations:

```csharp
public static class ServiceConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register Services as Singletons
        services.AddSingleton<JournalEventService>();
        services.AddSingleton<ExoBioService>();
        services.AddSingleton<VisitedSystemsService>();
        services.AddSingleton<VisitedSystemsManager>();
        services.AddSingleton<FSDService>();

        // Register MainWindow as Transient
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}
```

### 2. Application Entry Point (`App.xaml.cs`)

Configures DI on startup:

```csharp
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        InitializeComponent();
        
        // Configure dependency injection
        _serviceProvider = ServiceConfiguration.ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Resolve MainWindow from DI container
        _window = _serviceProvider?.GetRequiredService<MainWindow>();
        _window?.Activate();
    }

    // Provide access to services for controls
    public static IServiceProvider Services => ((App)Current)._serviceProvider 
        ?? throw new InvalidOperationException("Service provider not initialized");
}
```

### 3. Service Dependencies

Services now declare their dependencies explicitly:

```csharp
// ExoBioService depends on VisitedSystemsManager
public ExoBioService(VisitedSystemsManager systemsManager)
{
    _systemsManager = systemsManager ?? throw new ArgumentNullException(nameof(systemsManager));
}

// VisitedSystemsService depends on VisitedSystemsManager
public VisitedSystemsService(VisitedSystemsManager manager)
{
    _manager = manager ?? throw new ArgumentNullException(nameof(manager));
}
```

### 4. MainWindow Constructor Injection

MainWindow receives all required services:

```csharp
public MainWindow(
    JournalEventService journalEventService,
    ExoBioService exoBioService,
    VisitedSystemsService visitedSystemsService,
    VisitedSystemsManager visitedSystemsManager,
    FSDService fsdService)
{
    // Store dependencies
    _journalEventService = journalEventService ?? throw new ArgumentNullException(...);
    _exoBioService = exoBioService ?? throw new ArgumentNullException(...);
    // ...
    
    // Register services for event handling
    _journalEventService.RegisterEventHandler(_exoBioService);
    _journalEventService.RegisterEventHandler(_visitedSystemsService);
    _journalEventService.RegisterEventHandler(_fsdService);
}
```

### 5. Controls Use Service Locator Pattern

Since WinUI 3 doesn't support DI for controls, they use the service locator pattern:

```csharp
public sealed partial class ExoBioControl : UserControl
{
    private readonly ExoBioService _service;
    
    public ExoBioControl()
    {
        // Get service from DI container via App.Services
        _service = App.Services.GetRequiredService<ExoBioService>();
        
        // Create ViewModel with service
        _viewModel = new ExoBioViewModel(DispatcherQueue, _service);
        this.DataContext = _viewModel;
    }
}
```

### 6. ViewModels Constructor Injection

ViewModels receive their dependencies via constructor:

```csharp
public sealed class ExoBioViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ExoBioService _service;
    private readonly DispatcherQueue _dispatcherQueue;
    
    public ExoBioViewModel(DispatcherQueue dispatcherQueue, ExoBioService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        
        // Subscribe to service events
        _service.DiscoveryAdded += OnDiscoveryAdded;
    }
}
```

---

## Dependency Graph

```
ServiceProvider (Microsoft.Extensions.DependencyInjection)
    ├── JournalEventService (Singleton)
    ├── VisitedSystemsManager (Singleton)
    ├── ExoBioService (Singleton)
    │   └─→ VisitedSystemsManager (injected)
    ├── VisitedSystemsService (Singleton)
    │   └─→ VisitedSystemsManager (injected)
    ├── FSDService (Singleton)
    └── MainWindow (Transient)
        ├─→ JournalEventService (injected)
        ├─→ ExoBioService (injected)
        ├─→ VisitedSystemsService (injected)
        ├─→ VisitedSystemsManager (injected)
        └─→ FSDService (injected)
```

---

## Benefits of DI

### ✅ 1. Testability
```csharp
// Easy to test with mock dependencies
var mockManager = new Mock<VisitedSystemsManager>();
var service = new ExoBioService(mockManager.Object);

// Test service behavior
service.HandleEvent(testEvent);
mockManager.Verify(m => m.GetSystem(It.IsAny<long>()), Times.Once);
```

### ✅ 2. Explicit Dependencies
```csharp
// Dependencies are clear from constructor signature
public ExoBioService(VisitedSystemsManager systemsManager)
{
    // Can't create service without required dependencies
}
```

### ✅ 3. Lifetime Management
```csharp
// DI container manages lifetime
services.AddSingleton<ExoBioService>();  // One instance for app lifetime
services.AddTransient<MainWindow>();     // New instance each time
services.AddScoped<SomeService>();       // One instance per scope
```

### ✅ 4. Inversion of Control
```csharp
// Application doesn't control object creation
// DI container does

// Before: Tight coupling
var service = ExoBioService.Instance;

// After: Loose coupling
public MainWindow(ExoBioService exoBioService) { }
```

### ✅ 5. Easy to Replace Implementations
```csharp
// Can swap implementations without changing consumers
services.AddSingleton<IExoBioService, ExoBioService>();
// Later, change to:
services.AddSingleton<IExoBioService, MockExoBioService>();
```

### ✅ 6. Centralized Configuration
```csharp
// All registrations in one place
public static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();
    // All service registrations here
    return services.BuildServiceProvider();
}
```

---

## Service Lifetimes

### Singleton
- **Usage**: `services.AddSingleton<T>()`
- **Lifetime**: Created once for application lifetime
- **Used For**: 
  - JournalEventService
  - Domain services (ExoBioService, FSDService)
  - Shared state (VisitedSystemsManager)

### Transient
- **Usage**: `services.AddTransient<T>()`
- **Lifetime**: New instance created each time
- **Used For**:
  - MainWindow (though typically only created once)
  - Short-lived objects

### Scoped
- **Usage**: `services.AddScoped<T>()`
- **Lifetime**: One instance per scope
- **Not Used**: WinUI 3 doesn't have a natural scope concept

---

## Migration Steps Completed

1. ✅ Added `Microsoft.Extensions.DependencyInjection` NuGet package
2. ✅ Created `ServiceConfiguration` class
3. ✅ Updated `App.xaml.cs` to configure DI
4. ✅ Removed singleton patterns from all services
5. ✅ Added constructors with dependency parameters
6. ✅ Updated MainWindow to use constructor injection
7. ✅ Updated Controls to use service locator (via `App.Services`)
8. ✅ Updated ViewModels to receive services via constructor
9. ✅ Added null checks for all injected dependencies

---

## Pattern Comparison

### Singleton Pattern (Old)
```csharp
// ❌ Anti-pattern
public static ExoBioService Instance { get; }
private ExoBioService() { }

// Hard to test
// Hidden dependencies
// Tight coupling
// Global mutable state
```

### Dependency Injection (New)
```csharp
// ✅ Best practice
public ExoBioService(VisitedSystemsManager manager) { }

// Easy to test
// Explicit dependencies
// Loose coupling
// Controlled lifecycle
```

---

## Service Locator vs Constructor Injection

### Constructor Injection (Preferred - Used in MainWindow)
```csharp
public MainWindow(
    JournalEventService journalEventService,
    ExoBioService exoBioService)
{
    // Dependencies injected by DI container
}
```

### Service Locator (Acceptable - Used in Controls)
```csharp
public ExoBioControl()
{
    // WinUI 3 controls can't use constructor injection
    _service = App.Services.GetRequiredService<ExoBioService>();
}
```

**Why Service Locator in Controls?**
- WinUI 3 creates controls via parameterless constructors
- No built-in DI support for UserControls
- Service Locator is acceptable compromise

---

## Future Enhancements

### 1. Interface-Based Services
```csharp
// Define interfaces
public interface IExoBioService : IEventHandler { }
public interface IJournalEventService { }

// Register by interface
services.AddSingleton<IExoBioService, ExoBioService>();
services.AddSingleton<IJournalEventService, JournalEventService>();

// Depend on abstractions
public MainWindow(IJournalEventService journalService) { }
```

### 2. Scoped Services for Features
```csharp
// Create scopes for specific operations
using var scope = App.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<ISomeService>();
```

### 3. Configuration Options Pattern
```csharp
services.Configure<ExoBioOptions>(options =>
{
    options.AutoSave = true;
    options.SaveDebounceMs = 500;
});

public ExoBioService(IOptions<ExoBioOptions> options) { }
```

---

## Testing Example

```csharp
[TestClass]
public class ExoBioServiceTests
{
    [TestMethod]
    public void HandleEvent_NewDiscovery_RaisesDiscoveryAdded()
    {
        // Arrange
        var mockManager = new Mock<VisitedSystemsManager>();
        var service = new ExoBioService(mockManager.Object);
        bool eventRaised = false;
        service.DiscoveryAdded += (s, e) => eventRaised = true;

        var testEvent = new ScanOrganicEvent { /* ... */ };

        // Act
        service.HandleEvent(testEvent);

        // Assert
        Assert.IsTrue(eventRaised);
        mockManager.Verify(m => m.GetSystem(It.IsAny<long>()), Times.Once);
    }
}
```

---

## Conclusion

The application now implements **enterprise-grade Dependency Injection**:

✅ **No more singletons** - Services use proper DI lifecycle management
✅ **Explicit dependencies** - Constructor signatures show requirements
✅ **Testable** - Easy to mock dependencies for unit testing
✅ **Flexible** - Easy to swap implementations
✅ **Maintainable** - Centralized service configuration
✅ **Best practices** - Follows SOLID principles and IoC pattern

The architecture is now **production-ready and enterprise-grade**! 🚀
