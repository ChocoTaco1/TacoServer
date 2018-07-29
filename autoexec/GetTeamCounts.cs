//This function is Called at:
//CreateServer(%mission, %missionType) in Server.cs

package StartTeamCounts {

//Start
function CreateServer(%mission, %missionType)
{
	//Call default function
	parent::CreateServer(%mission, %missionType);
	//Start
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
		
		//echo ("Clientgroup " @ ClientGroup.getCount());
		//echo ("$PlayerCount[0] " @  $PlayerCount[0]);
		//echo ("$PlayerCount[1] " @  $PlayerCount[1]);
		//echo ("$PlayerCount[2] " @  $PlayerCount[2]);
		//echo ("client.team " @ %client.team);
		
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
		
		//Other variables
			//Amount of players on teams
			$TotalTeamPlayerCount = $PlayerCount[1] + $PlayerCount[2];
			//Amount of all players including observers
			$AllPlayerCount = $PlayerCount[1] + $PlayerCount[2] + $PlayerCount[0];
			
		//Call Team Balance Notify
		//Make sure it's CTF Mode
		if($CurrentMissionType $= "CTF" && $TotalTeamPlayerCount !$= 0 && $countdownStarted $= true && $MatchStarted $= true) {
		TeamBalanceNotify::AtSpawn( %game, %client, %respawn );
		}
		if($CurrentMissionType $= "sctf" && $TotalTeamPlayerCount !$= 0 && $countdownStarted $= true && $MatchStarted $= true) {
		TeamBalanceNotify::AtSpawn( %game, %client, %respawn );
		}
		//Start Base Rape Notify
		//Make sure it's CTF Mode
		if($CurrentMissionType $= "CTF" && $countdownStarted $= true && $MatchStarted $= true) {
		PlayerNotify::AtSpawn( %game, %client, %respawn );
		}
		//AntiCloak Start
		//if($CurrentMissionType $= "CTF") {
		//ActivateAntiCloak ();
		//}
 
		//Call itself again. Every 5 seconds.
		schedule(5000, 0, "GetTeamCounts");
		
}

//For instant Calls for an update
//function QuickTeamCounts( %game, %client, %respawn )
//{	
		//Team Count code by Keen
		//$PlayerCount[0] = 0;
		//$PlayerCount[1] = 0;
		//$PlayerCount[2] = 0;

		//for(%i = 0; %i < ClientGroup.getCount(); %i++)
		//{
			//%client = ClientGroup.getObject(%i);
    
			//if(!%client.isAIControlled())
				//$PlayerCount[%client.team]++;
		//}
//}