using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using ToolsForJobs;
using Verse;
using Verse.AI;

namespace ToolsForJobs
{
    public class WorkGiver_WorkWithTools_BuildRoof : WorkGiver_WorkWithTools_Scanner
    {

        public WorkGiver_WorkWithTools_BuildRoof()
        {
            toolWorkTypeName = "TFJ_Tool_Building";
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            return Find.AreaBuildRoof.ActiveCells;
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            if (Find.AreaBuildRoof[c] && !c.Roofed() && pawn.CanReserve(c, 1) && (pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn) || BuildingToTouchToBeAbleToBuildRoof(c, pawn) != null) && RoofCollapseUtility.WithinRangeOfRoofHolder(c) && RoofCollapseUtility.ConnectedToRoofHolder(c, true)) return true;
            {

                return false;
            }
        }

        private Building BuildingToTouchToBeAbleToBuildRoof(IntVec3 c, Pawn pawn)
        {
            if (c.Standable())
            {
                return null;
            }
            Building edifice = c.GetEdifice();
            if (edifice == null)
            {
                return null;
            }
            if (!pawn.CanReach(edifice, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn))
            {
                return null;
            }
            return edifice;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c)
        {
            TargetInfo targetB = c;
            if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn))
            {
                targetB = this.BuildingToTouchToBeAbleToBuildRoof(c, pawn);
            }
            return new Job(JobDefOf.BuildRoof, c, targetB);
        }
        public static bool reserveTool = false;

        
    }

}
