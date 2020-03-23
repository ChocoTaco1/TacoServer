//**********************************************************
// CREDITS
//
// Eolks - I think he wrote most (probably all) of the mine+disc support.
//**********************************************************

$DamageTypeText[50] = 'Mine+Disc'; // Teratos: Pseudo-damage type

// Eolk - Mine disc messages // Teratos - betterized.
$DeathMessageMineDiscCount = 4;
$DeathMessageMineDisc[0] = '\c0%4 kills %1 with a mine+disc.';
$DeathMessageMineDisc[1] = '\c0%4 unleashes a world of hurt on %1 with a mine+disc.';
$DeathMessageMineDisc[2] = '\c0%4 shows %1 the power of a spinfusor+mine combo!';
$DeathMessageMineDisc[3] = '\c0%1 never saw that mine+disc coming from %4.';

// Teratos: Guessing this was Eolk?
function resetMineDiscCheck(%cl)
{
   %cl.minediscCheck = 0;
}

package StatsMineDisc
{	
	function DefaultGame::displayDeathMessages(%game, %clVictim, %clKiller, %damageType, %implement)
	{
		// ----------------------------------------------------------------------------------
		// z0dd - ZOD, 6/18/02. From Panama Jack, send the damageTypeText as the last varible
		// in each death message so client knows what weapon it was that killed them.

		%victimGender = (%clVictim.sex $= "Male" ? 'him' : 'her');
		%victimPoss = (%clVictim.sex $= "Male" ? 'his' : 'her');
		%killerGender = (%clKiller.sex $= "Male" ? 'him' : 'her');
		%killerPoss = (%clKiller.sex $= "Male" ? 'his' : 'her');
		%victimName = %clVictim.name;
		%killerName = %clKiller.name;
		//error("DamageType = " @ %damageType @ ", implement = " @ %implement @ ", implement class = " @ %implement.getClassName() @ ", is controlled = " @ %implement.getControllingClient());

		if(%damageType == $DamageType::Explosion)
		{
			messageAll('msgExplosionKill', $DeathMessageExplosion[mFloor(getRandom() * $DeathMessageExplosionCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if(%damageType == $DamageType::Suicide)  //player presses ctrl-k
		{
			messageAll('msgSuicide', $DeathMessageSuicide[mFloor(getRandom() * $DeathMessageSuicideCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if(%damageType == $DamageType::VehicleSpawn)
		{
			messageAll('msgVehicleSpawnKill', $DeathMessageVehPad[mFloor(getRandom() * $DeathMessageVehPadCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if(%damageType == $DamageType::ForceFieldPowerup)
		{
			messageAll('msgVehicleSpawnKill', $DeathMessageFFPowerup[mFloor(getRandom() * $DeathMessageFFPowerupCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if(%damageType == $DamageType::Crash)
		{
			messageAll('msgVehicleCrash', $DeathMessageVehicleCrash[%damageType, mFloor(getRandom() * $DeathMessageVehicleCrashCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if(%damageType == $DamageType::Impact) // run down by vehicle
		{
			if( ( %controller = %implement.getControllingClient() ) > 0)
			{
				%killerGender = (%controller.sex $= "Male" ? 'him' : 'her');
				%killerPoss = (%controller.sex $= "Male" ? 'his' : 'her');
				%killerName = %controller.name;
				if(%controller.team != %clVictim.team)
				{
					messageAll('msgVehicleKill', $DeathMessageVehicle[mFloor(getRandom() * $DeathMessageVehicleCount)], %victimName, %victimGender, %victimPoss, %killerName ,%killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
				else
				{
					messageAll('msgTeamKill', $DeathMessageTeamKill[%damageType, mFloor(getRandom() * $DeathMessageTeamKillCount)],  %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
			}
			else
			{
				messageAll('msgVehicleKill', $DeathMessageVehicleUnmanned[mFloor(getRandom() * $DeathMessageVehicleUnmannedCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
			}
		}
		// ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// z0dd - ZOD, 5/15/02. Added Hover Vehicle so we get proper
		// death messages when killed with Wildcat chaingun
		//else if (isObject(%implement) && (%implement.getClassName() $= "Turret" || %implement.getClassName() $= "VehicleTurret" || %implement.getClassName() $= "FlyingVehicle"))   //player killed by a turret
		else if (isObject(%implement) && (%implement.getClassName() $= "Turret" || %implement.getClassName() $= "VehicleTurret" || %implement.getClassName() $= "FlyingVehicle" || %implement.getClassName() $= "HoverVehicle"))
		{
			if (%implement.getControllingClient() != 0)  //is turret being controlled?
			{
				%controller = %implement.getControllingClient();
				%killerGender = (%controller.sex $= "Male" ? 'him' : 'her');
				%killerPoss = (%controller.sex $= "Male" ? 'his' : 'her');
				%killerName = %controller.name;

				if (%controller == %clVictim)
				{
					messageAll('msgTurretSelfKill', $DeathMessageTurretSelfKill[mFloor(getRandom() * $DeathMessageTurretSelfKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
				else if (%controller.team == %clVictim.team) //controller TK'd a friendly
				{
					messageAll('msgCTurretKill', $DeathMessageCTurretTeamKill[%damageType, mFloor(getRandom() * $DeathMessageCTurretTeamKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
				else //controller killed an enemy
				{
					messageAll('msgCTurretKill', $DeathMessageCTurretKill[%damageType, mFloor(getRandom() * $DeathMessageCTurretKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
			}
			// use the handle associated with the deployed object to verify valid owner
			else if (isObject(%implement.owner))
			{
				%owner = %implement.owner;
				//error("Owner is " @ %owner @ "   Handle is " @ %implement.ownerHandle);
				//error("Turret is still owned");
				//turret is uncontrolled, but is owned - treat the same as controlled.
				%killerGender = (%owner.sex $= "Male" ? 'him' : 'her');
				%killerPoss = (%owner.sex $= "Male" ? 'his' : 'her');
				%killerName = %owner.name;

				if (%owner.team == %clVictim.team)  //player got in the way of a teammates deployed but uncontrolled turret.
				{
					messageAll('msgCTurretKill', $DeathMessageCTurretAccdtlKill[%damageType,mFloor(getRandom() * $DeathMessageCTurretAccdtlKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
				else  //deployed, uncontrolled turret killed an enemy
				{
					messageAll('msgCTurretKill', $DeathMessageCTurretKill[%damageType,mFloor(getRandom() * $DeathMessageCTurretKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
			}
			else  //turret is not a placed (owned) turret (or owner is no longer on it's team), and is not being controlled
			{
				if(%implement.team == %clVictim.team) // was it a teamkill?
				{
					messageAll('msgTurretKill', $DeathMessageCTurretAccdtlKill[%damageType,mFloor(getRandom() * $DeathMessageCTurretAccdtlKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
				else // it was not a teamkill
				{
					messageAll('msgTurretKill', $DeathMessageTurretKill[%damageType,mFloor(getRandom() * $DeathMessageTurretKillCount)],%victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
				}
			}
		}
		// END TURRET MESSAGES
		else if((%clKiller == %clVictim) || (%damageType == $DamageType::Ground)) //player killed himself or fell to death
		{
			messageAll('msgSelfKill', $DeathMessageSelfKill[%damageType,mFloor(getRandom() * $DeathMessageSelfKillCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if (%damageType == $DamageType::OutOfBounds) //killer died due to Out-of-Bounds damage
		{
			messageAll('msgOOBKill', $DeathMessageOOB[mFloor(getRandom() * $DeathMessageOOBCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if (%damageType == $DamageType::NexusCamping) //Victim died from camping near the nexus...
		{
			messageAll('msgCampKill', $DeathMessageCamping[mFloor(getRandom() * $DeathMessageCampingCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if(%clKiller.team == %clVictim.team) //was a TK
		{
			messageAll('msgTeamKill', $DeathMessageTeamKill[%damageType, mFloor(getRandom() * $DeathMessageTeamKillCount)],  %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if (%damageType == $DamageType::Lava)   //player died by falling in lava
		{
			messageAll('msgLavaKill',  $DeathMessageLava[mFloor(getRandom() * $DeathMessageLavaCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if ( %damageType == $DamageType::Lightning )  // player was struck by lightning
		{
			messageAll('msgLightningKill',  $DeathMessageLightning[mFloor(getRandom() * $DeathMessageLightningCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else if ( %damageType == $DamageType::Mine && !isObject(%clKiller) )
		{
			messageAll('MsgRogueMineKill', $DeathMessageRogueMine[%damageType, mFloor(getRandom() * $DeathMessageRogueMineCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
		}
		else  //was a legitimate enemy kill
		{
			if((%damageType == $DamageType::Mine || %damageType == $DamageType::Disc) && %clVictim.mineDisc)
			{
				// mine disc just occurred
				messageAll('MsgMineDiscKill', $DeathMessageMineDisc[mFloor(getRandom() * $DeathMessageMineDiscCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
			}
			else if(%damageType == 6 && (%clVictim.headShot))
			{
				// laser headshot just occurred
				messageAll('MsgHeadshotKill', $DeathMessageHeadshot[%damageType, mFloor(getRandom() * $DeathMessageHeadshotCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
			}
			// ----------------------------------------------------
			// z0dd - ZOD, 8/25/02. Rear Lance hits
			else if (%damageType == 10 && (%clVictim.rearshot))
			{
				// shocklance rearshot just occurred
				messageAll('MsgRearshotKill', $DeathMessageRearshot[%damageType, mFloor(getRandom() * $DeathMessageRearshotCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
			}
			// ----------------------------------------------------
			else
			{
				messageAll('MsgLegitKill', $DeathMessage[%damageType, mFloor(getRandom() * $DeathMessageCount)], %victimName, %victimGender, %victimPoss, %killerName, %killerGender, %killerPoss, %damageType, $DamageTypeText[%damageType]);
			}
		}
	}
};

// Prevent package from being activated if it is already
if (!isActivePackage(StatsMineDisc))
	activatePackage(StatsMineDisc);