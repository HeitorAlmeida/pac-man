using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public const int COMMAND_UP = 0;
	public const int COMMAND_LEFT = 1;
	public const int COMMAND_DOWN = 2;
	public const int COMMAND_RIGHT = 3;

	private StageController stageController;

	void Awake()
	{
		stageController = null;
	}

	public void SetStageController( StageController newStageController )
	{
		stageController = newStageController;
	}

	public void Progress( float timeStep )
	{
		if( stageController != null )
		{
			if( Input.GetKeyDown( KeyCode.W ) || Input.GetKeyDown( KeyCode.UpArrow ) )
			{
				stageController.ReactToCommand( COMMAND_UP, -1, true );
			}
			if( Input.GetKeyUp( KeyCode.W ) || Input.GetKeyUp( KeyCode.UpArrow ) )
			{
				stageController.ReactToCommand( COMMAND_UP, -1, false );
			}
			if( Input.GetKeyDown( KeyCode.A ) || Input.GetKeyDown( KeyCode.LeftArrow ) )
			{
				stageController.ReactToCommand( COMMAND_LEFT, -1, true );
			}
			if( Input.GetKeyUp( KeyCode.A ) || Input.GetKeyUp( KeyCode.LeftArrow ) )
			{
				stageController.ReactToCommand( COMMAND_LEFT, -1, false );
			}
			if( Input.GetKeyDown( KeyCode.S ) || Input.GetKeyDown( KeyCode.DownArrow ) )
			{
				stageController.ReactToCommand( COMMAND_DOWN, -1, true );
			}
			if( Input.GetKeyUp( KeyCode.S ) || Input.GetKeyUp( KeyCode.DownArrow ) )
			{
				stageController.ReactToCommand( COMMAND_DOWN, -1, false );
			}
			if( Input.GetKeyDown( KeyCode.D ) || Input.GetKeyDown( KeyCode.RightArrow ) )
			{
				stageController.ReactToCommand( COMMAND_RIGHT, -1, true );
			}
			if( Input.GetKeyUp( KeyCode.D ) || Input.GetKeyUp( KeyCode.RightArrow ) )
			{
				stageController.ReactToCommand( COMMAND_RIGHT, -1, false );
			}
		}
	}
}
