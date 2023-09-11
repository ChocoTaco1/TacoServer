//  __  __               _____       _        _   _
// |  \/  |             |  __ \     | |      | | (_)
// | \  / | __ _ _ __   | |__) |___ | |_ __ _| |_ _  ___  _ __
// | |\/| |/ _` | '_ \  |  _  // _ \| __/ _` | __| |/ _ \| '_ \
// | |  | | (_| | |_) | | | \ \ (_) | || (_| | |_| | (_) | | | |
// |_|  |_|\__,_| .__/  |_|  \_\___/ \__\__,_|\__|_|\___/|_| |_|
//              | |
//              |_|
//
// Use of this file
// To add a new map follow this rules:
// 1. Find the file name, ending in .mis (ie BeggarsRun.mis)
// 2. Add a line with this syntact: addRotationMap(filename, gametype, %voteable, %cycle, %min. %max);
//
//addRotationMap("MAP_NAME","MAP_MODE",VOTEABLE,IN-ROTATION,MINIMUM_PLAYERS,MAXIMUM_PLAYERS);
//
// Example: addRotationMap("BeggarsRun","CTF",1,1,-1,64);
//                                Beggar's Run will be in the mapRotation. It's in the cycle and can be voted. The map has no minimum player count requirement, and a 64 player max.
//
// Example: addRotationMap("BeggarsRun","CTF",1,0,10,20);
//                                Beggar's Run will be in the mapRotation. It's not in the cycle, but can be voted. The map has a minimum player count requirement of 10, and 20 players is the maximum to be picked.
//
// Example: addRotationMap("BeggarsRun","CTF",0,0,18,-1);
//                                Beggar's Run won't be in the mapRotation. It's not in the cycle and can't be voted. The map has a minimum player count requirement of 18, and has no maximum for players.
//
//
//addRotationMap("Casern_Cavite","Bounty",1,1,0,64);
//addRotationMap("Sirocco","CnH",1,1,0,64);
//addRotationMap("aabaaGH","CTF",1,0,0,64);
//addRotationMap("AcidRain","DnD",1,1,0,64);
//addRotationMap("AgentsOfFortune","Hunters",1,1,0,64);
//addRotationMap("SunDriedLak1","LakRabbit",1,1,0,64);
//addRotationMap("Arrakis","Rabbit",1,1,0,64);
//addRotationMap("VulcansHammer","Siege",1,1,0,64);
//addRotationMap("AgentsOfFortune","TeamHunters",1,1,0,64);
//


//   _____ _______ ______
//  / ____|__   __|  ____|
// | |       | |  | |__
// | |       | |  |  __|
// | |____   | |  | |
//  \_____|  |_|  |_|
/////////////////////////////////////////////////////////////////////

//Small Maps
/////////////////////////////////////////////////////////////////////

//addRotationMap("SmallCrossing",				"CTF",1,1,-1,16); //Using DMP Version
addRotationMap("TWL2_CanyonCrusadeDeluxe",		"CTF",1,1,-1,24);
addRotationMap("RoundTheMountain",				"CTF",1,1,-1,18);
addRotationMap("oasisintensity",				"CTF",1,1,-1,10);
addRotationMap("Minotaur",						"CTF",1,0,-1,18);
addRotationMap("SmallMelee",					"CTF",1,0,-1,12);
addRotationMap("Machineeggs",					"CTF",1,1,-1,10);
addRotationMap("Mac_FlagArena",					"CTF",1,0,-1,14);
addRotationMap("SmallTimeCTF",					"CTF",1,0,-1,14);
addRotationMap("TWL2_Hildebrand",				"CTF",1,1,-1,18);
addRotationMap("ArenaDome",						"CTF",1,0,-1,12);
addRotationMap("Firestorm",						"CTF",1,0,-1,12);
addRotationMap("Bulwark",						"CTF",1,1,-1,12);
addRotationMap("Blink",							"CTF",1,0,-1,10);
addRotationMap("Discord",						"CTF",1,1,6,16);
addRotationMap("TWL2_MidnightMayhemDeluxe",		"CTF",1,0,8,12);
addRotationMap("Sentry",						"CTF",1,1,-1,14);
addRotationMap("TWL2_Skylight",					"CTF",1,1,-1,10);
addRotationMap("Exhumed",						"CTF",1,0,-1,12);
addRotationMap("FrozenHope",					"CTF",1,1,8,12);
addRotationMap("ClusterUnFuct",					"CTF",1,0,8,12);
//addRotationMap("Raspian",						"CTF",1,0,8,12);
addRotationMap("Zilch",							"CTF",1,1,6,12);
addRotationMap("SuperHappyBouncyFunTime",		"CTF",1,0,-1,12);
addRotationMap("SuperiorWaterworks",			"CTF",1,0,-1,12);
//addRotationMap("TWL2_FrozenHope",				"CTF",1,0,8,20);
//addRotationMap("Island",						"CTF",1,1,-1,12);
//addRotationMap("TitForTat",					"CTF",1,1,-1,12);
//addRotationMap("Cinerarium",					"CTF",1,0,-1,12); //Using DMP Version
//addRotationMap("StarFallCTF",					"CTF",1,0,6,12); //Using DMP Version
//addRotationMap("TWL_OsIris",					"CTF",1,0,8,18);
//addRotationMap("FrostPipe",					"CTF",1,0,-1,12);

