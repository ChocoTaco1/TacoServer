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

addRotationMap("SmallCrossing",					"CTF",1,1,-1,12);
addRotationMap("TWL2_CanyonCrusadeDeluxe",		"CTF",1,1,-1,12);
addRotationMap("RoundTheMountain",				"CTF",1,1,-1,14);
addRotationMap("oasisintensity",				"CTF",1,1,-1,12);
addRotationMap("Minotaur",						"CTF",1,0,-1,12);
//addRotationMap("Island",						"CTF",1,1,-1,12);
//addRotationMap("TitForTat",					"CTF",1,1,-1,12);
addRotationMap("SmallMelee",					"CTF",1,0,-1,12);
//addRotationMap("SuperHappyBouncyFunTime",		"CTF",1,1,-1,12);
addRotationMap("Machineeggs",					"CTF",1,1,-1,12);
addRotationMap("Mac_FlagArena",					"CTF",1,0,-1,12);
addRotationMap("SmallTimeCTF",					"CTF",1,1,-1,10);
addRotationMap("TWL2_Hildebrand",				"CTF",1,1,-1,12);
addRotationMap("ArenaDome",						"CTF",1,1,-1,12);
addRotationMap("Firestorm",						"CTF",1,1,-1,12);
addRotationMap("Bulwark",						"CTF",1,1,-1,12);

//Medium Maps
/////////////////////////////////////////////////////////////////////

addRotationMap("HighOctane",					"CTF",1,1,8,20);
addRotationMap("S5_Mordacity",					"CTF",1,1,10,20);
addRotationMap("S5_Damnation",					"CTF",1,1,8,20);
addRotationMap("TWL2_JaggedClaw",				"CTF",1,0,8,20);
addRotationMap("S5_Massive",					"CTF",1,1,8,20);
addRotationMap("TWL_Stonehenge",				"CTF",1,1,8,20);
addRotationMap("TWL_Feign",						"CTF",1,0,8,20);
addRotationMap("TheFray",						"CTF",1,1,8,20);
addRotationMap("DangerousCrossing_nef",			"CTF",1,1,8,20);
addRotationMap("TWL2_Skylight",					"CTF",1,0,8,20);
addRotationMap("TWL2_Ocular",					"CTF",1,1,12,20);
addRotationMap("Dire",							"CTF",1,1,8,20);
addRotationMap("berlard",						"CTF",1,1,8,20);
addRotationMap("S8_Opus",						"CTF",1,0,8,20);
addRotationMap("BeggarsRun",					"CTF",1,0,8,20);
addRotationMap("Headstone",						"CTF",1,0,8,20);
addRotationMap("Signal",						"CTF",1,1,8,20);
addRotationMap("S5_Woodymyrk",					"CTF",1,1,8,20);
addRotationMap("Discord",						"CTF",1,1,8,20);
addRotationMap("TenebrousCTF",					"CTF",1,1,8,20);
addRotationMap("Pariah",						"CTF",1,0,8,20);
addRotationMap("Prismatic",						"CTF",1,0,8,20);
addRotationMap("TWL_WilderZone",				"CTF",1,1,8,20);
addRotationMap("Mirage",						"CTF",1,1,8,20);
addRotationMap("S5_Mimicry",					"CTF",1,1,10,20);
addRotationMap("TWL_Snowblind",					"CTF",1,0,12,20);
addRotationMap("ShortFall",						"CTF",1,0,8,20);
addRotationMap("IceRidge_nef",					"CTF",1,0,8,20);
//addRotationMap(Disjointed",					"CTF",1,1,8,20);
addRotationMap("TWL2_MuddySwamp",				"CTF",1,0,8,20);
addRotationMap("Blink",							"CTF",1,1,8,20);
addRotationMap("HighAnxiety",					"CTF",1,0,8,20);
addRotationMap("S5_Centaur",					"CTF",1,0,8,20);
addRotationMap("S8_Cardiac",					"CTF",1,1,12,24);
addRotationMap("CirclesEdge",					"CTF",1,1,10,20);
addRotationMap("S5_Icedance",					"CTF",1,1,8,20);
addRotationMap("Surreal",						"CTF",1,1,8,20);

