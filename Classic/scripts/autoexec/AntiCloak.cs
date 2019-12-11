// AntiCloak Script
//
// Amount of players needed on server for CloakPack to be banned/unbanned
// This is useful for low numbers
//
// Enable/Disable the feature
// $Host::AntiCloakEnable = 1;
// When you would like for it to deactivate
// $Host::AntiCloakPlayerCount = 6;
//

// Called in GetCounts.cs
function CheckAntiCloak( %game )
{
	//CTF only
	if( $Host::AntiCloakEnable && $CurrentMissionType $= "CTF" && !$Host::TournamentMode )
	{
		//echo("TotalTeamPlayerCount " @ $TotalTeamPlayerCount);
		//echo("AntiCloakPlayerCount " @ $AntiCloakPlayerCount);
	
		if( $TotalTeamPlayerCount < $Host::AntiCloakPlayerCount )
		{
			if( $AntiCloakStatus !$= "ACTIVEON" )
			$AntiCloakStatus = "ON";
		}
		//Off
		else
		{
			if( $AntiCloakStatus !$= "ACTIVEOFF" )
				$AntiCloakStatus = "OFF";
		}
	}
	//All other cases outside of CTF
	else
	{
		if( $AntiCloakStatus !$= "ACTIVEOFF" )
			$AntiCloakStatus = "OFF";
	}

	switch$($AntiCloakStatus)
	{
		case ON:
			$InvBanList[CTF, "CloakingPack"] = 1;
			if(!isActivePackage(DisableCloakPack))
				activatePackage(DisableCloakPack);
			$AntiCloakStatus = "ACTIVEON";
		case OFF:
			$InvBanList[CTF, "CloakingPack"] = 0;
			if(isActivePackage(DisableCloakPack))
				deactivatePackage(DisableCloakPack);
			$AntiCloakStatus = "ACTIVEOFF";
		case ACTIVEON:				
			//Do Nothing
		case ACTIVEOFF:				
			//Do Nothing
	}
}

// So if the player is able to get a cloakpack, he cant use it
package DisableCloakPack
{

function CloakingPackImage::onActivate(%data, %obj, %slot)
{
   if(%obj.client.armor $= "Light") 
   {
      if(%obj.canCloak() $= "true")
         messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloakpack is disabled until %1 players.', $Host::AntiCloakPlayerCount );
   }
   else
   {
      messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloaking available for light armors only.');
   }
}

};


