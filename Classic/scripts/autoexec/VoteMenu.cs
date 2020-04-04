//$Host::AllowAdmin2Admin = 0;
//$Host::AllowAdminBan = 0;
//$Host::AllowAdminVotes = 1;
//$Host::AllowAdminStopVote = 1;
//$Host::AllowAdminPassVote = 1;
//$Host::AllowMapScript = "True";
//$Host::AllowPlayerVoteChangeMission = 1;
//$Host::AllowPlayerVoteSkipMission = 1;
//$Host::AllowPlayerVoteTimeLimit = 1;
//$Host::AllowPlayerVoteTournamentMode = 1;

package ExtraVoteMenu
{

function DefaultGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
{
   switch$ (%typeName)
   {
	  case "cancelMatchStart":
         %game.cancelMatchStart(%admin, %arg1, %arg2, %arg3, %arg4);

      case "passRunningVote":
         %game.passRunningVote(%admin, %arg1, %arg2, %arg3, %arg4);

      case "stopRunningVote":
         %game.stopRunningVote(%admin, %arg1, %arg2, %arg3, %arg4);

      default:
         Parent::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4);
   }
}

function DefaultGame::sendGameVoteMenu(%game, %client, %key)
{
	%isAdmin = (%client.isAdmin || %client.isSuperAdmin);
	%multipleTeams = %game.numTeams > 1;

	// ********************************************
	//    Admin Vote For ... Submenu
	// ********************************************
	if (!$Host::TournamentMode)
	{	  
		if (%client.ForceVote > 0)
			%client.ForceVote = %client.ForceVote - 1;

		if (%client.ForceVote > 0)
		{
			messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTournamentMode', 'change server to Tournament.', 'Vote Tournament Mode');         
			messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Vote to Change the Mission');
			messageClient(%client, 'MsgVoteItem', "", %key, 'VoteSkipMission', 'skip the mission to', 'Vote to Skip Mission' );
			messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeTimeLimit', 'change the time limit', 'Vote to Change the Time Limit');
			messageClient(%client, 'MsgVoteItem',"",  %key, 'ForceVote', 'Cancel Force Vote', "Cancel 'Vote To...'");
			return; // Display no further vote options
		}
	}
	// TEAM OPTIONS
	if(!$Host::TournamentMode)
	{
		if(%client.team != 0) // he isn't an observer
		{
			if(%multipleTeams)
			messageClient(%client, 'MsgVoteItem', "", %key, 'ChooseTeam', "", 'Change your Team');

			if($MatchStarted)
			messageClient(%client, 'MsgVoteItem', "", %key, 'MakeObserver', "", 'Become an Observer');
		}
	}
	else if(%client.isAdmin) // only admins can change team during tournament mode
	{
		if(%client.team != 0) // he isn't an observer
		{
			if(%multipleTeams)
				messageClient(%client, 'MsgVoteItem', "", %key, 'ChooseTeam', "", 'Change your Team');

			messageClient(%client, 'MsgVoteItem', "", %key, 'MakeObserver', "", 'Become an Observer');
		}
	}
	if(!%client.canVote && !%isAdmin)
		return;

	if(%game.scheduleVote $= "")
	{
		if(!%client.isAdmin)
		{
			if(!$Host::TournamentMode)
			{
				if($Host::AllowPlayerVoteChangeMission)
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Vote to Change the Mission');
				if($Host::AllowPlayerVoteTournamentMode)
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTournamentMode', 'Change server to Tournament.', 'Vote Tournament Mode');
				if($Host::AllowPlayerVoteTimeLimit)
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeTimeLimit', 'change the time limit', 'Vote to Change the Time Limit');
				if($Host::AllowPlayerVoteSkipMission)  
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteSkipMission', 'skip the mission to', 'Vote to Skip Mission' );
			}
			else
			{
				if(!$MatchStarted && !$CountdownStarted)
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteMatchStart', 'Start Match', 'Vote to Start the Match');

					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Vote to Change the Mission');
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteFFAMode', 'Change server to Free For All.', 'Vote Free For All Mode');
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeTimeLimit', 'change the time limit', 'Vote to Change the Time Limit');

				if(%multipleTeams)
				{
					if($teamDamage)
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'disable team damage', 'Vote to Disable Team Damage');
					else
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'enable team damage', 'Vote to Enable Team Damage');
				}
			}
		}
		else
		{
			if(!$Host::TournamentMode)
			{
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTournamentMode', 'Change server to Tournament.', 'Tournament Mode');
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Change the Mission');
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteSkipMission', 'skip the mission to', 'Skip the Mission' );
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeTimeLimit', 'change the time limit', 'Change the Time Limit');

				if( $Host::AllowAdminVotes )
					messageClient(%client, 'MsgVoteItem', "", %key, 'ForceVote', 'Vote to ...', 'Vote to ...');
			}
			else
			{
			if(!$MatchStarted && !$CountdownStarted)
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteMatchStart', 'Start Match', 'Start Match');
			if(!$MatchStarted && $CountdownStarted)
				messageClient(%client, 'MsgVoteItem', "", %key, 'cancelMatchStart', 'Cancel Match Start', 'Cancel Match Start');

				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeMission', 'change the mission to', 'Change the Mission');
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteFFAMode', 'Change server to Free For All.', 'Free For All Mode');
				messageClient(%client, 'MsgVoteItem', "", %key, 'VoteChangeTimeLimit', 'change the time limit', 'Change the Time Limit');

			if($Host::Password !$= "")
				messageClient(%client, 'MsgVoteItem', "", %key, 'TogglePUGpassword', 'Disable PUG Password', 'Disable PUG Password');
			else
				messageClient(%client, 'MsgVoteItem', "", %key, 'TogglePUGpassword', 'Enable PUG Password', 'Enable PUG Password');			
			}
			if(%multipleTeams)
			{
				if($teamDamage)
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'disable team damage', 'Disable Team Damage');
				else
					messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'enable team damage', 'Enable Team Damage');
			}

			//Toggle Tournament Net Client
			if(%client.isAdmin && $Host::EnableNetTourneyClient)
				messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Disable Tournament Net Client', "Disable Tournament Net Client" );
			else if(%client.isAdmin)
				messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Enable Tournament Net Client', "Enable Tournament Net Client" );

		}

		if ($Host::ServerRules[1] !$= "" )
			messageClient( %client, 'MsgVoteItem', "", %key, 'showServerRules', 'show server rules', "Show Server Rules" );
	}
	else
	{
		if(%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminStopVote)) // allow admins to stop votes
			messageClient( %client, 'MsgVoteItem', "", %key, 'stopRunningVote', 'stop current vote', 'Stop the Vote');

		if(%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminPassVote)) // allow admins to pass votes
			messageClient( %client, 'MsgVoteItem', "", %key, 'passRunningVote', 'pass current vote', 'Pass the Vote');
	}
}

