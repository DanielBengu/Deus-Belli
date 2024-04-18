using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI.Table;

public class RogueManager : MonoBehaviour
{
    const int MINIMUM_NODES = 2;
    const int MAXIMUM_NODES = 4;

    public GeneralManager GeneralManager { get; set; }
    public StructureManager StructureManager { get; set; }

    public RogueNode origin;
    public GameObject tile;

    public List<NodeData> mapList = new();

    public Transform playerUnitTransform;
    public float playerSpeed;
    public float cameraSpeed;
    static readonly int MAP_LENGTH = 10;
    public int MapLength { get; set; }
    public Dictionary<SeedType, int> seedList = new();

	public bool IsGameOver { get { return GeneralManager.CurrentRow == MapLength; } }

	public bool IsAnyUnitMoving { get { return StructureManager.IsObjectMoving; } }

    public bool IsGameInStandby { get { return GeneralManager.IsGameInStandby; } }
    public DB_Event CurrentEvent { get; set; }
	public Merchant MerchantShop { get; set; }
    public RogueObjectMoving ObjectMoving { get; set; }

    private void Update()
	{
        ManageMovement();
    }

    void ManageMovement()
    {
        switch (ObjectMoving)
        {
            case RogueObjectMoving.Camera:
                StructureManager.MovementTick(cameraSpeed, CameraSwitch);
                break;
            case RogueObjectMoving.PlayerCharacter:
                StructureManager.MovementTick(playerSpeed, StartEncounter);
                break;
        }
	}

    void CameraSwitch()
    {
        ResetObjectMoving();
    }

    void StartEncounter()
	{
        RogueNode selectedNode = GeneralManager.selectedNode;
        GeneralManager.StartSection(selectedNode.rogueTileType);
        ResetObjectMoving();
    }

    void ResetObjectMoving()
    {
        ObjectMoving = RogueObjectMoving.Nothing;
    }

    public void IsRunCompleted(bool isDefeat)
	{
        GameScreens gameScreen = GameScreens.Default;

        if (isDefeat)
            gameScreen = GameScreens.RogueDefeatScreen;
        else if (GeneralManager.CurrentRow == MapLength)
            gameScreen = GameScreens.RogueVictoryScreen;

        if(gameScreen != GameScreens.Default)
            StructureManager.GetGameScreen(gameScreen, GeneralManager.Gold);
	}

    public void SetupRogue(StructureManager sm, GeneralManager gm, int currentRow, int currentPositionOnRow, int masterSeed)
	{
        GeneralManager = gm;
        gm.CurrentRow = currentRow;
        gm.CurrentPositionInRow = currentPositionOnRow;

        StructureManager = sm;
        StructureManager.uiManager.SetRogueVariables(gm.Gold, gm.GodSelected.ToString());

        seedList.Add(SeedType.Master, masterSeed);
        seedList.Add(SeedType.RogueTile, RandomManager.GetRandomValue(masterSeed, 0, 99999));
        seedList.Add(SeedType.MapLength, RandomManager.GetRandomValue(masterSeed, 100000, 199999));
        seedList.Add(SeedType.NodesOnRow, RandomManager.GetRandomValue(masterSeed, 200000, 299999));
        

        MapLength = MAP_LENGTH;

        //We generate the abstract version of the whole map
        GenerateMap();

        //We find the current node of the player and we put the player character on it
        RogueNode currentNodeScript = GameObject.Find($"RogueTile").GetComponent<RogueNode>();
        playerUnitTransform.position = new Vector3(currentNodeScript.transform.position.x, playerUnitTransform.position.y, currentNodeScript.transform.position.z);

        //We generate the physical nodes that are shown on the table
        GeneratePhysicalMap(currentNodeScript, mapList.Find(n => n.row == currentNodeScript.mapRow && n.positionInRow == currentNodeScript.positionInRow));
    }

    List<NodeData> GenerateMap()
    {
        NodeData startingNode = new(RogueTileType.Starting, 0, 1, 0, new(), new());
        mapList.Add(startingNode);
        List<NodeData> result = new()
        {
            startingNode
        };

        for (int i = 1; i <= MapLength; i++)
        {
            result.AddRange(GenerateNodeRow(i));
        };

        return result;
    }

