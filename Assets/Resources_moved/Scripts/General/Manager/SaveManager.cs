using System.Collections.Generic;
using UnityEngine;
using static GeneralManager;

public static class SaveManager
{
	public static void SaveGameProgress(int gold, int currentRow, int currentPositionInRow, int gameStatus, List<Unit> unitList)
	{
		PlayerPrefs.SetInt(GOLD, gold);
		PlayerPrefs.SetInt(CURRENT_ROW, currentRow);
		PlayerPrefs.SetInt(CURRENT_POSITION_IN_ROW, currentPositionInRow);
		PlayerPrefs.SetInt(GAME_STATUS, gameStatus);
		SaveUnits(unitList, OperationType.Overwrite);
	}
	public static void SaveUnits(List<Unit> unitsList, OperationType operationType)
	{
		string[] dataLines = LoadDataLines(unitsList);

		switch (operationType)
		{
			case OperationType.Overwrite:
				FileManager.OverwriteFile(FileManager.PLAYER_UNITS_PATH, dataLines);
				break;
			case OperationType.Append:
				FileManager.AppendFile(FileManager.PLAYER_UNITS_PATH, dataLines);
				break;
			default:
				break;
		}
	}

	static string[] LoadDataLines(List<Unit> units)
	{
		string[] dataLines = new string[units.Count];
		for (int i = 0; i < units.Count; i++)
		{
			Unit unitScript = units[i];
			dataLines[i] = $"{unitScript.name[..6]}#{unitScript.unitName}#{unitScript.unitImage.name}#{unitScript.hpMax}#{unitScript.movementMax}#{unitScript.attack}#{unitScript.range}#{unitScript.Movement.startingTileNumber}";
		}

		return dataLines;
	}

	public enum OperationType
	{
		Overwrite,
		Append
	}
}