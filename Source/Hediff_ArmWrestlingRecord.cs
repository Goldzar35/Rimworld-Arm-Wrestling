using Verse;

namespace RimworldArmWrestling
{
    public class Hediff_ArmWrestlingRecord : Hediff
    {
        public override string LabelInBrackets
        {
            get
            {
                var comp = Find.World?.GetComponent<WorldComponent_ArmWrestlingRecords>();
                if (comp == null || pawn == null) return null;
                var record = comp.GetOrCreate(pawn);
                return $"{record.wins}W / {record.losses}L";
            }
        }

        public override bool ShouldRemove => false;
    }
}
