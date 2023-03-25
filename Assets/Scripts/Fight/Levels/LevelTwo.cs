using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTwo : MonoBehaviour
{
    private const int _XLength = 10;
    private const int _YLength = 10;
    public Level level = new Level();

    public void StartLevel(){
        level.TopLeftSquarePositionX = 400;
        level.TopLeftSquarePositionZ = 1600;
        level.YPosition = 170;
        level.XLength = _XLength;
        level.YLength = _YLength;
        level.tilesDict = SetupTiles();
    }

    public Dictionary<int, GameObject> SetupTiles(){
        Dictionary<int, GameObject> tilesDict = new();
        GameObject tilePrefab = Resources.Load<GameObject>($"Prefabs/Fight/Tile");
        Sprite darkGrassMountain = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass_mountain base");
        Sprite darkGrass = Resources.Load<Sprite>($"Sprites/Terrain/dark_grass base");

        for (int i = 0; i < _XLength * _YLength; i++)
        {
            GameObject tileObject = GameObject.Instantiate(tilePrefab);
            tileObject.name = $"Terrain_{i}";
            SpriteRenderer spriteRenderer = tileObject.GetComponent<SpriteRenderer>();

            if(i == 7 || i == 20 || i == 17){
                spriteRenderer.sprite = darkGrassMountain;
                tileObject.GetComponent<Tile>().IsPassable = false;
            } else {
                spriteRenderer.sprite = darkGrass;
            }

            tilesDict.Add(i, tileObject);
        }
        return tilesDict;
    }
}