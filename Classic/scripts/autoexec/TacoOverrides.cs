// Taco Overrides Script
//
// Various Overrides
//

// Global water viscosity
$globalviscosity = 3;

package TacoOverrides
{

//Issue with the start grenade throw was very soft and bumped it up a tad
function serverCmdEndThrowCount(%client, %data)
{
   if(%client.player.throwStart == 5)
      return;

   // ---------------------------------------------------------------
   // z0dd - ZOD, 8/6/02. New throw str features
   %throwStrength = (getSimTime() - %client.player.throwStart) / 150;
   if(%throwStrength > $maxThrowStr)
      %throwStrength = $maxThrowStr;
   else if(%throwStrength < 0.5)
      %throwStrength = 0.5;
   // ---------------------------------------------------------------

   %throwScale = %throwStrength / 2;
   %client.player.throwStrength = %throwScale;

   %client.player.throwStart = 2; //was 0
}

//Tank UE code by Keen
//To fix tank UE by transporting the tank far away so nothing bumps into it causing a UE
function VehicleData::onDestroyed(%data, %obj, %prevState)
{
    if(%obj.lastDamagedBy)
    {
        %destroyer = %obj.lastDamagedBy;
        game.vehicleDestroyed(%obj, %destroyer);
        //error("vehicleDestroyed( "@ %obj @", "@ %destroyer @")");
    }

	radiusVehicleExplosion(%data, %obj);

   if(%obj.turretObject)
      if(%obj.turretObject.getControllingClient())
         %obj.turretObject.getDataBlock().playerDismount(%obj.turretObject);

   for(%i = 0; %i < %obj.getDatablock().numMountPoints; %i++)
   {
      if (%obj.getMountNodeObject(%i)) {
         %flingee = %obj.getMountNodeObject(%i);
         %flingee.getDataBlock().doDismount(%flingee, true);
         %xVel = 250.0 - (getRandom() * 500.0);
         %yVel = 250.0 - (getRandom() * 500.0);
         %zVel = (getRandom() * 100.0) + 50.0;
         %flingVel = %xVel @ " " @ %yVel @ " " @ %zVel;
         %flingee.applyImpulse(%flingee.getTransform(), %flingVel);
         echo("got player..." @ %flingee.getClassName());
         %flingee.damage(0, %obj.getPosition(), 0.4, $DamageType::Crash);
      }
   }

   // From AntiLou.vl2
   // Info: MPB just destroyed. Change the variable
   if(%data.getName() $= "MobileBaseVehicle") // If the vehicle is the MPB, change %obj.station.isDestroyed to 1
		%obj.station.isDestroyed = 1;

   %data.deleteAllMounted(%obj);
   // -----------------------------------------------------------------------------------------
   // z0dd - ZOD - Czar, 6/24/02. Move this vehicle out of the way so nothing collides with it.
   if(%data.getName() $="AssaultVehicle")
   {
      // %obj.setFrozenState(true);
      %obj.schedule(500, "delete"); //was 2000
      //%data.schedule(500, 'onAvoidCollisions', %obj);

	  //Transfer the vehicle far away
      %obj.schedule(1, "setPosition", vectorAdd(%obj.getPosition(), "40 -27 10000")); //Lowered: was 500
   }
   else if(%data.getName() $="BomberFlyer" || %data.getName() $="MobileBaseVehicle")
   {
      // %obj.setFrozenState(true);
      %obj.schedule(2000, "delete"); //was 2000
      //%data.schedule(500, 'onAvoidCollisions', %obj);

	  //Transfer the vehicle far away
      %obj.schedule(100, "setPosition", vectorAdd(%obj.getPosition(), "40 -27 10000")); //Lowered: was 500
   }
   else
   {
      %obj.setFrozenState(true);
      %obj.schedule(500, "delete"); //was 500
   }
   // -----------------------------------------------------------------------------------------
}

// stationTrigger::onEnterTrigger(%data, %obj, %colObj)
// Info: If the MPB is destroyed, don't allow players to use the inv
function stationTrigger::onEnterTrigger(%data, %obj, %colObj)
{
   //make sure it's a player object, and that that object is still alive
   if(%colObj.getDataBlock().className !$= "Armor" || %colObj.getState() $= "Dead")
      return;

   // z0dd - ZOD, 7/13/02 Part of hack to keep people from mounting
   // vehicles in disallowed armors.
   if(%obj.station.getDataBlock().getName() !$= "StationVehicle")
      %colObj.client.inInv = true;

   %colObj.inStation = true;
   commandToClient(%colObj.client,'setStationKeys', true);
   if(Game.stationOnEnterTrigger(%data, %obj, %colObj))
   {
      //verify station.team is team associated and isn't on player's team
      if((%obj.mainObj.team != %colObj.client.team) && (%obj.mainObj.team != 0))
      {
         //%obj.station.playAudio(2, StationAccessDeniedSound);
         messageClient(%colObj.client, 'msgStationDenied', '\c2Access Denied -- Wrong team.~wfx/powered/station_denied.wav');
      }
      else if(%obj.disableObj.isDisabled())
      {
         messageClient(%colObj.client, 'msgStationDisabled', '\c2Station is disabled.');
      }
      else if(!%obj.mainObj.isPowered())
      {
         messageClient(%colObj.client, 'msgStationNoPower', '\c2Station is not powered.');
      }
      else if(%obj.station.notDeployed)
      {
         messageClient(%colObj.client, 'msgStationNotDeployed', '\c2Station is not deployed.');
      }
	  else if(%obj.station.isDestroyed)
      {
      	messageClient(%colObj.client, 'msgStationDestroyed', '\c2Station is destroyed.');
      }
      else if(%obj.station.triggeredBy $= "")
      {
         if(%obj.station.getDataBlock().setPlayersPosition(%obj.station, %obj, %colObj))
         {
            messageClient(%colObj.client, 'CloseHud', "", 'inventoryScreen');
            commandToClient(%colObj.client, 'TogglePlayHuds', true);
            %obj.station.triggeredBy = %colObj;
            %obj.station.getDataBlock().stationTriggered(%obj.station, 1);
            %colObj.station = %obj.station;
            %colObj.lastWeapon = ( %colObj.getMountedImage($WeaponSlot) == 0 ) ? "" : %colObj.getMountedImage($WeaponSlot).item;
            %colObj.unmountImage($WeaponSlot);
         }
      }
   }
}

//OG Blaster Buff
function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC)
{
    //Takes 11 blaster shots to kill a heavy, 13 normal.
	if(%targetObject.client.armor $= "Heavy" && %damageType $= $DamageType::Blaster && !$Host::TournamentMode) //Free-for-all only
		%amount *= 1.15;

	Parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
}

