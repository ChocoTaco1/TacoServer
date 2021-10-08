//--------------------------------------------------------------------------
// Projectiles.cs: Note that the actual projectile blocks are stored with
//                  with weapon that uses them in base/scripts/weapons/*.cs,
//                  the blocks below are only to illustrate the default values
//                  for each block type.  Also, ProjectileData cannot be used
//                  as a concrete datablock type.
//  Inheritance:
//   ProjectileData            : GameBaseData
//   LinearProjectileData      : ProjectileData
//   LinearFlareProjectileData : LinearProjectileData
//   GrenadeProjectileData     : ProjectileData
//   SeekerProjectileData      : ProjectileData
//   SniperProjectileData      : ProjectileData
// 
//--------------------------------------------------------------------------
//-------------------------------------- Default functions
//
function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
   if(isObject(%targetObject)) // z0dd - ZOD, 4/24/02. Console spam fix.
   {
      %targetObject.damage(%projectile.sourceObject, %position, %data.directDamage * %modifier, %data.directDamageType);
   }
}

function SniperProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
   %damageAmount = %data.directDamage * %projectile.damageFactor;

   if(isObject(%targetObject)) // z0dd - ZOD, 4/24/02. Console spam fix.
   {
      if(%targetObject.getDataBlock().getClassName() $= "PlayerData")
      {
         %damLoc = firstWord(%targetObject.getDamageLocation(%position));
         if(%damLoc $= "head")
         {   
            %targetObject.getOwnerClient().headShot = 1;
            %modifier = %data.rifleHeadMultiplier;
         }
         else
         {   
            %modifier = 1;
            %targetObject.getOwnerClient().headShot = 0;
         }
      }
      %targetObject.damage(%projectile.sourceObject, %position, %modifier * %damageAmount, %data.directDamageType);
   }
}

// -------------------------------------------------------
// z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
function resetFire(%obj)
{
   if(isEventPending(%obj.reloadSchedule))
      cancel(%obj.reloadSchedule);

   %obj.cantFire = "";
}
// -------------------------------------------------------

function ShapeBaseImageData::onFire(%data, %obj, %slot)
{
   // ---------------------------------------------------------------------------
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if (%obj.cantFire !$= "")
   {
      return 0;
   }

   %wpnName = %data.getName();
   if((%wpnName $= "MortarImage") || (%wpnName $= "MissileLauncherImage"))
   {
      %obj.cantFire = 1;
      %preventTime = %data.stateTimeoutValue[4];
      %obj.reloadSchedule = schedule(%preventTime * 1000, %obj, resetFire, %obj);
   }
   // ---------------------------------------------------------------------------
   
   %data.lightStart = getSimTime();

   if( %obj.station $= "" && %obj.isCloaked() )
   {
      if( %obj.respawnCloakThread !$= "" )
      {
         Cancel(%obj.respawnCloakThread);
         %obj.setCloaked( false );
         %obj.respawnCloakThread = "";
      }
      else
      {
         if( %obj.getEnergyLevel() > 20 )
         {   
            %obj.setCloaked( false );
            %obj.reCloak = %obj.schedule( 500, "setCloaked", true );
         }
      }   
   }

   if( %obj.client > 0 )
   {   
      %obj.setInvincibleMode(0 ,0.00);
      %obj.setInvincible( false ); // fire your weapon and your invincibility goes away.   
   }
   
   %vehicle = 0;
   if(%data.usesEnergy)
   {
      if(%data.useMountEnergy)
      {
         %useEnergyObj = %obj.getObjectMount();
         if(!%useEnergyObj)
            %useEnergyObj = %obj;
         %energy = %useEnergyObj.getEnergyLevel();
         %vehicle = %useEnergyObj;
      }
      else
         %energy = %obj.getEnergyLevel();
      
      if(%data.useCapacitor && %data.usesEnergy)
      {   
         if( %useEnergyObj.turretObject.getCapacitorLevel() < %data.minEnergy )
         {   
            return;
         }
      }
      else if(%energy < %data.minEnergy)
         return;
   }
   // ---------------------------------------------------------------------
   // z0dd - ZOD, 4/24/02. Code optimization
   if(%data.projectileSpread)
   {
      %vec = %obj.getMuzzleVector(%slot);
      %x = (getRandom() - 0.5) * 2 * 3.1415926 * %data.projectileSpread;
      %y = (getRandom() - 0.5) * 2 * 3.1415926 * %data.projectileSpread;
      %z = (getRandom() - 0.5) * 2 * 3.1415926 * %data.projectileSpread;
      %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
      %vector = MatrixMulVector(%mat, %vec);
   }
   else
   {
      // z0dd - ZOD, 4/10/02. Founder - fixes off center projectile drift.
      //%vector = %obj.getMuzzleVector(%slot);
      %vector = MatrixMulVector("0 0 0 0 0 1 0", %obj.getMuzzleVector(%slot));
   }

   %p = new (%data.projectileType)() {
      dataBlock        = %data.projectile;
      initialDirection = %vector;
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      sourceSlot       = %slot;
      vehicleObject    = %vehicle;
   };
   // End streamlining
   // ---------------------------------------------------------------------
   if (isObject(%obj.lastProjectile) && %obj.deleteLastProjectile)
      %obj.lastProjectile.delete();

   %obj.lastProjectile = %p;
   %obj.deleteLastProjectile = %data.deleteLastProjectile;
   MissionCleanup.add(%p);
   
   // AI hook
   if(%obj.client)
      %obj.client.projectile = %p;

   if(%data.usesEnergy)
   {
      if(%data.useMountEnergy)
      {   
         if( %data.useCapacitor )
         {   
            %vehicle.turretObject.setCapacitorLevel( %vehicle.turretObject.getCapacitorLevel() - %data.fireEnergy );
         }
         else
            %useEnergyObj.setEnergyLevel(%energy - %data.fireEnergy);
      }
      else
         %obj.setEnergyLevel(%energy - %data.fireEnergy);
   }
   else
      %obj.decInventory(%data.ammo,1);
   return %p;
}

