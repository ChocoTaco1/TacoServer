//$Host::ClassicAdminLog = 1;
//$Host::ClassicChatLog = 1;
//$Host::ClassicConnectLog = 1;
//$Host::ClassicVoteLog = 1;
//$Host::ClassicTeamKillLog = 1;

//exec("scripts/autoexec/EnableLogs.cs");

//Enable Logs
setlogmode(1);

// adminLog(%client, %msg)
// Info: Logs the admin events
function adminLog(%client, %msg)
{
   if(%client.isAdmin && $Host::ClassicAdminLog)
   {
      // get the client info
      %authInfo = %client.getAuthInfo();
	  %ip = getField(strreplace(%client.getAddress(),":","\t"),1);

      // this is the info that will be logged
      $AdminLog = formatTimeString("M-d") SPC formatTimeString("[hh:nn:a]") SPC %client.nameBase @ " (" @ getField(%authInfo, 0) @ ", " @ %ip @ ", " @ %client.guid @ ", " @ %client.getAddress() @ ")" @ %msg SPC "[" @ $CurrentMission @ "]";

	  %logpath = $Host::ClassicAdminLogPath;
      export("$AdminLog", %logpath, true);
	  logEcho($AdminLog);
	  echo($AdminLog);
   }
}

// connectLog(%client, %realname, %tag)
// Info: Logs the connections
function connectLog(%client, %isDisconnect)
{
   if($Host::ClassicConnectLog && !%client.isAIControlled())
   {
      // get the client info
      %authInfo = %client.getAuthInfo();
	  %ip = getField(strreplace(%client.getAddress(),":","\t"),1);
	  
      // net tournament client present?
	  if (!%client.t2csri_sentComCertDone)
		%ntc = "N";
	  else
		%ntc = "Y";
	  
	  if(%isDisconnect)
		  %inout = "[Drop]";
	  else
		  %inout = "[Join]";
	  
	  if(%client.isSmurf)
		  %name = stripChars( detag( getTaggedString( %client.name ) ), "\cp\co\c6\c7\c8\c9" );
	  else
		  %name = %client.nameBase;
	  
	  $ConnectLog = %inout SPC "#P[" @ $HostGamePlayerCount @ "]" SPC formatTimeString("M-d") SPC formatTimeString("[hh:nn:a]") SPC %name SPC "(" @ getField(%authInfo, 0) @ "," SPC %client.guid @ "," SPC %ip @ ")" SPC "[" @ $CurrentMission @ "]" SPC "NTC[" @ %ntc @ "]";

	  %logpath = $Host::ClassicConnLogPath;
      export("$ConnectLog", %logpath, true);
	  logEcho($ConnectLog);
	  echo($ConnectLog);
   }
}

// voteLog(%client, %typeName, %arg1, %arg2, %arg3, %arg4)
// Info: Logs the vote events
function voteLog(%client, %typeName, %arg1, %arg2, %arg3, %arg4)
{
   if($Host::ClassicVoteLog)
   {
      // get the client info
      %authInfo = %client.getAuthInfo();
	  %ip = getField(strreplace(%client.getAddress(),":","\t"),1);
	  
	  // show name for Votekick
	  if(%typeName $= "VoteKickPlayer")
		   %arg1 = %arg1.nameBase @ "[" @ %arg1.teamkills + 1 @ "tks]";

      // this is the info that will be logged
      $VoteLog = "#P[" @ $HostGamePlayerCount @ "]" SPC formatTimeString("M-d") SPC formatTimeString("[hh:nn:a]") SPC %client.nameBase @ " (" @ getField(%authInfo, 0) @ "," SPC %client.guid @ ") Initiated a vote:" SPC %typeName SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4 SPC "CM[" @ $CurrentMission @ "]";

	  %logpath = $Host::ClassicVoteLogPath;
      export("$VoteLog", %logpath, true);
	  logEcho($VoteLog);
   }
}

