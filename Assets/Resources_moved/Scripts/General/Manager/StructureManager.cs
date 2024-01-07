using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Pathfinding;

public class StructureManager : MonoBehaviour
{
    const int FIGHT_TILES_DISTANCE = 100;
	const int ROGUE_TILES_DISTANCE = 5;
    const int ROGUE_TILES_RANDOM_DIFFERENCE_MIN = 3;
	const int ROGUE_TILES_RANDOM_DIFFERENCE_MAX = 6;
    const int ROGUE_TILES_FIGHT_WEIGHT = 6;
	const int ROGUE_TILES_MERCHANT_WEIGHT = 1;
	const int ROGUE_TILES_EVENT_WEIGHT = 2;

	public UIManager uiManager;
    public SpriteManager spriteManager;
    public ActionPerformer actionPerformer;

    Pathfinding pathfinding;

    public FightGameData gameData;

    public bool IsObjectMoving { get{ return actionPerformer.movement.IsObjectMoving;}}

    public List<Tuple<GameObject, Animation>> ObjectsAnimating { get; set; } = new();

	private void Start()
	{
        actionPerformer = new(this, spriteManager);
    }

	public Dictionary<int, Tile> SetupFightSection(Level level, FightManager manager)
	{
        var mapTiles = GenerateFightTiles(level.tilesDict, manager, level.spawnPosition, level.mapData.Rows, level.mapData.Columns);
        pathfinding = new(mapTiles, level.mapData.Rows, level.mapData.Columns);
        actionPerformer.pathfinding = pathfinding;
        return mapTiles;
    }

    public Dictionary<int, Tile> GenerateFightTiles(Dictionary<int, GameObject> tileList, FightManager manager, Vector3 spawnPosition, int rows, int columns)
    {
        Dictionary<int, Tile> mapTiles = new();
        Debug.Log("START TILE GENERATION");
        for(int i=0;i< columns;i++)
        {
            for(int x=0; x< rows; x++){
                Tile tile = SetupFightTile(spawnPosition, rows, columns, x, i, manager, tileList);
                mapTiles.Add(tile.data.PositionOnGrid, tile);
            }
        }
        Debug.Log("END TILE GENERATION");
        return mapTiles;
    }

    Tile SetupFightTile(Vector3 baseSpawnPosition, int rows, int columns, int positionInRow, int positionInColumn, FightManager manager, Dictionary<int, GameObject> tileList)
    {
		int index = positionInRow + (positionInColumn * columns);
		GameObject tile = tileList[index];

		try
        {
			tile.transform.position = GetFightTileSpawnPosition(baseSpawnPosition, positionInRow, positionInColumn);

			Tile tileScript = tile.GetComponent<Tile>();
			tileScript.SetupManager(manager);
			tileScript.data.PositionOnGrid = positionInRow + (positionInColumn * rows);
			return tileScript;
		}
        catch (Exception)
        {
            Debug.LogError("Missing \"Tile\" script on Prefab " + tile.name);
            return null;
        }
	}

    Vector3 GetFightTileSpawnPosition(Vector3 baseSpawnPosition, int positionInRow, int positionInColumn)
    {
		float spawnPositionX = baseSpawnPosition.x + (positionInRow * FIGHT_TILES_DISTANCE);
		float spawnPositionY = baseSpawnPosition.y;
		float spawnPositionZ = baseSpawnPosition.z - (positionInColumn * FIGHT_TILES_DISTANCE);
        return new(spawnPositionX, spawnPositionY, spawnPositionZ);
	}

	Vector3 GetRogueTileSpawnPosition(Vector3 baseSpawnPosition, int currentRow, int positionInRow, int nodeSeed)
	{
		int randomLength = RandomManager.GetRandomValue(nodeSeed, ROGUE_TILES_RANDOM_DIFFERENCE_MIN, ROGUE_TILES_RANDOM_DIFFERENCE_MAX);
		float precedentRowX = baseSpawnPosition.x + (15 * (currentRow - 1));

		float spawnPositionX = precedentRowX + randomLength + ROGUE_TILES_DISTANCE;
		float spawnPositionY = baseSpawnPosition.y;
		float spawnPositionZ = baseSpawnPosition.z - (ROGUE_TILES_DISTANCE * (positionInRow - 1));
		return new(spawnPositionX, spawnPositionY, spawnPositionZ);
	}