function ShapeBaseImageData::onUnmount(%data, %obj, %slot)
{
   if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
   {
      %obj.lastProjectile.delete();
      %obj.lastProjectile = "";
   }
}

function TurretImageData::deconstruct(%data, %obj, %slot)
{
   if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
   {
      %obj.lastProjectile.delete();
      %obj.lastProjectile = "";
   }
}

function ShapeBaseImageData::deconstruct(%data, %obj, %slot)
{
   if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
   {
      %obj.lastProjectile.delete();
      %obj.lastProjectile = "";
   }
}

function MissileLauncherImage::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   //--------------------------------------------------------
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if(!%p)
   {
      return;	
   }
   //--------------------------------------------------------
   MissileSet.add(%p);
   %target = %obj.getLockedTarget();
   if(%target)
   {
      // z0dd - ZOD, 5/07/04. Do not lock onto players if gameplay changes are in affect.
      if($Host::ClassicLoadMissileChanges)
      {
         if(%target.getClassName() !$= Player)
            %p.setObjectTarget(%target);
         else
            %p.setNoTarget();
      }
      else
         %p.setObjectTarget(%target);
   }
   else if(%obj.isLocked())
      %p.setPositionTarget(%obj.getLockedPosition());
   else
      %p.setNoTarget();
}

function MissileLauncherImage::onWetFire(%data, %obj, %slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   //--------------------------------------------------------
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if(!%p)
   {
      return;	
   }
   //--------------------------------------------------------
   MissileSet.add(%p);
   %p.setObjectTarget(0);
}

//--------------------------------------------------------------------------

function MissileBarrelLarge::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data,%obj,%slot);
   //--------------------------------------------------------
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if(!%p)
   {
      return;	
   }
   //--------------------------------------------------------
   MissileSet.add(%p); // z0dd - ZOD, 8/10/03. Bots need this.
   if (%obj.getControllingClient())
   {
      // a player is controlling the turret
      %target = %obj.getLockedTarget();
   }
   else
   {
      // The ai is controlling the turret
      %target = %obj.getTargetObject();
   }
   if(%target)
      %p.setObjectTarget(%target);
   else if(%obj.isLocked())
      %p.setPositionTarget(%obj.getLockedPosition());
   else
      %p.setNoTarget(); // set as unguided. Only happens when itchy trigger can't wait for lock tone.
}

//add mortars to the "grenade set" so the AI's can avoid them better...
function MortarImage::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if(!%p)
   {
      return;	
   }
   // z0dd - ZOD, 5/22/03, Spawn a mortar at the end of the projectiles lifetime.
   // Addresses long range mortar spam exploit.
   if($Host::ClassicLoadMortarChanges)
      schedule(9000, %p, "spawnMortar", %p);

   AIGrenadeThrown(%p);
}