//Voteable but Not in Rotation
/////////////////////////////////////////////////////////////////////

//addRotationMap("Snowcone",					"CTF",1,0,10,20);
//addRotationMap("S5_Drache",					"CTF",1,0,10,20);
//addRotationMap("S5_HawkingHeat",				"CTF",1,0,10,20);
//addRotationMap("JadeValley",					"CTF",1,0,10,20);
//addRotationMap("S5_Sherman",					"CTF",1,0,10,20);
addRotationMap("S5_Silenus",					"CTF",1,0,8,20);
addRotationMap("TWL2_FrozenHope",				"CTF",1,0,8,20);
//addRotationMap("TWL2_IceDagger",				"CTF",1,0,10,20);
//addRotationMap("S5_Reynard",					"CTF",1,0,10,20);
//addRotationMap("TWL_Cinereous",				"CTF",1,0,10,20);
//addRotationMap("TWL_OsIris, CTF",				"CTF",1,0,10,20);
addRotationMap("Coppersky",						"CTF",1,0,8,20);
//addRotationMap("TWL2_Crevice",				"CTF",1,0,10,20);
//addRotationMap("TWL_SubZero",					"CTF",1,0,10,20);
addRotationMap("TWL_Titan",						"CTF",1,0,10,20);
addRotationMap("Confusco",						"CTF",1,0,10,20);
//addRotationMap("Fallout",						"CTF",1,0,10,20);
addRotationMap("TheClocktower",					"CTF",1,0,10,20);
//addRotationMap("oylentGreen",					"CTF",1,0,10,20);
addRotationMap("TWL2_MidnightMayhemDeluxe",		"CTF",1,0,8,20);
addRotationMap("Nightdance",					"CTF",1,0,10,20);
//addRotationMap("Ramparts",					"CTF",1,0,10,20);
addRotationMap("TWL2_Celerity",					"CTF",1,0,8,20);
addRotationMap("Blastside_nef",					"CTF",1,0,10,20);
addRotationMap("Infernus",						"CTF",1,0,10,20);
addRotationMap("NatureMagic",					"CTF",1,0,10,20);
//addRotationMap(TWL_Damnation",				"CTF",1,0,10,20);
//addRotationMap(TWL_DangerousCrossing",		"CTF",1,0,10,20);
//addRotationMap(TWL_DeadlyBirdsSong",			"CTF",1,0,10,20);
addRotationMap("Vauban",						"CTF",1,0,12,20);

//Vehicle Maps
/////////////////////////////////////////////////////////////////////

addRotationMap("HostileLoch",					"CTF",1,1,12,20);
addRotationMap("TWL_BeachBlitz",				"CTF",1,0,12,20);
addRotationMap("TWL2_Magnum",					"CTF",1,1,12,20);
addRotationMap("Logans_Run",					"CTF",1,1,12,20);
addRotationMap("Rollercoaster_nef",				"CTF",1,1,12,24);
addRotationMap("MoonDance",						"CTF",1,1,12,24);
addRotationMap("Raindance_nef",					"CTF",1,1,12,20);
addRotationMap("TWL_Magmatic",					"CTF",1,1,12,20);
addRotationMap("TWL2_FrozenGlory",				"CTF",1,1,12,20);
addRotationMap("LandingParty",					"CTF",1,1,12,20);
addRotationMap("TitanV",						"CTF",1,0,12,20);
addRotationMap("TWL_Crossfire",					"CTF",1,1,12,20);
//addRotationMap("Surro",						"CTF",1,0,12,20);
//addRotationMap("The_Calm",					"CTF",1,0,12,20);

