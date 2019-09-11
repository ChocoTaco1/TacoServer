// To manage options in certain missiontypes

// Variables
//
// Turns Password on and off in Tournament mode
// Used to be auto but isnt anymore. 
// Enabled in the admin menu.
// $Host::PUGautoPassword = 1;
// The PUG password you want
// $Host::PUGPassword = "pickup";
//

package MissionTypeOptions
{

function loadMissionStage2()
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
		
	//Activate NetTourneyClient package if enabled. zCheckVar.cs
	if($Host::EnableNetTourneyClient && !isActivePackage(checkver)) //Added
		activatePackage(checkver);
		
	//echo ("PUGpassCheck");
   
    parent::loadMissionStage2();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(MissionTypeOptions))
    activatePackage(MissionTypeOptions);