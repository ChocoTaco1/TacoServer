//--------------------------------------------------------------------------
// Satchel Charge pack
// can be used by any armor type
// when activated, throws the pack -- when activated again (before
// picking up another pack), detonates with a BIG explosion.

//--------------------------------------------------------------------------
// Sounds

datablock EffectProfile(SatchelChargeActivateEffect)
{
   effectname = "packs/satchel_pack_activate";
   minDistance = 2.5;
   maxDistance = 2.5;
};

datablock EffectProfile(SatchelChargeExplosionEffect)
{
   effectname = "packs/satchel_pack_detonate";
   minDistance = 2.5;
   maxDistance = 5.0;
};

datablock EffectProfile(SatchelChargePreExplosionEffect)
{
   effectname = "explosions/explosion.xpl03";
   minDistance = 10.0;
   maxDistance = 30.0;
};

datablock AudioProfile(SatchelChargeActivateSound)
{
   filename    = "fx/packs/satchel_pack_activate.wav";
   description = AudioClose3d;
   preload = true;
   effect = SatchelChargeActivateEffect;
};

datablock AudioProfile(SatchelChargeExplosionSound)
{
   filename = "fx/packs/satchel_pack_detonate.wav";
   description = AudioBIGExplosion3d;
   preload = true;
   effect = SatchelChargeExplosionEffect;
};

datablock AudioProfile(SatchelChargePreExplosionSound)
{
   filename    = "fx/explosions/explosion.xpl03.wav";
   description = AudioBIGExplosion3d;
   preload = true;
   effect = SatchelChargePreExplosionEffect;
};

datablock AudioProfile(UnderwaterSatchelChargeExplosionSound)
{
   filename    = "fx/weapons/mortar_explode_UW.wav";
   description = AudioBIGExplosion3d;
   preload = true;
   effect = SatchelChargeExplosionEffect;
};

