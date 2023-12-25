using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

[System.Serializable]
public class TileMapData
{
	public string Id;
	public int Rows;
	public int Columns;
	public List<TileData> TileList;
	public TileData FillRemainingTilesWith;

	public void FillTiles()
	{
		IEnumerable<int> allNumbersInRange = Enumerable.Range(0, Rows * Columns);
		List<int> positionsMissing = allNumbersInRange.Except(TileList.Select(t => t.PositionOnGrid)).ToList();

		if (positionsMissing.Count == 0)
			return;

		for (int i = 0; i < positionsMissing.Count; i++)
		{
			TileData tileToAdd = FillRemainingTilesWith ?? new TileData()
			{
				PositionOnGrid = positionsMissing[i],
				MovementCost = Pathfinding.OUT_OF_BOUND_VALUE,
				StartPositionForFaction = -1,
				ValidForMovement = false
			};
			TileList.Add(tileToAdd);
		}
	}
}

[System.Serializable]
public class TileData
{
	public int PositionOnGrid;
	public string Model;
	public int StartPositionForFaction;
	public int MovementCost;
	public bool ValidForMovement;
}