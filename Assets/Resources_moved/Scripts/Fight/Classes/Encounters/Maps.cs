using System.Collections.Generic;

[System.Serializable]
public class TileMapData
{
	public string Id;
	public int Rows;
	public int Columns;
	public List<TileData> TileList;
	public TileData FillRemainingTilesWith;
}

[System.Serializable]
public class TileData
{
	public int PositionOnGrid;
	public string Model;
	public int StartPositionForFaction;
	public bool ValidForMovement;
}