function spawnMortar(%p)
{
   %exp = new Item() {
      dataBlock = mortarGrenadeThrown;
      sourceObject = %p.sourceObject;
      team = %p.team;
   };
   MissionCleanup.add(%exp);
   %pos = getBoxCenter(%p.getWorldBox());
   %exp.setTransform(%pos);
   %exp.detThread = schedule(100, %exp, "detonateGrenade", %exp);
}

// z0dd - ZOD, 4/10/04. Add mortar to the "grenade set" so the AI's can avoid them better...
function MortarBarrelLarge::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   AIGrenadeThrown(%p);
}

// z0dd - ZOD, 4/10/04. Add plasma to the "grenade set" so the AI's can avoid them better...
function PlasmaBarrelLarge::onFire(%data, %obj, %slot)
{
   %p = Parent::onFire(%data,%obj,%slot);
   AIGrenadeThrown(%p);
}

function SniperRifleImage::onFire(%data,%obj,%slot)
{
   if(!%obj.hasEnergyPack || %obj.getEnergyLevel() < %this.minEnergy) // z0dd - ZOD, 5/22/03. Check for energy too.
   {
      // siddown Junior, you can't use it
      serverPlay3D(SniperRifleDryFireSound, %obj.getTransform());
      return;
   }

   %pct = %obj.getEnergyLevel() / %obj.getDataBlock().maxEnergy;
   %p = new (%data.projectileType)() {
      dataBlock        = %data.projectile;
      initialDirection = %obj.getMuzzleVector(%slot);
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      damageFactor     = %pct * %pct;
      sourceSlot       = %slot;
   };
   %p.setEnergyPercentage(%pct);

   %obj.lastProjectile = %p;
   MissionCleanup.add(%p);
   serverPlay3D(SniperRifleFireSound, %obj.getTransform());
   
   // AI hook
   if(%obj.client)
      %obj.client.projectile = %p;

   %obj.setEnergyLevel(0);
   if($Host::ClassicLoadSniperChanges)
      %obj.decInventory(%data.ammo, 1);
}

function ElfGunImage::onFire(%data, %obj, %slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   //--------------------------------------------------------
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if(!%p)
   {
      return;	
   }
   //--------------------------------------------------------
   if(!%p.hasTarget())
      %obj.playAudio(0, ELFFireWetSound);
}

function TargetingLaserImage::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   //--------------------------------------------------------
   // z0dd - ZOD, 9/3/02. Anti rapid fire mortar/missile fix.
   if(!%p)
   {
      return;	
   }
   //--------------------------------------------------------
   %p.setTarget(%obj.team);
}

