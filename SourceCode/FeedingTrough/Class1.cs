using RimWorld;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace FeedingTrough
{
    public class FeedingTrough : Building_Storage
    {
        public bool dynamicOn = false;
        public int lowValue = 20;
        public string lowText = "meh"; //no idea why the text field needs a reference to a string, but I'm not using it.

        public int highValue = 90;
        public string highText = "meh"; //ditto

        public override void TickRare()
        {

            //Messages.Message("Tick", MessageSound.Silent);
            //run whatever the base wants to do
            base.TickRare();
            if( dynamicOn == true)
            {
                //count up the contents of the trough
                float totalItems = 0;
                float maxItems = 0;
                int stacks = 0;
                foreach (Thing t in base.slotGroup.HeldThings)
                {
                    totalItems += t.stackCount;
                    maxItems += t.def.stackLimit;
                    stacks += 1;
                }

                //the calculation of maxItems is incorrect if the trough has less than three stacks it, adjusting...
                if( stacks == 0 )
                {
                    maxItems = 1f;
                }
                else if (stacks == 1)
                {
                    maxItems *= 3f;
                }
                else if (stacks == 2)
                {
                    maxItems *= 1.5f;
                }

                //adjust the priorty based on how full the trough is
                if (totalItems / maxItems > (this.highValue / 100.0) && base.settings.Priority != StoragePriority.Normal)
                {
                    Messages.Message("Setting Trough to normal priority", MessageSound.Silent);
                    base.settings.Priority = StoragePriority.Normal;
                }
                if (totalItems / maxItems < (this.lowValue / 100.0) && base.settings.Priority != StoragePriority.Important)
                {
                    Messages.Message("Setting Trough to important priority", MessageSound.Silent);
                    base.settings.Priority = StoragePriority.Important;
                }
            }          

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            var l = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("dynamicp", true),
                defaultLabel = "Configure Dynamic Priorities",
                defaultDesc = "Configure Dynamic Priorities",
                action = delegate
                {
                    Find.WindowStack.Add(new FeedingTrough_DynamicP(this));
                }
            };
            yield return l;
        }
    }

    public class FeedingTrough_DynamicP : Window
    {
        private FeedingTrough feedingTrough;

        public FeedingTrough_DynamicP(FeedingTrough ft)
        {
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.closeOnEscapeKey = true;
            this.feedingTrough = ft;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(525f, 200f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleBar = new Rect(inRect.x, 0f, inRect.width, 30f);
            TextAnchor bak = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleBar, "Dynamic Priorities");
            Text.Anchor = bak; //I have absolutely no idea why this is required, but it stops an exception being thrown

            Rect onBar = new Rect(inRect.x, 30f, inRect.width, 30f);
            Widgets.CheckboxLabeled(onBar,"Turn on Dynamic Priorities?", ref feedingTrough.dynamicOn);


            Rect lowBar1 = new Rect(inRect.x, 60f, inRect.width, 40f);
            Widgets.TextFieldNumericLabeled(lowBar1, "Set to important when below x% of storage capacity", ref feedingTrough.lowValue, ref feedingTrough.lowText, 0, 100);

            Rect highBar1 = new Rect(inRect.x, 100f, inRect.width, 40f);
            Widgets.TextFieldNumericLabeled(highBar1, "Set to normal when above x% of storage capacity", ref feedingTrough.highValue, ref feedingTrough.highText, 0, 100);

        }
    }
}

