// --------------------------------------------------------
// Deathmatch mission type
// --------------------------------------------------------

// DisplayName = Deathmatch

//--- GAME RULES BEGIN ---
//There aren't many rules...
//Kill
//Don't get killed
//Points are scored for each kill you make and subtracted each time you die
//--- GAME RULES END ---

$InvBanList[DM, "TurretOutdoorDeployable"] = 1;
$InvBanList[DM, "TurretIndoorDeployable"] = 1;
$InvBanList[DM, "ElfBarrelPack"] = 1;
$InvBanList[DM, "MortarBarrelPack"] = 1;
$InvBanList[DM, "PlasmaBarrelPack"] = 1;
$InvBanList[DM, "AABarrelPack"] = 1;
$InvBanList[DM, "MissileBarrelPack"] = 1;
$InvBanList[DM, "AmmoPack"] = 1;
$InvBanList[DM, "MotionSensorDeployable"] = 1;
$InvBanList[DM, "PulseSensorDeployable"] = 1;
$InvBanList[DM, "CameraGrenade"] = 1;

function DMGame::setUpTeams(%game)
{  
   %group = nameToID("MissionGroup/Teams");
   if(%group == -1)
      return;
   
   // create a team0 if it does not exist
   %team = nameToID("MissionGroup/Teams/team0");
   if(%team == -1)
   {
      %team = new SimGroup("team0");
      %group.add(%team);
   }

   // 'team0' is not counted as a team here
   %game.numTeams = 0;
   while(%team != -1)
   {
      // create drop set and add all spawnsphere objects into it
      %dropSet = new SimSet("TeamDrops" @ %game.numTeams);
      MissionCleanup.add(%dropSet);

      %spawns = nameToID("MissionGroup/Teams/team" @ %game.numTeams @ "/SpawnSpheres");
      if(%spawns != -1)
      {
         %count = %spawns.getCount();
         for(%i = 0; %i < %count; %i++)
            %dropSet.add(%spawns.getObject(%i));
      }

      // set the 'team' field for all the objects in this team
      %team.setTeam(0);

      clearVehicleCount(%team+1);
      // get next group
      %team = nameToID("MissionGroup/Teams/team" @ %game.numTeams + 1);
      if (%team != -1)
         %game.numTeams++;
   }

   // set the number of sensor groups (including team0) that are processed
   setSensorGroupCount(%game.numTeams + 1);
   %game.numTeams = 1;

   // allow teams 1->31 to listen to each other (team 0 can only listen to self)
   for(%i = 1; %i < 32; %i++)
      setSensorGroupListenMask(%i, 0xfffffffe);
}

function DMGame::initGameVars(%game)
{
   %game.SCORE_PER_KILL = 1; 
   %game.SCORE_PER_DEATH = -1;
   %game.SCORE_PER_SUICIDE = -1;
}  

exec("scripts/aiDeathMatch.cs");

function DMGame::allowsProtectedStatics(%game)
{
   return true;
}

function DMGame::equip(%game, %player)
{
   for(%i =0; %i<$InventoryHudCount; %i++)
      %player.client.setInventoryHudItem($InventoryHudData[%i, itemDataName], 0, 1);
   %player.client.clearBackpackIcon();

   if( $Host::DMSLOnlyMode )
   {		
		%player.clearInventory();
		%player.setInventory(EnergyPack, 1);
		%player.setInventory(Shocklance, 1);
		%player.setInventory(RepairKit, 1);
		%player.setInventory(TargetingLaser, 1);
		%player.use("Shocklance");
   }
   else
   {
		buyDeployableFavorites(%player.client);
		%player.setEnergyLevel(%player.getDataBlock().maxEnergy);
		%player.selectWeaponSlot( 0 );

		// do we want to give players a disc launcher instead? GJL: Yes we do!
		%player.use("Disc");
   }
}

function DMGame::pickPlayerSpawn(%game, %client, %respawn)
{
   // all spawns come from team 1
   return %game.pickTeamSpawn(1);
}