//Vehicle Maps: Voteable, But Not in Rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("SubZeroV",						"CTF",1,0,12,20);
//addRotationMap("TWL2_RoughLand",				"CTF",1,0,12,20);
addRotationMap("S8_Geothermal",					"CTF",1,0,12,20);
addRotationMap("Lakefront",						"CTF",1,0,12,20);
addRotationMap("ShockRidge",					"CTF",1,0,12,20);
addRotationMap("TWL2_BlueMoon",					"CTF",1,0,12,20);
addRotationMap("FullCircle",					"CTF",1,0,12,20);
addRotationMap("TWL_Katabatic",					"CTF",1,0,12,20);
addRotationMap("TWL_Starfallen",				"CTF",1,0,12,20);
addRotationMap("ConstructionYard",				"CTF",1,0,12,20);
//addRotationMap("AcidRain",					"CTF",1,0,12,20);
addRotationMap("SandOcean",						"CTF",1,0,12,20);
//addRotationMap("StarIce",						"CTF",1,0,12,20);
addRotationMap("ks_braistv",					"CTF",1,0,12,20);
addRotationMap("FilteredDust",					"CTF",1,0,12,20);
//addRotationMap("Choke",						"CTF",1,0,12,20);
//addRotationMap("TWL2_Ruined",					"CTF",1,0,12,20);
//addRotationMap("TWL_Chokepoint",				"CTF",1,0,12,20);
//addRotationMap("Glade",						"CTF",1,0,12,20);

//BIG Vehicle Maps
/////////////////////////////////////////////////////////////////////

addRotationMap("HarvestDance",					"CTF",1,1,18,32);
addRotationMap("WindyGap",						"CTF",1,1,20,32);
addRotationMap("Fenix",							"CTF",1,1,24,32);
addRotationMap("Hillside",						"CTF",1,1,20,32);
//addRotationMap("Sangre_de_Grado",				"CTF",1,0,20,32);
addRotationMap("Slapdash",						"CTF",1,0,20,32);
addRotationMap("BerylBasin",					"CTF",1,0,18,32);
//addRotationMap("TWL_Frozen",					"CTF",1,0,20,32);
addRotationMap("TWL_Harvester",					"CTF",1,0,20,32);
addRotationMap("Archipelago",					"CTF",1,0,20,32);
addRotationMap("TWL2_Bleed",					"CTF",1,0,20,32);
//addRotationMap("Pantheon",					"CTF",1,0,20,32);
addRotationMap("Circleofstones",				"CTF",1,1,20,32);
addRotationMap("Scarabrae_nef",					"CTF",1,0,20,32);

//Not In Rotation - Not Voteable
/////////////////////////////////////////////////////////////////////