// Eolk - completely re-wrote this.
function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote)
{
	%isAdmin = ( %client.isAdmin || %client.isSuperAdmin );
	if(!%client.canVote && !%isAdmin)
		return;

	if(Game.scheduleVote !$= "" && (!%isAdmin || (%isAdmin && %client.adminVoteSet)))
	{
		messageClient(%client, 'voteAlreadyRunning', "\c2A vote is already in progress.");
		%client.adminVoteSet = 0;
		return;
	}

	%teamSpecific = 0;
	switch$(%typeName)
	{
		case "VoteKickPlayer":
			if(%client == %arg1) // client is trying to votekick himself
				return; // Use the leave button instead, pal.

			if(%isAdmin) // Admin is trying to kick
			{
				if((%arg1.isAdmin && !%client.isSuperAdmin) || %arg1.isSuperAdmin) // target is an admin and kicker is just an admin, or if target is a super admin
				{
					messageClient(%client, "MsgClient", "\c2You cannot kick "@%arg1.nameBase@".");
					return;
				}
			}
			else // Player is voting to kick
			{
				if(%arg1.isAdmin) // target is an admin
				{
					messageClient(%client, "MsgClient", "\c2You cannot vote to kick "@%arg1.nameBase@", "@(%arg1.sex $= "Male" ? "he" : "she")@" is an admin!");
					return;
				}

				if(%arg1.team != %client.team)
				{
					messageClient(%client, "MsgClient", "\c2Kick votes must be team based.");
					return;
				}

				if($CMHasVoted[%client.guid] >= $Host::ClassicMaxVotes && !%isAdmin) // they've voted too many times
				{
					messageClient(%client, "", "\c2You have exhausted your voting rights for this mission.");
					return;
				}

				%msg = %client.nameBase @ " initiated a vote to kick player " @ %arg1.nameBase @ ".";
				messageAdmins("", "\c5[A]\c1"@ %msg @"~wgui/objective_notification.wav");
				$CMHasVoted[%client.guid]++;
			}

			if(%arg1.team != 0)
				%teamSpecific = 1;

			Game.kickClient = %arg1;
			Game.kickClientName = %arg1.name;
			Game.kickClientNameBase = %arg1.nameBase;
			Game.kickGuid = %arg1.guid;
			Game.kickTeam = %arg1.team;

		case "VoteAdminPlayer":
			if(%arg1.isAdmin) // target is already an admin
				return; // can't vote to admin an admin!

			if(%client.isAdmin) // our pal is an admin
			{
				if(!%client.isSuperAdmin) // ... but not a super admin
					return; // insufficient privileges
			}
			else // not an admin
			{
				if(!$host::allowadminplayervotes) // admin player votes are NOT enabled
					return; // can't do that pal

				%msg = %client.nameBase @ " initiated a vote to admin player " @ %arg1.nameBase @ ".";
			}

		case "BanPlayer":
			if(%client.isSuperAdmin && !%arg1.isSuperAdmin) // we're a super admin, and our target isn't a super admin
				ban(%arg1, %client); // ban 'em
			return; // stop the function in its tracks

		case "VoteChangeMission":
			// Vote-spoof prevention right here
			%arg1 = $HostMissionFile[%arg3];
			%arg2 = $HostTypeName[%arg4];
			if(!checkMapExist(%arg1, %arg2))
				return;

			// We passed the spoof check, give it the fancy label
			%arg1 = $HostMissionName[%arg3];
			%arg2 = $HostTypeDisplayName[%arg4];
			if((!%isAdmin && $Host::AllowPlayerVoteChangeMission) || (%isAdmin && %client.ForceVote)) // not admin
			{
				if($CMHasVoted[%client.guid] >= $Host::ClassicMaxVotes && !%isAdmin) // they've voted too many times
				{
					messageClient(%client, "", "\c2You have exhausted your voting rights for this mission.");
					return;
				}

				%msg = %client.nameBase @ " initiated a vote to change the mission to " @ %arg1 @ " (" @ %arg2 @ ").";
				$CMHasVoted[%client.guid]++;
			}

		case "VoteTeamDamage":
			if(!%isAdmin)
			{
				%msg = %client.nameBase @ " initiated a vote to " @ ($TeamDamage == 0 ? "enable" : "disable") @ " team damage.";
			}

		case "VoteTournamentMode":
			if((!%isAdmin && $Host::AllowPlayerVoteTournamentMode) || (%isAdmin && %client.ForceVote))
			{
				if(getAdmin() == 0)
				{
					messageClient(%client, 'clientMsg', "There must be a server admin to play in Tournament Mode.");
					return;
				}

				%msg = %client.nameBase @ " initiated a vote to switch the server to Tournament Mode (" @ %arg1 @ ").";
			}

		case "VoteFFAMode":
			if(!%isAdmin || (%isAdmin && %client.ForceVote))
			{
				%msg = %client.nameBase @ " initiated a vote to switch the server to Free For All mode.";
			}

		case "VoteChangeTimeLimit":
			if($CurrentMissionType $= "Siege") // Can't change time in this one
			{
				messageClient(%client, "", "\c2Cannot change the time limit in this gametype.");
				return;
			}

			// Eolk - don't let votes/admins end the map immediately by switching the time limit below the elapsed time
			if((%arg1 * 60 * 1000) < (getSimTime() - $missionStartTime))
			{
				messageClient(%client, "", "\c2Switching to this time would cause the game to end immediately.");
				return;
			}

			if(%arg1 == $Host::TimeLimit)
			{
				messageClient(%client, "", "\c2Switching to this time wouldn't affect the time limit at all.");
				return;
			}

			if((!%isAdmin && $Host::AllowPlayerVoteTimeLimit) || (%isAdmin && %client.ForceVote))
			{
				if(%arg1 $= "999") %time = "unlimited"; else %time = %arg1;
				%msg = %client.nameBase @ " initiated a vote to change the time limit to " @ %time @ ".";
				// VoteOvertime
				StartVOTimeVote(%game);
			}

		case "VoteMatchStart":
			if(!%isAdmin)
			{
				if($MatchStarted || $CountdownStarted)
					return;

				%msg = %client.nameBase @ " initiated a vote to start the match.";
			}
			
	    case "CancelMatchStart":
			if(%isAdmin) // only admins can cancel match start
			{
				adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
				adminLog(%client, " canceled match start.");
			}

		case "VoteGreedMode":
			if($CurrentMissionType !$= "Hunters" || $CurrentMissionType !$= "TeamHunters")
				return;

			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.greedMode == 0 ? "enable" : "disable") @ " greed mode.";

		case "VoteHoardMode":
			if($CurrentMissionType !$= "Hunters" || $CurrentMissionType !$= "TeamHunters")
				return;

			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.hoardMode == 0 ? "enable" : "disable") @ " hoard mode.";

		case "VoteRandomTeams":
			if(!%isAdmin || (%isAdmin && %client.ForceVote))
			{
				%msg = %client.nameBase @ " initiated a vote to " @ ($RandomTeams == 0 ? "enable" : "disable") @ " random teams.";
			}

		case "VoteFairTeams":
			if(!%isAdmin || (%isAdmin && %client.ForceVote))
			{
				%msg = %client.nameBase @ " initiated a vote to " @ ($FairTeams == 0 ? "enable" : "disable") @ " fair teams.";
			}

		case "VoteSkipMission":
			if((!%isAdmin && $Host::AllowPlayerVoteSkipMission) || (%isAdmin && %client.ForceVote))
			{
				if($CMHasVoted[%client.guid] >= $Host::ClassicMaxVotes && !%isAdmin)
				{
					messageClient(%client, "", "\c2You have exhausted your voting rights for this mission.");
					return;
				}

				%msg = %client.nameBase @ " initiated a vote to skip the current mission.";
				$CMHasVoted[%client.guid]++;
			}

		case "passRunningVote":
			if (%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminPassVote))
			{
				adminStartNewVote( %client, %typename, %arg1, %arg2, %arg3, %arg4);
				adminLog(%client, " passed the vote in progress.");
			}
	 
		case "stopRunningVote":
			if(%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminStopVote))
			{
				adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
				adminLog(%client, " stopped the vote in progress.");
			}

		// LakRabbit Stuff
		case "VoteDuelMode":
			if(!$CurrentMissionType $= "LakRabbit")
				return;

			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.duelMode == 0 ? "enable" : "disable") @ " duel mode.";

		case "VoteSplashDamage":
			if(!$CurrentMissionType $= "LakRabbit")
				return;

			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.noSplashDamage == 0 ? "enable" : "disable") @ " splash damage.";

		case "VotePro":
			if(!$CurrentMissionType $= "LakRabbit")
				return;

			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.pubPro == 0 ? "enable" : "disable") @ " pro mode.";
			
		case "DMSLOnlyMode":
			if(!$CurrentMissionType $= "DM")
				return;
		
			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.DMSLOnlyMode == 0 ? "enable" : "disable") @ " shocklance only mode.";
		 
		case "SCtFProMode":
			if(!$CurrentMissionType $= "sctf")
				return;
		
			if(!%isAdmin || (%isAdmin && %client.ForceVote))
				%msg = %client.nameBase @ " initiated a vote to " @ (Game.SCtFProMode == 0 ? "enable" : "disable") @ " pro mode.";
		 
		case "showServerRules":
			if (($Host::ServerRules[1] !$= "") && (!%client.CantView))
			{
				for ( %i = 1; $Host::ServerRules[%i] !$= ""; %i++ )
				{
				   messageClient(%client, 'ServerRule', '\c2%1', $Host::ServerRules[%i] );
				}
				%client.cantView = true;
				%client.schedViewRules = schedule( 10000, %client, "resetViewSchedule", %client );
			}
			return;
		case "TogglePUGpassword":
			if (%client.isAdmin)
			{
				if($Host::Password !$= "")
				{
				   $Host::Password = "";
				   messageClient( %client, '', "PUG password been disabled.~wfx/powered/vehicle_screen_on.wav" );
				   adminLog(%client, " has disabled pug password." );
				}
				else
				{
				   $Host::Password = $Host::PUGPassword;
				   messageClient( %client, '', "PUG password been enabled.~wfx/powered/vehicle_screen_on.wav" );
				   adminLog(%client, " has enabled pug password." );
				}
			}
			return;
		case "ToggleTourneyNetClient":
			if (%client.isAdmin)
			{
				if($Host::EnableNetTourneyClient)
				{
				   $Host::EnableNetTourneyClient = 0;
				   
				   if(isActivePackage(checkver))
						deactivatePackage(checkver);
				   
				   messageClient( %client, '', "Tournament Net Client checking has been disabled.~wfx/powered/vehicle_screen_on.wav" );
				   adminLog(%client, " has disabled Net Tourney Client checking.");
				}
				else
				{
				   $Host::EnableNetTourneyClient = 1;
				   
				   if(!isActivePackage(checkver))
						activatePackage(checkver);
				   
				   //Boot Offenders into Obs
				   CheckVerObserver(%client);
				   
				   messageClient( %client, '', "Tournament Net Client checking has been enabled.~wfx/powered/vehicle_screen_on.wav" );
				   ResetGetCountsStatus();
				   adminLog(%client, " has enabled Net Tourney Client checking.");
				}
			}
			return;
		case "ForceVote":
			if (%client.isAdmin && $Host::AllowAdminVotes)
			{
				if (%client.ForceVote)
				{
				   %client.ForceVote = 0;
				   messageClient( %client, '', 'Vote to ... cancelled.' );
				}
				else
				{
				   %client.ForceVote = 2;
				   messageClient( %client, '', "Now select what to vote on, please." );
				}
			}
			return;

		default:
			return;
	}

	if(%isAdmin && !%client.adminVoteSet && !%client.ForceVote)
		adminStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4);
	else
		playerStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %teamSpecific, %msg);
}

