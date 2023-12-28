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
        Color overlayColor = GetColor(typeSelection); // Set your desired color and transparency


        foreach (var tile in tilesToSelect)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            tileRenderer.material.color = overlayColor;
        }
    }

    public Color GetColor(TileType typeSelection)
	{
		return typeSelection switch
		{
			TileType.Ally => Color.yellow,
			TileType.Enemy => Color.red,
			TileType.Selected => Color.blue,
			TileType.Positionable => new(1.0f, 0.5f, 0.0f, 1.0f), //orange
			TileType.Possible => Color.magenta,
			TileType.Base => Color.white,
            TileType.Default => Color.white,
            _ => Color.white,
		};
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