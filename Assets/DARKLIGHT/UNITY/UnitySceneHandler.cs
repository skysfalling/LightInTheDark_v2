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

    public class UnitySceneHandler : TaskQueen
    {
        public static UnitySceneHandler Instance { get; private set; }
        private void Awake() 
        {
            if (Instance != null) { Destroy(this); }
            else { Instance = this; }
        }
    }
}

