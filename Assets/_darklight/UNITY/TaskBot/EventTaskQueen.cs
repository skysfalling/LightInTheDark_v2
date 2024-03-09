using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Unity.Backend
{    
    public class EventTaskQueen : MonoBehaviour 
    {
        public string taskQueenName = "EventTaskQueen";
        public List<EventTaskBot> eventTaskBots = new();

        public void ExecuteAllBotsInQueue()
        {
            Queue<EventTaskBot> queue = new Queue<EventTaskBot>(eventTaskBots);

            while (queue.Count > 0)
            {
                EventTaskBot eventTaskBot = queue.Dequeue();
                eventTaskBot.Execute();

                eventTaskBot.Dispose();
            }
        }
    }
}