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
		return true;
	}
}