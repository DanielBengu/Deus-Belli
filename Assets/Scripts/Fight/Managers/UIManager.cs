using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Title;
    [SerializeField] 
    Image GodImage;

    public GameObject infoPanel;
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
        [SerializeField]
        TextMeshProUGUI attackValue;
        [SerializeField]
        TextMeshProUGUI rangeValue;
    #endregion

    void Start(){
        SetUI();
    }

    void SetUI(){
        string godSelected = PlayerPrefs.GetString("God Selected", "");
        Title.text = $"{godSelected} Run";
        GodImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godSelected}");
    }

    public void SetInfoPanel(bool active, Unit unit = null){
        infoPanel.SetActive(active);
        if(active){
            nameText.text = unit.unitName;
            unitImage.sprite = unit.unitImage;
            hpValue.text = $"{unit.hpCurrent}/{unit.hpMax}";
            movementValue.text = $"{unit.movementCurrent}/{unit.movementMax}";
            attackValue.text = unit.attack.ToString();
            rangeValue.text = unit.range.ToString();
        }
    }
}
