using UnityEngine;
using System.Collections;

public class TileFactory : MonoBehaviour
{
	public const int UNDEFINED_FACTORY_ID = -1;

	/*halmeida - the sprite for each tile shape will be set in the editor.*/
	public string factoryName;
	public int factoryID;
	public Sprite tileSeparated;  //01
	public Sprite tileU;          //02
	public Sprite tileL;          //03
	public Sprite tileD;          //04
	public Sprite tileR;          //05
	public Sprite tileUL;         //06
	public Sprite tileUD;         //07
	public Sprite tileUR;         //08
	public Sprite tileLD;         //09
	public Sprite tileLR;         //10
	public Sprite tileDR;         //11
	public Sprite tileULD;        //12
	public Sprite tileULR;        //13
	public Sprite tileUDR;        //14
	public Sprite tileLDR;        //15
	public Sprite tileUULL;       //16
	public Sprite tileLLDD;       //17
	public Sprite tileDDRR;       //18
	public Sprite tileURUR;       //19
	public Sprite tileULDR;       //20
	public Sprite tileULLDD;      //21
	public Sprite tileUDDRR;      //22
	public Sprite tileUULLD;      //23
	public Sprite tileUDRUR;      //24
	public Sprite tileULRUR;      //25
	public Sprite tileUULLR;      //26
	public Sprite tileLDDRR;      //27
	public Sprite tileLLDDR;      //28
	public Sprite tileUULLLDD;    //29
	public Sprite tileLLDDDRR;    //30
	public Sprite tileUDDRRUR;    //31
	public Sprite tileUULLRUR;    //32
	public Sprite tileULDRUR;     //33
	public Sprite tileUULLDR;     //34
	public Sprite tileULLDDR;     //35
	public Sprite tileULDDRR;     //36
	public Sprite tileUULLDRUR;   //37
	public Sprite tileUULLDDRR;   //38
	public Sprite tileULDDRRUR;   //39
	public Sprite tileUULLLDDR;   //40
	public Sprite tileULLDDRUR;   //41
	public Sprite tileULLDDDRR;   //42
	public Sprite tileUULLDDRRUR; //43
	public Sprite tileUULLLDDRUR; //44
	public Sprite tileUULLLDDDRR; //45
	public Sprite tileULLDDDRRUR; //46
	public Sprite tileConnected;  //47
	public Sprite[] tileULVariations;
	public Sprite[] tileUDVariations;
	public Sprite[] tileURVariations;
	public Sprite[] tileLDVariations;
	public Sprite[] tileLRVariations;
	public Sprite[] tileDRVariations;

	private int[] codes;
	private Sprite[] tileSpriteModels;
	private float tileWidth;
	private float tileHeight;

