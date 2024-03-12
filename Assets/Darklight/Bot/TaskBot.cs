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
		private TaskQueen queenParent;
		public Func<Task> task;
		public bool executeOnBackgroundThread = false;

		public string Name { get; set; } = "TaskBot";
		public Guid GuidId { get; } = Guid.NewGuid();
		public long ExecutionTime = 0;
		public TaskBot(TaskQueen queenParent, string name, Func<Task> task, bool executeOnBackgroundThread = false)
		{
			stopwatch = Stopwatch.StartNew();
			this.queenParent = queenParent;
			this.task = task;
			Name = name;
			this.executeOnBackgroundThread = executeOnBackgroundThread;
			queenParent.TaskBotConsole.Log(this, $"\t\t >> Hello {Name}! execute onBkgThread?= {executeOnBackgroundThread}");
		}

		public TaskBot(TaskQueen queenParent, string name, Task task, bool executeOnBackgroundThread = false)
		{
			stopwatch = Stopwatch.StartNew();
			this.queenParent = queenParent;
			this.task = () => task;
			Name = name;
			this.executeOnBackgroundThread = executeOnBackgroundThread;
			queenParent.TaskBotConsole.Log(this, $"\t\t >> Hello {Name}! execute onBkgThread?= {executeOnBackgroundThread}");
		}

		public virtual async Task ExecuteTask()
		{
			queenParent.TaskBotConsole.Log(this, $"\t\t >> Execute bot! {Name}");
			stopwatch.Start();
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
				queenParent.TaskBotConsole.Log(this, $"\t\t >> SUCCESS {ExecutionTime}");
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