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

    void Start(){
        SetUI();
    }

    void SetUI(){
        string godSelected = PlayerPrefs.GetString("God Selected", "");
        Title.text = $"{godSelected} Run";
        GodImage.sprite = Resources.Load<Sprite>($"Sprites/Gods/{godSelected}");
    }
}
