using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Darklight.World.Generation.Interaction
{
    using WorldEntity = Generation.Entity;

    public class Interactor : MonoBehaviour
    {
        WorldBuilder _worldGeneration;
        ChunkMap _worldChunkMap;
        WorldEnvironment _worldEnvironment;

        [Header("World Cursor")]
        public Transform worldCursor; // related transform to the cursor
        public Cell currCursorCell = null;

        [Header("Select Entity")]
        public WorldBuilder selectedEntity;

    }
}

