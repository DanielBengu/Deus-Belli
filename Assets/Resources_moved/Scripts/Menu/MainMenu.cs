using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu: MonoBehaviour
{
    [SerializeField]
    GameObject OptionPrefab;
    [SerializeField]
    GameObject StartPrefab;
    [SerializeField]
    GameObject NewGameInstance;
    [SerializeField]
    GameObject ArchivePrefab;
    [SerializeField]
    Button ContinueText;
    [SerializeField]
    TooltipManager continueButtonTooltip;

    GameObject currentSection;

    private void Start()
	{
		currentSection = transform.GetChild(0).gameObject;
        bool runIsOnGoing = PlayerPrefs.GetInt(GeneralManager.ONGOING_RUN) != 0;

        ContinueText.interactable = runIsOnGoing;
        if (runIsOnGoing)
        {
			string godSelected = PlayerPrefs.GetString(GeneralManager.GOD_SELECTED_PP);
			string goldAccumulated = PlayerPrefs.GetInt(GeneralManager.GOLD).ToString();
			int completedRows = PlayerPrefs.GetInt(GeneralManager.CURRENT_ROW) + 1;
            continueButtonTooltip.enabled = true;
			continueButtonTooltip.header = godSelected;
            continueButtonTooltip.text = $"Turn {completedRows}\n{goldAccumulated}g";
		}
	}

    public void SwitchSection(GameObject prefabToInstantiate, Vector3 position)
	{
        currentSection.SetActive(false);
		prefabToInstantiate.SetActive(true);
        currentSection = prefabToInstantiate;
	}

    #region Buttons
    public void NewGame()
    {
        SwitchSection(NewGameInstance, new(100, -300, 0));
    }

    public void ContinueRun()
    {
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.Fight);
    }

    public void Custom()
    {
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.CustomCreator);
    }

    public void Options()
    {
        SwitchSection(OptionPrefab, new(0, 0, 0));
    }

    public void Archive()
    {
        SwitchSection(ArchivePrefab, new(0, 0, 0));
    }
    #endregion
}
