package PizzaThings
{

// function OptionsDlg::onWake(%this)
// {
   // if(!$observeFlagBind)
   // {
   	// $ObsRemapName[$ObsRemapCount] = "Obs First Flag";
		// $ObsRemapCmd[$ObsRemapCount] = "observeFirstFlag";
		// $ObsRemapCount++;
		// $ObsRemapName[$ObsRemapCount] = "Obs Second Flag";
		// $ObsRemapCmd[$ObsRemapCount] = "observeSecondFlag";
		// $ObsRemapCount++;

		// $observeFlagBind = true;
	// }

	// parent::onWake(%this);
// }

// For Observe Flag
// From Evo
function Observer::onTrigger(%data, %obj, %trigger, %state)
{
   if (%state == 0 || %trigger > 5)
      return;

   //first, give the game the opportunity to prevent the observer action
   if (!Game.ObserverOnTrigger(%data, %obj, %trigger, %state))
      return;

   %client = %obj.getControllingClient();
   if (%client == 0)
      return;

   switch$(%obj.mode)
   {
      case "followFlag":
         if(!%client.observingFlag)
            return;

         if(%trigger == 0) // press FIRE, switch to the other flag
         {
            if(%client.flagObserved == $TeamFlag[1])
            {
               %otherFlag = $TeamFlag[2];
               %otherFlagTeam = 2;
            }
            else if(%client.flagObserved == $TeamFlag[2])
            {
               %otherFlag = $TeamFlag[1];
               %otherFlagTeam = 1;
            }
            else
               return;
	      
            // the flag isn't carried
            if(%otherFlag.carrier $= "")
               observeFlag(%client, %otherFlag, 1, %otherFlagTeam);
            else if(isObject(%otherFlag.carrier.client))
               observeFlag(%client, %otherFlag.carrier.client, 2, %otherFlagTeam);
         }
         else if(%trigger == 3) // press JET, switch to the other flag
         {
            if(%client.flagObserved == $TeamFlag[1])
            {
               %otherFlag = $TeamFlag[2];
               %otherFlagTeam = 2;
            }
            else if(%client.flagObserved == $TeamFlag[2])
            {
               %otherFlag = $TeamFlag[1];
               %otherFlagTeam = 1;
            }
            else
               return;

            // the flag isn't carried
            if(%otherFlag.carrier $= "")
               observeFlag(%client, %otherFlag, 1, %otherFlagTeam);
            else if(isObject(%otherFlag.carrier.client))
               observeFlag(%client, %otherFlag.carrier.client, 2, %otherFlagTeam);
         }
         else if(%trigger == 2) //press JUMP, stop observing flag
         {
            if(%client.observeClient != -1)
            {
               observerFollowUpdate(%client, -1, false);
               messageClient(%client.observeClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
               %client.observeClient = -1;
            }
            %obj.mode = "observerFly";
            %obj.setFlyMode();
            updateObserverFlyHud(%client);
            %client.observingFlag = false;
            %client.flagObserved = "";
            %client.flagObsTeam = "";
         }

      default:
         Parent::onTrigger(%data, %obj, %trigger, %state);
   }
}

function Observer::setMode(%data, %obj, %mode, %targetObj)
{
   if(%mode $= "")
      return;

   %client = %obj.getControllingClient();
   if(%client $= "")
      return;

   switch$(%mode)
   {
      case "followFlag":
         %transform = %targetObj.getTransform();
         // observe the flag 2x more far than the normal
         %obj.setOrbitMode(%targetObj, %transform, 1.0, 9.0, 9.0);

      default:
         Parent::setMode(%data, %obj, %mode, %targetObj);
   }
   %obj.mode = %mode;   
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(PizzaThings))
	activatePackage(PizzaThings);

function serverCmdObserveFirstFlag(%client)
{
  if(Game.class !$= CTFGame || Game.class !$= SCtFGame)
    return;
  
  // client must be an observer
  if(%client.team > 0)
    return;
  
  // check if the flag is carried by someone
  %player = $TeamFlag[1].carrier;
  
  if($TeamFlag[1].isHome || %player $= "")
    observeFlag(%client, $TeamFlag[1], 1, 1);
  else
    observeFlag(%client, %player.client, 2, 1);
}

function serverCmdObserveSecondFlag(%client)
{
  if(Game.class !$= CTFGame || Game.class !$= SCtFGame)
    return;
  
  // client must be an observer
  if(%client.team > 0)
    return;
  
  // check if the flag is carried by someone
  %player = $TeamFlag[2].carrier;
  
  if($TeamFlag[2].isHome || %player $= "")
    observeFlag(%client, $TeamFlag[2], 1, 2);
  else
    observeFlag(%client, %player.client, 2, 2);
}

// observeFlag(%client, %target, %type, %flagTeam)
// Info: handle the observe flag feature
// observeFlag(%cl, $TeamFlag[%flag.team], 1, %flag.team);
function observeFlag(%client, %target, %type, %flagTeam)
{
   if(!isObject(%client) || !isObject(%target) || !isObject(%client.camera))
     return;
  
   if(Game.class !$= CTFGame || Game.class !$= SCtFGame)
     return;
  
   if(%client.team > 0)
     return;
  
   // cancel any scheduled update
   if(isEventPending(%client.obsHudSchedule))
     cancel(%client.obsHudSchedule);
  
   // must be an observer when observing other clients
   if(%client.getControlObject() != %client.camera)
     return;
  
   //can't observer yourself
   if(%client == %target)
     return;
  
   %count = ClientGroup.getCount();
  
   //can't go into observer mode if you're the only client
   if(%count <= 1 && %type != 1)
     return;

   if(%type == 1) // Flag
   {
      if(isObject(%client.player))
	   %client.player.scriptKill(0); // the player is still playing (this shouldn't be happen)
      
      %client.camera.getDataBlock().setMode(%client.camera, "followFlag", $TeamFlag[%flagTeam]);
      %client.setControlObject(%client.camera);
      clearBottomPrint(%client);
      
      // was the client observing a player before?
      if(%client.observeClient != -1)
	{
	  observerFollowUpdate(%client, -1, false);
	  messageClient(%client.observeClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
	  %client.observeClient = -1;
	}
   }
   else // Player
   {
      // make sure the target actually exists
      if(%target > 0)
	{
	  %found = false;
	  for(%i = 0; %i < %count; %i++)
	    {
			if(ClientGroup.getObject(%i) == %target)
			{
			  %found = true;
			  break;
			}
	    }
	  if(!%found)
	    return;
	}
      
      if(isObject(%client.player))
	   %client.player.scriptKill(0); // the player is still playing (this shouldn't be happen)
      
      observerFollowUpdate(%client, %target, true);
      displayObserverHud(%client, %target);
      messageClient(%target, 'Observer', '\c1%1 is now observing you.', %client.name);

      // was the client observing a player before?
      if(%client.observeClient != -1)
	messageClient(%client.observeClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
      %client.camera.getDataBlock().setMode(%client.camera, "observerFollow", %target.player);
      %client.setControlObject(%client.camera);
      %client.observeClient = %target;
   }
  
   //clear the observer fly mode var...
   %client.observeFlyClient = -1;
   %client.observingFlag = true;
   %client.flagObserved = $TeamFlag[%flagTeam];
   %client.flagObsTeam = %flagTeam;
}

// function observeFirstFlag()
// {
	// commandToServer('ObserveFirstFlag');
// }

// function observeSecondFlag()
// {
	// commandToServer('ObserveSecondFlag');
// }

// function sendPizzaHudUpdate(%game, %client, %msg)
// {
   // %pizzaOptMask = 0;
   // if($RandomTeams == 1) %pizzaOptMask += 1;
   // if($Host::ClassicFairTeams == 1) %pizzaOptMask += 2;
   // if($Host::ClassicAutoPWEnabled == 1) %pizzaOptMask += 4;
   // if($Host::EvoFullServerPWEnabled == 1) %pizzaOptMask += 8;
   // if($Host::EvoNoBaseRapeEnabled == 1) %pizzaOptMask += 16;
   // if($PizzaHudRestartVar == 1) %pizzaOptMask += 32;
   // messageClient(%client, 'UpdatePizzaHud', %msg, %pizzaOptMask, %client.isAdmin + %client.isSuperAdmin);
// }

// function serverCmdRegisterPizzaClient(%client)
// {
   // if(!%client.pizza)
   // {
      // %client.pizza = true;
      // %msg = "\c2Pizza Client registered.";
      // sendPizzaHudUpdate(%game, %client, %msg);
   // }
// }