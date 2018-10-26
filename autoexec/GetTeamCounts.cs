//This function is Called at:
//CreateServer(%mission, %missionType) in Server.cs

package StartTeamCounts {

//Start
function CreateServer(%mission, %missionType)
{
	//Call default function
	parent::CreateServer(%mission, %missionType);
	//Start
	//Make sure our activation variable is set
	ResetClientChangedTeams();
	//Keeps server Auto Reset off
	$Host::Dedicated = 0;
	//Call for a GetTeamCount update
	GetTeamCounts( %game, %client, %respawn );

}

};

// Prevent package from being activated if it is already
if (!isActivePackage(StartTeamCounts))
    activatePackage(StartTeamCounts);

function GetTeamCounts( %game, %client, %respawn )
{	
		if (!isActivePackage(StartTeamCounts)) {
			deactivatePackage(StartTeamCounts);
		}
		
		//Check pug password
		CheckPUGpassword();
		
		//Get teamcounts
		if($GetCountsClientTeamChange && $countdownStarted && $MatchStarted) {
			//Team Count code by Keen
			$PlayerCount[0] = 0;
			$PlayerCount[1] = 0;
			$PlayerCount[2] = 0;

		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%client = ClientGroup.getObject(%i);
    
			//if(!%client.isAIControlled())
				$PlayerCount[%client.team]++;
		}
		
		//echo ("Clientgroup " @ ClientGroup.getCount());
		//echo ("$PlayerCount[0] " @  $PlayerCount[0]);
		//echo ("$PlayerCount[1] " @  $PlayerCount[1]);
		//echo ("$PlayerCount[2] " @  $PlayerCount[2]);
		//echo ("client.team " @ %client.team);
		
		//Other variables
			//Amount of players on teams
			$TotalTeamPlayerCount = $PlayerCount[1] + $PlayerCount[2];
			//Amount of all players including observers
			$AllPlayerCount = $PlayerCount[1] + $PlayerCount[2] + $PlayerCount[0];
			
		
			//Start Base Rape Notify
			//Make sure it's CTF Mode
			if($CurrentMissionType $= "CTF") {
				PlayerNotify::AtSpawn( %game, %client, %respawn );
			}
			//Call Team Balance Notify
			//Make sure it's CTF Mode
			if($CurrentMissionType $= "CTF" && $TotalTeamPlayerCount !$= 0) {
				TeamBalanceNotify::AtSpawn( %game, %client, %respawn );
			}
			if($CurrentMissionType $= "sctf" && $TotalTeamPlayerCount !$= 0) {
				TeamBalanceNotify::AtSpawn( %game, %client, %respawn );
			}
			//AntiCloak Start
			//if($CurrentMissionType $= "CTF") {
			//ActivateAntiCloak ();
			//}
		
		$GetCountsClientTeamChange = false;
		
		}
		
		//Call itself again. Every 5 seconds.
		schedule(5000, 0, "GetTeamCounts");
		
}

//Run at DefaultGame::clientJoinTeam, DefaultGame::clientChangeTeam, DefaultGame::assignClientTeam in evo defaultgame.ovl
//Also Run at DefaultGame::onClientEnterObserverMode, DefaultGame::AIChangeTeam, DefaultGame::onClientLeaveGame, DefaultGame::forceObserver in evo defaultgame.ovl
//And finally GameConnection::onConnect in evo server.ovl
//Added so the bulk of GetCounts doesnt run when it doesnt need to causing unnecessary latency that may or may not have existed, but probably is good practice.
//GetCounts still runs every 5 seconds as it did, but whether or not someone has changed teams, joined obs, left, etc etc will decide whether or not the bulk of it runs.
function ResetClientChangedTeams() {
   //Let GetTeamCounts run if there is a Teamchange.
   $GetCountsClientTeamChange = true;
}
