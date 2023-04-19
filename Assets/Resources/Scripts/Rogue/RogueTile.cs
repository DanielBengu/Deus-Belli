using UnityEngine;

public class RogueTile : MonoBehaviour
{
	RogueManager rogueManager;
    public int nodeNumber;

	public void SetupManager(RogueManager rm)
	{
		rogueManager = rm;
	}

    public void OnMouseDown()
	{
		if (rogueManager.IsGameInStandby)
			return;

		rogueManager.NodeClicked(this);
	}
}