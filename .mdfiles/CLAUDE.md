# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Three-project solution — no `.sln` file:

| Project | Framework | Purpose |
|---|---|---|
| `SlevinthHeavenEliteDangerous` | .NET 8.0 / WinUI 3 | Desktop companion app (Windows only) |
| `SlevinthHeavenEliteDangerous.Api` | .NET 10.0 / ASP.NET Core | Web API + Blazor Server frontend + Discord bot |
| `SlevinthHeavenEliteDangerous.Core` | .NET 8.0 | Shared library — 150+ ED journal event classes, helpers |

## Build Commands

```bash
# Desktop app — must specify platform (Any CPU fails for WinUI3)
dotnet build SlevinthHeavenEliteDangerous/SlevinthHeavenEliteDangerous.csproj -p:Platform=x64
dotnet run --project SlevinthHeavenEliteDangerous/SlevinthHeavenEliteDangerous.csproj -p:Platform=x64

# API
dotnet build SlevinthHeavenEliteDangerous.Api/SlevinthHeavenEliteDangerous.Api.csproj
dotnet run --project SlevinthHeavenEliteDangerous.Api/SlevinthHeavenEliteDangerous.Api.csproj
```

Target platforms (desktop): x86, x64, ARM64. Minimum Windows 10 build 19041.

## Desktop App Architecture

**MVVM + Dependency Injection + Event-Driven**

### Event pipeline (core data flow)

```
Elite Dangerous journal files
  → FileListener (monitors file changes)
  → EventParser (JSON → EventBase subclasses)
  → JournalEventService (routes to registered IEventHandler services)
  → Domain services (ExoBioService, VisitedSystemsService, FSDService, RankService)
  → ViewModels (INotifyPropertyChanged, updated via DispatcherQueue)
  → XAML controls (x:Bind data binding)
```

### DI setup

All services are registered as **singletons** in `Configuration/ServiceConfiguration.cs`. Controls access services via `App.Services` (service locator pattern for WinUI). `MainWindow` is registered as transient.

### Key service responsibilities

| Service | Purpose |
|---|---|
| `JournalEventService` | Owns FileListener + EventParser; routes events to all IEventHandler implementations |
| `StartupService` | Orchestrates initialization: registers handlers, starts journal monitoring |
| `ExoBioService` | Tracks organic discoveries, estimated earnings, sales history |
| `VisitedSystemsService` | Manages visited system/body/signal hierarchy |
| `VisitedSystemsManager` | Shared state store for visited systems data |
| `FSDService` | FSD jump timing statistics and navigation targets |
| `RankService` | Tracks 8 commander rank types with progress percentages |

### Data persistence (desktop)

JSON files stored in `Documents\SlevinthHeavenEliteDangerous\`. Services in `DataStorage/` handle serialization with a **500ms debounce** pattern.

| File | Type |
|------|------|
| `ranks_data.json` | `List<RankModel>` |
| `general_control_data.json` | `FSDTimingModel` |
| `exobio_data.json` | `ExoBioStateModel` |
| `visited_systems_data.json` | `List<VisitedSystemCard>` |
| `overlay_log_data.json` | `List<OverlayLogEntryRecord>` |

### Desktop project layout

- `Configuration/` — DI registration (`ServiceConfiguration.cs`)
- `Services/` — Domain logic; each implements `IEventHandler`
- `ViewModels/` — UI state with `INotifyPropertyChanged`; own all computed/formatted properties
- `Controls/` — XAML views + minimal code-behind
- `DataStorage/` — Persistence layer
- `Models/` — Pure data POCOs (no INPC)
- `Converters/` — XAML value converters

### Patterns to follow (desktop)

- New domain features: create Service (implements `IEventHandler`) → register in `ServiceConfiguration` → create ViewModel → create XAML Control
- All UI thread updates from background threads go through `DispatcherQueue`
- Services own all data and save internally — ViewModels NEVER call service save methods
- ViewModels receive data only via events or read-only getters
- All `LoadDataAsync()` calls are in `StartupService.InitializeDataAsync()` — controls have no `InitializeAsync()`
- Nullable reference types are enabled project-wide

## API Architecture

**ASP.NET Core Web API + Blazor Server (static SSR) + Discord bot**

### API data flow

```
Desktop app uploads raw journal files
  → JournalController → JournalFileStore (Data/Journals/{FID}/*.log)
  → JournalProcessingService (background, every 30s)
  → JournalLineProcessor (JsonDocument-based, no event type system)
  → CommanderDataStore (Data/Commanders/{FID}.json)
  → CommanderController (GET /api/commander/{fid})
```

### Key API components

| Component | Purpose |
|---|---|
| `JournalLineProcessor` | Stateless; parses raw JSON lines directly via `JsonDocument` — no Core event classes |
| `JournalProcessingService` | Background service; tracks processed line counts per file via `_processed.json` manifest; triggers full reprocess if earlier files arrive out of order |
| `CommanderDataStore` | Thread-safe, file-backed store; one JSON per commander keyed by FID |
| `JournalFileStore` | Raw journal storage; files kept verbatim at `Data/Journals/{FID}/` |
| `FrontierTokenAuthMiddleware` | Authenticates requests using Frontier CAPI tokens; sets `CommanderName` in `HttpContext.Items` |
| `DiscordBotService` | Discord.Net hosted service; slash commands via `InteractionHandler` |

### Visibility rules

`CommanderController.ToDto()` applies ownership-based filtering: current position, exact codex timestamps/systems, and ExoBio sale locations are hidden from non-owners.

### API project layout

- `Controllers/` — REST API controllers
- `Processing/` — `JournalLineProcessor`, `JournalProcessingService`
- `Storage/` — `CommanderDataStore`, `JournalFileStore`
- `Authentication/` — Frontier CAPI OAuth + cookie auth middleware
- `Discord/` — Bot service, interaction handler, slash command modules
- `Components/` — Blazor Server pages (`Pages/`, `Layout/`)
- `Models/` — Server-side data models (`ServerCommanderData` and nested types)

## Backwards Compatibility (persistent data)

When adding new features that require data rebuilt from journal history, follow these rules so existing users are not left with empty/stale data:

| Scenario | What to do |
|---|---|
| New service with its own save file | Add the filename to `journalDataFiles` in `StartupService.InitializeDataAsync()` |
| New field on an existing persisted model that needs journal history | Bump `RequiredDataSchemaVersion` in `StartupService` and add a history comment |
| New event type added to `JournalLineProcessor` (API) | Bump `CurrentProcessingSchemaVersion` in `JournalProcessingService` and add a history comment |
| New field with a sensible zero/empty default | No action — `System.Text.Json` fills it automatically |

**How it works (desktop):** `StartupService` checks `schema_version.json` against `RequiredDataSchemaVersion`. A mismatch deletes all data files and triggers a full journal rescan, then writes the new version.

**How it works (API):** Each commander's `_processed.json` manifest stores `SchemaVersion`. A mismatch on the next processing cycle resets the manifest, forcing a full reprocess of all journal files for that commander.

## Shared Core Library

`SlevinthHeavenEliteDangerous.Core` is referenced by both the desktop app and API.

- `Events/` — 150+ event classes (one per ED journal event type), `EventBase`, `IEventHandler`
- `Events/POCOs/` — Shared POCO models used across event types
- `Helpers/` — `EventParser` (JSON→EventBase), `FileListener`, `ParentEntry`
- `Models/Web/` — `CommanderProfileDto` and related DTOs returned by the API
