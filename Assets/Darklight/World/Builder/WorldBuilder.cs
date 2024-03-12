namespace Darklight.World.Builder
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;
	using System.Threading.Tasks;
	using Debug = UnityEngine.Debug;
	using Darklight.Bot;
	using Darklight.World.Settings;
	using Darklight.World.Map;

#if UNITY_EDITOR
	using UnityEditor;
	using Darklight.World.Generation;
#endif

	/// <summary> Initializes and handles the procedural world generation. </summary>
	public class WorldBuilder : TaskQueen
	{
		#region [[ STATIC INSTANCE ]] ------------------- // 
		/// <summary> A singleton instance of the WorldGeneration class. </summary>
		public static WorldBuilder Instance;
		private new void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(this);
			}
		}
		#endregion

		#region [[ GENERATION SETTINGS ]] -------------------------------------- >> 
		static GenerationSettings _settings = new();

		/// <summary> Contains settings used during the world generation process. </summary>
		public static GenerationSettings Settings => _settings;

		/// <summary> Override the default generation settings. </summary>
		public void OverrideSettings(CustomGenerationSettings customSettings)
		{
			if (customSettings == null) { _settings = new GenerationSettings(); return; }
			_settings = new GenerationSettings(customSettings);
		}
		#endregion

		#region [[ RANDOM SEED ]] -------------------------------------- >> 
		public static string Seed { get { return Settings.Seed; } }
		public static int EncodedSeed { get { return Settings.Seed.GetHashCode(); } }
		public static void InitializeSeedRandom()
		{
			UnityEngine.Random.InitState(EncodedSeed);
		}
		#endregion

		// [[ PRIVATE VARIABLES ]] 
		string _prefix = "[ WORLD BUILDER ] ";
		CoordinateMap _coordinateMap;
		Dictionary<Vector2Int, RegionBuilder> _regionMap = new();

		// [[ PUBLIC REFERENCE VARIABLES ]]
		public bool Initialized { get; private set; }
		public CoordinateMap CoordinateMap { get { return _coordinateMap; } }
		public Vector3 CenterPosition { get { return transform.position; } }
		public Vector3 OriginPosition
		{
			get
			{
				Vector3 origin = CenterPosition; // Start at center
				origin -= Settings.WorldWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				origin += Settings.RegionFullWidth_inGameUnits * new Vector3(0.5f, 0, 0.5f);
				return origin;
			}
		}
		public List<RegionBuilder> AllRegions { get { return _regionMap.Values.ToList(); } }
		public Dictionary<Vector2Int, RegionBuilder> RegionMap { get { return _regionMap; } }

		// [[ PUBLIC INSPECTOR VARIABLES ]] 
		public CustomGenerationSettings customWorldGenSettings; // Settings Scriptable Object
		public Material defaultMaterial;
		public bool initializeOnStart;

		#region == INITIALIZATION ============================================== >>>> 
		private async void Start()
		{
			if (initializeOnStart == true)
			{
				Debug.Log($"{_prefix} Initialize On Start");
				await Initialize();
			}
		}

		public override async Awaitable Initialize(bool startSequence = true)
		{
			this.Name = "World Builder Task Queen";
			OverrideSettings(customWorldGenSettings);
			InitializeSeedRandom();
			this._coordinateMap = new CoordinateMap(this);
			Debug.Log($"{_prefix} Initialized, Mods: {customWorldGenSettings != null}");

			// [[ INSTANTIATE REGIONS ]]
			// Create all Regions from coordinate map
			foreach (Coordinate regionCoordinate in CoordinateMap.AllCoordinates)
			{
				GameObject regionObject = new GameObject($"New Region ({regionCoordinate.ValueKey})");
				RegionBuilder region = regionObject.AddComponent<RegionBuilder>();
				region.transform.parent = this.transform;
				region.AssignToWorldParent(this, regionCoordinate);
				_regionMap[regionCoordinate.ValueKey] = region;
			}
			await Awaitable.WaitForSecondsAsync(1);
			await base.Initialize(startSequence);
		}


		/// <summary>
		/// Orchestrates the entire initialization sequence asynchronously, divided into stages.
		/// </summary>
		public override async Awaitable InitializationSequence()
		{


			// Stage 1: Generate Regions
			TaskBot task1 = new TaskBot(this, "GenerateRegions", async () =>
			{
				Debug.Log("GenerateRegions task started");
				// Initialize all regions onto the map
				foreach (RegionBuilder region in _regionMap.Values)
				{
					await region.Initialize(true);
					while (!region.Initialized)
					{
						await Awaitable.WaitForSecondsAsync(0.1f);
					}
				}
			});
			await Enqueue(task1);

			// Stage 2: Generate Exits based on neighboring regions
			TaskBot task2 = new TaskBot(this, "GenerateExits", async () =>
			{
				Debug.Log("GenerateExits task started");
				foreach (RegionBuilder region in AllRegions)
				{
					region.GenerateNecessaryExits(true);
					await Awaitable.WaitForSecondsAsync(0.1f);
				}
			});
			await Enqueue(task2);

			// Stage 3: Generate Paths Between Exits
			TaskBot task3 = new TaskBot(this, "GeneratePathsBetweenExits", async () =>
			{
				foreach (RegionBuilder region in AllRegions)
				{
					region.CoordinateMap.GeneratePathsBetweenExits();
					await Awaitable.WaitForSecondsAsync(0.1f);
				}
			});
			await Enqueue(task3);


			// Stage 4: Zone Generation and Height Assignments
			TaskBot task4 = new TaskBot(this, "ZoneGeneration", async () =>
			{
				foreach (RegionBuilder region in AllRegions)
				{
					region.CoordinateMap.GenerateRandomZones(3, 5, new List<Zone.TYPE> { Zone.TYPE.FULL });
					await Awaitable.WaitForSecondsAsync(0.1f);
				}
			});
			await Enqueue(task4);
			await ExecuteAllTasks();

			Debug.Log($"{_prefix} Initialization Complete");

			// Mark initialization as complete
			Initialized = true;
		}

		#endregion

		/// <summary> Fully Reset the World Generation </summary>
		public void ResetGeneration()
		{
			for (int i = 0; i < AllRegions.Count; i++)
			{
				if (AllRegions[i] != null)
					AllRegions[i].Destroy();
			}
			_regionMap.Clear();
			this._coordinateMap = null; // Clear coordinate map

			Initialized = false;
		}

		private void OnDrawGizmos()
		{
			if (CoordinateMap == null) { return; }
			foreach (Coordinate coord in CoordinateMap.AllCoordinates)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(coord.ScenePosition, Vector3.one * 0.5f);
			}
		}
	}
}