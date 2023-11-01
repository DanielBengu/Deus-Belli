using System.Collections.Generic;
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
		Dictionary<int, GameObject> result = new();
		GameObject tilePrefab = Resources.Load<GameObject>($"Prefabs/Fight/Tile");
		Sprite darkGrass = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass base");

		for (int i = 0; i < HorizontalTiles * VerticalTiles; i++)
		{
			GameObject tileObject = Object.Instantiate(tilePrefab);
			tileObject.name = $"Terrain_{i}";
			SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();
			spriteRenderer.sprite = darkGrass;
			result.Add(i, tileObject);
		}
		this.tilesDict = GenerateObstacles(result);
	}

	public Dictionary<int, GameObject> GenerateObstacles(Dictionary<int, GameObject> baseTerrain)
	{
		Sprite darkGrassMountain = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass_mountain base");
		GameObject mountainModel3D = Resources.Load<GameObject>($"Prefabs/Fight/Objects/Mountain");
		int numberOfMountains = RandomManager.GetRandomValue(seed, 0, 10);
		
		for (int i = 0; i < numberOfMountains; i++)
		{
			int mountainSeed = seed * (i + 1);
			int tileToChange = RandomManager.GetRandomValue(mountainSeed, 0, HorizontalTiles * VerticalTiles);

			GameObject tile = baseTerrain[tileToChange];
			tile.GetComponent<SpriteRenderer>().sprite = darkGrassMountain;
			Tile tileScript = tile.GetComponent<Tile>();
			tileScript.model3D = mountainModel3D;
			tileScript.IsPassable = false;
			tileScript.MovementCost = 10;
		}

		return baseTerrain;
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