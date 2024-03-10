using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Darklight.Unity;

namespace Darklight.World.Generation.CustomEditor
{
#if UNITY_EDITOR
	using UnityEditor;
	using CustomEditor = UnityEditor.CustomEditor;
	using DarklightEditor = Unity.CustomInspectorGUI;
	using DarklightGizmos = Unity.CustomGizmos;

	[CustomEditor(typeof(CoordinateMap))]
	public class CoordinateMapEditor : UnityEditor.Editor
	{
		private SerializedObject _serializedObject;
		private CoordinateMap _coordinateMap;

		public enum CoordinateMapView { GRID_ONLY, COORDINATE_VALUE, COORDINATE_TYPE, ZONE_ID }
		public CoordinateMapView coordinateMapView = CoordinateMapView.COORDINATE_TYPE;

		private bool coordinateMapFoldout = true;

		private void OnEnable()
		{
			_serializedObject = new SerializedObject(target);
			_coordinateMap = (CoordinateMap)target;
		}

		public override void OnInspectorGUI()
		{
			_serializedObject.Update();

			if (_coordinateMap.Initialized == false)
			{
					if (_coordinateMap.GetComponent<WorldBuilder>() != null && GUILayout.Button("Initialize World Coordinate Map"))
					{
						_coordinateMap.InitializeWorldCoordinateMap(_coordinateMap.GetComponent<WorldBuilder>());
					}
					else if (_coordinateMap.GetComponent<Region>() != null && GUILayout.Button("Initialize Region Coordinate Map"))
					{
						_coordinateMap.InitializeRegionCoordinateMap(_coordinateMap.GetComponent<Region>());
					}
					/*
					else if (_coordinateMap.GetComponent<Chunk>() != null && GUILayout.Button("Initialize Chunk Coordinate Map"))
					{
						_coordinateMap.InitializeChunkCoordinateMap(_coordinateMap.GetComponent<Chunk>());
					}
					*/
			}
			else
			{
				// >> select debug view
				DarklightEditor.DrawLabeledEnumPopup(ref coordinateMapView, "Coordinate Map View");
				EditorGUILayout.LabelField($"Unit Space => {_coordinateMap.UnitSpace}", DarklightEditor.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Max Coordinate Value => {_coordinateMap.MaxCoordinateValue}", DarklightEditor.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Coordinate Count => {_coordinateMap.AllCoordinates.Count}", DarklightEditor.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Exit Count => {_coordinateMap.Exits.Count}", DarklightEditor.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Path Count => {_coordinateMap.Paths.Count}", DarklightEditor.LeftAlignedStyle);
				EditorGUILayout.LabelField($"Zone Count => {_coordinateMap.Zones.Count}", DarklightEditor.LeftAlignedStyle);
			}

			_serializedObject.ApplyModifiedProperties();

		}

		private void OnSceneGUI()
		{
			DrawCoordinateMap(_coordinateMap, coordinateMapView, null);
		}


		void DrawCoordinateMap(CoordinateMap coordinateMap, CoordinateMapView mapView, System.Action<Coordinate> onCoordinateSelect)
		{
			GUIStyle coordLabelStyle = DarklightEditor.CenteredStyle;
			Color coordinateColor = Color.black;

			// Draw Coordinates
			if (coordinateMap != null && coordinateMap.Initialized && coordinateMap.AllCoordinateValues.Count > 0)
			{
				foreach (Vector2Int coordinateValue in coordinateMap.AllCoordinateValues)
				{
					Coordinate coordinate = coordinateMap.GetCoordinateAt(coordinateValue);

					// Draw Custom View
					switch (mapView)
					{
						case CoordinateMapView.GRID_ONLY:
							break;
						case CoordinateMapView.COORDINATE_VALUE:
							DarklightGizmos.DrawLabel($"{coordinate.ValueKey}", coordinate.ScenePosition, coordLabelStyle);
							coordinateColor = Color.white;
							break;
						case CoordinateMapView.COORDINATE_TYPE:
							coordLabelStyle.normal.textColor = coordinate.TypeColor;
							DarklightGizmos.DrawLabel($"{coordinate.Type.ToString()[0]}", coordinate.ScenePosition, coordLabelStyle);
							coordinateColor = coordinate.TypeColor;
							break;
						case CoordinateMapView.ZONE_ID:
							coordLabelStyle.normal.textColor = coordinate.TypeColor;

							if (coordinate.Type == Coordinate.TYPE.ZONE)
							{
								Zone zone = coordinateMap.GetZoneFromCoordinate(coordinate);
								if (zone != null)
								{
									DarklightGizmos.DrawLabel($"{zone.ID}", coordinate.ScenePosition, coordLabelStyle);
								}
							}

							break;
					}

					// Draw Selection Rectangle
					DarklightGizmos.DrawButtonHandle(coordinate.ScenePosition, Vector3.up, coordinateMap.CoordinateSize * 0.475f, coordinateColor, () =>
					{
						onCoordinateSelect?.Invoke(coordinate); // Invoke the action if the button is clicked
					}, Handles.RectangleHandleCap);
				}
			}
		}
	}

#endif
}

