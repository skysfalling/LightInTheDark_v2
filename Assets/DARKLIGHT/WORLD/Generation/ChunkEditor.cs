using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Darklight.World.Editor;
using Darklight.World;
using Darklight.World.Generation;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.World.Editor
{
    public class ChunkEditor : WorldEditor
    {
        public ChunkData chunkData = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ChunkEditor))]
public class ChunkEditorGUI : WorldEditorGUI
{
    private SerializedObject _serializedObject;
    private ChunkEditor _chunkEditor;

    public override void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _chunkEditor = (ChunkEditor)target;

        ChunkData chunkData = _chunkEditor.chunkData;
        if (chunkData != null)
        {
            //_chunkEditor.SelectChunk(chunkData);
        }
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();
        _serializedObject.ApplyModifiedProperties();
    }

}
#endif
