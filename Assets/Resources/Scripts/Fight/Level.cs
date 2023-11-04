using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level
{
    public int TopLeftSquarePositionX;
    public int TopLeftSquarePositionZ;
    public int YPosition;
    public int HorizontalTiles;
    public int VerticalTiles;

    //Key represents tile number
    public Dictionary<int, GameObject> tilesDict = new();

    //Key represents the assigned tile number of the unit
    public Dictionary<int, GameObject> enemyList;

	public int seed;
	public IGod enemyGod;

    public void StartLevel(int seed, RogueTileType tileType, int currentRow, int difficulty, IGod enemyGod)
    {
		this.seed = seed;
		int mapSize = RandomManager.GetRandomValue(seed, 10, 10);
		TopLeftSquarePositionX = 250;
        TopLeftSquarePositionZ = 1800;
        YPosition = 170;
        HorizontalTiles = mapSize;
        VerticalTiles = mapSize;
		this.enemyGod = enemyGod;
        //tilesDict = SetupTiles();
        SetupEnemies(tileType, currentRow, difficulty);
    }

	public void GenerateTerrain()
	{
		Transform objectsParent = GameObject.Find("Fight Objects").transform;
		int[] mountains = new int[RandomManager.GetRandomValue(seed, 0, 10)];
		for (int i = 0; i < mountains.Length; i++)
		{
			int mountainSeed = seed * (i + 1);
			mountains[i] = RandomManager.GetRandomValue(mountainSeed, 0, HorizontalTiles * VerticalTiles);
		}
		for (int i = 0; i < HorizontalTiles * VerticalTiles; i++)
		{
			if (mountains.Contains(i))
			{
				GameObject objectToSpawn = ObjectsManager.GetRandomObject(seed * 12 * (i + 1), ObjectsManager.TypeOfObstacle.SingleTile, ObjectsManager.MapTheme.Plains);
				GameObject tileObject = Object.Instantiate(objectToSpawn, objectsParent);
				Tile tileScript = tileObject.GetComponent<Tile>();

				tileObject.name = $"Terrain_{i}";
				tileScript.IsPassable = false;
				tileScript.MovementCost = 9;
				tilesDict.Add(i, tileObject);
			}
			else
			{
				GameObject objectToSpawn = ObjectsManager.GetRandomObject(seed * 12 / (i + 1), ObjectsManager.TypeOfObstacle.Terrain, ObjectsManager.MapTheme.Plains);
				GameObject tileObject = Object.Instantiate(objectToSpawn, objectsParent);
				tileObject.name = $"Terrain_{i}";
				tilesDict.Add(i, tileObject);
			}
		}
	}

	public void SetupEnemies(RogueTileType tileType, int currentRow, int difficulty)
	{
		Dictionary<int, GameObject> result = new();
		Encounter encounter = enemyGod.Encounters[RandomManager.GetRandomValue(seed, 0, enemyGod.Encounters.Length)];

		for (int i = 0; i < encounter.units.Length; i++)
		{
			Unit enemy = encounter.units[i];
			enemy.faction = 1;
			result.Add(encounter.positions[i], enemy.gameObject);
		}

		enemyList = result;
	}
}