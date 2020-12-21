//discordKill(); to kill bot connection
//discordCon(); to force connect
//sendToDiscord(%message, %channel); to send a message manually

//exec("scripts/autoexec/zzDiscordBot.cs");

$discordBot::AuthKey[0] = "";
$discordBot::IP[0] = "";
$discordBot::reconnectTimeout = 3 * 60000;
$discordBot::AuthSet = 0;
$discordBot::autoStart = 1;


package discordPackage
{

//function chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
//{
  //parent::chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
  //if ( getsubstr(detag(%a2),0,1) !$= "/" ){
	 //sendToDiscord(%sender.nameBase SPC %a2,1);
  //}
//}

function messageAll(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
	parent::messageAll(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
	%type = getTaggedString(%msgType);
	switch$(%type)
	{
		case "msgExplosionKill" or "msgSuicide" or "msgVehicleSpawnKill" or "msgVehicleCrash" or "msgVehicleKill" or "msgTurretSelfKill" or "msgTurretSelfKill" or "msgCTurretKill" or "msgTurretKill" or 
			 "msgSelfKill" or "msgOOBKill" or "msgCampKill" or "msgTeamKill" or "msgLavaKill" or "msgLightningKill" or "MsgRogueMineKill" or "MsgHeadshotKill" or "MsgRearshotKill" or "MsgLegitKill":  
		%message = getTaggedString(%msgString);
		%message =  strreplace(%message,"%1",getTaggedString(%a1)); 
		%message =  strreplace(%message,"%2",getTaggedString(%a2)); 
		%message =  strreplace(%message,"%3",getTaggedString(%a3)); 
		%message =  strreplace(%message,"%4",getTaggedString(%a4)); 
		%message =  strreplace(%message,"%5",getTaggedString(%a5)); 
		%message =  strreplace(%message,"%6",getTaggedString(%a6)); 
		%message =  strreplace(%message,"%7",getTaggedString(%a7)); 
		%message =  strreplace(%message,"%8",getTaggedString(%a8)); 
		%message = stripChars(%message, "\cp\co\c0\c6\c7\c8\c9");
		sendToDiscord(%message,2);
	}
}

function discordBotProcess(%type, %var1, %var2, %var3, %var4, %var5, %var6)
{
	//echo(%type SPC %var1 SPC %var2 SPC %var3 SPC %var4 SPC %var5 SPC %var6);
	
	switch$ (%type)
	{
		case "flagCap":
			%game = %var1;
			%player = %var2;
			%flag = %player.holdingFlag;
			%held = %game.formatTime(getSimTime() - %game.flagHeldTime[%flag], false);
			%msg = getTaggedString(%player.client.name) SPC "captured the" SPC getTaggedString(%game.getTeamName(%flag.team)) SPC "flag. (Held:" SPC %held @ ")";
		case "touchEnemyFlag":
			%game = %var1;
			%player = %var2;
			%flag = %var3;
			if(!%player.flagTossWait)
			{
				%grabspeed = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);
				if(%flag.isHome)
					%msg = getTaggedString(%player.client.name) SPC "took the" SPC getTaggedString(%game.getTeamName(%flag.team)) SPC "flag. (Speed:" SPC %grabspeed @ "Kph)";
				else if(!%flag.isHome)
					%msg = getTaggedString(%player.client.name) SPC "took the" SPC getTaggedString(%game.getTeamName(%flag.team)) SPC "flag in the field. (Speed:" SPC %grabspeed @ "Kph)";  
			}
		case "droppedFlag":
			%game = %var1;
			%player = %var2;
			%flag = %player.holdingFlag;
			%held = %game.formatTime(getSimTime() - %game.flagHeldTime[%flag], false);
			%msg = getTaggedString(%player.client.name) SPC "dropped the" SPC getTaggedString(%game.getTeamName(%flag.team)) SPC "flag. (Held:" SPC %held @ ")";
		case "flagReturn":
			%game = %var1;
			%flag = %var2;
			%player = %var3;
			if(%player !$= "")
				%msg = getTaggedString(%player.client.name) SPC "returned the" SPC getTaggedString(%game.getTeamName(%flag.team)) SPC "flag.";
			else
				%msg = "The" SPC getTaggedString(%game.getTeamName(%flag.team)) SPC "flag was returned to base.";
		case "lakTouchFlag":
			%game = %var1;
			%player = %var2;
			%flag = %var3;
			if(!%player.client.flagDeny && %player.getState() !$= "Dead" && PlayingPlayers() > 1)
				%msg = getTaggedString(%player.client.name) SPC "has taken the flag.";
		case "lakMApoints":
		    %sourceObject = %var1;
			%points = %var2;
			if(%points == 1)
				%s = "s";
			%hitType = %var3;
			%weapon = %var4;
			%distance = %var5;
			%vel = %var6;
			if(%points)
				%msg = getTaggedString(%sourceObject.client.name) SPC "receives" SPC %points @ %s SPC "points! [" @ %hitType SPC %weapon @ "] [Distance:" SPC %distance @ "] [Speed:" SPC %vel @ "]";
	}
	
	if(%msg !$= "")
	{
		%msg = stripChars(%msg, "\cp\co\c0\c6\c7\c8\c9");
		sendToDiscord(%msg, 2);
	}
}

function CTFGame::flagCap(%game, %player)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("flagCap", %game, %player, %var3, %var4, %var5, %var6);
	parent::flagCap(%game, %player);
}

function CTFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("touchEnemyFlag", %game, %player, %flag, %var4, %var5, %var6);
	parent::playerTouchEnemyFlag(%game, %player, %flag);
}

function CTFGame::playerDroppedFlag(%game, %player)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("droppedFlag", %game, %player, %var3, %var4, %var5, %var6);
	parent::playerDroppedFlag(%game, %player);
}

function CTFGame::flagReturn(%game, %flag, %player)
{
	parent::flagReturn(%game, %flag, %player);
	if(discord.lastState $= "Connected")
		discordBotProcess("flagReturn", %game, %flag, %player, %var4, %var5, %var6);
}

function SCtFGame::flagCap(%game, %player)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("flagCap", %game, %player, %var3, %var4, %var5, %var6);
	parent::flagCap(%game, %player);
}

function SCtFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("touchEnemyFlag", %game, %player, %flag, %var4, %var5, %var6);
	parent::playerTouchEnemyFlag(%game, %player, %flag);
}

function SCtFGame::playerDroppedFlag(%game, %player)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("droppedFlag", %game, %player, %var3, %var4, %var5, %var6);
	parent::playerDroppedFlag(%game, %player);
}

function SCtFGame::flagReturn(%game, %flag, %player)
{
	parent::flagReturn(%game, %flag, %player);
	if(discord.lastState $= "Connected")
		discordBotProcess("flagReturn", %game, %flag, %player, %var4, %var5, %var6);
}

function LakRabbitGame::playerTouchFlag(%game, %player, %flag)
{
	if(discord.lastState $= "Connected")
		discordBotProcess("lakTouchFlag", %game, %player, %flag, %var4, %var5, %var6);
	parent::playerTouchFlag(%game, %player, %flag);
}

// In LakRabbitGame.cs inside lakrabbit override
// function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC)
// {
	// Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
	// if(discord.lastState $= "Connected")
		// discordBotProcess("lakMApoints", %sourceObject, %points, %hitType, %weapon, %distance, %vel);
// }

function dtEventLog(%log, %save)
{
	parent::dtEventLog(%log, %save);
	if(discord.lastState $= "Connected")
		sendToDiscord(%log, 1);
}
function LogMessage(%client, %msg, %cat){
   if(discord.lastState $= "Connected")
      sendToDiscord("Message" SPC %client.nameBase SPC %msg, 1);
   parent::LogMessage(%client, %msg, %cat);
}

};

if(!isActivePackage(discordPackage))
   activatePackage(discordPackage);

function sendToDiscord(%msg,%channel)
{
   if(isObject(discord) && %msg !$= "")
   {
      if(discord.lastState $= "Connected")
	   {
         discord.send("MSG" SPC (%channel-1) SPC %msg @ "\r\n");
      }
   }
}

function discordCon(){  
   if(discord.lastState !$= "Connected"){
      if(isEventPending($discordBot::reconnectEvent))
         cancel($discordBot::reconnectEvent);
      if(isObject(discord))
         discord.delete();
      new TCPObject(discord);  
      discord.lastState = "Connecting";
      discord.connect($discordBot::IP[$discordBot::AuthSet]);  
   }
}
function discordKill(){
 if(isEventPending($discordBot::reconnectEvent))
      cancel($discordBot::reconnectEvent); 
   discord.delete();
}

function discord::onDNSFailed(%this){
   %this.lastState = "DNSFailed";
   error(%this.lastState);
}

function discord::onConnectFailed(%this){
   %this.lastState = "ConnectFailed";
   error(%this.lastState);
   discord.delete();
   if(isEventPending($discordBot::reconnectEvent))
      cancel($discordBot::reconnectEvent);
   $discordBot::reconnectEvent = schedule($discordBot::reconnectTimeout,0,"discordCon");
}

function discord::onDNSResolved(%this){
   %this.lastState = "DNSResolved";
   error(%this.lastState);
}

function discord::onConnected(%this){
   %this.lastState = "Connected";
   error(%this.lastState);
   discord.send("AUTH" SPC $discordBot::AuthKey[$discordBot::AuthSet] @ "\r\n");
}

function discord::onDisconnect(%this){
   %this.lastState = "Disconnected";
   error(%this.lastState);
   discord.delete();
   if(isEventPending($discordBot::reconnectEvent))
      cancel($discordBot::reconnectEvent);
   $discordBot::reconnectEvent = schedule($discordBot::reconnectTimeout,0,"discordCon");
}

function discord::onLine(%this, %line){
   %lineStrip = stripChars(%line,"\r\n");
   %cmd = getWord(%lineStrip,0);
   switch$(%cmd){
      //case "Discord":
         //messageAll( 'MsgDiscord', '\c3Discord: \c4%1 %2',getWord(%lineStrip,1),getWords(%lineStrip,2,getWordCount(%lineStrip) -1)); 
      case "PING":
         discord.send("PONG" @ "\r\n");
      case "PINGX":
         discord.send("PINGY" @ "\r\n");
      default:
         error("Bad Command" SPC %line);
   }
}
if(!isObject(discord) && $discordBot::autoStart)
    discordCon();