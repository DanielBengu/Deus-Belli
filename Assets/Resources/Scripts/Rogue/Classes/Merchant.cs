using System.Collections.Generic;
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
		Unit unit = Resources.Load<GameObject>("Prefabs/Units/Male A").GetComponent<Unit>();
		ItemList = new MerchantItem[1];
		ItemList[0] = new(unit, 100);
	}
}

public struct MerchantItem{
	public object Item { get; set; }
	public int Price { get; set; }

	public MerchantItem(object item, int price)
	{
		Item = item;
		Price = price;
	}
}
