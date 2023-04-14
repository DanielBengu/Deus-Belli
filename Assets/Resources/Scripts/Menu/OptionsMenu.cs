using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
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
            GameObject mainScene = GameObject.Instantiate(MainPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
            mainScene.name = "Main";
        }

        if(currentScene == "Fight"){
            FightManager fightManager = GameObject.Find("Manager").GetComponent<FightManager>();
            fightManager.IsOptionOpen = false;
        }

        GameObject optionsScene = GameObject.Find("Options");
        Object.Destroy(optionsScene);
    }

    void VideoOptions(){
        DestroySubOption();
            
        GameObject videoSettings = GameObject.Instantiate(VideoSettingsPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        videoSettings.name = "Video";
        optionSelected = "Video";
    }

    void SoundOptions(){
        DestroySubOption();

        GameObject soundSettings = GameObject.Instantiate(SoundSettingsPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        soundSettings.name = "Sound";
        optionSelected = "Sound";
    }

    void DestroySubOption(){
        if(optionSelected != null){
            GameObject subOption = GameObject.Find(optionSelected);
            Object.Destroy(subOption);
        }
    }

    void AbandonRun(){

    }
}
