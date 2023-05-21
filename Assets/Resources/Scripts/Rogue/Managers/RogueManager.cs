using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RogueManager : MonoBehaviour
{
    const int MINIMUM_NODES = 2;
    const int MAXIMUM_NODES = 4;

    GeneralManager generalManager;
    public StructureManager structureManager;

    public RogueTile origin;
    public GameObject tile;
	readonly List<RogueTile> tileList = new();

    public Transform playerUnitTransform;

    public int currentRow;
    public int currentPositionOnRow;
    int maxNode;
    public Dictionary<SeedType, int> seedList = new();
    public int seed;

	public int Gold { get { return generalManager.Gold; } }
	public string GodSelected { get { return generalManager.GodSelected; } }

	public bool IsGameOver { get { return currentRow == maxNode; } }

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
        if (currentRow == maxNode)
            structureManager.GetRogueVictoryScreen();
	}

    public void SetupRogue(StructureManager structureManager, int currentNode, int masterSeed)
	{
        generalManager = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();

        this.currentRow = currentNode;
        Random.InitState(masterSeed);
        seedList.Add(SeedType.Master, masterSeed);
        seedList.Add(SeedType.RogueTile, Random.Range(0, 99999));
        seedList.Add(SeedType.MapLength, Random.Range(100000, 199999));
        seedList.Add(SeedType.NodesOnRow, Random.Range(200000, 299999));
        this.structureManager = structureManager;

        Random.InitState(seedList[SeedType.MapLength]);
        maxNode = Random.Range(5, 8);

        GenerateMap();

        playerUnitTransform.position = new Vector3(tileList[currentNode].transform.position.x, playerUnitTransform.position.y, playerUnitTransform.position.z);
    }

	void GenerateMap()
	{
        RogueTile originTile = origin;
        originTile.SetupTile(this, RogueTileType.Fight, 0, 0);
        tileList.Add(originTile);

        for (int i = 1; i <= maxNode; i++)
        {
            CreateRowOfNodes(i, origin.transform);
            SetNodesChilds(tileList.Where(t => t.mapRow == i - 1).ToList(), tileList.Where(t => t.mapRow == i).ToList());
        }


        GenerateNewNodeLines();
    }

    void CreateRowOfNodes(int row, Transform firstNode)
	{
        List<RogueTile> newNodes = new();
        List<RogueTile> previousRowNodes = tileList.Where(t => t.mapRow == row - 1).OrderBy(t=> t.mapRow).ToList();
        Random.InitState(seedList[SeedType.NodesOnRow] + row);
        int nodesOnCurrentRow;
        int nodesOnPreviousRow = previousRowNodes.Count;
        int maximumNodesPossibleForCurrentRow = nodesOnPreviousRow < MAXIMUM_NODES ? nodesOnPreviousRow + 1 : MAXIMUM_NODES;
        if (nodesOnPreviousRow < MINIMUM_NODES)
            nodesOnCurrentRow = nodesOnPreviousRow + 1;
        else
            nodesOnCurrentRow = Random.Range(MINIMUM_NODES, maximumNodesPossibleForCurrentRow + 1);

		for (int i = 0; i < nodesOnCurrentRow; i++)
		{
            Random.InitState(seedList[SeedType.RogueTile] + i);
            int randomLength = Random.Range(3, 6);
            newNodes.Add(structureManager.GenerateRogueTile(randomLength, row, i, tile.transform, firstNode, this));
        }
        
        if(newNodes != null)
		{
            tileList.AddRange(newNodes);
        }
    }

    void SetNodesChilds(List<RogueTile> tileParents, List<RogueTile> tileChilds)
	{
		foreach (var parent in tileParents)
		{
            parent.rogueChilds = tileChilds.Where(t =>
            (t.positionInRow == parent.positionInRow - 1) ||
            (t.positionInRow == parent.positionInRow) ||
            (t.positionInRow == parent.positionInRow + 1)).ToList();
		}
	}

    public void GenerateNewNodeLines()
	{
        structureManager.GenerateRogueLine(tileList);
    }

    public void NodeClicked(RogueTile tile)
	{
		if (IsAnyUnitMoving) return;
        
        if (currentRow == tile.mapRow - 1)
		{
            currentRow++;
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

    public enum SeedType
    {
        Master,
        RogueTile, //Specific data for each node (e.g. Type of node)
        MapLength,
        NodesOnRow, //How much nodes we need to generate in each row
    }
}