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

// Called in GetCounts.cs
function CheckAntiPack( %game )
{
	//CTF only
	if( $Host::AntiPackEnable && $CurrentMissionType $= "CTF" && !$Host::TournamentMode )
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
			$InvBanList[CTF, "CloakingPack"] = 1;
			$InvBanList[CTF, "ShieldPack"] = 1;
			if(!isActivePackage(AntiPack))
				activatePackage(AntiPack);
			$AntiPackStatus = "ACTIVEON";
		case OFF:
			$InvBanList[CTF, "CloakingPack"] = 0;
			$InvBanList[CTF, "ShieldPack"] = 0;
			if(isActivePackage(AntiPack))
				deactivatePackage(AntiPack);
			$AntiPackStatus = "ACTIVEOFF";
		case ACTIVEON:				
			//Do Nothing
		case ACTIVEOFF:				
			//Do Nothing
	}
}

// So if the player is able to get said pack, he cant use it
package AntiPack
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


