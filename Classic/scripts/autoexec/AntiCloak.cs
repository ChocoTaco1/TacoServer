//Amount of players needed on server for CloakPack to be banned/unbanned
//
//$Host::AntiCloakEnable = 1;
//$Host::AntiCloakPlayerCount = 6;

//Called in GetCounts.cs
function ActivateAntiCloak()
{
	//CTF only
	if( $Host::AntiCloakEnable && $CurrentMissionType $= "CTF" )
	{
		//echo("TotalTeamPlayerCount " @ $TotalTeamPlayerCount);
		//echo("AntiCloakPlayerCount " @ $AntiCloakPlayerCount);
	
		//If server is in Tourny mode and the team population is lower than the AntiCloakPlayerCount cloak is not selectable.
		if( !$Host::TournamentMode && $TotalTeamPlayerCount < $Host::AntiCloakPlayerCount && $CloakpackRunOnce !$= 0)
		{
			$InvBanList[CTF, "CloakingPack"] = true;
			
			if(!isActivePackage(DisableCloakPack))
				activatePackage(DisableCloakPack);
			
			$CloakpackRunOnce = 0;
		}
		//All other cases it is.
		else if( $CloakpackRunOnce !$= 1 )
		{
			$InvBanList[CTF, "CloakingPack"] = false;
			
			if(isActivePackage(DisableCloakPack))
				deactivatePackage(DisableCloakPack);
			
			$CloakpackRunOnce = 1;
		}
	}		
}

//So if the player is able to get a cloakpack, he cant use it
package DisableCloakPack
{

function CloakingPackImage::onActivate(%data, %obj, %slot)
{
   if(%obj.client.armor $= "Light") 
   {
      if(%obj.canCloak() $= "true")
      {
         messageClient(%obj.client, 'MsgCloakingPackOn', '\c2Cloakpack is disabled.');
      }
      else
      {
		//Nothing
      }
   }
   else 
   {
      messageClient(%obj.client, 'MsgCloakingPackInvalid', '\c2Cloaking available for light armors only.');

   }
}

};


