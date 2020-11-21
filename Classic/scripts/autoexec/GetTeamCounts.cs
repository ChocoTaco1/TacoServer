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
	GetTeamCounts(%game);
	
	// Set when server starts
	// Used to reset timelimit (if voted) when map changes
	$DefaultTimeLimit = $Host::TimeLimit;
	
	// Prevent package from being activated if it is already
	if (!isActivePackage(TeamCountsTriggers))
		activatePackage(TeamCountsTriggers);
	
	// Auto Daily Hard Server Restart at a specific time
	// getTimeDif from zDarkTigerStats.cs
	if($dtStats::version)
		schedule(getTimeDif("10\t00\tam"),0,"quit");
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(StartTeamCounts))
    activatePackage(StartTeamCounts);

function GetTeamCounts(%game)
{	
	switch$($GetCountsStatus)
	{
		case UPDATE:
			if($countdownStarted && $MatchStarted ) 
			{	
				//Variables
				$TotalTeamPlayerCount = $TeamRank[1, count] + $TeamRank[2, count];
				$AllPlayerCount = $HostGamePlayerCount;
				$Observers = $HostGamePlayerCount - ($TeamRank[1, count] + $TeamRank[2, count]);
				
				//echo("$PlayerCount[0] " @  $HostGamePlayerCount - ($TeamRank[1, count] + $TeamRank[2, count]));
				//echo("$PlayerCount[1] " @  $TeamRank[1, count]);
				//echo("$PlayerCount[2] " @  $TeamRank[2, count]);
		
				if( !$Host::TournamentMode )
				{
					if( $CurrentMissionType $= "CTF" )
					{
						NBRStatusNotify(%game);
						CheckAntiPack(%game);
					}
					TeamBalanceNotify(%game);
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