// votePercentLog(%client, %typeName, %key, %game.votesFor[%game.kickTeam], %game.votesAgainst[%game.kickTeam], %totalVotes, %game.totalVotesNone)
// Info: Logs voting percent events
function votePercentLog(%display, %typeName, %key, %voteYea, %voteNay, %voteTotal, %voteNone) //%voteNone = Did Not Vote (DNV) (Abstain)
{
   if($Host::ClassicVoteLog)
   { 
	  // Dif calc for "VoteKickPlayer"
	  if(%typeName $= "VoteKickPlayer")
	  {
		   %percent = mFloor((%voteYea/ClientGroup.getCount()) * 100);
		   %voteNone = "N/A";
		   %display = %typeName SPC "[" @ %display.nameBase @ "]";
	  }
	  else
	  {
		  %percent = mFloor((%voteYea/(ClientGroup.getCount() - %voteNone)) * 100);
		  %display = %typeName SPC "[" @ %display @ "]";
	  }

   $VoteLog = "[" @ %key @ "]" SPC %display SPC "Yea[" @ %voteYea @ "] Nay[" @ %voteNay @ "] Abstain[" @ %voteNone @ "] Total[" @ %voteTotal @ "] Vote%[" @ %percent @ "]";

      // this is the info that will be logged
	  %logpath = $Host::ClassicVoteLogPath;
	  export("$VoteLog", %logpath, true);
	  logEcho($VoteLog);
   }
}

// From Goon
// Slightly more elegant solution rather than spamming console
function ClassicChatLog(%client, %id, %team, %msg)
{
   // We don't care about bots.
   if(%client.isAIControlled())
      return;

   // Don't log voicepack stuff.
   if(strstr(%msg, "~w") != -1 || strstr(%msg, "flag") != -1)
      return;

   switch$(%id)
   {
      case 0:
         %team = "[Global]";
      case 1:
         if($countdownStarted)
			 %team = getTaggedString(Game.getTeamName(%team));
		 else
			 %team = "Debrief";
		 
		 if(%team $= "Unassigned")
			 %team = "Observer";
		 else if($CurrentMissionType $= "LakRabbit" || $CurrentMissionType $= "DM") 
	         %team = $dtStats::gtNameLong[%client.lgame]; //from zDarktigerStats.cs
		 
		 %team = "[" @ %team @ "]";
      case 2:
         %team = "[Admin]";
      case 3:
         %team = "[Bottomprint]";
      case 4:
         %team = "[Centerprint]";
   }

   $ClassicChatLog = "["@formattimestring("hh:nn:ss:a")@"] "@%team SPC getTaggedString(%client.name)@": "@%msg;
   $ClassicChatLog = stripChars($ClassicChatLog, "\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9\x10\x11\co\cp");
   %path = $Host::ClassicChatLogPath @ formatTimeString("/yy/mm-MM/dd.log");
   export("$ClassicChatLog", %path, true);
}

// Log Teamkills
function teamkillLog(%victimID, %killerID, %damageType)
{
   if(!$Host::ClassicTeamKillLog)
      return;
   
   if(!$CurrentMissionType $= "CTF" && !$CurrentMissionType $= "SCTF")
      return;

   //damageType
   %type = getTaggedString($DamageTypeText[%damageType]);

   //Killer tks / Victim tks
   //Note: %killerID.teamkills + 1 as this is added later
   //Tks For this map only
   %ktk = %killerID.teamkills + 1;
   %vtk = %victimID.teamkills;

   //Stage in warnings
   %s = "";
   if(!%killerID.isAdmin) //Admins dont get warnings
   {
      if(%ktk >= $Host::TKWarn1 && %ktk < $Host::TKWarn2)
         %s = "[Warned] ";
      else if(%ktk >= $Host::TKWarn2 && %ktk < $Host::TKMax) 
         %s = "[Warned 2] ";
      else if(%ktk >= $Host::TKMax)
         %s = "[Kicked] ";
   }
   
   $teamkillLog = formatTimeString("M-d") SPC formatTimeString("[hh:nn:a]") SPC %s @ %killerID.nameBase @ "(" @ %killerID.guid @ ")[" @ %type @ "][" @ %ktk @ " tk] teamkilled" SPC %victimID.nameBase @ "[" @ %vtk @ " tk]. #P[" @ $HostGamePlayerCount @ "]" SPC "CM[" @ $CurrentMission @ "]";
   $teamkillLog = stripChars($teamkillLog, "\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9\x10\x11\co\cp");

	%logpath = $Host::ClassicTeamKillLogPath;
   export("$teamkillLog", %logpath, true);
	logEcho($teamkillLog);
	echo($teamkillLog);
}