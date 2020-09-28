// MidAir Detection Replacement
// For CTF, LCTF, DM
// Script By: DarkTiger
// v1.0
//
$MADR::Minimum = 10;

//Replacing Classic Midair Hit Detection
//
//function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
//{
//      if(!isObject(%targetObject) && !isObject(%projectile.sourceObject))
//         return;
//      if(!(%targetObject.getType() & ($TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | 
//                                      $TypeMasks::TerrainObjectType | $TypeMasks::WaterObjectType)))
//      {
//         if(%projectile.sourceObject.team !$= %targetObject.team)
//         {
//            if(%targetObject.getDataBlock().getClassName() $= "PlayerData" && %data.getName() $= "DiscProjectile")
//            {
//	         %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType; 
//	         %start = %targetObject.getWorldBoxCenter();
//               %distance = mFloor(VectorDist(%start, %projectile.initialPosition));
//	         %end = getWord(%start, 0) SPC getWord(%start, 1) SPC getWord(%start, 2) - 15;
//	         %grounded = ContainerRayCast(%start, %end, %mask, 0);
//               if(!%grounded)
//               {
//                  %projectile.sourceObject.client.scoreMidAir++;
//                  messageClient(%projectile.sourceObject.client, 'MsgMidAir', '\c0You received a %1 point bonus for a successful mid air shot.~wfx/misc/bounty_bonus.wav', Game.SCORE_PER_MIDAIR, %data.radiusDamageType, %distance);
//                  messageTeamExcept(%projectile.sourceObject.client, 'MsgMidAir', '\c5%1 hit a mid air shot.', %projectile.sourceObject.client.name, %data.radiusDamageType, %distance);
//                  Game.recalcScore(%projectile.sourceObject.client);
//               }
//            }
//         }
//         Parent::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal);
//      }
//}

package midAirMsg
{
	
function detonateGrenade(%obj) // from lakRabbitGame.cs for grenade tracking 
{
	%obj.maNade = 1;
	$maObjExplode = %obj;
	parent::detonateGrenade(%obj);
} 

function ProjectileData::onExplode(%data, %proj, %pos, %mod)
{
	%cl = %proj.sourceObject.client;
	
	if(isObject(%cl))
		%cl.expData = %data TAB %proj.initialPosition TAB %pos; 
	
	parent::onExplode(%data, %proj, %pos, %mod);
}

function DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{ 
	if(%game.class $= "CTFGame" || %game.class $= "DMGame" || %game.class $= "SCtFGame") // did it this way so dont have to copy paste 3 fucntions 
	{
		if(isObject(%clVictim.player) && isObject(%clAttacker.player))
		{
			if(%clVictim != %clAttacker && %clVictim.team != %clAttacker.team)
			{
				%dist = vectorDist(%clAttacker.player.getPosition(), %clVictim.player.getPosition());
				switch$(%damageType)
				{
					//case $DamageType::Blaster:
						//if(maRayTestDis(%clVictim.player) >= $MADR::Minimum)
							//maMessage(%clAttacker,"Blaster",%dist);
					case $DamageType::Plasma:
						if(maRayTestDis(%clVictim.player) >= $MADR::Minimum && maDirect(%clAttacker))
							maMessage(%clAttacker,"Plasma Rifle",%dist);
					case $DamageType::Disc:
						if(maRayTestDis(%clVictim.player) >= $MADR::Minimum && maDirect(%clAttacker))
							maMessage(%clAttacker,"Spinfusor",%dist);
					case $DamageType::Grenade:
						if($dtObjExplode.dtNade)
						{//for hand genades method out of lakRabbit
							//if(maRayTestDis(%clVictim.player) >= $MADR::Minimum)
								//maMessage(%clAttacker,"Hand Grenade",%dist);
						}
						else //Grenade Launcher
						{
							if(maRayTestDis(%clVictim.player) >= $MADR::Minimum && maDirect(%clAttacker))
								maMessage(%clAttacker,"Grenade Launcher",%dist);
						}
					//case $DamageType::Laser:  
						//if(maRayTestDis(%clVictim.player) >= $MADR::Minimum)
							//maMessage(%clAttacker,"Laser Rifle",%dist);
					case $DamageType::Mortar:
						if(maRayTestDis(%clVictim.player) >= $MADR::Minimum && maDirect(%clAttacker))
							maMessage(%clAttacker,"Fusion Mortar",%dist);
					//case $DamageType::ShockLance:
						//if(maRayTestDis(%clVictim.player) >= $MADR::Minimum)
							//maMessage(%clAttacker,"ShockLance",%dist);
					//case $DamageType::Mine:
						//if(maRayTestDis(%clVictim.player) >= $MADR::Minimum)
							//maMessage(%clAttacker,"Mine",%dist);
				}
			}
		}
	}

	parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
}

};

// Prevent package from being activated if it is already
if(!isActivePackage(midAirMsg))
	activatePackage(midAirMsg);

function maDirect(%client)// tests for direct hit with aoe weapons
{
	%field = %client.expData;
	%data = getField(%field,0); %sPos = getField(%field,1); %ePos = getField(%field,2);
	
	if(%data.hasDamageRadius)
	{
		%mask = $TypeMasks::PlayerObjectType;
		%vec = vectorNormalize(vectorSub(%ePos,%sPos));
		%ray = containerRayCast(%ePos, VectorAdd(%ePos, VectorScale(VectorNormalize(%vec), 5)), %mask, -1);  
		if(%ray)
			return 1;
	}
	return 0;
}

function maRayTestDis(%targetObject)// tests for height of target
{
	%mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
	%rayStart = %targetObject.getWorldBoxCenter();
	%rayEnd = VectorAdd(%rayStart,"0 0" SPC -5000);
	%ray = ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject); 
	
	if(%ray)
		return vectorDist(%rayStart,getWords(%ray,1,3)) - 1.15;
	
	return 0;
}

function maMessage(%client,%porjName,%distance)// Send message
{
	%client.scoreMidAir++;
	messageClient(%client, 'MsgMidAir', '\c0You received a %1 point bonus for a successful mid air shot. [%2m, %3]~wfx/misc/bounty_bonus.wav', Game.SCORE_PER_MIDAIR, mFloor(%distance), %porjName);
	messageTeamExcept(%client, 'MsgMidAir', '\c5%1 hit a mid air shot. [%2m, %3]', %client.name, mFloor(%distance), %porjName);
	Game.recalcScore(%client);  
}
