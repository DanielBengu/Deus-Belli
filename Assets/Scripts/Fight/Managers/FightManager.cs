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
        
        float scrollWheelInput;

        [SerializeField]
        GameObject OptionsPrefab;
        
        // This property stores the currently selected unit
        public Unit UnitSelected { get; set; }

        //public bool IsCameraFocused { get{return cameraManager.GetCameraFocusStatus();} set{cameraManager.SetCameraFocusStatus(value);} }
        public bool IsAnyUnitMoving { get{return structureManager.IsObjectMoving;}}
        public bool IsOptionOpen { get; set; }
        public bool IsScrollButtonDown { get; set; }
        public int CurrentTurn { get; set; }
        public bool IsGameInStandby { get{return IsAnyUnitMoving || IsOptionOpen || CurrentTurn != 0;}}
        public bool IsShowingPath { get; set; }
        public ActionPerformed ActionInQueue { get; set; }
        public GameObject ActionTarget { get; set; }
        public List<Tile> TilesSelected { get { return structureManager.selectedTiles; } set { structureManager.selectedTiles = value; } }

    #endregion

    #region Start and Update

    void Start()
        {
            Debug.Log("START FIGHT MANAGER SETUP");

            structureManager = this.GetComponent<StructureManager>();
            aiManager = this.GetComponent<AIManager>();

            StartLevel();

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
            if (!IsAnyUnitMoving)
                return;

            // If an object is being moved, check if it has finished moving
            if (structureManager.MovementTick())
            {
                if(ActionInQueue == ActionPerformed.Attack)
                    structureManager.actionPerformer.StartAction(ActionPerformed.Attack, UnitSelected, ActionTarget.GetComponent<Unit>().CurrentTile);

                UnitSelected = null;
            }    
        }

        void ManageInputs(){
            if(!IsGameInStandby && (Input.anyKeyDown || IsScrollButtonDown))
                ManageKeysDown();
        }

        //Method that manages the press of a key (only for the frame it is clicked)
        void ManageKeysDown(){
            if(Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON)){
                structureManager.ClearSelection();
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                MainMenu.GeneratePrefab(OptionsPrefab, "Options");
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
            if(scrollWheelInput != 0f)
                cameraManager.ScrollWheel(scrollWheelInput);
            if(IsScrollButtonDown)
                cameraManager.UpdatePosition();
        }
    #endregion

    #region Startup Methods

        void StartLevel()
        {
            var levelBase = GameObject.Find("Level One").GetComponent<LevelOne>();
            levelBase.StartLevel();
            level = levelBase.level;

            // Setup the terrain based on the level information
            var tiles = structureManager.Setup(level.tilesDict, level.TopLeftSquarePositionX, level.YPosition, level.TopLeftSquarePositionZ, level.XLength, level.YLength);

            var units = GenerateUnits(tiles);
            structureManager.gameData = new(tiles, units, level.XLength, level.YLength);
        }

        List<Unit> GenerateUnits(Dictionary<int, Tile> mapTiles)
        {
            List<Unit> unitList = new();
            Tile tileWarrior = GameObject.Find($"Terrain_1").GetComponent<Tile>();
            unitList.Add(GenerateSingleUnit(warriorPrefab, tileWarrior));

            foreach (var enemy in level.enemyList)
            {
                Tile tile = mapTiles[enemy.Key];
                unitList.Add(GenerateSingleUnit(enemy.Value, tile));
            }
            return unitList;
        }

        Unit GenerateSingleUnit(GameObject unit, Tile tile)
        {
            Quaternion rotation = new(0, 180, 0, 0);
            var unitGenerated = GameObject.Instantiate(unit, tile.transform.position, rotation) as GameObject;
            var unitScript = unitGenerated.GetComponent<Unit>();

            unitScript.CurrentTile = tile;
            tile.unitOnTile = unitGenerated.GetComponent<Unit>();
            return unitScript;
        }

    #endregion

    void StartUserTurn()
    {
        UnitSelected = null;
        structureManager.selectedTiles = new();
        structureManager.possibleAttacks = new();
        CurrentTurn = USER_FACTION;
        structureManager.SetEndTurnButton(true);
        foreach (var unit in structureManager.gameData.unitsOnField.Where(u => u.faction == USER_FACTION))
        {
            unit.movementCurrent = unit.movementMax;
            unit.HasPerformedMainAction = false;
        }
    }

    void EndUserTurn(){
        CurrentTurn = ENEMY_FACTION;
        UnitSelected = null;
        IsShowingPath = false;
        structureManager.SetEndTurnButton(false);
        structureManager.ClearSelection(true);
        aiManager.StartAITurn();
    }

    public void ManageClick(ObjectClickedEnum objectClicked, GameObject reference){
        switch (objectClicked)
        {
            case ObjectClickedEnum.EmptyTile:
                var tileSelected = reference.GetComponent<Tile>();
                ManageClick_EmptyTileSelected(tileSelected);
                structureManager.SetInfoPanel(false, UnitSelected);
            break;

            case ObjectClickedEnum.UnitTile:
                var unitSelected = reference.GetComponent<Unit>();
                ManageClick_UnitSelected(unitSelected);
                structureManager.SetInfoPanel(true, unitSelected);
            break;

            case ObjectClickedEnum.RightClickOnField:
                structureManager.ClearSelection();
            break;

            case ObjectClickedEnum.Default:
            break;
        }
    }

    #region Manage Click Actions

        void ManageClick_UnitSelected(Unit unit)
        {
            //Allied unit already performed his action this turn
            if ((UnitSelected && UnitSelected.HasPerformedMainAction) || unit.HasPerformedMainAction)
            {
                structureManager.SelectTiles(unit.CurrentTile.ToList(), true);
                return;
            }

            if(unit.faction == USER_FACTION){
                structureManager.GeneratePossibleMovementForUnit(UnitSelected, true);
                structureManager.FindPossibleAttacks(UnitSelected, structureManager.selectedTiles);
                IsShowingPath = false;
            }
            else{
                if (!UnitSelected)
                    return;

                if (!IsShowingPath)
                {
                    AskForMovementConfirmation(unit.CurrentTile);
                    return;
                }

                QueueAttack(UnitSelected, unit);
            }
        }

        void ManageClick_EmptyTileSelected(Tile tileSelected)
        {
            if (!UnitSelected)
            {
                IsShowingPath = false;
                structureManager.SelectTiles(tileSelected.ToList(), true);
                return;
            }

            //If tile clicked is in range
            if(structureManager.selectedTiles.Contains(tileSelected)){
                if(IsShowingPath){
                    structureManager.StartUnitMovement(UnitSelected, tileSelected, UNIT_MOVEMENT_SPEED);
                    structureManager.SetInfoPanel(false, UnitSelected);
                    IsShowingPath = false;
                    UnitSelected = null;
                    return;
                }else{
                    AskForMovementConfirmation(tileSelected);
                }
            }else{
                //User clicked outside the range
                structureManager.ClearSelection();
                structureManager.SelectTiles(tileSelected.ToList(), true);
                IsShowingPath = false;
                UnitSelected = null;
            }  
        }

    void AskForMovementConfirmation(Tile destinationTile)
    {
        /*if (!structureManager.selectedTiles.Find(t => t.tileNumber == destinationTile.tileNumber))
            return;*/
        structureManager.ClearSelection(false);
        structureManager.FindPathToDestination(destinationTile, true, true);
        IsShowingPath = true;
    }

    public void QueueAttack(Unit attacker, Unit defender)
    {
        int secondLastTile = structureManager.selectedTiles.Count - 2;
        Tile targetTile = structureManager.selectedTiles[secondLastTile]; //The unit moves to the second-last tile of the path, adjacent to the enemy
        structureManager.MoveUnit(UnitSelected, targetTile);
        ActionInQueue = ActionPerformed.Attack;
        ActionTarget = defender.gameObject;
        UnitSelected = attacker;
        IsShowingPath = false;
        structureManager.ClearSelection();
    }

	#endregion

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

    //Called by "End Turn" button of UI
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

public enum ActionPerformed
{
    Movement,
    Attack,
    Default,
}