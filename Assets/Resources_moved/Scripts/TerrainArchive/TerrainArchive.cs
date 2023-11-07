using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectsManager;

public static class TerrainArchive
{
	public static Dictionary<MapTheme, TerrainSetup> TerrainsDict = new()
	{
		

	};

	//WIP
	public static List<TerrainSetup> SetPlainsTerrains()
	{
		List<TerrainSetup> result = new();
		List<TerrainSetup> terrains = new List<TerrainSetup>()
		{

		};
		return result;
	}

	public struct TerrainSetup
	{
		public int columns;
		public int rows;
		readonly Dictionary<int, TypeOfObstacle> terrainTile;

		public TerrainSetup(int columns, int rows, Dictionary<int, TypeOfObstacle> terrainTile)
		{
			this.columns = columns;
			this.rows = rows;
			this.terrainTile = terrainTile;
		}
	}
}