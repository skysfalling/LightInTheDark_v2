using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum WorldDirection { West, East, North, South }

[System.Serializable]
public class WorldExit
{
    WorldGeneration _worldGen;
    [HideInInspector] public WorldChunk chunk;
    public WorldDirection edgeDirection;
    public int edgeIndex;

    // Constructor
    public WorldExit(WorldDirection edgeDirection, int edgeIndex)
    {
        this.chunk = WorldChunkMap.Instance.GetEdgeChunk(edgeDirection, edgeIndex);
        this.edgeDirection = edgeDirection;
        this.edgeIndex = edgeIndex;
    }

    int CalculateMaxIndex(WorldDirection direction)
    {
        int maxIndex = 10;
        if (WorldGeneration.Instance != null)
        {
            switch (direction)
            {
                case WorldDirection.North:
                case WorldDirection.South:
                    maxIndex = WorldGeneration.WorldWidthInChunks - 1;
                    break;
                case WorldDirection.East:
                case WorldDirection.West:
                    maxIndex = WorldGeneration.WorldWidthInChunks - 1; // Adjust as needed
                    break;
            }
        }
        return maxIndex;
    }

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(WorldExit))]
public class WorldExitDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var directionRect = new Rect(position.x, position.y, position.width / 2, position.height);
        var indexRect = new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, position.height);

        // Draw the direction field
        EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("edgeDirection"), GUIContent.none);

        // Assuming you can get a max index from somewhere, for demonstration it's hardcoded
        int maxIndex = WorldGeneration.WorldWidthInChunks - 1;

        // Fetch the current edgeIndex value
        SerializedProperty edgeIndexProp = property.FindPropertyRelative("edgeIndex");

        // Draw the slider for edgeIndex
        edgeIndexProp.intValue = EditorGUI.IntSlider(indexRect, GUIContent.none, edgeIndexProp.intValue, 0, maxIndex);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
#endif


