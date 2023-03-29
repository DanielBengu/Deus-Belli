using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IComparable<Tile>
{
    #region Stats
        public bool IsPassable = true;
        public float MovementCost = 1;
    #endregion

    #region Dijkstra
        public bool IsVisited = false;
        public float tentativeCost = 99999;
    #endregion
    

    [SerializeField]
    public GameObject unitOnTile;

    public FightManager manager;

    public int tileNumber;

    void Start(){
        manager = GameObject.Find("Manager").GetComponent<FightManager>();
    }

    public void OnMouseDown()
    {
        ObjectClickedEnum objClicked;
        if(manager.IsGameInStandby)
            return;

        if(unitOnTile){
            manager.UnitSelected = unitOnTile.GetComponent<Unit>();
            objClicked = ObjectClickedEnum.UnitTile;
            
        }else{
            objClicked = ObjectClickedEnum.EmptyTile;
        }

        manager.ManageClick(objClicked, gameObject);
    }

    //Workaround to manage right click like we do for the left click in OnMouseDown()
    void OnMouseOver () {
        if(Input.GetMouseButtonDown(FightManager.RIGHT_MOUSE_BUTTON)){
            //manager.ManageClick(ObjectClickedEnum.RightClickOnField, gameObject);
        }
    }

    public int CompareTo(Tile other){
        if(this.tentativeCost < other.tentativeCost) return -1;
        if(this.tentativeCost == other.tentativeCost) return 0;
        return 1;
    }
}