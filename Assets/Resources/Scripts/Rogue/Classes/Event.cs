using System;
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

		int randomLength = RandomManager.GetRandomValue(seed, 0, possibleEvents.Length);

		LoadEvent(possibleEvents[randomLength]);
	}

	public void AddGold(ref EventEntity eventEntity)
	{
		switch (eventEntity.objToAddEnum)
		{
			case ObjectToAdd.Gold:
				eventEntity.runData.gold += Convert.ToInt32(eventEntity.objToAdd);
				break;
			case ObjectToAdd.Unit:
				break;
		}
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
		string[] eventParse = data[4].Split(',');
		char typeOfEvent = eventParse[0][0];
		object eventValue = GetEventValue(typeOfEvent, eventParse[1].Remove(0, 1));
		EventFunction func = new(GetTypeOfObject(typeOfEvent), eventValue, GetFunction(GetEvent(typeOfEvent)));
		EventOption option1 = new(this)
		{
			OptionDescription = data[3],
			OptionFunction = new() { func },
		};
		EventOption option2 = new(this)
		{
			OptionDescription = data[5],
			OptionFunction = new() { func },
		};
		Options = new() { option1, option2 };
	}

	EventOptionFunc GetFunction(int funcToCall)
	{
		return funcToCall switch
		{
			1 => AddGold,
			_ => AddGold,
		};
	}

	int GetEvent(char typeOfEvent)
	{
		return typeOfEvent switch
		{
			'g' => 1,
			'u' => 2,
			_ => 0,
		};
	}

	object GetEventValue(char typeOfEvent, string value)
	{
		return typeOfEvent switch
		{
			'g' => value,
			'u' => 2,
			_ => 0,
		};
	}

	Unit ConvertDataToUnit(string data)
	{
		return null;
	}

	ObjectToAdd GetTypeOfObject(char type)
	{
		return type switch
		{
			'g' => ObjectToAdd.Gold,
			'u' => ObjectToAdd.Unit,
			_ => ObjectToAdd.Default
		};
	}

	public delegate void EventOptionFunc(ref EventEntity runData);
}
