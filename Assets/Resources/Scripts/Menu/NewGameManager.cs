using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameManager : MonoBehaviour
{
    [SerializeField]
    GameObject ZeusPrefab;
    [SerializeField]
    GameObject PoseidonPrefab;
    [SerializeField]
    GameObject HadesPrefab;

    [SerializeField]
    Button startButton;
    [SerializeField]
    TMP_InputField seedInputField;

    private string godSelected;

	private void Update()
	{
        startButton.enabled = seedInputField.text.Length == 0 || seedInputField.text.Length > 2;
	}

	public void ZeusSelected(){
        DestroyGod();
        GameObject god = Instantiate(ZeusPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        god.name = "Zeus";
        godSelected = "Zeus";
    }

    public void PoseidonSelected(){
        DestroyGod();
        GameObject god = Instantiate(PoseidonPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        god.name = "Poseidon";
        godSelected = "Poseidon";
    }

    public void HadesSelected(){
        DestroyGod();
        GameObject god = Instantiate(HadesPrefab,new Vector3(600, 250, 0),Quaternion.identity) as GameObject;
        god.name = "Hades";
        godSelected = "Hades";
    }

    public void StartGame(){
        if(godSelected != null){
            PlayerPrefs.SetString(GeneralManager.GOD_SELECTED_PP, godSelected);
            PlayerPrefs.SetInt(GeneralManager.ONGOING_RUN, 0);
            SceneManager.LoadScene(1);
        }
    }

    public void DestroyGod(){
        if(godSelected != null){
            GameObject god = GameObject.Find(godSelected);
            Destroy(god);
        }
    }
}
