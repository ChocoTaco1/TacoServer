//Amount of players needed on server for CloakPack to be banned/unbanned
//
//$Host::AntiCloakEnable = 1;
//$Host::AntiCloakPlayerCount = 6;
//
//TotalTeamCount based on how many on team, not how many on the server.

//Called in GetCounts
function ActivateAntiCloak()
{
	//CTF only
	if( $Host::AntiCloakEnable && $CurrentMissionType $= "CTF" )
	{
		//echo("TotalTeamPlayerCount " @ $TotalTeamPlayerCount);
		//echo("AntiCloakPlayerCount " @ $AntiCloakPlayerCount);
	
		if( !$Host::TournamentMode && $TotalTeamPlayerCount < $Host::AntiCloakPlayerCount )
			//If server is in Tourny mode or if the server population isnt higher than the AntiCloakPlayerCount the CloakPack is not selectable.	
			$InvBanList[CTF, "CloakingPack"] = true;
		else
		//If AntiCloakPlayerCount is lower than server population, CloakPack is enabled and Selectable.
			$InvBanList[CTF, "CloakingPack"] = false;
			
	}
			
}

