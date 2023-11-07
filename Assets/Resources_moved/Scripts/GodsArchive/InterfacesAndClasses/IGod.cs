using System.Collections.Generic;
public interface IGod
{
	public static string Name;
	public static IReligion Religion;
	public Encounter[] Encounters { get; }

	//We preload all possible units so that we don't have to do it everytime we need to call a specific one. If this takes too much time consider using lazy loading instead
	public static Dictionary<string, Unit> unitsDict;

	public Unit GetUnit(string unitName);
	public string GetName();
	public IReligion GetReligion();
}
