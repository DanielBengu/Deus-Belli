using System.Collections.Generic;
using UnityEngine;

public class RogueTile : MonoBehaviour
{
	RogueManager rogueManager;
	public RogueTileType rogueTileType;
	public List<RogueTile> rogueChilds;
	public int mapRow;
	public int positionInRow;

	public void SetupTile(RogueManager rm, RogueTileType tileType, int mapRow, int positionInRow)
	{
		rogueManager = rm;
		rogueTileType = tileType;
		this.mapRow = mapRow;
		this.positionInRow = positionInRow;
	}

    public void OnMouseDown()
	{
		if (rogueManager.IsGameInStandby)
			return;

		rogueManager.NodeClicked(this);
	}
}

public enum RogueTileType
{
	Fight,
	Merchant
}