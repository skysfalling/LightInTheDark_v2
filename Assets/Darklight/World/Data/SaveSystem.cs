namespace Darklight.World.Data
{
	using System;
	using UnityEngine;
	using Darklight.DataService;
	using Darklight.World.Builder;

	public class SaveSystem : MonoBehaviour
	{
		private IDataService DataService = new JsonDataService(); // Assuming JsonDataService implements IDataService
		private bool EncryptionEnabled = false;
		private long SaveTime;
		private long LoadTime;

		public void ToggleEncryption(bool EncryptionEnabled)
		{
			this.EncryptionEnabled = EncryptionEnabled;
		}

		public void SaveWorldSettings()
		{
			WorldBuilder worldGeneration = GetComponent<WorldBuilder>();

			WorldData saveData = new WorldData(worldGeneration);

			long startTime = DateTime.Now.Ticks;
			if (DataService.SaveData("/world-data.json", saveData, EncryptionEnabled))
			{
				SaveTime = DateTime.Now.Ticks - startTime;
				Debug.Log($"Save Time: {(SaveTime / 10000):N4}ms");
			}
			else
			{
				Debug.LogError("Could not save file!");
			}
		}

		public void LoadWorldSettings()
		{
			long startTime = DateTime.Now.Ticks;
			try
			{
				WorldData worldSaveData = DataService.LoadData<WorldData>("/world-data.json", EncryptionEnabled);
				if (worldSaveData == null)
				{
					Debug.LogError("Failed to load world data or world data is null.");
				}
				LoadTime = DateTime.Now.Ticks - startTime;
				Debug.Log($"Load Time: {(LoadTime / 10000):N4}ms");

				Debug.Log($"World Data Loaded : Seed -> {worldSaveData.settings.Seed}");
			}
			catch (Exception e)
			{
				Debug.LogError($"Could not read file!\n{e}");
			}
		}
	}
}