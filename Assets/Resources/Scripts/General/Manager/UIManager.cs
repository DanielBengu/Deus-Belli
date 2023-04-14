using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Title;
    [SerializeField] 
    Image GodImage;

    [SerializeField]
    GameObject infoPanel;
    [SerializeField]
    GameObject endTurnButton;
    [SerializeField]
    GameObject victoryScreen;

    #region Info Panel
        [SerializeField] 
        Image unitImage;
        [SerializeField]
        TextMeshProUGUI nameText;
        [SerializeField]
        TextMeshProUGUI hpValue;
        [SerializeField]
        TextMeshProUGUI movementValue;
        [SerializeField]
        TextMeshProUGUI attackValue;
        [SerializeField]
        TextMeshProUGUI rangeValue;
    #endregion

    void Start(){
        SetVariables();
    }

    void SetVariables(){
        /*string godSelected = PlayerPrefs.GetString("God Selected", "");
        Title.text = $"{godSelected} Run";
        GodImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godSelected}");*/
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
        SetGameObject(victoryScreen, true);
    }

    public void SetGameObject(GameObject gameObject, bool active)
	{
        gameObject.SetActive(active);
    }
}
