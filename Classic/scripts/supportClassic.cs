/////////////////////////////////////////////////////////////////////////////////////////
// z0dd - ZOD: Generic Console Spam fixes ///////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function Projectile::isMounted(%this)
{
   return 0;
}

function VehicleBlocker::getDataBlock(%this)
{
   return %this;
}

function VehicleBlocker::getName(%this)
{
   return %this;
}

function WaterBlock::damage(%this)
{
   // Do nothing
}

function InteriorInstance::getDataBlock(%this)
{
   return %this;
}

function InteriorInstance::getName(%this)
{
   return "InteriorInstance";
}

function TerrainBlock::getDataBlock(%this)
{
   return %this;
}

function TerrainBlock::getName(%this)
{
   return "Terrain";
}

function AIConnection::isMounted(%client)
{
   %vehicle = %client.getControlObject();
   %className = %vehicle.getDataBlock().className;
   if(%className $= WheeledVehicleData || %className $= FlyingVehicleData || %className $= HoverVehicleData)
      return true;
   else
      return false;
}

function ForceFieldBareData::isMounted(%obj)
{
   // created to prevent console errors
}

function ForceFieldBareData::damageObject(%data, %targetObject, %position, %sourceObject, %amount, %damageType)
{
   // created to prevent console errors
}

/////////////////////////////////////////////////////////////////////////////////////////
// Random Teams code by Founder (founder@mechina.com) 6/13/02 ///////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

// Couple other files edited for Random Teams.
// Hud.cs and DefaultGame.cs
function AIConnection::startMission(%client)
{
   // assign the team
   if (%client.team <= 0)
      Game.assignClientTeam(%client);

   if(%client.lastTeam !$= "")
   {
      if(%client.team != %client.lastTeam)
         Game.AIChangeTeam( %client, %client.lastTeam );
   }
   // set the client's sensor group...
   setTargetSensorGroup( %client.target, %client.team );
   %client.setSensorGroup( %client.team );

   // sends a message so everyone know the bot is in the game...
   Game.AIHasJoined(%client);
   %client.matchStartReady = true;

   // spawn the bot...
   onAIRespawn(%client);
}

/////////////////////////////////////////////////////////////////////////////////////////
// z0dd - ZOD: Universal functions //////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

function stripTaggedVar(%var)
{
   return stripChars( detag( getTaggedString( %var ) ), "\cp\co\c6\c7\c8\c9" );
}

// Removes triggers from Siege when players switch sides, also used in practiceCTF
function cleanTriggers(%group)
{
   if (%group > 0)
      %depCount = %group.getCount();
   else
      return;

   for(%i = 0; %i < %depCount; %i++)
   {
      %deplObj = %group.getObject(%i);
      if(isObject(%deplObj))
      {
         if(%deplObj.trigger !$= "")
            %deplObj.trigger.schedule(0, "delete");
      }
   }
}

// -----------------------------------------------------
// z0dd - ZOD, 6/22/02. Hack to eliminate texture cheats
package cloaking
{
   function ShapeBase::setCloaked(%obj, %bool)
   {
      parent::setCloaked(%obj, %bool);
      if(%bool)
         %obj.startFade(0, 800, true);
      else
         %obj.startFade(0, 0, false);
   }
};
activatePackage(cloaking);

// z0dd - ZOD, 5/18/03. Bug fix, added !=0 for deployed turrets because of tagging players name to nameTag.
// No package because this file declared AFTER GameBase.cs
function GameBaseData::onRemove(%data, %obj)
{
   %target = %obj.getTarget();

   // first 32 targets are team targets
   if(%target >= 32)
   {
      if(%obj.nameTag !$= "" && %obj.nameTag != 0)
         removeTaggedString(%obj.nameTag);

      freeTarget(%target);
   }
}

function serverCmdPrivateMessageSent(%client, %target, %text)
{
   // Client side:
   //commandToServer('PrivateMessageSent', %target, %text);

   if((%text $= "") || spamAlert(%client))
      return;

   if(%client.isAdmin)
   {
      %snd = '~wfx/misc/diagnostic_on.wav';
      if(strlen(%text) >= $Host::MaxMessageLen)
         %text = getSubStr(%text, 0, $Host::MaxMessageLen);

      messageClient(%target, 'MsgPrivate', '\c5Message from %1: \c3%2%3', %client.name, %text, %snd);
   }
   else
      messageClient(%client, 'MsgError', '\c4Only admins can send private messages');
}

