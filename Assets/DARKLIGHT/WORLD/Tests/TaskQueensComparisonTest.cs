using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Unity.Backend.Test
{
	public class TaskQueensComparisonTest
	{
		private TaskQueen taskQueen;
		private AsyncTaskQueen asyncTaskQueen;

		// Setup method to initialize test conditions
		[SetUp]
		public void Setup()
		{
			// Setup for TaskQueen
			GameObject taskQueenGameObject = new GameObject("TaskQueen");
			taskQueen = taskQueenGameObject.AddComponent<TaskQueen>();
			taskQueen.Initialize("TaskQueen");

			// Setup for AsyncTaskQueen
			GameObject asyncTaskQueenGameObject = new GameObject("AsyncTaskQueen");
			asyncTaskQueen = asyncTaskQueenGameObject.AddComponent<AsyncTaskQueen>();
			asyncTaskQueen.Initialize("AsyncTaskQueen");
		}

		// Test method for initialization
		[Test]
		public void Queens_InitializeCorrectly()
		{
			Assert.AreEqual("TaskQueen", taskQueen.Name);
			Assert.AreEqual("AsyncTaskQueen", asyncTaskQueen.Name);
		}

		// Test method for queue management
		[Test]
		public void Queens_EnqueueTasksCorrectly()
		{
			var initialCountTaskQueen = taskQueen.ExecutionQueueCount;
			var initialCountAsyncTaskQueen = asyncTaskQueen.ExecutionQueueCount;

			TaskBot taskBot = new TaskBot("TestEnqueue", taskQueen, async () =>
			{
				await Awaitable.WaitForSecondsAsync(1); // Simulate work
				Debug.Log("TestEnqueue completed.");
			});
			taskQueen.Enqueue(taskBot);
			asyncTaskQueen.Enqueue(taskBot);

			Assert.AreEqual(initialCountTaskQueen + 1, taskQueen.ExecutionQueueCount);
			Assert.AreEqual(initialCountAsyncTaskQueen + 1, asyncTaskQueen.ExecutionQueueCount);
		}

		// Test method for execution of tasks
		[UnityTest]
		public IEnumerator Queens_ExecuteTasksCorrectly()
		{
			TaskBot taskBotTaskQueen = new TaskBot("TestExecutionTaskQueen", taskQueen, async () =>
			{
				await Awaitable.WaitForSecondsAsync(1); // Simulate work
				Debug.Log("TaskBot for TaskQueen completed.");
			});

			TaskBot taskBotAsyncTaskQueen = new TaskBot("TestExecutionAsyncTaskQueen", asyncTaskQueen, async () =>
			{
				await Awaitable.WaitForSecondsAsync(1); // Simulate work
				Debug.Log("TaskBot for AsyncTaskQueen completed.");
			});

			taskQueen.Enqueue(taskBotTaskQueen);
			asyncTaskQueen.Enqueue(taskBotAsyncTaskQueen);

			// Trigger execution
			taskQueen.ExecuteAllTasks();
			asyncTaskQueen.ExecuteAllTasks();

			// Wait for tasks to potentially complete.
			yield return new WaitForSeconds(2); // Adjust based on execution time

			// Verify
			Assert.AreEqual(0, taskQueen.ExecutionQueueCount);
			Assert.AreEqual(0, asyncTaskQueen.ExecutionQueueCount);
		}

		// Cleanup method to tear down test conditions
		[TearDown]
		public void Teardown()
		{
			// Destroy game objects to clean up after tests
			if (taskQueen != null) GameObject.Destroy(taskQueen.gameObject);
			if (asyncTaskQueen != null) GameObject.Destroy(asyncTaskQueen.gameObject);
		}
	}
}
