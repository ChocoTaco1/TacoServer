//discordKill(); to kill bot connection
//discordCon(); to force connect
//sendToDiscord(%message, %channel); to send a message manually

//exec("scripts/autoexec/zzDiscordBot.cs");

//ip of the bot
$discordBot::IP = "127.0.0.1:28003";
$discordBot::reconnectTimeout = 3 * 60000;
//auto connect on start
$discordBot::autoStart = 0;
//used on the bot to help split thigns up
$discordBot::cmdSplit = "%c%";
$discordBot::cmdSubSplit = "%t%";
//These are set via the bot
$discordBot::monitorChannel = 0;
$discordBot::serverFeed = 1;

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
		sendToDiscordEmote(%message, $discordBot::serverFeed);
	}
	if($missionName !$= $discordBot::cm && ClientGroup.getCount() > 2){
		   sendToDiscordEmote("The mission changed to" SPC  $missionName, $discordBot::serverFeed);
		   $discordBot::cm = $missionName;
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
		sendToDiscord(%log, $discordBot::monitorChannel);
}
function LogMessage(%client, %msg, %cat){
   if(discord.lastState $= "Connected")
      sendToDiscord("Message" SPC %client.nameBase SPC %msg, $discordBot::monitorChannel);
   parent::LogMessage(%client, %msg, %cat);
}

};

if(!isActivePackage(discordPackage))
   activatePackage(discordPackage);

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
			if(%points !$=1)
				%s = "s";
			%hitType = %var3;
			%weapon = %var4;
			%distance = %var5;
			%vel = %var6;
			if(%points)
				%msg = getTaggedString(%sourceObject.client.name) SPC "receives" SPC %points SPC "point" @ %s @ "! [" @ %hitType SPC %weapon @ "] [Distance:" SPC %distance @ "] [Speed:" SPC %vel @ "]";
	}

	if(%msg !$= "")
	{
		%msg = stripChars(%msg, "\cp\co\c0\c6\c7\c8\c9");
		sendToDiscordEmote(%msg, $discordBot::serverFeed);
	}
}
function sendToDiscord(%msg,%channel)
{
   if(isObject(discord) && %msg !$= "")
   {
      if(discord.lastState $= "Connected")
	   {
         discord.send("MSG" @ $discordBot::cmdSplit @ (%channel) @ $discordBot::cmdSplit @ "0" @ $discordBot::cmdSplit @ %msg @ "\r\n");
      }
   }
}
function sendToDiscordEmote(%msg,%channel)//emote filter will be applyed used in server feed
{
   if(isObject(discord) && %msg !$= "")
   {
      if(discord.lastState $= "Connected")
	   {
         discord.send("MSG" @ $discordBot::cmdSplit @ (%channel) @ $discordBot::cmdSplit @ "emote" @ $discordBot::cmdSplit @ %msg @ "\r\n");
      }
   }
}
function discordCon(){
   if(discord.lastState !$= "Connected" && discord.lastState !$=  "Connecting"){
      if(isEventPending($discordBot::reconnectEvent))
         cancel($discordBot::reconnectEvent);
      if(isObject(discord))
         discord.delete();
      new TCPObject(discord);
      discord.lastState = "Connecting";
      //discord.connect($discordBot::IP);
      discord.schedule(1000, "connect", $discordBot::IP);
      //discord.schedule(5000, "send", "AUTH" @ $discordBot::cmdSplit @ $discordBot::cmdSplit @ $Host::GameName @ "\r\n");
   }
}
function discordKill(){
 if(isEventPending($discordBot::reconnectEvent))
      cancel($discordBot::reconnectEvent);
   discord.delete();
}

function discord::onDNSFailed(%this){
   %this.lastState = "DNSFailed";
   error("Discord" SPC %this.lastState);
}

function discord::onConnectFailed(%this){
   if(isEventPending($discordBot::reconnectEvent))
      cancel($discordBot::reconnectEvent);
   $discordBot::reconnectEvent = schedule($discordBot::reconnectTimeout,0,"discordCon");
   %this.lastState = "ConnectFailed";
   error("Discord" SPC %this.lastState);
   discord.delete();
}

