using UnityEngine;
using System.Collections;

public class BaseAnimator : MonoBehaviour
{
	public const int INVALID_STATUS = -1;

	public const bool SIDE_RIGHT = true;
	public const bool SIDE_LEFT = false;

	protected GameObject[] sequenceObjects;
	protected int totalSequences;
	protected int sequenceIndex;
	protected Sprite[] sprites;
	protected Sprite sprite;
	protected int spriteIndex;
	protected float spriteElapsed;
	protected bool spriteFlip;
	protected float spriteWidth;
	protected int[] firstIndexPerSequence;
	protected int[] lastIndexPerSequence;
	protected float[] speedPerSequence;
	protected bool[] loopPerSequence;
	protected int firstIndex;
	protected int lastIndex;
	protected float spriteDuration;
	protected bool loop;
	protected bool keepSequenceProgress;
	protected int status;
	protected int previousStatus;
	protected bool over;
	protected bool forceSpriteFeeding;
	protected float fadeSpeed;
	protected bool fadeImmediately;
	protected bool fadedToTransparent;
	protected bool fadedToOpaque;
	protected float opaqueAlpha;
	protected float currentAlpha;
	protected bool spriteFrozen;
	protected bool paused;

	protected virtual void Awake()
	{
		Clear( false );
		if( ExtractRenderingComponent() )
		{
			opaqueAlpha = ExtractRenderingColor().a;
			currentAlpha = opaqueAlpha;
			fadedToOpaque = true;
			FillSequencesArray();
			if( (sequenceObjects != null) && (totalSequences > 0) )
			{
				firstIndexPerSequence = new int[totalSequences];
				lastIndexPerSequence = new int[totalSequences];
				speedPerSequence = new float[totalSequences];
				loopPerSequence = new bool[totalSequences];
				ExtractSprites();
				Progress( 0f );
			}
		}
	}

	public virtual void Clear( bool renderingComponentInitialized = true )
	{
		if( renderingComponentInitialized )
		{
			FeedRenderingComponent( null, false );
		}
		ClearRenderingComponent();
		sequenceObjects = null;
		totalSequences = 0;
		sequenceIndex = -1;
		sprites = null;
		sprite = null;
		spriteIndex = -1;
		spriteElapsed = 0f;
		spriteFlip = false;
		spriteWidth = 0f;
		firstIndexPerSequence = null;
		lastIndexPerSequence = null;
		speedPerSequence = null;
		loopPerSequence = null;
		firstIndex = -1;
		lastIndex = -1;
		spriteDuration = 0f;
		loop = false;
		keepSequenceProgress = false;
		status = INVALID_STATUS;
		previousStatus = INVALID_STATUS;
		over = false;
		forceSpriteFeeding = false;
		fadeSpeed = 0f;
		fadeImmediately = false;
		fadedToTransparent = false;
		fadedToOpaque = false;
		opaqueAlpha = 0f;
		currentAlpha = 0f;
		spriteFrozen = false;
		paused = false;
	}

	protected virtual void ClearRenderingComponent()
	{
		/*halmeida - the subclasses will determine their own rendering components. It can be a SpriteRenderer or
		a canvas Image, for example.*/
		//Ex.: spriteRenderer = null;
	}

	protected virtual bool ExtractRenderingComponent()
	{
		/*halmeida - the subclasses will determine their own rendering components. It can be a SpriteRenderer or
		a canvas Image, for example.*/
		//Ex.: GetComponent<SpriteRenderer>();
		return false;
	}

	protected virtual void FeedRenderingComponent( Sprite newSprite, bool newSpriteFlip )
	{
		/*halmeida - the subclasses will determine their own rendering components. It can be a SpriteRenderer or
		a canvas Image, for example.*/
		//Ex.: spriteRenderer.sprite = newSprite;
	}

	protected virtual Color ExtractRenderingColor()
	{
		/*halmeida - the subclasses will determine their own rendering components. It can be a SpriteRenderer or
		a canvas Image, for example.*/
		//Ex.: return spriteRenderer.color;
		return Color.white;
	}

	protected virtual void FeedRenderingColor( Color newColor )
	{
		/*halmeida - the subclasses will determine their own rendering components. It can be a SpriteRenderer or
		a canvas Image, for example.*/
		//Ex.: spriteRenderer.color = newColor;
	}

	protected virtual void FillSequencesArray()
	{
		/*halmeida - the subclasses are supposed to place their particular sequence objects into this array.*/
		sequenceObjects = null;
		totalSequences = 0;
	}

	public void Progress( float timeStep )
	{
		int oldSpriteIndex = -1;
		bool oldSpriteFlip = false;

		if( !paused )
		{
			if( !over )
			{
				oldSpriteIndex = spriteIndex;
				oldSpriteFlip = spriteFlip;
				UpdateAnimation( timeStep );
				if( !over )
				{
					if( (spriteIndex != oldSpriteIndex) || (spriteFlip != oldSpriteFlip) || forceSpriteFeeding )
					{
						FeedRenderingComponent( sprite, spriteFlip );
						forceSpriteFeeding = false;
					}
					RecordChangeVerifiers();
				}
				else
				{
					sprite = null;
					FeedRenderingComponent( sprite, spriteFlip );
				}
			}
		}
	}

