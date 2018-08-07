//Changes were also made in the Evo Admin.ovl and DefaultGame.ovl
//DefaultGame::voteChangeMission, DefaultGame::voteChangeTimeLimit, serverCmdStartNewVote

package VoteOverTime {

//CTF
function CTFGame::timeLimitReached(%game)
{
   if( !$VoteInProgress && !$TimeLimitChanged ) {
		logEcho("game over (timelimit)");
		%game.gameOver();
		cycleMissions();
		
		$VoteInProgress = false;
		$TimeLimitChanged = false;
		$VoteInProgressMessege = false;
		$VoteSoundInProgress = false;
   }   
   else if( $missionRunning && $VoteInProgress && !$TimeLimitChanged ) {
		schedule(1000, 0, "CTFRestarttimeLimitReached", %game);
		
			if( !$VoteInProgressMessege ) {
			messageAll('', '\c2Vote Overtime Initiated.', %display);
			$VoteInProgressMessege = true;
			}
	}
}

function CTFRestarttimeLimitReached(%game)
{
	CTFGame::timeLimitReached(%game);
}

//LakRabbit
function LakRabbitGame::timeLimitReached(%game)
{
   if( !$VoteInProgress && !$TimeLimitChanged ) {
		logEcho("game over (timelimit)");
		%game.gameOver();
		cycleMissions();
		
		$VoteInProgress = false;
		$TimeLimitChanged = false;
		$VoteInProgressMessege = false;
		$VoteSoundInProgress = false;
   }   
   else if( $missionRunning && $VoteInProgress && !$TimeLimitChanged ) {
		schedule(1000, 0, "LakRabbitRestarttimeLimitReached", %game);
		
			if( !$VoteInProgressMessege ) {
			messageAll('', '\c2Vote Overtime Initiated.', %display);
			$VoteInProgressMessege = true;
			}
	}
}

function LakRabbitRestarttimeLimitReached(%game)
{
	LakRabbitGame::timeLimitReached(%game);
}

//SCtF
function SCtFGame::timeLimitReached(%game)
{
   if( !$VoteInProgress && !$TimeLimitChanged ) {
		logEcho("game over (timelimit)");
		%game.gameOver();
		cycleMissions();
		
		$VoteInProgress = false;
		$TimeLimitChanged = false;
		$VoteInProgressMessege = false;
		$VoteSoundInProgress = false;
   }   
   else if( $missionRunning && $VoteInProgress && !$TimeLimitChanged ) {
		schedule(1000, 0, "SCtFRestarttimeLimitReached", %game);
		
			if( !$VoteInProgressMessege ) {
			messageAll('', '\c2Vote Overtime Initiated.', %display);
			$VoteInProgressMessege = true;
			}
	}
}

function SCtFRestarttimeLimitReached(%game)
{
	SCtFGame::timeLimitReached(%game);
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(VoteOverTime))
    activatePackage(VoteOverTime);