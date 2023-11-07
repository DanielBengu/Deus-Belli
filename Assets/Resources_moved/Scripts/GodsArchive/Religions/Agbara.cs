using System.Collections.Generic;

public class Agbara : IReligion
{
	public string Name => GetType().Name;

	public List<IGod> GodsOfReligion => new()
	{
		new Ataiku(),
		new Omi(),
	};

	public List<IGod> PlayableGods => new()
	{
		new Ataiku(),
		new Ataiku(),
		new Omi(),
		new Omi(),
	};

	bool IReligion.IsPlayable => true;
}