// Global water viscosity
function DefaultGame::missionLoadDone(%game)
{
   parent::missionLoadDone(%game);

   InitContainerRadiusSearch("0 0 0",  2048, $TypeMasks::WaterObjectType);
   while ((%itemObj = containerSearchNext()) != 0)
   {
      if(%itemObj.getClassName() $= "WaterBlock")
         %itemObj.viscosity = $globalviscosity;
   }
}

// Temp Fix for Asset nameTag Strings
function GameBaseData::onAdd(%data, %obj)
{
   if(%data.targetTypeTag !$= "")
   {
      // use the name given to the object in the mission file
      if(%obj.nameTag !$= "" && strpos(%obj.nameTag,"\x01") == -1)
      {
         %obj.nameTag = addTaggedString(%obj.nameTag);
         %nameTag = %obj.nameTag;
      }
      else if(%data.targetNameTag !$= "")
         %nameTag = %data.targetNameTag;
      else
      {
         if(%obj.name !$= "")
            %nameTag = %obj.nameTag = addTaggedString(%obj.name);
         else
            %nameTag = %obj.nameTag = addTaggedString("Base"); // fail safe so it shows up on cc
      }
       %obj.target = createTarget(%obj, %nameTag, "", "", %data.targetTypeTag, 0, 0);
   }
   else
      %obj.target = -1;
}