//exec("scripts/autoexec/VoteMenu.cs");

function playerStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %teamSpecific, %msg)
{
	%clientsVoting = 0;
	%count = ClientGroup.getCount();
	if(%teamSpecific)
	{
		for(%i = 0; %i < %count; %i++)
		{
			%cl = ClientGroup.getObject(%i);
			if(%cl.team == %client.team && !%cl.isAIControlled())
			{
				//Specifically for votehud compatibility
				switch$(%typeName)
				{
					case "VoteKickPlayer":			
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "kick player", %arg1.name);
					default:
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, %arg1);
				}
				%clientsVoting++;
			}
		}

		for(%i = 0; %i < %count; %i++)
		{
			%cl = ClientGroup.getObject(%i);
			if(%cl.team == %client.team && !%cl.isAIControlled())
				messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));
		}
	}
	else
	{
		if(%typeName $= "VoteChangeTimeLimit")
		{
			if(%arg1 $= "999") 
				%time = "Unlimited"; 
			else 
				%time = %arg1;
		}
		
		%count = ClientGroup.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%cl = ClientGroup.getObject(%i);
			if(!%cl.isAIControlled())
			{
				//Specifically for votehud compatibility
				switch$(%typeName)
				{
					case "VoteChangeMission":
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "change the mission to", %arg1, %arg2);
					case "VoteSkipMission":
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "skip the mission");
					case "VoteChangeTimeLimit":
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "change the time limit to", %time);
					case "VoteKickPlayer":			
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "kick player", %arg1.name);
					case "VoteTournamentMode":
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "Tournament Mode", %arg1);
					case "VoteMatchStart":
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "Start Match");
					case "VoteFFAMode":
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, "Free For All Mode");
					default:
						messageClient( %cl, 'VoteStarted', "\c2" @ %msg, %client.name, %arg1);
				}
				%clientsVoting++;
			}
		}

		for(%i = 0; %i < %count; %i++)
		{
			%cl = ClientGroup.getObject(%i);
			if(!%cl.isAIControlled())
				messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));
		}
	}

	clearVotes();
	Game.voteType = %typeName;
	Game.scheduleVote = schedule(($Host::VoteTime * 1000), 0, "calcVotes", %typeName, %arg1, %arg2, %arg3, %arg4);
	
	// Eolk - Voting control variables
	Game.votingArgs[typeName] = %typeName;
	Game.votingArgs[arg1] = %arg1;
	Game.votingArgs[arg2] = %arg2;
	Game.votingArgs[arg3] = %arg3;
	Game.votingArgs[arg4] = %arg4;

	%client.vote = true;
	messageAll('addYesVote', "");
	if(%client.team != 0)
		clearBottomPrint(%client);

   %client.canVote = false;
   %client.rescheduleVote = schedule(($Host::voteSpread * 1000) + ($Host::voteTime * 1000) , 0, "resetVotePrivs", %client);
   
   echo(%msg);
   
   // Log Vote
   if($Host::ClassicVoteLog)
   {
	   %votemsg = %typeName SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4;
	   voteLog(%client, %votemsg);
   }
   
   if($Host::EnableVoteSoundReminders > 0)
   {
		%time = mFloor($Host::VoteTime / ($Host::EnableVoteSoundReminders + 1)) * 1000;
		//echo(%time);
		for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
				Game.voteReminder[%i] = schedule((%time * (%i + 1)), 0, "VoteSound", %game, %typename, %arg1, %arg2);
   }
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset ClassicMaxMapChangeVotes
	deleteVariables("$CMHasVoted*"); // Eolk - let people who have voted vote again
}

