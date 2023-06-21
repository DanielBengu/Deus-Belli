using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GeneralManager;

public class Event
{
	
	public string Title { get; set; }
	public string Description { get; set; }
	public string ImageName { get; set; }
	public List<EventOption> Options { get; set; }

	public Event(int seed)
	{
		string[] possibleEvents = GetPossibleEvents();

		Random.InitState(seed);
		int randomLength = Random.Range(0, possibleEvents.Length);

		LoadEvent(possibleEvents[randomLength]);

		/*EventOption opt = new();
		opt.OptionFunction = new(Test);*/
	}

	public void Add50Gold(ref RunData runData)
	{
		runData.gold += 50;
	}

	public void Add100Gold(ref RunData runData)
	{
		runData.gold += 100;
	}

	string[] GetPossibleEvents()
	{
		return File.ReadAllLines("Assets\\Resources\\Scripts\\Rogue\\TXT\\Encounters.txt");
	}

	void LoadEvent(string eventChosen){
		string[] data = eventChosen.Split(';');

		ImageName = data[0];
		Title = data[1];
		Description = data[2];
		EventOption option1 = new(this)
		{
			OptionDescription = data[3],
			OptionFunction = GetFunction(int.Parse(data[4])),
		};
		EventOption option2 = new(this)
		{
			OptionDescription = data[5],
			OptionFunction = GetFunction(int.Parse(data[6])),
		};
		Options = new() { option1, option2 };
	}

	EventOptionFunc GetFunction(int funcToCall)
	{
		return funcToCall switch
		{
			1 => Add50Gold,
			_ => Add100Gold,
		};
	}

	public delegate void EventOptionFunc(ref RunData runData);
}
