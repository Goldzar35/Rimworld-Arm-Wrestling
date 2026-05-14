using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimworldArmWrestling
{
    public class JobDriver_ArmWrestle : JobDriver
    {
        private Building Table => (Building)job.GetTarget(TargetIndex.A).Thing;
        private IntVec3 MyCell => job.GetTarget(TargetIndex.C).Cell;
        private Pawn Opponent => job.GetTarget(TargetIndex.B).Thing as Pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.C), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

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

            Toil wrestle = ToilMaker.MakeToil("ArmWrestle");
            wrestle.tickAction = () =>
            {
                pawn.rotationTracker.FaceCell(Table.Position);
                float joyFactor = pawn.story?.traits?.HasTrait(TraitDef.Named("ArmWrestling_OverTheTop")) == true ? 2f : 1f;
                JoyUtility.JoyTickCheckEnd(pawn, 1, JoyTickFullJoyAction.GoToNextToil, joyFactor);
                // ~1% chance to break an arm over a full 4000-tick session
                if (Rand.MTBEventOccurs(6.6f, 60000f, 1))
                    TryBreakArm();
            };
            wrestle.defaultCompleteMode = ToilCompleteMode.Delay;
            wrestle.defaultDuration = 4000;
            wrestle.socialMode = RandomSocialMode.SuperActive;
            wrestle.handlingFacing = true;
            wrestle.AddFinishAction(() =>
            {
                JoyUtility.TryGainRecRoomThought(pawn);
                ResolveMatch();
            });
            yield return wrestle;
        }

        private void ResolveMatch()
        {
            Pawn opponent = Opponent;
            if (opponent == null || !opponent.Spawned) return;

            // Table may have been destroyed by the time the finish action fires
            Building table = Table;
            if (table == null || !table.Spawned) return;

            // If the opponent has wandered far from the table the match was interrupted — no result
            if (!opponent.Position.InHorDistOf(table.Position, 3f)) return;

            var worldComp = Find.World?.GetComponent<WorldComponent_ArmWrestlingRecords>();
            if (worldComp == null) return;

            // Whichever pawn finishes first resolves; the other finds it already done and skips
            if (!worldComp.TryMarkMatchResolved(pawn, opponent)) return;
            float myScore = MatchScore(pawn, worldComp);
            float opponentScore = MatchScore(opponent, worldComp);

            Pawn winner = myScore >= opponentScore ? pawn : opponent;
            Pawn loser = winner == pawn ? opponent : pawn;

            worldComp.RecordWin(winner);
            worldComp.RecordLoss(loser);

            EnsureRecordHediff(winner);
            EnsureRecordHediff(loser);

            winner.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDef.Named("ArmWrestlingWin"));
            loser.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDef.Named("ArmWrestlingLoss"));

            Messages.Message(
                winner.LabelShortCap + " won the arm wrestling match against " + loser.LabelShort + "!",
                winner,
                MessageTypeDefOf.PositiveEvent
            );
        }

        private static float MatchScore(Pawn p, WorldComponent_ArmWrestlingRecords worldComp)
        {
            var record = worldComp.GetOrCreate(p);
            float manipulation = p.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            float winBonus = Mathf.Min(record.wins * 0.05f, 0.25f);
            float traitBonus = p.story?.traits?.HasTrait(TraitDef.Named("ArmWrestling_OverTheTop")) == true ? 0.2f : 0f;
            return manipulation * (1f + winBonus + traitBonus) * Rand.Value;
        }

        private static void EnsureRecordHediff(Pawn p)
        {
            if (!p.health.hediffSet.HasHediff(HediffDef.Named("ArmWrestlingRecord")))
                p.health.AddHediff(HediffDef.Named("ArmWrestlingRecord"));
        }

        private void TryBreakArm()
        {
            var arms = pawn.health.hediffSet.GetNotMissingParts()
                .Where(r => r.def.defName == "Arm")
                .ToList();

            if (arms.Count == 0) return;

            BodyPartRecord arm = arms.RandomElement();
            pawn.health.AddHediff(HediffDef.Named("Fracture"), arm);

            Messages.Message(
                pawn.LabelShortCap + " broke their arm arm wrestling!",
                pawn,
                MessageTypeDefOf.NegativeEvent
            );
        }
    }
}
