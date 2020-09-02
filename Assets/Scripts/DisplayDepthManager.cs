public class DisplayDepthManager
{
	public const float ELEMENT_FIRST_DEPTH = 0f; /*halmeida - the higher it goes, the deeper into the screen it goes.*/
	public const float ELEMENT_TO_ELEMENT_OFFSET = 0.2f;
	
	public const int SMALLEST_ELEMENT_CODE = 0;
	public const int ELEMENT_CODE_PLAYER = 0;
	public const int ELEMENT_CODE_ENEMY = 1;
	public const int ELEMENT_CODE_ITEM = 2;
	public const int ELEMENT_CODE_TILE = 3;
	public const int ELEMENT_CODE_BACKGROUND = 4;
	public const int BIGGEST_ELEMENT_CODE = 4;
	
	public static float GetElementDepth( int elementCode )
	{
		float totalDepth = 0.0f;
		int steps = 0;
		
		totalDepth = ELEMENT_FIRST_DEPTH;
		steps = elementCode - SMALLEST_ELEMENT_CODE;
		totalDepth += steps * ELEMENT_TO_ELEMENT_OFFSET;
		return totalDepth;
	}
}
