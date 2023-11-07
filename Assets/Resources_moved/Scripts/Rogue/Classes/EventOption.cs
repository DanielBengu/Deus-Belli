using System.Collections.Generic;

public class EventOption
{

	public EventOption(Event eventFather)
	{
		EventFather = eventFather;
	}
	public Event EventFather { get; set; }
	public string OptionDescription { get; set; }

	public ObjectToAdd ObjToAddEnum { get; set; }
	public object ObjToAdd { get; set; }

	public List<EventFunction> OptionFunction { get; set; }
}

public struct EventFunction
{
    public ObjectToAdd objToAddEnum;
    public object objToAdd;
	public Event.EventOptionFunc funcToCall;

	public EventFunction(ObjectToAdd objToAddEnum, object objToAdd, Event.EventOptionFunc funcToCall)
    {
		this.objToAddEnum = objToAddEnum;
		this.objToAdd = objToAdd;
		this.funcToCall = funcToCall;
    }
}