// These have been secured against all those wanna-be-hackers. 
$VoteMessage["VoteAdminPlayer"] = "Admin Player";
$VoteMessage["VoteKickPlayer"] = "Kick Player";
$VoteMessage["BanPlayer"] = "Ban Player";
$VoteMessage["VoteChangeMission"] = "change the mission to";
$VoteMessage["VoteTeamDamage", 0] = "enable team damage";
$VoteMessage["VoteTeamDamage", 1] = "disable team damage";
$VoteMessage["VoteTournamentMode"] = "change the server to";
$VoteMessage["VoteFFAMode"] = "change the server to";
$VoteMessage["VoteChangeTimeLimit"] = "change the time limit to";
$VoteMessage["VoteMatchStart"] = "start the match";
$VoteMessage["VoteGreedMode", 0] = "enable Hoard Mode";
$VoteMessage["VoteGreedMode", 1] = "disable Hoard Mode";
$VoteMessage["VoteHoardMode", 0] = "enable Greed Mode";
$VoteMessage["VoteHoardMode", 1] = "disable Greed Mode";
// z0dd - ZOD, 5/13/03. Added vote Random, Fair teams and armor limiting
$VoteMessage["VoteRandomTeams", 0] = "enable random teams";
$VoteMessage["VoteRandomTeams", 1] = "disable random teams";
$VoteMessage["VoteFairTeams", 0] = "enable fair teams";
$VoteMessage["VoteFairTeams", 1] = "disable fair teams";
$VoteMessage["VoteArmorLimits", 0] = "enable armor limiting";
$VoteMessage["VoteArmorLimits", 1] = "disable armor limiting";
$VoteMessage["VoteAntiTurtleTime"] = "change the anti turtle time to";
$VoteMessage["VoteArmorClass"] = "change the armor class to";
$VoteMessage["VoteClearServer"] = "clear server for match";
$VoteMessage["VoteSkipMission"] = "skip the mission";

