using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Darklight.World.Generation;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics; // Include this for Stopwatch
using Debug = UnityEngine.Debug;

namespace Darklight.Unity
{
    using Darklight.Unity.Backend;
    using Darklight.World.Generation.Entity.Spawner;

    public class UnitySceneHandler : AsyncTaskQueen
    {
        #region [[ STATIC INSTANCE ]] ============================= >>
        public static UnitySceneHandler Instance { get; private set; }
        private void Awake() 
        {
            if (Instance != null) { Destroy(this); }
            else { Instance = this; }
        }
        #endregion

        WorldBuilder _worldBuilder => WorldBuilder.Instance;
        public GameObject playerObject;

        // Start is called before the first frame update
        void Start()
        {
            if (_worldBuilder != null && _worldBuilder.Initialized)
            {
                _worldBuilder.ResetGeneration();
            }

            if (playerObject != null)
            {
                playerObject.SetActive(false);
            }

            InitializeScene();
        }

        async void InitializeScene()
        {
            Debug.Log("Initializing Scene");

            // [[ Create Task Bots ]]
            // Generate the world
            NewTaskBot("Generate World", async () =>
            {
                await _worldBuilder.InitializeAsync();
                _worldBuilder.StartGeneration();
            });

            // Spawn the player
            NewTaskBot("Spawn Player", async () =>
            {
                MainSpawner.Instance.SpawnEntityInRandomValidZone(playerObject);
                await Task.Delay(1000); // wait one second
            });

            await ExecuteAllBotsInQueue();
        }

    }
}

