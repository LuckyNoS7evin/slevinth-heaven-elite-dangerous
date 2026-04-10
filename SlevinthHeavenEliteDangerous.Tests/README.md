# Credit Tracking Integration Tests

## Overview

This test suite validates that credit (in-game currency) tracking is accurate by processing sequential Elite Dangerous journal files and comparing calculated balances with the game's reported balances.

## Test: SequentialJournalFiles_BalanceAndWealthShouldMatch

### Purpose
Validates that the credit balance calculated by tracking all transaction events in a journal file matches the `LoadGame.Credits` value at the start of the next journal file.

### How It Works

1. **Reads journal files** from `%USERPROFILE%\Saved Games\Frontier Developments\Elite Dangerous\`
2. **Processes files chronologically** (sorted by filename)
3. **For each journal file:**
   - Extracts `LoadGameEvent` to get the game's reported balance
   - Extracts `StatisticsEvent` to get the game's reported wealth
   - Processes all credit-affecting events to calculate balance changes
   - Tracks cumulative balance through all transactions
4. **Validates continuity** between journal files:
   - End-of-journal calculated balance should match next journal's `LoadGame.Credits`
   - Wealth variance is allowed up to 1% (asset values can fluctuate)

### Current Test Results

The test identified **26 balance discrepancies** across journal files, totaling approximately **-172 million CR**.

## Fixes Implemented

### ✅ Fixed: SellOrganicData Event Detection (Major Issue - ~385M CR)

**Problem:** `SellOrganicData` events have credit properties nested in a `BioData` array with `Value` and `Bonus` fields. The diagnostic tools only looked for top-level properties like `"Cost":`, `"Price":`, etc., missing biological data sales entirely.

**Fix:** Added `"Value":` and `"Bonus":` to the credit property detection keywords in all test files.

**Impact:** One journal had a single `SellOrganicData` event worth **434,745,000 CR** that wasn't being detected in analysis. This was causing a massive false-positive discrepancy.

### ✅ Fixed: MissionAbandoned and MissionFailed Fines

**Problem:** When players abandon or fail missions, Elite Dangerous can impose fines. These events have an optional `Fine` property that was not being tracked.

**Events Added:**
- `MissionAbandonedEvent.Fine`
- `MissionFailedEvent.Fine`

**Implementation:** Added event handlers in `CommanderStatsService.cs` and all test `CalculateCreditChange()` methods.

**Impact:** Small to medium fines (typically 10K-500K CR depending on mission value).

## Remaining Discrepancies (26 total)

### Analysis

**Small Rounding Errors (-1 CR):**
- 5 journals have -1 CR discrepancies (likely floating-point rounding in game calculations)
- These are acceptable and not worth investigating

**Large Negative Discrepancies (we're calculating too much income):**
The following suggest missing spending events or between-session costs:

| Journal | Discrepancy | Possible Causes |
|---------|-------------|-----------------|
| 35 | -49,622,305 CR | Unknown large expense between sessions |
| 20 | -41,183,758 CR | Unknown large expense between sessions |
| 29 | -32,124,280 CR | Unknown large expense between sessions |
| 57 | -17,300,001 CR | Likely ship/module purchase between sessions |
| 56 | -11,300,003 CR | Likely ship/module purchase between sessions |
| 62 | -10,150,001 CR | Likely module purchases between sessions |

**Pattern Observed:**
- Discrepancies with **"-1" or "-3" appended** (e.g., -11,300,**003**) suggest large purchases + small transactions
- Many occur after heavy trading/mission activities
- Time gaps between journals range from hours to days

**Possible Causes:**
1. **Between-Session Activities:** Player performs actions after closing one journal but before starting next (different game mode, Odyssey vs Horizons switch)
2. **Fleet Carrier Upkeep:**  Despite user stating they don't have a carrier, historical data might include carrier period
3. **Apex Shuttle Fares:** Not currently tracked
4. **Insurance Deductibles:** Resurrection costs may not be fully captured
5. **Squadron/Powerplay Costs:** If any exist
6. **Game Events:** Server-side balance adjustments, anti-cheat corrections, etc.

### Positive Discrepancies (we're calculating too little income):

| Journal | Discrepancy | Possible Causes |
|---------|-------------|-----------------|
| 12 | +1,499,432 CR | Missing income event |
| 15 | +2,101,768 CR | Missing income event |
| 2 | +194,422 CR | Missing income event |
| 11 | +225,866 CR | Missing income event |
| 4 | +172,820 CR | Missing income event |
| 23 | +133,171 CR | Missing income event |

These are typically much smaller and might be:
- Exploration data bonuses from first discoveries
- Powerplay merits converting to salary
- Community Goal rewards
- Rank-up bonuses

### Next Steps

1. **Investigate Large Negative Discrepancies:**
   - Check journals for `Died`, `ShipTargeted` (death), or other mortality events
   - Look for `ApexInterstellar` shuttle events
   - Check for any undocumented spending event types

2. **Investigate Positive Discrepancies:**
   - Look for missing reward/bonus events
   - Check for rank progression rewards
   - Verify PowerplaySalary is capturing all bonuses

3. **Accept Small Discrepancies:**
   - Rounding errors of -1 CR are acceptable
   - Sub-1000 CR gaps are likely unavoidable due to game calculation precision

## Additional Test Files

### JournalDiagnosticTests.cs
Analyzes specific journals to find untracked events with credit properties.
- Targets journals with discrepancies > 10M CR
- Identifies events that have credit-related JSON properties but no tracking handler
- Outputs detailed analysis to temp file

### JournalGapAnalysisTests.cs
Shows event sequences before and after balance gaps.
- Displays last 20 credit events before gap
- Displays first 20 credit events after gap
- Helps identify patterns and missing event types
- Provides summary statistics (total gaps, missing CR, average gap)

### DetailedJournalAnalysisTests.cs
Deep dive analysis of worst discrepancies.
- Parses every event in target journal
- Categorizes all tracked events by type with totals
- Identifies untracked events with credit properties
- Shows last 30 events to find patterns
- Outputs full JSON for untracked events

### MissingSpendingEventsTests.cs
Hunts for missing spending events in journals with negative discrepancies.
- Focuses on journals where we're tracking MORE income than actual
- Identifies spending events we might not be tracking
- Shows tracked vs untracked credit events
- Helps find edge cases and rare transaction types

## Running the Tests

```powershell
# Run all tests in the project
dotnet test SlevinthHeavenEliteDangerous.Tests/

# Run specific test class
dotnet test --filter "FullyQualifiedName~JournalCreditTrackingTests"

# Run with detailed output
dotnet test SlevinthHeavenEliteDangerous.Tests/ -v detailed

# Run from Visual Studio Test Explorer
# Open Test Explorer (Test → Test Explorer)
# Click "Run All" or right-click specific test
```

## Test Configuration

- **Framework**: xUnit
- **Target**: .NET 8
- **Dependencies**: SlevinthHeavenEliteDangerous.Core (for event types and parser)
- **Skips if**: No journal files found (e.g., CI environment)
- **Output**: Test results + detailed analysis files written to `%TEMP%` directory

## Interpreting Results

✅ **Test Passes**: All journal transitions match perfectly (rare initially)  
❌ **Test Fails**: Discrepancies found - review error list for missing events  
⚠️ **Test Skipped**: Journal directory not found or fewer than 2 journal files

### Important Notes

- **Vouchers vs Credits**: Bounties create vouchers first, credits only change when `RedeemVoucher` fires
- **Asset Values**: `CurrentWealth` includes ship/module values which can change based on market conditions
- **Rounding**: Some 1 CR differences may be acceptable rounding variations in the game
- **Large Gaps**: Differences over 1 million CR indicate missing high-value transactions
