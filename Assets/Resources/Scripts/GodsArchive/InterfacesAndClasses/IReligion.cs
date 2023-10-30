using System.Collections.Generic;

public interface IReligion
{
	public string Name { get; }
	public List<IGod> GodsOfReligion { get; }
	public bool IsPlayable { get; }
	public List<IGod> PlayableGods { get; }
}