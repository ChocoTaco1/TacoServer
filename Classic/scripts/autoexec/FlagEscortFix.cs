package FlagEscortFix
{

function CTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{ 
   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   //if victim is carrying a flag and is not on the attackers team, mark the attacker as a threat for x seconds(for scoring purposes)
   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
   {
      %clAttacker.dmgdFlagCarrier = true;
      cancel(%clAttacker.threatTimer);  //restart timer    
      %clAttacker.threatTimer = schedule(%game.TIME_CONSIDERED_FLAGCARRIER_THREAT,0,"dmgFlagReset",%clAttacker);   
   }
}

function SCtFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{ 
   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   //if victim is carrying a flag and is not on the attackers team, mark the attacker as a threat for x seconds(for scoring purposes)
   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
   {
      %clAttacker.dmgdFlagCarrier = true;
      cancel(%clAttacker.threatTimer);  //restart timer    
      %clAttacker.threatTimer = schedule(%game.TIME_CONSIDERED_FLAGCARRIER_THREAT,0,"dmgFlagReset",%clAttacker);   
   }
}

function PracticeCTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{ 
   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   //if victim is carrying a flag and is not on the attackers team, mark the attacker as a threat for x seconds(for scoring purposes)
   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
   {
      %clAttacker.dmgdFlagCarrier = true;
      cancel(%clAttacker.threatTimer);  //restart timer
      %clAttacker.threatTimer = schedule(%game.TIME_CONSIDERED_FLAGCARRIER_THREAT,0,"dmgFlagReset",%clAttacker);   
   }
}

};

function dmgFlagReset(%clAttacker){
   %clAttacker.dmgdFlagCarrier = false;
}

// Prevent package from being activated if it is already
if (!isActivePackage(FlagEscortFix))
    activatePackage(FlagEscortFix);