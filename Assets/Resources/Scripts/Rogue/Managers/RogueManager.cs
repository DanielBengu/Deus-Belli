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

    public RogueNode origin;
    public GameObject tile;
	public readonly List<RogueNode> tileList = new();

    public Transform playerUnitTransform;

	public int CurrentRow { get { return generalManager.CurrentRow; } set { generalManager.CurrentRow = value; } }
    public int CurrentPositionInRow { get { return generalManager.CurrentPositionInRow; } set { generalManager.CurrentPositionInRow = value; } }

    int maxNode;
    public Dictionary<SeedType, int> seedList = new();
    public int seed;

	public int Gold { get { return generalManager.Gold; } }
	public string GodSelected { get { return generalManager.GodSelected; } }

	public bool IsGameOver { get { return CurrentRow == maxNode; } }

	public bool IsAnyUnitMoving { get { return structureManager.IsObjectMoving; } }

    public bool IsGameInStandby { get { return generalManager.IsGameInStandby; } }

	private void Update()
	{
        bool isPlayerMovementFinished = structureManager.MovementTick();
        if (isPlayerMovementFinished)
            StartEncounter();
    }

    void StartEncounter()
	{
        RogueNode selectedNode = generalManager.selectedNode;
        if (selectedNode.IsFightEncounter())
            generalManager.StartFight();
        else if (selectedNode.rogueTileType == RogueTileType.Merchant)
            generalManager.StartMerchant();
        else if(selectedNode.rogueTileType == RogueTileType.Event)
            generalManager.StartEvent();
    }

    public void IsRunCompleted()
	{
        if (CurrentRow == maxNode)
            structureManager.GetRogueVictoryScreen();
	}

    public void SetupRogue(StructureManager structureManager, int currentRow, int currentPositionOnRow, int masterSeed)
	{
        generalManager = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();

        CurrentRow = currentRow;
        CurrentPositionInRow = currentPositionOnRow;
        Random.InitState(masterSeed);
        seedList.Add(SeedType.Master, masterSeed);
        seedList.Add(SeedType.RogueTile, Random.Range(0, 99999));
        seedList.Add(SeedType.MapLength, Random.Range(100000, 199999));
        seedList.Add(SeedType.NodesOnRow, Random.Range(200000, 299999));
        this.structureManager = structureManager;

        Random.InitState(seedList[SeedType.MapLength]);
        maxNode = Random.Range(7, 9);
        
        GenerateMap();
        RogueNode currentNode = tileList.Find(t => t.mapRow == currentRow && t.positionInRow == currentPositionOnRow);
        playerUnitTransform.position = new Vector3(currentNode.transform.position.x, playerUnitTransform.position.y, currentNode.transform.position.z);
    }

	void GenerateMap()
	{
        RogueNode originTile = origin;
        originTile.SetupTile(this, RogueTileType.Starting, originTile.mapRow, originTile.positionInRow, 0, new());
        tileList.Add(originTile);

        for (int i = 1; i <= maxNode; i++)
        {
            CreateRowOfNodes(i, maxNode, origin.transform);
            SetNodesChilds(tileList.Where(t => t.mapRow == i - 1).ToList(), tileList.Where(t => t.mapRow == i).ToList());
        }


        GenerateNewNodeLines();
    }

    void CreateRowOfNodes(int row, int maxRowOfMap, Transform firstNode)
	{
        List<RogueNode> newNodes = new();
        List<RogueNode> previousRowNodes = tileList.Where(t => t.mapRow == row - 1).OrderBy(t=> t.positionInRow).ToList();
        Random.InitState(seedList[SeedType.NodesOnRow] + row);
        int nodesOnCurrentRow;
        int nodesOnPreviousRow = previousRowNodes.Count;
        int maximumNodesPossibleForCurrentRow = nodesOnPreviousRow < MAXIMUM_NODES ? nodesOnPreviousRow + 1 : MAXIMUM_NODES;
        if (nodesOnPreviousRow < MINIMUM_NODES)
            nodesOnCurrentRow = nodesOnPreviousRow + 1;
        else if (row == maxNode)
            nodesOnCurrentRow = 1;
        else if (row == maxNode - 1)
            nodesOnCurrentRow = 2;
        else
            nodesOnCurrentRow = Random.Range(MINIMUM_NODES, maximumNodesPossibleForCurrentRow + 1);

		for (int i = 0; i < nodesOnCurrentRow; i++)
		{
            int positionOnRow = i + FindOffsetPositionOnRow(nodesOnCurrentRow, row, previousRowNodes);
            if(positionOnRow == -1)
			{
                Debug.Log($"Error during map generation for seed {seedList[SeedType.Master]}: positionOnRow returned -1");
			}

            Random.InitState(seedList[SeedType.RogueTile] + i);
            int randomLength = Random.Range(3, 6);
            newNodes.Add(structureManager.GenerateRogueTile(randomLength, row, positionOnRow, maxRowOfMap, tileList.Count + i + 1, tile.transform, firstNode, this));
        }
        
        if(newNodes != null)
		{
            tileList.AddRange(newNodes);
        }
    }

    int FindOffsetPositionOnRow(int nodesOnCurrentRow, int currentRow, List<RogueNode> previousRowNodes)
	{
        int randomSeed = seedList[SeedType.NodesOnRow] * nodesOnCurrentRow * (currentRow + 1);

        Random.InitState(randomSeed);
        int firstNodeInPreviousRowPosition = previousRowNodes.First().positionInRow;

        int minimumOffset = -1;
        if (nodesOnCurrentRow == previousRowNodes.Count - 1)
            minimumOffset = firstNodeInPreviousRowPosition;
        else
            minimumOffset = firstNodeInPreviousRowPosition - 1 >= 0 ? firstNodeInPreviousRowPosition - 1 : 0;

        int maximumOffset = -1;
        if (nodesOnCurrentRow == previousRowNodes.Count + 1)
            maximumOffset = firstNodeInPreviousRowPosition;
        else
            maximumOffset = firstNodeInPreviousRowPosition + 1 <= MAXIMUM_NODES ? firstNodeInPreviousRowPosition + 1 : MAXIMUM_NODES;

        if (minimumOffset == -1 || maximumOffset == -1)
            return -1;

        int position;
        if (previousRowNodes.Count == MAXIMUM_NODES && nodesOnCurrentRow == MINIMUM_NODES)
            position = firstNodeInPreviousRowPosition + 1;
        else if (previousRowNodes.Count == 1)
            position = Random.Range(minimumOffset, firstNodeInPreviousRowPosition + 1);
        else if (previousRowNodes.Count == 2 && nodesOnCurrentRow == 1)
            position = Random.Range(firstNodeInPreviousRowPosition, firstNodeInPreviousRowPosition + 1);
        else if (previousRowNodes.Count == 4 && nodesOnCurrentRow == 2)
            position = firstNodeInPreviousRowPosition + 1;
        else
            position = Random.Range(minimumOffset, maximumOffset + 1);


		return position;
    }

    void SetNodesChilds(List<RogueNode> tileParents, List<RogueNode> tileChilds)
	{
        
        foreach (var parent in tileParents)
		{
            Random.InitState(seedList[SeedType.RogueTile] * parent.mapRow * parent.positionInRow);
            List<RogueNode> possibleLinks = tileChilds.Where(t =>
            (t.positionInRow == parent.positionInRow - 1) ||
            (t.positionInRow == parent.positionInRow) ||
            (t.positionInRow == parent.positionInRow + 1)).ToList();

            bool isParentTileFirstInRow = parent.positionInRow == tileParents.Min(t => t.positionInRow);
            bool isParentTileLastInRow = parent.positionInRow == tileParents.Max(t => t.positionInRow);
            
            List<RogueNode> linksToAdd = new();

			foreach (var link in possibleLinks)
			{
                RogueNode previousNodeInParentRow = tileParents.Find(t => t.positionInRow == parent.positionInRow - 1);
                bool isLastPossibleLinkForChild = parent.positionInRow == link.positionInRow + 1;
                bool isCrossingWithPreviousParent = previousNodeInParentRow != null && previousNodeInParentRow.rogueChilds.Any(t => t.positionInRow == link.positionInRow + 1); //Check that we do not create a crossing of links (e.g. A1 <-> B2 && A2 <-> B1
                if (isParentTileFirstInRow && link.positionInRow == possibleLinks.Min(t => t.positionInRow) ||
                        isParentTileLastInRow && link.positionInRow == possibleLinks.Max(t => t.positionInRow))
                    linksToAdd.Add(link);
                else if (!isCrossingWithPreviousParent && Random.Range(0, 2) == 1)
                    linksToAdd.Add(link);
            }

            parent.rogueChilds.AddRange(linksToAdd);
            linksToAdd.ForEach(t => t.rogueParents.Add(parent));
        }

		//Check for unlinked childs to force
		foreach (var node in tileChilds.Where(t => t.rogueParents.Count == 0))
		{
            Random.InitState(seedList[SeedType.RogueTile] * node.mapRow * node.positionInRow);
            List<RogueNode> possibleChoices = tileParents.Where(t =>
            (t.positionInRow == node.positionInRow - 1) ||
            (t.positionInRow == node.positionInRow) ||
            (t.positionInRow == node.positionInRow + 1)).ToList();
            List<RogueNode> tempChoices = new();
			foreach (var parent in possibleChoices)
			{
                RogueNode previousNodeInParentRow = tileParents.Find(t => t.positionInRow == parent.positionInRow - 1);
                bool isCrossingWithPreviousParent = previousNodeInParentRow != null && previousNodeInParentRow.rogueChilds.Any(t => t.positionInRow == node.positionInRow + 1); //Check that we do not create a crossing of links (e.g. A1 <-> B2 && A2 <-> B1
                if (!isCrossingWithPreviousParent)
                    tempChoices.Add(parent);
            }

            int randomChoice = Random.Range(tempChoices.Min(t => t.positionInRow), tempChoices.Max(t => t.positionInRow) + 1);
            RogueNode parentToLink = tileParents.Find(t => t.positionInRow == randomChoice);
            parentToLink.rogueChilds.Add(node);
            node.rogueParents.Add(parentToLink);
        }

        //Check for unlinked parents to force
        foreach (var node in tileParents.Where(t => t.rogueChilds.Count == 0))
        {
            Random.InitState(seedList[SeedType.RogueTile] * node.mapRow * node.positionInRow);
            List<RogueNode> possibleChoices = tileChilds.Where(t =>
            (t.positionInRow == node.positionInRow - 1) ||
            (t.positionInRow == node.positionInRow) ||
            (t.positionInRow == node.positionInRow + 1)).ToList();
            List<RogueNode> tempChoices = new();
            foreach (var child in possibleChoices)
            {
                RogueNode followingNodeInChildRow = tileChilds.Find(t => t.positionInRow == child.positionInRow + 1);
                bool isCrossingWithFollowingParent = followingNodeInChildRow != null && followingNodeInChildRow.rogueParents.Any(t => t.positionInRow == node.positionInRow -1); //Check that we do not create a crossing of links (e.g. A1 <-> B2 && A2 <-> B1
                if (!isCrossingWithFollowingParent)
                    tempChoices.Add(child);
            }

            int randomChoice = Random.Range(tempChoices.Min(t => t.positionInRow), tempChoices.Max(t => t.positionInRow) + 1);
            RogueNode childToLink = tileChilds.Find(t => t.positionInRow == randomChoice);
            childToLink.rogueParents.Add(node);
            node.rogueChilds.Add(childToLink);
        }
    }

    public void GenerateNewNodeLines()
	{
        structureManager.GenerateRogueLine(tileList);
    }

    public void NodeClicked(RogueNode tile)
	{
		if (IsAnyUnitMoving) return;

        RogueNode startingNode = tileList.Find(t => t.mapRow == CurrentRow && t.positionInRow == CurrentPositionInRow);
        if (startingNode.rogueChilds.Contains(tile))
		{
            generalManager.selectedNode = tile;
            CurrentRow = tile.mapRow;
            CurrentPositionInRow = tile.positionInRow;
            structureManager.MoveUnit(playerUnitTransform, tile);
        }
            
    }

    //Method called by the red "Abandon Run" button on the Rogue section UI
    public void AbandonRunClick()
	{
        if (!generalManager.IsGameInStandby)
            EndRun(1);
	}

    public void EventChoiceClick(int choice)
	{
        GeneralManager fm = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
        fm.ReturnToRogueFromEventButton();
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
        NodesOnRow, //How much nodes we need to generate in each row and position offset
    }
}