function discord::onDNSResolved(%this){
   %this.lastState = "DNSResolved";
   error("Discord" SPC %this.lastState);
}

function discord::onConnected(%this){
   discord.schedule(1000, "send", "AUTH" @ $discordBot::cmdSplit @ $discordBot::cmdSplit @ $Host::GameName @ "\r\n");
   %this.lastState = "Connected";
   error("Discord" SPC %this.lastState);
}

function discord::onDisconnect(%this){
   if(%this.lastState $= "Connecting" && $discordBot::failCon++ < 20){
      schedule(5000,0,"discordCon");
   }
   if(isEventPending($discordBot::reconnectEvent))
      cancel($discordBot::reconnectEvent);
   $discordBot::reconnectEvent = schedule($discordBot::reconnectTimeout,0,"discordCon");
   %this.lastState = "Disconnected";
   error("Discord" SPC %this.lastState);
   discord.delete();
}

function discord::onLine(%this, %line){
   %lineStrip = stripChars(%line,"\r\n");
   %cmd = getWord(%lineStrip,0);
   switch$(%cmd){
      //case "Discord":
         //messageAll( 'MsgDiscord', '\c3Discord: \c4%1 %2',getWord(%lineStrip,1),getWords(%lineStrip,2,getWordCount(%lineStrip) -1));
      case "PLOTSTOP":
         $pathMaps::running = 0;
      case "PLOTPLAYER":
         startPlayerPlot(getWord(%lineStrip,1));
      case "GETSTAT":
            %var = getWord(%lineStrip,1);
            %mon = getWord(%lineStrip,2);
            %year = getWord(%lineStrip,3);
            %game = getWord(%lineStrip,4);
            %returnIndex = getWord(%lineStrip,5);
            %nameList = $lData::name[%var,%game,"month",%mon,%year];
            %dataList = $lData::data[%var,%game,"month",%mon,%year];
            %nameList = strreplace(%nameList,"\t","%t");
            %dataList = strreplace(%dataList,"\t","%t");
            error("Discord" SPC "GETSTAT" SPC %var SPC %mon SPC %year SPC %game SPC %returnIndex);
            discord.schedule(32,"send","SINSTAT" @ "%c%" @ %nameList @ "%c%" @ %dataList @ "%c%" @ %returnIndex @ "%c%" @ %var @ "\r\n");
      case "GENSTATS":
         if(!$genStatsLockout){
            $genStatsLockout = 1;
            %month = getWord(%lineStrip,1);
            %year = getWord(%lineStrip,2);
            if(%month > 0 && %year > 0){
               schedule(1000, 0, "sendLDATA", %month, %year, "CTFGame");
            }
         }
         else{
            sendToDiscord("Already Building Please Wait", $discordBot::monitorChannel);
         }
      case "PING":
         discord.send("PONG" @ $discordBot::cmdSplit @ "\r\n");
      case "PINGAVG":
      %min = 10000;
      %max = -10000;
      %lowCount = %lowPing = 0;
         for(%i = 0; %i < ClientGroup.getCount(); %i++){
            %cl = ClientGroup.getObject(%i);
            %ping = %cl.isAIControlled() ? 0 : %cl.getPing();
               %min  =  (%ping < %min) ? %ping : %min;
               %max  =  (%ping > %max) ? %ping : %max;
               if(%ping < 250){
                  %lowCount++;
                  %lowPing += %ping;
               }
               %pc++;
               %pingT += %ping;
         }
         %lowCount = (%lowCount == 0) ? 1 : %lowCount;
         if(!%pc){
            sendToDiscord("Ping AVG:" @ 0 SPC "Low Avg:" @ 0 SPC "Min:" @ 0 SPC "Max:" @ 0, $discordBot::monitorChannel);
         }
         else{
            %avg = mFloor(%pingT/%pc);
            %lavg = mFloor(%lowPing/%lowCount);
            sendToDiscord("Ping AVG:" @ %avg SPC "Low Avg:" @ %lavg SPC "Min:" @ %min SPC "Max:" @ %max, $discordBot::monitorChannel);
         }
      case "PINGLIST":
         if(isObject(discord) && discord.lastState $= "Connected"){
            if(ClientGroup.getCount() > 0){
               for(%i = 0; %i < ClientGroup.getCount(); %i++){
                  %cl = ClientGroup.getObject(%i);
                  %ping = %cl.isAIControlled() ? 0 : %cl.getPing();
                  %msg = %cl.namebase @ $discordBot::cmdSubSplit @ %ping @ $discordBot::cmdSubSplit @ %i;
                  discord.schedule(%i*32,"send","MSGSTACK" @ $discordBot::cmdSplit @ ($discordBot::monitorChannel) @ $discordBot::cmdSplit @ %msg @ "\r\n");
               }
               discord.schedule((%i+1)*32,"send","PROCSTACK" @ $discordBot::cmdSplit @ ($discordBot::monitorChannel) @ $discordBot::cmdSplit @ "msgList" @ "\r\n");
            }
         }
      case "BANLIST":
         if($dtBanList::NameListCount){
            for (%i = 0; %i <  $dtBanList::NameListCount; %i++){
               %fieldList = $dtBanList::NameList[%i];
               %msg = "Index:" @ %i SPC "Name:" @ getField(%fieldList,0) SPC  "GUID:" @ getField(%fieldList,1)  SPC "IP:" @  getField(%fieldList,2);
               discord.schedule(%i*32,"send","MSGSTACK" @ $discordBot::cmdSplit @ ($discordBot::monitorChannel) @ $discordBot::cmdSplit @ %msg @ "\r\n");
            }
            discord.schedule((%i+1)*32,"send","PROCSTACK" @ $discordBot::cmdSplit @ ($discordBot::monitorChannel) @ $discordBot::cmdSplit @ "banList" @ "\r\n");
         }
         else{
            sendToDiscord("No active bans, see ban file for manual/older entries", $discordBot::monitorChannel);
         }
      case "UNBANINDEX":
         %var = getWord(%lineStrip,1);
         if(%var < $dtBanList::NameListCount){
            %name = unbanIndex(%var);
            if(%name !$= ""){
               sendToDiscord("User:" @ %name SPC "has been unbanned", $discordBot::monitorChannel);
            }
            else{
               sendToDiscord("Index Removed", $discordBot::monitorChannel);
            }
         }
         else{
            sendToDiscord("Invalid Index", $discordBot::monitorChannel);
         }

      default:
         error("Discord Bad Command" SPC %line);
   }
}
if(!isObject(discord) && $discordBot::autoStart){
   discordCon();
}

