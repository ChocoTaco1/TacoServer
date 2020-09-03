// Team Autobalance Script
//
// Determines which team needs players and proceeds to switch them
// Goon style: At respawn
//
// Enable or Disable Autobalance
// $Host::EnableAutobalance = 1;
//
// exec("scripts/autoexec/Autobalance.cs");

// Run from TeamBalanceNotify.cs via NotifyUnbalanced
function Autobalance( %game )
{	
	if(isEventPending($AutoBalanceSchedule)) 
		cancel($AutoBalanceSchedule);
	
	if( $TBNStatus !$= "NOTIFY" ) //If Status has changed to EVEN or anything else (GameOver reset).
		return;
	
	//Difference Variables
	%team1difference = $TeamRank[1, count] - $TeamRank[2, count];
	%team2difference = $TeamRank[2, count] - $TeamRank[1, count];
	
	//Determine BigTeam
	if( %team1difference >= 2 )
		$BigTeam = 1;
	else if( %team2difference >= 2 )
		$BigTeam = 2;
	else
		return;
	
	%otherteam = $BigTeam == 1 ? 2 : 1;
	if($TeamRank[$BigTeam, count] - $TeamRank[%otherteam, count] >= 3)
		%s = "s";
	
	//Warning message
	messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0Autobalance will switch the next respawning player%2 on Team %1.', $TeamName[$BigTeam], %s);
}

package Autobalance
{

// called from player scripts
function DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
   parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);

   if($BigTeam !$= "" && %clVictim.team == $BigTeam)
   {
		%otherteam = $BigTeam == 1 ? 2 : 1;
		if($TeamRank[$BigTeam, count] - $TeamRank[%otherteam, count] >= 2)
		{	
			//If someone switches to observer or disconnects
			if(%damageType !$= 0)
			{
				echo(%clVictim.nameBase @ " has been moved to Team " @ %otherTeam @ " for balancing.");
				messageClient(%clVictim, 'MsgTeamBalanceNotify', '\c0You were switched to Team %1 for balancing.~wfx/powered/vehicle_screen_on.wav', $TeamName[%otherteam]);
				messageAllExcept(%clVictim, -1, 'MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
				
				Game.clientChangeTeam( %clVictim, %otherTeam, 0 );
			}
		}
		else
		{
			$BigTeam = "";
			ResetTBNStatus();
		}
   }
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset Autobalance
	$BigTeam = "";
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(Autobalance))
	activatePackage(Autobalance);