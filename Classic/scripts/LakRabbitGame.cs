//-------------------------------------------------------------------
// Team Rabbit script
// -------------------------------------------------------------------

// DisplayName = LakRabbit

//--- GAME RULES BEGIN ---
//Spawn with your favorites and double ammunition
//Rabbit is invincible for 2 seconds upon grab
//No laser, no chain.  Eat it Altimor.
//Sounds for regular & special hits
//Points based on difficulty, speed, and distance
//Duel Mode forces rabbit to fight and rewards for killing
//Stand and fight to get the most points.
//--- GAME RULES END ---

// Thanks for helping me test!
// maradona, pip, phantom jaguar, hilikus, the_ham, pip, wiggle, dragon, pancho villa, w/o, nectar and many others..
//
// v3.34 Febuary 2019
// Added SetNextMission support
// Indoor Spawning support
//
// v3.33 January 2019
// Took out slap headshot.
// Added footnotes for voting references with evo admin mod.
// An improved Flag-Waypoint.
//
// v3.32 December 2018
// Fixed an issue with lak vote items in the Evo Admin Votemenu
//
// v3.31 October 2018
// Adjusted the flag updater code so its more like the way it was. A little harder to catch. Down from 10 times a second to 2.
// Fixed a bug that broke weapons cycling. Players just run as if they have ammo packs now.
// Cleaned up voting language.
// Fixed a bug where the player would stay invisible when slapped.
// Added disc headshot functionality to slap.
// Fixed a bug where you would get multiple back in bounds messeges even when you were dead after a slap.
//
// v3.3 - July 2018
// Nerfed Midair Flag grab points since its easier with the flag updater code.
// Nerfed Shock height points just a tad.
//
// v3.2 - April 2018
// Made spawning closer to the Rabbit 200>150
// Made Debrief font slightly larger
// Added flag updater code from CTF
// Took out Snipe and Chain Stats. Replaced them with more relevant stats.
// Added Unlimited DiscJump Variable.
//
// v3.1 9/11 Version
// Fixed Boundary walls
// Banned Certain types of weapons
//
// -2.17xxx points bug may be fixed.
// Fixed the Resetting... spam while toggling duel mode.
// Voting looks better (makes sense).
// No mysterious 'hoorah' when you get 100% mine on ground.
// Splash damage is based on how many are playing (more people less splash damage).  Minimum 10% damage.  No Splash is still votable.
// Heavies are easier to kill.
// Point adjustments.
// Flag wont just drop to the ground in duel mode.  Basically, if you don't have enough upward momentum, it will be added to the flag so it can fly up.
// No 'duel kills' with missile special.
// Rabbit cannot disk jump in duel mode.
// DJ + Gren spam addressed again :)
// Throwing a hand nade removes invincibility.
// There is a minimum 'seconds per kill', to help with the 2 kills in 2 seconds scenario.
// Point maximum raised to 2000.
// Fixed No-Splash so you still take damage with direct hits on ground.
// Extra duel time for rabbit with No-Splash enabled and when it's 1on1 in duel mode.
// If you want to see the objective hud download LakRabbitObjHud.cs and put in /base/scripts/autoexec
//
// v3
// Code optimization and bug fixes.
// Mortars are enabled, but only do half damage.  Mortars can get long-distance or MA points.
// Long-distance shots are a little easier to get.
// You get one free disk-jump at spawn (DJ does no damage to you), but lose it if you get the flag.
// 'Specials'.  You'll know it when you see it ;)
// No damage taken from mines if you aren't in the air, and mine damage in general is greatly reduced.
// [Fix] You only get points with sniper if you have 50% energy.
// Extra points for snipe headshot, and it is displayed with sound (regular snipe w/points has no sound).
// Points show up as soon as you get them (don't have to respawn)
// Distance and speed is now taken from where you shot, not where you are when the projectile hits.
// Duel mode -- rabbit has to fight or blow up, and is awarded points for killing people quickly (votable on/off).
// Grid is 'bouncy' like TR2 (code from TR2 as well -- thanks) for players and flags, and you can no longer spawn out of bounds. <-- may help with oob flag/ghost/sticky corpse bugs
// Ground damage cut in half in duel mode, except for rabbit.
// Messed with MA flag-grab points again, and it now shows your speed and height.
// Points for hand grenades.
// Hand nades don't detonate mines in duel mode (thx mista).
// You can vote off splash damage.  Mines and grenades may still damage you with splash, however, and you can still get long distance shot points.
// You get points from chaingun every 5 hits (less spam, better looking points).  Firing a different weapon resets your accuracy and hits same as before.
// Shocklance points are based mostly on speed and height now, and your height is displayed.
// Objective hud fixed.
// Spawn a little closer to rabbit, and the rabbit waypoint is only shown if someone grabs flag and is far away from you.
// Removed old AI Rabbit stuff (less console spam).
// DJ + Gren spam abuse addressed.  Good luck getting points this way...
// New debriefing information at the end of a map: score, kills, MAs, avg speed and distance, overall chain accuracy, percent of snipe and shock hits.
// Getting an MA with shocklance and sniper is now based on if -you- are in the air, not the opponent.
// Point limit reduced to 1500.
//
// v2
// On CTF maps (or maps with two or more flags) you can capture the flag for points when 6 people are playing.  Only one flag can be in play at a time.  Once capped, your flag gets returned and you immediately take the new flag.
// Mines are now available.
// Flag gets 'tossed' when carrier dies, no longer drops like a rock.
// Better messages, showing speed and distance.  Distance bonuses are calculated into points; no multipliers.
// Spawn with full energy.
// MA flag-grab points are better calculated.
// Spawning has been redone so you spawn within a radius of 250m from either the rabbit or flag.
// Lots of new maps, all competition CTF maps, set up for LakRabbit (no generators, etc)
// Rabbit doesn't suffer the CG or Laser damage subtraction based on players.
// Point system revamped.
// 2500 point limit.
// Long-Distance disc and GL shots may give points (depending on difficulty of shot).
// Flag-skipping bug fixed.

// v1 - See game rules.

// Vars:
// $Host::ShowFlagIcon
//   0 - Don't show any
//   1 - Show flag icon when flag dropped only
//   2 - Show flag icon on rabbit
//
// $Host::LakRabbitPubPro
//   0 - Disable LakPro features
//   1 - Enable LakPro features
//
// $Host::LakRabbitDuelMode
//   0 - Disable Duel Mode
//   1 - Enable Duel Mode
//
// $Host::LakRabbitNoSplashDamage
//   0 - Disable No Splash Dame
//   1 - Enable No Splash Damage
//
// $Host::ShowFlagTask
//   0 - Do not show flag task
//   1 - Show the flag when dropped as a task
//
// $Host::EnableLakUnlimitedDJ
//   0 - Players only get one DiscJump
//   1 - Players get 999 or unlimited DiscJumps
//


