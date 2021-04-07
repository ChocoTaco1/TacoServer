// grenade (thrown by hand) script
// ------------------------------------------------------------------------

datablock EffectProfile(ConcussionGrenadeThrowEffect)
{
   effectname = "weapons/grenade_throw";
   minDistance = 2.5;
   maxDistance = 2.5;
};

datablock EffectProfile(ConcussionGrenadeSwitchEffect)
{
   effectname = "weapons/generic_switch";
   minDistance = 2.5;
   maxDistance = 2.5;
};

datablock EffectProfile(ConcussionGrenadeExplosionEffect)
{
   effectname = "explosions/grenade_explode";
   minDistance = 10;
   maxDistance = 50;
};

datablock AudioProfile(ConcussionGrenadeExplosionSound)
{
   filename = "fx/weapons/grenade_explode.wav";
   description = AudioExplosion3d;
   preload = true;
   effect = ConcussionGrenadeExplosionEffect;
};

// ------------------------------------------------------
// z0dd - ZOD, 5/8/02. Duplicate datablock, waste of mem.
//datablock AudioProfile(ConcussionGrenadeExplosionSound)
//{
//   filename = "fx/weapons/grenade_explode.wav";
//   description = AudioExplosion3d;
//   preload = true;
//   effect = ConcussionGrenadeExplosionEffect;
//};
// ------------------------------------------------------

//--------------------------------------------------------------------------
// Sparks
//--------------------------------------------------------------------------
datablock ParticleData(ConcussionGrenadeSparks)
{
   dragCoefficient      = 1;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 350;
   textureName          = "special/bigSpark";
   colors[0]     = "0.56 0.36 1.0 1.0";
   colors[1]     = "0.56 0.36 1.0 1.0";
   colors[2]     = "1.0 0.36 1.0 0.0";
   sizes[0]      = 0.5;
   sizes[1]      = 0.25;
   sizes[2]      = 0.25;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(ConcussionGrenadeSparkEmitter)
{
   ejectionPeriodMS = 1;
   periodVarianceMS = 0;
   ejectionVelocity = 12;
   velocityVariance = 6.75;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 180;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 100;
   particles = "ConcussionGrenadeSparks";
};

datablock ParticleData( ConcussionGrenadeCrescentParticle )
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = -0.0;
   lifetimeMS           = 600;
   lifetimeVarianceMS   = 000;
   textureName          = "special/crescent3";
   colors[0] = "0.8 0.8 1.0 1.00";
   colors[1] = "0.8 0.5 1.0 0.20";
   colors[2] = "0.2 0.8 1.0 0.0";
   sizes[0]      = 2.0;
   sizes[1]      = 4.0;
   sizes[2]      = 5.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( ConcussionGrenadeCrescentEmitter )
{
   ejectionPeriodMS = 15;
   periodVarianceMS = 0;
   ejectionVelocity = 20;
   velocityVariance = 10.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 200;
   particles = "ConcussionGrenadeCrescentParticle";
};

//--------------------------------------------------------------------------
// Shockwave
//--------------------------------------------------------------------------
datablock ShockwaveData(ConcussionGrenadeShockwave)
{
   width = 4.0;
   numSegments = 20;
   numVertSegments = 2;
   velocity = 5;
   acceleration = 10.0;
   lifetimeMS = 1000;
   height = 1.0;
   is2D = true;

   texture[0] = "special/shockwave4";
   texture[1] = "special/gradient";
   texWrap = 6.0;

   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

   colors[0] = "0.8 0.8 1.0 1.00";
   colors[1] = "0.8 0.5 1.0 0.20";
   colors[2] = "0.2 0.8 1.0 0.0";
};

//--------------------------------------------------------------------------
// Explosion
//--------------------------------------------------------------------------
datablock ExplosionData(ConcussionGrenadeExplosion)
{
   soundProfile   = ConcussionGrenadeExplosionSound;
   shockwave =  ConcussionGrenadeShockwave;

   emitter[0] = ConcussionGrenadeSparkEmitter;
   emitter[1] = ConcussionGrenadeCrescentEmitter;

   shakeCamera = true;
   camShakeFreq = "4.0 5.0 4.5";
   camShakeAmp = "140.0 140.0 140.0";
   camShakeDuration = 1.0;
   camShakeRadius = 15.0;
};

//--------------------------------------------------------------------------
// Item Data
//--------------------------------------------------------------------------
datablock ItemData(ConcussionGrenadeThrown)
{
   shapeFile = "grenade.dts";
   mass = 0.7;
   elasticity = 0.2;
   friction = 1;
   pickupRadius = 2;
   maxDamage = 0.5;
   explosion = ConcussionGrenadeExplosion;
   damageRadius        = 15.0;
   radiusDamageType    = $DamageType::Grenade;
   kickBackStrength    = 3500;

   computeCRC = true;

};

datablock ItemData(ConcussionGrenade)
{
   className = HandInventory;
   catagory = "Handheld";
   shapeFile = "grenade.dts";
   mass = 0.7;
   elasticity = 0.2;
   friction = 1;
   pickupRadius = 2;
   thrownItem = ConcussionGrenadeThrown;
	pickUpName = "some concussion grenades";
	isGrenade = true;
};

//--------------------------------------------------------------------------
// Functions:
//--------------------------------------------------------------------------
function ConcussionGrenadeThrown::onCollision( %data, %obj, %col )
{
   // Do nothing...
}