function DefaultGame::cancelMatchStart(%game, %admin)
{
   if(%admin && $Host::TournamentMode && !$MatchStarted && $CountdownStarted)
   {
      CancelCountdown();
      for(%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         messageClient(%cl, 'MsgAdminForce', "\c2Match Start Canceled.");
         messageClient(%cl, 'MsgSystemClock', "", 0, 0);

         %cl.notready = 1;
         %cl.notReadyCount = "";
         centerprint(%cl, "\nPress FIRE when ready.", 0, 3);
      }
   }
}

//------------------------------------------------------------------------------
// all team based votes here
function DefaultGame::voteKickPlayer(%game, %admin, %client)
{
   %cause = "";
   
   if(isObject(%admin)) 
   {
      kick(%client, %admin, %client.guid );
      %cause = "(admin)";
   }
   else 
   {
      %team = %client.team;
      %totalVotes = %game.votesFor[%game.kickTeam] + %game.votesAgainst[%game.kickTeam];
      if(%totalVotes > 0 && (%game.votesFor[%game.kickTeam] / %totalVotes) > ($Host::VotePasspercent / 100)) 
      {
         kick(%client, %admin, %game.kickGuid);
         %cause = "(vote)";
      }
      else
      {   
         for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ ) 
         {
            %cl = ClientGroup.getObject( %idx );

            if (%cl.team == %game.kickTeam && !%cl.isAIControlled())
               messageClient( %cl, 'MsgVoteFailed', '\c2Kick player vote did not pass.' ); 
         }
      }
   }
   
   %game.kickTeam = "";
   %game.kickGuid = "";
   %game.kickClientName = "";

   if(%cause !$= "")
      logEcho($AdminCl.nameBase @ ": " @ %name @ " (cl " @ %game.kickClient @ ") kicked " @ %cause, 1);
}

