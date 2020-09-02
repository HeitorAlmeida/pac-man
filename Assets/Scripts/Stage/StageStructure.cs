using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageStructure : MonoBehaviour
{
	private TileFactory tileFactory;
	private float tileWidth;
	private float tileHeight;
	private int tileRows;
	private int tileColumns;
	private float width;
	private float height;
	private int[][] blueprint;
	private GameObject[][] tileObjects;
	private Tile[][] tileComponents;
	private bool playerStartFound;
	private Vector2 playerStartOffset;
	private Vector2 playerStartRowAndColumn;
	private bool ghostStartFound;
	private Vector2 ghostStartOffset;
	private Vector2 ghostStartRowAndColumn;
	/*halmeida - the vector below will carry the offsets for the three positions within the cage: left, middle and right.*/
	private Vector2[] ghostCageOffsets;
	private Vector2[] ghostCageRowsAndColumns;
	private bool built;

	void Awake()
	{
		tileFactory = null;
		tileWidth = 0f;
		tileHeight = 0f;
		tileRows = 0;
		tileColumns = 0;
		width = 0f;
		height = 0f;
		blueprint = null;
		tileObjects = null;
		tileComponents = null;
		playerStartFound = false;
		playerStartOffset = Vector2.zero;
		playerStartRowAndColumn = new Vector2( -1f, -1f );
		ghostStartFound = false;
		ghostStartOffset = Vector2.zero;
		ghostStartRowAndColumn = new Vector2( -1f, -1f );
		ghostCageOffsets = null;
		ghostCageRowsAndColumns = null;
		built = false;
	}

	public void SetTileFactory( TileFactory newTileFactory )
	{
		Vector2 tileWorldDimensions = Vector2.zero;

		tileFactory = newTileFactory;
		if( tileFactory != null )
		{
			tileWorldDimensions = tileFactory.GetTileWorldDimensions();
			tileWidth = tileWorldDimensions.x;
			tileHeight = tileWorldDimensions.y;
		}
		else
		{
			tileWidth = 0f;
			tileHeight = 0f;
		}
		if( tileObjects != null )
		{
			/*halmeida - since the tile factory establishes how the tiles should look, I clear the current tiles,
			which were built with the previous tile factory, and recreate them with the new one.*/
			ClearTiles();
			CreateTiles();
		}
	}

	public void SetBlueprint( int[][] newBlueprint, bool buildTiles )
	{
		int[] blueprintRow = null;

		tileRows = 0;
		tileColumns = 0;
		blueprint = newBlueprint;
		if( blueprint != null )
		{
			if( blueprint.Length > 0 )
			{
				blueprintRow = blueprint[0];
				if( blueprintRow != null )
				{
					if( blueprintRow.Length > 0 )
					{
						tileRows = blueprint.Length;
						tileColumns = blueprintRow.Length;
					}
				}
			}
		}
		if( (tileObjects != null) || buildTiles )
		{
			ClearTiles();
			CreateTiles();
		}
	}

	public bool CreateTiles()
	{
		GameObject[] tileObjectsRow = null;
		Tile[] tileComponentsRow = null;
		int[] blueprintRow = null;
		int blueprintCode = StageController.BLUEPRINT_CODE_INVALID;
		int tileShapeCode = Tile.SHAPE_CODE_INVALID;
		int tileTextureVariation = 0;
		GameObject newTileObject = null;
		Tile newTileComponent = null;
		float cageCenterY = 0f;

		if( (blueprint == null) || (tileFactory == null) || (tileObjects != null) || (tileRows < 1) || (tileColumns < 1) )
		{
			return false;
		}
		tileObjects = new GameObject[tileRows][];
		tileComponents = new Tile[tileRows][];
		for( int i=0; i<tileRows; i++ )
		{
			tileObjectsRow = new GameObject[tileColumns];
			tileComponentsRow = new Tile[tileColumns];
			blueprintRow = blueprint[i];
			for( int j=0; j<tileColumns; j++ )
			{
				newTileObject = null;
				newTileComponent = null;
				blueprintCode = blueprintRow[j];
				if( IsTilePresenceCode( blueprintCode ) )
				{
					tileShapeCode = GetTileShapeCodeFromBlueprint( i, j );
					if( tileShapeCode != Tile.SHAPE_CODE_INVALID )
					{
						tileTextureVariation = GetTileTextureVariation( blueprintCode );
						CreateTile( i, j, tileShapeCode, tileTextureVariation, ref newTileObject, ref newTileComponent );
					}
				}
				else
				{
					if( !playerStartFound )
					{
						if( blueprintCode == StageController.BLUEPRINT_CODE_PAC_START_RIGHT )
						{
							playerStartFound = true;
							playerStartOffset.x = j * tileWidth;
							playerStartOffset.y = -(i + 0.5f) * tileHeight;
							playerStartRowAndColumn = new Vector2( i, j );
						}
					}
					if( !ghostStartFound )
					{
						if( blueprintCode == StageController.BLUEPRINT_CODE_GHOST_START_RIGHT )
						{
							ghostStartFound = true;
							ghostStartOffset.x = j * tileWidth;
							ghostStartOffset.y = -(i + 0.5f) * tileHeight;
							ghostStartRowAndColumn = new Vector2( i, j );
							ghostCageOffsets = new Vector2[3];
							ghostCageRowsAndColumns = new Vector2[3];
							cageCenterY = ghostStartOffset.y - 3*tileHeight;
							ghostCageOffsets[0] = new Vector2( ghostStartOffset.x - 2*tileWidth, cageCenterY );
							ghostCageOffsets[1] = new Vector2( ghostStartOffset.x, cageCenterY );
							ghostCageOffsets[2] = new Vector2( ghostStartOffset.x + 2*tileWidth, cageCenterY );
							ghostCageRowsAndColumns[0] = new Vector2( ghostStartRowAndColumn.x + 3, ghostStartRowAndColumn.y - 2 );
							ghostCageRowsAndColumns[1] = new Vector2( ghostStartRowAndColumn.x + 3, ghostStartRowAndColumn.y );
							ghostCageRowsAndColumns[2] = new Vector2( ghostStartRowAndColumn.x + 3, ghostStartRowAndColumn.y + 2 );  
						}
					}
				}
				tileObjectsRow[j] = newTileObject;
				tileComponentsRow[j] = newTileComponent;
			}
			tileObjects[i] = tileObjectsRow;
			tileComponents[i] = tileComponentsRow;
		}
		width = tileColumns * tileWidth;
		height = tileRows * tileHeight;
		built = true;
		return true;
	}

	public void ClearTiles()
	{
		Tile[] tileComponentsRow = null;
		Tile tileComponent = null;
		GameObject[] tileObjectsRow = null;
		GameObject tileObject = null;

		if( tileComponents != null )
		{
			for( int i=0; i<tileComponents.Length; i++ )
			{
				tileComponentsRow = tileComponents[i];
				if( tileComponentsRow != null )
				{
					for( int j=0; j<tileComponentsRow.Length; j++ )
					{
						tileComponent = tileComponentsRow[j];
						if( tileComponent != null )
						{
							tileComponent.Clear();
							tileComponentsRow[j] = null;
						}
					}
					tileComponents[i] = null;
				}
			}
			tileComponents = null;
		}
		if( tileObjects != null )
		{
			for( int i=0; i<tileObjects.Length; i++ )
			{
				tileObjectsRow = tileObjects[i];
				if( tileObjectsRow != null )
				{
					for( int j=0; j<tileObjectsRow.Length; j++ )
					{
						tileObject = tileObjectsRow[j];
						if( tileObject != null )
						{
							Destroy( tileObject );
							tileObjectsRow[j] = null;
						}
					}
					tileObjects[i] = null;
				}
			}
			tileObjects = null;
		}
		width = 0f;
		height = 0f;
		playerStartFound = false;
		playerStartOffset = Vector2.zero;
		ghostStartFound = false;
		ghostStartOffset = Vector2.zero;
		ghostCageOffsets = null;
		built = false;
	}

	private bool CreateTile( int row, int column, int shapeCode, int textureVariation, ref GameObject tileObject,
		ref Tile tileComponent )
	{
		Vector3 tileObjectPosition = Vector3.zero;

		if( tileFactory != null )
		{
			tileObject = tileFactory.GetTileByShapeCode( shapeCode, textureVariation, ref tileComponent );
			if( tileObject != null )
			{
				tileObject.transform.SetParent( gameObject.transform, false );
				tileObjectPosition.x = (column + 0.5f) * tileWidth;
				tileObjectPosition.y = -1f * (row + 0.5f) * tileHeight;
				tileObjectPosition.z = 0f;
				tileObject.transform.localPosition = tileObjectPosition;
				return true;
			}
		}
		return false;
	}

	public bool SuccessfullyBuilt()
	{
		return built;
	}

	/*halmeida - gives the offset of the center of the slot.*/
	public Vector2 GetOffsetForRowAndColumn( int row, int column, ref bool validSlot )
	{
		Vector2 slotOffset = Vector2.zero;

		validSlot = false;
		if( (tileFactory != null) && (blueprint != null) )
		{
			slotOffset.x = (column + 0.5f) * tileWidth;
			slotOffset.y = -(row + 0.5f) * tileHeight;
			validSlot = ((row > -1) && (row < tileRows) && (column > -1) && (column < tileColumns));
		}
		return slotOffset;
	}

	public bool GetRowAndColumnForOffset( Vector2 offset, ref float rowFloat, ref float columnFloat )
	{
		rowFloat = 0f;
		columnFloat = 0f;
		if( tileFactory == null )
		{
			return false;
		}
		rowFloat = -offset.y / tileHeight;
		columnFloat = offset.x / tileWidth;
		return true;
	}

	public bool GetOccupationForRowAndColumn( int row, int column, ref bool validSlot )
	{
		validSlot = false;
		if( tileObjects != null )
		{
			validSlot = ((row > -1) && (row < tileRows) && (column > -1) && (column < tileColumns));
			if( validSlot )
			{
				if( tileObjects[row][column] != null )
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector2 GetPlayerStartOffset( ref bool hasPlayerStart )
	{
		hasPlayerStart = playerStartFound;
		return playerStartOffset;
	}

	public Vector2 GetPlayerStartRowAndColumn( ref bool hasPlayerStart )
	{
		hasPlayerStart = playerStartFound;
		return playerStartRowAndColumn;
	}

	/*halmeida - this returns a position between two slots.*/
	public Vector2 GetGhostStartOffset( ref bool hasGhostStart )
	{
		hasGhostStart = ghostStartFound;
		return ghostStartOffset;
	}

	/*halmeida - this returns the row and column of the slot to the right.*/
	public Vector2 GetGhostStartRowAndColumn( ref bool hasGhostStart )
	{
		hasGhostStart = ghostStartFound;
		return ghostStartRowAndColumn;
	}

	public Vector2 GetGhostCageOffset( int cageOffsetIndex, ref bool hasCageOffset )
	{
		Vector2 cageOffset = Vector2.zero;

		hasCageOffset = false;
		if( ghostCageOffsets != null )
		{
			if( (cageOffsetIndex > -1) && (cageOffsetIndex < ghostCageOffsets.Length) )
			{
				hasCageOffset = true;
				cageOffset = ghostCageOffsets[cageOffsetIndex];
			}
		}
		return cageOffset;
	}

	public void Clear()
	{
		ClearTiles();
		tileFactory = null;
		blueprint = null;
	}

	public Vector2 GetStructureDimensions()
	{
		return new Vector2( width, height );
	}

	public Vector2 GetTileDimensions()
	{
		return new Vector2( tileWidth, tileHeight );
	}

	public int GetTileTextureVariation( int blueprintCode )
	{
		int variation = 0;

		if( (blueprintCode > StageController.BLUEPRINT_CODE_TILE_FIRST) && (blueprintCode <= StageController.BLUEPRINT_CODE_TILE_LAST) )
		{
			variation = blueprintCode - StageController.BLUEPRINT_CODE_TILE_FIRST;
		}
		return variation;
	}

	public int GetTileShapeCodeFromBlueprint( int row, int column )
	{
		bool U = false;
		bool UL = false;
		bool L = false;
		bool LD = false;
		bool D = false;
		bool DR = false;
		bool R = false;
		bool UR = false;
		int shapeCode = Tile.SHAPE_CODE_INVALID;

		if( GetOccupiedDirections( row, column, ref U, ref UL, ref L, ref LD, ref D, ref DR, ref R, ref UR ) )
		{
			shapeCode = Tile.GetShapeCodeByNeighborhood( U, UL, L, LD, D, DR, R, UR );
		}
		return shapeCode;
	}

	/*halmeida - the function below is used for tiles. Outside the limits of the structure, every position is considered
	unoccupied.*/
	private bool GetOccupiedDirections( int row, int column, ref bool up, ref bool upLeft, ref bool left, ref bool leftDown,
		ref bool down, ref bool downRight, ref bool right, ref bool upRight )
	{
		int[] blueprintRow = null;
		int checkingRow = 0;
		int checkingColumn = 0;

		if( (tileRows <= 0) || (tileColumns <= 0) )
		{
			return false;
		}
		if( (row < 0) || (row >= tileRows) || (column < 0) || (column >= tileColumns) )
		{
			return false;
		}
		if( blueprint == null )
		{
			return false;
		}

		checkingRow = row - 1;
		/*halmeida - outside the limits determined by the tileRows and tileColumns variables,
		every position should be considered unoccupied.*/
		if( checkingRow < 0 )
		{
			upLeft = false;
			up = false;
			upRight = false;
		}
		else
		{
			blueprintRow = blueprint[checkingRow];
			checkingColumn = column - 1;
			if( checkingColumn < 0 )
			{
				upLeft = false;
			}
			else
			{
				upLeft = IsTilePresenceCode( blueprintRow[checkingColumn] );
			}
			checkingColumn = column;
			up = IsTilePresenceCode( blueprintRow[checkingColumn] );
			checkingColumn = column + 1;
			if( checkingColumn < tileColumns )
			{
				upRight = IsTilePresenceCode( blueprintRow[checkingColumn] );
			}
			else
			{
				upRight = false;
			}
		}

		checkingRow = row;
		blueprintRow = blueprint[checkingRow];
		checkingColumn = column - 1;
		if( checkingColumn < 0 )
		{
			left = false;
		}
		else
		{
			left = IsTilePresenceCode( blueprintRow[checkingColumn] );
		}
		checkingColumn = column + 1;
		if( checkingColumn < tileColumns )
		{
			right = IsTilePresenceCode( blueprintRow[checkingColumn] );
		}
		else
		{
			right = false;
		}

		checkingRow = row + 1;
		if( checkingRow >= tileRows )
		{
			leftDown = false;
			down = false;
			downRight = false;
		}
		else
		{
			blueprintRow = blueprint[checkingRow];
			checkingColumn = column - 1;
			if( checkingColumn < 0 )
			{
				leftDown = false;
			}
			else
			{
				leftDown = IsTilePresenceCode( blueprintRow[checkingColumn] );
			}
			checkingColumn = column;
			down = IsTilePresenceCode( blueprintRow[checkingColumn] );
			checkingColumn = column + 1;
			if( checkingColumn < tileColumns )
			{
				downRight = IsTilePresenceCode( blueprintRow[checkingColumn] );
			}
			else
			{
				downRight = false;
			}
		}
		return true;
	}

	public static bool IsTilePresenceCode( int blueprintCode )
	{
		return !( (blueprintCode < StageController.BLUEPRINT_CODE_TILE_FIRST) ||
			(blueprintCode > StageController.BLUEPRINT_CODE_TILE_LAST));
	}
}