	void Awake()
	{
		Sprite tile = null;

		codes = new int[47];
		tileSpriteModels = new Sprite[47];
		codes[46] = Tile.SHAPE_CODE_CONNECTED;
		tileSpriteModels[46] =  tileConnected;
		codes[45] = Tile.SHAPE_CODE_ULLDDDRRUR;
		tileSpriteModels[45] =  tileULLDDDRRUR;
		codes[44] = Tile.SHAPE_CODE_UULLLDDDRR;
		tileSpriteModels[44] =  tileUULLLDDDRR;
		codes[43] = Tile.SHAPE_CODE_UULLLDDRUR;
		tileSpriteModels[43] =  tileUULLLDDRUR;
		codes[42] = Tile.SHAPE_CODE_UULLDDRRUR;
		tileSpriteModels[42] =  tileUULLDDRRUR;
		codes[41] = Tile.SHAPE_CODE_ULLDDDRR;
		tileSpriteModels[41] =  tileULLDDDRR;
		codes[40] = Tile.SHAPE_CODE_ULLDDRUR;
		tileSpriteModels[40] =  tileULLDDRUR;
		codes[39] = Tile.SHAPE_CODE_UULLLDDR;
		tileSpriteModels[39] =  tileUULLLDDR;
		codes[38] = Tile.SHAPE_CODE_ULDDRRUR;
		tileSpriteModels[38] =  tileULDDRRUR;
		codes[37] = Tile.SHAPE_CODE_UULLDDRR;
		tileSpriteModels[37] =  tileUULLDDRR;
		codes[36] = Tile.SHAPE_CODE_UULLDRUR;
		tileSpriteModels[36] =  tileUULLDRUR;
		codes[35] = Tile.SHAPE_CODE_ULDDRR;
		tileSpriteModels[35] =  tileULDDRR;
		codes[34] = Tile.SHAPE_CODE_ULLDDR;
		tileSpriteModels[34] =  tileULLDDR;
		codes[33] = Tile.SHAPE_CODE_UULLDR;
		tileSpriteModels[33] =  tileUULLDR;
		codes[32] = Tile.SHAPE_CODE_ULDRUR;
		tileSpriteModels[32] =  tileULDRUR;
		codes[31] = Tile.SHAPE_CODE_UULLRUR;
		tileSpriteModels[31] =  tileUULLRUR;
		codes[30] = Tile.SHAPE_CODE_UDDRRUR;
		tileSpriteModels[30] =  tileUDDRRUR;
		codes[29] = Tile.SHAPE_CODE_LLDDDRR;
		tileSpriteModels[29] =  tileLLDDDRR;
		codes[28] = Tile.SHAPE_CODE_UULLLDD;
		tileSpriteModels[28] =  tileUULLLDD;
		codes[27] = Tile.SHAPE_CODE_LLDDR;
		tileSpriteModels[27] =  tileLLDDR;
		codes[26] = Tile.SHAPE_CODE_LDDRR;
		tileSpriteModels[26] =  tileLDDRR;
		codes[25] = Tile.SHAPE_CODE_UULLR;
		tileSpriteModels[25] =  tileUULLR;
		codes[24] = Tile.SHAPE_CODE_ULRUR;
		tileSpriteModels[24] =  tileULRUR;
		codes[23] = Tile.SHAPE_CODE_UDRUR;
		tileSpriteModels[23] =  tileUDRUR;
		codes[22] = Tile.SHAPE_CODE_UULLD;
		tileSpriteModels[22] =  tileUULLD;
		codes[21] = Tile.SHAPE_CODE_UDDRR;
		tileSpriteModels[21] =  tileUDDRR;
		codes[20] = Tile.SHAPE_CODE_ULLDD;
		tileSpriteModels[20] =  tileULLDD;
		codes[19] = Tile.SHAPE_CODE_ULDR;
		tileSpriteModels[19] =  tileULDR;
		codes[18] = Tile.SHAPE_CODE_URUR;
		tileSpriteModels[18] =  tileURUR;
		codes[17] = Tile.SHAPE_CODE_DDRR;
		tileSpriteModels[17] =  tileDDRR;
		codes[16] = Tile.SHAPE_CODE_LLDD;
		tileSpriteModels[16] =  tileLLDD;
		codes[15] = Tile.SHAPE_CODE_UULL;
		tileSpriteModels[15] =  tileUULL;
		codes[14] = Tile.SHAPE_CODE_LDR;
		tileSpriteModels[14] =  tileLDR;
		codes[13] = Tile.SHAPE_CODE_UDR;
		tileSpriteModels[13] =  tileUDR;
		codes[12] = Tile.SHAPE_CODE_ULR;
		tileSpriteModels[12] =  tileULR;
		codes[11] = Tile.SHAPE_CODE_ULD;
		tileSpriteModels[11] =  tileULD;
		codes[10] = Tile.SHAPE_CODE_DR;
		tileSpriteModels[10] =  tileDR;
		codes[9] = Tile.SHAPE_CODE_LR;
		tileSpriteModels[9] =  tileLR;
		codes[8] = Tile.SHAPE_CODE_LD;
		tileSpriteModels[8] =  tileLD;
		codes[7] = Tile.SHAPE_CODE_UR;
		tileSpriteModels[7] =  tileUR;
		codes[6] = Tile.SHAPE_CODE_UD;
		tileSpriteModels[6] =  tileUD;
		codes[5] = Tile.SHAPE_CODE_UL;
		tileSpriteModels[5] =  tileUL;
		codes[4] = Tile.SHAPE_CODE_R;
		tileSpriteModels[4] =  tileR;
		codes[3] = Tile.SHAPE_CODE_D;
		tileSpriteModels[3] =  tileD;
		codes[2] = Tile.SHAPE_CODE_L;
		tileSpriteModels[2] =  tileL;
		codes[1] = Tile.SHAPE_CODE_U;
		tileSpriteModels[1] =  tileU;
		codes[0] = Tile.SHAPE_CODE_SEPARATED;
		tileSpriteModels[0] = tileSeparated;
		tileWidth = 0f;
		tileHeight = 0f;
		if( tileConnected != null )
		{
			tile = tileConnected;
		}
		else
		{
			tile = tileSeparated;
		}
		if( tile != null )
		{
			tileWidth = tile.bounds.size.x;
			tileHeight = tile.bounds.size.y;
		}
	}

