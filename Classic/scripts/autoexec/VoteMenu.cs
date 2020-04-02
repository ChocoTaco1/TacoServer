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

// These have been secured against all those wanna-be-hackers. 
$VoteMessage["VoteAdminPlayer"] 	 = "1";
$VoteMessage["VoteKickPlayer"] 		 = "1";
$VoteMessage["BanPlayer"] 			 = "1";
$VoteMessage["VoteChangeMission"] 	 = "1";
$VoteMessage["VoteTeamDamage", 0] 	 = "1";
$VoteMessage["VoteTeamDamage", 1] 	 = "1";
$VoteMessage["VoteTournamentMode"]	 = "1";
$VoteMessage["VoteFFAMode"]			 = "1";
$VoteMessage["VoteChangeTimeLimit"]  = "1";
$VoteMessage["VoteMatchStart"] 		 = "1";
$VoteMessage["VoteGreedMode", 0]	 = "1";
$VoteMessage["VoteGreedMode", 1] 	 = "1";
$VoteMessage["VoteHoardMode", 0]	 = "1";
$VoteMessage["VoteHoardMode", 1]	 = "1";
// z0dd - ZOD, 5/13/03. Added vote Random, Fair teams and armor limiting
$VoteMessage["VoteRandomTeams", 0]	 = "1";
$VoteMessage["VoteRandomTeams", 1]	 = "1";
$VoteMessage["VoteFairTeams", 0] 	 = "1";
$VoteMessage["VoteFairTeams", 1] 	 = "1";
$VoteMessage["VoteArmorLimits", 0]   = "1";
$VoteMessage["VoteArmorLimits", 1] 	 = "1";
$VoteMessage["VoteAntiTurtleTime"]	 = "1";
$VoteMessage["VoteArmorClass"] 		 = "1";
$VoteMessage["VoteClearServer"]		 = "1";
$VoteMessage["VoteSkipMission"] 	 = "1";
$VoteMessage["ForceVote"] 			 = "1";
$VoteMessage["CancelMatchStart"]  	 = "1";
$VoteMessage["passRunningVote"] 	 = "1";
$VoteMessage["stopRunningVote"] 	 = "1";
$VoteMessage["ToggleTourneyNetClient"] = "1";
$VoteMessage["TogglePUGpassword"]    = "1";
$VoteMessage["showServerRules"] 	 = "1";
$VoteMessage["DMSLOnlyMode"] 	     = "1";
$VoteMessage["SCtFProMode"] 	     = "1";
$VoteMessage["VoteDuelMode"]		 = "1";
$VoteMessage["VoteSplashDamage"]	 = "1";
$VoteMessage["VotePro"]				 = "1";


