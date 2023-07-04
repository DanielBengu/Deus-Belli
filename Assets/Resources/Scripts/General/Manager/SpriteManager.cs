using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public FightManager fightManager;
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
        if(typeSelection == TileType.Default)
		{
            if (tile.unitOnTile)
            {
                if (tile.unitOnTile.GetComponent<Unit>().faction == FightManager.USER_FACTION)
                    result += " Ally";
                else
                    result += " Enemy";
            }
            else
            {
                if (fightManager.UnitSelected && tile.IsPassable)
                    result += " Possible";
                else
                    result += " Selected";
            }
        } else
            result += $" {typeSelection}";

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
    Selected,
    Positionable,
}