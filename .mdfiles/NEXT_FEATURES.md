# Next Feature Suggestions

Ideas for what to build next, roughly ordered by complexity.

---

## 1. Mission Tracker

Track active and completed missions from the journal.

**Events to handle:** `MissionAccepted`, `MissionCompleted`, `MissionAbandoned`, `MissionFailed`, `MissionRedirected`, `Missions`

**What to show:**
- List of active missions with name, faction, destination system, expiry time
- Mission type badge (kill, delivery, courier, salvage, etc.)
- Reward amount
- Count of completed / failed / abandoned missions this session / all time

**Notes:** `MissionsEvent` fires on game load with the full active mission list — good for restoring state. `MissionFailedEvent` was added to the event library; it should be tracked alongside abandoned missions.

---

## 2. Combat Log

Track kills, bounties collected, and crime stats.

**Events to handle:** `Bounty`, `CapShipBond`, `FactionKillBond`, `PVPKill`, `CommitCrime`, `CrimeVictim`, `Died`, `UnderAttack`, `Interdiction`, `InterdictionEscaped`, `Interdicted`, `HullDamage`, `ShieldState`

**What to show:**
- NPC kill count and PVP kill count (tracked separately via `PVPKill`)
- Total bounty + combat bond credits earned
- Capital ship bond credits (`CapShipBond`) — useful during CG/war zones
- Breakdown by victim faction
- Deaths counter
- Interdictions: attempted vs escaped
- Hull integrity low-water mark this session (from `HullDamage`)
- Shield up/down events (from `ShieldState`) — useful for dangerous encounters

**Notes:** `BountyEvent` already has `Rewards` (per-faction) and `TotalReward`. `FactionKillBondEvent` covers conflict zone payouts. `PVPKillEvent` records player kills separately.

---

## 3. Mining Tracker

Track mining activity and materials collected.

**Events to handle:** `AsteroidCracked`, `MiningRefined`, `ProspectedAsteroid`, `CargoDepot`, `MarketSell`

**What to show:**
- Materials refined this session (type + quantity)
- Total units mined
- Prospector results: yield percentages per asteroid (`ProspectedAsteroid` includes `Content`, `Remaining`, `Materials`)
- Estimated sell value (if market data available)
- Limpet usage (via `BuyDrones` + `EjectCargo`)

**Notes:** `ProspectedAsteroidEvent` was added and gives `MotherlodeMaterial` — great for spotting high-value rocks before cracking.

---

## 4. Engineer Progress Tracker

Show engineering unlock status and material progress.

**Events to handle:** `EngineerProgress`, `EngineerContribution`, `EngineerCraft`, `MaterialCollected`, `Materials`

**What to show:**
- Each engineer: name, location system, unlock status, rank achieved
- Progress bar toward next rank where applicable
- Count of contributions made
- Recent crafts (blueprint name, level, from `EngineerCraft`)

**Notes:** `EngineerProgressEvent` already has `Engineers` array with `EngineerID`, `Name`, `Progress`, `Rank`, `RankProgress`, `Status`. `EngineerCraftEvent` records what was actually built.

---

## 5. Material Inventory

Track raw/manufactured/encoded materials for engineering.

**Events to handle:** `Materials`, `MaterialCollected`, `MaterialDiscarded`, `MaterialDiscovered`, `MaterialTrade`

**What to show:**
- Three sections: Raw / Manufactured / Encoded
- Each material: name, current count, max capacity (raw=300, others=1000 total)
- Highlight materials at max capacity
- Recent trades (from `MaterialTrade`) — what was exchanged at material traders
- Filter/search

**Notes:** `MaterialsEvent` gives the full snapshot on game load. `MaterialCollected` gives delta updates. `MaterialTrade` records material trader exchanges; useful for showing efficiency.

---

## 6. Fleet Carrier Tracker *(if you have one)*

Track carrier jump history, market, and finances.