	private void UpdateAnimation( float timeStep )
	{
		int oldSequenceIndex = -1;
		int newSequenceIndex = -1;
		bool oldSpriteFlip = false;
		bool newSpriteFlip = false;
		int newSpriteIndex = -1;

		UpdateStatus();
		if( status == INVALID_STATUS )
		{
			over = true;
			return;
		}
		UpdateTransform( timeStep );
		UpdateMaterial( timeStep );
		oldSequenceIndex = sequenceIndex;
		oldSpriteFlip = spriteFlip;
		if( RequiresNewSequence() )
		{
			GetSequenceIndexForStatus( status, ref newSequenceIndex, ref newSpriteFlip );
			UpdateSequence( newSequenceIndex, newSpriteFlip );
		}
		if( (oldSequenceIndex == sequenceIndex) && (oldSpriteFlip == spriteFlip) ) 
		{
			/*halmeida - we didn't proceed to a new animation sequence, so we must progress the current one.*/
			if( (status != INVALID_STATUS) && (sequenceIndex != -1) && (spriteIndex != -1) )
			{
				spriteElapsed += timeStep;
				if( spriteElapsed > spriteDuration )
				{
					newSpriteIndex = spriteIndex;
					newSpriteIndex++;
					if( newSpriteIndex > lastIndex )
					{
						if( loop )
						{
							newSpriteIndex = firstIndex;
						}
						else
						{
							/*halmeida - first I will bring the sprite index back to a valid value, before reevaluating the status,
							because if the status reevaluation does not change the index and make it valid, we will still have a
							good index to use.*/
							newSpriteIndex = lastIndex;
							/*halmeida - now I reevaluate the status.*/ 
							AdvanceToNextStatus();
							if( status != INVALID_STATUS )
							{
								if( RequiresNewSequence() )
								{
									GetSequenceIndexForStatus( status, ref newSequenceIndex, ref newSpriteFlip );
									UpdateSequence( newSequenceIndex, newSpriteFlip );
									return;
								}
							}
							else
							{
								over = true;
								return;
							}
						}
					}
					UpdateSprite( newSpriteIndex, 0f );
				}
			}
		}
	}

	protected virtual void UpdateStatus()
	{
		status = INVALID_STATUS;
		keepSequenceProgress = false;
	}

	protected virtual void UpdateTransform( float timeStep )
	{
		/*halmeida - reserved for subclasses.*/
	}

	protected virtual void UpdateMaterial( float timeStep )
	{
		Color renderingColor = Color.white;

		renderingColor = ExtractRenderingColor();
		currentAlpha = renderingColor.a;
		UpdateAlphaFading( timeStep );
		renderingColor.a = currentAlpha;
		FeedRenderingColor( renderingColor );
	}

	private void UpdateAlphaFading( float timeStep )
	{
		float fadeAmount = 0f;

		if( fadeSpeed != 0f )
		{
			fadeAmount = fadeSpeed * timeStep;
			currentAlpha += fadeAmount;
			if( fadeSpeed > 0f )
			{
				fadedToTransparent = false;
				if( (currentAlpha > opaqueAlpha) || fadeImmediately )
				{
					currentAlpha = opaqueAlpha;
					fadeSpeed = 0f;
					fadeImmediately = false;
					fadedToOpaque = true;
				}
			}
			else
			{
				fadedToOpaque = false;
				if( (currentAlpha < 0f) || fadeImmediately )
				{
					currentAlpha = 0f;
					fadeSpeed = 0f;
					fadeImmediately = false;
					fadedToTransparent = true;
				}
			}
		}
	}

	protected virtual void AdvanceToNextStatus()
	{
		status = INVALID_STATUS;
		keepSequenceProgress = false;
	} 

	protected virtual bool RequiresNewSequence()
	{
		return (previousStatus != status);
	}

	private void UpdateSequence( int newSequenceIndex, bool newSpriteFlip )
	{
		int progressIndex = 0;
		float progressTime = 0f;
		int newSpriteIndex = -1;
		float newSpriteElapsed = 0f;

		if( (newSequenceIndex > -1) && (newSequenceIndex < totalSequences) && (firstIndexPerSequence != null) &&
		(lastIndexPerSequence != null) && (speedPerSequence != null) && (loopPerSequence != null) )
		{
			if( keepSequenceProgress && (spriteIndex > -1) && (firstIndex > -1) )
			{
				progressIndex = spriteIndex - firstIndex;
				progressTime = spriteElapsed;
				keepSequenceProgress = false;
			}
			sequenceIndex = newSequenceIndex;
			firstIndex = firstIndexPerSequence[sequenceIndex];
			lastIndex = lastIndexPerSequence[sequenceIndex];
			spriteDuration = speedPerSequence[sequenceIndex];
			loop = loopPerSequence[sequenceIndex];
			spriteFlip = newSpriteFlip;
			newSpriteIndex = firstIndex + progressIndex;
			newSpriteElapsed = progressTime;
			UpdateSprite( newSpriteIndex, newSpriteElapsed );
		}
		else
		{
			/*halmeida - somebody is trying an UpdateAnimation after a Clear().*/
			over = true;
		}
	}

