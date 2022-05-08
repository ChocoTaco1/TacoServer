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
// PUG Password is always on no matter what
// $Host::$PUGpasswordAlwaysOn = 1;
// Enable a center print between map changes
// $Host::MapChangeMSG = 0;
// Message Content
// $Host::MapChangeMSGContent = "<color:3cb4b4><font:Sui Generis:22>Pickup Night\n<color:3cb4b4><font:Univers:18>Saturday, March 5th\n<color:3cb4b4><font:Univers:16>Join discord for details";

package MissionTypeOptions
{

function loadMissionStage2()
{
	switch$($Host::PUGpasswordAlwaysOn)
	{
		case 0:
			if($CurrentMissionType !$= "LakRabbit")
			{
				if($Host::TournamentMode && $Host::PUGautoPassword)
					$Host::Password = $Host::PUGPassword;
				else if(!$Host::TournamentMode)
				{
					if($Host::Password)
						$Host::Password = "";
					if($LockedTeams)
						$LockedTeams = 0;
				}

				//Set server mode to SPEED
				$Host::HiVisibility = "0";
			}
			else if($CurrentMissionType $= "LakRabbit")
			{
				if($Host::Password)
					$Host::Password = "";
				if($LockedTeams)
					$LockedTeams = 0;
				if($Host::TournamentMode)
					$Host::TournamentMode = 0;

				//Set server mode to DISTANCE
				$Host::HiVisibility = "1";
			}
		case 1:
			$Host::Password = $Host::PUGPassword;
			$Host::HiVisibility = "0"; //always SPEED
	}

	//TimeLimit Always 30 minutes in Tourney Mode
	if($Host::TournamentMode)
		$Host::TimeLimit = "30";

	//Siege NoBaseRape Fix
	if($CurrentMissionType $= "Siege")
		$Host::NoBaseRapeEnabled = 0;
	else
		$Host::NoBaseRapeEnabled = 1;

	if(isActivePackage(LockedTeams) && !$LockedTeams)
		deactivatePackage(LockedTeams);

    parent::loadMissionStage2();

	if($Host::MapChangeMSG)
		centerPrintAll($Host::MapChangeMSGContent, 12, 3);
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(MissionTypeOptions))
    activatePackage(MissionTypeOptions);
