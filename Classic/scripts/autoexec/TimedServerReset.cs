// Timed Server Reset Script
//
// Reset Server after a certain time
// after the last client leaves
//
// Time in Minutes
// $Host::EmptyServerResetTime = 120;
// To control whether the server auto resets when empty
// $Host::EmptyServerReset = 1;

// Changed in evo Server.ovl
//
//function GameConnection::onDrop(%client, %reason)
//{
//   if(isObject(Game))
//      Game.onClientLeaveGame(%client);
//
//   // make sure that tagged string of player name is not used
//   if($CurrentMissionType !$= "SinglePlayer")
//       messageAllExcept(%client, -1, 'MsgClientDrop', '\c1%1 has left the game.', getTaggedString(%client.name), %client);
//   else
//      messageAllExcept(%client, -1, 'MsgClientDrop', "", getTaggedString(%client.name), %client);
//
//   if(isObject(%client.camera))
//       %client.camera.delete();
//
//   // z0dd - ZOD, 6/19/02. Strip the hit sound tags
//   removeTaggedString(%client.playerHitWav);
//   removeTaggedString(%client.vehicleHitWav);
//   removeTaggedString(%client.name);
//   removeTaggedString(%client.voiceTag);
//   removeTaggedString(%client.skin);
//   freeClientTarget(%client);
//
//   echo("CDROP: " @ %client @ " " @ %client.getAddress());
//   $HostGamePlayerCount--;
//
//   // z0dd - ZOD, 5/05/04. Add a bot for every client drop if balanced bots are set
//   if( $Host::BotsEnabled )
//   {
//      if($Host::ClassicBalancedBots)
//      {
//         if(!%client.isAIControlled())
//         {
//            if (serverCanAddBot())
//            {
//               aiConnectMultiple( 1, $Host::MinBotDifficulty, $Host::MaxBotDifficulty, -1 );
//               $HostGameBotCount++;
//            }
//         }
//      }
//   }
//   if($Host::ClassicAutoPWEnabled)
//   {
//      if( ($HostGamePlayerCount < $Host::ClassicAutoPWPlayerCount) &&
//	   (!$Host::TournamentMode || ($Host::TournamentMode && !$Host::EvoAutoPWTourneyNoRemove)))
//	{
//	   AutoPWServer(0);
//	}
//   }
//   if($Host::EvoFullServerPWEnabled)
//   {
//      if($HostGamePlayerCount == ($MaxPlayers - 1))
//	{
//	   FullServerPW(0);
//	}
//   }
//   //if($Host::EvoNoBaseRapeEnabled)
//   //{
//      //if(!$Host::TournamentMode && 
//	  //(EvoPlayersOnTeamCount() < $Host::EvoNoBaseRapeClassicPlayerCount))
//	//{
//	   //$EvoNoBaseRape = 1;
//	//}
//   //}
//   // Reset the server if everyone has left the game
//   
//   if( $HostGamePlayerCount - $HostGameBotCount == 0 && $Host::EmptyServerReset && !$resettingServer && !$LoadingMission && !$TimedServerResetActive && $CurrentMissionType !$= $Host::MissionType )
//   {
//      if($Evo::ETMMode)
//	{
//	   $Evo::ETMMode = false;
//	   ETMreset();
//	}  
//	
//	%resettime = $Host::EmptyServerResetTime * 60000;
//	if(%resettime <= 0) %resettime = 1;
//	schedule(%resettime, 0, "ResetServerTimed");
//	$TimedServerResetActive = true;
//	error(formatTimeString("HH:nn:ss") SPC "Timed Server Reset schedule started..." );
//	
//   }
//}

function ResetServerTimed()
{
	if($HostGamePlayerCount - $HostGameBotCount == 0 && $Host::EmptyServerReset && !$resettingServer && !$LoadingMission && $TimedServerResetActive)
      schedule(10, 0, "resetServerDefaults");
	  // Instead of simply resetting the defaults, reinitialize the
	  // entire server...
	  // ReallyQuit();
	else
		error(formatTimeString("HH:nn:ss") SPC "Timed Server Reset schedule cancelled..." );
	
	$TimedServerResetActive = false;
}