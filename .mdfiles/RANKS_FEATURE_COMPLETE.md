# Commander Ranks Feature - Complete Implementation

## Overview
Added comprehensive rank tracking system that displays all Elite Dangerous ranks with progress bars, updated in real-time as you play.

## Files Created

### Data Models
1. **RankNames.cs** - Complete rank name lookup for all 8 rank types
2. **RankModel.cs** - UI model with progress tracking
3. **RankData.cs** - Added to GeneralControlData.cs for persistence
4. **RankService.cs** - Event handler service
5. **RanksViewModel.cs** - ViewModel connecting service to UI
6. **RanksControl.xaml/.cs** - UI display with cards and progress bars

## Rank Types & Names

### Combat (9 levels)
Harmless → Mostly Harmless → Novice → Competent → Expert → Master → Dangerous → Deadly → **Elite**

### Trade (9 levels)
Penniless → Mostly Penniless → Peddler → Dealer → Merchant → Broker → Entrepreneur → Tycoon → **Elite**

### Exploration (9 levels)
Aimless → Mostly Aimless → Scout → Surveyor → Trailblazer → Pathfinder → Ranger → Pioneer → **Elite**

### Soldier (9 levels)
Defenceless → Mostly Defenceless → Rookie → Soldier → Gunslinger → Warrior → Gladiator → Deadeye → **Elite**

### Exobiologist (9 levels)
Directionless → Mostly Directionless → Compiler → Collector → Cataloguer → Taxonomist → Ecologist → Geneticist → **Elite**

### Empire (15 levels)
None → Outsider → Serf → Master → Squire → Knight → Lord → Baron → Viscount → Count → Earl → Marquis → Duke → Prince → **King**

### Federation (15 levels)
None → Recruit → Cadet → Midshipman → Petty Officer → Chief Petty Officer → Warrant Officer → Ensign → Lieutenant → Lieutenant Commander → Post Commander → Post Captain → Rear Admiral → Vice Admiral → **Admiral**

### CQC (9 levels)
Helpless → Mostly Helpless → Amateur → Semi Professional → Professional → Champion → Hero → Legend → **Elite**

## Events Handled

### RankEvent
- Fired when entering game or after certain actions
- Contains current rank levels (0-8 or 0-14)
- Updates all rank values at once

### ProgressEvent
- Fired periodically during gameplay
- Contains progress % toward next rank (0-100)
- Updates all progress values

### PromotionEvent
- Fired when you achieve a new rank
- Contains the new rank level (only for promoted rank)
- Resets progress to 0% for that rank

## Data Flow

```
Journal Event (Rank/Progress/Promotion)
    ↓
RankService (processes event)
    ↓
Raises RankUpdated or ProgressUpdated event
    ↓
RanksViewModel (subscribes to events)
    ↓
Updates RankModel in ObservableCollection
    ↓
UI updates automatically (data binding)
```

## Data Persistence

**File:** `general_control_data.json`

**Structure:**
```json
{
  "FSDTiming": { ... },
  "Ranks": {
    "Combat": 5,
    "CombatProgress": 67,
    "Trade": 3,
    "TradeProgress": 42,
    ...
  }
}
```

## UI Display

**Layout:** 3-column grid of cards
**Each Card Shows:**
- Rank type name (e.g., "Combat")
- Current rank name (e.g., "Dangerous")
- Rank level number (e.g., "(6)")
- Progress bar (visual)
- Progress percentage (e.g., "67%")

**Card Size:** 280x140px with rounded corners and theme colors

## Integration

- ✅ Added to DI container (ServiceConfiguration.cs)
- ✅ Registered as event handler (StartupService.cs)
- ✅ Integrated into General tab (GeneralControl.xaml)
- ✅ ViewModel initialized on startup
- ✅ Data loaded on app launch
- ✅ Auto-saves on every update

## Testing

1. **Launch app** - Should load saved rank data
2. **Play Elite Dangerous** - Ranks update as events occur
3. **Check progress** - Watch progress bars fill as you play
4. **Get promotion** - Rank name changes, progress resets to 0%
5. **Restart app** - Rank data persists across sessions

---

**Status:** ✅ Complete and ready to use!
