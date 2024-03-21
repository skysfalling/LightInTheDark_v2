namespace Darklight
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	public class Console
	{
		public enum LogSeverity { Info, Warning, Error }
		public class LogEntry
		{
			public DateTime Timestamp { get; }
			public string Message { get; }
			public LogSeverity Severity { get; }
			public LogEntry(string message, LogSeverity severity = LogSeverity.Info)
			{
				Timestamp = DateTime.Now;
				Message = message;
				Severity = severity;
			}
		}
		private List<LogEntry> allLogEntries = new List<LogEntry>();
		public void Log(string message)
		{

		}

		public List<string> GetActiveConsole()
		{
			List<string> entryList = new List<string>();
			foreach (LogEntry log in allLogEntries)
			{
				string newMessage = $"[{GetTimestamp(log)}] {log.Message}";
				entryList.Add(newMessage);
			}
			return entryList;
		}

		public string GetTimestamp(LogEntry logEntry)
		{
			return logEntry.Timestamp.ToString("hh:mm:ss:ff");
		}

		public void Reset()
		{
			allLogEntries.Clear();
		}
	}
}