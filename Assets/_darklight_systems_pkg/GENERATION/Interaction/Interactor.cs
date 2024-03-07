using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Darklight.ThirdDimensional.Generation.Interaction
{
    using WorldEntity = Generation.Entity;

    public class Interactor : MonoBehaviour
    {
        WorldGeneration _worldGeneration;
        ChunkMap _worldChunkMap;
        WorldEnvironment _worldEnvironment;

        [Header("World Cursor")]
        public Transform worldCursor; // related transform to the cursor
        public Cell currCursorCell = null;

        [Header("Select Entity")]
        public WorldEntity selectedEntity;

    }
}

