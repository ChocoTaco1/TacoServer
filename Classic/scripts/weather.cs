datablock AudioProfile(Universal_Rain_Light_1)
{
   filename    = "fx/environment/rain_light_1.wav";
   description = AudioLooping2d;
};

datablock PrecipitationData(Rain)
{
   type = 0;
   soundProfile = "Universal_Rain_Light_1";
   materialList = "raindrops.dml";
   sizeX = 0.2;
   sizeY = 0.45;

   movingBoxPer = 0.35;
   divHeightVal = 1.5;
   sizeBigBox = 1;
   topBoxSpeed = 20;
   frontBoxSpeed = 30;
   topBoxDrawPer = 0.5;
   bottomDrawHeight = 40;
   skipIfPer = -0.3;
   bottomSpeedPer = 1.0;
   frontSpeedPer = 1.5;
   frontRadiusPer = 0.5;

};

datablock PrecipitationData(Snow)
{
   type = 1;
   materialList = "snowflakes.dml";
   sizeX = 0.20;
   sizeY = 0.20;
   
   movingBoxPer = 0.35;
   divHeightVal = 1.5;
   sizeBigBox = 1;
   topBoxSpeed = 20;
   frontBoxSpeed = 30;
   topBoxDrawPer = 0.5;
   bottomDrawHeight = 40;
   skipIfPer = -0.3;
   bottomSpeedPer = 1.0;
   frontSpeedPer = 1.5;
   frontRadiusPer = 0.5;
};

datablock PrecipitationData(Sand)
{
   type = 2;
   maxSize = 2;

   movingBoxPer = 0.35;
   divHeightVal = 1.5;
   sizeBigBox = 1;
   topBoxSpeed = 20;
   frontBoxSpeed = 30;
   topBoxDrawPer = 0.5;
   bottomDrawHeight = 40;
   skipIfPer = -0.3;
   bottomSpeedPer = 1.0;
   frontSpeedPer = 1.5;
   frontRadiusPer = 0.5;
};

function Sky::setStormClouds(%obj, %inStartT, %inLengthT, %outStartT, %outLengthT)
{
   if(%inStartT < %outStartT)
      %obj.stormCloudsShow(false);
   else
      %obj.stormCloudsShow(true);

   %obj.schedule(%inStartT*1000, "stormClouds", 1, %inLengthT);
   %obj.schedule(%outStartT*1000, "stormClouds", 0, %outLengthT);
}

function Sky::setStormFog(%obj, %inStartT, %inLengthT, %inPer, %outStartT, %outLengthT, %outPer)
{
   if(%inStartT < %outStartT)
      %obj.stormFogShow(false);
   else
      %obj.stormFogShow(true);
   %obj.schedule(%inStartT*1000, "stormFog", %inPer, %inLengthT);
   %obj.schedule(%outStartT*1000, "stormFog", %outPer, %outLengthT);
}

function Precipitation::setStorm(%obj, %inStartT, %inLengthT, %inPer, %outStartT, %outLengthT, %outPer)
{
   if(%inStartT < %outStartT)
      %obj.stormShow(false);
   else
      %obj.stormShow(true);
   %obj.schedule(%inStartT*1000, "stormPrecipitation", %inPer, %inLengthT);
   %obj.schedule(%outStartT*1000, "stormPrecipitation", %outPer, %outLengthT);
}

function testStorm()
{
   Sky.setStormClouds(0, 60, 70, 60);
   Sky.setStormFog(0, 60, 1, 70, 60, 0);
   Precipitation.setStorm(30, 30, 1, 70, 30, 0);
}


//--------------------------------------------------------------------------
// Fireball data
//--------------------------------------------------------------------------

datablock ParticleData( FireballAtmosphereCrescentParticle )
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = -0.0;
   lifetimeMS           = 600;
   lifetimeVarianceMS   = 000;
   textureName          = "special/crescent3";
   colors[0]     = "1.0 0.75 0.2 1.0";
   colors[1]     = "1.0 0.75 0.2 0.5";
   colors[2]     = "1.0 0.75 0.2 0.0";
   sizes[0]      = 2.0;
   sizes[1]      = 4.0;
   sizes[2]      = 5.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( FireballAtmosphereCrescentEmitter )
{
   ejectionPeriodMS = 25;
   periodVarianceMS = 0;
   ejectionVelocity = 20;
   velocityVariance = 5.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 200;
   particles = "FireballAtmosphereCrescentParticle";
};

datablock ParticleData(FireballAtmosphereExplosionParticle)
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.2;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 750;
   lifetimeVarianceMS   = 150;
   textureName          = "particleTest";
   colors[0]     = "0.56 0.36 0.26 1.0";
   colors[1]     = "0.56 0.36 0.26 0.0";
   sizes[0]      = 1;
   sizes[1]      = 2;
};

datablock ParticleEmitterData(FireballAtmosphereExplosionEmitter)
{
   ejectionPeriodMS = 7;
   periodVarianceMS = 0;
   ejectionVelocity = 12;
   velocityVariance = 1.75;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 60;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   particles = "FireballAtmosphereExplosionParticle";
};


