// Map Repetition Checker Script
//
// To help decrease the chances of a repeated map in the map rotation by correcting repeated maps thru script
//
// Runs at the beginning of every map change
// Keeps track of maps played (Last 4)
// If any are repeating it picks a new map
//
// $EvoCachedNextMission = "RoundTheMountain";
// $EvoCachedNextMission = "Arrakis";
// $EvoCachedNextMission = "RoundTheMountainLT";
// $EvoCachedNextMission = "ArenaDomeDM";
//


$PreviousMission4back = "";
$PreviousMission3back = "";		
$PreviousMission2back = "";
$PreviousMission1back = "";

//Ran in MissionTypeOptions.cs
function MapRepetitionChecker( %game )
{
	//Debug
	//%MapRepetitionCheckerDebug = true;
		
	if(!$GetRandomMapsLoaded) //Make sure GetRandomMaps.cs is present
		return;
		
	if($EvoCachedNextMission $= "")
		return;
	
	if(!$Host::TournamentMode && $Host::EnableMapRepetitionChecker)
	{	
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
}

function MapRepetitionCheckerFindRandom()
{
	if(!$GetRandomMapsLoaded) //Make sure GetRandomMaps.cs is present
		return;

	//Backup
	$SetNextMissionRestore = $EvoCachedNextMission;
	
	//Do work
	//getRandomMap() is in GetRandomMaps.cs
	$EvoCachedNextMission = getRandomMap();
	
	//Make sure new map still complies
	if( $EvoCachedNextMission $= $PreviousMission1back || $EvoCachedNextMission $= $PreviousMission2back || 
	    $EvoCachedNextMission $= $PreviousMission3back || $EvoCachedNextMission $= $PreviousMission4back ||
		$CurrentMission $= $EvoCachedNextMission )
		MapRepetitionCheckerFindRandom();
	else
	{	
		error(formatTimeString("HH:nn:ss") SPC "Map Repetition Corrected from" SPC $SetNextMissionRestore SPC "to" SPC $EvoCachedNextMission @ "." );

		//Admin Message Only
		for(%idx = 0; %idx < ClientGroup.getCount(); %idx++) 
		{
			%cl = ClientGroup.getObject(%idx);
					  
			if(%cl.isAdmin)
				messageClient(%cl, 'MsgMapRepCorrection', '\crMap Repetition Corrected: Next mission set from %1 to %2.', $SetNextMissionRestore, $EvoCachedNextMission);
		}
	}
}