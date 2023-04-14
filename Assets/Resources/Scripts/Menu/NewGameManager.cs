using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameManager : MonoBehaviour
{
    [SerializeField]
    GameObject ZeusPrefab;
    [SerializeField]
    GameObject PoseidonPrefab;
    [SerializeField]
    GameObject HadesPrefab;

    private string godSelected;

    public void ZeusSelected(){
        DestroyGod();
        GameObject god = GameObject.Instantiate(ZeusPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        god.name = "Zeus";
        godSelected = "Zeus";
    }

    public void PoseidonSelected(){
        DestroyGod();
        GameObject god = GameObject.Instantiate(PoseidonPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        god.name = "Poseidon";
        godSelected = "Poseidon";
    }

    public void HadesSelected(){
        DestroyGod();
        GameObject god = GameObject.Instantiate(HadesPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        god.name = "Hades";
        godSelected = "Hades";
    }

    public void StartGame(){
        if(godSelected != null){
            PlayerPrefs.SetString("God Selected", godSelected);
            SceneManager.LoadScene(1);
        }
    }

    public void DestroyGod(){
        if(godSelected != null){
            GameObject god = GameObject.Find(godSelected);
            Object.Destroy(god);
        }
    }
}