function sendLDATA(%month, %year, %type){
    %file = new FileObject();
   RootGroup.add(%file);
   %folderPath = "serverStats/LData/*.cs";
   %count = getFileCount(%folderPath);
   %found = 0;
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%folderPath);
      %fieldPath =strreplace(%filePath,"-","\t");
      %game = getField(%fieldPath,1);
      %m = getField(%fieldPath,2); // 0 path / 1  game / 2 mon / 3 year / 4 type / 5 .cs
      %y = getField(%fieldPath,3);
      //%lType = getField(%fieldPath,4);
      if(%month $= %m && %y $= %year && %game $= %type){
         %found = 1;
         break;
      }
   }
   $dtSendDataMon = %month;
   $dtSendDataYear = %year;
   $dtSendDataType = %type;
   if(isFile(%filepath) && %found){
      sendToDiscord("Building Big Stats", $discordBot::monitorChannel);
      %file.OpenForRead(%filepath);
      %i = 0;
      while(!%file.isEOF()){
         %line =  %file.readLine();
         if(strPos(%line,"%tguid") == -1){
            discord.schedule((%i++)*32,"send","STATSDATA" @ "%c%" @ $dtSendDataMon @ "%c%" @ $dtSendDataType @ "%c%" @ %line @ "\r\n");
         }
      }
      error("Sent LData To Discord" SPC %month SPC %year SPC %type SPC %i);
      discord.schedule((%i++*32)+1000,"send","PROCSTACK" @ $discordBot::cmdSplit @ ($discordBot::monitorChannel) @ $discordBot::cmdSplit @ "buildStats" @ $discordBot::cmdSplit @ %month @ $discordBot::cmdSplit @ %year @ $discordBot::cmdSplit @ %type @ $discordBot::cmdSplit @ "\r\n");
      schedule((%i++*32)+1000, 0, "unlockStatGen");

   }
   else{
      sendToDiscord("Error no file found", $discordBot::monitorChannel);
      $genStatsLockout = 0;
   }
   %file.close();
   %file.delete();
}
function unlockStatGen(){
   $genStatsLockout = 0;
}

