using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Merchant
{
	public MerchantItem[] ItemList { get; set; }

	public Merchant(int seed)
	{
		GenerateMerchantList(seed);
	}

	public bool BuyItem(int itemIndex, int availableGold, out int newGoldAmount)
	{
		newGoldAmount = availableGold;
		MerchantItem itemToBuy = ItemList[itemIndex];

		//Already bought/unavailable
		if (itemToBuy.Price == -1)
			return false;

		//Poor bitch
		if (availableGold < itemToBuy.Price)
			return false;

		newGoldAmount -= itemToBuy.Price;
		itemToBuy.Price = -1;
		return true;
	}

	void GenerateMerchantList(int seed)
	{
		string[] items = GetMerchantItems();

		ItemList = new MerchantItem[items.Length];

		for (int i = 0; i < items.Length; i++)
		{
			string[] data = items[i].Split(';');
			Unit unit = Resources.Load<GameObject>($"Prefabs/Units/{data[1]}").GetComponent<Unit>();
			if (int.Parse(data[0]) == (int)ItemType.Unit)
				ItemList[i] = new(unit, "Warrior", int.Parse(data[2]));
		}
	}

	string[] GetMerchantItems()
	{
		return File.ReadAllLines("Assets\\Resources\\Scripts\\Rogue\\TXT\\Merchant.txt");
	}
}

public struct MerchantItem{
	public object Item { get; set; }
	public string Name { get; set; }
	public int Price { get; set; }

	public MerchantItem(object item, string name, int price)
	{
		Item = item;
		Name = name;
		Price = price;
	}
}

enum ItemType
{
	Unit,
}

enum ItemPool
{
	Neutral,
	Zeus
}