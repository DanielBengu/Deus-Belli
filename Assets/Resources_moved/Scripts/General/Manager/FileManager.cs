using System.Collections.Generic;
using System.IO;
using UnityEngine;
public static class FileManager
{
	public const string PLAYER_UNITS_PATH = "Assets\\Resources_moved\\Scripts\\General\\Player Data\\Unit list.txt";
	public static List<Unit> GetUnits(DataSource source, string[] CustomData = null)
	{
		List<Unit> playerUnits = new();
		string[] units;

		switch (source)
		{
			case DataSource.PlayerUnits:
				units = File.ReadAllLines(PLAYER_UNITS_PATH);
				break;
			case DataSource.Custom:
				units = CustomData;
				break;
			default:
				return null;
		}

		for (int i = 0; i < units.Length; i++)
		{
			try
			{
				string[] data = units[i].Split('#');
				GameObject unitObject = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, data[0]);
				Unit unitScript = unitObject.GetComponent<Unit>();
				unitScript.Load(unitScript);
				unitScript.LoadData(data);
				playerUnits.Add(unitScript);
			}
			catch
			{
			}
		}

		return playerUnits;
	}

	public static void OverwriteFile(string filePath, string[] dataLines)
	{
		File.WriteAllLines(filePath, dataLines);
	}

	public static void AppendFile(string filePath, string[] dataLines)
	{
		File.AppendAllLines(filePath, dataLines);
	}

	public enum DataSource
	{
		PlayerUnits,
		Custom,
	}
}