//addRotationMap("Sandstorm",					"CTF",1,0,-1,32);
//addRotationMap("Starfallen",					"CTF",1,0,-1,32);
//addRotationMap("Stonehenge_nef",				"CTF",1,0,-1,32);
//addRotationMap("Extractor",					"CTF",1,0,-1,32);
//addRotationMap("AstersDescent",				"CTF",1,0,-1,32);
//addRotationMap("Azoth",						"CTF",1,0,-1,32);
//addRotationMap("BattleGrove",					"CTF",1,0,-1,32);
//addRotationMap("Durango",						"CTF",1,0,-1,32);
//addRotationMap("DustLust",					"CTF",1,0,-1,32);
//addRotationMap("IceGulch",					"CTF",1,0,-1,32);
//addRotationMap("MountainMist",				"CTF",1,0,-1,32);
//addRotationMap("Peak",						"CTF",1,0,-1,32);
//addRotationMap("Pendulum",					"CTF",1,0,-1,32);
//addRotationMap("S5_Misadventure",				"CTF",1,0,-1,32);
//addRotationMap("S8_CentralDogma",				"CTF",1,0,-1,32);
//addRotationMap("S8_Mountking",				"CTF",1,0,-1,32);
//addRotationMap("S8_Zilch",					"CTF",1,0,-1,32);
//addRotationMap("TWL2_CloakOfNight",			"CTF",1,0,-1,32);
//addRotationMap("TWL2_Dissention",				"CTF",1,0,-1,32);
//addRotationMap("TWL2_Drifts",					"CTF",1,0,-1,32);
//addRotationMap("TWL2_Drorck",					"CTF",1,0,-1,32);
//addRotationMap("TWL2_Norty",					"CTF",1,0,-1,32);
//addRotationMap("TWL_Abaddon",					"CTF",1,0,-1,32);
//addRotationMap("TWL_BaNsHee",					"CTF",1,0,-1,32);
//addRotationMap("TWL_Boss",					"CTF",1,0,-1,32);
//addRotationMap("TWL_NoShelter",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Clusterfuct",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Curtilage",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Deserted",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Frostclaw",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Horde",					"CTF",1,0,-1,32);
//addRotationMap("TWL_Neve",					"CTF",1,0,-1,32);
//addRotationMap("TWL_Pandemonium",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Ramparts",				"CTF",1,0,-1,32);
//addRotationMap("TWL_Sandstorm",				"CTF",1,0,-1,32);
//addRotationMap("TWL_WoodyMyrk",				"CTF",1,0,-1,32);
//addRotationMap("OctoberRust",					"CTF",1,0,-1,32);
//addRotationMap("DevilsElbow",					"CTF",1,0,-1,32);
//addRotationMap("CloudCity",					"CTF",1,0,-1,32);
//addRotationMap("Damnation",					"CTF",1,0,-1,32);
//addRotationMap("DeathBirdsFly",				"CTF",1,0,-1,32);
//addRotationMap("Desiccator",					"CTF",1,0,-1,32);
//addRotationMap("DustToDust",					"CTF",1,0,-1,32);
//addRotationMap("Katabatic",					"CTF",1,0,-1,32);
//addRotationMap("Quagmire",					"CTF",1,0,-1,32);
//addRotationMap("Recalescence",				"CTF",1,0,-1,32);
//addRotationMap("Reversion",					"CTF",1,0,-1,32);
//addRotationMap("RiverDance",					"CTF",1,0,-1,32);
//addRotationMap("Sanctuary",					"CTF",1,0,-1,32);
//addRotationMap("ThinIce",						"CTF",1,0,-1,32);
//addRotationMap("Tombstone",					"CTF",1,0,-1,32);
//addRotationMap("Broadside_nef",				"CTF",1,0,-1,32);
//addRotationMap("Camelland",					"CTF",1,0,-1,32);
//addRotationMap("HighTrepidation",				"CTF",1,0,-1,32);
//addRotationMap("SmallDesertofDeath",			"CTF",1,0,-1,32);
//addRotationMap("Agorazscium",					"CTF",1,0,-1,32);
//addRotationMap("BasinFury",					"CTF",1,0,-1,32);
//addRotationMap("Cadaver",						"CTF",1,0,-1,32);
//addRotationMap("EivoItoxico",					"CTF",1,0,-1,32);
//addRotationMap("Einfach",						"CTF",1,0,-1,32);
//addRotationMap("PicnicTable",					"CTF",1,0,-1,32);
//addRotationMap("Hostility",					"CTF",1,0,-1,32);
//addRotationMap("HighWire",					"CTF",1,0,-1,32);
//addRotationMap("CloudBurst",					"CTF",1,0,-1,32);
//addRotationMap("CloseCombat",					"CTF",1,0,-1,32);
//addRotationMap("DesertofDeath_nef",			"CTF",1,0,-1,32);
//addRotationMap("Gorgon",						"CTF",1,0,-1,32);
//addRotationMap("Magamatic",					"CTF",1,0,-1,32);
//addRotationMap("Sub-zero",					"CTF",1,0,-1,32);



//  _           _              _     _     _ _   
// | |         | |            | |   | |   (_) |  
// | |     __ _| | ___ __ __ _| |__ | |__  _| |_ 
// | |    / _` | |/ / '__/ _` | '_ \| '_ \| | __|
// | |___| (_| |   <| | | (_| | |_) | |_) | | |_ 
// |______\__,_|_|\_\_|  \__,_|_.__/|_.__/|_|\__|
/////////////////////////////////////////////////////////////////////

