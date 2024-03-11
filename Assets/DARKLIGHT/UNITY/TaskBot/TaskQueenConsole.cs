namespace Darklight.Unity.Backend
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class TaskQueenConsole
    {
        public struct Tag
        {
            public string name;
            public Guid guidId;
            public Tag(TaskQueen queen)
            {
                this.name = queen.Name;
                this.guidId = queen.GuidId;
            }
            public Tag(TaskBot bot)
            {
                this.name = bot.Name;
                this.guidId = bot.GuidId;
            }
        }
        Dictionary<Tag, List<string>> ConsoleDictionary;
	
    	public TaskQueenConsole()
        {
            ConsoleDictionary = new Dictionary<Tag, List<string>>();
        }

        public void Log(TaskQueen queen, string message)
        {
            Tag queenTag = new Tag(queen);

            if (!ConsoleDictionary.ContainsKey(queenTag))
            {
                ConsoleDictionary.Add(queenTag, new List<string>());
            }
            ConsoleDictionary[queenTag].Add(message);
        }

        public void Log(TaskBot TaskBot, string message)
        {
            Tag botTag = new Tag(TaskBot);

            if (!ConsoleDictionary.ContainsKey(botTag))
            {
                ConsoleDictionary.Add(botTag, new List<string>());
            }

            ConsoleDictionary[botTag].Add(message);
        }

        public List<string> GetActiveConsole()
        {
            List<string> result = new();
            foreach (Tag tag in ConsoleDictionary.Keys)
            {
                result.Add($"{tag.name}: {tag.guidId}");
                foreach (string value in ConsoleDictionary[tag])
                {
                    result.Add($"\t {value}");
                }
            }
            return result;
        }

        public void Reset()
        {
            ConsoleDictionary.Clear();
        }
    }
}