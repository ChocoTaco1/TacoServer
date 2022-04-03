//-------------------------------------- Ammo functions
function Ammo::onCollision(%data, %obj, %col)
{
   // %data = datablock of object; %obj = object number
   // %col = thing that collided with object (hopefully a player)

   if (%col.getDataBlock().className $= Armor)
   {
      %ammoName = %data.getName();
      %ammoStore = %col.inv[%ammoName];
      
      // if player has ammo pack, increase max amount of ammo
      if(%col.getMountedImage($BackpackSlot) != 0)
      {
         if(%col.getMountedImage($BackpackSlot).getName() $= "AmmoPackImage")
            %aMax = (%col.getDataBlock().max[%ammoName]) + AmmoPack.max[%ammoName];
         else
            %aMax = %col.getDataBlock().max[%ammoName];
      }
      else
         %aMax = %col.getDataBlock().max[%ammoName];

      if(%col.inv[%ammoName] < %aMax)
      {
         if( %obj.ammoStore $= "" )
            %obj.ammoStore = $AmmoIncrement[ %ammoName ];

         %col.incInventory(%ammoName, %obj.ammoStore);
         serverPlay3D(ItemPickupSound, %col.getTransform());
         %obj.respawn();
         if (%col.client > 0)
            messageClient(%col.client, 'MsgItemPickup', '\c0You picked up %1.', %data.pickUpName);
      }
   }
}

function GrenadeThrown::onCollision(%data, %obj, %col)
{
   // nothing you can do now...
}

function HandInventory::onCollision(%data, %obj, %col)
{
   // %data = datablock of object; %obj = object number
   // %col = thing that collided with object (hopefully a player)
   if (%col.getDataBlock().className $= Armor)
   {
      %ammoName = %data.getName();
      %ammoStore = %col.inv[%ammoName];

      // if player has ammo pack, increase max amount of ammo
      if(%col.getMountedImage($BackpackSlot) != 0)
      {
         if(%col.getMountedImage($BackpackSlot).getName() $= "AmmoPackImage")
            %aMax = (%col.getDataBlock().max[%ammoName]) + AmmoPack.max[%ammoName];
         else
            %aMax = %col.getDataBlock().max[%ammoName];
      }
      else
         %aMax = %col.getDataBlock().max[%ammoName];

      if(%data.isGrenade)
      {
         // it's a grenade -- see if it matches the type the player is carrying
         %pgType = "None";
         for(%x = 0; $InvGrenade[%x] !$= ""; %x++)
         {
            %gren = $NameToInv[$InvGrenade[%x]];
            if(%col.inv[%gren] > 0)
            {
               %pgType = %gren;
               break;
            }
         }
         if((%pgType $= "None") || (%pgType $= %ammoName))
         {
            // player either has no grenades or this type of grenades -- OK to pick up more
            %canPickup = true;
         }
         else
         {
            // player has a different kind of grenade -- don't pick this kind up
            %canPickup = false;
         }
      }
      else if(%data.isMine) // z0dd - ZOD, 5/19/03. Check mines too!
      {
         // it's a mine -- see if it matches the type the player is carrying
         %pmType = "None";
         for(%y = 0; $InvMine[%y] !$= ""; %y++)
         {
            %mine = $NameToInv[$InvMine[%y]];
            if(%col.inv[%mine] > 0)
            {
               %pmType = %mine;
               break;
            }
         }
         if((%pmType $= "None") || (%pmType $= %ammoName))
         {
            // player either has no mines or this type of mine -- OK to pick up more
            %canPickup = true;
         }
         else
         {
            // player has a different kind of mine -- don't pick this kind up
            %canPickup = false;
         }
      }
      else
         %canPickup = true;

      if(%canPickup)
      {
         if(%col.inv[%ammoName] < %aMax)
         {
            //-------------------------------------------------------------------------------------------
            // z0dd - ZOD, 4/17/02. Don't allow player to pickup full ammo if they tossed less than full. 
            if( %obj.ammoStore $= "" )
               %obj.ammoStore = $AmmoIncrement[ %ammoName ];
            %col.incInventory(%ammoName, %obj.ammoStore);
            //-------------------------------------------------------------------------------------------
            serverPlay3D(ItemPickupSound, %col.getTransform());
            %obj.respawn();
            if (%col.client > 0)
               messageClient(%col.client, 'MsgItemPickup', '\c0You picked up %1.', %data.pickUpName);
         }
      }
   }
}