//Medium Maps
/////////////////////////////////////////////////////////////////////

addRotationMap("HighOctane",					"CTF",1,0,10,24);
addRotationMap("S5_Mordacity",					"CTF",1,1,10,24);
addRotationMap("S5_Damnation",					"CTF",1,1,12,20);
addRotationMap("TWL2_JaggedClaw",				"CTF",1,1,14,28);
addRotationMap("S5_Massive",					"CTF",1,1,12,32);
addRotationMap("TWL_Stonehenge",				"CTF",1,1,8,20);
addRotationMap("TWL_Feign",						"CTF",1,1,10,28);
addRotationMap("TheFray",						"CTF",1,0,10,20);
//addRotationMap("DangerousCrossing_nef",		"CTF",1,1,8,20);
addRotationMap("TWL_DangerousCrossing",			"CTF",1,0,10,28);
addRotationMap("TWL2_Ocular",					"CTF",1,1,20,-1);
addRotationMap("Dire",							"CTF",1,0,10,16);
addRotationMap("berlard",						"CTF",1,1,12,24);
addRotationMap("S8_Opus",						"CTF",1,1,12,24);
addRotationMap("TWL_BeggarsRun",				"CTF",1,0,18,24);
addRotationMap("Headstone",						"CTF",1,1,8,24);
addRotationMap("Signal",						"CTF",1,0,8,24);
addRotationMap("S5_Woodymyrk",					"CTF",1,1,10,32);
addRotationMap("TenebrousCTF",					"CTF",1,0,8,12);
addRotationMap("Prismatic",						"CTF",1,0,16,32);
addRotationMap("TWL_WilderZone",				"CTF",1,1,12,32);
addRotationMap("S5_Mimicry",					"CTF",1,0,12,32);
addRotationMap("TWL_Snowblind",		    		"CTF",1,1,20,-1);
addRotationMap("ShortFall",						"CTF",1,0,18,28);
addRotationMap("IceRidge_nef",			    	"CTF",1,0,12,32);
//addRotationMap("Disjointed",					"CTF",1,0,8,20);
addRotationMap("TWL2_MuddySwamp",				"CTF",1,0,10,24);
addRotationMap("HighAnxiety",					"CTF",1,1,-1,12);
addRotationMap("S5_Centaur",					"CTF",1,1,-1,12);
addRotationMap("S8_Cardiac",					"CTF",1,1,18,-1);
addRotationMap("CirclesEdge",					"CTF",1,0,10,24);
addRotationMap("S5_Icedance",					"CTF",1,1,10,28);
addRotationMap("Surreal",						"CTF",1,0,24,-1);
addRotationMap("TWL2_Drifts",					"CTF",1,0,14,24);
//addRotationMap("TWL2_Drorck",					"CTF",1,0,12,24);
addRotationMap("S5_HawkingHeat",				"CTF",1,0,16,28);
addRotationMap("TWL2_Celerity",					"CTF",1,1,12,32);
addRotationMap("Draco",							"CTF",1,0,8,24);
//addRotationMap("Pariah",						"CTF",1,0,8,24); //Using DMP Version
//addRotationMap("Mirage",						"CTF",1,0,8,20);
addRotationMap("Norty",							"CTF",1,1,18,32);

//Voteable but Not in Rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("TWL2_IceDagger",				"CTF",1,1,10,20);
addRotationMap("TWL_Titan",					    "CTF",1,0,16,32);
addRotationMap("Burstside",					    "CTF",1,0,10,20);
addRotationMap("Infernus",			    		"CTF",1,0,18,26);
addRotationMap("Confusco",						"CTF",1,0,10,20);
//addRotationMap("Snowcone",					"CTF",1,0,10,20);
addRotationMap("S5_Drache",						"CTF",1,0,16,28);
//addRotationMap("JadeValley",					"CTF",1,0,10,20);
addRotationMap("S5_Sherman",					"CTF",1,1,24,-1);
addRotationMap("S5_Silenus",					"CTF",1,0,8,20);
addRotationMap("S5_Reynard",					"CTF",1,0,10,20);
//addRotationMap("Coppersky",			   		"CTF",1,0,8,20);
//addRotationMap("TWL2_Crevice",				"CTF",1,0,10,20);
addRotationMap("TWL_SubZero",					"CTF",1,0,16,32);
addRotationMap("Fallout",						"CTF",1,1,16,24);
//addRotationMap("TheClocktower",				"CTF",1,0,10,20);
//addRotationMap("SoylentGreen",				"CTF",1,0,10,20);
//addRotationMap("Ramparts",					"CTF",1,0,10,20);
//addRotationMap("Blastside_nef",				"CTF",1,0,10,24);
//addRotationMap("NatureMagic",					"CTF",1,0,10,20);
//addRotationMap("TWL_Damnation",			   	"CTF",1,0,10,20);
addRotationMap("TWL_DeadlyBirdsSong",			"CTF",1,0,24,32);
addRotationMap("Vauban",						"CTF",1,0,18,-1);
addRotationMap("TWL_Pandemonium",				"CTF",1,0,16,24);
addRotationMap("TWL_WhiteDwarf",				"CTF",1,0,18,24);
//addRotationMap("Cinereous",					"CTF",1,0,12,20);