datablock ExplosionData(FireballAtmosphereSubExplosion1)
{
   explosionShape = "effect_plasma_explosion.dts";
   faceViewer           = true;

   delayMS = 50;

   offset = 3.0;

   playSpeed = 1.5;

   sizes[0] = "0.5 0.5 0.5";
   sizes[1] = "0.5 0.5 0.5";
   times[0] = 0.0;
   times[1] = 1.0;

};             

datablock ExplosionData(FireballAtmosphereSubExplosion2)
{
   explosionShape = "effect_plasma_explosion.dts";
   faceViewer           = true;

   delayMS = 100;

   offset = 3.5;

   playSpeed = 1.0;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "1.0 1.0 1.0";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(FireballAtmosphereSubExplosion3)
{
   explosionShape = "effect_plasma_explosion.dts";
   faceViewer           = true;

   delayMS = 0;

   offset = 0.0;

   playSpeed = 0.7;


   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "2.0 2.0 2.0";
   times[0] = 0.0;
   times[1] = 1.0;

};

datablock ExplosionData(FireballAtmosphereBoltExplosion)
{
   soundProfile   = PlasmaBarrelExpSound;
   particleEmitter = FireballAtmosphereExplosionEmitter;
   particleDensity = 250;
   particleRadius = 1.25;
   faceViewer = true;

   emitter[0] = FireballAtmosphereCrescentEmitter;

   subExplosion[0] = FireballAtmosphereSubExplosion1;
   subExplosion[1] = FireballAtmosphereSubExplosion2;
   subExplosion[2] = FireballAtmosphereSubExplosion3;

   shakeCamera = true;
   camShakeFreq = "10.0 9.0 9.0";
   camShakeAmp = "70.0 70.0 70.0";
   camShakeDuration = 1.3;
   camShakeRadius = 15.0;
};

datablock ParticleData(FireballAtmosphereParticle)
{
   dragCoeffiecient     = 0.0;
   gravityCoefficient   = -0.0;
   inheritedVelFactor   = 0.85;

   lifetimeMS           = 1600;
   lifetimeVarianceMS   = 0;

   textureName          = "particleTest";

   useInvAlpha = false;
   spinRandomMin = -100.0;
   spinRandomMax = 100.0;

   animateTexture = true;
   framesPerSec = 15;

   animTexName[00]       = "special/Explosion/exp_0002";
   animTexName[01]       = "special/Explosion/exp_0004";
   animTexName[02]       = "special/Explosion/exp_0006";
   animTexName[03]       = "special/Explosion/exp_0008";
   animTexName[04]       = "special/Explosion/exp_0010";
   animTexName[05]       = "special/Explosion/exp_0012";
   animTexName[06]       = "special/Explosion/exp_0014";
   animTexName[07]       = "special/Explosion/exp_0016";
   animTexName[08]       = "special/Explosion/exp_0018";
   animTexName[09]       = "special/Explosion/exp_0020";
   animTexName[10]       = "special/Explosion/exp_0022";
   animTexName[11]       = "special/Explosion/exp_0024";
   animTexName[12]       = "special/Explosion/exp_0026";
   animTexName[13]       = "special/Explosion/exp_0028";
   animTexName[14]       = "special/Explosion/exp_0030";
   animTexName[15]       = "special/Explosion/exp_0032";
   animTexName[16]       = "special/Explosion/exp_0034";
   animTexName[17]       = "special/Explosion/exp_0036";
   animTexName[18]       = "special/Explosion/exp_0038";
   animTexName[19]       = "special/Explosion/exp_0040";
   animTexName[20]       = "special/Explosion/exp_0042";
   animTexName[21]       = "special/Explosion/exp_0044";
   animTexName[22]       = "special/Explosion/exp_0046";
   animTexName[23]       = "special/Explosion/exp_0048";
   animTexName[24]       = "special/Explosion/exp_0050";
   animTexName[25]       = "special/Explosion/exp_0052";


   colors[0]     = "1.0 0.7 0.5 1.0";
   colors[1]     = "1.0 0.5 0.2 1.0";
   colors[2]     = "1.0 0.25 0.1 0.0";
   sizes[0]      = 10.0;
   sizes[1]      = 4.0;
   sizes[2]      = 2.0;
   times[0]      = 0.0;
   times[1]      = 0.2;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(FireballAtmosphereEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;

   ejectionVelocity = 0.25;
   velocityVariance = 0.0;

   thetaMin         = 0.0;
   thetaMax         = 30.0;

   particles = "FireballAtmosphereParticle";
};

datablock DebrisData( FireballAtmosphereDebris )
{
   emitters[0] = FireballAtmosphereEmitter;

   explosion = FireballAtmosphereBoltExplosion;
   explodeOnMaxBounce = true;

   elasticity = 0.0;
   friction = 1.0;

   lifetime = 100.0;
   lifetimeVariance = 0.0;

   numBounces = 0;
   bounceVariance = 0;

   ignoreWater = false;
};             

datablock FireballAtmosphereData(Fireball)
{
   fireball = FireballAtmosphereDebris;
};

