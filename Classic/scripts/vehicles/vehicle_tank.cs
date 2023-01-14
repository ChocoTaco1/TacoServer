//**************************************************************
// BEOWULF ASSAULT VEHICLE
//**************************************************************
//**************************************************************
// SOUNDS
//**************************************************************
datablock EffectProfile(AssaultVehicleEngineEffect)
{
   effectname = "vehicles/tank_engine";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock EffectProfile(AssaultVehicleThrustEffect)
{
   effectname = "vehicles/tank_boost";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock EffectProfile(AssaultTurretActivateEffect)
{
   effectname = "vehicles/tank_activate";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock EffectProfile(AssaultMortarDryFireEffect)
{
   effectname = "weapons/mortar_dryfire";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock EffectProfile(AssaultMortarFireEffect)
{
   effectname = "vehicles/tank_mortar_fire";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock EffectProfile(AssaultMortarReloadEffect)
{
   effectname = "weapons/mortar_reload";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock EffectProfile(AssaultChaingunFireEffect)
{
   effectname = "weapons/chaingun_fire";
   minDistance = 5.0;
   maxDistance = 10.0;
};

datablock AudioProfile(AssaultVehicleSkid)
{
   filename    = "fx/vehicles/tank_skid.wav";
   description = ClosestLooping3d;
   preload = true;
};

datablock AudioProfile(AssaultVehicleEngineSound)
{
   filename    = "fx/vehicles/tank_engine.wav";
   description = AudioDefaultLooping3d;
   preload = true;
   effect = AssaultVehicleEngineEffect;
};

datablock AudioProfile(AssaultVehicleThrustSound)
{
   filename    = "fx/vehicles/tank_boost.wav";
   description = AudioDefaultLooping3d;
   preload = true;
   effect = AssaultVehicleThrustEffect;
};

datablock AudioProfile(AssaultChaingunFireSound)
{
   filename    = "fx/vehicles/tank_chaingun.wav";
   description = AudioDefaultLooping3d;
   preload = true;
   effect = AssaultChaingunFireEffect;
};

datablock AudioProfile(AssaultChaingunReloadSound)
{
   filename    = "fx/weapons/chaingun_dryfire.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(TankChaingunProjectile)
{
   filename    = "fx/weapons/chaingun_projectile.wav";
   description = ProjectileLooping3d;
   preload = true;
};

datablock AudioProfile(AssaultTurretActivateSound)
{
    filename    = "fx/vehicles/tank_activate.wav";
   description = AudioClose3d;
   preload = true;
   effect = AssaultTurretActivateEffect;
};

datablock AudioProfile(AssaultChaingunDryFireSound)
{
   filename    = "fx/weapons/chaingun_dryfire.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(AssaultChaingunIdleSound)
{
   filename    = "fx/misc/diagnostic_on.wav";
   description = ClosestLooping3d;
   preload = true;
};

datablock AudioProfile(AssaultMortarDryFireSound)
{
   filename    = "fx/weapons/mortar_dryfire.wav";
   description = AudioClose3d;
   preload = true;
   effect = AssaultMortarDryFireEffect;
};

datablock AudioProfile(AssaultMortarFireSound)
{
   filename    = "fx/vehicles/tank_mortar_fire.wav";
   description = AudioClose3d;
   preload = true;
   effect = AssaultMortarFireEffect;
};

datablock AudioProfile(AssaultMortarReloadSound)
{
   filename    = "fx/weapons/mortar_reload.wav";
   description = AudioClose3d;
   preload = true;
   effect = AssaultMortarReloadEffect;
};

datablock AudioProfile(AssaultMortarIdleSound)
{
   filename    = "fx/misc/diagnostic_on.wav";
   description = ClosestLooping3d;
   preload = true;
};

//**************************************************************
// LIGHTS
//**************************************************************
datablock RunningLightData(TankLight1)
{
   radius = 1.5;
   color = "1.0 1.0 1.0 0.2";
   nodeName = "Headlight_node01";
   direction = "0.0 1.0 0.0";
   texture = "special/headlight4";
};

datablock RunningLightData(TankLight2)
{
   radius = 1.5;
   color = "1.0 1.0 1.0 0.2";
   nodeName = "Headlight_node02";
   direction = "0.0 1.0 0.0";
   texture = "special/headlight4";
};

datablock RunningLightData(TankLight3)
{
   radius = 1.5;
   color = "1.0 1.0 1.0 0.2";
   nodeName = "Headlight_node03";
   direction = "0.0 1.0 0.0";
   texture = "special/headlight4";
};

datablock RunningLightData(TankLight4)
{
   radius = 1.5;
   color = "1.0 1.0 1.0 0.2";
   nodeName = "Headlight_node04";
   direction = "0.0 1.0 0.0";
   texture = "special/headlight4";
};

//**************************************************************
// VEHICLE CHARACTERISTICS
//**************************************************************

datablock HoverVehicleData(AssaultVehicle) : TankDamageProfile
{
   spawnOffset = "0 0 4";

   floatingGravMag = 4.5;

   catagory = "Vehicles";
   shapeFile = "vehicle_grav_tank.dts";
   multipassenger = true;
   computeCRC = true;
   renderWhenDestroyed = false;
                                                 
   weaponNode = 1;
   // z0dd - ZOD, 5/07/04. Attempt at squashing the UE bug
   //debrisShapeName = "vehicle_land_assault_debris.dts";
   debrisShapeName = "vehicle_land_mpbase_debris.dts";
   debris = ShapeDebris;

   drag = 0.0;
   density = 0.9;

   mountPose[0] = sitting;
   mountPose[1] = sitting;
   numMountPoints = 2;
   isProtectedMountPoint[0] = true;
   isProtectedMountPoint[1] = true;

   cameraMaxDist = 20;
   cameraOffset = 3;
   cameraLag = 1.5;
   explosion = LargeGroundVehicleExplosion;
   explosionDamage = 0.5;
   explosionRadius = 5.0;

   maxSteeringAngle = 0.5;  // 20 deg.

   maxDamage = 3.15;
   destroyedLevel = 3.15;

   isShielded = true;
   rechargeRate = 1.0;
   energyPerDamagePoint = 135;
   maxEnergy = 400;
   minJetEnergy = 15;
   jetEnergyDrain = 2.0;

   // Rigid Body
   mass = 1500;
   bodyFriction = 0.8;
   bodyRestitution = 0.5;
   minRollSpeed = 3;
   gyroForce = 400;
   gyroDamping = 0.3;
   stabilizerForce = 20;
   minDrag = 10;
   softImpactSpeed = 18;  // Play SoftImpact Sound. z0dd - ZOD, 3/30/02. Higher speed before tank takes ground collision dmg. Was 15
   hardImpactSpeed = 21;  // Play HardImpact Sound. z0dd - ZOD, 3/30/02. Higher speed before tank takes ground collision dmg. Was 18

   // Ground Impact Damage (uses DamageType::Ground)
   minImpactSpeed = 20; // z0dd - ZOD, 3/30/02. Higher speed before tank takes ground collision dmg. Was 17
   speedDamageScale = 0.060;

   // Object Impact Damage (uses DamageType::Impact)
   collDamageThresholdVel = 18;
   collDamageMultiplier   = 0.045;

   dragForce            = 40 / 20;
   vertFactor           = 0.0;
   floatingThrustFactor = 0.15; // z0dd - ZOD, 3/30/02. Stronger air cushion. Was 0.15

   mainThrustForce    = 55;  // z0dd - ZOD, 3/30/02. Was 50
   reverseThrustForce = 40;
   strafeThrustForce  = 40;
   turboFactor        = 1.85; // z0dd - ZOD, 3/30/02. Was 1.7

   brakingForce = 25;
   brakingActivationSpeed = 4;

   stabLenMin = 3.25;
   stabLenMax = 4;
   stabSpringConstant  = 50;
   stabDampingConstant = 20;

   gyroDrag = 20;
   normalForce = 20;
   restorativeForce = 10;
   steeringForce = 15;
   rollForce  = 5;
   pitchForce = 3;

   dustEmitter = TankDustEmitter;
   triggerDustHeight = 3.5;
   dustHeight = 1.0;
   dustTrailEmitter = TireEmitter;
   dustTrailOffset = "0.0 -1.0 0.5";
   triggerTrailHeight = 3.6;
   dustTrailFreqMod = 15.0;

   jetSound         = AssaultVehicleThrustSound;
   engineSound      = AssaultVehicleEngineSound;
   floatSound       = AssaultVehicleSkid;
   softImpactSound  = GravSoftImpactSound;
   hardImpactSound  = HardImpactSound;
   wheelImpactSound = WheelImpactSound;

   forwardJetEmitter = TankJetEmitter;
   
   //
   softSplashSoundVelocity = 5.0; 
   mediumSplashSoundVelocity = 10.0;   
   hardSplashSoundVelocity = 15.0;   
   exitSplashSoundVelocity = 10.0;
   
   exitingWater      = VehicleExitWaterMediumSound;
   impactWaterEasy   = VehicleImpactWaterSoftSound;
   impactWaterMedium = VehicleImpactWaterMediumSound;
   impactWaterHard   = VehicleImpactWaterMediumSound;
   waterWakeSound    = VehicleWakeMediumSplashSound; 

   minMountDist = 4;

   damageEmitter[0] = SmallLightDamageSmoke;
   damageEmitter[1] = SmallHeavyDamageSmoke;
   damageEmitter[2] = DamageBubbles;
   damageEmitterOffset[0] = "0.0 -1.5 3.5 ";
   damageLevelTolerance[0] = 0.3;
   damageLevelTolerance[1] = 0.7;
   numDmgEmitterAreas = 1;

   splashEmitter[0] = VehicleFoamDropletsEmitter;
   splashEmitter[1] = VehicleFoamEmitter;

   shieldImpact = VehicleShieldImpact;

   cmdCategory = "Tactical";
   cmdIcon = CMDGroundTankIcon;
   cmdMiniIconName = "commander/MiniIcons/com_tank_grey";
   targetNameTag = 'Beowulf';
   targetTypeTag = 'Assault Tank';
   sensorData = VehiclePulseSensor;
   sensorRadius = VehiclePulseSensor.detectRadius; // z0dd - ZOD, 3/30/02. Allows sensor to be shown on CC   
   
   checkRadius = 5.5535;
   observeParameters = "1 10 10";
   runningLight[0] = TankLight1;
   runningLight[1] = TankLight2;
   runningLight[2] = TankLight3;
   runningLight[3] = TankLight4;
   shieldEffectScale = "0.9 1.0 0.6";
   showPilotInfo = 1;
};

//**************************************************************
// WEAPONS
//**************************************************************

//-------------------------------------
// ASSAULT CHAINGUN (projectile)
//-------------------------------------

datablock TracerProjectileData(AssaultChaingunBullet)
{
   doDynamicClientHits = true;

   projectileShapeName = "";
   directDamage        = 0.16;
   directDamageType    = $DamageType::TankChaingun;
   hasDamageRadius     = false;
   splash			   = ChaingunSplash;

   kickbackstrength    = 0.0;
   sound          	   = TankChaingunProjectile;

   dryVelocity       = 425.0;
   wetVelocity       = 100.0;
   velInheritFactor  = 1.0;
   fizzleTimeMS      = 3000;
   lifetimeMS        = 3000;
   explodeOnDeath    = false;
   reflectOnWaterImpactAngle = 0.0;
   explodeOnWaterImpact      = false;
   deflectionOnWaterImpact   = 0.0;
   fizzleUnderwaterMS        = 3000;

   tracerLength    = 15.0;
   tracerAlpha     = false;
   tracerMinPixels = 6;
   tracerColor     = 211.0/255.0 @ " " @ 215.0/255.0 @ " " @ 120.0/255.0 @ " 0.75";
	tracerTex[0]  	 = "special/tracer00";
	tracerTex[1]  	 = "special/tracercross";
	tracerWidth     = 0.10;
   crossSize       = 0.20;
   crossViewAng    = 0.990;
   renderCross     = true;

   decalData[0] = ChaingunDecal1;
   decalData[1] = ChaingunDecal2;
   decalData[2] = ChaingunDecal3;
   decalData[3] = ChaingunDecal4;
   decalData[4] = ChaingunDecal5;
   decalData[5] = ChaingunDecal6;

   activateDelayMS   = 100;

   explosion = ChaingunExplosion;
};

//-------------------------------------
// ASSAULT CHAINGUN CHARACTERISTICS
//-------------------------------------

datablock TurretData(AssaultPlasmaTurret) : TurretDamageProfile
{
   className      = VehicleTurret;
   catagory       = "Turrets";
   shapeFile      = "Turret_tank_base.dts";
   preload        = true;

   mass           = 1.0;  // Not really relevant

   maxEnergy               = 1;
   maxDamage               = AssaultVehicle.maxDamage;
   destroyedLevel          = AssaultVehicle.destroyedLevel;
   repairRate              = 0;
   
   // capacitor
   maxCapacitorEnergy      = 250;
   capacitorRechargeRate   = 1.0;

   thetaMin = 0;
   thetaMax = 100;

   inheritEnergyFromMount = true;
   firstPersonOnly = true;
   useEyePoint = true;
   numWeapons = 2;

   cameraDefaultFov = 90.0;
   cameraMinFov = 5.0;
   cameraMaxFov = 120.0;

   targetNameTag = 'Beowulf Chaingun';
   targetTypeTag = 'Turret';
};

datablock TurretImageData(AssaultPlasmaTurretBarrel)
{
   shapeFile = "turret_tank_barrelchain.dts";
   mountPoint = 1;

   projectile = AssaultChaingunBullet;
   projectileType = TracerProjectile;

   casing              = ShellDebris;
   shellExitDir        = "1.0 0.3 1.0";
   shellExitOffset     = "0.15 -0.56 -0.1";
   shellExitVariance   = 15.0;
   shellVelocity       = 3.0;

   projectileSpread = 12.0 / 1000.0;

   useCapacitor = true;
   usesEnergy = true;
   useMountEnergy = true;
   fireEnergy = 7.5;
   minEnergy = 15.0;

   // Turret parameters
   activationMS      = 4000;
   deactivateDelayMS = 500;
   thinkTimeMS       = 200;
   degPerSecTheta    = 360;
   degPerSecPhi      = 360;
   attackRadius      = 1000;

   // State transitions
   stateName[0]                        = "Activate";
   stateTransitionOnNotLoaded[0]       = "Dead";
   stateTransitionOnLoaded[0]          = "ActivateReady";
   stateSound[0]                       = AssaultTurretActivateSound;

   stateName[1]                        = "ActivateReady";
   stateSequence[1]                    = "Activate";
   stateSound[1]                       = AssaultTurretActivateSound;
   stateTimeoutValue[1]                = 1;
   stateTransitionOnTimeout[1]         = "Ready";
   stateTransitionOnNotLoaded[1]       = "Deactivate";

   stateName[2]                        = "Ready";
   stateTransitionOnNotLoaded[2]       = "Deactivate";
   stateTransitionOnTriggerDown[2]     = "Fire";
   stateTransitionOnNoAmmo[2]          = "NoAmmo";

   stateName[3]                        = "Fire";
   stateSequence[3]                    = "Fire";
   stateSequenceRandomFlash[3]         = true;
   stateFire[3]                        = true;
   stateAllowImageChange[3]            = false;
   stateSound[3]                       = AssaultChaingunFireSound;
   stateScript[3]                      = "onFire";
   stateTimeoutValue[3]                = 0.1;
   stateTransitionOnTimeout[3]         = "Fire";
   stateTransitionOnTriggerUp[3]       = "Reload";
   stateTransitionOnNoAmmo[3]          = "noAmmo";

   stateName[4]                        = "Reload";
   stateSequence[4]                    = "Reload";
   stateTimeoutValue[4]                = 0.1;
   stateAllowImageChange[4]            = false;
   stateTransitionOnTimeout[4]         = "Ready";
   stateTransitionOnNoAmmo[4]          = "NoAmmo";
   stateWaitForTimeout[4]              = true;

   stateName[5]                        = "Deactivate";
   stateSequence[5]                    = "Activate";
   stateDirection[5]                   = false;
   stateTimeoutValue[5]                = 30;
   stateTransitionOnTimeout[5]         = "ActivateReady";

   stateName[6]                        = "Dead";
   stateTransitionOnLoaded[6]          = "ActivateReady";
   stateTransitionOnTriggerDown[6]     = "DryFire";

   stateName[7]                        = "DryFire";
   stateSound[7]                       = AssaultChaingunDryFireSound;
   stateTimeoutValue[7]                = 0.5;
   stateTransitionOnTimeout[7]         = "NoAmmo";

   stateName[8]                        = "NoAmmo";
   stateTransitionOnAmmo[8]            = "Reload";
   stateSequence[8]                    = "NoAmmo";
   stateTransitionOnTriggerDown[8]     = "DryFire";

};

//-------------------------------------
// ASSAULT MORTAR (projectile)
//-------------------------------------

datablock ItemData(AssaultMortarAmmo)
{
   className = Ammo;
   catagory = "Ammo";
   shapeFile = "repair_kit.dts";
   mass = 1;
   elasticity = 0.5;
   friction = 0.6;
   pickupRadius = 1;

   computeCRC = true;
};

datablock GrenadeProjectileData(AssaultMortar)
{
   projectileShapeName = "mortar_projectile.dts";
   emitterDelay        = -1;
   directDamage        = 0.0;
   hasDamageRadius     = true;
   indirectDamage      = 1.0;
   damageRadius        = 25.0;
   radiusDamageType    = $DamageType::TankMortar;
   kickBackStrength    = 2500;

   sound          = MortarProjectileSound;
   explosion           = "MortarExplosion";
   velInheritFactor    = 1.0;

   baseEmitter         = MortarSmokeEmitter;

   grenadeElasticity = 0.0;
   grenadeFriction   = 0.4;
   armingDelayMS     = 250;
   muzzleVelocity    = 77.15; // z0dd - ZOD, 9/27/02. More velocity to compensate for higher gravity. Was 65
   drag              = 0.1;

   hasLight    = true;
   lightRadius = 4;
   lightColor  = "0.1 0.4 0.1";
};

//-------------------------------------
// ASSAULT MORTAR CHARACTERISTICS
//-------------------------------------

datablock TurretImageData(AssaultMortarTurretBarrel)
{
   shapeFile = "turret_tank_barrelmortar.dts";
   mountPoint = 0;

//   ammo = AssaultMortarAmmo;
   projectile = AssaultMortar;
   projectileType = GrenadeProjectile;

   usesEnergy = true;
   useMountEnergy = true;
   fireEnergy = 77.00;
   minEnergy = 77.00;
   useCapacitor = true;

   // Turret parameters
   activationMS                        = 4000;
   deactivateDelayMS                   = 1500;
   thinkTimeMS                         = 200;
   degPerSecTheta                      = 360;
   degPerSecPhi                        = 360;
   attackRadius                        = 1000;

   // State transitions
   stateName[0]                        = "Activate";
   stateTransitionOnNotLoaded[0]       = "Dead";
   stateTransitionOnLoaded[0]          = "ActivateReady";

   stateName[1]                        = "ActivateReady";
   stateSequence[1]                    = "Activate";
   stateSound[1]                       = AssaultTurretActivateSound;
   stateTimeoutValue[1]                = 1.0;
   stateTransitionOnTimeout[1]         = "Ready";
   stateTransitionOnNotLoaded[1]       = "Deactivate";

   stateName[2]                        = "Ready";
   stateTransitionOnNotLoaded[2]       = "Deactivate";
   stateTransitionOnNoAmmo[2]          = "NoAmmo";
   stateTransitionOnTriggerDown[2]     = "Fire";

   stateName[3]                        = "Fire";
   stateSequence[3]                    = "Fire";
   stateTransitionOnTimeout[3]         = "Reload";
   stateTimeoutValue[3]                = 1.0;
   stateFire[3]                        = true;
   stateRecoil[3]                      = LightRecoil;
   stateAllowImageChange[3]            = false;
   stateSound[3]                       = AssaultMortarFireSound;
   stateScript[3]                      = "onFire";

   stateName[4]                        = "Reload";
   stateSequence[4]                    = "Reload";
   stateTimeoutValue[4]                = 1.0;
   stateAllowImageChange[4]            = false;
   stateTransitionOnTimeout[4]         = "Ready";
   //stateTransitionOnNoAmmo[4]          = "NoAmmo";
   stateWaitForTimeout[4]              = true;

   stateName[5]                        = "Deactivate";
   stateDirection[5]                   = false;
   stateSequence[5]                    = "Activate";
   stateTimeoutValue[5]                = 1.0;
   stateTransitionOnLoaded[5]          = "ActivateReady";
   stateTransitionOnTimeout[5]         = "Dead";

   stateName[6]                        = "Dead";
   stateTransitionOnLoaded[6]          = "ActivateReady";
   stateTransitionOnTriggerDown[6]     = "DryFire";

   stateName[7]                        = "DryFire";
   stateSound[7]                       = AssaultMortarDryFireSound;
   stateTimeoutValue[7]                = 1.0;
   stateTransitionOnTimeout[7]         = "NoAmmo";

   stateName[8]                        = "NoAmmo";
   stateSequence[8]                    = "NoAmmo";
   stateTransitionOnAmmo[8]            = "Reload";
   stateTransitionOnTriggerDown[8]     = "DryFire";
};

datablock TurretImageData(AssaultTurretParam)
{
   mountPoint = 2;
   shapeFile = "turret_muzzlepoint.dts";

   projectile = AssaultChaingunBullet;
   projectileType = TracerProjectile;

   useCapacitor = true;
   usesEnergy = true;

   // Turret parameters
   activationMS      = 1000;
   deactivateDelayMS = 1500;
   thinkTimeMS       = 200;
   degPerSecTheta    = 500;
   degPerSecPhi      = 500;
   
   attackRadius      = 1000;
};              