//Vehicle Maps
/////////////////////////////////////////////////////////////////////

addRotationMap("Logans_Run",					"CTF",1,1,14,28);
addRotationMap("Rollercoaster_nef",				"CTF",1,1,8,-1);
addRotationMap("HostileLoch",					"CTF",1,1,8,12);
addRotationMap("TWL_BeachBlitz",				"CTF",1,1,18,28);
addRotationMap("TWL2_Magnum",					"CTF",1,1,12,-1);
//addRotationMap("MoonDance",					"CTF",1,0,12,28); //Using DMP Version
addRotationMap("Raindance_nef",					"CTF",1,1,12,28);
addRotationMap("TWL_Magmatic",					"CTF",1,0,14,28);
addRotationMap("TWL2_FrozenGlory",				"CTF",1,1,12,24);
addRotationMap("LandingParty",					"CTF",1,0,16,20);
addRotationMap("TWL_Crossfire",					"CTF",1,1,18,28);
//addRotationMap("TitanV",						"CTF",1,0,12,28);
//addRotationMap("Rollercoaster_nef_FZ",		"CTF",1,1,8,28);
//addRotationMap("The_Calm",					"CTF",1,0,12,24);
addRotationMap("TWL2_Dissention",				"CTF",1,0,16,32);

//Vehicle Maps: Voteable, But Not in Rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("SubZeroV",			    		"CTF",1,0,12,24);
addRotationMap("S8_Geothermal",		    		"CTF",1,0,12,24);
addRotationMap("Lakefront",			 	  	    "CTF",1,0,16,24);
addRotationMap("ShockRidge",					"CTF",1,0,12,24);
addRotationMap("TWL2_BlueMoon",					"CTF",1,1,18,32);
addRotationMap("TWL_Katabatic",					"CTF",1,0,16,32);
addRotationMap("SandOcean",			   			"CTF",1,0,12,28);
addRotationMap("ks_braistv",					"CTF",1,0,12,24);
addRotationMap("Nightdance",					"CTF",1,0,16,24);
addRotationMap("TWL_Abaddon",					"CTF",1,0,16,24);
//addRotationMap("StarIce",						"CTF",1,0,12,24);
//addRotationMap("FilteredDust",				"CTF",1,0,12,24);
//addRotationMap("Choke",						"CTF",1,0,12,24);
addRotationMap("TWL_Chokepoint",				"CTF",1,0,12,24);
//addRotationMap("TWL_Starfallen",				"CTF",1,0,12,24);
//addRotationMap("ConstructionYard",			"CTF",1,0,12,24);
//addRotationMap("AcidRain",					"CTF",1,0,12,24);
addRotationMap("Agorazscium",					"CTF",1,0,12,24);

//BIG Vehicle Maps
/////////////////////////////////////////////////////////////////////

addRotationMap("Circleofstones",				"CTF",1,1,16,-1);
addRotationMap("Ruined",						"CTF",1,0,24,-1);
//addRotationMap("FullCircle",					"CTF",1,0,18,30);
//addRotationMap("Surro",				    	"CTF",1,0,18,24);
addRotationMap("TWL2_RoughLand",				"CTF",1,0,16,-1);
addRotationMap("HarvestDance",					"CTF",1,1,24,-1);
//addRotationMap("WindyGap",		    		"CTF",1,0,24,-1);
addRotationMap("Fenix",							"CTF",1,1,26,-1);
addRotationMap("Hillside",						"CTF",1,0,28,-1);
addRotationMap("TWL_Slapdash",				   	"CTF",1,0,24,-1);
addRotationMap("BerylBasin",					"CTF",1,1,20,-1);
//addRotationMap("TWL_Frozen",					"CTF",1,0,20,-1);
addRotationMap("TWL_Harvester",					"CTF",1,1,24,-1);
addRotationMap("TWL2_Bleed",					"CTF",1,0,24,-1);
addRotationMap("DeathBirdsFly1",				"CTF",1,1,24,-1);
//addRotationMap("Scarabrae_nef",				"CTF",1,0,28,-1);
addRotationMap("aabaa",							"CTF",1,1,28,-1);
addRotationMap("TWL_Reversion",					"CTF",1,1,28,-1);
addRotationMap("Horde",							"CTF",1,0,24,-1);
addRotationMap("CloakofNightV",					"CTF",1,0,24,-1);
//addRotationMap("TWL2_Ruined",					"CTF",1,1,18,-1);
//addRotationMap("Glade",						"CTF",1,1,28,-1);
//addRotationMap("Pantheon",					"CTF",1,0,20,-1);
//addRotationMap("Archipelago",					"CTF",1,0,20,-1);
//addRotationMap("Sangre_de_Grado",				"CTF",1,0,20,-1);