**Events to handle:** `CarrierJump`, `CarrierJumpRequest`, `CarrierJumpCancelled`, `CarrierBuy`, `CarrierStats`, `CarrierFinance`, `CarrierBankTransfer`, `CarrierDepositFuel`, `CarrierCrewServices`, `CarrierTradeOrder`, `CarrierShipPack`, `CarrierModulePack`, `CarrierNameChanged`, `CarrierDecommission`, `CarrierCancelDecommission`, `CarrierDockingPermission`

**What to show:**
- Current carrier name, callsign, location
- Tritium fuel level
- Jump history (system + timestamp); show pending jump from `CarrierJumpRequest`
- Finance summary (balance, running costs, bank transfers)
- Active trade orders
- Decommission status if initiated

**Notes:** Many more carrier events are now in the library than were listed before. `CarrierJumpRequestEvent` lets you show a "jump pending" indicator before it fires.

---

## 7. Market Price Tracker

Log buy/sell prices encountered at stations.

**Events to handle:** `Market`, `MarketBuy`, `MarketSell`, `SellExplorationData`, `MultiSellExplorationData`, `SellOrganicData`

**What to show:**
- History of commodities bought/sold with station, price, and profit
- Best buy/sell prices seen for a commodity across visited stations
- Session profit summary broken out by type: trade, exploration data, organic data sales
- `SellOrganicData` gives system/body/species details already — cross-reference with exobio log

**Notes:** `MultiSellExplorationDataEvent` is the batch exploration data sale event; `SellOrganicDataEvent` records exobiology sales with value per entry.

---

## 8. Codex Discovery Tracker

Track first codex entries discovered.

**Events to handle:** `CodexEntry`

**What to show:**
- List of codex entries logged this session / all time
- Category and subcategory (e.g. "Stellar Bodies > Neutron Stars")
- System and body where discovered
- Whether it was a first discovery (`IsNewEntry`)
- Total unique entries discovered

**Notes:** `CodexEntryEvent` includes `Name_Localised`, `Category_Localised`, `SubCategory_Localised`, `System`, `Body`, `IsNewEntry`. Pairs naturally with the existing exobiology and visited-systems data.

---

## 9. Signal Source (USS) Tracker

Log unidentified signal source encounters.

**Events to handle:** `FSSSignalDiscovered`, `USSDrop`

**What to show:**
- Signal type (encoded, weapons, combat, salvage, high grade emissions, etc.)
- Threat level (from `FSSSignalDiscovered`)
- System where found
- Count per signal type this session

**Notes:** `FSSSignalDiscoveredEvent` fires when the FSS scanner finds a signal; `USSDropEvent` fires when you drop into one. High-grade emission signals are the ones that drop rare mats — worth flagging visually.

---

## 10. Powerplay Tracker

Track powerplay activity and standing.

**Events to handle:** `Powerplay`, `PowerplayJoin`, `PowerplayLeave`, `PowerplayDefect`, `PowerplayCollect`, `PowerplayDeliver`, `PowerplaySalary`, `PowerplayFastTrack`, `PowerplayVote`, `PowerplayVoucher`

**What to show:**
- Current power pledged to, rank, and merits
- Merits earned this session (collect + deliver)
- Salary received
- Vouchers redeemed
- Vote cast this cycle
- History of power changes (join/leave/defect)

**Notes:** `PowerplayEvent` fires on game load with current state. `PowerplayDeliverEvent` and `PowerplayCollectEvent` give merit counts per action.

---

## 11. Squadron Tracker

Track squadron membership and activity.

**Events to handle:** `SquadronStartup`, `SquadronCreated`, `JoinedSquadron`, `AppliedToSquadron`, `InvitedToSquadron`, `LeftSquadron`, `DisbandedSquadron`, `KickedFromSquadron`, `SquadronPromotion`, `SquadronDemotion`, `WonATrophyForSquadron`, `SharedBookmarkToSquadron`

**What to show:**
- Current squadron name and rank
- Promotion / demotion history
- Trophies won
- Shared bookmarks sent

**Notes:** `SquadronStartupEvent` fires on game load and gives the current squadron state — good for restoring display without replaying history.

---

## 12. On-Foot / Odyssey Tracker

Track Odyssey on-foot activity — suits, weapons, backpack, settlements.

