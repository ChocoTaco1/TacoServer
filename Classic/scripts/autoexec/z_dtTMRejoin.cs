// auto reteam clients that have drop
//script by: DarkTiger
//version 1.1
function tmMirrorTeam(%client){
   if (!$Host::TournamentMode){
      return;
   }

   if (%client.team <= 0){
      return;
   }

   %guid = %client.guid;
   if (%guid $= "" || %guid == 0){
      return;
   }

   $TourneyRejoinTeam[%guid] = %client.team;
   $TourneyRejoinSeq[%guid]  = $missionSequence;
   %client.recDrop = 1;
   echo("TourneyRejoin: saved team info for " @ %client.nameBase
           @ " (GUID:" @ %guid @ ") team=" @ %client.team
           @ " seq=" @ $missionSequence);
}

function tmRejoin(%game, %client){
   if (!$Host::TournamentMode || !$missionRunning){
      return;
   }

   %guid = %client.guid;
   if (%guid $= "" || %guid == 0){
      return;
   }

   if ($TourneyRejoinSeq[%guid] $= ""){
         return;
   }

   if ($TourneyRejoinSeq[%guid] != $missionSequence){
      $TourneyRejoinTeam[%guid] = "";
      $TourneyRejoinSeq[%guid]  = "";
      return;
   }

   %savedTeam = $TourneyRejoinTeam[%guid];

   // Consume the record so a second reconnect goes through normal pick-team flow
   $TourneyRejoinTeam[%guid] = "";
   $TourneyRejoinSeq[%guid]  = "";

   echo("TourneyRejoin: restoring" SPC %client.nameBase  SPC "(GUID:" @ %guid @ ") to team " @ %savedTeam);


   %game.clientChangeTeam(%client, %savedTeam,  1, true);

   //%game.clientJoinTeam( %client, %savedTeam, false );

   if (!$MatchStarted && !$countdownStarted){
      clearBottomPrint(%client);
      %client.observerMode = "pregame";
      %client.notReady     = true;
      centerprint(%client, "\nPress FIRE when ready.", 0, 3);
      messageClient(%client, 'MsgClient',
         '\c2You have been re-teamed to %1 after reconnecting. Press FIRE when ready.',
         %game.getTeamName(%savedTeam));
   }
   else{
      clearBottomPrint(%client);
      %client.notReady = false;
      commandToClient(%client, 'setHudMode', 'Standard');
      messageClient(%client, 'MsgClient',
         '\c2You have been re-teamed to %1 after reconnecting.',
         %game.getTeamName(%savedTeam));
   }
   %client.isRejoin = 1;
   messageAllExcept(%client, -1, 'MsgClient',
      '\c2%1 reconnected and has been restored to %2.',
      %client.name, %game.getTeamName(%savedTeam));
}

package TourneyRejoinPackage{
      // Mirror team to global whenever a client joins a team
   function DefaultGame::clientJoinTeam(%game, %client, %team, %respawn){
      Parent::clientJoinTeam(%game, %client, %team, %respawn);
      tmMirrorTeam(%client);
   }
   // Mirror team to global whenever a client changes team
   function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs, %respawned){
      Parent::clientChangeTeam(%game, %client, %team, %fromObs, %respawned);
      tmMirrorTeam(%client);
   }

   function DefaultGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      schedule(32, 0, "tmRejoin", %game, %client);
      %client.isRejoin = 0;
   }
   function serverCmdPlayContentSet( %client ){
      if(!%client.isRejoin){// block the join team ui if they are already ready on a team
         parent::serverCmdPlayContentSet(%client);
      }
      else{
         %client.isRejoin = 0;
      }
   }
};

if (!isActivePackage(TourneyRejoinPackage)){
   activatePackage(TourneyRejoinPackage);
}
