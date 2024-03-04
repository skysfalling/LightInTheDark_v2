using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WorldSave : MonoBehaviour
{
    public WorldGenerationSettings WorldGenerationSettings;

    private IDataService DataService = new JsonDataService(); // Assuming JsonDataService implements IDataService
    private bool EncryptionEnabled = false;
    private long SaveTime;
    private long LoadTime;

    public void ToggleEncryption(bool EncryptionEnabled)
    {
        this.EncryptionEnabled = EncryptionEnabled;
    }


    [EasyButtons.Button]
    public void SaveWorldSettings()
    {
        WorldSaveData saveData = new WorldSaveData(WorldGenerationSettings);

        // Since we are now loading the actual WorldGenerationSettings object, we can directly log the values
        Debug.Log($"Saving Data:\nSeed: {saveData.gameSeed}\n" +
                  $"Cell Width: {saveData.cellWidthInWorldSpace}\n" +
                  $"Chunk Width: {saveData.chunkWidthInCells}\n" +
                  $"Chunk Depth: {saveData.chunkDepthInCells}\n" +
                  $"Play Region Width: {saveData.playRegionWidthInChunks}\n" +
                  $"Boundary Wall Count: {saveData.boundaryWallCount}\n" +
                  $"Max Chunk Height: {saveData.maxChunkHeight}\n" +
                  $"World Width: {saveData.worldWidthInRegions}");



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

    [EasyButtons.Button]
    public void LoadWorldSettings()
    {
        long startTime = DateTime.Now.Ticks;
        try
        {
            // Assuming DataService is an instance of JsonDataService
            WorldSaveData worldSaveData = DataService.LoadData<WorldSaveData>("/world-data.json", EncryptionEnabled);
            if (worldSaveData == null)
            {
                Debug.LogError("Failed to load world data or world data is null.");
                // Handle the situation, such as by initializing worldSaveData with default values.
            }
            long loadTime = DateTime.Now.Ticks - startTime;
            Debug.Log($"Load Time: {(loadTime / 10000):N4}ms");

            // Since we are now loading the actual WorldGenerationSettings object, we can directly log the values
            Debug.Log($"Loaded from file:\nSeed: {worldSaveData.gameSeed}\n" +
                      $"Cell Width: {worldSaveData.cellWidthInWorldSpace}\n" +
                      $"Chunk Width: {worldSaveData.chunkWidthInCells}\n" +
                      $"Chunk Depth: {worldSaveData.chunkDepthInCells}\n" +
                      $"Play Region Width: {worldSaveData.playRegionWidthInChunks}\n" +
                      $"Boundary Wall Count: {worldSaveData.boundaryWallCount}\n" +
                      $"Max Chunk Height: {worldSaveData.maxChunkHeight}\n" +
                      $"World Width: {worldSaveData.worldWidthInRegions}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not read file!\n{e}");
        }
    }

}