package ExtraVoteMenu
{

function DefaultGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
{
   switch$(%typeName)
   {   
      case "voteChangeMission":
         %game.voteChangeMission(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteTeamDamage":
         %game.voteTeamDamage(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteTournamentMode":
         %game.voteTournamentMode(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteMatchStart":
         %game.voteMatchStart(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteFFAMode":
         %game.voteFFAMode(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteChangeTimeLimit":
         %game.voteChangeTimeLimit(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteResetServer":
         %game.voteResetServer(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteKickPlayer":
         %game.voteKickPlayer(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteAdminPlayer":
         %game.voteAdminPlayer(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteGreedMode":
         %game.voteGreedMode(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteHoardMode":
         %game.voteHoardMode(%admin, %arg1, %arg2, %arg3, %arg4);

      // z0dd - ZOD, 5/23/03. Added vote for Random, Fair teams and armor limiting
      case "voteRandomTeams":
         %game.voteRandomTeams(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteFairTeams":
         %game.voteFairTeams(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteArmorLimits":
         %game.voteArmorLimits(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteClearServer":
         %game.voteClearServer(%admin, %arg1, %arg2, %arg3, %arg4);

      case "voteSkipMission":
         %game.voteSkipMission(%admin, %arg1, %arg2, %arg3, %arg4);
	  
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

function playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting, %teamSpecific)
{
   if(!%teamSpecific) // isn't a team specific vote (kick)
   {
      for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
      {
         %cl = ClientGroup.getObject(%idx);
         if(!%cl.isAIControlled())
            messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));
      }
   }
   else // is a team specific vote (kick)
   {
      for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
      {
         %cl = ClientGroup.getObject(%idx);
         if(%cl.team == %client.team && !%cl.isAIControlled())
            messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));
      }
   }
   clearVotes();
   Game.voteType = %typeName;
   Game.scheduleVote = schedule(($Host::VoteTime * 1000), 0, "calcVotes", %typeName, %arg1, %arg2, %arg3, %arg4);
   Game.scheduleVoteArgs[typename] = %typename;
   Game.scheduleVoteArgs[arg1]     = %arg1;
   Game.scheduleVoteArgs[arg2]     = %arg2;
   Game.scheduleVoteArgs[arg3]     = %arg3;
   Game.scheduleVoteArgs[arg4]     = %arg4;
   %client.vote = true;
   messageAll('addYesVote', "");
   if(%client.team != 0)
      clearBottomPrint(%client);

   %client.canVote = false;
   %client.rescheduleVote = schedule(($Host::voteSpread * 1000) + ($Host::voteTime * 1000) , 0, "resetVotePrivs", %client);
   
   echo("Vote Initiated by" SPC %client.nameBase SPC %typeName SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4);
   
   if($Host::EnableVoteSoundReminders > 0)
   {
		%time = mFloor($Host::VoteTime / ($Host::EnableVoteSoundReminders + 1)) * 1000;
		//echo(%time);
		for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
				Game.voteReminder[%i] = schedule((%time * (%i + 1)), 0, "VoteSound", %game, %typename, %arg1, %arg2);
   }
   //Added so vote will end on bogus votes
   //$AutoVoteTimeoutSchedule = schedule(($Host::VoteTime * 1000) + 1000, 0, "AutoVoteTimeout");
}

function AutoVoteTimeout()
{
   if(isEventPending($AutoVoteTimeoutSchedule)) 
		cancel($AutoVoteTimeoutSchedule);
   
   if( Game.scheduleVote !$= "")
   {
	   cancel(Game.scheduleVote);
	   Game.scheduleVote = "";
	   Game.kickClient = "";
	   Game.scheduleVoteArgs = "";
	   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
	   {
		  %cl = ClientGroup.getObject(%idx);
		  messageClient(%cl, 'closeVoteHud', "");
		  if(%cl.team != 0)
			 clearBottomPrint(%cl);
	   }
	   clearVotes();
	   $VoteSoundRandom = "";
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
         messageClient(%client, 'MsgVoteItem',"", %key, 'ForceVote', 'Cancel Force Vote', "Cancel 'Vote To...'");
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
			
			//if(%multipleTeams && $Host::AllowPlayerVoteTeamDamage)
            //{
               //if($teamDamage)
                  //messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'disable team damage', 'Vote to Disable Team Damage');
               //else
                  //messageClient(%client, 'MsgVoteItem', "", %key, 'VoteTeamDamage', 'enable team damage', 'Vote to Enable Team Damage');
            //}
            //if($CurrentMissionType !$= TR2) // z0dd - ZOD, 5/23/03. Added vote for Random teams
            //{
               //if ( $RandomTeams )
                  //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRandomTeams', 'disable random teams', 'Vote to Disable Random Teams' );
               //else
                  //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRandomTeams', 'enable random teams', 'Vote to Enable Random Teams' );
            //}
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

            //if($CurrentMissionType !$= TR2) // z0dd - ZOD, 5/23/03. Added vote for Random teams
            //{
               //if ( $RandomTeams )
                  //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRandomTeams', 'disable random teams', 'Disable Random Teams' );
               //else
                  //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteRandomTeams', 'enable random teams', 'Enable Random Teams' );
            //}
         }
		 
		 //Toggle Tournament Net Client
		 if(%client.isAdmin && $Host::EnableNetTourneyClient)
			messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Disable Tournament Net Client', "Disable Tournament Net Client" );
		 else if(%client.isAdmin)
			messageClient( %client, 'MsgVoteItem', "", %key, 'ToggleTourneyNetClient', 'Enable Tournament Net Client', "Enable Tournament Net Client" );
		 
      }

    if ($Host::ServerRules[1] !$= "" )
	{
	  messageClient( %client, 'MsgVoteItem', "", %key, 'showServerRules', 'show server rules', "Show Server Rules" );
	}

	//messageClient( %client, 'MsgVoteItem', "", %key, 'showNextMission', 'show next mission', "Show Next Mission" );
   }
   else
   {
      if(%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminStopVote)) // allow admins to stop votes
	{
	   messageClient(%client, 'MsgVoteItem', "", %key, 'stopRunningVote', 'stop current vote', 'Stop the Vote');
	}

      if (%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminPassVote) )
	{
	   messageClient( %client, 'MsgVoteItem', "", %key, 'passRunningVote', 'pass current vote', 'Pass the Vote');
	}
   }
   // Admin only options:
   if ( %client.isAdmin )
   {
      //if ( $LimitArmors )
         //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteArmorLimits', 'disable armor limiting', 'Disable armor limits' );
      //else
         //messageClient( %client, 'MsgVoteItem', "", %key, 'VoteArmorLimits', 'enable armor limiting', 'Enable armor limits' );

      // -----------------------------------------------------------------------------
      // z0dd - ZOD, 5/12/02. Add bot menu for admins
      //%totalSlots = $Host::maxPlayers - ($HostGamePlayerCount + $HostGameBotCount);
      //if( $HostGameBotCount > 0 && %totalSlots > 0)
         //messageClient( %client, 'MsgVoteItem', "", %key, 'Addbot', "", 'Add a Bot' );
      // -----------------------------------------------------------------------------
   }
}

function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote)
{
  // z0dd - ZOD, 9/29/02. Removed T2 demo code from here

  %typePass = true;

  // if not a valid vote, turn back.
  // z0dd - ZOD, 5/13/03. Added vote Random, Fair teams, armor limting, Anti-Turtle and Armor Class
  if($VoteMessage[%typeName] $= "" 	&& (%typeName !$= "VoteTeamDamage" 		&& %typeName !$= "VoteHoardMode" 
									&& %typeName !$= "VoteGreedMode" 		&& %typeName !$= "VoteRandomTeams" 
									&& %typeName !$= "VoteFairTeams" 		&& %typeName !$= "VoteArmorLimits"
									&& %typeName !$= "VoteAntiTurtleTime" 	&& %typeName !$= "VoteArmorClass"
									&& %typeName !$= "VoteChangeMission" 	&& %typeName !$= "VoteSkipMission"
									&& %typeName !$= "VoteKickPlayer" 		&& %typeName !$= "BanPlayer"
									&& %typeName !$= "VoteFFAMode" 			&& %typeName !$= "VoteTournamentMode"
									&& %typeName !$= "ForceVote" 			&& %typeName !$= "VoteMatchStart"
									&& %typeName !$= "VoteClearServer" 		&& %typeName !$= "VoteAdminPlayer"
									&& %typeName !$= "CancelMatchStart"		&& %typeName !$= "DMSLOnlyMode"
									&& %typeName !$= "SCtFProMode"			&& %typeName !$= "VotePro"
									&& %typeName !$= "VoteSplashDamage" 	&& %typeName !$= "VoteDuelMode")) {
      %typePass = false;
  }

  if(( $VoteMessage[ %typeName, $TeamDamage ] $= "" && %typeName $= "VoteTeamDamage" ))
      %typePass = false;

  if( !%typePass ){
      echo("New Vote failed. (%typePass)");
	  return; // -> bye ;)
  }
  
  %isAdmin = (%client.isAdmin || %client.isSuperAdmin);
  if(!%client.canVote && !%isAdmin)
    return;

  %clientsVoting = 0;

   // z0dd - ZOD, 5/19/03. Get the Admins client.
   if(%isAdmin)
      $AdminCl = %client;

  // Is this a tricon style call
  if ( TriconWrapper( %client, %arg1, %typename ) )
	return;

   switch$(%typeName)
   {
      case "VoteKickPlayer":
         if(%isAdmin && %client != %arg1) // client is an admin and the player to kick isn't the player himself
	   {
            if(!%client.isSuperAdmin && %arg1.isAdmin) // only super admins can kick admins
            {
               messageClient(%client, '', '\c2You can not kick %1, %2 is an Admin!', %arg1.name, %arg1.sex $= "Male" ? 'he' : 'she');
	         return;
            }
            Game.kickClientName = %arg1.name;
            kick(%arg1, %client, %arg1.guid); // kick the player without entering the vote process
            %authInfo = %arg1.getAuthInfo();
            adminLog(%client, " kicked " @ %arg1.nameBase @ "( " @ getField(%authInfo, 0) @ ", " @ getField(%authInfo, 1) @ ", " @ %arg1.guid @ ", " @ %arg1.getAddress() @ ")");
         }
         else // normal vote
         {
            if(%arg1.isAdmin) // don't let players to kick admins
            {
               messageClient(%client, '', '\c2You can not kick %1, %2 is an Admin!', %arg1.name, %arg1.sex $= "Male" ? 'he' : 'she');
               return;
            }
            if(%client.team != %arg1.team && %arg1.team != 0) // kick works only with teammates or observers
            {
               messageClient(%client, '', '\c2Kick votes must be team based.');
               return;
            }
            if(Game.scheduleVote !$= "") // a vote is already in progress
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            Game.kickClient = %arg1;
            Game.kickClientName = %arg1.name;
            Game.kickGuid = %arg1.guid;
            Game.kickTeam = %arg1.team;
            if(%arg1.team != 0 && Game.numTeams > 1)
            {
               for(%idx = 0; %idx < ClientGroup.getCount(); %idx++) 
               {
                  %cl = ClientGroup.getObject(%idx);

                  	if (%cl.isAdmin == true || (%cl.team == %client.team && !%cl.isAIControlled()))
					{   
						if(%cl.isAdmin == true && %cl.team !$= %client.team) 
						{
							messageClient(%cl, 'AdminOtherTeamKickVoteStarted', '\c2%1 has initiated a vote to kick %2 on the other team.~wgui/objective_notification.wav', %client.name, %arg1.name);
						}
						else
							messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, "kick player", %arg1.name);
							
						%clientsVoting++;
					}
               }
               playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting, true);
            }
            else
            {
               for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
               {
                  %cl = ClientGroup.getObject(%idx);
                  if(!%cl.isAIControlled())
                  {
                     messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, "kick player", %arg1.name);
					 %clientsVoting++;
                  }
               }
               playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
            }
         }

      case "BanPlayer":
         if((%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdminBan)) && %client != %arg1) // only admins can use ban
         {
            if(!%client.isSuperAdmin && %arg1.isAdmin) // only super admins can ban admins
            {
               messageClient(%client, '', '\c2You can not ban %1, %2 is an Admin!', %arg1.name, %arg1.sex $= "Male" ? 'he' : 'she');
               return;
            }
            ban(%arg1, %client); // ban the player without entering the vote process
            %authInfo = %arg1.getAuthInfo();
            adminLog(%client, " banned " @ %arg1.nameBase @ "( " @ getField(%authInfo, 0) @ ", " @ getField(%authInfo, 1) @ ", " @ %arg1.guid @ ", " @ %arg1.getAddress() @ ")");
         }
      
      case "VoteAdminPlayer":
         if((%client.isSuperAdmin || (%client.isAdmin && $Host::AllowAdmin2Admin)) && %client != %arg1)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " made " @ %arg1.nameBase @ " an Admin.");
         }
         else if($Host::allowAdminPlayerVotes) // normal vote
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, "admin player", %arg1.name);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "VoteChangeMission":
         if(%isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " changed the mission to " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else if($Host::TournamentMode || (!$Host::TournamentMode && $Host::AllowPlayerVoteChangeMission))
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            %mission = $HostMissionFile[%arg3];
            %missionType = $HostTypeName[%arg4];
            if(!checkMapExist(%mission, %missionType))
               return; // map doesn't exist

            if(!$Host::TournamentMode && $Host::MapFFA[%mission, %missionType] !$= "" && $Host::MapFFA[%mission, %missionType] == 0)
               return; // is FFA, but the map can't be played in FFA

            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3 (%4).', %client.name, "change the mission to", %arg1, %arg2);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

	  case "VoteSkipMission":
         if(%isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " skipped the mission.");
         }
         else if($Host::TournamentMode || (!$Host::TournamentMode && $Host::AllowPlayerVoteChangeMission))
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, "skip the mission" );
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "VoteFFAMode":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " changed the server to FFA Mode.");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 Free For All Mode.', %client.name, "change the server to");
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "VoteTournamentMode":
         if(%isAdmin && !%client.ForceVote)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " changed the server to Tournament Mode " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else if(!$Host::TournamentMode)
         {
            if(getAdmin() == 0)
            {
               messageClient(%client, 'clientMsg', 'There must be a server admin to play in Tournament Mode.');
               return;
            }
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 Tournament Mode (%3).', %client.name, "change the server to", %arg1);
				  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "VoteMatchStart":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " started the match.");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, "start the match");
				  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "CancelMatchStart":
         if(%isAdmin) // only admins can cancel match start
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " canceled match start.");
         }

      case "VoteTeamDamage":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, (%arg1 $= "enable team damage" ? " ENABLED team damage." : " DISABLED team damage."));
         }
         else if($Host::TournamentMode || (!$Host::TournamentMode && $Host::AllowPlayerVoteTeamDamage))
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            %actionMsg = $TeamDamage ? "disable team damage" : "enable team damage";
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "VoteChangeTimeLimit":
         if(%isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " changed the time limit to " @ %arg1);
         }
         else if($Host::TournamentMode || (!$Host::TournamentMode && $Host::AllowPlayerVoteTimeLimit))
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            if(%arg1 $= "999") %time = "unlimited"; else %time = %arg1;
			for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
				  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, "change the time limit to", %time);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
			StartVOTimeVote(%game);
         }

      case "VoteGreedMode":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, (%arg1 $= "enable greed mode" ? " ENABLED Greed mode." : " DISABLED Greed mode."));
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            %actionMsg = Game.greedMode ? "disable Greed mode" : "enable Greed mode";
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
				  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }

      case "VoteHoardMode":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, (%arg1 $= "enable hoard mode" ? " ENABLED Hoard mode." : " DISABLED Hoard mode."));
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            %actionMsg = Game.hoardMode ? "disable Hoard mode" : "enable Hoard mode";
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
				  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
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
      case "cancelServerRestart":
         if(%client.isSuperAdmin || (%client.isAdmin && $Host::EvoAdminCRCCheck))
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, " canceled a server restart.");
         }

      case "clearServerForMatch":
         if ((%client.isSuperAdmin && $Host::EvoSuperClearServer) || (%client.isAdmin && $Host::EvoAdminClearServer))
         {
            adminStartNewVote( %client, %typename, %arg1, %arg2, %arg3, %arg4 );
            adminLog(%client, " cleared server for match.");
         }
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
	  case "showNextMission":
         if ( !%client.CantView )
		 {
			%ShowNextMissionVar = $EvoCachedNextMission;
			if($Host::EvoTourneySameMap && $Host::TournamentMode) %ShowNextMissionVar = $CurrentMission @ " (Same)";
			//MessageAll('MsgNotifyEvoNextMission', '\c2Next Mission: \c1%1', %ShowNextMissionVar);
			messageClient(%client, 'MsgNotifyEvoNextMission', '\c2Next Mission: \c1%1', %ShowNextMissionVar);

            %client.cantView = true;
            %client.schedViewRules = schedule( 10000, %client, "resetViewSchedule", %client );
		 } 
      //
      // sonic9k 11/6/2003 - Added support for LakRabbit DuelMode option
      //
      case "VoteDuelMode":
         if( %isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
			%actionMsg = ($Host::LakRabbitDuelMode ? "disable Duel mode" : "enable Duel mode");
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }
      //
      // sonic9k 11/6/2003 - Added support for LakRabbit SplashDamage option
      //
      case "VoteSplashDamage":
         if( %isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
			%actionMsg = ($Host::LakRabbitNoSplashDamage ? "enable SplashDamage" : "disable SplashDamage");
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }
	  //
      // chocotaco 8/7/2018 - Added support for LakRabbit Pro option
      //
      case "VotePro":
         if( %isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
			%actionMsg = ($Host::LakRabbitPubPro ? "disable Pro mode" : "enable Pro mode");
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }
	  case "SCtFProMode":
         if( %isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
			%actionMsg = ($Host::SCtFProMode ? "disable Pro mode" : "enable Pro mode");
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }
	  case "DMSLOnlyMode":
         if( %isAdmin && !%client.ForceVote )
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
			%actionMsg = ($Host::DMSLOnlyMode ? "disable Shocklance Only Mode" : "enable Shocklance Only Mode");
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }
      //---------------------------- CLASSIC MOD
      case "VoteArmorLimits":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, %arg3);
         }

      case "VoteRandomTeams":
         if(%isAdmin)
         {
            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
            adminLog(%client, %arg3);
         }
         else
         {
            if(Game.scheduleVote !$= "")
            {
               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
               return;
            }
            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
            {
               %cl = ClientGroup.getObject(%idx);
               if(!%cl.isAIControlled())
               {
                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %arg3, %arg1);
                  %clientsVoting++;
               }
            }
            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
         }
	  default:
		return;
   }
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

