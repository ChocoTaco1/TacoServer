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
// If it takes too long for specific canidates to die. After a time choose anyone.
$Autobalance::Fallback = 120000; //60000 is 1 minute

// Set reset string
$GetCountsStatus = "UPDATE";

package StartTeamCounts
{

function CreateServer( %mission, %missionType )
{
	parent::CreateServer( %mission, %missionType );
	//Call for a GetTeamCount update
	GetTeamCounts(%game);

	//Prevent package from being activated if it is already
	if (!isActivePackage(TeamCountsTriggers))
		activatePackage(TeamCountsTriggers);
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

				//Observers
				$Observers = $HostGamePlayerCount - ($TeamRank[1, count] + $TeamRank[2, count]);

				//echo("$PlayerCount[0] " @  $HostGamePlayerCount - ($TeamRank[1, count] + $TeamRank[2, count]));
				//echo("$PlayerCount[1] " @  $TeamRank[1, count]);
				//echo("$PlayerCount[2] " @  $TeamRank[2, count]);

				if( !$Host::TournamentMode )
				{
					if($CurrentMissionType $= "CTF")
					{
						NBRStatusNotify(%game);
						CheckAntiPack(%game);
					}
					else if($CurrentMissionType $= "DM")
						CheckAntiPack(%game);

					TeamBalanceNotify(%game); //Has check for # teams
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


// Team Balance Notify Script
//
// Give the client a notification on the current state of balancing
// Furthermore if Autobalance is enabled. Proceed to Autobalancing
// Autobalance does not need TeamBalanceNotify to be enabled to run
//
// Enable or Disable
// $Host::EnableTeamBalanceNotify = 1;
// Enable or Disable Autobalance
// $Host::EnableAutobalance = 1;
//


// Called in GetTeamCounts
function TeamBalanceNotify(%game)
{
	if(!$Host::EnableTeamBalanceNotify && !$Host::EnableAutobalance)
		return;

	if( Game.numTeams > 1 && $TotalTeamPlayerCount !$= 0 )
	{
		//Uneven
		if($TeamRank[1, count] !$= $TeamRank[2, count])
		{
			%team1difference = $TeamRank[1, count] - $TeamRank[2, count];
			%team2difference = $TeamRank[2, count] - $TeamRank[1, count];

			//echo("%Team1Difference " @ %team1difference);
			//echo("%Team2Difference " @ %team2difference);

			if( %team1difference >= 2 || %team2difference >= 2 ) //Teams are unbalanced
			{
				if( $TBNStatus !$= "NOTIFY" ) //Stops any new schedules
					$TBNStatus = "UNBALANCED";
			}
			else
				//Man down. 6vs7, 4vs3 etc
				$TBNStatus = "UNEVEN";
		}
		//Teams are even
		else if($TeamRank[1, count] == $TeamRank[2, count] && $TBNStatus !$= "PLAYEDEVEN" )
			$TBNStatus = "EVEN";

		switch$($TBNStatus)
		{
			case IDLE:
				//Do Nothing
			case UNEVEN:
				//Do Nothing
			case UNBALANCED:
				//Start Schedule to Notify
				$NotifySchedule = schedule(15000, 0, "NotifyUnbalanced", %game );
				$TBNStatus = "NOTIFY";
			case EVEN:
				//messageAll('MsgTeamBalanceNotify', '\c1Teams are balanced.');
				$TBNStatus = "PLAYEDEVEN";
			case PLAYEDEVEN:
				//Do Nothing
			case NOTIFY:
				//Do Nothing
		}
	}
	//echo($TBNStatus);
}

//Check to see if teams are still unbalanced
//Fire AutoBalance in 30 sec if enabled
function NotifyUnbalanced( %game )
{
	if(isEventPending($NotifySchedule))
		cancel($NotifySchedule);

	if(!$Host::EnableTeamBalanceNotify && !$Host::EnableAutobalance)
		return;

	if( $TBNStatus !$= "NOTIFY" ) //If Status has changed to EVEN or anything else (GameOver reset).
		return;

	//Difference Variables
	%team1difference = $TeamRank[1, count] - $TeamRank[2, count];
	%team2difference = $TeamRank[2, count] - $TeamRank[1, count];

	if( %team1difference >= 2 || %team2difference >= 2 )
	{
		//Autobalance Warning
		if( $Host::EnableAutobalance )
		{
			messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0Autobalance Initializing.~wgui/vote_nopass.wav');
			$AutoBalanceSchedule = schedule(30000, 0, "Autobalance", %game );
		}
		//If Autobalance is disabled, message only.
		else if( $Host::EnableTeamBalanceNotify )
		{
			%observers = $HostGamePlayerCount - ($TeamRank[1, count] + $TeamRank[2, count]);
			messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0%1 vs %2 with %3 observers.~wgui/vote_nopass.wav', $TeamRank[1, count], $TeamRank[2, count], %observers );
			schedule(13000, 0, "ResetTBNStatus");
			schedule(15000, 0, "ResetGetCountsStatus");
		}
	}
	else
		ResetTBNStatus();
}

// Reset TBNStatus
function ResetTBNStatus()
{
	$TBNStatus = "IDLE";
}

// Reset every map change
package ResetTBNGameOver
{

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);

	//Reset TBNStatus
	ResetTBNStatus();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ResetTBNGameOver))
    activatePackage(ResetTBNGameOver);


// No Base Rape Notify Script
//
// Notifys clients if NoBase rape is on or off.
//
// Enable or Disable
// $Host::EnableNoBaseRapeNotify = 1;
//

// Called in GetTeamCounts.cs
function NBRStatusNotify( %game )
{
	if( $Host::EnableNoBaseRapeNotify && $Host::NoBaseRapeEnabled )
	{
		//On
		if( $Host::NoBaseRapePlayerCount > $TotalTeamPlayerCount )
		{
			if( $NBRStatus !$= "PLAYEDON" )
				$NBRStatus = "ON";
		}
		//Off
		else
		{
			if( $NBRStatus !$= "PLAYEDOFF" )
				$NBRStatus = "OFF";
		}

		switch$($NBRStatus)
		{
			case ON:
				messageAll('MsgNoBaseRapeNotify', '\c1No Base Rape: \c0Enabled.');
				$NBRStatus = "PLAYEDON";
			case OFF:
				messageAll('MsgNoBaseRapeNotify', '\c1No Base Rape: \c0Disabled.~wfx/misc/diagnostic_on.wav');
				$NBRStatus = "PLAYEDOFF";
			case PLAYEDON:
				//Do Nothing
			case PLAYEDOFF:
				//Do Nothing
		}
	}
}

// Reset gameover
package ResetNBRNotify
{

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);

	//Reset NoBaseRapeNotify
	$NBRStatus = "IDLE";
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ResetNBRNotify))
    activatePackage(ResetNBRNotify);



