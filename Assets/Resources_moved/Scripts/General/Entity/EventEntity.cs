using static GeneralManager;

public class EventEntity
{
	public RunData runData;
	public ObjectToAdd objToAddEnum;
	public object objToAdd;
}

public enum ObjectToAdd{
	Default,
	Gold,
	Unit
}
