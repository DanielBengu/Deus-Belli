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
        /*Color overlayColor = GetColor(typeSelection); // Set your desired color and transparency
        overlayColor.a = 0.2f;

        foreach (var tile in tilesToSelect)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();

            // Create a new material using the Standard shader
            Material newMaterial = new Material(Shader.Find("Standard"));
            newMaterial.color = overlayColor;

            // Assign the new material to the tile
            tileRenderer.material = newMaterial;
        }*/
    }

    public Color GetColor(TileType typeSelection)
	{
		return typeSelection switch
		{
			TileType.Ally => Color.yellow,
			TileType.Enemy => Color.red,
			TileType.Selected => Color.white,
			TileType.Positionable => Color.cyan,
			TileType.Possible => Color.magenta,
			TileType.Base => Color.clear,
            TileType.Default => Color.clear,
            _ => Color.clear,
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