function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote)
{
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here

   // haha - who gets the last laugh... No admin for you!
   if( %typeName $= "VoteAdminPlayer" && !$Host::allowAdminPlayerVotes )
      if( !%client.isSuperAdmin ) // z0dd - ZOD, 5/12/02. Allow Supers to do whatever the hell they want
         return;

   %typePass = true;

   // if not a valid vote, turn back.
   // z0dd - ZOD, 5/13/03. Added vote Random, Fair teams, armor limting, Anti-Turtle and Armor Class
   if($VoteMessage[%typeName] $= "" && (%typeName !$= "VoteTeamDamage" && %typeName !$= "VoteHoardMode" 
                                      && %typeName !$= "VoteGreedMode" && %typeName !$= "VoteRandomTeams" 
                                      && %typeName !$= "VoteFairTeams" && %typeName !$= "VoteArmorLimits"
                                      && %typeName !$= "VoteAntiTurtleTime" && %typeName !$= "VoteArmorClass"
                                      && %typeName !$= "VoteClearServer" && %typeName !$= "VoteSkipMission")) {
      %typePass = false;
   }

   if(( $VoteMessage[ %typeName, $TeamDamage ] $= "" && %typeName $= "VoteTeamDamage" ))
      %typePass = false;

   if( !%typePass )
      return; // -> bye ;)

   // ------------------------------------
   // z0dd - ZOD, 10/03/02. Fixed ban code
   //if( %typeName $= "BanPlayer" )
   //   if( !%client.isSuperAdmin )
   //      return; // -> bye ;)
   if( %typeName $= "BanPlayer" )
   {
      if( !%client.isSuperAdmin )
      {
         return; // -> bye ;)
      }
      else
      {
         ban( %arg1, %client );
         return;
      }
   }
   // ------------------------------------

   %isAdmin = ( %client.isAdmin || %client.isSuperAdmin );

   // z0dd - ZOD, 5/19/03. Get the Admins client.
   if(%isAdmin)
      $AdminCl = %client;

   // keep these under the server's control. I win.
   if( !%playerVote )
      %actionMsg = $VoteMessage[ %typeName ];
   else if( %typeName $= "VoteTeamDamage" || %typeName $= "VoteGreedMode" || %typeName $= "VoteHoardMode" )
      %actionMsg = $VoteMessage[ %typeName, $TeamDamage ];
   else
      %actionMsg = $VoteMessage[ %typeName ];
   
   if( !%client.canVote && !%isAdmin )
      return;
   
   if ( ( !%isAdmin || ( %arg1.isAdmin && ( %client != %arg1 ) ) ) &&     // z0dd - ZOD, 4/7/02. Allow SuperAdmins to kick Admins
        !( ( %typeName $= "VoteKickPlayer" ) && %client.isSuperAdmin ) )  // z0dd - ZOD, 4/7/02. Allow SuperAdmins to kick Admins
   {
      %teamSpecific = false;
      %gender = (%client.sex $= "Male" ? 'he' : 'she');
      if ( Game.scheduleVote $= "" ) 
      {
         %clientsVoting = 0;

         //send a message to everyone about the vote...
       if ( %playerVote )
	 {   
            %teamSpecific = ( %typeName $= "VoteKickPlayer" ) && ( Game.numTeams > 1 );
            %kickerIsObs = %client.team == 0;
            %kickeeIsObs = %arg1.team == 0;
            %sameTeam = %client.team == %arg1.team;
            
            if( %kickeeIsObs )
            {
               %teamSpecific = false;
               %sameTeam = false;  
            }
            if(( !%sameTeam && %teamSpecific) && %typeName !$= "VoteAdminPlayer")
            {
               messageClient(%client, '', '\c2Player votes must be team based.');
               return;
            }

            // kicking is team specific
            if( %typeName $= "VoteKickPlayer" )
            {
               if(%arg1.isSuperAdmin)
               {
                  messageClient(%client, '', '\c2You can not %1 %2, %3 is a Super Admin!', %actionMsg, %arg1.name, %gender);
                  return;
               }
               
               Game.kickClient = %arg1;
               Game.kickClientName = %arg1.name;
               Game.kickGuid = %arg1.guid;
               Game.kickTeam = %arg1.team;

               if(%teamSpecific)
               {   
                  for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ ) 
                  {
                     %cl = ClientGroup.getObject( %idx );
            
                     if (%cl.team == %client.team && !%cl.isAIControlled())
                     {   
                        messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1.name); 
                        %clientsVoting++;
                     }
                  }
               }
               else
               {
                  for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
                  {
                     %cl = ClientGroup.getObject( %idx );
                     if ( !%cl.isAIControlled() )
                     {
                        messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1.name); 
                        %clientsVoting++;
                     }
                  }
               }
            }
            else
            {
               for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
               {
                  %cl = ClientGroup.getObject( %idx );
                  if ( !%cl.isAIControlled() )
                  {
                     messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1.name); 
                     %clientsVoting++;
                  }
               }
            }   
         }
         else if ( %typeName $= "VoteChangeMission" )
         {
            for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
            {
               %cl = ClientGroup.getObject( %idx );
               if ( !%cl.isAIControlled() )
               {
                  messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3 (%4).', %client.name, %actionMsg, %arg1, %arg2 );
                  %clientsVoting++;
               }
            }
         }
         else if (%arg1 !$= 0)
         {
		if (%arg2 !$= 0)
            {   
               if(%typeName $= "VoteTournamentMode")
               {   
                  %admin = getAdmin();
                  if(%admin > 0)
                  {
                     for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
                     {
                        %cl = ClientGroup.getObject( %idx );
                        if ( !%cl.isAIControlled() )
                        {
                           messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 Tournament Mode (%3).', %client.name, %actionMsg, %arg1); 
                           %clientsVoting++;
                        }
                     }
                  }
                  else
                  {   
                     messageClient( %client, 'clientMsg', 'There must be a server admin to play in Tournament Mode.');
                     return; 
                  }
               }
               else
               {
                  for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
                  {
                     %cl = ClientGroup.getObject( %idx );
                     if ( !%cl.isAIControlled() )
                     {
                        messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3 %4.', %client.name, %actionMsg, %arg1, %arg2); 
                        %clientsVoting++;
                     }
                  }
               }
            }
            else
            {
               for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
               {
                  %cl = ClientGroup.getObject( %idx );
                  if ( !%cl.isAIControlled() )
                  {
		               messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1);
                     %clientsVoting++;
                  }
               }
            }
         }
         else
         {
            for ( %idx = 0; %idx < ClientGroup.getCount(); %idx++ )
            {
               %cl = ClientGroup.getObject( %idx );
               if ( !%cl.isAIControlled() )
               {
	               messageClient( %cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg); 
                  %clientsVoting++;
               }
            }
         }

         // open the vote hud for all clients that will participate in this vote
         if(%teamSpecific)
         {
            for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
            {
               %cl = ClientGroup.getObject( %clientIndex );
      
               if(%cl.team == %client.team && !%cl.isAIControlled())
                  messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));    
            }
         }
         else
         {
            for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
            {
               %cl = ClientGroup.getObject( %clientIndex );
               if ( !%cl.isAIControlled() )
                  messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));    
            }
         }
         clearVotes();
         Game.voteType = %typeName;
         Game.scheduleVote = schedule( ($Host::VoteTime * 1000), 0, "calcVotes", %typeName, %arg1, %arg2, %arg3, %arg4 );
         %client.vote = true;
         messageAll('addYesVote', "");
         
         if(!%client.team == 0)
            clearBottomPrint(%client);
      }
      else
         messageClient( %client, 'voteAlreadyRunning', '\c2A vote is already in progress.' );	                       
   }
   else 
   {
      if ( Game.scheduleVote !$= "" && Game.voteType $= %typeName ) 
      {
         messageAll('closeVoteHud', "");
         cancel(Game.scheduleVote);
         Game.scheduleVote = "";
      }
      
      // if this is a superAdmin, don't kick or ban
      if(%arg1 != %client)
      {   
         if(!%arg1.isSuperAdmin)
         {
            // Set up kick/ban values:
            if ( %typeName $= "VoteBanPlayer" || %typeName $= "VoteKickPlayer" )
            {
               Game.kickClient = %arg1;
               Game.kickClientName = %arg1.name;
               Game.kickGuid = %arg1.guid;
               Game.kickTeam = %arg1.team;
            }
            
            //Tinman - PURE servers can't call "eval"
            //Mark - True, but neither SHOULD a normal server
            //     - thanks Ian Hardingham
            //if (!isPureServer())
            //   eval( "Game." @ %typeName @ "(true,\"" @ %arg1 @ "\",\"" @ %arg2 @ "\",\"" @ %arg3 @ "\",\"" @ %arg4 @ "\");" );
            //else
            Game.evalVote(%typeName, true, %arg1, %arg2, %arg3, %arg4);
         }
         else
            messageClient(%client, '', '\c2You can not %1 %2, %3 is a Super Admin!', %actionMsg, %arg1.name, %gender);
      }      
   }
   %client.canVote = false;
   %client.rescheduleVote = schedule( ($Host::voteSpread * 1000) + ($Host::voteTime * 1000) , 0, "resetVotePrivs", %client );        
}

