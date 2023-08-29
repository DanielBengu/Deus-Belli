using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour
{
	const string PLAYER_UNITS_PATH = "Assets\\Resources\\Scripts\\General\\Player Data\\Unit list.txt";
	public static List<GameObject> GetUnits(DataSource source, string[] CustomData = null)
	{
		List<GameObject> playerUnits = new();
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
			string[] data = units[i].Split('#');
			GameObject unitObject = Resources.Load<GameObject>($"Prefabs/Units/{data[0]}");
			Unit unitScript = unitObject.GetComponent<Unit>();
			unitScript.LoadData(data);
			playerUnits.Add(unitObject);
		}

		return playerUnits;
	}

	public static void SaveUnits(List<GameObject> units)
	{
		List<string> dataLines = new();
		foreach (var item in units)
		{
			dataLines.Add($"");
		}
		File.WriteAllLines(PLAYER_UNITS_PATH, dataLines);
	}

	public enum DataSource
	{
		PlayerUnits,
		Custom,
	}
}