// Team Autobalance Script
//
// Determines which team needs players and proceeds to switch them
// Goon style: At respawn
//
// Enable or Disable Autobalance
// $Host::EnableAutobalance = 1;
//
// exec("scripts/autoexec/Autobalance.cs");
//

// Run from TeamBalanceNotify via NotifyUnbalanced
function Autobalance( %game )
{
	if(isEventPending($AutoBalanceSchedule))
		cancel($AutoBalanceSchedule);

	if(!$Host::EnableAutobalance)
		return;

	if($TBNStatus !$= "NOTIFY") //If Status has changed to EVEN or anything else (GameOver reset).
		return;

	//Difference Variables
	%team1difference = $TeamRank[1, count] - $TeamRank[2, count];
	%team2difference = $TeamRank[2, count] - $TeamRank[1, count];

	//Determine BigTeam
	if( %team1difference >= 2 )
		$BigTeam = 1;
	else if( %team2difference >= 2 )
		$BigTeam = 2;
	else
		return;

	$Autobalance::UseAllMode = 0;
	$Autobalance::FallbackTime = getSimTime();
	%otherTeam = $BigTeam == 1 ? 2 : 1;
	$Autobalance::AMThreshold = mCeil(MissionGroup.CTF_scoreLimit/3) * 100;

	//If BigTeam score is greater than otherteam score + threshold
	if($TeamScore[$BigTeam] > ($TeamScore[%otherTeam] + $Autobalance::AMThreshold) || $TeamRank[%otherTeam, count] $= 0)
		$Autobalance::UseAllMode = 1;
	//If BigTeam Top Players score is greater than otherTeam Top Players score + threshold
	else if($TeamRank[$BigTeam, count] >= 5 && $TeamRank[%otherTeam, count] >= 3)
	{
		%max = mfloor($TeamRank[$BigTeam, count]/2);
		if(%max > $TeamRank[%otherTeam, count])
			%max = $TeamRank[%otherTeam, count];
		%threshold = %max * 100;
		for(%i = 0; %i < %max; %i++)
		{
			%bigTeamTop = %bigTeamTop + $TeamRank[$BigTeam, %i].score;
			%otherTeamTop = %otherTeamTop + $TeamRank[%otherTeam, %i].score;
		}

		if(%bigTeamTop > (%otherTeamTop + %threshold))
			$Autobalance::UseAllMode = 1;
	}
	//echo("Allmode " @  $Autobalance::UseAllMode);

	//Select lower half of team rank as canidates for team change
	if(!$Autobalance::UseAllMode)
	{
		//Reset clients canidate var
		ResetABClients();

		$Autobalance::Max = mFloor($TeamRank[$BigTeam, count]/2);
		for(%i = $Autobalance::Max; %i < $TeamRank[$BigTeam, count]; %i++)
		{
			//echo("[Autobalance]: Selected" SPC $TeamRank[$BigTeam, %i].nameBase @ ", " @ %i);
			$TeamRank[$BigTeam, %i].abCanidate = true;
		}
		%a = " selected";
	}

	if($TeamRank[$BigTeam, count] - $TeamRank[%otherTeam, count] >= 3)
		%s = "s";

	//Warning message
	messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0Autobalance will switch the next%3 respawning player%2 on Team %1.', $TeamName[$BigTeam], %s, %a);
}

