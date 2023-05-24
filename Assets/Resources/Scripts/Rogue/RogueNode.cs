using System.Collections.Generic;
using UnityEngine;

public class RogueNode : MonoBehaviour
{
	RogueManager rogueManager;
	public RogueTileType rogueTileType;
	public List<RogueNode> rogueParents;
	public List<RogueNode> rogueChilds;
	public int mapRow;
	public int positionInRow;

	public void SetupTile(RogueManager rm, RogueTileType tileType, int mapRow, int positionInRow)
	{
		rogueManager = rm;
		rogueTileType = tileType;
		this.mapRow = mapRow;
		this.positionInRow = positionInRow;
		SetupMaterial();
	}

    public void OnMouseDown()
	{
		if (rogueManager.IsGameInStandby)
			return;

		rogueManager.NodeClicked(this);
	}

	void SetupMaterial()
	{
		Material objMaterial = GetComponent<MeshRenderer>().material;
		switch (rogueTileType)
		{
			case RogueTileType.Starting:
				objMaterial.color = Color.green;
				break;
			case RogueTileType.Boss:
				objMaterial.color = Color.black;
				break;
			case RogueTileType.Fight:
				objMaterial.color = Color.red;
				break;
			case RogueTileType.Merchant:
				objMaterial.color = Color.yellow;
				break;
			case RogueTileType.Miniboss:
				objMaterial.color = Color.blue;
				break;
		}
	}
}

public enum RogueTileType
{
	Starting,
	Boss,
	Fight,
	Merchant,
	Miniboss
}