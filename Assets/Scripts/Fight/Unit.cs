using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Tile currentTile;
    FightManager manager;

    public string unitName;
    public Sprite unitImage;

    #region Stats
        public int hpMax;
        public int hpCurrent;
        public int movementMax;
        public int movementCurrent;
        public int faction;
    #endregion
    
    void Start(){
        manager = GameObject.Find("Manager").GetComponent<FightManager>();
    }

    public void OnMouseDown()
    {
        if(manager.IsGameInStandby)
            return;

        manager.UnitSelected = this;
        manager.ManageClick(ObjectClickedEnum.UnitTile, gameObject);
    }

    //Workaround to manage right click like we do for the left click in OnMouseDown()
    void OnMouseOver () {
        if(Input.GetMouseButtonDown(FightManager.RIGHT_MOUSE_BUTTON)){
            //manager.ManageClick(ObjectClickedEnum.RightClickOnField, currentTile.gameObject);
        }
    }
}
