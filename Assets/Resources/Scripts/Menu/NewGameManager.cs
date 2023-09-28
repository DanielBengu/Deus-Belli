using System.Collections;
using System.Collections.Generic;
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
    TextMeshProUGUI ReligionSelectedText;
    [SerializeField]
    GameObject GodsPrefab;

    private string godSelected;

	private void Update()
	{
        startButton.enabled = seedInputField.text.Length == 0 || seedInputField.text.Length > 2;
	}

    string GetGodName(int god)
	{
        return ((Gods)god).ToString();
	}

    public void SelectReligion(int religionSelected)
    {
        ReligionSelectedText.gameObject.SetActive(true);
        ReligionSelectedText.text = religionSelected.ToString();
        ReligionsPrefab.SetActive(false);
        Instantiate(GodsPrefab, transform);
    }

    public void SelectGod(int god)
	{
        string godName = GetGodName(god);
        godSelected = godName;
        godData.text = $"{godName}";

        if(godImage.color.a == 0f)
		{
            Color currentColor = godImage.color;
            currentColor.a = 1f;
            godImage.color = currentColor;
        }

        godImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godName}");
    }
    public void StartGame()
    {
        if (godSelected != null)
        {
            int seed = seedInputField.text == string.Empty ? 0 : int.Parse(seedInputField.text);
            PlayerPrefs.SetInt(GeneralManager.SEED, seed);
            PlayerPrefs.SetString(GeneralManager.GOD_SELECTED_PP, godSelected);
            PlayerPrefs.SetInt(GeneralManager.ONGOING_RUN, 0);
            SceneManager.LoadScene(1);
        }
    }

    public enum Gods
	{
        Zeus,
        Poseidon,
        Hades
	}
}
