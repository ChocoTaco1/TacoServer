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
		%votemsg = "Press Insert for Yes or Delete for No.";
		
		switch$(%typename)
		{
			case "VoteChangeMission":
				messageAll('', '\c1Vote in Progress: \c0To change the mission to %1 (%2). %3~wgui/objective_notification.wav', %arg1, %arg2, %votemsg );
			
			case "VoteSkipMission":
				messageAll('', '\c1Vote in Progress: \c0To skip the mission to %1. %2~wgui/objective_notification.wav', $EvoCachedNextMission, %votemsg  );
			
			case "VoteChangeTimeLimit":
				if(%arg1 $= "999") %arg1 = "unlimited";
				messageAll('', '\c1Vote in Progress: \c0To change the time limit to %1. %2~wgui/objective_notification.wav', %arg1, %votemsg );
				
			case "VoteKickPlayer":			
				if(%arg1.team != 0 && Game.numTeams > 1) //Not observer
				{
				   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++) 
				   {
					  %cl = ClientGroup.getObject(%idx);
					  
						if (%cl.isAdmin == true)
						{ 
							if(%cl.team !$= %arg1.team) //Not on admins team
							{
								messageClient(%cl, '', '\c1Vote in Progress: \c0To kick %1 on the other team.~wgui/objective_notification.wav', %arg1.name);
							}
							else //Is on admins team
								messageClient(%cl, '', '\c1Vote in Progress: \c0To kick player %1. %2~wgui/objective_notification.wav', %arg1.name, %votemsg );
						}
						else if(%cl.team $= %arg1.team) //Everyone else
							messageClient(%cl, '', '\c1Vote in Progress: \c0To kick player %1. %2~wgui/objective_notification.wav', %arg1.name, %votemsg );
					}
				}
				else //Is observer
					messageAll('', '\c1Vote in Progress: \c0To kick player %1. %3~wgui/objective_notification.wav', %arg1.name, %votemsg );
			case "VoteTournamentMode":
				messageAll('', '\c1Vote in Progress: \c0To change the mission to Tournament Mode (%1). %3~wgui/objective_notification.wav', %arg1, %arg2, %votemsg );
		}

		schedule(12000, 0, "VoteSound", %game, %typename, %arg1, %arg2, %VoteSoundRandom);
	}
}
