using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; //halmeida - to use Exception types and String methods

public class StageController : MonoBehaviour
{
	public const int BLUEPRINT_CODE_INVALID = -1;
	public const int SMALLEST_BLUEPRINT_CODE = 0;
	public const int BLUEPRINT_CODE_NOTHING = 0;
	public const int BLUEPRINT_CODE_TILE_FIRST = 1;
	public const int BLUEPRINT_CODE_TILE_LAST = 5;
	public const int BLUEPRINT_CODE_PELLET_SMALL = 6;
	public const int BLUEPRINT_CODE_PELLET_BIG = 7;
	/*halmeida - pac-man actually starts in a position between two horizontal tile slots. The code below will denote the
	slot at the right of this position.*/
	public const int BLUEPRINT_CODE_PAC_START_RIGHT = 8;
	/*halmeida - the door to the cage of ghosts occupies two wall tiles. Above these door tiles we will have the tiles were
	the ghosts join the hunt.*/
	public const int BLUEPRINT_CODE_GHOST_START_RIGHT = 9;
	public const int BIGGEST_BLUEPRINT_CODE = 9;

	private GameController gameController;
	private int[][] blueprint;
	private int blueprintRows;
	private int blueprintColumns;
	/*halmeida - a linear version of the blueprint will be better for a path finding algorithm to search through.*/
	private int[] linearBlueprint;
	private GameObject structureObject;
	private StageStructure structureComponent;
	private GameObject itemLayer;
	private ItemDatabase itemDatabase;
	private GameObject[] itemObjects;
	private ItemController[] itemComponents;
	private GameObject playerModel;
	private GameObject playerObject;
	private PlayerController playerComponent;
	private GameObject enemyLayer;
	private GameObject[] enemyModels;
	private GameObject[] enemyObjects;
	private EnemyController[] enemyComponents;
	private float enemyFragileDuration;
	private float enemyRecoveringDuration;
	private bool stageReady;
	private bool stageCompleted;
	private bool stageFailed;

	void Awake()
	{
		Vector3 correctDepthPosition = Vector3.zero;

		gameController = null;
		blueprint = null;
		blueprintRows = 0;
		blueprintColumns = 0;
		linearBlueprint = null;
		structureObject = null;
		structureComponent = null;
		itemLayer = null;
		itemDatabase = null;
		itemObjects = null;
		itemComponents = null;
		playerModel = null;
		playerObject = null;
		playerComponent = null;
		enemyLayer = null;
		enemyModels = null;
		enemyObjects = null;
		enemyComponents = null;
		enemyFragileDuration = 0f;
		enemyRecoveringDuration = 0f;
		stageReady = false;
		stageCompleted = false;
		stageFailed = false;

		structureObject = new GameObject("StageStructure");
		structureObject.transform.SetParent( gameObject.transform, false );
		structureObject.transform.rotation = Quaternion.identity;
		structureObject.transform.localScale = Vector3.one;
		correctDepthPosition.z = DisplayDepthManager.GetElementDepth( DisplayDepthManager.ELEMENT_CODE_TILE );
		structureObject.transform.localPosition = correctDepthPosition;
		structureComponent = structureObject.AddComponent<StageStructure>();

		/*halmeida - the structure object will already act as the parent of all tiles, placing them at the correct depth.
		We will use the itemLayer object as the parent of all items, placing them at the correct depth.*/
		itemLayer = new GameObject("ItemLayer");
		itemLayer.transform.SetParent( gameObject.transform, false );
		itemLayer.transform.rotation = Quaternion.identity;
		itemLayer.transform.localScale = Vector3.one;
		correctDepthPosition.z = DisplayDepthManager.GetElementDepth( DisplayDepthManager.ELEMENT_CODE_ITEM );
		itemLayer.transform.localPosition = correctDepthPosition;

		enemyLayer = new GameObject("EnemyLayer");
		enemyLayer.transform.SetParent( gameObject.transform, false );
		enemyLayer.transform.rotation = Quaternion.identity;
		enemyLayer.transform.localScale = Vector3.one;
		correctDepthPosition.z = DisplayDepthManager.GetElementDepth( DisplayDepthManager.ELEMENT_CODE_ENEMY );
		enemyLayer.transform.localPosition = correctDepthPosition;
	}

	public void SetGameController( GameController newGameController )
	{
		gameController = newGameController;
	}

	public void SetTileFactory( TileFactory newTileFactory )
	{
		if( structureComponent != null )
		{
			structureComponent.SetTileFactory( newTileFactory );
		}
	}

	public void SetItemDatabase( ItemDatabase newItemDatabase )
	{
		itemDatabase = newItemDatabase;
	}

	public void SetPlayerModel( GameObject newPlayerModel )
	{
		playerModel = newPlayerModel;
	}

	public void SetEnemyModels( GameObject[] newEnemyModels )
	{
		enemyModels = newEnemyModels;
	}

	public void SetEnemyStateDurations( float newFragileDuration, float newRecoveringDuration )
	{
		enemyFragileDuration = newFragileDuration;
		enemyRecoveringDuration = newRecoveringDuration;
	}

	public bool LoadFromResourcesFile( string pathWithName = null )
	{
		bool stepFailed = false;
		TextAsset loadedTextAsset = null;

		if( stageReady )
		{
			/*halmeida - the stage has already been loaded. Unless we clear it first, we can't load again.*/
			return false;
		}
		if( pathWithName != null )
		{
			try
			{
				loadedTextAsset = Resources.Load( pathWithName ) as TextAsset;
			}
			catch( Exception e )
			{
				Debug.Log ("Debug : StageController : resource could not be loaded. Load caused exception.");
				Debug.Log ("Debug : StageController : exception message = "+e.Message+".");
				stepFailed = true;
			}
			if( loadedTextAsset == null )
			{
				Debug.Log("Debug : StageController : resource could not be loaded. Wasn't found.");
				stepFailed = true;
			}
		}
		else
		{
			Debug.Log(" Debug : StageController : resource could not be loaded. Path is null.");
			stepFailed = true;
		}
		if( !stepFailed )
		{
			stepFailed = !CreateBlueprintFromString( loadedTextAsset.text );
		}
		if( loadedTextAsset != null )
		{
			Resources.UnloadAsset( loadedTextAsset );
		}
		if( !stepFailed )
		{
			stepFailed = true;
			if( structureComponent != null )
			{
				structureComponent.SetBlueprint( blueprint, true );
				stepFailed = !structureComponent.SuccessfullyBuilt();
			}
		}
		if( !stepFailed )
		{
			stepFailed = !CreatePlayer();
		}
		if( !stepFailed )
		{
			stepFailed = !CreateItemsFromBlueprint();
		}
		if( !stepFailed )
		{
			/*halmeida - a stage without enemies can exist, it would be like a bonus stage.*/
			CreateEnemies();
		}
		stageReady = !stepFailed;
		if( stepFailed )
		{
			/*halmeida - if any part of the loading process failed, we reset for another attempt.*/
			ClearBlueprint( true );
		}
		return stageReady;
	}