//-------------------------------------- Specific turret functions

function SentryTurret::onAdd(%data, %obj)
{
   Parent::onAdd(%data, %obj);

   //error("error");
   %obj.mountImage(%data.barrel, 0, true);
}

function TurretDeployedCamera::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);   
   %obj.mountImage(DeployableCameraBarrel, 0, true);
	%obj.setRechargeRate(%this.rechargeRate);

   %obj.setAutoFire(false); // z0dd - ZOD, 4/17/02. Server crash fix related to controlable cameras
}

function TurretDeployedCamera::onDestroyed(%this, %obj, %prevState)
{
   Parent::onDestroyed(%this, %obj, %prevState);
   $TeamDeployedCount[%obj.team, DeployedCamera]--;
   // doesn't seem to delete itself, so...
   %obj.schedule(500, "delete");
}

function ScoutFlyer::onTrigger(%data, %obj, %trigger, %state)
{
   // data = ScoutFlyer datablock
   // obj = ScoutFlyer object number
   // trigger = 0 for "fire", 1 for "jump", 3 for "thrust"
   // state = 1 for firing, 0 for not firing
   if(%trigger == 0)
   {
      switch (%state) {
         case 0:
            %obj.fireWeapon = false;
            %obj.setImageTrigger(2, false);
            %obj.setImageTrigger(3, false);
         case 1:
            %obj.fireWeapon = true;
            if(%obj.nextWeaponFire == 2) {
               %obj.setImageTrigger(2, true);
               %obj.setImageTrigger(3, false);
            }
            else {
               %obj.setImageTrigger(2, false);
               %obj.setImageTrigger(3, true);
            }
      }
   }
}

function ScoutFlyer::playerDismounted(%data, %obj, %player)
{
   %obj.fireWeapon = false;
   %obj.setImageTrigger(2, false);
   %obj.setImageTrigger(3, false);
   setTargetSensorGroup(%obj.getTarget(), %obj.team);

   if( %player.client.observeCount > 0 )
      resetObserveFollow( %player.client, true );
}

function ScoutChaingunImage::onFire(%data,%obj,%slot)
{
   // obj = ScoutFlyer object number
   // slot = 2

   Parent::onFire(%data,%obj,%slot);
   %obj.nextWeaponFire = 3;
   schedule(%data.fireTimeout, 0, "fireNextGun", %obj);
}

function ScoutChaingunPairImage::onFire(%data,%obj,%slot)
{
   // obj = ScoutFlyer object number
   // slot = 3

   Parent::onFire(%data,%obj,%slot);
   %obj.nextWeaponFire = 2;
   schedule(%data.fireTimeout, 0, "fireNextGun", %obj);
}

function fireNextGun(%obj)
{
   if(%obj.fireWeapon)
   {
      if(%obj.nextWeaponFire == 2)
      {
         %obj.setImageTrigger(2, true);
         %obj.setImageTrigger(3, false);
      }
      else
      {
         %obj.setImageTrigger(2, false);
         %obj.setImageTrigger(3, true);
      }
   }
   else
   {
      %obj.setImageTrigger(2, false);
      %obj.setImageTrigger(3, false);
   }
}

function ScoutChaingunImage::onTriggerDown(%this, %obj, %slot)
{
}

function ScoutChaingunImage::onTriggerUp(%this, %obj, %slot)
{
}

function ScoutChaingunImage::onMount(%this, %obj, %slot)
{
//   %obj.setImageAmmo(%slot,true);
}

function ScoutChaingunPairImage::onMount(%this, %obj, %slot)
{
//   %obj.setImageAmmo(%slot,true);
}

function ScoutChaingunImage::onUnmount(%this,%obj,%slot)
{
}

function ScoutChaingunPairImage::onUnmount(%this,%obj,%slot)
{
}