    List<NodeData> GenerateNodeRow(int row)
    {
        List<NodeData> nodeRow = new();
        List<NodeData> previousRowNodes = mapList.Where(t => t.row == row - 1).OrderBy(t => t.positionInRow).ToList();
        int nodesOnCurrentRow = GenerateAmountNodesCurrentRow(row, previousRowNodes);

        for (int i = 0; i < nodesOnCurrentRow; i++)
        {
            int positionOnRow = i + FindOffsetPositionOnRow(nodesOnCurrentRow, row, previousRowNodes);
            int nodeIndex = mapList.Count + i + 1;
            int nodeSeed = seedList[SeedType.RogueTile] * nodeIndex;
            RogueTileType nodeType = StructureManager.GenerateRogueNodeType(row, MapLength, this, nodeIndex);
            NodeData node = new(nodeType, row, positionOnRow, nodeSeed, new(), new());

            nodeRow.Add(node);
            mapList.Add(node);

            SetNodesChilds(mapList.Where(t => t.row == i - 1).ToList(), mapList.Where(t => t.row == i).ToList());
        }

        return nodeRow;
    }

    int GenerateAmountNodesCurrentRow(int row, List<NodeData> previousRowNodes)
    {
        int nodesOnCurrentRow;
        int nodesOnPreviousRow = previousRowNodes.Count;
        int maximumNodesPossibleForCurrentRow = nodesOnPreviousRow < MAXIMUM_NODES ? nodesOnPreviousRow + 1 : MAXIMUM_NODES;

        //Since a row with 4 nodes is locked at position 0 we can't generate it if the previous node starts with a position of 2 or greater
        if (previousRowNodes.Max(n => n.positionInRow) > 1 && maximumNodesPossibleForCurrentRow == 4) maximumNodesPossibleForCurrentRow -= 1;

        if (nodesOnPreviousRow < MINIMUM_NODES)
            nodesOnCurrentRow = nodesOnPreviousRow + 1;
        else if (row == MapLength)
            nodesOnCurrentRow = 1;
        else if (row == MapLength - 1)
            nodesOnCurrentRow = 2;
        else
            nodesOnCurrentRow = RandomManager.GetRandomValue(seedList[SeedType.NodesOnRow] + row, MINIMUM_NODES, maximumNodesPossibleForCurrentRow + 1);
        
        return nodesOnCurrentRow;
    }

    //We only show the possible next nodes for the player to move onto
    void GeneratePhysicalMap(RogueNode currentNode, NodeData nodeData)
	{
        CreateRowOfNodes(currentNode, nodeData);

        GenerateNewNodeLines();
    }

    void CreateRowOfNodes(RogueNode currentNode, NodeData nodeData)
	{
        int row = currentNode.mapRow;
        int maxRowOfMap = mapList.Max(n => n.row);

        foreach (var node in nodeData.nodeChilds)
            StructureManager.GenerateRogueTile(node.row, node.positionInRow, maxRowOfMap, mapList.IndexOf(node), node.seed, tile.transform, currentNode.transform, this);
    }

    int FindOffsetPositionOnRow(int nodesOnCurrentRow, int currentRow, List<NodeData> previousRowNodes)
    {
        int randomSeed = seedList[SeedType.NodesOnRow] * nodesOnCurrentRow * (currentRow + 1);

        int firstNodeInPreviousRowPosition = previousRowNodes.First().positionInRow;

        int minimumOffset;
        if (nodesOnCurrentRow == previousRowNodes.Count - 1)
            minimumOffset = firstNodeInPreviousRowPosition;
        else
            minimumOffset = firstNodeInPreviousRowPosition - 1 >= 0 ? firstNodeInPreviousRowPosition - 1 : 0;

        int maximumOffset;
        if (nodesOnCurrentRow == previousRowNodes.Count + 1)
            maximumOffset = firstNodeInPreviousRowPosition;
        else
            maximumOffset = firstNodeInPreviousRowPosition + 1 <= 3 ? firstNodeInPreviousRowPosition + 1 : 3;

        int position;
        if (previousRowNodes.Count == MAXIMUM_NODES && nodesOnCurrentRow == MINIMUM_NODES)
            position = firstNodeInPreviousRowPosition + 1;
        else if (previousRowNodes.Count == 2 && nodesOnCurrentRow == 1)
            position = RandomManager.GetRandomValue(randomSeed, firstNodeInPreviousRowPosition, firstNodeInPreviousRowPosition + 1);
        else if (nodesOnCurrentRow == 4)
            position = 0;
        else
            position = RandomManager.GetRandomValue(randomSeed, minimumOffset, maximumOffset + 1);

        return position;
    }

