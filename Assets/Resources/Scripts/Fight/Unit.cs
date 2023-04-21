using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Tile CurrentTile { get; set; }
    public bool HasPerformedMainAction { get; set; }

    public List<Tile> PossibleAttacks { get { return fightManager.GetPossibleAttacksForUnit(this); } }

    FightManager fightManager;

    public string unitName;
    public Sprite unitImage;



    #region Stats
        public int hpMax;
        public int hpCurrent;
        public float movementMax;
        public float movementCurrent;
        public int attack;
        public int faction;
        public int range;
    #endregion

    public void OnMouseDown()
    {
        if(fightManager.IsGameInStandby)
            return;

        if(faction == FightManager.USER_FACTION)
            fightManager.UnitSelected = this;
        fightManager.ManageClick(ObjectClickedEnum.UnitTile, gameObject);
    }

    //Workaround to manage right click like we do for the left click in OnMouseDown()
    void OnMouseOver () {
        if(Input.GetMouseButtonDown(FightManager.RIGHT_MOUSE_BUTTON)){
            //manager.ManageClick(ObjectClickedEnum.RightClickOnField, CurrentTile.gameObject);
        }
    }

    public void SetupManager(FightManager manager)
	{
        fightManager = manager;
	}
}