function ShockLanceImage::onFire(%this, %obj, %slot)
{
	// Added Spawn Invinciblity check
	if(%obj.client > 0)
	{
		%obj.setInvincibleMode(0, 0.00);
		%obj.setInvincible( false );
	}
   
   // z0dd - ZOD, 4/10/04. ilys - Added rapidfire shocklance fix
   if(%obj.cantfire !$= "")
      return;

   %obj.cantfire = 1;
   %preventTime = %this.stateTimeoutValue[4];
   %obj.reloadSchedule = schedule(%preventTime * 1000, %obj, resetFire, %obj);

   if( %obj.getEnergyLevel() < %this.minEnergy ) // z0dd - ZOD, 5/22/03. Check energy level first
   {
      %obj.playAudio(0, ShockLanceMissSound);
      return;
   }
   if( %obj.isCloaked() )
   {
      if( %obj.respawnCloakThread !$= "" )
      {
         Cancel(%obj.respawnCloakThread);
         %obj.setCloaked( false );
      }
      else
      {
         if( %obj.getEnergyLevel() > 20 )
         {   
            %obj.setCloaked( false );
            %obj.reCloak = %obj.schedule( 500, "setCloaked", true ); 
         }
      }   
   }

   %muzzlePos = %obj.getMuzzlePoint(%slot);
   %muzzleVec = %obj.getMuzzleVector(%slot);
   %endPos    = VectorAdd(%muzzlePos, VectorScale(%muzzleVec, %this.projectile.extension));
   %damageMasks = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType |
                  $TypeMasks::StationObjectType | $TypeMasks::GeneratorObjectType |
                  $TypeMasks::SensorObjectType | $TypeMasks::TurretObjectType;

   %everythingElseMask = $TypeMasks::TerrainObjectType |
                         $TypeMasks::InteriorObjectType |
                         $TypeMasks::ForceFieldObjectType |
                         $TypeMasks::StaticObjectType |
                         $TypeMasks::MoveableObjectType |
                         $TypeMasks::DamagableItemObjectType;

   // did I miss anything? players, vehicles, stations, gens, sensors, turrets
   %hit = ContainerRayCast(%muzzlePos, %endPos, %damageMasks | %everythingElseMask, %obj);
   %noDisplay = true;

   if(%hit !$= "0")
   {
      %obj.setEnergyLevel(%obj.getEnergyLevel() - %this.hitEnergy);
      %hitobj = getWord(%hit, 0);
      %hitpos = getWord(%hit, 1) @ " " @ getWord(%hit, 2) @ " " @ getWord(%hit, 3);

      if(%hitObj.getType() & %damageMasks)
      {
         // z0dd - ZOD, 5/18/03. Do not apply impulse to MPB.
         if(%hitObj.getDataBlock().classname !$= WheeledVehicleData)
            %hitobj.applyImpulse(%hitpos, VectorScale(%muzzleVec, %this.projectile.impulse));

         %obj.playAudio(0, ShockLanceHitSound);

         // This is truly lame, but we need the sourceobject property present...
         %p = new ShockLanceProjectile() {
            dataBlock        = %this.projectile;
            initialDirection = %obj.getMuzzleVector(%slot);
            initialPosition  = %obj.getMuzzlePoint(%slot);
            sourceObject     = %obj;
            sourceSlot       = %slot;
            targetId         = %hit;
         };
         MissionCleanup.add(%p);

         %damageMultiplier = 1.0;
         
         if(%hitObj.getDataBlock().getClassName() $= "PlayerData")
         {
            // Now we see if we hit from behind...
            %forwardVec = %hitobj.getForwardVector();
            %objDir2D   = getWord(%forwardVec, 0) @ " " @ getWord(%forwardVec,1) @ " " @ "0.0";
            %objPos     = %hitObj.getPosition();
            %dif        = VectorSub(%objPos, %muzzlePos);
            %dif        = getWord(%dif, 0) @ " " @ getWord(%dif, 1) @ " 0";
            %dif        = VectorNormalize(%dif);
            %dot        = VectorDot(%dif, %objDir2D);

            // 120 Deg angle test...
            // 1.05 == 60 degrees in radians
            if (%dot >= mCos(1.05))
            {
               // Rear hit
               %damageMultiplier = 3.0;
               if(!%hitObj.getOwnerClient().isAIControlled())
                  %hitObj.getOwnerClient().rearshot = 1; // z0dd - ZOD, 8/25/02. Added Lance rear shot messages
            }
            // --------------------------------------------------------------
            // z0dd - ZOD, 8/25/02. Added Lance rear shot messages
            else
            {
               if(!%hitObj.getOwnerClient().isAIControlled())
                  %hitObj.getOwnerClient().rearshot = 0;
            }
            // --------------------------------------------------------------
         }
         
         %totalDamage = %this.Projectile.DirectDamage * %damageMultiplier;
         %hitObj.getDataBlock().damageObject(%hitobj, %p.sourceObject, %hitpos, %totalDamage, $DamageType::ShockLance);

         %noDisplay = false;
      }
   } 

   if( %noDisplay )
   {
      // Miss
      %obj.setEnergyLevel(%obj.getEnergyLevel() - %this.missEnergy);
      %obj.playAudio(0, ShockLanceMissSound);

      %p = new ShockLanceProjectile() {
         dataBlock        = %this.projectile;
         initialDirection = %obj.getMuzzleVector(%slot);
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         sourceSlot       = %slot;
      };
      MissionCleanup.add(%p);
   }
   // z0dd - ZOD, 4/10/04. AI hook
   if(%obj.client != -1)
      %obj.client.projectile = %p;

   return %p;
}

$ELFZapSound = 2;
$ELFFireSound = 3;

function ELFProjectileData::zapTarget(%data, %projectile, %target, %targeter)
{
   %oldERate = %target.getRechargeRate();
   %target.teamDamageStateOnZap = $teamDamage;
   %teammates = %target.client.team == %targeter.client.team;

   if( %target.teamDamageStateOnZap || !%teammates )
      %target.setRechargeRate(%oldERate - %data.drainEnergy);
   else
      %target.setRechargeRate(%oldERate);	

   %projectile.checkELFStatus(%data, %target, %targeter);
}

