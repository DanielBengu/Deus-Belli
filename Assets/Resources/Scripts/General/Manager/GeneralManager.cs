using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralManager : MonoBehaviour
{
    public const string GOD_SELECTED_PP = "God Selected";
    public const string ONGOING_RUN = "OngoingRun";
    public const string CURRENT_ROW = "CurrentNode";
    public const string CURRENT_POSITION_IN_ROW = "CurrentPositionInRow";
    public const string GAME_STATUS = "GameStatus";
    public const string SEED = "Seed";
    public const string GOLD = "Gold";

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
    FileManager fileManager;

    [SerializeField]
    GameObject OptionsPrefab;

    public RunData runData;
    public RogueNode selectedNode;

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsScrollButtonDown { get; set; }
    public bool IsOptionOpen { get; set; }
    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }
	public int Gold { get { return runData.gold; } set { runData.gold = value; } }
    public string GodSelected { get { return runData.godSelected; } set { runData.godSelected = value; } }
	public int CurrentRow { get { return runData.currentRow; } set { runData.currentRow = value; } }
	public int CurrentPositionInRow { get { return runData.currentPositionInRow; } set { runData.currentPositionInRow = value; } }
    public List<Unit> PlayerUnits { get { return runData.unitList; } }


	private void Start()
	{
        bool isOngoingRun = PlayerPrefs.GetInt(ONGOING_RUN) != 0;
        if (isOngoingRun)
            runData = LoadRunData();
		else
		{
            string godSelected = PlayerPrefs.GetString(GOD_SELECTED_PP);
            int optionalSeed = PlayerPrefs.GetInt(SEED);
            int masterSeed = optionalSeed > 0 ? optionalSeed : Math.Abs(Guid.NewGuid().GetHashCode());
            List<Unit> startingUnits = FileManager.GetUnits(FileManager.DataSource.PlayerUnits);
            runData = new RunData(godSelected, 0, 1, masterSeed, 0, startingUnits);

            PlayerPrefs.SetInt(SEED, masterSeed);
            PlayerPrefs.SetInt(GOLD, 0);
            PlayerPrefs.SetInt(CURRENT_ROW, 0);
            PlayerPrefs.SetInt(CURRENT_POSITION_IN_ROW, 1);
            PlayerPrefs.SetInt(ONGOING_RUN, 1);
        }

        GenerateRogueSection(false);
    }

	private void Update()
	{
        if (!IsGameInStandby && (Input.anyKeyDown || IsScrollButtonDown))
            ManageKeysDown();

        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelInput != 0f)
            switch (currentSection)
            {
                case CurrentSection.Fight:
                    cameraManager.ScrollWheel(scrollWheelInput);
                    break;
                case CurrentSection.Rogue:
                    cameraManager.ScrollWheel(scrollWheelInput, rogueSectionInstance.transform);
                    rogueManager.GenerateNewNodeLines();
                    break;
            }

        if (IsScrollButtonDown)
		{
            
            if(currentSection == CurrentSection.Rogue)
			{
                cameraManager.UpdatePosition(rogueSectionInstance.transform);
                rogueManager.GenerateNewNodeLines();
            }
        }
            
    }

    RunData LoadRunData()
	{
        int masterSeed = PlayerPrefs.GetInt(SEED);
        string godSelected = PlayerPrefs.GetString(GOD_SELECTED_PP);
        int currentRow = PlayerPrefs.GetInt(CURRENT_ROW);
        int currentPositionInRow = PlayerPrefs.GetInt(CURRENT_POSITION_IN_ROW);
        int gold = PlayerPrefs.GetInt(GOLD);
        List<Unit> unitList = FileManager.GetUnits(FileManager.DataSource.PlayerUnits);
        return new RunData(godSelected, currentRow, currentPositionInRow, masterSeed, gold, unitList);
	}

	public void SaveGameProgress(GameStatus status)
	{
        PlayerPrefs.SetInt(GOLD, runData.gold);
        PlayerPrefs.SetInt(CURRENT_ROW, runData.currentRow);
        PlayerPrefs.SetInt(CURRENT_POSITION_IN_ROW, runData.currentPositionInRow);
        PlayerPrefs.SetInt(GAME_STATUS, (int)status);
        FileManager.SaveUnits(runData.unitList.Select(u => u.gameObject).ToList(), true);
    }

    void ManageKeysDown()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			if (!IsOptionOpen)
			{
                MainMenu.GeneratePrefab(OptionsPrefab, "Options");
                IsOptionOpen = true;
            }
        } else
        if (Input.GetMouseButtonDown(SCROLL_WHEEL_BUTTON))
        {
            IsScrollButtonDown = true;
        }
        else if (Input.GetMouseButtonUp(SCROLL_WHEEL_BUTTON))
        {
            IsScrollButtonDown = false;
        }
    }

    bool IsGameInStandbyMethod()
	{
		return currentSection switch
		{
			CurrentSection.Fight => fightManager.IsAnyUnitMoving || IsOptionOpen || fightManager.CurrentTurn != 0 || fightManager.isGameOver,
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
        runData = new(GodSelected, CurrentRow, CurrentPositionInRow, rogueManager.seedList[RogueManager.SeedType.Master], Gold, PlayerUnits);
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
                fightManager.structureManager = structureManager;
                fightManager.cameraManager = cameraManager;
                fightManager.generalManager = this;
                fightManager.structureManager.uiManager.SetFightVariables();
                fightManager.structureManager.spriteManager.fightManager = fightManager;
                fightManager.Setup(node.level);
                break;
			case RogueTileType.Event:
                eventSectionInstance = Instantiate(eventSectionPrefab);
                rogueManager.StructureManager.uiManager.SetEventVariables(node.currentEvent);
                rogueManager.CurrentEvent = node.currentEvent;
                break;
            case RogueTileType.Merchant:
                merchantSectionInstance = Instantiate(merchantSectionPrefab);
                rogueManager.StructureManager.uiManager.SetMerchantVariables(runData.gold);
                rogueManager.StructureManager.InstantiateMerchantItems(node.merchant.ItemList);
                rogueManager.MerchantShop = node.merchant;
                break;
		}
	}

    void GenerateRogueSection(bool isDefeat)
    {
        rogueSectionInstance = Instantiate(rogueSectionPrefab);
        rogueManager = GameObject.Find(ROGUE_MANAGER_OBJ_NAME).GetComponent<RogueManager>();
        rogueManager.SetupRogue(structureManager, runData.currentRow, runData.currentPositionInRow, runData.masterSeed);
        rogueManager.StructureManager.uiManager.SetRogueVariables(Gold, GodSelected, runData.masterSeed);
        currentSection = CurrentSection.Rogue;

        rogueManager.IsRunCompleted(isDefeat);
    }

    public static void CloseRun()
	{
        PlayerPrefs.SetInt(ONGOING_RUN, 0);
        SceneManager.LoadScene(0);
    }

    public void ReturnToRogue(RogueTileType tileTypeReturning, bool isDefeat)
	{
        cameraManager.ResetCamera();

        selectedNode = null;
        SaveGameProgress(GameStatus.Current);

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
                structureManager.uiManager.SetRogueVariables(Gold, GodSelected, runData.masterSeed);
                Destroy(merchantSectionInstance);
                break;
			case RogueTileType.Event:
                structureManager.uiManager.SetRogueVariables(Gold, GodSelected, runData.masterSeed);
                Destroy(eventSectionInstance);
                break;
		}
    }

    public struct RunData
    {
        public string godSelected;
        public int currentRow;
        public int currentPositionInRow;
        public int masterSeed;

        public int gold;
        public List<Unit> unitList;

        public RunData(string godSelected,int currentRow, int currentPositionInRow, int masterSeed, int gold, List<Unit> unitList)
        {
            this.godSelected = godSelected;
            this.currentRow = currentRow;
            this.currentPositionInRow = currentPositionInRow;
            this.masterSeed = masterSeed;
            this.gold = gold;
            this.unitList = unitList;
        }
    }

    public enum CurrentSection
	{
        Fight,
        Rogue,
	}

    public enum GameStatus
    {
        Current,
        Won,
        Lost
    }
}
