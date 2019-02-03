//Enable or Disable
//$Host::EnableTeamBalanceNotify = 1;
//
//Give the client a notification on the current state of balancing.
//This function is in GetTeamCounts.cs
function TeamBalanceNotify( %game, %client, %respawn )
{	
	if( $CurrentMissionType !$= "LakRabbit" && $TotalTeamPlayerCount !$= 0 && $Host::EnableTeamBalanceNotify && !$Host::TournamentMode )
	{	
		//echo ("%Team1Difference " @ %Team1Difference);
		//echo ("%Team2Difference " @ %Team2Difference);

		if( $PlayerCount[1] !$= $PlayerCount[2] )
		{
			//Uneven. Reset Balanced.
			$BalancedMsgPlayed = 0;
				
			if( $Team1Difference >= 2 || $Team2Difference >= 2 )
			{				
				if( $StatsMsgPlayed !$= 1)
				{
					//Run once.
					$StatsMsgPlayed = 1;
					//Start Sound Schedule for 60 secs
					schedule(15000, 0, "StatsUnbalanceSound", %game, %client, %respawn );
				}
			}
		}
		//If teams are balanced and teams dont equal 0.		
		else if( $PlayerCount[1] == $PlayerCount[2] && $TotalTeamPlayerCount !$= 0 && $BalancedMsgPlayed !$= 1 )
		{
				//messageAll('MsgTeamBalanceNotify', '\c1Teams are balanced.');
				//Once per cycle.
				$BalancedMsgPlayed = 1;
				//Reset Stats.
				$StatsMsgPlayed = 0;
		}
	}
}

//Reset Notify at defaultgame::gameOver in evo defaultgame.ovl
function ResetTeamBalanceNotifyGameOver() 
{
	//Reset All TeamBalance Variables
	$BalancedMsgPlayed = -1;
	$StatsMsgPlayed = -1;
}

//Check to see if teams are still unbalanced
//Fire AutoBalance in 30 sec
function StatsUnbalanceSound( %game, %client, %respawn )
{
	if( $CurrentMissionType !$= "LakRabbit" && $Host::EnableTeamBalanceNotify && $StatsMsgPlayed $= 1 && !$Host::TournamentMode )
	{				
		if( $Team1Difference == 1 || $Team2Difference == 1 || $PlayerCount[1] == $PlayerCount[2] )
		{
			$StatsMsgPlayed = 0;
			return;
		}
		else if( $Team1Difference >= 2 || $Team2Difference >= 2 )
		{
			messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0Autobalance Initializing.~wgui/vote_nopass.wav');
			schedule(30000, 0, "Autobalance", %game, %client, %respawn);
		}
	}
}