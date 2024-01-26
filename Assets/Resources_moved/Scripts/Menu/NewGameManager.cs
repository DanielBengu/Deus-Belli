using Assets.Resources_moved.Scripts.General.Classes;
using System;
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

    private God godSelected;
    private Religion religionSelected;

	private void Update()
	{
        startButton.enabled = seedInputField.text.Length == 0 || seedInputField.text.Length > 2;
	}

    public void ResetNewGameMenu()
    {
		ClearGodShowcase();
		ReligionsPrefab.SetActive(true);
		godsInstance.SetActive(false);
		ReligionSelectedText.SetActive(false);
	}

    public void BackButton()
    {
        ResetNewGameMenu();
		MainMenu mm = GameObject.Find("Main").GetComponent<MainMenu>();
        mm.BackFromNewGame();
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
        string path = $"{FileManager.GODS_PATH}\\{religion}.json";
        return FileManager.GetFileFromJSON<Religion>(path);
	}
    
    public void LoadGods(Religion religionSelected)
	{
        char[] Characters = new char[4] { 'A', 'B', 'C', 'D' };
        for(int i = 0; i < religionSelected.ListOfGods.Count && i < 4; i++)
			GameObject.Find($"Character {Characters[i]} Text").GetComponent<TextMeshProUGUI>().text = religionSelected.ListOfGods[i].Name;
    }

    public void SelectGod(int god)
	{
		ClearGodShowcase();

		godSelected = religionSelected.ListOfGods[god];
		string godName = godSelected.Name.ToString();
        godData.text = $"{godSelected.BuffDescription}";

		GameObject prefab = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.God, godName);
        Instantiate(prefab, godPrefab.transform.position, prefab.transform.rotation, godPrefab.transform);
    }

    void ClearGodShowcase()
    {
		godData.text = string.Empty;
		for (int i = 0; i < godPrefab.transform.childCount; i++)
			Destroy(godPrefab.transform.GetChild(i).gameObject);
	}
    public void StartGame()
    {
        if (godSelected == null)
            return;

		int seed = seedInputField.text == string.Empty ? Math.Abs(Guid.NewGuid().GetHashCode()) : int.Parse(seedInputField.text);
		List<UnitData> startingUnits = godSelected.StartingCharacterUnits;
		int startingGold = 0;
        int ascension = 0;

		GeneralManager.RunData runData = new(religionSelected.Name, godSelected.Name, 0, 1, seed, startingGold, startingUnits, ascension);
        SaveManager.SaveGameProgress(runData);
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
