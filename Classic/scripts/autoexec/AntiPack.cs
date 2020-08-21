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
   if(%obj.client.armor $= "Light") 
   {
      if(%obj.canCloak() $= "true")
         messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloakpack is disabled until %1 players.', $Host::AntiPackPlayerCount );
   }
   else
   {
      messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloaking available for light armors only.');
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