	private bool CreateBlueprintFromString( string blueprintString )
	{
		string[] textLines = null;
		char[] lineSeparators = null;
		string[] lineValueStrings = null;
		char[] valueSeparators = null;
		int[] lineValues = null;
		int readValue = 0;
		bool invalidValue = false;

		if( blueprint != null )
		{
			/*halmeida - the blueprint has already been loaded. Unless we clear it first, we can't load again.*/
			return false;
		}
		blueprintRows = 0;
		blueprintColumns = 0;
		if( blueprintString != null )
		{
			lineSeparators = new char[2];
			lineSeparators[0] = '\n';
			lineSeparators[1] = '\r';
			textLines = blueprintString.Split( lineSeparators, StringSplitOptions.RemoveEmptyEntries );
			if( textLines != null )
			{
				blueprintRows = textLines.Length;
				valueSeparators = new char[3];
				valueSeparators[0] = ' ';
				valueSeparators[1] = ',';
				valueSeparators[2] = '\t';
				for( int i=0; i<blueprintRows; i++ )
				{
					lineValueStrings = textLines[i].Split( valueSeparators, StringSplitOptions.RemoveEmptyEntries );
					if( lineValueStrings != null )
					{
						if( blueprintColumns == 0 )
						{
							/*halmeida - if the number of columns hasn't been set, we set it and allocate the blueprint.*/
							blueprintColumns = lineValueStrings.Length;
							if( blueprintColumns == 0 )
							{
								break;
							}
							blueprint = new int[blueprintRows][];
						}
						else
						{
							/*halmeida - if the number of columns has already been set, it must be respected.*/
							if( lineValueStrings.Length != blueprintColumns )
							{
								Debug.Log("Debug : StageController : number of columns was not respected at blueprint row "+i+".");
								blueprintRows = 0;
								break;
							}
						}
						lineValues = new int[blueprintColumns];
						for( int j=0; j<blueprintColumns; j++ )
						{
							try
							{
								readValue = int.Parse( lineValueStrings[j] );
							}
							catch( Exception e )
							{
								Debug.Log ("Debug : StageController : parsing to interger failed. Parsing caused exception.");
								Debug.Log ("Debug : StageController : exception message = "+e.Message+".");
								readValue = -1;
							}
							if( (readValue < SMALLEST_BLUEPRINT_CODE) || (readValue > BIGGEST_BLUEPRINT_CODE) )
							{
								invalidValue = true;
								break;
							}
							lineValues[j] = readValue;
						}
						if( invalidValue )
						{
							Debug.Log("Debug : StageController : invalid value at blueprint row "+i+".");
							blueprintRows = 0;
							break;
						}
						blueprint[i] = lineValues;
					}
					else
					{
						/*halmeida - line had no values, just separation characters. Consider configuration invalid.*/
						blueprintRows = 0;
						break;
					}
				}
			}
		}
		if( (blueprintRows == 0) || (blueprintColumns == 0) )
		{
			if( blueprint != null )
			{
				for( int i=0; i<blueprint.Length; i++ )
				{
					blueprint[i] = null;
				}
				blueprint = null;
			}
		}
		linearBlueprint = null;
		BuildLinearBlueprint();
		return (blueprint != null);
	}

	private void BuildLinearBlueprint()
	{
		int[] blueprintRow = null;
		int blueprintCode = BLUEPRINT_CODE_INVALID;

		if( blueprint != null )
		{
			linearBlueprint = new int[blueprintRows * blueprintColumns];
			for( int i=0; i<blueprintRows; i++ )
			{
				blueprintRow = blueprint[i];
				for( int j=0; j<blueprintColumns; j++ )
				{
					blueprintCode = blueprintRow[j];
					/*halmeida - the linear blueprint is meant to be used by pathfinding algorithms. They only care if a
					slot is occupied by a tile or not. All other information is not relevant.*/
					if( (blueprintCode < BLUEPRINT_CODE_TILE_FIRST) || (blueprintCode > BLUEPRINT_CODE_TILE_LAST) )
					{
						blueprintCode = BLUEPRINT_CODE_NOTHING;
					}
					else
					{
						blueprintCode = BLUEPRINT_CODE_TILE_FIRST;
					}
					linearBlueprint[ (i*blueprintColumns) + j ] = blueprintCode;
				}
			}
		}
	}

	private bool CreatePlayer()
	{
		Vector2 playerOffset = Vector2.zero;
		Vector3 playerPosition = Vector3.zero;
		bool validOffset = false;

		if( (structureComponent != null) && (playerModel != null) && (playerObject == null) )
		{
			playerOffset = structureComponent.GetPlayerStartOffset( ref validOffset );
			if( validOffset )
			{
				playerObject = Instantiate( playerModel, Vector3.zero, Quaternion.identity ) as GameObject;
				playerComponent = playerObject.GetComponent<PlayerController>();
				if( playerComponent == null )
				{
					Destroy( playerObject );
					playerObject = null;
					return false;
				}
				playerObject.transform.SetParent( gameObject.transform, false );
				playerPosition.x = playerOffset.x;
				playerPosition.y = playerOffset.y;
				playerPosition.z = DisplayDepthManager.GetElementDepth( DisplayDepthManager.ELEMENT_CODE_PLAYER );
				playerObject.transform.localPosition = playerPosition;
				return true;
			}
		}
		return false;
	}

	private bool CreateEnemies()
	{
		GameObject enemyModel = null;
		GameObject enemyObject = null;
		EnemyController enemyComponent = null;
		Vector2 enemyOffset = Vector2.zero;
		Vector3 enemyPosition = Vector3.zero;
		bool validOffset = false;
		bool enemyCreated = false;

		if( (structureComponent != null) && (enemyModels != null) && (enemyObjects == null) && (enemyLayer != null) )
		{
			for( int i=0; i<4; i++ )
			{
				enemyModel = null;
				if( enemyModels.Length > i )
				{
					enemyModel = enemyModels[i];
				}
				if( i == 0 )
				{
					enemyOffset = structureComponent.GetGhostStartOffset( ref validOffset );
				}
				else
				{
					enemyOffset = structureComponent.GetGhostCageOffset( i-1, ref validOffset );
				}
				if( (enemyModel != null) && validOffset )
				{
					enemyObject = Instantiate( enemyModel, Vector3.zero, Quaternion.identity ) as GameObject;
					enemyComponent = enemyObject.GetComponent<EnemyController>();
					if( enemyComponent == null )
					{
						Destroy( enemyObject );
						enemyObject = null;
					}
					else
					{
						UsefulFunctions.IncreaseArray<GameObject>( ref enemyObjects, enemyObject );
						UsefulFunctions.IncreaseArray<EnemyController>( ref enemyComponents, enemyComponent );
						enemyObject.transform.SetParent( enemyLayer.transform, false );
						enemyPosition.x = enemyOffset.x;
						enemyPosition.y = enemyOffset.y;
						enemyObject.transform.localPosition = enemyPosition;
						enemyComponent.SetAdversary( playerComponent );
						if( i==0 )
						{
							enemyComponent.StartHuntingBehaviour();
						}
						enemyCreated = true;
					}
				}
			}
		}
		return enemyCreated;
	}

