# Rimworld Arm-Wrestling Mod

## Overview
A RimWorld 1.6 mod by Goldzar that adds an arm wrestling table. Two adult colonists use it together during recreation time for social joy. Includes win/loss tracking, mood effects, a custom trait, and a small arm fracture risk.

## Project Structure
```
About/About.xml                               - Mod metadata (packageId: Goldzar.RimworldArmWrestling)
Assemblies/RimworldArmWrestling.dll           - Compiled C# assembly (DO NOT edit manually)
Defs/HediffDefs/ArmWrestlingHediffs.xml       - Cosmetic record hediff (defName: ArmWrestlingRecord)
Defs/JobDefs/ArmWrestlingJob.xml              - JobDef (defName: ArmWrestle)
Defs/JoyGiverDefs/ArmWrestlingJoyGiver.xml   - JoyGiverDef (defName: ArmWrestle)
Defs/ThingDefs/ArmWrestlingTable.xml          - Building ThingDef (defName: ArmWrestlingTable)
Defs/ThoughtDefs/ArmWrestlingThoughts.xml     - Win/loss moodlets (ArmWrestlingWin, ArmWrestlingLoss)
Defs/TraitDefs/ArmWrestlingTraits.xml         - Over the Top trait (ArmWrestling_OverTheTop)
Source/Hediff_ArmWrestlingRecord.cs           - Custom hediff that displays W/L record in Health tab
Source/JobDriver_ArmWrestle.cs                - Job driver for both pawns; resolves match outcome
Source/JoyGiver_ArmWrestle.cs                 - Finds table + partner, assigns seat cells
Source/WorldComponent_ArmWrestlingRecords.cs  - Persists win/loss records across saves
Source/RimworldArmWrestling.csproj            - .NET 4.7.2 project, outputs to Assemblies/
Textures/Things/Buildings/                    - V3 PNG textures, one per rotation (north/east/south/west)
```

## How It Works

### Joy Session
`JoyGiver_ArmWrestle.TryGiveJob` is called when an adult colonist needs recreation. It:
1. Finds the closest reachable `ArmWrestlingTable`
2. Computes the two seat cells based on the table's rotation (see **Seating** below)
3. Checks both cells are standable and reservable
4. Finds a free adult partner who can reach and reserve the partner cell
5. Gives the partner an ordered job with `targetC = partnerCell`, `targetB = initiator`
6. Returns the initiator job with `targetC = initiatorCell`, `targetB = partner`

`JobDriver_ArmWrestle` is used by both pawns. It reserves `targetC` (the cell, not the building), walks to that cell, then runs the wrestling toil facing the table.

### Seating (Rotation-Aware)
The table uses `Graphic_Multi` with 4 textures. Seat cells depend on rotation:
- **North / South** facing → pawns sit at the **north and south** cells
- **East / West** facing → pawns sit at the **east and west** cells

Do not use `InteractionCell` — that puts pawns at the wrong side of the table.

### Match Resolution
At the end of the wrestle toil, whichever pawn's `AddFinishAction` fires first calls `ResolveMatch`. It:
- Checks the table is still spawned (guards against mid-match destruction)
- Checks the opponent is within 3 cells of the table (guards against interrupted matches)
- Calls `WorldComponent_ArmWrestlingRecords.TryMarkMatchResolved` — returns true only once per pair, preventing double resolution
- Scores each pawn: `Manipulation * (1 + winBonus + traitBonus) * Rand.Value`
- Awards win/loss to the WorldComponent, gives moodlets, ensures the record hediff exists

### Win/Loss Record
`WorldComponent_ArmWrestlingRecords` stores a `Dictionary<int, ArmWrestlingRecord>` keyed by `pawn.thingIDNumber`. It persists via `IExposable`. The `Hediff_ArmWrestlingRecord` cosmetic hediff reads from this component and shows the record in `LabelInBrackets` (e.g. `5W / 2L`) in the Health tab.

### Over the Top Trait
Colonists with `ArmWrestling_OverTheTop` (commonality 0.02) gain joy at 2× the normal rate and receive a +20% win score bonus. Checked at runtime via `pawn.story.traits.HasTrait(TraitDef.Named(...))`.

### Arm Fracture
Each tick of the wrestle toil rolls `Rand.MTBEventOccurs(6.6f, 60000f, 1)` — roughly a 1% chance per session. On trigger, a random non-missing arm receives the `Fracture` hediff and a negative message is shown.

## Building the C# Code
```bash
cd Source
dotnet build RimworldArmWrestling.csproj
```
Output goes to `Assemblies/` automatically. Always rebuild after any `.cs` file change.

## Key RimWorld 1.6 API Notes
- `JoyUtility.JoyTickCheckEnd(pawn, int, JoyTickFullJoyAction, float)` — 2nd arg is int, not float
- `JoyGiverDef` does NOT have `joyGainRate` in 1.6 — put it in the `JobDef` instead
- `JoyGiverDef` requires `<baseChance>` or it will never be selected
- Reserve cells (not the building) with `maxPawns=1` — reserving a building with `stackCount=-1` causes warnings
- `WorldComponent` subclasses are auto-discovered via reflection; no XML registration needed
- `Manipulation` is a pawn capacity (`PawnCapacityDefOf.Manipulation`), not a stat — use `health.capacities.GetLevel()`
- `TraitDef` uses `<degreeDatas>` in 1.6, not `<degrees>`
- RimWorld DLLs are at: `/Users/goldzar/Library/Application Support/Steam/steamapps/common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/`
