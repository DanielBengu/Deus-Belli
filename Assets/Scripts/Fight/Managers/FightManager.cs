using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FightManager : MonoBehaviour
{
    [SerializeField]
    GeneralManager generalManager;

    public const int USER_FACTION = 0;
    public const int ENEMY_FACTION = 1;
    public const int LEFT_MOUSE_BUTTON = 1;
    public const int RIGHT_MOUSE_BUTTON = 1;
    public const int SCROLL_WHEEL_BUTTON = 2;
    //private const int UNIT_MOVEMENT_SPEED = 800;

    #region Fields

        [SerializeField]
        CameraManager cameraManager;
        StructureManager structureManager;
        AIManager aiManager;

        [SerializeField]
        GameObject warriorPrefab;
        [SerializeField]
        GameObject warrior2Prefab;

        [SerializeField]
        GameObject rogueSection;

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

        structureManager = GetComponent<StructureManager>();
        aiManager = GetComponent<AIManager>();

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

                ResetGameState(true);
            }    
        }

        void ManageInputs(){
            if(!IsGameInStandby && (Input.anyKeyDown || IsScrollButtonDown))
                ManageKeysDown();
        }

        //Method that manages the press of a key (only for the frame it is clicked)
        void ManageKeysDown(){
            if(Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON)){
                ResetGameState(true);
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
        var tiles = structureManager.Setup(level.tilesDict, this, level.TopLeftSquarePositionX, level.YPosition, level.TopLeftSquarePositionZ, level.XLength, level.YLength);

        var units = GenerateUnits(tiles);
        structureManager.gameData = new(tiles, units, level.XLength, level.YLength);
    }

    List<Unit> GenerateUnits(Dictionary<int, Tile> mapTiles)
    {
        List<Unit> unitList = new();

        Tile tileWarrior = GameObject.Find($"Terrain_21").GetComponent<Tile>();
        unitList.Add(GenerateSingleUnit(warriorPrefab, tileWarrior));

        Tile tileWarrior2 = GameObject.Find($"Terrain_28").GetComponent<Tile>();
        unitList.Add(GenerateSingleUnit(warrior2Prefab, tileWarrior2));

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
        var unitGenerated = Instantiate(unit, tile.transform.position, rotation);
        var unitScript = unitGenerated.GetComponent<Unit>();

        unitScript.SetupManager(this);
        unitScript.CurrentTile = tile;
        tile.unitOnTile = unitGenerated.GetComponent<Unit>();
        return unitScript;
    }

    #endregion

    void StartUserTurn()
    {
        CurrentTurn = USER_FACTION;
        ResetGameState(true);
        structureManager.SetEndTurnButton(true);
        foreach (var unit in structureManager.gameData.unitsOnField.Where(u => u.faction == USER_FACTION))
        {
            unit.movementCurrent = unit.movementMax;
            unit.HasPerformedMainAction = false;
        }
    }

    void EndUserTurn(){
        CurrentTurn = ENEMY_FACTION;
        ResetGameState(true);
        structureManager.SetEndTurnButton(false);
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
                ResetGameState(true);
            break;

            case ObjectClickedEnum.Default:
            break;
        }
    }

    #region Manage Click Actions

    void ManageClick_UnitSelected(Unit unit)
    {
        //We selected an allied unit that has already performed his action this turn
        if ((UnitSelected && UnitSelected.HasPerformedMainAction))
        {
            ResetGameState(false);
            structureManager.SelectTiles(unit.CurrentTile.ToList(), true, true);
            return;
        }

        //We selected an allied unit that can still perform an action
        if(unit.faction == USER_FACTION){
            ResetGameState(false);
            structureManager.GeneratePossibleMovementForUnit(UnitSelected, true);
            structureManager.FindPossibleAttacks(UnitSelected, structureManager.selectedTiles);
            return;
        }
        
        ///We either selected an enemy unit and didn't select an ally to perform an attack before,
        ///or we selected an enemy out of reach from the ally
        if (!UnitSelected || (UnitSelected && !structureManager.possibleAttacks.Find(t => t.tileNumber == unit.CurrentTile.tileNumber)))
		{
            ResetGameState(true);
            structureManager.SelectTiles(unit.CurrentTile.ToList(), true, true);
            return;
		}

        //User wants to attack an enemy, we confirm the action and the path to take
        if (!IsShowingPath)
        {
            AskForMovementConfirmation(unit.CurrentTile, UnitSelected.range);
            return;
        }

        //User confirmed the action
        QueueAttack(UnitSelected, unit);
    }

    void ManageClick_EmptyTileSelected(Tile tileSelected)
    {
        if (!UnitSelected)
        {
            ResetGameState(true);
            structureManager.SelectTiles(tileSelected.ToList(), true, true);
            return;
        }

        //If tile clicked is in range
        if(structureManager.selectedTiles.Contains(tileSelected)){
            if(IsShowingPath){
                structureManager.MoveUnit(UnitSelected, tileSelected);
                ResetGameState(true);
            }else{
                AskForMovementConfirmation(tileSelected);
            }
            return;
        }

        //User clicked outside the range
        ResetGameState(true);
        structureManager.SelectTiles(tileSelected.ToList(), true, true);
    }

    void AskForMovementConfirmation(Tile destinationTile, int attackerRange = 0)
    {
        structureManager.ClearSelection(false);
        List<Tile> path = structureManager.FindPathToDestination(destinationTile, false, false);
        List<Tile> attackTiles = path.Skip(path.Count - attackerRange).ToList();
        path = path.SkipLast(attackerRange).ToList();
        structureManager.SelectTiles(path, true, true, TileType.Selected);
        structureManager.SelectTiles(attackTiles, false, false, TileType.Enemy);
        IsShowingPath = true;
    }

    public void QueueAttack(Unit attacker, Unit defender)
    {
        structureManager.FindPathToDestination(defender.CurrentTile, false, true);

        int targetMovementTileIndex = structureManager.selectedTiles.Count - 1 - attacker.range;
        if (targetMovementTileIndex < 0) targetMovementTileIndex = 0;
        Tile targetTile = structureManager.selectedTiles[targetMovementTileIndex];

        structureManager.MoveUnit(UnitSelected, targetTile);
        ActionInQueue = ActionPerformed.Attack;
        ActionTarget = defender.gameObject;
        UnitSelected = attacker;
        IsShowingPath = false;
        structureManager.ClearSelection(true);
    }

	#endregion

    void ResetGameState(bool resetUnitSelected)
    {
        if (resetUnitSelected) UnitSelected = null;
        structureManager.possibleAttacks = new();
        IsShowingPath = false;
        structureManager.ClearSelection(true);
        structureManager.SetInfoPanel(false);
        ActionInQueue = ActionPerformed.Default;
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

    //Called by "End Turn" button of UI
    public void EndTurnButton(int faction){
        FightManager fm = GameObject.Find("Fight Manager").GetComponent<FightManager>();
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