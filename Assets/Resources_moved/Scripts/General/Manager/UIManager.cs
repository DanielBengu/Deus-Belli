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

	public void SetFightVariables(IGod god){
        string godSelected = god.GetName();
        string religionSelected = god.GetReligion().Name;
        Title = GameObject.Find("GOD_RUN").GetComponent<TextMeshProUGUI>();
        Title.text = $"{godSelected} Run";

        infoPanel = GameObject.Find("Info");
        endTurnButton = GameObject.Find("End Turn Button");
        fightVictoryScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, "Fight Victory");
        fightDefeatScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, "Fight Defeat");

        unitImage = infoPanel.transform.Find("Image").gameObject.GetComponent<Image>();
        nameText = infoPanel.transform.Find("Unit title").gameObject.GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HP value").gameObject.GetComponent<TextMeshProUGUI>();
        movementValue = infoPanel.transform.Find("Movement value").gameObject.GetComponent<TextMeshProUGUI>();
        attackValue = infoPanel.transform.Find("Attack value").gameObject.GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("Range value").gameObject.GetComponent<TextMeshProUGUI>();
        SetInfoPanel(false);
    }
    public void SetFightEndPhaseButton()
    {
        endTurnButton = GameObject.Find("End Phase TEXT");
        TextMeshProUGUI buttonText = endTurnButton.GetComponent<TextMeshProUGUI>();
        buttonText.text = $"End Turn";
        SetInfoPanel(false);
    }

    public void SetRogueVariables(int gold, string godSelected, int seed)
	{
        rogueCanvas = GameObject.Find("RogueCanvas");
        rogueVictoryScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, "Rogue Victory");
        rogueDefeatScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, "Fight Defeat");
        TextMeshProUGUI rogueGold = GameObject.Find("Gold Value").GetComponent<TextMeshProUGUI>();
        rogueGold.text = gold.ToString();
        TextMeshProUGUI rogueGod = GameObject.Find("God Text").GetComponent<TextMeshProUGUI>();
        rogueGod.text = godSelected;
    }

    public void SetMerchantVariables(int availableGold)
	{
    }

    public void SetMerchantItemVariables(MerchantItem item, int index)
	{
        TextMeshProUGUI itemName = GameObject.Find($"{index}_Name").GetComponent<TextMeshProUGUI>();
        itemName.text = item.Name;
        TextMeshProUGUI itemPrice = GameObject.Find($"{index}_Price").GetComponent<TextMeshProUGUI>();
        itemPrice.text = $"{item.Price}g";
    }

    public void SetEventVariables(DB_Event data)
	{
        Image image = GameObject.Find("Image").GetComponent<Image>();
        image.sprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, data.ImageName);
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
        Destroy(GameObject.Find($"{itemIndex}_Item"));
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
