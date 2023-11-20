using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]
    GameObject MainPrefab;
    [SerializeField]
    TextMeshProUGUI SeedText;

    string optionSelected;
    string currentScene;

    void Start(){
        currentScene = SceneManager.GetActiveScene().name;
        if(currentScene != "Fight"){
            GameObject abandonRunButton = GameObject.Find("Abandon RUN");
            abandonRunButton.SetActive(false);
        }
        SeedText.text = PlayerPrefs.GetInt(GeneralManager.SEED).ToString();
    }

    void Update(){
        if(Input.anyKeyDown && Input.GetKeyDown(KeyCode.Escape))
            Back();
    }

    public void Back(){
        if(currentScene == "Main Menu"){
            GameObject mainScene = Instantiate(MainPrefab,new Vector3(600, 250, 0),Quaternion.identity);
            mainScene.name = "Main";
            return;
        }

        GeneralManager generalManager = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
        generalManager.IsOptionOpen = false;
        
        Destroy(this.gameObject);
    }

    public void AbandonRun(){
        GeneralManager.CloseRun();
    }

    public void ToMainMenu()
	{
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.MainMenu);
    }
}
