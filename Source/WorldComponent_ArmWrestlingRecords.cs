using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimworldArmWrestling
{
    public class ArmWrestlingRecord : IExposable
    {
        public int wins;
        public int losses;

        public void ExposeData()
        {
            Scribe_Values.Look(ref wins, "wins", 0);
            Scribe_Values.Look(ref losses, "losses", 0);
        }
    }

    public class WorldComponent_ArmWrestlingRecords : WorldComponent
    {
        private Dictionary<int, ArmWrestlingRecord> records = new Dictionary<int, ArmWrestlingRecord>();
        private List<int> keysWorkingList;
        private List<ArmWrestlingRecord> valuesWorkingList;

        // Not persisted — only needed within a play session to prevent double-resolution
        private readonly HashSet<long> resolvedMatchKeys = new HashSet<long>();

        public WorldComponent_ArmWrestlingRecords(World world) : base(world) { }

        public ArmWrestlingRecord GetOrCreate(Pawn pawn)
        {
            if (!records.TryGetValue(pawn.thingIDNumber, out var record))
            {
                record = new ArmWrestlingRecord();
                records[pawn.thingIDNumber] = record;
            }
            return record;
        }

        public void RecordWin(Pawn pawn) => GetOrCreate(pawn).wins++;
        public void RecordLoss(Pawn pawn) => GetOrCreate(pawn).losses++;

        // Returns true the first time this pair is resolved; false if already done.
        // Whichever pawn finishes first calls this and gets to resolve.
        public bool TryMarkMatchResolved(Pawn a, Pawn b)
        {
            int lo = a.thingIDNumber < b.thingIDNumber ? a.thingIDNumber : b.thingIDNumber;
            int hi = a.thingIDNumber < b.thingIDNumber ? b.thingIDNumber : a.thingIDNumber;
            long key = ((long)lo << 32) | (uint)hi;
            return resolvedMatchKeys.Add(key);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref records, "armWrestlingRecords", LookMode.Value, LookMode.Deep, ref keysWorkingList, ref valuesWorkingList);
            if (records == null) records = new Dictionary<int, ArmWrestlingRecord>();
        }
    }
}
