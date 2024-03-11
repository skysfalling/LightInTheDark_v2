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
		static AsyncTaskQueen asyncTaskQueen;

		public void Awake()
		{
			taskQueen = new GameObject("TaskQueen").AddComponent<TaskQueen>();
			asyncTaskQueen = new GameObject("AsyncTaskQueen").AddComponent<AsyncTaskQueen>();
		}

		public void Start()
		{
			taskQueen.Initialize("TaskQueen");
			asyncTaskQueen.Initialize("AsyncTaskQueen");
		}

		[EasyButtons.Button]
		public void EnqueueTest()
		{
			TaskBot taskBot = new TaskBot("TestTaskBot", taskQueen, async () =>
			{
				await Awaitable.WaitForSecondsAsync(1);
				Debug.Log("TestTaskBot completed.");
			});
			taskQueen.Enqueue(taskBot);
			asyncTaskQueen.Enqueue(taskBot);
		}
	}
}