package LakRabbitGame {

function Flag::objectiveInit(%data, %flag)
{
   $flagStatus = "<At Home>";
   %flag.carrier = "";
   %flag.originalPosition = %flag.getTransform();  
   %flag.isHome = true;
   %flag.rotate = true;

   // ilys -- add the icon to the flag
   if( $Host::ShowFlagIcon == 1 || $Host::ShowFlagIcon == 2 )
   {
      %flag.scopeWhenSensorVisible(true);
      setTargetSensorGroup(%flag.getTarget(), $NonRabbitTeam);
      setTargetRenderMask(%flag.getTarget(), getTargetRenderMask(%flag.getTarget()) | 0x2);
      setTargetAlwaysVisMask(%flag.getTarget(), 0x7);
   }
   
   // create a waypoint to the flag's starting place
   if( $Host::ShowFlagIcon == 0 )
   {
		%flagWaypoint = new WayPoint()
		{
			position = %flag.position;
			rotation = "1 0 0 0";
			name = "Flag Home";
			dataBlock = "WayPointMarker";
			team = $NonRabbitTeam;
		};
   }

   $AIRabbitFlag = %flag;

   MissionCleanup.add(%flagWaypoint);

   // -------------------------------------------------------------------------------------------
   // z0dd - ZOD, 10/03/02. Use triggers for flags that are at home, hack for flag collision bug.
   %flag.trigger = new Trigger()
   {
      dataBlock = flagTrigger;
      polyhedron = "-0.6 0.6 0.1 1.2 0.0 0.0 0.0 -1.2 0.0 0.0 0.0 2.5";
      position = %flag.position;
      rotation = %flag.rotation;
   };
   MissionCleanup.add(%flag.trigger);
   %flag.trigger.flag = %flag;
   // -------------------------------------------------------------------------------------------
}

// AI consoel spam
function AIThrowObject(%object)
{
	return;
}
function AIGrenadeThrown(%object)
{
	return;
}
function AICorpseAdded(%corpse)
{
	return;
}

// eat it ZP/Altimor
function ChaingunImage::onFire(%data,%obj,%slot){}
function SniperRifleImage::onFire(%data,%obj,%slot){}
function ShockLanceImage::onFire(%data, %obj, %slot)
{
	%p = parent::onFire(%data,%obj,%slot);
	%obj.client.totalShocks++;
	%obj.setInvincible(false);
	return %p;
}
function ShapeBaseImageData::onFire(%data, %obj, %slot)
{
	if(%data.projectile $= ChaingunBullet)
		$LakFired[%obj, ChaingunBullet, 0]++;
	else
	{
		$LakFired[%obj, ChaingunBullet, 0] = 0;
		$LakFired[%obj, ChaingunBullet, 1] = 0;
	}
	
	%p = parent::onFire(%data, %obj, %slot);
	%p.shotFrom = %obj.getWorldBoxCenter();
	%p.shotSpeed = getSpeed(%obj);
	
// borlak -- remove height from grenades to get rid of DJ + gren spam abuse
	switch$(%data.projectile)
	{
	case BasicGrenade:
		%p.shotFrom = setWord(%p.shotFrom, 2, getWord(%p.shotFrom,2) - getHeight(%obj));
	}		
	return %p;
}

// borlak -- this is going to be one ugly hack, but I can't find where a non radius damage
// object calls damageObject on a -hit-, therefore cannot pass on the projectiles data to 
// damageObject and get it's true origin of fire
function ProjectileData::onExplode(%data, %proj, %pos, %mod)
{
	$lastObjExplode = %proj;

	parent::onExplode(%data, %proj, %pos, %mod);
}
// this too...
function detonateGrenade(%obj)
{
	%obj.shotSpeed = -1;
	%obj.isHandNade = 1;
	$lastObjExplode = %obj;
	
	parent::detonateGrenade(%obj);
}
function GrenadeThrown::onThrow(%this, %gren)
{
	%gren.sourceObject.setInvincible(false);
	%gren.shotFrom = %gren.getWorldBoxCenter();
	parent::onThrow(%this, %gren);
}

// borlak -- prevent getting hurt while knocked back (more fun)
function Armor::onImpact(%data, %playerObject, %collidedObject, %vec, %vecLen)
{
	if(%playerObject.knockback)
		return;
	parent::onImpact(%data, %playerObject, %collidedObject, %vec, %vecLen);
}
// borlak -- ma testing and points
function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC)
{
	%distance = 0;
	%energy = 0;
	%velBonus = 0;
	%sound = "";
	%defaultSound = '~wfx/misc/bounty_bonus.wav';
	%weapon = 0;
	%points = 0;
	%ma = 0;
	%special = "";
	%long = 0;
	%accuracy = "";
	%velstring = "";
	%maxDamage = 0;
	%percentDam = 0;

	if(%targetObject.invincible || %targetObject.getState() $= "Dead")
		return;
		
	// rabbit can't DJ in duel mode
	if(Game.duelMode && %targetObject.holdingFlag && %targetObject == %sourceObject
	&& %damageType == $DamageType::Disc
	&& getWord(%targetObject.getMuzzleVector(0),2) < -0.7
	&& !TestForMA(%sourceObject, 5))
	{
		%sound = '~wfx/misc/missed.wav';
		%amount = 2.0;
		%targetObject.blowup();
	}

	// Zeph - PubPro no DJ

	if(Game.PubPro && %targetObject == %sourceObject
	&& %damageType == $DamageType::Disc
	&& getWord(%targetObject.getMuzzleVector(0),2) < -0.7
	&& !TestForMA(%sourceObject, 5))
	{
		%sound = '~wfx/misc/missed.wav';
		%amount = 2.0;
		%targetObject.blowup();
		%targetObject.scriptKill();
	}

	// hurting yourself, check for free DJ
	if( isObject(%sourceObject)
	&&  %sourceObject.getDataBlock().getClassName() $= "PlayerData"
	&&  %targetObject.client == %sourceObject.client)
	{
		if(%damageType == $DamageType::Disc && %sourceObject.freeDJ)
		{
			%amount = 0;
			%sourceObject.freeDJ--;
		}
	}

	%targetObject.lastDamageType = %damageType;

	// borlak -- MA, long distance, mine and grenade points
	if( isObject(%sourceObject) 
	&&  %sourceObject.getDataBlock().getClassName() $= "PlayerData" && %targetObject.getDataBlock().getClassName() $= "PlayerData" 
	&&  %targetObject.client.team != %sourceObject.client.team
	&&  %damageType)
	{
		$LakDamaged[%targetObject.client]	= %sourceObject.client;
		%targetObject.lastDamagedBy		= %sourceObject;
		%targetPosition				= %targetObject.getWorldBoxCenter();
		
		// borlak -- DJ + gren spam abuse, remove player height
		if(%damageType == $DamageType::Grenade && !$lastObjExplode.isHandNade)
			%targetPosition = setWord(%targetPosition, 2, getWord(%targetPosition,2) - getHeight(%targetObject));

		if(%damageType == $DamageType::ShockLance || %damageType == $DamageType::Laser)
		{
			$lastObjExplode	= 0;
			%distance	= VectorDist(%targetPosition, %sourceObject.getWorldBoxCenter());
			%vel		= getSpeed(%sourceObject);
		}
		else
		{
			%maxDamage	= $lastObjExplode.getDataBlock().directDamage ? $lastObjExplode.getDataBlock().directDamage : $lastObjExplode.getDataBlock().indirectDamage;
			%distance	= VectorDist(%targetPosition, $lastObjExplode.shotFrom);
			if($lastObjExplode.shotSpeed < 0)
				%vel	= getSpeed(%sourceObject);
			else
				%vel	= $lastObjExplode.shotSpeed;

		}
		%percentDam	= mFloor((%amount/%maxDamage)*100);
		%energy		= %sourceObject.getEnergyLevel()/%sourceObject.getDataBlock().maxEnergy;
		%velTarget	= getSpeed(%targetObject);
		%distanceBonus		= (mPow(%distance,1.35)/10)+1;
		%longDistanceBonus	= (mPow(%distance,1.10)/10)+1;
		%velBonus	= (mPow(%vel,1.125)/10)+1;

		// for shock and laser, you get MA points if -you- are in the air not the opponent
		if(%damageType == $DamageType::ShockLance || %damageType == $DamageType::Laser)
			%ma = TestForMA(%sourceObject, 6);
		else
			%ma = TestForMA(%targetObject, 6);

		// lower splash damage depending on how many people are playing.. duel mode only
		if(Game.duelMode && %percentDam < 98 && $lastObjExplode && $lastObjExplode.getDataBlock().indirectDamage > 0)
		{
			%mult = 1-(ClientGroup.getCount()-1)/10;
			if(%mult < 0.10)
				%mult = 0.10;
			%amount *= %sourceObject.holdingFlag ? 1 : %mult;
			%maxDamage *= %sourceObject.holdingFlag ? 1 : %mult;
		}

		// no splash damage vote
		if(Game.noSplashDamage && %percentDam < 98 && $lastObjExplode && !$lastObjExplode.isHandNade && %damageType != $DamageType::Mine)
			%amount = 0.0;
		
		if(%damageType == $DamageType::Laser && (%energy > 0.5 || %players > 7))
		{
			%players = (ClientGroup.getCount()-1)/1.5;
		   	%points = (%distance/75)+1;
		
			if(%ma)
				%points *= 1.75;
				
			if(%targetObject.client.headshot)
			{
				%sound = %defaultSound;
				%special = " Headshot";
				%points *= 1.75;
			}
			%sourceObject.client.totalSnipeHits++;
			%weapon = "Snipe";
		}
		else if(%damageType == $DamageType::Bullet)
		{
			// doesn't matter if it's MA
			%ma = 0;

			%players = (ClientGroup.getCount()-1)/1.75;

			$LakFired[%sourceObject, ChaingunBullet, 1]++;
			
			if($LakFired[%sourceObject, ChaingunBullet, 1] % 5 == 0)
			{
				%accamount = mFloor(($LakFired[%sourceObject, ChaingunBullet, 1] / $LakFired[%sourceObject, ChaingunBullet, 0])*100);
				%velBonus = 0;
				%points = (%accamount/3)+1;
				%accuracy = " [Accuracy:" @ %accamount @ "%]";

				%sourceObject.client.totalChainAccuracy += %accamount;
				%sourceObject.client.totalChainHits++;
			}
			%weapon = "Chaingun";
		}
		else if(%damageType == $DamageType::Disc)
		{
			if(%ma && %percentDam >= 98)
			{
				%points = %distanceBonus;
				%sound = %defaultSound;
			}
			if(%percentDam >= 25 && !%ma && %distance >= 80 && %velTarget > 20)
			{
				%points = %longDistanceBonus;
				%velBonus /= 2;
				%long = 1;
				%sound = %defaultSound;
			}

			// special knockback if you hit too close, max 15% chance (point blank).. 5% at 30meters, 1% chance for any MA

			// Slap based on a Disc headshot
			//%chance = mFloor(25 - %distance/3);
			//if(%ma && getRandom(1,50) <= %chance && %targetObject.client.headshot)

				
			//Normal Slap
			%chance = mFloor(15 - %distance/3);
            if(%chance <= 0) %chance = 1;
			
            if(%ma && getRandom(1,100) <= %chance)
			{
				if(%targetObject.holdingFlag)
				{
					Game.playerDroppedFlag(%targetObject);
					//Added so cloak is turned off when slapped.
					%targetObject.setCloaked(false);
					%targetObject.freeDJ = 1;
				}
				if(%sourceObject.holdingFlag && Game.duelMode)
				{
					duelBonus(%sourceObject.client);
					$LakDamaged[%targetObject.client] = 0;
				}
					
				// lower damage and make invincible to ground damage to make it a little more fun
				%amount = 0.01;
				%targetObject.setKnockback(true);
				%targetObject.schedule(15000, "setKnockback", false);

				%p = %targetObject.getWorldBoxCenter();
				%muzzleVec = %sourceObject.getMuzzleVector(0);
				%impulseVec = VectorScale(%muzzleVec, 25000);
				%targetObject.applyImpulse(%p, %impulseVec);
				%sound = '~wfx/misc/slapshot.wav';
				
				%slapmsg = getRandom(1,3);
				
				if(%slapmsg == 1)
				messageAll('msgSlapMessege','\c0%1 wonders what the five fingers said to the face.', %targetObject.client.name );
				if(%slapmsg == 2)
				messageAll('msgSlapMessege','\c0%1 gets slapped the heck out!', %targetObject.client.name );
				if(%slapmsg == 3)				
				messageAll('msgSlapMessege','\c0%1 is taking a short tour around the map.', %targetObject.client.name );
				
			}
			%weapon = "Disc";
		}
		else if(%damageType == $DamageType::Grenade && $lastObjExplode.isHandNade)
		{
			if(%percentDam > 20)
			{
				%accuracy = " [Accuracy:" @ %percentDam @ "%]";
				%points = (%percentDam/10)+1;
				%velBonus = 0;
				%sound = '~wfx/misc/coin.wav';

			}
			if(%percentDam >= 99)
			{
				%sound = '~wfx/misc/Cheer.wav';
				%points *= 2.0;
			}
			if(%ma)
				%points *= 1.5;

			%weapon = "Hand-Nade";
		}
		else if(%damageType == $DamageType::Grenade)
		{
			if(Game.PubPro)
			{
				if(%ma && %percentDam >= 98)
				{
					%points = %distanceBonus;
					%sound = %defaultSound;
				}
				if(%percentDam >= 25 && !%ma && %distance >= 80 && %velTarget > 20)
				{
					%points = %longDistanceBonus;
					%velBonus /= 2;
					%long = 1;
					%sound = %defaultSound;
				}
			}
			else 
			{
				if(%ma && %percentDam >= 98)
				{
					%points = %distanceBonus/1.85;
					%sound = %defaultSound;
				}
				if(%percentDam >= 25 && !%ma && %distance >= 100 && %velTarget > 30)
				{
					%points = %longDistanceBonus/1.85;
					%velBonus /= 4;
					%long = 1;
					%sound = %defaultSound;
				}
			}
			%weapon = "Grenade-Launcher";
		}   
		else if(%damageType == $DamageType::Mortar)
		{
			if(%ma && %percentDam >= 98)
			{
				%points = %distanceBonus*2.66;
				%sound = %defaultSound;
			}
			if(%percentDam >= 25 && !%ma && %distance >= 100 && %velTarget > 30)
			{
				%points = %longDistanceBonus;
				%velBonus /= 2;
				%long = 1;
				%sound = %defaultSound;
			}
			if(!Game.duelMode)
				%amount /= 2;
			%weapon = "Mortar";
		}		
		else if(%damageType == $DamageType::Mine)
		{
			%amount /= %amount > 0 ? 3 : 1;
			
			if(%ma)
			{
				%accuracy = " [Accuracy:" @ %percentDam @ "%]";
				%points = (%percentDam/7)+2;
				%velBonus = 0;
				%sound = '~wfx/misc/coin.wav';

				if(%percentDam >= 99)
				{
					%sound = '~wfx/misc/Cheer.wav';
					%points *= 2.0;
				}
			}
			if(!%ma)
				%amount = 0;
			%weapon = "MINE";			
		}
		else if(%damageType == $DamageType::ShockLance)
		{	
			%height = getHeight(%sourceObject);
			%heightBonus = (mPow(%height,1.20)/14)+1; //was 10
			%velBonus /= 2;
			%points = mFloor(%distance/2) + (%heightBonus);
			
			%accuracy = " [Height:" @ %height @"m]";
			// borlak -- check rear shocklance hit
			%muzzlePos  = %sourceObject.getMuzzlePoint(0);
		        %forwardVec = %targetObject.getForwardVector();
		        %objDir2D   = getWord(%forwardVec, 0) @ " " @ getWord(%forwardVec,1) @ " " @ "0.0";
		        %objPos     = %targetObject.getPosition();
		        %dif        = VectorSub(%objPos, %muzzlePos);
		        %dif        = getWord(%dif, 0) @ " " @ getWord(%dif, 1) @ " 0";
		        %dif        = VectorNormalize(%dif);
		        %dot        = VectorDot(%dif, %objDir2D);

		        if(%dot >= mCos(1.05))
			{
				if(%sourceObject.holdingFlag && TestForMA(%targetObject, 6))
				{
					%sourceObject.setCloaked(true);
					if($host::dontcloakflag)
						%sourceObject.holdingFlag.setCloaked(false);
				}
					
				%points *= 2;
				%special = "-in-the-back";
			}
			if(%ma)
				%points += 3;
				%sound = %defaultSound;
				%sourceObject.client.totalShockHits++;
				%weapon = "ShockLance";
		}
		else if(%damageType == $DamageType::Blaster)
		{
			if(%ma)
			{
				%points = %distanceBonus/2;
				%velBonus /= 2;
				%sound = %defaultSound;
			}
			%weapon = "Blaster";
		}
		else if(%damageType == $DamageType::Plasma)
		{
			if(%ma && %percentDam >= 98)
			{
				%points = %distanceBonus/1.3+2;
				%sound = %defaultSound;
			}
			if(%percentDam >= 25 && !%ma && %distance >= 40 && %velTarget > 30)
			{
				%points = %longDistanceBonus*1.5;
				%velBonus /= 1.75;
				%long = 1;
				%sound = %defaultSound;
			}
			%weapon = "Plasma";
		}
	}

	// borlak -- recalc score for hits
	if( isObject(%sourceObject)
	&&  %sourceObject.getDataBlock().getClassName() $= "PlayerData" && %targetObject.getDataBlock().getClassName() $= "PlayerData" 
	&& %points)
	{
		%distance = mFloor(%distance);
		%points += %velBonus;
		%points = mFloor(%points);
		%sourceObject.client.morepoints += %points;

		if(%ma)
		{
			%sourceObject.client.mas++;
			%sourceObject.client.totalSpeed += %vel;
			%sourceObject.client.totalDistance += %distance;
			%hitType = "Mid-Air ";
		}
		else if(%long)
			%hitType = "Long-Distance ";
		else
			%hitType = "";
		
		messageClient(%sourceObject.client,'msgPlrPointBonus', '\c4You receive %1 point%2! [%3%4%5] [Distance:%6m] [Speed:%7kph] %8',
			%points, %points == 1 ? "" : "s",
			%hitType,
			%weapon,
			%special !$= "" ? %special : "",
			%distance,
			%vel,
			%accuracy);
		messageAllExcept(%sourceObject.client, -1, 'msgRabbitPointBonus', '\c4%1 receives %2 point%3! [%4%5%6] [Distance:%7m] [Speed:%8kph] %9',
			%sourceObject.client.name,
			%points, %points == 1 ? "" : "s",
			%hitType,
			%weapon,
			%special !$= "" ? %special : "",
			%distance,
			%vel,
			%accuracy );
			
		if(%sourceObject.holdingFlag && %points >= 75)
		{
			missileEveryone(%sourceObject);
			%sound = '~wfx/Bonuses/horz_straipass2_heist.wav';
		}

		Game.recalcScore(%sourceObject.client);
	}

// borlak -- make a sound when you hit someone
	if(%sound $= "" && %sourceObject.client.team != %targetObject.client.team)
		messageClient(%sourceObject.client,'MsgHitSound','~wfx/misc/diagnostic_beep.wav');
	else if(%sound !$= "")
		messageAll('msgSpecialHitSound', %sound);

// borlak -- rabbit should be able to kill heavies/mediums fast(er) in duel mode
	if(%targetObject.client.armor $= "Heavy" || %targetObject.client.armor $= "Medium")
	{
		if(Game.duelMode && %targetObject != %sourceObject && %sourceObject.holdingFlag)
			%amount *= 2;
		else
			%amount *= 1.5;
	}
	parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
}

