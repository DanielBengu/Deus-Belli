using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public UIManager uiManager;
    public SpriteManager spriteManager;
    public ActionPerformer actionPerformer;

    Pathfinding pathfinding;

    public GameData gameData;

    public bool IsObjectMoving { get{ return actionPerformer.movement.IsObjectMoving;}}

	private void Start()
	{
        actionPerformer = new() { structureManager = this, movement = new(), spriteManager = spriteManager };
    }

	public Dictionary<int, Tile> SetupFightSection(Dictionary<int, GameObject> tileList, FightManager manager, int topX, int y, int topZ, int X_Length, int Y_Length)
	{
        var mapTiles = GenerateFightTiles(tileList, manager, topX, y, topZ, X_Length, Y_Length);
        pathfinding = new(mapTiles, X_Length, Y_Length);
        actionPerformer.pathfinding = pathfinding;
        return mapTiles;
    }

    public void SetEndTurnButton(bool active){
        uiManager.SetEndTurnButton(active);
    }


    public Dictionary<int, Tile> GenerateFightTiles(Dictionary<int, GameObject> tileList, FightManager manager, int topX, int y, int topZ, int XLength, int YLength)
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

                    float angle = 90f;
                    Vector3 rotation = new(angle, 0f, 0f);
                    tile.transform.rotation = Quaternion.Euler(rotation);
                    mapTiles.Add(tileScript.tileNumber, tileScript);
                #endregion
            }
        }
        Debug.Log("END TILE GENERATION");
        return mapTiles;
    }

    public RogueNode GenerateRogueTile(int randomLength, int currentRow, int positionOnRow, Transform origin, Transform firstNode, RogueManager rm)
	{
        Vector3 tilePosition = origin.position;
        float precedentRowX = firstNode.transform.position.x + (450 * (currentRow - 1));
        tilePosition.x = precedentRowX + 150 + (50 * randomLength); //sourceTile.transform.position.x + 150 + (50 * randomLength);
        tilePosition.z = firstNode.transform.position.z + 200 - (200 * positionOnRow);

        GameObject newTile = Instantiate(origin.gameObject, tilePosition, origin.rotation, firstNode);
        newTile.transform.localScale = new Vector3(1, 1, 1);
        RogueNode newTileScript = newTile.GetComponent<RogueNode>();
        newTileScript.SetupTile(rm, RogueTileType.Fight, currentRow, positionOnRow);

        return newTileScript;
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

    public void SetInfoPanel(bool active, Unit unit = null){
        uiManager.SetInfoPanel(active, unit);
    }

    public void SelectTiles(List<Tile> tilelist, bool clearBeforeSelecting, TileType tileType = TileType.Default)
    {
        if(clearBeforeSelecting)
            ClearSelectedTiles();

        spriteManager.GenerateTileSelection(tilelist, tileType);
    }

    public void ClearSelection(bool closeInfoPanel){
        if(closeInfoPanel)
            uiManager.SetInfoPanel(false);
        ClearSelectedTiles();
    }

    public void GetFightVictoryScreen(int gold)
	{
        uiManager.GetFightVictoryScreen(gold);
	}

    public void GetRogueVictoryScreen()
    {
        uiManager.GetRogueVictoryScreen();
    }

    public List<Tile> FindPathToDestination(Tile targetTile, bool selectTiles){
        List<Tile> path = pathfinding.FindPathToDestination(targetTile);
        if(selectTiles)
            spriteManager.GenerateTileSelection(path);
        return path;
    }

    public List<Tile> GeneratePossibleMovementForUnit(Unit unit, bool selectTiles){
        ClearSelectedTiles();

        List<Tile> possibleMovements = CalculateMapTilesDistance(unit);

        if (selectTiles)
            SelectTiles(possibleMovements, false);

        return possibleMovements;
    }

    #region Method Forwarding

    public List<Tile> CalculateMapTilesDistance(Unit startingUnit)
    {
        return pathfinding.CalculateMapTilesDistance(startingUnit);
    }

    public bool MovementTick()
    {
        return actionPerformer.movement.MovementTick();
    }

    public void MoveUnit(Unit unit, Tile targetTile)
    {
        actionPerformer.StartAction(ActionPerformed.FightMovement, unit.gameObject, targetTile.gameObject);
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

    public List<Tile> GetPossibleAttacksForUnit(Unit unit, bool selectTiles)
	{
        List<Tile> possibleAttacks = pathfinding.FindPossibleAttacks(unit);

        if (selectTiles)
            SelectTiles(possibleAttacks, false);

        return possibleAttacks;
    }

    #endregion

    #region Private functions

    void ClearSelectedTiles(){
            spriteManager.ClearMapTilesSprite();
    }
    #endregion
}

public struct GameData
{
    public Dictionary<int, Tile> mapTiles;
    public List<Unit> unitsOnField;

    public int Map_X_Length;
    public int Map_Y_Length;

    public GameData(Dictionary<int, Tile> mapTiles, List<Unit> unitsOnField, int Map_X_Length, int Map_Y_Length)
    {
        this.mapTiles = mapTiles;
        this.unitsOnField = unitsOnField;
        this.Map_X_Length = Map_X_Length;
        this.Map_Y_Length = Map_Y_Length;
    }
}