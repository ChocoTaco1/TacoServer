// Team Autobalance Script
//
// Determines which team needs players and proceeds to switch them
// Goon style: At respawn
//
// Enable or Disable Autobalance
// $Host::EnableAutobalance = 1;
//
// exec("scripts/autoexec/Autobalance.cs");
//
// How far behind otherteam must be to use All Mode.
// Meaning picking from a pool of all players on the $BigTeam instead of just the lowest scoring players.
// 400 equals 400 points. 4 caps behind.
$Autobalance::AMThreshold = 300;
// Based on BigTeams Top3 scorers. If BigTeams Top3 scorers is greater than the otherTeams Top3 + Top3Threshold. Use AllMode. 
$Autobalance::Top3Threshold = 300;
// If it takes too long for specific canidates to die. After a time choose anyone.
$Autobalance::Fallback = 90000;

// Run from TeamBalanceNotify.cs via NotifyUnbalanced
function Autobalance( %game )
{	
	if(isEventPending($AutoBalanceSchedule)) 
		cancel($AutoBalanceSchedule);
	
	if($TBNStatus !$= "NOTIFY") //If Status has changed to EVEN or anything else (GameOver reset).
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

	$Autobalace::UseAllMode = 0;
	%otherTeam = $BigTeam == 1 ? 2 : 1;
	%bigTeamTop3 = $TeamRank[$BigTeam, 0].score + $TeamRank[$BigTeam, 1].score + $TeamRank[$BigTeam, 2].score;
	%otherTeamTop3 = $TeamRank[%otherTeam, 0].score + $TeamRank[%otherTeam, 1].score + $TeamRank[%otherTeam, 2].score;
	//Anyone who dies is eligable to switch
	//If BigTeam score is greater than otherteam score + threshold or BigTeam Top3 is greater than otherTeam Top3 + Top3Threshold
	if($TeamScore[$BigTeam] > ($TeamScore[%otherTeam] + $Autobalance::AMThreshold) || $TeamRank[%otherTeam, count] $= 0 || (%bigTeamTop3 > %otherTeamTop3 + $Autobalance::Top3Threshold))
		$Autobalace::UseAllMode = 1;
	//echo("Allmode " @  $Autobalace::UseAllMode);
	
	//Select lower half of team rank as canidates for team change
	if(!$Autobalace::UseAllMode)
	{
		$Autobalance::Max = mFloor($TeamRank[$BigTeam, count]/2);
		for(%i = $Autobalance::Max; %i < $TeamRank[$BigTeam, count]; %i++)
		{
			//echo(%i); echo($TeamRank[$BigTeam, %i].nameBase);
			$Autobalance::Canidate[%i] = $TeamRank[$BigTeam, %i];
		}
		%a = " selected";
	}
	
	if($TeamRank[$BigTeam, count] - $TeamRank[%otherTeam, count] >= 3)
		%s = "s";
	
	//Warning message
	messageAll('MsgTeamBalanceNotify', '\c1Teams are unbalanced: \c0Autobalance will switch the next%3 respawning player%2 on Team %1.', $TeamName[$BigTeam], %s, %a);
}

// Return true if client is a canidate
function CheckCanidate(%client)
{
    for(%i = $Autobalance::Max; %i < $TeamRank[$BigTeam, count]; %i++)
    {
        if(%client $= $Autobalance::Canidate[%i])
            return true;
    }
    return false;
}

package Autobalance
{

function DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation)
{
   parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);

   if($BigTeam !$= "" && %clVictim.team == $BigTeam)
   {
		%otherTeam = $BigTeam == 1 ? 2 : 1;
		if($TeamRank[$BigTeam, count] - $TeamRank[%otherTeam, count] >= 2)
		{	
			if($Autobalance::CanidateFallbackTime $= "")
				$Autobalance::CanidateFallbackTime = getSimTime();
			
			//damageType 0: If someone switches to observer or disconnects
			if(%damageType !$= 0 && (CheckCanidate(%clVictim) || $Autobalace::UseAllMode || (getSimTime() - $Autobalance::CanidateFallbackTime > $Autobalance::Fallback))) 
			{
				echo(%clVictim.nameBase @ " has been moved to Team " @ %otherTeam @ " for balancing.");
				messageClient(%clVictim, 'MsgTeamBalanceNotify', '\c0You were switched to Team %1 for balancing.~wfx/powered/vehicle_screen_on.wav', $TeamName[%otherTeam]);
				messageAllExcept(%clVictim, -1, 'MsgTeamBalanceNotify', '~wfx/powered/vehicle_screen_on.wav');
				
				Game.clientChangeTeam( %clVictim, %otherTeam, 0 );
			}
		}
		else
		{
			$BigTeam = "";
			ResetTBNStatus();
			deleteVariables("$Autobalace::Canidate*");
		}
   }
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset Autobalance
	$BigTeam = "";
	deleteVariables("$Autobalace::Canidate*");
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(Autobalance))
	activatePackage(Autobalance);