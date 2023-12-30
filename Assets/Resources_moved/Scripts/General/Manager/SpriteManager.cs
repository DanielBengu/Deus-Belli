using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public FightManager fightManager;

    //Use RGB 0-1, not RGB 0-255
    readonly Color COLOR_ALLY = Color.yellow;
	readonly Color COLOR_ENEMY = Color.red;
    readonly Color COLOR_SELECTED = Color.blue;
    readonly Color COLOR_POSITIONABLE = new(0.04f, 0.75f, 0.05f, 1.0f); //new(1.0f, 0.5f, 0.0f, 1.0f); //orange

	readonly Color COLOR_POSSIBLE = Color.magenta;
    readonly Color COLOR_BASE = Color.white; //This removes all previous colors and leaves the default textures

	void Start()
    {
        fightManager = GetComponent<FightManager>();
    }

    public void GenerateTileSelection(List<Tile> tilesToSelect, bool isSetup, TileType typeSelection)
    {
        foreach (var tile in tilesToSelect)
        {
			Color overlayColor = GetColor(typeSelection, tile, isSetup);
			Renderer tileRenderer = tile.GetComponent<Renderer>();
            tileRenderer.material.color = overlayColor;
        }
    }

    public Color GetColor(TileType typeSelection, Tile tile, bool isSetup)
	{
		return typeSelection switch
		{
			TileType.Ally => COLOR_ALLY,
			TileType.Enemy => COLOR_ENEMY,
			TileType.Selected => COLOR_SELECTED,
			TileType.Positionable => COLOR_POSITIONABLE,
			TileType.Possible => COLOR_POSSIBLE,
			TileType.Base => COLOR_BASE,
            TileType.Default => GetDefaultColor(tile, isSetup),
            _ => Color.white,
		};
	}

    public Color GetDefaultColor(Tile tile, bool isSetup)
    {
        bool isSetupForPlayer = (isSetup && tile.data.StartPositionForFaction == FightManager.USER_FACTION);
		return isSetupForPlayer ? COLOR_POSITIONABLE : COLOR_BASE;
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