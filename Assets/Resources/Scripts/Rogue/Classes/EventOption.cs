public class EventOption
{

	public EventOption(Event eventFather)
	{
		EventFather = eventFather;
	}
	public Event EventFather { get; set; }
	public string OptionDescription { get; set; }

	public Event.EventOptionFunc OptionFunction { get; set; }
}
