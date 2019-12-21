// VoteSound Script
//
// Make a sound every so seconds to make sure everyone votes
//
// Enable or Disable VoteSound
// $Host::EnableVoteSound = 1;
//
// %VotesoundRandom must match $VoteSoundRandom to prevent duplicate messages
// $VoteSoundRandom is generated everytime a vote starts and if it doesnt match an ongoing schedule does not play.

function VoteSound( %game, %typename, %arg1, %arg2, %VoteSoundRandom )
{	
	if( Game.scheduleVote !$= "" && $Host::EnableVoteSound && $VoteSoundRandom $= %VoteSoundRandom ) //Game.scheduleVote !$= "" is if vote is active
	{
		%votemsg = "Press Insert for Yes or Delete for No.";
		
		switch$(%typename)
		{
			case "VoteChangeMission":
				messageAll('', '\c1Vote in Progress: \c0To change the mission to %1 (%2). %3~wgui/objective_notification.wav', %arg1, %arg2, %votemsg );
				echo("Vote in Progress: To change the mission to" SPC %arg1 SPC "(" @ %arg2 @ ").");
			case "VoteSkipMission":
				messageAll('', '\c1Vote in Progress: \c0To skip the mission to %1. %2~wgui/objective_notification.wav', $EvoCachedNextMission, %votemsg  );
				echo("Vote in Progress: To skip the mission to" SPC $EvoCachedNextMission @ ".");
			case "VoteChangeTimeLimit":
				if(%arg1 $= "999") %arg1 = "unlimited";
				messageAll('', '\c1Vote in Progress: \c0To change the time limit to %1. %2~wgui/objective_notification.wav', %arg1, %votemsg );
				echo("Vote in Progress: To change the time limit to" SPC %arg1 @ ".");
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
				echo("Vote in Progress: To kick player" SPC %arg1.name @ ".");
			case "VoteTournamentMode":
				messageAll('', '\c1Vote in Progress: \c0To change the mission to Tournament Mode (%1). %3~wgui/objective_notification.wav', %arg1, %arg2, %votemsg );
				echo("Vote in Progress: To change the mission to Tournament Mode" SPC "(" @ %arg1 @ ").");
		}

		if(isEventPending($VoteSoundSchedule)) 
			cancel($VoteSoundSchedule);
		
		$VoteSoundSchedule = schedule(12000, 0, "VoteSound", %game, %typename, %arg1, %arg2, %VoteSoundRandom);
	}
}
