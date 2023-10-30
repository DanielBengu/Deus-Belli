using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BanditLord : IGod
{
	public static string name = "Bandit Lord";
	public static string religion = "None";

	//We preload all possible units so that we don't have to do it everytime we need to call a specific one. If this takes too much time consider using lazy loading instead
	public Dictionary<string, Unit> unitsDict;

	public BanditLord()
	{
		unitsDict = new()
		{
			{ "Bandit", GetUnit("Bandit") },
			{ "Goblin_Green", GetUnit("Goblin_Green") },
			{ "Goblin_Red", GetUnit("Goblin_Red") },
			{ "Goblin_Yellow", GetUnit("Goblin_Yellow") }
		};
	}

	public Encounter[] Encounters { get { return new Encounter[2] {
		new() 
		{ 
			EnemyGod = this, 
			units = new Unit[2] { unitsDict["Bandit"], unitsDict["Bandit"] },
			positions = new int[2] { 0, 1 } 
		},
		new()
		{
			EnemyGod = this,
			units = new Unit[3] { unitsDict["Goblin_Red"], unitsDict["Goblin_Green"], unitsDict["Goblin_Yellow"] },
			positions = new int[3] { 0, 1, 2 }
		},
	};}}

	public Unit GetUnit(string unitName)
	{
		return Resources.Load<GameObject>($"Prefabs/Units/Bandit Lord/{unitName}").GetComponent<Unit>();
	}
}