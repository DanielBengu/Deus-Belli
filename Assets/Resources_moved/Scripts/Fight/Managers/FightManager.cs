using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.Device;
using static Pathfinding;
using static FightInput;

public class FightManager : MonoBehaviour
{
    public GeneralManager generalManager;

    public const int USER_FACTION = 0;
    public const int ENEMY_FACTION = 1;
    //private const int UNIT_MOVEMENT_SPEED = 800;

    #region Fields

    public CameraManager cameraManager;
    public StructureManager structureManager;
    AIManager aiManager;

    public Transform fightObjects;
    public Transform fightObjectsRotator;

    [SerializeField]
    GameObject rogueSection;

    public Level level;

    public bool isGameOver = false;

    public float unitsSpeed;
        
    // This property stores the currently selected unit
    public Unit UnitSelected { get; set; }
    //Possible attacks for the selected unit
    public List<PossibleAttack> PossibleAttacks { get; set; }

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
	public FightInput FightInput { get; set; }
	#endregion

	#region Update Methods

	void Update()
    {
        ManageEndGame();
        ManageMovements();
        ManageInputs();
    }

    void ManageEndGame()
    {
        if (isGameOver)
            return;

        //I check first the defeat section to avoid ties (if both are dead at the same time, player lost)
        bool isDefeat = ManageDefeat();
        if(!isDefeat)
		    ManageVictory();
    }

    #endregion

    #region Key Input Management
    void ManageInputs(){
        if (!IsGameInStandby && Input.anyKeyDown)
            ManageKeysDown();
    }

    //Method that manages the press of a key (only for the frame it is clicked)
    void ManageKeysDown(){
        if (Input.GetMouseButtonDown((int)MouseButton.Middle))
            ResetGameState(true);
    }
    #endregion

    #region Startup Methods

    //Substitute of Start()
    public void Setup(Level level, StructureManager sm, CameraManager cm, GeneralManager gm)
	{
        Debug.Log("START FIGHT MANAGER SETUP");

        structureManager = sm;
        cameraManager = cm;
        generalManager = gm;
        structureManager.uiManager.SetFightVariables(gm.GodSelected);
        structureManager.spriteManager.fightManager = this;
        FightInput = new FightInput(this, structureManager);

        this.level = level;

        aiManager = GetComponent<AIManager>();
        aiManager.seed = level.seed;

        StartLevel(level);

        Debug.Log("END FIGHT MANAGER SETUP");
    }

    void StartLevel(Level level)
    {
        IsSetup = true;

        level.GenerateTerrain(false, null);

        // Setup the terrain based on the level information
        var tiles = structureManager.SetupFightSection(level.tilesDict, this, level.TopLeftSquarePositionX, level.YPosition, level.TopLeftSquarePositionZ, level.mapData.Rows, level.mapData.Columns);

        Transform lastTile = tiles[level.mapData.Columns * level.mapData.Rows - 1].transform;
        float rotatorX = (tiles[0].transform.position.x + lastTile.position.x) / 2;
        float rotatorZ = (tiles[0].transform.position.z + lastTile.position.z) / 2;
        fightObjects.transform.position = new Vector3(rotatorX, fightObjectsRotator.transform.position.y, rotatorZ);
		for (int i = 0; i < tiles.Count; i++)
            tiles[i].transform.parent = fightObjects;

        var units = GenerateUnits(tiles, level.mapData.Columns, level.seed);
        structureManager.gameData = new(tiles, units, level.mapData.Rows, level.mapData.Columns);

        SetupTiles = SetupUnitPosition();
    }

    public int[] SetupUnitPosition()
	{
        List<Tile> tileList = new();
		for (int i = 0; i < level.mapData.Rows; i++)
		{
			for (int j = 0; j < 2; j++)
			{
                Tile tile = structureManager.gameData.mapTiles[(i * level.mapData.Rows) + j];
                if (tile.IsPassable)
                    tileList.Add(tile);
			}
		}

        structureManager.SelectTiles(tileList, false, TileType.Positionable);
        return tileList.Select(t => t.tileNumber).ToArray();
    }

    List<Unit> GenerateUnits(Dictionary<int, Tile> mapTiles, int VerticalTiles, int seed)
    {
		List<Unit> unitList = new();
		Transform unitsParent = GameObject.Find("Fight Units").transform;

		//First part of the where clause removes unavailable terrain (like mountains), the second selects all the tiles of the first two columns of the game
		int[] possiblePlayerStartingUnits = mapTiles.Where(k => k.Value.IsPassable &&
		(k.Key % VerticalTiles == 0 || k.Key % VerticalTiles == 1)).Select(k => k.Key).ToArray();
		UnitData[] playerUnits = generalManager.PlayerUnits.ToArray();

		//Player units
		unitList.AddRange(GenerateListOfUnits(possiblePlayerStartingUnits, playerUnits, seed, unitsParent));

		//First part of the where clause removes unavailable terrain (like mountains), the second selects all the tiles of the last two columns of the game
		int[] possibleEnemyStartingUnits = mapTiles.Where(k => k.Value.IsPassable &&
		(k.Key % VerticalTiles == VerticalTiles - 1 || k.Key % VerticalTiles == VerticalTiles - 2)).Select(k => k.Key).ToArray();
		UnitData[] enemyUnits = level.enemyList.Values.ToArray();

		//Enemy units
		unitList.AddRange(GenerateListOfUnits(possibleEnemyStartingUnits, enemyUnits, seed, unitsParent));

        return unitList;
    }