//DMP Mappack
/////////////////////////////////////////////////////////////////////
addRotationMap("DMP_Agroleon",					"CTF",1,1,18,28);
addRotationMap("DMP_Astro",						"CTF",1,0,16,26);
addRotationMap("DMP_BastardForge",				"CTF",1,1,8,28);
addRotationMap("DMP_BitterGorge",				"CTF",1,0,24,-1);
addRotationMap("DMP_Bunkered",					"CTF",1,0,6,14);
addRotationMap("DMP_Cinerarium",				"CTF",1,1,-1,20);
addRotationMap("DMP_DermCity",			    	"CTF",1,0,-1,-1);
addRotationMap("DMP_Embers",					"CTF",1,0,-1,14);
addRotationMap("DMP_EmeraldSpit",				"CTF",1,0,10,20);
addRotationMap("DMP_FaceCrossing",				"CTF",1,0,10,20);
addRotationMap("DMP_Hoth",						"CTF",1,1,26,-1);
addRotationMap("DMP_IceGiant",					"CTF",1,0,20,32);
addRotationMap("DMP_IsleDeBatalla",				"CTF",1,0,10,20);
addRotationMap("DMP_LavaGods",					"CTF",1,0,10,20);
addRotationMap("DMP_Magellan",					"CTF",1,1,10,28);
addRotationMap("DMP_MoonDance",					"CTF",1,1,12,32);
addRotationMap("DMP_Pantheon",					"CTF",1,0,16,24);
addRotationMap("DMP_Paranoia",					"CTF",1,0,6,10);
addRotationMap("DMP_Pariah",					"CTF",1,1,8,24);
addRotationMap("DMP_PipeDream",					"CTF",1,0,8,16);
addRotationMap("DMP_RavineV",					"CTF",1,0,12,28);
addRotationMap("DMP_ScorchedEarth",				"CTF",1,0,24,-1);
addRotationMap("DMP_SimpleFlagArena",			"CTF",1,0,-1,12);
addRotationMap("DMP_SpinCycle",					"CTF",1,1,-1,16);
addRotationMap("DMP_StarFall",					"CTF",1,0,-1,16);
addRotationMap("DMP_Tyre",						"CTF",1,1,16,-1);
addRotationMap("DMP_Wasteland",					"CTF",1,0,10,20);

addRotationMap("SC_Normal",						"CTF",1,0,-1,16);
//addRotationMap("SC_Badlands",					"CTF",1,0,-1,16);
//addRotationMap("SC_Desert",					"CTF",1,0,-1,16);
//addRotationMap("SC_Lush",						"CTF",1,0,-1,16);
addRotationMap("SC_Ice",						"CTF",1,1,-1,16);
addRotationMap("SC_Night",						"CTF",1,0,-1,16);
//addRotationMap("DX_Badlands",					"CTF",1,0,10,20);
//addRotationMap("DX_Desert",					"CTF",1,0,10,20);
addRotationMap("DX_Ice",						"CTF",1,1,10,28);
//addRotationMap("HO_Badlands",					"CTF",1,1,8,24);
//addRotationMap("HO_Desert",					"CTF",1,0,8,24);
addRotationMap("HO_Ice",						"CTF",1,1,8,28);
addRotationMap("HO_Lush",						"CTF",1,0,8,24);
addRotationMap("Attrition",						"CTF",1,0,-1,10);
//addRotationMap("Chasmaclysmic",				"CTF",1,1,-1,-1);
//addRotationMap("DBS_Smoothed",		    	"CTF",1,0,-1,-1);
addRotationMap("PuliVeivari",					"CTF",1,0,-1,-1);
addRotationMap("PlanetX",						"CTF",1,0,-1,-1);
//addRotationMap("Ravine",						"CTF",1,1,-1,-1);
addRotationMap("Stripmine",						"CTF",1,0,-1,-1);

//Not In Rotation - Not Voteable
/////////////////////////////////////////////////////////////////////

