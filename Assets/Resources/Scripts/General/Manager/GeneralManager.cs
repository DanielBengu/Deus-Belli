using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralManager : MonoBehaviour
{
    public const string GOD_SELECTED_PP = "God Selected";
    public const string ONGOING_RUN = "OngoingRun";
    public const string CURRENT_ROW = "CurrentNode";
    public const string CURRENT_POSITION_IN_ROW = "CurrentPositionInRow";
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
    GameObject fightSectionPrefab;
    GameObject fightSectionInstance;
    FightManager fightManager;

    [SerializeField]
    GameObject rogueSectionPrefab;
    GameObject rogueSectionInstance;
    RogueManager rogueManager;

    [SerializeField]
    CameraManager cameraManager;
    [SerializeField]
    StructureManager structureManager;

    [SerializeField]
    GameObject OptionsPrefab;

    RunData runData;
    public RogueNode selectedNode;

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsScrollButtonDown { get; set; }
    public bool IsOptionOpen { get; set; }
    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }
	public int Gold { get { return runData.gold; } set { runData.gold = value; } }
    public string GodSelected { get { return runData.godSelected; } set { runData.godSelected = value; } }
	public int CurrentRow { get { return runData.currentRow; } set { runData.currentRow = value; } }
	public int CurrentPositionInRow { get { return runData.currentPositionInRow; } set { runData.currentPositionInRow = value; } }

	private void Start()
	{
        bool isOngoingRun = PlayerPrefs.GetInt(ONGOING_RUN) != 0;
        if (isOngoingRun)
            runData = LoadRunData();
		else
		{
            string godSelected = PlayerPrefs.GetString(GOD_SELECTED_PP);
            int masterSeed = Guid.NewGuid().GetHashCode();
            runData = new RunData(godSelected, 0, 1, masterSeed, 0);

            PlayerPrefs.SetInt(SEED, masterSeed);
            PlayerPrefs.SetInt(GOLD, 0);
            PlayerPrefs.SetInt(CURRENT_ROW, 0);
            PlayerPrefs.SetInt(CURRENT_POSITION_IN_ROW, 1);
            PlayerPrefs.SetInt(ONGOING_RUN, 1);
        }

        GenerateRogueSection();
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
            cameraManager.UpdatePosition(rogueSectionInstance.transform);
            rogueManager.GenerateNewNodeLines();
        }
            
    }

    RunData LoadRunData()
	{
        int masterSeed = PlayerPrefs.GetInt(SEED);
        string godSelected = PlayerPrefs.GetString(GOD_SELECTED_PP);
        int currentRow = PlayerPrefs.GetInt(CURRENT_ROW);
        int currentPositionInRow = PlayerPrefs.GetInt(CURRENT_POSITION_IN_ROW);
        int gold = PlayerPrefs.GetInt(GOLD);
        return new RunData(godSelected, currentRow, currentPositionInRow, masterSeed, gold);
	}

	public void SaveMapProgress()
	{
        PlayerPrefs.SetInt(CURRENT_ROW, runData.currentRow);
        PlayerPrefs.SetInt(CURRENT_POSITION_IN_ROW, runData.currentPositionInRow);
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

    public void StartFight()
	{
        cameraManager.ResetCamera();

        DestroyRogueSection();
        GenerateFightSection(selectedNode.level);

        currentSection = CurrentSection.Fight;
    }

    public void StartMerchant()
	{
        cameraManager.ResetCamera();
        DestroyRogueSection();
        GenerateMerchantSection(selectedNode);

        currentSection = CurrentSection.Encounter;
    }

    public void StartEvent()
	{
        cameraManager.ResetCamera();

        GenerateEventSection(selectedNode);

        currentSection = CurrentSection.Encounter;
    }

    void DestroyFightSection()
	{
        fightManager.DisableFightSection();
        Destroy(fightSectionInstance);
    }

    void DestroyRogueSection()
    {
        runData = new(GodSelected, CurrentRow, CurrentPositionInRow, rogueManager.seedList[RogueManager.SeedType.Master], Gold);
        Destroy(rogueSectionInstance);
    }

    public void DestroyEventSection()
	{
        Destroy(eventSectionInstance);
    }

    void GenerateFightSection(Level level)
	{
        fightSectionInstance = Instantiate(fightSectionPrefab);
        fightManager = GameObject.Find(FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
        fightManager.structureManager = structureManager;
        fightManager.cameraManager = cameraManager;
        fightManager.generalManager = this;
        fightManager.structureManager.uiManager.SetFightVariables();
        fightManager.structureManager.spriteManager.fightManager = fightManager;
        fightManager.Setup(level);
	}

    void GenerateMerchantSection(RogueNode node)
    {

    }

    void GenerateEventSection(RogueNode node)
    {
        eventSectionInstance = Instantiate(eventSectionPrefab);
    }

    void GenerateRogueSection()
    {
        rogueSectionInstance = Instantiate(rogueSectionPrefab);
        rogueManager = GameObject.Find(ROGUE_MANAGER_OBJ_NAME).GetComponent<RogueManager>();
        rogueManager.SetupRogue(structureManager, runData.currentRow, runData.currentPositionInRow, runData.masterSeed);
        rogueManager.StructureManager.uiManager.SetRogueVariables(Gold, GodSelected);
        currentSection = CurrentSection.Rogue;

        rogueManager.IsRunCompleted();
    }

    public static void CloseRun()
	{
        PlayerPrefs.SetInt(ONGOING_RUN, 0);
        SceneManager.LoadScene(0);
    }

    public void ReturnToRogueFromFightButton()
	{
        cameraManager.ResetCamera();

        selectedNode = null;
        SaveMapProgress();
        DestroyFightSection();
        GenerateRogueSection();
    }
    public void ReturnToRogueFromEventButton()
    {
        cameraManager.ResetCamera();

        selectedNode = null;
        SaveMapProgress();
        DestroyEventSection();
    }
    struct RunData
    {
        public string godSelected;
        public int currentRow;
        public int currentPositionInRow;
        public int masterSeed;

        public int gold;

        public RunData(string godSelected,int currentRow, int currentPositionInRow, int masterSeed, int gold)
        {
            this.godSelected = godSelected;
            this.currentRow = currentRow;
            this.currentPositionInRow = currentPositionInRow;
            this.masterSeed = masterSeed;
            this.gold = gold;
        }
    }

    enum CurrentSection
	{
        Fight,
        Rogue,
        Encounter,
	}
}
