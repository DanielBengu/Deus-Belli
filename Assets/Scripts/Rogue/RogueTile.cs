using System.Collections;
using System.Collections.Generic;
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
		//We only enable the input of the next playable node
		if (rogueManager.GetPlayerCurrentNode() != nodeNumber - 1)
			return;

		rogueManager.NodeClicked();
	}
}