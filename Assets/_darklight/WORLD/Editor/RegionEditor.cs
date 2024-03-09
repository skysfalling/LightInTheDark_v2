using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Darklight.Unity.Backend;

namespace Darklight.World.Generation.Editor
{
	[UnityEditor.CustomEditor(typeof(Region))]
	public class RegionEditor : UnityEditor.Editor
	{
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}

