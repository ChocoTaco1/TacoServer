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


// Called in GetTeamCounts.cs
function TeamBalanceNotify(%game)
{	
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