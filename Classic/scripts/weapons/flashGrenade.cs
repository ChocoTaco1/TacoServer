// grenade (thrown by hand) script
// ------------------------------------------------------------------------
datablock EffectProfile(FlashGrenadeExplosionEffect)
{
   effectname = "explosions/grenade_flash_explode";
   minDistance = 10;
   maxDistance = 30;
};

datablock AudioProfile(FlashGrenadeExplosionSound)
{
   filename = "fx/explosions/grenade_flash_explode.wav";
   description = AudioExplosion3d;
   preload = true;
   effect = FlashGrenadeExplosionEffect;
};

datablock ExplosionData(FlashGrenadeExplosion)
{
   explosionShape = "disc_explosion.dts";
   soundProfile   = FlashGrenadeExplosionSound;

   faceViewer     = true;
};

datablock ItemData(FlashGrenadeThrown)
{
   shapeFile = "grenade_flash.dts"; // z0dd - ZOD, 5/19/03. Was grenade.dts
   mass = 0.7;
   elasticity = 0.2;
   friction = 1;
   pickupRadius = 2;
   maxDamage = 0.4;
   explosion = FlashGrenadeExplosion;
   indirectDamage = 0.5;
   damageRadius = 10.0;
   radiusDamageType = $DamageType::Grenade;
   kickBackStrength = 1000;
   computeCRC = true;
   maxWhiteout = 0.9; // z0dd - ZOD, 9/8/02. Was 1.2
};

datablock ItemData(FlashGrenade)
{
   className = HandInventory;
   catagory = "Handheld";
   shapeFile = "grenade_flash.dts"; // z0dd - ZOD, 5/19/03. Was grenade.dts
   mass = 0.7;
   elasticity = 0.2;
   friction = 1;
   pickupRadius = 2;
   thrownItem = FlashGrenadeThrown;
   pickUpName = "some flash grenades";
   isGrenade = true;
   //computeCRC = true; // z0dd - ZOD, 5/19/03. Only need to check this model once.
};

//--------------------------------------------------------------------------
// Functions:
//--------------------------------------------------------------------------
function FlashGrenadeThrown::onCollision( %data, %obj, %col )
{
   // Do nothing...
}

