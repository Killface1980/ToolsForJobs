using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace ToolsForJobs
{
    class Genstep_ToolsDropPod : GenStep
    {
        public override void Generate()
        {
            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef handaxeDef = DefDatabase<ThingDef>.GetNamed("TFJ_Tool_Woodcutting_HandAxe");
            ThingDef pickaxeDef = DefDatabase<ThingDef>.GetNamed("TFJ_Tool_Mining_Pickaxe");
            ThingDef hammerDef = DefDatabase<ThingDef>.GetNamed("TFJ_Tool_Building_Hammer");

            Thing handaxe = ThingMaker.MakeThing(handaxeDef, GenStuff.DefaultStuffFor(handaxeDef));
            Thing pickaxe = ThingMaker.MakeThing(pickaxeDef, GenStuff.DefaultStuffFor(pickaxeDef));
            Thing hammer = ThingMaker.MakeThing(hammerDef, GenStuff.DefaultStuffFor(hammerDef));

            listOfThingsToDrop.Add(handaxe);
            listOfThingsToDrop.Add(pickaxe);
            listOfThingsToDrop.Add(hammer);

            DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, true, true);
        }
    }
}
