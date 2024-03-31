//Fixes for collision tunneling among other flag issues, note only works in classic ctf game types
//Script By:DarkTiger
$ftEnable = 1;//disables all 
$limitFlagCalls = 1; // prevents frame perfect events witch can cause bad outcomes
$antiCeiling = 1; // keep flags from getting stuck in the ceiling
$antiTerTunnel = 0;//prevents terrain tunneling keeps the flag above the terrain leave this off unless you think its a problem 
$antiFlagImpluse = 1000;//time out period to prevent explosions from effecting flags on drop/toss


//best to leave these values alone unless you understand what the code is doing 
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
      if($antiCeiling){
         %flag.stuckChkTimer += $flagSimTime;
         if(%flag.stuckChkTimer  > $flagStuckTime){ // rate limit are checks
            if(vectorLen(%flag.getVelocity()) < 2){ // only check if we are not at speed
               %fpos = %flag.getPosition();
               //0.1 offset any fp errors with the flag position being at ground level, 2.4 offset flag height offset + some extra 
               %upRay = containerRayCast(vectorAdd(%fpos,"0 0 0.1"), vectorAdd(%fpos,"0 0 2.4"), $TypeMasks::InteriorObjectType | $TypeMasks::StaticTSObjectType | $TypeMasks::ForceFieldObjectType, %flag);
               if(%upRay){
                  %dist = vectorDist(%fpos,getWords(%upRay,1,3));
                  %flag.setTransform(vectorSub(%fpos,"0 0" SPC (2.5 - %dist)) SPC getWords(%flag.getTransform(), 3, 6));
               }
            }
            %flag.stuckChkTimer  = 0;
         }
      }
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




//original flag impluse fix 
//dont use this function, it likes to ue im leaving this here just as a reminder
//also note flag gains velocity from things when not in play aka at stand or on a player backs
//if this is ever used that built up velocity needs to be zeroed out when it not in play 
//function Item::applyImpulse(%this, %position, %impulseVec){
   //%data = %this.getDatablock();
   //%x = getWord(%impulseVec, 0) / %data.mass;
   //%y = getWord(%impulseVec, 1) / %data.mass;
   //%z = getWord(%impulseVec, 2) / %data.mass;
   //%vel = %x SPC %y SPC %z;
   //%this.setVelocity(vectorAdd(%this.getVelocity(), %vel));
//}
