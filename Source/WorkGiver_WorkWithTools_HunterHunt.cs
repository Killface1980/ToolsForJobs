using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace ToolsForJobs
{
    class WorkGiver_WorkWithTools_HunterHunt : WorkGiver_HunterHunt
    {
        public override bool ShouldSkip(Pawn pawn)
        {
            return !WorkGiver_HunterHunt.HasHuntingWeapon(pawn) || pawn.equipment.Primary.def.defName.Contains("TFJ_Tool") || WorkGiver_HunterHunt.HasShieldAndRangedWeapon(pawn);
        }
    }
}