function deployMineCheck(%mineObj, %player)
{
	// explode it vgc
	schedule(2000, %mineObj, "explodeMine", %mineObj, true);
}
// thanks mista
function MineDeployed:: damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
// NO NIFTY NADE/MINE SCRIPTS 
	if (Game.duelMode && %damageType == $DamageType::Grenade && $lastObjExplode.isHandNade)
		return;

	parent:: damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
}
function MineDeployed::onThrow(%this, %mine, %thrower)
{
	%mine.shotFrom = %mine.getWorldBoxCenter();
	parent::onThrow(%this, %mine, %thrower);
}
function MineDeployed::onDestroyed(%data, %obj, %lastState)
{
	%obj.shotFrom = %obj.getWorldBoxCenter();
	%obj.shotSpeed = -1;
	$lastObjExplode = %obj;

	parent::onDestroyed(%data, %obj, %lastState);
}

function Player::maxInventory(%this, %data)
{
	//chocotaco - just runs as an ammo pack cuz messing with the inv max messes up weapons cycling
	//%max = ShapeBase::maxInventory(%this,%data) * 2;
	%max = ShapeBase::maxInventory(%this,%data);
	//if (%this.getInventory(AmmoPack))
		%max += AmmoPack.max[%data.getName()];
	return %max;
}

function Player::setKnockback(%this, %val)
{
	%this.knockback = %val;
}

//For slap headshot detection
//Took out
//
//function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
//{
//    %damLoc = firstWord(%targetObject.getDamageLocation(%position));
//    if(%damLoc $= "head")
//    {   
//        %targetObject.getOwnerClient().headShot = 1;
//        //%modifier = %data.rifleHeadMultiplier;
//		%targetObject.damage(%projectile.sourceObject, %position, %data.directDamage * %modifier, %data.directDamageType);
//    }
//    else
//    {   
//        //%modifier = 1;
//        %targetObject.getOwnerClient().headShot = 0;
//		%targetObject.damage(%projectile.sourceObject, %position, %data.directDamage * %modifier, %data.directDamageType);
//    }
//}

};

//exec the AI scripts
//exec("scripts/aiRabbit.cs");

$InvBanList[LakRabbit, "TurretOutdoorDeployable"] = 1;
$InvBanList[LakRabbit, "TurretIndoorDeployable"] = 1;
$InvBanList[LakRabbit, "ElfBarrelPack"] = 1;
$InvBanList[LakRabbit, "MortarBarrelPack"] = 1;
$InvBanList[LakRabbit, "PlasmaBarrelPack"] = 1;
$InvBanList[LakRabbit, "AABarrelPack"] = 1;
$InvBanList[LakRabbit, "MissileBarrelPack"] = 1;
$InvBanList[LakRabbit, "MissileLauncher"] = 1;
$InvBanList[LakRabbit, "MotionSensorDeployable"] = 1;
$InvBanList[LakRabbit, "PulseSensorDeployable"] = 1;
$InvBanList[LakRabbit, "ELFGun"] = 1;
$InvBanList[LakRabbit, "CameraGrenade"] = 1;
$InvBanList[LakRabbit, "FlareGrenade"] = 1;
$InvBanList[LakRabbit, "FlashGrenade"] = 1;
$InvBanList[LakRabbit, "ConcussionGrenade"] = 1;

// PubPro
$InvBanList[LakRabbit, "AmmoPack"] = 1;
$InvBanList[LakRabbit, "CloakingPack"] = 1;
$InvBanList[LakRabbit, "RepairPack"] = 1;
$InvBanList[LakRabbit, "SatchelCharge"] = 1;
$InvBanList[LakRabbit, "SensorJammerPack"] = 1;
$InvBanList[LakRabbit, "ShieldPack"] = 1;

$InvBanList[LakRabbit, "SniperRifle"] = 1;
$InvBanList[LakRabbit, "Chaingun"] = 1;

$InvBanList[LakRabbit, "GrenadeLauncher"] = $Host::LakRabbitPubPro;
$InvBanList[LakRabbit, "ShockLance"] = $Host::LakRabbitPubPro;
$InvBanList[LakRabbit, "Mortar"] = $Host::LakRabbitPubPro;
$InvBanList[LakRabbit, "Grenade"] = $Host::LakRabbitPubPro;

$InvBanList[LakRabbit, "Disc"] = 0;
$InvBanList[LakRabbit, "Plasma"] = 0;
$InvBanList[LakRabbit, "Blaster"] = 0;
$InvBanList[LakRabbit, "EnergyPack"] = 0;
$InvBanList[LakRabbit, "Mine"] = 0;
$InvBanList[LakRabbit, "TargetingLaser"] = 0;


// borlak functions
function TestForMA(%player, %distance)
{
	%mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType; 
	%rayStart = %player.getWorldBoxCenter();
	%rayEnd = getWord(%rayStart, 0) SPC getWord(%rayStart, 1) SPC getWord(%rayStart, 2) - %distance;
	%ground = ContainerRayCast(%rayStart, %rayEnd, %mask, 0);

	return !%ground;
}

function getSpeed(%obj)
{
	return mFloor(VectorLen(%obj.getVelocity())*3.6);
}

function PlayingPlayers()
{
	%players = 0;
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if(isObject(%cl.player) && %cl.player.team > 0)
			%players++;
	}
	return %players;
}	

function duelBonus(%client)
{
	%client.duelSeconds += 12;
	if(Game.noSplashDamage)
		%client.duelSeconds += 5;
	%client.duelKills++;
	messageClient(%client, 'MsgDuelBonus', '\c4You now have %1 seconds to live.', %client.duelSeconds);
}

//for(%i = 0; %i < ClientGroup.getCount(); %i++){%target = ClientGroup.getObject(%i).player; if(%target.holdingFlag) missileEveryone(%target);}