//////////////////////////////////////////////////////////////////////////////
// z0dd - ZOD, 10/03/02. Part of flag collision bug hack.
//////////////////////////////////////////////////////////////////////////////

datablock TriggerData(flagTrigger)
{
   tickPeriodMS = 10;
};

function flagTrigger::onEnterTrigger(%data, %obj, %colObj)
{
   %flag = %obj.flag;
   if($flagStatus[%flag.team] $= "<At Base>")
      %flag.getDataBlock().onCollision(%flag, %colObj);
   else
      return;
}

function flagTrigger::onLeaveTrigger(%data, %obj, %colObj)
{
   // Thou shalt not spam
}

function flagTrigger::onTickTrigger(%data, %obj)
{
   // Thou shalt not spam
}

//////////////////////////////////////////////////////////////////////////////
// Eolk - Ban functions.
//////////////////////////////////////////////////////////////////////////////
function ClassicAddBan(%label, %entry, %skipExport)
{
   // Add to in-memory list
   $ClassicPermaBan[$ClassicPermaBans] = %entry TAB %label;
   $ClassicPermaBans++;

   // Only write to file if we're supposed to.
   if (!%skipExport)
   {
      // Write to file
      %fp = new FileObject();
      if (%fp.openForAppend($Host::ClassicBanlist))
         %fp.writeLine("ClassicAddBan(\"" @ %label @ "\", \"" @ %entry @ "\", true);");
      else
         error("Encountered an I/O error while updating banlist.");

      %fp.close();
      %fp.delete();
   }
}

function ClassicAddWhitelist(%label, %entry)
{
   $ClassicWhitelist[$ClassicWhitelists] = %entry TAB %label;
   $ClassicWhitelists++;
}

function ClassicIsBanned(%client)
{
   %guid = %client.guid;
   %addr = getIPAddress(%client);
   %type = 0;

   for (%i = 0; %i < $ClassicPermaBans; %i++)
   {
      %entry = getField($ClassicPermaBan[%i], 0);

      if (%guid == %entry)
         %type |= 1;
      if (strstr(%addr, %entry) == 0)
         %type |= 2;
   }

   for (%x = 0; %x < $ClassicWhitelists; %x++)
   {
      %entry = getField($ClassicWhitelist[%x], 0);
      error(%entry);

      if (%guid == %entry || strstr(%addr, %entry) == 0)
         return error("SUCCESS!"); // We're whitelisted! Whee!
   }

   return %type;
}

function ClassicLoadBanlist()
{
   $ClassicPermaBans = 0;
   exec($Host::ClassicBanlist);
   $ClassicWhitelists = 0;
   exec($Host::ClassicWhitelist);
}

// From Eolks
function getIPAddress(%client)
{
   %port = nextToken( nextToken(%client.getAddress(), "ip", ":"), "addr", ":");
   return %addr;
}

// From Eolks
// We are not an admin MOD but this comes in handy when name is missing on races.
function KickByCID(%client, %reason, %time)
{
    if(!isObject(%client))
        return;

    // AI handler
    if(%client.isAIControlled())
    {
        %client.drop();
        $HostGameBotCount--;
        if($HostGameBotCount < 0)
            $HostGameBotCount = 0;
        return;
    }

    if (%reason $= "")
        %reason = "You have been kicked from the server.";
    // Perhaps we don't want time done.
    //if (%time < 0 || %time $= "")
    //    %time = $Host::KickBanTime;

    // Send proper messages
    messageClient(%client, 'onClientKicked', "");
    messageAllExcept(%client, -1, 'MsgClientDrop', "", %client.name, %client);

    // Remove their player, if one exists
    if (isObject(%client.player))
        %client.player.scriptKill(0);

    // Set reason, schedule removal
    %client.setDisconnectReason(%reason);
    %client.schedule(500, "delete");

    // Keep them out
   // if(%time != 0)
   //     BanList::add(%client.guid, %client.getAddress(), %time);
}

//More Spam
function TSStatic::onTrigger(%this, %triggerId, %on){
//anti console spam
}

function TSStatic::onTriggerTick(%this, %triggerId){
//anti console spam
}

function SimObject::setPosition(%obj, %pos){
     %obj.setTransform(%pos SPC getWords(%obj.getTransform(), 3, 6));
}