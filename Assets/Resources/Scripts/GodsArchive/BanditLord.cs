using System.Collections.Generic;

public class BanditLord : IGod
{

	public static string name = "Bandit Lord";
	public static string religion = "None";
	public List<Encounter> Encounters { get { return new() {
		new() { EnemyGod = this, 
			//units = new Unit[3] {new()}
			positions = new int[3] { 0, 1, 2 } }
	};}}
}
