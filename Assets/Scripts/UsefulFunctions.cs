using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsefulFunctions
{
	public UsefulFunctions()
	{
		return;
	}

	public static void IncreaseArray<T>( ref T[] array, T newElement )
	{
		T[] newArray = null;
		int length = 0;

		if( array != null )
		{
			length = array.Length;
			newArray = new T[length + 1];
			for( int i=0; i<length; i++ )
			{
				newArray[i] = array[i];
			}
		}
		else
		{
			newArray = new T[1];
		}
		newArray[newArray.Length - 1] = newElement;
		array = newArray;
		newArray = null;
	}

	public static void DecreaseArray<T>( ref T[] array, int removeIndex )
	{
		T[] newArray = null;
		int length = 0;

		if( array != null )
		{
			length = array.Length;
			if( (removeIndex > -1) && (removeIndex < length) )
			{
				if( length > 1 )
				{
					newArray = new T[length - 1];
					for( int i=0; i<removeIndex; i++ )
					{
						newArray[i] = array[i];
					}
					for( int i=removeIndex; i<(length-1); i++ )
					{
						newArray[i] = array[i+1];
					}
					array = newArray;
					newArray = null;
				}
				else
				{
					array = null;
				}
			}
		}
	}

	/*halmeida - implementation of the A* algorithm over an array-represented matrix. It is an array but we will interpret it as
	a matrix. If a neighbor of an unoccupied element in the matrix is also not occupied, then they are connected. The heuristic
	used is the Manhatan distance to the target slot.*/
	public static Vector2[] AStarManhatan( int[] linearMatrix, int matrixRows, int matrixColumns, int occupiedSlotCode,
		Vector2 startRowAndColumn, Vector2 targetRowAndColumn )
	{
		int totalSlots = 0;
		int startRow = -1;
		int startColumn = -1;
		int targetRow = -1;
		int targetColumn = -1;
		bool[] visited = null;
		int[] previousRow = null;
		int[] previousColumn = null;
		int[] distanceTraveled = null;
		int[] functionHeuristic = null;
		int[] functionValue = null;
		int startSlotIndex = -1;
		int targetSlotIndex = -1;
		int visitedRow = -1;
		int visitedColumn = -1;
		int visitedIndex = -1;
		int slotRow = -1;
		int slotColumn = -1;
		int slotIndex = -1;
		int slotFunctionValue = -1;
		int[] calculatedSlotIndexes = null;
		int distance = 0;
		int heuristic = 0;
		int direction = 0;
		int slotToVisitIndex = -1;
		int slotToVisitFunctionValue = -1;
		int calculatedIndex = -1;
		Vector2 stepRowAndColumn = Vector2.zero;
		Vector2[] backwardsPath = null;
		Vector2[] finalPath = null;
		int pathLength = 0;

		if( (linearMatrix == null) || (matrixRows < 1) || (matrixColumns < 1) )
		{
			return null;
		}
		totalSlots = matrixRows * matrixColumns;
		if( linearMatrix.Length != totalSlots )
		{
			return null;
		}
		startRow = (int)startRowAndColumn.x;
		startColumn = (int)startRowAndColumn.y;
		targetRow = (int)targetRowAndColumn.x;
		targetColumn = (int)targetRowAndColumn.y;
		if( (startRow < 0) || (startRow >= matrixRows) || (startColumn < 0) || (startColumn >= matrixColumns) )
		{
			return null;
		}
		if( (targetRow < 0) || (targetRow >= matrixRows) || (targetColumn < 0) || (targetColumn >= matrixColumns) )
		{
			return null;
		}
		visited = new bool[totalSlots];
		previousRow = new int[totalSlots];
		previousColumn = new int[totalSlots];
		distanceTraveled = new int[totalSlots];
		functionHeuristic = new int[totalSlots];
		functionValue = new int[totalSlots];
		for( int i=0; i<totalSlots; i++ )
		{
			visited[i] = false;
			previousRow[i] = -1;
			previousColumn[i] = -1;
			distanceTraveled[i] = 0;
			functionHeuristic[i] = -1;
			functionValue[i] = 0;
		}
		/*halmeida - we visit the starting slot, regardless of its slot code.*/
		startSlotIndex = (startRow * matrixColumns) + startColumn;
		visitedIndex = startSlotIndex;
		visited[visitedIndex] = true;
		visitedRow = startRow;
		visitedColumn = startColumn;
		/*halmeida - every time we visit a slot, we will ask if it was the target slot.*/
		targetSlotIndex = (targetRow * matrixColumns) + targetColumn;
		while( !visited[targetSlotIndex] )
		{
			/*halmeida - update the function value for every slot that is reacheable from the last visited slot.*/
			direction = 0;
			while( direction < 4 )
			{
				slotRow = -1;
				slotColumn = -1;
				slotIndex = -1;
				switch( direction )
				{
					case 0:
						/*halmeida - update the function value for the slot above.*/
						if( visitedRow > 0 )
						{
							slotRow = visitedRow-1;
							slotColumn = visitedColumn;
							slotIndex = visitedIndex - matrixColumns;
						}
						break;
					case 1:
						/*halmeida - update the function value for the slot to the left.*/
						if( visitedColumn > 0 )
						{
							slotRow = visitedRow;
							slotColumn = visitedColumn-1;
							slotIndex = visitedIndex-1;
						}
						break;
					case 2:
						/*halmeida - update the function value for the slot below.*/
						if( visitedRow < (matrixRows-1) )
						{
							slotRow = visitedRow+1;
							slotColumn = visitedColumn;
							slotIndex = visitedIndex + matrixColumns;
						}
						break;
					case 3:
						/*halmeida - update the function value for the slot to the right.*/
						if( visitedColumn < (matrixColumns-1) )
						{
							slotRow = visitedRow;
							slotColumn = visitedColumn+1;
							slotIndex = visitedIndex+1;
						}
						break;
				}
				/*halmeida - if the slot towards direction exists in the matrix, we will have an index for it.*/
				if( slotIndex > -1 )
				{
					/*halmeida - if the slot is not occupied and hasn't been visited, we update all its potential reaching information.*/
					if( (linearMatrix[slotIndex] != occupiedSlotCode) && !visited[slotIndex] )
					{
						previousRow[slotIndex] = visitedRow;
						previousColumn[slotIndex] = visitedColumn;
						distance = distanceTraveled[visitedIndex] + 1;
						distanceTraveled[slotIndex] = distance;
						heuristic = functionHeuristic[slotIndex];
						if( heuristic == -1 )
						{
							heuristic = ManhatanDistance( slotRow, slotColumn, targetRow, targetColumn );
							functionHeuristic[slotIndex] = heuristic;
						}
						functionValue[slotIndex] = distance + heuristic;
						/*halmeida - add this slot as a potential next slot to be visited.*/
						IncreaseArray<int>( ref calculatedSlotIndexes, slotIndex );
					}
				}
				direction++;
			}
			/*halmeida - at this point, we will have added to the list of calculated indexes all the potential next slots.
			We need to choose the one we will visit, which is the one with the smallest function value. If there is no
			calculated index left in the list, then we must stop cause there is no path from start slot to target slot.*/
			if( calculatedSlotIndexes == null )
			{
				return null;
			}
			else
			{
				calculatedIndex = -1;
				slotToVisitIndex = -1;
				slotToVisitFunctionValue = -1;
				for( int i=0; i<calculatedSlotIndexes.Length; i++ )
				{
					slotIndex = calculatedSlotIndexes[i];
					slotFunctionValue = functionValue[slotIndex];
					if( slotToVisitIndex == -1 )
					{
						calculatedIndex = i;
						slotToVisitIndex = slotIndex;
						slotToVisitFunctionValue = slotFunctionValue;
					}
					else
					{
						if( slotFunctionValue < slotToVisitFunctionValue )
						{
							calculatedIndex = i;
							slotToVisitIndex = slotIndex;
							slotToVisitFunctionValue = slotFunctionValue;
						}
					}
				}
				/*halmeida - now that I have the smallest one, I visit it and remove it from the list of candidates for visiting.*/
				visited[slotToVisitIndex] = true;
				visitedIndex = slotToVisitIndex;
				visitedRow = visitedIndex / matrixColumns;
				visitedColumn = visitedIndex % matrixColumns;
				DecreaseArray<int>( ref calculatedSlotIndexes, calculatedIndex );
			}
		}
		/*halmeida - at this point we have just visited the target slot. All that's left to do is write the path. We
		will move backwards from the end of the path towards the start, following the information of previous slot for
		each visited slot.*/
		stepRowAndColumn = new Vector2( visitedRow, visitedColumn );
		IncreaseArray<Vector2>( ref backwardsPath, stepRowAndColumn );
		slotRow = previousRow[visitedIndex];
		slotColumn = previousColumn[visitedIndex];
		while( slotRow > -1 )
		{
			stepRowAndColumn = new Vector2( slotRow, slotColumn );
			IncreaseArray<Vector2>( ref backwardsPath, stepRowAndColumn );
			slotIndex = (slotRow * matrixColumns) + slotColumn;
			slotRow = previousRow[slotIndex];
			slotColumn = previousColumn[slotIndex];
		}
		/*halmeida - now we have the path backwards. We correct it and return it.*/
		pathLength = backwardsPath.Length;
		finalPath = new Vector2[pathLength];
		for( int i=0; i<pathLength; i++ )
		{
			finalPath[i] = backwardsPath[pathLength-1-i];
		}
		backwardsPath = null;
		return finalPath;
	}

	public static int ManhatanDistance( int originRow, int originColumn, int targetRow, int targetColumn )
	{
		int rows = 0;
		int columns = 0;

		rows = targetRow - originRow;
		rows *= (rows < 0) ? -1 : 1;
		columns = targetColumn - originColumn;
		columns *= (columns < 0) ? -1 : 1;
		return (rows + columns);
	}
}