function BomberTurret::onDamage(%data, %obj)
{
   %newDamageVal = %obj.getDamageLevel();
   if(%obj.lastDamageVal !$= "")
      if(isObject(%obj.getObjectMount()) && %obj.lastDamageVal > %newDamageVal)   
         %obj.getObjectMount().setDamageLevel(%newDamageVal);
   %obj.lastDamageVal = %newDamageVal;
}

function BomberTurret::damageObject(%this, %targetObject, %sourceObject, %position, %amount, %damageType ,%vec, %client, %projectile)
{
   //If vehicle turret is hit then apply damage to the vehicle
   %vehicle = %targetObject.getObjectMount();
   if(%vehicle)
      %vehicle.getDataBlock().damageObject(%vehicle, %sourceObject, %position, %amount, %damageType, %vec, %client, %projectile);
}

function VehicleTurret::onEndSequence(%data, %obj, %thread)
{
   if($DeployThread == %thread)
      %obj.stopThread($DeployThread);
}

function BomberTurret::onTrigger(%data, %obj, %trigger, %state)
{
   //error("onTrigger: trigger = " @ %trigger @ ", state = " @ %state);
   //error("obj = " @ %obj @ ", class " @ %obj.getClassName());
   switch (%trigger)
   {
      case 0:
         %obj.fireTrigger = %state;
         if(%obj.selectedWeapon == 1)
         {
            %obj.setImageTrigger(4, false);
            if(%obj.getImageTrigger(6))
            {
               %obj.setImageTrigger(6, false);
               ShapeBaseImageData::deconstruct(%obj.getMountedImage(6), %obj);
            }
            if(%state)
               %obj.setImageTrigger(2, true);
            else
               %obj.setImageTrigger(2, false);
         }
         else if(%obj.selectedWeapon == 2)
         {
            %obj.setImageTrigger(2, false);
            if(%obj.getImageTrigger(6))
            {
               %obj.setImageTrigger(6, false);
               ShapeBaseImageData::deconstruct(%obj.getMountedImage(6), %obj);
            }
            if(%state)
               %obj.setImageTrigger(4, true);
            else
               %obj.setImageTrigger(4, false);
         } 
         else
         {
            %obj.setImageTrigger(2, false);
            %obj.setImageTrigger(4, false);
            if(%state)
               %obj.setImageTrigger(6, true);
            else
            {
               %obj.setImageTrigger(6, false);
               BomberTargetingImage::deconstruct(%obj.getMountedImage(6), %obj);
            }                   
         }

      case 2:
         if(%state)
         {
            %obj.getDataBlock().playerDismount(%obj);
         }
   }
}

function BomberTurret::playerDismount(%data, %obj)
{
   //Passenger Exiting
   %obj.fireTrigger = 0;
   %obj.setImageTrigger(2, false);
   %obj.setImageTrigger(4, false);
   if(%obj.getImageTrigger(6))
   {
      %obj.setImageTrigger(6, false);
      ShapeBaseImageData::deconstruct(%obj.getMountedImage(6), %obj);
   }
   %client = %obj.getControllingClient();
   %client.player.isBomber = false;
   commandToClient(%client, 'endBomberSight');
//   %client.player.setControlObject(%client.player);
   %client.player.mountVehicle = false;
//   %client.player.getDataBlock().doDismount(%client.player);

   //turret auto fire if ai mounted, if ai, %client = -1 - Lagg...
   if (%client > 0)
   {
      if(%client.player.getState() !$= "Dead")
         %client.player.mountImage(%client.player.lastWeapon, $WeaponSlot);
   }
   setTargetSensorGroup(%obj.getTarget(), 0);
   setTargetNeverVisMask(%obj.getTarget(), 0xffffffff);
}

//function BomberTurret::getHudNum(%data, %num)
//{
//   if(%num == 1)
//      return 0;
//   else
//      return 4;
//}

function AIAimingTurretBarrel::onFire(%this,%obj,%slot)
{
}

function BomberBombImage::onUnmount(%this,%obj,%slot)
{
}

function BomberBombPairImage::onUnmount(%this,%obj,%slot)
{
}

function BomberTurretBarrel::firePair(%this, %obj, %slot)
{
   %obj.setImageTrigger( 3, true);
}

