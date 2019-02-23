// To activate a password in certain gamemodes
// called in Getcounts.cs
// and also other options like distance and speed
// turn tournament mode off when switched to lak

// Variables
// Add these to ServerPrefs
//
// Turn on Auto Password at a player limit
// $Host::PUGautoPassword = 1;
// What value does Auto Password turn on
// $Host::PUGautoPasswordLimit = 10;
// The PUG password you want
// $Host::PUGPassword = "pickup";
//

function CheckPUGpassword()
{	
	//Only run before mission start and countdown start
	if( !$MatchStarted && !$countdownStarted )
	{
		if( $CurrentMissionType !$= "LakRabbit" ) 
		{
			if( $Host::TournamentMode )
				$Host::Password = $Host::PUGPassword;
		
			else if( !$Host::TournamentMode )
			{
				$Host::Password = "";
			
				//if 10 players are already on the server when the map changes
				if( $TotalTeamPlayerCount >= $Host::PUGautoPasswordLimit && $Host::PUGautoPassword )
					$Host::Password = $Host::PUGPassword;
			}
		
			$Host::HiVisibility = "0";
		}
		else if( $CurrentMissionType $= "LakRabbit" ) 
		{
			$Host::Password = "";
			$Host::TournamentMode = 0;
		
			$Host::HiVisibility = "1";
		}
		//For zCheckVar.cs TournyNetClient
		if( $CurrentMissionType !$= "CTF" && $CheckVerObserverRunOnce )
			CheckVerObserverReset();
		
		//echo ("PUGpassCheck");
	}
	
	//If someone changes teams
	else if( $Host::PUGautoPassword && $CurrentMissionType !$= "LakRabbit" && !$Host::TournamentMode )
	{
		if( $TotalTeamPlayerCount < $Host::PUGautoPasswordLimit )
			$Host::Password = "";
		else if( $TotalTeamPlayerCount >= $Host::PUGautoPasswordLimit )
			$Host::Password = $Host::PUGPassword;
			
		//echo ("PUGpassCheckTeamchange");
	}
}