//Fixes for collision tunneling on flag objects note only in CTF type games
//Script By:DarkTiger
$ftEnable = 1;
$flagSimTime = 60;//note a higher the time, the larger the sweep scans will be
$flagCheckRadius = 50;
$playerBoxA = "-0.6 -0.6 0";
$playerBoxB = "0.6 0.6 2.3";
$flagStuckTime = 1000; 
package flagFix{
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
};
activatePackage(flagFix);

function DefaultGame::flagColTest(%game, %flag){
//flag ceiling check
   if( !%flag.isHome ){
      %flag.stuckChkTimer += $flagSimTime;
      if(%flag.stuckChkTimer  > $flagStuckTime){ // rate limit are checks
         if(vectorLen(%flag.getVelocity()) < 2){ // only check if we are not at speed
            %fpos = %flag.getPosition();
            //0.1 offset any fp errors with the flag position being at ground level, 2.4 offset flag height offset + some extra 
            %upRay = containerRayCast(vectorAdd(%fpos,"0 0 0.1"), vectorAdd(%fpos,"0 0 2.4"), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %flag);
            if(%upRay){
               %dist = vectorDist(%fpos,getWords(%upRay,1,3));
               //error(%dist);
               %flag.setPosition(vectorSub(%fpos,"0 0" SPC (2.5 - %dist)));
            }
         }
         %flag.stuckChkTimer  = 0;
      }
   }
////////////////////////////////////////////////////////////////////////////////
//flag collision check
   %Box2 = %flag.getWorldBox();
   InitContainerRadiusSearch( %flag.getWorldBoxCenter(), $flagCheckRadius, $TypeMasks::PlayerObjectType);
   while((%player = containerSearchNext()) != 0){
      %playerPos = %player.getPosition();
      if(%player.lastSim > 0 && (%player.getState() !$= "Dead")){// only check at speed
         if((getSimTime() - %player.lastSim) <= 256){//make sure are last position is valid
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