//------------------------------------------------------------------------------
function DefaultGame::voteChangeMission(%game, %admin, %missionDisplayName, %typeDisplayName, %missionId, %missionTypeId)
{
   %mission = $HostMissionFile[%missionId];
   if ( %mission $= "" )
   {
      error( "Invalid mission index passed to DefaultGame::voteChangeMission!" );
      return;
   }

   %missionType = $HostTypeName[%missionTypeId];
   if ( %missionType $= "" )
   {
      error( "Invalid mission type id passed to DefaultGame::voteChangeMission!" );
      return;
   }

   // Eolk - Part of $admincl fix.
   if(isObject(%admin))
   {
      messageAll('MsgAdminChangeMission', '\c2The Admin %3 has changed the mission to %1 (%2).', %missionDisplayName, %typeDisplayName, %admin.name );   
      %game.gameOver();
      loadMission( %mission, %missionType, false );   
   }
   else 
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))
      {
         messageAll('MsgVotePassed', '\c2The mission was changed to %1 (%2) by vote.', %missionDisplayName, %typeDisplayName ); 
         %game.gameOver();
         loadMission( %mission, %missionType, false );   
      }
      else
         messageAll('MsgVoteFailed', '\c2Change mission vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100)); 
   }
}

//------------------------------------------------------------------------------
function DefaultGame::voteTournamentMode( %game, %admin, %missionDisplayName, %typeDisplayName, %missionId, %missionTypeId )
{
   %mission = $HostMissionFile[%missionId];
   if ( %mission $= "" )
   {
      error( "Invalid mission index passed to DefaultGame::voteTournamentMode!" );
      return;
   }

   %missionType = $HostTypeName[%missionTypeId];
   if ( %missionType $= "" )
   {
      error( "Invalid mission type id passed to DefaultGame::voteTournamentMode!" );
      return;
   }

   %cause = "";
   if (isObject(%admin)) 
   {
      messageAll( 'MsgAdminForce', '\c2The Admin %2 has switched the server to Tournament mode (%1).', %missionDisplayName, %admin.name );
      setModeTournament( %mission, %missionType );
   }
   else 
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))  
      {
         messageAll('MsgVotePassed', '\c2Server switched to Tournament mode by vote (%1): %2 percent.', %missionDisplayName, mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100)); 
         setModeTournament( %mission, %missionType );
      }
      else
         messageAll('MsgVoteFailed', '\c2Tournament mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
   }

   if(%cause !$= "")
      logEcho($AdminCl.nameBase @ ": tournament mode set "@%cause, 1);
}

