using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstStageArchive
{
	//Always available
	public static List<string> ENEMIES_AV = new()
	{
		"Bandit Lord",
	};

	//Difficulty 5 required
	public static List<string> ENEMIES_Five = new()
	{
		"Ork"
	};

	//Difficulty 10 required
	public static List<string> ENEMIES_Ten = new()
	{
		"Dragon"
	};

	public static List<string> GetPossibleEnemies(int difficulty)
	{
		List<string> enemies = ENEMIES_AV;

		if (difficulty >= 5) enemies.AddRange(ENEMIES_Five);
		if (difficulty >= 10) enemies.AddRange(ENEMIES_Ten);

		return enemies;
	}
}
