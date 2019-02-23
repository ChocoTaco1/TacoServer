//Make a sound every so seconds to make sure everyone votes
//
//Enable or Disable VoteSound
//$Host::EnableVoteSound = 1;
//
//%VotesoundRandom must match $VoteSoundRandom to prevent duplicate messages
//$VoteSoundRandom is generated everytime a vote starts and if it doesnt match an ongoing schedule does not play.

function VoteSound( %game, %typename, %arg1, %arg2, %VoteSoundRandom )
{	
	if( $VoteSoundInProgress && $Host::EnableVoteSound && $VoteSoundRandom $= %VoteSoundRandom ) 
	{
		if(%typename $= "VoteChangeMission")
			messageAll('', '\c1Vote in Progress: \c0To change the mission to %1 (%2). Press Insert for Yes or Delete for No.~wgui/objective_notification.wav', %arg1, %arg2 );
		else if(%typename $= "VoteSkipMission")
			messageAll('', '\c1Vote in Progress: \c0To skip the mission to %1. Press Insert for Yes or Delete for No.~wgui/objective_notification.wav', $EvoCachedNextMission );
	    else if(%typename $= "VoteChangeTimeLimit")
		{
			if(%arg1 $= "999") %arg1 = "unlimited";
			messageAll('', '\c1Vote in Progress: \c0To change the time limit to %1. Press Insert for Yes or Delete for No.~wgui/objective_notification.wav', %arg1 );
		}
		else if(%typename $= "VoteKickPlayer")
			messageAll('', '\c1Vote in Progress: \c0To kick player %1. Press Insert for Yes or Delete for No.~wgui/objective_notification.wav', %arg1.name );
		else
			messageAll('', '\c1Vote in Progress: \c0Press Insert for Yes or Delete for No.~wgui/objective_notification.wav');

		schedule(12000, 0, "VoteSound", %game, %typename, %arg1, %arg2, %VoteSoundRandom);
	}
	else
	return;
}
