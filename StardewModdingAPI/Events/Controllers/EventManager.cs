using System;
using System.Collections.Generic;
using StardewModdingAPI.Inheritance;

namespace StardewModdingAPI.Events.Controllers
{
    public class EventManager
    {
        private Dictionary<Type, IEventController> eventControllers;

        public EventManager()
        {
            eventControllers = new Dictionary<Type, IEventController>();
        }

        public void Initialize()
        {
            foreach (IEventController eventController in eventControllers.Values)
            {
                eventController.Initialize();
            }
        }

        public void Update()
        {
            foreach (IEventController eventController in eventControllers.Values)
            {
                eventController.Update();
            }
        }

        public void UpdateEventCalls()
        {
            foreach (IEventController eventController in eventControllers.Values)
            {
                eventController.UpdateEventCalls();
            }
        }

        public void RegisterEventController(IEventController eventController)
        {
            if (eventControllers.ContainsKey(eventController.GetType()))
            {
                throw new Exception("An event controller of type \"" + eventController.GetType().ToString() + "\" already exists.");
            }

            eventControllers[eventController.GetType()] = eventController;
        }

        public IEventController GetEventController<T>()
        {
            return eventControllers[typeof(T)];
        }
    }
}
