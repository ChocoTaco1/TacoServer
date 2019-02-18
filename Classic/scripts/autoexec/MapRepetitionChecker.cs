//To help decrease the chances of a repeated map in the map rotation by correcting repeated maps thru script
//$EvoCachedNextMission = "RoundTheMountain";
//

//Run in GetTeamCounts.cs
function MapRepetitionChecker( %game )
{
	//Debug
	//%MapRepetitionCheckerDebug = true;
	
	if( $CurrentMissionType $= "CTF" && !$Host::TournamentMode && $MapRepetitionCheckerRunOnce !$= 1 )
	{
		//First map
		if($PreviousMission1back $= "")
		{
			//Set vars
			$PreviousMission1back = $CurrentMission;
			
			//Debug
			if(%MapRepetitionCheckerDebug)
				echo("PM1: " @ $PreviousMission1back);
			
			return;
		}
		//1 map back
		//Second map
		else if($PreviousMission2back $= "")
		{		
			if( $PreviousMission1back $= $EvoCachedNextMission )
				MapRepetitionCheckerFindRandom();
			
			//Set vars
			$PreviousMission2back = $PreviousMission1back;
			$PreviousMission1back = $CurrentMission;
			
			//Debug
			if(%MapRepetitionCheckerDebug)	
			{
				echo("PM1: " @ $PreviousMission1back);
				echo("PM2: " @ $PreviousMission2back);
			}
			
			return;
		}
		//2 maps back
		//Third map
		else if($PreviousMission3back $= "")
		{		
			if( $PreviousMission1back $= $EvoCachedNextMission || $PreviousMission2back $= $EvoCachedNextMission )
				MapRepetitionCheckerFindRandom();
			
			//Set vars		
			$PreviousMission3back = $PreviousMission2back;
			$PreviousMission2back = $PreviousMission1back;
			$PreviousMission1back = $CurrentMission;
			
			//Debug
			if(%MapRepetitionCheckerDebug)	
			{
				echo("PM1: " @ $PreviousMission1back);
				echo("PM2: " @ $PreviousMission2back);
				echo("PM3: " @ $PreviousMission3back);
			}
			
			return;
		}
		//3 maps back
		//Forth map
		else if($PreviousMission4back $= "")
		{
			if( $PreviousMission1back $= $EvoCachedNextMission || $PreviousMission2back $= $EvoCachedNextMission || $PreviousMission3back $= $EvoCachedNextMission )
				MapRepetitionCheckerFindRandom();
			
			//Set vars
			$PreviousMission4back = $PreviousMission3back;
			$PreviousMission3back = $PreviousMission2back;		
			$PreviousMission2back = $PreviousMission1back;
			$PreviousMission1back = $CurrentMission;
			
			//Debug
			if(%MapRepetitionCheckerDebug)	
			{
				echo("PM1: " @ $PreviousMission1back);
				echo("PM2: " @ $PreviousMission2back);
				echo("PM3: " @ $PreviousMission3back);
				echo("PM4: " @ $PreviousMission4back);
			}
			
			return;
		}
		//4 maps back
		//Fifth map
		else
		{
			if( $PreviousMission1back $= $EvoCachedNextMission || $PreviousMission2back $= $EvoCachedNextMission || $PreviousMission3back $= $EvoCachedNextMission || $PreviousMission4back $= $EvoCachedNextMission )
				MapRepetitionCheckerFindRandom();
			
			//Set vars
			$PreviousMission4back = $PreviousMission3back;
			$PreviousMission3back = $PreviousMission2back;		
			$PreviousMission2back = $PreviousMission1back;
			$PreviousMission1back = $CurrentMission;
			
			//Debug
			if(%MapRepetitionCheckerDebug)	
			{
				echo("PM1: " @ $PreviousMission1back);
				echo("PM2: " @ $PreviousMission2back);
				echo("PM3: " @ $PreviousMission3back);
				echo("PM4: " @ $PreviousMission4back);
			}
			
			return;
		}
		
		$MapRepetitionCheckerRunOnce = 1;
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
	
	if($EvoCachedNextMission $= $PreviousMission1back || $EvoCachedNextMission $= $PreviousMission2back || $EvoCachedNextMission $= $PreviousMission3back || $EvoCachedNextMission $= $PreviousMission4back)
		MapRepetitionCheckerFindRandom();
	else
		messageAll('MsgNoBaseRapeNotify', '\crMap Repetition Corrected: Next mission set to %1.', $EvoCachedNextMission);
}

//Once per match
//Called in DefaultGame::gameOver(%game) in defaultGame.ovl evo
function MapRepetitionCheckerReset( %game )
{
	$MapRepetitionCheckerRunOnce = 0;
}