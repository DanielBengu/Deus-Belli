using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]
    GameObject OptionsPrefab;
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

    void Back(){
        DestroySubOption();

        if(currentScene == "Main Menu"){
            GameObject mainScene = Instantiate(MainPrefab,new Vector3(600, 250, 0),Quaternion.identity);
            mainScene.name = "Main";
        }

        if(currentScene == "Fight"){
            GeneralManager generalManager = GameObject.Find(GeneralManager.GENERAL_MANAGER_OBJ_NAME).GetComponent<GeneralManager>();
            generalManager.IsOptionOpen = false;
        }

        Destroy(OptionsPrefab);
    }

    void DestroySubOption(){
        if(optionSelected != null){
            GameObject subOption = GameObject.Find(optionSelected);
            Destroy(subOption);
        }
    }

    void AbandonRun(){
        GeneralManager.CloseRun();
    }

    public void ToMainMenu()
	{
        SceneManager.LoadScene(0);
    }
}
