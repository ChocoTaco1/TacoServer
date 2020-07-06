//----------------------------------------------------------------------------

// When first mounted (assuming there is ammo):
//    SingleShot     activate -> ready
//    Spinning       activate -> idle (spin 0)
//    Sustained      activate -> ready
//    DiscLauncher   activate -> reload -> spinup -> ready
//
// Normal operation:
//    SingleShot     ready -> fire -> reload -> ready
//    Spinning       idle (spin 0) -> spinup -> ready -> fire -> spindown -> idle
//    Sustained      ready -> fire -> reload -> ready
//    DiscLauncher   ready -> fire -> reload -> spinup -> ready

// Image properties
//    emap
//    preload
//    shapeFile
//    mountPoint
//    offset
//    rotation
//    firstPerson
//    mass
//    usesEnergy
//    minEnergy
//    accuFire
//    lightType
//    lightTime
//    lightRadius
//    lightColor

// Image state variables
//    stateName
//    stateTransitionOnLoaded
//    stateTransitionOnNotLoaded
//    stateTransitionOnAmmo
//    stateTransitionOnNoAmmo
//    stateTransitionOnTriggerUp
//    stateTransitionOnTriggerDown
//    stateTransitionOnTimeout
//    stateTimeoutValue
//    stateFire
//    stateEnergyDrain
//    stateAllowImageChange
//    stateScaleAnimation
//    stateDirection
//    stateLoadedFlag
//    stateSpinThread
//    stateRecoil
//    stateSequence
//    stateSound
//    stateScript
//    stateEmitter
//    stateEmitterTime
//    stateEmitterNode

//----------------------------------------------------------------------------

$ItemRespawnTime = 30000;
$ItemPopTime = 30 * 1000; // 30 seconds

$WeaponSlot = 0;
$AuxiliarySlot = 1;
$BackpackSlot = 2;
$FlagSlot = 3;

//----------------------------------------------------------------------------
datablock EffectProfile(ItemPickupEffect)
{
    effectname = "packs/packs.pickupPack";
    minDistance = 2.5;
};

datablock AudioProfile(ItemPickupSound)
{
   filename = "fx/packs/packs.pickuppack.wav";
   description = AudioClosest3d;
   effect = ItemPickupEffect;
   preload = true;
};

datablock EffectProfile(ItemThrowEffect)
{
   effectname = "packs/packs.throwpack";
   minDistance = 2.5;
	maxDistance = 2.5;
};

datablock AudioProfile(ItemThrowSound)
{
   filename = "fx/packs/packs.throwpack.wav";
   description = AudioClosest3d;
   effect = ItemThrowEffect;
   preload = true;
};

datablock AudioProfile(RepairPatchSound)
{
   filename    = "fx/misc/health_patch.wav";
   description = AudioClosest3d;
   preload = true;
   effect = ItemPickupEffect;
   preload = true;
};

function ItemData::create(%block)
{
   if(%block $= "flag")
      %obj = new Item() {
         className = FlagObj;
         dataBlock = %block;
         static = false;
         rotate = false;
      };
   else
      %obj = new Item() {
         dataBlock = %block;
         static = true;
         //rotate = true;
         // don't make "placed items" rotate
         rotate = false;
      };
   return(%obj);
}

//--------------------------------------------------------------------------
function Item::schedulePop(%this)
{
   %itemFadeTime = 1000; // items will take 1 second (1000 milliseconds) to fade out
   %this.startFade(%itemFadeTime, $ItemPopTime - %itemFadeTime, true);
   %this.schedule($ItemPopTime, "delete");
}   

function Item::respawn(%this)
{
   %this.startFade(0, 0, true);
   %this.schedule($ItemRespawnTime + 100, "startFade", 1000, 0, false);
   %this.hide(true);
   %this.schedule($ItemRespawnTime, "hide", false);
}

function ItemData::onThrow(%data,%obj,%shape)
{
   serverPlay3D(ItemThrowSound, %obj.getTransform());
   // don't schedule a delete for satchelCharges when they're deployed
   if(!%data.noTimeout)
      %obj.schedulePop();
}

function ItemData::onInventory(%data,%shape,%value)
{
   if (!%value) {
      // If we don't have any more of these items, make sure
      // we don't have an image mounted.
      %slot = %shape.getMountSlot(%data.image);
      if (%slot != -1)
         %shape.unmountImage(%slot);
   }
}

