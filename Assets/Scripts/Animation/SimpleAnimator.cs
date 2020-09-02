using UnityEngine;
using System.Collections;

/*halmeida - this kind of animator just runs one animation sequence.*/
public class SimpleAnimator : BaseAnimator
{
	public const int STATUS_RUNNING = 0;

	public GameObject sequenceObject;

	private SpriteRenderer spriteRenderer;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Clear( bool renderingComponentInitialized = true )
	{
		base.Clear( renderingComponentInitialized );
	}

	protected override void ClearRenderingComponent()
	{
		spriteRenderer = null;
	}

	protected override bool ExtractRenderingComponent()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		return (spriteRenderer != null);
	}

	protected override void FeedRenderingComponent( Sprite newSprite, bool newSpriteFlip )
	{
		if( spriteRenderer != null )
		{
			spriteRenderer.sprite = newSprite;
			spriteRenderer.flipX = newSpriteFlip;
		}
	}

	protected override Color ExtractRenderingColor()
	{
		if( spriteRenderer != null )
		{
			return spriteRenderer.color;
		}
		return Color.white;
	}

	protected override void FeedRenderingColor( Color newColor )
	{
		if( spriteRenderer != null )
		{
			spriteRenderer.color = newColor;
		}
	}

	protected override void FillSequencesArray()
	{
		totalSequences = 1;
		sequenceObjects = new GameObject[1];
		sequenceObjects[0] = sequenceObject;
	}

	protected override void UpdateStatus()
	{
		if( (status == INVALID_STATUS) && !over )
		{
			status = STATUS_RUNNING;
		}
	}

	protected override void AdvanceToNextStatus()
	{
		status = INVALID_STATUS;
	}

	protected override void GetSequenceIndexForStatus( int statusValue, ref int newSequenceIndex, ref bool newSpriteFlip )
	{
		if( statusValue == STATUS_RUNNING )
		{
			newSequenceIndex = statusValue;
			newSpriteFlip = spriteFlip;
			return;
		}
		newSequenceIndex = -1;
		newSpriteFlip = false;
	}
}
