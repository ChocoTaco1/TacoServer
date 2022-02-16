// AntiPack Script
//
// Amount of players needed on server for (Cloak and Shield) Pack to be banned/unbanned
// This is useful for low numbers
//
// Enable/Disable the feature
// $Host::AntiPackEnable = 1;
// When you would like for it to deactivate
// $Host::AntiPackPlayerCount = 6;
//

// Choose which packs to limit
$AntiPackIncludeCloak = 1;
$AntiPackIncludeShield = 0;


// Called in GetCounts.cs
function CheckAntiPack( %game )
{
	//CTF only
	if( $Host::AntiPackEnable )
	{
		//echo("TotalTeamPlayerCount " @ $TotalTeamPlayerCount);
		//echo("AntiPackPlayerCount " @ $AntiPackPlayerCount);
	
		if( $TotalTeamPlayerCount < $Host::AntiPackPlayerCount )
		{
			if( $AntiPackStatus !$= "ACTIVEON" )
			$AntiPackStatus = "ON";
		}
		//Off
		else
		{
			if( $AntiPackStatus !$= "ACTIVEOFF" )
				$AntiPackStatus = "OFF";
		}
	}
	//All other cases outside of CTF
	else
	{
		if( $AntiPackStatus !$= "ACTIVEOFF" )
			$AntiPackStatus = "OFF";
	}

	switch$($AntiPackStatus)
	{
		case ON:
			if($AntiPackIncludeCloak)
			{
				$InvBanList[CTF, "CloakingPack"] = 1;
				if(!isActivePackage(AntiPackCloak))
					activatePackage(AntiPackCloak);
			}
			if($AntiPackIncludeShield)
			{
				$InvBanList[CTF, "ShieldPack"] = 1;
				if(!isActivePackage(AntiPackShield))
					activatePackage(AntiPackShield);
			}
			$AntiPackStatus = "ACTIVEON";
		case OFF:
			$InvBanList[CTF, "CloakingPack"] = 0;
			$InvBanList[CTF, "ShieldPack"] = 0;
			if(isActivePackage(AntiPackCloak))
				deactivatePackage(AntiPackCloak);
			if(isActivePackage(AntiPackShield))
				deactivatePackage(AntiPackShield);
			$AntiPackStatus = "ACTIVEOFF";
		case ACTIVEON:				
			//Do Nothing
		case ACTIVEOFF:				
			//Do Nothing
	}
}

// So if the player is able to get said pack, he cant use it
package AntiPackCloak
{

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
			if(%obj.getImageState($BackpackSlot) $= "activate")
			{
			// cancel recloak thread
			if(%obj.reCloak !$= "")
			{   
				Cancel(%obj.reCloak);
				%obj.reCloak = "";
			}

			messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloakpack is disabled until %1 players.', $Host::AntiPackPlayerCount );
			%obj.setCloaked(false);
			%obj.setImageTrigger($BackpackSlot, false);
		}
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

};

package AntiPackShield
{

function ShieldPackImage::onActivate(%data, %obj, %slot)
{
	messageClient(%obj.client, 'MsgShieldPackInvalid', '\c2Shieldpack is disabled until %1 players.', $Host::AntiPackPlayerCount );
	%obj.setImageTrigger(%slot,false);
    %obj.isShielded = "";
}

function ShieldPackImage::onDeactivate(%data, %obj, %slot)
{
	//Nothing
}

};

package AntiPack
{

//Reset Everything
function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);

	if( $Host::AntiPackEnable )
	{
		if($InvBanList[CTF, "CloakingPack"])
			$InvBanList[CTF, "CloakingPack"] = 0;
		if(isActivePackage(AntiPackCloak))
			deactivatePackage(AntiPackCloak);

		if($InvBanList[CTF, "ShieldPack"])
			$InvBanList[CTF, "ShieldPack"] = 0;
		if(isActivePackage(AntiPackShield))
			deactivatePackage(AntiPackShield);

		$AntiPackStatus = "OFF";
	}
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(AntiPack))
    activatePackage(AntiPack);


