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
		rogueManager.NodeClicked(this);
	}
}