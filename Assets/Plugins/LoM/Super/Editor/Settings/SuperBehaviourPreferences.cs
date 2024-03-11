using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LoM.Super.Editor
{
    public static class SuperBehaviourPreferences
    {
        [SettingsProvider]
        public static SettingsProvider SuperBehaviourSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/SuperBehaviour", SettingsScope.User)
            {
                // Define the GUI for your preferences here
                guiHandler = (searchContext) =>
                {
                    // Label
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("  Connect Server", EditorStyles.boldLabel);
                    
                    // Example preference: A simple toggle
                    EditorGUI.BeginChangeCheck();
                    string port = EditorPrefs.GetString("SuperBehaviour.ConnectServer.Port", "20635");
                    port = EditorGUILayout.TextField("  Port", port, new GUILayoutOption[] { GUILayout.Width(250) });
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetString("SuperBehaviour.ConnectServer.Port", port);
                    }
                },

                // Keywords to support search functionality in the Preferences window
                keywords = new HashSet<string>(new[] { "Super", "Behaviour", "Connect", "Server" })
            };

            return provider;
        }
    }
}