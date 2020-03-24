// Team Autobalance Script
//
// Determines which team needs players and proceeds to find candidates
// Candidates are based on low scores then switches the candidate
//
// Enable or Disable Autobalance
// $Host::EnableAutobalance = 1;

// How far behind littleTeam must be to use All Mode.
// Meaning picking from a pool of all players on the bigTeam instead of just the lowest scoring player.
// 400 equals 400 points. 4 caps behind.
$AllModeThreshold = 400;


// Run from TeamBalanceNotify.cs via UnbalancedSound( %game )
function Autobalance( %game, %AutobalanceSafetynetTrys )
{	
	if(isEventPending($AutoBalanceSchedule)) 
		cancel($AutoBalanceSchedule);
	
	if( $TBNStatus !$= "NOTIFY" ) //If Status has changed to EVEN or anything else (GameOver reset).
		return;
	
	//Debug: Uncomment to enable
	//%AutobalanceDebug = true;
	
	//Reset
	%lastclient1 = "";
	%lastclient2 = "";
	
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
	
	//Difference Variables
	%team1difference = $PlayerCount[1] - $PlayerCount[2];
	%team2difference = $PlayerCount[2] - $PlayerCount[1];
	
	//If even, stop.
	if( %team1difference == 1 || %team2difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
	{
		//Reset TBN
		ResetTBNStatus();
		return;
	}
	//Determine bigTeam
	else if( %team1difference >= 2 )
		%bigTeam = 1;
	else if( %team2difference >= 2 )
		%bigTeam = 2;

	%littleTeam = ( %bigTeam == 1 ) ? 2 : 1;
	
	//Toggle for All Mode
	//If a team is behind pick anyone, not just a low scoring player
	if( $TeamScore[%bigTeam] > ($TeamScore[%littleTeam] + $AllModeThreshold) )
	{
		%UseAllMode = 1;
		%autobalanceRandom = getRandom(1,($PlayerCount[%bigTeam] - 1));
	}
	
    //Pick a client for autobalance
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%team = %client.team;
		
		if(%UseAllMode)
		{
			//Try to pick any player
			if(%autobalanceRandom == %autobalanceLoop || %lastclient[%team] $= "")
			{
				//If random player has the flag pick the next person
				if(!%client.player.holdingFlag)
					%teamcanidate[%team] = %client;
				else
					%autobalanceRandom = %autobalanceRandom + 1;
			}
			
			%autobalanceLoop++;
		}
		else
		{
			//Normal circumstances
			//Try to pick a low scoring player
			if(%client.score < %lastclient[%team].score || %lastclient[%team] $= "")
			{
				if(!%client.player.holdingFlag)
					%teamcanidate[%team] = %client;
			}
		}

		%lastclient[%team] = %client;
	}

	//Debug
	if( %AutobalanceDebug )
		AutobalanceDebug(%teamcanidate1, %teamcanidate2, %team1difference, %team2difference, %bigTeam, %AutobalanceSafetynetTrys, %UseAllMode);
				
	%client = %teamcanidate[%bigTeam];
	%team = %teamcanidate[%bigTeam].team;
	%otherTeam = ( %team == 1 ) ? 2 : 1;
			
	// Fire Autobalance
	Game.clientChangeTeam( %client, %otherTeam, 0 );
	messageClient(%client, 'MsgTeamBalanceNotify', "\c0You were switched to the other team for balancing.~wfx/powered/vehicle_screen_on.wav");
	messageAllExcept(%client, -1, 'MsgTeamBalanceNotify', "~wfx/powered/vehicle_screen_on.wav");
			
	//Trigger GetCounts
	ResetGetCountsStatus();
	//Reset TBN
	ResetTBNStatus();
}

function AutobalanceDebug(%teamcanidate1, %teamcanidate2, %team1difference, %team2difference, %bigTeam, %AutobalanceSafetynetTrys, %UseAllMode)
{
	if( %teamcanidate[%bigTeam] $= "" )
	{
		%AutobalanceSafetynetTrys++; 
		if(%AutobalanceSafetynetTrys $= 3) 
			return;
			
		if( %teamcanidate1 $= "" && %teamcanidate2 $= "" )
			%error = "Both Teams";
		else if( %teamcanidate[%bigTeam] $= "" )
			%error = "Team " @ %bigTeam;
				
		if( %error !$= "" )
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: %1', %error );
				
		//Trigger GetCounts
		ResetGetCountsStatus();
		//Rerun in 10 secs
		schedule(10000, 0, "Autobalance", %game, %AutobalanceSafetynetTrys );
	}
	
	if(%UseAllMode)
		%mode = "All Mode";
	else
		%mode = "Low Mode";
		
	if( %teamcanidate1 $= "" ) 
		%teamcanidate1 = "NULL"; 
	if( %teamcanidate2 $= "" ) 
		%teamcanidate2 = "NULL";
		
	messageAll('MsgTeamBalanceNotify', '\c0Autobalance stat: %1, %2, %3, %4, %5', %teamcanidate1, %team1difference, %teamcanidate2, %team2difference, %mode );
	
	return;
}