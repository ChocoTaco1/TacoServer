//Enable or Disable
//$Host::EnableTeamBalanceNotify = 1;
//
//Give the client a notification on the current state of balancing.
//This function is in GetTeamCounts.cs
function TeamBalanceNotify( %game, %client, %respawn )
{	
	if( $CurrentMissionType !$= "LakRabbit" && $TotalTeamPlayerCount !$= 0 && $Host::EnableTeamBalanceNotify )
	{
		//variables
		%Team1Difference = $PlayerCount[1] - $PlayerCount[2];
		%Team2Difference = $PlayerCount[2] - $PlayerCount[1];
		//Make Global
		$Team1Difference = %Team1Difference;
		$Team2Difference = %Team2Difference;

	
		//echo ("%Team1Difference " @ %Team1Difference);
		//echo ("%Team2Difference " @ %Team2Difference);

		if( $PlayerCount[1] !$= $PlayerCount[2] )
		{
			//Uneven. Reset Balanced.
			$BalancedMsgPlayed = 0;
				
			if( %Team1Difference >= 2 || %Team2Difference >= 2 )
			{
				if( $UnbalancedMsgPlayed !$= 1 && %Team2Difference == 2 || %Team1Difference == 2 ) 
				{
					messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced.');
					//Once per cycle.
					$UnbalancedMsgPlayed = 1;
					//Reset Stats.
					$StatsMsgPlayed = 0;					
				}
				//Stats Aspect. 3 or more difference gets a stats notify. 				
				else if( $StatsMsgPlayed !$= 1)
				{
					messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0It is currently %1 vs %2 with %3 observers.', $PlayerCount[1], $PlayerCount[2], $PlayerCount[0] );
					//Run once.
					$StatsMsgPlayed = 1;
				}
				
				if( $StatsBalancedSoundPlayed !$= 1 )
				{
					//Called in 30 secs with sound
					schedule(30000, 0, "StatsUnbalanceSound");
					//Once per cycle.
					$StatsBalancedSoundPlayed = 1;
				}
			}
		}
		//If teams are balanced and teams dont equal 0.		
		else if( $PlayerCount[1] == $PlayerCount[2] && $TotalTeamPlayerCount !$= 0 && $BalancedMsgPlayed !$= 1 )
		{
				//messageAll('MsgTeamBalanceNotify', '\c1Teams are balanced.');
				//Once per cycle.
				$BalancedMsgPlayed = 1;
				//Reset unbalanced.				
				$UnbalancedMsgPlayed = 0;
				//Reset Stats.
				$StatsMsgPlayed = 0;
				//Reset Stats with sound.
				$StatsBalancedSoundPlayed = 0;
		}
	}
}

//Reset Notify at defaultgame::gameOver in evo defaultgame.ovl
function ResetTeamBalanceNotifyGameOver( %game ) 
{
	//Reset All TeamBalance Variables
	$BalancedMsgPlayed = -1;
	$UnbalancedMsgPlayed = -1;
	$StatsMsgPlayed = -1;
	$StatsBalancedSoundPlayed = -1;
}

//Stats with Sound
//Called every 30 seconds
//2 or more difference
function StatsUnbalanceSound()
{
	if( $Team1Difference >= 2 || $Team2Difference >= 2 )
		{				
			//Added so the notification wont sound between the 5 sec interval of get counts and the 30 sec unbalanced notification when someone switches at the last minute before a check.
			if( !$GetCountsClientTeamChange )
			{
				messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0It is currently %1 vs %2 with %3 observers.~wfx/misc/bounty_objrem2.wav', $PlayerCount[1], $PlayerCount[2], $PlayerCount[0] );
				//Called in 30 secs with sound
				schedule(30000, 0, "StatsUnbalanceSound");
			}
			else
				//In the event that a player switches up instead of down during an interval the function can be called again.
				schedule(5000, 0, "StatsUnbalanceSound");
		}
	else
	{
		$StatsBalancedSoundPlayed = 0;
		return;
	}
}
