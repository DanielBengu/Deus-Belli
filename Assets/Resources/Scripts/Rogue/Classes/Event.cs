using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
				List<GameObject> unitList = ((List<Unit>)eventEntity.objToAdd).Select(u => u.gameObject).ToList();
				FileManager.SaveUnits(unitList, false);
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
		string[] eventParse1 = data[4].Split(',');
		List<EventFunction> funcList1 = new();
		for (int i = 0; i < eventParse1.Length; i++)
		{
			char typeOfEvent = eventParse1[i][0];
			object eventValue = GetEventValue(typeOfEvent, eventParse1[i].Remove(0, 1));
			funcList1.Add(new(GetTypeOfObject(typeOfEvent), eventValue, GetFunction(GetEvent(typeOfEvent))));
		}
		string[] eventParse2 = data[6].Split(',');
		List<EventFunction> funcList2 = new();
		for (int i = 0; i < eventParse2.Length; i++)
		{
			char typeOfEvent = eventParse2[i][0];
			object eventValue = GetEventValue(typeOfEvent, eventParse2[i].Remove(0, 1));
			funcList2.Add(new(GetTypeOfObject(typeOfEvent), eventValue, GetFunction(GetEvent(typeOfEvent))));
		}
		EventOption option1 = new(this)
		{
			OptionDescription = data[3],
			OptionFunction = funcList1,
		};
		EventOption option2 = new(this)
		{
			OptionDescription = data[5],
			OptionFunction = funcList2,
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
			'u' => FileManager.GetUnits(FileManager.DataSource.Custom, value.Split(';')),
			_ => 0,
		};
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
