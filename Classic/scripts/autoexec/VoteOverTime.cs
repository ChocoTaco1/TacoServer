// Vote OverTime Script
//
// Dont allow the match to end if a time vote is pending
// Or if the timelimit has changed
//
// Changes were also made in how time votes are handled in scripts/autoexec/VoteMenu.cs
// DefaultGame::voteChangeMission, DefaultGame::voteChangeTimeLimit, serverCmdStartNewVote
//
// The VoteChangeTimeLimit functions in evo dictate VOStatus conditions

$VOStatus = "Normal";

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
		//Check if Time Vote is starting or active or if the timelimit has changed.
		//If the timelimit has changed, don't end the game.
		switch$($VOStatus)
		{
			case Starting:
				if($missionRunning)
				{
					messageAll('', '\c2Vote Overtime Initiated.~wfx/powered/turret_heavy_activate.wav', %display);
					$VOStatus = "InProgress";
				}
			case InProgress:
				//Do Nothing
			case TimeChanged:
				//Do Nothing
			case Normal:
				// time's up, put down your pencils
				%game.timeLimitReached();
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

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);

	//Reset everything to do with Vote Overtime
	ResetVOall(%game);
}

};

// Various Flags for the different situations
// Starting a TimeVote - Sets flags so the game wont end during this vote
function StartVOTimeVote(%game)
{
	$VOStatus = "Starting";
}

// Tribes wont change the time after its reached zero and you cant change it again afterwards until a gameover/map change.
// But this serves its purpose for extending the game whether it works (technically) or not.
function ResetVOTimeChanged(%game)
{
	$VOStatus = "TimeChanged";
}

// Reset everything. So everything functions normally after a map change.
function ResetVOall(%game)
{
	$VOStatus = "Normal";
}


// Prevent package from being activated if it is already
if (!isActivePackage(VoteOverTime))
    activatePackage(VoteOverTime);
