//AFK Timeout
//Script BY: DarkTiger
//
//Client specific to save on schedules
//Add clients who are normally AFK
//0 minutes disables

$dtVar::AFKtime = 60000 * 2;//if player is afk specific amount of time in minutes, force them into observer
$dtVar::AFKloop = 1000 * 30;//loop check timer

////////////////////////////////////////////////////////////////////////////////

package afkScript
{
   
function GameConnection::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch)
{
	Parent::onConnect( %client, %name, %raceGender, %skin, %voice, %voicePitch );
	
	if($dtVar::AFKtime > 0) //0 minutes disables
	{
		%guid = %client.guid;
	
		//Add clients here
		%AFKWatchList1 = "";
		%AFKWatchList2 = "";
		%AFKWatchList3 = "";
		%AFKWatchList4 = "";
		
		if(%guid $= %AFKWatchList1 || %guid $= %AFKWatchList2 || %guid $= %AFKWatchList3 || %guid $= %AFKWatchList4 )
		{
			if(%guid $= "")
				return;
			
			%client.afkLoopCheck();// starts it
		}
	}
}

};

// Prevent package from being activated if it is already
if(!isActivePackage(afkScript))
    activatePackage(afkScript);


////////////////////////////////////////////////////////////////////////////////


function GameConnection::afkLoopCheck(%this)
{
	if(isObject(%this) && !%this.isAiControlled())// make sure client is still there other wise stop
		%this.schedule($dtVar::AFKloop,"afkLoopCheck");
	else
		return;
      
	//echo(%this.team);
	  
	if(isObject(%this.player) && %this.player.getState() !$= "Dead" && $matchStarted && %this.team !$= 0) //Added no check if observer
		AFKChk(%this);
}

function AFKChk(%client)
{
   if(%client.player.curTransform  $= %client.player.getTransform())//checks to see if there position and rotation are the same. 
   {
      //echo("is not moving");
      // error(%client.player.curTransform  SPC %client.player.getTransform());
      %client.player.afkTimer += $dtVar::AFKloop;
      if(%client.player.afkTimer >= $dtVar::AFKtime)
      {
         Game.AFKForceObserver(%client);
         return;
      }
   }
   else
   {
      //echo("is moving");
      %client.player.afkTimer = 0;//reset if moving
   }
   
   %client.player.curTransform = %client.player.getTransform();//save current transform 
}

function DefaultGame::AFKForceObserver(%game, %client)
{
   //make sure we have a valid client...
   if (%client <= 0)
      return;
   
   // first kill this player
   if(%client.player)
      %client.player.scriptKill(0);
   
   if( %client.respawnTimer )
      cancel(%client.respawnTimer);
   
   %client.respawnTimer = "";
   
   // remove them from the team rank array
   %game.removeFromTeamRankArray(%client);
   
   // place them in observer mode
   %client.lastObserverSpawn = -1;
   %client.observerStartTime = getSimTime();

   
   %client.camera.getDataBlock().setMode( %client.camera, "observerFly" );
   messageClient(%client, 'MsgClientJoinTeam', '\c2You have been placed into observer mode due to inactivity.', %client.name, %game.getTeamName(0), %client, 0 );
   logEcho(%client.nameBase@" (cl "@%client@") was forced into observer mode due to inactivity");
   %client.lastTeam = %client.team;
   
   if($Host::TournamentMode)
   {
      if(!$matchStarted)
      {
         if(%client.camera.Mode $= "pickingTeam")
         {
            commandToClient( %client, 'processPickTeam');
            clearBottomPrint( %client );
         }
         else
         {
            clearCenterPrint(%client);
            %client.notReady = true;
         }
      }
   }
   
   // switch client to team 0 (observer)
   %client.team = 0;
   %client.player.team = 0;
   setTargetSensorGroup( %client.target, %client.team );
   %client.setSensorGroup( %client.team );
   
   // set their control to the obs. cam
   %client.setControlObject( %client.camera );
   commandToClient(%client, 'setHudMode', 'Observer');
   
   // display the hud
   //displayObserverHud(%client, 0);
   updateObserverFlyHud(%client);
   
   messageAllExcept(%client, -1, 'MsgClientJoinTeam', '\c2%1 has been placed into observer mode due to inactivity.', %client.name, %game.getTeamName(0), %client, 0 );
   
   updateCanListenState( %client );
   
   // call the onEvent for this game type
   %game.onClientEnterObserverMode(%client);  //Bounty uses this to remove this client from others' hit lists
}