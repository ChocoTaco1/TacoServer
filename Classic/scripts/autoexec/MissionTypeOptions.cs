// To manage options in certain missiontypes
// called in Getcounts.cs

// Variables
// Add these to ServerPrefs
//
// Turn on Auto Password in Tournament mode
// $Host::PUGautoPassword = 1;
// The PUG password you want
// $Host::PUGPassword = "pickup";
//

package MissionTypeOptions
{

function loadMissionStage2()
{
   //Run MissionTypeOptions
   MissionTypeOptions();
   
   parent::loadMissionStage2();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(MissionTypeOptions))
    activatePackage(MissionTypeOptions);



function MissionTypeOptions()
{	
	if( $CurrentMissionType !$= "LakRabbit" ) 
	{
		if( $Host::TournamentMode && $Host::PUGautoPassword )
			
			$Host::Password = $Host::PUGPassword;
	
		else if( !$Host::TournamentMode )
		{
			$Host::Password = "";
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