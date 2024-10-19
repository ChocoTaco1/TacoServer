//Fixes for collision tunneling among other flag issues, note only works in classic ctf game types
//Script By:DarkTiger
//V2.1
$ftEnable = 1;//disables all
$limitFlagCalls = 1; // prevents frame perfect events witch can cause bad outcomes
$antiCeiling = 1; // note this is auto enabled with $boxStuckFix as it needs to check for this
$antiTerTunnel = 0;//prevents terrain tunneling keeps the flag above the terrain leave this off unless you think its a problem
$antiFlagImpluse = 1000;//time out period to prevent explosions from effecting flags on drop/toss
$boxStuckFix = 1;

//best to leave these values alone unless you understand what the code is doing
$flagSimTime = 60;//note a higher the time, the larger the sweep scans will be
$flagCheckRadius = 50;
$playerBoxA = "-0.6 -0.6 0";
$playerBoxB = "0.6 0.6 2.3";
$flagStuckTime = 1000;

$flagSimRate = 30;
$tickDelay = 1; //delay the sim by 1 ticks in case its not needed


function flagSim(%flag,%player){
   %bypassBoxCheck = 0;// debug sim
   //%mass = 80;// not needed
   %bounceFriction  = 0.6;
   %bounceElasticity = 0.2;
   %drag = 0.5;
   %simRate = $flagSimRate;
   %timeFrame = mFloor(1000 / %simRate);
   %dt = %timeFrame / 1000;
   %Box2 = %flag.getWorldBox();
   %Box1 = %player.getWorldBox();
   if(!%flag.isHome && !isObject(%flag.carrier)){
      if((boxIntersect(%Box1, %Box2) || %bypassBoxCheck) && vectorLen(%flag.finalVel) > 1 && %flag.countsim < 32){
         %flag.setVelocity("0 0 0");// keep it zeroed out
         %xform = %flag.getTransform();
         %oldPos = getWords(%xform,0,2);// we dont use lastSimPos here as it will be fighting the engine too much  instead we use it in the raycast
         %flag.finalVel = vectorAdd(%flag.finalVel, "0 0" SPC (getGravity() * 1) * %dt);// engine adds its gravity so... prob not needed
         %flag.finalVel = vectorSub(%flag.finalVel, vectorScale(%flag.finalVel, %drag * %dt));
         %newPos = vectorAdd(%oldPos, vectorScale(%flag.finalVel,%dt));

         %mask = $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::TerrainObjectType;//$TypeMasks::ForceFieldObjectType
         %movRayB = containerRayCast(%flag.lastSimPos, %newPos, %mask, %flag);
         %movRayT = containerRayCast(vectorAdd(%flag.lastSimPos,"0 0 2.4"), vectorAdd(%newPos,"0 0 2.4"), %mask, %flag);

         if(%movRayB){
            %hitRay = %movRayB;
         }
         if(%movRayT){
            %hitRay = getWord(%movRayT,0) SPC %oldPos SPC getWords(%movRayT,4,6);
         }

         if(%hitRay){
            %normal = getWords(%hitRay, 4, 6);
            // Reflect the velocity around the surface normal
            %reflectedVel = vectorSub(%flag.finalVel, vectorScale(%normal, vectorDot(%flag.finalVel, %normal) * 2.0));
            // Calculate the tangent component for surface friction
            %tangent = vectorSub(%reflectedVel, vectorScale(%normal, vectorDot(%reflectedVel, %normal)));
            // Apply surface friction
            %bounceVel = vectorSub(%reflectedVel, vectorScale(%tangent, %bounceFriction));
            // Apply elasticity to modulate the speed of the bounce
            %flag.finalVel = vectorScale(%bounceVel, %bounceElasticity);
            // Adjust the new position to the point of impact plus a small offset along the normal
            %newPos = vectorAdd(getWords(%hitRay, 1, 3), vectorScale(%normal, 0.05));
         }
         %flag.lastSimPos = %newPos;// we do this so theres no gaps in are sim other wise it could end up underground
         %flag.setTransform(%newPos SPC getWords(%xform,3,6));
         //%flag.setVelocity(%flag.finalVel);
         %flag.countsim++;
         schedule(%timeFrame,0,"flagSim",%flag,%player);
      }
      else{
         //error("simCount" SPC %flag.countsim SPC vectorLen(%flag.finalVel));
         if(vectorLen(%flag.finalVel) && %flag.countsim > 0){
            %flag.setVelocity(%flag.finalVel);
         }
      }
   }
}

