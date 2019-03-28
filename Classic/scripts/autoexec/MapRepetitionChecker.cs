//To help decrease the chances of a repeated map in the map rotation by correcting repeated maps thru script
//$EvoCachedNextMission = "RoundTheMountain";
//$EvoCachedNextMission = "Arrakis";
//$EvoCachedNextMission = "RoundTheMountainLT";
//$EvoCachedNextMission = "ArenaDomeDM";
//
//
$PreviousMission4back = "";
$PreviousMission3back = "";		
$PreviousMission2back = "";
$PreviousMission1back = "";

//Run in GetTeamCounts.cs
function MapRepetitionChecker( %game )
{
	//Debug
	//%MapRepetitionCheckerDebug = true;
	
	if(!$GetRandomMapsLoaded) //Make sure GetRandomMaps.cs is present
		return;
	
	if($EvoCachedNextMission $= "")
		return;
	
	if(!$Host::TournamentMode && $MapRepetitionCheckerRunOnce !$= 1 )
	{	
		//Backup
		$SetNextMissionRestore = $EvoCachedNextMission;
		
		//Do work
		if( $PreviousMission1back $= $EvoCachedNextMission || $PreviousMission2back $= $EvoCachedNextMission || 
		    $PreviousMission3back $= $EvoCachedNextMission || $PreviousMission4back $= $EvoCachedNextMission ||
			$CurrentMission $= $EvoCachedNextMission )
			MapRepetitionCheckerFindRandom();
		
		//Set vars	
		if($PreviousMission3back !$= "") $PreviousMission4back = $PreviousMission3back;
		if($PreviousMission2back !$= "") $PreviousMission3back = $PreviousMission2back;		
		if($PreviousMission1back !$= "") $PreviousMission2back = $PreviousMission1back;
										 $PreviousMission1back = $CurrentMission;
			
		//Debug
		if(%MapRepetitionCheckerDebug)	
		{
			if($PreviousMission1back !$= "") echo("PM1: " @ $PreviousMission1back);
			if($PreviousMission2back !$= "") echo("PM2: " @ $PreviousMission2back);
			if($PreviousMission3back !$= "") echo("PM3: " @ $PreviousMission3back);
			if($PreviousMission4back !$= "") echo("PM4: " @ $PreviousMission4back);
		}
	}
	$MapRepetitionCheckerRunOnce = 1;
}

function MapRepetitionCheckerFindRandom()
{
	if($GetRandomMapsLoaded) //Make sure GetRandomMaps.cs is present
		SetNextMapGetRandoms( %client ); //Get Random Set Next Mission maps	
	else
		return;
	
	if( $CurrentMissionType $= "Deathmatch" )
		%MapCheckerRandom = getRandom(1,8);
	else
		%MapCheckerRandom = getRandom(1,6);
	
	$EvoCachedNextMission = $SetNextMissionMapSlot[%MapCheckerRandom];
	
	//Do work
	if( $EvoCachedNextMission $= $PreviousMission1back || $EvoCachedNextMission $= $PreviousMission2back || 
	    $EvoCachedNextMission $= $PreviousMission3back || $EvoCachedNextMission $= $PreviousMission4back ||
		$CurrentMission $= $EvoCachedNextMission )
		MapRepetitionCheckerFindRandom();
	else
	{	
		error(formatTimeString("HH:nn:ss") SPC "Map Repetition Corrected from" SPC $SetNextMissionRestore SPC "to" SPC $EvoCachedNextMission @ "." );
		messageAll('MsgNoBaseRapeNotify', '\crMap Repetition Corrected: Next mission set to %1.', $EvoCachedNextMission);
	}
}

//Once per match
//Called in DefaultGame::gameOver(%game) in defaultGame.ovl evo
function MapRepetitionCheckerReset( %game )
{
	$MapRepetitionCheckerRunOnce = 0;
}