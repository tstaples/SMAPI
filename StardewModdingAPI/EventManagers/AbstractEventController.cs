using System;
using System.Collections.Generic;

namespace StardewModdingAPI.EventManagers
{
    public abstract class AbstractEventController : IEventController
    {
        public virtual void Initialize() { }
        public virtual void Update() { }
        public virtual void UpdateEventCalls() { }
    }
}
