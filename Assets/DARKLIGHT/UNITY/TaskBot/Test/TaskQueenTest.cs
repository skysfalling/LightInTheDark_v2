using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using EasyButtons;

using NUnit.Framework;


namespace Darklight.Unity.Backend.Test
{
	public class TaskQueenTest : MonoBehaviour
	{
		static TaskQueen taskQueen;
		static TaskQueen asyncTaskQueen;

		public void Awake()
		{
			taskQueen = new GameObject("TaskQueen").AddComponent<TaskQueen>();
			asyncTaskQueen = new GameObject("AsyncTaskQueen").AddComponent<TaskQueen>();
		}

		public void Start()
		{
			//taskQueen.Initialize("TaskQueen");
			//asyncTaskQueen.Initialize("AsyncTaskQueen");
		}

		[EasyButtons.Button]
		public void EnqueueAndExecuteTests()
		{
			TaskBot taskBot = new TaskBot("TestTaskBot", async () =>
			{
				for (int i = 0; i < 100; i++)
				{
					Debug.Log("TestTaskBot: " + i);
					await Task.Delay(1000);
				}
				Debug.Log("TestTaskBot completed.");
			});

			taskQueen.Enqueue(taskBot);
			asyncTaskQueen.Enqueue(taskBot);
			taskQueen.ExecuteAllTasks();
			asyncTaskQueen.ExecuteAllTasks();
		}
	}
}