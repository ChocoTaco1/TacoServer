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

//exec("scripts/autoexec/missiontypeoptions.cs");
package MissionTypeOptions
{

function loadMissionStage2()
{
	//Disable Tournament Mode for Lak
	if($CurrentMissionType $= "Lakrabbit" || $CurrentMissionType $= "DM" || $CurrentMissionType $= "PracticeCTF")
	{
		if($Host::TournamentMode)
			$Host::TournamentMode = 0;
	}

	//PUGpassword
	if($Host::PUGpasswordAlwaysOn) //ON
		$Host::Password = $Host::PUGPassword;

	//Set Visibility
	if($CurrentMissionType $= "Lakrabbit")
	{
		//Set server mode to DISTANCE
		$Host::HiVisibility = 1; //Lakrabbit
	}
	else //Set server mode to SPEED for CTF/SCTF/Anything Else
		$Host::HiVisibility = 0;

	//Tournament Mode specifics
	if($Host::TournamentMode)
		$Host::TimeLimit = 30; //TimeLimit Always 30 minutes in Tourney Mode
	else
	{
		//Disable if active
		if($LockedTeams)
			$LockedTeams = 0;
		if(isActivePackage(LockedTeams) && !$LockedTeams)
			deactivatePackage(LockedTeams);

		//Disable if active
		if($Host::Password !$= "" && !$Host::PUGpasswordAlwaysOn) //No Password
			$Host::Password = "";

		//Disable if active
		if($RestrictedVoting)
			$RestrictedVoting = 0;
		if($Host::AllowAdmin2Admin)
			$Host::AllowAdmin2Admin = 0;
	}

	//Siege NoBaseRape Fix
	if($CurrentMissionType $= "Siege")
		$Host::NoBaseRapeEnabled = 0;
	else
		$Host::NoBaseRapeEnabled = 1;

    parent::loadMissionStage2();

	//Map Change Center Print. Used to advertise upcoming events
	if($Host::MapChangeMSG)
		centerPrintAll($Host::MapChangeMSGContent, 12, 3);

	//Set random seed
	setRandomSeed(getrandom(1,1000));
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(MissionTypeOptions))
    activatePackage(MissionTypeOptions);
