//Changes were also made in the Evo Admin.ovl and DefaultGame.ovl
//DefaultGame::voteChangeMission, DefaultGame::voteChangeTimeLimit, serverCmdStartNewVote

package VoteOverTime
{

function DefaultGame::checkTimeLimit(%game, %forced)
{
   // Don't add extra checks:
   if ( %forced )
      cancel( %game.timeCheck );

   // if there is no time limit, check back in a minute to see if it's been set
   if(($Host::TimeLimit $= "") || $Host::TimeLimit == 0)
   {
      %game.timeCheck = %game.schedule(20000, "checkTimeLimit");
      return;
   }
   
   %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) + $missionStartTime - getSimTime();

   if (%curTimeLeftMS <= 0)
   {
        //Vote Overtime
		//Check if Vote is active or if the timelimit has changed.
		if( !$VoteInProgress && !$TimeLimitChanged )
		{
			// time's up, put down your pencils
			%game.timeLimitReached();
		}
		else if( $missionRunning && $VoteInProgress && !$TimeLimitChanged )
		{
			//Restart the function so the map can end if the Vote doesnt pass.
			schedule(2000, 0, "RestartcheckTimeLimit", %game, %forced);

			if( !$VoteInProgressMsg )
			{
				messageAll('', '\c2Vote Overtime Initiated.~wfx/powered/turret_heavy_activate.wav', %display);
				$VoteInProgressMsg = true;
			}
		}
   }
   else
   {
      if(%curTimeLeftMS >= 20000)
         %game.timeCheck = %game.schedule(20000, "checkTimeLimit");
      else
         %game.timeCheck = %game.schedule(%curTimeLeftMS + 1, "checkTimeLimit");

      //now synchronize everyone's clock
      messageAll('MsgSystemClock', "", $Host::TimeLimit, %curTimeLeftMS);
   }
}

};

function RestartcheckTimeLimit(%game, %forced)
{
	%game.checkTimeLimit(%game, %forced);
}

function StartVOTimeVote(%game)
{
	$VoteSoundInProgress = true;
	$VoteInProgress = true;
	$TimeLimitChanged = false;
}

function ResetVOTimeChanged(%game)
{
	$VoteInProgress = false;
	$TimeLimitChanged = true;
	$VoteInProgressMsg = false;
	$VoteSoundInProgress = false;
}

function ResetVOall(%game)
{
	$VoteInProgress = false;
	$TimeLimitChanged = false;
	$VoteInProgressMsg = false;
	$VoteSoundInProgress = false;
}


// Prevent package from being activated if it is already
if (!isActivePackage(VoteOverTime))
    activatePackage(VoteOverTime);