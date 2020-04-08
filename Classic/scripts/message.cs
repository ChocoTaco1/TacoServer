$MaxMessageWavLength = 5200;

function addMessageCallback(%msgType, %func)
{
   for(%i = 0; (%afunc = $MSGCB[%msgType, %i]) !$= ""; %i++)
   {
      // only add each callback once
      if(%afunc $= %func)
         return;
   }
   $MSGCB[%msgType, %i] = %func;
}

function messagePump(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7 ,%a8, %a9, %a10)
{
   clientCmdServerMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
}

function clientCmdServerMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
   %tag = getWord(%msgType, 0);
   for(%i = 0; (%func = $MSGCB["", %i]) !$= ""; %i++)
      call(%func, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
   
   if(%tag !$= "")
      for(%i = 0; (%func = $MSGCB[%tag, %i]) !$= ""; %i++)
         call(%func, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
}

function defaultMessageCallback(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
   if ( %msgString $= "" )
      return;
      
   %message = detag( %msgString );
   // search for wav tag marker
   %wavStart = strstr( %message, "~w" );
   if ( %wavStart != -1 )
   {
      %wav = getSubStr( %message, %wavStart + 2, 1000 );
      %wavLengthMS = alxGetWaveLen( %wav );
      if ( %wavLengthMS <= $MaxMessageWavLength )
      {
         %handle = alxCreateSource( AudioChat, %wav );
         alxPlay( %handle );
      }
      else
         error( "WAV file \"" @ %wav @ "\" is too long! **" );

      %message = getSubStr( %message, 0, %wavStart );
      if ( %message !$= "" )
      	addMessageHudLine( %message );
   }
   else
	  addMessageHudLine( %message );
}

//--------------------------------------------------------------------------
function handleClientJoin(%msgType, %msgString, %clientName, %clientId, %targetId, %isAI, %isAdmin, %isSuperAdmin, %isSmurf, %guid)
{
   logEcho("got client join: " @ detag(%clientName) @ " : " @ %clientId);

	//create the player list group, and add it to the ClientConnectionGroup...
   if(!isObject("PlayerListGroup"))
	{
      %newGroup = new SimGroup("PlayerListGroup");
		ClientConnectionGroup.add(%newGroup);
	}

   %player = new ScriptObject() 
   {
      className = "PlayerRep";
      name = detag(%clientName);
      guid = %guid;
      clientId = %clientId;
      targetId = %targetId;
      teamId = 0; // start unassigned
      score = 0;
      ping = 0;
      packetLoss = 0;
      chatMuted = false;
      canListen = false;
      voiceEnabled = false;
      isListening = false;
      isBot = %isAI;
      isAdmin = %isAdmin;
      isSuperAdmin = %isSuperAdmin;
      isSmurf = %isSmurf;
   };
   PlayerListGroup.add(%player);
   $PlayerList[%clientId] = %player;

   if ( !%isAI )
      getPlayerPrefs(%player);
   lobbyUpdatePlayer( %clientId );
}

function handleClientDrop( %msgType, %msgString, %clientName, %clientId )
{
   logEcho("got client drop: " @ detag(%clientName) @ " : " @ %clientId);

   %player = $PlayerList[%clientId];
   if( %player )
   {
      %player.delete();
      $PlayerList[%clientId] = "";
      lobbyRemovePlayer( %clientId );
   }
}

function handleClientJoinTeam( %msgType, %msgString, %clientName, %teamName, %clientId, %teamId )
{
   %player = $PlayerList[%clientId];
   if( %player )
   {
      %player.teamId = %teamId;
      lobbyUpdatePlayer( %clientId );
   }
}

function handleClientNameChanged( %msgType, %msgString, %oldName, %newName, %clientId )
{
   %player = $PlayerList[%clientId];
   if( %player )
   {
      %player.name = detag( %newName );
      lobbyUpdatePlayer( %clientId );
   }
}

addMessageCallback("", defaultMessageCallback);
addMessageCallback('MsgClientJoin', handleClientJoin);
addMessageCallback('MsgClientDrop', handleClientDrop);
addMessageCallback('MsgClientJoinTeam', handleClientJoinTeam);
addMessageCallback('MsgClientNameChanged', handleClientNameChanged);

//---------------------------------------------------------------------------
// Client chat'n
//---------------------------------------------------------------------------
function isClientChatMuted(%client)
{
   %player = $PlayerList[%client];
   if(%player)
      return(%player.chatMuted ? true : false);
   return(true);
}

//---------------------------------------------------------------------------
function clientCmdChatMessage(%sender, %voice, %pitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
   %message = detag( %msgString );
   %voice = detag( %voice );

   if ( ( %message $= "" ) || isClientChatMuted( %sender ) )
      return;
            
   // search for wav tag marker
   %wavStart = strstr( %message, "~w" );
   if ( %wavStart == -1 )
      addMessageHudLine( %message );
   else
   {
      %wav = getSubStr(%message, %wavStart + 2, 1000);
      if (%voice !$= "")
         %wavFile = "voice/" @ %voice @ "/" @ %wav @ ".wav";
      else
         %wavFile = %wav;

      //only play voice files that are < 5000ms in length
      if (%pitch < 0.5 || %pitch > 2.0)
         %pitch = 1.0;
      %wavLengthMS = alxGetWaveLen(%wavFile) * %pitch;
      if (%wavLengthMS < $MaxMessageWavLength )
      {
         if ( $ClientChatHandle[%sender] != 0 )
            alxStop( $ClientChatHandle[%sender] );
         $ClientChatHandle[%sender] = alxCreateSource( AudioChat, %wavFile );

         //pitch the handle
         if (%pitch != 1.0)
            alxSourcef($ClientChatHandle[%sender], "AL_PITCH", %pitch);
         alxPlay( $ClientChatHandle[%sender] );
      }
      else
         error( "** WAV file \"" @ %wavFile @ "\" is too long! **" );

      %message = getSubStr(%message, 0, %wavStart);
      addMessageHudLine(%message);
   }
}

//---------------------------------------------------------------------------
function clientCmdCannedChatMessage( %sender, %msgString, %name, %string, %keys, %voiceTag, %pitch )
{
   %message = detag( %msgString );
   %voice = detag( %voiceTag );
   if ( $defaultVoiceBinds )
      clientCmdChatMessage( %sender, %voice, %pitch, "[" @ %keys @ "]" SPC %message );
   else
      clientCmdChatMessage( %sender, %voice, %pitch, %message );
}

//---------------------------------------------------------------------------
// silly spam protection...
$SPAM_PROTECTION_PERIOD     = 10000;
$SPAM_MESSAGE_THRESHOLD     = 4;
$SPAM_PENALTY_PERIOD        = 10000;
$SPAM_MESSAGE               = '\c3FLOOD PROTECTION:\cr You must wait another %1 seconds.';

function GameConnection::spamMessageTimeout(%this)
{
   if(%this.spamMessageCount > 0)
      %this.spamMessageCount--;
}

function GameConnection::spamReset(%this)
{
   %this.isSpamming = false;
}

function spamAlert(%client)
{
   if($Host::FloodProtectionEnabled != true)
      return(false);

   if(!%client.isSpamming && (%client.spamMessageCount >= $SPAM_MESSAGE_THRESHOLD))
   {
      %client.spamProtectStart = getSimTime();
      %client.isSpamming = true;
      %client.schedule($SPAM_PENALTY_PERIOD, spamReset);
   }

   if(%client.isSpamming)
   {
      %wait = mFloor(($SPAM_PENALTY_PERIOD - (getSimTime() - %client.spamProtectStart)) / 1000);
      messageClient(%client, "", $SPAM_MESSAGE, %wait);
      return(true);
   }

   %client.spamMessageCount++;
   %client.schedule($SPAM_PROTECTION_PERIOD, spamMessageTimeout);
   return(false);
}

function chatMessageClient( %client, %sender, %voiceTag, %voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
	//see if the client has muted the sender
	if ( !%client.muted[%sender] )
	   commandToClient( %client, 'ChatMessage', %sender, %voiceTag, %voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
}

function cannedChatMessageClient( %client, %sender, %msgString, %name, %string, %keys )
{
   if ( !%client.muted[%sender] )
      commandToClient( %client, 'CannedChatMessage', %sender, %msgString, %name, %string, %keys, %sender.voiceTag, %sender.voicePitch );
}

function chatMessageTeam( %sender, %team, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %obj = ClientGroup.getObject( %i );
      if ( %obj.team == %sender.team )
         chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
   }
}

function cannedChatMessageTeam( %sender, %team, %msgString, %name, %string, %keys )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;

   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %obj = ClientGroup.getObject( %i );
      if ( %obj.team == %sender.team )
         cannedChatMessageClient( %obj, %sender, %msgString, %name, %string, %keys );
   }
}

function chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
   {
      return;
   }

   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %obj = ClientGroup.getObject( %i );

      //----------------------------------------------------------------------------------------------------------------------------------------------------
      // z0dd - ZOD, 6/03/02. Allow observer global chat to be seen by all, not just admins and other observers
      chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );

      //if(%sender.team != 0)
      //{
      //   chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
      //}
      //else
      //{
         // message sender is an observer -- only send message to other observers
         //if(%obj.team == %sender.team || %obj.isAdmin || %obj.isSuperAdmin)
         //{
         //   chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
         //}
      //}
      //----------------------------------------------------------------------------------------------------------------------------------------------------
   }

   // z0dd - ZOD 5/13/02. Echo chat to console for remote telnet apps.
   if($Host::ClassicEchoChat)
   {
      echo( stripTaggedVar(%sender.name), ": ", %a2 );
   }
   //echo( "SAY: " @ stripchars(detag(gettaggedstring(%sender.name)),"\cp\co\c6\c7\c8\c9") @ " \"" @ %a2 @ "\"");
}

function cannedChatMessageAll( %sender, %msgString, %name, %string, %keys )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;

   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
      cannedChatMessageClient( ClientGroup.getObject(%i), %sender, %msgString, %name, %string, %keys );

   // z0dd - ZOD 5/13/02. Echo chat to console for remote telnet apps.
   if($Host::ClassicEchoChat)
   {
      echo( stripTaggedVar(%sender.name), ": ", getSubStr(%string, 0, strstr(%string, "~w")) );
   }
   //echo("SAY: " @ stripchars(detag(gettaggedstring(%sender.name)),"\cp\co\c6\c7\c8\c9") @ " \"" @ %string @ "\"");
}

