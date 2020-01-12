// Map Repetition Checker Script
//
// To help decrease the chances of a repeated map in the map rotation by correcting repeated maps thru script
//
// Runs at the beginning of every map change
// Keeps track of maps played (Last [$MRC::PastMapsDepth] Maps)
// If any are repeating it picks a new map
//
// $EvoCachedNextMission = "RoundTheMountain";
// $EvoCachedNextMission = "Arrakis";
// $EvoCachedNextMission = "RoundTheMountainLT";
// $EvoCachedNextMission = "ArenaDomeDM";

// How many previous maps you want to compare TheNextCached Map to
$MRC::PastMapsDepth = 6;

for(%x = 1; %x <= $MRC::PastMapsDepth; %x++) 
{
	$MRC::PrevMap[%x] = "";
	//echo("PM" @ %x @ ": " @ $MRC::PrevMap[%x]);
}

//Ran in MissionTypeOptions.cs
function MapRepetitionChecker( %game )
{
	//Debug
	//%MapRepetitionCheckerDebug = true;
	
	if(isEventPending($MapRepetitionSchedule)) 
		cancel($MapRepetitionSchedule);
	
	//Make sure GetRandomMaps.cs is present	
	if(!$GetRandomMapsLoaded)
		return;
		
	if($EvoCachedNextMission $= "")
		return;
	
	if(!$Host::TournamentMode && $Host::EnableMapRepetitionChecker)
	{
		//Do work	
		for(%x = 1; %x <= $MRC::PastMapsDepth; %x++) 
		{
			if( $MRC::PrevMap[%x] !$= "" && $MRC::PrevMap[%x] $= $EvoCachedNextMission || $CurrentMission $= $EvoCachedNextMission )
				MapRepetitionCheckerFindRandom();
		}
		
		//Set vars	
		for(%x = $MRC::PastMapsDepth; %x >= 1; %x = %x - 1) 
		{
			if(%x > 1)
			{
				if($MRC::PrevMap[%x - 1] !$= "")
					$MRC::PrevMap[%x] = $MRC::PrevMap[%x - 1];
			}
			else if(%x $= 1)
				$MRC::PrevMap[%x] = $CurrentMission;
		}
		
		//Debug
		if(%MapRepetitionCheckerDebug)	
		{		
			for(%x = 1; %x <= $MRC::PastMapsDepth; %x++) 
			{
				if( $MRC::PrevMap[%x] !$= "" )
					echo("PM" @ %x @ ": " @ $MRC::PrevMap[%x]);
			}
		}
	}
}

function MapRepetitionCheckerFindRandom(%redone)
{
	//Make sure GetRandomMaps.cs is present
	if(!$GetRandomMapsLoaded)
		return;

	//Backup
	if(%redone $="")
		$SetNextMissionRestore = $EvoCachedNextMission;
	
	//Do work
	//getRandomMap() is in GetRandomMaps.cs
	$EvoCachedNextMission = getRandomMap();
	
	//Make sure new map still complies
	%redo = 0;
	for(%x = 1; %x <= $MRC::PastMapsDepth; %x++) 
	{
		if($MRC::PrevMap[%x] !$= "" && $MRC::PrevMap[%x] $= $EvoCachedNextMission)
			%redo = 1;
	}
	
	//Make sure its within maplimits
	%newmaplimits = $Host::MapPlayerLimits[$EvoCachedNextMission, $CurrentMissionType];
	%min = getWord(%newmaplimits,0);
	%max = getWord(%newmaplimits,1);
	if((%min > $AllPlayerCount || $AllPlayerCount > %max) && $AllPlayerCount > 2 )
		%redo = 1;

	if( %redo && %redone < 3 )
	{
		%redone++;
		MapRepetitionCheckerFindRandom(%redone);
	}
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