//----------------------------------------------------------------------------
// Satchel Debris
//----------------------------------------------------------------------------
datablock ParticleData( SDebrisSmokeParticle )
{
   dragCoeffiecient     = 1.0;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;

   lifetimeMS           = 1000;  
   lifetimeVarianceMS   = 100;

   textureName          = "particleTest";

   useInvAlpha =     true;

   spinRandomMin = -60.0;
   spinRandomMax = 60.0;

   colors[0]     = "0.4 0.4 0.4 1.0";
   colors[1]     = "0.3 0.3 0.3 0.5";
   colors[2]     = "0.0 0.0 0.0 0.0";
   sizes[0]      = 0.0;
   sizes[1]      = 2.0;
   sizes[2]      = 3.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( SDebrisSmokeEmitter )
{
   ejectionPeriodMS = 7;
   periodVarianceMS = 1;

   ejectionVelocity = 1.0;  // A little oomph at the back end
   velocityVariance = 0.2;

   thetaMin         = 0.0;
   thetaMax         = 40.0;

   particles = "SDebrisSmokeParticle";
};


datablock DebrisData( SatchelDebris )
{
   emitters[0] = SDebrisSmokeEmitter;

   explodeOnMaxBounce = true;

   elasticity = 0.4;
   friction = 0.2;

   lifetime = 0.3;
   lifetimeVariance = 0.02;
};             

//----------------------------------------------------------------------------
// Bubbles
//----------------------------------------------------------------------------
datablock ParticleData(SatchelBubbleParticle)
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

datablock ParticleEmitterData(SatchelBubbleEmitter)
{
   ejectionPeriodMS = 10;
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

//--------------------------------------------------------------------------
// Satchel Explosion Particle effects
//--------------------------------------
datablock ParticleData(SatchelExplosionSmoke)
{
   dragCoeffiecient     = 0.4;
   gravityCoefficient   = -0.0;   // rises slowly
   inheritedVelFactor   = 0.025;

   lifetimeMS           = 2000;
   lifetimeVarianceMS   = 0;

   textureName          = "particleTest";

   useInvAlpha =  true;
   spinRandomMin = -200.0;
   spinRandomMax =  200.0;

   textureName = "special/Smoke/smoke_001";

   colors[0]     = "1.0 0.7 0.0 1.0";
   colors[1]     = "0.2 0.2 0.2 0.5";
   colors[2]     = "0.0 0.0 0.0 0.0";
   sizes[0]      = 7.0;
   sizes[1]      = 17.0;
   sizes[2]      = 2.0;
   times[0]      = 0.0;
   times[1]      = 0.4;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(SatchelExplosionSmokeEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;

   ejectionVelocity = 14.25;
   velocityVariance = 2.25;

   thetaMin         = 0.0;
   thetaMax         = 180.0;

   lifetimeMS       = 200;

   particles = "SatchelExplosionSmoke";
};

datablock ParticleData(UnderwaterSatchelExplosionSmoke)
{
   dragCoeffiecient     = 105.0;
   gravityCoefficient   = -0.0;
   inheritedVelFactor   = 0.025;

   constantAcceleration = -1.0;
   
   lifetimeMS           = 1500;
   lifetimeVarianceMS   = 0;

   textureName          = "particleTest";

   useInvAlpha =  false;
   spinRandomMin = -200.0;
   spinRandomMax =  200.0;

   textureName = "special/Smoke/smoke_001";

   colors[0]     = "0.4 0.4 1.0 1.0";
   colors[1]     = "0.4 0.4 1.0 0.5";
   colors[2]     = "0.0 0.0 0.0 0.0";
   sizes[0]      = 7.0;
   sizes[1]      = 17.0;
   sizes[2]      = 2.0;
   times[0]      = 0.0;
   times[1]      = 0.2;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(UnderwaterSatchelExplosionSmokeEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;

   ejectionVelocity = 14.25;
   velocityVariance = 2.25;

   thetaMin         = 0.0;
   thetaMax         = 180.0;

   lifetimeMS       = 200;

   particles = "UnderwaterSatchelExplosionSmoke";
};


datablock ParticleData(SatchelSparks)
{
   dragCoefficient      = 1;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 150;
   textureName          = "special/bigSpark";
   colors[0]     = "0.56 0.36 0.26 1.0";
   colors[1]     = "0.56 0.36 0.26 1.0";
   colors[2]     = "1.0 0.36 0.26 0.0";
   sizes[0]      = 0.5;
   sizes[1]      = 0.5;
   sizes[2]      = 0.75;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(SatchelSparksEmitter)
{
   ejectionPeriodMS = 1;
   periodVarianceMS = 0;
   ejectionVelocity = 40;
   velocityVariance = 20.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 180;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 200;
   particles = "SatchelSparks";
};

datablock ParticleData(UnderwaterSatchelSparks)
{
   dragCoefficient      = 1;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 350;
   textureName          = "special/underwaterSpark";
   colors[0]     = "0.6 0.6 1.0 1.0";
   colors[1]     = "0.6 0.6 1.0 1.0";
   colors[2]     = "0.6 0.6 1.0 0.0";
   sizes[0]      = 0.5;
   sizes[1]      = 0.5;
   sizes[2]      = 0.75;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(UnderwaterSatchelSparksEmitter)
{
   ejectionPeriodMS = 2;
   periodVarianceMS = 0;
   ejectionVelocity = 30;
   velocityVariance = 5.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 70;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 100;
   particles = "UnderwaterSatchelSparks";
};


//---------------------------------------------------------------------------
// Explosion
//---------------------------------------------------------------------------

datablock ExplosionData(SatchelSubExplosion)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "0.5 0.5 0.5";

   debris = SatchelDebris;
   debrisThetaMin = 10;
   debrisThetaMax = 80;
   debrisNum = 8;
   debrisVelocity = 60.0;
   debrisVelocityVariance = 15.0;

   lifetimeMS = 1000;
   delayMS = 0;

   emitter[0] = SatchelExplosionSmokeEmitter;
   emitter[1] = SatchelSparksEmitter;

   offset = 0.0;

   playSpeed = 1.5;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "3.0 3.0 3.0";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(SatchelSubExplosion2)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "0.7 0.7 0.7";

   debris = SatchelDebris;
   debrisThetaMin = 10;
   debrisThetaMax = 170;
   debrisNum = 8;
   debrisVelocity = 60.0;
   debrisVelocityVariance = 15.0;

   lifetimeMS = 1000;
   delayMS = 50;

   emitter[0] = SatchelExplosionSmokeEmitter;
   emitter[1] = SatchelSparksEmitter;

   offset = 9.0;

   playSpeed = 1.5;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "1.5 1.5 1.5";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(SatchelSubExplosion3)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "1.0 1.0 1.0";

   debris = SatchelDebris;
   debrisThetaMin = 10;
   debrisThetaMax = 170;
   debrisNum = 8;
   debrisVelocity = 60.0;
   debrisVelocityVariance = 15.0;

   lifetimeMS = 2000;
   delayMS = 100;

   emitter[0] = SatchelExplosionSmokeEmitter;
   emitter[1] = SatchelSparksEmitter;

   offset = 9.0;

   playSpeed = 2.5;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "1.0 1.0 1.0";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(SatchelMainExplosion)
{
   soundProfile = SatchelChargePreExplosionSound;
   
   subExplosion[0] = SatchelSubExplosion;
   subExplosion[1] = SatchelSubExplosion2;
   subExplosion[2] = SatchelSubExplosion3;
};

//---------------------------------------------------------------------------
// Underwater Explosion
//---------------------------------------------------------------------------

datablock ExplosionData(UnderwaterSatchelSubExplosion)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "0.5 0.5 0.5";


   lifetimeMS = 1000;
   delayMS = 0;

   emitter[0] = UnderwaterSatchelExplosionSmokeEmitter;
   emitter[1] = UnderwaterSatchelSparksEmitter;
   emitter[2] = SatchelBubbleEmitter;

   offset = 0.0;

   playSpeed = 0.75;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "2.5 2.5 2.5";
   sizes[2] = "2.0 2.0 2.0";
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;
};

datablock ExplosionData(UnderwaterSatchelSubExplosion2)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "0.7 0.7 0.7";


   lifetimeMS = 1000;
   delayMS = 50;

   emitter[0] = UnderwaterSatchelExplosionSmokeEmitter;
   emitter[1] = UnderwaterSatchelSparksEmitter;
   emitter[2] = SatchelBubbleEmitter;

   offset = 9.0;

   playSpeed = 0.75;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "1.0 1.0 1.0";
   sizes[2] = "0.75 0.75 0.75";
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;
};

datablock ExplosionData(UnderwaterSatchelSubExplosion3)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "1.0 1.0 1.0";


   lifetimeMS = 2000;
   delayMS = 100;

   emitter[0] = UnderwaterSatchelExplosionSmokeEmitter;
   emitter[1] = UnderwaterSatchelSparksEmitter;
   emitter[2] = SatchelBubbleEmitter;

   offset = 9.0;

   playSpeed = 1.25;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "1.0 1.0 1.0";
   sizes[2] = "0.5 0.5 0.5";
   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;
};

datablock ExplosionData(UnderwaterSatchelMainExplosion)
{
   soundProfile = UnderwaterSatchelChargeExplosionSound;

   subExplosion[0] = UnderwaterSatchelSubExplosion;
   subExplosion[1] = UnderwaterSatchelSubExplosion2;
   subExplosion[2] = UnderwaterSatchelSubExplosion3;
};


//--------------------------------------------------------------------------
// Projectile

//-------------------------------------------------------------------------
// shapebase datablocks
datablock ShapeBaseImageData(SatchelChargeImage)
{
   shapeFile = "pack_upgrade_satchel.dts";
   item = SatchelCharge;
   mountPoint = 1;
   offset = "0 0 0";
   emap = true;
};

datablock ItemData(SatchelCharge)
{
   className = Pack;
   catagory = "Packs";
   image = SatchelChargeImage;
   shapeFile = "pack_upgrade_satchel.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   rotate = true;
   pickUpName = "a satchel charge pack";

   computeCRC = true;
};

datablock ItemData(SatchelChargeTossed)
{
   shapeFile = "pack_upgrade_satchel.dts";
   mass = 1.2;
   elasticity = 0.1;
   friction = 0.9;
   rotate = false;
   pickupRadius = 0;
   noTimeout = true;
   sticky = true;
};

datablock StaticShapeData(SatchelChargeThrown) : StaticShapeDamageProfile
{
   shapeFile = "pack_upgrade_satchel.dts";
   explosion = SatchelMainExplosion;
   underwaterExplosion = UnderwaterSatchelMainExplosion;
   armDelay = 2500;
   maxDamage = 0.6;

   disabledLevel = 0.5;
   destroyedLevel = 0.6;
   dynamicType = $TypeMasks::StaticShapeObjectType;
   renderWhenDestroyed = false;

   kickBackStrength    = 4000;
};

//--------------------------------------------------------------------------

function SatchelCharge::onUse(%this, %obj)
{
   %item = new Item() {
      dataBlock = SatchelChargeTossed;
      rotation = "0 0 1 " @ (getRandom() * 360);
   };
   MissionCleanup.add(%item);
   // take pack out of inventory and unmount image
   %obj.decInventory(SatchelCharge, 1);
   %obj.throwObject(%item);
   %item.sourceObject = %obj;

   // z0dd - ZOD, 5/16/02. Schedule a check to see if the satchel is at rest but not stuck to anything
   %item.checkCount = 0;
   %item.velocCheck = %item.getDataBlock().schedule(1000, "checkVelocity", %item);
}

function initArmSatchelCharge(%satchel)
{
   // "deet deet deet" sound
   %satchel.playAudio(1, SatchelChargeActivateSound);
   // also need to play "antenna extending" animation
   %satchel.playThread(0, "deploy");
   %satchel.playThread(1, "activate");

   // delay the actual arming until after sound is done playing
   schedule( 2200, 0, "armSatchelCharge", %satchel );
}

function armSatchelCharge(%satchel)
{
   %satchel.armed = true;
   commandToClient( %satchel.sourceObject.client, 'setSatchelArmed' );
}

function detonateSatchelCharge(%player)
{
   %satchelCharge = %player.thrownChargeId;
   // can't detonate the satchel charge if it isn't armed
   if(!%satchelCharge.armed)
      return;

   //error("Detonating satchel charge #" @ %satchelCharge @ " for player #" @ %player);

   if(%satchelCharge.getDamageState() !$= Destroyed)
   {
      %satchelCharge.setDamageState(Destroyed);
      %satchelCharge.blowup(); 
   }
   
   // Clear the player's HUD:
   %player.client.clearBackpackIcon();   
}

function SatchelChargeThrown::onEnterLiquid(%data, %obj, %coverage, %type)
{
   // lava types
   if(%type >=4 && %type <= 6)
   {
      if(%obj.getDamageState() !$= "Destroyed")
      {
         %obj.armed = true;
         detonateSatchelCharge(%obj.sourceObject);
         return;
      }
   }
   
   // quickSand   
   if(%type == 7)
      if(isObject(%obj.sourceObject))
         %obj.sourceObject.thrownChargeId = 0;
      
  Parent::onEnterLiquid(%data, %obj, %coverage, %type);
}

function SatchelChargeImage::onMount(%data, %obj, %node)
{
   %obj.thrownChargeId = 0;
}

function SatchelChargeImage::onUnmount(%data, %obj, %node)
{
}

function SatchelChargeThrown::onDestroyed(%this, %object, %lastState)
{
   if(%object.kaboom)
      return;
   else
   {
      %object.kaboom = true;

      // the "thwart" flag is set if the charge is destroyed with weapons rather
      // than detonated. A less damaging explosion, but visually the same scale.
      if(%object.thwart)
      {
         messageClient(%object.sourceObject.client, 'msgSatchelChargeDetonate', "\c2Satchel charge destroyed.");
         %dmgRadius = 15; // z0dd - ZOD, 9/27/02. Was 10
         %dmgMod = 0.35; // z0dd - ZOD, 9/27/02. Was 0.3
         %expImpulse = 2000; // z0dd - ZOD, 9/27/02. Was 1000
         %dmgType = $DamageType::Explosion;
      }
      else
      {
         messageClient(%object.sourceObject.client, 'msgSatchelChargeDetonate', "\c2Satchel charge detonated!");
         %dmgRadius = 25; // z0dd - ZOD, 9/27/02. Was 20
         %dmgMod = 1.15; // z0dd - ZOD, 9/27/02. Was 1.0
         %expImpulse = 5000; // z0dd - ZOD, 9/27/02. Was 2500
         %dmgType = $DamageType::SatchelCharge;
      }

      %object.blowingUp = true;
      RadiusExplosion(%object, %object.getPosition(), %dmgRadius, %dmgMod, %expImpulse, %object.sourceObject, %dmgType);

      %object.schedule(1000, "delete");
   }

   // --------------------------------------------------------------------------------
   // z0dd - ZOD, 4/25/02. Satchel bug fix. Prior to fix, clients couldn't pick up 
   // packs when satchel was destroyed from dmg
   if(isObject(%object.sourceObject))
      %object.sourceObject.thrownChargeId = 0;
   // --------------------------------------------------------------------------------
}

function SatchelChargeThrown::onCollision(%data,%obj,%col)
{
   // Do nothing...
}

function SatchelChargeThrown::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
   if (!%object.blowingUp)
   {
      %targetObject.damaged += %amount;

      if(%targetObject.damaged >= %targetObject.getDataBlock().maxDamage && 
         %targetObject.getDamageState() !$= Destroyed)
      {   
         %targetObject.thwart = true;
         %targetObject.setDamageState(Destroyed);
         %targetObject.blowup(); 
         
         // clear the player's HUD:
         %targetObject.sourceObject.client.clearBackPackIcon();
      }
   }
}
// z0dd - ZOD, 5/18/03. Removed functions, created parent. Streamline.
//function SatchelCharge::onPickup(%this, %obj, %shape, %amount)
//{
   // created to prevent console errors
//}

//**************************************************************
// STICKY SATCHEL FUNCTIONS: z0dd - ZOD, 5/16/02
//**************************************************************

function SatchelChargeTossed::onEnterLiquid(%data, %obj, %coverage, %type)
{
   // If it lands in lava or quicksand, delete it
   if(%type >=4 && %type <= 7)
   {
      cancel(%obj.velocCheck);
      if(isObject(%obj.sourceObject))
         %obj.sourceObject.thrownChargeId = 0;

      %obj.sourceObject.client.clearBackPackIcon();
      %obj.schedule(100, "delete");
   }
   else
      cancel(%obj.velocCheck);
}

function SatchelChargeTossed::onLeaveLiquid(%data, %obj, %type)
{
   // On the off chance it passes through to air, reschedule the velocity check
   %obj.checkCount = 0;
   %obj.velocCheck = %obj.getDataBlock().schedule(1000, "checkVelocity", %obj);
}

function SatchelChargeTossed::onCollision(%data, %obj, %col)
{
   // Lets keep thing from floating mid air, the check velocity should handle it afterwards
   if(%col.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::TurretObjectType))
   {
      %vec = (-1.0 + getRandom() * 2.0) SPC (-1.0 + getRandom() * 2.0) SPC getRandom();
      %vec = vectorScale(%vec, 15);
      %pos = %col.getWorldBoxCenter();
      %obj.applyImpulse(%pos, %vec);
   }
}

function SatchelChargeTossed::checkVelocity(%data, %item)
{
   %item.checkCount++;
   if(VectorLen(%item.getVelocity()) < 0.1)
   {
      // Satchel has come to rest but not activated, probably on a 
      // staticshape (station, gen, etc) lets force activation
      cancel(%item.velocCheck);
      activateSatchel(posFromTransform(%item.getTransform()), rotFromTransform(%item.getTransform()), %item.sourceObject);
      %item.schedule(100, "delete");
   }
   else if(%item.checkCount >= 6)
   {
      // satchel's still moving but it's been checked several times,
      // probably thrown off face of earth, delete it
      cancel(%item.velocCheck);
      if(isObject(%item.sourceObject))
         %item.sourceObject.thrownChargeId = 0;

      %item.sourceObject.client.clearBackPackIcon();
      %item.schedule(100, "delete");
   }
   else
   {
      // check back in a little while
      %item.velocCheck = %data.schedule(1000, "checkVelocity", %item);
   }
}

function SatchelChargeTossed::onStickyCollision(%data, %obj)
{
   // We have sticky! Lets setup for the actual charge
   cancel(%obj.velocCheck);
   %pos = %obj.getLastStickyPos();
   %norm = %obj.getLastStickyNormal();
   %intAngle = getTerrainAngle(%norm);
   %rotAxis = vectorNormalize(vectorCross(%norm, "0 0 1"));
   if (getWord(%norm, 2) == 1 || getWord(%norm, 2) == -1)
      %rotAxis = vectorNormalize(vectorCross(%norm, "0 1 0"));

   %rot = %rotAxis @ " " @ %intAngle;
   activateSatchel(%pos, %rot, %obj.sourceObject);
   %obj.schedule(50, "delete");
}

function activateSatchel(%pos, %rot, %source)
{
  // Create the charge and schedule arming
  %satchel = new StaticShape() {
      dataBlock = SatchelChargeThrown;
      sourceObject = %source;
      position = %pos;
      rotation = %rot;
   };
   MissionCleanup.add(%satchel);
   %source.thrownChargeId = %satchel;
   %satchel.armed = false;
   %satchel.damaged = 0.0;
   %satchel.thwart = false;
   messageClient(%source.client, 'MsgSatchelChargePlaced', "\c2Satchel charge deployed.");

   // arm itself 2.5 seconds after creation
   if(%source.thrownChargeId != %satchel) // z0dd - ZOD, 5/19/03. Address multiple charges exploit.
      %satchel.schedule(100, "delete");
   else
      schedule(%satchel.getDatablock().armDelay, %satchel, "initArmSatchelCharge", %satchel);
}