// z0dd - ZOD - Founder, 5/18/03. ItemData pickup Parent. Streamline
// Function also servers as console spam fix related to item pickup
function ItemData::onPickup(%this, %pack, %player, %amount)
{
   // %this = Pack datablock
   // %pack = Pack object number
   // %player = player
   // %amount = amount picked up (1)

   if(%pack.sensors !$= "")
   {
      // find out how many sensor were in the pack
      %player.deploySensors = %pack.sensors;
      %player.client.updateSensorPackText(%player.deploySensors);
   }
}

function ItemData::onEnterLiquid(%data, %obj, %coverage, %type)
{
   if(%data.isInvincible)
      return;
      
   switch(%type)
   {
      case 0:
         //Water
      case 1:
         //Ocean Water
      case 2:
         //River Water
      case 3:
         //Stagnant Water
      case 4:
         //Lava
         %obj.delete();
      case 5:
         //Hot Lava
         %obj.delete();
      case 6:    
         //Crusty Lava
         %obj.delete();
      case 7:
         //Quick Sand
   }
}

function ItemData::onLeaveLiquid(%data, %obj, %type)
{
   // dummy
}

function ItemData::onCollision(%data,%obj,%col)
{
   // Default behavior for items is to get picked 
   // by the colliding object.
   if (%col.getDataBlock().className $= Armor && %col.getState() !$= "Dead")
   {
      if (%col.isMounted())
         return;

      if (%col.pickup(%obj, 1))
      {
         if (%col.client)
         {
            messageClient(%col.client, 'MsgItemPickup', '\c0You picked up %1.', %data.pickUpName);
            serverPlay3D(ItemPickupSound, %col.getTransform());
         }
         if (%obj.isStatic())
            %obj.respawn();
         else
            %obj.delete();
      }
   }
}

//----------------------------------------------------------------------------
datablock ItemData(RepairKit)
{
   className = HandInventory;
   catagory = "Misc";
   shapeFile = "repair_kit.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2.0;
   pickUpName = "a repair kit";
   alwaysAmbient = true;

   computeCRC = true;
   emap = true;

};

function RepairKit::onUse(%data,%obj)
{ 
   //----------------------------------------------------------------------------
   // z0dd - ZOD, 8/10/02. Let players use repair kit regardless of health status
   // if they choose so via client $pref::
   if (%obj.client.wasteRepKit == 1)
   {
      %obj.decInventory(%data,1);
      messageClient(%obj.client, 'MsgRepairKitUsed', '\c2Repair Kit Used.');
      if (%obj.getDamageLevel() != 0)
      {
         %obj.applyRepair(0.2);
      }
   }
   else
   {
      // Don't use the kit unless we're damaged
      if (%obj.getDamageLevel() != 0)
      {
         %obj.applyRepair(0.2);
         %obj.decInventory(%data,1);
         messageClient(%obj.client, 'MsgRepairKitUsed', '\c2Repair Kit Used.');
      }
   }
}

function serverCmdSetRepairKitWaste(%client, %value)
{
   %val = deTag(%value);
   %client.wasteRepKit = %val;
}

//----------------------------------------------------------------------------

datablock ItemData(RepairPatch)
{
   catagory = "Misc";
   shapeFile = "repair_patch.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2.0;
   pickUpName = "a repair patch";
   alwaysAmbient = true;

   computeCRC = true;
   emap = true;

};

function RepairPatch::onCollision(%data,%obj,%col)
{
   if ( %col.getDataBlock().className $= Armor 
     && %col.getDamageLevel() != 0
     && %col.getState() !$= "Dead" ) 
   {
      if (%col.isMounted())
         return;

      %col.playAudio(0, RepairPatchSound);
      %col.applyRepair(0.125);
      %obj.respawn();
      if (%col.client > 0)
         messageClient(%col.client, 'MsgItemPickup', '\c0You picked up %1.', %data.pickUpName);
   }
}

//----------------------------------------------------------------------------
// Flag:
//----------------------------------------------------------------------------
datablock ShapeBaseImageData(FlagImage)
{
   shapeFile = "flag.dts";
   item = Flag;
   mountPoint = 2;
   offset = "0 0 0";

   lightType = "PulsingLight";
   lightColor = "0.5 0.5 0.5 1.0";
   lightTime = "1000";
   lightRadius = "3";
   cloakable = false;
};