    void SetNodesChilds(List<NodeData> tileParents, List<NodeData> tileChilds)
    {

        foreach (var parent in tileParents)
        {
            int seed = seedList[SeedType.RogueTile] * parent.row * parent.positionInRow;
            List<NodeData> possibleLinks = tileChilds.Where(t =>
            (t.positionInRow == parent.positionInRow - 1) ||
            (t.positionInRow == parent.positionInRow) ||
            (t.positionInRow == parent.positionInRow + 1)).ToList();

            bool isParentTileFirstInRow = parent.positionInRow == tileParents.Min(t => t.positionInRow);
            bool isParentTileLastInRow = parent.positionInRow == tileParents.Max(t => t.positionInRow);

            List<NodeData> linksToAdd = new();

            foreach (var link in possibleLinks)
            {
                NodeData previousNodeInParentRow = tileParents.Find(t => t.positionInRow == parent.positionInRow - 1);
                bool isLastPossibleLinkForChild = parent.positionInRow == link.positionInRow + 1;
                bool isCrossingWithPreviousParent = previousNodeInParentRow.nodeChilds != null && previousNodeInParentRow.nodeChilds.Any(t => t.positionInRow == link.positionInRow + 1); //Check that we do not create a crossing of links (e.g. A1 <-> B2 && A2 <-> B1)
                if (isParentTileFirstInRow && link.positionInRow == possibleLinks.Min(t => t.positionInRow) ||
                        isParentTileLastInRow && link.positionInRow == possibleLinks.Max(t => t.positionInRow))
                    linksToAdd.Add(link);
                else if (!isCrossingWithPreviousParent && RandomManager.GetRandomValue(seed, 0, 2) == 1)
                    linksToAdd.Add(link);
            }

            parent.nodeChilds.AddRange(linksToAdd);
            linksToAdd.ForEach(t => t.nodeParents.Add(parent));
        }
    }

    public void GenerateNewNodeLines()
	{
        StructureManager.GenerateRogueLine(origin);
    }

    public void NodeClicked(RogueNode tile)
	{
		if (IsAnyUnitMoving) return;

        NodeData startingNode = mapList.Find(t => t.row == GeneralManager.CurrentRow && t.positionInRow == GeneralManager.CurrentPositionInRow);
        if (tile.mapRow != startingNode.row)
		{
            ObjectMoving = RogueObjectMoving.PlayerCharacter;
            GeneralManager.selectedNode = tile;
            GeneralManager.CurrentRow = tile.mapRow;
            GeneralManager.CurrentPositionInRow = tile.positionInRow;
            StructureManager.MoveUnit(playerUnitTransform, tile);
        }
    }

    //Method called by the red "Abandon Run" button on the Rogue section UI
    public void AbandonRunClick()
	{
        if (!GeneralManager.IsGameInStandby)
            EndRun(1);
	}
    public void UnitsCheckClick()
    {
    }

    public void EventChoiceClick(int choice)
	{
        GeneralManager gm = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();

        EventOption eventOption = gm.selectedNode.currentEvent.Options[choice];

        foreach (var func in eventOption.OptionFunction)
        {
            EventEntity entity = new()
            {
                runData = gm.runData,
                objToAdd = func.objToAdd,
                objToAddEnum = func.objToAddEnum
            };

            func.funcToCall(ref entity);
            gm.runData = entity.runData;
        }

        gm.ReturnToRogue(RogueTileType.Event, false);
    }

    public static void MerchantBuyClick(int objectBoughtIndex)
	{
		RogueManager rm = GameObject.Find(GeneralManager.ROGUE_MANAGER_OBJ_NAME).GetComponent<RogueManager>();
		if (rm.MerchantShop.BuyItem(objectBoughtIndex, rm.GeneralManager.Gold, out int newGoldAmount))
		{
            rm.GeneralManager.Gold = newGoldAmount;
            rm.StructureManager.ClearMerchantItem(objectBoughtIndex, rm.GeneralManager.Gold);
        }
	}

    public void MerchantCloseClick()
	{
        GeneralManager fm = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
        fm.ReturnToRogue(RogueTileType.Merchant, false);
    }

    public void EndRun(int runType)
	{
        RunEndType runEndType = (RunEndType)runType;
		switch (runEndType)
		{
            //Player temporarily closed the run
			case RunEndType.Close:
                GeneralManager.CloseRun();
                ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
                break;
            //Player abandoned the run
			case RunEndType.Abandon:
                GeneralManager.CloseRun();
                ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
                break;
            //Player completed a run
			case RunEndType.Finish:
                GeneralManager.CloseRun();
                ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
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
        EnemyAI,
    }

    public enum RogueObjectMoving
    {
        Camera,
        PlayerCharacter,
        Nothing
    }

    public struct NodeData
    {
        public RogueTileType tileType;
        public int row;
        public int positionInRow;
        public int seed;
        public List<NodeData> nodeChilds;
        public List<NodeData> nodeParents;

        public NodeData(RogueTileType type, int row, int positionInRow, int seed, List<NodeData> nodeChilds, List<NodeData> nodeParents)
        {
            tileType = type;
            this.row = row;
            this.positionInRow = positionInRow;
            this.seed = seed;
            this.nodeChilds = nodeChilds;
            this.nodeParents = nodeParents;
        }
    }
}