	public RogueNode GenerateRogueTile(int currentRow, int positionOnRow, int maxRowOnMap, int nodeIndex, int nodeSeed, Transform origin, Transform firstNode, RogueManager rm)
	{
		Vector3 tileSpawnPosition = GetRogueTileSpawnPosition(firstNode.transform.position, currentRow, positionOnRow, nodeSeed);
        GameObject newTile = Instantiate(origin.gameObject, tileSpawnPosition, Quaternion.identity, firstNode);
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

        Dictionary<RogueTileType, int> weights = new()
        {
            { RogueTileType.Fight, ROGUE_TILES_FIGHT_WEIGHT },
			{ RogueTileType.Event, ROGUE_TILES_EVENT_WEIGHT },
			{ RogueTileType.Merchant, ROGUE_TILES_MERCHANT_WEIGHT },
		};
        return RandomManager.GetRandomValueWithWeights(seed, weights);
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
        SelectTiles(gameData.GetTileList(), true, TileType.Default);
    }

    public List<Tile> FindPathToDestination(Tile targetTile, bool selectTiles, int startingTileNumber){
		List<Tile> path = pathfinding.FindPathToDestination(targetTile, out _, startingTileNumber);
        if (selectTiles)
            SelectTiles(path, false);
        return path;
    }

    public void InstantiateShowcaseUnit(Unit unit, Transform positionOfShowcase, Transform parent)
    {
		var unitShowcase = Instantiate(unit, parent);
        if (unitShowcase.TryGetComponent<BoxCollider>(out var component))
            Destroy(component);
        Destroy(unitShowcase);
        unitShowcase.transform.SetPositionAndRotation(positionOfShowcase.position, positionOfShowcase.rotation);
    }

    public void StartShowcaseAnimation(GameObject unit, Animation animation, bool countsTowardsObjectsAnimating)
    {
        actionPerformer.PerformAnimation(unit, animation, countsTowardsObjectsAnimating);
	}

    public void ClearShowcase(Transform parent)
    {
        //Unity doesnt automatically update childCount after the destroy method
		foreach (Transform child in parent)
		{
			Destroy(child.gameObject);
		}
	}

    public List<Tile> GeneratePossibleMovementForUnit(Unit unit, bool selectTiles){
        List<Tile> possibleMovements = CalculateMapTilesDistance(unit);
        List<Tile> tilesWithAllies = possibleMovements.FindAll(t => t.unitOnTile && t.unitOnTile.UnitData.Faction == unit.UnitData.Faction && t.unitOnTile != unit);
        possibleMovements.RemoveAll(t => tilesWithAllies.Contains(t));

        if (selectTiles)
        {
            TileType tileType = unit.UnitData.Faction == FightManager.USER_FACTION ? TileType.PossibleAlly : TileType.PossibleEnemy;
			SelectTiles(possibleMovements, false, tileType);
            SelectTiles(tilesWithAllies, false, TileType.Ally);
            SelectTiles(unit.Movement.CurrentTile, false, TileType.Selected);
		}

        return possibleMovements;
    }

	public void KillUnit(Unit unitToKill)
	{
		gameData.unitsOnField.Remove(unitToKill);
		Destroy(unitToKill.gameObject);
	}

    public void FinishAnimation(GameObject target)
    {
        actionPerformer.FinishAnimation(target);
    }

