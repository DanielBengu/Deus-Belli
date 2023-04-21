using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    List<Tile> _possibleMovements;

    public Tile CurrentTile { get; set; }
    public bool HasPerformedMainAction { get; set; }

    FightManager fightManager;

    public string unitName;
    public Sprite unitImage;

    public List<Tile> PossibleMovements { get { return GetPossibleMovements(); } }



    #region Stats
        public int hpMax;
        public int hpCurrent;
        public float movementMax;
        public float movementCurrent;
        public int attack;
        public int faction;
        public int range;
    #endregion

    public void SetupManager(FightManager manager)
	{
        fightManager = manager;
	}

	public List<Tile> GetPossibleAttacks()
	{
        return fightManager.GetPossibleAttacksForUnit(this);
    }

    List<Tile> GetPossibleMovements()
    {
        //if map has changed
        if (true)
            _possibleMovements = fightManager.GetPossibleMovements(this);

        return _possibleMovements;
    }
}
