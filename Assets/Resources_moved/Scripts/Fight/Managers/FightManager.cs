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
	public bool IsSetup { get { return structureManager.gameData.IsSetup; } }
    public int[] SetupTiles { get; set; }

    public int CurrentTurn { get { return TurnManager.CurrentTurn; } set { TurnManager.CurrentTurn = value; } }
    public int TurnCount { get; set; } = 0;
    public List<Unit> UnitsOnField { get { return structureManager.gameData.unitsOnField; } }
    public List<Tile> TileList { get { return structureManager.gameData.GetTileList(); } }
    public FightInput FightInput { get; set; }
    public TurnManager TurnManager { get; set; }
    #endregion

	internal Transform leftUnitShowcasePosition;
	internal Transform leftUnitShowcaseParent;
	internal Transform rightUnitShowcasePosition;
	internal Transform rightUnitShowcaseParent;

	#region Update Methods

	void Update()
    {
        ManageEndGame();
        ManageMovements();
        FightInput.ManageInputs();
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

    #region Startup Methods

    //Substitute of Start()
    public void Setup(Level level, StructureManager sm, CameraManager cm, GeneralManager gm)
	{
        Debug.Log("START FIGHT MANAGER SETUP");

		leftUnitShowcasePosition = GameObject.Find("Left Character Position").transform;
		leftUnitShowcaseParent = GameObject.Find("ShowcaseChildrenLeft").transform;
		rightUnitShowcasePosition = GameObject.Find("Right Character Position").transform;
		rightUnitShowcaseParent = GameObject.Find("ShowcaseChildrenRight").transform;
		structureManager = sm;

        cameraManager = cm;
        cameraManager.SetupFightCamera(cameraManager.transform);

		generalManager = gm;
        structureManager.uiManager.SetFightVariables(gm.GodSelected);
        structureManager.spriteManager.fightManager = this;

        this.level = level;

        aiManager = GetComponent<AIManager>();
        aiManager.seed = level.seed;

		FightInput = new FightInput(this, structureManager);
		TurnManager = new(this, structureManager, aiManager);

		StartLevel(level);

        Debug.Log("END FIGHT MANAGER SETUP");
    }

    void StartLevel(Level level)
    {
        level.GenerateTerrain(false, null);

        // Setup the terrain based on the level information
        var tiles = structureManager.SetupFightSection(level, this);

        Transform lastTile = tiles[level.mapData.Columns * level.mapData.Rows - 1].transform;
        float rotatorX = (tiles[0].transform.position.x + lastTile.position.x) / 2;
        float rotatorZ = (tiles[0].transform.position.z + lastTile.position.z) / 2;
        fightObjects.transform.position = new Vector3(rotatorX, fightObjectsRotator.transform.position.y, rotatorZ);
		for (int i = 0; i < tiles.Count; i++)
            tiles[i].transform.parent = fightObjects;

        var units = GenerateUnitsOnField(level);
        structureManager.gameData = new(tiles, units, level.mapData.Rows, level.mapData.Columns);

        SetupTiles = SetupUnitPosition();
    }

    public int[] SetupUnitPosition()
	{
        //We retrieve the Tiles that have StartPositionForFaction = 1 and ValidForMovement = true in the map config
		List<Tile> tileList = structureManager.gameData.mapTiles.Where(t => t.Value.data.StartPositionForFaction == USER_FACTION && t.Value.data.ValidForMovement).Select(t => t.Value).ToList();

		structureManager.SelectTiles(tileList, true, TileType.Positionable);
        return tileList.Select(t => t.data.PositionOnGrid).ToArray();
    }

    List<Unit> GenerateUnitsOnField(Level level)
    {
		List<Unit> unitList = new();
        List<UnitData> unitsOnMap = generalManager.PlayerUnits;
        unitsOnMap.AddRange(level.enemyList.Select(e => e.Value).ToList());
		Transform unitsParent = GameObject.Find("Fight Units").transform;

        int[] factionsPresent = unitsOnMap.DistinctBy(u => u.Faction).Select(u => u.Faction).ToArray();

        for (int i = 0; i < factionsPresent.Count(); i++)
        {
            int faction = factionsPresent[i];
            UnitData[] unitsOfFaction = unitsOnMap.Where(u => u.Faction == faction).ToArray();
            int[] possibleStartingTiles = level.GetValidStartingPositionForFaction(faction);
			unitList.AddRange(GenerateListOfUnits(possibleStartingTiles, unitsOfFaction, level.seed, unitsParent));
		}

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
				alreadyOccupiedTiles.Add(unitTile.data.PositionOnGrid);
				unitList.Add(GenerateSingleUnit(unit, unitTile, unitsParent));
				exitClause = !(unitTile != null && unitTile.data.ValidForMovement && possiblePlayerStartingUnits.Length > 0);
			}
		}
        return unitList;
	}

    Unit GenerateSingleUnit(UnitData unit, Tile tile, Transform parent)
    {
        Quaternion rotation = new(0, 180, 0, 0);
        GameObject model = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, unit.ModelName);

		var unitGenerated = Instantiate(model, tile.transform.position, rotation, parent);
		var unitScript = unitGenerated.AddComponent<Unit>();
		unitScript.Load(unit, this, tile);

        tile.unitOnTile = unitScript;

        return unitScript;
    }

	#endregion

	#region Manage Methods
	void ManageAction()
	{
		if (ActionInQueue != ActionPerformed.SimpleAttack)
			return;

		structureManager.actionPerformer.StartAction(ActionPerformed.SimpleAttack, UnitSelected.gameObject, ActionTarget.GetComponent<Unit>().Movement.CurrentTile.gameObject);
		ResetGameState(true);
	}

    void ManageVictory()
    {
		bool isFightWon = UnitsOnField.Count(u => u.UnitData.Faction == ENEMY_FACTION) == 0;

        if (!isFightWon) return;

		isGameOver = true;
		int goldGenerated = GenerateAndAddGold();
		generalManager.SaveGameProgress(GeneralManager.GameStatus.Won);
		structureManager.GetGameScreen(GameScreens.FightVictoryScreen, goldGenerated);
	}

    bool ManageDefeat()
    {
		bool isFightLost = UnitsOnField.Count(u => u.UnitData.Faction == USER_FACTION) == 0;

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
        List<Tile> path = structureManager.FindPathToDestination(tileToMoveTo, false, attacker.Movement.CurrentTile.data.PositionOnGrid).ToList();

        int targetMovementTileIndex = path.Count - 1;
        if (targetMovementTileIndex < 0) targetMovementTileIndex = 0;

        Tile targetTile = attacker.Movement.CurrentTile;
        if(path.Count > 0)
            targetTile = structureManager.gameData.mapTiles[path[targetMovementTileIndex].data.PositionOnGrid];

		structureManager.MoveUnit(UnitSelected, targetTile, false);
		ActionInQueue = ActionPerformed.SimpleAttack;
        ActionTarget = defender.gameObject;
        UnitSelected = attacker;
        IsShowingPath = false;
        structureManager.ClearSelection(true);
    }

    public void HandleShowcase(Unit unitSelected, bool putOnLeftShowcase, bool clearLeftShowcase, Animation animation)
    {
        //For now we always clear right showcase, add parameters if needed to change
		structureManager.ClearShowcase(rightUnitShowcaseParent);

		if (clearLeftShowcase)
            structureManager.ClearShowcase(leftUnitShowcaseParent);

        Transform showcase = putOnLeftShowcase ? leftUnitShowcaseParent : rightUnitShowcaseParent;
        Transform position = putOnLeftShowcase ? leftUnitShowcasePosition : rightUnitShowcasePosition;
		structureManager.InstantiateShowcaseUnit(unitSelected, position, showcase, animation);
        HandleShowcaseAnimations(animation, true, true);
	}

    void HandleShowcaseAnimations(Animation animation, bool animateLeft, bool animateRight)
    {
		if (animateLeft && leftUnitShowcaseParent.childCount > 0)
		{
			GameObject unitOnLeft = leftUnitShowcaseParent.GetChild(0).gameObject;
			structureManager.StartShowcaseAnimation(unitOnLeft, animation);
		}

		if (animateRight && rightUnitShowcaseParent.childCount > 0)
		{
			GameObject unitOnRight = rightUnitShowcaseParent.GetChild(0).gameObject;
			structureManager.StartShowcaseAnimation(unitOnRight, animation);
		}
	}

    public void ClearShowcase()
    {
		structureManager.ClearShowcase(leftUnitShowcaseParent);
		structureManager.ClearShowcase(rightUnitShowcaseParent);
	}

    public void ResetGameState(bool resetUnitSelected)
    {
        if (resetUnitSelected) UnitSelected = null;
        IsShowingPath = false;
        structureManager.ClearSelection(true);
        structureManager.SetInfoPanel(false);
        ClearShowcase();
        ActionInQueue = ActionPerformed.Default;
        PossibleAttacks = new();
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

    public void ApplyDamage()
    {

    }

	#region Button Calls
	//Called by "End Turn" button of UI
	public void EndTurnButton(int faction)
	{
		FightManager fm = GameObject.Find(GeneralManager.FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
		fm.TurnManager.EndPhase(faction, fm.IsSetup);
	}

	public void MakeUnitTakeDamage()
	{
		structureManager.actionPerformer.StartTakeDamageAnimation();
	}

    public void UnitDies(Unit unit)
    {
        structureManager.FinishAnimation(unit.gameObject);
		structureManager.KillUnit(unit);
    }

	#endregion
}
public enum ActionPerformed
{
    FightMovement,
    RogueMovement,
    FightTeleport,
    SimpleAttack, //Attack that don't spawn anything else
    ProjectileAttack, //all attacks that spawns some kind of projectile or effect
    CameraFocus,
    Default,
}