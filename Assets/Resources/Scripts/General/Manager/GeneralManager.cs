using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralManager : MonoBehaviour
{
    public const int SCROLL_WHEEL_BUTTON = 2;
    public const string FIGHT_MANAGER_OBJ_NAME = "Fight Manager";
    public const string ROGUE_MANAGER_OBJ_NAME = "Rogue Manager";
    public const string GENERAL_MANAGER_OBJ_NAME = "General Manager";

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

    CurrentSection currentSection = CurrentSection.Rogue;

    public bool IsScrollButtonDown { get; set; }
    public bool IsOptionOpen { get; set; }
    public bool IsGameInStandby { get { return IsGameInStandbyMethod(); } }
	public int Gold { get { return runData.gold; } set { runData.gold = value; } }

	private void Start()
	{
        bool isOngoingRun = PlayerPrefs.GetInt("OngoingRun") != 0;
        if (isOngoingRun)
            runData = LoadRunData();
		else
		{
            int baseSeed = Random.Range(0, 1000);
            PRNG seed = new(baseSeed);
            runData = new RunData(0, seed, 0, 0);

            PlayerPrefs.SetInt("Seed", baseSeed);
            PlayerPrefs.SetInt("Gold", 0);
            PlayerPrefs.SetInt("CurrentNode", 0);
            PlayerPrefs.SetFloat("PlayerX", 0);
            PlayerPrefs.SetInt("OngoingRun", 1);
        }

        GenerateRogueSection();
    }

	private void Update()
	{
        if (!IsGameInStandby && (Input.anyKeyDown || IsScrollButtonDown))
            ManageKeysDown();

        if (IsScrollButtonDown)
            cameraManager.UpdatePosition();
    }

    RunData LoadRunData()
	{
        PRNG seed = new(PlayerPrefs.GetInt("Seed"));
        int currentNode = PlayerPrefs.GetInt("CurrentNode");
        float playerX = PlayerPrefs.GetFloat("PlayerX");
        int gold = PlayerPrefs.GetInt("Gold");
        return new RunData(currentNode, seed, playerX, gold);
	}

	public void SaveMapProgress()
	{
        PlayerPrefs.SetInt("CurrentNode", runData.currentNode);
        PlayerPrefs.SetFloat("PlayerX", runData.playerX);
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

        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelInput != 0f)
            cameraManager.ScrollWheel(scrollWheelInput);
    }

    bool IsGameInStandbyMethod()
	{
		return currentSection switch
		{
			CurrentSection.Fight => fightManager.IsAnyUnitMoving || IsOptionOpen || fightManager.CurrentTurn != 0 || fightManager.isGameOver,
			CurrentSection.Rogue => rogueManager.IsAnyUnitMoving || IsOptionOpen,
			_ => false,
		};
	}

    public void StartFight()
	{
        DestroyRogueSection();
        GenerateFightSection();

        currentSection = CurrentSection.Fight;
    }

    void DestroyFightSection()
	{
        fightManager.DisableFightSection();
        Destroy(fightSectionInstance);
    }
    void DestroyRogueSection()
    {
        runData = new(rogueManager.currentNode, rogueManager.seed, rogueManager.playerUnitTransform.position.x, Gold);
        rogueManager.DisableRogueSection();
        Destroy(rogueSectionInstance);
    }

    void GenerateFightSection()
	{
        fightSectionInstance = Instantiate(fightSectionPrefab);
        fightManager = GameObject.Find(FIGHT_MANAGER_OBJ_NAME).GetComponent<FightManager>();
        fightManager.structureManager = structureManager;
        fightManager.cameraManager = cameraManager;
        fightManager.generalManager = this;
        fightManager.structureManager.uiManager.SetFightVariables();
        fightManager.structureManager.spriteManager.fightManager = fightManager;
	}

    void GenerateRogueSection()
    {
        rogueSectionInstance = Instantiate(rogueSectionPrefab);
        rogueManager = GameObject.Find(ROGUE_MANAGER_OBJ_NAME).GetComponent<RogueManager>();
        rogueManager.SetupRogue(structureManager, runData.currentNode, runData.seed, runData.playerX);
        rogueManager.structureManager.uiManager.SetRogueVariables(Gold);
        currentSection = CurrentSection.Rogue;

        rogueManager.IsRunCompleted();
    }

    public static void AbandonRun()
	{
        PlayerPrefs.SetInt("OngoingRun", 0);
        SceneManager.LoadScene(0);
    }

    public void ReturnToRogueFromFightButton()
	{
        DestroyFightSection();
        GenerateRogueSection();
    }
    struct RunData
    {
        public int currentNode;
        public PRNG seed;
        public float playerX;

        public int gold;

        public RunData(int currentNode, PRNG seed, float playerX, int gold)
        {
            this.currentNode = currentNode;
            this.seed = seed;
            this.playerX = playerX;
            this.gold = gold;
        }
    }

    enum CurrentSection
	{
        Fight,
        Rogue
	}
}
