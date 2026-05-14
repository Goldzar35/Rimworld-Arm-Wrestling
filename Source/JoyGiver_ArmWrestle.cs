using RimWorld;
using Verse;
using Verse.AI;

namespace RimworldArmWrestling
{
    public class JoyGiver_ArmWrestle : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.DevelopmentalStage.Juvenile()) return null;

            Thing table = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(ThingDef.Named("ArmWrestlingTable")),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                validator: t => !t.IsForbidden(pawn)
            );

            if (table == null)
                return null;

            Building building = (Building)table;
            GetSeatCells(building, out IntVec3 initiatorCell, out IntVec3 partnerCell);

            if (!initiatorCell.Standable(pawn.Map))
                return null;
            if (!partnerCell.Standable(pawn.Map))
                return null;
            if (!pawn.CanReserve(initiatorCell))
                return null;
            if (!pawn.CanReserve(partnerCell))
                return null;

            Pawn partner = FindPartner(pawn, partnerCell);
            if (partner == null)
                return null;

            Job partnerJob = JobMaker.MakeJob(def.jobDef, table);
            partnerJob.targetC = partnerCell;
            partnerJob.targetB = pawn;
            if (!partner.jobs.TryTakeOrderedJob(partnerJob, JobTag.SatisfyingNeeds))
                return null;

            Job initiatorJob = JobMaker.MakeJob(def.jobDef, table);
            initiatorJob.targetC = initiatorCell;
            initiatorJob.targetB = partner;
            return initiatorJob;
        }

        private static void GetSeatCells(Building table, out IntVec3 cellA, out IntVec3 cellB)
        {
            // North/South sprites are portrait — long axis runs north-south, pads at north and south ends
            // East/West sprites are landscape — long axis runs east-west, pads at east and west ends
            if (table.Rotation == Rot4.North || table.Rotation == Rot4.South)
            {
                cellA = table.Position + new IntVec3(0, 0, 1);  // North
                cellB = table.Position + new IntVec3(0, 0, -1); // South
            }
            else
            {
                cellA = table.Position + new IntVec3(1, 0, 0);  // East
                cellB = table.Position + new IntVec3(-1, 0, 0); // West
            }
        }

        private Pawn FindPartner(Pawn initiator, IntVec3 partnerCell)
        {
            foreach (Pawn p in initiator.Map.mapPawns.FreeColonistsSpawned)
            {
                if (p == initiator) continue;
                if (p.DevelopmentalStage.Juvenile()) continue;
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