function ELFProjectileData::unzapTarget(%data, %projectile, %target, %targeter)
{
   cancel(%projectile.ELFrecur);
   %target.stopAudio($ELFZapSound);
   %targeter.stopAudio($ELFFireSound);
   %target.zapSound = false;
   %targeter.zappingSound = false;
   %teammates = %target.client.team == %targeter.client.team;

   if(!%target.isDestroyed())
   {
      if(%target.hasEnergyPack == true) // z0dd - ZOD, 5/18/03. Fix for screwed up recharge when player has E-Pack
         %target.setRechargeRate(%target.getDataBlock().rechargeRate + 0.15);
      else
         %target.setRechargeRate(%target.getDataBlock().rechargeRate);
   }
}

function ELFProjectileData::targetDestroyedCancel(%data, %projectile, %target, %targeter)
{
   cancel(%projectile.ELFrecur);
   %target.stopAudio($ELFZapSound);
   %targeter.stopAudio($ELFFireSound);
   %target.zapSound = false;
   %targeter.zappingSound = false;
   %projectile.delete();
}

function ELFProjectile::checkELFStatus(%this, %data, %target, %targeter)
{
   %class = %targeter.getClassName();
   if(isObject(%target) && isObject(%targeter)) //Added %targeter for niche cases
   {
      if(%target.getDamageState() $= "Destroyed")
      {
         %data.targetDestroyedCancel(%this, %target, %targeter);
         return;
      }
      if(%class $= "Turret")
      {
         if(%targeter.getDamageState() $= "Disabled")
         {
            if(%targeter.zappingSound)
            {
               %targeter.stopAudio($ELFFireSound);
               %targeter.zappingSound = false;
            }
            %data.targetDestroyedCancel(%this, %target, %targeter);
            return;
         }
      }
      %enLevel = %target.getEnergyLevel();
      if(%enLevel < 1.0)
      {
         %dataBlock = %target.getDataBlock();
         %dataBlock.damageObject(%target, %this.sourceObject, %target.getPosition(), %data.drainHealth, %data.directDamageType);

      }
      else
      {
         %normal = "0.0 0.0 1.0";
         %target.playShieldEffect( %normal );
      }
      %this.ELFrecur = %this.schedule(32, checkELFStatus, %data, %target, %targeter);

      %targeter.playAudio($ELFFireSound, ELFGunFireSound);
      if(!%target.zapSound)
      {
         %target.playAudio($ELFZapSound, ELFHitTargetSound);
         %target.zapSound = true;
         %targeter.zappingSound = true;
      }
   }
   // -------------------------------------------------------
   // z0dd - ZOD, 5/27/02. Stop firing if there is no target,
   // fixes continuous fire bug.
   else
   {
      %data.targetDestroyedCancel(%this, %target, %targeter);
      return;
   }
}

