using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static FightInput;

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

    FightManager manager;

    public int tileNumber;

    public bool isEdit;
    public string modelName;

    public void OnMouseDown()
    {
        //We are on the custom map editor
		if (isEdit)
		{
            MapEditorManager editor = GameObject.Find("Map Editor").GetComponent<MapEditorManager>();
            editor.ChangeTile(this);
            return;
		}

        //We are on the fight section
        ObjectClickedEnum objClicked;
        GameObject objectToManage;
        if (manager.IsGameInStandby)
            return;

        if (unitOnTile)
        {
            if (unitOnTile.unitData.Faction == FightManager.USER_FACTION)
                manager.UnitSelected = unitOnTile.GetComponent<Unit>();
            objClicked = ObjectClickedEnum.UnitTile;
            objectToManage = unitOnTile.gameObject;

        }
        else
        {
            objClicked = ObjectClickedEnum.EmptyTile;
            objectToManage = gameObject;
        }

        manager.ManageClick(objClicked, objectToManage);
    }

    //Workaround to manage right click like we do for the left click in OnMouseDown()
    void OnMouseOver () {
        if(Input.GetMouseButtonDown((int)MouseButton.Middle))
        {
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