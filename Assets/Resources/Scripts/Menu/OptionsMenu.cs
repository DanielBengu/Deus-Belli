using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]
    GameObject OptionsPrefab;
    [SerializeField]
    GameObject MainPrefab;
    [SerializeField]
    GameObject VideoSettingsPrefab;
    [SerializeField]
    GameObject SoundSettingsPrefab;

    string optionSelected;
    string currentScene;

    void Start(){
        currentScene = SceneManager.GetActiveScene().name;
        if(currentScene != "Fight"){
            GameObject abandonRunButton = GameObject.Find("Abandon RUN");
            abandonRunButton.SetActive(false);
        }
    }

    void Update(){
        if(Input.anyKeyDown){
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Back();
            }
        }
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

    void VideoOptions(){
        DestroySubOption();
            
        GameObject videoSettings = Instantiate(VideoSettingsPrefab,new Vector3(600, 250, 0),Quaternion.identity);
        videoSettings.name = "Video";
        optionSelected = "Video";
    }

    void SoundOptions(){
        DestroySubOption();

        GameObject soundSettings = Instantiate(SoundSettingsPrefab,new Vector3(600, 250, 0),Quaternion.identity);
        soundSettings.name = "Sound";
        optionSelected = "Sound";
    }

    void DestroySubOption(){
        if(optionSelected != null){
            GameObject subOption = GameObject.Find(optionSelected);
            Destroy(subOption);
        }
    }

    void AbandonRun(){
        GeneralManager.AbandonRun();
    }

    public void ToMainMenu()
	{
        SceneManager.LoadScene(0);
    }
}
