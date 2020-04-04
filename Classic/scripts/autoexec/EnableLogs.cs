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

// voteLog(%client, %votemsg)
// Info: Logs the vote events
function voteLog(%client, %votemsg)
{
   if($Host::ClassicVoteLog)
   {
      // get the client info
      %authInfo = %client.getAuthInfo();
	  %ip = getField(strreplace(%client.getAddress(),":","\t"),1);

      // this is the info that will be logged
      $VoteLog = "#P[" @ $HostGamePlayerCount @ "]" SPC formatTimeString("M-d") SPC formatTimeString("[HH:nn]") SPC %client.nameBase @ " (" @ getField(%authInfo, 0) @ "," SPC %client.guid @ ") Initiated a vote:" SPC %votemsg SPC "CM[" @ $CurrentMission @ "]";

	  %logpath = $Host::ClassicVoteLogPath;
      export("$VoteLog", %logpath, true);
	  logEcho($VoteLog);
   }
}