// Throw Spam fix
function serverCmdThrow(%client, %data)
{
	if(%client.tossLock)
	{
		if(getSimTime() - %client.tossLockTime < 30000)
			return;
		else
			%client.tossLock = 0;
	}

	if(getSimTime() - %client.tossTime < 128)
	{
		%client.tossCounter++;
		if(%client.tossCounter > 30)
		{
			if(%client.tossLockWarning)
			{
				echo(%client.nameBase SPC "was Banned for exceeding" SPC %client.tossCounter SPC "Toss Limit.");
				messageAll('msgAll',"\c3" @ %client.namebase SPC "is attempting to lag the server!");
				messageClient(%client, 'onClientBanned', "");
				messageAllExcept( %client, -1, 'MsgClientDrop', "", %client.name, %client );
				if(isObject(%client.player))
					%client.player.scriptKill(0);
				if (isObject(%client))
				{
					%client.setDisconnectReason("Item Spew scripts are not allowed on this server." );
					%client.schedule(700, "delete");
				}
				BanList::add(%client.guid, %client.getAddress(), $Host::BanTime);
			}
			else
			{
				echo(%client.nameBase SPC "throwing items has been temporarily suspended for exceeding" SPC %client.tossCounter SPC "throw limit.");
				centerprint(%client, "You are recieving this warning for throw spamming items.\nContinuing to use throw spew scripts will result in a ban.", 10, 2);
				messageClient(%client, '', "Throwing items has been temporarily suspended.");
				%client.tossLockTime = getSimTime();
				%client.tossLockWarning = 1;
			}
			%client.tossLock = 1;
			return;
		}
	}
	else
		%client.tossCounter = 0;

	parent::serverCmdThrow(%client, %data);
	%client.tossTime = getSimTime();
}

//Vehicle Respawn bug fix - Respawn schedules carrying into map changes
function VehicleData::respawn(%data, %marker)
{
   if($MatchStarted + $missionRunning == 2 && isObject(%marker))
   {
	   %mask = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::TurretObjectType;
	   InitContainerRadiusSearch(%marker.getWorldBoxCenter(), %data.checkRadius, %mask);
	   if(containerSearchNext() == 0)
	   {
		  %newObj = %data.create(%marker.curTeam, %marker);
		  %newObj.startFade(1000, 0, false);
		  %newObj.setTransform(%marker.getTransform());

		  setTargetSensorGroup(%newObj.target, %newObj.team);
		  MissionCleanup.add(%newObj);
	   }
	   else
	   {
		  %marker.schedule = %data.schedule(3000, "respawn", %marker);
	   }
   }
}

function VehicleData::createPositionMarker(%data, %obj)
{
   %marker = new Trigger(PosMarker)
   {
      dataBlock = markerTrigger;
      mountable = %obj.mountable;
      disableMove = %obj.disableMove;
      resetPos = %obj.resetPos;
      data = %obj.getDataBlock().getName();
      deployed = %obj.deployed;
      curTeam = %obj.team;
      respawnTime = %obj.respawnTime;
   };
   %marker.setTransform(%obj.getTransform());
   MissionCleanup.add(%marker);
   return %marker;
}

//Conc Throw (Almost Normal Grenades) 1500 Normal
function ConcussionGrenadeThrown::onThrow(%this, %gren)
{
   AIGrenadeThrown(%gren);
   %gren.detThread = schedule(1800, %gren, "detonateGrenade", %gren); // Was 2000
}

//Attack LOS Sky Fix
function serverCmdSendTaskToClient(%client, %targetClient, %fromCmdMap)
{
   %obj = getTargetObject(%client.getTargetId());
   if(isObject(%obj))
   {
      if(%obj.getClassName() $= "Player" && !%client.player.ccActive)
      {
         %vec = %client.player.getMuzzleVector(0);
         %vec2 = vectorNormalize(vectorSub(%obj.getWorldBoxCenter(), %client.player.getMuzzlePoint(%slot)));
         %dot = vectorDot(%vec, %vec2);
         if(%dot < 0.9)
            return;
      }
   }

   parent::serverCmdSendTaskToClient(%client, %targetClient, %fromCmdMap);
}

function serverCmdScopeCommanderMap(%client, %scope)
{
   parent::serverCmdScopeCommanderMap(%client, %scope);
   %client.player.ccActive = %scope;
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TacoOverrides))
    activatePackage(TacoOverrides);
