public class BanditLord : IGod
{
	public BanditLord()
	{
		Name = GetType().Name;
		unitsDict = new()
		{
			{ "Bandit", GetUnit("Bandit") },
			{ "Goblin_Green", GetUnit("Goblin_Green") },
			{ "Goblin_Red", GetUnit("Goblin_Red") },
			{ "Goblin_Yellow", GetUnit("Goblin_Yellow") }
		};
	}

	public new Encounter[] Encounters { get { return new Encounter[2] {
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
}