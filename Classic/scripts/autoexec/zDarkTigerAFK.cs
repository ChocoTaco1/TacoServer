// AFK Timeout Script
// Script BY: DarkTiger
// Worked on: ChocoTaco
// If player is afk specific amount of time in minutes, force them into observer

// Enable/Disable entire script
$dtVar::AFKTimeout = 1;
// 60000 * 2 is 2 minutes
// 0 minutes disables
$dtVar::AFKtime = 60000 * 2;
// Run from List Only instead of All clients on the server. 1 is yes, 0 is no
$dtVar::ListOnly = 1;
// Add clients who are normally AFK 
$dtVar::AFKList[$AFKCount++] = "";
$dtVar::AFKList[$AFKCount++] = "";
// Loop Check Timer
// How often do you want a AFKLoop. 1000 * 30 is 30 seconds
$dtVar::AFKloop = 1000 * 30; 

////////////////////////////////////////////////////////////////////////////////

// Set Status Var
if($dtVar::ListOnly)
	$DT_AFKStatus = "IDLE";
else
	$DT_AFKStatus = "ACTIVE";

////////////////////////////////////////////////////////////////////////////////

package DT_AFKPackage
{
	
function CreateServer( %mission, %missionType )
{
	parent::CreateServer( %mission, %missionType );
	
	//Call to start AFKTimeout update
	DT_AFKtimeoutLoop();
	
	// Prevent package from being activated if it is already
	if(!isActivePackage(DT_AFKOverrides) && $dtVar::ListOnly)
		activatePackage(DT_AFKOverrides);
}

};

// Prevent package from being activated if it is already
if(!isActivePackage(DT_AFKPackage) && $dtVar::AFKTimeout)
	activatePackage(DT_AFKPackage);

package DT_AFKOverrides
{

function GameConnection::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch)
{
	Parent::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch);
   
	DT_AFKStatusConnect(%client);
}

function GameConnection::onDrop(%client, %reason)
{
	Parent::onDrop(%client, %reason);
	
	DT_AFKStatusDrop(%client);
}

};

////////////////////////////////////////////////////////////////////////////////

function DT_AFKStatusConnect(%client)
{
	for(%x = 1; %x <= $AFKCount; %x++) 
	{
		%guid = $dtVar::AFKList[%x];
		if(%client.guid $= %guid && %guid !$= "")
		{
			$DT_AFKStatus = "ACTIVE";
			$DT_AFKListCount++;
		}
	}
}

function DT_AFKStatusDrop(%client)
{
	for(%x = 1; %x <= $AFKCount; %x++) 
	{
		%guid = $dtVar::AFKList[%x];
		if(%client.guid $= %guid && %guid !$= "")
			$DT_AFKListCount = $DT_AFKListCount - 1;
	}
	if($DT_AFKListCount $= 0) //Wont set IDLE until all List Clients are off the server
		$DT_AFKStatus = "IDLE";
}

////////////////////////////////////////////////////////////////////////////////
   
function DT_AFKtimeoutLoop()
{	
	//echo($DT_AFKStatus);
	
	switch$($DT_AFKStatus)
	{
		case ACTIVE:
		  if($dtVar::AFKtime != 0 && !$Host::TournamentMode)
		  { //0 minutes disables
			 if($dtVar::ListOnly)
			 {
				for(%i = 0; %i < ClientGroup.getCount(); %i ++) 
				{
				   %client = ClientGroup.getObject(%i);
				   for(%x = 1; %x <= $AFKCount; %x++) 
				   {
					  %guid = $dtVar::AFKList[%x];
					  if(!%client.isAIControlled() && isObject(%client.player) && %client.player.getState() !$= "Dead" )
					  {
						 if(%client.guid $= %guid && %guid !$= "")
							AFKChk(%client);
					  }
				   }
				}
			 }
			 else
			 {
				for(%i = 0; %i < ClientGroup.getCount(); %i ++) 
				{
				   %client = ClientGroup.getObject(%i);
					if(!%client.isAIControlled() && isObject(%client.player) && %client.player.getState() !$= "Dead")
						AFKChk(%client);
				}
			 }
		  }
		case IDLE:				
			//Do Nothing
	}
  
	if(isEventPending($dtVar::AFKloopSchedule)) 
		cancel($dtVar::AFKloopSchedule);

	$dtVar::AFKloopSchedule = schedule($dtVar::AFKloop, 0, "DT_AFKtimeoutLoop");	
}

////////////////////////////////////////////////////////////////////////////////

function AFKChk(%client)
{
   if(%client.player.curTransform  $= %client.player.getTransform())
   {//checks to see if there position and rotation are the same. 
	  %client.player.afkTimer += $dtVar::AFKloop;
      if(%client.player.afkTimer >= $dtVar::AFKtime)
	  {
          Game.AFKForceObserver(%client);
          return;
      }
   }
   else
   {
      %client.player.afkTimer = 0;//reset if moving
   }
   
   %client.player.curTransform = %client.player.getTransform();//save current transform 
}

////////////////////////////////////////////////////////////////////////////////

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
   messageClient(%client, 'MsgClientJoinTeam', '\c2You have been placed into observer mode due to inactivity.', %client.name, game.getTeamName(0), %client, 0 );
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
   
   messageAllExcept(%client, -1, 'MsgClientJoinTeam', '\c2%1 has been placed into observer mode due to inactivity.', %client.name, game.getTeamName(0), %client, 0 );
   
   updateCanListenState( %client );
   
   // call the onEvent for this game type
   %game.onClientEnterObserverMode(%client);  //Bounty uses this to remove this client from others' hit lists
}