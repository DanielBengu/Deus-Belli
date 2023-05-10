using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RogueManager : MonoBehaviour
{
    GeneralManager generalManager;
    public StructureManager structureManager;

    public RogueTile origin;
    public GameObject tile;
	readonly List<RogueTile> tileList = new();

    public Transform playerUnitTransform;

    private LineRenderer lineRenderer;

    public int currentNode;
	readonly int maxNode = 5;
    public PRNG seed;

	public int Gold { get { return generalManager.Gold; } }
	public string GodSelected { get { return generalManager.GodSelected; } }

	public bool IsGameOver { get { return currentNode == maxNode; } }

	public bool IsAnyUnitMoving { get { return structureManager.IsObjectMoving; } }

    public bool IsGameInStandby { get { return generalManager.IsGameInStandby; } }

	private void Update()
	{
        bool isPlayerMovementFinished = structureManager.MovementTick();
        if (isPlayerMovementFinished)
            generalManager.StartFight();
    }

    public void IsRunCompleted()
	{
        if (currentNode == maxNode)
            structureManager.GetRogueVictoryScreen();
	}

    public void SetupRogue(StructureManager structureManager, int currentNode, PRNG seed)
	{
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = maxNode + 1;
        generalManager = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();

        this.currentNode = currentNode;
        this.seed = seed;
        this.structureManager = structureManager;

        GenerateMap();

        playerUnitTransform.position = new Vector3(tileList[currentNode].transform.position.x, playerUnitTransform.position.y, playerUnitTransform.position.z);
    }

	void GenerateMap()
	{
        int tileLength = seed.Next(10);
        RogueTile originTile = origin;
        originTile.SetupTile(this, RogueTileType.Fight);
        tileList.Add(originTile);
		for (int i = 0; i < maxNode; i++) originTile = CreateNewNode(tileLength, originTile, origin.transform);

        GenerateNewNodeLines();
    }

    RogueTile CreateNewNode(int randomLength, RogueTile destinationTile, Transform parent)
	{
        RogueTile newTileScript = structureManager.GenerateRogueTiles(randomLength, destinationTile, tile.transform, parent, this);
        tileList.Add(newTileScript);
        return newTileScript;
    }

    public void GenerateNewNodeLines()
	{
        structureManager.GenerateRogueLine(tileList, lineRenderer);
    }

    public void NodeClicked(RogueTile tile)
	{
		if (IsAnyUnitMoving) return;
        
        if (currentNode == tile.nodeNumber - 1)
		{
            currentNode++;
            structureManager.MoveUnit(playerUnitTransform, tile);
        }
            
    }

    //Method called by the red "Abandon Run" button on the Rogue section UI
    public void AbandonRunClick()
	{
        if (!generalManager.IsGameInStandby)
            EndRun(1);
	}

    public void EndRun(int runType)
	{
        RunEndType runEndType = (RunEndType)runType;
		switch (runEndType)
		{
            //Player temporarily closed the run
			case RunEndType.Close:
                GeneralManager.CloseRun();
                SceneManager.LoadScene(0);
                break;
            //Player abandoned the run
			case RunEndType.Abandon:
                GeneralManager.CloseRun();
                break;
            //Player completed a run
			case RunEndType.Finish:
                GeneralManager.CloseRun();
                break;
		}
    }

    public enum RunEndType
	{
        Close,
        Abandon,
        Finish
	}
}