//addRotationMap("Sandstorm",					"CTF",1,0,-1,-1);
//addRotationMap("Starfallen",					"CTF",1,0,-1,-1);
//addRotationMap("Stonehenge_nef",				"CTF",1,0,-1,-1);
addRotationMap("Extractor",						"CTF",1,0,16,-1);
//addRotationMap("AstersDescent",				"CTF",1,0,-1,-1);
//addRotationMap("Azoth",						"CTF",1,0,-1,-1);
//addRotationMap("BattleGrove",					"CTF",1,0,-1,-1);
//addRotationMap("Durango",						"CTF",1,0,-1,-1);
//addRotationMap("DustLust",					"CTF",1,0,-1,-1);
//addRotationMap("IceGulch",					"CTF",1,0,-1,-1);
//addRotationMap("MountainMist",				"CTF",1,0,-1,-1);
//addRotationMap("Peak",						"CTF",1,0,-1,-1);
//addRotationMap("Pendulum",					"CTF",1,0,-1,-1);
//addRotationMap("S5_Misadventure",				"CTF",1,0,-1,-1);
//addRotationMap("S8_CentralDogma",				"CTF",1,0,-1,-1);
//addRotationMap("S8_Mountking",				"CTF",1,0,-1,-1);
//addRotationMap("S8_Zilch",					"CTF",1,0,-1,-1);
//addRotationMap("TWL2_CloakOfNight",			"CTF",1,0,-1,-1);
//addRotationMap("TWL_BaNsHee",					"CTF",1,0,-1,-1);
//addRotationMap("TWL_Boss",					"CTF",1,0,-1,-1);
//addRotationMap("TWL_NoShelter",				"CTF",1,0,-1,-1);
//addRotationMap("TWL_Clusterfuct",				"CTF",1,0,-1,-1);
addRotationMap("TWL_Curtilage",					"CTF",1,0,-1,-1);
//addRotationMap("TWL_Deserted",				"CTF",1,0,-1,-1);
//addRotationMap("TWL_Frostclaw",				"CTF",1,0,-1,-1);
//addRotationMap("TWL_Neve",					"CTF",1,0,-1,-1);
//addRotationMap("TWL_Ramparts",				"CTF",1,0,-1,-1);
//addRotationMap("TWL_Sandstorm",				"CTF",1,0,-1,-1);
//addRotationMap("TWL_WoodyMyrk",				"CTF",1,0,-1,-1);
//addRotationMap("OctoberRust",					"CTF",1,0,-1,-1);
//addRotationMap("DevilsElbow",					"CTF",1,0,-1,-1);
//addRotationMap("CloudCity",					"CTF",1,0,-1,-1);
//addRotationMap("Damnation",					"CTF",1,0,-1,-1);
//addRotationMap("DeathBirdsFly",				"CTF",1,0,-1,-1);
//addRotationMap("Desiccator",					"CTF",1,0,-1,-1);
//addRotationMap("DustToDust",					"CTF",1,0,-1,-1);
//addRotationMap("Katabatic",					"CTF",1,0,-1,-1);
//addRotationMap("Quagmire",					"CTF",1,0,-1,-1);
//addRotationMap("Recalescence",				"CTF",1,0,-1,-1);
//addRotationMap("Reversion",					"CTF",1,0,-1,-1);
//addRotationMap("RiverDance",					"CTF",1,0,-1,-1);
//addRotationMap("Sanctuary",					"CTF",1,0,-1,-1);
//addRotationMap("ThinIce",						"CTF",1,0,-1,-1);
//addRotationMap("Tombstone",					"CTF",1,0,-1,-1);
//addRotationMap("Broadside_nef",				"CTF",1,0,-1,-1);
//addRotationMap("Camelland",					"CTF",1,0,-1,-1);
//addRotationMap("HighTrepidation",				"CTF",1,0,-1,-1);
//addRotationMap("SmallDesertofDeath",			"CTF",1,0,-1,-1);
//addRotationMap("BasinFury",					"CTF",1,0,-1,-1);
//addRotationMap("Cadaver",						"CTF",1,0,-1,-1);
//addRotationMap("EivoItoxico",					"CTF",1,0,-1,-1);
//addRotationMap("Einfach",						"CTF",1,0,-1,-1);
//addRotationMap("PicnicTable",					"CTF",1,0,-1,-1);
//addRotationMap("Hostility",					"CTF",1,0,-1,-1);
//addRotationMap("HighWire",					"CTF",1,0,-1,-1);
//addRotationMap("CloudBurst",					"CTF",1,0,-1,-1);
//addRotationMap("CloseCombat",					"CTF",1,0,-1,-1);
//addRotationMap("DesertofDeath_nef",			"CTF",1,0,-1,-1);
//addRotationMap("Gorgon",						"CTF",1,0,-1,-1);
//addRotationMap("Magmatic",					"CTF",1,0,-1,-1);
//addRotationMap("Sub-zero",					"CTF",1,0,-1,-1);


// Custom
/////////////////////////////////////////////////////////////////////

addRotationMap("CoconutFury",					"CTF",1,0,-1,-1);
addRotationMap("CinderCone",					"CTF",1,0,-1,-1);
addRotationMap("Sector1",						"CTF",1,0,-1,-1);
//addRotationMap("Slamnation",					"CTF",0,0,-1,-1);
//addRotationMap("MountaintopMayhem",			"CTF",0,0,-1,-1);

addRotationMap("ChasmClash",					"CTF",1,0,-1,14);
//addRotationMap("FrostBite",					"CTF",1,0,-1,-1);
//addRotationMap("Lapiths",						"CTF",1,0,-1,-1);
addRotationMap("GreenLawn",						"CTF",1,1,12,32);
addRotationMap("OuterWilds",					"CTF",1,1,-1,12);
addRotationMap("Jettisoned",					"CTF",1,0,-1,10);
addRotationMap("Moonwalk",						"CTF",1,1,12,-1);
addRotationMap("Drafts",						"CTF",1,1,-1,12);
//addRotationMap("IslaHermano",					"CTF",1,0,-1,-1);
addRotationMap("DermCrossingDeluxe",			"CTF",1,0,-1,-1);
addRotationMap("DaftGulch",						"CTF",1,1,16,24);
addRotationMap("Triad",		    				"CTF",1,0,12,20);
addRotationMap("Vaubania",						"CTF",1,0,-1,12);
addRotationMap("VanDamnedCTF",					"CTF",1,1,-1,14);
addRotationMap("HillKingCTF",					"CTF",1,1,-1,18);
addRotationMap("Bonespur",					    "CTF",1,1,-1,14);
addRotationMap("Bridgepoint",				    "CTF",1,0,-1,18);
addRotationMap("Narcolepsy",				    "CTF",1,1,-1,20);
addRotationMap("WhiteDwarfDeluxe",			    "CTF",1,0,-1,14);
addRotationMap("SmallSpit",					    "CTF",1,1,-1,20);
addRotationMap("MisadventureV2",			    "CTF",1,1,12,28);
addRotationMap("El_Fin",					    "CTF",1,1,-1,14);