datablock ItemData(Flag)
{
   catagory = "Objectives";
   shapefile = "flag.dts";
   mass = 80; // z0dd - ZOD, 3/27/02. Keep flag from flying all over when damaged. was 55.
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 4.0; // z0dd - ZOD, 8/11/02. Was 3
   pickUpName = "a flag";
   computeCRC = true;

   lightType = "PulsingLight";
   lightColor = "0.5 0.5 0.5 1.0";
   lightTime = "1000";
   lightRadius = "3";

   isInvincible = true;
   cmdCategory = "Objectives";
   cmdIcon = CMDFlagIcon;
   cmdMiniIconName = "commander/MiniIcons/com_flag_grey";
   targetTypeTag = 'Flag';

   //used in CTF to mark the flag during a stalemate...
   hudImageNameFriendly[1] = "commander/MiniIcons/com_flag_grey";
   hudImageNameEnemy[1] = "commander/MiniIcons/com_flag_grey";
   hudRenderModulated[1] = true;
   hudRenderAlways[1] = true;
   hudRenderCenter[1] = true;
   hudRenderDistance[1] = true;
   hudRenderName[1] = true;
};

//----------------------------------------------------------------------------
function Flag::onThrow(%data,%obj,%src)
{
   Game.playerDroppedFlag(%src);   
}

function Flag::onAdd(%this, %obj)
{
   // make sure flags play "flapping" ambient thread
   Parent::onAdd(%this, %obj);
   %obj.playThread($AmbientThread, "ambient");

   %blocker = new VehicleBlocker()
   {
      position = %obj.position;
      rotation = %obj.rotation;
      dimensions = "2 2 4";
   };
   MissionCleanup.add(%blocker);
}

function Flag::onCollision(%data,%obj,%col)
{
   if (%col.getDataBlock().className $= Armor)
   {
      if (%col.isMounted())
         return;

      // z0dd - ZOD, 6/13/02. Touch the flag and your invincibility and cloaking goes away.
      if(%col.station $= "" && %col.isCloaked())
      {
         if( %col.respawnCloakThread !$= "" )
         {
            Cancel(%col.respawnCloakThread);
            %col.setCloaked( false );
            %col.respawnCloakThread = "";
         }
      }
      if( %col.client > 0 )
      {
         %col.setInvincibleMode(0, 0.00);
         %col.setInvincible( false );
      }

      // a player hit the flag
      Game.playerTouchFlag(%col, %obj);
   }
}

//----------------------------------------------------------------------------
// HuntersFlag:
//----------------------------------------------------------------------------
datablock ShapeBaseImageData(HuntersFlagImage)
{
   shapeFile = "Huntersflag.dts";
   item = Flag;
   mountPoint = 2;
   offset = "0 0 0";

   lightType = "PulsingLight";
   lightColor = "0.5 0.5 0.5 1.0";
   lightTime = "1000";
   lightRadius = "3";
};

// 1: red
// 2: blue
// 4: yellow
// 8: green
datablock ItemData(HuntersFlag1)
{
   className = HuntersFlag;

   shapefile = "Huntersflag.dts";
   mass = 80; // z0dd - ZOD, 3/27/02. Keep flag from flying all over when damaged. was 55.
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 4.0; // z0dd - ZOD, 8/11/02. Was 3
   isInvincible = true;
   pickUpName = "a flag";
   //computeCRC = true; // z0dd - ZOD, 5/18/03. This causes several crc checks, moved.

   lightType = "PulsingLight";
   lightColor = "0.8 0.2 0.2 1.0";
   lightTime = "1000";
   lightRadius = "3";
};

datablock ItemData(HuntersFlag2) : HuntersFlag1
{
   lightColor = "0.2 0.2 0.8 1.0";
   computeCRC = true; // z0dd - ZOD, 5/18/03. Check this model just once.
};

datablock ItemData(HuntersFlag4) : HuntersFlag1
{
   lightColor = "0.8 0.8 0.2 1.0";
};

datablock ItemData(HuntersFlag8) : HuntersFlag1
{
   lightColor = "0.2 0.8 0.2 1.0";
};

function HuntersFlag::onRemove(%data, %obj)
{
   // dont want target removed...
}

function HuntersFlag::onThrow(%data,%obj,%src)
{
   Game.playerDroppedFlag(%src);   
}

function HuntersFlag::onCollision(%data,%obj,%col)
{
   if (%col.getDataBlock().className $= Armor)
   {
      if (%col.isMounted())
         return;

      // a player hit the flag
      Game.playerTouchFlag(%col, %obj);
   }
}

//----------------------------------------------------------------------------
// Nexus:
//----------------------------------------------------------------------------
datablock ItemData(Nexus)
{
   catagory = "Objectives";
   shapefile = "nexus_effect.dts";
   mass = 10;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   icon = "CMDNexusIcon";
   targetTypeTag = 'Nexus';

   computeCRC = true;

};

