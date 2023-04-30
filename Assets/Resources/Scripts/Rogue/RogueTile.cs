using UnityEngine;

public class RogueTile : MonoBehaviour
{
	RogueManager rogueManager;
    public int nodeNumber;
	public RogueTileType rogueTileType;

	public void SetupTile(RogueManager rm, RogueTileType tileType)
	{
		rogueManager = rm;
		rogueTileType = tileType;
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