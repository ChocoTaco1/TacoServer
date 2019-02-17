// Random Set Next Mission maps
// 
//

//Map pool
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


function SetNextMapGetRandoms( %client )
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
//1-5
if( %RandomPick1 $= 1) $SetNextMissionMapSlot1 = $RandomMapPick1;
else if( %RandomPick1 $= 2) $SetNextMissionMapSlot1 = $RandomMapPick2;
else if( %RandomPick1 $= 3) $SetNextMissionMapSlot1 = $RandomMapPick3;
else if( %RandomPick1 $= 4) $SetNextMissionMapSlot1 = $RandomMapPick4;
else if( %RandomPick1 $= 5) $SetNextMissionMapSlot1 = $RandomMapPick5;
//6-10
if( %RandomPick2 $= 6) $SetNextMissionMapSlot2 = $RandomMapPick6;
else if( %RandomPick2 $= 7) $SetNextMissionMapSlot2 = $RandomMapPick7;
else if( %RandomPick2 $= 8) $SetNextMissionMapSlot2 = $RandomMapPick8;
else if( %RandomPick2 $= 9) $SetNextMissionMapSlot2 = $RandomMapPick9;
else if( %RandomPick2 $= 10) $SetNextMissionMapSlot2 = $RandomMapPick10;
//11-15
if( %RandomPick3 $= 11) $SetNextMissionMapSlot3 = $RandomMapPick11;
else if( %RandomPick3 $= 12) $SetNextMissionMapSlot3 = $RandomMapPick12;
else if( %RandomPick3 $= 13) $SetNextMissionMapSlot3 = $RandomMapPick13;
else if( %RandomPick3 $= 14) $SetNextMissionMapSlot3 = $RandomMapPick14;
else if( %RandomPick3 $= 15) $SetNextMissionMapSlot3 = $RandomMapPick15;
//16-20
if( %RandomPick4 $= 16) $SetNextMissionMapSlot4 = $RandomMapPick16;
else if( %RandomPick4 $= 17) $SetNextMissionMapSlot4 = $RandomMapPick17;
else if( %RandomPick4 $= 18) $SetNextMissionMapSlot4 = $RandomMapPick18;
else if( %RandomPick4 $= 19) $SetNextMissionMapSlot4 = $RandomMapPick19;
else if( %RandomPick4 $= 20) $SetNextMissionMapSlot4 = $RandomMapPick20;
//21-25
if( %RandomPick5 $= 21) $SetNextMissionMapSlot5 = $RandomMapPick21;
else if( %RandomPick5 $= 22) $SetNextMissionMapSlot5 = $RandomMapPick22;
else if( %RandomPick5 $= 23) $SetNextMissionMapSlot5 = $RandomMapPick23;
else if( %RandomPick5 $= 24) $SetNextMissionMapSlot5 = $RandomMapPick24;
else if( %RandomPick5 $= 25) $SetNextMissionMapSlot5 = $RandomMapPick25;
//26-30
if( %RandomPick6 $= 26) $SetNextMissionMapSlot6 = $RandomMapPick26;
else if( %RandomPick6 $= 27) $SetNextMissionMapSlot6 = $RandomMapPick27;
else if( %RandomPick6 $= 28) $SetNextMissionMapSlot6 = $RandomMapPick28;
else if( %RandomPick6 $= 29) $SetNextMissionMapSlot6 = $RandomMapPick29;
else if( %RandomPick6 $= 30) $SetNextMissionMapSlot6 = $RandomMapPick30;
//31-35
if( %RandomPick7 $= 31) $SetNextMissionMapSlot7 = $RandomMapPick31;
else if( %RandomPick7 $= 32) $SetNextMissionMapSlot7 = $RandomMapPick32;
else if( %RandomPick7 $= 33) $SetNextMissionMapSlot7 = $RandomMapPick33;
else if( %RandomPick7 $= 34) $SetNextMissionMapSlot7 = $RandomMapPick34;
else if( %RandomPick7 $= 35) $SetNextMissionMapSlot7 = $RandomMapPick35;
//36-40
if( %RandomPick8 $= 36) $SetNextMissionMapSlot8 = $RandomMapPick36;
else if( %RandomPick8 $= 37) $SetNextMissionMapSlot8 = $RandomMapPick37;
else if( %RandomPick8 $= 38) $SetNextMissionMapSlot8 = $RandomMapPick38;
else if( %RandomPick8 $= 39) $SetNextMissionMapSlot8 = $RandomMapPick39;
else if( %RandomPick8 $= 40) $SetNextMissionMapSlot8 = $RandomMapPick40;

}