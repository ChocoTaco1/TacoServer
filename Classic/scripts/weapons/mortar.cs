//--------------------------------------
// Mortar
//--------------------------------------

//--------------------------------------------------------------------------
// Sounds
//--------------------------------------
datablock AudioProfile(MortarSwitchSound)
{
   filename    = "fx/weapons/mortar_activate.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(MortarReloadSound)
{
   filename    = "fx/weapons/mortar_reload.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(MortarIdleSound)
{
   //filename    = "fx/weapons/weapon.mortarIdle.wav";
   filename = "fx/weapons/plasma_rifle_idle.wav";
   description = ClosestLooping3d;
   preload = true;
};

datablock AudioProfile(MortarFireSound)
{
   filename    = "fx/weapons/mortar_fire.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(MortarProjectileSound)
{
   filename    = "fx/weapons/mortar_projectile.wav";
   description = ProjectileLooping3d;
   preload = true;
};

datablock AudioProfile(MortarExplosionSound)
{
   filename    = "fx/weapons/mortar_explode.wav";
   description = AudioBIGExplosion3d;
   preload = true;
};

datablock AudioProfile(UnderwaterMortarExplosionSound)
{
   filename    = "fx/weapons/mortar_explode_UW.wav";
   description = AudioBIGExplosion3d;
   preload = true;
};

datablock AudioProfile(MortarDryFireSound)
{
   filename    = "fx/weapons/mortar_dryfire.wav";
   description = AudioClose3d;
   preload = true;
};

//----------------------------------------------------------------------------
// Bubbles
//----------------------------------------------------------------------------
datablock ParticleData(MortarBubbleParticle)
{
   dragCoefficient      = 0.0;
   gravityCoefficient   = -0.25;
   inheritedVelFactor   = 0.0;
   constantAcceleration = 0.0;
   lifetimeMS           = 1500;
   lifetimeVarianceMS   = 600;
   useInvAlpha          = false;
   textureName          = "special/bubbles";

   spinRandomMin        = -100.0;
   spinRandomMax        =  100.0;

   colors[0]     = "0.7 0.8 1.0 0.4";
   colors[1]     = "0.7 0.8 1.0 0.4";
   colors[2]     = "0.7 0.8 1.0 0.0";
   sizes[0]      = 0.8;
   sizes[1]      = 0.8;
   sizes[2]      = 0.8;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(MortarBubbleEmitter)
{
   ejectionPeriodMS = 9;
   periodVarianceMS = 0;
   ejectionVelocity = 1.0;
   ejectionOffset   = 0.1;
   velocityVariance = 0.5;
   thetaMin         = 0;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   particles = "MortarBubbleParticle";
};

//--------------------------------------------------------------------------
// Splash
//--------------------------------------------------------------------------
datablock ParticleData( MortarSplashParticle )
{
   dragCoefficient      = 1;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = -1.4;
   lifetimeMS           = 300;
   lifetimeVarianceMS   = 0;
   textureName          = "special/droplet";
   colors[0]     = "0.7 0.8 1.0 1.0";
   colors[1]     = "0.7 0.8 1.0 0.5";
   colors[2]     = "0.7 0.8 1.0 0.0";
   sizes[0]      = 0.05;
   sizes[1]      = 0.2;
   sizes[2]      = 0.2;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( MortarSplashEmitter )
{
   ejectionPeriodMS = 4;
   periodVarianceMS = 0;
   ejectionVelocity = 3;
   velocityVariance = 1.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 50;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 100;
   particles = "MortarSplashParticle";
};


datablock SplashData(MortarSplash)
{
   numSegments = 10;
   ejectionFreq = 10;
   ejectionAngle = 20;
   ringLifetime = 0.4;
   lifetimeMS = 400;
   velocity = 3.0;
   startRadius = 0.0;
   acceleration = -3.0;
   texWrap = 5.0;

   texture = "special/water2";

   emitter[0] = MortarSplashEmitter;

   colors[0] = "0.7 0.8 1.0 0.0";
   colors[1] = "0.7 0.8 1.0 1.0";
   colors[2] = "0.7 0.8 1.0 0.0";
   colors[3] = "0.7 0.8 1.0 0.0";
   times[0] = 0.0;
   times[1] = 0.4;
   times[2] = 0.8;
   times[3] = 1.0;
};

//---------------------------------------------------------------------------
// Mortar Shockwaves
//---------------------------------------------------------------------------
datablock ShockwaveData(UnderwaterMortarShockwave)
{
   width = 6.0;
   numSegments = 32;
   numVertSegments = 6;
   velocity = 10;
   acceleration = 20.0;
   lifetimeMS = 900;
   height = 1.0;
   verticalCurve = 0.5;
   is2D = false;

   texture[0] = "special/shockwave4";
   texture[1] = "special/gradient";
   texWrap = 6.0;

   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

   colors[0] = "0.4 0.4 1.0 0.50";
   colors[1] = "0.4 0.4 1.0 0.25";
   colors[2] = "0.4 0.4 1.0 0.0";

   mapToTerrain = true;
   orientToNormal = false;
   renderBottom = false;
};

datablock ShockwaveData(MortarShockwave)
{
   width = 6.0;
   numSegments = 32;
   numVertSegments = 6;
   velocity = 15;
   acceleration = 20.0;
   lifetimeMS = 500;
   height = 1.0;
   verticalCurve = 0.5;
   is2D = false;

   texture[0] = "special/shockwave4";
   texture[1] = "special/gradient";
   texWrap = 6.0;

   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

   colors[0] = "0.4 1.0 0.4 0.50";
   colors[1] = "0.4 1.0 0.4 0.25";
   colors[2] = "0.4 1.0 0.4 0.0";

   mapToTerrain = true;
   orientToNormal = false;
   renderBottom = false;
};


//--------------------------------------------------------------------------
// Mortar Explosion Particle effects
//--------------------------------------
datablock ParticleData( MortarCrescentParticle )
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = -0.0;
   lifetimeMS           = 600;
   lifetimeVarianceMS   = 000;
   textureName          = "special/crescent3";
   colors[0]     = "0.7 1.0 0.7 1.0";
   colors[1]     = "0.7 1.0 0.7 0.5";
   colors[2]     = "0.7 1.0 0.7 0.0";
   sizes[0]      = 4.0;
   sizes[1]      = 8.0;
   sizes[2]      = 9.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( MortarCrescentEmitter )
{
   ejectionPeriodMS = 25;
   periodVarianceMS = 0;
   ejectionVelocity = 40;
   velocityVariance = 5.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 200;
   particles = "MortarCrescentParticle";
};


datablock ParticleData(MortarExplosionSmoke)
{
   dragCoeffiecient     = 0.4;
   gravityCoefficient   = -0.30;   // rises slowly
   inheritedVelFactor   = 0.025;

   lifetimeMS           = 1250;
   lifetimeVarianceMS   = 500;

   textureName          = "particleTest";

   useInvAlpha =  true;
   spinRandomMin = -100.0;
   spinRandomMax =  100.0;

   textureName = "special/Smoke/bigSmoke";

   colors[0]     = "0.7 0.7 0.7 0.0";
   colors[1]     = "0.4 0.4 0.4 0.5";
   colors[2]     = "0.4 0.4 0.4 0.5";
   colors[3]     = "0.4 0.4 0.4 0.0";
   sizes[0]      = 5.0;
   sizes[1]      = 6.0;
   sizes[2]      = 10.0;
   sizes[3]      = 12.0;
   times[0]      = 0.0;
   times[1]      = 0.333;
   times[2]      = 0.666;
   times[3]      = 1.0;



};

datablock ParticleEmitterData(MortarExplosionSmokeEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;

   ejectionOffset = 8.0;


   ejectionVelocity = 1.25;
   velocityVariance = 1.2;

   thetaMin         = 0.0;
   thetaMax         = 90.0;

   lifetimeMS       = 500;

   particles = "MortarExplosionSmoke";

};

//---------------------------------------------------------------------------
// Underwater Explosion
//---------------------------------------------------------------------------
datablock ParticleData(UnderwaterExplosionSparks)
{
   dragCoefficient      = 0;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 350;
   textureName          = "special/crescent3";
   colors[0]     = "0.4 0.4 1.0 1.0";
   colors[1]     = "0.4 0.4 1.0 1.0";
   colors[2]     = "0.4 0.4 1.0 0.0";
   sizes[0]      = 3.5;
   sizes[1]      = 3.5;
   sizes[2]      = 3.5;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(UnderwaterExplosionSparksEmitter)
{
   ejectionPeriodMS = 2;
   periodVarianceMS = 0;
   ejectionVelocity = 17;
   velocityVariance = 4;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 60;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 100;
   particles = "UnderwaterExplosionSparks";
};

datablock ParticleData(MortarExplosionBubbleParticle)
{
   dragCoefficient      = 0.0;
   gravityCoefficient   = -0.25;
   inheritedVelFactor   = 0.0;
   constantAcceleration = 0.0;
   lifetimeMS           = 1500;
   lifetimeVarianceMS   = 600;
   useInvAlpha          = false;
   textureName          = "special/bubbles";

   spinRandomMin        = -100.0;
   spinRandomMax        =  100.0;

   colors[0]     = "0.7 0.8 1.0 0.0";
   colors[1]     = "0.7 0.8 1.0 0.4";
   colors[2]     = "0.7 0.8 1.0 0.0";
   sizes[0]      = 2.0;
   sizes[1]      = 2.0;
   sizes[2]      = 2.0;
   times[0]      = 0.0;
   times[1]      = 0.8;
   times[2]      = 1.0;
};
datablock ParticleEmitterData(MortarExplosionBubbleEmitter)
{
   ejectionPeriodMS = 5;
   periodVarianceMS = 0;
   ejectionVelocity = 1.0;
   ejectionOffset   = 7.0;
   velocityVariance = 0.5;
   thetaMin         = 0;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   particles = "MortarExplosionBubbleParticle";
};

datablock DebrisData( UnderwaterMortarDebris )
{
   emitters[0] = MortarExplosionBubbleEmitter;

   explodeOnMaxBounce = true;

   elasticity = 0.4;
   friction = 0.2;

   lifetime = 1.5;
   lifetimeVariance = 0.2;

   numBounces = 1;
};             

datablock ExplosionData(UnderwaterMortarSubExplosion1)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   delayMS = 100;
   offset = 3.0;
   playSpeed = 1.5;

   sizes[0] = "0.75 0.75 0.75";
   sizes[1] = "1.0  1.0  1.0";
   sizes[2] = "0.5 0.5 0.5";
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

};             

datablock ExplosionData(UnderwaterMortarSubExplosion2)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   delayMS = 50;
   offset = 3.0;
   playSpeed = 0.75;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "1.5 1.5 1.5";
   sizes[2] = "1.0 1.0 1.0";
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;
};

datablock ExplosionData(UnderwaterMortarSubExplosion3)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   delayMS = 0;
   offset = 0.0;
   playSpeed = 0.5;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "2.0 2.0 2.0";
   sizes[2] = "1.5 1.5 1.5";
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

};

datablock ExplosionData(UnderwaterMortarExplosion)
{
   soundProfile   = UnderwaterMortarExplosionSound;

   shockwave = UnderwaterMortarShockwave;
   shockwaveOnTerrain = true;

   subExplosion[0] = UnderwaterMortarSubExplosion1;
   subExplosion[1] = UnderwaterMortarSubExplosion2;
   subExplosion[2] = UnderwaterMortarSubExplosion3;

   emitter[0] = MortarExplosionBubbleEmitter;
   emitter[1] = UnderwaterExplosionSparksEmitter;
   
   shakeCamera = true;
   camShakeFreq = "8.0 9.0 7.0";
   camShakeAmp = "100.0 100.0 100.0";
   camShakeDuration = 1.3;
   camShakeRadius = 25.0;
};


//---------------------------------------------------------------------------
// Explosion
//---------------------------------------------------------------------------

datablock ExplosionData(MortarSubExplosion1)
{
   explosionShape = "mortar_explosion.dts";
   faceViewer           = true;

   delayMS = 100;

   offset = 5.0;

   playSpeed = 1.5;

   sizes[0] = "0.5 0.5 0.5";
   sizes[1] = "0.5 0.5 0.5";
   times[0] = 0.0;
   times[1] = 1.0;

};             

datablock ExplosionData(MortarSubExplosion2)
{
   explosionShape = "mortar_explosion.dts";
   faceViewer           = true;

   delayMS = 50;

   offset = 5.0;

   playSpeed = 1.0;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "1.0 1.0 1.0";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(MortarSubExplosion3)
{
   explosionShape = "mortar_explosion.dts";
   faceViewer           = true;
   delayMS = 0;
   offset = 0.0;
   playSpeed = 0.7;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "2.0 2.0 2.0";
   times[0] = 0.0;
   times[1] = 1.0;

};

datablock ExplosionData(MortarExplosion)
{
   soundProfile   = MortarExplosionSound;

   shockwave = MortarShockwave;
   shockwaveOnTerrain = true;

   subExplosion[0] = MortarSubExplosion1;
   subExplosion[1] = MortarSubExplosion2;
   subExplosion[2] = MortarSubExplosion3;

   emitter[0] = MortarExplosionSmokeEmitter;
   emitter[1] = MortarCrescentEmitter;

   shakeCamera = true;
   camShakeFreq = "8.0 9.0 7.0";
   camShakeAmp = "100.0 100.0 100.0";
   camShakeDuration = 1.3;
   camShakeRadius = 25.0;
};

//---------------------------------------------------------------------------
// Smoke particles
//---------------------------------------------------------------------------
datablock ParticleData(MortarSmokeParticle)
{
   dragCoeffiecient     = 0.4;
   gravityCoefficient   = -0.3;   // rises slowly
   inheritedVelFactor   = 0.125;

   lifetimeMS           =  1200;
   lifetimeVarianceMS   =  200;
   useInvAlpha          =  true;
   spinRandomMin        = -100.0;
   spinRandomMax        =  100.0;

   animateTexture = false;

   textureName = "special/Smoke/bigSmoke";

   colors[0]     = "0.7 1.0 0.7 0.5";
   colors[1]     = "0.3 0.7 0.3 0.8";
   colors[2]     = "0.0 0.0 0.0 0.0";
   sizes[0]      = 1.0;
   sizes[1]      = 2.0;
   sizes[2]      = 4.5;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;

};


datablock ParticleEmitterData(MortarSmokeEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 3;

   ejectionVelocity = 2.25;
   velocityVariance = 0.55;

   thetaMin         = 0.0;
   thetaMax         = 40.0;

   particles = "MortarSmokeParticle";
};


//--------------------------------------------------------------------------
// Projectile
//--------------------------------------
datablock GrenadeProjectileData(MortarShot)
{
   projectileShapeName = "mortar_projectile.dts";
   emitterDelay        = -1;
   directDamage        = 0.0;
   hasDamageRadius     = true;
   indirectDamage      = 1.0;
   damageRadius        = 19.0; // z0dd - ZOD, 8/13/02. Was 20.0
   radiusDamageType    = $DamageType::Mortar;
   kickBackStrength    = 2500;

   explosion           = "MortarExplosion";
   underwaterExplosion = "UnderwaterMortarExplosion";
   velInheritFactor    = 0.5;
   splash              = MortarSplash;
   depthTolerance      = 10.0; // depth at which it uses underwater explosion
   
   baseEmitter         = MortarSmokeEmitter;
   bubbleEmitter       = MortarBubbleEmitter;
   
   grenadeElasticity = 0.15;
   grenadeFriction   = 0.4;
   armingDelayMS     = 1500; // z0dd - ZOD, 4/14/02. Was 2000

   gravityMod        = 1.1;  // z0dd - ZOD, 5/18/02. Make mortar projectile heavier, less floaty
   muzzleVelocity    = 75.95; // z0dd - ZOD, 8/13/02. More velocity to compensate for higher gravity. Was 63.7
   drag              = 0.1;
   sound	     = MortarProjectileSound;

   hasLight    = true;
   lightRadius = 4;
   lightColor  = "0.05 0.2 0.05";

   hasLightUnderwaterColor = true;
   underWaterLightColor = "0.05 0.075 0.2";

   // z0dd - ZOD, 5/22/03. Limit the mortars effective range to around 450 meters.
   // Addresses long range mortar spam exploit.
   explodeOnDeath = $Host::ClassicLoadMortarChanges ? true : false;
   fizzleTimeMS = $Host::ClassicLoadMortarChanges ? 8000 : -1;
   lifetimeMS = $Host::ClassicLoadMortarChanges ? 10000 : -1;
   fizzleUnderwaterMS = $Host::ClassicLoadMortarChanges ? 8000 : -1;
   reflectOnWaterImpactAngle = 0.0;
   explodeOnWaterImpact = false;
   deflectionOnWaterImpact = 0.0;
};

// z0dd - ZOD, 5/20/03. New block, spawn this at end of MortarShots lifetime.
datablock ItemData(MortarGrenadeThrown)
{
   className = Weapon;
   shapeFile = "mortar_projectile.dts";
   mass = 0.7;
   elasticity = 0.15;
   friction = 1;
   pickupRadius = 2;
   maxDamage = 0.5;
   explosion = "mortarExplosion";
   underwaterExplosion = UnderwaterMortarExplosion;
   indirectDamage = 1.0;
   damageRadius = 19.0;
   radiusDamageType = $DamageType::Mortar;
   kickBackStrength = 2500;
};

//--------------------------------------------------------------------------
// Ammo
//--------------------------------------

datablock ItemData(MortarAmmo)
{
   className = Ammo;
   catagory = "Ammo";
   shapeFile = "ammo_mortar.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   pickUpName = "some mortar ammo";
   computeCRC = true;
};

//--------------------------------------------------------------------------
// Weapon
//--------------------------------------
datablock ItemData(Mortar)
{
   className = Weapon;
   catagory = "Spawn Items";
   shapeFile = "weapon_mortar.dts";
   image = MortarImage;
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   pickUpName = "a mortar gun";
   computeCRC = true;
   emap = true;
};

datablock ShapeBaseImageData(MortarImage)
{
   className = WeaponImage;
   shapeFile = "weapon_mortar.dts";
   item = Mortar;
   ammo = MortarAmmo;
   offset = "0 0 0";
   emap = true;

   projectile = MortarShot;
   projectileType = GrenadeProjectile;

   stateName[0] = "Activate";
   stateTransitionOnTimeout[0] = "ActivateReady";
   stateTimeoutValue[0] = 0.5;
   stateSequence[0] = "Activate";
   stateSound[0] = MortarSwitchSound;

   stateName[1] = "ActivateReady";
   stateTransitionOnAmmo[1] = "Ready";
   stateTransitionOnNoAmmo[1] = "FirstLoad";

   stateName[2] = "Ready";
   stateTransitionOnNoAmmo[2] = "NoAmmo";
   stateTransitionOnTriggerDown[2] = "Fire";
   stateSound[2] = MortarIdleSound;

   stateName[3] = "Fire";
   stateSequence[3] = "Recoil";
   stateTransitionOnTimeout[3] = "Reload";
   stateTimeoutValue[3] = 0.8;
   stateFire[3] = true;
   stateRecoil[3] = LightRecoil;
   stateAllowImageChange[3] = false;
   stateScript[3] = "onFire";
   stateSound[3] = MortarFireSound;

   stateName[4] = "Reload";
   stateTransitionOnNoAmmo[4] = "NoAmmo";
   stateTransitionOnTimeout[4] = "Ready";
   stateTimeoutValue[4] = 2.0;
   stateAllowImageChange[4] = false;
   stateSequence[4] = "Reload";
   stateSound[4] = MortarReloadSound;

   stateName[5] = "NoAmmo";
   stateTransitionOnAmmo[5] = "Reload";
   stateSequence[5] = "NoAmmo";
   stateTransitionOnTriggerDown[5] = "DryFire";

   stateName[6]       = "DryFire";
   stateSound[6]      = MortarDryFireSound;
   stateTimeoutValue[6]        = 1.5;
   stateTransitionOnTimeout[6] = "NoAmmo";
   
   stateName[7] = "FirstLoad";
   stateTransitionOnAmmo[7] = "Ready";
};

function MortarImage::onUnmount(%this,%obj,%slot){
   parent::onUnmount(%this,%obj,%slot);
   if(isEventPending(%obj.reloadDelaySch))
      cancel(%obj.reloadDelaySch);  
}
function MortarImage::onMount(%this,%obj,%slot){
      
   if(%obj.getClassName() !$= "Player")
      return;

   if (%this.armthread $= "")
      %obj.setArmThread(look);
   else
      %obj.setArmThread(%this.armThread);

   if(%obj.getMountedImage($WeaponSlot).ammo !$= ""){
      if (%obj.getInventory(%this.ammo)){
         %fireAndReloadTime = mFloor((%this.stateTimeoutValue[4] + %this.stateTimeoutValue[3]) * 1000);
         if(%obj.lfireTime[%this.getName()] && (getSimTime() - %obj.lfireTime[%this.getName()]) < %fireAndReloadTime){
            if(isEventPending(%obj.reloadDelaySch)){
              cancel(%obj.reloadDelaySch);  
            }
            %time = mFloor(%fireAndReloadTime - (getSimTime() - %obj.lfireTime[%this.getName()]));
            %obj.reloadDelaySch = schedule(%time, 0, "ammoStateDelay", %obj, %slot, true);
         }
         else{
            %obj.setImageAmmo(%slot,true);
         }
      }
   }
   
   %obj.client.setWeaponsHudActive(%this.item);
   if(%obj.getMountedImage($WeaponSlot).ammo !$= "")
      %obj.client.setAmmoHudCount(%obj.getInventory(%this.ammo));
   else
      %obj.client.setAmmoHudCount(-1);   
}

function ammoStateDelay(%obj, %slot, %state){
   if(isObject(%obj) && %obj.getState() !$= "Dead")
      %obj.setImageAmmo(%slot, %state);
}   