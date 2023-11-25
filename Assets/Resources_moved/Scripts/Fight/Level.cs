using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unit;

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
    public Dictionary<int, Unit> enemyList;

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
			GameObject objectToSpawn = ObjectsManager.GetObject("Grass1");
			GameObject tileObject = UnityEngine.Object.Instantiate(objectToSpawn, objectsParent);

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
			GameObject tileObject = UnityEngine.Object.Instantiate(objectToSpawn, objectsParent);

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
		int enemiesSetupSeed = RandomManager.GetRandomValue(seed, 0, 100000000);
		Dictionary<int, Unit> result = new();
		Encounter encounter = enemyGod.Encounters[RandomManager.GetRandomValue(seed, 0, enemyGod.Encounters.Length)];

		for (int i = 0; i < encounter.units.Length; i++)
		{
			int enemySeed = RandomManager.GetRandomValue(enemiesSetupSeed, 0, 100000000);
			Unit enemy = encounter.units[i];
			SetupEnemy(encounter.units[i], enemySeed);
			result.Add(encounter.positions[i], enemy);
		}

		enemyList = result;
	}

	public void SetupEnemy(Unit unit, int enemySeed)
	{
		unit.faction = 1;
		SetRandomTraits(unit, enemySeed);
	}

	public void SetRandomTraits(Unit unit, int enemySeed)
	{
		List<TraitsEnum> unitTraits = new();
		int numOfTraits = RandomManager.GetRandomValue(enemySeed, 0, 6);
		for (int i = 0; i < numOfTraits; i++)
		{
			int traitSeed = RandomManager.GetRandomValue(enemySeed * (i + 1), 0, 10000000);
			unitTraits.Add((TraitsEnum)RandomManager.GetRandomValue(traitSeed, 0, Enum.GetNames(typeof(TraitsEnum)).Length));
			unit.Traits = unitTraits;
		}
	}
}