	private bool CreateItemsFromBlueprint()
	{
		int[] blueprintRow = null;
		int blueprintCode = BLUEPRINT_CODE_INVALID;
		int itemCode = ItemDatabase.ITEM_CODE_INVALID;
		GameObject itemModel = null;
		GameObject itemObject = null;
		ItemController itemComponent = null;
		Vector2 slotOffset = Vector2.zero;
		bool slotOffsetValid = false;
		Vector3 objectPosition = Vector3.zero;

		if( (blueprint != null) && (itemDatabase != null) && (structureComponent != null) && (itemLayer != null) && (itemObjects == null) )
		{
			for( int i=0; i<blueprintRows; i++ )
			{
				blueprintRow = blueprint[i];
				for( int j=0; j<blueprintColumns; j++ )
				{
					blueprintCode = blueprintRow[j];
					itemCode = ConvertBlueprintCodeToItemCode( blueprintCode );
					itemModel = itemDatabase.GetItemModel( itemCode );
					if( itemModel != null )
					{
						itemObject = Instantiate( itemModel, Vector3.zero, Quaternion.identity ) as GameObject;
						itemComponent = itemObject.GetComponent<ItemController>();
						if( itemComponent == null )
						{
							Destroy( itemObject );
							itemObject = null;
						}
						else
						{
							UsefulFunctions.IncreaseArray<GameObject>( ref itemObjects, itemObject );
							UsefulFunctions.IncreaseArray<ItemController>( ref itemComponents, itemComponent );
							itemObject.transform.SetParent( itemLayer.transform, false );
							slotOffset = structureComponent.GetOffsetForRowAndColumn( i, j, ref slotOffsetValid );
							objectPosition.x = slotOffset.x;
							objectPosition.y = slotOffset.y;
							itemObject.transform.localPosition = objectPosition;
							itemComponent.SetPlayerController( playerComponent );
						}
					}
				}
			}
		}
		return true;
	}

	public static int ConvertBlueprintCodeToItemCode( int blueprintCode )
	{
		int itemCode = ItemDatabase.ITEM_CODE_INVALID;

		switch( blueprintCode )
		{
			case BLUEPRINT_CODE_PELLET_SMALL:
				itemCode = ItemDatabase.ITEM_CODE_PELLET_SMALL;
				break;
			case BLUEPRINT_CODE_PELLET_BIG:
				itemCode = ItemDatabase.ITEM_CODE_PELLET_BIG;
				break;
		}
		return itemCode;
	}

	public void Progress( float timeStep )
	{
		ItemController itemComponent = null;
		EnemyController enemyComponent = null;


		if( stageReady )
		{
			if( itemComponents != null )
			{
				for( int i=0; i<itemComponents.Length; i++ )
				{
					itemComponent = itemComponents[i];
					if( itemComponent != null )
					{
						itemComponent.Progress( timeStep );
						if( itemComponent.IsOver() )
						{
							if( gameController != null )
							{
								gameController.IncreaseScore( itemComponent.rewardScore );
							}
							if( itemComponent.itemID == ItemController.ITEM_ID_PELLET_BIG )
							{
								StartEnemyFragility();
							}
							RemoveItem( i );
							if( itemComponents != null )
							{
								i--;
							}
							else
							{
								break;
							}
						}
					}
				}
			}
			else
			{
				stageCompleted = true;
			}
			if( playerComponent != null )
			{
				/*halmeida - the declaration of the method called below is at the end of the file.*/
				AccelerateAndMoveAgent( timeStep, playerComponent );
				playerComponent.Progress( timeStep );
				if( playerComponent.IsDead() )
				{
					stageFailed = true;
				}
			}
			if( enemyComponents != null )
			{
				for( int i=0; i<enemyComponents.Length; i++ )
				{
					enemyComponent = enemyComponents[i];
					if( enemyComponent != null )
					{
						AccelerateAndMoveAgent( timeStep, enemyComponent );
						enemyComponent.Progress( timeStep );
						ProvideEnemyAINeeds( enemyComponent );
					}
				}
			}
		}
	}

	private void ProvideEnemyAINeeds( EnemyController enemyComponent )
	{
		int row = -1;
		int column = -1;
		int currentRow = -1;
		int currentColumn = -1;
		Vector2 currentRowAndColumn = Vector2.zero;
		Vector2 ghostStartRowAndColumn = Vector2.zero;
		bool hasGhostStart = false;
		Vector2[] pathOfSlots = null;
		Vector2[] pathOfPositions = null;
		bool hasSlotTarget = false;
		bool hasPositionTarget = false;
		Vector2 position = Vector2.zero;
		int enemyAIState = EnemyController.AI_STATE_INVALID;

		if( enemyComponent != null )
		{
			enemyAIState = enemyComponent.GetCurrentAIState();
			switch( enemyAIState )
			{
				case EnemyController.AI_STATE_CENTERING:
					hasPositionTarget = enemyComponent.HasPositionTarget( ref position );
					if( !hasPositionTarget && !enemyComponent.CompletedThePath() )
					{
						pathOfPositions = new Vector2[1];
						pathOfPositions[0] = structureComponent.GetGhostCageOffset( 1, ref hasGhostStart );
						enemyComponent.SetPath( null, pathOfPositions );
					}
					break;
				case EnemyController.AI_STATE_LEAVING:
					hasPositionTarget = enemyComponent.HasPositionTarget( ref position );
					if( !hasPositionTarget && !enemyComponent.CompletedThePath() )
					{
						pathOfPositions = new Vector2[1];
						pathOfPositions[0] = structureComponent.GetGhostStartOffset( ref hasGhostStart );
						enemyComponent.SetPath( null, pathOfPositions );
					}
					break;
				case EnemyController.AI_STATE_DEAD:
					hasSlotTarget = enemyComponent.HasSlotTarget( ref row, ref column );
					hasPositionTarget = enemyComponent.HasPositionTarget( ref position );
					if( !hasSlotTarget && !hasPositionTarget && !enemyComponent.CompletedThePath() )
					{
						enemyComponent.GetCurrentRowAndColumn( ref currentRow, ref currentColumn );
						currentRowAndColumn = new Vector2( currentRow, currentColumn );
						ghostStartRowAndColumn = structureComponent.GetGhostStartRowAndColumn( ref hasGhostStart );
						pathOfSlots = UsefulFunctions.AStarManhatan( linearBlueprint, blueprintRows, blueprintColumns,
							BLUEPRINT_CODE_TILE_FIRST, currentRowAndColumn, ghostStartRowAndColumn );
						pathOfPositions = new Vector2[2];
						pathOfPositions[0] = structureComponent.GetGhostStartOffset( ref hasGhostStart );
						pathOfPositions[1] = structureComponent.GetGhostCageOffset( 1, ref hasGhostStart );
						/*halmeida - the target slot of the A* is the ghost start slot which is in one specific
						side of the cage's door front, the right side. If I give this path to an enemy that is
						on the left side of the cage's door front, he will be forced to cross the front of the
						door just to come back to the center of the door front and enter. For that reason, I
						will discard the last slot of the path of slots.*/
						if( pathOfSlots != null )
						{
							UsefulFunctions.DecreaseArray<Vector2>( ref pathOfSlots, pathOfSlots.Length-1 ); 
						}
						enemyComponent.SetPath( pathOfSlots, pathOfPositions );
					}
					break;
			}
		}
	}

	private void StartEnemyFragility()
	{
		EnemyController enemyComponent = null;

		if( enemyComponents != null )
		{
			for( int i=0; i<enemyComponents.Length; i++ )
			{
				enemyComponent = enemyComponents[i];
				if( enemyComponent != null )
				{
					enemyComponent.BecomeFragile( enemyFragileDuration, enemyRecoveringDuration );
				}
			}
		}
	}

