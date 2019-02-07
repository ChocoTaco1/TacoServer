// Use of this file
// To add a new map follow this rules:
// 1. Find the file name, ending in .mis (ie BeggarsRun.mis)
// 2. Add a line with this syntact: addRotationMap(filename, gametype, %ffa, %cycle);
//
// %ffa = changing this to 0, will remove the map from the FFA mode
// %cycle = changing this to 0 will remove the map from the FFA cycle, but not from the rotation (optional)
//                                if %ffa is 0, %cycle is useless
//
// Note: filename must be written without .mis
//
// Example: addRotationMap(BeggarsRun, CTF, 1, 1);
//                                Beggar's Run will be in the FFA mapRotation. It's in the cycle and can be voted
//
// Example: addRotationMap(BeggarsRun, CTF, 1, 0);
//                                Beggar's Run will be in the FFA mapRotation. It's not in the cycle, but can be voted
//
// Example: addRotationMap(BeggarsRun, CTF, 0);
//                                Beggar's Run won't be in the FFA mapRotation. It's not in the cycle and can't be voted
//
// Info:
// If you don't want a map to be played neither in Tournement Mode, write // at the beginning of the line (or delete the line)
// If you don't want a gametype to be played, just don't add any map of that gametype
// Some Mods (ie LakLakRabbit) use own maps. To use this mods, you must add the maps in this file
// %cycle variable is optional and if not included the map will always be in the cycle (if %ffa is 1).


// ******************************
// * Capture The Flag Maps *
// ******************************

