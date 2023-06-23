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
    public GameObject prefab;
    public int startingTileNumber;

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

    public void LoadData(string[] data)
	{
        unitName = data[1];
        unitImage = Resources.Load<Sprite>($"Sprites/Units/{data[2]}");
        hpMax = int.Parse(data[3]);
        movementMax = int.Parse(data[4]);
        attack = int.Parse(data[5]);
        range = int.Parse(data[6]);
        startingTileNumber = int.Parse(data[7]);
    }
}
