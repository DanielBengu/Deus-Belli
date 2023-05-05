using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FightManager : MonoBehaviour
{
    public GeneralManager generalManager;

    public const int USER_FACTION = 0;
    public const int ENEMY_FACTION = 1;
    public const int LEFT_MOUSE_BUTTON = 1;
    public const int RIGHT_MOUSE_BUTTON = 1;
    //private const int UNIT_MOVEMENT_SPEED = 800;

    #region Fields

    public CameraManager cameraManager;
    public StructureManager structureManager;
    AIManager aiManager;

    [SerializeField]
    GameObject warriorPrefab;
    [SerializeField]
    GameObject warrior2Prefab;

    [SerializeField]
    GameObject rogueSection;

    Level level;

    public bool isGameOver = false;
        
    // This property stores the currently selected unit
    public Unit UnitSelected { get; set; }

	//public bool IsCameraFocused { get{return cameraManager.GetCameraFocusStatus();} set{cameraManager.SetCameraFocusStatus(value);} }
	public bool IsAnyUnitMoving { get{return structureManager.IsObjectMoving;}}
    public int CurrentTurn { get; set; }
    public bool IsGameInStandby { get{return generalManager.IsGameInStandby;}}
    public bool IsShowingPath { get; set; }
    public ActionPerformed ActionInQueue { get; set; }
    public GameObject ActionTarget { get; set; }

    #endregion

    #region Start and Update

    void Start()
    {
        Debug.Log("START FIGHT MANAGER SETUP");
        aiManager = GetComponent<AIManager>();

        StartLevel();

        Debug.Log("END FIGHT MANAGER SETUP");
    }

    void Update()
    {
        ManageGame();
        ManageMovements();
        ManageInputs();
    }

    #endregion
    
    #region Key Input Management

    void ManageMovements(){
        // If an object is being moved, check if it has finished moving
        if (structureManager.MovementTick())
        {
            if(ActionInQueue == ActionPerformed.Attack)
                structureManager.actionPerformer.StartAction(ActionPerformed.Attack, UnitSelected.gameObject, ActionTarget.GetComponent<Unit>().CurrentTile.gameObject);

            ResetGameState(true);
        }    
    }

    void ManageInputs(){
        if (!IsGameInStandby && Input.anyKeyDown)
            ManageKeysDown();
    }

    //Method that manages the press of a key (only for the frame it is clicked)
    void ManageKeysDown(){
        if (Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON))
            ResetGameState(true);
    }
    #endregion

    #region Startup Methods

    void StartLevel()
    {
        var levelBase = GameObject.Find("Level One").GetComponent<LevelOne>();
        levelBase.StartLevel();
        level = levelBase.level;

        // Setup the terrain based on the level information
        var tiles = structureManager.SetupFightSection(level.tilesDict, this, level.TopLeftSquarePositionX, level.YPosition, level.TopLeftSquarePositionZ, level.XLength, level.YLength);
       

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

    public void SetIsOptionOpen(bool isOptionOpen)
	{
        generalManager.IsOptionOpen = isOptionOpen;
	}

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

    void ManageGame()
	{
        //Killed every enemy unit
        if(isGameOver == false && structureManager.gameData.unitsOnField.Where(u => u.faction == ENEMY_FACTION).ToList().Count == 0)
		{
            isGameOver = true;
            int gold = GenerateAndAddGold();
            structureManager.GetFightVictoryScreen(gold);
        }
            
	}

    int GenerateAndAddGold()
	{
        int goldGenerated = 100;
        generalManager.Gold += goldGenerated;
        PlayerPrefs.SetInt("Gold", generalManager.Gold);
        return goldGenerated;
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
            structureManager.SelectTiles(unit.CurrentTile.ToList(), true);
            return;
        }

        //We selected an allied unit that can still perform an action
        if (unit.faction == USER_FACTION) {
            ResetGameState(false);
            structureManager.GeneratePossibleMovementForUnit(UnitSelected, true);
            structureManager.GetPossibleAttacksForUnit(UnitSelected, true);
            return;
        }

        ///We either selected an enemy unit and didn't select an ally to perform an attack before,
        ///or we selected an enemy out of reach from the ally
        if (!UnitSelected || !IsAttackPossible(UnitSelected, unit))
        {
            ResetGameState(true);
            structureManager.SelectTiles(unit.CurrentTile.ToList(), true);
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
            structureManager.SelectTiles(tileSelected.ToList(), true);
            return;
        }

        //If tile clicked is in range
        if(UnitSelected.PossibleMovements.Contains(tileSelected)){
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
        structureManager.SelectTiles(tileSelected.ToList(), true);
    }

    bool IsAttackPossible(Unit attacker, Unit defender)
	{
        if(structureManager.IsAttackPossible(attacker, defender))
            return true;

        return false;
	}

    void AskForMovementConfirmation(Tile destinationTile, int attackerRange = 0)
    {
        structureManager.ClearSelection(false);
        List<Tile> path = structureManager.FindPathToDestination(destinationTile, false);
        List<Tile> attackTiles = path.Skip(path.Count - attackerRange).ToList();
        path = path.SkipLast(attackerRange).ToList();
        structureManager.SelectTiles(path, true, TileType.Selected);
        structureManager.SelectTiles(attackTiles, false, TileType.Enemy);
        IsShowingPath = true;
    }

    public void QueueAttack(Unit attacker, Unit defender)
    {
        List<Tile> path = structureManager.FindPathToDestination(defender.CurrentTile, false);

        int targetMovementTileIndex = path.Count - 1 - attacker.range;
        if (targetMovementTileIndex < 0) targetMovementTileIndex = 0;
        Tile targetTile = structureManager.gameData.mapTiles[path[targetMovementTileIndex].tileNumber];

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

    public void DisableFightSection()
	{
		foreach (var tile in structureManager.gameData.mapTiles.Values)
            Destroy(tile.gameObject);

		foreach (var unit in structureManager.gameData.unitsOnField)
            Destroy(unit.gameObject);
    }
    public void ReturnToRogueButton()
    {
        GeneralManager fm = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
        fm.ReturnToRogueFromFightButton();
    }

    public List<Tile> GetPossibleAttacksForUnit(Unit unit)
	{
        return structureManager.GetPossibleAttacksForUnit(unit, false);
	}


    public List<Tile> GetPossibleMovements(Unit unit)
    {
        return structureManager.GeneratePossibleMovementForUnit(unit, false);
    }

    //Called by "End Turn" button of UI
    public void EndTurnButton(int faction){
        FightManager fm = GameObject.Find(GeneralManager.FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
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
    FightMovement,
    RogueMovement,
    Attack,
    Default,
}