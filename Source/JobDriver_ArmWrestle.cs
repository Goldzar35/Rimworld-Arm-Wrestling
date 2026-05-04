using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimworldArmWrestling
{
    public class JobDriver_ArmWrestle : JobDriver
    {
        private Building Table => (Building)job.GetTarget(TargetIndex.A).Thing;
        private IntVec3 MyCell => job.GetTarget(TargetIndex.C).Cell;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Reserve the specific cell this pawn will stand at, not the building
            // This avoids conflicts since each pawn reserves a different cell
            return pawn.Reserve(job.GetTarget(TargetIndex.C), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            // Walk to assigned cell (interaction cell for initiator, opposite side for partner)
            Toil gotoCell = ToilMaker.MakeToil("GotoCell");
            gotoCell.initAction = () => pawn.pather.StartPath(MyCell, PathEndMode.OnCell);
            gotoCell.tickAction = () =>
            {
                if (!pawn.pather.Moving)
                    ReadyForNextToil();
            };
            gotoCell.defaultCompleteMode = ToilCompleteMode.Never;
            gotoCell.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return gotoCell;

            // Arm wrestle
            Toil wrestle = ToilMaker.MakeToil("ArmWrestle");
            wrestle.tickAction = () =>
            {
                pawn.rotationTracker.FaceCell(Table.Position);
                JoyUtility.JoyTickCheckEnd(pawn, 1, JoyTickFullJoyAction.GoToNextToil, 1f);
            };
            wrestle.defaultCompleteMode = ToilCompleteMode.Delay;
            wrestle.defaultDuration = 4000;
            wrestle.socialMode = RandomSocialMode.SuperActive;
            wrestle.handlingFacing = true;
            wrestle.AddFinishAction(() => JoyUtility.TryGainRecRoomThought(pawn));
            yield return wrestle;
        }
    }
}