function RadiusExplosion(%explosionSource, %position, %radius, %damage, %impulse, %sourceObject, %damageType)
{
   InitContainerRadiusSearch(%position, %radius, $TypeMasks::PlayerObjectType      |
                                                 $TypeMasks::VehicleObjectType     |
                                                 $TypeMasks::StaticShapeObjectType |
                                                 $TypeMasks::TurretObjectType      |
                                                 $TypeMasks::ItemObjectType);

   %numTargets = 0;
   while ((%targetObject = containerSearchNext()) != 0)
   {
      %dist = containerSearchCurrRadDamageDist();

      if (%dist > %radius)
         continue;

      // z0dd - ZOD, 5/18/03. Changed to stop Force Field console spam
      // if (%targetObject.isMounted())
      if (!(%targetObject.getType() & $TypeMasks::ForceFieldObjectType) && %targetObject.isMounted())
      {
         %mount = %targetObject.getObjectMount();
         %found = -1;
         for (%i = 0; %i < %mount.getDataBlock().numMountPoints; %i++)
         {
            if (%mount.getMountNodeObject(%i) == %targetObject)
            {
               %found = %i;
               break;
            }
         }
         if (%found != -1)
         {
            if (%mount.getDataBlock().isProtectedMountPoint[%found])
            {
               continue;
            }
         }
      }
      %targets[%numTargets]     = %targetObject;
      %targetDists[%numTargets] = %dist;
      %numTargets++;
   }

   for (%i = 0; %i < %numTargets; %i++)
   {
      %targetObject = %targets[%i];
      %dist = %targetDists[%i];
      if(isObject(%targetObject)) // z0dd - ZOD, 5/18/03 Console spam fix.
      {
         %coverage = calcExplosionCoverage(%position, %targetObject,
                                          ($TypeMasks::InteriorObjectType |
                                           $TypeMasks::TerrainObjectType |
                                           $TypeMasks::ForceFieldObjectType |
                                           $TypeMasks::VehicleObjectType));
         if (%coverage == 0)
            continue;

         //if ( $splashTest )
            %amount = (1.0 - ((%dist / %radius) * 0.88)) * %coverage * %damage;
         //else
            //%amount = (1.0 - (%dist / %radius)) * %coverage * %damage;

         //error( "damage: " @ %amount @ " at distance: " @ %dist @ " radius: " @ %radius @ " maxDamage: " @ %damage );
      
         %data = %targetObject.getDataBlock();
         %className = %data.className;

         if (%impulse && %data.shouldApplyImpulse(%targetObject))
         {
            %p = %targetObject.getWorldBoxCenter();
            %momVec = VectorSub(%p, %position);
            %momVec = VectorNormalize(%momVec);
         
            //------------------------------------------------------------------------------
            // z0dd - ZOD, 7/08/02. More kick when player damages self with disc or mortar.
            // Stronger DJs and mortar jumps without impacting others (mainly HoFs)
            if(%sourceObject == %targetObject)
            {
               if (%damageType == $DamageType::Disc)
               {
                  %impulse = 4475;
               }
               else if (%damageType == $DamageType::Mortar)
               {
            	%impulse = 5750;
               }
            }
            //------------------------------------------------------------------------------
         
            %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
            %doImpulse = true;
         }
         else if( %className $= FlyingVehicleData || %className $= HoverVehicleData ) // Removed WheeledVehicleData. z0dd - ZOD, 4/24/02. Do not allow impulse applied to MPB, conc MPB bug fix.
         {
            %p = %targetObject.getWorldBoxCenter();
            %momVec = VectorSub(%p, %position);
            %momVec = VectorNormalize(%momVec);
            %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
         
            if( getWord( %momVec, 2 ) < -0.5 )
               %momVec = "0 0 1";
            
            // Add obj's velocity into the momentum vector
            %velocity = %targetObject.getVelocity();
            //%momVec = VectorNormalize( vectorAdd( %momVec, %velocity) );
            %doImpulse = true;
         }
         else
         {   
            %momVec = "0 0 1";
            %doImpulse = false;
         }
      
         if(%amount > 0)
            %data.damageObject(%targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %explosionSource.theClient, %explosionSource);
         else if( %explosionSource.getDataBlock().getName() $= "ConcussionGrenadeThrown" && %data.getClassName() $= "PlayerData" )
	   {
            %data.applyConcussion( %dist, %radius, %sourceObject, %targetObject );
 	  	
 	  	if(!$teamDamage && %sourceObject != %targetObject && %sourceObject.client.team == %targetObject.client.team)
 	  	{
			messageClient(%targetObject.client, 'msgTeamConcussionGrenade', '\c1You were hit by %1\'s concussion grenade.', getTaggedString(%sourceObject.client.name));
		}
	   }
         //-------------------------------------------------------------------------------
         // z0dd - ZOD, 4/16/02. Tone done the how much bomber & HPC flip out when damaged
         if( %doImpulse )
         {
            %vehName = %targetObject.getDataBlock().getName();
            if ((%vehName $= "BomberFlyer") || (%vehName $= "HAPCFlyer"))
            {
              %bomberimp = VectorScale(%impulseVec, 0.6);
              %impulseVec = %bomberimp;
            }
            %targetObject.applyImpulse(%position, %impulseVec);
         }
         //if( %doImpulse )
         //   %targetObject.applyImpulse(%position, %impulseVec);
         //-------------------------------------------------------------------------------
      }
   }
}

function ProjectileData::onExplode(%data, %proj, %pos, %mod)
{
   if (%data.hasDamageRadius)
      RadiusExplosion(%proj, %pos, %data.damageRadius, %data.indirectDamage, %data.kickBackStrength, %proj.sourceObject, %data.radiusDamageType);
}

function Flag::shouldApplyImpulse(%data, %obj)
{
   if(%obj.isHome)
      return false;
   else
      return true;
}
