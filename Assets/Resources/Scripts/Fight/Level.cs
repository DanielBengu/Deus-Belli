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

    public void StartLevel(int seed, Dictionary<int, GameObject> enemyList)
    {
		int MapSize = RandomManager.GetRandomValue(seed, 7, 8);
		TopLeftSquarePositionX = 400;
        TopLeftSquarePositionZ = 1600;
        YPosition = 170;
        HorizontalTiles = MapSize;
        VerticalTiles = MapSize;
        //tilesDict = SetupTiles();
        this.enemyList = enemyList;
    }

	public void SetupTiles()
	{
		Dictionary<int, GameObject> tilesDict = new();
		GameObject tilePrefab = Resources.Load<GameObject>($"Prefabs/Fight/Tile");
		Sprite darkGrassMountain = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass_mountain base");
		Sprite darkGrass = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass base");

		for (int i = 0; i < HorizontalTiles * VerticalTiles; i++)
		{
			GameObject tileObject = Object.Instantiate(tilePrefab);
			tileObject.name = $"Terrain_{i}";
			SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();

			if (i == 7 || i == 20 || i == 17 || i == 16 || i == 24 || i == 31 || i == 32)
			{
				spriteRenderer.sprite = darkGrassMountain;
				Tile tileScript = tileObject.GetComponent<Tile>();
				tileScript.IsPassable = false;
				tileScript.MovementCost = 10;
			}
			else
			{
				spriteRenderer.sprite = darkGrass;
			}

			tilesDict.Add(i, tileObject);
		}
		this.tilesDict = tilesDict;
	}
}