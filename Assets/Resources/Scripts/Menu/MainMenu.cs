using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu: MonoBehaviour
{
    [SerializeField]
    GameObject OptionPrefab;
    [SerializeField]
    GameObject NewGamePrefab;

    public void NewGame(){
        GameObject mainScene = GameObject.Find("Main");
        GeneratePrefab(NewGamePrefab, "NewGame");
        Object.Destroy(mainScene);
    }

    public void LoadGame(){
        SceneManager.LoadScene(1);
    }

    public void Options(){
        GameObject mainScene = GameObject.Find("Main");

        GeneratePrefab(OptionPrefab, "Options");
        Object.Destroy(mainScene);
    }

    public static GameObject GeneratePrefab(GameObject prefab, string name){
        GameObject Options = GameObject.Instantiate(prefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        Options.name = name;
        return Options;
    }
}
