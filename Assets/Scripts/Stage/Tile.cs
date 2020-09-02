using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
	/*halmeida - the list of possible tile shapes, with basis on the possible connections to the neighboring tiles.
	A diagonal connection can only happen if the two adjacent connections happen. For instance, a tile can only connect
	to its UL neighbor if it's connected to both its U neighbor and its L neighbor.*/
	public const int SHAPE_CODE_INVALID = -1;
	public const int SHAPE_CODE_CONNECTED  = 11111111;
	public const int SHAPE_CODE_ULLDDDRRUR = 10111111;
	public const int SHAPE_CODE_UULLLDDDRR = 11111110;
	public const int SHAPE_CODE_UULLLDDRUR = 11111011;
	public const int SHAPE_CODE_UULLDDRRUR = 11101111;
	public const int SHAPE_CODE_ULLDDDRR =   10111110;
	public const int SHAPE_CODE_ULLDDRUR =   10111011;
	public const int SHAPE_CODE_UULLLDDR =   11111010;
	public const int SHAPE_CODE_ULDDRRUR =   10101111;
	public const int SHAPE_CODE_UULLDDRR =   11101110;
	public const int SHAPE_CODE_UULLDRUR =   11101011;
	public const int SHAPE_CODE_ULDDRR =     10101110;
	public const int SHAPE_CODE_ULLDDR =     10111010;
	public const int SHAPE_CODE_UULLDR =     11101010;
	public const int SHAPE_CODE_ULDRUR =     10101011;
	public const int SHAPE_CODE_UULLRUR =    11100011;
	public const int SHAPE_CODE_UDDRRUR =    10001111;
	public const int SHAPE_CODE_LLDDDRR =    00111110;
	public const int SHAPE_CODE_UULLLDD =    11111000;
	public const int SHAPE_CODE_LLDDR =      00111010;
	public const int SHAPE_CODE_LDDRR =      00101110;
	public const int SHAPE_CODE_UULLR =      11100010;
	public const int SHAPE_CODE_ULRUR =      10100011;
	public const int SHAPE_CODE_UDRUR =      10001011;
	public const int SHAPE_CODE_UULLD =      11101000;
	public const int SHAPE_CODE_UDDRR =      10001110;
	public const int SHAPE_CODE_ULLDD =      10111000;
	public const int SHAPE_CODE_ULDR =       10101010;
	public const int SHAPE_CODE_URUR =       10000011;
	public const int SHAPE_CODE_DDRR =       00001110;
	public const int SHAPE_CODE_LLDD =       00111000;
	public const int SHAPE_CODE_UULL =       11100000;
	public const int SHAPE_CODE_LDR =        00101010;
	public const int SHAPE_CODE_UDR =        10001010;
	public const int SHAPE_CODE_ULR =        10100010;
	public const int SHAPE_CODE_ULD =        10101000;
	public const int SHAPE_CODE_DR =         00001010;
	public const int SHAPE_CODE_LR =         00100010;
	public const int SHAPE_CODE_LD =         00101000;
	public const int SHAPE_CODE_UR =         10000010;
	public const int SHAPE_CODE_UD =         10001000;
	public const int SHAPE_CODE_UL =         10100000;
	public const int SHAPE_CODE_R =          00000010;
	public const int SHAPE_CODE_D =          00001000;
	public const int SHAPE_CODE_L =          00100000;
	public const int SHAPE_CODE_U =          10000000;
	public const int SHAPE_CODE_SEPARATED =  00000000;

	private int shapeCode;
	private Sprite sprite;
	private SpriteRenderer tileRenderer;

	void Awake()
	{
		shapeCode = -1;
		sprite = null;
		/*halmeida - I do not seek for the rendering component within this object because the
		rendering component will be added to this object during its life cycle, after the awake.
		It should be provided to this class through SetTileRenderer, sparing the component search.*/
		tileRenderer = null;
	}

	public void SetShapeCode( int code )
	{
		shapeCode = code;
	}

	public int GetShapeCode()
	{
		return shapeCode;
	}

	public void SetTileRenderer( SpriteRenderer newTileRenderer )
	{
		tileRenderer = newTileRenderer;
	}

	public void SetSprite( Sprite newSprite )
	{
		if( tileRenderer != null )
		{
			sprite = newSprite;
			tileRenderer.sprite = sprite;
		}
	}

	public void SetRenderingAlpha( float newAlpha )
	{
		Color color = Color.white;

		if( tileRenderer != null )
		{
			color = tileRenderer.color;
			color.a = newAlpha;
			tileRenderer.color = color;
		}
	}

	public void Clear()
	{
		/*halmeida - do nothing since no memory is allocated by the instance.*/
	}

	public static int GetShapeCodeByNeighborhood( bool U = false, bool UL = false, bool L = false, bool LD = false, bool D = false,
		bool DR = false, bool R = false, bool UR = false )
	{
		int checksum = 0;

		/*halmeida - we have to remove the diagonals in case they are not effective, or the checksum
		will produce values that correspond to no tiles at all. For example, if a tile has its U, UL,
		L, LD, R and UR occupied, the produced checksum would be 11110011, which corresponds to no
		code in the list of codes. That happens because the code list was produced with basis on
		connections to the neighbors, not occupation of the neighboring positions. By removing the
		ineffective diagonals we will be transforming the information of occupation into the needed
		information of connections.*/
		if( !( U && L ) )
		{
			UL = false;
		}
		if( !( L && D ) )
		{
			LD = false;
		}
		if( !( D && R ) )
		{
			DR = false;
		}
		if( !( U && R ) )
		{
			UR = false;
		}
		/*halmeida - now we have transformed the information of occupation into the information
		of connections, for the diagonals are only connected if both adjacent directions are
		occupied.*/
		if( U )
		{
			checksum += 10000000;
		}
		if( UL )
		{
			checksum += 01000000;
		}
		if( L )
		{
			checksum += 00100000;
		}
		if( LD )
		{
			checksum += 00010000;
		}
		if( D )
		{
			checksum += 00001000;
		}
		if( DR )
		{
			checksum += 00000100;
		}
		if( R )
		{
			checksum += 00000010;
		}
		if( UR )
		{
			checksum += 00000001;
		}
		return checksum;
	}
}