addRotationMap("SmallCrossing", CTF, 1, 1);
addRotationMap("HighOctane", CTF, 1, 1);
addRotationMap("TWL2_CanyonCrusadeDeluxe", CTF, 1, 1);
addRotationMap("S5_Mordacity", CTF, 1, 1);
addRotationMap("RoundTheMountain", CTF, 1, 1);
addRotationMap("S5_Damnation", CTF, 1, 1);
addRotationMap("TWL2_JaggedClaw", CTF, 1, 1);
addRotationMap("S5_Massive", CTF, 1, 1);
addRotationMap("TWL_Stonehenge", CTF, 1, 1);
addRotationMap("TWL_Feign", CTF, 1, 1);
addRotationMap("TheFray", CTF, 1, 1);
addRotationMap("oasisintensity", CTF, 1, 1);
addRotationMap("DangerousCrossing_nef", CTF, 1, 1);
addRotationMap("TWL2_Skylight", CTF, 1, 1);
addRotationMap("SmallTimeCTF", CTF, 1, 1);
addRotationMap("TWL2_Hildebrand", CTF, 1, 1);
addRotationMap("TWL2_Ocular", CTF, 1, 1);
addRotationMap("Dire", CTF, 1, 1);
addRotationMap("berlard", CTF, 1, 0);
addRotationMap("HostileLoch", CTF, 1, 1);
addRotationMap("Minotaur", CTF, 1, 0);
addRotationMap("S8_Opus", CTF, 1, 1);
addRotationMap("BeggarsRun", CTF, 1, 0);
addRotationMap("TitForTat", CTF, 1, 0);
addRotationMap("Surreal", CTF, 1, 1);
addRotationMap("Signal", CTF, 1, 1);
addRotationMap("Headstone", CTF, 1, 1);
addRotationMap("Fenix", CTF, 1, 0);
addRotationMap("Mac_FlagArena", CTF, 1, 0);
addRotationMap("S5_Centaur", CTF, 1, 1);
addRotationMap("S8_Cardiac", CTF, 1, 1);
addRotationMap("CirclesEdge", CTF, 1, 1);
addRotationMap("S5_Icedance", CTF, 1, 1);
addRotationMap("Bulwark", CTF, 1, 1);
addRotationMap("S5_Woodymyrk", CTF, 1, 0);
addRotationMap("Discord", CTF, 1, 1);
addRotationMap("NatureMagic", CTF, 1, 0);
addRotationMap("TenebrousCTF", CTF, 1, 0);
addRotationMap("Pariah", CTF, 1, 0);
addRotationMap("Prismatic", CTF, 1, 1);
addRotationMap("TWL_WilderZone", CTF, 1, 1);
addRotationMap("Mirage", CTF, 1, 1);
addRotationMap("S5_Mimicry", CTF, 1, 1);
addRotationMap("Infernus", CTF, 1, 0);
//addRotationMap("TWL_Snowblind", CTF, 1, 1);
addRotationMap("SmallMelee", CTF, 1, 0);
addRotationMap("TWL2_Celerity", CTF, 1, 0);
addRotationMap("TWL_BeachBlitz", CTF, 1, 1);
addRotationMap("TWL2_Magnum", CTF, 1, 1);
addRotationMap("Logans_Run", CTF, 1, 1);
addRotationMap("Rollercoaster_nef", CTF, 1, 1);
addRotationMap("MoonDance", CTF, 1, 1);
addRotationMap("Raindance_nef", CTF, 1, 1);
addRotationMap("Circleofstones", CTF, 1, 1);
//addRotationMap("Hillside", CTF, 1, 1);
addRotationMap("TWL2_RoughLand", CTF, 1, 0);
addRotationMap("S8_Geothermal", CTF, 1, 0);
//addRotationMap("Lakefront", CTF, 1, 1);
//addRotationMap("TWL_Magamatic", CTF, 1, 1);
addRotationMap("TWL2_FrozenGlory", CTF, 1, 1);
addRotationMap("ShockRidge", CTF, 1, 0);
addRotationMap("TWL2_Bleed", CTF, 1, 0);
addRotationMap("TWL2_BlueMoon", CTF, 1, 0);
addRotationMap("Blastside_nef", CTF, 1, 0);
addRotationMap("ShortFall", CTF, 1, 0);
addRotationMap("ArenaDome", CTF, 1, 1);
addRotationMap("TWL2_MidnightMayhemDeluxe", CTF, 1, 0);
//addRotationMap("IceRidge_nef", CTF, 1, 0);
addRotationMap("Ramparts", CTF, 1, 0);
//addRotationMap("Sandstorm", CTF, 1, 0);
//addRotationMap("Scarabrae_nef", CTF, 0, 0);
//addRotationMap("Starfallen", CTF, 1, 0);
//addRotationMap("Stonehenge_nef", CTF, 1, 0);
addRotationMap("TitanV", CTF, 1, 0);
//addRotationMap("Extractor", CTF, 0, 0);
//addRotationMap("AstersDescent", CTF, 1, 0);
//addRotationMap("Azoth", CTF, 0, 0);
//addRotationMap("BattleGrove", CTF, 0, 0);
//addRotationMap("BerylBasin", CTF, 1, 0);
//addRotationMap("Durango", CTF, 0, 0);
//addRotationMap("DustLust", CTF, 0, 0);
//addRotationMap("Island", CTF, 1, 0);
//addRotationMap("Disjointed", CTF, 0, 0);
//addRotationMap("FullCircle", CTF, 1, 0);
//addRotationMap("IceGulch", CTF, 0, 0);
addRotationMap("JadeValley", CTF, 1, 0);
//addRotationMap("MountainMist", CTF, 0, 0);
//addRotationMap("Peak", CTF, 0, 0);
//addRotationMap("Pendulum", CTF, 0, 0);
//addRotationMap("Snowcone", CTF, 0, 0);
//addRotationMap("S5_Drache", CTF, 1, 0);
addRotationMap("S5_HawkingHeat", CTF, 1, 0);
//addRotationMap("S5_Misadventure", CTF, 1, 0);
addRotationMap("S5_Reynard", CTF, 1, 0);
//addRotationMap("S5_Sherman", CTF, 1, 0);
addRotationMap("S5_Silenus", CTF, 1, 0);
//addRotationMap("S8_CentralDogma", CTF, 0, 0);
//addRotationMap("S8_Mountking", CTF, 0, 0);
//addRotationMap("S8_Zilch", CTF, 0, 0);
//addRotationMap("TWL2_CloakOfNight", CTF, 1, 0);
addRotationMap("TWL2_Crevice", CTF, 1, 0);
//addRotationMap("TWL2_Dissention", CTF, 0, 0);
//addRotationMap("TWL2_Drifts", CTF, 1, 0);
//addRotationMap("TWL2_Drorck", CTF, 0, 0);
addRotationMap("TWL2_FrozenHope", CTF, 1, 0);
addRotationMap("TWL2_IceDagger", CTF, 1, 0);
//addRotationMap("TWL2_MuddySwamp", CTF, 1, 0);
//addRotationMap("TWL2_Norty", CTF, 1, 0);
//addRotationMap("TWL2_Ruined", CTF, 1, 0);
//addRotationMap("TWL_Abaddon", CTF, 0, 0);
//addRotationMap("TWL_BaNsHee", CTF, 0, 0);
//addRotationMap("TWL_Boss", CTF, 0, 0);
//addRotationMap("TWL_Chokepoint", CTF, 1, 0);
addRotationMap("TWL_Cinereous", CTF, 1, 0);
addRotationMap("TWL_Crossfire", CTF, 1, 0);
addRotationMap("TWL_NoShelter", CTF, 1, 0);
addRotationMap("TWL_OsIris", CTF, 1, 0);
//addRotationMap("TWL_Clusterfuct", CTF, 0, 0);
//addRotationMap("TWL_Curtilage", CTF, 0, 0);
addRotationMap("TWL_Damnation", CTF, 1, 0);
addRotationMap("TWL_DangerousCrossing", CTF, 1, 0);
//addRotationMap("TWL_DeadlyBirdsSong", CTF, 1, 0);
addRotationMap("TWL_Deserted", CTF, 1, 0);
//addRotationMap("TWL_Frostclaw", CTF, 1, 0);
//addRotationMap("TWL_Frozen", CTF, 1, 0);
//addRotationMap("TWL_Harvester", CTF, 0, 0);
//addRotationMap("TWL_Horde", CTF, 0, 0);
addRotationMap("TWL_Katabatic", CTF, 1, 0);
//addRotationMap("TWL_Neve", CTF, 0, 0);
//addRotationMap("TWL_Pandemonium", CTF, 0, 0);
//addRotationMap("TWL_Ramparts", CTF, 0, 0);
//addRotationMap("TWL_Sandstorm", CTF, 0, 0);
//addRotationMap("TWL_Starfallen", CTF, 1, 0);
addRotationMap("TWL_SubZero", CTF, 1, 0);
addRotationMap("TWL_Titan", CTF, 1, 0);
//addRotationMap("TWL_WoodyMyrk", CTF, 1, 0);
addRotationMap("Vauban", CTF, 1, 1);
addRotationMap("Glade", CTF, 1, 0);
addRotationMap("WindyGap", CTF, 1, 0);
//addRotationMap("OctoberRust", CTF, 0, 0);
//addRotationMap("DevilsElbow", CTF, 0, 0);
//addRotationMap("Pantheon", CTF, 1, 0);
//addRotationMap("Coppersky", CTF, 1, 0);
addRotationMap("SuperHappyBouncyFunTime", CTF, 1, 0);
//addRotationMap("CloudCity", CTF, 0, 0);
//addRotationMap("ConstructionYard", CTF, 0, 0);
//addRotationMap("Archipelago", CTF, 0, 0);
//addRotationMap("Damnation", CTF, 0, 0);
//addRotationMap("DeathBirdsFly", CTF, 0, 0);
//addRotationMap("Desiccator", CTF, 0, 0);
//addRotationMap("DustToDust", CTF, 0, 0);
addRotationMap("Firestorm", CTF, 1, 0);
//addRotationMap("Katabatic", CTF, 0, 0);
//addRotationMap("Quagmire", CTF, 0, 0);
//addRotationMap("Recalescence", CTF, 0, 0);
//addRotationMap("Reversion", CTF, 0, 0);
//addRotationMap("RiverDance", CTF, 1, 0);
//addRotationMap("Sanctuary", CTF, 0, 0);
//addRotationMap("Slapdash", CTF, 0, 0);
//addRotationMap("ThinIce", CTF, 0, 0);
//addRotationMap("Tombstone", CTF, 0, 0);
//addRotationMap("AcidRain", CTF, 1, 0);
//addRotationMap("Broadside_nef", CTF, 0, 0);
//addRotationMap("Confusco", CTF, 0, 0);
//addRotationMap("Camelland", CTF, 0, 0);
addRotationMap("SandOcean", CTF, 1, 0);
//addRotationMap("HighTrepidation", CTF, 0, 0);
//addRotationMap("SmallDesertofDeath", CTF, 0, 0);
//addRotationMap("Agorazscium", CTF, 0, 0);
//addRotationMap("BasinFury", CTF, 0, 0);
//addRotationMap("Cadaver", CTF, 0, 0);
//addRotationMap("Fallout", CTF, 1, 0);
//addRotationMap("EivoItoxico", CTF, 0, 0);
//addRotationMap("Einfach", CTF, 0, 0);
addRotationMap("Nightdance", CTF, 1, 0);
addRotationMap("PicnicTable", CTF, 1, 0);
addRotationMap("TheClocktower", CTF, 1, 0);
addRotationMap("Surro", CTF, 1, 0);
//addRotationMap("StarIce", CTF, 1, 0);
addRotationMap("SoylentGreen", CTF, 1, 0);
addRotationMap("ks_braistv" , CTF, 1, 0);
//addRotationMap("Hostility", CTF, 0, 0);
//addRotationMap("HighWire", CTF, 0, 0);
addRotationMap("FilteredDust", CTF, 1, 0);
//addRotationMap("CloudBurst", CTF, 1, 0);
//addRotationMap("CloseCombat", CTF, 1, 0);
addRotationMap("Choke", CTF, 1, 0);
//addRotationMap("DesertofDeath_nef", CTF, 0, 0);
//addRotationMap("Gorgon", CTF, 1, 0);
addRotationMap("Magmatic", CTF, 1, 0);
addRotationMap("Sub-zero", CTF, 1, 0);
addRotationMap("Blink", CTF, 1, 1);


