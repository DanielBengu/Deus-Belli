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
    GameObject NewGamePrefab;
    [SerializeField]
    GameObject ArchivePrefab;
    [SerializeField]
    Button ContinueText;
    [SerializeField]
    TextMeshProUGUI ContinueInfoText;

    GameObject currentSection;

    private void Start()
	{
        currentSection = transform.GetChild(0).gameObject;
        bool runIsOnGoing = PlayerPrefs.GetInt(GeneralManager.ONGOING_RUN) != 0;

        ContinueText.interactable = runIsOnGoing;
        if(runIsOnGoing)
            ContinueInfoText.text = GetContinueInfoText();
    }

    string GetContinueInfoText()
	{
        string godSelected = PlayerPrefs.GetString(GeneralManager.GOD_SELECTED_PP);
        string goldAccumulated = PlayerPrefs.GetInt(GeneralManager.GOLD).ToString();
        return $"{godSelected}: {goldAccumulated}g";
    }

    public void SwitchSection(GameObject prefabToInstantiate, Vector3 position)
	{
        Destroy(currentSection);
        currentSection = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);
    }

    #region Buttons
    public void NewGame()
    {
        SwitchSection(NewGamePrefab, new(100, -300, 0));
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
