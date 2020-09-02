using UnityEngine;
using System.Collections;

public class ItemDatabase : MonoBehaviour
{
	public const int ITEM_CODE_INVALID = -1;
	public const int SMALLEST_ITEM_CODE = 0;
	public const int ITEM_CODE_PELLET_SMALL = 0;
	public const int ITEM_CODE_PELLET_BIG = 1;
	public const int ITEM_CODE_FRUIT_CHERRY = 2;
	public const int ITEM_CODE_FRUIT_STRAWBERRY = 3;
	public const int BIGGEST_ITEM_CODE = 3;

	public GameObject modelPelletSmall;
	public GameObject modelPelletBig;
	public GameObject modelFruitCherry;
	public GameObject modelFruitStrawberry;

	void Awake()
	{
		/*halmeida - do nothing.*/
	}

	public GameObject GetItemModel( int itemCode )
	{
		GameObject model = null;

		switch( itemCode )
		{
			case ITEM_CODE_PELLET_SMALL:
				model = modelPelletSmall;
				break;
			case ITEM_CODE_PELLET_BIG:
				model = modelPelletBig;
				break;
			case ITEM_CODE_FRUIT_CHERRY:
				model = modelFruitCherry;
				break;
			case ITEM_CODE_FRUIT_STRAWBERRY:
				model = modelFruitStrawberry;
				break;
		}
		return model;
	}

	public string GetItemName( int itemCode )
	{
		string name = "Invalid item name";

		switch( itemCode )
		{
			case ITEM_CODE_PELLET_SMALL:
				name = "Small Pellet";
				break;
			case ITEM_CODE_PELLET_BIG:
				name = "Power Pellet";
				break;
			case ITEM_CODE_FRUIT_CHERRY:
				name = "Cherry";
				break;
			case ITEM_CODE_FRUIT_STRAWBERRY:
				name = "Strawberry";
				break;
		}
		return name;
	}
}