function BomberTurretBarrelPair::stopFire(%this, %obj, %slot)
{
   %obj.setImageTrigger( 3, false);
}

function BomberTurretBarrelPair::onMount(%this, %obj, %slot)
{
//   %obj.setImageAmmo(%slot,true);
}

function BomberTurretBarrel::onMount(%this, %obj, %slot)
{
//   %obj.setImageAmmo(%slot,true);
}

function BomberBombImage::firePair(%this, %obj, %slot)
{
   %obj.setImageTrigger( 5, true);
}

function BomberBombPairImage::stopFire(%this, %obj, %slot)
{
   %obj.setImageTrigger( 5, false);
}

function BomberBombPairImage::onMount(%this, %obj, %slot)
{
//   %obj.setImageAmmo(%slot,true);
}

function BomberBombImage::onMount(%this, %obj, %slot)
{
}

function BomberBombImage::onUnmount(%this,%obj,%slot)
{
}

function BomberBombPairImage::onUnmount(%this,%obj,%slot)
{
}

function MobileTurretBase::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
   setTargetSensorGroup(%obj.target, %obj.team);
//   setTargetNeverVisMask(%obj.target, 0xffffffff); // z0dd - ZOD, 4/17/02. Causes mpb sensor to be shown in middle of map on cmd screen instead of MPB as origin. no idea why
}

function MobileTurretBase::onDamage(%data, %obj)
{
   %newDamageVal = %obj.getDamageLevel();
   if(%obj.lastDamageVal !$= "")
      if(isObject(%obj.getObjectMount()) && %obj.lastDamageVal > %newDamageVal)   
         %obj.getObjectMount().setDamageLevel(%newDamageVal);
   %obj.lastDamageVal = %newDamageVal;
}

function MobileTurretBase::damageObject(%this, %targetObject, %sourceObject, %position, %amount, %damageType ,%vec, %client, %projectile)
{
   //If vehicle turret is hit then apply damage to the vehicle
   %vehicle = %targetObject.getObjectMount();
   if(%vehicle)
      %vehicle.getDataBlock().damageObject(%vehicle, %sourceObject, %position, %amount, %damageType, %vec, %client, %projectile);
}

function MobileTurretBase::onEndSequence(%data, %obj, %thread)
{
   //Used so that the parent wont be called..
}

function AssaultPlasmaTurret::onDamage(%data, %obj)
{
   %newDamageVal = %obj.getDamageLevel();
   if(%obj.lastDamageVal !$= "")
      if(isObject(%obj.getObjectMount()) && %obj.lastDamageVal > %newDamageVal)   
         %obj.getObjectMount().setDamageLevel(%newDamageVal);
   %obj.lastDamageVal = %newDamageVal;
}

function AssaultPlasmaTurret::damageObject(%this, %targetObject, %sourceObject, %position, %amount, %damageType ,%vec, %client, %projectile)
{                                           
   //If vehicle turret is hit then apply damage to the vehicle
   %vehicle = %targetObject.getObjectMount();
   if(%vehicle)
      %vehicle.getDataBlock().damageObject(%vehicle, %sourceObject, %position, %amount, %damageType, %vec, %client, %projectile);
}

function AssaultPlasmaTurret::onTrigger(%data, %obj, %trigger, %state)
{
   switch (%trigger) {
      case 0:
         %obj.fireTrigger = %state;
         if(%obj.selectedWeapon == 1)
         {
            %obj.setImageTrigger(4, false);
            if(%state)
               %obj.setImageTrigger(2, true);
            else
               %obj.setImageTrigger(2, false);
         }                       
         else
         {
            %obj.setImageTrigger(2, false);
            if(%state)
               %obj.setImageTrigger(4, true);
            else           
               %obj.setImageTrigger(4, false);
         } 
      case 2:
         if(%state) 
         {
            %obj.getDataBlock().playerDismount(%obj);
         }
   }
}

