$Camera::movementSpeed = 40;

datablock CameraData(Observer)
{
   mode = "observerStatic";
   firstPersonOnly = true;
};

datablock CameraData(CommanderCamera)
{
   mode = "observerStatic";
   firstPersonOnly = true;
};

function CommanderCamera::onTrigger( %data, %obj, %trigger, %state )
{
   // no need to do anything here.
}

function Observer::onTrigger(%data,%obj,%trigger,%state)
{
   // state = 0 means that a trigger key was released
   if (%state == 0)
      return;

   //first, give the game the opportunity to prevent the observer action
   if (!Game.ObserverOnTrigger(%data, %obj, %trigger, %state))
      return;

   //now observer functions if you press the "throw"
   if (%trigger >= 4)
      return;

   //trigger types:   0:fire 1:altTrigger 2:jump 3:jet 4:throw
   %client = %obj.getControllingClient();
   if (%client == 0)
      return;

   switch$ (%obj.mode)
   {
      case "justJoined":
         // z0dd - ZOD, 7/15/03. Don't need to waste CPU on a demo check
         //if (isDemo())
         //   clearCenterPrint(%client);
         if (%trigger == 0) //press FIRE
         {
            // clear intro message
            clearBottomPrint( %client );

            //spawn the player
		commandToClient(%client, 'setHudMode', 'Standard');
            Game.assignClientTeam(%client);
            Game.spawnPlayer( %client, $MatchStarted );

            if( $MatchStarted )
            {
               %client.camera.setFlyMode();
               %client.setControlObject( %client.player );
            }
            else
            {
               %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
               %client.setControlObject( %client.camera );
            }
         }
         else if (%trigger == 3) //press JET
         {
            //cycle throw the static observer spawn points
            %markerObj = Game.pickObserverSpawn(%client, true);
            %transform = %markerObj.getTransform();
            %obj.setTransform(%transform);
            %obj.setFlyMode();
         }
         else if (%trigger == 2) //press JUMP
         {
            //switch the observer mode to observing clients
            if (isObject(%client.observeFlyClient))
               serverCmdObserveClient(%client, %client.observeFlyClient);
            else
               serverCmdObserveClient(%client, -1);

            displayObserverHud(%client, %client.observeClient);
            if(!%client.isAdmin && !%client.isWatchOnly) // z0dd - ZOD, 7/15/03. Only warn them if it isn't an admin watching.
               messageClient(%client.observeClient, 'Observer', '\c1%1 is now observing you.', %client.name);
         }

      case "playerDeath":
         // Attached to a dead player - spawn regardless of trigger type
         if(!%client.waitRespawn && getSimTime() > %client.suicideRespawnTime)
         {
		commandToClient(%client, 'setHudMode', 'Standard');
            Game.spawnPlayer( %client, true );
            %client.camera.setFlyMode();
            %client.setControlObject(%client.player);
         }

      case "PreviewMode":
         if (%trigger == 0)
         {
		commandToClient(%client, 'setHudMode', 'Standard');
            if( %client.lastTeam )
               Game.clientJoinTeam( %client, %client.lastTeam );
            else
            {
               Game.assignClientTeam( %client, true );

               // Spawn the player:
               Game.spawnPlayer( %client, false );
            }
            %client.camera.setFlyMode();
            %client.setControlObject( %client.player );
         }

      case "toggleCameraFly":
      // this is the default camera mode

      case "observerFly":
         // Free-flying observer camera
         if (%trigger == 0)
         {
            if(!%client.waitRespawn && getSimTime() > %client.suicideRespawnTime)
            {
               if( !$Host::TournamentMode && $MatchStarted )
               {
                  // reset observer params
                  clearBottomPrint(%client);
                  commandToClient(%client, 'setHudMode', 'Standard');

                  if( %client.lastTeam !$= "" && %client.lastTeam != 0 && Game.numTeams > 1)
                  {
                     Game.clientJoinTeam( %client, %client.lastTeam, $MatchStarted );
                     %client.camera.setFlyMode();
                     %client.setControlObject( %client.player );
                  }
                  else
                  {
                     Game.assignClientTeam( %client );

                     // Spawn the player:
                     Game.spawnPlayer( %client, true );
                     %client.camera.setFlyMode();
                     %client.setControlObject( %client.player );
                     ClearBottomPrint( %client );
                  }
               }
               else if( !$Host::TournamentMode )
               {
                  clearBottomPrint(%client);
                  Game.assignClientTeam( %client );

                  // Spawn the player:
                  Game.spawnPlayer( %client, false );
                  %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
                  %client.setControlObject( %client.camera );
               }
            }
         }
         else if (%trigger == 3) //press JET
         {
            %markerObj = Game.pickObserverSpawn(%client, true);
            %transform = %markerObj.getTransform();
            %obj.setTransform(%transform);
            %obj.setFlyMode();
         }
         else if (%trigger == 2) //press JUMP
         {
            //switch the observer mode to observing clients
            if (isObject(%client.observeFlyClient))
               serverCmdObserveClient(%client, %client.observeFlyClient);
            else
               serverCmdObserveClient(%client, -1);

            observerFollowUpdate( %client, %client.observeClient, false );
            displayObserverHud(%client, %client.observeClient);
            if(!%client.isAdmin && !%client.isWatchOnly) // z0dd - ZOD, 7/15/03. Only warn them if it isn't an admin watching.
               messageClient(%client.observeClient, 'Observer', '\c1%1 is now observing you.', %client.name);
         }

      case "observerStatic":
         // Non-moving observer camera
         %next = (%trigger == 3 ? true : false);
         %markerObj = Game.pickObserverSpawn(%client, %next);
         %transform = %markerObj.getTransform();
         %obj.setTransform(%transform);
         %obj.setFlyMode();

      case "observerStaticNoNext":
         // Non-moving, non-cycling observer camera

      case "observerTimeout":
         // Player didn't respawn quickly enough
         if (%trigger == 0)
         {
            clearBottomPrint(%client);
		commandToClient(%client, 'setHudMode', 'Standard');
            if( %client.lastTeam )
               Game.clientJoinTeam( %client, %client.lastTeam, true );
            else
            {
               Game.assignClientTeam( %client );

               // Spawn the player:
               Game.spawnPlayer( %client, true );
            }
            %client.camera.setFlyMode();
            %client.setControlObject( %client.player );
         }
         else if (%trigger == 3) //press JET
         {
            %markerObj = Game.pickObserverSpawn(%client, true);
            %transform = %markerObj.getTransform();
            %obj.setTransform(%transform);
            %obj.setFlyMode();
         }
         else if (%trigger == 2) //press JUMP
         {
            //switch the observer mode to observing clients
            if (isObject(%client.observeFlyClient))
               serverCmdObserveClient(%client, %client.observeFlyClient);
            else
               serverCmdObserveClient(%client, -1);

            // update the observer list for this client
            observerFollowUpdate( %client, %client.observeClient, false );

            displayObserverHud(%client, %client.observeClient);
            if(!%client.isAdmin && !%client.isWatchOnly) // z0dd - ZOD, 7/15/03. Only warn them if it isn't an admin watching.
               messageClient(%client.observeClient, 'Observer', '\c1%1 is now observing you.', %client.name);
         }

      case "observerFollow":
         // Observer attached to a moving object (assume player for now...)
         //press FIRE - cycle to next client
         if (%trigger == 0)
         {
            %nextClient = findNextObserveClient(%client);
            %prevObsClient = %client.observeClient;
            if (%nextClient > 0 && %nextClient != %client.observeClient)
            {
               // update the observer list for this client
               observerFollowUpdate( %client, %nextClient, true );

               //set the new object
               %transform = %nextClient.player.getTransform();
               //if( !%nextClient.isMounted() ) z0dd - ZOD, 7/15/03. DUH!
               if( !%nextClient.player.isMounted() )
               {
                  //z0dd - ZOD, 7/15/03. Use datablock of armor for observer params
                  %params = %nextClient.player.getDataBlock().observeParameters;
                  %obj.setOrbitMode(%nextClient.player, %transform, getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
                  //%obj.setOrbitMode(%nextClient.player, %transform, 0.5, 4.5, 4.5);
                  %client.observeClient = %nextClient;
               }
               else
               {
                  %mount = %nextClient.player.getObjectMount();
                  if( %mount.getDataBlock().observeParameters $= "" )
                     %params = %transform;
                  else
                     %params = %mount.getDataBlock().observeParameters;

                  %obj.setOrbitMode(%mount, %mount.getTransform(), getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
                  %client.observeClient = %nextClient;
               }

               //send the message(s)
               displayObserverHud(%client, %nextClient);
               if(!%client.isAdmin && !%client.isWatchOnly) // z0dd - ZOD, 7/15/03. Only warn them if it isn't an admin watching.
               {
                  messageClient(%nextClient, 'Observer', '\c1%1 is now observing you.', %client.name);
                  messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
               }
            }
         }
         else if (%trigger == 3) //press JET - cycle to prev client
         {
            %prevClient = findPrevObserveClient(%client);
            %prevObsClient = %client.observeClient;
            if (%prevClient > 0 && %prevClient != %client.observeClient)
            {
               // update the observer list for this client
               observerFollowUpdate( %client, %prevClient, true );

               //set the new object
               %transform = %prevClient.player.getTransform();
               //if( !%prevClient.isMounted() ) z0dd - ZOD, 7/15/03. DUH!
               if( !%prevClient.player.isMounted() )
               {
                  //z0dd - ZOD, 7/15/03. Use datablock of armor for observer params
                  %params = %prevClient.player.getDataBlock().observeParameters;
                  %obj.setOrbitMode(%prevClient.player, %transform, getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
                  //%obj.setOrbitMode(%prevClient.player, %transform, 0.5, 4.5, 4.5);
                  %client.observeClient = %prevClient;
               }
               else
               {
                  %mount = %prevClient.player.getObjectMount();
                  if( %mount.getDataBlock().observeParameters $= "" )
                     %params = %transform;
                  else
                     %params = %mount.getDataBlock().observeParameters;

                  %obj.setOrbitMode(%mount, %mount.getTransform(), getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
                  %client.observeClient = %prevClient;
               }
               //send the message(s)
               displayObserverHud(%client, %prevClient);
               if(!%client.isAdmin && !%client.isWatchOnly) // z0dd - ZOD, 7/15/03. Only warn them if it isn't an admin watching.
               {
                  messageClient(%prevClient, 'Observer', '\c1%1 is now observing you.', %client.name);
                  messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
               }
            }
         }
         else if (%trigger == 2) //press JUMP
         {
            // update the observer list for this client
            observerFollowUpdate( %client, -1, false );

            //toggle back to observer fly mode
            %obj.mode = "observerFly";
            %obj.setFlyMode();
            updateObserverFlyHud(%client);
            if(!%client.isAdmin) // z0dd - ZOD, 7/15/03. Only warn them if it isn't an admin watching.
               messageClient(%client.observeClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
         }

      case "pre-game":
         if(!$Host::TournamentMode || $CountdownStarted)
            return;

         %readySpamDif  = getSimTime() - %client.readySpam;
         %readySpamDif1  = getSimTime() - %client.readySpamMsg;
         if(%readySpamDif > 10000 || !%client.readySpam)
         {
            %client.readySpam = getSimTime();
            if(%client.notReady)
            {
               %client.notReady = "";
               MessageAll( 0, '\c1%1 is READY.', %client.name );
               if(%client.notReadyCount < 3)
                  centerprint( %client, "\nWaiting for match start (FIRE if not ready)", 0, 3);
               else
                  centerprint( %client, "\nWaiting for match start", 0, 3);
            }
            else
            {
               %client.notReadyCount++;
               if(%client.notReadyCount < 4)
               {
                  %client.notReady = true;
                  MessageAll( 0, '\c1%1 is not READY.', %client.name );
                  centerprint( %client, "\nPress FIRE when ready.", 0, 3 );
               }
               return;
            }

            CheckTourneyMatchStart();
         }
         else if((%readySpamDif1 > 1000 || !%client.readySpamMsg) && %client.notReadyCount < 4)
         {
            %client.readySpamMsg = getSimTime();
            %wait = mFloor((10000 - (getSimTime() - %client.readySpam)) / 1000);
            messageClient(%client, 'MsgObserverCooldown', '\c3Ready Cooldown:\cr Please wait another %1 seconds.', %wait );
         }

   }
}

function Observer::setMode(%data, %obj, %mode, %targetObj)
{
   if(%mode $= "")
      return;
   %client = %obj.getControllingClient();
   switch$ (%mode) {
      case "justJoined":
         commandToClient(%client, 'setHudMode', 'Observer');
         %markerObj = Game.pickObserverSpawn(%client, true);
         %transform = %markerObj.getTransform();
         %obj.setTransform(%transform);
         %obj.setFlyMode();

      case "pre-game":
         commandToClient(%client, 'setHudMode', 'Observer');
         //z0dd - ZOD, 7/15/03. Use datablock of armor for observer params
         %params = %targetObj.getDataBlock().observeParameters;
         %obj.setOrbitMode(%targetObj, %targetObj.getTransform(), getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
         //%obj.setOrbitMode( %targetObj, %targetObj.getTransform(), 0.5, 4.5, 4.5);

      case "observerFly":
         // Free-flying observer camera
         commandToClient(%client, 'setHudMode', 'Observer');
         %markerObj = Game.pickObserverSpawn(%client, true);
         %transform = %markerObj.getTransform();
         %obj.setTransform(%transform);
         %obj.setFlyMode();

      case "observerStatic" or "observerStaticNoNext":
         // Non-moving observer camera
         %markerObj = Game.pickObserverSpawn(%client, true);
         %transform = %markerObj.getTransform();
         %obj.setTransform(%transform);

      case "observerFollow":
         // Observer attached to a moving object (assume player for now...)
         %transform = %targetObj.getTransform();

         if( !%targetObj.isMounted() )
         {
            //z0dd - ZOD, 7/15/03. Use datablock of armor for observer params
            %params = %targetObj.getDataBlock().observeParameters;
            %obj.setOrbitMode(%targetObj, %transform, getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
            //%obj.setOrbitMode(%targetObj, %transform, 0.5, 4.5, 4.5);
         }
         else
         {
            %mount = %targetObj.getObjectMount();
            if( %mount.getDataBlock().observeParameters $= "" )
               %params = %transform;
            else
               %params = %mount.getDataBlock().observeParameters;

            %obj.setOrbitMode(%mount, %mount.getTransform(), getWord( %params, 0 ), getWord( %params, 1 ), getWord( %params, 2 ));
         }

      case "observerTimeout":
         commandToClient(%client, 'setHudMode', 'Observer');
         %markerObj = Game.pickObserverSpawn(%client, true);
         %transform = %markerObj.getTransform();
         %obj.setTransform(%transform);
         %obj.setFlyMode();
   }
   %obj.mode = %mode;
}

function findNextObserveClient(%client)
{
   %index = -1;
   %count = ClientGroup.getCount();
   if (%count <= 1)
      return -1;

   for (%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl == %client.observeClient)
      {
         %index = %i;
         break;
      }
   }

   //now find the next client (note, if not found, %index still == -1)
   %index++;
   if (%index >= %count)
      %index = 0;

   %newClient = -1;
   for (%i = %index; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl != %client && %cl.player > 0)
      {
         %newClient = %cl;
         break;
      }
   }

   //if we didn't find anyone, search from the beginning again
   if (%newClient < 0)
   {
      for (%i = 0; %i < %count; %i++)
      {
         %cl = ClientGroup.getObject(%i);
         if (%cl != %client && %cl.player > 0)
         {
            %newClient = %cl;
            break;
         }
      }
   }

   //if we still haven't found anyone (new), give up..
   if (%newClient < 0 || %newClient.player == %player)
      return -1;
}

function findPrevObserveClient(%client)
{
   %index = -1;
   %count = ClientGroup.getCount();
   if (%count <= 1)
      return -1;

   for (%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl == %client.observeClient)
      {
         %index = %i;
         break;
      }
   }

   //now find the prev client
   %index--;
   if (%index < 0)
      %index = %count - 1;

   %newClient = -1;
   for (%i = %index; %i >= 0; %i--)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl != %client && %cl.player > 0)
      {
         %newClient = %cl;
         break;
      }
   }

   //if we didn't find anyone, search from the end again
   if (%newClient < 0)
   {
      for (%i = %count - 1; %i >= 0; %i--)
      {
         %cl = ClientGroup.getObject(%i);
         if (%cl != %client && %cl.player > 0)
         {
            %newClient = %cl;
            break;
         }
      }
   }

   //if we still haven't found anyone (new), give up..
   if (%newClient < 0 || %newClient.player == %player)
      return -1;
}

function observeClient(%client)
{
   if( $testcheats )
   {
      //pass in -1 to choose any client...
      commandToServer('observeClient', %client);
   }
}

function serverCmdObserveClient(%client, %target)
{
   //clear the observer fly mode var...
   %client.observeFlyClient = -1;

   //cancel any scheduled update
   cancel(%client.obsHudSchedule);

   // must be an observer when observing other clients
   if( %client.getControlObject() != %client.camera)
      return;

   //can't observer yourself
   if (%client == %target)
      return;

   %count = ClientGroup.getCount();

   //can't go into observer mode if you're the only client
   if (%count <= 1)
      return;

   //make sure the target actually exists
   if (%target > 0)
   {
      %found = false;
      for (%i = 0; %i < %count; %i++)
      {
          %cl = ClientGroup.getObject(%i);
          if (%cl == %target)
          {
            %found = true;
            break;
          }
      }

      if (!%found)
         return;
   }
   else
   {
      %client.observeClient = -1;
      %target = findNextObserveClient(%client);
      if (%target <= 0)
         return;
   }

   //send the message
   if (%client.camera.mode !$= "observerFollow")
   {
      if (isObject(%client.player))
         %client.player.scriptKill(0);

      //messageAllExcept(%client, -1, 'ClientNowObserver', '\c1%1 is now an observer.', %client.name);
      //messageClient(%client, 'YouNowObserver', '\c1You are now observing %1.', %target.name);
   }

   %client.camera.getDataBlock().setMode(%client.camera, "observerFollow", %target.player);
   %client.setControlObject(%client.camera);

   //tag is used if a client who is being observed dies...
   %client.observeClient = %target;
}

function observerFollowUpdate( %client, %nextClient, %cycle )
{
   %Oclient = %client.observeClient;
   if( %Oclient $= "" )
      return;

   // changed to observer fly...
   if( %nextClient == -1 )
   {
      // find us in their observer list and remove, then reshuffle the list...
      for( %i = 0; %i < %Oclient.observeCount; %i++ )
      {
         if( %Oclient.observers[%i] == %client )
         {
            %Oclient.observeCount--;
            %Oclient.observers[%i] = %Oclient.observers[%Oclient.observeCount];
            %Oclient.observers[%Oclient.observeCount] = "";
            break;
         }
      }
      return; // were done..
   }

   // changed from observer fly to observer follow...
   if( !%cycle && %nextClient != -1 )
   {
      // if nobody is observing this guy, initialize their observer count...
      if( %nextClient.observeCount $= "" )
         %nextClient.observeCount = 0;

      // add us to their list of observers...
      %nextClient.observers[%nextClient.observeCount] = %client;
      %nextClient.observeCount++;
      return; // were done.
   }

   if( %nextClient != -1 )
   {
      // cycling to the next client...
      for( %i = 0; %i < %Oclient.observeCount; %i++ )
      {
         // first remove us from our prev client's list...
         if( %Oclient.observers[%i] == %client )
         {
            %Oclient.observeCount--;
            %Oclient.observers[%i] = %Oclient.observers[%Oclient.observeCount];
            %Oclient.observers[%Oclient.observeCount] = "";
            break; // screw you guys, i'm goin home!
         }
      }

      // if nobody is observing this guy, initialize their observer count...
      if( %nextClient.observeCount $= "" )
         %nextClient.observeCount = 0;

      // now add us to the new clients list...
      %nextClient.observeCount++;
      %nextClient.observers[%nextClient.observeCount - 1] = %client;
   }
}

function updateObserverFlyHud(%client)
{
   //just in case there are two threads going...
   cancel(%client.obsHudSchedule);
   %client.observeFlyClient = -1;

   //make sure the client is supposed to be in observer fly mode...
   if (!isObject(%client) || %client.team != 0 || %client.getControlObject() != %client.camera || %client.camera.mode $= "observerFollow")
      return;

	//get various info about the player's eye
	%srcEyeTransform = %client.camera.getTransform();
	%srcEyePoint = firstWord(%srcEyeTransform) @ " " @ getWord(%srcEyeTransform, 1) @ " " @ getWord(%srcEyeTransform, 2);

	%srcEyeVector = MatrixMulVector("0 0 0 " @ getWords(%srcEyeTransform, 3, 6), "0 1 0");
	%srcEyeVector = VectorNormalize(%srcEyeVector);

   //see if there's an enemy near our defense location...
   %clientCount = 0;
   %count = ClientGroup.getCount();
   %viewedClient = -1;
   %clientDot = -1;
   for(%i = 0; %i < %count; %i++)
   {
		%cl = ClientGroup.getObject(%i);

		//make sure we find an AI who's alive and not the client
		if (%cl != %client && isObject(%cl.player))
		{
			//make sure the player is within range
		   %clPos = %cl.player.getWorldBoxCenter();
		   %distance = VectorDist(%clPos, %srcEyePoint);
			if (%distance <= 30)
			{
				//create the vector from the client to the client
				%clVector = VectorNormalize(VectorSub(%clPos, %srcEyePoint));

				//see if the dot product is greater than our current, and greater than 0.6
				%dot = VectorDot(%clVector, %srcEyeVector);

				if (%dot > 0.6 && %dot > %clientDot)
				{
					//make sure we're not looking through walls...
					%mask = $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::StaticShapeObjectType;
					%losResult = containerRayCast(%srcEyePoint, %clPos, %mask);
					%losObject = GetWord(%losResult, 0);
					if (!isObject(%losObject))
					{
						%viewedClient = %cl;
						%clientDot = %dot;
					}
				}
			}
		}
   }

   if (isObject(%viewedClient))
      displayObserverHud(%client, 0, %viewedClient);
   else
      displayObserverHud(%client, 0);

   %client.observeFlyClient = %viewedClient;

   //schedule the next...
   %client.obsHudSchedule = schedule(500, %client, updateObserverFlyHud, %client);
}