function DefaultGame::voteChangeMission(%game, %admin, %missionDisplayName, %typeDisplayName, %missionId, %missionTypeId)
{
   %mission = $HostMissionFile[%missionId];
   %missionType = $HostTypeName[%missionTypeId];
   if(!checkMapExist(%mission, %missionType))
      return; // map doesn't exist

   if(!$Host::TournamentMode && $Host::MapFFA[%mission, %missionType] !$= "" && $Host::MapFFA[%mission, %missionType] == 0)
      return; // is FFA, but the map can't be played in FFA

   if(%admin) 
   {
      messageAll('MsgAdminChangeMission', '\c2The Admin has changed the mission to %1 (%2).', %missionDisplayName, %typeDisplayName);   
	  logEcho("mission changed to " @ %missionDisplayName @ "/" @ %typeDisplayName @ " (admin)");
      %game.gameOver();
      
      // set a flag, so next map the skip mission vote won't start
      $AdminChangedMission = 1;
      loadMission(%mission, %missionType, false);   
   }
   else 
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - ZOD
	  // Changed it back. Choco
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))
      {
         messageAll('MsgVotePassed', '\c2The mission was changed to %1 (%2) by vote.', %missionDisplayName, %typeDisplayName); 
         logEcho("mission changed to " @ %missionDisplayName @ "/" @ %typeDisplayName @ " (vote)");
		 %game.gameOver();
 
         loadMission(%mission, %missionType, false);		 
      }
      else 
	  {
         messageAll('MsgVoteFailed', '\c2Change mission vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
	  }
   }
}

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
   if (%admin) 
   {
      messageAll( 'MsgAdminForce', '\c2The Admin %2 has switched the server to Tournament mode (%1).', %missionDisplayName, $AdminCl.name );
      setModeTournament( %mission, %missionType );
      %cause = "(admin)";
   }
   else 
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - ZOD
	  // Changed it back. Choco
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))
      {
         messageAll('MsgVotePassed', '\c2Server switched to Tournament mode by vote (%1): %2 percent.', %missionDisplayName, mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100)); 
         setModeTournament( %mission, %missionType );
         %cause = "(vote)";
      }
      else
	  {
         messageAll('MsgVoteFailed', '\c2Tournament mode vote did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
      }
   }
   if(%cause !$= "")
      logEcho($AdminCl.nameBase @ ": tournament mode set "@%cause, 1);
}