function resetVotePrivs( %client )
{
   //messageClient( %client, '', 'You may now start a new vote.');
   %client.canVote = true;
   %client.rescheduleVote = "";
}

function serverCmdSetPlayerVote(%client, %vote)
{
   // players can only vote once
   if( %client.vote $= "" )
   {
      %client.vote = %vote;
      if(%client.vote == 1)
         messageAll('addYesVote', "");
      else
         messageAll('addNoVote', "");

      commandToClient(%client, 'voteSubmitted', %vote);
   }
}

function calcVotes(%typeName, %arg1, %arg2, %arg3, %arg4)
{
   if(%typeName $= "voteMatchStart")
      if($MatchStarted || $countdownStarted)
         return;
   
   for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
   {
      %cl = ClientGroup.getObject( %clientIndex );
      messageClient(%cl, 'closeVoteHud', "");
      
      if ( %cl.vote !$= "" ) 
      {
         if ( %cl.vote ) 
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
   //Tinman - PURE servers can't call "eval"
   //Mark - True, but neither SHOULD a normal server
   //     - thanks Ian Hardingham
   //if (!isPureServer())
   //   eval( "Game." @ %typeName @ "(false,\"" @ %arg1 @ "\",\"" @ %arg2 @ "\",\"" @ %arg3 @ "\",\"" @ %arg4 @ "\");" );
   //else
      Game.evalVote(%typeName, false, %arg1, %arg2, %arg3, %arg4);
   Game.scheduleVote = "";
   Game.kickClient = "";
   clearVotes();
}

function clearVotes()
{
   for(%clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++)
   {   
      %client = ClientGroup.getObject(%clientIndex);
      %client.vote = "";
      messageClient(%client, 'clearVoteHud', ""); 
   }
   
   for(%team = 1; %team < 5; %team++) 
   {
      Game.votesFor[%team] = 0;
      Game.votesAgainst[%team] = 0;
      Game.votesNone[%team] = 0;
      Game.totalVotesFor = 0;
      Game.totalVotesAgainst = 0;
      Game.totalVotesNone = 0;
   }
}

// Tournament mode Stuff-----------------------------------

function setModeFFA( %mission, %missionType )
{
   if( $Host::TournamentMode )
   {
      $Host::TournamentMode = false;
      
      if( isObject( Game ) )
         Game.gameOver();
      
      loadMission(%mission, %missionType, false);   
   }
}

function setModeTournament( %mission, %missionType )
{
   if( !$Host::TournamentMode )
   {
      $Host::TournamentMode = true;
      
      if( isObject( Game ) )
         Game.gameOver();
         
      loadMission(%mission, %missionType, false);   
   }
}

// z0dd - ZOD, 5/23/03. New function, vote for CTF Anti-Turtle time setting
function serverCmdGetAntiTurtleTimeList( %client, %key )
{
   if ( isObject( Game ) )
      Game.sendAntiTurtleTimeList( %client, %key );
}

//------------------------------------------------------------------
// z0dd - ZOD- Founder, 7/13/03. From Mechina Mod, Admin punishments etc.

function serverCmdTogglePlayerGag(%client, %who)
{
   if(%client.isAdmin || %client.isSuperAdmin)
   {
      if(!%who.isGagged && !%who.isSuperAdmin)
      {
         %who.isGagged = true;
         messageClient(%client, 'MsgAdmin', 'You have Gagged %1.', %who.name);
         messageAllExcept(%who, -1, 'MsgAdminForce', '%1 has been Gagged by %2 for talking too much crap.', %who.name, %client.name);
         messageClient(%who, 'MsgAdminAction', 'You have Been Gagged by %1, quit talking trash and play.', %client.name);
         adminLog(%client, " gagged " @ %who.nameBase);
      }
      else if (%who.isGagged)
      {
         %who.isGagged = false;
         messageClient(%client, 'MsgAdmin', 'You have UnGagged %1.', %who.name);
         messageAllExcept(%who, -1, 'MsgAdminAction', '%1 has been UnGagged by %2.', %who.name, %client.name);
         messageClient(%who, 'MsgAdminAction', 'You have Been UnGagged by %1, quit talking trash and play.', %client.name);
         adminLog(%client, " ungagged " @ %who.nameBase);
      }
   }
   // else
   //    messageClient(%client, 'MsgError', '\c2Only Super Admins can use this command.');
}

function serverCmdTogglePlayerFreeze(%client, %who)
{
   if(%client.isSuperAdmin)
   {
      if(!$MatchStarted)
      {
         messageClient(%client, 'MsgError', 'You must wait for the match to start!');
         return;
      }
      if (!%who.isFroze && !%who.isSuperAdmin)
      {
         if(!isobject(%who.player))
         {
            messageClient(%client, 'MsgError', 'You must wait for the player to spawn!');	
            return;
         }
         %who.isFroze = true;
         %who.player.setvelocity("0 0 0");
         %who.player.setMoveState(true);
         %who.player.invincible = true;
         messageClient(%client, 'MsgAdmin', 'You have Frozen %1.', %who.name);
         messageAllExcept(%who, -1, 'MsgAdminForce', '%1 has been Frozen by %2 for being a Llama.', %who.name, %client.name);
         messageClient(%who, 'MsgAdminAction', 'You have Been Frozen by %1, Think about what you have been doing.', %client.name);
         adminLog(%client, " froze " @ %who.nameBase);
      }
      else if (%who.isFroze)
      {
         %who.isFroze = false;
         %who.player.setMoveState(false);
         %who.player.invincible = false;
         messageClient(%client, 'MsgAdmin', 'You have de-iced %1.', %who.name);
         messageAllExcept(%who, -1, 'MsgAdminForce', '%1 has been Un Frozen by %2.', %who.name, %client.name);
         messageClient(%who, 'MsgAdminAction', 'You have Been de-Iced by %1, now behave.', %client.name);
         adminLog(%client, " unfroze " @ %who.nameBase);
      }
   }
   else
      messageClient(%client, 'MsgError', '\c2Only Super Admins can use this command.');
}

function serverCmdBootToTheRear(%client, %who)
{
   if(%client.isSuperAdmin)
   {
      if(!$MatchStarted)
      {
         messageClient(%client, 'MsgError', 'You must wait for the match to start!');
         return;
      }
      if(isObject(%who.player) && !%who.isSuperAdmin)
      {
         %time = getTime();
         %obj = %who.player;
         %vec = "0 0 10";
         %obj.applyImpulse(%obj.position, VectorScale(%vec, %obj.getDataBlock().mass*20));
         messageAllExcept(%who, -1, 'MsgAdminForce', '%1 has been given a boot to the rear by %2.', %who.name, %client.name);
         messageClient(%who, 'MsgAdminAction', 'You have Been given a boot to the ass by %1, now behave.', %client.name);
         adminLog(%client, " gave " @ %who.nameBase @ " a boot to the rear");
      }
      else
      {
         messageClient(%client, 'MsgError', 'You must wait for the player to spawn!');
      }
   }
   else
      messageClient(%client, 'MsgError', '\c2Only Super Admins can use this command.');
}

function serverCmdExplodePlayer(%client, %who)
{
   if(%client.isSuperAdmin)
   {
      if(!$MatchStarted)
      {
         messageClient(%client, 'MsgError', 'You must wait for the match to start!');
         return;
      }
      if(isObject(%who.player) && !%who.isSuperAdmin)
      {
         %who.player.blowup();
         %who.player.scriptKill(0);
         messageAllExcept(%who, -1, 'MsgAdminForce', '%1 found some explosives in his pants planted by %2.', %who.name, %client.name);
         messageClient(%who, 'MsgAdminAction', 'You have Been dissasembled for inspection by the Super Admin %1, now behave.', %client.name);
         adminLog(%client, " exploded " @ %who.nameBase);
      }
      else
      {
         messageClient(%client, 'MsgError', 'You must wait for the player to spawn!');
      }	
   }
   else
      messageClient(%client, 'MsgError', '\c2Only Super Admins can use this command.');
}

// z0dd - ZOD - MeBad, 7/13/03. Send client information.
function ServerCmdPrintClientInfo(%client, %targetClient)
{
   if (%client.isAdmin)
   {
      if ((!%targetClient.isSuperAdmin) && (%client.isSuperAdmin))
      {
         %wonid = getField( %targetClient.getAuthInfo(), 3);
         %ip = %targetClient.getAddress();
      }
      else
      {
         %wonid = "PROTECTED";
         %ip = "PROTECTED";
      }
      MessageClient(%client, '', '---------------------------------------------------------------');
      MessageClient(%client, 'ClientInfo', "\c3Client Info...\n" @ 
                                           "ClientName: \c2" @ %targetClient.nameBase @ "\n" @
                                           "\c3Wonid: \c2" @ %wonid @ "\n" @
                                           "\c3IP: \c2" @ %ip @ "\n\n" @
                                           "\c3TeamKills:\c2 " @ %targetClient.teamkills @ "\n" @
                                           "\c3BK (BaseKills): \c2" @ %targetClient.tkDestroys @ "\n" @
                                           "\c3Suicides:\c2 " @ %targetClient.suicides @ "\n");
      MessageClient(%client, '', '---------------------------------------------------------------');
   }
   else
      messageClient(%client, 'MsgError', '\c2Only Admins can use this command.');
}
