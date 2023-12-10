using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;


namespace RidgesideVillage
{
    internal class DialogueBoxWithActions : DialogueBox
    {
        private List<Action> ResponseActions;

        internal DialogueBoxWithActions(string dialogue, Response[] responses, List<Action> Actions) : base(dialogue, responses)
        {
            this.ResponseActions = Actions;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (base.safetyTimer <= 0 && StardewModdingAPI.Constants.TargetPlatform == GamePlatform.Android)
            {
                //need to call base on android so this.selectedResponse is set correctly
                base.receiveLeftClick(x, y, playSound);
            }
            int responseIndex = this.selectedResponse;
            base.receiveLeftClick(x, y, playSound);
            //Log.Debug($"selected response {responseIndex}");
            if(base.safetyTimer <= 0 && responseIndex > -1 && responseIndex < this.ResponseActions.Count && this.ResponseActions[responseIndex] != null)
            {
                this.ResponseActions[responseIndex]();
            }
        }
    }
}
