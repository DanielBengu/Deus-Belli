using System.Collections.Generic;

[System.Serializable]
public class EncounterListData
{
	//This list is for encounters that can be randomly picked
	public List<EncounterData> GenericEncounterList;
	//This list is for encounters that represents specific battles, like bosses and events. Can't be randomly picked
	public List<EncounterData> SpecialEncounterList;
}
[System.Serializable]
public class EncounterData
{
	public string Name;
	public string Map;
	public List<UnitData> EnemyList;
}