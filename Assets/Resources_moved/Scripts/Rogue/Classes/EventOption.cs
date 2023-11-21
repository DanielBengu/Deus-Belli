using System.Collections.Generic;

public class EventOption
{

	public EventOption(DB_Event eventFather)
	{
		EventFather = eventFather;
	}
	public DB_Event EventFather { get; set; }
	public string OptionDescription { get; set; }

	public ObjectToAdd ObjToAddEnum { get; set; }
	public object ObjToAdd { get; set; }

	public List<EventFunction> OptionFunction { get; set; }
}

public struct EventFunction
{
    public ObjectToAdd objToAddEnum;
    public object objToAdd;
	public DB_Event.EventOptionFunc funcToCall;

	public EventFunction(ObjectToAdd objToAddEnum, object objToAdd, DB_Event.EventOptionFunc funcToCall)
    {
		this.objToAddEnum = objToAddEnum;
		this.objToAdd = objToAdd;
		this.funcToCall = funcToCall;
    }
}