//  _           _              _     _     _ _
// | |         | |            | |   | |   (_) |
// | |     __ _| | ___ __ __ _| |__ | |__  _| |_
// | |    / _` | |/ / '__/ _` | '_ \| '_ \| | __|
// | |___| (_| |   <| | | (_| | |_) | |_) | | |_
// |______\__,_|_|\_\_|  \__,_|_.__/|_.__/|_|\__|
/////////////////////////////////////////////////////////////////////

//In Rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("VaubanLak",						"Lakrabbit",1,0,8,-1);
addRotationMap("MiniSunDried",					"Lakrabbit",1,1,-1,12);
addRotationMap("Sundance",						"Lakrabbit",1,1,-1,-1);
addRotationMap("TWL_BeachBlitzLak",				"Lakrabbit",1,0,-1,-1);
addRotationMap("DesertofDeathLak",				"Lakrabbit",1,1,-1,-1);
addRotationMap("Raindance_nefLak",				"Lakrabbit",1,1,-1,-1);
addRotationMap("SunDriedLak",					"Lakrabbit",1,1,-1,-1);
addRotationMap("SkinnyDipLak",					"Lakrabbit",1,0,-1,-1);
addRotationMap("SaddiesHill",					"Lakrabbit",1,0,-1,-1);
addRotationMap("HavenLak",						"Lakrabbit",1,0,-1,-1);
addRotationMap("LushLak",						"Lakrabbit",1,1,8,-1);
addRotationMap("BoxLak",						"Lakrabbit",1,1,-1,10);
addRotationMap("TitaniaLak",					"Lakrabbit",1,0,8,-1);
addRotationMap("TibbawLak",						"Lakrabbit",1,1,-1,-1);
addRotationMap("InfernusLak",					"Lakrabbit",1,1,-1,-1);
addRotationMap("S8_GeothermalLak",				"Lakrabbit",1,1,-1,-1);
addRotationMap("CankerLak",						"Lakrabbit",1,1,-1,-1);
addRotationMap("DustRunLak",					"Lakrabbit",1,1,-1,-1);
addRotationMap("CrossfiredLak",					"Lakrabbit",1,1,-1,-1);
addRotationMap("CloakofLak",					"Lakrabbit",1,1,-1,-1);
addRotationMap("SpectreLak",					"Lakrabbit",1,1,-1,-1);

//Voteable, But not in rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("TreasureIslandLak",				"Lakrabbit",1,1,-1,-1);
addRotationMap("Sulfide",						"Lakrabbit",1,0,-1,-1);
addRotationMap("FrozenFuryLak",					"Lakrabbit",1,0,-1,-1);
addRotationMap("Arrakis",						"Lakrabbit",1,0,-1,-1);
addRotationMap("EquinoxLak",					"Lakrabbit",1,0,-1,-1);
addRotationMap("PhasmaDustLak",					"Lakrabbit",1,0,-1,12);
addRotationMap("GodsRiftLak",					"Lakrabbit",1,0,-1,-1);
addRotationMap("SolsDescentLak",				"Lakrabbit",1,0,-1,-1);
addRotationMap("Crater71Lak",					"Lakrabbit",1,0,6,-1);

//Not Voteable, Not in rotation
/////////////////////////////////////////////////////////////////////

//addRotationMap("EscaladeLak",					"Lakrabbit",1,0,-1,-1);
//addRotationMap("MagmaticLak",					"Lakrabbit",1,0,-1,-1);
//addRotationMap("HillsOfSorrow",				"Lakrabbit",1,0,-1,-1);
//addRotationMap("TWL2_MuddySwampLak",			"Lakrabbit",1,0,-1,-1);
//addRotationMap("SandStormLak",				"Lakrabbit",1,0,-1,-1);
//addRotationMap("BeggarsRunLak",				"Lakrabbit",1,0,-1,-1);
//addRotationMap("DamnnationLak",				"Lakrabbit",1,0,-1,-1);


//  _      _____ _______ ______
// | |    / ____|__   __|  ____|
// | |   | |       | |  | |__
// | |   | |       | |  |  __|
// | |___| |____   | |  | |
// |______\_____|  |_|  |_|
/////////////////////////////////////////////////////////////////////