////////////////////////////////////////////////////////////////////////////////
//Player Path Maps
////////////////////////////////////////////////////////////////////////////////

$pathMaps::maxCount = 32000;// default point count
$pathMaps::speed = 500;
function sendPrx(%x){
   for(%i = %x; %i < $prx::count && (%i - %x) < 50; %i++){
      %line =  $prx::data[%i];
      %msg = "CDATA" @ "%c%" @ %i @ "%c%" @ %line @ "\r\n";
      if(isObject(discord))
         discord.send(%msg);
      //discord.schedule(%i,"send",%msg);
   }
   if(%i < $prx::count)
      schedule(128, 0, "sendPrx", %i);
   else
      discord.schedule(5000,"send","PROCSTACK" @ $discordBot::cmdSplit @ ($discordBot::monitorChannel) @ $discordBot::cmdSplit @ "buildprx" @ $discordBot::cmdSplit @ $prx::terFile @ $discordBot::cmdSplit @ $prx::misFile @ "\r\n");
}

function startPlayerPlot(%count){
   if(!$pathMaps::running && (($MatchStarted + $missionRunning) == 2)){
      if(%count > 1000){
         $pathMaps::maxCount = %count;
      }
      $prx::terFile = Terrain.terrainFile;
      $prx::misFile = $missionName;
      $prx::count = 0;
      $pathMaps::running = 1;
      pathMapData();
      sendToDiscord("Player Plot Started" SPC $pathMaps::maxCount, $discordBot::monitorChannel);
      error("Player Plot Started");
   }
   else{
      sendToDiscord("Game Has Not Started Yet", $discordBot::monitorChannel);
   }
}
function floorVector(%vec){
   return mFloor(getWord(%vec,0)) SPC mFloor(getWord(%vec,1)) SPC mFloor(getWord(%vec,2));
}
function pathDataPoint(%client){
   %player = %client.player;
   if(isObject(%player)){
      %veh =  (isObject(%client.vehicleMounted)) ? %client.vehicleMounted.getDataBlock().getName() : 0;
      $prx::data[$prx::Count] = %client.nameBase @ "%c" @ %pos @ "%c" @ %client.team @ "%c" @ isObject(%player.holdingFlag) @ "%c" @ %veh @ "%c" @ getSimTime();
      $prx::count++;
   }
}
function pathMapData(){ //loop to collect player position data
   for(%x = 0; %x < ClientGroup.getCount(); %x++){
      %client = ClientGroup.getObject(%x);
      %player = %client.player;
      if(isObject(%player)){
         %pos = %player.getPosition();
         %fpos = floorVector(%pos);
         if(%player.lpm !$= %fpos){
            %veh =  (isObject(%client.vehicleMounted)) ? %client.vehicleMounted.getDataBlock().getName() : 0;
            $prx::data[$prx::Count] = %client.nameBase @ "%c" @ %pos @ "%c" @ %client.team @ "%c" @ isObject(%player.holdingFlag) @ "%c" @ %veh @ "%c" @ getSimTime();
            $prx::count++;
         }
         %player.lpm = %fpos;
      }
   }
   if(!($prx::eCount++ % 10)){error("pathMapData" SPC $prx::Count);}

   if($pathMaps::running && (($MatchStarted + $missionRunning) == 2)  && $prx::Count < $pathMaps::maxCount){// note will stop at end of mission
      schedule($pathMaps::speed, 0,"pathMapData");
   }
   else{
      $pathMaps::running = 0;
      sendToDiscord("Player Plot Processing", $discordBot::monitorChannel);
      error("Player Plot Tracking Has Ended");
      sendPrx(0);
   }
}

