using System;
using System.Collections.Generic;
using StardewModdingAPI.Inheritance;

namespace StardewModdingAPI.Events.Controllers
{
    public class GameEventsController : AbstractEventController
    {
        /// <summary>
        /// Whether or not this update frame is the very first of the entire game
        /// </summary>
        public bool FirstUpdate { get; private set; }

        /// <summary>
        /// The current index of the update tick. Recycles every 60th tick to 0. (Int32 between 0 and 59)
        /// </summary>
        public int CurrentUpdateTick { get; private set; }

        public GameEventsController(SGame game) : base (game)
        {
        }

        public override void Initialize()
        {
            FirstUpdate = true;
        }

        public override void Update()
        {
            GameEvents.InvokeUpdateTick();
            if (FirstUpdate)
            {
                GameEvents.InvokeFirstUpdateTick();
                FirstUpdate = false;
            }

            if (CurrentUpdateTick % 2 == 0)
                GameEvents.InvokeSecondUpdateTick();

            if (CurrentUpdateTick % 4 == 0)
                GameEvents.InvokeFourthUpdateTick();

            if (CurrentUpdateTick % 8 == 0)
                GameEvents.InvokeEighthUpdateTick();

            if (CurrentUpdateTick % 15 == 0)
                GameEvents.InvokeQuarterSecondTick();

            if (CurrentUpdateTick % 30 == 0)
                GameEvents.InvokeHalfSecondTick();

            if (CurrentUpdateTick % 60 == 0)
                GameEvents.InvokeOneSecondTick();

            CurrentUpdateTick += 1;
            if (CurrentUpdateTick >= 60)
                CurrentUpdateTick = 0;
        }

        public override void UpdateEventCalls()
        {

        }
    }
}
