using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    FightManager fightManager;
    StructureManager structureManager;

    void Start()
    {
        fightManager = GetComponent<FightManager>();
        structureManager = GetComponent<StructureManager>();
    }

    public void GenerateTileSelection(List<Tile> tilesToSelect, TileType typeSelection = TileType.Default)
    {
        foreach (var tile in tilesToSelect)
        {
            string spriteName = GetTileSelection(tile, typeSelection);
            Sprite sprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteName}");
            ChangeObjectSprite(tile.gameObject, sprite);
        }
    }

    public void ClearMapTilesSprite()
    {
        foreach (var tile in structureManager.gameData.mapTiles.Values)
        {
            string spriteName = tile.GetComponent<SpriteRenderer>().sprite.name;
            Sprite newSprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteName.Split(' ')[0] + " base"}");
            ChangeObjectSprite(tile.gameObject, newSprite);
        }
    }

    void ChangeObjectSprite(GameObject obj, Sprite sprite)
    {
        SpriteRenderer spriteRendererOld = obj.GetComponent<SpriteRenderer>();
        spriteRendererOld.sprite = sprite;
    }

    string GetTileSelection(Tile tile, TileType typeSelection)
	{
        string result = tile.gameObject.GetComponent<SpriteRenderer>().sprite.name.Split(' ')[0];
        switch (typeSelection)
		{
			case TileType.Default:
                if (tile.unitOnTile)
                {
                    if (tile.unitOnTile.GetComponent<Unit>().faction == FightManager.USER_FACTION)
                        result += " ally";
                    else
                        result += " enemy";
                }
                else
                {
                    if (fightManager.UnitSelected && tile.IsPassable)
                        result += " possible";
                    else
                        result += " selected";
                }
                break;
			case TileType.Base:
                result += " base";
				break;
			case TileType.Ally:
                result += " ally";
                break;
			case TileType.Enemy:
                result += " enemy";
                break;
			case TileType.Possible:
                result += " possible";
                break;
			case TileType.Selected:
                result += " selected";
                break;
		}
        return result;
	}
}

public enum TileType
{
    Default,
    Base,
    Ally,
    Enemy,
    Possible,
    Selected
}