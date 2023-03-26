using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    FightManager fightManager;

    
    [SerializeField]
    TextMeshProUGUI Title;
    [SerializeField] 
    Image GodImage;
    [SerializeField]
    public GameObject infoPanel;
    [SerializeField]
    public GameObject endTurnButton;

    #region Info Panel
        [SerializeField] 
        Image unitImage;
        [SerializeField]
        TextMeshProUGUI nameText;
        [SerializeField]
        TextMeshProUGUI hpValue;
        [SerializeField]
        TextMeshProUGUI movementValue;
    #endregion

    void Start(){
        SetUI();
    }

    void SetUI(){
        string godSelected = PlayerPrefs.GetString("God Selected", "");
        Title.text = $"{godSelected} Run";
        GodImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godSelected}");
    }

    public void SetInfoPanel(bool active, Unit unit){
        infoPanel.SetActive(active);
        if(active){
            hpValue.text = $"{unit.hpCurrent}/{unit.hpMax}";
            movementValue.text = $"{unit.movementCurrent}/{unit.movementMax}";
            nameText.text = unit.unitName;
            unitImage.sprite = unit.unitImage;
        }
    }
}