//In Rotation
/////////////////////////////////////////////////////////////////////
addRotationMap("DangerousCrossingLT",			"sctf",1,0,-1,-1);
addRotationMap("SmallCrossingLT",				"sctf",1,0,-1,12);
addRotationMap("DireLT",						"sctf",1,0,-1,-1);
addRotationMap("RoundTheMountainLT",			"sctf",1,1,-1,-1);
addRotationMap("CirclesEdgeLT",					"sctf",1,1,-1,-1);
addRotationMap("TenebrousCTF",					"sctf",1,0,-1,-1);
addRotationMap("TheFray",						"sctf",1,0,-1,-1);
addRotationMap("SignalLT",						"sctf",1,0,-1,-1);
addRotationMap("S5_DamnationLT",				"sctf",1,0,8,-1);
addRotationMap("S5_Icedance",					"sctf",1,1,-1,-1);
addRotationMap("S5_Mordacity",					"sctf",1,1,10,-1);
addRotationMap("S5_SilenusLT",					"sctf",1,0,-1,-1);
addRotationMap("TWL2_CanyonCrusadeDeluxeLT",	"sctf",1,1,-1,-1);
addRotationMap("TWL2_HildebrandLT",				"sctf",1,1,-1,-1);
addRotationMap("TWL2_SkylightLT",				"sctf",1,1,-1,-1);
addRotationMap("TWL_FeignLT",					"sctf",1,0,-1,-1);
addRotationMap("TWL_RollercoasterLT",			"sctf",1,0,-1,-1);
addRotationMap("TWL_StonehengeLT",				"sctf",1,1,-1,-1);
addRotationMap("TWL_WilderZoneLT",				"sctf",1,0,-1,-1);
addRotationMap("oasisintensity",				"sctf",1,1,-1,10);
addRotationMap("berlard",						"sctf",1,1,-1,-1);
addRotationMap("RaindanceLT",					"sctf",1,0,-1,-1);
addRotationMap("SmallTimeLT",					"sctf",1,0,-1,-1);
addRotationMap("ArenaDome",						"sctf",1,1,-1,10);
addRotationMap("Bulwark",						"sctf",1,1,-1,-1);
addRotationMap("Discord",						"sctf",1,1,-1,-1);
addRotationMap("S5_MassiveLT",					"sctf",1,0,-1,-1);
addRotationMap("Blink",							"sctf",1,0,-1,-1);
addRotationMap("HillSideLT",					"sctf",1,1,-1,10);
addRotationMap("IcePick",						"sctf",1,1,-1,16);
addRotationMap("OsIrisLT",						"sctf",1,0,-1,-1);
addRotationMap("GrassyKnoll",					"sctf",1,1,-1,-1);
addRotationMap("TWL2_MuddySwamp",				"sctf",1,0,8,-1);
addRotationMap("SandyRunLT",					"sctf",1,0,-1,12);
addRotationMap("Sentry",						"sctf",1,1,-1,-1);
addRotationMap("Cinerarium",					"sctf",1,1,-1,-1);
addRotationMap("Exhumed",						"sctf",1,1,-1,-1);
addRotationMap("S8_ZilchLT",					"sctf",1,1,-1,-1);
addRotationMap("TWL_BeachBlitzLT",				"sctf",1,0,-1,-1);
addRotationMap("FrozenHopeLT",					"sctf",1,1,-1,-1);
//addRotationMap("Confusco",						"sctf",1,0,6,-1);
addRotationMap("TWL2_JaggedClawLT",				"sctf",1,1,-1,-1);

//Voteable, But not in rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("BeggarsRunLT",					"sctf",1,0,-1,-1);
addRotationMap("S5_WoodymyrkLT",				"sctf",1,0,-1,-1);
addRotationMap("S8_Opus",						"sctf",1,0,-1,-1);
addRotationMap("TWL_DangerousCrossing",			"sctf",1,0,-1,-1);

//DMP Mappack
/////////////////////////////////////////////////////////////////////
addRotationMap("AttritionLT",					"sctf",1,1,-1,-1);
addRotationMap("RushLT",						"sctf",1,1,-1,-1);
addRotationMap("HillKingLT",					"sctf",1,1,-1,-1);
addRotationMap("DMP_BastardForgeLT",			"sctf",1,1,-1,-1);
addRotationMap("VanDamnedLT",					"sctf",1,1,-1,-1);
addRotationMap("DMP_Paranoia",					"sctf",1,0,-1,-1);
addRotationMap("DMP_Spincycle",					"sctf",1,1,-1,-1);
addRotationMap("Ravine",						"sctf",1,1,-1,-1);
// addRotationMap("DX_Badlands",				"sctf",1,0,-1,-1);
// addRotationMap("DX_Desert",					"sctf",1,0,-1,-1);
addRotationMap("DX_Ice",						"sctf",1,1,-1,-1);
addRotationMap("PipeDreamLT",					"sctf",1,0,-1,-1);
addRotationMap("DMP_LavaGods",					"sctf",1,0,10,20);
addRotationMap("DMP_StarFallLT",				"sctf",1,1,-1,-1);
addRotationMap("DMP_BunkeredLT",				"sctf",1,1,-1,12);
addRotationMap("DMP_PariahLT",					"sctf",1,1,-1,-1);

//Not Voteable, Not in rotation
/////////////////////////////////////////////////////////////////////

//addRotationMap("SurrealLT",					"sctf",1,0,-1,-1);
addRotationMap("DuelersDelight",				"sctf",1,0,-1,12);
addRotationMap("SmallSpit",					    "sctf",1,1,-1,14);
addRotationMap("BonespurLT",				    "sctf",1,1,-1,14);
addRotationMap("Bridgepoint",				    "sctf",1,1,-1,14);
addRotationMap("NarcolepsyLT",				    "sctf",1,1,-1,14);
addRotationMap("WhiteDwarfDeluxeLT",		    "sctf",1,0,-1,14);
addRotationMap("ClusterUnFuct",					"sctf",1,1,8,-1);
addRotationMap("OuterWilds",					"sctf",1,1,-1,12);
addRotationMap("Drafts",						"sctf",1,1,-1,12);
addRotationMap("DermCrossingDeluxeLT",			"sctf",1,1,-1,-1);
addRotationMap("SuperiorWaterworks",			"sctf",1,0,-1,12);
addRotationMap("FrozenForgeLT",					"sctf",1,1,-1,20);
addRotationMap("TWL2_CelerityLT",				"sctf",1,1,-1,20);
addRotationMap("El_Fin",					    "sctf",1,1,-1,14);