function delayStartFlag(%obj,%this){
   %obj.countsim = 0;
   %obj.lastSimPos = %obj.getPosition();
   %drag = 0.5;
   %dt = mFloor(1000 / $flagSimRate) / 1000;
   for(%i = 0; %i < $tickDelay; %i++){
      %obj.finalVel = vectorSub(%obj.finalVel, vectorScale(%obj.finalVel, %drag * %dt));
   }
   flagSim(%obj,%this);
}

package flagFix{
   function ShapeBase::throwObject(%this,%obj){
      parent::throwObject(%this,%obj);
      %data = %obj.getDatablock();
      if(%data.getName() $= "Flag"){
         %fpos = %obj.getPosition();

         if($boxStuckFix){
            %obj.finalVel = %obj.getVelocity();
            %upRay = containerRayCast(vectorAdd(%fpos,"0 0 0.1"), vectorAdd(%fpos,"0 0 2.5"), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %obj);
            if(%upRay){
               %dist = vectorDist(%fpos,getWords(%upRay,1,3));
               %obj.setTransform(vectorSub(%fpos,"0 0" SPC (2.6 - %dist)) SPC getWords(%obj.getTransform(), 3, 6));
               error("Ceiling" SPC %dist);
            }
            %wallRay = containerRayCast(%fpos, VectorAdd(%fpos, VectorScale(%this.getForwardVector(), 2)), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %obj);
            if(!%wallRay){
               schedule($tickDelay * mFloor(1000 / $flagSimRate),0,"delayStartFlag",%obj,%this);
            }
         }
         else if($antiCeiling){
            //0.1 offset any fp errors with the flag position being at ground level, 2.4 offset flag height offset + some extra
            %upRay = containerRayCast(vectorAdd(%fpos,"0 0 0.1"), vectorAdd(%fpos,"0 0 2.5"), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %obj);
            if(%upRay){
               %dist = vectorDist(%fpos,getWords(%upRay,1,3));
               %obj.setTransform(vectorSub(%fpos,"0 0" SPC (2.6 - %dist)) SPC getWords(%obj.getTransform(), 3, 6));
               //error("bump2");
            }
         }
      }
   }

   function CTFGame::startFlagCollisionSearch(%game, %flag){
      parent::startFlagCollisionSearch(%game, %flag);
      if((getSimTime() - %game.fcs) >= $flagSimTime && $ftEnable){
         %game.flagColTest(%flag);
         %game.fcs = getSimTime();
      }
   }
   function SCtFGame::startFlagCollisionSearch(%game, %flag){
      parent::startFlagCollisionSearch(%game, %flag);
      if((getSimTime() - %game.fcs) >= $flagSimTime  && $ftEnable){
         %game.flagColTest(%flag);
         %game.fcs = getSimTime();
      }
   }

   function PracticeCTFGame::startFlagCollisionSearch(%game, %flag){
      parent::startFlagCollisionSearch(%game, %flag);
      if((getSimTime() - %game.fcs) >= $flagSimTime && $ftEnable){
         %game.flagColTest(%flag);
         %game.fcs = getSimTime();
      }
   }

   function CTFGame::startMatch(%game){
      parent::startMatch(%game);
      if(!isEventPending(Game.flagLoop)){
         %game.atHomeFlagLoop();
      }
      %game.fcs = getSimTime();
   }

   function SCtFGame::startMatch(%game){
      parent::startMatch(%game);
      if(!isEventPending(Game.flagLoop)){
         %game.atHomeFlagLoop();
      }
      %game.fcs = getSimTime();
   }

   function PracticeCTFGame::startMatch(%game){
      parent::startMatch(%game);
       if(!isEventPending(Game.flagLoop)){
         %game.atHomeFlagLoop();
      }
      %game.fcs = getSimTime();
   }

   //prevents frame perfect flag touches witch can cause bad stuff to happen in ctf
   function CTFGame::playerTouchFlag(%game, %player, %flag){
      if(%flag.lastFlagCallms > 0 && $limitFlagCalls){
         %timeDif = getSimTime() - %flag.lastFlagCallms;
         if(%timeDif < 32){
            return;
         }
      }
      %flag.lastFlagCallms = getSimTime();
      parent::playerTouchFlag(%game, %player, %flag);
   }

   function SCtFGame::playerTouchFlag(%game, %player, %flag){
      if(%flag.lastFlagCallms > 0 && $limitFlagCalls){
         %timeDif = getSimTime() - %flag.lastFlagCallms;
         if(%timeDif < 32){
            return;
         }
      }
      %flag.lastFlagCallms = getSimTime();
      parent::playerTouchFlag(%game, %player, %flag);
   }

   function PracticeCTFGame::playerTouchFlag(%game, %player, %flag){
       if(%flag.lastFlagCallms > 0 && $limitFlagCalls){
         %timeDif = getSimTime() - %flag.lastFlagCallms;
         if(%timeDif < 32){
            return;
         }
      }
      %flag.lastFlagCallms = getSimTime();
      parent::playerTouchFlag(%game, %player, %flag);
   }

   function CTFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %flag.lastDropTime = getSimTime();
      parent::playerDroppedFlag(%game, %player);
   }

   function SCtFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %flag.lastDropTime = getSimTime();
      parent::playerDroppedFlag(%game, %player);
   }

   function PracticeCTFGame::playerDroppedFlag(%game, %player){
      %flag = %player.holdingFlag;
      %flag.lastDropTime = getSimTime();
      parent::playerDroppedFlag(%game, %player);
   }

   function Flag::shouldApplyImpulse(%data, %obj){
      %val = parent::shouldApplyImpulse(%data, %obj);
      //error("meow");
      if($antiFlagImpluse > 0 && %val && %obj.lastDropTime > 0){
         %time = getSimTime() - %obj.lastDropTime;
         if(%time < $antiFlagImpluse){
             %val = 0;
         }
      }
      return %val;
   }
};
activatePackage(flagFix);
function DefaultGame::flagColTest(%game, %flag){
//flag ceiling check
   %curPos = %flag.getWorldBoxCenter();
   if( !%flag.isHome ){
      if($antiTerTunnel){
         // anti flag tunneling on terrain
         if(%flag.lastUpdate > 0 && (getSimTime() - %flag.lastUpdate) < 128){//make sure are last position is current
            %terRay = containerRayCast(%flag.dtLastPos, %curPos, $TypeMasks::TerrainObjectType, %flag);
            if(%terRay){
               %curPos = vectorAdd(getWords(%terRay,1,3), "0 0 0.5");
               %flag.setTransform(%curPos SPC getWords(%flag.getTransform(), 3, 6));
            }
         }
         %flag.lastUpdate = getSimTime();
         %flag.dtLastPos = %curPos;
      }
   }
////////////////////////////////////////////////////////////////////////////////
//flag collision check
   %Box2 = %flag.getWorldBox();
   InitContainerRadiusSearch( %curPos, $flagCheckRadius, $TypeMasks::PlayerObjectType);
   while((%player = containerSearchNext()) != 0){
      %playerPos = %player.getPosition();
      if(%player.lastSim > 0 && (%player.getState() !$= "Dead")){// only check at speed
         if((getSimTime() - %player.lastSim) <= 128){//make sure are last position is valid
            %sweepCount = mFloor(vectorDist(%playerPos, %player.oldPos) + 1);
            for(%i = 0; %i < %sweepCount; %i++){// sweep behind us to see if  we should have hit something
               %lerpPos = vectorLerp(%playerPos, %player.oldPos, (%i+1)/%sweepCount);//back sweep
               %Box1 = vectorAdd($playerBoxA, %lerpPos) SPC vectorAdd($playerBoxB, %lerpPos);
               if(boxIntersect(%Box1, %Box2)){
                  %flag.getDataBlock().onCollision(%flag, %player);
                  break;
               }
            }
         }
      }
      %player.oldPos = %playerPos;
      %player.lastSim = getSimTime();
   }
}

function vectorLerp(%point1, %point2, %t) {
	return vectorAdd(%point1, vectorScale(vectorSub(%point2, %point1), %t));
}

function boxIntersect(%a, %b){
  return (getWord(%a, 0) <= getWord(%b, 3) && getWord(%a, 3) >= getWord(%b, 0)) &&
         (getWord(%a, 1) <= getWord(%b, 4) && getWord(%a, 4) >= getWord(%b, 1)) &&
         (getWord(%a, 2) <= getWord(%b, 5) && getWord(%a, 5) >= getWord(%b, 2));
}

function DefaultGame::atHomeFlagLoop(%game){
   if($TeamFlag[1].isHome ){
         %game.flagColTest($TeamFlag[1], 0);
   }
   if($TeamFlag[2].isHome){
         %game.flagColTest($TeamFlag[2], 0);
   }
   %speed = ($HostGamePlayerCount - $HostGameBotCount > 0) ? $flagSimTime  :  30000;
   if(isObject(Game) && $missionRunning && $ftEnable)
      %game.flagLoop = %game.schedule(%speed, "atHomeFlagLoop");
}