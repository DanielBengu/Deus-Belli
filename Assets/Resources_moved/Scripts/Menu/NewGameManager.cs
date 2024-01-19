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
    Image godImage;

    [SerializeField]
    GameObject ReligionsPrefab;
    [SerializeField]
    GameObject ReligionSelectedText;
    [SerializeField]
    GameObject GodsPrefab;

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
        Instantiate(GodsPrefab, transform);
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

        if(godImage.color.a == 0f)
		{
            Color currentColor = godImage.color;
            currentColor.a = 1f;
            godImage.color = currentColor;
        }

        godImage.sprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, godSelectedFromList.Character_Sprite);
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