function AssaultPlasmaTurret::playerDismount(%data, %obj)
{
   //Passenger Exiting
   %obj.fireTrigger = 0;
   %obj.setImageTrigger(2, false);
   %obj.setImageTrigger(4, false);
   %client = %obj.getControllingClient();
// %client.setControlObject(%client.player);
   //turret auto fire if ai mounted, if ai, %client = -1 - Lagg...
   if (%client > 0)
   {
      %client.player.mountImage(%client.player.lastWeapon, $WeaponSlot);
   }
   %client.player.mountVehicle = false;
   setTargetSensorGroup(%obj.getTarget(), 0);
   setTargetNeverVisMask(%obj.getTarget(), 0xffffffff);
//   %client.player.getDataBlock().doDismount(%client.player);
}

//function AssaultPlasmaTurret::getHudNum(%data, %num)
//{
//   if(%num == 1)
//      return 1;
//   else
//      return 3;
//}


// ------------------------------------------
// camera functions
// ------------------------------------------

$CameraDeployTime = 1000;
$CameraDeployCheckMax = 6;
$CameraMinVelocity = 0.1;

function CameraGrenadeThrown::onThrow(%this, %gren)
{
   // schedule a check to see if the camera is at rest but not deployed
   %gren.checkCount = 0;
   %gren.velocCheck = %this.schedule($CameraDeployTime, "checkCameraDeploy", %gren);
}

function CameraGrenadeThrown::onStickyCollision(%data, %obj)
{
   cancel(%obj.velocCheck);
   %pos = %obj.getLastStickyPos();
   %norm = %obj.getLastStickyNormal();
   
   %intAngle = getTerrainAngle(%norm);  // staticShape.cs
   %rotAxis = vectorNormalize(vectorCross(%norm, "0 0 1"));
   if (getWord(%norm, 2) == 1 || getWord(%norm, 2) == -1)
      %rotAxis = vectorNormalize(vectorCross(%norm, "0 1 0"));

   %rotation = %rotAxis @ " " @ %intAngle;
   %dcSucc = activateCamera(%pos, %rotation, %obj.sourceObject, %obj.sourceObject.team);
   if(%dcSucc == 0)
      messageClient(%obj.sourceObject.client, 'MsgDeployFailed', '\c2Your team\'s control network has reached its capacity for this item.~wfx/misc/misc.error.wav');
   %obj.schedule(50,"delete");
}

function CameraGrenadeThrown::checkCameraDeploy(%this, %gren)
{
   %gren.checkCount++;
   if(VectorLen(%gren.getVelocity()) < $CameraMinVelocity)
   {
      // camera has come to rest but not deployed -- probably on a staticshape (station, gen, etc)
      // no resolution, so get rid of it
      %gren.schedule(50, "delete");
   }
   else if(%gren.checkCount >= $CameraDeployCheckMax)
   {
      // camera's still moving but it's been check several times -- it was thrown from too great
      // a height or off the edge of the world -- delete it
      %gren.schedule(50, "delete");
   }
   else
   {
      // check back in a little while
      %gren.velocCheck = %this.schedule($CameraDeployTime, "checkCameraDeploy", %gren);
   }
}

function activateCamera(%position, %rotation, %sourceObj, %team)
{
   if($TeamDeployedCount[%team, DeployedCamera] >= $TeamDeployableMax[DeployedCamera])
   {
      // team has too many cameras deployed already, don't deploy this one
      return 0;
   }
   %dCam = new Turret()
   {
      dataBlock = "TurretDeployedCamera";                            
      team = %team;
      needsNoPower = true;
      owner = %sourceObj.client;
      ownerHandle = %sourceObj.client.handle;
      position = %position;
      rotation = %rotation;
   };
   addToDeployGroup(%dCam);

   if(%dCam.getTarget() != -1)
      setTargetSensorGroup(%dCam.getTarget(), %team);

   %dCam.playAudio($DeploySound, CameraGrenadeAttachSound);
   %dCam.deploy();
   %dCam.playThread($AmbientThread, "ambient");

   // increment team's deployed count for cameras
   $TeamDeployedCount[%team, DeployedCamera]++;
   return 1;
}

