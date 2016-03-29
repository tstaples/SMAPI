using System;
using System.Collections.Generic;

namespace StardewModdingAPI.Events.Controllers
{
    public interface IEventController
    {
        void Initialize();
        void Update();
        void UpdateEventCalls();
    }
}
