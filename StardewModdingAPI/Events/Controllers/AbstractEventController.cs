using System;
using System.Collections.Generic;
using StardewModdingAPI.Inheritance;

namespace StardewModdingAPI.Events.Controllers
{
    public abstract class AbstractEventController : IEventController
    {
        protected SGame game { get; private set; }

        public AbstractEventController(SGame game)
        {
            this.game = game;
        }

        public virtual void Initialize() { }
        public virtual void Update() { }
        public virtual void UpdateEventCalls() { }
    }
}