	protected virtual void GetSequenceIndexForStatus( int statusValue, ref int newSequenceIndex, ref bool newSpriteFlip )
	{
		/*halmeida - reserved for subclasses.*/
	}

	protected virtual void UpdateSprite( int newSpriteIndex, float newSpriteElapsed )
	{
		if( !spriteFrozen )
		{
			spriteIndex = newSpriteIndex;
			spriteElapsed = newSpriteElapsed;
			sprite = null;
			if( sprites != null )
			{
				if( (spriteIndex > -1) && (sprites.Length > spriteIndex) )
				{
					sprite = sprites[spriteIndex];
				}
			}
			if( sprite == null )
			{
				spriteWidth = 0f;
			}
			else
			{
				spriteWidth = sprite.bounds.size.x;
			}
		}
	}

	protected virtual void RecordChangeVerifiers()
	{
		previousStatus = status;
	}

	private void ExtractSprites()
	{
		GameObject sequenceObject = null;
		AnimationSequence sequence = null;
		Sprite[] sequenceSprites = null;
		int length = 0;
		int oldLength = 0;
		Sprite[] newSprites = null;
		bool sequenceExtracted = false;
		Sprite newSprite = null;
		float framesPerSecond = 0f;

		sprites = null;
		for( int i=0; i<totalSequences; i++ )
		{
			sequenceExtracted = false;
			sequenceObject = sequenceObjects[i];
			if( sequenceObject != null )
			{
				sequence = sequenceObject.GetComponent<AnimationSequence>();
				if( sequence != null )
				{
					sequenceSprites = sequence.sprites;
					if( sequenceSprites != null )
					{
						length = sequenceSprites.Length;
						if( length > 0 )
						{
							if( sprites == null )
							{
								oldLength = 0;
							}
							else
							{
								oldLength = sprites.Length;
							}
							length = oldLength + length;
							newSprites = new Sprite[length];
							for( int j=0; j<oldLength; j++ )
							{
								newSprites[j] = sprites[j];
							}
							for( int j=oldLength; j<length; j++ )
							{
								newSprite = sequenceSprites[j-oldLength];
								newSprites[j] = newSprite;
							}
							sprites = newSprites;
							newSprites = null;
							firstIndexPerSequence[i] = oldLength;
							lastIndexPerSequence[i] = length-1;
							framesPerSecond = (sequence.framesPerSecond < 1) ? 1 : sequence.framesPerSecond;
							speedPerSequence[i] = 1f / framesPerSecond;
							loopPerSequence[i] = sequence.loop;
							sequenceExtracted = true;
						}
					}
				}
			}
			if( !sequenceExtracted )
			{
				firstIndexPerSequence[i] = -1;
				lastIndexPerSequence[i] = -1;
				speedPerSequence[i] = 0f;
				loopPerSequence[i] = false;
			}
		}
	}

	public int GetCurrentStatus()
	{
		return status;
	}

	public bool IsOver()
	{
		return over;
	}

	public void SetOpaqueAlpha( float newOpaqueAlpha )
	{
		opaqueAlpha = (newOpaqueAlpha > 1f) ? 1f : newOpaqueAlpha;
		opaqueAlpha = (opaqueAlpha < 0f) ? 0f : opaqueAlpha;
		if( currentAlpha >= opaqueAlpha )
		{
			StartAlphaFading( 1f, true );
		}
	}

	public float GetOpaqueAlpha()
	{
		return opaqueAlpha;
	}

	public virtual void StartAlphaFading( float newFadeSpeed, bool immediately )
	{
		fadeSpeed = newFadeSpeed;
		if( immediately )
		{
			fadeImmediately = true;
			UpdateMaterial( 0f );
		}
	}

	public virtual bool IsOpaque()
	{
		return fadedToOpaque;
	}

	public virtual bool IsTransparent()
	{
		return fadedToTransparent;
	}

	public virtual void SetSide( bool newSide )
	{
		/*halmeida - reserved for subclasses.*/
	}

	public virtual bool GetSide()
	{
		return SIDE_RIGHT;
	}

	public virtual void StartDying()
	{
		/*halmeida - reserved for subclasses.*/
	}

	public virtual void ToggleSpriteFreeze( bool freeze )
	{
		spriteFrozen = freeze;
	}

	public virtual void TogglePause( bool pause )
	{
		paused = pause;
	}

	public bool IsPaused()
	{
		return paused;
	}
}
