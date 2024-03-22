namespace Darklight.Bot
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Debug = UnityEngine.Debug;
	public interface ITaskEntity
	{
		string Name { get; set; }
		Guid GuidId { get; }
	}

	public class TaskBot : IDisposable, ITaskEntity
	{
		private Stopwatch stopwatch;
		private TaskBotQueen taskQueen;
		public Func<Task> task;
		public bool executeOnBackgroundThread = false;
		public string Name { get; set; } = "TaskBot";
		public Guid GuidId { get; } = Guid.NewGuid();
		public long ExecutionTime = 0;
		public TaskBot(TaskBotQueen queenParent, string name, Func<Task> task, bool executeOnBackgroundThread = false)
		{
			stopwatch = Stopwatch.StartNew();
			this.taskQueen = queenParent;
			this.task = task;
			Name = name;
			this.executeOnBackgroundThread = executeOnBackgroundThread;
		}

		public TaskBot(TaskBotQueen queenParent, string name, Task task, bool executeOnBackgroundThread = false)
		{
			stopwatch = Stopwatch.StartNew();
			this.taskQueen = queenParent;
			this.task = () => task;
			Name = name;
			this.executeOnBackgroundThread = executeOnBackgroundThread;
		}

		public async Task ExecuteTask()
		{
			stopwatch.Restart();
			try
			{
				await task();
			}
			catch (OperationCanceledException operation)
			{
				taskQueen.TaskBotConsole.Log($"{Name}: Operation canceled.", 0, Darklight.Console.LogEntry.Severity.Warning);
				Debug.LogWarning($"{Name} || {GuidId} => Operation Canceled: {operation.Message}", taskQueen);
			}
			catch (Exception ex)
			{
				taskQueen.TaskBotConsole.Log($"{Name}: Error encountered. See Unity Console for details.", 0, Darklight.Console.LogEntry.Severity.Error);
				Debug.LogError($"{Name} || {GuidId} => Exception: {ex.Message}\n{ex.StackTrace}", taskQueen);
			}
			finally
			{
				stopwatch.Stop();
				ExecutionTime = stopwatch.ElapsedMilliseconds;
				taskQueen.TaskBotConsole.Log($"{Name}: Execution successful. Time: {stopwatch.ElapsedMilliseconds}ms");
			}
		}

		public void Dispose()
		{
			stopwatch?.Stop();
			stopwatch = null;
		}
	}
}