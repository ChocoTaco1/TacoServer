//To activate a password in certain gamemodes
//called in Getcounts.cs
function CheckPUGpassword()
{	
	if( $CurrentMissionType !$= "LakRabbit" ) {
		
		if( $Host::TournamentMode ) 
			$Host::Password = "pickup";
		else if( ($AllPlayerCount < 10) && !$Host::TournamentMode )
			$Host::Password = "";
		
		if($AllPlayerCount >= 10) {
			$Host::Password = "pickup";
		}
	}
	else if( $CurrentMissionType $= "LakRabbit" ) {
			$Host::Password = "";
			$Host::TournamentMode = 0;
	}
}