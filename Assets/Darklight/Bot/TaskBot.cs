namespace Darklight.Bot
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using UnityEngine;
	using static Darklight.Bot.Console;
	using Debug = UnityEngine.Debug;
	public interface ITaskEntity
	{
		string Name { get; set; }
		Guid GuidId { get; }
	}

	public class TaskBot : IDisposable, ITaskEntity
	{
		private Stopwatch stopwatch;
		private TaskBotQueen queenParent;
		public Func<Task> task;
		public bool executeOnBackgroundThread = false;

		public string Name { get; set; } = "TaskBot";
		public Guid GuidId { get; } = Guid.NewGuid();
		public long ExecutionTime = 0;
		public TaskBot(TaskBotQueen queenParent, string name, Func<Task> task, bool executeOnBackgroundThread = false)
		{
			stopwatch = Stopwatch.StartNew();
			this.queenParent = queenParent;
			this.task = task;
			Name = name;
			this.executeOnBackgroundThread = executeOnBackgroundThread;
		}

		public TaskBot(TaskBotQueen queenParent, string name, Task task, bool executeOnBackgroundThread = false)
		{
			stopwatch = Stopwatch.StartNew();
			this.queenParent = queenParent;
			this.task = () => task;
			Name = name;
			this.executeOnBackgroundThread = executeOnBackgroundThread;
		}

		public virtual async Task ExecuteTask()
		{
			queenParent.TaskBotConsole.Log(this, $"\t\t >> Execute bot! {Name}");
			stopwatch.Reset();
			try
			{
				await task();
			}
			catch (OperationCanceledException operation)
			{
				queenParent.TaskBotConsole.Log(this, $"\t\t OperationCanceled: See Unity Console", LogSeverity.Error);
				Debug.LogError(operation, queenParent);
			}
			catch (Exception ex)
			{
				queenParent.TaskBotConsole.Log(this, $"\t\t Error: See Unity Console", LogSeverity.Error);
				queenParent.TaskBotConsole.Log(this, $"\t\t\t {this.Name} || {this.GuidId}", LogSeverity.Error);
				Debug.LogError($"{this.Name} || {this.GuidId} => {ex}" + ex.StackTrace, queenParent);
			}
			finally
			{
				stopwatch.Stop();
				ExecutionTime = stopwatch.ElapsedMilliseconds;
				queenParent.TaskBotConsole.Log(this, $"\t\t >> SUCCESS! Execution Time : {ExecutionTime}");
			}
		}

		public virtual void Dispose()
		{
			stopwatch.Stop();
			stopwatch.Reset();
			stopwatch = null;
		}
	}
}