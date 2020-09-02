using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : PlayerController
{
	public const int AI_STATE_INVALID = -1;
	public const int AI_STATE_CAGED = 0;
	public const int AI_STATE_CENTERING = 1;
	public const int AI_STATE_LEAVING = 2;
	public const int AI_STATE_FREE = 3;
	public const int AI_STATE_DEAD = 4;

	public float movementSpeedReduced;
	public int rewardScore;
	public float cagedDuration;
	public float agressivityPercentage;

	/*halmeida - using design pattern "Factory Method". PlayerController has a PlayerAnimator, but its derived class
	EnemyController needs an EnemyAnimator. For that reason the animator component is private to the class and every
	operation performed on the animator is done through virtual methods.*/
	private EnemyAnimator enemyAnimator;
	private bool fragile;
	private float fragileDuration;
	private float fragileElapsed;
	private bool recovering;
	private float recoveringDuration;
	private PlayerController playerController;
	private int inteligenceState;
	private float cagedElapsed;
	private Vector2[] pathSlots;
	private int pathSlotsIndex;
	private Vector2[] pathPositions;
	private int pathPositionsIndex;
	private bool pathCompleted;
	private int currentRow;
	private int currentColumn;
	private int targetRow;
	private int targetColumn;
	private Vector2 targetPosition;
	private bool targetSet;

	protected override void Awake()
	{
		enemyAnimator = null;
		fragile = false;
		fragileDuration = 0f;
		fragileElapsed = 0f;
		recovering = false;
		recoveringDuration = 0f;
		playerController = null;
		inteligenceState = AI_STATE_CAGED;
		cagedElapsed = 0f;
		pathSlots = null;
		pathSlotsIndex = -1;
		pathPositions = null;
		pathPositionsIndex = -1;
		pathCompleted = false;
		currentRow = -1;
		currentColumn = -1;
		targetRow = -1;
		targetColumn = -1;
		targetPosition = Vector2.zero;
		targetSet = false;
		base.Awake();
	}

	protected override void ClearAnimatorComponent()
	{
		if( enemyAnimator != null )
		{
			enemyAnimator.Clear();
			enemyAnimator = null;
		}
	}

	protected override void ExtractAnimatorComponent()
	{
		enemyAnimator = GetComponent<EnemyAnimator>();
	}

	protected override bool ExistsAnimatorComponent()
	{
		return (enemyAnimator != null);
	}

	protected override void FeedAnimatorComponentSpeeds( Vector2 newSpeeds )
	{
		if( enemyAnimator != null )
		{
			enemyAnimator.SetSpeeds( newSpeeds );
		}
	}

	protected override void FeedAnimatorComponentSide( bool newSide )
	{
		if( enemyAnimator != null )
		{
			enemyAnimator.SetSide( newSide );
		}
	} 

	protected override void ProgressAnimatorComponent( float timeStep )
	{
		if( enemyAnimator != null )
		{
			enemyAnimator.Progress( timeStep );
		}
	}

	public void BecomeFragile( float newFragileDuration, float newRecoveringDuration )
	{
		int newMovementDirection = DIRECTION_INVALID;

		if( !dead )
		{
			fragile = true;
			fragileDuration = newFragileDuration;
			fragileElapsed = 0f;
			recovering = false;
			recoveringDuration = newRecoveringDuration;
			if( enemyAnimator != null )
			{
				enemyAnimator.ToggleFragility( true );
			}
			CancelInput();
			if( IsGoingTowards( DIRECTION_UP ) )
			{
				newMovementDirection = DIRECTION_DOWN;
			}
			else if( IsGoingTowards( DIRECTION_LEFT ) )
			{
				newMovementDirection = DIRECTION_RIGHT;
			}
			else if( IsGoingTowards( DIRECTION_DOWN ) )
			{
				newMovementDirection = DIRECTION_UP;
			}
			else if( IsGoingTowards( DIRECTION_RIGHT ) )
			{
				newMovementDirection = DIRECTION_LEFT;
			}
			TogglePushing( newMovementDirection, true );
			StartGoingTowards( newMovementDirection );
		}
	}

	public override void StartDying()
	{
		if( !dead )
		{
			dead = true;
			fragile = false;
			recovering = false;
			if( enemyAnimator != null )
			{
				enemyAnimator.ToggleLife( false );
			}
			CancelInput();
			StopGoing( false );
			inteligenceState = AI_STATE_DEAD;
			pathCompleted = false;
		}
	}

	public void StartHuntingBehaviour()
	{
		if( !dead )
		{
			inteligenceState = AI_STATE_FREE;
		}
	}

	public override void SetCurrentRowAndColumn( int row, int column )
	{
		currentRow = row;
		currentColumn = column;
	}

	public override void GetCurrentRowAndColumn( ref int row, ref int column )
	{
		row = currentRow;
		column = currentColumn;
	}

	public int GetCurrentAIState()
	{
		return inteligenceState;
	}

	public override void SetPath( Vector2[] newPathSlots, Vector2[] newPathPositions )
	{
		targetSet = false;
		pathSlotsIndex = -1;
		targetRow = -1;
		targetColumn = -1;
		pathPositionsIndex = -1;
		pathCompleted = false;
		pathSlots = newPathSlots;
		pathPositions = newPathPositions;
		if( pathSlots != null )
		{
			pathSlotsIndex = 0;
			targetRow = (int)(pathSlots[0].x);
			targetColumn = (int)(pathSlots[0].y);
			targetSet = true;
		}
		if( !targetSet )
		{
			if( pathPositions != null )
			{
				pathPositionsIndex = 0;
				targetPosition = pathPositions[0];
				targetSet = true;
			}
		}
	}

	public void Resurrect()
	{
		if( dead )
		{
			dead = false;
			if( enemyAnimator != null )
			{
				enemyAnimator.ToggleLife( true );
			}
		}
	}

	public override void Progress( float timeStep )
	{
		ProgressFragility( timeStep );
		ProgressAI( timeStep );
		base.Progress( timeStep );
	}

	private void ProgressFragility( float timeStep )
	{
		float fragileTimeLeft = 0f;

		if( fragile )
		{
			fragileElapsed += timeStep;
			fragileTimeLeft = fragileDuration - fragileElapsed;
			if( fragileTimeLeft <= 0f )
			{
				fragile = false;
				if( enemyAnimator != null )
				{
					enemyAnimator.ToggleFragility( false );
				}
			}
			else
			{
				if( !recovering && (fragileTimeLeft < recoveringDuration) )
				{
					recovering = true;
					if( enemyAnimator != null )
					{
						enemyAnimator.ToggleRecovery( true );
					}
				}
			}
		}
	}

	private void ProgressAI( float timeStep )
	{
		switch( inteligenceState )
		{
			case AI_STATE_CAGED:
				cagedElapsed += timeStep;
				if( cagedElapsed > cagedDuration )
				{
					inteligenceState = AI_STATE_CENTERING;
					pathCompleted = false;
				}
				break;
			case AI_STATE_CENTERING:
				if( pathCompleted )
				{
					inteligenceState = AI_STATE_LEAVING;
					pathCompleted = false;
				}
				break;
			case AI_STATE_LEAVING:
				if( pathCompleted )
				{
					inteligenceState = AI_STATE_FREE;
					pathCompleted = false;
				}
				break;
			case AI_STATE_FREE:
				break;
			case AI_STATE_DEAD:
				if( pathCompleted )
				{
					Resurrect();
					if( !dead )
					{
						inteligenceState = AI_STATE_LEAVING;
						pathCompleted = false;
					}
				}
				break;
		}
	}

	public override bool IsAbleToMove()
	{
		return true;
	}

	public override bool CanMoveWithoutTarget()
	{
		switch( inteligenceState )
		{
			case AI_STATE_CAGED:
				return true;
			case AI_STATE_CENTERING:
				return false;
			case AI_STATE_LEAVING:
				return false;
			case AI_STATE_FREE:
				return true;
			case AI_STATE_DEAD:
				return false;
		}
		return false;
	}

	protected override float GetProperMovementSpeed()
	{
		float properMovementSpeed = 0f;

		properMovementSpeed = fragile ? movementSpeedReduced : movementSpeed;
		return properMovementSpeed;
	}

	public override bool HasAI()
	{
		return true;
	}

	public override bool HasSlotTarget( ref int targetSlotRow, ref int targetSlotColumn )
	{
		targetSlotRow = -1;
		targetSlotColumn = -1;
		if( targetSet && (pathSlotsIndex > -1) )
		{
			targetSlotRow = targetRow;
			targetSlotColumn = targetColumn;
			return true;
		}
		return false;
	}

	public override bool HasPositionTarget( ref Vector2 targetWorldPosition )
	{
		targetWorldPosition = Vector2.zero;
		if( targetSet && (pathPositionsIndex > -1) )
		{
			targetWorldPosition = targetPosition;
			return true;
		}
		return false;
	}

	public override void AdvanceTarget()
	{
		Vector2 newTargetRowAndColumn = Vector2.zero;
		bool slotsCompleted = true;
		bool positionsCompleted = true;

		targetSet = false;
		if( (pathSlots != null) && (pathSlotsIndex > -1) )
		{
			pathSlotsIndex++;
			if( pathSlotsIndex < pathSlots.Length )
			{
				newTargetRowAndColumn = pathSlots[pathSlotsIndex];
				targetRow = (int)newTargetRowAndColumn.x;
				targetColumn = (int)newTargetRowAndColumn.y;
				targetSet = true;
				slotsCompleted = false;
			}
			else
			{
				pathSlots = null;
				pathSlotsIndex = -1;
				targetRow = -1;
				targetColumn = -1;
			}
		}
		if( slotsCompleted && (pathPositions != null) )
		{
			if( pathPositionsIndex == -1 )
			{
				pathPositionsIndex = 0;
			}
			else
			{
				pathPositionsIndex++;
			}
			if( pathPositionsIndex < pathPositions.Length )
			{
				targetPosition = pathPositions[pathPositionsIndex];
				targetSet = true;
				positionsCompleted = false;
			}
			else
			{
				pathPositions = null;
				pathPositionsIndex = -1;
			}
		}
		pathCompleted = slotsCompleted && positionsCompleted;
	}

	public bool CompletedThePath()
	{
		return pathCompleted;
	}

	public override int SetDirectionFromVicinityOccupation( bool blockedUp, bool blockedLeft, bool blockedDown, bool blockedRight )
	{
		int newDirection = DIRECTION_INVALID;

		CancelInput();
		newDirection = CreateValidInputRandom( blockedUp, blockedLeft, blockedDown, blockedRight );
		return newDirection;
	}

	private int CreateValidInputRandom( bool blockedUp, bool blockedLeft, bool blockedDown, bool blockedRight )
	{
		int validDirections = 0;
		int directionCounter = 0;
		int newDirection = DIRECTION_INVALID;
		bool beAgressive = false;
		Vector3 adversaryLocalPosition = Vector3.zero;
		bool adversaryUp = false;
		bool adversaryLeft = false;
		bool adversaryDown = false;
		bool adversaryRight = false;
		int random = 0;

		if( CanMoveWithoutTarget() )
		{
			random = Random.Range( 1, 100 );
			beAgressive = (random <= agressivityPercentage);
			if( beAgressive && (playerController != null) )
			{
				/*halmeida - accessing transform.localPosition is faster than accessing transform.position. Since
				they are both parented by the same object, we can safely use localPosition.*/
				adversaryLocalPosition = playerController.gameObject.transform.localPosition;
				adversaryUp = adversaryLocalPosition.y > transform.localPosition.y;
				adversaryLeft = adversaryLocalPosition.x < transform.localPosition.x;
				adversaryDown = adversaryLocalPosition.y < transform.localPosition.y;
				adversaryRight = adversaryLocalPosition.x > transform.localPosition.x;
				validDirections += (!blockedUp && adversaryUp) ? 1 : 0;
				validDirections += (!blockedLeft && adversaryLeft) ? 1 : 0;
				validDirections += (!blockedDown && adversaryDown) ? 1 : 0;
				validDirections += (!blockedRight && adversaryRight) ? 1 : 0;
				if( validDirections > 0 )
				{
					directionCounter = Random.Range( 1, validDirections+1 );
					directionCounter -= (!blockedUp && adversaryUp) ? 1 : 0;
					if( directionCounter == 0 )
					{
						newDirection = DIRECTION_UP;
					}
					else
					{
						directionCounter -= (!blockedLeft && adversaryLeft) ? 1 : 0;
						if( directionCounter == 0 )
						{
							newDirection = DIRECTION_LEFT;
						}
						else
						{
							directionCounter -= (!blockedDown && adversaryDown) ? 1 : 0;
							if( directionCounter == 0 )
							{
								newDirection = DIRECTION_DOWN;
							}
							else
							{
								directionCounter -= (!blockedRight && adversaryRight) ? 1 : 0;
								if( directionCounter == 0 )
								{
									newDirection = DIRECTION_RIGHT;
								}
							}
						}
					}
				}
			}
			if( newDirection == DIRECTION_INVALID )
			{
				/*halmeida - either we are not hunting the adversary or it is currently not possible to hunt the adversary.*/
				validDirections = 0;
				validDirections += !blockedUp ? 1 : 0;
				validDirections += !blockedLeft ? 1 : 0;
				validDirections += !blockedDown ? 1 : 0;
				validDirections += !blockedRight ? 1 : 0;
				if( validDirections > 0 )
				{
					directionCounter = Random.Range( 1, validDirections+1 );
					directionCounter -= !blockedUp ? 1 : 0;
					if( directionCounter == 0 )
					{
						newDirection = DIRECTION_UP;
					}
					else
					{
						directionCounter -= !blockedLeft ? 1 : 0;
						if( directionCounter == 0 )
						{
							newDirection = DIRECTION_LEFT;
						}
						else
						{
							directionCounter -= !blockedDown ? 1 : 0;
							if( directionCounter == 0 )
							{
								newDirection = DIRECTION_DOWN;
							}
							else
							{
								directionCounter -= !blockedRight ? 1 : 0;
								if( directionCounter == 0 )
								{
									newDirection = DIRECTION_RIGHT;
								}
							}
						}
					}
				}
			}
		}
		if( newDirection != DIRECTION_INVALID )
		{
			TogglePushing( newDirection, true );
			StartGoingTowards( newDirection );
		}
		else
		{
			StopGoing( true );
		}
		return newDirection;
	}

	public override void Clear()
	{
		ClearAnimatorComponent();
		pushingDirections = null;
		goingDirections = null;
	}

	public void SetAdversary( PlayerController newPlayerController )
	{
		playerController = newPlayerController;
	}

	void OnTriggerEnter2D( Collider2D otherCollider )
	{
		if( !dead && (playerController != null) )
		{
			if( otherCollider.gameObject == playerController.gameObject )
			{
				if( fragile )
				{
					playerController.IncreaseScore( rewardScore );
					StartDying();
				}
				else
				{
					playerController.StartDying();
				}
			}
		}
	}
}
