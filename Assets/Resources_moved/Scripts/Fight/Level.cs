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

    public void StartLevel(int seed, RogueTileType tileType, int currentRow, int difficulty, IGod enemyGod, int customSize = -1)
    {
		this.seed = seed;
		int mapSize = customSize == -1 ? RandomManager.GetRandomValue(seed, 10, 10) : customSize;
		TopLeftSquarePositionX = 250;
        TopLeftSquarePositionZ = 1800;
        YPosition = 170;
        HorizontalTiles = mapSize;
        VerticalTiles = mapSize;
		this.enemyGod = enemyGod;
        //tilesDict = SetupTiles();
        SetupEnemies(tileType, currentRow, difficulty);
    }

	// if isEdit is true then the map generated will be a basic map with only grass
	public void GenerateTerrain(bool isEdit, Transform objectsParent)
	{
		for (int i = 0; i < HorizontalTiles * VerticalTiles; i++)
		{
				GameObject objectToSpawn = ObjectsManager.GetRandomObject(seed * 12 / (i + 1), ObjectsManager.TypeOfObstacle.Terrain, ObjectsManager.MapTheme.Plains);
				GameObject tileObject = Object.Instantiate(objectToSpawn, objectsParent);

				LoadTile(tileObject, $"Terrain_{i}", objectToSpawn.name, isEdit, true);
				tilesDict.Add(i, tileObject);
		}
	}
	public void GenerateTerrain(bool isEdit, Transform objectsParent, string[] customMap)
	{
		List<GameObject> models = new();
		//We load onto the list all the different objects we need for this custom map, beforehand
		for (int i = 0; i < customMap.Length; i++)
			if (!models.Any(m => m.name.Equals(customMap[i])))
				models.Add(ObjectsManager.GetObject(customMap[i]));

		for (int i = 0; i < HorizontalTiles * VerticalTiles; i++)
		{
			GameObject objectToSpawn = models.First(m => m.name == customMap[i]);
			GameObject tileObject = Object.Instantiate(objectToSpawn, objectsParent);

			LoadTile(tileObject, $"Terrain_{i}", objectToSpawn.name, isEdit, true);
			tilesDict.Add(i, tileObject);
		}
	}

	void LoadTile(GameObject tile, string name, string modelName, bool isEdit, bool isPassable)
	{
		tile.name = name;

		Tile script = tile.GetComponent<Tile>();
		script.modelName = modelName;
		script.isEdit = isEdit;
		script.IsPassable = isPassable;
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