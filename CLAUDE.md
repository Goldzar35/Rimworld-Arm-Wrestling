# Rimworld Arm-Wrestling Mod

## Overview
A RimWorld 1.6 mod by Goldzar that adds an arm wrestling table. Two colonists use it together during recreation time for social joy.

## Project Structure
```
About/About.xml                          - Mod metadata (packageId: Goldzar.RimworldArmWrestling)
Assemblies/RimworldArmWrestling.dll      - Compiled C# assembly (DO NOT edit manually)
Defs/JobDefs/ArmWrestlingJob.xml         - JobDef (defName: ArmWrestle)
Defs/JoyGiverDefs/ArmWrestlingJoyGiver.xml - JoyGiverDef (defName: ArmWrestle)
Defs/ThingDefs/ArmWrestlingTable.xml     - Building ThingDef (defName: ArmWrestlingTable)
Source/JobDriver_ArmWrestle.cs           - Job driver for both pawns
Source/JoyGiver_ArmWrestle.cs           - Joy giver that finds table + partner
Source/RimworldArmWrestling.csproj       - .NET 4.7.2 project, outputs to Assemblies/
Textures/Things/Buildings/               - PNG textures (currently using V2)
```

## How It Works
- `JoyGiver_ArmWrestle` is called when a colonist needs recreation. It finds the closest `ArmWrestlingTable`, checks that the east and west cells are reservable, finds a free partner pawn, gives the partner a job targeting the west cell (`targetC`), and returns an initiator job targeting the east cell.
- `JobDriver_ArmWrestle` is used by both pawns. It reserves `targetC` (the cell, not the building) with `maxPawns=1`, walks to that cell, then gains social joy facing the table.
- The table is **non-rotatable** (`<rotatable>false</rotatable>`) because `Graphic_Single` doesn't support proper rotation.
- Pawns stand on the **east and west** sides of the table (at the pads). Do not change this to use `InteractionCell` — that puts them at the pegs (north/south).

## Building the C# Code
```bash
cd Source
dotnet build RimworldArmWrestling.csproj
```
Output goes to `Assemblies/` automatically. Always rebuild after any `.cs` file change.

## Key RimWorld 1.6 API Notes
- `JoyUtility.JoyTickCheckEnd(pawn, int, JoyTickFullJoyAction, float)` — note the int as 2nd arg (not float)
- `JoyGiverDef` does NOT have `joyGainRate` in 1.6 — put it in the `JobDef` instead
- `JoyGiverDef` requires `<baseChance>` or it will never be selected
- Reserving a building with `maxPawns > 1` and `stackCount = -1` causes warnings and fails — reserve cells instead
- RimWorld DLLs are at: `/Users/goldzar/Library/Application Support/Steam/steamapps/common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed/`
