using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

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
    GameObject rogueSection;

    public Level level;

    public bool isGameOver = false;
        
    // This property stores the currently selected unit
    public Unit UnitSelected { get; set; }

	//public bool IsCameraFocused { get{return cameraManager.GetCameraFocusStatus();} set{cameraManager.SetCameraFocusStatus(value);} }
	public bool IsAnyUnitMoving { get{return structureManager.IsObjectMoving;}}
    public bool IsGameInStandby { get{return generalManager.IsGameInStandby;}}
    public bool IsShowingPath { get; set; }
    public ActionPerformed ActionInQueue { get; set; }
    public GameObject ActionTarget { get; set; }
	public bool IsSetup { get; set; }
    public int[] SetupTiles { get; set; }

    public int CurrentTurnCount { get; set; }
    public List<Unit> UnitsOnField { get { return structureManager.gameData.unitsOnField; } }
	#endregion

	#region Update Methods

	void Update()
    {
        ManageGame();
        ManageMovements();
        ManageInputs();
    }

    void ManageGame()
    {
        bool isFightWon = !isGameOver && UnitsOnField.Count(u => u.faction == ENEMY_FACTION) == 0;
        bool isFightLost = !isGameOver && UnitsOnField.Count(u => u.faction == USER_FACTION) == 0;

        if (isFightWon || isFightLost)
        {
            int goldGenerated = 0;
            if (isFightWon) goldGenerated = GenerateAndAddGold();
            isGameOver = true;
            GeneralManager.GameStatus status = isFightWon ? GeneralManager.GameStatus.Won : GeneralManager.GameStatus.Lost;
            GameScreens screen = isFightWon ? GameScreens.FightVictoryScreen : GameScreens.FightDefeatScreen;
            generalManager.SaveGameProgress(status);
            structureManager.GetGameScreen(screen, goldGenerated);
        }
    }

    #endregion

    #region Key Input Management

    void ManageMovements(){
        // If an object is being moved, check if it has finished moving
        int finishedMoving = structureManager.MovementTick();

        switch (ActionInQueue)
		{
			case ActionPerformed.FightMovement:
				break;
			case ActionPerformed.Attack:
                if(finishedMoving != 0)
				{
                    structureManager.actionPerformer.StartAction(ActionPerformed.Attack, UnitSelected.gameObject, ActionTarget.GetComponent<Unit>().CurrentTile.gameObject);
                    ResetGameState(true);
                }
                return;
            case ActionPerformed.Default:
			default:
                return;
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

    public void Setup(Level level)
	{
        Debug.Log("START FIGHT MANAGER SETUP");

        this.level = level;

        aiManager = GetComponent<AIManager>();
        aiManager.seed = level.seed;

        StartLevel(level);

        Debug.Log("END FIGHT MANAGER SETUP");
    }

    void StartLevel(Level level)
    {
        IsSetup = true;

        Transform objectsParent = GameObject.Find("Fight Objects").transform;
        level.GenerateTerrain(false, objectsParent);

        // Setup the terrain based on the level information
        var tiles = structureManager.SetupFightSection(level.tilesDict, this, level.TopLeftSquarePositionX, level.YPosition, level.TopLeftSquarePositionZ, level.HorizontalTiles, level.VerticalTiles);

        var units = GenerateUnits(tiles, level.VerticalTiles, level.seed);
        structureManager.gameData = new(tiles, units, level.HorizontalTiles, level.VerticalTiles);

        SetupTiles = SetupUnitPosition();
    }

    int[] SetupUnitPosition()
	{
        List<Tile> tileList = new();
		for (int i = 0; i < level.HorizontalTiles; i++)
		{
			for (int j = 0; j < 2; j++)
			{
                Tile tile = structureManager.gameData.mapTiles[(i * level.HorizontalTiles) + j];
                if (tile.IsPassable)
                    tileList.Add(tile);
			}
		}

        structureManager.SelectTiles(tileList, false, TileType.Positionable);
        return tileList.Select(t => t.tileNumber).ToArray();
    }

    List<Unit> GenerateUnits(Dictionary<int, Tile> mapTiles, int VerticalTiles, int seed)
    {
        Transform unitsParent = GameObject.Find("Fight Units").transform;
        List<Unit> unitList = new();
        GameObject[] playerUnits = generalManager.PlayerUnits.Select(u => u.gameObject).ToArray();
        List<int> alreadyOccupiedTiles = new();
		for (int i = 0; i < playerUnits.Length; i++)
		{
            GameObject unit = playerUnits[i];
            //First part of the where clause removes unavailable terrain (like mountains), the second selects all the tiles of the first two columns of the game
            int[] possiblePlayerStartingUnits = mapTiles.Where(k => 
            (k.Value.IsPassable && !alreadyOccupiedTiles.Contains(k.Key)) && 
            (k.Key % VerticalTiles == 0 || k.Key % VerticalTiles == 1)).Select(k => k.Key).ToArray();

            Tile unitTile = null;
            bool exitClause = true;
            while (exitClause)
            {
                int startingTile = RandomManager.GetRandomValue(seed * (i + 1), 0, possiblePlayerStartingUnits.Length);
                unitTile = GameObject.Find($"Terrain_{possiblePlayerStartingUnits[startingTile]}").GetComponent<Tile>();
                alreadyOccupiedTiles.Add(unitTile.tileNumber);
                unitList.Add(GenerateSingleUnit(unit, unitTile, unitsParent));
                exitClause = !(unitTile != null && unitTile.IsPassable && possiblePlayerStartingUnits.Length > 0);
            }
        }

        GameObject[] enemyUnits = level.enemyList.Values.ToArray();
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            GameObject unit = enemyUnits[i];
            //First part of the where clause removes unavailable terrain (like mountains), the second selects all the tiles of the last two columns of the game
            int[] possiblePlayerStartingUnits = mapTiles.Where(k =>
            (k.Value.IsPassable && !alreadyOccupiedTiles.Contains(k.Key)) &&
            (k.Key % VerticalTiles == VerticalTiles - 1 || k.Key % VerticalTiles == VerticalTiles - 2)).Select(k => k.Key).ToArray();

            Tile unitTile = null;
            bool exitClause = true;
            while (exitClause)
            {
                int startingTile = RandomManager.GetRandomValue(seed * (i + 1), 0, possiblePlayerStartingUnits.Length);
                unitTile = GameObject.Find($"Terrain_{possiblePlayerStartingUnits[startingTile]}").GetComponent<Tile>();
                alreadyOccupiedTiles.Add(unitTile.tileNumber);
                unitList.Add(GenerateSingleUnit(unit, unitTile, unitsParent));
                exitClause = !(unitTile != null && unitTile.IsPassable && possiblePlayerStartingUnits.Length > 0);
            }
        }

        return unitList;
    }

    Unit GenerateSingleUnit(GameObject unit, Tile tile, Transform parent)
    {
        Quaternion rotation = new(0, 180, 0, 0);
        var unitGenerated = Instantiate(unit, tile.transform.position, rotation, parent);
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
        CurrentTurnCount = USER_FACTION;
        ResetGameState(true);
        structureManager.SetEndTurnButton(true);
        foreach (var unit in UnitsOnField.Where(u => u.faction == USER_FACTION))
        {
            unit.movementCurrent = unit.movementMax;
            unit.HasPerformedMainAction = false;
        }
    }

    void EndUserTurn(){
        CurrentTurnCount = ENEMY_FACTION;
        ResetGameState(true);
        structureManager.SetEndTurnButton(false);
        aiManager.StartAITurn();
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
		if (IsSetup)
		{
            ResetGameState(false);
            SetupUnitPosition();
            if (unit.faction == ENEMY_FACTION)
                UnitSelected = null;
            structureManager.SelectTiles(unit.CurrentTile.ToList(), false, TileType.Selected);
        }
		else
		{
            //We selected an allied unit that has already performed his action this turn
            if ((UnitSelected && UnitSelected.HasPerformedMainAction))
            {
                ResetGameState(false);
                structureManager.SelectTiles(unit.CurrentTile.ToList(), true);
                return;
            }

            //We selected an allied unit that can still perform an action
            if (unit.faction == USER_FACTION)
            {
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
    }

    void ManageClick_EmptyTileSelected(Tile tileSelected)
    {
        if(IsSetup)
		{
			//User wants to move a unit
			if (UnitSelected)
			{
                //User clicked on an allowed tile to teleport the unit
                if(tileSelected.unitOnTile == null && tileSelected.IsPassable && SetupTiles.Contains(tileSelected.tileNumber))
                    structureManager.MoveUnit(UnitSelected, tileSelected, true);
                ResetGameState(true);
                SetupUnitPosition();
            }
            else
			{
                ResetGameState(true);
                structureManager.ClearSelection(true);
                SetupUnitPosition();
                structureManager.SelectTiles(tileSelected.ToList(), false);
            }
		} 
        else {
            if (!UnitSelected)
            {
                ResetGameState(true);
                structureManager.SelectTiles(tileSelected.ToList(), true);
                return;
            }

            //If tile clicked is in range
            if (UnitSelected.PossibleMovements.Contains(tileSelected))
            {
                if (IsShowingPath)
                {
                    structureManager.MoveUnit(UnitSelected, tileSelected, false);
                    ResetGameState(true);
                }
                else
                {
                    AskForMovementConfirmation(tileSelected);
                }
                return;
            }

            //User clicked outside the range
            ResetGameState(true);
            structureManager.SelectTiles(tileSelected.ToList(), true);
        }
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

        structureManager.MoveUnit(UnitSelected, targetTile, false);
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

    void EndPhase(int faction)
	{
        if (IsSetup)
		{
            IsSetup = false;
            List<Tile> tileList = structureManager.gameData.mapTiles.Values.ToList();
            structureManager.SelectTiles(tileList, false, TileType.Base);
            structureManager.UpdateFightEndPhaseButton();
        }
        else
            EndTurn(faction);
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
		{
            Destroy(tile.model3D);
            Destroy(tile.gameObject);
        }   

		foreach (var unit in UnitsOnField)
            Destroy(unit.gameObject);
    }
    public void ReturnToRogueButton(bool isDefeat)
    {
        GeneralManager fm = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
        fm.ReturnToRogue(RogueTileType.Fight, isDefeat);
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
        fm.EndPhase(faction);
    }

    public void MakeUnitTakeDamage()
	{
        structureManager.actionPerformer.StartTakeDamageAnimation();
	}

    public bool IsAnyUnitNotIdle()
	{
        return true;
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
    FightTeleport,
    Attack,
    Default,
}