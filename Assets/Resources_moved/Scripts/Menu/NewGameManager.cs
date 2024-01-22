using Assets.Resources_moved.Scripts.General.Classes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameManager : MonoBehaviour
{
    [SerializeField]
    Button startButton;
    [SerializeField]
    TMP_InputField seedInputField;

    [SerializeField]
    TextMeshProUGUI godData;
    [SerializeField]
    GameObject godPrefab;

    [SerializeField]
    GameObject ReligionsPrefab;
    [SerializeField]
    GameObject ReligionSelectedText;
    [SerializeField]
    GameObject godsInstance;

    private string godSelected;
    private Religion religionSelected;

	private void Update()
	{
        startButton.enabled = seedInputField.text.Length == 0 || seedInputField.text.Length > 2;
	}

    public void SelectReligion(string religion)
    {
        religionSelected = LoadReligion(religion);
        ReligionSelectedText.SetActive(true);
        ReligionSelectedText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = religionSelected.Name;
        ReligionsPrefab.SetActive(false);
        godsInstance.SetActive(true);
		LoadGods(religionSelected);
    }

    public Religion LoadReligion(string religion)
	{
        return FileManager.GetGodOfReligion(religion);
	}
    
    public void LoadGods(Religion religionSelected)
	{
        char[] Characters = new char[4] { 'A', 'B', 'C', 'D' };
        for(int i = 0; i < religionSelected.ListOfGods.Count && i < 4; i++)
			GameObject.Find($"Character {Characters[i]} Text").GetComponent<TextMeshProUGUI>().text = religionSelected.ListOfGods[i].Name;
    }

    public void SelectGod(int god)
	{
        God godSelectedFromList = religionSelected.ListOfGods[god];
		string godName = godSelectedFromList.Name.ToString();
        godSelected = godName;
        godData.text = $"{godSelectedFromList.BuffDescription}";

        for(int i = 0; i < godPrefab.transform.childCount; i++)
            Destroy(godPrefab.transform.GetChild(i).gameObject);

        GameObject prefab = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.God, godName);
        Instantiate(prefab, godPrefab.transform.position, prefab.transform.rotation, godPrefab.transform);
    }
    public void StartGame()
    {
        if (godSelected == null)
            return;

        int seed = seedInputField.text == string.Empty ? 0 : int.Parse(seedInputField.text);
        PlayerPrefs.SetInt(GeneralManager.SEED, seed);
        PlayerPrefs.SetString(GeneralManager.GOD_SELECTED_PP, godSelected);
        PlayerPrefs.SetInt(GeneralManager.ONGOING_RUN, 0);
        ScenesManager.LoadSceneAsync(ScenesManager.Scenes.Fight);
    }

    public enum Religions
	{
        Agbara,
        Beta,
        Charlie,
        Delta
	}
}