function DefaultGame::voteChangeTimeLimit(%game, %admin, %newLimit)
{  
   if(%newLimit == 999)
      %display = "unlimited";
   else
      %display = %newLimit;
      
   %cause = "";
   if(%admin)
   {
	  messageAll('MsgAdminForce', '\c2The Admin changed the mission time limit to %1 minutes.', %display);
      $Host::TimeLimit = %newLimit;
      %cause = "(admin)";
	  
		// reset the voted time limit when changing mission
      	$TimeLimitChanged = 1;
   }
   else if($Host::AllowPlayerVoteTimeLimit)
   {
      %totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
      // Added people who dont vote into the equation, now if you do not vote, it doesn't count as a no. - ZOD
	  // Changed it back. Choco
      if(%totalVotes > 0 && (%game.totalVotesFor / (ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone)) > ($Host::VotePasspercent / 100))
      {
		 ResetVOTimeChanged(%game);
		 messageAll('MsgVotePassed', '\c2The mission time limit was set to %1 minutes by vote.', %display);   
		 $Host::TimeLimit = %newLimit;
         %cause = "(vote)";

         // reset the voted time limit when changing mission
         $TimeLimitChanged = 1;
      }
      else 
	  {
		messageAll('MsgVoteFailed', '\c2The vote to change the mission time limit did not pass: %1 percent.', mFloor(%game.totalVotesFor/(ClientGroup.getCount() - $HostGameBotCount - %game.totalVotesNone) * 100));
		ResetVOall(%game);
		//called so the game can end with a failed timevote for voteovertime
		DefaultGame::checkTimeLimit(%game, %forced);
	  }
   }

   //if the time limit was actually changed...
   if(%cause !$= "")
   {
      logEcho("time limit set to " @ %display SPC %cause);

      //if the match has been started, reset the end of match countdown
      if($matchStarted)
      {
         //schedule the end of match countdown
         %elapsedTimeMS = getSimTime() - $missionStartTime;
         %curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) - %elapsedTimeMS;
         CancelEndCountdown();
	 
	 if ( %newLimit != 999 )
	   {
	     EndCountdown(%curTimeLeftMS);
	     cancel( %game.timeSync );
	     %game.checkTimeLimit( true );
	   }
	 else
	   {
	     cancel(%game.timeSync);
	   }
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
      messageAll('MsgAdminForce', '\c2The admin has passed the vote.' );
   }
}