	public GameObject GetTileByNeighborhood( bool U = false, bool UL = false, bool L = false, bool LD = false, bool D = false,
		bool DR = false, bool R = false, bool UR = false )
	{
		int shapeCode = 0;
		Tile tileComponent = null;

		if( (codes == null) || (tileSpriteModels == null) )
		{
			return null;
		}
		shapeCode = Tile.GetShapeCodeByNeighborhood( U, UL, L, LD, D, DR, R, UR );
		return GetTileByShapeCode( shapeCode, 0, ref tileComponent );
	}

	public GameObject GetTileByShapeCode( int shapeCode, int textureVariation, ref Tile tileComponent )
	{
		Sprite tileSpriteModel = null;
		GameObject tileObject = null;
		SpriteRenderer tileSpriteRenderer = null;
		Sprite alternateModel = null;

		tileComponent = null;
		if( (codes == null) || (tileSpriteModels == null) || (shapeCode == Tile.SHAPE_CODE_INVALID) )
		{
			return null;
		}
		for( int i=0; i<codes.Length; i++ )
		{
			if( shapeCode == codes[i] )
			{
				if( i < tileSpriteModels.Length )
				{
					tileSpriteModel = tileSpriteModels[i];
				}
				if( HasTextureVariation( shapeCode, textureVariation, ref alternateModel ) )
				{
					tileSpriteModel = alternateModel;
				}
				break;
			}
		}
		if( tileSpriteModel != null )
		{
			tileObject = new GameObject( "Tile" );
			tileObject.transform.position = Vector3.zero;
			tileObject.transform.rotation = Quaternion.identity;
			tileObject.transform.localScale = Vector3.one;
			tileComponent = tileObject.AddComponent<Tile>();
			if( tileComponent != null )
			{
				tileComponent.SetShapeCode( shapeCode );
				tileSpriteRenderer = tileObject.AddComponent<SpriteRenderer>();
				tileComponent.SetTileRenderer( tileSpriteRenderer );
				tileComponent.SetSprite( tileSpriteModel );
			}
			else
			{
				Destroy( tileObject );
				tileObject = null;
			}
		}
		return tileObject;
	}

	public bool HasTextureVariation( int shapeCode, int textureVariation, ref Sprite alternateSprite )
	{
		alternateSprite = null;
		if( textureVariation < 1 )
		{
			return false;
		}
		switch( shapeCode )
		{
			case Tile.SHAPE_CODE_UL:
				if( tileULVariations != null )
				{
					textureVariation--;
					if( textureVariation < tileULVariations.Length )
					{
						alternateSprite = tileULVariations[textureVariation];
					}
				}
				break;
			case Tile.SHAPE_CODE_UD:
				if( tileUDVariations != null )
				{
					textureVariation--;
					if( textureVariation < tileUDVariations.Length )
					{
						alternateSprite = tileUDVariations[textureVariation];
					}
				}
				break;
			case Tile.SHAPE_CODE_UR:
				if( tileURVariations != null )
				{
					textureVariation--;
					if( textureVariation < tileURVariations.Length )
					{
						alternateSprite = tileURVariations[textureVariation];
					}
				}
				break;
			case Tile.SHAPE_CODE_LD:
				if( tileLDVariations != null )
				{
					textureVariation--;
					if( textureVariation < tileLDVariations.Length )
					{
						alternateSprite = tileLDVariations[textureVariation];
					}
				}
				break;
			case Tile.SHAPE_CODE_LR:
				if( tileLRVariations != null )
				{
					textureVariation--;
					if( textureVariation < tileLRVariations.Length )
					{
						alternateSprite = tileLRVariations[textureVariation];
					}
				}
				break;
			case Tile.SHAPE_CODE_DR:
				if( tileDRVariations != null )
				{
					textureVariation--;
					if( textureVariation < tileDRVariations.Length )
					{
						alternateSprite = tileDRVariations[textureVariation];
					}
				}
				break;
		}
		return (alternateSprite != null);
	}

	public Vector2 GetTileWorldDimensions()
	{
		return new Vector2( tileWidth, tileHeight );
	}

	public void Clear()
	{
		codes = null;
		tileSpriteModels = null;
	}
}