// ********************
// * LakRabbit Maps *
// ********************


addRotationMap("VaubanLak", LakRabbit, 1, 1);
addRotationMap("MiniSunDried", LakRabbit, 1, 1);
addRotationMap("Sundance", LakRabbit, 1, 1);
addRotationMap("TWL_BeachBlitzLak", LakRabbit, 1, 0);
addRotationMap("DesertofDeathLak", LakRabbit, 1, 1);
addRotationMap("TreasureIslandLak", LakRabbit, 1, 0);
addRotationMap("Raindance_nefLak", LakRabbit, 1, 1);
addRotationMap("SunDriedLak", LakRabbit, 1, 1);
addRotationMap("SkinnyDipLak", LakRabbit, 1, 1);
addRotationMap("SolsDescentLak", LakRabbit, 1, 0);
addRotationMap("Crater71Lak", LakRabbit, 1, 0);
addRotationMap("SaddiesHill", LakRabbit, 1, 1);
addRotationMap("GodsRiftLak", LakRabbit, 1, 0);
addRotationMap("HavenLak", LakRabbit, 1, 1);
addRotationMap("LushLak", LakRabbit, 1, 1);
addRotationMap("PhasmaDustLak", LakRabbit, 1, 0);
addRotationMap("BoxLak", LakRabbit, 1, 1);
//addRotationMap("DamnnationLak", LakRabbit, 1, 0);
addRotationMap("TitaniaLak", LakRabbit, 1, 1);
//addRotationMap("TWL2_MuddySwampLak", LakRabbit, 0, 0);
//addRotationMap("SandStormLak", LakRabbit, 1, 0);
//addRotationMap("BeggarsRunLak", LakRabbit, 1, 0);
//addRotationMap("Tibbaw", LakRabbit, 1, 0);
//addRotationMap("MagmaticLak", LakRabbit, 1, 0);
addRotationMap("HillsOfSorrow", LakRabbit, 1, 1);
//addRotationMap("EscaladeLak", LakRabbit, 1, 0);
addRotationMap("Arrakis", LakRabbit, 1, 1);
//addRotationMap("EquinoxLak", LakRabbit, 1, 0);
//addRotationMap("FrozenFuryLak", LakRabbit, 1, 0);
//addRotationMap("S8_GeothermalLak", LakRabbit, 1, 1);
addRotationMap("Sulfide", LakRabbit, 1, 0);