	private void ClearBlueprint( bool withDependencies )
	{
		if( blueprint != null )
		{
			if( structureComponent != null )
			{
				structureComponent.SetBlueprint( null, false );
			}
			for( int i=0; i<blueprint.Length; i++ )
			{
				blueprint[i] = null;
			}
			blueprint = null;
			blueprintRows = 0;
			blueprintColumns = 0;
			linearBlueprint = null;
		}
		if( withDependencies )
		{
			ClearTiles();
			ClearItems();
			ClearEnemies();
			ClearPlayer();
		}
		stageReady = false;
		stageCompleted = false;
		stageFailed = false;
	}

	private void ClearTiles()
	{
		if( structureComponent != null )
		{
			structureComponent.ClearTiles();
		}
	}

	private void ClearItems()
	{
		ItemController itemComponent = null;
		GameObject itemObject = null;

		if( itemComponents != null )
		{
			for( int i=0; i<itemComponents.Length; i++ )
			{
				itemComponent = itemComponents[i];
				if( itemComponent != null )
				{
					itemComponent.Clear();
					itemComponents[i] = null;
				}
			}
			itemComponents = null;
		}
		if( itemObjects != null )
		{
			for( int i=0; i<itemObjects.Length; i++ )
			{
				itemObject = itemObjects[i];
				if( itemObject != null )
				{
					Destroy( itemObject );
					itemObjects[i] = null;
				}
			}
			itemObjects = null;
		}
	}

	private void ClearEnemies()
	{
		EnemyController enemyComponent = null;
		GameObject enemyObject = null;

		if( enemyComponents != null )
		{
			for( int i=0; i<enemyComponents.Length; i++ )
			{
				enemyComponent = enemyComponents[i];
				if( enemyComponent != null )
				{
					enemyComponent.Clear();
					enemyComponents[i] = null;
				}
			}
			enemyComponents = null;
		}
		if( enemyObjects != null )
		{
			for( int i=0; i<enemyObjects.Length; i++ )
			{
				enemyObject = enemyObjects[i];
				if( enemyObject != null )
				{
					Destroy( enemyObject );
					enemyObjects[i] = null;
				}
			}
			enemyObjects = null;
		}
	}

	private void ClearPlayer()
	{
		if( playerComponent != null )
		{
			playerComponent.Clear();
			playerComponent = null;
		}
		if( playerObject != null )
		{
			Destroy( playerObject );
			playerObject = null;
		}
	}

	private void RemoveItem( int itemIndex )
	{
		ItemController itemComponent = null;
		GameObject itemObject = null;

		if( itemObjects != null )
		{
			if( (itemIndex > -1) && (itemIndex < itemObjects.Length) )
			{
				itemComponent = itemComponents[itemIndex];
				if( itemComponent != null )
				{
					itemComponent.Clear();
					itemComponents[itemIndex] = null;
				}
				itemObject = itemObjects[itemIndex];
				if( itemObject != null )
				{
					Destroy( itemObject );
					itemObjects[itemIndex] = null;
				}
				UsefulFunctions.DecreaseArray<ItemController>( ref itemComponents, itemIndex );
				UsefulFunctions.DecreaseArray<GameObject>(ref itemObjects, itemIndex );
			}
		}
	}

	public bool PrepareForRetry()
	{
		ItemController itemComponent = null;

		if( stageReady && stageFailed )
		{
			stageFailed = false;
			ClearEnemies();
			ClearPlayer();
			CreatePlayer();
			CreateEnemies();
			if( itemComponents != null )
			{
				for( int i=0; i<itemComponents.Length; i++ )
				{
					itemComponent = itemComponents[i];
					if( itemComponent != null )
					{
						itemComponent.SetPlayerController( playerComponent );
					}
				}
			}
			return true;
		}
		return false;
	}

	/*halmeida - Reset() brings the StageController to a state in which it can be loaded again.*/
	public void Reset()
	{
		ClearBlueprint( true );
	}

	/*halmeida - Clear() brings the StageController to a memory free state, in which it can no longer be used.*/
	public void Clear()
	{
		ClearBlueprint( true );
		gameController = null;
		itemDatabase = null;
		playerModel = null;
		enemyModels = null;
		if( enemyLayer != null )
		{
			Destroy( enemyLayer );
			enemyLayer = null;
		}
		if( itemLayer != null )
		{
			Destroy( itemLayer );
			itemLayer = null;
		}
		if( structureComponent != null )
		{
			structureComponent.Clear();
			structureComponent = null;
		}
		if( structureObject != null )
		{
			Destroy( structureObject );
			structureObject = null;
		}
	}

	public bool IsCompleted()
	{
		return stageCompleted;
	}

	public bool IsFailed()
	{
		return stageFailed;
	}

	public Vector2 GetStructureDimensions()
	{
		if( structureComponent != null )
		{
			if( structureComponent.SuccessfullyBuilt() )
			{
				return structureComponent.GetStructureDimensions();
			}
		}
		return Vector2.zero;
	}

	public void ReactToCommand( int command, int agentIndex, bool startCommand )
	{
		PlayerController agentController = null;

		if( agentIndex == -1 )
		{
			agentController = playerComponent;
		}
		if( agentController != null )
		{
			switch( command )
			{
				case InputManager.COMMAND_UP:
					agentController.TogglePushing( PlayerController.DIRECTION_UP, startCommand );
					break;
				case InputManager.COMMAND_LEFT:
					agentController.TogglePushing( PlayerController.DIRECTION_LEFT, startCommand );
					break;
				case InputManager.COMMAND_DOWN:
					agentController.TogglePushing( PlayerController.DIRECTION_DOWN, startCommand );
					break;
				case InputManager.COMMAND_RIGHT:
					agentController.TogglePushing( PlayerController.DIRECTION_RIGHT, startCommand );
					break;
			}
		}
	}