function DMGame::clientJoinTeam( %game, %client, %team, %respawn )
{
   %game.assignClientTeam( %client );
   
   // Spawn the player:
   %game.spawnPlayer( %client, %respawn );
}

function DMGame::assignClientTeam(%game, %client)
{
   for(%i = 1; %i < 32; %i++)
      $DMTeamArray[%i] = false;

   %maxSensorGroup = 0;
   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl != %client)
      {
         $DMTeamArray[%cl.team] = true;
         if (%cl.team > %maxSensorGroup)
            %maxSensorGroup = %cl.team;
      }
   }

   //now loop through the team array, looking for an empty team
   for(%i = 1; %i < 32; %i++)
   {
      if (! $DMTeamArray[%i])
      {
         %client.team = %i;
         if (%client.team > %maxSensorGroup)
            %maxSensorGroup = %client.team;
         break;
      }
   }

   // set player's skin pref here
   setTargetSkin(%client.target, %client.skin);

   // Let everybody know you are no longer an observer:
   messageAll( 'MsgClientJoinTeam', '\c1%1 has joined the fray.', %client.name, "", %client, 1 );
   updateCanListenState( %client );

   //now set the max number of sensor groups...
   setSensorGroupCount(%maxSensorGroup + 1);
}

function DMGame::clientMissionDropReady(%game, %client)
{
   messageClient(%client, 'MsgClientReady',"", %game.class);
   messageClient(%client, 'MsgYourScoreIs', "", 0);
   messageClient(%client, 'MsgDMPlayerDies', "", 0);
   messageClient(%client, 'MsgDMKill', "", 0);
   %game.resetScore(%client);

   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 
   
   DefaultGame::clientMissionDropReady(%game, %client);
}

function DMGame::AIHasJoined(%game, %client)
{
   //let everyone know the player has joined the game
   //messageAllExcept(%client, -1, 'MsgClientJoinTeam', '%1 has joined the fray.', %client.name, "", %client, 1 );
}

function DMGame::checkScoreLimit(%game, %client)
{
   //there's no score limit in DM
}

function DMGame::createPlayer(%game, %client, %spawnLoc, %respawn)
{
   DefaultGame::createPlayer(%game, %client, %spawnLoc, %respawn);
   %client.setSensorGroup(%client.team);
}

function DMGame::resetScore(%game, %client)
{
   %client.deaths = 0;
   %client.kills = 0;
   %client.score = 0;
   %client.efficiency = 0.0;
   %client.suicides = 0;
}

function DMGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc)
{
   cancel(%clVictim.player.alertThread);
   DefaultGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLoc);
}

function DMGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   if (%game.testKill(%clVictim, %clKiller)) //verify victim was an enemy
   {
      %game.awardScoreKill(%clKiller);
      messageClient(%clKiller, 'MsgDMKill', "", %clKiller.kills);
      %game.awardScoreDeath(%clVictim);
   }       
   else if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
      %game.awardScoreSuicide(%clVictim);     

   messageClient(%clVictim, 'MsgDMPlayerDies', "", %clVictim.deaths + %clVictim.suicides);
}

function DMGame::recalcScore(%game, %client)
{
   %killValue = %client.kills * %game.SCORE_PER_KILL;
   %deathValue = %client.deaths * %game.SCORE_PER_DEATH;
   %suicideValue = %client.suicides * %game.SCORE_PER_SUICIDE;

   if (%killValue - %deathValue == 0)
      %client.efficiency = %suicideValue;
   else
      %client.efficiency = ((%killValue * %killValue) / (%killValue - %deathValue)) + %suicideValue;

   %client.score = mFloatLength(%client.efficiency, 1);
   messageClient(%client, 'MsgYourScoreIs', "", %client.score);
   %game.recalcTeamRanks(%client);
   %game.checkScoreLimit(%client);
}

function DMGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function DMGame::scoreLimitReached(%game)
{
   logEcho("game over (scorelimit)");
   %game.gameOver();
   cycleMissions();
}

function DMGame::gameOver(%game)
{
   //call the default
   DefaultGame::gameOver(%game);

   messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

   cancel(%game.timeThread);
   messageAll('MsgClearObjHud', "");
   for(%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %game.resetScore(%client);
   }
}

