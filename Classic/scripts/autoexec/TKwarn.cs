//exec("scripts/autoexec/TKwarn.cs");

package TKwarn
{


// From Evo
function DefaultGame::testTeamKill(%game, %victimID, %killerID)
{
   %tk = Parent::testTeamKill(%game, %victimID, %killerID);
   if(!%tk)		
      return false; // is not a tk
  
   if($Host::TournamentMode || %killerID.isAdmin || %killerID.isAIcontrolled() || %victimID.isAIcontrolled())
      return true;

   // Ignore this map
   if($CurrentMission $= "Mac_FlagArena")
	  return true;
   
   // warn the player
   if((%killerID.teamkills == $Host::TKWarn1 - 1) && $Host::TKWarn1 != 0)
	  centerprint(%killerID, "You are recieving this warning for inappropriate teamkilling.\nBehave or you will be kicked.", 10, 2);
   // warn the player of his imminent kick
   else if((%killerID.teamkills == $Host::TKWarn2 - 1) && $Host::TKWarn2 != 0)
	  centerprint(%killerID, "You are recieving this second warning for inappropriate teamkilling.\nBehave or you will be kicked.", 10, 2);
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