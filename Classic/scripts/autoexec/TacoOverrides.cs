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

   %client.player.throwStart = 5; //was 0
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
    //Takes 10 blaster shots to kill a heavy, 13 normal.
	if(%targetObject.client.armor $= "Heavy" && %damageType $= $DamageType::Blaster)
		%amount *= 1.3;
	
	Parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
}

// Global water viscosity
function DefaultGame::missionLoadDone(%game)
{
   parent::missionLoadDone(%game);
   
   for(%i = 0; %i < MissionGroup.getCount(); %i++)
   {
      %obj = MissionGroup.getObject(%i);
      if(%obj.getClassName() $= "WaterBlock")
         %obj.viscosity = $globalviscosity;
   }  
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TacoOverrides))
    activatePackage(TacoOverrides);