function DMGame::enterMissionArea(%game, %playerData, %player)
{
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

function DMGame::leaveMissionArea(%game, %playerData, %player)
{
   if(%player.getState() $= "Dead")
      return;
                                         
   plzBounceOffGrid(%player, 65);
}

function DMGame::DMAlertPlayer(%game, %count, %player)
{
   // MES - I commented below line out because it prints a blank line to chat window
   //messageClient(%player.client, 'MsgDMLeftMisAreaWarn', '~wfx/misc/red_alert.wav');
   if(%count > 1)
      %player.alertThread = %game.schedule(1000, "DMAlertPlayer", %count - 1, %player);
   else 
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
}

function DMGame::MissionAreaDamage(%game, %player)
{
   if(%player.getState() !$= "Dead") {                                   
      %player.setDamageFlash(0.1);
      %prevHurt = %player.getDamageLevel();
      %player.setDamageLevel(%prevHurt + 0.05);
      %player.alertThread = %game.schedule(1000, "MissionAreaDamage", %player);
   }
   else
      %game.onClientKilled(%player.client, 0, $DamageType::OutOfBounds);
}

function DMGame::updateScoreHud(%game, %client, %tag)
{
   // Clear the header:
   messageClient( %client, 'SetScoreHudHeader', "", "" );

   // Send the subheader:
   messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,340,415>\tPLAYER\tRATING\tKILLS\tDEATHS');

   for (%index = 0; %index < $TeamRank[0, count]; %index++)
   {
      //get the client info
      %cl = $TeamRank[0, %index];

      //get the score
      %clScore = mFloatLength( %cl.efficiency, 1 );

      %clKills = mFloatLength( %cl.kills, 0 );
      %clDeaths = mFloatLength( %cl.deaths + %cl.suicides, 0 );
      %clStyle = %cl == %client ? "<color:dcdcdc>" : "";

      //if the client is not an observer, send the message
      if (%client.team != 0)
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:370><just:right>%3<rmargin:460><just:right>%4', 
               %cl.name, %clScore, %clKills, %clDeaths, %clStyle );
      }
      //else for observers, create an anchor around the player name so they can be observed
      else
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:200><a:gamelink\t%6>%1</a></clip><rmargin:280><just:right>%2<rmargin:370><just:right>%3<rmargin:460><just:right>%4', 
               %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %cl );
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

//function DMGame::applyConcussion(%game, %player)
//{
//}

package DMGame
{

function deployMineCheck(%mineObj, %player)
{
	// explode it vgc
	schedule(2000, %mineObj, "explodeMine", %mineObj, true);
}

function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
      if(!isObject(%targetObject) && !isObject(%projectile.sourceObject))
         return;
      if(!(%targetObject.getType() & ($TypeMasks::StaticTSObjectType | $TypeMasks::InteriorObjectType | 
                                      $TypeMasks::TerrainObjectType | $TypeMasks::WaterObjectType)))
      {
         if(%projectile.sourceObject.team !$= %targetObject.team)
         {
            if(%targetObject.getDataBlock().getClassName() $= "PlayerData" && %data.getName() $= "DiscProjectile")
            {
	         %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType; 
	         %start = %targetObject.getWorldBoxCenter();
               %distance = mFloor(VectorDist(%start, %projectile.initialPosition));
	         %end = getWord(%start, 0) SPC getWord(%start, 1) SPC getWord(%start, 2) - 15;
	         %grounded = ContainerRayCast(%start, %end, %mask, 0);
               if(!%grounded)
               {
                  //%projectile.sourceObject.client.scoreMidAir++;
                  messageClient(%projectile.sourceObject.client, 'MsgMidAir', '\c0You hit a successful mid air shot.~wfx/misc/bounty_bonus.wav', %data.radiusDamageType, %distance);
                  messageTeamExcept(%projectile.sourceObject.client, 'MsgMidAir', '\c5%1 hit a mid air shot.', %projectile.sourceObject.client.name, %data.radiusDamageType, %distance);
                  //Game.recalcScore(%projectile.sourceObject.client);
               }
            }
         }
         Parent::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal);
      }
}

};