function DefaultGame::stopRunningVote(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
   if(%admin && Game.scheduleVote !$= "")
   {
      stopCurrentVote();
      messageAll('MsgAdminForce', '\c2The Admin stopped the vote.~wfx/misc/bounty_completed.wav');
   }
}

function adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4)
{
   // this function handle only admin votes
   if(%client.isAdmin && %client != %arg1)
   {
      if(Game.scheduleVote !$= "" && Game.voteType $= %typeName) 
      {
         messageAll('closeVoteHud', "");
         cancel(Game.scheduleVote);
         Game.scheduleVote = "";
         Game.scheduleVoteArgs = "";
      }
      Game.evalVote(%typeName, true, %arg1, %arg2, %arg3, %arg4);
   }
}

};

function PizzaTriconPopup(%client, %key, %text, %function, %number)
{
   if ( %client.pizza )
   {
      messageClient( %client, 'MsgPlayerPopupItem', "", %key, %function, "",  %text, %number );
      return;
   }
   if ( %client.tricon )
   {
      messageClient( %client, 'MsgPlayerPopupItem', "", %key, %function, "", %text, 10000+%number );
      return;
   }
}

function TriconWrapper(%client, %target, %function)
{
   switch(%function)
   {
      case 10016:
         serverCmdWhois( %client, %target );
         return 1;

      case 10017:
         serverCmdAddToBanList( %client, %target );
         return 1;

      case 10019:
         serverCmdSuperAdminPlayer( %client, %target );
         return 1;
   }
   return 0;
}

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
   cancel(Game.scheduleVote);
   Game.totalVotesFor = ClientGroup.getCount() - $HostGameBotCount;
   Game.totalVotesAgainst = 0;
   Game.evalVote(Game.scheduleVoteArgs[typeName], false, Game.scheduleVoteArgs[arg1], Game.scheduleVoteArgs[arg2], Game.scheduleVoteArgs[arg3], Game.scheduleVoteArgs[arg4]);
   Game.scheduleVote = "";
   Game.scheduleVoteArgs = "";
   Game.kickClient = "";
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
   {
      %cl = ClientGroup.getObject(%idx);
      messageClient(%cl, 'closeVoteHud', "");
      if(%cl.team != 0)
         clearBottomPrint(%cl);
   }
   clearVotes();
   //Stop vote chimes
   for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
   {
      if(isEventPending(Game.voteReminder[%i]))
         cancel(Game.voteReminder[%i]);
      Game.voteReminder[%i] = "";
   }
}

// stopCurrentVote()
// Info: stop a vote that is still running
function stopCurrentVote()
{
   cancel(Game.scheduleVote);
   Game.scheduleVote = "";
   Game.kickClient = "";
   Game.scheduleVoteArgs = "";
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
   {
      %cl = ClientGroup.getObject(%idx);
      messageClient(%cl, 'closeVoteHud', "");
      if(%cl.team != 0)
         clearBottomPrint(%cl);
   }
   clearVotes();
   //Stop vote chimes
   for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
   {
      if(isEventPending(Game.voteReminder[%i]))
         cancel(Game.voteReminder[%i]);
      Game.voteReminder[%i] = "";
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

// Prevent package from being activated if it is already
if (!isActivePackage(ExtraVoteMenu))
    activatePackage(ExtraVoteMenu);