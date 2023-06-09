using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    const string fightVictoryScreenPrefabName = "Fight Victory";
    const string rogueVictoryScreenPrefabName = "Rogue Victory";

    TextMeshProUGUI Title;
    Image GodImage;

    GameObject rogueCanvas;
    GameObject infoPanel;
    GameObject endTurnButton;
    GameObject fightVictoryScreen;
    GameObject rogueVictoryScreen;

    #region Info Panel
    Image unitImage;
        TextMeshProUGUI nameText;
        TextMeshProUGUI hpValue;
        TextMeshProUGUI movementValue;
        TextMeshProUGUI attackValue;
        TextMeshProUGUI rangeValue;
    #endregion

	public void SetFightVariables(){
        string godSelected = PlayerPrefs.GetString(GeneralManager.GOD_SELECTED_PP, "");
        Title = GameObject.Find("GOD_RUN").GetComponent<TextMeshProUGUI>();
        Title.text = $"{godSelected} Run";

        GodImage = GameObject.Find("God").GetComponent<Image>();
        GodImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godSelected}");

        infoPanel = GameObject.Find("Info");
        endTurnButton = GameObject.Find("End Turn Button");
        fightVictoryScreen = Resources.Load<GameObject>($"Prefabs/Fight/{fightVictoryScreenPrefabName}");
        

        unitImage = infoPanel.transform.Find("Image").gameObject.GetComponent<Image>();
        nameText = infoPanel.transform.Find("Unit title").gameObject.GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HP value").gameObject.GetComponent<TextMeshProUGUI>();
        movementValue = infoPanel.transform.Find("Movement value").gameObject.GetComponent<TextMeshProUGUI>();
        attackValue = infoPanel.transform.Find("Attack value").gameObject.GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("Range value").gameObject.GetComponent<TextMeshProUGUI>();
        SetInfoPanel(false);
    }

    public void SetRogueVariables(int gold, string godSelected, int seed)
	{
        rogueCanvas = GameObject.Find("RogueCanvas");
        rogueVictoryScreen = Resources.Load<GameObject>($"Prefabs/Rogue/Children/{rogueVictoryScreenPrefabName}");
        TextMeshProUGUI rogueGold = GameObject.Find("Gold Value").GetComponent<TextMeshProUGUI>();
        rogueGold.text = gold.ToString();
        TextMeshProUGUI rogueGod = GameObject.Find("God Text").GetComponent<TextMeshProUGUI>();
        rogueGod.text = godSelected;
        TextMeshProUGUI rogueSeed = GameObject.Find("Seed Value").GetComponent<TextMeshProUGUI>();
        rogueSeed.text = seed.ToString();
    }

    public void SetMerchantVariables(int availableGold)
	{
        TextMeshProUGUI rogueSeed = GameObject.Find("Current Gold").GetComponent<TextMeshProUGUI>();
        rogueSeed.text = $"Current Gold: {availableGold}g";
    }

    public void SetInfoPanel(bool active, Unit unit = null){
        SetGameObject(infoPanel, active);
        if (active){
            nameText.text = unit.unitName;
            unitImage.sprite = unit.unitImage;
            hpValue.text = $"{unit.hpCurrent}/{unit.hpMax}";
            movementValue.text = $"{unit.movementCurrent}/{unit.movementMax}";
            attackValue.text = unit.attack.ToString();
            rangeValue.text = unit.range.ToString();
        }
    }

    public void SetEndTurnButton(bool active)
	{
        SetGameObject(endTurnButton, active);
    }

	internal void GetFightVictoryScreen(int gold)
	{
        var rew1 = fightVictoryScreen.transform.Find("Reward 1").GetComponent<TextMeshProUGUI>();
        rew1.text = $"{gold} gold";
        Instantiate(fightVictoryScreen, infoPanel.transform.parent);
    }

    internal void GetRogueVictoryScreen()
    {
        Instantiate(rogueVictoryScreen, rogueCanvas.transform);
    }

    public void SetGameObject(GameObject gameObject, bool active)
	{
        gameObject.SetActive(active);
    }
}
