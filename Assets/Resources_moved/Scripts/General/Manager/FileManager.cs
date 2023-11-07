using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FileManager : MonoBehaviour
{
	const string PLAYER_UNITS_PATH = "Assets\\Resources_moved\\Scripts\\General\\Player Data\\Unit list.txt";
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
				unitScript.LoadData(data);
				playerUnits.Add(unitScript);
			}
			catch
			{
			}
		}

		return playerUnits;
	}

	public static void LoadUnit(GameObject prefab)
	{

	}

	public static void SaveUnits(List<GameObject> units, bool overwriteCurrentList)
	{
		List<string> dataLines = new();
		foreach (var item in units)
		{
			Unit unitScript = item.GetComponent<Unit>();
			dataLines.Add($"{item.name.Substring(0, 6)}#{unitScript.unitName}#{unitScript.unitImage.name}#{unitScript.hpMax}#{unitScript.movementMax}#{unitScript.attack}#{unitScript.range}#{unitScript.startingTileNumber}");
		}

		if (overwriteCurrentList)
			File.WriteAllLines(PLAYER_UNITS_PATH, dataLines);
		else
			File.AppendAllLines(PLAYER_UNITS_PATH, dataLines);
		
	}

	public enum DataSource
	{
		PlayerUnits,
		Custom,
	}
}
