using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FightManager : MonoBehaviour
{
    public const int USER_FACTION = 0;
    public const int ENEMY_FACTION = 1;
    public const int LEFT_MOUSE_BUTTON = 1;
    public const int RIGHT_MOUSE_BUTTON = 1;
    public const int SCROLL_WHEEL_BUTTON = 2;
    private const int UNIT_MOVEMENT_SPEED = 800;

    #region Fields

        [SerializeField]
        CameraManager cameraManager;
        StructureManager structureManager;
        AIManager aiManager;

        [SerializeField]
        GameObject warriorPrefab;

        Level level;

        // This list stores all the units on the battlefield
        public List<Unit> units;
        
        float scrollWheelInput;

        [SerializeField]
        GameObject OptionsPrefab;
        GameObject OptionsObject;
        
        // This property stores the currently selected unit
        public Unit UnitSelected { get; set; }

        public bool IsCameraFocused { get{return cameraManager.GetCameraFocusStatus();} set{cameraManager.SetCameraFocusStatus(value);} }
        public bool IsObjectMoving { get{return structureManager.IsObjectMoving;}}
        public bool IsOptionOpen { get; set; }
        public bool IsScrollButtonDown { get; set; }
        public int CurrentTurn { get; set; }
        public bool IsGameInStandby { get{return IsObjectMoving || IsOptionOpen || CurrentTurn != 0;}}
        public bool IsShowingPath { get; set; }

    #endregion

    #region Start and Update
        
        void Start()
        {
            Debug.Log("START FIGHT MANAGER SETUP");

            structureManager = this.GetComponent<StructureManager>();
            aiManager = this.GetComponent<AIManager>();

            var levelBase = GameObject.Find("Level One").GetComponent<LevelOne>();
            levelBase.StartLevel();
            level = levelBase.level;

            // Setup the terrain based on the level information
            structureManager.Setup(level.tilesDict, level.TopLeftSquarePositionX, level.YPosition, level.TopLeftSquarePositionZ, level.XLength, level.YLength);

            GenerateUnits();
            Debug.Log("END FIGHT MANAGER SETUP");
        }

        void Update()
        {
            ManageMovements();
            ManageInputs();
            ManageConstants();
        }

    #endregion
    
    #region Key Input Management

        void ManageMovements(){
            if(IsObjectMoving)
                // If an object is being moved, check if it has finished moving
                if(structureManager.MovementTick())
                    UnitSelected = null;
        }

        void ManageInputs(){
            if(!IsGameInStandby && (Input.anyKeyDown || IsScrollButtonDown))
                ManageKeysDown();
        }

        //Method that manages the press of a key (only for the frame it is clicked)
        void ManageKeysDown(){
            if(Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON)){
                structureManager.ClearSelection();
            } else if (Input.GetKeyDown("escape")) {
                OptionsObject = MainMenu.GeneratePrefab(OptionsPrefab, "Options");
                IsOptionOpen = true;
            } else if(Input.GetMouseButtonDown(SCROLL_WHEEL_BUTTON)){
                IsScrollButtonDown = true;
            } else if(Input.GetMouseButtonUp(SCROLL_WHEEL_BUTTON)){
                IsScrollButtonDown = false;
            }
        }
        //Method that manages constant inputs (like wheel scroll)
        void ManageConstants(){
            scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
            if(scrollWheelInput != 0f){
                cameraManager.ScrollWheel(scrollWheelInput);
            }
            if(IsScrollButtonDown){
                float scrollWheelValue = Input.GetAxisRaw("Mouse ScrollWheel");
                cameraManager.UpdatePosition();
            }
        }
    #endregion

    void GenerateUnits(){
        
        Tile tileWarrior = GameObject.Find($"Terrain_1").GetComponent<Tile>();
        GenerateSingleUnit(warriorPrefab, tileWarrior);

        foreach (var enemy in level.enemyList)
        {
            Tile tile = structureManager.GetMapTiles()[enemy.Key];
            GenerateSingleUnit(enemy.Value, tile);
        }
    }

    void GenerateSingleUnit(GameObject unit, Tile tile){
        Quaternion rotation = new Quaternion(0, 180, 0, 0);
        var unitGenerated = GameObject.Instantiate(unit,tile.transform.position,rotation) as GameObject;
        var unitScript = unitGenerated.GetComponent<Unit>();

        unitScript.currentTile = tile;
        tile.unitOnTile = unitGenerated;
        units.Add(unitScript);
    }

    void StartUserTurn(){
        CurrentTurn = USER_FACTION;
        structureManager.SetEndTurnButton(true);
        foreach(var unit in units.Where(u => u.faction == USER_FACTION)){
            unit.movementCurrent = unit.movementMax;
        }
    }

    void EndUserTurn(){
        CurrentTurn = ENEMY_FACTION;
        structureManager.SetEndTurnButton(false);
        structureManager.ClearSelection(true);
        aiManager.StartAITurn();
    }

    public void ManageClick(ObjectClickedEnum objectClicked, GameObject reference){
        switch (objectClicked)
        {
            case ObjectClickedEnum.EmptyTile:
                var tileSelected = reference.GetComponent<Tile>();
                if(UnitSelected && UnitSelected.faction == USER_FACTION){
                    //If tile clicked is in range
                    if(structureManager.tiles.Contains(tileSelected)){
                        if(IsShowingPath){
                            structureManager.StartUnitMovement(UnitSelected, reference.GetComponent<Tile>(), UNIT_MOVEMENT_SPEED);
                            structureManager.SetInfoPanel(false, UnitSelected);
                            IsShowingPath = false;
                            return;
                        }else{
                            structureManager.ClearSelection(false);
                            structureManager.FindPathToDestination(UnitSelected.currentTile, tileSelected, true, true);
                            IsShowingPath = true;
                        }
                    }else{
                        //User clicked outside the range
                        structureManager.ClearSelection();
                        structureManager.TileSelected(tileSelected);
                        IsShowingPath = false;
                        UnitSelected = null;
                    }
                }else{
                    IsShowingPath = false;
                    structureManager.TileSelected(tileSelected);
                }   
                structureManager.SetInfoPanel(false, UnitSelected);
            break;
            case ObjectClickedEnum.UnitTile:
                if(UnitSelected.faction == USER_FACTION){
                    structureManager.GeneratePossibleMovementForUnit(UnitSelected, true);
                }else{
                    structureManager.TileSelected(UnitSelected.currentTile);
                }
                IsShowingPath = false;
                structureManager.SetInfoPanel(true, UnitSelected);
            break;
            case ObjectClickedEnum.RightClickOnField:
                structureManager.ClearSelection();
            break;
            case ObjectClickedEnum.Default:
            break;
        }
    }

    public void EndTurn(int faction){
        switch (faction)
        {
            case USER_FACTION:
                EndUserTurn();
                break;
            case ENEMY_FACTION:
                StartUserTurn();
                break;
            case 2:
                break;
            default:
                break;
        }
    }

    public void EndTurnButton(int faction){
        FightManager fm = GameObject.Find("Manager").GetComponent<FightManager>();
        fm.EndTurn(faction);
    }
}

public enum ObjectClickedEnum{
    EmptyTile,
    UnitTile,
    RightClickOnField,
    Default,
}