// awards/tricks/fun things
function missileEveryone(%attacker)
{
	for(%i = 0; %i < ClientGroup.getCount(); %i++)   
	{
		%target = ClientGroup.getObject(%i);

		if(!%target.player || %target.player == %attacker)
			continue;

		%p = ShapeBaseImageData::onFire(MissileLauncherImage,%attacker,0);
		MissileSet.add(%p);
		%p.setObjectTarget(%target.player);
		%attacker.cantFire = "";
	}

	// make him invincible so he doesn't get killed and ruin the effect!
	%attacker.setInvincible(true);
	%attacker.schedule(5000, "setInvincible", false);
	
	// give the rabbit some more duel seconds if it's duel mode..
	if(Game.duelMode && %attacker.holdingFlag)
		%attacker.client.duelSeconds += 15;
}
function killEveryone(%ignore, %message)
{
	if(!%message)
		messageAll('msgKillEveryone', 'Resetting...');
	else
		messageAll('msgKillEveryone', %message);

	for(%i = 0; %i < ClientGroup.getCount(); %i++)   
	{
		%target = ClientGroup.getObject(%i);
		
		if(!%target.player || %target.player == %ignore)
			continue;
		
		%target.player.blowup();
		%target.player.scriptKill();
	}
}

function checkDuelTimer(%client)
{
	%client.duelTimer = schedule(1000, 0, checkDuelTimer, %client);

	if(PlayingPlayers() < 2)
		return;

	%client.duelSeconds--;
	%client.duelSecondsCounted++;
	
	if(%client.duelSeconds <= 0)
	{
		cancel(%client.duelTimer);

		messageAll('MsgDuelTimeout', '\c2%1 failed to kill within the alloted time!~wfx/misc/whistle.wav', %client.name);
		%client.player.blowup();
		%client.player.scriptKill();
	}
	else
	{
		if(%client.duelSeconds == 10 || %client.duelSeconds == 5 || %client.duelSeconds <= 3)
		{
			%plural = (%client.duelSeconds != 1 ? 's' : "");
		        messageClient(%client, 'MsgDuelTimer', '\c4[Duel Mode] You have %1 second%2 to kill someone, or die!~wfx/misc/red_alert_short.wav', %client.duelSeconds, %plural);
		}
	}
}

function setFlagDeny(%client, %value)
{
	%client.flagDeny = %value;
}

// new "modes" and voting functions
$VoteMessage["VoteDuelMode"] = "turn";
$VoteMessage["VoteSplashDamage"] = "turn";
$VoteMessage["VotePro"] = "turn";

function LakRabbitGame::sendGameVoteMenu( %game, %client, %key )
{
	parent::sendGameVoteMenu( %game, %client, %key );
	
	%isAdmin = ( %client.isAdmin || %client.isSuperAdmin );

	if( %game.scheduleVote $= "" )
	{
		if(!%isAdmin)
		{
			if(!Game.duelMode)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteDuelMode', 'Enable Duel Mode', 'Vote to enable Duel Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteDuelMode', 'Disable Duel Mode', 'Vote to disable Duel Mode' );

			if(!Game.noSplashDamage)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSplashDamage', 'Disable Splash Damage', 'Vote to disable Splash Damage' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSplashDamage', 'Enable Splash Damage', 'Vote to enable Splash Damage' );
			// DeVast - PubPro votes
			if(!Game.PubPro)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePro', 'Enable Pro Mode', 'Vote to enable Pro Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePro', 'Disable Pro Mode', 'Vote to disable Pro Mode' );
		}
		//Added so lak vote items are properly displayed in evo adminvotemenu
		//A lot of changes were added to admin.ovl in evo
		//see footnotes below
		else if (%client.ForceVote > 0 && %client.NextMission !$= 1 ) //Added for SetNextMission
		{
			if(!Game.duelMode)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteDuelMode', 'Enable Duel Mode', 'Vote to enable Duel Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteDuelMode', 'Disable Duel Mode', 'Vote to disable Duel Mode' );

			if(!Game.noSplashDamage)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSplashDamage', 'Disable Splash Damage', 'Vote to disable Splash Damage' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSplashDamage', 'Enable Splash Damage', 'Vote to enable Splash Damage' );
			// DeVast - PubPro votes
			if(!Game.PubPro)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePro', 'Enable Pro Mode', 'Vote to enable Pro Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePro', 'Disable Pro Mode', 'Vote to disable Pro Mode' );
		} 
		else if ( %client.NextMission !$= 1 ) //Added for SetNextMission
		{
			if(!Game.duelMode)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteDuelMode', 'Enable Duel Mode', 'Enable Duel Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteDuelMode', 'Disable Duel Mode', 'Disable Duel Mode' );

			if(!Game.noSplashDamage)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSplashDamage', 'Disable Splash Damage', 'Disable Splash Damage' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VoteSplashDamage', 'Enable Splash Damage', 'Enable Splash Damage' );
			// DeVast - PubPro votes
			if(!Game.PubPro)
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePro', 'Enable Pro Mode', 'Enable Pro Mode' );
			else
				messageClient( %client, 'MsgVoteItem', "", %key, 'VotePro', 'Disable Pro Mode', 'Disable Pro Mode' );
		}
	}
}

function LakRabbitGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
{
	switch$ (%typeName)
	{
	case "VoteDuelMode":
		%game.voteDuelMode(%admin, %arg1, %arg2, %arg3, %arg4);
	case "VoteSplashDamage":
		%game.voteSplashDamage(%admin, %arg1, %arg2, %arg3, %arg4);
	case "VotePro":
		%game.VotePro(%admin, %arg1, %arg2, %arg3, %arg4);
	}

	parent::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4);
}

// Zeph - PubPro voting

function LakRabbitGame::VotePro(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
	if(%admin) 
	{
		killeveryone();

		if(%game.PubPro)
		{
			messageAll('MsgAdminForce', '\c2The Admin has disabled Pro.');

			$InvBanList[LakRabbit, "GrenadeLauncher"] = 0;
			$InvBanList[LakRabbit, "ShockLance"] = 0;
			$InvBanList[LakRabbit, "Mortar"] = 0;
			$InvBanList[LakRabbit, "Grenade"] = 0;

			%game.PubPro = false;
		}
		else
		{
			messageAll('MsgAdminForce', '\c2The Admin has enabled Pro.');

			$InvBanList[LakRabbit, "GrenadeLauncher"] = 1;
			$InvBanList[LakRabbit, "ShockLance"] = 1;
			$InvBanList[LakRabbit, "Mortar"] = 1;
			$InvBanList[LakRabbit, "Grenade"] = 1;

			%game.PubPro = true;
		}
	}
	else 
	{
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / ClientGroup.getCount()) > ($Host::VotePasspercent / 100))
		{
			killeveryone();

			if(%game.PubPro)
			{
				messageAll('MsgVotePassed', '\c2PubPro Disabled.');

				$InvBanList[LakRabbit, "GrenadeLauncher"] = 0;
				$InvBanList[LakRabbit, "ShockLance"] = 0;
				$InvBanList[LakRabbit, "Mortar"] = 0;
				$InvBanList[LakRabbit, "Grenade"] = 0;
				
				%game.PubPro = false;
			}
			else
			{
				messageAll('MsgVotePassed', '\c2PubPro Enabled.');

				$InvBanList[LakRabbit, "GrenadeLauncher"] = 1;
				$InvBanList[LakRabbit, "ShockLance"] = 1;
				$InvBanList[LakRabbit, "Mortar"] = 1;
				$InvBanList[LakRabbit, "Grenade"] = 1;
			
				%game.PubPro = true;
			}
		}
		else
			messageAll('MsgVoteFailed', '\c2Mode change did not pass: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100)); 
	}

	$Host::LakRabbitPubPro = %game.PubPro;
}

function LakRabbitGame::voteDuelMode(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
	if(%admin) 
	{
		killEveryone();
		if(%game.duelMode)
		{
			messageAll('MsgAdminForce', '\c2The Admin has disabled Duel Mode.');   
			%game.duelMode = false;
		}
		else
		{
			messageAll('MsgAdminForce', '\c2The Admin has enabled Duel Mode.');   
			%game.duelMode = true;
		}
	}
	else 
	{
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / ClientGroup.getCount()) > ($Host::VotePasspercent / 100))
		{
			killEveryone();
			if(%game.duelMode)
			{
				messageAll('MsgVotePassed', '\c2Duel Mode disabled.'); 
				%game.duelMode = false;
			}
			else
			{
				messageAll('MsgVotePassed', '\c2Duel Mode enabled.'); 
				%game.duelMode = true;
			}
		}
		else
			messageAll('MsgVoteFailed', '\c2Mode change did not pass: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100)); 
	}

	// save
	$Host::LakRabbitDuelMode = %game.duelMode;
}

