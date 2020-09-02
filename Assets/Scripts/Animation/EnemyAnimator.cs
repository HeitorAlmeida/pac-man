using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : BaseAnimator
{
	public const int STATUS_ALIVE_IDLE = 0;
	public const int STATUS_ALIVE_MOVING_SIDES = 1;
	public const int STATUS_ALIVE_MOVING_UP = 2;
	public const int STATUS_ALIVE_MOVING_DOWN = 3;
	public const int STATUS_ALIVE_FRAGILE = 4;
	public const int STATUS_ALIVE_RECOVERING = 5;
	public const int STATUS_DEAD_IDLE = 6;
	public const int STATUS_DEAD_MOVING_SIDES = 7;
	public const int STATUS_DEAD_MOVING_UP = 8;
	public const int STATUS_DEAD_MOVING_DOWN = 9;

	public GameObject sequenceAliveIdle;
	public GameObject sequenceAliveMovingRight;
	public GameObject sequenceAliveMovingUp;
	public GameObject sequenceAliveMovingDown;
	public GameObject sequenceAliveFragile;
	public GameObject sequenceAliveRecovering;
	public GameObject sequenceDeadIdle;
	public GameObject sequenceDeadMovingRight;
	public GameObject sequenceDeadMovingUp;
	public GameObject sequenceDeadMovingDown;

	private SpriteRenderer bodyRenderer;
	private bool side;
	private bool previousSide;
	private Vector2 speeds;
	private bool fragile;
	private bool recovering;
	private bool dead;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Clear( bool renderingComponentInitialized = true )
	{
		base.Clear( renderingComponentInitialized );
		side = SIDE_RIGHT;
		previousSide = SIDE_RIGHT;
		speeds = Vector2.zero;
		fragile = false;
		recovering = false;
		dead = false;
	}

	protected override void ClearRenderingComponent()
	{
		bodyRenderer = null;
	}

	protected override bool ExtractRenderingComponent()
	{
		bodyRenderer = gameObject.GetComponent<SpriteRenderer>();
		return (bodyRenderer != null);
	}

	protected override void FeedRenderingComponent( Sprite newSprite, bool newSpriteFlip )
	{
		Vector4 completeFlip = Vector4.one;

		if( bodyRenderer != null )
		{
			bodyRenderer.sprite = newSprite;
			bodyRenderer.enabled = (newSprite != null);
			bodyRenderer.flipX = newSpriteFlip;
		}
	}

	protected override Color ExtractRenderingColor()
	{
		if( bodyRenderer != null )
		{
			return bodyRenderer.color;
		}
		return base.ExtractRenderingColor();
	}

	protected override void FeedRenderingColor( Color newColor )
	{
		if( bodyRenderer != null )
		{
			bodyRenderer.color = newColor;
		}
	}

	public override void SetSide( bool newSide )
	{
		side = newSide;
		base.SetSide( newSide );
	}

	public override bool GetSide()
	{
		return side;
	}

	public void SetSpeeds( Vector2 newSpeeds )
	{
		speeds = newSpeeds;
	}

	public void ToggleFragility( bool newFragility )
	{
		if( !over )
		{
			if( newFragility && !dead )
			{
				fragile = true;
				recovering = false;
			}
			else if( !newFragility )
			{
				fragile = false;
				recovering = false;
			}
		}
	}

	public void ToggleRecovery( bool newRecovery )
	{
		if( !over )
		{
			if( newRecovery && !dead && fragile && !recovering )
			{
				recovering = true;
			}
			else if( !newRecovery )
			{
				recovering = false;
			}
		}
	}

	public void ToggleLife( bool becomeAlive )
	{
		if( !over )
		{
			if( dead && becomeAlive )
			{
				dead = false;
			}
			else if( !dead && !becomeAlive )
			{
				dead = true;
				fragile = false;
				recovering = false;
			}
		}
	}

	public override void StartDying()
	{
		ToggleLife( false );
	}

	protected override void UpdateStatus()
	{
		if( (status == INVALID_STATUS) && !over )
		{
			status = STATUS_ALIVE_IDLE;
		}
		if( dead )
		{
			if( speeds == Vector2.zero )
			{
				status = STATUS_DEAD_IDLE;
			}
			else
			{
				if( speeds.x != 0f )
				{
					status = STATUS_DEAD_MOVING_SIDES;
				}
				else if( speeds.y > 0f )
				{
					status = STATUS_DEAD_MOVING_UP;
				}
				else
				{
					status = STATUS_DEAD_MOVING_DOWN;
				}
			}
		}
		else
		{
			if( fragile )
			{
				if( recovering )
				{
					status = STATUS_ALIVE_RECOVERING;
				}
				else
				{
					status = STATUS_ALIVE_FRAGILE;
				}
			}
			else
			{
				if( speeds == Vector2.zero )
				{
					status = STATUS_ALIVE_IDLE;
				}
				else
				{
					if( speeds.x != 0f )
					{
						status = STATUS_ALIVE_MOVING_SIDES;
					}
					else if( speeds.y > 0f )
					{
						status = STATUS_ALIVE_MOVING_UP;
					}
					else
					{
						status = STATUS_ALIVE_MOVING_DOWN;
					}
				}
			}
		}
	}

	protected override void AdvanceToNextStatus()
	{
		/*halmeida - Every state animation should loop and never go to a next state naturally.*/
		status = INVALID_STATUS;
	}

	protected override bool RequiresNewSequence()
	{ 
		return ( (previousStatus != status) || (previousSide != side) );
	}

	protected override void RecordChangeVerifiers()
	{
		base.RecordChangeVerifiers();
		previousSide = side;
	}

	protected override void FillSequencesArray()
	{
		totalSequences = 10;
		sequenceObjects = new GameObject[totalSequences];
		for( int i=0; i<totalSequences; i++ )
		{
			switch( i )
			{
				case 0:
					sequenceObjects[0] = sequenceAliveIdle;
					break;
				case 1:
					sequenceObjects[1] = sequenceAliveMovingRight;
					break;
				case 2:
					sequenceObjects[2] = sequenceAliveMovingUp;
					break;
				case 3:
					sequenceObjects[3] = sequenceAliveMovingDown;
					break;
				case 4:
					sequenceObjects[4] = sequenceAliveFragile;
					break;
				case 5:
					sequenceObjects[5] = sequenceAliveRecovering;
					break;
				case 6:
					sequenceObjects[6] = sequenceDeadIdle;
					break;
				case 7:
					sequenceObjects[7] = sequenceDeadMovingRight;
					break;
				case 8:
					sequenceObjects[8] = sequenceDeadMovingUp;
					break;
				case 9:
					sequenceObjects[9] = sequenceDeadMovingDown;
					break;
			}
		}
	}

	protected override void GetSequenceIndexForStatus( int statusValue, ref int newSequenceIndex, ref bool newSpriteFlip )
	{
		newSpriteFlip = !side;
		switch( statusValue )
		{
			case STATUS_ALIVE_IDLE:
				newSequenceIndex = 0;
				break;
			case STATUS_ALIVE_MOVING_SIDES:
				newSequenceIndex = 1;
				break;
			case STATUS_ALIVE_MOVING_UP:
				newSequenceIndex = 2;
				break;
			case STATUS_ALIVE_MOVING_DOWN:
				newSequenceIndex = 3;
				break;
			case STATUS_ALIVE_FRAGILE:
				newSequenceIndex = 4;
				break;
			case STATUS_ALIVE_RECOVERING:
				newSequenceIndex = 5;
				break;
			case STATUS_DEAD_IDLE:
				newSequenceIndex = 6;
				break;
			case STATUS_DEAD_MOVING_SIDES:
				newSequenceIndex = 7;
				break;
			case STATUS_DEAD_MOVING_UP:
				newSequenceIndex = 8;
				break;
			case STATUS_DEAD_MOVING_DOWN:
				newSequenceIndex = 9;
				break;
			default:
				newSequenceIndex = -1;
				break;
		}
	}
}
