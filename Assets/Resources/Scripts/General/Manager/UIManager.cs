using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    const string fightVictoryScreenPrefabName = "Fight Victory";
    const string fightDefeatScreenPrefabName = "Fight Defeat";
    const string rogueVictoryScreenPrefabName = "Rogue Victory";
    const string rogueDefeatScreenPrefabName = "Rogue Defeat";

    TextMeshProUGUI Title;
    Image GodImage;

    GameObject rogueCanvas;
    GameObject infoPanel;
    GameObject endTurnButton;
    GameObject fightVictoryScreen;
    GameObject fightDefeatScreen;
    GameObject rogueVictoryScreen;
    GameObject rogueDefeatScreen;

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
        fightDefeatScreen = Resources.Load<GameObject>($"Prefabs/Fight/{fightDefeatScreenPrefabName}");


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
        rogueDefeatScreen = Resources.Load<GameObject>($"Prefabs/Rogue/Children/{rogueDefeatScreenPrefabName}");
        TextMeshProUGUI rogueGold = GameObject.Find("Gold Value").GetComponent<TextMeshProUGUI>();
        rogueGold.text = gold.ToString();
        TextMeshProUGUI rogueGod = GameObject.Find("God Text").GetComponent<TextMeshProUGUI>();
        rogueGod.text = godSelected;
        TextMeshProUGUI rogueSeed = GameObject.Find("Seed Value").GetComponent<TextMeshProUGUI>();
        rogueSeed.text = seed.ToString();
    }

    public void SetMerchantVariables(int availableGold)
	{
    }

    public void SetMerchantItemVariables(MerchantItem item)
	{
        TextMeshProUGUI itemName = GameObject.Find($"0_Name_TXT").GetComponent<TextMeshProUGUI>();
        itemName.text = item.Name;
        TextMeshProUGUI itemPrice = GameObject.Find($"0_Price_TXT").GetComponent<TextMeshProUGUI>();
        itemPrice.text = $"{item.Price}g";
    }

    public void SetEventVariables(Event data)
	{
        Image image = GameObject.Find("Image").GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>($"Sprites/Events/{data.ImageName}");
        TextMeshProUGUI title = GameObject.Find("Title").GetComponent<TextMeshProUGUI>();
        title.text = data.Title;
        TextMeshProUGUI description = GameObject.Find("Description").GetComponent<TextMeshProUGUI>();
        description.text = data.Description;
        TextMeshProUGUI choice1 = GameObject.Find("Choice 1 text").GetComponent<TextMeshProUGUI>();
        choice1.text = data.Options[0].OptionDescription;
        TextMeshProUGUI choice2 = GameObject.Find("Choice 2 text").GetComponent<TextMeshProUGUI>();
        choice2.text = data.Options[1].OptionDescription;
    }

    public void ItemBought(int itemIndex, int newPlayerGoldAmount)
	{
        Destroy(GameObject.Find($"Item_{itemIndex}"));
        TextMeshProUGUI currentGold = GameObject.Find("Gold Value").GetComponent<TextMeshProUGUI>();
        currentGold.text = $"{newPlayerGoldAmount}g";
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

    public void GetScreen(GameScreens screen, int gold)
	{
		switch (screen)
		{
			case GameScreens.FightVictoryScreen:
                var rew1 = fightVictoryScreen.transform.Find("Reward 1").GetComponent<TextMeshProUGUI>();
                rew1.text = $"{gold} gold";
                Instantiate(fightVictoryScreen, infoPanel.transform.parent);
                break;
			case GameScreens.FightDefeatScreen:
                Instantiate(fightDefeatScreen, infoPanel.transform.parent);
                break;
			case GameScreens.RogueVictoryScreen:
                Instantiate(rogueVictoryScreen, rogueCanvas.transform);
                break;
			case GameScreens.RogueDefeatScreen:
                Instantiate(rogueDefeatScreen, rogueCanvas.transform);
                break;
		}
	}

    public void SetGameObject(GameObject gameObject, bool active)
	{
        gameObject.SetActive(active);
    }
}
