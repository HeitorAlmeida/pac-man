using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : BaseAnimator
{
	public const int STATUS_IDLE = 0;
	public const int STATUS_MOVING_SIDES = 1;
	public const int STATUS_MOVING_UP = 2;
	public const int STATUS_MOVING_DOWN = 3;
	public const int STATUS_DYING = 4;
	public const int STATUS_DEAD = 5;

	public GameObject sequenceIdle;
	public GameObject sequenceMovingRight;
	public GameObject sequenceMovingUp;
	public GameObject sequenceMovingDown;
	public GameObject sequenceDying;
	public GameObject sequenceDead;

	private SpriteRenderer bodyRenderer;
	private bool side;
	private bool previousSide;
	private Vector2 speeds;
	private bool shouldDie;
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
		shouldDie = false;
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

	public override void StartDying()
	{
		if( !dead && !over && (status != STATUS_DYING) )
		{
			shouldDie = true;
		}
	}

	public bool IsDying()
	{
		return (status == STATUS_DYING);
	}

	public bool IsDead()
	{
		return dead;
	}

	protected override void UpdateStatus()
	{
		if( (status == INVALID_STATUS) && !over )
		{
			status = STATUS_IDLE;
		}
		if( shouldDie )
		{
			status = STATUS_DYING;
			shouldDie = false;
		}
		else
		{
			/*halmeida - I will only consider changing the animation status if any speed is not zero.*/
			if( speeds != Vector2.zero )
			{
				if( speeds.x != 0f )
				{
					status = STATUS_MOVING_SIDES;
				}
				else if( speeds.y > 0f )
				{
					status = STATUS_MOVING_UP;
				}
				else
				{
					status = STATUS_MOVING_DOWN;
				}
			}
		}
	}

	protected override void UpdateTransform( float timeStep )
	{
		/*halmeida - gonna leave this here to do some scale changes at some moments if I have the time.*/
	}

	protected override void AdvanceToNextStatus()
	{
		/*halmeida - except for the dying state, every state animation should loop and never go to a next state.*/
		switch( status )
		{
			case STATUS_IDLE:
				status = INVALID_STATUS;
				break;
			case STATUS_MOVING_SIDES:
				status = INVALID_STATUS;
				break;
			case STATUS_MOVING_UP:
				status = INVALID_STATUS;
				break;
			case STATUS_MOVING_DOWN:
				status = INVALID_STATUS;
				break;
			case STATUS_DYING:
				status = STATUS_DEAD;
				dead = true;
				break;
			case STATUS_DEAD:
				status = INVALID_STATUS;
				break;
			default:
				status = INVALID_STATUS;
				break;
		}
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
		totalSequences = 6;
		sequenceObjects = new GameObject[totalSequences];
		for( int i=0; i<totalSequences; i++ )
		{
			switch( i )
			{
				case 0:
					sequenceObjects[0] = sequenceIdle;
					break;
				case 1:
					sequenceObjects[1] = sequenceMovingRight;
					break;
				case 2:
					sequenceObjects[2] = sequenceMovingUp;
					break;
				case 3:
					sequenceObjects[3] = sequenceMovingDown;
					break;
				case 4:
					sequenceObjects[4] = sequenceDying;
					break;
				case 5:
					sequenceObjects[5] = sequenceDead;
					break;
			}
		}
	}

	protected override void GetSequenceIndexForStatus( int statusValue, ref int newSequenceIndex, ref bool newSpriteFlip )
	{
		newSpriteFlip = !side;
		switch( statusValue )
		{
			case STATUS_IDLE:
				newSequenceIndex = 0;
				break;
			case STATUS_MOVING_SIDES:
				newSequenceIndex = 1;
				break;
			case STATUS_MOVING_UP:
				newSequenceIndex = 2;
				break;
			case STATUS_MOVING_DOWN:
				newSequenceIndex = 3;
				break;
			case STATUS_DYING:
				newSequenceIndex = 4;
				break;
			case STATUS_DEAD:
				newSequenceIndex = 5;
				break;
			default:
				newSequenceIndex = -1;
				break;
		}
	}
}

