//$Host::ClassicAdminLog = 1;
//$Host::ClassicConnectLog = 1;
//$Host::ClassicVoteLog = 1;

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
      $AdminLog = formatTimeString("M-d") SPC formatTimeString("[HH:nn]") SPC %client.nameBase @ " (" @ getField(%authInfo, 0) @ ", " @ %ip @ ", " @ %client.guid @ ", " @ %client.getAddress() @ ")" @ %msg SPC "[" @ $CurrentMission @ "]";

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
	  
	  $ConnectLog = %inout SPC "#P[" @ $HostGamePlayerCount @ "]" SPC formatTimeString("M-d") SPC formatTimeString("[HH:nn]") SPC %name SPC "(" @ getField(%authInfo, 0) @ "," SPC %client.guid @ "," SPC %ip @ ")" SPC "[" @ $CurrentMission @ "]" SPC "NTC[" @ %ntc @ "]";

	  %logpath = $Host::ClassicConnLogPath;
      export("$ConnectLog", %logpath, true);
	  logEcho($ConnectLog);
	  echo($ConnectLog);
   }
}

   {
      // get the client info
      %authInfo = %client.getAuthInfo();
	  
	  // show name for Votekick
	  if(%typeName $= "VoteKickPlayer")
		   %arg1 = %arg1.nameBase;

      // this is the info that will be logged
      $VoteLog = "#P[" @ $HostGamePlayerCount @ "]" SPC formatTimeString("M-d") SPC formatTimeString("[HH:nn]") SPC %client.nameBase @ " (" @ getField(%authInfo, 0) @ "," SPC %client.guid @ ") Initiated a vote:" SPC %typeName SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4 SPC "CM[" @ $CurrentMission @ "]";

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

   $ClassicChatLog = "["@formattimestring("H:nn:ss")@"] "@%team SPC getTaggedString(%client.name)@": "@%msg;
   $ClassicChatLog = stripChars($ClassicChatLog, "\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9\x10\x11\co\cp");
   %path = $Host::ClassicChatLogPath @ formatTimeString("/yy/mm-MM/dd.log");
   export("$ClassicChatLog", %path, true);
}