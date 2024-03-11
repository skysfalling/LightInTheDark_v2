namespace Darklight.Unity.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using UnityEngine;


	public class TaskQueenConsole
	{
		private class LogEntry
		{
			public DateTime Timestamp { get; }
			public string Message { get; }
			public LogSeverity Severity { get; }

			public LogEntry(string message, LogSeverity severity)
			{
				Timestamp = DateTime.Now;
				Message = message;
				Severity = severity;
			}
		}

		private Dictionary<Guid, List<LogEntry>> consoleDictionary = new Dictionary<Guid, List<LogEntry>>();

		public void Log<T>(T entity, string message, LogSeverity severity = LogSeverity.Info) where T : ITaskEntity
		{
			if (!consoleDictionary.TryGetValue(entity.GuidId, out List<LogEntry> logEntries))
			{
				logEntries = new List<LogEntry>();
				consoleDictionary[entity.GuidId] = logEntries;
			}

			logEntries.Add(new LogEntry(message, severity));
		}

		public List<string> GetActiveConsole()
		{
			List<string> result = new List<string>();
			StringBuilder sb = new StringBuilder();

			foreach (KeyValuePair<Guid, List<LogEntry>> entry in consoleDictionary)
			{
				// Assuming we have a method to get the name from Guid
				string entityName = GetEntityNameFromGuid(entry.Key); // This needs to be implemented
				sb.AppendLine($"{entityName}: {entry.Key}");

				foreach (LogEntry logEntry in entry.Value)
				{
					sb.Clear();
					sb.Append($"\t[{logEntry.Timestamp:HH:mm:ss}] [{logEntry.Severity}] {logEntry.Message}");
					result.Add(sb.ToString());
				}
			}

			return result;
		}

		public void Reset()
		{
			consoleDictionary.Clear();
		}

		// Implement this method based on your system to resolve entity names from GUIDs
		private string GetEntityNameFromGuid(Guid guid)
		{
			// Example implementation, needs actual logic to map GUID to entity name
			return "EntityNamePlaceholder";
		}
	}

	public enum LogSeverity
	{
		Info,
		Warning,
		Error
	}
}
