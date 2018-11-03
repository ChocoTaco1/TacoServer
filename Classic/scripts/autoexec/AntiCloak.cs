$AntiCloakPlayerCount = 6; // amount of players needed on server for CloakPack to be selectable

//Activates when default Inventory for player is set so player cant select it thru hotkeys or select it thru the gui.
function ActivateAntiCloak ()
{
	//echo("TotalTeamPlayerCount " @ $TotalTeamPlayerCount);
	//echo("AntiCloakPlayerCount " @ $AntiCloakPlayerCount);
	
	if(!$Host::TournamentMode && $TotalTeamPlayerCount < $AntiCloakPlayerCount)
//If server is in Tourny mode or if the server population isnt higher than the AntiCloakPlayerCount the CloakPack is not selectable.	
			$InvBanList[CTF, "CloakingPack"] = true;
	else
//If AntiCloakPlayerCount is lower than server population, CloakPack is enabled and Selectable.
			$InvBanList[CTF, "CloakingPack"] = false;
}