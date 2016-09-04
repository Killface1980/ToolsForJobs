using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace ToolsForJobs
{
    public class WorkGiver_WorkWithTools_Repair : WorkGiver_WorkWithTools
    {
        public IEnumerable<Thing> availableRepairables;
        public Thing closestAvailableRepairable;
        //used to avoid conflict with Construction jobs cause using the same tool workType
        public static bool reserveTool = false;

        public WorkGiver_WorkWithTools_Repair()
        {
            toolWorkTypeName = "TFJ_Tool_Building";
        }

        public void FindAvailableRepairables(Pawn pawn)
        {
            // find all available repairables
            availableRepairables = ListerBuildingsRepairable.RepairableBuildings(pawn.Faction);

            // needed to avoid null references if availableRepairables == 0
            if (availableRepairables.Any())
            {
                // repairable is in home area, can e reserved, not being decontructed, not burning
                availableRepairables = availableRepairables.Where(Repairable => pawn.Faction == Faction.OfPlayer && Find.AreaHome[Repairable.Position] && pawn.CanReserve(Repairable) && Repairable.def.useHitPoints && Repairable.HitPoints < Repairable.MaxHitPoints && Find.DesignationManager.DesignationOn(Repairable, DesignationDefOf.Deconstruct) == null && !Repairable.IsBurning());
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // skip everything if no appropriate tools for this job
            if (!GiveJobIfToolsAvailable(pawn)) return null;

            FindAvailableRepairables(pawn);

            // available repairables present
            if (availableRepairables.Any())
            {
                closestAvailableRepairable = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availableRepairables, PathEndMode.Touch, TraverseParms.For(pawn), 9999);
                // forbid other workgivers to drop tool while these jobs available
                reserveTool = true;

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    //reserve repairable for future work
                    pawn.Reserve(closestAvailableRepairable);
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
                    // finish repairable job allocation
                    return new Job(JobDefOf.Repair, closestAvailableRepairable);
                }
                    // tool in hands, but not appropriate
                if (pawn.equipment.Primary.def.defName.Contains("TFJ_Tool"))
                {
                    //reserve repairable for future work
                    pawn.Reserve(closestAvailableRepairable);
                    // equip appropriate tool
                    return EquipTool(pawn);
                }
                    // not tool in hands
                // reserve repairable for future work
                pawn.Reserve(closestAvailableRepairable);
                // add current weapon to reservation list to reserve it later
                if (WeaponReservations.ContainsKey(pawn))
                    WeaponReservations.Remove(pawn);
                WeaponReservations.Add(pawn, pawn.equipment.Primary);
                // equip appropriate tool
                return EquipTool(pawn);
            }
            // no available repairables
            if (!availableRepairables.Any())
            {
                reserveTool = false;
                // hands occupied
                if (pawn.equipment.Primary != null)
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // has place to store tool and not forbidden to do it
                        if (StorageCellExists(pawn, pawn.equipment.Primary)
                            && WorkGiver_WorkWithTools_ConstructFinishFrames.reserveTool == false
                            && WorkGiver_WorkWithTools_Repair.reserveTool == false
                            && WorkGiver_WorkWithTools_BuildRoof.reserveTool == false)
                         //   && WorkGiver_WorkWithTools_RemoveRoof.reserveTool == false)
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
                        return null;
                    }
                        // something else in hands, skip job
                    return null;
                }
                    // hands free
                // has reserved weapon
                if (WeaponReservations.ContainsKey(pawn))
                {
                    return EquipReservedWeapon(pawn);
                }
                    // skip job
                return null;
            }

            Log.Message("EXCEPTIONAL JOB SKIP");
            return null;
        }
    }
}
