//To help decrease the chances of a repeated map in the map rotation by correcting repeated maps thru script
//
//

//Run at serverCmdMissionStartPhase3Done(%client, %seq) in server.ovl evo
function MapRepetitionChecker()
{
	if( $CurrentMissionType $= "CTF" && !$Host::TournamentMode )
	{
		//CTF Start
		if($TempMapCheckerPrevious $= "")
		$TempMapCheckerPrevious = $CurrentMission;
		//1 map back
		else if($TempMapCheckerPrevious2back $= "")
		{
			$MapCheckerPrevious = $TempMapCheckerPrevious;
		
			//Set temp vars
			$TempMapCheckerPrevious2back = $TempMapCheckerPrevious;
			$TempMapCheckerPrevious = $CurrentMission;
	
			//If the next mission equals the last map, find a new map.		
			if( $MapCheckerPrevious $= $EvoCachedNextMission )
			{
				MapRepetitionCheckerFindRandom();
			}
		}
		//2 maps back
		else if($TempMapCheckerPrevious3back $= "")
		{
			$MapCheckerPrevious = $TempMapCheckerPrevious;
			$MapCheckerPrevious2back = $TempMapCheckerPrevious2back;
		
			//Set temp vars		
			$TempMapCheckerPrevious3back = $TempMapCheckerPrevious2back;
			$TempMapCheckerPrevious2back = $TempMapCheckerPrevious;
			$TempMapCheckerPrevious = $CurrentMission;
	
			//If the next mission equals anything that has been played the last 2 maps, find a new map.		
			if( $MapCheckerPrevious $= $EvoCachedNextMission || $MapCheckerPrevious2back $= $EvoCachedNextMission )
			{
				MapRepetitionCheckerFindRandom();
			}
		}
		//3 maps back
		//This is what is run most of the time.
		else
		{
			$MapCheckerPrevious = $TempMapCheckerPrevious;
			$MapCheckerPrevious2back = $TempMapCheckerPrevious2back;
			$MapCheckerPrevious3back = $TempMapCheckerPrevious3back;
		
			//Set temp vars
			$TempMapCheckerPrevious3back = $TempMapCheckerPrevious2back;		
			$TempMapCheckerPrevious2back = $TempMapCheckerPrevious;
			$TempMapCheckerPrevious = $CurrentMission;
	
			//If the next mission equals anything that has been played the last 3 maps, find a new map.
			if( $MapCheckerPrevious $= $EvoCachedNextMission || $MapCheckerPrevious2back $= $EvoCachedNextMission || $MapCheckerPrevious3back $= $EvoCachedNextMission )
			{
				MapRepetitionCheckerFindRandom();
			}
		}
	}
}

function MapRepetitionCheckerFindRandom()
{
	SetNextMapGetRandoms( %client );
	%MapCheckerRandom = getRandom(1,6);
			
	if(%MapCheckerRandom $= 1) $EvoCachedNextMission = $SetNextMissionMapSlot1;
	else if(%MapCheckerRandom $= 2) $EvoCachedNextMission = $SetNextMissionMapSlot2;
	else if(%MapCheckerRandom $= 3) $EvoCachedNextMission = $SetNextMissionMapSlot3;
	else if(%MapCheckerRandom $= 4) $EvoCachedNextMission = $SetNextMissionMapSlot4;
	else if(%MapCheckerRandom $= 5) $EvoCachedNextMission = $SetNextMissionMapSlot5;
	else if(%MapCheckerRandom $= 6) $EvoCachedNextMission = $SetNextMissionMapSlot6;
			
	messageAll('MsgNoBaseRapeNotify', '\crMap Repetition Corrected: Next mission set to %1.', $EvoCachedNextMission);
}