//---------------------------------------------------------------------------
function messageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   commandToClient(%client, 'ServerMessage', %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
}

function messageTeam(%team, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   %count = ClientGroup.getCount();
   for(%cl= 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
	  if(%recipient.team == %team)
	      messageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function messageTeamExcept(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   %team = %client.team;
   %count = ClientGroup.getCount();
   for(%cl= 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
	  if((%recipient.team == %team) && (%recipient != %client))
	      messageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function messageAll(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   %count = ClientGroup.getCount();
   for(%cl = 0; %cl < %count; %cl++)
   {
      %client = ClientGroup.getObject(%cl);
      messageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function messageAllExcept(%client, %team, %msgtype, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{  
   //can exclude a client, a team or both. A -1 value in either field will ignore that exclusion, so
   //messageAllExcept(-1, -1, $Mesblah, 'Blah!'); will message everyone (since there shouldn't be a client -1 or client on team -1).
   %count = ClientGroup.getCount();
   for(%cl= 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
      if((%recipient != %client) && (%recipient.team != %team))
         messageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

//---------------------------------------------------------------------------
// functions to support repair messaging
//---------------------------------------------------------------------------
function clientCmdTeamRepairMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6)
{
   if($pref::ignoreTeamRepairMessages)
      %msgString = ""; // z0dd - ZOD, 8/23/02. Yogi. The message gets to the client but is "muted" from the HUD
   clientCmdServerMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6);
}

function teamRepairMessage(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6)
{
   %team = %client.team;

   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %recipient = ClientGroup.getObject(%i); // z0dd - ZOD, 8/20/02. param to getObject was $cl, which is an error
      if((%recipient.team == %team) && (%recipient != %client))
      {
         commandToClient(%recipient, 'TeamRepairMessage', %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6); // z0dd - ZOD, 6/18/02. Was sending to the wrong variable. 1st param was %client
      }
   }
}

// -----------------------------------------------------------------------------------------------------------
// z0dd - ZOD, 8/20/02. Added this team destroy message function
function teamDestroyMessage(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6)
{
   %team = %client.team;

   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %recipient = ClientGroup.getObject(%i);
      if((%recipient.team == %team) && (%recipient != %client))
      {
         commandToClient(%recipient, 'TeamDestroyMessage', %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6);
      }
   }
}
// -----------------------------------------------------------------------------------------------------------
