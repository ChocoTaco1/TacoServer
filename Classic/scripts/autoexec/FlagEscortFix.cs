package FlagEscortFix
{

function CTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){ 
   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
      %clAttacker.dmgdFlagTime = getSimTime();  
}
function CTFGame::testEscortAssist(%game, %victimID, %killerID){
   if((getSimTime() - %victimID.dmgdFlagTime) < %game.TIME_CONSIDERED_FLAGCARRIER_THREAT)
      return true;
   return false;
}

function SCtFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){ 
   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
      %clAttacker.dmgdFlagTime = getSimTime();  
}
function SCtFGame::testEscortAssist(%game, %victimID, %killerID){
   if((getSimTime() - %victimID.dmgdFlagTime) < %game.TIME_CONSIDERED_FLAGCARRIER_THREAT)
      return true;
   return false;
}

function PracticeCTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){ 
   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
      %clAttacker.dmgdFlagTime = getSimTime();  
}
function PracticeCTFGame::testEscortAssist(%game, %victimID, %killerID){
   if((getSimTime() - %victimID.dmgdFlagTime) < %game.TIME_CONSIDERED_FLAGCARRIER_THREAT)
      return true;
   return false;
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(FlagEscortFix))
    activatePackage(FlagEscortFix);