using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class GeneralManager : MonoBehaviour
{
    public const string GOD_SELECTED_PP = "God Selected";
    public const string ONGOING_RUN = "OngoingRun";
    public const string CURRENT_ROW = "CurrentNode";
    public const string CURRENT_POSITION_IN_ROW = "CurrentPositionInRow";
    public const string GAME_STATUS = "GameStatus";
    public const string SEED = "Seed";
    public const string GOLD = "Gold";
    public const string DIFFICULTY = "Difficulty";

    public const int SCROLL_WHEEL_BUTTON = 2;
    public const string FIGHT_MANAGER_OBJ_NAME = "Fight Manager";
    public const string ROGUE_MANAGER_OBJ_NAME = "Rogue Manager";
    public const string GENERAL_MANAGER_OBJ_NAME = "General Manager";

    [SerializeField]
    GameObject eventSectionPrefab;
    GameObject eventSectionInstance;

    [SerializeField]
    GameObject merchantSectionPrefab;
    GameObject merchantSectionInstance;

    [SerializeField]
    GameObject fightSectionPrefab;
    GameObject fightSectionInstance;
    FightManager fightManager;

    [SerializeField]
    GameObject rogueSectionPrefab;
    GameObject rogueSectionInstance;
    public RogueManager rogueManager;

    [SerializeField]
    CameraManager cameraManager;
    [SerializeField]
    StructureManager structureManager;

    [SerializeField]
    GameObject OptionsPrefab;
    [SerializeField]
    Transform OptionsParent;

    public RunData runData;
    public RogueNode selectedNode;

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsScrollButtonDown { get; set; }
    public bool IsOptionOpen { get; set; }
    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }
	public int Gold { get { return runData.gold; } set { runData.gold = value; } }
    public string GodSelected { get { return runData.god; } set { runData.god = value; } }
	public string ReligionSelected { get { return runData.religion; } set { runData.religion = value; } }
	public int CurrentRow { get { return runData.currentRow; } set { runData.currentRow = value; } }
	public int CurrentPositionInRow { get { return runData.currentPositionInRow; } set { runData.currentPositionInRow = value; } }
	public int Difficulty { get { return runData.difficulty; } set { runData.difficulty = value; } }
    public List<UnitData> PlayerUnits { get { return runData.unitList; } }


	private void Start()
	{
        if (PlayerPrefs.GetInt(ONGOING_RUN) != 0)
            LoadRun();
        else
            StartRun();

        LoadRogueStructure();

        StartEnemyGodIntro();
    }

	private void Update()
	{
        if (IsGameInStandby)
            return;

        if (Input.anyKeyDown || IsScrollButtonDown)
            ManageKeysDown();

        switch (currentSection)
        {
            case CurrentSection.Fight:
				HandleFightSectionUpdate();
				break;
            case CurrentSection.Rogue:
				//HandleRogueSectionUpdate();
				break;
            case CurrentSection.Custom:
            default:
                break;
        }
    }

    void LoadRogueStructure()
    {
        GenerateRogueSection(false);
        GenerateEnemyGod();
    }

    void GenerateEnemyGod()
    {
        GameObject godSpawnPosition = GameObject.Find("EnemyGodPosition");
        GameObject prefab = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.God, runData.god);
        var godInstance = Instantiate(prefab, godSpawnPosition.transform.position, prefab.transform.rotation, godSpawnPosition.transform);
        godInstance.transform.localScale = new(35, 35, 35);
    }

    void LoadRun()
    {
		runData = LoadRunData();
	}

    void StartRun()
    {
		SaveData saveData = FileManager.GetFileFromJSON<SaveData>(FileManager.SAVEDATA_PATH);

		runData = new RunData(saveData.Religion, saveData.God, saveData.CurrentRow, saveData.CurrentPositionInRow, saveData.Seed, saveData.Gold, saveData.UnitList.unitList.ToList(), saveData.Ascension);
		PlayerPrefs.SetInt(ONGOING_RUN, 1);
	}

    void StartEnemyGodIntro()
    {

    }

    void HandleFightSectionUpdate()
    {
		float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
		if (scrollWheelInput != 0f)
			cameraManager.HandleScroll(scrollWheelInput, cameraManager.transform, currentSection, -1);

		CameraManager.UpdatePositionOrRotation(fightManager.fightObjects, currentSection, new(structureManager.gameData.GetMapBounds()) { Rotator = fightManager.fightObjectsRotator, Tiles = fightManager.TileList });
	}

	RunData LoadRunData()
	{
        SaveData saveData = FileManager.GetFileFromJSON<SaveData>(FileManager.SAVEDATA_PATH);
        List<UnitData> unitList = saveData.UnitList.unitList.ToList();
        return new RunData(saveData.Religion, saveData.God, saveData.CurrentRow, saveData.CurrentPositionInRow, saveData.Seed, saveData.Gold, unitList, saveData.Ascension);
	}

	public void SaveGameProgress()
	{
        SaveManager.SaveGameProgress(runData);
    }

    void ManageKeysDown()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			if (!IsOptionOpen)
			{
                Transform options = GameObject.Find("OptionsSection").transform;
                GameObject Options = Instantiate(OptionsPrefab, options);
                Options.name = "Options";
                IsOptionOpen = true;
            }
        } else if (Input.GetMouseButtonDown(SCROLL_WHEEL_BUTTON))
        {
            IsScrollButtonDown = true;
        } else if (Input.GetMouseButtonUp(SCROLL_WHEEL_BUTTON))
        {
            IsScrollButtonDown = false;
        } else if (Input.GetKeyDown(KeyCode.P))
        {
            rogueManager.ObjectMoving = RogueManager.RogueObjectMoving.Camera;
            cameraManager.CameraFocus(structureManager, currentSection);
		}
    }

    bool IsGameInStandbyMethod()
	{
		return currentSection switch
		{
			CurrentSection.Fight => fightManager.IsAnyUnitMoving || IsOptionOpen || fightManager.CurrentTurn != 0 || fightManager.isGameOver || structureManager.ObjectsAnimating.Count > 0,
			CurrentSection.Rogue => rogueManager.IsAnyUnitMoving || rogueManager.IsGameOver || IsOptionOpen,
			_ => false,
		};
	}

    public void StartSection(RogueTileType currentSectionType)
	{
        bool isFightEncounter = selectedNode.IsFightEncounter();
        currentSection = isFightEncounter ? CurrentSection.Fight : CurrentSection.Rogue;
        cameraManager.ResetCamera();
        GenerateSection(currentSectionType, selectedNode);
        if (isFightEncounter)
            DestroyRogueSection();
    }

    void DestroyRogueSection()
    {
        runData = new(ReligionSelected, GodSelected, CurrentRow, CurrentPositionInRow, rogueManager.seedList[RogueManager.SeedType.Master], Gold, PlayerUnits, Difficulty);
        Destroy(rogueSectionInstance);
    }

    void GenerateSection(RogueTileType sectionToGenerate, RogueNode node = null)
	{
		switch (sectionToGenerate)
		{
            case RogueTileType.Boss:
            case RogueTileType.Miniboss:
			case RogueTileType.Fight:
                fightSectionInstance = Instantiate(fightSectionPrefab);
                fightManager = GameObject.Find(FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
                fightManager.Setup(node.level, structureManager, cameraManager, this);
                break;
			case RogueTileType.Event:
                eventSectionInstance = Instantiate(eventSectionPrefab);
                rogueManager.StructureManager.uiManager.SetEventVariables(node.currentEvent);
                rogueManager.CurrentEvent = node.currentEvent;
                break;
            case RogueTileType.Merchant:
                merchantSectionInstance = Instantiate(merchantSectionPrefab);
                rogueManager.StructureManager.uiManager.SetMerchantVariables();//runData.gold
				rogueManager.StructureManager.InstantiateMerchantItems(node.merchant.ItemList);
                rogueManager.MerchantShop = node.merchant;
                break;
		}
	}

    void GenerateRogueSection(bool isDefeat)
    {
        rogueSectionInstance = Instantiate(rogueSectionPrefab);
        rogueManager = rogueSectionInstance.transform.Find(ROGUE_MANAGER_OBJ_NAME).GetComponent<RogueManager>();

        rogueManager.SetupRogue(structureManager, this, runData.currentRow, runData.currentPositionInRow, runData.masterSeed);
        rogueManager.IsRunCompleted(isDefeat);

        Vector3 playerUnit = rogueManager.playerUnitTransform.position;
        cameraManager.rogueCameraPositionOnFocus = new(playerUnit.x, playerUnit.y + 100, playerUnit.z);
        cameraManager.SetupCameraToRogueOOF();

        currentSection = CurrentSection.Rogue;
    }

    public void CompleteRun()
	{
        AchievementManager.CompleteRun("Agbara");
	}

    public static void CloseRun()
	{
        PlayerPrefs.SetInt(ONGOING_RUN, 0);
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }

    public void ReturnToRogue(RogueTileType tileTypeReturning, bool isDefeat)
	{
        selectedNode = null;
        SaveGameProgress();

        switch (tileTypeReturning)
		{
            case RogueTileType.Miniboss:
            case RogueTileType.Boss:
            case RogueTileType.Fight:
                fightManager.DisableFightSection();
                Destroy(fightSectionInstance);
                GenerateRogueSection(isDefeat);
                break;
			case RogueTileType.Merchant:
                structureManager.uiManager.SetRogueVariables(Gold, GodSelected.ToString());
                Destroy(merchantSectionInstance);
                break;
			case RogueTileType.Event:
                structureManager.uiManager.SetRogueVariables(Gold, GodSelected.ToString());
                Destroy(eventSectionInstance);
                break;
		}
    }

    public struct RunData
    {
        public string religion;
        public string god;
        public int currentRow;
        public int currentPositionInRow;
        public int masterSeed;
        public int difficulty;

        public int gold;
        public List<UnitData> unitList;

        public RunData(string religion, string god,int currentRow, int currentPositionInRow, int masterSeed, int gold, List<UnitData> unitList, int difficulty)
        {
            this.religion = religion;
            this.god = god;
            this.currentRow = currentRow;
            this.currentPositionInRow = currentPositionInRow;
            this.masterSeed = masterSeed;
            this.gold = gold;
            this.unitList = unitList;
            this.difficulty = difficulty;
        }
    }

    public enum CurrentSection
	{
        Fight,
        Rogue,
        Custom
	}

    public enum GameStatus
    {
        Current,
        Won,
        Lost
    }
}
