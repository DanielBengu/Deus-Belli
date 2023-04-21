using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    TextMeshProUGUI Title;
    Image GodImage;

    GameObject infoPanel;
    GameObject endTurnButton;
    GameObject victoryScreen;

    #region Info Panel
        Image unitImage;
        TextMeshProUGUI nameText;
        TextMeshProUGUI hpValue;
        TextMeshProUGUI movementValue;
        TextMeshProUGUI attackValue;
        TextMeshProUGUI rangeValue;
    #endregion

    public void SetFightVariables(){
        string godSelected = PlayerPrefs.GetString("God Selected", "");
        Title = GameObject.Find("GOD_RUN").GetComponent<TextMeshProUGUI>();
        Title.text = $"{godSelected} Run";

        GodImage = GameObject.Find("God").GetComponent<Image>();
        GodImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godSelected}");

        infoPanel = GameObject.Find("Info");
        endTurnButton = GameObject.Find("End Turn Button");
        victoryScreen = Resources.Load<GameObject>($"Prefabs/Fight/Victory");

        unitImage = infoPanel.transform.Find("Image").gameObject.GetComponent<Image>();
        nameText = infoPanel.transform.Find("Unit title").gameObject.GetComponent<TextMeshProUGUI>();
        hpValue = infoPanel.transform.Find("HP value").gameObject.GetComponent<TextMeshProUGUI>();
        movementValue = infoPanel.transform.Find("Movement value").gameObject.GetComponent<TextMeshProUGUI>();
        attackValue = infoPanel.transform.Find("Attack value").gameObject.GetComponent<TextMeshProUGUI>();
        rangeValue = infoPanel.transform.Find("Range value").gameObject.GetComponent<TextMeshProUGUI>();
        SetInfoPanel(false);
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

	internal void GetVictoryScreen()
	{
        Instantiate(victoryScreen, infoPanel.transform.parent);
    }

    public void SetGameObject(GameObject gameObject, bool active)
	{
        gameObject.SetActive(active);
    }
}