//------------------------------------------------------------------------------
function DefaultGame::voteChangeTimeLimit( %game, %admin, %newLimit )
{
   if( %newLimit == 999 )
      %display = "unlimited";
   else
      %display = %newLimit;
      
   %cause = "";
   if ( %admin )
   {
      messageAll( 'MsgAdminForce', '\c2The Admin %2 changed the mission time limit to %1 minutes.', %display, %admin.name );
      $Host::TimeLimit = %newLimit;
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100)) 
      {
         messageAll('MsgVotePassed', '\c2The mission time limit was set to %1 minutes by vote.', %display);   
         $Host::TimeLimit = %newLimit;
		 // VoteOvertime
		 ResetVOTimeChanged(%game);
		 // Reset the voted time limit when changing mission
         $TimeLimitChanged = 1;
      }
      else 
	  {
         messageAll('MsgVoteFailed', '\c2The vote to change the mission time limit did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
		 // VoteOvertime
		 ResetVOall(%game);
	  }
   }

   //if the match has been started, reset the end of match countdown
   if ($matchStarted)
   {
      //schedule the end of match countdown
      %elapsedTimeMS = getSimTime() - $missionStartTime;
      %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) - %elapsedTimeMS;
      error("time limit="@$Host::TimeLimit@", elapsed="@(%elapsedTimeMS / 60000)@", curtimeleftms="@%curTimeLeftMS);
      CancelEndCountdown();
      EndCountdown(%curTimeLeftMS);
      cancel(%game.timeSync);
      %game.checkTimeLimit(true);
   }
}

//------------------------------------------------------------------------------
function DefaultGame::voteFFAMode( %game, %admin, %client )
{
   %cause = "";
   %name = getTaggedString(%client.name);
   
   if(isObject(%admin))
   {
      messageAll('MsgAdminForce', '\c2The Admin %1 has switched the server to Free For All mode.', %admin.name);   
      setModeFFA($CurrentMission, $CurrentMissionType); 
   }
   else 
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100)) 
      {
         messageAll('MsgVotePassed', '\c2Server switched to Free For All mode by vote.', %client); 
         setModeFFA($CurrentMission, $CurrentMissionType); 
      }
      else 
         messageAll('MsgVoteFailed', '\c2Free For All mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100)); 
   }
}

function DefaultGame::voteSkipMission(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
   if(isObject(%admin)) 
   {
      messageAll('MsgAdminForce', '\c2The Admin %1 has skipped to the next mission.',%admin.name );
      %game.gameOver();
      //loadMission( findNextCycleMission(), $CurrentMissionType, false );
      cycleMissions();
   }
   else
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))
      {
         messageAll('MsgVotePassed', '\c2The mission was skipped to next by vote.'); 
         echo("mission skipped (vote)");
         %game.gameOver();
         //loadMission( findNextCycleMission(), $CurrentMissionType, false );
         cycleMissions();
      }
      else
         messageAll('MsgVoteFailed', '\c2Skip mission vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
   }
}

//------------------------------------------------------------------------------
function DefaultGame::voteMatchStart( %game, %admin)
{
   %cause = "";
   %ready = forceTourneyMatchStart();
   if(isObject(%admin))
   {
      if(!%ready)
      {
         // z0dd - ZOD, 5/19/03. This was sending to %client, there is no %client declared, duh
         messageClient( %admin, 'msgClient', '\c2No players are ready yet.');
         return;
      }
      else
      {
         messageAll('msgMissionStart', '\c2The admin %1 has forced the match to start.', %admin.name);
         startTourneyCountdown();
      }
   }
   else
   {
      if(!%ready)
      {
         messageAll( 'msgClient', '\c2Vote passed to start match, but no players are ready yet.');
         return;
      }
      else
      {  
         %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
         // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
         if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))  
         {
            messageAll('MsgVotePassed', '\c2The match has been started by vote: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
            startTourneyCountdown();
         } 
         else
            messageAll('MsgVoteFailed', '\c2Start Match vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100)); 
      }
   }
}

//------------------------------------------------------------------------------
function DefaultGame::voteTeamDamage(%game, %admin)
{
   %setto = "";
   %cause = "";
   if(isObject(%admin)) 
   {
      if($teamDamage)
      {
         messageAll('MsgAdminForce', '\c2The Admin %1 has disabled team damage.', %admin.name);   
         $Host::TeamDamageOn = $TeamDamage = 0;
         %setto = "disabled";
      }
      else 
      {
         messageAll('MsgAdminForce', '\c2The Admin %1 has enabled team damage.', %admin.name);   
         $Host::TeamDamageOn = $TeamDamage = 1;
         %setto = "enabled";
      }
   }
   else 
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - z0dd - ZOD
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))
      {
         if($teamDamage) 
         {
            messageAll('MsgVotePassed', '\c2Team damage was disabled by vote.'); 
            $Host::TeamDamageOn = $TeamDamage = 0;
            %setto = "disabled";
         }
         else 
         {
            messageAll('MsgVotePassed', '\c2Team damage was enabled by vote.');  
            $Host::TeamDamageOn = $TeamDamage = 1;
            %setto = "enabled";
         }
      }
      else 
      {
         if($teamDamage)
            messageAll('MsgVoteFailed', '\c2Disable team damage vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));  
         else 
            messageAll('MsgVoteFailed', '\c2Enable team damage vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));   
      }
   }
}