// ************************
// * DuelMod Maps *
// ************************

//addRotationMap("AgentsOfFortune", Duel, 0, 0);
//addRotationMap("Casern_Cavite", Duel, 0, 0);
//addRotationMap("Equinox", Duel, 1, 1);
//addRotationMap("Escalade", Duel, 1, 1);
//addRotationMap("Fracas", Duel, 0, 0);
//addRotationMap("Invictus", Duel, 0, 0);
//addRotationMap("MyrkWood", Duel, 1, 1);
//addRotationMap("Oasis", Duel, 1, 1);
//addRotationMap("Pyroclasm", Duel, 1, 1);
//addRotationMap("Rasp", Duel, 1, 1);
//addRotationMap("SunDried", Duel, 1, 1);
//addRotationMap("Talus", Duel, 0, 0);
//addRotationMap("Underhill", Duel, 0, 0);
//addRotationMap("Whiteout", Duel, 1, 1);
//addRotationMap("Tombstone", Duel, 1, 1);
//addRotationMap("VaubanLak", Duel, 1, 1);
//addRotationMap("DesertofDeathLak", Duel, 1, 1);





// ************************
// * SpawnCTF Maps *
// ************************

addRotationMap("BastardForgeLT", sctf, 1, 1);
addRotationMap("FirestormLT", sctf, 1, 1);
addRotationMap("DangerousCrossingLT", sctf, 1, 1);
addRotationMap("SmallCrossingLT", sctf, 1, 1);
addRotationMap("DireLT", sctf, 1, 1);
addRotationMap("RoundTheMountainLT", sctf, 1, 1);
addRotationMap("CirclesEdgeLT", sctf, 1, 1);
addRotationMap("TenebrousCTF", sctf, 1, 1);
addRotationMap("TheFray", sctf, 1, 1);
addRotationMap("SignalLT", sctf, 1, 1);
addRotationMap("StarFallLT", sctf, 1, 1);
addRotationMap("S5_DamnationLT", sctf, 1, 1);
addRotationMap("S5_Icedance", sctf, 1, 1);
addRotationMap("S5_Mordacity", sctf, 1, 1);
addRotationMap("S5_SilenusLT", sctf, 1, 1);
addRotationMap("TWL2_CanyonCrusadeDeluxeLT", sctf, 1, 1);
addRotationMap("TWL2_FrozenHopeLT", sctf, 1, 1);
addRotationMap("TWL2_JaggedClawLT", sctf, 1, 1);
addRotationMap("TWL2_HildebrandLT", sctf, 1, 1);
addRotationMap("TWL2_SkylightLT", sctf, 1, 1);
addRotationMap("TWL_BeachBlitzLT", sctf, 1, 1);
addRotationMap("TWL_FeignLT", sctf, 1, 1);
addRotationMap("TWL_RollercoasterLT", sctf, 1, 1);
addRotationMap("TWL_StonehengeLT", sctf, 1, 1);
addRotationMap("TWL_WilderZoneLT", sctf, 1, 1);
addRotationMap("oasisintensity", sctf, 1, 1);
addRotationMap("berlard", sctf, 1, 1);
//addRotationMap("SurrealLT", sctf, 1, 1);
addRotationMap("RaindanceLT", sctf, 1, 1);
//addRotationMap("Coppersky", sctf, 1, 1);
//addRotationMap("DuelersDelight", sctf, 1, 1);
//addRotationMap("SuperHappyBouncyFunTime", sctf, 1, 0);
addRotationMap("SmallTimeLT", sctf, 1, 1);
//addRotationMap("PariahLT", sctf, 1, 1);
//addRotationMap("SmallMelee", sctf, 1, 1);
addRotationMap("ArenaDome", sctf, 1, 1);
addRotationMap("Bulwark", sctf, 1, 1);
addRotationMap("Discord", sctf, 1, 1);
//addRotationMap("TitForTat", sctf, 1, 1);
//addRotationMap("CloseCombatLT", sctf, 1, 1);
//addRotationMap("Prismatic", sctf, 1, 1);
addRotationMap("JadeValley", sctf, 1, 1);
addRotationMap("BeggarsRunLT", sctf, 1, 0);
//addRotationMap("Damnation", sctf, 0, 0);
//addRotationMap("DustToDust", sctf, 0, 0);
//addRotationMap("Minotaur", sctf, 0, 0);
//addRotationMap("DesertofDeath_nef", sctf, 0, 0);
//addRotationMap("Gorgon", sctf, 0, 0);
//addRotationMap("Titan", sctf, 0, 0);
//addRotationMap("Mac_FlagArena", sctf, 0, 0);
//addRotationMap("Extractor", sctf, 0, 0);
//addRotationMap("AstersDescent", sctf, 0, 0);
//addRotationMap("Azoth", sctf, 0, 0);
//addRotationMap("DustLust", sctf, 0, 0);
//addRotationMap("Disjointed", sctf, 0, 0);
addRotationMap("Headstone", sctf, 1, 0);
addRotationMap("Mirage", sctf, 1, 0);
//addRotationMap("Peak", sctf, 0, 0);
//addRotationMap("Snowcone", sctf, 0, 0);
//addRotationMap("S5_Centaur", sctf, 0, 0);
//addRotationMap("S5_Drache", sctf, 0, 0);
addRotationMap("S5_HawkingHeat", sctf, 1, 0);
addRotationMap("S5_MassiveLT", sctf, 1, 1);
addRotationMap("S5_Mimicry", sctf, 1, 0);
//addRotationMap("S5_Misadventure", sctf, 0, 0);
//addRotationMap("S5_Reynard", sctf, 0, 0);
//addRotationMap("S5_Sherman", sctf, 0, 0);
addRotationMap("S5_Woodymyrk", sctf, 1, 0);
addRotationMap("S8_Cardiac", sctf, 1, 0);
//addRotationMap("S8_Geothermal", sctf, 0, 0);
addRotationMap("S8_Opus", sctf, 1, 0);
//addRotationMap("S8_Zilch", sctf, 0, 0);
addRotationMap("TWL2_Celerity", sctf, 1, 0);
addRotationMap("TWL2_Crevice", sctf, 1, 0);
//addRotationMap("TWL2_Drifts", sctf, 0, 0);
//addRotationMap("TWL2_Drorck", sctf, 0, 0);
//addRotationMap("TWL2_FrozenGlory", sctf, 0, 0);
//addRotationMap("TWL2_IceDagger", sctf, 0, 0);
addRotationMap("TWL2_MidnightMayhemDeluxe", sctf, 1, 0);
//addRotationMap("TWL2_MuddySwamp", sctf, 0, 0);
//addRotationMap("TWL2_Norty", sctf, 0, 0);
addRotationMap("TWL2_Ocular", sctf, 1, 0);
//addRotationMap("TWL2_RoughLand", sctf, 0, 0);
//addRotationMap("TWL2_Ruined", sctf, 0, 0);
//addRotationMap("TWL_BaNsHee", sctf, 0, 0);
//addRotationMap("TWL_Boss", sctf, 0, 0);
addRotationMap("TWL_Cinereous", sctf, 1, 0);
//addRotationMap("TWL_Crossfire", sctf, 0, 0);
addRotationMap("TWL_DangerousCrossing", sctf, 1, 0);
//addRotationMap("TWL_NoShelter", sctf, 0, 0);
addRotationMap("TWL_OsIris", sctf, 1, 0);
//addRotationMap("TWL_Clusterfuct", sctf, 0, 0);
//addRotationMap("TWL_Curtilage", sctf, 0, 0);
addRotationMap("TWL_Damnation", sctf, 1, 0);
//addRotationMap("TWL_DeadlyBirdsSong", sctf, 0, 0);
addRotationMap("TWL_Deserted", sctf, 1, 0);
//addRotationMap("TWL_Frostclaw", sctf, 0, 0);
//addRotationMap("TWL_Magamatic", sctf, 0, 0);
//addRotationMap("TWL_Neve", sctf, 0, 0);
//addRotationMap("TWL_Pandemonium", sctf, 0, 0);
//addRotationMap("TWL_Ramparts", sctf, 0, 0);
addRotationMap("TWL_Titan", sctf, 1, 0);
//addRotationMap("DehSwamp", sctf, 0, 0);
//addRotationMap("HostileLoch", sctf, 0, 0);
//addRotationMap("DevilsElbow", sctf, 0, 0);
//addRotationMap("Camelland", sctf, 0, 0);
//addRotationMap("SmallDesertofDeath", sctf, 0, 0);
//addRotationMap("ShortFall", sctf, 0, 0);
//addRotationMap("Fallout", sctf, 0, 0);
//addRotationMap("SoylentGreen", sctf, 0, 0);
//addRotationMap("Island", sctf, 0, 0);
//addRotationMap("HighOctane", sctf, 0, 0);
addRotationMap("Blink", sctf, 1, 1);



// ************************
// * Siege Maps *
// ************************

//addRotationMap("Isleofman", Siege, 1, 1);
//addRotationMap("Trident", Siege, 1, 1);
//addRotationMap("Alcatraz", Siege, 1, 1);




// ************************
// * DM Maps *
// ************************

addRotationMap("RaspDM", DM, 1, 1);
//addRotationMap("Mac_FlagArena", DM, 1, 1);
addRotationMap("ArenaDomeDM", DM, 1, 1);
//addRotationMap("SmallCrossingLT", DM, 1, 1);
//addRotationMap("MiniSunDried", DM, 1, 1);