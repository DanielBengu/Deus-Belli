using System;
using System.Collections.Generic;
using UnityEngine;

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

    public Unit unitOnTile;
    public GameObject model3D = null;

    FightManager manager;

    public int tileNumber;

    public void OnMouseDown()
    {
        ObjectClickedEnum objClicked;
        GameObject objectToManage;
        if(manager.IsGameInStandby)
            return;

        if(unitOnTile){
            if(unitOnTile.faction == FightManager.USER_FACTION)
                manager.UnitSelected = unitOnTile.GetComponent<Unit>();
            objClicked = ObjectClickedEnum.UnitTile;
            objectToManage = unitOnTile.gameObject;
            
        }else{
            objClicked = ObjectClickedEnum.EmptyTile;
            objectToManage = gameObject;
        }

        manager.ManageClick(objClicked, objectToManage);
    }

    //Workaround to manage right click like we do for the left click in OnMouseDown()
    void OnMouseOver () {
        if(Input.GetMouseButtonDown(FightManager.RIGHT_MOUSE_BUTTON)){
            //manager.ManageClick(ObjectClickedEnum.RightClickOnField, gameObject);
        }
    }

    //Necessary for tile comparement during dijkstra calculations
    public int CompareTo(Tile other){
        if(tentativeCost < other.tentativeCost) return -1;
        if(tentativeCost == other.tentativeCost) return 0;
        return 1;
    }

    public List<Tile> ToList()
    {
        return new List<Tile>() { this };
    }

    public void SetupManager(FightManager manager)
    {
        this.manager = manager;
    }
}