function LakRabbitGame::voteSplashDamage(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
	if(%admin) 
	{
		if(%game.noSplashDamage)
		{
			messageAll('MsgAdminForce', '\c2The Admin has enabled Splash Damage.');
			%game.noSplashDamage = false;
		}
		else
		{
			messageAll('MsgAdminForce', '\c2The Admin has disabled Splash Damage.');   
			%game.noSplashDamage = true;
		}
	}
	else 
	{
		%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
		if(%totalVotes > 0 && (%game.totalVotesFor / ClientGroup.getCount()) > ($Host::VotePasspercent / 100))
		{
			if(%game.noSplashDamage)
			{
				messageAll('MsgVotePassed', '\c2Splash Damage enabled.'); 
				%game.noSplashDamage = false;
			}
			else
			{
				messageAll('MsgVotePassed', '\c2Splash Damage disabled.'); 
				%game.noSplashDamage = true;
			}
		}
		else
			messageAll('MsgVoteFailed', '\c2Mode change did not pass: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100)); 
	}
	
	// save
	$Host::LakRabbitNoSplashDamage = %game.noSplashDamage;
}

// new debriefing stuff
function LakRabbitGame::sendDebriefing( %game, %client )
{
	messageClient( %client, 'MsgDebriefAddLine', "", '<spush><lmargin:0><color:00dc00><font:Arial:15>PLAYER<lmargin%%:23>SCORE<lmargin%%:34>KILLS<lmargin%%:44>MAS<lmargin%%:52>SPEED<lmargin%%:62>DIST<lmargin%%:70>TOT DIST<lmargin%%:80>SHOCK<lmargin%%:90>SL HITS<spop>' );
	//													%cl.name, %score, %kills, %mas, %avgSpeed, %avgDistance, %alltotdistance, %shockPercent, %totshockhits
	// Scores:
	%totscore	= 0;
	%totkills	= 0;
	%totmas		= 0;
	%totspeed	= 0;
	%totdistance	= 0;
	//%totchainacc	= 0;
	//%totsnipepercent= 0;
	%totshockpercent= 0;
	%speeds		= 0;
	%dists		= 0;
	//%chains		= 0;
	//%snipes		= 0;
	%shocks		= 0;
	%alltotdistance		= 0;
	%totshockhits		= 0;
	
	%count = $TeamRank[0, count];
	for(%i = 0; %i < %count; %i++)
	{
		// Send the debrief line:
		%cl = $TeamRank[0, %i];

		if(%cl.score == 0)	%score = 0;
		else			%score = %cl.score;
		if(%cl.kills == 0)	%kills = 0;
		else			%kills = %cl.kills;
		if(%cl.mas == 0)	%mas = 0;
		else			%mas = %cl.mas;
		
		//if(%cl.totalSnipes == 0) %cl.totalSnipes = 1;
		if(%cl.totalShocks == 0) %cl.totalShocks = 1;
		
		if(%cl.totalSpeed == 0)	%avgSpeed = 0;
		else				%avgSpeed = mFloor(%cl.totalSpeed/%cl.mas);
		if(%cl.totalDistance == 0)	%avgDistance = 0;
		else				%avgDistance = mFloor(%cl.totalDistance/%cl.mas);
		//if(%cl.totalChainAccuracy == 0)	%avgChainAcc = 0;
		//else				%avgChainAcc = mFloor(%cl.totalChainAccuracy/%cl.totalChainHits);
		//if(%cl.totalSnipeHits == 0)	%snipePercent = 0;
		//else				%snipePercent = mFloor(%cl.totalSnipeHits/%cl.totalSnipes*100);
		if(%cl.totalShockHits == 0)	%shockPercent = 0;
		else				%shockPercent = mFloor(%cl.totalShockHits/%cl.totalShocks*100);
		if(%cl.totalDistance == 0) %othertotdistance = 0;
		else				%othertotdistance = mFloor(%cl.totalDistance);
		if(%cl.totalShockHits == 0) %shockhits = 0;
		else				%shockhits = mFloor(%cl.totalShockHits);
		messageClient( %client, 'MsgDebriefAddLine', "", '<lmargin:0><Font:Arial:14><clip%%:18> %1</clip><lmargin%%:23>%2<lmargin%%:34>%3<lmargin%%:44>%4<lmargin%%:52>%5<lmargin%%:62>%6<lmargin%%:70>%7<lmargin%%:80>%8%%<lmargin%%:90>%9',
			%cl.name, %score, %kills, %mas, %avgSpeed, %avgDistance, %othertotdistance, %shockPercent, %shockhits);

		if(%score)		%totscore		+= %score;
		if(%kills)		%totkills		+= %kills;
		if(%mas)		%totmas			+= %mas;
		if(%avgSpeed){		%totspeed		+= %avgSpeed;		%speeds++; }
		if(%avgDistance){	%totdistance		+= %avgDistance;	%dists++; }
		//if(%avgChainAcc){	%totchainacc		+= %avgChainAcc;	%chains++; }
		//if(%snipePercent){	%totsnipepercent	+= %snipePercent;	%snipes++; }
		if(%shockPercent){	%totshockpercent	+= %shockPercent;	%shocks++; }
		if(%othertotdistance){	%alltotdistance			+= %othertotdistance;  }
		if(%shockhits){			%totshockhits			+= %shockhits;  }
		
	}
	
	messageClient( %client, 'MsgDebriefAddLine', "", '<spush><lmargin:0><Font:Arial:15><color:00FF7F>%1<lmargin%%:23>%2<lmargin%%:34>%3<lmargin%%:44>%4<lmargin%%:52>%5<lmargin%%:62>%6<lmargin%%:70>%7<lmargin%%:80>%8%%<lmargin%%:90>%9<spop>\n',
		"   Totals:", %totscore, %totkills, %totmas, mFloor(%totspeed/%speeds), mFloor(%totdistance/%dists), %alltotdistance, mFloor(%totshockpercent/%shocks), %totshockhits);
}

// regular game functions
function LakRabbitGame::setUpTeams(%game)
{
   // Force the numTeams variable to one:
   DefaultGame::setUpTeams(%game);
   %game.numTeams = 1;
   setSensorGroupCount(3);

   //team damage should always be off for Rabbit
   $teamDamage = 0;

   //make all the sensor groups visible at all times
   if (!Game.teamMode)
   {
      setSensorGroupAlwaysVisMask($NonRabbitTeam, 0xffffffff);
      setSensorGroupAlwaysVisMask($RabbitTeam, 0xffffffff);

      // non-rabbits can listen to the rabbit: all others can only listen to self
      setSensorGroupListenMask($NonRabbitTeam, (1 << $RabbitTeam) | (1 << $NonRabbitTeam));
   }
}

function LakRabbitGame::initGameVars(%game)
{
   %game.playerBonusValue = 1;
   %game.playerBonusTime = 3 * 1000;

   %game.teamBonusValue = 3;
   %game.teamBonusTime = 5 * 1000; 
   %game.flagReturnTime = 25 * 1000;

   %game.waypointFrequency = 24000;
   %game.waypointDuration = 6000;
   
   %game.duelMode = $Host::LakRabbitDuelMode;
   %game.PubPro = $Host::LakRabbitPubPro;
   %game.noSplashDamage = $Host::LakRabbitNoSplashDamage;
}

$RabbitTeam = 2;
$NonRabbitTeam = 1;

// ----- These functions supercede those in DefaultGame.cs

function LakRabbitGame::allowsProtectedStatics(%game)
{
   return true;
}

function LakRabbitGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady', "", %game.class);
   messageClient(%client, 'MsgYourScoreIs', "", 0);
   //messageClient(%client, 'MsgYourRankIs', "", -1);
   messageClient(%client, 'MsgRabbitFlagStatus', "", $flagStatus);
   
   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 

   DefaultGame::clientMissionDropReady(%game,%client);
}

function LakRabbitGame::AIHasJoined(%game, %client)
{
   //let everyone know the player has joined the game
   //messageAllExcept(%client, -1, 'MsgClientJoinTeam', '%1 has joined the hunt.', %client.name, "", %client, $NonRabbitTeam);
}

function LakRabbitGame::clientJoinTeam( %game, %client, %team, %respawn )
{
   %game.assignClientTeam( %client );
   
   // Spawn the player:
   %game.spawnPlayer( %client, %respawn );
   %game.recalcScore( %client );
}


function LakRabbitGame::assignClientTeam(%game, %client)
{
   // all players start on team 1
   %client.team = $NonRabbitTeam;

   // set player's skin pref here
   setTargetSkin(%client.target, %client.skin);

   if(%client.kills $= "")
   	%game.resetScore(%client);

   // Let everybody know you are no longer an observer:
   messageAll( 'MsgClientJoinTeam', '\c1%1 has joined the hunt.', %client.name, "", %client, %client.team );
   updateCanListenState( %client );
}

function LakRabbitGame::playerSpawned(%game, %player)
{
   //call the default stuff first...
   DefaultGame::playerSpawned(%game, %player);

   //find the rabbit
   %clRabbit = -1;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (isObject(%cl.player) && isObject(%cl.player.holdingFlag))
      {
         %clRabbit = %cl;
         break;
      }
   }

   //now set a waypoint just for that client...
   // ilys -- show client waypoint if not showing flag icon
   if($Host::ShowFlagIcon == 0 && $Host::ShowFlagTask)
   {
      cancel(%player.client.waypointSchedule);
      if (isObject(%clRabbit) && !%player.client.isAIControlled())
         %player.client.waypointSchedule = %game.showRabbitWaypointClient(%clRabbit, %player.client);
   }

// borlak -- start with favorites
   if(!Game.PubPro)
	buyFavorites(%player.client);

	// Zeph - PubPro weapons

   else
   {
	%player.clearInventory();
	%player.setInventory(Disc,1);
	%player.setInventory(Blaster,1);
	%player.setInventory(Plasma,1);
	%player.setInventory(DiscAmmo,30);
	%player.setInventory(PlasmaAmmo,40);
	%player.setInventory(Mine,6);
	%player.setInventory(RepairKit,1);
	%player.setInventory(EnergyPack,1);
   	%player.use("Disc");
   }
   
   %player.schedule(250,"selectWeaponSlot", 0);
   %player.setEnergyLevel(%player.getDatablock().maxEnergy);
   	
	if($Host::EnableLakUnlimitedDJ == 1)
		%player.freeDJ = 999; // free diskjump
	else
		%player.freeDJ = 1; // free diskjump
	
}


// modified to spawn you near rabbit or flag
function LakRabbitGame::pickTeamSpawn(%game, %team) 
{
    //Use traditional spawnspheres for indoor maps
	if($CurrentMission $= "BoxLak")
	   return parent::pickTeamSpawn(%game, %team);

	
	//find the rabbit
	%spawnNear = -1;
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (isObject(%cl.player) && isObject(%cl.player.holdingFlag))
		{
			%spawnNear = %cl.player;
			break;
		}
	}
   
	if(!isObject(%spawnNear))
	{
		for (%i = 0; %i < Team0.getCount(); %i++)
		{
			%obj = Team0.getObject(%i);
			
			if(%obj.dataBlock $= Flag)
			{
				%spawnNear = %obj;
				if(!%obj.isHome)
					break;
			}
		}
	}

	for(%i = 0; %i < 25; %i++)
	{
		%pos = %spawnNear.getWorldBoxCenter();
		%randx = getWord(%pos,0)+getRandom(-150,150);
		%randy = getWord(%pos,1)+getRandom(-150,150);
		%mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType; 
		%rayStart = %randx SPC %randy SPC "1000";
		%rayEnd = %randx SPC %randy SPC "-1000";
		%ground = ContainerRayCast(%rayStart, %rayEnd, %mask, 0);
		if(%ground)
		{
			%loc = getWord(%ground,1) SPC getWord(%ground,2) SPC getWord(%ground,3);
			if(isOutOfBounds(%loc))
			{
				%ground = 0;
				continue;
			}
			break;
		}
	}
	if(%ground)
		return %loc;
	return "0 0 500";
}            


function LakRabbitGame::pickPlayerSpawn(%game, %client, %respawn)
{
// borlak -- reset values at spawn
	$LakDamaged[%client]		= 0;
	%client.duelSecondsCounted	= 0;
	%client.duelSeconds		= 0;
	%client.duelKills		= 0;
	%client.player.lastDamageType	= 0;
	%client.player.knockback	= 0;
	cancel(%client.duelTimer);
	
	// all spawns come from team 1
	return %game.pickTeamSpawn($NonRabbitTeam);
}

function LakRabbitGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
   %client.team = $NonRabbitTeam;
   DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
}

function LakRabbitGame::recalcScore(%game, %client)
{
   //score is grabs + kills + (totalTime / 15 seconds);
   %timeHoldingFlagMS = %client.flagTimeMS;
   if (isObject(%client.player.holdingFlag))
      %timeHoldingFlagMS += getSimTime() - %client.startTime;
   if(Game.PubPro) %client.score = %client.flagGrabs + (%client.kills*5) + %client.morepoints + mFloor(%timeHoldingFlagMS / 2000);
   else %client.score = %client.flagGrabs + %client.kills + %client.morepoints + mFloor(%timeHoldingFlagMS / 2000);
   messageClient(%client, 'MsgYourScoreIs', "", %client.score);
   %game.recalcTeamRanks(%client);
   %game.checkScoreLimit(%client);
}


function LakRabbitGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject)
{
   //if the victim is the rabbit, and the attacker is not the rabbit, set the damage time...
   if (isObject(%clAttacker) && %clAttacker != %clVictim)
   {
      if (%clVictim.team == $RabbitTeam)
         %game.rabbitDamageTime = getSimTime();
   }

   //call the default
   DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject);
}

function LakRabbitGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc)
{
// borlak - make sure victim isn't killer
	if(%clKiller != %clVictim)
	{
		%clKiller.kills++;
		%game.recalcScore(%clKiller);

		if($LakDamaged[%clVictim])
			%killer = $LakDamaged[%clVictim];
		else
			%killer = %clKiller;

		if(Game.duelMode && %killer.player.holdingFlag && %damageType != $DamageType::Missile)
			duelBonus(%killer);
	}
// borlak -- flag bug fix
	%clVictim.flagDeny = schedule(2500, 0, setFlagDeny, %clVictim, 0);
	
	DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc);
}

// z0dd - ZOD, 8/4/02: KineticPoet's flag updater code
function LakRabbitGame::updateFlagTransform(%game, %flag)
{
   %flag.setTransform(%flag.getTransform());
   %game.updateFlagThread[%flag] = %game.schedule(500, "updateFlagTransform", %flag);
}

function LakRabbitGame::playerDroppedFlag(%game, %player)
{
   //set the flag status
   %flag = %player.holdingFlag;
   %player.holdingFlag = "";
   %flag.carrier = "";
   $flagStatus = "<In the Field>";

   %player.unmountImage($FlagSlot);
   %flag.hide(false);
   // ilys -- remove flag icon from player
   if($Host::ShowFlagIcon == 1 || $Host::ShowFlagIcon == 2)
   {
      setTargetSensorGroup(%flag.getTarget(), $Observer);
      %player.scopeWhenSensorVisible(true);
      %target = %player.getTarget();
      setTargetRenderMask(%target, getTargetRenderMask(%target) & ~0x2);
      setTargetAlwaysVisMask(%target, (1 << getTargetSensorGroup(%target)));
   }

   //just always true
   //if( $Host::ShowFlagIcon == 1 )
      //%flag.scopeWhenSensorVisible(true);
   //else if($Host::ShowFlagIcon == 2)
      //%flag.scopeWhenSensorVisible(false);

   // borlak -- throw the flag, don't just drop it like dead weight
   // v3.1 -- in duel mode, make flag bounce up always, even if player isn't moving.. more midair grabs
   %flag.static = false;

   %vec = (-1.0 + getRandom() * 2.0) SPC (-1.0 + getRandom() * 2.0) SPC getRandom();
   %vec = vectorScale(%vec, 10);
   %dot = vectorDot("0 0 1",%eye);
   if (%dot < 0)
      %dot = -%dot;
   %vec = vectorAdd(%vec,vectorScale("0 0 12",1 - %dot));
   %vec = vectorAdd(%vec,%player.getVelocity());
   if(%game.duelMode && getWord(%vec,2) < 35)
	%vec = getWord(%vec, 0) @ " " @ getWord(%vec, 1) @ " 35";
   %pos = getBoxCenter(%player.getWorldBox());
   %vec = vectorScale(%vec, 75);

   %flag.setTransform(%pos);
   %flag.applyImpulse(%pos,%vec);
   %flag.setCollisionTimeout(%player);

   // ilys -- hide waypoint if not showing flag icon
   if($Host::ShowFlagIcon == 0 && $Host::ShowFlagTask)
   {
      cancel(%game.waypointSchedule);
      %game.hideRabbitWaypoint(%player.client);
   }

   //set the client status
   %player.client.flagTimeMS += getSimTime() - %player.client.startTime;
   %player.client.team = $NonRabbitTeam;
   %player.client.setSensorGroup($NonRabbitTeam);
   setTargetSensorGroup(%player.getTarget(), $NonRabbitTeam);

   messageAllExcept(%player.client, -1, 'MsgRabbitFlagDropped', '\c2%1 dropped the flag!~wfx/misc/flag_drop.wav', %player.client.name);
   // if the player left the mission area, he's already been notified
   if(!%player.outArea)
      messageClient(%player.client, 'MsgRabbitFlagDropped', '\c2You dropped the flag!~wfx/misc/flag_drop.wav');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") dropped flag");

   %flag.returnThread = %game.schedule(%game.flagReturnTime, "returnFlag", %flag);
   // z0dd - ZOD - SquirrelOfDeath, 10/02/02. Hack for flag collision bug.
   %flag.searchSchedule = Game.schedule(10, "startFlagCollisionSearch", %flag);
   %game.updateFlagTransform(%flag); // z0dd - ZOD, 8/4/02, Call to KineticPoet's flag updater

// borlak -- timer fix and re-catch fix
	%player.client.flagDeny = schedule(2500, 0, setFlagDeny, %player.client, 0);
	cancel(%player.client.duelTimer);

// borlak -- give bonus for duel mode
	if(%game.duelMode)
	{
		if(%player.lastDamageType == $DamageType::Suicide)
		{
			messageAll('MsgDuelBonus', '\c2%1 suicides and gets no points!', %player.client.name);
			return;
		}
		
		%points = 0;
		for(%i = 1; %i <= %player.client.duelKills; %i++)
			%points += (%i*4);
		%kills = (%player.client.duelKills*12)+12;
		if(%kills <= 0) %kills = 1;
		%seconds = %player.client.duelSecondsCounted;
		if(%seconds < %player.duelKills*4)
			%seconds = %player.duelKills*4;
		%points *= %kills / %seconds;
		%points = mFloor(%points);
		%plural = (%points != 1 ? 's' : "");
		%pluralP = (%player.client.duelKills != 1 ? 'people' : 'person');
		%pluralSec = (%player.client.duelSecondsCounted != 1 ? 's' : "");
		
		if(%player.client.duelKills == 0)
			messageAll('MsgDuelBonus', '\c2%1 kills nobody and gets nothing!  Better luck next time....', %player.client.name, %points, %plural, %player.client.duelKills, %player.client.duelSecondsCounted);
		else
			messageAll('MsgDuelBonus', '\c2%1 gets %2 bonus point%3 for killing %4 %5 in %6 second%7.', %player.client.name, %points, %plural, %player.client.duelKills, %pluralP, %player.client.duelSecondsCounted, %pluralSec);
		%player.client.morepoints += %points;
		%game.recalcScore(%player.client);
	}
}

function LakRabbitGame::playerTouchFlag(%game, %player, %flag)
{
	if(%player.getState() $= "Dead" || %player.client.flagDeny)
		return;

// borlak - can't pick up flag until 2 ppl are on
	if(PlayingPlayers() < 2)
	{
		messageClient(%player.client, 'msgNoFlagWarning', "\c2You can't pick up the flag until another person joins." );
		return;
	}

	// borlak -- flag capturing in CTF maps.. extra points.. rabbit mode only
	if ((%flag.carrier $= "") && %player.holdingFlag && !%game.duelMode)
	{
		if(PlayingPlayers() < 6)
		{
			messageClient(%player.client, 'msgNoFlagWarning', "\c2You can't start capping until there are 6 players." );
			return;
		}
		%points = mFloor((getSimTime() - %player.client.startTime)/2000);
		if(%points < 10)
			%points = 10; // minimum 10 points
		messageAll('MsgRabbitFlagTaken', '\c4%1 gets %2 points for a capturing the flag!~wfx/misc/flipflop_lost.wav', %player.client.name, %points);
		%player.client.flagTimeMS += getSimTime() - %player.client.startTime;
		%game.resetFlag(%player.holdingFlag);
	}

	// check if someone already has other flag.. only one flag can be in play.. also make sure you can only pick up flag that IS in play
	%flagInPlay = 0;
	for (%i = 0; %i < Team0.getCount(); %i++)
	{
		%obj = Team0.getObject(%i);
			
		if(%obj.dataBlock $= Flag)
		{
			if(!%obj.isHome)
				%flagInPlay = %obj;
				
			if(!%obj.isHome && %obj.carrier)
			{
				messageClient(%player.client, 'msgNoFlagWarning', "\c2Only one flag may be in play.");
				return;
			}
		}
	}

	if(%flagInPlay && %flag != %flagInPlay)
	{
		messageClient(%player.client, 'msgNoFlagWarning', "\c2You can only pick up the flag in play.");
		return;
	}
	
   if(%flag.carrier $= "")
   {
// borlak cancel flag search and remove free diskjump
      cancel(%flag.searchSchedule);
	  cancel(%game.updateFlagThread[%flag]); // z0dd - ZOD, 8/4/02. Cancel this flag's thread to KineticPoet's flag updater
      %player.freeDJ = 0;
      %flag.bounced = 0;

      %player.client.startTime = getSimTime();
      %player.holdingFlag = %flag;
      %flag.carrier = %player;
      %player.mountImage(FlagImage, $FlagSlot, true); //, $teamSkin[$RabbitTeam]);
      cancel(%flag.returnThread);
      %flag.hide(true);
      // ilys -- add flag icon to player
      if( $Host::ShowFlagIcon == 2 )
      {
         setTargetSensorGroup(%flag.getTarget(), $RabbitTeam);
         %player.scopeWhenSensorVisible(true);
         %target = %player.getTarget();
         setTargetRenderMask(%target, getTargetRenderMask(%target) | 0x2);
         setTargetAlwaysVisMask(%target, 0x7);
      }
      %flag.isHome = false;
      $flagStatus = %client.name;

// borlak -- points for MA flag grabs
      %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType; 
      %rayStart = %player.getWorldBoxCenter();
      %rayEnd = getWord(%rayStart, 0) SPC getWord(%rayStart, 1) SPC getWord(%rayStart, 2) - 5;
      %ground = ContainerRayCast(%rayStart, %rayEnd, %mask, 0);
      if(!%ground)
      {
         %points = mFloor((getSpeed(%player)/7.3) + (getHeight(%player)/3.3)); //was 5.3 - 2.3
         %points = %points > 5 ? %points : 5;
         messageAll('MsgRabbitFlagTaken', '\c4%1 gets %2 points for a Mid-Air flag grab! [Speed:%3] [Height:%4]~wfx/misc/hunters_horde.wav', %player.client.name, %points, getSpeed(%player), getHeight(%player));
	 %player.client.morepoints += %points;
      }
      else
         messageAll('MsgRabbitFlagTaken', '\c2%1 has taken the flag!~wfx/misc/flag_snatch.wav', %player.client.name);

      logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") took flag");
      %player.client.team = $RabbitTeam;
      %player.client.setSensorGroup($RabbitTeam);
      setTargetSensorGroup(%player.getTarget(), $RabbitTeam);

      //increase the score
      %player.client.flagGrabs++;
      %game.recalcScore(%player.client);
      %game.schedule(5000, "RabbitFlagCheck", %player);

      //show the rabbit waypoint
      %game.rabbitDamageTime = 0;
      // ilys -- waypoint if not showing flag icon
      if($Host::ShowFlagIcon == 0 && $Host::ShowFlagTask)
      {
         cancel(%game.waypointSchedule);
         %game.showRabbitWaypoint(%player.client);
      }

// borlak - make rabbit invincible for 2 seconds ..
      %player.setInvincible(true);
      %player.schedule(2000, "setInvincible", false);
      
// duel mode
      if(%game.duelMode)
      {
         %player.client.duelTimer = schedule(1000, 0, checkDuelTimer, %player.client);
         %player.client.duelSeconds = 36 - (PlayingPlayers()*4);
         if(PlayingPlayers() == 2 || %game.noSplashDamage)
         	%player.client.duelSeconds += 10;
         if(%player.client.duelSeconds < 20)
         	%player.client.duelSeconds = 20;
         %player.client.duelKills = 0;
      }
   }
}

