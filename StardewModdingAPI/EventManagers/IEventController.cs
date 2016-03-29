using System;
using System.Collections.Generic;

namespace StardewModdingAPI.EventManagers
{
    public interface IEventController
    {
        void Initialize();
        void Update();
        void UpdateEventCalls();
    }
}
