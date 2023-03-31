using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public List<Tile> selectedTiles = new();
    readonly Dictionary<int, Tile> mapTiles = new Dictionary<int, Tile>();
    public int X_Length;
    public int Y_Length;

    UIManager uiManager;

    Pathfinding pathfinding;
    readonly Movement movement = new();

    public bool IsObjectMoving { get{ return movement.isObjectMoving;}}

    public void Setup(Dictionary<int, GameObject> tileList, int topX, int y, int topZ, int X_Length, int Y_Length){
        this.X_Length = X_Length;
        this.Y_Length = Y_Length;

        GenerateTiles(tileList, topX, y, topZ, X_Length, Y_Length);

        pathfinding = new(mapTiles, X_Length, Y_Length);
        uiManager = this.GetComponent<UIManager>();
    }

    public void SetEndTurnButton(bool active){
        uiManager.endTurnButton.SetActive(active);
    }


    public void GenerateTiles(Dictionary<int, GameObject> tileList, int topX, int y, int topZ, int XLength, int YLength)
    {
        Debug.Log("START TILE GENERATION");
        for(int i=0;i< YLength;i++)
        {
            for(int x=0; x< XLength; x++){
                #region Spawn background tile

                    Vector3 spawnPosition = new Vector3(topX + (x * 100), y, topZ - (i * 100));
                    GameObject tile = tileList[x + (i * YLength)];
                    tile.transform.position = spawnPosition;
                    Tile tileScript = tile.GetComponent<Tile>();
                    tileScript.tileNumber = x + (i * X_Length);

                    float angle = 90f;
                    Vector3 rotation = new Vector3(angle, 0f, 0f);
                    tile.transform.rotation = Quaternion.Euler(rotation);
                    mapTiles.Add(tileScript.tileNumber, tileScript);
                #endregion
            }
        }
        Debug.Log("END TILE GENERATION");
    }

    public void SetInfoPanel(bool active, Unit unit = null){
        uiManager.SetInfoPanel(active, unit);
    }

    public void ChangeObjectSprite(GameObject obj, Sprite sprite){
        SpriteRenderer spriteRendererOld = obj.GetComponent<SpriteRenderer>();
        spriteRendererOld.sprite = sprite;
    }

    public void TileSelected(Tile tile){
        ClearSelectedTiles();
        selectedTiles.Add(tile);

        SpriteRenderer spriteRendererNew = tile.GetComponent<SpriteRenderer>();
        spriteRendererNew.sprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteRendererNew.sprite.name.Split(' ')[0] + " selected"}");
    }

    public void ClearSelection(bool closeInfoPanel = true){
        if(closeInfoPanel)
            uiManager.SetInfoPanel(false);
        ClearSelectedTiles();
    }

    public bool MovementTick(){
        return movement.MovementTick();
    }

    public void MoveUnit(Unit unit, Tile targetTile, int movementSpeed){
        List<Tile> tilesPath = FindPathToDestination(unit.CurrentTile, targetTile, false, false);
        movement.MoveUnit(unit, targetTile, tilesPath, movementSpeed);
    }

    public List<Tile> FindPathToDestination(Tile startingPoint, Tile targetTile, bool selectTiles, bool addToSelectedMapTiles){
        List<Tile> path = pathfinding.FindPathToDestination(targetTile);
        int factionOfUnit = startingPoint.unitOnTile.GetComponent<Unit>().faction;
        if(selectTiles)
            GenerateTileSelection(path, factionOfUnit);
        if(addToSelectedMapTiles)
            AddToMapSelectedTiles(path);
        return path;
    }

    public void StartUnitMovement(Unit unitToMove, Tile destinationTile, int movementSpeed){
        RemoveMovementFromUnit(unitToMove, destinationTile.tentativeCost);
        MoveUnit(unitToMove, destinationTile, movementSpeed);
        ClearSelectedTiles();
    }

    public void CalculateMapTilesDistance(Unit startingUnit){
        pathfinding.CalculateMapTilesDistance(startingUnit);
    }

    public List<Tile> GeneratePossibleMovementForUnit(Unit unit, bool selectTiles){
        ClearSelectedTiles();

        CalculateMapTilesDistance(unit);

        //We remove the starting tile for the unit and the tiles that costs too much movement for it
        List<Tile> tilesList = GetMapTiles().Select(t => t.Value).Where(t => t.tileNumber != unit.CurrentTile.tileNumber && t.tentativeCost <= unit.movementCurrent).ToList();
        selectedTiles = GetMapTiles().Select(t => t.Value).Where(t => t.tentativeCost <= unit.movementCurrent).ToList();

        
        if(selectTiles)
            GenerateTileSelection(selectedTiles, unit.faction);

        return tilesList;
    }

    public Dictionary<int, Tile> GetMapTiles(){
        return mapTiles;
    }

    #region Private functions

        void ClearSelectedTiles(){
            foreach (var tile in selectedTiles)
            {
                string spriteName = tile.GetComponent<SpriteRenderer>().sprite.name;
                Sprite newSprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteName.Split(' ')[0] + " base"}");
                ChangeObjectSprite(tile.gameObject, newSprite);
            }

            selectedTiles.Clear();
        }

        void GenerateTileSelection(List<Tile> tilesToSelect, int factionOfUnit = -1){
            foreach (var tile in tilesToSelect)
            {
                string spriteName = tile.gameObject.GetComponent<SpriteRenderer>().sprite.name.Split(' ')[0];
                if(tile.unitOnTile){
                    if(tile.unitOnTile.GetComponent<Unit>().faction == factionOfUnit)
                        spriteName += " ally";
                    else
                        spriteName += " enemy";
                }else{
                    if(tile.IsPassable)
                        spriteName += " possible";
                    else
                        spriteName += " base";
                }
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteName}");
                ChangeObjectSprite(tile.gameObject, sprite);
            }
        }

        void AddToMapSelectedTiles(List<Tile> tilesToAdd){
            selectedTiles = tilesToAdd;
        }

        void RemoveMovementFromUnit(Unit unit, float movementToRemove){
            unit.movementCurrent -= movementToRemove;
            if(unit.movementCurrent < 0) unit.movementCurrent = 0;
        }

    #endregion
}