function FlareGrenade::onUse(%this, %obj)
{
   // a stripped-down version of HandInventory::onUse from weapons.cs
   if(Game.handInvOnUse(%data, %obj)) {
      %obj.decInventory(%this, 1);
      %p = new FlareProjectile() {
         dataBlock        = FlareGrenadeProj;
         initialDirection = %obj.getEyeVector();
         initialPosition  = getBoxCenter(%obj.getWorldBox());
         sourceObject     = %obj;
         sourceSlot       = 0;
      };
      FlareSet.add(%p);
      MissionCleanup.add(%p);
      serverPlay3D(GrenadeThrowSound, getBoxCenter(%obj.getWorldBox()));
      %p.schedule(6000, "delete");
      // miscellaneous grenade-throwing cleanup stuff
      %obj.lastThrowTime[%data] = getSimTime();
      %obj.throwStrength = 0;
   }
}

// uncomment when explosion type can be set from script (dont want underwater explosion here)
//function grenadeOnEnterLiquid(%data, %obj, %coverage, %type, %flash)
//{
//   // 4: Lava 
//   // 5: Hot Lava
//   // 6: Crusty Lava
//   if(%type >=4 && %type <= 6)
//   {
//      if(%obj.getDamageState() !$= "Destroyed")
//      {
//         cancel(%obj.detThread);
//         if(%flash)
//            detonateFlashGrenade(%obj);
//         else
//            detonateGrenade(%obj);
//         return(true);
//      }
//   }
//   
//   // flash grenades do not ignore quicksand
//   if((%type == 7) && !%flash)
//      return(true);
//
//   return(false);
//}

function GrenadeThrown::onThrow(%this, %gren)
{
   AIGrenadeThrown(%gren);
   %gren.detThread = schedule(1500, %gren, "detonateGrenade", %gren);
}

//function GrenadeThrown::onEnterLiquid(%data, %obj, %coverage, %type)
//{
//   if(!grenadeOnEnterLiquid(%data, %obj, %coverage, %type, false))
//      Parent::onEnterLiquid(%data, %obj, %coverage, %type);
//}

function ConcussionGrenadeThrown::onThrow(%this, %gren)
{
   AIGrenadeThrown(%gren);
   %gren.detThread = schedule(2000, %gren, "detonateGrenade", %gren);
}

//function ConcussionGrenadeThrown::onEnterLiquid(%data, %obj, %coverage, %type)
//{
//   if(!grenadeOnEnterLiquid(%data, %obj, %coverage, %type, false))
//      Parent::onEnterLiquid(%data, %obj, %coverage, %type);
//}

function detonateGrenade(%obj)
{
   %obj.setDamageState(Destroyed);
   %data = %obj.getDataBlock();
   RadiusExplosion( %obj, %obj.getPosition(), %data.damageRadius, %data.indirectDamage, 
                   %data.kickBackStrength, %obj.sourceObject, %data.radiusDamageType);
   %obj.schedule(500,"delete");
}

function FlashGrenadeThrown::onThrow(%this, %gren)
{
   %gren.detThread = schedule(2000, %gren, "detonateFlashGrenade", %gren);
}

//function FlashGrenadeThrown::onEnterLiquid(%data, %obj, %coverage, %type)
//{
//   if(!grenadeOnEnterLiquid(%data, %obj, %coverage, %type, true))
//      Parent::onEnterLiquid(%data, %obj, %coverage, %type);
//}

