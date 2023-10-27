using System.Collections.Generic;

public interface IGod
{
	public static List<string> unitsPaths;
	public static string name;
	public static string religion;
	public List<Encounter> Encounters { get; }
}
