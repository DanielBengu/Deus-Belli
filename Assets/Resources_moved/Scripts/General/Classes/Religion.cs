using System.Collections.Generic;
namespace Assets.Resources_moved.Scripts.General.Classes
{
	[System.Serializable]
	public class Religion
	{
		public string Name;
		public List<God> ListOfGods;
	}

	[System.Serializable]
	public class God
	{
		public string Name;
		public string Character_Model;
		public string BuffDescription;
		public List<UnitData> StartingCharacterUnits;
	}
}
