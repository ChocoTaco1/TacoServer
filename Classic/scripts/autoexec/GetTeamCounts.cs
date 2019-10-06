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
// All included in EvoClassicTaco.vl2

// Set reset string
$GetCountsClientTeamChange = true;

package StartTeamCounts 
{

function CreateServer( %mission, %missionType )
{
	parent::CreateServer( %mission, %missionType );
	//Call for a GetTeamCount update
	GetTeamCounts( %game, %client, %respawn );
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(StartTeamCounts))
    activatePackage(StartTeamCounts);

function GetTeamCounts( %game, %client, %respawn )
{	
	//Get teamcounts
	if( $GetCountsClientTeamChange && $countdownStarted && $MatchStarted ) 
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
		
		//Start Base Rape Notify
		schedule(500, 0, "NBRStatusNotify", %game);
		//Start Team Balance Notify
		schedule(1000, 0, "TeamBalanceNotify", %game, %team1difference, %team2difference);
		//Start AntiCloak
		schedule(1500, 0, "CheckAntiCloak", %game);
		
		//Set so counter wont run when it doesnt need to.
		$GetCountsClientTeamChange = false;
	}
		
	//Call itself again. Every 5 seconds.
	schedule(5000, 0, "GetTeamCounts");	
}


// Triggers a Full run
function ResetClientChangedTeams() 
{
   $GetCountsClientTeamChange = true;
}

// Proper Overrides
// Events that determine a TeamGetCounts update
package TeamCountsTriggers
{

function DefaultGame::clientJoinTeam( %game, %client, %team, %respawn )
{
	Parent::clientJoinTeam( %game, %client, %team, %respawn );

	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs, %respawned)
{
   Parent::clientChangeTeam(%game, %client, %team, %fromObs, %respawned);
   
   //Trigger GetCounts
   ResetClientChangedTeams();
}

function DefaultGame::assignClientTeam(%game, %client, %respawn )
{
	Parent::assignClientTeam(%game, %client, %respawn );
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::onClientEnterObserverMode( %game, %client )
{
	Parent::onClientEnterObserverMode( %game, %client );
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::AIChangeTeam(%game, %client, %newTeam)
{
	Parent::AIChangeTeam(%game, %client, %newTeam);

	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::onClientLeaveGame(%game, %client)
{
	Parent::onClientLeaveGame(%game, %client);
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::forceObserver(%game, %client, %reason)
{
	Parent::forceObserver(%game, %client, %reason);
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function GameConnection::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch)
{
	Parent::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch);
   
	//Reset GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset GetCounts
	ResetClientChangedTeams();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TeamCountsTriggers))
    activatePackage(TeamCountsTriggers);
