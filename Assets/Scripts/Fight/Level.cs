using System.Collections.Generic;
using UnityEngine;

public class Level
{
    public int TopLeftSquarePositionX;
    public int TopLeftSquarePositionZ;
    public int YPosition;
    public int XLength;
    public int YLength;

    //Key represents tile number
    public Dictionary<int, GameObject> tilesDict = new Dictionary<int, GameObject>();

    //Key represents the assigned tile number of the unit
    public Dictionary<int, GameObject> enemyList;
}