//In Rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("VaubanLak",						"Lakrabbit",1,1,-1,32);
addRotationMap("MiniSunDried",					"Lakrabbit",1,1,-1,12);
addRotationMap("Sundance",						"Lakrabbit",1,1,-1,32);
addRotationMap("TWL_BeachBlitzLak",				"Lakrabbit",1,1,-1,32);
addRotationMap("DesertofDeathLak",				"Lakrabbit",1,1,-1,32);
addRotationMap("Raindance_nefLak",				"Lakrabbit",1,1,-1,32);
addRotationMap("SunDriedLak",					"Lakrabbit",1,1,-1,32);
addRotationMap("SkinnyDipLak",					"Lakrabbit",1,1,-1,32);
addRotationMap("SaddiesHill",					"Lakrabbit",1,1,-1,32);
addRotationMap("HavenLak",						"Lakrabbit",1,0,-1,32);
addRotationMap("LushLak",						"Lakrabbit",1,1,-1,32);
addRotationMap("BoxLak",						"Lakrabbit",1,1,-1,10);
addRotationMap("TitaniaLak",					"Lakrabbit",1,0,8,32);
addRotationMap("TibbawLak",						"Lakrabbit",1,1,-1,32);
addRotationMap("InfernusLak",					"Lakrabbit",1,1,-1,32);
addRotationMap("S8_GeothermalLak",				"Lakrabbit",1,1,-1,32);
addRotationMap("CankerLak",						"Lakrabbit",1,1,-1,32);
addRotationMap("DustRunLak",					"Lakrabbit",1,1,-1,32);
addRotationMap("CrossfiredLak",					"Lakrabbit",1,0,-1,32);

//Voteable, But not in rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("TreasureIslandLak",				"Lakrabbit",1,0,-1,32);
addRotationMap("Sulfide",						"Lakrabbit",1,0,-1,32);
addRotationMap("FrozenFuryLak",					"Lakrabbit",1,0,-1,32);
addRotationMap("Arrakis",						"Lakrabbit",1,0,-1,32);
addRotationMap("EquinoxLak",					"Lakrabbit",1,0,-1,32);
addRotationMap("PhasmaDustLak",					"Lakrabbit",1,1,-1,12);
addRotationMap("GodsRiftLak",					"Lakrabbit",1,0,-1,32);
addRotationMap("SolsDescentLak",				"Lakrabbit",1,0,-1,32);
addRotationMap("Crater71Lak",					"Lakrabbit",1,0,-1,32);

//Not Voteable, Not in rotation
/////////////////////////////////////////////////////////////////////

//addRotationMap("EscaladeLak",					"Lakrabbit",1,0,-1,32);
//addRotationMap("MagmaticLak",					"Lakrabbit",1,0,-1,32);
//addRotationMap("HillsOfSorrow",				"Lakrabbit",1,0,-1,32);
//addRotationMap("TWL2_MuddySwampLak",			"Lakrabbit",1,0,-1,32);
//addRotationMap("SandStormLak",				"Lakrabbit",1,0,-1,32);
//addRotationMap("BeggarsRunLak",				"Lakrabbit",1,0,-1,32);
//addRotationMap("DamnnationLak",				"Lakrabbit",1,0,-1,32);


//  _      _____ _______ ______ 
// | |    / ____|__   __|  ____|
// | |   | |       | |  | |__   
// | |   | |       | |  |  __|  
// | |___| |____   | |  | |     
// |______\_____|  |_|  |_|     
/////////////////////////////////////////////////////////////////////

