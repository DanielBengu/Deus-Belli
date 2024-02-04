using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Level
{
	public RogueTileType tileType;
	const int BASE_ENEMY_GOLD_REWARD = 100;
	public TileMapData mapData;

	public Vector3 spawnPosition;

    //Key represents tile number
    public Dictionary<int, GameObject> tilesDict = new();

    //Key represents the assigned tile number of the unit
    public Dictionary<int, UnitData> enemyList;

	public int goldReward = 0;

	public int seed;

    public void StartLevel(int seed, RogueTileType rogueTileType)
    {
		tileType = rogueTileType;
		this.seed = seed;
		mapData = FileManager.GetRandomGenericMap(seed);
		spawnPosition = new(250, 170, 1800);
        SetupEnemies();
		SetupLevelRewards();
	}

	// if isEdit is true then the map generated will be a basic map with only grass
	public void GenerateTerrain(bool isEdit, Transform objectsParent)
	{
		for (int i = 0; i < mapData.Rows * mapData.Columns; i++)
		{
			GameObject objectToSpawn = ObjectsManager.GetObject(mapData.TileList[i].Model);
			GameObject tileObject = UnityEngine.Object.Instantiate(objectToSpawn, objectsParent);

			LoadTile(tileObject, $"Terrain_{i}", isEdit, mapData.TileList[i]);
			tilesDict.Add(i, tileObject);
		}
	}

	void LoadTile(GameObject tile, string name, bool isEdit, TileData tileData)
	{
		tile.name = name;

		try
		{
			Tile script = tile.GetComponent<Tile>();
			script.data = tileData;
			script.isEdit = isEdit;
		}
		catch
		{
			Debug.LogError("Missing \"Tile\" script on Prefab " + tile.name);
		}
	}

	public void SetupEnemies()
	{
		int enemiesSetupSeed = RandomManager.GetRandomValue(seed, 0, 100000000);
		Dictionary<int, UnitData> result = new();
		string path = $"{FileManager.ENCOUNTERS_PATH}\\{FileManager.EncounterTypes.Generic}.json";
		var encounterData = FileManager.GetFileFromJSON<EncounterListData>(path);
		EncounterData encounter = encounterData.GenericEncounterList[RandomManager.GetRandomValue(seed, 0, encounterData.GenericEncounterList.Count)];
		for (int i = 0; i < encounter.EnemyList.Count; i++)
		{
			int enemySeed = RandomManager.GetRandomValue(enemiesSetupSeed, 0, 100000000);
			UnitData enemy = encounter.EnemyList[i];
			SetupEnemy(enemy, enemySeed);
			result.Add(i, enemy);
		}
		enemyList = result;
	}

	void SetupLevelRewards()
	{
		foreach (var enemy in enemyList.Values)
		{
			int goldDroppedByUnit = BASE_ENEMY_GOLD_REWARD;

			Traits wealthyTrait = enemy.Traits.Find(t => t.Name == TraitsEnum.Wealthy.ToString());

			if (wealthyTrait != null)
				goldDroppedByUnit += TraitStruct.GetBonus(TraitsEnum.Wealthy, wealthyTrait.Level);

			goldReward += goldDroppedByUnit;
		}
	}

	public void SetupEnemy(UnitData unit, int enemySeed)
	{
		if(unit.RandomizedTraits)
			SetRandomTraits(unit, enemySeed);
	}

	public void SetRandomTraits(UnitData unit, int enemySeed)
	{
		List<Traits> unitTraits = new();
		int seed = (enemySeed * unit.Stats.Attack / unit.Stats.Hp * unit.Stats.Range + unit.Stats.Movement);
		int numOfTraits = RandomManager.GetRandomValue(seed, 0, 6);
		for (int i = 0; i < numOfTraits; i++)
		{
			bool breakOut = false;
			int failSafe = 1;
			while(!breakOut && failSafe < 100) {
                int traitSeed = RandomManager.GetRandomValue(seed * (i + 1) * failSafe, 0, 10000000);
                Traits randomTrait = GetRandomTrait(traitSeed, unit);
				if(!unitTraits.Any(t => t.Name == randomTrait.Name))
				{
                    unitTraits.Add(randomTrait);
                    breakOut = true;
                }

				failSafe++;
            }
		}
		unit.Traits = unitTraits;
	}

	public int[] GetValidStartingPositionForFaction(int faction)
	{
		return mapData.TileList.Where(t => t.StartPositionForFaction == faction && t.ValidForMovement).Select(t => t.PositionOnGrid).ToArray();
	}
    Traits GetRandomTrait(int traitSeed, UnitData unit)
    {
        List<TraitsEnum> availableTraits = new();
        foreach (TraitsEnum t in (TraitsEnum[])Enum.GetValues(typeof(TraitsEnum)))
		{
			List<TraitAttributes> attributes = TraitStruct.GetAttributesOfTrait(t);

			if(attributes.Contains(TraitAttributes.RandomFightTrait) && TraitStruct.ValidateAttributesForUnit(unit, attributes))
                availableTraits.Add(t);
        }

        TraitsEnum trait = availableTraits[RandomManager.GetRandomValue(traitSeed, 0, availableTraits.Count)];
        int traitLevel = RandomManager.GetRandomValue(traitSeed, 1, TraitStruct.GetMaxLevelOfTrait(trait) + 1);

        return new()
        {
            Name = trait.ToString(),
            Level = traitLevel
        };
    }
}