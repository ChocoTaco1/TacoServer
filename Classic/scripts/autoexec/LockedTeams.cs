// LockedTeams.cs

// Toggled in serverCmdStartNewVote in VoteMenu.cs
//
// case "ToggleLockedTeams":
// 	if (%client.isAdmin)
// 	{
// 		if(!$LockedTeams)
// 		{
// 			if(!isActivePackage(LockedTeams))
// 				activatePackage(LockedTeams);
// 			$LockedTeams = 1;
// 			messageClient( %client, '', "Locked Teams has been enabled.~wfx/powered/vehicle_screen_on.wav" );
// 			adminLog(%client, " has enabled Locked Teams.");
// 		}
// 		else
// 		{
// 			if(isActivePackage(LockedTeams))
// 				deactivatePackage(LockedTeams);
// 			$LockedTeams = 0;
// 			messageClient( %client, '', "Locked Teams has been disabled.~wfx/powered/vehicle_screen_on.wav" );
// 			adminLog(%client, " has disabled Locked Teams.");
// 		}
// 	}
// 	return;

// Reset in MissionTypeOptions.cs
//
// if(isActivePackage(LockedTeams) && !$LockedTeams)
// 	deactivatePackage(LockedTeams);

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