function LakRabbitGame::rabbitFlagCheck(%game, %player)
{
   // this function calculates the score for the rabbit. It must be done periodically
   // since the rabbit's score is based on how long the flag has been in possession.
   if((%player.holdingFlag != 0) && (%player.getState() !$= "Dead"))
   {
      %game.recalcScore(%player.client);
      //reschedule this flagcheck for 5 seconds
      %game.schedule(5000, "RabbitFlagCheck", %player);
   }
}

function LakRabbitGame::returnFlag(%game, %flag)
{
   messageAll('MsgRabbitFlagReturned', '\c2The flag was returned to its starting point.~wfx/misc/flag_return.wav');
   logEcho("flag return (timeout)");
   %game.resetFlag(%flag);
}

function LakRabbitGame::resetFlag(%game, %flag)
{
   %flag.setVelocity("0 0 0");
   %flag.setTransform(%flag.originalPosition);
   %flag.isHome = true;
   %flag.carrier = "";
   $flagStatus = "<At Home>";
   %flag.hide(false);
   
   //so flag turns back green
   if($Host::ShowFlagIcon == 1 || $Host::ShowFlagIcon == 2)
   {
      setTargetSensorGroup(%flag.getTarget(), $NonRabbitTeam);
   }

   cancel(%flag.searchSchedule);
   cancel(%game.updateFlagThread[%flag]); // z0dd - ZOD, 8/4/02. Cancel this flag's thread to KineticPoet's flag updater
}

// ----- These functions are native to Rabbit

function LakRabbitGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function LakRabbitGame::scoreLimitReached(%game)
{
   logEcho("game over (scorelimit)");
   %game.gameOver();
   cycleMissions();
}

function LakRabbitGame::checkScoreLimit(%game, %client)
{
   %scoreLimit = MissionGroup.Rabbit_scoreLimit;
   // default of 1200 if scoreLimit not defined  (that's 1200 seconds worth - 20 minutes)
   if(%scoreLimit $= "")
      %scoreLimit = 2000;
   if(%client.score >= %scoreLimit) 
      %game.scoreLimitReached();
}

function LakRabbitGame::gameOver(%game)
{
   // z0dd - ZOD, 9/28/02. Hack for flag collision bug.
   for(%f = 1; %f <= %game.numTeams; %f++)
   {
      cancel($TeamFlag[%f].searchSchedule);
   }
   
   //call the default
   DefaultGame::gameOver(%game);

   //send the message
   messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

   cancel(%game.rabbitWaypointThread);
   messageAll('MsgClearObjHud', "");
   for(%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %client = ClientGroup.getObject(%i);
      %game.resetScore(%client);
      // ilys -- cancel waypoint if not showing flag icon
      if($Host::ShowFlagIcon == 0 && $Host::ShowFlagTask)
         cancel(%client.waypointSchedule);
      cancel(%client.duelTimer);
   }

   // ilys -- cancel waypoint if not showing flag icon
   if($Host::ShowFlagIcon == 0 && $Host::ShowFlagTask)
      cancel(%game.waypointSchedule);
   
// borlak -- delete variables
   deleteVariables("$LakFired*");
   deleteVariables("$LakDamaged*");
}

function LakRabbitGame::resetScore(%game, %client)
{
	%client.score		= 0;
	%client.kills		= 0;
	%client.deaths		= 0;
	%client.suicides	= 0;
	%client.flagGrabs	= 0;
	%client.flagTimeMS	= 0;
	%client.morepoints	= 0;
	
	// new debriefing stuff
	%client.mas			= 0;
	%client.totalSpeed		= 0;
	%client.totalDistance		= 0;
	%client.totalChainAccuracy	= 0;
	%client.totalChainHits		= 0;
	%client.totalSnipeHits		= 0;
	%client.totalSnipes		= 0;
	%client.totalShockHits		= 0;
	%client.totalShocks		= 0;
}

function LakRabbitGame::enterMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;
  
   %player.client.outOfBounds = false;
   messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") entered mission area");
   cancel(%player.alertThread);
}

	
// borlak -- TAKEN FROM TR2 -- thanks! :D
function plzBounceOffGrid(%obj, %bounceForce, %count)
{
	%bounds = MissionArea.area;
	%boundsWest = firstWord(%bounds);
	%boundsNorth = getWord(%bounds, 1);
	%boundsEast = %boundsWest + getWord(%bounds, 2);
	%boundsSouth = %boundsNorth + getWord(%bounds, 3);

	%shapePos = %obj.getPosition();
	%shapex = firstWord(%shapePos);
	%shapey = getWord(%shapePos, 1);
	
	if( %shapex >= %boundsWest && %shapex <= %boundsEast && %shapey >= %boundsNorth && %shapey <= %boundsSouth) {
		// we don't need to bounce at all
		return;
	}

	if( %count == 8 ) {
		// just kill this retard
		%obj.scriptKill($DamageType::OutOfBounds);
		return;
	}

	if (%bounceForce $= "")
		%bounceForce = 65;
      
	%oldVel = %obj.getVelocity();
	%obj.setVelocity("0 0 0");

	%vecx = firstWord(%oldVel);
	%vecy = getWord(%oldVel, 1);
	%vecz = getWord(%oldVel, 2);

	// four cases, not two cases you fucktard kineticpoet
	// no wonder the trives of vengrances failed
	if(%shapex <= %boundsWest) {
		%vecx = mAbs(%vecx);
	}
	else if(%shapex >= %boundsEast) {
		%vecx = -mAbs(%vecx);
	}

	if(%shapey <= %boundsNorth) {
		%vecy = mAbs(%vecy);
	}
	else if(%shapey >= %boundsSouth) {
		%vecy = -mAbs(%vecy);
	}

	%vec = %vecx SPC %vecy SPC %vecz;

	// If the object's speed was pretty slow, give it a boost
	%oldSpeed = VectorLen(%oldVel);
	if (%oldSpeed < 25)
	{
		%vec = VectorNormalize(%vec);
		%vec = VectorScale(%vec, 25);
	}
	else
		%vec = VectorScale(%vec, 1.15);

	// apply the impulse to the object
	//%obj.applyImpulse(%obj.getWorldBoxCenter(), %vec);
	%obj.setVelocity(%vec);
	
	// repeat this bounce 4 times per second.  if we're oob for 2 seconds, take action
	// don't do this with the flag because that has its own thread
	if( %obj.dataBlock !$= "Flag" ) {
		schedule(250, 0, plzBounceOffGrid, %obj, %bounceForce, %count + 1);
	}
}
function isOutOfBounds(%position)
{
	%shapePos = %position;
	%shapex = firstWord(%shapePos);
	%shapey = getWord(%shapePos, 1);
	%bounds = MissionArea.area;
	%boundsWest = firstWord(%bounds);
	%boundsNorth = getWord(%bounds, 1);
	%boundsEast = %boundsWest + getWord(%bounds, 2);
	%boundsSouth = %boundsNorth + getWord(%bounds, 3);
   
	return (%shapex < %boundsWest  || %shapex > %boundsEast ||
		%shapey < %boundsNorth || %shapey > %boundsSouth);
}
function getHeight(%this)
{
	%z = getWord(%this.getPosition(), 2);
	return mFloor((%z - getTerrainHeight(%this.getPosition())));
}

function LakRabbitGame::leaveMissionArea(%game, %playerData, %player)
{
	plzBounceOffGrid(%player, 65);
}

// z0dd - ZOD, 10/02/02. Hack for flag collision bug.
// borlak -- stolen from classic
function LakRabbitGame::startFlagCollisionSearch(%game, %flag)
{
	if(isOutOfBounds(%flag.getPosition()))
	{
		plzBounceOffGrid(%flag, 15);
		%flag.bounced++;
		if(%flag.bounced >= 25)
		{
			cancel(%flag.returnThread);
			%game.returnFlag(%flag);
		}
	}

	%flag.searchSchedule = %game.schedule(10, "startFlagCollisionSearch", %flag); // SquirrelOfDeath, 10/02/02. Moved from after the while loop
	%pos = %flag.getWorldBoxCenter();
	InitContainerRadiusSearch( %pos, 1.0, $TypeMasks::VehicleObjectType | $TypeMasks::CorpseObjectType | $TypeMasks::PlayerObjectType );
	while((%found = containerSearchNext()) != 0)
	{
		%flag.getDataBlock().onCollision(%flag, %found);
		// SquirrelOfDeath, 10/02/02. Removed break to catch all players possibly intersecting with flag
	}
}

function LakRabbitGame::dropFlag(%game, %player)
{
   //you can no longer throw the flag in Rabbit...
}

