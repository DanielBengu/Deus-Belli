using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StructureManager : MonoBehaviour
{
    public UIManager uiManager;
    public SpriteManager spriteManager;
    public ActionPerformer actionPerformer;

    Pathfinding pathfinding;

    public FightGameData gameData;

    public bool IsObjectMoving { get{ return actionPerformer.movement.IsObjectMoving;}}

	private void Start()
	{
        actionPerformer = new() { structureManager = this, movement = new(), spriteManager = spriteManager };
    }

	public Dictionary<int, Tile> SetupFightSection(Dictionary<int, GameObject> tileList, FightManager manager, int topX, int y, int topZ, int X_Length, int Y_Length)
	{
        Transform objectsParent = GameObject.Find("Fight Objects").transform;
        var mapTiles = GenerateFightTiles(tileList, manager, objectsParent, topX, y, topZ, X_Length, Y_Length);
        pathfinding = new(mapTiles, X_Length, Y_Length);
        actionPerformer.pathfinding = pathfinding;
        return mapTiles;
    }

    public Dictionary<int, Tile> GenerateFightTiles(Dictionary<int, GameObject> tileList, FightManager manager, Transform objectsParent, int topX, int y, int topZ, int XLength, int YLength)
    {
        Dictionary<int, Tile> mapTiles = new();
        Debug.Log("START TILE GENERATION");
        for(int i=0;i< YLength;i++)
        {
            for(int x=0; x< XLength; x++){
                #region Spawn background tile

                    Vector3 spawnPosition = new(topX + (x * 100), y, topZ - (i * 100));
                    GameObject tile = tileList[x + (i * YLength)];
                    tile.transform.position = spawnPosition;
                    Tile tileScript = tile.GetComponent<Tile>();

                    tileScript.SetupManager(manager);
                    tileScript.tileNumber = x + (i * XLength);
                    mapTiles.Add(tileScript.tileNumber, tileScript);
                #endregion
            }
        }
        Debug.Log("END TILE GENERATION");
        return mapTiles;
    }

    public RogueNode GenerateRogueTile(int currentRow, int positionOnRow, int maxRowOnMap, int nodeIndex, int nodeSeed, Transform origin, Transform firstNode, RogueManager rm)
	{
        int randomLength = RandomManager.GetRandomValue(nodeSeed, 3, 6);
        Vector3 tilePosition = origin.localPosition;
        float precedentRowX = firstNode.transform.position.x + (15 * (currentRow - 1));
        tilePosition.x = precedentRowX + 5 + (randomLength); //sourceTile.transform.position.x + 150 + (50 * randomLength);
        tilePosition.y = firstNode.transform.position.y;
        tilePosition.z = firstNode.transform.position.z + 5 - (5 * positionOnRow);

        GameObject newTile = Instantiate(origin.gameObject, tilePosition, Quaternion.identity, firstNode);
        RogueNode newTileScript = newTile.GetComponent<RogueNode>();
        RogueTileType typeOfNode = GenerateRogueNodeType(currentRow, maxRowOnMap, rm, nodeIndex);
        newTileScript.SetupTile(rm, typeOfNode, currentRow, positionOnRow, nodeIndex, nodeSeed);

        return newTileScript;
    }

    public RogueTileType GenerateRogueNodeType(int currentRow, int maxRowOnMap, RogueManager rm, int nodeIndex)
	{
		if (currentRow == maxRowOnMap)
            return RogueTileType.Boss;

        int seed = rm.seedList[RogueManager.SeedType.RogueTile];
        int minibossRow = RandomManager.GetRandomValue(seed, 3, 5);
        if (minibossRow == currentRow)
            return RogueTileType.Miniboss;
        if (currentRow == 1)
            return RogueTileType.Fight;

        seed = rm.seedList[RogueManager.SeedType.RogueTile] * (nodeIndex + 1);

        int weightOfFightEncounter = 6;
        int weightOffEventEncounter = 2;
        int weightOffMerchantEncounter = 1;
        int offset = 0;
        RogueTileType[] weights = new RogueTileType[weightOfFightEncounter + weightOffEventEncounter + weightOffMerchantEncounter];

        for (int i = 0; i < weightOfFightEncounter; i++)
            weights[offset + i] = RogueTileType.Fight;
        offset += weightOfFightEncounter;
        for (int i = 0; i < weightOffEventEncounter; i++)
            weights[offset + i] = RogueTileType.Event;
        offset += weightOffEventEncounter;
        for (int i = 0; i < weightOffMerchantEncounter; i++)
            weights[offset + i] = RogueTileType.Merchant;

        int weightIndex = RandomManager.GetRandomValue(seed, 0, weights.Length);
        RogueTileType rogueTileType = weights[weightIndex];

        return rogueTileType;
    }

    public void GenerateRogueLine(List<RogueNode> tileList)
	{
        LineRenderer lr;
		foreach (var tile in tileList)
		{
            lr = tile.GetComponent<LineRenderer>();
            lr.positionCount = tile.rogueChilds.Count * 2;

            for (int i = 0; i < tile.rogueChilds.Count; i++)
			{
                lr.SetPosition(i * 2, tile.transform.position);
                lr.SetPosition((i * 2) + 1, tile.rogueChilds[i].transform.position);
            }
		}
    }

    public void ClearSelection(bool closeInfoPanel){
        if(closeInfoPanel)
            uiManager.SetInfoPanel(false);
    }

    public List<Tile> FindPathToDestination(Tile targetTile, bool selectTiles){
        List<Tile> path = pathfinding.FindPathToDestination(targetTile);
        if(selectTiles)
            spriteManager.GenerateTileSelection(path);
        return path;
    }

    public List<Tile> GeneratePossibleMovementForUnit(Unit unit, bool selectTiles){
        List<Tile> possibleMovements = CalculateMapTilesDistance(unit);

        if (selectTiles)
            SelectTiles(possibleMovements, false);

        return possibleMovements;
    }

    #region Method Forwarding
    public void UpdateFightEndPhaseButton()
	{
        uiManager.SetFightEndPhaseButton();
    }
    public void SetEndTurnButton(bool active)
    {
        uiManager.SetEndTurnButton(active);
    }
    public List<Tile> CalculateMapTilesDistance(Unit startingUnit)
    {
        return pathfinding.CalculateMapTilesDistance(startingUnit);
    }
    // -1 if nothing was queued for movement, 0 if object is still moving, 1 if object is done moving
    public int MovementTick()
    {
        return actionPerformer.movement.MovementTick();
    }
    public void MoveUnit(Unit unit, Tile targetTile, bool isTeleport)
    {
        if (unit.CurrentTile.tileNumber == targetTile.tileNumber)
            return;
        ActionPerformed action = isTeleport ? ActionPerformed.FightTeleport : ActionPerformed.FightMovement;
        actionPerformer.StartAction(action, unit.gameObject, targetTile.gameObject);
    }
    public void MoveUnit(Transform unit, RogueNode targetTile)
    {
        actionPerformer.StartAction(ActionPerformed.RogueMovement, unit.gameObject, targetTile.gameObject);
    }
    public bool IsAttackPossible(Unit attacker, Unit defender)
	{
        //Unit out of movement range
        if (attacker && !attacker.GetPossibleAttacks().Find(t => t.tileNumber == defender.CurrentTile.tileNumber))
            return false;


        return true;
	}
    public void InstantiateMerchantItems(MerchantItem[] itemList)
	{
        GameObject itemPrefab = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, "MerchantItem");

        for (int i = 0; i < itemList.Length; i++)
        {
            int index = i;
            GameObject item = Instantiate(itemPrefab, GameObject.Find("Objects").transform);
            item.transform.position = new(item.transform.position.x + (150 * i), item.transform.position.y);
            item.name = $"{i}_Item";
            foreach (var child in item.transform.GetComponentsInChildren<Transform>(true)) {
                string childName = child.name.Split('_')[1];
                child.name = $"{i}_{childName}";
                if (childName.Equals("Item"))
                    child.GetComponent<Button>().onClick.AddListener(() => RogueManager.MerchantBuyClick(index));
            }
            uiManager.SetMerchantItemVariables(itemList[i], i);
        }  
	}
    public void ClearMerchantItem(int itemIndex, int newPlayerGoldAmount)
    {
        uiManager.ItemBought(itemIndex, newPlayerGoldAmount);
    }
    public void GetGameScreen(GameScreens screen, int gold)
    {
        uiManager.GetScreen(screen, gold);
    }
    public List<Tile> GetPossibleAttacksForUnit(Unit unit, bool selectTiles)
	{
        List<Tile> possibleAttacks = pathfinding.FindPossibleAttacks(unit);

        if (selectTiles)
            SelectTiles(possibleAttacks, false);

        return possibleAttacks;
    }
    public void SetInfoPanel(bool active, Unit unit = null)
    {
        uiManager.SetInfoPanel(active, unit);
    }
    public void SelectTiles(List<Tile> tilelist, bool clearBeforeSelecting, TileType tileType = TileType.Default)
    {
        spriteManager.GenerateTileSelection(tilelist, tileType);
    }

    #endregion
}

public struct FightGameData
{
    public Dictionary<int, Tile> mapTiles;
    public List<Unit> unitsOnField;

    public int Map_X_Length;
    public int Map_Y_Length;

    public FightGameData(Dictionary<int, Tile> mapTiles, List<Unit> unitsOnField, int Map_X_Length, int Map_Y_Length)
    {
        this.mapTiles = mapTiles;
        this.unitsOnField = unitsOnField;
        this.Map_X_Length = Map_X_Length;
        this.Map_Y_Length = Map_Y_Length;
    }
}

public enum GameScreens
{
    Default,
    FightVictoryScreen,
    FightDefeatScreen,
    RogueVictoryScreen,
    RogueDefeatScreen
}