**Events to handle:** `Disembark`, `Embark`, `ApproachSettlement`, `BuySuit`, `SellSuit`, `UpgradeSuit`, `SuitLoadout`, `BuyWeapon`, `SellWeapon`, `UpgradeWeapon`, `Backpack`, `BackpackChange`, `DropItems`, `CollectItems`, `UseConsumable`, `BookTaxi`, `CancelTaxi`, `BookDropship`, `CancelDropship`, `ScanOrganic`, `SellOrganicData`

**What to show:**
- Currently equipped suit and loadout
- Weapon inventory with upgrade levels
- Backpack contents (consumables, items, data, components)
- Settlement visits this session
- Taxi / dropship trips taken
- On-foot organic scans (links naturally to existing exobiology feature)

**Notes:** `BackpackEvent` gives a full snapshot on embark/disembark. `ScanOrganicEvent` is already partially handled by the exobio service — on-foot scanning state could integrate here.

---

## 13. Notification / Alert System

Raise in-app toast notifications for important events.

**Candidates:**
- FSD interdiction attempt
- Under attack / hull below threshold (from `HullDamage`)
- Shield state change in a dangerous system (from `ShieldState`)
- Low fuel warning (`FuelScoop` not triggered before threshold)
- Mission expiring soon
- Bounty earned above threshold (e.g. > 1,000,000 Cr)
- New codex discovery (`CodexEntry` with `IsNewEntry = true`)
- High-grade emission signal found (`FSSSignalDiscovered` with threat level)
- Carrier jump scheduled (`CarrierJumpRequest`)

**Notes:** WinUI 3 supports `AppNotificationBuilder` (Windows App SDK toast notifications). Could also send a Discord message for high-value events.

---

## 14. Settings / Configuration UI

Give users control over app behaviour without editing files.

**What to include:**
- Discord webhook URL input (currently edited in config file)
- Toggle: enable/disable diagnostic reports to Discord
- Toggle: enable/disable individual Discord notification types
- Journal folder path override (default is `%UserProfile%\Saved Games\Frontier Developments\Elite Dangerous`)
- Audio announcement voice selection (Edge TTS voice name)
- Theme: light / dark / system

**Notes:** `ApiConfigService` already persists some of this. A dedicated settings control would surface those values in the UI.

---

## 15. Export / Share

Let users export their data.

**Options:**
- Export exobiology discoveries to CSV / JSON
- Export visited systems list to CSV
- Copy current system info to clipboard
- Generate a "session summary" (systems visited, bodies scanned, exobio found, bounties earned, etc.)
- Export codex entries to CSV

---

## Quick Wins (small, self-contained)

| Idea | Events / Source | Notes |
|------|----------------|-------|
| **Session timer** | `LoadGame`, `Shutdown` | Show how long the current game session has been running |
| ~~**Credits display**~~ ❌ | `LoadGame`, `Statistics` | Already shown on General tab |
| **Ship name/ident display** | `LoadGame`, `Loadout` | Show ship type, name, and callsign on General tab |
| ~~**Fuel gauge**~~ ❌ | `FuelScoop`, `ReservoirReplenished` | Displayed in-game, no tracking needed |
| **Current body info** | `ApproachBody`, `LeaveBody`, `Scan` | Show landable status, gravity, atmosphere of targeted body |
| ~~**Nav route indicator**~~ ✅ | `NavRoute`, `NavRouteClear`, `FSDTarget` | Shows next system, jumps remaining, and final destination |
| **Docked status** | `Docked`, `Undocked` | Show current station name, services available, and time docked |
| **SRV / Fighter status** | `LaunchSRV`, `DockSRV`, `SRVDestroyed`, `LaunchFighter`, `FighterDestroyed` | Show whether SRV/fighter is deployed; flag destructions |
| ~~**Shield state indicator**~~ ❌ | `ShieldState` | Displayed in-game, no tracking needed |
| **Community goal progress** | `CommunityGoal`, `CommunityGoalJoin`, `CommunityGoalReward` | Show active CGs the commander has joined with contribution tier |