function ResetABClients()
{
	for(%i = 0; %i < $TeamRank[$BigTeam, count]; %i++)
	{
		$TeamRank[$BigTeam, %i].abCanidate = false;
	}
}

package Autobalance
{

function DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
	parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);

	if($BigTeam !$= "" && %clVictim.team == $BigTeam)
	{
		%otherTeam = $BigTeam == 1 ? 2 : 1;
		if($TeamRank[$BigTeam, count] - $TeamRank[%otherTeam, count] >= 2)
		{
			%fallback = 0;
			if((getSimTime() - $Autobalance::FallbackTime) > $Autobalance::Fallback)
				%fallback = 1;

			//damageType 0: If someone switches to observer or disconnects
			if(%damageType !$= 0 && (%clVictim.abCanidate || $Autobalance::UseAllMode || %fallback))
			{
				echo("[Autobalance]" SPC %clVictim.nameBase @ " has been moved to Team " @ %otherTeam @ " for balancing. [AM:" @ $Autobalance::UseAllMode SPC "#BT:" @ ($TeamRank[$BigTeam, count]-1) SPC "#OT:" @ ($TeamRank[%otherTeam, count]+1) SPC "FB:" @ %fallback @ "]");
				messageClient(%clVictim, 'MsgTeamBalanceNotify', '\c0You were switched to Team %1 for balancing.~wfx/powered/vehicle_screen_on.wav', $TeamName[%otherTeam]);
				messageAllExcept(%clVictim, -1, 'MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');

				Game.clientChangeTeam( %clVictim, %otherTeam, 0 );
			}
		}
		else
		{
			ResetABClients();
			ResetTBNStatus();
			$BigTeam = "";
		}
	}
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);

	//Reset Autobalance
	$BigTeam = "";

	//Reset all clients canidate var
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%client.abCanidate = false;
	}
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(Autobalance))
	activatePackage(Autobalance);
