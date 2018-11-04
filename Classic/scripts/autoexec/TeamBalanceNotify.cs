//Enable or Disable
//$Host::EnableTeamBalanceNotify = 1;
//
//Give the client a notification on the current state of balancing.
//This function is in GetTeamCounts( %game, %client, %respawn ) GetTeamCounts.cs
function TeamBalanceNotify::AtSpawn( %game, %client, %respawn )
{	
	if( $CurrentMissionType !$= "LakRabbit" && $TotalTeamPlayerCount !$= 0 && $Host::EnableTeamBalanceNotify )
	{
	//Call for a GetTeamCount update
	//GetTeamCounts( %game, %client, %respawn );
	
	//evoplayercount does not count yourself
	
	//variables
	//%balancedifference = 2; //player difference you want to allow before sending notifications between teams.
	//%Team1Difference = $PlayerCount[1] - $PlayerCount[2];
	//%Team2Difference = $PlayerCount[2] - $PlayerCount[1];

	
	//echo ("%Team1Difference " @ %Team1Difference);
	//echo ("%Team2Difference " @ %Team2Difference);

	
		//Are teams unbalanced?
		if( $PlayerCount[1] !$= $PlayerCount[2] ) {
				//Reset Balanced
				$BalancedCount = 0;
			if( ($PlayerCount[1] - $PlayerCount[2]) >= 2 || ($PlayerCount[2] - $PlayerCount[1]) >= 2 ) {
				//Has the client gotten the notification already
				if($TeamBalanceNotifyCount !$= 1) {
					//If unbalanced, send a notification. Will continue to send notifications until teams are balanced.
					messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced.');
				//Only get the notification once per spawn.
				$TeamBalanceNotifyCount = 1;
				//Reset Stat
				$StatsBalanceCount = 0;
				}	
			}
		}
		else 
			//If teams are balanced and teams dont equal 0.
			if( $PlayerCount[1] == $PlayerCount[2] && $TotalTeamPlayerCount !$= 0 ) {
					//Has the client gotten the notification already
					if($BalancedCount !$= 1) {
						//If balanced, send a notification.
						messageAll('MsgTeamBalanceNotify', '\c1Teams are balanced.');
						//Only get the balance notification once.
						$BalancedCount = 1;
						//Reset Unbalanced
						$TeamBalanceNotifyCount = 0;
						//Reset Stat
						$StatsBalanceCount = 0;
			}
		}
		
		//3 or more difference gets a count notify
		if( ($PlayerCount[1] - $PlayerCount[2]) >= 3 || ($PlayerCount[2] - $PlayerCount[1]) >= 3 ) {
			//Run it once
			if($StatsBalanceCount !$= 1) {
			messageAll('MsgTeamBalanceNotify', '\c1It is currently %1 vs %2 with %3 observers.', $PlayerCount[1], $PlayerCount[2], $PlayerCount[0] );
			$StatsBalanceCount = 1;
			}
		}
	}
}

//Called in CTFGame::flagCap in evo CTFGame.ovl
//Allows for another unbalanced notification everytime the flag is capped.
function ResetUnbalancedNotifyPerCap()
{
	$TeamBalanceNotifyCount = 0;
	$StatsBalanceCount = 0;
}

//Reset Notify at defaultgame::gameOver in evo defaultgame.ovl
function ResetTeamBalanceNotifyGameOver( %game ) {
	//Reset TeamBalance Variables
	$BalancedCount = -1;
	$TeamBalanceNotifyCount = -1;
	$StatsBalanceCount = -1;
	
}
