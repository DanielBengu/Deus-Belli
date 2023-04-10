using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public List<Tile> selectedTiles = new();
    public List<Tile> possibleAttacks = new();

    UIManager uiManager;
    SpriteManager spriteManager;
    public ActionPerformer actionPerformer;

    Pathfinding pathfinding;
    readonly Movement movement = new();

    public GameData gameData;

    public bool IsObjectMoving { get{ return movement.isObjectMoving;}}

    public Dictionary<int, Tile> Setup(Dictionary<int, GameObject> tileList, int topX, int y, int topZ, int X_Length, int Y_Length){

        var mapTiles = GenerateTiles(tileList, topX, y, topZ, X_Length, Y_Length);

        pathfinding = new(mapTiles, X_Length, Y_Length);
        uiManager = GetComponent<UIManager>();
        spriteManager = GetComponent<SpriteManager>();
        actionPerformer = new() { structureManager = this, pathfinding = pathfinding, movement = movement, spriteManager = spriteManager};
        return mapTiles;
    }

    public void SetEndTurnButton(bool active){
        uiManager.endTurnButton.SetActive(active);
    }


    public Dictionary<int, Tile> GenerateTiles(Dictionary<int, GameObject> tileList, int topX, int y, int topZ, int XLength, int YLength)
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

    public void SetInfoPanel(bool active, Unit unit = null){
        uiManager.SetInfoPanel(active, unit);
    }

    public void SelectTiles(List<Tile> tilelist, bool clearBeforeSelecting, bool AddToSelected, TileType tileType = TileType.Default)
    {
        if(clearBeforeSelecting)
            ClearSelectedTiles();

        if(AddToSelected)
            foreach (var tile in tilelist)
                selectedTiles.Add(tile);

        spriteManager.GenerateTileSelection(tilelist, tileType);
    }

    public void ClearSelection(bool closeInfoPanel){
        if(closeInfoPanel)
            uiManager.SetInfoPanel(false);
        ClearSelectedTiles();
    }

    public List<Tile> FindPathToDestination(Tile targetTile, bool selectTiles, bool addToSelectedMapTiles){
        List<Tile> path = pathfinding.FindPathToDestination(targetTile);
        if(selectTiles)
            spriteManager.GenerateTileSelection(path);
        if(addToSelectedMapTiles)
            selectedTiles = path;
        return path;
    }

    public List<Tile> GeneratePossibleMovementForUnit(Unit unit, bool selectTiles){
        ClearSelectedTiles();

        CalculateMapTilesDistance(unit);

        //We remove the starting tile for the unit and the tiles that costs too much movement for it
        List<Tile> tilesList = gameData.mapTiles.Select(t => t.Value).Where(t => t.tileNumber != unit.CurrentTile.tileNumber && t.tentativeCost <= unit.movementCurrent).ToList();
        selectedTiles = gameData.mapTiles.Select(t => t.Value).Where(t => t.tentativeCost <= unit.movementCurrent).ToList();

        
        if(selectTiles)
            spriteManager.GenerateTileSelection(selectedTiles);

        return tilesList;
    }

    #region Method Forwarding

        public void CalculateMapTilesDistance(Unit startingUnit)
        {
            pathfinding.CalculateMapTilesDistance(startingUnit);
        }

        public List<Tile> FindPossibleAttacks(Unit unit, List<Tile> possibleMovements)
        {
            possibleAttacks = pathfinding.FindPossibleAttacks(unit, possibleMovements);
            SelectTiles(possibleAttacks, false, true);
            return possibleAttacks;
        }

        public bool MovementTick()
        {
            return movement.MovementTick();
        }

        public void MoveUnit(Unit unit, Tile targetTile)
        {
            actionPerformer.StartAction(ActionPerformed.Movement, unit, targetTile);
        }

    #endregion

    #region Private functions

        void ClearSelectedTiles(){
            spriteManager.ClearMapTilesSprite();
            selectedTiles.Clear();
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