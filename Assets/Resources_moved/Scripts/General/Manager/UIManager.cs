using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
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
    Transform traitParent;
    Transform statsParent;
    TextMeshProUGUI nameText;
    GameObject tooltipParent;
    #endregion

	public void SetFightVariables(IGod god){
        string godSelected = god.ToString();
        //string religionSelected = god.GetReligion().Name;
        Title = GameObject.Find("GOD_RUN").GetComponent<TextMeshProUGUI>();
        Title.text = $"{godSelected} Run";

        infoPanel = GameObject.Find("Info");
		endTurnButton = GameObject.Find("End Phase Button");
		fightVictoryScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, fightVictoryScreenPrefabName);
        fightDefeatScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, fightDefeatScreenPrefabName);

        traitParent = infoPanel.transform.Find("Traits");
        statsParent = infoPanel.transform.Find("Stats");
        nameText = infoPanel.transform.Find("Unit title").gameObject.GetComponent<TextMeshProUGUI>();
        tooltipParent = GameObject.Find("TooltipParent");
		SetInfoPanel(false);
    }
    public void SetFightEndPhaseButton()
    {
		TextMeshProUGUI buttonText = endTurnButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        buttonText.text = $"End Turn";
        SetInfoPanel(false);
    }

    public void SetRogueVariables(int gold, string godSelected)
	{
        rogueCanvas = GameObject.Find("RogueCanvas");
        rogueVictoryScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, rogueVictoryScreenPrefabName);
        rogueDefeatScreen = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Prefab, rogueDefeatScreenPrefabName);
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

        if (!active || !Validator.Validate(unit))
            return;

        SetupTraitsOnInfoPanel(unit);
        SetupStatsOnInfoPanel(unit);
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
	public void ClearTooltip()
	{
		for (int i = 0; i < tooltipParent.transform.childCount; i++)
		{
			GameObject tooltip = tooltipParent.transform.GetChild(i).gameObject;
			Destroy(tooltip);
		}
	}
	#region Private Methods
	void SetupTraitsOnInfoPanel(Unit unit)
	{
		for (int i = 0; i < traitParent.childCount; i++)
		{
			Transform traitBox = traitParent.GetChild(i);
			if (unit.UnitData.Traits.Count > i)
			{
                Traits currentTrait = unit.UnitData.Traits[i];
                TraitsEnum traitsEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), currentTrait.Name);
				traitBox.GetComponent<Image>().sprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, currentTrait.Name);
				traitBox.gameObject.SetActive(true);
                string tooltipHeader = TraitText.GetTraitHeader(traitsEnum, currentTrait.Level);
                string tooltipText = TraitText.GetTraitText(traitsEnum, currentTrait.Level);
				SetupTooltip(traitBox.gameObject, tooltipHeader, tooltipText);
			}
			else
				traitBox.gameObject.SetActive(false);
		}
	}

	void SetupStatsOnInfoPanel(Unit unit)
	{
		nameText.text = unit.UnitData.Name;
		for (int i = 0; i < statsParent.childCount; i++)
		{
			TextMeshProUGUI stat = statsParent.GetChild(i).GetComponent<TextMeshProUGUI>();
			switch (stat.name)
			{
				case "HP":
                    SetupStat(stat, unit, unit.FightData.baseStats.HP, unit.FightData.currentStats.CURRENT_HP, unit.FightData.currentStats.MAXIMUM_HP);
					break;
				case "Movement":
                    SetupStat(stat, unit, unit.FightData.baseStats.MOVEMENT, unit.FightData.currentStats.MOVEMENT);
					break;
				case "Attack":
					SetupStat(stat, unit, unit.FightData.baseStats.ATTACK, unit.FightData.currentStats.ATTACK);
					break;
				case "Range":
					SetupStat(stat, unit, unit.FightData.baseStats.RANGE, unit.FightData.currentStats.RANGE);
					break;
                case "Armor":
					SetupStat(stat, unit, unit.FightData.baseStats.ARMOR, unit.FightData.currentStats.ARMOR);
					break;
                case "Ward":
					SetupStat(stat, unit, unit.FightData.baseStats.WARD, unit.FightData.currentStats.WARD);
					break;
				default:
					break;
			}
		}
	}

    void SetupStat(TextMeshProUGUI stat, Unit unit, int baseStat, int currentStat, int currentMaximum = -1)
    {
		stat.text = LoadStatText(currentStat, currentMaximum);
		stat.color = GetColor(currentStat, baseStat);
        string tooltipHeader = stat.name.ToUpper();
        string tooltipText = unit.FightData.GetStatText(baseStat, currentStat);
		SetupTooltip(stat.gameObject, tooltipHeader, tooltipText);
	}

    void SetupTooltip(GameObject item, string tooltipHeader, string tooltipText)
    {
		TooltipManager tooltipData = item.GetComponentInChildren<TooltipManager>();
        tooltipData.header = tooltipHeader;
        tooltipData.text = tooltipText;
	}

    string LoadStatText(int currentValue, int maxValue = -1)
    {
		if (maxValue != -1)
			return $"{currentValue}/{maxValue}";

		return currentValue.ToString();
	}

    Color GetColor(int currentValue, int maxValue)
    {
        if (currentValue > maxValue)
            return Color.green;
        if(currentValue < maxValue) 
            return Color.red;

        return Color.white;
    }
	#endregion
}