function LakRabbitGame::updateScoreHud(%game, %client, %tag)
{
   //tricky stuff here...  use two columns if we have more than 15 clients...
   %numClients = $TeamRank[0, count];
   if ( %numClients > $ScoreHudMaxVisible )
      %numColumns = 2;

   // Clear the header:
   messageClient( %client, 'SetScoreHudHeader', "", "" );

   // Send subheader:
   if (%numColumns == 2)
      messageClient(%client, 'SetScoreHudSubheader', "", '<tab:5,155,225,305,455,525>\tPLAYER\tSCORE\tTIME\tPLAYER\tSCORE\tTIME');
   else
      messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,335>\tPLAYER\tSCORE\tTIME');

   //recalc the score for whoever is holding the flag
   if (isObject($AIRabbitFlag.carrier))
      %game.recalcScore($AIRabbitFlag.carrier.client);

   %countMax = %numClients;
   if ( %countMax > ( 2 * $ScoreHudMaxVisible ) )
   {
      if ( %countMax & 1 )
         %countMax++;
      %countMax = %countMax / 2;
   }
   else if ( %countMax > $ScoreHudMaxVisible )
      %countMax = $ScoreHudMaxVisible;

   for (%index = 0; %index < %countMax; %index++)
   {
      //get the client info
      %col1Client = $TeamRank[0, %index];
      %col1ClientScore = %col1Client.score $= "" ? 0 : %col1Client.score;
      %col1Style = "";

      if (isObject(%col1Client.player.holdingFlag))
      {
         %col1ClientTimeMS = %col1Client.flagTimeMS + getSimTime() - %col1Client.startTime;
         %col1Style = "<color:00dc00>";
      }
      else
      {
         %col1ClientTimeMS = %col1Client.flagTimeMS;
         if ( %col1Client == %client )
            %col1Style = "<color:dcdcdc>";
      }

      if (%col1ClientTimeMS <= 0)
         %col1ClientTime = "";
      else
      {
         %minutes = mFloor(%col1ClientTimeMS / (60 * 1000));
         if (%minutes <= 0)
            %minutes = "0";
         %seconds = mFloor(%col1ClientTimeMS / 1000) % 60;
         if (%seconds < 10)
            %seconds = "0" @ %seconds;

         %col1ClientTime = %minutes @ ":" @ %seconds;
      }

      //see if we have two columns
      if (%numColumns == 2)
      {
         %col2Client = "";
         %col2ClientScore = "";
         %col2ClientTime = "";
         %col2Style = "";

         //get the column 2 client info
         %col2Index = %index + %countMax;
         if (%col2Index < %numClients)
         {
            %col2Client = $TeamRank[0, %col2Index];
            %col2ClientScore = %col2Client.score $= "" ? 0 : %col2Client.score;

            if (isObject(%col2Client.player.holdingFlag))
            {
               %col2ClientTimeMS = %col2Client.flagTimeMS + getSimTime() - %col2Client.startTime;
               %col2Style = "<color:00dc00>";
            }
            else
            {
               %col2ClientTimeMS = %col2Client.flagTimeMS;
               if ( %col2Client == %client )
                  %col2Style = "<color:dcdcdc>";
            }

            if (%col2ClientTimeMS <= 0)
               %col2ClientTime = "";
            else
            {
               %minutes = mFloor(%col2ClientTimeMS / (60 * 1000));
               if (%minutes <= 0)
                  %minutes = "0";
               %seconds = mFloor(%col2ClientTimeMS / 1000) % 60;
               if (%seconds < 10)
                  %seconds = "0" @ %seconds;

               %col2ClientTime = %minutes @ ":" @ %seconds;
            }
         }
      }

      //if the client is not an observer, send the message
      if (%client.team != 0)
      {
         if ( %numColumns == 2 )
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6', 
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style );
         else
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3', 
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style );
      }
      //else for observers, create an anchor around the player name so they can be observed
      else
      {
         if ( %numColumns == 2 )
         {
            //this is really crappy, but I need to save 1 tag - can only pass in up to %9, %10 doesn't work...
            if (%col2Style $= "<color:00dc00>")
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6', 
                              %col1Client.name, %col1ClientScore, %col1ClientTime,
                              %col2Client.name, %col2ClientScore, %col2ClientTime,
                              %col1Style, %col1Client, %col2Client );
            }
            else if (%col2Style $= "<color:dcdcdc>")
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:dcdcdc><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6', 
                              %col1Client.name, %col1ClientScore, %col1ClientTime,
                              %col2Client.name, %col2ClientScore, %col2ClientTime,
                              %col1Style, %col1Client, %col2Client );
            }
            else
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6', 
                              %col1Client.name, %col1ClientScore, %col1ClientTime,
                              %col2Client.name, %col2ClientScore, %col2ClientTime,
                              %col1Style, %col1Client, %col2Client );
            }
         }
         else
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3', 
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
      }
   }

   // Tack on the list of observers:
   %observerCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team == 0)
         %observerCount++;
   }

   if (%observerCount > 0)
   {
	   messageClient( %client, 'SetLineHud', "", %tag, %index, "");
      %index++;
		messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
      %index++;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         //if this is an observer
         if (%cl.team == 0)
         {
            %obsTime = getSimTime() - %cl.observerStartTime;
            %obsTimeStr = %game.formatTime(%obsTime, false);
		      messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2',
		                     %cl.name, %obsTimeStr );
            %index++;
         }
      }
   }

   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient( %client, 'ClearHud', "", %tag, %index );
}

function LakRabbitGame::showRabbitWaypointClient(%game, %clRabbit, %client)
{
   //make sure we have a rabbit
   if (!isObject(%clRabbit) || !isObject(%clRabbit.player) || !isObject(%clRabbit.player.holdingFlag))
      return;

   //no waypoints for bots
   if (%client.isAIControlled())
      return;

   //scope the client, then set the always vis mask...
   %clRabbit.player.scopeToClient(%client);
   %visMask = getSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup());
   %visMask |= (1 << %client.getSensorGroup());
   setSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup(), %visMask);

   //now issue a command to kill the target
   %client.setTargetId(%clRabbit.target);
   commandToClient(%client, 'TaskInfo', %client, -1, false, "Kill the Rabbit!");
   %client.sendTargetTo(%client, true);

   //send the "waypoint is here sound"
   messageClient(%client, 'MsgRabbitWaypoint', '~wfx/misc/target_waypoint.wav');

   //and hide the waypoint
   %client.waypointSchedule = %game.schedule(%game.waypointDuration, "hideRabbitWaypointClient", %clRabbit, %client);
}

function LakRabbitGame::hideRabbitWaypointClient(%game, %clRabbit, %client)
{
   //no waypoints for bots
   if (%client.isAIControlled())
      return;

   //unset the always vis mask...
   %visMask = getSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup());
   %visMask &= ~(1 << %client.getSensorGroup());
   setSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup(), %visMask);

   //kill the actually task...
   removeClientTargetType(%client, "AssignedTask");
}

function LakRabbitGame::showRabbitWaypoint(%game, %clRabbit)
{
   //make sure we have a rabbit
   if (!isObject(%clRabbit) || !isObject(%clRabbit.player) || !isObject(%clRabbit.player.holdingFlag))
      return;

   //only show the rabbit waypoint if the rabbit hasn't been damaged within the frequency period
   if (getSimTime() - %game.rabbitDamageTime < %game.waypointFrequency)
   {
      %game.waypointSchedule = %game.schedule(%game.waypointFrequency, "showRabbitWaypoint", %clRabbit);
      return;
   }

   //loop through all the clients and flash a waypoint at the rabbits position
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.isAIControlled() || %cl == %clRabbit || !%cl.player)
         continue;

// borlak -- don't make waypoing if client is nearby (mess up aim)
      %distance	= VectorDist(%clRabbit.player.getWorldBoxCenter(), %cl.player.getWorldBoxCenter());
      if(%distance < 100)
      	continue;

      //scope the client, then set the always vis mask...
      %clRabbit.player.scopeToClient(%cl);
      %visMask = getSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup());
      %visMask |= (1 << %cl.getSensorGroup());
      setSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup(), %visMask);

      //now issue a command to kill the target
      %cl.setTargetId(%clRabbit.target);
      commandToClient(%cl, 'TaskInfo', %cl, -1, false, "Kill the Rabbit!");
      %cl.sendTargetTo(%cl, true);

      //send the "waypoint is here sound"
      messageClient(%cl, 'MsgRabbitWaypoint', '~wfx/misc/target_waypoint.wav');
   }

   //schedule the time to hide the waypoint
   %game.waypointSchedule = %game.schedule(%game.waypointDuration, "hideRabbitWaypoint", %clRabbit);
}

function LakRabbitGame::hideRabbitWaypoint(%game, %clRabbit)
{
   //make sure we have a valid client
   if (!isObject(%clRabbit))
      return;

   //loop through all the clients and hide the waypoint
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.isAIControlled())
         continue;

      //unset the always vis mask...
      %visMask = getSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup());
      %visMask &= ~(1 << %cl.getSensorGroup());
      setSensorGroupAlwaysVisMask(%clRabbit.getSensorGroup(), %visMask);

      //kill the actually task...
      removeClientTargetType(%cl, "AssignedTask");
   }

   //make sure we have a rabbit before scheduling the next showRabbitWaypoint...
   if (isObject(%clRabbit.player) && isObject(%clRabbit.player.holdingFlag))
      %game.waypointSchedule = %game.schedule(%game.waypointFrequency, "showRabbitWaypoint", %clRabbit);
}

function LakRabbitGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   if(%game.testTurretKill(%implement))   //check for turretkill before awarded a non client points for a kill
        %game.awardScoreTurretKill(%clVictim, %implement);  
   else if (%game.testKill(%clVictim, %clKiller)) //verify victim was an enemy
   {
     %game.awardScoreKill(%clKiller);
     %game.awardScoreDeath(%clVictim);          
   }
   else
   {        
     if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
     {
       %game.awardScoreSuicide(%clVictim);
     }
     else
     {
        if (%game.testTeamKill(%clVictim, %clKiller)) //otherwise test for a teamkill
              %game.awardScoreTeamKill(%clVictim, %clKiller);
     }
   }        
}

function LakRabbitGame::applyConcussion(%game, %player)
{
   // MES -- this won't do anything, the function LakRabbitGame::dropFlag is empty
   %game.dropFlag( %player );
}

//--------------------------------Footnotes---------------------------------------
//
//
//To make vote options work in evo admin mod, demonstration only below
//
//function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote)
//{
//		parent::serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %playerVote);
//	
//	    // sonic9k 11/6/2003 - Added support for LakRabbit DuelMode option
//      //
//      case "VoteDuelMode":
//         if( %isAdmin && !%client.ForceVote )
//         {
//            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
//			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
//         }
//         else
//         {
//            if(Game.scheduleVote !$= "")
//            {
//               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
//               return;
//            }
//			%actionMsg = ($Host::LakRabbitDuelMode ? "disable Duel mode" : "enable Duel mode");
//            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
//            {
//               %cl = ClientGroup.getObject(%idx);
//               if(!%cl.isAIControlled())
//               {
//                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
//                  %clientsVoting++;
//               }
//            }
//            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
//         }
//      //
//      // sonic9k 11/6/2003 - Added support for LakRabbit SplashDamage option
//      //
//      case "VoteSplashDamage":
//         if( %isAdmin && !%client.ForceVote )
//         {
//            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
//			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
//         }
//         else
//         {
//            if(Game.scheduleVote !$= "")
//            {
//               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
//               return;
//            }
//			%actionMsg = ($Host::LakRabbitNoSplashDamage ? "enable SplashDamage" : "disable SplashDamage");
//            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
//            {
//               %cl = ClientGroup.getObject(%idx);
//               if(!%cl.isAIControlled())
//               {
//                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
//                  %clientsVoting++;
//               }
//            }
//            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
//         }
//	    //
//      // chocotaco 8/7/2018 - Added support for LakRabbit Pro option
//      //
//      case "VotePro":
//         if( %isAdmin && !%client.ForceVote )
//         {
//            adminStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4);
//			adminLog(%client, " has toggled " @ %arg1 @ " (" @ %arg2 @ ")");
//         }
//         else
//         {
//            if(Game.scheduleVote !$= "")
//            {
//               messageClient(%client, 'voteAlreadyRunning', '\c2A vote is already in progress.');
//               return;
//            }
//			%actionMsg = ($Host::LakRabbitPubPro ? "disable Pro mode" : "enable Pro mode");
//            for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
//            {
//               %cl = ClientGroup.getObject(%idx);
//               if(!%cl.isAIControlled())
//               {
//                  messageClient(%cl, 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg);
//                  %clientsVoting++;
//               }
//            }
//            playerStartNewVote(%client, %typename, %arg1, %arg2, %arg3, %arg4, %clientsVoting);
//         }
//}
