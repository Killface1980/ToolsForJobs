using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace ToolsForJobs
{
    public class WorkGiver_WorkWithTools_PlantsCut : WorkGiver_WorkWithTools
    {
        public IEnumerable<Thing> availablePlants;
        public Thing closestAvailablePlant;

        public WorkGiver_WorkWithTools_PlantsCut()
        {
            toolWorkTypeName = "TFJ_Tool_Woodcutting";
        }

        public void FindAvailablePlants(Pawn pawn)
        {
            // find all plants designated to be cut of harvest
            availablePlants = Find.DesignationManager.allDesignations.FindAll(designation => designation.def == DesignationDefOf.CutPlant || designation.def == DesignationDefOf.HarvestPlant).Select(designation => designation.target.Thing);

            // needed to avoid null references if availablePlants == 0
            var enumerable = availablePlants as Thing[] ?? availablePlants.ToArray();
            if (enumerable.Length > 0)
            {
                // could be reserved and not on fire among them
                availablePlants = enumerable.Where(plant => pawn.CanReserve(plant) && !plant.IsBurning());
            }
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // skip everything if no appropriate tools for this job
            if (!GiveJobIfToolsAvailable(pawn)) return null;

            FindAvailablePlants(pawn);

            // available plants present
            if (availablePlants.Any())
            {
                closestAvailablePlant = GenClosest.ClosestThing_Global_Reachable(pawn.Position, availablePlants, PathEndMode.Touch, TraverseParms.For(pawn), 9999);

                // hands free
                if (pawn.equipment.Primary == null)
                {
                    //reserve plant for future work
                    pawn.Reserve(closestAvailablePlant);
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
                    // cut or harvest plant job allocation
                    foreach (Designation current in Find.DesignationManager.AllDesignationsOn(closestAvailablePlant))
                    {
                        if (current.def == DesignationDefOf.HarvestPlant)
                        {
                            if (current.def == DesignationDefOf.HarvestPlant && !((Plant)closestAvailablePlant).HarvestableNow)
                            {
                                return null;
                            }
                            return new Job(JobDefOf.Harvest, closestAvailablePlant);
                        }
                        if (current.def == DesignationDefOf.CutPlant)
                        {
                            return new Job(JobDefOf.CutPlant, closestAvailablePlant);
                        }
                    }
                }
                // tool in hands, but not appropriate
                else if (pawn.equipment.Primary.def.defName.Contains("TFJ_Tool"))
                {
                    //reserve plant for future work
                    pawn.Reserve(closestAvailablePlant);
                    // equip appropriate tool
                    return EquipTool(pawn);
                }
                // not tool in hands
                else
                {
                    // reserve plant for future work
                    pawn.Reserve(closestAvailablePlant);
                    // add current weapon to reservation list to reserve it later
                    if (WeaponReservations.ContainsKey(pawn))
                        WeaponReservations.Remove(pawn);
                    WeaponReservations.Add(pawn, pawn.equipment.Primary);
                    // equip appropriate tool
                    return EquipTool(pawn);
                }
            }
            // no available plants
            else if (availablePlants.Count() == 0)
            {
                // hands occupied
                if (pawn.equipment.Primary != null)
                {
                    // appropriate tool in hands
                    if (pawn.equipment.Primary.def.defName.Contains(toolWorkTypeName))
                    {
                        // has place to store tool
                        if (StorageCellExists(pawn, pawn.equipment.Primary))
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
