using System.Collections.Generic;
using UnityEngine;

public class RogueNode : MonoBehaviour
{
	RogueManager rogueManager;
	public Level level = new();
	public Merchant merchant;
	public Event currentEvent;
	public RogueTileType rogueTileType;
	public List<RogueNode> rogueParents;
	public List<RogueNode> rogueChilds;
	public int mapRow;
	public int positionInRow;
	public int nodeIndex;
	public int nodeSeed;

	public void SetupTile(RogueManager rm, RogueTileType tileType, int mapRow, int positionInRow, int nodeIndex, int nodeSeed)
	{
		rogueManager = rm;
		rogueTileType = tileType;
		this.mapRow = mapRow;
		this.positionInRow = positionInRow;
		this.nodeIndex = nodeIndex;
		this.nodeSeed = nodeSeed;
		SetupMaterial();
			
		switch (tileType)
		{
			case RogueTileType.Boss:
			case RogueTileType.Miniboss:
			case RogueTileType.Fight:
				level.StartLevel(nodeSeed, tileType, mapRow, rm.generalManager.Difficulty);
				break;
			case RogueTileType.Merchant:
				merchant = new(nodeSeed);
				break;
			case RogueTileType.Event:
				currentEvent = new(nodeSeed);
				break;
			case RogueTileType.Starting:
				break;
		}
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
			case RogueTileType.Event:
				objMaterial.color = Color.magenta;
				break;
		}
	}

	public bool IsFightEncounter()
	{
		return rogueTileType == RogueTileType.Fight || rogueTileType == RogueTileType.Miniboss || rogueTileType == RogueTileType.Boss;
	}
}

public enum RogueTileType
{
	Fight,
	Merchant,
	Event,
	//Out of bound for general map generation
	Miniboss,
	Starting,
	Boss
}