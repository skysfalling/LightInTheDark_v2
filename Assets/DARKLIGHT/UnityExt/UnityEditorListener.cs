using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt
{
    public interface IUnityEditorListener
    {
        void OnEditorReloaded();
    }

    [InitializeOnLoad]
    public class EditorReloadHandler
    {
        public static string Prefix = "( Darklight.UnityExt )";
        private static List<IUnityEditorListener> Listeners = new List<IUnityEditorListener>();
        static EditorReloadHandler()
        {
            // Find all MonoBehaviour instances that implement IUnityEditorListener
            Listeners = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IUnityEditorListener>().ToList();

            Debug.Log($"{Prefix} EditorReload -> Notifying {Listeners.Count} Listeners");

            // Notify all found listeners
            foreach (IUnityEditorListener listener in Listeners)
            {
                listener.OnEditorReloaded();
            }
        }
    }
}