	#region Method Forwarding
	public void UpdateEndPhaseButtonText()
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
    public void MovementTick(float speed, Action callback)
    {
        actionPerformer.movement.MovementTick(speed, callback);
    }
    public void MoveUnit(Unit unit, Tile targetTile, bool isTeleport)
    {
        ActionPerformed action = isTeleport ? ActionPerformed.FightTeleport : ActionPerformed.FightMovement;
        actionPerformer.StartAction(action, unit.gameObject, targetTile.gameObject);
    }
    public void MoveUnit(Transform unit, RogueNode targetTile)
    {
        actionPerformer.StartAction(ActionPerformed.RogueMovement, unit.gameObject, targetTile.gameObject);
    }
    public bool IsAttackPossible(Unit attacker, Unit defender)
	{
        if (!attacker || !defender)
            return false;

        bool noAttacksInRange = !attacker.Movement.GetPossibleAttacks().Any(a => a.tileToAttack.data.PositionOnGrid == defender.Movement.CurrentTile.data.PositionOnGrid);

		if(noAttacksInRange)
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
    public List<PossibleAttack> GetPossibleAttacksForUnit(Unit unit, bool selectTiles, List<Tile> possibleMovements = null)
	{
        List<PossibleAttack> possible = pathfinding.FindPossibleAttacks(unit, possibleMovements);

		if (selectTiles)
            SelectTiles(possible.Select(a => a.tileToAttack).ToList(), false, TileType.Enemy);

        return possible;
    }

    public Tile CheapestTileToMoveTo(List<PossibleAttack> possibleAttacks, Unit attacker, Unit defender)
    {
        Tile lowestTile = possibleAttacks.First().tileToMoveTo;
        float lowestCost = OUT_OF_BOUND_VALUE;
		foreach (PossibleAttack possibleAttack in possibleAttacks.Where(a => a.tileToAttack.data.PositionOnGrid == defender.Movement.CurrentTile.data.PositionOnGrid))
        {
            Tile tile = pathfinding.FindPathToDestination(possibleAttack.tileToMoveTo, out float cost, attacker.Movement.CurrentTile.data.PositionOnGrid).Last();
            //If true, we found a cheaper road
            if(cost < lowestCost)
            {
                lowestTile = tile;
                lowestCost = cost;
			}
        }

        return lowestTile;
    }
    public void SetInfoPanel(bool active, Unit unit = null)
    {
        uiManager.SetInfoPanel(active, unit);
    }
	public void ClearTooltips()
	{
		uiManager.ClearTooltip();
	}
	public void SelectTiles(List<Tile> tilelist, bool clearBeforeSelecting, TileType tileType = TileType.Base)
    {
        if (clearBeforeSelecting)
			spriteManager.GenerateTileSelection(gameData.GetTileList(), gameData.IsSetup, TileType.Default);

		spriteManager.GenerateTileSelection(tilelist, gameData.IsSetup, tileType);
    }

	public void SelectTiles(Tile tile, bool clearBeforeSelecting, TileType tileType = TileType.Base)
	{
		if (clearBeforeSelecting)
			spriteManager.GenerateTileSelection(gameData.GetTileList(), gameData.IsSetup, TileType.Default);

		spriteManager.GenerateTileSelection(tile, gameData.IsSetup, tileType);
	}

	#endregion
}

public struct FightGameData
{
    public Dictionary<int, Tile> mapTiles;
    public List<Unit> unitsOnField;

    public int Map_Rows;
    public int Map_Columns;

	readonly float Map_MaxLeft_Position;
	readonly float Map_MaxRight_Position;
	readonly float Map_MaxUpward_Position;
	readonly float Map_MaxDownward_Position;

    public bool IsSetup { get; set; }

    public FightGameData(Dictionary<int, Tile> mapTiles, List<Unit> unitsOnField, int Map_Rows, int Map_Columns)
    {
		this.mapTiles = mapTiles;
        this.unitsOnField = unitsOnField;
        this.Map_Rows = Map_Rows;
        this.Map_Columns = Map_Columns;

		int maxTile = mapTiles.Max(t => t.Key);
		Tile[] tiles =  new[] { mapTiles.First(t => t.Key == 0).Value, mapTiles.First(t => t.Key == maxTile).Value };

		Vector3 firstTile = tiles[0].transform.position;
		Vector3 lastTile = tiles[1].transform.position;

		Map_MaxLeft_Position = firstTile.x + 200;
		Map_MaxRight_Position = lastTile.x - 200;
		Map_MaxUpward_Position = firstTile.z - 400;
		Map_MaxDownward_Position = lastTile.z;

        IsSetup = true;
	}

    public void SetIsSetup(bool isSetup)
    {
        IsSetup = isSetup;
    }

    public readonly List<Tile> GetTileList()
    {
        return mapTiles.Values.ToList();
    }

    public readonly float[] GetMapBounds()
    {
        return new float[] { Map_MaxLeft_Position, Map_MaxRight_Position, Map_MaxDownward_Position, Map_MaxUpward_Position };
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