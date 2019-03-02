//Random Set Next Mission maps
//Runs for SetNextMisssion (Random) and Map Repetition Checker
//

//This file is present
$GetRandomMapsLoaded = true;

//Map pool
//
//CTF
//
//1-5
$RandomMapPick1 = "SmallCrossing"; 		$RandomMapPick2 = "OasisIntensity"; 		$RandomMapPick3 = "Blink"; 					$RandomMapPick4 = "SmallTimeCTF"; 					$RandomMapPick5 = "ArenaDome"; 
//6-10
$RandomMapPick6 = "HighOctane"; 		$RandomMapPick7 = "S5_Damnation"; 			$RandomMapPick8 = "TWL_Feign"; 				$RandomMapPick9 = "TWL2_Skylight"; 					$RandomMapPick10 = "Prismatic"; 
//11-15
$RandomMapPick11 = "Dire"; 				$RandomMapPick12 = "TWL2_JaggedClaw"; 		$RandomMapPick13 = "TWL2_Hildebrand"; 		$RandomMapPick14 = "TWL_WilderZone"; 				$RandomMapPick15 = "TWL_Stonehenge"; 
//16-20
$RandomMapPick16 = "TWL2_Ocular"; 		$RandomMapPick17 = "S8_Opus"; 				$RandomMapPick18 = "Mirage"; 				$RandomMapPick19 = "DangerousCrossing_nef"; 		$RandomMapPick20 = "S5_Massive"; 
//21-25
$RandomMapPick21 = "TheFray"; 			$RandomMapPick22 = "RoundTheMountain"; 		$RandomMapPick23 = "S5_Mordacity"; 			$RandomMapPick24 = "TWL2_CanyonCrusadeDeluxe"; 		$RandomMapPick25 = "Signal"; 
//26-30
$RandomMapPick26 = "S8_Cardiac"; 		$RandomMapPick27 = "CirclesEdge"; 			$RandomMapPick28 = "S5_Icedance"; 			$RandomMapPick29 = "Bulwark"; 						$RandomMapPick30 = "Discord"; 
//31-35
$RandomMapPick31 = "MoonDance"; 		$RandomMapPick32 = "Rollercoaster_nef"; 	$RandomMapPick33 = "Logans_Run"; 			$RandomMapPick34 = "TWL_BeachBlitz"; 				$RandomMapPick35 = "TWL2_FrozenGlory"; 
//36-40
$RandomMapPick36 = "CircleofStones"; 	$RandomMapPick37 = "TitanV"; 				$RandomMapPick38 = "TWL_Katabatic"; 		$RandomMapPick39 = "TWL2_Magnum"; 					$RandomMapPick40 = "Fenix"; 
//
//LakRabbit
//
//1-3
$LakRandomMapPick1 = "VaubanLak"; 			$LakRandomMapPick2 = "MiniSunDried"; 		$LakRandomMapPick3 = "TitaniaLak";
//4-6
$LakRandomMapPick4 = "DesertofDeathLak";	$LakRandomMapPick5 = "Sundance";			$LakRandomMapPick6 = "SunDriedLak";
//7-9
$LakRandomMapPick7 = "SkinnyDipLak"; 		$LakRandomMapPick8 = "LushLak";				$LakRandomMapPick9 = "InfernusLak";
//10-12
$LakRandomMapPick10 = "Arrakis"; 			$LakRandomMapPick11 = "BoxLak"; 			$LakRandomMapPick12 = "TreasureIslandLak";
//13-15
$LakRandomMapPick13 = "Raindance_nefLak"; 	$LakRandomMapPick14 = "SaddiesHill"; 		$LakRandomMapPick15 = "TWL_BeachBlitzLak"; 
//16-18
$LakRandomMapPick16 = "PhasmaDustLak"; 		$LakRandomMapPick17 = "Sulfide"; 			$LakRandomMapPick18 = "HavenLak"; 
//19-21
$LakRandomMapPick19 = "Crater71Lak"; 		$LakRandomMapPick20 = "SolsDescentLak"; 	$LakRandomMapPick21 = "FrozenFuryLak"; 



function SetNextMapGetRandoms( %client )
{

	if( $CurrentMissionType $= "CTF" )
	{
		//Get random numbers
		%RandomPick1 = getRandom(1,5);
		%RandomPick2 = getRandom(6,10);
		%RandomPick3 = getRandom(11,15);
		%RandomPick4 = getRandom(16,20);
		%RandomPick5 = getRandom(21,25);
		%RandomPick6 = getRandom(26,30);
		%RandomPick7 = getRandom(31,35);
		%RandomPick8 = getRandom(36,40);

		//Deduction code
		$SetNextMissionMapSlot1 = $RandomMapPick[%RandomPick1];
		$SetNextMissionMapSlot2 = $RandomMapPick[%RandomPick2];
		$SetNextMissionMapSlot3 = $RandomMapPick[%RandomPick3];
		$SetNextMissionMapSlot4 = $RandomMapPick[%RandomPick4];
		$SetNextMissionMapSlot5 = $RandomMapPick[%RandomPick5];
		$SetNextMissionMapSlot6 = $RandomMapPick[%RandomPick6];
		$SetNextMissionMapSlot7 = $RandomMapPick[%RandomPick7];
		$SetNextMissionMapSlot8 = $RandomMapPick[%RandomPick8];
	}
	else if( $CurrentMissionType $= "LakRabbit" )
	{
		//Get random numbers		
		%LakRandomPick1 = getRandom(1,3);
		%LakRandomPick2 = getRandom(4,6);
		%LakRandomPick3 = getRandom(7,9);
		%LakRandomPick4 = getRandom(10,12);
		%LakRandomPick5 = getRandom(13,15);
		%LakRandomPick6 = getRandom(16,18);
		%LakRandomPick7 = getRandom(19,21);
		
		//Deduction code		
		$SetNextMissionMapSlot1 = $LakRandomMapPick[%LakRandomPick1];
		$SetNextMissionMapSlot2 = $LakRandomMapPick[%LakRandomPick2];
		$SetNextMissionMapSlot3 = $LakRandomMapPick[%LakRandomPick3];
		$SetNextMissionMapSlot4 = $LakRandomMapPick[%LakRandomPick4];
		$SetNextMissionMapSlot5 = $LakRandomMapPick[%LakRandomPick5];
		$SetNextMissionMapSlot6 = $LakRandomMapPick[%LakRandomPick6];
		$SetNextMissionMapSlot7 = $LakRandomMapPick[%LakRandomPick7];
	}

}