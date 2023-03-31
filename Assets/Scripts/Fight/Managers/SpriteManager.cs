using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    FightManager fightManager;

    void Start()
    {
        fightManager = this.GetComponent<FightManager>();
    }

    public void GenerateTileSelection(List<Tile> tilesToSelect)
    {
        foreach (var tile in tilesToSelect)
        {
            string spriteName = tile.gameObject.GetComponent<SpriteRenderer>().sprite.name.Split(' ')[0];
            if (tile.unitOnTile)
            {
                if (tile.unitOnTile.GetComponent<Unit>().faction == FightManager.USER_FACTION)
                    spriteName += " ally";
                else
                    spriteName += " enemy";
            }
            else
            {
                if (tile.IsPassable)
                    spriteName += " possible";
                else
                    spriteName += " base";
            }
            Sprite sprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteName}");
            ChangeObjectSprite(tile.gameObject, sprite);
        }
    }

    public void ClearSelectedTilesSprite()
    {
        foreach (var tile in fightManager.TilesSelected)
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
}
