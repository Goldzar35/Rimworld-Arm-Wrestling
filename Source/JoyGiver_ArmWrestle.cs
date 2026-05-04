using RimWorld;
using Verse;
using Verse.AI;

namespace RimworldArmWrestling
{
    public class JoyGiver_ArmWrestle : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            // Find the closest reachable arm wrestling table
            Thing table = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(ThingDef.Named("ArmWrestlingTable")),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                validator: t => !t.IsForbidden(pawn)
            );

            if (table == null)
                return null;

            Building building = (Building)table;
            // Pads are on the east and west sides of the table
            IntVec3 initiatorCell = table.Position + new IntVec3(1, 0, 0);  // East pad
            IntVec3 partnerCell   = table.Position + new IntVec3(-1, 0, 0); // West pad

            // Make sure both cells are standable and reservable
            if (!partnerCell.Standable(pawn.Map))
                return null;
            if (!pawn.CanReserve(initiatorCell))
                return null;
            if (!pawn.CanReserve(partnerCell))
                return null;

            // Find a free partner pawn
            Pawn partner = FindPartner(pawn, partnerCell);
            if (partner == null)
                return null;

            // Give partner their job (opposite side of the table)
            Job partnerJob = JobMaker.MakeJob(def.jobDef, table);
            partnerJob.targetC = partnerCell;
            bool partnerTookJob = partner.jobs.TryTakeOrderedJob(partnerJob, JobTag.SatisfyingNeeds);
            if (!partnerTookJob)
                return null;

            // Return initiator job (interaction cell side)
            Job initiatorJob = JobMaker.MakeJob(def.jobDef, table);
            initiatorJob.targetC = initiatorCell;
            return initiatorJob;
        }

        private Pawn FindPartner(Pawn initiator, IntVec3 partnerCell)
        {
            foreach (Pawn p in initiator.Map.mapPawns.FreeColonistsSpawned)
            {
                if (p == initiator) continue;
                if (p.Drafted) continue;
                if (p.IsPrisoner) continue;
                if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)) continue;
                if (!p.CanReserve(partnerCell)) continue;
                if (!p.CanReach(partnerCell, PathEndMode.OnCell, Danger.None)) continue;
                return p;
            }
            return null;
        }
    }
}
