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


// This function is called in GetTeamCounts.cs
function TeamBalanceNotify( %game, %team1difference, %team2difference )
{	
	if( $CurrentMissionType !$= "LakRabbit" && $TotalTeamPlayerCount !$= 0 && !$Host::TournamentMode )
	{	
		//echo ("%Team1Difference " @ %Team1Difference);
		//echo ("%Team2Difference " @ %Team2Difference);

		//Uneven
		if( $PlayerCount[1] !$= $PlayerCount[2] )
		{
			//Reset Balanced.
			$BalancedMsgPlayed = 0;
				
			if( %team1difference >= 2 || %team2difference >= 2 )
			{				
				if( $UnbalancedMsgPlayed !$= 1)
				{
					//Run once.
					$UnbalancedMsgPlayed = 1;
					//Start Sound Schedule
					schedule(15000, 0, "UnbalancedSound", %game );
				}
			}
		}
		//If teams are balanced	
		else if( $PlayerCount[1] == $PlayerCount[2] && $TotalTeamPlayerCount !$= 0 && $BalancedMsgPlayed !$= 1 )
		{
			//messageAll('MsgTeamBalanceNotify', '\c1Teams are balanced.');
			//Once per cycle.
			$BalancedMsgPlayed = 1;
			//Reset Unbalanced.
			$UnbalancedMsgPlayed = 0;
		}
	}
}

//Check to see if teams are still unbalanced
//Fire AutoBalance in 30 sec if enabled
function UnbalancedSound( %game )
{
	if( $UnbalancedMsgPlayed $= 1 )
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
			
		//Difference Variables
		%team1difference = $PlayerCount[1] - $PlayerCount[2];
		%team2difference = $PlayerCount[2] - $PlayerCount[1];
		
		if( %team1difference == 1 || %team2difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
		{
			//Reset
			$UnbalancedMsgPlayed = 0;
			return;
		}
		//Continue
		else if( %team1difference >= 2 || %team2difference >= 2 )
		{
			//Autobalance Warning
			if( $Host::EnableAutobalance )
			{
				messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0Autobalance Initializing.~wgui/vote_nopass.wav');
				schedule(30000, 0, "Autobalance", %game );
			}
			//If Autobalance is disabled, message only.
			else if( $Host::EnableTeamBalanceNotify )
			{		
				messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0%1 vs %2 with %3 observers.~wgui/vote_nopass.wav', $PlayerCount[1], $PlayerCount[2], $PlayerCount[0] );
				schedule(13000, 0, "ResetTeamBalanceNotifyGameOver");
				schedule(15000, 0, "ResetClientChangedTeams");
			}
		}
	}
}


//Reset Notify at defaultgame::gameOver in evo defaultgame.ovl
function ResetTeamBalanceNotifyGameOver() 
{
	//Reset All TeamBalance Variables
	$BalancedMsgPlayed = -1;
	$UnbalancedMsgPlayed = -1;
}