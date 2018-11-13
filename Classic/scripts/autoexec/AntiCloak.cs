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
		if( !$Host::TournamentMode && $TotalTeamPlayerCount < $Host::AntiCloakPlayerCount )	
			$InvBanList[CTF, "CloakingPack"] = true;
		//All other cases it is.
		else
			$InvBanList[CTF, "CloakingPack"] = false;	
	}
			
}

