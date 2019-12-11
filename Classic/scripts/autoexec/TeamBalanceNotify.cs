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
function TeamBalanceNotify( %game, %team1difference, %team2difference )
{	
	if( ($CurrentMissionType $= "CTF" || $CurrentMissionType $= "sctf") && $TotalTeamPlayerCount !$= 0 && !$Host::TournamentMode )
	{	
		//echo ("%Team1Difference " @ %Team1Difference);
		//echo ("%Team2Difference " @ %Team2Difference);

		//Uneven
		if( $PlayerCount[1] !$= $PlayerCount[2] )
		{	
			if( %team1difference >= 2 || %team2difference >= 2 ) //Teams are unbalanced
			{				
				if( $TBNStatus !$= "NOTIFY" ) //Stops any new schedules
					$TBNStatus = "UNBALANCED";
			}
			else
				//Means teams arnt even, but arnt so uneven to do anything about. Meaning a team is a man down. 6vs7, 4vs3 etc
				$TBNStatus = "UNEVEN";
		}
		//Teams are even
		else if( $PlayerCount[1] == $PlayerCount[2] && $TBNStatus !$= "PLAYEDEVEN" )
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
	
	if( $TBNStatus $= "NOTIFY" ) //If Status has changed to EVEN or anything else.
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
			ResetTBNStatus();
			return;
		}
		//Continue
		else if( %team1difference >= 2 || %team2difference >= 2 )
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
				messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0%1 vs %2 with %3 observers.~wgui/vote_nopass.wav', $PlayerCount[1], $PlayerCount[2], $PlayerCount[0] );
				schedule(13000, 0, "ResetTBNStatus");
				schedule(15000, 0, "ResetGetCountsStatus");
			}
		}
	}
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