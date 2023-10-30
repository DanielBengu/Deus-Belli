using System.Collections.Generic;
using UnityEngine;

public abstract class IGod
{
	public string Name { get; set; }
	public IReligion Religion { get; set; }
	public Encounter[] Encounters { get; }

	//We preload all possible units so that we don't have to do it everytime we need to call a specific one. If this takes too much time consider using lazy loading instead
	public Dictionary<string, Unit> unitsDict;

	public IGod()
	{
		Religion = null;
	}

	public Unit GetUnit(string unitName)
	{
		return Resources.Load<GameObject>($"Prefabs/Units/{Religion}/{Name}/{unitName}").GetComponent<Unit>();
	}
}
