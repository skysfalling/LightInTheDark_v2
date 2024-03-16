using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.World.Generation
{
	using Map;
	public class Zone
	{
		public enum Shape { SINGLE, FULL, NATURAL_CROSS, DIAGONAL_CROSS, HORIZONTAL, VERTICAL }
		public static Shape GetRandomTypeFromList(List<Shape> typeList)
		{
			// Choose a random index
			int randomIndex = Random.Range(0, typeList.Count);

			// Return the randomly selected WorldZone.TYPE
			return typeList[randomIndex];
		}

		// [[ PRIVATE VARIABLES ]]
		bool _valid;
		int _id;
		CoordinateMap _coordinateMapParent;
		Coordinate _coordinate;
		Shape _type;
		int _height;
		Dictionary<Vector2Int, Coordinate> _zoneCoordinateValueMap = new();

		public bool Valid => _valid;
		public int ID => _id;
		public List<Vector2Int> Positions => _zoneCoordinateValueMap.Keys.ToList();
		public List<Coordinate> Coordinates => _zoneCoordinateValueMap.Values.ToList();
		public Coordinate CenterCoordinate => _coordinate;
		public Zone(Coordinate coordinate, Shape zoneShape, int zoneID)
		{
			this._coordinateMapParent = coordinate.ParentMap;
			this._coordinate = coordinate;
			this._type = zoneShape;
			this._id = zoneID;

			// Get affected neighbors
			List<Coordinate> neighborsInZone = new();
			switch (_type)
			{
				case Shape.SINGLE:
					break;
				case Shape.FULL:
					neighborsInZone = _coordinate.GetAllValidNeighbors();
					break;
				case Shape.NATURAL_CROSS:
					neighborsInZone = _coordinate.GetValidNaturalNeighbors();
					break;
				case Shape.DIAGONAL_CROSS:
					neighborsInZone = _coordinate.GetValidDiagonalNeighbors();
					break;
				case Shape.HORIZONTAL:
					neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.WEST));
					neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.EAST));
					break;
				case Shape.VERTICAL:
					neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.NORTH));
					neighborsInZone.Add(_coordinate.GetNeighborInDirection(WorldDirection.SOUTH));
					break;
			}

			// Assign Zone Coordinates
			List<Coordinate> zoneCoordinates = new List<Coordinate> { _coordinate };
			zoneCoordinates.AddRange(neighborsInZone);

			// Extract coordinates into values map
			_zoneCoordinateValueMap = _coordinateMapParent.GetCoordinateValueMapFrom(zoneCoordinates);

			// Extract  & check coordinate types
			List<Coordinate.TYPE?> _zoneCoordinateTypes = _coordinateMapParent.GetCoordinateTypesAt(_zoneCoordinateValueMap.Keys.ToList());
			if (_zoneCoordinateTypes.Any(type => type != Coordinate.TYPE.NULL))
			{
				_valid = false;
				return;
			}

			_valid = true;
		}

		// Helper function to find the closest coordinate to a given coordinate
		public Coordinate GetClosestExternalNeighborTo(Vector2Int mapValue)
		{
			float closestDistance = float.MaxValue;
			var coordinateValueMap = _zoneCoordinateValueMap;

			// >> get the closest zone coordinate value
			Vector2Int closestZoneValue = Vector2Int.zero;
			foreach (Vector2Int value in coordinateValueMap.Keys.ToList())
			{
				float currentDistance = Vector2Int.Distance(value, mapValue);
				if (currentDistance < closestDistance)
				{
					closestDistance = currentDistance;
					closestZoneValue = value;
				}
			}

			// >> get coordinate reference
			Coordinate closestZoneCoordinate = _coordinateMapParent.GetCoordinateAt(closestZoneValue);
			if (closestZoneCoordinate == null) return null;

			// >> get coordinate neighbors
			Vector2Int closestExternalValue = closestZoneValue;
			List<Coordinate> neighbors = closestZoneCoordinate.GetValidNaturalNeighbors();
			foreach (Coordinate neighbor in neighbors)
			{
				if (neighbor.Type == Coordinate.TYPE.ZONE) continue;

				float currentDistance = Vector2Int.Distance(neighbor.ValueKey, mapValue);
				if (currentDistance < closestDistance)
				{
					closestDistance = currentDistance;
					closestExternalValue = neighbor.ValueKey;
				}
			}

			return _coordinateMapParent.GetCoordinateAt(closestExternalValue);

		}
	}
}