//  _____             _   _                     _       _
// |  __ \           | | | |                   | |     | |
// | |  | | ___  __ _| |_| |__  _ __ ___   __ _| |_ ___| |__
// | |  | |/ _ \/ _` | __| '_ \| '_ ` _ \ / _` | __/ __| '_ \
// | |__| |  __/ (_| | |_| | | | | | | | | (_| | || (__| | | |
// |_____/ \___|\__,_|\__|_| |_|_| |_| |_|\__,_|\__\___|_| |_|
/////////////////////////////////////////////////////////////////////

addRotationMap("RaspDM",						"DM",1,1,8,-1);
addRotationMap("EntombedDM",					"DM",1,1,-1,-1);
addRotationMap("IceDomeDM",						"DM",1,1,-1,-1);
addRotationMap("HoofToeDM",						"DM",1,1,6,-1);
addRotationMap("ArenaDomeDM",					"DM",1,1,-1,-1);
addRotationMap("VulcansWrathDM",				"DM",1,1,-1,-1);
addRotationMap("RampartsDM",					"DM",1,1,-1,-1);
addRotationMap("ShrineDM",						"DM",1,1,-1,12);
addRotationMap("LiveBaitDM",					"DM",1,1,-1,-1);
addRotationMap("FourSquareDM",					"DM",1,1,-1,10);
addRotationMap("BrigDM",						"DM",1,1,-1,8);
addRotationMap("PitsDM",						"DM",1,1,-1,10);
addRotationMap("RingofFireDM",					"DM",1,1,-1,10);
addRotationMap("GorgonDM",						"DM",1,1,-1,10);


//  _____             _
// |  __ \           | |
// | |  | |_   _  ___| |
// | |  | | | | |/ _ \ |
// | |__| | |_| |  __/ |
// |_____/ \__,_|\___|_|
/////////////////////////////////////////////////////////////////////

//addRotationMap("AgentsOfFortune",				"Duel",1,1,-1,-1);
//addRotationMap("Casern_Cavite",				"Duel",1,1,-1,-1);
//addRotationMap("Equinox",						"Duel",1,1,-1,-1);
//addRotationMap("Escalade",					"Duel",1,1,-1,-1);
//addRotationMap("Fracas",						"Duel",1,1,-1,-1);
//addRotationMap("Invictus",					"Duel",1,1,-1,-1);
//addRotationMap("MyrkWood",					"Duel",1,1,-1,-1);
//addRotationMap("Oasis",						"Duel",1,1,-1,-1);
//addRotationMap("Pyroclasm",					"Duel",1,1,-1,-1);
//addRotationMap("Rasp",						"Duel",1,1,-1,-1);
//addRotationMap("SunDried",					"Duel",1,1,-1,-1);
//addRotationMap("Talus",						"Duel",1,1,-1,-1);
//addRotationMap("Underhill",					"Duel",1,1,-1,-1);
//addRotationMap("Whiteout",					"Duel",1,1,-1,-1);
//addRotationMap("Tombstone",					"Duel",1,1,-1,-1);
//addRotationMap("VaubanLak",					"Duel",1,1,-1,-1);
//addRotationMap("DesertofDeathLak",			"Duel",1,1,-1,-1);



//   _____ _
//  / ____(_)
// | (___  _  ___  __ _  ___
//  \___ \| |/ _ \/ _` |/ _ \
//  ____) | |  __/ (_| |  __/
// |_____/|_|\___|\__, |\___|
//                 __/ |
//                |___/
/////////////////////////////////////////////////////////////////////

//addRotationMap("Alcatraz",				"Siege",1,1,-1,-1);
//addRotationMap("BridgeTooFar",			"Siege",1,1,-1,-1);
//addRotationMap("Caldera",					"Siege",1,1,-1,-1);
//addRotationMap("Gauntlet",				"Siege",1,1,-1,-1);
//addRotationMap("IceBound",				"Siege",1,1,-1,-1);
//addRotationMap("Isleofman",				"Siege",1,1,-1,-1);
//addRotationMap("Masada",					"Siege",1,1,-1,-1);
//addRotationMap("Respite",					"Siege",1,1,-1,-1);
//addRotationMap("Trident",					"Siege",1,1,-1,-1);
//addRotationMap("UltimaThule",				"Siege",1,1,-1,-1);



//  _    _             _
// | |  | |           | |
// | |__| |_   _ _ __ | |_ ___ _ __ ___
// |  __  | | | | '_ \| __/ _ \ '__/ __|
// | |  | | |_| | | | | ||  __/ |  \__ \
// |_|  |_|\__,_|_| |_|\__\___|_|  |___/
/////////////////////////////////////////////////////////////////////

//addRotationMap("MiniSunDried","Hunters",1,1,-1,-1);
//addRotationMap("DustToDust","Hunters",1,1,-1,-1);
