using System;
using System.Collections.Generic;
using RimWorld;
using ToolsForJobs;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ToolsForJobs
{
    public class WorkGiver_WorkWithTools_RemoveRoof : WorkGiver_WorkWithTools_Scanner
    {
        public WorkGiver_WorkWithTools_RemoveRoof()
        {
            toolWorkTypeName = "TFJ_Tool_Building";
        }

        public override bool Prioritized
        {
            get
            {
                return true;
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            return Find.AreaNoRoof.ActiveCells;
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            if (Find.AreaNoRoof[c] && c.Roofed() && pawn.CanReserveAndReach(c, PathEndMode.ClosestTouch, pawn.NormalMaxDanger(), 1)) return true;
            {
                return false;
            }
        }
        public static bool reserveTool = false;

        public override Job JobOnCell(Pawn pawn, IntVec3 c)
        {
            reserveTool = true;

            // hands free
            if (pawn.equipment.Primary == null)
            {
                //reserve frame for future work
                pawn.Reserve(c);
                return EquipTool(pawn);
            }
            // hands occupied
            // appropriate tool in hands
            if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
            {
                // if reservation of previous weapon was not made, make one
                if (WeaponReservations.ContainsKey(pawn) && pawn.CanReserve(WeaponReservations[pawn]))
                {
                    pawn.Reserve(WeaponReservations[pawn]);
                }
                // finish frame job allocation
                return new Job(JobDefOf.RemoveRoof, c, c);
            }
            // tool in hands, but not appropriate
            if (pawn.equipment.Primary.def.defName.Contains("TFJ_Tool"))
            {
                //reserve frame for future work
                pawn.Reserve(c);
                // equip appropriate tool
                return EquipTool(pawn);
            }
            // not tool in hands
            // reserve frame for future work
            pawn.Reserve(c);
            // add current weapon to reservation list to reserve it later
            if (WeaponReservations.ContainsKey(pawn))
                WeaponReservations.Remove(pawn);
            WeaponReservations.Add(pawn, pawn.equipment.Primary);
            // equip appropriate tool
            return EquipTool(pawn);

        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            IntVec3 cell = t.Cell;
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                IntVec3 c = cell + GenAdj.AdjacentCells[i];
                if (c.InBounds())
                {
                    Building edifice = c.GetEdifice();
                    if (edifice != null && edifice.def.holdsRoof)
                    {
                        return -60f;
                    }
                    if (c.Roofed())
                    {
                        num++;
                    }
                }
            }
            return (float)(-(float)Mathf.Min(num, 3));
        }
        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            if (reserveTool)
            {
                reserveTool = false;
                // hands occupied
                if (pawn.equipment.Primary != null)
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // has place to store tool
                        if (StorageCellExists(pawn, pawn.equipment.Primary)
                            && WorkGiver_WorkWithTools_ConstructFinishFrames.reserveTool == false
                        && WorkGiver_WorkWithTools_Repair.reserveTool == false
                        && WorkGiver_WorkWithTools_BuildRoof.reserveTool == false
                        && WorkGiver_WorkWithTools_RemoveRoof.reserveTool == false)
                        {
                            // store it
                            return ReturnTool(pawn);
                        }
                        // no storage, but has reserved weapon
                        if (WeaponReservations.ContainsKey(pawn))
                        {
                            return EquipReservedWeapon(pawn);
                        }
                        // no storage, no reserve, keep the tool
                    }
                    // something else in hands, skip job
                }
                // hands free
                // has reserved weapon
                if (WeaponReservations.ContainsKey(pawn))
                {
                    return EquipReservedWeapon(pawn);
                }
                // skip job 
            }
            return null;
        }
    }

}
