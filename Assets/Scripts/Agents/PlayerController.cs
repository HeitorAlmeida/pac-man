using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	public const int DIRECTION_INVALID = -1;
	public const int SMALLEST_DIRECTION = 0;
	public const int DIRECTION_UP = 0;
	public const int DIRECTION_LEFT = 1;
	public const int DIRECTION_DOWN = 2;
	public const int DIRECTION_RIGHT = 3;
	public const int BIGGEST_DIRECTION = 3;
	public const int TOTAL_DIRECTIONS = 4;

	public float movementSpeed;
	public bool debug;

	protected bool[] pushingDirections;
	protected bool[] goingDirections;
	protected Vector2 speeds;
	protected bool dying;
	protected bool dead;

	/*halmeida - using design pattern "Factory Method". PlayerController has a PlayerAnimator, but its derived class
	EnemyController needs an EnemyAnimator. For that reason the animator component is private to the class and every
	operation performed on the animator is done through virtual methods.*/
	private PlayerAnimator playerAnimator;
	private int score;

	protected virtual void Awake()
	{
		movementSpeed = (movementSpeed < 0f) ? 0f : movementSpeed; 
		pushingDirections = new bool[TOTAL_DIRECTIONS];
		goingDirections = new bool[TOTAL_DIRECTIONS];
		for( int i=0; i<TOTAL_DIRECTIONS; i++ )
		{
			pushingDirections[i] = false;
			goingDirections[i] = false;
		}
		speeds = Vector2.zero;
		dying = false;
		dead = false;
		playerAnimator = null;
		ClearAnimatorComponent();
		ExtractAnimatorComponent();
		score = 0;
	}

	protected virtual void ClearAnimatorComponent()
	{
		if( playerAnimator != null )
		{
			playerAnimator.Clear();
			playerAnimator = null;
		}
	}

	protected virtual void ExtractAnimatorComponent()
	{
		playerAnimator = GetComponent<PlayerAnimator>();
	}

	protected virtual bool ExistsAnimatorComponent()
	{
		return (playerAnimator != null);
	}

	protected virtual void FeedAnimatorComponentVariables()
	{
		FeedAnimatorComponentSpeeds( speeds );
		if( goingDirections != null )
		{
			if( goingDirections[DIRECTION_UP] || goingDirections[DIRECTION_DOWN] || goingDirections[DIRECTION_RIGHT] )
			{
				FeedAnimatorComponentSide( BaseAnimator.SIDE_RIGHT );
			}
			if( goingDirections[DIRECTION_LEFT] )
			{
				FeedAnimatorComponentSide( BaseAnimator.SIDE_LEFT );
			}
		}
	}

	protected virtual void FeedAnimatorComponentSpeeds( Vector2 newSpeeds )
	{
		if( playerAnimator != null )
		{
			playerAnimator.SetSpeeds( newSpeeds );
		}
	}

	protected virtual void FeedAnimatorComponentSide( bool newSide )
	{
		if( playerAnimator != null )
		{
			playerAnimator.SetSide( newSide );
		}
	} 

	protected virtual void ToggleAnimatorComponentFreeze( bool newFreeze )
	{
		if( playerAnimator != null )
		{
			playerAnimator.ToggleSpriteFreeze( newFreeze );
		}
	}

	protected virtual void ProgressAnimatorComponent( float timeStep )
	{
		if( playerAnimator != null )
		{
			playerAnimator.Progress( timeStep );
			if( dying && playerAnimator.IsDead() )
			{
				dying = false;
				dead = true;
			}
		}
	}

	public void ReceiveItem( int itemID, int itemRewardScore )
	{
		IncreaseScore( itemRewardScore );
		if( itemID == ItemController.ITEM_ID_PELLET_BIG )
		{
			/*halmeida - do some sort of transformation if I have time to.*/
		}
	}

	public void IncreaseScore( int additionalScore )
	{
		score += additionalScore;
	}

	public virtual void StartDying()
	{
		if( !dead && !dying )
		{
			if( playerAnimator != null )
			{
				if( !playerAnimator.IsDying() )
				{
					playerAnimator.ToggleSpriteFreeze( false );
					playerAnimator.StartDying();
					dying = true;
				}
			}
			else
			{
				dead = true;
			}
			CancelInput();
			StopGoing( false );
		}
	}

	public virtual void SetCurrentRowAndColumn( int row, int column )
	{
		/*halmeida - reserved for AI provided subclasses.*/
	}

	public virtual void GetCurrentRowAndColumn( ref int row, ref int column )
	{
		/*halmeida - reserved for AI provided subclasses.*/
	}

	public virtual void SetPath( Vector2[] newPathSlots, Vector2[] newPathPositions )
	{
		/*halmeida - reserved for AI provided subclasses.*/
	}

	public virtual void Progress( float timeStep )
	{
		ProgressAnimation( timeStep );
	}

	protected virtual void ProgressAnimation( float timeStep )
	{
		FeedAnimatorComponentVariables();
		ProgressAnimatorComponent( timeStep );
	}

	public Vector2 GetIntendedOffset( float timeStep )
	{
		return speeds * timeStep;
	}

	public void TogglePushing( int direction, bool enable )
	{
		if( (pushingDirections != null) && (direction >= SMALLEST_DIRECTION) && (direction <= BIGGEST_DIRECTION) && !dying && !dead )
		{
			pushingDirections[direction] = enable;
		}
	}

	public bool IsPushing( int direction )
	{
		if( (pushingDirections != null) && (direction >= SMALLEST_DIRECTION) && (direction <= BIGGEST_DIRECTION) )
		{
			return pushingDirections[direction];
		}
		return false;
	}

	public void StartGoingTowards( int direction )
	{
		float newMovementSpeed = 0f;

		if( IsAbleToMove() )
		{
			if( (goingDirections != null) && (direction >= SMALLEST_DIRECTION) && (direction <= BIGGEST_DIRECTION) )
			{
				/*halmeida - movements towards different directions are mutually exclusive. The player can only move
				towards one direction at any given time.*/
				for( int i=0; i<goingDirections.Length; i++ )
				{
					goingDirections[i] = (i == direction);
				}
				newMovementSpeed = GetProperMovementSpeed();
				switch( direction )
				{
					case DIRECTION_UP:
						speeds = new Vector2( 0f, newMovementSpeed );
						break;
					case DIRECTION_LEFT:
						speeds = new Vector2( -newMovementSpeed, 0f );
						break;
					case DIRECTION_DOWN:
						speeds = new Vector2( 0f, -newMovementSpeed );
						break;
					case DIRECTION_RIGHT:
						speeds = new Vector2( newMovementSpeed, 0f );
						break;
				}
				ToggleAnimatorComponentFreeze( false );
				/*if( debug )
				{
					if( IsGoingTowards( direction ) )
					{
						Debug.Log("Debug : PlayerController : started going to direction "+direction+".");
					}
					else
					{
						Debug.Log("Debug : PlayerController : failed to go to direction "+direction+".");
					}
				}*/
			}
		}
	}

	public virtual bool IsAbleToMove()
	{
		return (!dying && !dead);
	}

	public virtual bool CanMoveWithoutTarget()
	{
		return true;
	}

	protected virtual float GetProperMovementSpeed()
	{
		return movementSpeed;
	}

	public bool IsGoingTowards( int direction )
	{
		if( (goingDirections != null) && (direction >= SMALLEST_DIRECTION) && (direction <= BIGGEST_DIRECTION) )
		{
			return goingDirections[direction];
		}
		return false;
	}

	public void CancelInput()
	{
		if( pushingDirections != null )
		{
			for( int i=0; i<TOTAL_DIRECTIONS; i++ )
			{
				pushingDirections[i] = false;
			}
		}
	}

	public void StopGoing( bool freezeSprite )
	{
		if( goingDirections != null )
		{
			for( int i=0; i<goingDirections.Length; i++ )
			{
				goingDirections[i] = false;
			}
			speeds = Vector2.zero;
			/*halmeida - when pac-man hits a wall, he stops and his animation stops as well.*/
			if( freezeSprite )
			{
				ToggleAnimatorComponentFreeze( true );
			}
		}
	}

	public Vector2 GetSpeeds()
	{
		return speeds;
	}

	public bool IsDead()
	{
		return dead;
	}

	public virtual bool HasAI()
	{
		return false;
	}

	public virtual bool HasSlotTarget( ref int targetSlotRow, ref int targetSlotColumn )
	{
		return false;
	}

	public virtual bool HasPositionTarget( ref Vector2 targetWorldPosition )
	{
		return false;
	}

	public virtual void AdvanceTarget()
	{
		/*halmeida - reserved for AI provided subclasses.*/
	}

	public virtual int SetDirectionFromVicinityOccupation( bool up, bool left, bool down, bool right )
	{
		/*halmeida - reserved for AI provided subclasses.*/
		return DIRECTION_INVALID;
	}

	public virtual void Clear()
	{
		ClearAnimatorComponent();
		pushingDirections = null;
		goingDirections = null;
	}
}