function DefaultGame::sendGamePlayerPopupMenu( %game, %client, %targetClient, %key )
{
   if( !%targetClient.matchStartReady )
      return;

   %isAdmin = ( %client.isAdmin || %client.isSuperAdmin );
   %isSuperAdmin = (%client.isSuperAdmin);
   %isTargetSelf = ( %client == %targetClient );
   %isTargetAdmin = ( %targetClient.isAdmin || %targetClient.isSuperAdmin );
   %isTargetBot = %targetClient.isAIControlled();
   %isTargetObserver = ( %targetClient.team == 0 );
   %outrankTarget = false;

   if ( %client.isSuperAdmin ) // z0dd - ZOD, 7/11/03. Super admins should outrank even themseleves.
      %outrankTarget = 1; //!%targetClient.isSuperAdmin;
   else if ( %client.isAdmin )
      %outrankTarget = !%targetClient.isAdmin;

   if( %client.isSuperAdmin && %targetClient.guid != 0 ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   {
      messageClient( %client, 'MsgPlayerPopupItem', "", %key, "addAdmin", "", 'Add to Server Admin List', 10);
      messageClient( %client, 'MsgPlayerPopupItem', "", %key, "addSuperAdmin", "", 'Add to Server SuperAdmin List', 11);
   }

   //mute options
   if ( !%isTargetSelf )
   {
      if ( %client.muted[%targetClient] )
         messageClient( %client, 'MsgPlayerPopupItem', "", %key, "MutePlayer", "", 'Unmute Text Chat', 1);
      else
         messageClient( %client, 'MsgPlayerPopupItem', "", %key, "MutePlayer", "", 'Mute Text Chat', 1);

      if ( !%isTargetBot && %client.canListenTo( %targetClient ) )
      {
         if ( %client.getListenState( %targetClient ) )
            messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ListenPlayer", "", 'Disable Voice Com', 9 );
         else
            messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ListenPlayer", "", 'Enable Voice Com', 9 );
      }
      // ------------------------------------------
      // z0dd - ZOD 4/4/02. Observe a specific player
      if (%client.team == 0 && !%isTargetObserver)
         messageClient(%client, 'MsgPlayerPopupItem', "", %key, "ObservePlayer", "", 'Observe Player', 12);
   }
   if( !%client.canVote && !%isAdmin )
      return;

   // regular vote options on players
   if ( %game.scheduleVote $= "" && !%isAdmin && !%isTargetAdmin )
   {
      if ( $Host::allowAdminPlayerVotes && !%isTargetBot ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
         messageClient( %client, 'MsgPlayerPopupItem', "", %key, "AdminPlayer", "", 'Vote to Make Admin', 2 );
      
      if ( !%isTargetSelf )
      {
         messageClient( %client, 'MsgPlayerPopupItem', "", %key, "KickPlayer", "", 'Vote to Kick', 3 );
      }
   }
   // Admin only options on players:
   else if ( %isAdmin ) // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   {
      if ( !%isTargetBot && !%isTargetAdmin )
         messageClient( %client, 'MsgPlayerPopupItem', "", %key, "AdminPlayer", "", 'Make Admin', 2 );

      if ( !%isTargetSelf && %outrankTarget )
      {
         messageClient( %client, 'MsgPlayerPopupItem', "", %key, "KickPlayer", "", 'Kick', 3 );

         if ( !%isTargetBot )
         {
            // ------------------------------------------------------------------------------------------------------
            // z0dd - ZOD - Founder 7/13/03. Bunch of new admin features
            messageClient(%client, 'MsgPlayerPopupItem', "", %key, "Warn", "", 'Warn player', 13);
            if(%isTargetAdmin)
               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "StripAdmin", "", 'Strip admin', 14 );

            messageClient( %client, 'MsgPlayerPopupItem', "", %key, "SendMessage", "", 'Send Private Message', 15 );
            messageClient( %client, 'MsgPlayerPopupItem', "", %key, "PrintClientInfo", "", 'Client Info', 16 ); // z0dd - ZOD - MeBad, 7/13/03. Send client information.

            if( %client.isSuperAdmin )
            {
               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "BanPlayer", "", 'Ban', 4 );

               if ( %targetClient.isGagged )
                  messageClient( %client, 'MsgPlayerPopupItem', "", %key, "UnGagPlayer", "", 'UnGag Player', 17);
               else
                  messageClient( %client, 'MsgPlayerPopupItem', "", %key, "GagPlayer", "", 'Gag Player', 17);

               if ( %targetClient.isFroze )
                  messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ThawPlayer", "", 'Thaw Player', 18);
               else
                  messageClient( %client, 'MsgPlayerPopupItem', "", %key, "FreezePlayer", "", 'Freeze Player', 18);

               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "BootPlayer", "", 'Boot to the Rear', 19);
               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ExplodePlayer", "", 'Explode Player', 20);
            }
            if ( !%isTargetObserver )
            {
               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ToObserver", "", 'Force observer', 5 );
            }
         }
      }
      if ( %isTargetSelf || %outrankTarget )
      {
         if(%isTargetAdmin)
            messageClient( %client, 'MsgPlayerPopupItem', "", %key, "StripAdmin", "", 'Strip admin', 14 );

         if ( %game.numTeams > 1 )
         {   
            if ( %isTargetObserver )
            {
               %action = %isTargetSelf ? "Join " : "Change to ";
               %str1 = %action @ getTaggedString( %game.getTeamName(1) );      
               %str2 = %action @ getTaggedString( %game.getTeamName(2) );      

               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ChangeTeam", "", %str1, 6 );
               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ChangeTeam", "", %str2, 7 );
            }
            else if( %isSuperAdmin || ($Host::AllowAdminSwitchTeams && %isAdmin) )
            {
               %changeTo = %targetClient.team == 1 ? 2 : 1;   
               %str = "Switch to " @ getTaggedString( %game.getTeamName(%changeTo) );
               %caseId = 5 + %changeTo;      

               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ChangeTeam", "", %str, %caseId );

               // z0dd - ZOD, 7/11/03. Allow Super admins to force themselves to obs.
               messageClient( %client, 'MsgPlayerPopupItem', "", %key, "ToObserver", "", 'Force observer', 5 );
            }
         }
         else if ( %isTargetObserver )
         {
            %str = %isTargetSelf ? 'Join the Game' : 'Add to Game';
            messageClient( %client, 'MsgPlayerPopupItem', "", %key, "JoinGame", "", %str, 8 );
         }
      }
   }
}