	/*halmeida - since this turned out being a huge method, I decided to place it at the end of the file
	to prevent it from getting in the way of scrolling for other methods.*/
	private void AccelerateAndMoveAgent( float timeStep, PlayerController agentComponent )
	{
		Vector2 agentOffset = Vector2.zero;
		Vector2 agentSpeeds = Vector2.zero;
		Vector2 slotCenter = Vector2.zero;
		Vector3 agentPosition = Vector3.zero;
		Vector2 maxStepSizes = Vector2.zero;
		float rowFloat = 0f;
		float columnFloat = 0f;
		float rowFraction = 0f;
		float columnFraction = 0f;
		float movementDistanceX = 0f;
		float movementDistanceY = 0f;
		float movementLeftOver = 0f;
		float movementStep = 0f;
		int rowInt = 0;
		int columnInt = 0;
		bool pushingUp = false;
		bool pushingLeft = false;
		bool pushingDown = false;
		bool pushingRight = false;
		bool occupiedUp = false;
		bool occupiedLeft = false;
		bool occupiedDown = false;
		bool occupiedRight = false;
		bool beforeHalf = false;
		int newMovementDirection = PlayerController.DIRECTION_INVALID;
		int rowToVerifyNormal = -1;
		int rowToVerifyAlternate = -1;
		int columnToVerifyNormal = -1;
		int columnToVerifyAlternate = -1;
		bool verifyAlternateSlot = false;
		bool alternateDirectionTaken = false;
		bool validSlot = false;
		bool finalOffsetReached = false;
		bool reevaluateAgentSlot = false;
		bool hasSlotTarget = false;
		bool hasPositionTarget = false;
		int targetRow = -1;
		int targetColumn = -1;
		Vector2 targetPosition = Vector2.zero;
		bool beforeTarget = false;
		bool atTarget = false;
		bool targetIsHalfSlot = false;
		bool targetIsPosition = false;

		if( stageReady )
		{
			if( agentComponent != null )
			{
				pushingUp = agentComponent.IsPushing( PlayerController.DIRECTION_UP );
				pushingLeft = agentComponent.IsPushing( PlayerController.DIRECTION_LEFT );
				pushingDown = agentComponent.IsPushing( PlayerController.DIRECTION_DOWN );
				pushingRight = agentComponent.IsPushing( PlayerController.DIRECTION_RIGHT );
				agentSpeeds = agentComponent.GetSpeeds();

				/*halmeida - acceleration from zero.*/
				if( agentSpeeds == Vector2.zero )
				{
					agentPosition = agentComponent.gameObject.transform.localPosition;
					agentOffset.x = agentPosition.x;
					agentOffset.y = agentPosition.y;
					structureComponent.GetRowAndColumnForOffset( agentOffset, ref rowFloat, ref columnFloat );
					rowInt = (int)rowFloat;
					columnInt = (int)columnFloat;
					rowFraction = rowFloat - rowInt;
					columnFraction = columnFloat - columnInt;
					/*halmeida - if the agent is stopped and pushing at any direction, I will let it start moving towards
					this direction if the agent is before the half of his current stage slot. If he is at the half or after
					the half, he will only be able to move if the next slot is not occupied by a tile.*/
					if( pushingUp && !pushingDown )
					{
						/*halmeida - since the row number increases as the y coordinate decreases, to be below the y coordinate
						of the center of the slot, we need row number that is bigger than 0.5.*/
						beforeHalf = (rowFraction > 0.5f);
						newMovementDirection = PlayerController.DIRECTION_UP;
						rowToVerifyAlternate = rowInt-1;
						columnToVerifyAlternate = columnInt;
					}
					else if( pushingDown && !pushingUp )
					{
						beforeHalf = (rowFraction < 0.5f);
						newMovementDirection = PlayerController.DIRECTION_DOWN;
						rowToVerifyAlternate = rowInt+1;
						columnToVerifyAlternate = columnInt;
					}
					if( pushingLeft && !pushingRight )
					{
						beforeHalf = (columnFraction > 0.5f);
						newMovementDirection = PlayerController.DIRECTION_LEFT;
						rowToVerifyAlternate = rowInt;
						columnToVerifyAlternate = columnInt-1;
					}
					else if( pushingRight && !pushingLeft )
					{
						beforeHalf = (columnFraction < 0.5f);
						newMovementDirection = PlayerController.DIRECTION_RIGHT;
						rowToVerifyAlternate = rowInt;
						columnToVerifyAlternate = columnInt+1;
					}
					if( newMovementDirection != PlayerController.DIRECTION_INVALID )
					{
						if( beforeHalf )
						{
							agentComponent.StartGoingTowards( newMovementDirection );
						}
						else
						{
							if( !structureComponent.GetOccupationForRowAndColumn( rowToVerifyAlternate, columnToVerifyAlternate,
								ref validSlot ) )
							{
								agentComponent.StartGoingTowards( newMovementDirection );
							}
						}
						/*halmeida - I will get the speeds again since they might have changed.*/
						agentSpeeds = agentComponent.GetSpeeds();
					}
					else if( agentComponent.HasAI() )
					{
						/*halmeida - this is an enemy who is stopped and is not currently pushing any directions or
						is pushing directions that cancel each other.*/
						hasSlotTarget = agentComponent.HasSlotTarget( ref targetRow, ref targetColumn );
						hasPositionTarget = agentComponent.HasPositionTarget( ref targetPosition );
						if( hasSlotTarget )
						{
							/*halmeida - push the enemy in the direction of the center of the target slot.*/
							if( (rowInt > targetRow) || ((rowInt == targetRow) && (rowFraction > 0.5f)) )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_UP, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_UP );
							}
							else if( (rowInt < targetRow) || ((rowInt == targetRow) && (rowFraction < 0.5f)) )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_DOWN, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_DOWN );
							}
							else if( (columnInt > targetColumn) || ((columnInt == targetColumn) && (columnFraction > 0.5f)) )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_LEFT, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_LEFT );
							}
							else if( (columnInt < targetColumn) || ((columnInt == targetColumn) && (columnFraction < 0.5f)) )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_RIGHT, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_RIGHT );
							}
							else
							{
								/*halmeida - the agent is exactly at the half of the target slot, it should
								move on to its next target.*/
								agentComponent.AdvanceTarget();
							}
						}
						else if( hasPositionTarget )
						{
							/*halmeida - push the enemy in the direction of the target position.*/
							if( agentOffset.y < targetPosition.y )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_UP, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_UP );
							}
							else if( agentOffset.y > targetPosition.y )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_DOWN, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_DOWN );
							}
							else if( agentOffset.x > targetPosition.x )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_LEFT, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_LEFT );
							}
							else if( agentOffset.x < targetPosition.x )
							{
								agentComponent.TogglePushing( PlayerController.DIRECTION_RIGHT, true );
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_RIGHT );
							}
							else
							{
								/*halmeida - the agent is exactly at the target position, it should
								move on to its next target.*/
								agentComponent.AdvanceTarget();
							}
						}
						else
						{
							/*halmeida - the agent has an AI but has no targets.*/
							if( agentComponent.CanMoveWithoutTarget() )
							{
								/*halmeida - provide the enemy with vicinity occupation information to let him start pushing
								one valid direction.*/
								occupiedUp = structureComponent.GetOccupationForRowAndColumn( rowInt-1, columnInt, ref validSlot );
								occupiedLeft = structureComponent.GetOccupationForRowAndColumn( rowInt, columnInt-1, ref validSlot );
								occupiedDown = structureComponent.GetOccupationForRowAndColumn( rowInt+1, columnInt, ref validSlot );
								occupiedRight = structureComponent.GetOccupationForRowAndColumn( rowInt, columnInt+1, ref validSlot );
								agentComponent.SetDirectionFromVicinityOccupation( occupiedUp, occupiedLeft, occupiedDown, occupiedRight );
							}
						}
						agentSpeeds = agentComponent.GetSpeeds();
					}
				}

				/*halmeida - movement, speed direction changing and breaking.*/
				if( agentSpeeds != Vector2.zero )
				{
					if( agentComponent.HasAI() )
					{
						if( agentComponent.HasPositionTarget( ref targetPosition ) )
						{
							targetIsPosition = true;
						}
						else
						{
							targetIsHalfSlot = true;
						}
					}
					else
					{
						targetIsHalfSlot = true;
					}
					/*halmeida - if the agent is located before the half of the current slot, it has to continue moving at least
					until it reaches the half of the slot. If the agent is at the half of the slot and still has some distance
					left to move, it can only continue moving if the next slot is not occupied by a tile, or if he is pushing
					towards a direction that has an empty slot. Direction reversals are always allowed.*/
					maxStepSizes = structureComponent.GetTileDimensions();
					maxStepSizes *= 0.4f;
					movementDistanceX = agentSpeeds.x * timeStep;
					movementDistanceY = agentSpeeds.y * timeStep;
					agentPosition = agentComponent.gameObject.transform.localPosition;
					agentOffset.x = agentPosition.x;
					agentOffset.y = agentPosition.y;
					reevaluateAgentSlot = true;
					while( (movementDistanceX != 0f) || (movementDistanceY != 0f) )
					{
						atTarget = false;
						if( reevaluateAgentSlot )
						{
							structureComponent.GetRowAndColumnForOffset( agentOffset, ref rowFloat, ref columnFloat );
							rowInt = (int)rowFloat;
							columnInt = (int)columnFloat;
							slotCenter = structureComponent.GetOffsetForRowAndColumn( rowInt, columnInt, ref validSlot );
							if( targetIsHalfSlot )
							{
								targetPosition = slotCenter;
							}
							reevaluateAgentSlot = false;
						}
						if( movementDistanceX < 0f )
						{
							beforeTarget = (agentOffset.x > targetPosition.x);
							if( beforeTarget )
							{
								if( agentOffset.x + movementDistanceX < targetPosition.x )
								{
									/*halmeida - the movement should pass the half of the slot. We will move until
									the half and save the rest of the movement to continue at the next iteration.*/
									movementStep = -(agentOffset.x - targetPosition.x);
									movementDistanceX -= movementStep;
									agentOffset.x = targetPosition.x;
									atTarget = true;
								}
								else if( agentOffset.x + movementDistanceX == targetPosition.x )
								{
									agentOffset.x = targetPosition.x;
									movementDistanceX = 0f;
									atTarget = true;
									finalOffsetReached = true;
								}
								else
								{
									agentOffset.x += movementDistanceX;
									movementDistanceX = 0f;
									finalOffsetReached = true;
								}
								/*halmeida - if the agent reached the half of the slot, we will need to verify the next
								slot to allow him to continue moving or not. This verification is done later in the method.*/
								if( targetIsHalfSlot )
								{
									rowToVerifyNormal = rowInt;
									columnToVerifyNormal = columnInt-1;
									newMovementDirection = PlayerController.DIRECTION_INVALID;
								}
							}
							else
							{
								if( targetIsHalfSlot )
								{
									/*halmeida - just keep moving but make sure to stop before the half of the next slot, or
									the chance to change direction or stop the movement at the half of the next slot could be lost.
									By setting a maximum step distance shorter than half of a slot, we ensure that.*/
									movementStep = (movementDistanceX < -maxStepSizes.x) ? -maxStepSizes.x : movementDistanceX;
									movementDistanceX = (movementDistanceX < movementStep) ? movementDistanceX - movementStep : 0f;
									agentOffset.x += movementStep;
									if( movementDistanceX == 0f )
									{
										finalOffsetReached = true;
									}
									else
									{
										/*halmeida - this movement step, started from the half or after the half of the slot, might have
										brought the agent into a new slot.*/
										reevaluateAgentSlot = true;
									}
								}
								else if( targetIsPosition && (agentOffset.x < targetPosition.x) )
								{
									/*halmeida - we have to reverse the speed cause the target position is behind the agent.*/
									pushingRight = true;
									pushingLeft = false;
								}
							}
							/*halmeida - perform speed reversal.*/
							if( pushingRight && !pushingLeft )
							{
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_RIGHT );
								movementDistanceX = -movementDistanceX;
							}
						}
						else if( movementDistanceX > 0f )
						{
							beforeTarget = (agentOffset.x < targetPosition.x);
							if( beforeTarget )
							{
								if( agentOffset.x + movementDistanceX > targetPosition.x )
								{
									/*halmeida - the movement should pass the half of the slot. We will move until
									the half and save the rest of the movement to continue at the next iteration.*/
									movementStep = targetPosition.x - agentOffset.x;
									movementDistanceX -= movementStep;
									agentOffset.x = targetPosition.x;
									atTarget = true;
								}
								else if( agentOffset.x + movementDistanceX == targetPosition.x )
								{
									agentOffset.x = targetPosition.x;
									movementDistanceX = 0f;
									atTarget = true;
									finalOffsetReached = true;
								}
								else
								{
									agentOffset.x += movementDistanceX;
									movementDistanceX = 0f;
									finalOffsetReached = true;
								}
								/*halmeida - if the agent reached the half of the slot, we will need to verify the next
								slot to allow him to continue moving or not. This verification is done later in the method.*/
								if( targetIsHalfSlot )
								{
									rowToVerifyNormal = rowInt;
									columnToVerifyNormal = columnInt+1;
									newMovementDirection = PlayerController.DIRECTION_INVALID;
								}
							}
							else
							{
								if( targetIsHalfSlot )
								{
									/*halmeida - just keep moving but make sure to stop before the half of the next slot, or
									the chance to change direction or stop the movement at the half of the next slot could be lost.
									By setting a maximum step distance shorter than half of a slot, we ensure that.*/
									movementStep = (movementDistanceX > maxStepSizes.x) ? maxStepSizes.x : movementDistanceX;
									movementDistanceX = (movementDistanceX > movementStep) ? movementDistanceX - movementStep : 0f;
									agentOffset.x += movementStep;
									if( movementDistanceX == 0f )
									{
										finalOffsetReached = true;
									}
									else
									{
										/*halmeida - this movement step, started from the half or after the half of the slot, might have
										brought the agent into a new slot.*/
										reevaluateAgentSlot = true;
									}
								}
								else if( targetIsPosition && (agentOffset.x > targetPosition.x) )
								{
									/*halmeida - we have to reverse the speed cause the target position is behind the agent.*/
									pushingLeft = true;
									pushingRight = false;
								}
							}
							/*halmeida - perform speed reversal.*/
							if( pushingLeft && !pushingRight )
							{
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_LEFT );
								movementDistanceX = -movementDistanceX;
							}
						}
						if( movementDistanceY < 0f )
						{
							beforeTarget = (agentOffset.y > targetPosition.y);
							if( beforeTarget )
							{
								if( agentOffset.y + movementDistanceY < targetPosition.y )
								{
									/*halmeida - the movement should pass the half of the slot. We will move until
									the half and save the rest of the movement to continue at the next iteration.*/
									movementStep = -(agentOffset.y - targetPosition.y);
									movementDistanceY -= movementStep;
									agentOffset.y = targetPosition.y;
									atTarget = true;
								}
								else if( agentOffset.y + movementDistanceY == targetPosition.y )
								{
									agentOffset.y = targetPosition.y;
									movementDistanceY = 0f;
									atTarget = true;
									finalOffsetReached = true;
								}
								else
								{
									agentOffset.y += movementDistanceY;
									movementDistanceY = 0f;
									finalOffsetReached = true;
								}
								/*halmeida - if the agent reached the half of the slot, we will need to verify the next
								slot to allow him to continue moving or not. This verification is done later in the method.*/
								if( targetIsHalfSlot )
								{
									rowToVerifyNormal = rowInt+1;
									columnToVerifyNormal = columnInt;
									newMovementDirection = PlayerController.DIRECTION_INVALID;
								}
							}
							else
							{
								if( targetIsHalfSlot )
								{
									/*halmeida - just keep moving but make sure to stop before the half of the next slot, or
									the chance to change direction or stop the movement at the half of the next slot could be lost.
									By setting a maximum step distance shorter than half of a slot, we ensure that.*/
									movementStep = (movementDistanceY < -maxStepSizes.y) ? -maxStepSizes.y : movementDistanceY;
									movementDistanceY = (movementDistanceY < movementStep) ? movementDistanceY - movementStep : 0f;
									agentOffset.y += movementStep;
									if( movementDistanceY == 0f )
									{
										finalOffsetReached = true;
									}
									else
									{
										/*halmeida - this movement step, started from the half or after the half of the slot, might have
										brought the agent into a new slot.*/
										reevaluateAgentSlot = true;
									}
								}
								else if( targetIsPosition && (agentOffset.y < targetPosition.y) )
								{
									/*halmeida - we have to reverse the speed cause the target position is behind the agent.*/
									pushingUp = true;
									pushingDown = false;
								}
							}
							/*halmeida - perform speed reversal.*/
							if( pushingUp && !pushingDown )
							{
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_UP );
								movementDistanceY = -movementDistanceY;
							}
						}
						else if( movementDistanceY > 0f )
						{
							beforeTarget = (agentOffset.y < targetPosition.y);
							if( beforeTarget )
							{
								if( agentOffset.y + movementDistanceY > targetPosition.y )
								{
									/*halmeida - the movement should pass the half of the slot. We will move until
									the half and save the rest of the movement to continue at the next iteration.*/
									movementStep = targetPosition.y - agentOffset.y;
									movementDistanceY -= movementStep;
									agentOffset.y = targetPosition.y;
									atTarget = true;
								}
								else if( agentOffset.y + movementDistanceY == targetPosition.y )
								{
									agentOffset.y = targetPosition.y;
									movementDistanceY = 0f;
									atTarget = true;
									finalOffsetReached = true;
								}
								else
								{
									agentOffset.y += movementDistanceY;
									movementDistanceY = 0f;
									finalOffsetReached = true;
								}
								/*halmeida - if the agent reached the half of the slot, we will need to verify the next
								slot to allow him to continue moving or not. This verification is done later in the method.*/
								if( targetIsHalfSlot )
								{
									rowToVerifyNormal = rowInt-1;
									columnToVerifyNormal = columnInt;
									newMovementDirection = PlayerController.DIRECTION_INVALID;
								}
							}
							else
							{
								if( targetIsHalfSlot )
								{
									/*halmeida - just keep moving but make sure to stop before the half of the next slot, or
									the chance to change direction or stop the movement at the half of the next slot could be lost.
									By setting a maximum step distance shorter than half of a slot, we ensure that.*/
									movementStep = (movementDistanceY > maxStepSizes.y) ? maxStepSizes.y : movementDistanceY;
									movementDistanceY = (movementDistanceY > movementStep) ? movementDistanceY - movementStep : 0f;
									agentOffset.y += movementStep;
									if( movementDistanceY == 0f )
									{
										finalOffsetReached = true;
									}
									else
									{
										/*halmeida - this movement step, started from the half or after the half of the slot, might have
										brought the agent into a new slot.*/
										reevaluateAgentSlot = true;
									}
								}
								else if( targetIsPosition && (agentOffset.y > targetPosition.y) )
								{
									/*halmeida - we have to reverse the speed cause the target position is behind the agent.*/
									pushingDown = true;
									pushingUp = false;
								}
							}
							/*halmeida - perform speed reversal.*/
							if( pushingDown && !pushingUp )
							{
								agentComponent.StartGoingTowards( PlayerController.DIRECTION_DOWN );
								movementDistanceY = -movementDistanceY;
							}
						}
						/*halmeida - perform curve or break at half of slot.*/
						if( atTarget )
						{
							/*halmeida - record the distance that still has to be moved.*/
							movementLeftOver = 0f;
							if( movementDistanceX != 0f )
							{
								movementLeftOver = (movementDistanceX < 0f) ? -movementDistanceX : movementDistanceX;
							}
							else if( movementDistanceY != 0f )
							{
								movementLeftOver = (movementDistanceY < 0f) ? -movementDistanceY : movementDistanceY;
							}
							/*halmeida - we will operate differently if the agent has an AI or not.*/
							if( !agentComponent.HasAI() )
							{
								/*halmeida - check if other direction is being pushed, to change the direction of the speed.*/
								verifyAlternateSlot = false;
								alternateDirectionTaken = false;
								if( pushingUp && !pushingDown )
								{
									verifyAlternateSlot = true;
									rowToVerifyAlternate = rowInt-1;
									columnToVerifyAlternate = columnInt;
									newMovementDirection = PlayerController.DIRECTION_UP;
								}
								else if( pushingDown && !pushingUp )
								{
									verifyAlternateSlot = true;
									rowToVerifyAlternate = rowInt+1;
									columnToVerifyAlternate = columnInt;
									newMovementDirection = PlayerController.DIRECTION_DOWN;
								}
								if( pushingLeft && !pushingRight )
								{
									verifyAlternateSlot = true;
									rowToVerifyAlternate = rowInt;
									columnToVerifyAlternate = columnInt-1;
									newMovementDirection = PlayerController.DIRECTION_LEFT;
								}
								else if( pushingRight && !pushingLeft )
								{
									verifyAlternateSlot = true;
									rowToVerifyAlternate = rowInt;
									columnToVerifyAlternate = columnInt+1;
									newMovementDirection = PlayerController.DIRECTION_RIGHT;
								}
								if( verifyAlternateSlot )
								{
									if( !structureComponent.GetOccupationForRowAndColumn( rowToVerifyAlternate, columnToVerifyAlternate,
										ref validSlot ) )
									{
										agentComponent.StartGoingTowards( newMovementDirection );
										/*halmeida - the distance that still had to be moved will be moved in this new direction.*/
										switch( newMovementDirection )
										{
											case PlayerController.DIRECTION_UP:
												movementDistanceX = 0f;
												movementDistanceY = movementLeftOver;
												break;
											case PlayerController.DIRECTION_LEFT:
												movementDistanceX = -movementLeftOver;
												movementDistanceY = 0f;
												break;
											case PlayerController.DIRECTION_DOWN:
												movementDistanceX = 0f;
												movementDistanceY = -movementLeftOver;
												break;
											case PlayerController.DIRECTION_RIGHT:
												movementDistanceX = movementLeftOver;
												movementDistanceY = 0f;
												break;
										}
										alternateDirectionTaken = true;
									}
								}
								if( !alternateDirectionTaken )
								{
									if( structureComponent.GetOccupationForRowAndColumn( rowToVerifyNormal, columnToVerifyNormal,
										ref validSlot ) )
									{
										agentComponent.StopGoing( true );
										agentOffset.x = slotCenter.x;
										agentOffset.y = slotCenter.y;
										movementDistanceX = 0f;
										movementDistanceY = 0f;
										finalOffsetReached = true;
									}
								}
							}
							else
							{
								/*halmeida - agent has an AI and is at target.*/
								hasSlotTarget = agentComponent.HasSlotTarget( ref targetRow, ref targetColumn );
								hasPositionTarget = agentComponent.HasPositionTarget( ref targetPosition );
								if( !hasSlotTarget && !hasPositionTarget )
								{
									if( agentComponent.debug )
									{
										Debug.Log("Debug : StageController : agent without target reached center of slot.");
									}
									/*halmeida - when an AI agent reaches the half of a slot, it should not have the possibility of
									speed reversal. That's a rule of the original game, where ghosts only reverse their speed when
									the player collects a big pellet. To prevent reversal, I establish that the way back is always
									occupied with a tile.*/
									if( agentComponent.IsGoingTowards( PlayerController.DIRECTION_DOWN ) )
									{
										occupiedUp = true;
									}
									else
									{
										occupiedUp = structureComponent.GetOccupationForRowAndColumn( rowInt-1, columnInt, ref validSlot );
									}
									if( agentComponent.IsGoingTowards( PlayerController.DIRECTION_RIGHT ) )
									{
										occupiedLeft = true;
									}
									else
									{
										occupiedLeft = structureComponent.GetOccupationForRowAndColumn( rowInt, columnInt-1, ref validSlot );
									}
									if( agentComponent.IsGoingTowards( PlayerController.DIRECTION_UP ) )
									{
										occupiedDown = true;
									}
									else
									{
										occupiedDown = structureComponent.GetOccupationForRowAndColumn( rowInt+1, columnInt, ref validSlot );
									}
									if( agentComponent.IsGoingTowards( PlayerController.DIRECTION_LEFT ) )
									{
										occupiedRight = true;
									}
									else
									{
										occupiedRight = structureComponent.GetOccupationForRowAndColumn( rowInt, columnInt+1, ref validSlot );
									}
									newMovementDirection = agentComponent.SetDirectionFromVicinityOccupation( occupiedUp, occupiedLeft,
										occupiedDown, occupiedRight );
									if( newMovementDirection == PlayerController.DIRECTION_INVALID )
									{
										if( agentComponent.debug )
										{
											Debug.Log("Debug : StageController : agent couldn't get a valid movement direction, will stop.");
										}
										/*halmeida - no need to ask the agent to stop going, beacause it is already done within the
										SetDirectionFromVicinityOccupation call.*/
										//agentComponent.StopGoing( false );
										agentOffset.x = slotCenter.x;
										agentOffset.y = slotCenter.y;
										movementDistanceX = 0f;
										movementDistanceY = 0f;
										finalOffsetReached = true;
									}
									else
									{
										if( agentComponent.debug )
										{
											Debug.Log("Debug : StageController : agent got a new valid movement direction, "+newMovementDirection+".");
										}
										/*halmeida - no need to ask the agent to start going, beacause it is already done within the
										SetDirectionFromVicinityOccupation call.*/
										//agentComponent.StartGoingTowards( newMovementDirection );
										switch( newMovementDirection )
										{
											case PlayerController.DIRECTION_UP:
												movementDistanceX = 0f;
												movementDistanceY = movementLeftOver;
												break;
											case PlayerController.DIRECTION_LEFT:
												movementDistanceX = -movementLeftOver;
												movementDistanceY = 0f;
												break;
											case PlayerController.DIRECTION_DOWN:
												movementDistanceX = 0f;
												movementDistanceY = -movementLeftOver;
												break;
											case PlayerController.DIRECTION_RIGHT:
												movementDistanceX = movementLeftOver;
												movementDistanceY = 0f;
												break;
										}
									}
								}
								else
								{
									/*halmeida - agent has a target and he just reached the exact point.*/
									agentComponent.AdvanceTarget();
									newMovementDirection = PlayerController.DIRECTION_INVALID;
									hasSlotTarget = agentComponent.HasSlotTarget( ref targetRow, ref targetColumn );
									hasPositionTarget = agentComponent.HasPositionTarget( ref targetPosition );
									if( hasSlotTarget )
									{
										if( rowInt > targetRow )
										{
											newMovementDirection = PlayerController.DIRECTION_UP;
											movementDistanceX = 0f;
											movementDistanceY = movementLeftOver;
										}
										else if( rowInt < targetRow )
										{
											newMovementDirection = PlayerController.DIRECTION_DOWN;
											movementDistanceX = 0f;
											movementDistanceY = -movementLeftOver;
										}
										else if( columnInt > targetColumn )
										{
											newMovementDirection = PlayerController.DIRECTION_LEFT;
											movementDistanceX = -movementLeftOver;
											movementDistanceY = 0f;
										}
										else if( columnInt < targetColumn )
										{
											newMovementDirection = PlayerController.DIRECTION_RIGHT;
											movementDistanceX = movementLeftOver;
											movementDistanceY = 0f;
										}
									}
									else if( hasPositionTarget )
									{
										if( agentOffset.y < targetPosition.y )
										{
											newMovementDirection = PlayerController.DIRECTION_UP;
											movementDistanceX = 0f;
											movementDistanceY = movementLeftOver;
										}
										else if( agentOffset.y > targetPosition.y )
										{
											newMovementDirection = PlayerController.DIRECTION_DOWN;
											movementDistanceX = 0f;
											movementDistanceY = -movementLeftOver;
										}
										else if( agentOffset.x > targetPosition.x )
										{
											newMovementDirection = PlayerController.DIRECTION_LEFT;
											movementDistanceX = -movementLeftOver;
											movementDistanceY = 0f;
										}
										else if( agentOffset.x < targetPosition.x )
										{
											newMovementDirection = PlayerController.DIRECTION_RIGHT;
											movementDistanceX = movementLeftOver;
											movementDistanceY = 0f;
										}

									}
									if( newMovementDirection != PlayerController.DIRECTION_INVALID )
									{
										agentComponent.TogglePushing( newMovementDirection, true );
										agentComponent.StartGoingTowards( newMovementDirection );
									}
									else
									{
										agentComponent.CancelInput();
										agentComponent.StopGoing( false );
										movementDistanceX = 0f;
										movementDistanceY = 0f;
										finalOffsetReached = true;
									}
								}
							}
						}
						/*halmeida - place agent at the final position.*/
						if( finalOffsetReached )
						{
							/*halmeida - the final position may lie outside the limits of the stage. If that's the case,
							we need to bring the agent around to the extreme opposite position.*/
							structureComponent.GetRowAndColumnForOffset( agentOffset, ref rowFloat, ref columnFloat );
							rowInt = (int)rowFloat;
							columnInt = (int)columnFloat;
							if( (rowInt < 0) || (rowInt >= blueprintRows) || (columnInt < 0) || (columnInt >= blueprintColumns) )
							{
								slotCenter = structureComponent.GetOffsetForRowAndColumn( rowInt, columnInt, ref validSlot );
								agentOffset.x = agentOffset.x - slotCenter.x;
								agentOffset.y = agentOffset.y - slotCenter.y;
								rowInt = (rowInt < 0) ? blueprintRows-1 : rowInt;
								rowInt = (rowInt >= blueprintRows) ? 0 : rowInt;
								columnInt = (columnInt < 0) ? blueprintColumns-1 : columnInt;
								columnInt = (columnInt >= blueprintColumns) ? 0 : columnInt;
								slotCenter = structureComponent.GetOffsetForRowAndColumn( rowInt, columnInt, ref validSlot );
								agentOffset.x = slotCenter.x + agentOffset.x;
								agentOffset.y = slotCenter.y + agentOffset.y;
							}
							agentPosition.x = agentOffset.x;
							agentPosition.y = agentOffset.y;
							agentComponent.gameObject.transform.localPosition = agentPosition;
							agentComponent.SetCurrentRowAndColumn( rowInt, columnInt );
						}
					}
				}
			}
		}
	}
}