//In Rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("BastardForgeLT",				"sctf",1,1,-1,32);
//addRotationMap("FirestormLT",					"sctf",1,1,-1,32);
addRotationMap("DangerousCrossingLT",			"sctf",1,1,-1,32);
addRotationMap("SmallCrossingLT",				"sctf",1,0,-1,12);
addRotationMap("DireLT",						"sctf",1,1,-1,32);
addRotationMap("RoundTheMountainLT",			"sctf",1,1,-1,32);
addRotationMap("CirclesEdgeLT",					"sctf",1,1,-1,32);
addRotationMap("TenebrousCTF",					"sctf",1,1,-1,32);
addRotationMap("TheFray",						"sctf",1,0,-1,32);
addRotationMap("SignalLT",						"sctf",1,1,-1,32);
addRotationMap("StarFallLT",					"sctf",1,1,-1,32);
addRotationMap("S5_DamnationLT",				"sctf",1,1,-1,32);
addRotationMap("S5_Icedance",					"sctf",1,1,-1,32);
addRotationMap("S5_Mordacity",					"sctf",1,1,-1,32);
addRotationMap("S5_SilenusLT",					"sctf",1,0,-1,32);
addRotationMap("TWL2_CanyonCrusadeDeluxeLT",	"sctf",1,1,-1,32);
addRotationMap("TWL2_FrozenHopeLT",				"sctf",1,1,-1,32);
//addRotationMap("TWL2_JaggedClawLT",			"sctf",1,1,-1,32);
addRotationMap("TWL2_HildebrandLT",				"sctf",1,1,-1,32);
addRotationMap("TWL2_SkylightLT",				"sctf",1,1,-1,32);
//addRotationMap("TWL_BeachBlitzLT",			"sctf",1,1,-1,32);
addRotationMap("TWL_FeignLT",					"sctf",1,0,-1,32);
addRotationMap("TWL_RollercoasterLT",			"sctf",1,1,-1,32);
addRotationMap("TWL_StonehengeLT",				"sctf",1,1,-1,32);
addRotationMap("TWL_WilderZoneLT",				"sctf",1,1,-1,32);
addRotationMap("oasisintensity",				"sctf",1,1,-1,10);
addRotationMap("berlard",						"sctf",1,0,-1,32);
addRotationMap("RaindanceLT",					"sctf",1,1,-1,32);
addRotationMap("SmallTimeLT",					"sctf",1,0,-1,32);
addRotationMap("ArenaDome",						"sctf",1,1,-1,32);
addRotationMap("Bulwark",						"sctf",1,1,-1,32);
addRotationMap("Discord",						"sctf",1,1,-1,32);
//addRotationMap("JadeValley",					"sctf",1,1,-1,32);
addRotationMap("S5_MassiveLT",					"sctf",1,1,-1,32);
addRotationMap("Blink",							"sctf",1,1,-1,32);
addRotationMap("HillSideLT",					"sctf",1,1,-1,12);
addRotationMap("IcePick",						"sctf",1,1,-1,16);
addRotationMap("OsIrisLT",						"sctf",1,1,-1,32);
addRotationMap("GrassyKnoll",					"sctf",1,1,-1,32);
addRotationMap("TWL2_MuddySwamp",				"sctf",1,1,8,32);
addRotationMap("SandyRunLT",					"sctf",1,1,-1,12);

//Voteable, But not in rotation
/////////////////////////////////////////////////////////////////////

addRotationMap("CamellandLT",					"sctf",1,1,-1,32);
//addRotationMap("Headstone",					"sctf",1,0,-1,32);
//addRotationMap("Mirage",						"sctf",1,0,-1,32);
addRotationMap("BeggarsRunLT",					"sctf",1,0,-1,32);
//addRotationMap("S5_HawkingHeat",				"sctf",1,0,-1,32);
//addRotationMap("S5_Mimicry",					"sctf",1,0,-1,32);
addRotationMap("S5_Woodymyrk",					"sctf",1,0,-1,32);
//addRotationMap("S8_Cardiac",					"sctf",1,0,-1,32);
//addRotationMap("TWL2_Celerity",				"sctf",1,0,-1,32);
//addRotationMap("TWL2_Crevice",				"sctf",1,0,-1,32);
addRotationMap("S8_Opus",						"sctf",1,0,-1,32);
//addRotationMap("TWL2_MidnightMayhemDeluxe",	"sctf",1,0,-1,32);
//addRotationMap("TWL2_Ocular",					"sctf",1,0,-1,32);
//addRotationMap("TWL_Cinereous",				"sctf",1,0,-1,32);
//addRotationMap("TWL_Deserted",				"sctf",1,0,-1,32);
addRotationMap("TWL_DangerousCrossing",			"sctf",1,0,-1,32);
//addRotationMap("TWL_OsIris",					"sctf",1,0,-1,32);
//addRotationMap("TWL_Damnation",				"sctf",1,0,-1,32);
//addRotationMap("TWL_Titan",					"sctf",1,0,-1,32);

//Not Voteable, Not in rotation
/////////////////////////////////////////////////////////////////////

