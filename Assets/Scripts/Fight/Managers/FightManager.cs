using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FightManager : MonoBehaviour
{
    public const int RIGHT_MOUSE_BUTTON = 1;
    public const int SCROLL_WHEEL_BUTTON = 2;

    #region Fields

        [SerializeField]
        CameraManager cameraManager;
        StructureManager structureManager;
        AIManager aiManager;

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
        GameObject warriorPrefab = Resources.Load<GameObject>($"Prefabs/Fight/Warrior");
        
        Tile tileWarrior = GameObject.Find($"Terrain_1").GetComponent<Tile>();
        GenerateSingleUnit(warriorPrefab, tileWarrior);

        foreach (var enemy in level.enemyList)
        {
            Tile tile = structureManager.GetMapTiles()[enemy.Key];
            GenerateSingleUnit(enemy.Value, tile);
        }
    }

    void GenerateSingleUnit(GameObject unit, Tile tile){
        var unitGenerated = GameObject.Instantiate(unit,tile.transform.position,Quaternion.identity) as GameObject;
        var unitScript = unitGenerated.GetComponent<Unit>();

        unitScript.currentTile = tile;
        tile.unitOnTile = unitGenerated;
        units.Add(unitScript);
    }

    public void ManageClick(ObjectClickedEnum obj, GameObject reference){
        switch (obj)
        {
            case ObjectClickedEnum.EmptyTile:
                var tileSelected = reference.GetComponent<Tile>();
                if(UnitSelected){
                    //If tile clicked is in range
                    if(structureManager.tiles.Contains(tileSelected)){
                        if(IsShowingPath){
                            structureManager.StartUnitMovement(UnitSelected, reference.GetComponent<Tile>());
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
                    }
                }else{
                    structureManager.TileSelected(tileSelected);
                }   
                structureManager.SetInfoPanel(false, UnitSelected);
            break;
            case ObjectClickedEnum.UnitTile:
                if(UnitSelected.faction == 0){
                    structureManager.GeneratePossibleMovementForUnit(UnitSelected, true);
                }else{
                    structureManager.TileSelected(UnitSelected.currentTile);
                }
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
            case 0:
                CurrentTurn = 1;
                structureManager.SetEndTurnButton(false);
                aiManager.CalculateTurn();
                break;
            case 1:
                CurrentTurn = 0;
                structureManager.SetEndTurnButton(true);
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