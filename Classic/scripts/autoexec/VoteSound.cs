//Make a sound every so seconds to make sure everyone votes
//
// Enable or Disable VoteSound
// $Host::EnableVoteSound = 1;

function VoteSound( %game )
{

	if( $VoteSoundInProgress && $Host::EnableVoteSound ) 
	{
		messageAll('', '\c1Vote in Progress: \c0Press Insert for Yes or Delete for No.~wgui/objective_notification.wav', %display);
		schedule(12000, 0, "VoteSound", %game);
	}
	else
	return;

}