function DMGame::sendGameVoteMenu(%game, %client, %key)
{
   	parent::sendGameVoteMenu( %game, %client, %key );
	
	%isAdmin = ( %client.isAdmin || %client.isSuperAdmin );
   
   if ( %game.scheduleVote $= "" )
   {
     if(!%client.isAdmin)
	 {
		if(!$Host::DMSLOnlyMode)
		messageClient( %client, 'MsgVoteItem', "", %key, 'DMSLOnlyMode', 'vote to enable Shocklance Only Mode', 'Vote to enable Shocklance Only Mode' );
		else
		messageClient( %client, 'MsgVoteItem', "", %key, 'DMSLOnlyMode', 'vote to disable Shocklance Only Mode', 'Vote to disable Shocklance Only Mode' );
	 }
	 else if (%client.ForceVote > 0)
	 {
		if(!$Host::DMSLOnlyMode)
		messageClient( %client, 'MsgVoteItem', "", %key, 'DMSLOnlyMode', 'vote to enable Shocklance Only Mode', 'Vote to enable Shocklance Only Mode' );
		else
		messageClient( %client, 'MsgVoteItem', "", %key, 'DMSLOnlyMode', 'vote to disable Shocklance Only Mode', 'Vote to disable Shocklance Only Mode' );
	 }
     else
	 {
		if(!$Host::DMSLOnlyMode)
		messageClient( %client, 'MsgVoteItem', "", %key, 'DMSLOnlyMode', 'change to enable Shocklance Only Mode', 'Enable Shocklance Only Mode' );
		else
		messageClient( %client, 'MsgVoteItem', "", %key, 'DMSLOnlyMode', 'change to disable Shocklance Only Mode', 'Disable Shocklance Only Mode' );
	 }
   }
}

function DMGame::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4)
{ 
   switch$ (%typeName)
   {
	  case "DMSLOnlyMode":
         %game.DMSLOnlyMode(%admin, %arg1, %arg2, %arg3, %arg4);
   }
   
   	parent::evalVote(%game, %typeName, %admin, %arg1, %arg2, %arg3, %arg4);
}

//--------------------------------DMSLOnlyMode--------------------------------
//
$VoteMessage["DMSLOnlyMode"] = "turn";
		 
function DMGame::DMSLOnlyMode(%game, %admin, %arg1, %arg2, %arg3, %arg4)
{
	if(	$countdownStarted && $MatchStarted )
	{
		if(%admin) 
		{
			killeveryone();

			if( $Host::DMSLOnlyMode )
			{
				messageAll('MsgAdminForce', '\c2The Admin has disabled Shocklance Only Mode.');
				
				$Host::DMSLOnlyMode = false;
			}
			else
			{
				messageAll('MsgAdminForce', '\c2The Admin has enabled Shocklance Only Mode.');
				
				$Host::DMSLOnlyMode = true;
			}
		}
		else 
		{
			%totalVotes = %game.totalVotesFor + %game.totalVotesAgainst;
			if(%totalVotes > 0 && (%game.totalVotesFor / ClientGroup.getCount()) > ($Host::VotePasspercent / 100))
			{
				killeveryone();

				if( $Host::DMSLOnlyMode )
				{
					messageAll('MsgVotePassed', '\c2Shocklance Only Mode Disabled.');
					
					$Host::DMSLOnlyMode = false;
				}
				else
				{
					messageAll('MsgVotePassed', '\c2Shocklance Only Mode Enabled.');
					
					$Host::DMSLOnlyMode = true;
				}
			}
			else
				messageAll('MsgVoteFailed', '\c2Mode change did not pass: %1 percent.', mFloor(%game.totalVotesFor/ClientGroup.getCount() * 100)); 
		}
	}
}
// For voting to work properly - evo admin.ovl
//
//	  case "DMSLOnlyMode":
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
//			%actionMsg = ($Host::DMSLOnlyMode ? "disable Shocklance Only Mode" : "enable Shocklance Only Mode");
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
