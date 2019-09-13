// Team Autobalance Script
//
// Determines which team needs players and proceeds to find candidates
// Candidates are based on low scores then switches the candidate
//
// Enable or Disable Autobalance
// $Host::EnableAutobalance = 1;
//


// Run from TeamBalanceNotify.cs via UnbalancedSound( %game )
// Fire Autobalance
function Autobalance( %game, %AutobalanceSafetynetTrys )
{	
	//%AutobalanceDebug = true;
	
	//For autobalance
	%lastclient1 = "";
	%lastclient2 = "";
		
	//Team Count code by Keen
	$PlayerCount[0] = 0;
	$PlayerCount[1] = 0;
	$PlayerCount[2] = 0;
			

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
			
		//Pick a client for autobalance
		%team = %client.team;
		if(%client.score < %lastclient[%team].score || %lastclient[%team] $= "") 
			%teamcanidate[%team] = %client; 

		%lastclient[%team] = %client;
			
		//if(!%client.isAIControlled())
			$PlayerCount[%client.team]++;
	}
	
	//Difference Variables
	%team1difference = $PlayerCount[1] - $PlayerCount[2];
	%team2difference = $PlayerCount[2] - $PlayerCount[1];
	
	if( %team1difference == 1 || %team2difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
	{
		$StatsMsgPlayed = 0;
		return;
	}
	//Safetynet
	else if(( %teamcanidate1 $= "" && %team1difference >= 2 )||( %teamcanidate2 $= "" && %team2difference >= 2 ))
	{
		%AutobalanceSafetynetTrys++; if(%AutobalanceSafetynetTrys $= 3) return;

		if( %AutobalanceDebug )
		{
			if( %teamcanidate1 $= "" ) %teamcanidate1 = "NULL"; if( %teamcanidate2 $= "" ) %teamcanidate2 = "NULL";
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: %1, %2, %3, %4', %teamcanidate1, %teamcanidate2, %team1difference, %team2difference );
			if( %teamcanidate1 $= "NULL" ) %teamcanidate1 = ""; if( %teamcanidate2 $= "NULL" ) %teamcanidate2 = "";
		}
		else if( %teamcanidate1 $= "" && %teamcanidate2 $= "" ) messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Both Teams' );
		else if( %teamcanidate1 $= "" ) messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team1' );
		else if( %teamcanidate2 $= "" ) messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team2' );
			
		//Trigger GetCounts
		ResetClientChangedTeams();
		//Rerun in 10 secs
		schedule(10000, 0, "Autobalance", %game, %AutobalanceSafetynetTrys );
		return;
	}
	//Team 1
	else if( %team1difference >= 2 )
	{	
		if( %AutobalanceDebug )
		{
			if( %teamcanidate1 $= "" ) %teamcanidate1 = "NULL"; if( %teamcanidate2 $= "" ) %teamcanidate2 = "NULL";
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance stat: %1, %2, %3, %4', %teamcanidate1, %teamcanidate2, %team1difference, %team2difference );
			if( %teamcanidate1 $= "NULL" ) %teamcanidate1 = ""; if( %teamcanidate2 $= "NULL" ) %teamcanidate2 = "";
		}
			
		%client = %teamcanidate1;
		%team = %teamcanidate1.team;
		%otherTeam = ( %team == 1 ) ? 2 : 1;
			
		if( %teamcanidate1.team $= 1 )
		{
			Game.clientChangeTeam( %client, %otherTeam, 0 );
			messageAll('MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
		}
		else
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team1 mismatch.' );
			
		//Trigger GetCounts
		ResetClientChangedTeams();
		//Reset Unbalanced
		$UnbalancedMsgPlayed = 0;
		return;
	}
	//Team 2
	else if( %team2difference >= 2 )
	{
		if( %AutobalanceDebug )
		{
			if( %teamcanidate1 $= "" ) %teamcanidate1 = "NULL"; if( %teamcanidate2 $= "" ) %teamcanidate2 = "NULL";
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance stat: %1, %2, %3, %4', %teamcanidate1, %teamcanidate2, %team1difference, %team2difference );
			if( %teamcanidate1 $= "NULL" ) %teamcanidate1 = ""; if( %teamcanidate2 $= "NULL" ) %teamcanidate2 = "";
		}
			
		%client = %teamcanidate2;
		%team = %teamcanidate2.team;
		%otherTeam = ( %team == 1 ) ? 2 : 1;
			
		if( %teamcanidate2.team $= 2 )
		{
			Game.clientChangeTeam( %client, %otherTeam, 0 );
			messageAll('MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
		}
		else
			messageAll('MsgTeamBalanceNotify', '\c0Autobalance error: Team2 mismatch.' );
		
		//Trigger GetCounts
		ResetClientChangedTeams();
		//Reset Unbalanced
		$UnbalancedMsgPlayed = 0;
		return;
	}
}