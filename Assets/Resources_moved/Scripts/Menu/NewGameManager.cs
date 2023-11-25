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
    private IReligion religionSelected;

	private void Update()
	{
        startButton.enabled = seedInputField.text.Length == 0 || seedInputField.text.Length > 2;
	}

    public void SelectReligion(int religion)
    {
        religionSelected = LoadReligion(religion);
        ReligionSelectedText.SetActive(true);
        ReligionSelectedText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = religionSelected.Name;
        ReligionsPrefab.SetActive(false);
        Instantiate(GodsPrefab, transform);
        LoadGods(religionSelected);
    }

    public IReligion LoadReligion(int religion)
	{
		switch (religion)
		{
            case 0:
                return new Agbara();
            default:
                return new Agbara();
		}
	}
    
    public void LoadGods(IReligion religionSelected)
	{
        string[] playableGods = religionSelected.PlayableGods.Select(g => g.GetName()).ToArray();

		GameObject.Find("Character A Text").GetComponent<TextMeshProUGUI>().text = playableGods[0];
        GameObject.Find("Character B Text").GetComponent<TextMeshProUGUI>().text = playableGods[1];
        GameObject.Find("Character C Text").GetComponent<TextMeshProUGUI>().text = playableGods[2];
        GameObject.Find("Character D Text").GetComponent<TextMeshProUGUI>().text = playableGods[3];
    }

    public void SelectGod(int god)
	{
        string godName = religionSelected.PlayableGods[god].GetName();
        godSelected = godName;
        godData.text = $"{godName}";

        if(godImage.color.a == 0f)
		{
            Color currentColor = godImage.color;
            currentColor.a = 1f;
            godImage.color = currentColor;
        }

        godImage.sprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, godName);
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