    List<Unit> GenerateListOfUnits(int[] possiblePlayerStartingUnits, UnitData[] playerUnits, int seed, Transform unitsParent)
    {
        List<Unit> unitList = new();
		List<int> alreadyOccupiedTiles = new();
		for (int i = 0; i < playerUnits.Length; i++)
		{
			var unit = playerUnits[i];
			bool exitClause = true;
			while (exitClause)
			{
				int startingTile = RandomManager.GetRandomValue(seed * (i + 1), 0, possiblePlayerStartingUnits.Length);
				Tile unitTile = GameObject.Find($"Terrain_{possiblePlayerStartingUnits[startingTile]}").GetComponent<Tile>();
				alreadyOccupiedTiles.Add(unitTile.tileNumber);
				unitList.Add(GenerateSingleUnit(unit, unitTile, unitsParent));
				exitClause = !(unitTile != null && unitTile.IsPassable && possiblePlayerStartingUnits.Length > 0);
			}
		}
        return unitList;
	}

    Unit GenerateSingleUnit(UnitData unit, Tile tile, Transform parent)
    {
        Quaternion rotation = new(0, 180, 0, 0);
        GameObject model = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, unit.ModelName);
		var unitGenerated = Instantiate(model, tile.transform.position, rotation, parent);
		var unitScript = unitGenerated.GetComponent<Unit>();
		unitScript.Load(unit);
		unitScript.FightManager = this;
		unitScript.Movement.CurrentTile = tile;
        unitScript.fightData = new(unit.PortraitName, unit.Stats.Hp, unit.Stats.Movement);
        tile.unitOnTile = unitScript;
        return unitScript;
    }

	#endregion

	#region Manage Methods
	void ManageAction()
	{
		if (ActionInQueue != ActionPerformed.Attack)
			return;

		structureManager.actionPerformer.StartAction(ActionPerformed.Attack, UnitSelected.gameObject, ActionTarget.GetComponent<Unit>().Movement.CurrentTile.gameObject);
		ResetGameState(true);
	}

    void ManageVictory()
    {
		bool isFightWon = UnitsOnField.Count(u => u.unitData.Faction == ENEMY_FACTION) == 0;

        if (!isFightWon) return;

		isGameOver = true;
		int goldGenerated = GenerateAndAddGold();
		generalManager.SaveGameProgress(GeneralManager.GameStatus.Won);
		structureManager.GetGameScreen(GameScreens.FightVictoryScreen, goldGenerated);
	}

    bool ManageDefeat()
    {
		bool isFightLost = UnitsOnField.Count(u => u.unitData.Faction == USER_FACTION) == 0;

        if(!isFightLost) return false;

		isGameOver = true;
		generalManager.SaveGameProgress(GeneralManager.GameStatus.Lost);
		structureManager.GetGameScreen(GameScreens.FightVictoryScreen, -1);
        return true;
	}

	void ManageMovements()
	{
		structureManager.MovementTick(unitsSpeed, ManageAction);
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
        foreach (var unit in UnitsOnField.Where(u => u.unitData.Faction == USER_FACTION))
        {
            unit.fightData.currentMovement = unit.unitData.Stats.Movement;
            unit.Movement.HasPerformedMainAction = false;
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

	public void ManageClick(ObjectClickedEnum objectClicked, GameObject reference)
	{
		FightInput.ManageClick(objectClicked, reference);
	}

    public void QueueAttack(Unit attacker, Unit defender, Tile tileToMoveTo)
    {
        List<Tile> path = structureManager.FindPathToDestination(tileToMoveTo, false, attacker.Movement.CurrentTile.tileNumber).ToList();

        int targetMovementTileIndex = path.Count - 1;
        if (targetMovementTileIndex < 0) targetMovementTileIndex = 0;

        Tile targetTile = attacker.Movement.CurrentTile;
        if(path.Count > 0)
            targetTile = structureManager.gameData.mapTiles[path[targetMovementTileIndex].tileNumber];

		structureManager.MoveUnit(UnitSelected, targetTile, false);
		ActionInQueue = ActionPerformed.Attack;
        ActionTarget = defender.gameObject;
        UnitSelected = attacker;
        IsShowingPath = false;
        structureManager.ClearSelection(true);
    }

    public void ResetGameState(bool resetUnitSelected)
    {
        if (resetUnitSelected) UnitSelected = null;
        IsShowingPath = false;
        structureManager.ClearSelection(true);
        structureManager.SetInfoPanel(false);
        ActionInQueue = ActionPerformed.Default;
        PossibleAttacks = new();
    }

    void EndPhase(int faction)
	{
        if (IsSetup)
        {
            IsSetup = false;
            List<Tile> tileList = structureManager.gameData.mapTiles.Values.ToList();
            structureManager.SelectTiles(tileList, false, TileType.Base);
            structureManager.UpdateFightEndPhaseButton();
            return;
        }

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
            case 2://For neutral armies
                break;
            default:
                break;
        }
    }

    public void DisableFightSection()
	{
		foreach (var tile in structureManager.gameData.mapTiles.Values)
            Destroy(tile.gameObject);

		foreach (var unit in UnitsOnField)
            Destroy(unit.gameObject);
    }

    public List<PossibleAttack> GetPossibleAttacksForUnit(Unit unit, List<Tile> possibleMovements)
	{
        return structureManager.GetPossibleAttacksForUnit(unit, false, possibleMovements);
	}


    public List<Tile> GetPossibleMovements(Unit unit)
    {
        return structureManager.GeneratePossibleMovementForUnit(unit, false);
    }

	#region Button Calls
	//Called by "End Turn" button of UI
	public void EndTurnButton(int faction)
	{
		FightManager fm = GameObject.Find(GeneralManager.FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
		fm.EndPhase(faction);
	}

	public void MakeUnitTakeDamage()
	{
		structureManager.actionPerformer.StartTakeDamageAnimation();
	}
	#endregion
}
public enum ActionPerformed
{
    FightMovement,
    RogueMovement,
    FightTeleport,
    Attack,
    Default,
}