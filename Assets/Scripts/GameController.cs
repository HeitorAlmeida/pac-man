using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	/*halmeida - the models will be cloned within the class and then used. This way we can
	safely mess around with the clones without messing with the prefabs and we also ensure
	the execution of the awake calls on their components.*/
	public GameObject[] tileFactoryModels;
	public GameObject[] stageSettingObjects;
	/*halmeida - placing objects in public fields spares the trouble of searching for them (by name, for example).*/
	public GameObject cameraObject;
	public GameObject canvasObject;
	/*halmeida - other models.*/
	public GameObject itemDatabaseModel;
	public GameObject playerModel;
	public GameObject[] enemyModels;
	public GameObject inputManagerModel;
	public GameObject scoreTitleModel;
	public GameObject scoreValueModel;
	public GameObject lifeImageModel;
	public int lifeImagesPerRow;
	public int totalLives;
	public GameObject endingModel;
	public GameObject creditsModel;
	public GameObject stageClearImageModel;
	public float stageClearAlphaSpeed;
	public GameObject stageClearTextModel;
	public float stageClearTextSpeed;
	public float stageClearDuration;

	private TileFactory[] tileFactoryModelComponents;
	private StageSettings[] stageSettingComponents;
	private GameObject stageObject;
	private StageController stageComponent;
	private int stageIndex;
	private GameObject tileFactoryObject;
	private TileFactory tileFactoryComponent;
	private Camera cameraComponent;
	private float cameraOriginalOrthoSize;
	private RectTransform canvasRectTrans;
	private GameObject itemDatabaseObject;
	private ItemDatabase itemDatabase;
	private GameObject inputManagerObject;
	private InputManager inputManager;
	private GameObject scoreTitleObject;
	private GameObject scoreValueObject;
	private Text scoreValueText;
	private int totalScore;
	private float lifeImageWidth;
	private float lifeImageHeight;
	private GameObject[] lifeImageObjects;
	private int spareLives;
	private GameObject endingObject;
	private GameObject creditsObject;
	private GameObject stageClearImageObject;
	private Image stageClearImage;
	private GameObject stageClearTextObject;
	private RectTransform stageClearTextTrans;
	private float clearTextStartX;
	private float clearTextEndX;
	private bool stageCleared;
	private float stageClearedElapsed;

	void Awake()
	{
		int totalFactories = 0;
		GameObject tileFactoryModel = null;
		RectTransform rectTrans = null;
		GameObject stageSettingObject = null;
		StageSettings stageSettingComponent = null;

		lifeImagesPerRow = (lifeImagesPerRow < 1) ? 1 : lifeImagesPerRow;
		totalLives = (totalLives < 1) ? 1 : totalLives;
		stageClearAlphaSpeed = (stageClearAlphaSpeed <= 0f) ? 1f : stageClearAlphaSpeed;
		stageClearTextSpeed = (stageClearTextSpeed <= 0f) ? 1f : stageClearTextSpeed;
		/*halmeida - I will extract references to every TileFactory component of the models, so that I
		can consult them quickly later.*/
		tileFactoryModelComponents = null;
		if( tileFactoryModels != null )
		{
			totalFactories = tileFactoryModels.Length;
			tileFactoryModelComponents = new TileFactory[totalFactories];
			for( int i=0; i<totalFactories; i++ )
			{
				tileFactoryModel = tileFactoryModels[i];
				if( tileFactoryModel != null )
				{
					tileFactoryModelComponents[i] = tileFactoryModel.GetComponent<TileFactory>();
				}
				else
				{
					tileFactoryModelComponents[i] = null;
				}
			}
		}
		stageSettingComponents = null;
		if( stageSettingObjects != null )
		{
			for( int i=0; i<stageSettingObjects.Length; i++ )
			{
				stageSettingObject = stageSettingObjects[i];
				if( stageSettingObject != null )
				{
					stageSettingComponent = stageSettingObject.GetComponent<StageSettings>();
					if( stageSettingComponent != null )
					{
						UsefulFunctions.IncreaseArray<StageSettings>( ref stageSettingComponents, stageSettingComponent );
					}
				}
			}
		}
		stageObject = null;
		stageComponent = null;
		stageIndex = -1;
		tileFactoryObject = null;
		tileFactoryComponent = null;
		cameraComponent = null;
		cameraOriginalOrthoSize = 0f;
		if( cameraObject != null )
		{
			cameraComponent = cameraObject.GetComponent<Camera>();
			if( cameraComponent != null )
			{
				cameraOriginalOrthoSize = cameraComponent.orthographicSize;
			}
		}
		canvasRectTrans = null;
		if( canvasObject != null )
		{
			canvasRectTrans = canvasObject.GetComponent<RectTransform>();
		}
		itemDatabaseObject = null;
		itemDatabase = null;
		if( itemDatabaseModel != null )
		{
			itemDatabaseObject = Instantiate( itemDatabaseModel, Vector3.zero, Quaternion.identity ) as GameObject;
			itemDatabase = itemDatabaseObject.GetComponent<ItemDatabase>();
			if( itemDatabase == null )
			{
				Destroy( itemDatabaseObject );
				itemDatabaseObject = null;
			}
		}
		inputManagerObject = null;
		inputManager = null;
		if( inputManagerModel != null )
		{
			inputManagerObject = Instantiate( inputManagerModel, Vector3.zero, Quaternion.identity ) as GameObject;
			inputManager = inputManagerObject.GetComponent<InputManager>();
			if( inputManager == null )
			{
				Destroy( inputManagerObject );
				inputManagerObject = null;
			}
		}
		scoreTitleObject = null;
		scoreValueObject = null;
		scoreValueText = null;
		totalScore = 0;
		lifeImageWidth = 0f;
		lifeImageHeight = 0f;
		if( lifeImageModel != null )
		{
			rectTrans = lifeImageModel.GetComponent<RectTransform>();
			if( rectTrans != null )
			{
				lifeImageWidth = rectTrans.rect.width;
				lifeImageHeight = rectTrans.rect.height;
			}
			else
			{
				lifeImageModel = null;
			}
		}
		lifeImageObjects = null;
		spareLives = totalLives-1;
		endingObject = null;
		creditsObject = null;
		stageClearImageObject = null;
		stageClearImage = null;
		stageClearTextObject = null;
		stageClearTextTrans = null;
		clearTextStartX = 0f;
		clearTextEndX = 0f;
		stageCleared = false;
		stageClearedElapsed = 0f;
		CreateUIElements();
		stageIndex = 0;
		LoadStage();
	}

	private void CreateUIElements()
	{
		CreateScoreDisplay();
		for( int i=0; i<spareLives; i++ )
		{
			AddDisplayedLife();
		}
	}

	private void CreateScoreDisplay()
	{
		RectTransform rectTrans = null;

		if( canvasRectTrans != null )
		{
			if( scoreTitleModel != null )
			{
				scoreTitleObject = Instantiate( scoreTitleModel ) as GameObject;
				rectTrans = scoreTitleObject.GetComponent<RectTransform>();
				if( rectTrans != null )
				{
					rectTrans.SetParent( canvasRectTrans, false );
				}
				else
				{
					Destroy( scoreTitleObject );
					scoreTitleObject = null;
				}
			}
			if( scoreValueModel != null )
			{
				scoreValueObject = Instantiate( scoreValueModel ) as GameObject;
				rectTrans = scoreValueObject.GetComponent<RectTransform>();
				scoreValueText = scoreValueObject.GetComponent<Text>();
				if( (rectTrans != null) && (scoreValueText != null) )
				{
					rectTrans.SetParent( canvasRectTrans, false );
					scoreValueText.text = ""+totalScore;
				}
				else
				{
					scoreValueText = null;
					Destroy( scoreValueObject );
					scoreValueObject = null;
				}
			}
		}
	}

	private void AddDisplayedLife()
	{
		int lifeIndex = -1;
		int row = -1;
		int column = -1;
		GameObject lifeObject = null;
		RectTransform rectTrans = null;
		Vector2 anchoredPosition = Vector2.zero;

		if( (lifeImageModel != null) && (canvasRectTrans != null) )
		{
			lifeIndex = (lifeImageObjects == null) ? 0 : lifeImageObjects.Length;
			row = lifeIndex / lifeImagesPerRow;
			column = lifeIndex % lifeImagesPerRow;
			lifeObject = Instantiate( lifeImageModel ) as GameObject;
			rectTrans = lifeObject.GetComponent<RectTransform>();
			if( rectTrans != null )
			{
				rectTrans.SetParent( canvasRectTrans, false );
				anchoredPosition = rectTrans.anchoredPosition;
				anchoredPosition.x += column * lifeImageWidth;
				anchoredPosition.y -= row * lifeImageHeight;
				rectTrans.anchoredPosition = anchoredPosition;
				UsefulFunctions.IncreaseArray<GameObject>( ref lifeImageObjects, lifeObject );
			}
		}
	}

	private void RemoveDisplayedLife()
	{
		GameObject lifeObject = null;
		int lastIndex = -1;

		if( lifeImageObjects != null )
		{
			lastIndex = lifeImageObjects.Length - 1;
			lifeObject = lifeImageObjects[lastIndex];
			if( lifeObject != null )
			{
				Destroy( lifeObject );
				lifeImageObjects[lastIndex] = null;
			}
			UsefulFunctions.DecreaseArray<GameObject>( ref lifeImageObjects, lastIndex );
		}
	}

	private void ClearDisplayedLives()
	{
		GameObject lifeObject = null;

		if( lifeImageObjects != null )
		{
			for( int i=0; i<lifeImageObjects.Length; i++ )
			{
				lifeObject = lifeImageObjects[i];
				if( lifeObject != null )
				{
					Destroy( lifeObject );
					lifeImageObjects[i] = null;
				}
			}
			lifeImageObjects = null;
		}
	}

	private void LoadStage()
	{
		string stageFileName = null;
		int requestedFactoryID = TileFactory.UNDEFINED_FACTORY_ID;
		TileFactory modelComponent = null;
		int modelIndex = -1;
		Vector2 stageDimensions = Vector2.zero;
		Vector3 cameraPosition = Vector3.zero;
		float halfStageHeight = 0f;
		float newOrthoSize = 0f;
		StageSettings stageSettings = null;

		if( stageIndex < 0 )
		{
			return;
		}
		if( stageObject == null )
		{
			stageObject = new GameObject("Stage");
			stageObject.transform.SetParent( gameObject.transform, false );
			stageObject.transform.rotation = Quaternion.identity;
			stageObject.transform.localScale = Vector3.one;
			stageObject.transform.localPosition = Vector3.zero;
			stageComponent = stageObject.AddComponent<StageController>();
			stageComponent.SetGameController( this );
			stageComponent.SetItemDatabase( itemDatabase );
			stageComponent.SetPlayerModel( playerModel );
			stageComponent.SetEnemyModels( enemyModels );
		}
		else
		{
			if( stageComponent != null )
			{
				stageComponent.Reset();
			}
		}
		if( tileFactoryComponent != null )
		{
			tileFactoryComponent.Clear();
			tileFactoryComponent = null;
		}
		if( tileFactoryObject != null )
		{
			Destroy( tileFactoryObject );
			tileFactoryObject = null;
		}
		/*halmeida - get the file name and the model of the tile factory that the stage wants to use.*/
		if( stageSettingComponents != null )
		{
			if( stageSettingComponents.Length > stageIndex )
			{
				stageSettings = stageSettingComponents[stageIndex];
				/*halmeida - since the array is previously verified, the element is surely not null.*/
				stageFileName = stageSettings.fileName;
				requestedFactoryID = stageSettings.tileFactoryID;
				if( tileFactoryModelComponents != null )
				{
					for( int i=0; i<tileFactoryModelComponents.Length; i++ )
					{
						modelComponent = tileFactoryModelComponents[i];
						if( modelComponent != null )
						{
							if( modelComponent.factoryID == requestedFactoryID )
							{
								modelIndex = i;
								break;
							}
						}
					}
				}
			}
			else
			{
				/*halmeida - player reached the end of the game.*/
				if( stageComponent != null )
				{
					stageComponent.Clear();
					stageComponent = null;
				}
				Destroy( stageObject );
				stageObject = null;
				DisplayEnding();
			}
		}
		if( (stageFileName != null) && (modelIndex > -1) )
		{
			tileFactoryObject = Instantiate( tileFactoryModels[modelIndex], Vector3.zero, Quaternion.identity ) as GameObject;
			if( tileFactoryObject != null )
			{
				tileFactoryComponent = tileFactoryObject.GetComponent<TileFactory>();
				if( (tileFactoryComponent != null) && (stageComponent != null) )
				{
					if( inputManager != null )
					{
						inputManager.SetStageController( stageComponent );
					}
					stageComponent.SetTileFactory( tileFactoryComponent );
					stageComponent.SetEnemyStateDurations( stageSettings.durationFragileGhost, stageSettings.durationRecoveringGhost );
					if( stageComponent.LoadFromResourcesFile( "Stages/"+stageFileName ) )
					{
						if( cameraObject != null )
						{
							stageDimensions = stageComponent.GetStructureDimensions();
							halfStageHeight = stageDimensions.y / 2f;
							cameraPosition = cameraObject.transform.position;
							cameraPosition.x = stageDimensions.x / 2f;
							cameraPosition.y = -halfStageHeight; 
							cameraObject.transform.position = cameraPosition;
							if( cameraComponent != null )
							{
								newOrthoSize = (halfStageHeight > cameraOriginalOrthoSize) ? halfStageHeight : cameraOriginalOrthoSize;
								cameraComponent.orthographicSize = newOrthoSize;
							}
						}
					}
				}
			}
		}
	}

	/*halmeida - the only Update in the entire program is supposed to be this one. Having a single update allows
	us to have a much tighter control over what is being done by the program. Multiple updates distribute processing
	through a number of classes and that quickly gets hard to track. In large programs one may end up forgetting
	the update of one or more classes and they make the game heavier even if they are no longer needed.*/
	void Update()
	{
		if( !stageCleared )
		{
			if( inputManager != null )
			{
				inputManager.Progress( Time.deltaTime );
			}
			if( stageComponent != null )
			{
				stageComponent.Progress( Time.deltaTime );
				if( stageComponent.IsCompleted() )
				{
					stageCleared = true;
					stageClearedElapsed = 0f;
					CreateStageClearUIElements();
				}
				else if( stageComponent.IsFailed() )
				{
					if( spareLives > 0 )
					{
						/*halmeida - player still has a spare life, we will continue at the same stage, after preparing it for a retry.*/
						spareLives--;
						RemoveDisplayedLife();
						if( !stageComponent.PrepareForRetry() )
						{
							RestartGame();
						}
					}
					else
					{
						RestartGame();
					}
				}
			}
		}
		else
		{
			if( stageClearedElapsed == 0 )
			{
				if( PerformStageClearedTransformations( true, Time.deltaTime ) )
				{
					stageClearedElapsed += Time.deltaTime;
				}
			}
			else
			{
				if( stageClearedElapsed < stageClearDuration )
				{
					stageClearedElapsed += Time.deltaTime;
				}
				else
				{
					if( PerformStageClearedTransformations( false, Time.deltaTime ) )
					{
						if( stageClearImageObject != null )
						{
							stageClearImage = null;
							Destroy( stageClearImageObject );
							stageClearImageObject = null;
						}
						if( stageClearTextObject != null )
						{
							stageClearTextTrans = null;
							Destroy( stageClearTextObject );
							stageClearTextObject = null;
						}
						stageCleared = false;
						stageIndex++;
						LoadStage();
					}
				}
			}
		}
	}

	private void CreateStageClearUIElements()
	{
		RectTransform rectTrans = null;
		Color color = Color.black;

		if( canvasRectTrans != null )
		{
			if( stageClearImageModel != null )
			{
				stageClearImageObject = Instantiate( stageClearImageModel ) as GameObject;
				rectTrans = stageClearImageObject.GetComponent<RectTransform>();
				stageClearImage = stageClearImageObject.GetComponent<Image>();
				if( (rectTrans != null) && (stageClearImage != null) )
				{
					rectTrans.SetParent( canvasRectTrans, false );
					color = stageClearImage.color;
					color.a = 0f;
					stageClearImage.color = color; 
				}
				else
				{
					rectTrans = null;
					stageClearImage = null;
					Destroy( stageClearImageObject );
					stageClearImageObject = null;
				}
			}
			if( stageClearTextModel != null )
			{
				stageClearTextObject = Instantiate( stageClearTextModel ) as GameObject;
				rectTrans = stageClearTextObject.GetComponent<RectTransform>();
				if( rectTrans != null )
				{
					rectTrans.SetParent( canvasRectTrans, false );
					clearTextStartX = -canvasRectTrans.rect.width/2f - rectTrans.rect.width/2f;
					clearTextEndX = -clearTextStartX;
					rectTrans.anchoredPosition = new Vector2( clearTextStartX, rectTrans.anchoredPosition.y );
					stageClearTextTrans = rectTrans;
				}
				else
				{
					Destroy( stageClearTextObject );
					stageClearTextObject = null;
				}
			}
		}
	}

	private bool PerformStageClearedTransformations( bool begining, float timeStep )
	{
		bool transformationsOver = true;
		Color color = Color.black;
		float alpha = 0f;
		Vector2 anchoredPosition = Vector2.zero;
		float anchoredX = 0f;
		float targetX = 0f;

		if( stageClearImage != null )
		{
			color = stageClearImage.color;
			alpha = color.a;
			if( begining )
			{
				if( alpha < 1f )
				{
					alpha += stageClearAlphaSpeed * timeStep;
					if( alpha >= 1f )
					{
						alpha = 1f;
					}
					else
					{
						transformationsOver = false;
					}
					color.a = alpha;
					stageClearImage.color = color;
				}
			}
			else
			{
				/*halmeida - do nothing cause I don't want the image to fade back to transparent.*/
			}
		}
		if( transformationsOver && (stageClearTextTrans != null) )
		{
			anchoredPosition = stageClearTextTrans.anchoredPosition;
			anchoredX = anchoredPosition.x;
			if( begining )
			{
				targetX = 0f;
			}
			else
			{
				targetX = clearTextEndX;
			}
			if( anchoredX < targetX )
			{
				anchoredX += stageClearTextSpeed * timeStep;
				if( anchoredX >= targetX )
				{
					anchoredX = targetX;
				}
				else
				{
					transformationsOver = false;
				}
				anchoredPosition.x = anchoredX;
				stageClearTextTrans.anchoredPosition = anchoredPosition;
			}
		}
		return transformationsOver;
	}

	private void RestartGame()
	{
		IncreaseScore( -totalScore );
		ClearDisplayedLives();
		spareLives = totalLives-1;
		for( int i=0; i<spareLives; i++ )
		{
			AddDisplayedLife();
		}
		stageIndex = 0;
		LoadStage();
	}

	public void IncreaseScore( int additionalScore )
	{
		totalScore += additionalScore;
		if( scoreValueText != null )
		{
			scoreValueText.text = ""+totalScore;
		}
	}

	private void DisplayEnding()
	{
		RectTransform rectTrans = null;

		if( canvasRectTrans != null )
		{
			if( endingModel != null )
			{
				endingObject = Instantiate( endingModel ) as GameObject;
				rectTrans = endingObject.GetComponent<RectTransform>();
				if( rectTrans != null )
				{
					rectTrans.SetParent( canvasRectTrans, false );
				}
				else
				{
					Destroy( endingObject );
					endingObject = null;
				}
			}
			if( creditsModel != null )
			{
				creditsObject = Instantiate( creditsModel ) as GameObject;
				rectTrans = creditsObject.GetComponent<RectTransform>();
				if( rectTrans != null )
				{
					rectTrans.SetParent( canvasRectTrans, false );
				}
				else
				{
					Destroy( creditsObject );
					creditsObject = null;
				}
			}
		}
	}
}
