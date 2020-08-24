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
$AllModeThreshold = 300;


// Run from TeamBalanceNotify.cs via UnbalancedSound( %game )
function Autobalance( %game, %AutobalanceSafetynetTrys )
{	
	if(isEventPending($AutoBalanceSchedule)) 
		cancel($AutoBalanceSchedule);
	
	if( $TBNStatus !$= "NOTIFY" ) //If Status has changed to EVEN or anything else (GameOver reset).
		return;
	
	//Debug: Uncomment to enable
	//%AutobalanceDebug = true;
	
	//Difference Variables
	%team1difference = $TeamRank[1, count] - $TeamRank[2, count];
	%team2difference = $TeamRank[2, count] - $TeamRank[1, count];
	
	//Determine bigTeam
	if( %team1difference >= 2 )
		%bigTeam = 1;
	else if( %team2difference >= 2 )
		%bigTeam = 2;
	else
		return;

	%littleTeam = ( %bigTeam == 1 ) ? 2 : 1;
	
	//Toggle for All Mode
	//If a team is behind pick anyone, not just a low scoring player
	if( $TeamScore[%bigTeam] > ($TeamScore[%littleTeam] + $AllModeThreshold))
	{
		%UseAllMode = 1;
		//Find if anyone is holding a flag for exceptions for the loop
		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%client = ClientGroup.getObject(%i);
			%team = %client.team;
			
			if(%team $= %bigTeam)
			{
				//Holding flag?
				if(%client.player.holdingFlag !$= "")
					%exception = 1;
			}
		}
		%autobalanceRandom = getRandom(1,($TeamRank[%bigTeam, count] - %exception));
	}
	
    //Pick a client for autobalance
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%team = %client.team;
		
		if(%team $= %bigTeam)
		{
			//Holding flag?
			if(%client.player.holdingFlag !$= "")
				continue;
			
			if(%UseAllMode)
			{
				//Pick our random
				if(%autobalanceRandom == %AllmodeLoop || %canidate $= "")
					%canidate = %client;
				
				%AllmodeLoop++;
			}
			else
			{
				//Try to pick a low scoring player
				if(%client.score < %canidate.score || %canidate $= "")
					%canidate = %client;
			}
		}
	}

	//Debug
	if( %AutobalanceDebug )
		AutobalanceDebug(%canidate, %team1difference, %team2difference, %bigTeam, %AutobalanceSafetynetTrys, %UseAllMode);
				
	%client = %canidate;
	%team = %canidate.team;
	%otherTeam = ( %team == 1 ) ? 2 : 1;
			
	//Fire Autobalance
	Game.clientChangeTeam( %client, %otherTeam, 0 );
	messageClient(%client, 'MsgTeamBalanceNotify', "\c0You were switched to the other team for balancing.~wfx/powered/vehicle_screen_on.wav");
	messageAllExcept(%client, -1, 'MsgTeamBalanceNotify', "~wfx/powered/vehicle_screen_on.wav");
			
	//Trigger GetCounts
	ResetGetCountsStatus();
	//Reset TBN
	ResetTBNStatus();
}

function AutobalanceDebug(%canidate, %team1difference, %team2difference, %bigTeam, %AutobalanceSafetynetTrys, %UseAllMode)
{
	if( %teamcanidate[%bigTeam] $= "" )
	{
		%AutobalanceSafetynetTrys++; 
		if(%AutobalanceSafetynetTrys $= 3) 
			return;
			
		if( %canidate $= "" )
			%error = "Team " @ %bigTeam;
				
		if( %error !$= "" )
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: %1', %error );
				
		//Trigger GetCounts
		ResetGetCountsStatus();
		//Rerun in 10 secs
		schedule(10000, 0, "Autobalance", %game, %AutobalanceSafetynetTrys );
	}
	
	if(%UseAllMode)
		%mode = "All";
	else
		%mode = "Low";
		
	if( %teamcanidate1 $= "" ) 
		%teamcanidate1 = "NULL"; 
	if( %teamcanidate2 $= "" ) 
		%teamcanidate2 = "NULL";
		
	messageAll('MsgTeamBalanceNotify', '\c0Autobalance stat: %1, %2, %3, %4', %canidate, %team1difference, %team2difference, %mode );
	
	return;
}