//addRotationMap("SurrealLT",					"sctf",1,1,-1,32);
//addRotationMap("Coppersky",					"sctf",1,1,-1,32);
addRotationMap("DuelersDelight",				"sctf",1,0,-1,12);
//addRotationMap("SuperHappyBouncyFunTime",		"sctf",1,1,-1,32);
addRotationMap("PariahLT",						"sctf",1,1,-1,16);
//addRotationMap("SmallMelee",					"sctf",1,1,-1,32);
//addRotationMap("TitForTat",					"sctf",1,1,-1,32);
//addRotationMap("CloseCombatLT",				"sctf",1,1,-1,32);
//addRotationMap("Prismatic",					"sctf",1,1,-1,32);
//addRotationMap("Damnation",					"sctf",1,1,-1,32);
//addRotationMap("DustToDust",					"sctf",1,1,-1,32);
//addRotationMap("Minotaur",					"sctf",1,1,-1,32);
//addRotationMap("DesertofDeath_nef",			"sctf",1,1,-1,32);
//addRotationMap("Gorgon",						"sctf",1,1,-1,32);
//addRotationMap("Titan",						"sctf",1,1,-1,32);
//addRotationMap("Mac_FlagArena",				"sctf",1,1,-1,32);
//addRotationMap("Extractor",					"sctf",1,1,-1,32);
//addRotationMap("AstersDescent",				"sctf",1,1,-1,32);
//addRotationMap("Azoth",						"sctf",1,1,-1,32);
//addRotationMap("DustLust",					"sctf",1,1,-1,32);
//addRotationMap("Disjointed",					"sctf",1,1,-1,32);
//addRotationMap("Peak",						"sctf",1,1,-1,32);
//addRotationMap("Snowcone",					"sctf",1,1,-1,32);
//addRotationMap("S5_Centaur",					"sctf",1,1,-1,32);
//addRotationMap("S5_Drache",					"sctf",1,1,-1,32);
//addRotationMap("S5_Misadventure",				"sctf",1,1,-1,32);
//addRotationMap("S5_Reynard",					"sctf",1,1,-1,32);
//addRotationMap("S5_Sherman",					"sctf",1,1,-1,32);
//addRotationMap("S8_Geothermal",				"sctf",1,1,-1,32);
//addRotationMap("S8_Zilch",					"sctf",1,1,-1,32);
//addRotationMap("TWL2_Drifts",					"sctf",1,1,-1,32);
//addRotationMap("TWL2_Drorck",					"sctf",1,1,-1,32);
//addRotationMap("TWL2_FrozenGlory",			"sctf",1,1,-1,32);
//addRotationMap("TWL2_IceDagger",				"sctf",1,1,-1,32);
//addRotationMap("TWL2_Norty",					"sctf",1,1,-1,32);
//addRotationMap("TWL2_RoughLand",				"sctf",1,1,-1,32);
//addRotationMap("TWL2_Ruined",					"sctf",1,1,-1,32);
//addRotationMap("TWL_BaNsHee",					"sctf",1,1,-1,32);
//addRotationMap("TWL_Boss",					"sctf",1,1,-1,32);
//addRotationMap("TWL_Crossfire",				"sctf",1,1,-1,32);
//addRotationMap("TWL_NoShelter",				"sctf",1,1,-1,32);
//addRotationMap("TWL_Clusterfuct",				"sctf",1,1,-1,32);
//addRotationMap("TWL_Curtilage",				"sctf",1,1,-1,32);
//addRotationMap("TWL_DeadlyBirdsSong",			"sctf",1,1,-1,32);
//addRotationMap("TWL_Frostclaw",				"sctf",1,1,-1,32);
//addRotationMap("TWL_Magamatic",				"sctf",1,1,-1,32);
//addRotationMap("TWL_Neve",					"sctf",1,1,-1,32);
//addRotationMap("TWL_Pandemonium",				"sctf",1,1,-1,32);
//addRotationMap("TWL_Ramparts",				"sctf",1,1,-1,32);
//addRotationMap("DehSwamp",					"sctf",1,1,-1,32);
//addRotationMap("HostileLoch",					"sctf",1,1,-1,32);
//addRotationMap("DevilsElbow",					"sctf",1,1,-1,32);
//addRotationMap("SmallDesertofDeath",			"sctf",1,1,-1,32);
//addRotationMap("ShortFall",					"sctf",1,1,-1,32);
//addRotationMap("Fallout",						"sctf",1,1,-1,32);
//addRotationMap("SoylentGreen",				"sctf",1,1,-1,32);
//addRotationMap("Island",						"sctf",1,1,-1,32);
//addRotationMap("HighOctane",					"sctf",1,1,-1,32);

