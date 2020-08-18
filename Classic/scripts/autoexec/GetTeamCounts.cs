// GetTeamCounts Script
//
// GetTeamCounts was made to accurately keep track of how many players
// are on teams, on the server, on each team, observer, etc.
// AntiTurrets, Team Balance Notify, AntiCloak, AutoBalance, Base Rape and Base Rape Notify all rely on this.
// It runs every 5 seconds.
// 
// Whether or not a portion of the script is run or not depends on 
// if a player has switched teams or not. Or if a team changing event has occurred.
// This is triggered via various event functions throughout the game.
//

// Set reset string
$GetCountsStatus = "UPDATE";

package StartTeamCounts 
{

function CreateServer( %mission, %missionType )
{
	parent::CreateServer( %mission, %missionType );
	//Call for a GetTeamCount update
	GetTeamCounts( %game, %client, %respawn );
	
	// Set when server starts
	// Used to reset timelimit (if voted) when map changes
	$DefaultTimeLimit = $Host::TimeLimit;
	
	// Prevent package from being activated if it is already
	if (!isActivePackage(TeamCountsTriggers))
		activatePackage(TeamCountsTriggers);
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(StartTeamCounts))
    activatePackage(StartTeamCounts);

function GetTeamCounts( %game, %client, %respawn )
{	
	switch$($GetCountsStatus)
	{
		case UPDATE:
			//Get teamcounts
			if($countdownStarted && $MatchStarted ) 
			{	
				//Team Count code by Keen
				$PlayerCount[0] = 0;
				$PlayerCount[1] = 0;
				$PlayerCount[2] = 0;

				for(%i = 0; %i < ClientGroup.getCount(); %i++)
				{
					%client = ClientGroup.getObject(%i);
						
					//if(!%client.isAIControlled())
						$PlayerCount[%client.team]++;
				}
				
				//echo ("$PlayerCount[0] " @  $PlayerCount[0]);
				//echo ("$PlayerCount[1] " @  $PlayerCount[1]);
				//echo ("$PlayerCount[2] " @  $PlayerCount[2]);

				//Amount of players on teams
				$TotalTeamPlayerCount = $PlayerCount[1] + $PlayerCount[2];
				//Amount of all players including observers
				$AllPlayerCount = $PlayerCount[1] + $PlayerCount[2] + $PlayerCount[0];
				//Difference Variables
				%team1difference = $PlayerCount[1] - $PlayerCount[2];
				%team2difference = $PlayerCount[2] - $PlayerCount[1];
				
				if( !$Host::TournamentMode )
				{
					if( $CurrentMissionType $= "CTF" )
					{
						NBRStatusNotify(%game);
						CheckAntiPack(%game);
					}
					TeamBalanceNotify(%game, %team1difference, %team2difference);
				}
				
				//Set so counter wont run when it doesnt need to.
				$GetCountsStatus = "IDLE";
			}
		case IDLE:
			//Do Nothing
	}
	
	if(isEventPending($GetCountsSchedule)) 
		cancel($GetCountsSchedule);
		
	//Call itself again. Every 5 seconds.
	$GetCountsSchedule = schedule(5000, 0, "GetTeamCounts");	
}


// Triggers a Full run
function ResetGetCountsStatus() 
{
   $GetCountsStatus = "UPDATE";
}

// Proper Overrides
// Events that determine a TeamGetCounts update
package TeamCountsTriggers
{

function DefaultGame::clientJoinTeam( %game, %client, %team, %respawn )
{
	Parent::clientJoinTeam( %game, %client, %team, %respawn );

	//Trigger GetCounts
	ResetGetCountsStatus();
}

function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs, %respawned)
{
   Parent::clientChangeTeam(%game, %client, %team, %fromObs, %respawned);
   
   //Trigger GetCounts
   ResetGetCountsStatus();
}

function DefaultGame::assignClientTeam(%game, %client, %respawn )
{
	Parent::assignClientTeam(%game, %client, %respawn );
   
	//Trigger GetCounts
	ResetGetCountsStatus();
}

function DefaultGame::onClientEnterObserverMode( %game, %client )
{
	Parent::onClientEnterObserverMode( %game, %client );
   
	//Trigger GetCounts
	ResetGetCountsStatus();
}

function DefaultGame::AIChangeTeam(%game, %client, %newTeam)
{
	Parent::AIChangeTeam(%game, %client, %newTeam);

	//Trigger GetCounts
	ResetGetCountsStatus();
}

function GameConnection::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch)
{
	Parent::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch);
   
	//Reset GetCounts
	ResetGetCountsStatus();
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset GetCounts
	ResetGetCountsStatus();
}

function GameConnection::onDrop(%client, %reason)
{
	Parent::onDrop(%client, %reason);
	
	//Reset GetCounts
	ResetGetCountsStatus();
}

};