function detonateFlashGrenade(%hg)
{
   %maxWhiteout = %hg.getDataBlock().maxWhiteout;
   %thrower = %hg.sourceObject.client;
   %hg.setDamageState(Destroyed);   
   %hgt = %hg.getTransform();
   %plX = firstword(%hgt);
   %plY = getWord(%hgt, 1);
   %plZ = getWord(%hgt, 2);
   %pos = %plX @ " " @ %plY @ " " @ %plZ;
   //all this stuff below ripped from projectiles.cs

   InitContainerRadiusSearch(%pos, 100.0, $TypeMasks::PlayerObjectType |
                                          $TypeMasks::TurretObjectType);

   while ((%damage = containerSearchNext()) != 0)
   {
      %dist = containerSearchCurrDist();

      %eyeXF = %damage.getEyeTransform();
      %epX   = firstword(%eyeXF);
      %epY   = getWord(%eyeXF, 1);
      %epZ   = getWord(%eyeXF, 2);
      %eyePos = %epX @ " " @ %epY @ " " @ %epZ;
      %eyeVec = %damage.getEyeVector();

      // Make sure we can see the thing...
      if (ContainerRayCast(%eyePos, %pos, $TypeMasks::TerrainObjectType |
                                          $TypeMasks::InteriorObjectType |
                                          $TypeMasks::StaticObjectType, %damage) !$= "0")
      {
         continue;
      }

      %distFactor = 1.0;
      if (%dist >= 100)
         %distFactor = 0.0;
      else if (%dist >= 20) {
         %distFactor = 1.0 - ((%dist - 20.0) / 80.0);
      }

      %dif = VectorNormalize(VectorSub(%pos, %eyePos));
      %dot = VectorDot(%eyeVec, %dif);

      %difAcos = mRadToDeg(mAcos(%dot));
      %dotFactor = 1.0;
      if (%difAcos > 60)
         %dotFactor = ((1.0 - ((%difAcos - 60.0) / 120.0)) * 0.2) + 0.3;
      else if (%difAcos > 45)
         %dotFactor = ((1.0 - ((%difAcos - 45.0) / 15.0)) * 0.5) + 0.5;

      %totalFactor = %dotFactor * %distFactor;
              
	  %prevWhiteOut = %damage.getWhiteOut();

		if(!%prevWhiteOut)
			if(!$teamDamage)
			{
				if(%damage.client != %thrower && %damage.client.team == %thrower.team)
					messageClient(%damage.client, 'teamWhiteOut', '\c1You were hit by %1\'s whiteout grenade.', getTaggedString(%thrower.name)); 
			}
		
      %whiteoutVal = %prevWhiteOut + %totalFactor;
      if(%whiteoutVal > %maxWhiteout)
      {
        //error("whitout at max");
        %whiteoutVal = %maxWhiteout;
      }
      //bot cheat! don't blind the thrower - Lagg... 1-8-2004 
      if (%damage.client == %thrower && %thrower.isAIControlled())
         continue;

      %damage.setWhiteOut( %whiteoutVal );
   }
   %hg.schedule( 500, "delete" );
}

// ----------------------------------------------
// mine functions
// ----------------------------------------------


function MineDeployed::onThrow(%this, %mine, %thrower)
{
   %mine.armed = false;
   %mine.damaged = 0;
   %mine.detonated = false;
   %mine.depCount = 0;
   %mine.theClient = %thrower.client;
   $TeamDeployedCount[%mine.sourceObject.team, MineDeployed]++; // z0dd - ZOD, 8/13/02, Moved this from deployMineCheck to here. Fixes mine count bug

   schedule(1500, %mine, "deployMineCheck", %mine, %thrower);
}

function deployMineCheck(%mineObj, %player)
{
   if(%mineObj.depCount > %mineObj.getDatablock().maxDepCount)
      explodeMine(%mineObj, true);
   
   // wait until the mine comes to rest
   if(%mineObj.getVelocity() $= "0 0 0")
   {
      // 2-second delay before mine is armed -- let deploy thread play out etc.
      schedule(%mineObj.getDatablock().armTime, %mineObj, "armDeployedMine", %mineObj);

      // check for other deployed mines in the vicinity
      InitContainerRadiusSearch(%mineObj.getWorldBoxCenter(), %mineObj.getDatablock().spacing, $TypeMasks::ItemObjectType);
      while((%itemObj = containerSearchNext()) != 0)
      {
         if(%itemObj == %mineObj)
            continue;
         %ioType = %itemObj.getDatablock().getName();
         if(%ioType $= "MineDeployed")
            schedule(100, %mineObj, "explodeMine", %mineObj, true);
         else
            continue;
      }
      // play "deploy" thread
      %mineObj.playThread(0, "deploy");
      serverPlay3D(MineDeploySound, %mineObj.getTransform());
      %mineTeam = %mineObj.sourceObject.team;
      //$TeamDeployedCount[%mineTeam, MineDeployed]++; // z0dd - ZOD, 8/13/02, Moved the increment to MineDeployed::onThrow. Fixes mine count bug
      if($TeamDeployedCount[%mineTeam, MineDeployed] > $TeamDeployableMax[MineDeployed])
      {   
         messageClient( %player.client, '', 'Maximum allowable mines deployed.' );
         schedule(100, %mineObj, "explodeMine", %mineObj, true);
      }
      else
      {
         //start the thread that keeps checking for objects near the mine...
         mineCheckVicinity(%mineObj);

         //let the AI know *after* it's come to rest...
         AIDeployMine(%mineObj);

         //let the game know there's a deployed mine
         Game.notifyMineDeployed(%mineObj);
      }
   }
   else
   {
      //schedule this deploy check again a little later
      %mineObj.depCount++;
      schedule(500, %mineObj, "deployMineCheck", %mineObj, %player);
   }
}

