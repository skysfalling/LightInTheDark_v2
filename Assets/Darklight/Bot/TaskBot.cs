namespace Darklight.Bot
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using UnityEngine;
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
			stopwatch.Reset();
			try
			{
				await task();
			}
			catch (OperationCanceledException operation)
			{
				queenParent.TaskBotConsole.Log($"OperationCanceled: See Unity Console");
				Debug.LogError(operation, queenParent);
			}
			catch (Exception ex)
			{
				queenParent.TaskBotConsole.Log($"\t\t Error: See Unity Console");
				queenParent.TaskBotConsole.Log($"\t\t\t {this.Name} || {this.GuidId}");
				Debug.LogError($"{this.Name} || {this.GuidId} => {ex}" + ex.StackTrace, queenParent);
			}
			finally
			{
				stopwatch.Stop();
				ExecutionTime = stopwatch.ElapsedMilliseconds;
				queenParent.TaskBotConsole.Log($"\t\t >> SUCCESS! Execution Time : {ExecutionTime}");
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