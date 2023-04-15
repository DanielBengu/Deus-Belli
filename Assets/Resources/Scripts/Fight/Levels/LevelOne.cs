using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOne : MonoBehaviour
{
    [SerializeField]
    GameObject sorceressPrefab;
    [SerializeField]
    GameObject sorceress2Prefab;
    private const int _XLength = 7;
    private const int _YLength = 7;
    public Level level = new();

    public int enemyStartTile;

    public void StartLevel(){
        level.TopLeftSquarePositionX = 400;
        level.TopLeftSquarePositionZ = 1600;
        level.YPosition = 170;
        level.XLength = _XLength;
        level.YLength = _YLength;
        level.tilesDict = SetupTiles();
        level.enemyList = SetupEnemies();
    }

    Dictionary<int, GameObject> SetupTiles(){
        Dictionary<int, GameObject> tilesDict = new();
        GameObject tilePrefab = Resources.Load<GameObject>($"Prefabs/Fight/Tile");
        Sprite darkGrassMountain = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass_mountain base");
        Sprite darkGrass = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass base");

        for (int i = 0; i < _XLength * _YLength; i++)
        {
            GameObject tileObject = GameObject.Instantiate(tilePrefab);
            tileObject.name = $"Terrain_{i}";
            SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();

            if(i == 7 || i == 20 || i == 17 || i == 16 || i == 24 || i == 31 || i == 32){
                spriteRenderer.sprite = darkGrassMountain;
                Tile tileScript = tileObject.GetComponent<Tile>();
                tileScript.IsPassable = false;
                tileScript.MovementCost = 10;
            } else {
                spriteRenderer.sprite = darkGrass;
            }

            tilesDict.Add(i, tileObject);
        }
        return tilesDict;
    }

    Dictionary<int, GameObject> SetupEnemies(){
        Dictionary<int, GameObject> enemyList = new();

        Unit sorceressUnit = sorceressPrefab.GetComponent<Unit>();
        Unit sorceressUnit2 = sorceress2Prefab.GetComponent<Unit>();
        sorceressUnit.faction = 1;
        sorceressUnit2.faction = 1;
        

        enemyList.Add(enemyStartTile, sorceressUnit.gameObject);

        return enemyList;
    }
}