//exec("scripts/autoexec/NoFlagZone.cs");
$TurleCampTime = 10000; //10secs

function CTFGame::onEnterTrigger(%game, %triggerName, %data, %obj, %colobj)
{
   %client = %colobj.client;
   switch$(%obj.type)
	{
      case NOFLAGZONE:
         if(%client.player.holdingFlag !$= "" && $Host::NoBaseRapePlayerCount > $TotalTeamPlayerCount && %obj.team == %client.team)
         {
            //%colobj.throwObject(%colobj.holdingFlag);
            CTFGame::zoneTossFlag(%game, %colobj, %obj);
         }

      case TURTLEDAMAGE:
         if(%client.player.holdingFlag !$= "")
         {
            //schedule a warning in 10 seconds
            %client = %colobj.client;
            %client.turtledamage = 1;
            %client.campingThread = %game.schedule($TurleCampTime, "CampingDamage", %client, true);
         } 
   }
}

function CTFGame::zoneTossFlag(%game, %player, %obj)
{
   // ------------------------------------------------------------------------------
   // z0dd - ZOD - SquirrelOfDeath, 9/27/02. Delay on grabbing flag after tossing it
   %player.flagTossWait = true;
   %player.schedule(1500, resetFlagTossWait);
   // ------------------------------------------------------------------------------

   %client = %player.client;
   %flag = %player.holdingFlag;
   %flag.setVelocity("0 0 0");
   %flag.setTransform(%player.getWorldBoxCenter());
   %flag.setCollisionTimeout(%player);

   %held = %game.formatTime(getSimTime() - %game.flagHeldTime[%flag], false); // z0dd - ZOD, 8/15/02. How long did player hold flag?
   
   if($Host::ClassicEvoStats)
      %game.totalFlagHeldTime[%flag] = 0;

   %game.playerDroppedFlag(%player);

   //Need home to be away from the trigger box location
   %vec = vectorNormalize(vectorSub(%player.getWorldBoxCenter(),%obj.getWorldBoxCenter()));

   // normalize the vector, scale it, and add an extra "upwards" component
   %vecNorm = VectorNormalize(%vec);
   %vec = VectorScale(%vecNorm, 1000);
   %vec = vectorAdd(%vec, "0 0 300");

   // z0dd - ZOD, 6/09/02. Remove anti-hover so flag can be thrown properly
   %flag.static = false;

   // z0dd - ZOD, 10/02/02. Hack for flag collision bug.
   %flag.searchSchedule = %game.schedule(10, "startFlagCollisionSearch", %flag);

   // apply the impulse to the flag object
   %flag.applyImpulse(%obj.getWorldBoxCenter(), %vec);

   // z0dd - ZOD 3/30/02. Above message was sending the wrong varible to objective hud.
   messageClient(%player.client, 'MsgCTFFlagDropped', '\c1You are not allowed to have the flag in this area. (Held: %4)~wfx/misc/flag_drop.wav', %client.name, 0, %flag.team, %held); // Yogi, 8/18/02. 3rd param changed 0 -> %client.name
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") lost flag (No flag zone)"@" (Held: "@%held@")");
}

function CTFGame::onLeaveTrigger(%game, %triggerName, %data, %obj, %colobj)
{
   %client.turtledamage = 0;
   %client = %colobj.client;
   cancel(%client.campingThread);
}

function CTFGame::CampingDamage(%game, %client, %firstWarning)
{
    %player = %client.player;
    
    if(isEventPending(%client.campingThread))
      cancel(%client.campingThread);

   //make sure we're still alive...
   if (!isObject(%player) || %player.getState() $= "Dead")
      return;

   //if the match hasn't yet started, don't warn or apply damage yet...
   if (!$MatchStarted)
   {
      %client.campingThread = %game.schedule($TurleCampTime / 2, "CampingDamage", %client, true);
   }
   else if (%firstWarning)
   {
      messageClient(%client, 'MsgHuntersNoCampZone', '\c2No turtling inside the base.', 1);
      %client.campingThread = %game.schedule($TurleCampTime / 2, "CampingDamage", %client, false);
   }
   else if(%client.turtledamage)
   {
      %player.setDamageFlash(0.1);
      %player.damage(0, %player.position, 0.05, $DamageType::NexusCamping);
      %client.campingThread = %game.schedule(1000, "CampingDamage", %client, false);
   }

}