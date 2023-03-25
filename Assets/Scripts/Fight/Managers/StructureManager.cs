using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructureManager : MonoBehaviour
{
    public List<Tile> tiles = new();
    Dictionary<int, Tile> mapTiles = new Dictionary<int, Tile>();
    public int X_Length;
    public int Y_Length;

    Pathfinding pathfinding;

    public void Setup(Dictionary<int, GameObject> tileList, int topX, int y, int topZ, int X_Length, int Y_Length){

        this.X_Length = X_Length;
        this.Y_Length = Y_Length;

        GenerateTiles(tileList, topX, y, topZ, X_Length, Y_Length);

        pathfinding = new(mapTiles, X_Length, Y_Length);
    }

    #region Movement variables
        private float startTime; // the time when the movement started
        private float journeyLength; // the total distance between the start and end markers
        public bool isObjectMoving = false;
        private Transform objectMovingTransform;

        Vector3 startingPosition;
        Quaternion startingRotation;

        Vector3 targetPosition;
        Quaternion targetRotation;

        [SerializeField]
        float speed;

        //List of steps necessary to go from point A to B
        public List<Transform> movementSteps = new();
    #endregion

    #region Info Panel
        [SerializeField] 
        Image unitImage;
        [SerializeField]
        TextMeshProUGUI nameText;
        [SerializeField]
        TextMeshProUGUI hpValue;
        [SerializeField]
        TextMeshProUGUI movementValue;
    #endregion

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

    public void SetInfoPanel(bool active, Unit unit, GameObject infoPanel){
        infoPanel.SetActive(active);
        if(active){
            hpValue.text = $"{unit.hpCurrent}/{unit.hpMax}";
            movementValue.text = $"{unit.movementCurrent}/{unit.movementMax}";
            nameText.text = unit.unitName;
            unitImage.sprite = unit.unitImage;
        }
    }

    public void GenerateTileSelection(Unit unit, List<Tile> tilesToSelect){
        foreach (var tile in tilesToSelect)
        {
            string spriteName = tile.gameObject.GetComponent<SpriteRenderer>().sprite.name.Split(' ')[0];
            if(tile.unitOnTile){
                if(tile.unitOnTile.GetComponent<Unit>().faction == unit.faction)
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

    public bool IsTileWalkableForUnit(Unit unit, Tile tile){
        return (tile.IsPassable && !tile.unitOnTile);
    }

    ///<summary>
    /// GetWalkableTilesForUnit calculates for a given list of tiles which ones are walkable by the unit passed,
    /// and returns the result in a new list.
    ///</summary>
    public List<Tile> GetWalkableTilesForUnit(Unit unit, List<Tile> tiles){
        List<Tile> result = new();
        foreach (var tile in tiles)
        {
            if(IsTileWalkableForUnit(unit, tile))
                result.Add(tile);
        }
        return result;
    }

    public void ChangeObjectSprite(GameObject obj, Sprite sprite){
        SpriteRenderer spriteRendererOld = obj.GetComponent<SpriteRenderer>();
        spriteRendererOld.sprite = sprite;
    }

    public void TileSelected(Tile tile){
        ClearSelectedTiles();
        tiles.Add(tile);

        SpriteRenderer spriteRendererNew = tile.GetComponent<SpriteRenderer>();
        spriteRendererNew.sprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteRendererNew.sprite.name.Split(' ')[0] + " selected"}");
    }
    
    public void ClearSelectedTiles(){
        foreach (var tile in tiles)
        {
            string spriteName = tile.GetComponent<SpriteRenderer>().sprite.name;
            Sprite newSprite = Resources.Load<Sprite>($"Sprites/Terrain/{spriteName.Split(' ')[0] + " base"}");
            ChangeObjectSprite(tile.gameObject, newSprite);
        }

        tiles.Clear();
    }

    public bool StartObjectMovement(Transform starting, Transform target, float objectSpeed){
        //Something else is already moving
        if(isObjectMoving)
            return false;

        isObjectMoving = true;
        objectMovingTransform = starting;
        startTime = Time.time;
        startingPosition = starting.position;
        startingRotation = starting.rotation;
        targetPosition = target.position;
        targetRotation = target.rotation;
        speed = objectSpeed;
        journeyLength = Vector3.Distance(startingPosition, targetPosition);
        Debug.Log("Started movement to " + targetPosition);
        return true;
    }

    public bool MovementTick(){
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;
        objectMovingTransform.position = Vector3.Lerp(startingPosition,targetPosition, fractionOfJourney);
        objectMovingTransform.rotation = Quaternion.Lerp(startingRotation, targetRotation, fractionOfJourney);

        if(objectMovingTransform.position == targetPosition && objectMovingTransform.rotation == targetRotation){
            if(movementSteps.Count > 0){
                isObjectMoving = false;
                Transform destination = new GameObject().transform;
                destination.position = movementSteps.First().position;
                destination.rotation = objectMovingTransform.rotation;

                StartObjectMovement(objectMovingTransform, destination, 800);
                GameObject.Destroy(destination.gameObject);
                movementSteps.RemoveAt(0);
            }else{
                isObjectMoving = false;
                return true;
            }
        }

        return false;
    }

    public void MoveUnit(GameObject unit, Tile targetTile){
        Unit unitScript = unit.GetComponent<Unit>();
        List<Tile> tilesPath = pathfinding.FindPathToDestination(unitScript.currentTile, targetTile);

        foreach (var tile in tilesPath.Skip(1))
        {
            movementSteps.Add(tile.transform);
        }


        unitScript.currentTile.unitOnTile = null;

        Transform destination = new GameObject().transform;
        destination.position = movementSteps.First().position;
        destination.rotation = unitScript.transform.rotation;
        movementSteps.RemoveAt(0);

        targetTile.unitOnTile = unit;
        unitScript.currentTile = targetTile;

        StartObjectMovement(unit.transform, destination, 800);
        GameObject.Destroy(destination.gameObject);
    }

    public void StartUnitMovement(GameObject unitToMove, Tile tileScript){
        MoveUnit(unitToMove, tileScript);
        ClearSelectedTiles();
    }

    public void CalculateMapTilesDistance(Unit startingUnit){
        pathfinding.CalculateMapTilesDistance(startingUnit);
    }

    public List<Tile> GeneratePossibleMovementForUnit(Unit unit, bool selectTiles){
        ClearSelectedTiles();

        CalculateMapTilesDistance(unit);

        List<Tile> tilesList = GetMapTiles().Select(t => t.Value).Where(t => t.tentativeCost <= unit.movementCurrent).ToList();
        tiles = GetMapTiles().Select(t => t.Value).Where(t => t.tentativeCost <= unit.movementCurrent).ToList();

        
        if(selectTiles)
            GenerateTileSelection(unit, tiles);

        return tilesList;
    }

    public Dictionary<int, Tile> GetMapTiles(){
        return mapTiles;
    }
}