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

		public string Name { get; set; } = "TaskBot";
		public Guid GuidId { get; } = Guid.NewGuid();
		public long ExecutionTime = 0;
		public TaskBot(TaskQueen queenParent, string name, Func<Task> task)
		{
			stopwatch = Stopwatch.StartNew();
			this.queenParent = queenParent;
			this.task = task;
			Name = name;
		}
		public virtual async Task ExecuteTask()
		{
			stopwatch.Start();
			try
			{
				await task();
			}
			catch (OperationCanceledException operation)
			{
				queenParent.Console.Log(this, $"\t\t ERROR: Operation was cancelled.");
				UnityEngine.Debug.LogError(operation);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex);
			}
			finally
			{
				stopwatch.Stop();
				ExecutionTime = stopwatch.ElapsedMilliseconds;
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