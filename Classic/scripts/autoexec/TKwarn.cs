//exec("scripts/autoexec/TKwarn.cs");

package TKwarn
{

// From Evo
function DefaultGame::testTeamKill(%game, %victimID, %killerID)
{ 
   if(!$countdownStarted && !$MatchStarted)
      return;
   
   %tk = Parent::testTeamKill(%game, %victimID, %killerID);
   if(!%tk)		
      return false; // is not a tk

   // No Bots
   if(%killerID.isAIcontrolled() || %victimID.isAIcontrolled())
     return true;

   // Log TeamKill
   teamkillLog(%victimID, %killerID);
  
   // No Admins
   if(%killerID.isAdmin)
      return true;

   // Ignore this map
   if($CurrentMission $= "Mac_FlagArena" || $CurrentMission $= "Machineeggs" || $CurrentMission $= "DMP_SimpleFlagArena")
	  return true;
   
   // warn the player of the imminent kick vote
   if((%killerID.teamkills == $Host::TKWarn1 - 1) && $Host::TKWarn1 != 0)
	  centerprint(%killerID, "You are receiving this warning for inappropriate teamkilling.\nBehave or a vote to kick will be started.", 10, 2);
   // warn the player of his imminent kick
   else if((%killerID.teamkills == $Host::TKWarn2 - 1) && $Host::TKWarn2 != 0)
   {
     TKvote("VoteKickPlayer", %killerID);
	  centerprint(%killerID, "You are receiving this second warning for inappropriate teamkilling.\nBehave or you will be kicked.", 10, 2);
   }
   // kick the player
   else if((%killerID.teamkills >= $Host::TKMax - 1) && $Host::TKMax != 0)
   {
      Game.kickClientName = %killerID.name;
      TKkick(%killerID, true, %killerID.guid);
      adminLog( %killerID, " was autokicked for teamkilling." );
   }
   return true;
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TKwarn))
    activatePackage(TKwarn);

// we pass the guid as well, in case this guy leaves the server.
function TKkick( %client, %admin, %guid )
{
   messageAll( 'MsgAdminForce', '\c2%1 has been autokicked for teamkilling.', %client.name ); // z0dd - ZOD, 7/13/03. Tell who kicked
   
   messageClient(%client, 'onClientKicked', "");
   messageAllExcept( %client, -1, 'MsgClientDrop', "", Game.kickClientName, %client );

   if( %client.isAIControlled() )
   {
      if($Host::ClassicCanKickBots || %admin.isAdmin)
      {
         if(!$Host::ClassicBalancedBots)
         {
            $HostGameBotCount--;
            %client.drop();
         }
      }
   }
   else
   {
      if( $playingOnline ) // won games
      {
         %count = ClientGroup.getCount();
         %found = false;
         for( %i = 0; %i < %count; %i++ ) // see if this guy is still here...
         {
            %cl = ClientGroup.getObject( %i );
	      if( %cl.guid == %guid )
            {
	         %found = true; 

	         // kill and delete this client, their done in this server.
	         if( isObject( %cl.player ) )
	            %cl.player.scriptKill(0);
            
               if ( isObject( %cl ) )
               {
                  %client.setDisconnectReason( "You have been kicked out of the game for teamkilling." ); // z0dd - ZOD, 7/13/03. Tell who kicked
	              %cl.schedule(700, "delete");
               }
			 // ban by IP as well
	         BanList::add( %guid, %client.getAddress(), $Host::KickBanTime );
            }   
	   }
         if( !%found )
	      BanList::add( %guid, "0", $Host::KickBanTime ); // keep this guy out for a while since he left. 
      }
      else // lan games
      {
         // kill and delete this client
         if( isObject( %client.player ) )
            %client.player.scriptKill(0);
         
            if ( isObject( %client ) )
            {
               %client.setDisconnectReason( "You have been kicked out of the game for teamkilling." );
               %client.schedule(700, "delete");
            }
         BanList::add( 0, %client.getAddress(), $Host::KickBanTime );
      }
   }
}

// From Evo
// Info: Auto start a new vote
function TKvote(%typeName, %arg1, %arg2, %arg3, %arg4)
{  
   // works only for kicking players
   if(%typeName !$= "VoteKickPlayer")
      return;

   // only works for FFA mode
   if($Host::TournamentMode)
      return;
  
   // a vote is already running, cancel it
   if(Game.scheduleVote !$= "")
      stopCurrentVote();
  
   %clientsVoting = 0;
  
   // admins can't be kicked
   if(%arg1.isAdmin)
      return;
   
   Game.kickClient = %arg1;
   Game.kickClientName = %arg1.name;
   Game.kickGuid = %arg1.guid;
   Game.kickTeam = %arg1.team;
   
   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.team == %arg1.team && !%cl.isAIControlled() && %cl !$= %arg1)
      {   
         messageClient(%cl, 'VoteStarted', '\c2Vote initiated to kick the teamkiller %1 with %2 teamkills.', %arg1.name, $Host::TKWarn2);
         %clientsVoting++;
      }
   }
   
   for(%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.team == %arg1.team && !%cl.isAIControlled() && %cl !$= %arg1)
         messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));    
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

   // Log Vote
   voteLog(%client, %typeName, %arg1, %arg2, %arg3, "TeamkillAutovote");

   if($Host::EnableVoteSoundReminders > 0)
   {
		%time = mFloor($Host::VoteTime / ($Host::EnableVoteSoundReminders + 1)) * 1000;
		//echo(%time);
		for(%i = 0; %i < $Host::EnableVoteSoundReminders; %i++)
				Game.voteReminder[%i] = schedule((%time * (%i + 1)), 0, "VoteSound", %game, %typename, %arg1, %arg2);
   }
}