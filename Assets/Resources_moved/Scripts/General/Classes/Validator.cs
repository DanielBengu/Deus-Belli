using System.Linq;
using Unity.VisualScripting;

public static class Validator
{
	public static bool Validate(object obj)
	{
		return obj.GetType().Name switch
		{
			"TileMapData" => ValidateMap((TileMapData)obj),
			_ => true,
		};
	}

	public static bool ValidateMap(TileMapData obj)
	{
		bool isCountCorrect = obj.TileList.DistinctBy(t => t.PositionOnGrid).Count() == obj.Rows * obj.Columns;
		return isCountCorrect;
	}
}