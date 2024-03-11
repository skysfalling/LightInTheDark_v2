namespace Darklight.Bot
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;

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
		public TaskBot(string name, Func<Task> task)
		{
			stopwatch = Stopwatch.StartNew();
			this.task = task;
			Name = name;
		}
		public virtual async Task ExecuteTask()
		{
			try
			{
				stopwatch.Reset();
				await task();
			}
			catch (OperationCanceledException)
			{
				queenParent.Console.Log(this, $"Operation was cancelled.");
			}
			catch (Exception ex)
			{
				queenParent.Console.Log(this, $"Error {ex}");
				UnityEngine.Debug.LogError($"AsyncTaskBot '{Name}' encountered an error: {ex.Message}");
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