function DefaultGame::passRunningVote(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
   if ( %admin && Game.scheduleVote !$= "" )
   {
      passCurrentVote();
      messageAll('MsgAdminForce', '\c2The Admin passed the vote.' );
	  echo("The admin" SPC %admin.nameBase SPC "has passed the vote.");
   }
}

function DefaultGame::stopRunningVote(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
   %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) + $missionStartTime - getSimTime();
   if(%admin && Game.scheduleVote !$= "" && %curTimeLeftMS > 0)
   {
      stopCurrentVote();
      messageAll('MsgAdminForce', '\c2The Admin stopped the vote.');
	  echo("The admin" SPC %admin.nameBase SPC "has stopped the vote.");
   }
}

function adminStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4)
{
	if ( Game.scheduleVote !$= "" && Game.voteType $= %typeName ) 
	{
		messageAll('closeVoteHud', "");
		cancel(Game.scheduleVote);
		Game.scheduleVote = "";

		// Eolk - Voting control variables
		Game.votingArgs[typeName] = "";
		Game.votingArgs[arg1] = "";
		Game.votingArgs[arg2] = "";
		Game.votingArgs[arg3] = "";
		Game.votingArgs[arg4] = "";
	}
	Game.evalVote(%typeName, %client, %arg1, %arg2, %arg3, %arg4);
}

};

// checkMapExist(%missionName, %missionType)
// Info: check if a map exist in the mission type
function checkMapExist(%missionName, %missionType)
{
	// Find if the mission exists
   for(%mis = 0; %mis < $HostMissionCount; %mis++)
       if($HostMissionFile[%mis] $= %missionName)
           break;

   // Now find if the mission type exists
   for(%type = 0; %type < $HostTypeCount; %type++)
       if($HostTypeName[%type] $= %missionType)
           break;

   // Now find if the mission's index in the mission-type specific sub-list exists
   for(%i = 0; %i < $HostMissionCount[%type]; %i++)
       if($HostMission[%type, %i] == %mis)
           break;

	if($HostMission[%type, %i] !$= "")
		return true; // valid map
   else
   	return false; // invalid map
}

// passCurrentVote()
// Info: passes a vote that is running.
function passCurrentVote() // Edit GG
{
   if(Game.scheduleVote !$= "")
   {
		messageAll('closeVoteHud', "");
		cancel(Game.scheduleVote);
		Game.scheduleVote = "";
		Game.kickClient = "";

		if(Game.votingArgs[typeName] $= "VoteKickPlayer") // special case here
		{
			Game.votesFor[Game.kickTeam] = ClientGroup.getCount() - $HostGameBotCount;
			Game.votesAgainst[Game.kickTeam] = 0;
		}
		else
		{
			Game.totalVotesFor = ClientGroup.getCount() - $HostGameBotCount;
			Game.totalVotesAgainst = 0;
		}

		Game.evalVote(Game.votingArgs[typeName], false, Game.votingArgs[arg1], Game.votingArgs[arg2], Game.votingArgs[arg3], Game.votingArgs[arg4]);
		clearVotes();

		//Stop vote chimes
		for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
		{
			if(isEventPending(Game.voteReminder[%i]))
				cancel(Game.voteReminder[%i]);
			Game.voteReminder[%i] = "";
		}
   }
}

// stopCurrentVote()
// Info: stop a vote that is still running
function stopCurrentVote()
{
	if(Game.scheduleVote !$= "")
	{
		messageAll('closeVoteHud', "");
		cancel(Game.scheduleVote);
		Game.scheduleVote = "";
		Game.kickClient = "";
		clearVotes();

		//Stop vote chimes
		for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
		{
			if(isEventPending(Game.voteReminder[%i]))
				cancel(Game.voteReminder[%i]);
			Game.voteReminder[%i] = "";
		}
	}
}

// calcVotes(%typeName, %arg1, %arg2, %arg3, %arg4)
// Info: fixed a bug that doesn't close properly the vote hud
function calcVotes(%typeName, %arg1, %arg2, %arg3, %arg4)
{
   if(%typeName $= "voteMatchStart")
   {
      if(($MatchStarted || $countdownStarted) && Game.scheduleVote !$= "")
      {
         stopCurrentVote();
         return;
      }
   }
   
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
   {
      %cl = ClientGroup.getObject(%idx);
      messageClient(%cl, 'closeVoteHud', "");
      
      if(%cl.vote !$= "") 
      {
         if(%cl.vote) 
         {
            Game.votesFor[%cl.team]++;
            Game.totalVotesFor++;
         } 
         else 
         {
            Game.votesAgainst[%cl.team]++;
            Game.totalVotesAgainst++;
         }
      }
      else 
      {
         Game.votesNone[%cl.team]++;
         Game.totalVotesNone++;
      }
   }   

   Game.evalVote(%typeName, false, %arg1, %arg2, %arg3, %arg4);
   Game.scheduleVote = "";
   Game.scheduleVoteArgs = "";
   Game.kickClient = "";
   clearVotes();
}

function messageAdmins(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   for(%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.isAdmin)
         messageClient(%cl, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function resetViewSchedule(%client)
{
  %client.cantView = false;
  %client.schedViewRules = "";
}

// Prevent package from being activated if it is already
if (!isActivePackage(ExtraVoteMenu))
    activatePackage(ExtraVoteMenu);