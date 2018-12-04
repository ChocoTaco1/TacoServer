//Make a sound every so seconds to make sure everyone votes

function VoteSound( %game )
{

	if($VoteSoundInProgress) 
	{
		messageAll('', '\c1Vote in Progress: \c0Press Insert for Yes or Delete for No.~wgui/objective_notification.wav', %display);
		schedule(12000, 0, "VoteSound", %game);
	}
	else
	return;

}