//  _____             _   _                     _       _     
// |  __ \           | | | |                   | |     | |    
// | |  | | ___  __ _| |_| |__  _ __ ___   __ _| |_ ___| |__  
// | |  | |/ _ \/ _` | __| '_ \| '_ ` _ \ / _` | __/ __| '_ \ 
// | |__| |  __/ (_| | |_| | | | | | | | | (_| | || (__| | | |
// |_____/ \___|\__,_|\__|_| |_|_| |_| |_|\__,_|\__\___|_| |_|
/////////////////////////////////////////////////////////////////////

addRotationMap("RaspDM",						"DM",1,1,8,32);
addRotationMap("EntombedDM",					"DM",1,1,-1,32);
addRotationMap("IceDomeDM",						"DM",1,1,-1,32);
addRotationMap("HoofToeDM",						"DM",1,1,6,32);
addRotationMap("ArenaDomeDM",					"DM",1,1,-1,32);
addRotationMap("VulcansWrathDM",				"DM",1,1,-1,32);
addRotationMap("RampartsDM",					"DM",1,1,-1,32);
addRotationMap("ShrineDM",						"DM",1,1,-1,12);
addRotationMap("LiveBaitDM",					"DM",1,1,-1,32);
addRotationMap("FourSquareDM",					"DM",1,1,-1,10);
addRotationMap("BrigDM",						"DM",1,1,-1,8);



//  _____             _ 
// |  __ \           | |
// | |  | |_   _  ___| |
// | |  | | | | |/ _ \ |
// | |__| | |_| |  __/ |
// |_____/ \__,_|\___|_|
/////////////////////////////////////////////////////////////////////

//addRotationMap("AgentsOfFortune",				"Duel",1,1,-1,32);
//addRotationMap("Casern_Cavite",				"Duel",1,1,-1,32);
//addRotationMap("Equinox",						"Duel",1,1,-1,32);
//addRotationMap("Escalade",					"Duel",1,1,-1,32);
//addRotationMap("Fracas",						"Duel",1,1,-1,32);
//addRotationMap("Invictus",					"Duel",1,1,-1,32);
//addRotationMap("MyrkWood",					"Duel",1,1,-1,32);
//addRotationMap("Oasis",						"Duel",1,1,-1,32);
//addRotationMap("Pyroclasm",					"Duel",1,1,-1,32);
//addRotationMap("Rasp",						"Duel",1,1,-1,32);
//addRotationMap("SunDried",					"Duel",1,1,-1,32);
//addRotationMap("Talus",						"Duel",1,1,-1,32);
//addRotationMap("Underhill",					"Duel",1,1,-1,32);
//addRotationMap("Whiteout",					"Duel",1,1,-1,32);
//addRotationMap("Tombstone",					"Duel",1,1,-1,32);
//addRotationMap("VaubanLak",					"Duel",1,1,-1,32);
//addRotationMap("DesertofDeathLak",			"Duel",1,1,-1,32);



//   _____ _                 
//  / ____(_)                
// | (___  _  ___  __ _  ___ 
//  \___ \| |/ _ \/ _` |/ _ \
//  ____) | |  __/ (_| |  __/
// |_____/|_|\___|\__, |\___|
//                 __/ |     
//                |___/    
/////////////////////////////////////////////////////////////////////

addRotationMap("Alcatraz",					"Siege",1,1,-1,32);
addRotationMap("BridgeTooFar",				"Siege",1,1,-1,32);
addRotationMap("Caldera",					"Siege",1,1,-1,32);
addRotationMap("Gauntlet",					"Siege",1,1,-1,32);
addRotationMap("IceBound",					"Siege",1,1,-1,32);
addRotationMap("Isleofman",					"Siege",1,1,-1,32);
addRotationMap("Masada",					"Siege",1,1,-1,32);
addRotationMap("Respite",					"Siege",1,1,-1,32);
addRotationMap("Trident",					"Siege",1,1,-1,32);
addRotationMap("UltimaThule",				"Siege",1,1,-1,32);

