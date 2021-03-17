//LockedTeams.cs

package LockedTeams
{

// function serverCmdClientJoinTeam(%client, %team)
// {
	// if ($LockedTeams)
	// {
		// messageClient( %client, '', "Teams are locked. Asked the admin to set your team. (JoinTeam)" );
		// return;
	// }
	// Parent::serverCmdClientJoinTeam(%client, %team);
// }

function serverCmdClientJoinGame(%client)
{
	if ($LockedTeams)
	{
		messageClient( %client, '', "Teams are locked. Asked the admin to set your team." );
		return;
	}
	Parent::serverCmdClientJoinGame(%client);
}

function serverCmdClientPickedTeam(%client, %option)
{
	Parent::serverCmdClientPickedTeam(%client, %option); //Put first
	if($LockedTeams) //Added
	{		
		if($Host::TournamentMode && %client.team !$= 0) //Added
		{	
			messageClient( %client, '', "Teams are locked. Asked the admin to set your team." );
			serverCmdClientMakeObserver( %client );
		}	
		return;
	}
}

function serverCmdClientTeamChange(%client, %option)
{
	if ($LockedTeams)
	{
		messageClient( %client, '', "Teams are locked. Asked the admin to set your team." );
		return;
	}
	Parent::serverCmdClientTeamChange(%client, %option);
}
	
};