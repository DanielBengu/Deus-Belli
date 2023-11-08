using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorManager : MonoBehaviour
{
	[SerializeField] Image carousel0;
	[SerializeField] Image carousel1;
	[SerializeField] Image carousel2;
	[SerializeField] Image carousel3;
	[SerializeField] Image carousel4;
	[SerializeField] Image carousel5;
	[SerializeField] Transform tileParents;

	Image currentCarousel;
	GameObject itemSelected;
	

	private void Start()
	{
		currentCarousel = carousel0;
	}

	public void ItemFromCarouselSelected(int item)
	{
		//First we reset back the selection
		currentCarousel.color = new Color(255, 255, 255);

		switch (item)
		{
			case 0:
				currentCarousel = carousel0;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "Grass1");
				break;
			case 1:
				currentCarousel = carousel1;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "PineTree");
				break;
			case 2:
				currentCarousel = carousel2;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "GrassPath");
				break;
			case 3:
				currentCarousel = carousel3;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "Mountain");
				break;
			case 4:
				currentCarousel = carousel4;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "Tree");
				break;
			case 5:
				currentCarousel = carousel5;
				itemSelected = AddressablesManager.LoadResource<GameObject>(AddressablesManager.TypeOfResource.Terrains, "BirchTree");
				break;
		}
		currentCarousel.color = new Color(170, 170, 170);
	}

	public void ChangeTile(Tile tile)
	{
		GameObject newTile = Instantiate(itemSelected, tileParents);
		newTile.name = tile.name;
		newTile.transform.SetPositionAndRotation(tile.transform.position, tile.transform.rotation);
		Tile tileScript = newTile.GetComponent<Tile>();
		tileScript.tileNumber = tile.tileNumber;
		tileScript.isEdit = tile.isEdit;
		Destroy(tile.gameObject);
	}
}
