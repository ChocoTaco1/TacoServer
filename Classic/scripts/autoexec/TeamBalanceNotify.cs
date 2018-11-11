//Enable or Disable
//$Host::EnableTeamBalanceNotify = 1;
//
//Give the client a notification on the current state of balancing.
//This function is in GetTeamCounts.cs
function TeamBalanceNotify::AtSpawn( %game, %client, %respawn )
{	
	if( $CurrentMissionType !$= "LakRabbit" && $TotalTeamPlayerCount !$= 0 && $Host::EnableTeamBalanceNotify )
	{
		//variables

		%Team1Difference = $PlayerCount[1] - $PlayerCount[2];
		%Team2Difference = $PlayerCount[2] - $PlayerCount[1];

	
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
					//Once per cycle
					$UnbalancedMsgPlayed = 1;
					//Reset Stats.
					$StatsMsgPlayed = 0;
				}
				//Stats Aspect. 3 or more difference gets a stats notify. 				
				else if( $StatsMsgPlayed !$= 1 )
				{
				messageAll('MsgTeamBalanceNotify', '\c1It is currently %1 vs %2 with %3 observers.', $PlayerCount[1], $PlayerCount[2], $PlayerCount[0] );
				$StatsMsgPlayed = 1;
				}
			}
		}
		//If teams are balanced and teams dont equal 0.		
		else if( $PlayerCount[1] == $PlayerCount[2] && $TotalTeamPlayerCount !$= 0 && $BalancedMsgPlayed !$= 1 )
		{
				messageAll('MsgTeamBalanceNotify', '\c1Teams are balanced.');
				//Once per cycle.
				$BalancedMsgPlayed = 1;
				//Reset unbalanced.				
				$UnbalancedMsgPlayed = 0;
				//Reset Stats.
				$StatsMsgPlayed = 0;
		}
	}
}

//Called in CTFGame::flagCap in evo CTFGame.ovl
//Allows for another unbalanced notification everytime the flag is capped.
function ResetUnbalancedNotifyPerCap()
{
	$UnbalancedMsgPlayed = 0;
	$StatsMsgPlayed = 0;
}

//Reset Notify at defaultgame::gameOver in evo defaultgame.ovl
function ResetTeamBalanceNotifyGameOver( %game ) 
{
	//Reset All TeamBalance Variables
	$BalancedMsgPlayed = -1;
	$UnbalancedMsgPlayed = -1;
	$StatsMsgPlayed = -1;
	
}
