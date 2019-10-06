// Mission Type Options Script
//
// To manage options in certain missiontypes
//
// PUG Password
// Turns Password on and off in Tournament mode
// Enabled in the admin menu.
// If you want a password automatically when switched to tournament mode
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
			$Host::Password = "";
		
		//Set server mode to SPEED
		$Host::HiVisibility = "0";
	}
	else if( $CurrentMissionType $= "LakRabbit" ) 
	{
		$Host::Password = "";
		$Host::TournamentMode = 0;
		
		//Set server mode to DISTANCE
		$Host::HiVisibility = "1";
	}
	
	//Start MapRepetitionChecker
	schedule(20000, 0, "MapRepetitionChecker", %game);
   
    parent::loadMissionStage2();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(MissionTypeOptions))
    activatePackage(MissionTypeOptions);