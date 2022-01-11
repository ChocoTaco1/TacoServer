// ------------------------------------------------------------------
// CLOAKING PACK
//
// When activated, this pack cloaks the user from all visual detection
// by other players and cameras. This is a local effect only (someone standing
// next to the cloaked player will still be visible). Motion sensors will
// still detect a (moving) cloaked player.
//
// When not activated, the pack creates a localized passive sensor dampening
// field.  The user will not be visible to any non-motion-detecting sensor.
//
//Only light armors may equip with this item.

datablock EffectProfile(CloakingPackActivateEffect)
{
   effectname = "packs/cloak_on";
   minDistance = 2.5;
   maxDistance = 2.5;
};

// datablock AudioProfile(CloakingPackActivateSound)
// {
//    filename = "fx/packs/cloak_on.wav";
//    description = CloseLooping3d;
//    preload = true;
//    effect = CloakingPackActivateEffect;
// };

//Added (From CloseLooping3d)
datablock AudioDescription(CloakLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   minDistance= 15.0; //Was 10
   MaxDistance= 55.0; //Was 50
   type     = $EffectAudioType;
   environmentLevel = 1.0;
};

datablock AudioProfile(CloakingPackActivateSound)
{
   filename = "fx/packs/cloak_on.wav";
   description = CloakLooping3d;
   preload = true;
   effect = CloakingPackActivateEffect;
};

datablock ShapeBaseImageData(CloakingPackImage)
{
   shapeFile = "pack_upgrade_cloaking.dts";
   item = CloakingPack;
   mountPoint = 1;
   offset = "0 0 0";

   usesEnergy = true;
   minEnergy = 3;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";
   
   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateSequence[1] = "fire";
   stateSound[1] = "CloakingPackActivateSound";
   stateEnergyDrain[1] = 10.5;  // z0dd - ZOD, 7/16/03. Improve cloak pack. Was 12 (Base). Was 11.75 in Classic 1.3, now match jammer pack
   stateTransitionOnTriggerUp[1] = "Deactivate";
   stateTransitionOnNoAmmo[1] = "Deactivate";

   stateName[2] = "Deactivate";
   stateScript[2] = "onDeactivate";
   stateTransitionOnTimeout[2] = "Idle";
};

datablock ItemData(CloakingPack)
{
   className = Pack;
   catagory = "Packs";
   shapeFile = "pack_upgrade_cloaking.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   rotate = true;
   image = "CloakingPackImage";
   pickUpName = "a cloaking pack";
   computeCRC = true;
};

function CloakingPackImage::onMount(%data, %obj, %node)
{
   // player is sensor-invisible while wearing pack (except to motion sensors)
   %obj.setPassiveJammed(true);
}

function CloakingPackImage::onUnmount(%data, %obj, %node)
{
   %obj.setPassiveJammed(false);
   %obj.setCloaked(false);
   %obj.setImageTrigger(%node, false);
}

// make player completely invisible to all players/sensors (except motion)
function CloakingPackImage::onActivate(%data, %obj, %slot)
{
   if(%obj.reCloak !$= "")
   {   
      Cancel(%obj.reCloak);
      %obj.reCloak = "";
   }
   
   if(%obj.client.armor $= "Light") 
   {
      // can the player currently cloak (function returns "true" or reason for failure)?
      if(%obj.canCloak() $= "true")
      {
         messageClient(%obj.client, 'MsgCloakingPackOn', '\c2Cloaking pack on.');
         %obj.setCloaked(true);
         // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
         commandToClient( %obj.client, 'setCloakIconOn' );
      }
      else
      {
         // notify player that they cannot cloak
         messageClient(%obj.client, 'MsgCloakingPackFailed', '\c2Jamming field prevents cloaking.');
         %obj.setImageTrigger(%slot, false);
      }
   }
   else 
   {
      // hopefully avoid some loopholes
      messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloaking available for light armors only.');
      %obj.setImageTrigger(%slot, false);
   }
}

function CloakingPackImage::onDeactivate(%data, %obj, %slot)
{
   if(%obj.reCloak !$= "")
   {   
      Cancel(%obj.reCloak);
      %obj.reCloak = "";
   }
   
   // if pack is not on then dont bother...
   if(%obj.getImageState($BackpackSlot) $= "activate")
      messageClient(%obj.client, 'MsgCloakingPackOff', '\c2Cloaking pack off.');

   %obj.setCloaked(false);
   %obj.setImageTrigger(%slot, false);
   // z0dd - ZOD, 9/29/02. Removed T2 demo code from here
   commandToClient( %obj.client, 'setCloakIconOff' );
}
// z0dd - ZOD, 5/18/03. Removed functions, created parent. Streamline.
//function CloakingPack::onPickup(%this, %obj, %shape, %amount)
//{
   // created to prevent console errors
//}

function ShapeBaseData::onForceUncloak(%this, %obj, %reason)
{
   // dummy
}

function Armor::onForceUncloak(%this, %obj, %reason)
{
   %pack = %obj.getMountedImage($BackpackSlot);
   if((%pack <= 0) || (%pack.item !$= "CloakingPack"))
      return;
   
   if(%obj.getImageState($BackpackSlot) $= "activate")
   {
      // cancel recloak thread
      if(%obj.reCloak !$= "")
      {   
         Cancel(%obj.reCloak);
         %obj.reCloak = "";
      }

      messageClient(%obj.client, 'MsgCloakingPackOff', '\c2Cloaking pack off.  Jammed.');
      %obj.setCloaked(false);
      %obj.setImageTrigger($BackpackSlot, false);
   }
}