function armDeployedMine(%mine)
{
   %mine.armed = true;
}

function mineCheckVicinity(%mine)
{
   // this function is called after the mine has been deployed. It will check the
   // immediate area around the mine (2.5 meters at present) for players or vehicles
   // passing by, and detonate if any are found. This is to extend the range of the
   // mine so players don't have to collide with them to set them off.

   // don't bother to check if mine isn't armed yet
   if(%mine.armed)
   {
      // don't keep checking if mine is already detonating
      if(!%mine.boom)
      {
         // the actual check for objects in the area
         %mineLoc = %mine.getWorldBoxCenter();
         %masks = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType;
         %detonateRange = %mine.getDatablock().proximity;
         %noExplode = 0;
         InitContainerRadiusSearch(%mineLoc, %detonateRange, %masks);
         while((%tgt = containerSearchNext()) != 0) 
         {
            if(!$TeamDamage)
            {
               if(%mine.team == %tgt.team)
                  %noExplode = 1;
            }
            if(%noExplode == 0)
            {
               %mine.detonated = true;
               schedule(50, %mine, "explodeMine", %mine, false);
               break;
            }
         }
      }
   }
   // if nothing set off the mine, schedule another check
   if(!%mine.detonated)
      schedule(300, %mine, "mineCheckVicinity", %mine);
}

function MineDeployed::onCollision(%data, %obj, %col)
{
   // don't detonate if mine isn't armed yet
   if(!%obj.armed)
      return;

   // don't detonate if mine is already detonating
   if(%obj.boom)
      return;

   %noExplode = 0;
   //check to see what it is that collided with the mine
   %struck = %col.getClassName();
   if(%struck $= "Player" || %struck $= "WheeledVehicle" || %struck $= "FlyingVehicle")
   {
      if(!$teamDamage)
      {
         if(%obj.team == %col.getOwnerClient().team)
            %noExplode = 1;
      }
      if(%noExplode == 0)
      {
         //error("Mine detonated due to collision with #"@%col@" ("@%struck@"); armed = "@%obj.armed);
         explodeMine(%obj, false);
      }
   }
}

function explodeMine(%mo, %noDamage)
{
   %mo.noDamage = %noDamage;
   %mo.setDamageState(Destroyed);
}

function MineDeployed::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
   // -----------------------------
   // z0dd - ZOD, 5/09/04. If gameplay changes in affect, no mine disc
   if($Host::ClassicLoadMineChanges)
   {
      if(!%targetObject.armed)
         return;
   }
   // -----------------------------
     
   if(%targetObject.boom)
      return;

   %targetObject.damaged += %amount;

   if(%targetObject.damaged >= %data.maxDamage)
   {   
      %targetObject.setDamageState(Destroyed);
   }
}

function MineDeployed::onDestroyed(%data, %obj, %lastState)
{
   %obj.boom = true;
   %mineTeam = %obj.team;
   $TeamDeployedCount[%mineTeam, MineDeployed]--;
   // %noDamage is a boolean flag -- don't want to set off all other mines in
   // vicinity if there's a "mine overload", so apply no damage/impulse if true
   if(!%obj.noDamage)
       RadiusExplosion(%obj,
                      %obj.getPosition(),
                      %data.damageRadius,
                      %data.indirectDamage,
                      %data.kickBackStrength,
                      %obj.sourceObject,
                      %data.radiusDamageType);

   %obj.schedule(600, "delete");
}
