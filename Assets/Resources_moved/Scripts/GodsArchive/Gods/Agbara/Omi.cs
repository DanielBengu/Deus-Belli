using System.Collections.Generic;
using UnityEngine;

public class Omi : IGod
{
	static string _name;
	static IReligion _religion;
	static Dictionary<string, Unit> _unitsDict;

	public Omi()
	{
		_name = GetType().Name;
		_religion = new Agbara();
		_unitsDict = new()
		{
			{ "Bandit", GetUnit("Bandit") },
			{ "Goblin_Green", GetUnit("Goblin_Green") },
			{ "Goblin_Red", GetUnit("Goblin_Red") },
			{ "Goblin_Yellow", GetUnit("Goblin_Yellow") }
		};
	}

	public Encounter[] Encounters
	{
		get
		{
			return new Encounter[2] {
		new()
		{
			EnemyGod = this,
			units = new Unit[2] { _unitsDict["Bandit"], _unitsDict["Bandit"] },
			positions = new int[2] { 0, 1 }
		},
		new()
		{
			EnemyGod = this,
			units = new Unit[3] { _unitsDict["Goblin_Red"], _unitsDict["Goblin_Green"], _unitsDict["Goblin_Yellow"] },
			positions = new int[3] { 0, 1, 2 }
		},
	};
		}
	}

	public Unit GetUnit(string unitName)
	{
		return AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Units, unitName).GetComponent<Unit>();
	}

	public string GetName()
	{
		return _name;
	}
	public IReligion GetReligion()
	{
		return _religion;
	}
}
