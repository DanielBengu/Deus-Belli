using System.Collections.Generic;
using UnityEngine;

public class RogueTile : MonoBehaviour
{
	RogueManager rogueManager;
    public int nodeNumber;
	public RogueTileType rogueTileType;
	public List<RogueTile> rogueChilds;

	public void SetupTile(RogueManager rm, RogueTileType tileType, int nodeNumber)
	{
		rogueManager = rm;
		rogueTileType = tileType;
		this.nodeNumber = nodeNumber;
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