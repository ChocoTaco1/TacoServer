// altWarmup.cs
//
// This script allows players time before a match to pick teams
// Puts everyone in observer at mission end
//
// exec("scripts/autoexec/altWarmup.cs");

// Enable or Disable
$AW::EnableALTWarmUp = 1;
// Normal Warmup Time (In Seconds) - 20 is 20 Seconds
$AW::DefaultWarmUpTime = 20;
// The amount of time you want to allow for players to switch teams (In seconds) - 60 is 60 seconds
$AW::ALTWarmUpTime = 60;
// Minimum Population to Activate
$AW::MinALTWarmUpPop = 8;

package altWarmup
{

function DefaultGame::setupClientTeams(%game)
{
   $Host::warmupTime = $AW::DefaultWarmUpTime;
   if($HostGamePlayerCount >= $AW::MinALTWarmUpPop && $AW::EnableALTWarmUp && ($CurrentMissionType $= "CTF" || $CurrentMissionType $= "SCtF"))
	   %altWarmup = 1;
   
   if(%altWarmup)
   {
	   $Host::warmupTime = $AW::ALTWarmUpTime;
	   for(%i = 0; %i < ClientGroup.getCount(); %i ++) 
	   {
		  %client = ClientGroup.getObject(%i);
		  
		  //Put everyone in observer
		  %client.team = 0;
		  %client.lastTeam = 0;
	   }
   }
   else
	  parent::setupClientTeams(%game);
}

// Re-done for our needs
// If team change time too low can crash server --z0dd - ZOD
function serverCmdClientJoinTeam(%client, %team, %admin)
{
   // z0dd - ZOD, 4/10/04. ilys - if the client does not enter a team, uses a team less than -1,
   // more than the number of teams for the gametype or zero, set his team to -1 (switch)
   if(%team $= "" || %team < -1 || %team == 0 || %team > Game.numTeams)
      %team = -1;

   if( %team == -1 )
   {
      if( %client.team == 1 )
         %team = 2;
      else
         %team = 1;
   }
   
   if(isObject(Game) && Game.kickClient != %client)
   {
      if(%client.team != %team)   
      {
		 if(!$MatchStarted && $AW::EnableALTWarmUp)
	     {
			 if(!%client.waitStart || (getSimTime() - %client.waitStart) > 5000 || %client.isAdmin)
			 {
				%client.waitStart = getSimTime();
				
				if(%client.team == 0)
				   clearBottomPrint(%client);
			 
				if(%client.isAIControlled())
				   Game.AIChangeTeam( %client, %team );
				else
				   Game.clientChangeTeam( %client, %team, %fromObs );
			 }
			 else
			 {
				%wait = mFloor((5000 - (getSimTime() - %client.waitStart)) / 1000);
					messageClient(%client, "", '\c3WAIT MESSAGE:\cr You must wait another %1 seconds', %wait);
			 }
		 }
		 // z0dd - ZOD, 9/17/02. Fair teams, check for Team Rabbit 2 as well.
		 else 
		 {
			 if(($FairTeams && !%client.isAdmin) && ($CurrentMissionType !$= TR2))
			 {
				%otherTeam = %team == 1 ? 2 : 1;
				if(!%admin.isAdmin && %team != 0 && ($TeamRank[%team, count]+1) > $TeamRank[%otherTeam, count])
				{
				   messageClient(%client, 'MsgFairTeams', '\c2Teams will be uneven, please choose another team.');
				   return;
				}
			 }
			 
			 if(!%client.waitStart || (getSimTime() - %client.waitStart) > 15000 || %client.isAdmin)
			 {
				%client.waitStart = getSimTime();
			 
				if(%client.team == 0)
				   clearBottomPrint(%client);
			 
				if(%client.isAIControlled())
				   Game.AIChangeTeam( %client, %team );
				else
				   Game.clientChangeTeam( %client, %team, %fromObs );
			 }
			 else
			 {
				%wait = mFloor((15000 - (getSimTime() - %client.waitStart)) / 1000);
					messageClient(%client, "", '\c3WAIT MESSAGE:\cr You must wait another %1 seconds', %wait);
			 }
	     }
	  }
   }   
}

// So flag snatch sound wont play at the end of the match
function CTFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
   if(!$missionRunning)
		return;
   
   parent::playerTouchEnemyFlag(%game, %player, %flag);
}

function SCtFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
   if(!$missionRunning)
		return;
   
   parent::playerTouchEnemyFlag(%game, %player, %flag);
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(altWarmup))
    activatePackage(altWarmup);