datablock ParticleData(NexusParticleDenied)
{
   dragCoeffiecient     = 0.4;
   gravityCoefficient   = 3.0;
   inheritedVelFactor   = 0.0;

   lifetimeMS           = 1200;
   lifetimeVarianceMS   = 400;

   textureName          = "particleTest";

   useInvAlpha =  false;
   spinRandomMin = -200.0;
   spinRandomMax =  200.0;

   colors[0]     = "0.3 0.0 0.0 1.0";
   colors[1]     = "0.5 0.0 0.0 0.5";
   colors[2]     = "0.7 0.0 0.0 0.0";
   sizes[0]      = 0.2;
   sizes[1]      = 0.1;
   sizes[2]      = 0.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(NexusParticleDeniedEmitter)
{
   ejectionPeriodMS = 2;
   ejectionOffset = 0.2;
   periodVarianceMS = 0.5;
   ejectionVelocity = 10.0;
   velocityVariance = 4.0;
   thetaMin         = 0.0;
   thetaMax         = 30.0;
   lifetimeMS       = 0;

   particles = "NexusParticleDenied";
};

datablock ParticleData(NexusParticleCap)
{
   dragCoeffiecient     = 0.4;
   gravityCoefficient   = 3.0;
   inheritedVelFactor   = 0.0;

   lifetimeMS           = 1200;
   lifetimeVarianceMS   = 400;

   textureName          = "particleTest";

   useInvAlpha =  false;
   spinRandomMin = -200.0;
   spinRandomMax =  200.0;

   colors[0]     = "0.5 0.8 0.2 1.0";
   colors[1]     = "0.6 0.9 0.3 1.0";
   colors[2]     = "0.7 1.0 0.4 1.0";
   sizes[0]      = 0.2;
   sizes[1]      = 0.1;
   sizes[2]      = 0.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(NexusParticleCapEmitter)
{
   ejectionPeriodMS = 2;
   ejectionOffset = 0.5;
   periodVarianceMS = 0.5;
   ejectionVelocity = 10.0;
   velocityVariance = 4.0;
   thetaMin         = 0.0;
   thetaMax         = 30.0;
   lifetimeMS       = 0;

   particles = "NexusParticleCap";
};

//----------------------------------------------------------------------------

function getVector(%string, %num)
{
   %start = %num * 3;
   return getWords(%string,%start, %start + 2); 
}

// --------------------------------------------
// explosion datablock
// --------------------------------------------

datablock ExplosionData(DeployablesExplosion)
{
   soundProfile = DeployablesExplosionSound;
   faceViewer = true;

   explosionShape = "effect_plasma_explosion.dts";
   sizes[0] = "0.2 0.2 0.2";
   sizes[1] = "0.3 0.3 0.3";
};

$TeamDeployableMax[TargetBeacon] = 10;
$TeamDeployableMax[MarkerBeacon] = 20;

datablock ItemData(Beacon)
{
   className = HandInventory;
   catagory = "Misc";
   shapeFile = "beacon.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.8;
   pickupRadius = 1;
   pickUpName = "a deployable beacon";

   computeCRC = true;

};

datablock StaticShapeData(DeployedBeacon) : StaticShapeDamageProfile
{
   shapeFile = "beacon.dts";
   explosion = DeployablesExplosion;
   maxDamage = 0.45;
   disabledLevel = 0.45;
   destroyedLevel = 0.45;
   targetNameTag = 'beacon';
   deployedObject = true;
   dynamicType = $TypeMasks::SensorObjectType;
   debrisShapeName = "debris_generic_small.dts";
   debris = SmallShapeDebris;
   damageScale[$DamageType::Mine] = 1.0; // z0dd - ZOD, 5/17/03. Kill beacons that mark mines.
};

function DeployedBeacon::onDestroyed(%data, %obj, %prevState)
{
   if(%obj.getBeaconType() $= "friend")
      %bType = "MarkerBeacon";
   else
      %bType = "TargetBeacon";
   $TeamDeployedCount[%obj.team, %bType]--;
   %obj.schedule(500, delete);
}

function Beacon::onUse(%data, %obj)
{
   // look for 3 meters along player's viewpoint for interior or terrain
   %searchRange = 3.0;
   %mask = $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::StaticShapeObjectType | $TypeMasks::ForceFieldObjectType;
   // get the eye vector and eye transform of the player
   %eyeVec = %obj.getEyeVector();
   %eyeTrans = %obj.getEyeTransform();
   // extract the position of the player's camera from the eye transform (first 3 words)
   %eyePos = posFromTransform(%eyeTrans);
   // normalize the eye vector
   %nEyeVec = VectorNormalize(%eyeVec);
   // scale (lengthen) the normalized eye vector according to the search range
   %scEyeVec = VectorScale(%nEyeVec, %searchRange);
   // add the scaled & normalized eye vector to the position of the camera
   %eyeEnd = VectorAdd(%eyePos, %scEyeVec);
   // see if anything gets hit
   %searchResult = containerRayCast(%eyePos, %eyeEnd, %mask, 0);
   if(!%searchResult )
   {
      // no terrain/interior collision within search range
      if(%obj.inv[%data.getName()] > 0)
         messageClient(%obj.client, 'MsgBeaconNoSurface', '\c2Cannot place beacon. Too far from surface.');
      return 0;
   }
   else
   {
      %searchObj = GetWord(%searchResult, 0);
      if(%searchObj.getType() & ($TypeMasks::StaticShapeObjectType | $TypeMasks::ForceFieldObjectType) )
      {
         // if there's already a beacon where player is aiming, switch its type
         // otherwise, player can't deploy a beacon there
         if(%searchObj.getDataBlock().getName() $= DeployedBeacon)
            switchBeaconType(%searchObj);
         else
            messageClient(%obj.client, 'MsgBeaconNoSurface', '\c2Cannot place beacon. Not a valid surface.');
         return 0;
      }
      else if(%obj.inv[%data.getName()] <= 0) 
         return 0;   
   }
   // newly deployed beacons default to "target" type
   if($TeamDeployedCount[%obj.team, TargetBeacon] >= $TeamDeployableMax[TargetBeacon])
   {
      messageClient(%obj.client, 'MsgDeployFailed', '\c2Your team\'s control network has reached its capacity for this item.~wfx/misc/misc.error.wav');
      return 0;
   }
   %terrPt = posFromRaycast(%searchResult);
   %terrNrm = normalFromRaycast(%searchResult);

   %intAngle = getTerrainAngle(%terrNrm);  // getTerrainAngle() function found in staticShape.cs
   %rotAxis = vectorNormalize(vectorCross(%terrNrm, "0 0 1"));
   if (getWord(%terrNrm, 2) == 1 || getWord(%terrNrm, 2) == -1)
      %rotAxis = vectorNormalize(vectorCross(%terrNrm, "0 1 0"));
   %rotation = %rotAxis @ " " @ %intAngle;

   %obj.decInventory(%data, 1);
   %depBeac = new BeaconObject() {
      dataBlock = "DeployedBeacon";
      position = VectorAdd(%terrPt, VectorScale(%terrNrm, 0.05));
      rotation = %rotation;
   };
   $TeamDeployedCount[%obj.team, TargetBeacon]++;
   
   %depBeac.playThread($AmbientThread, "ambient");
   %depBeac.team = %obj.team;
   %depBeac.sourceObject = %obj;

   // give it a team target
   %depBeac.setTarget(%depBeac.team);
   MissionCleanup.add(%depBeac);
}

function switchBeaconType(%beacon)
{
   if(%beacon.getBeaconType() $= "friend")
   {
      // switch from marker beacon to target beacon
      if($TeamDeployedCount[%beacon.team, TargetBeacon] >= $TeamDeployableMax[TargetBeacon])
      {
         messageClient(%beacon.sourceObject.client, 'MsgDeployFailed', '\c2Your team\'s control network has reached its capacity for this item.~wfx/misc/misc.error.wav');
         return 0;
      }
      %beacon.setBeaconType(enemy);
      $TeamDeployedCount[%beacon.team, MarkerBeacon]--;
      $TeamDeployedCount[%beacon.team, TargetBeacon]++;
   }
   else
   {
      // switch from target beacon to marker beacon
      if($TeamDeployedCount[%beacon.team, MarkerBeacon] >= $TeamDeployableMax[MarkerBeacon])
      {
         messageClient(%beacon.sourceObject.client, 'MsgDeployFailed', '\c2Your team\'s control network has reached its capacity for this item.~wfx/misc/misc.error.wav');
         return 0;
      }
      %beacon.setBeaconType(friend);
      $TeamDeployedCount[%beacon.team, TargetBeacon]--;
      $TeamDeployedCount[%beacon.team, MarkerBeacon]++;
   }
}