//To help decrease the chances of a repeated map in the map rotation by correcting repeated maps thru script
//$EvoCachedNextMission = "RoundTheMountain";
//$EvoCachedNextMission = "Arrakis";
//
//$GetRandomMapsLoaded makes sure GetRandomMaps.cs is present
//$SNMPresetsLoaded makes sure if GetRandomMaps.cs isnt present presets are loaded.
//Presets are in defaultgame.ovl evo
//
//Set up to not run if GetRandomMaps.cs not loaded or Presets not loaded.
//Server can crash if $EvoCachedNextMission = "";
//

//Run in GetTeamCounts.cs
function MapRepetitionChecker( %game )
{
	//Debug
	//%MapRepetitionCheckerDebug = true;
	
	if( ($CurrentMissionType $= "CTF" || $CurrentMissionType $= "LakRabbit" ) && !$Host::TournamentMode && $MapRepetitionCheckerRunOnce !$= 1 && ($GetRandomMapsLoaded || $SNMPresetsLoaded))
	{
		if( $PreviousMission1back $= $EvoCachedNextMission || $PreviousMission2back $= $EvoCachedNextMission || $PreviousMission3back $= $EvoCachedNextMission || $PreviousMission4back $= $EvoCachedNextMission )
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
	
	if( $CurrentMissionType $= "CTF" )
	{
		%MapCheckerRandom = getRandom(1,6);
		$EvoCachedNextMission = $SetNextMissionMapSlot[%MapCheckerRandom];
	}
	else if( $CurrentMissionType $= "LakRabbit" && $GetRandomMapsLoaded )
	{
		%MapCheckerRandom = getRandom(1,3);
		$EvoCachedNextMission = $SetNextMissionMapSlot[%MapCheckerRandom];
	}
	else
		return;
	
	if($EvoCachedNextMission $= $PreviousMission1back || $EvoCachedNextMission $= $PreviousMission2back || $EvoCachedNextMission $= $PreviousMission3back || $EvoCachedNextMission $= $PreviousMission4back)
		MapRepetitionCheckerFindRandom();
	else
	{	
		error(formatTimeString("HH:nn:ss") SPC "Map Repetition Corrected.");
		messageAll('MsgNoBaseRapeNotify', '\crMap Repetition Corrected: Next mission set to %1.', $EvoCachedNextMission);
	}
}

//Once per match
//Called in DefaultGame::gameOver(%game) in defaultGame.ovl evo
function MapRepetitionCheckerReset( %game )
{
	$MapRepetitionCheckerRunOnce = 0;
}