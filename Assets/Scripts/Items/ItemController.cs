using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
	public const int ITEM_ID_INVALID = -1;
	public const int ITEM_ID_PELLET_SMALL = 0;
	public const int ITEM_ID_PELLET_BIG = 1;

	public int itemID;
	public int rewardScore;

	private SimpleAnimator itemAnimator;
	private PlayerController playerController;
	private bool over;

	void Awake()
	{
		itemAnimator = GetComponent<SimpleAnimator>();
		playerController = null;
		over = false;
	}

	public void SetPlayerController( PlayerController newPlayerController )
	{
		playerController = newPlayerController;
	}

	public void Progress( float timeStep )
	{
		if( !over )
		{
			if( itemAnimator != null )
			{
				itemAnimator.Progress( timeStep );
				if( itemAnimator.IsOver() )
				{
					over = true;
				}
			}
		}
	}

	void OnTriggerEnter2D( Collider2D otherCollider )
	{
		if( playerController != null )
		{
			if( otherCollider.gameObject == playerController.gameObject )
			{
				playerController.ReceiveItem( itemID, rewardScore );
				over = true;
			}
		}
	}

	public bool IsOver()
	{
		return over;
	}

	public void Clear()
	{
		if( itemAnimator != null )
		{
			itemAnimator.Clear();
			itemAnimator = null;
		}
		playerController = null;
	}
}
