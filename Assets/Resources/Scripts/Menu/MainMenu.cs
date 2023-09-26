using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu: MonoBehaviour
{
    [SerializeField]
    GameObject OptionPrefab;
    [SerializeField]
    GameObject NewGamePrefab;
    [SerializeField]
    Button ContinueText;
    [SerializeField]
    TextMeshProUGUI ContinueInfoText;

    private void Start()
	{
        bool isRunOngoing = PlayerPrefs.GetInt(GeneralManager.ONGOING_RUN) != 0;
        ContinueText.interactable = isRunOngoing;
        ContinueInfoText.text = isRunOngoing ? GetContinueInfoText() : string.Empty;
    }

    string GetContinueInfoText()
	{
        string godSelected = PlayerPrefs.GetString(GeneralManager.GOD_SELECTED_PP);
        string goldAccumulated = PlayerPrefs.GetInt(GeneralManager.GOLD).ToString();
        return $"{godSelected}: {goldAccumulated}g";
    }

	public void NewGame(){
        GameObject mainScene = GameObject.Find("Main");
        GeneratePrefab(NewGamePrefab, "NewGame");
        Destroy(mainScene);
    }

    public void ContinueRun(){
        SceneManager.LoadScene(1);
    }

    public void Options(){
        GameObject mainScene = GameObject.Find("Main");

        GeneratePrefab(OptionPrefab, "Options");
        Destroy(mainScene);
    }

    public static GameObject GeneratePrefab(GameObject prefab, string name){
        GameObject Options = Instantiate(prefab,new Vector3(600, 250, 0),Quaternion.identity);
        Options.name = name;
        return Options;
    }
}
