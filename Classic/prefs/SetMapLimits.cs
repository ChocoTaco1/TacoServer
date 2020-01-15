//  __  __               _      _           _ _       
// |  \/  |             | |    (_)         (_) |      
// | \  / | __ _ _ __   | |     _ _ __ ___  _| |_ ___ 
// | |\/| |/ _` | '_ \  | |    | | '_ ` _ \| | __/ __|
// | |  | | (_| | |_) | | |____| | | | | | | | |_\__ \
// |_|  |_|\__,_| .__/  |______|_|_| |_| |_|_|\__|___/
//              | |                                   
//              |_|  

// To set when tribes can select a map(based on the current population of the server) when looking thru the rotation for a suitable Next map.
// The First number is the minimum. The Second number is the maximum. 
//
// $Host::MapPlayerLimitsAcidRain_CTF = "8 32";
//
// This Example shows a map with the mimimum of 8 for the map to be selected, and a max of 32.
// Furthermore, if you did not want to have a minimum or maximum you can just set these values to -1.
//
// To see if a Limit has indeed loaded.
// echo( $Host::MapPlayerLimitsSmallCrossing_CTF );
// echo( $Host::MapPlayerLimitsHighOctane_CTF );
// echo( $Host::MapPlayerLimitsSnowcone_CTF );
//
// %min SPC %max is just the preset %min SPC(Space) %max in the "Set Values for this Group" section
// "8 32" or "-1 -1" or "-1 16" will also work. Just dont forget the quotes.
// As the list goes down %min %max will stay the same unless you change them (a new %min = ? %max = ?) etc

// This is done so a crash cant occur using local variables
// Using exec( $Host::EvoCustomMapLimitsFile ); in console
function setmaps()
{

//   _____ _______ ______ 
//  / ____|__   __|  ____|
// | |       | |  | |__   
// | |       | |  |  __|  
// | |____   | |  | |     
//  \_____|  |_|  |_|  
/////////////////////////////////////////////////////////////////////

//Small Maps
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = -1;
%max = 18;

$Host::MapPlayerLimitsSmallCrossing_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_CanyonCrusadeDeluxe_CTF = %min SPC %max;
$Host::MapPlayerLimitsRoundTheMountain_CTF = %min SPC "14";
$Host::MapPlayerLimitsoasisintensity_CTF = %min SPC "12";
$Host::MapPlayerLimitsMinotaur_CTF = %min SPC %max;
$Host::MapPlayerLimitsIsland_CTF = %min SPC %max;
$Host::MapPlayerLimitsTitForTat_CTF = %min SPC %max;
$Host::MapPlayerLimitsSmallMelee_CTF = %min SPC "12";
$Host::MapPlayerLimitsSuperHappyBouncyFunTime_CTF = %min SPC "10";
$Host::MapPlayerLimitsMachineeggs_CTF = %min SPC "10";
$Host::MapPlayerLimitsMac_FlagArena_CTF = %min SPC "12";
$Host::MapPlayerLimitsSmallTimeCTF_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Hildebrand_CTF = %min SPC %max;
$Host::MapPlayerLimitsArenaDome_CTF = %min SPC "10";
$Host::MapPlayerLimitsFirestorm_CTF = %min SPC %max;
$Host::MapPlayerLimitsBulwark_CTF = %min SPC "12";

//Medium Maps
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = 8;
%max = 32;

$Host::MapPlayerLimitsHighOctane_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Mordacity_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Damnation_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_JaggedClaw_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Massive_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Stonehenge_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Feign_CTF = %min SPC %max;
$Host::MapPlayerLimitsTheFray_CTF = %min SPC %max;
$Host::MapPlayerLimitsDangerousCrossing_nef_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Skylight_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Ocular_CTF = %min SPC %max;
$Host::MapPlayerLimitsDire_CTF = %min SPC %max;
$Host::MapPlayerLimitsberlard_CTF = %min SPC %max;
$Host::MapPlayerLimitsS8_Opus_CTF = %min SPC %max;
$Host::MapPlayerLimitsBeggarsRun_CTF = %min SPC %max;
$Host::MapPlayerLimitsSignal_CTF = %min SPC %max;
$Host::MapPlayerLimitsHeadstone_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Centaur_CTF = %min SPC %max;
$Host::MapPlayerLimitsS8_Cardiac_CTF = %min SPC %max;
$Host::MapPlayerLimitsCirclesEdge_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Icedance_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Woodymyrk_CTF = %min SPC %max;
$Host::MapPlayerLimitsDiscord_CTF = %min SPC %max;
$Host::MapPlayerLimitsTenebrousCTF_CTF = %min SPC %max;
$Host::MapPlayerLimitsPariah_CTF = %min SPC %max;
$Host::MapPlayerLimitsPrismatic_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_WilderZone_CTF = %min SPC %max;
$Host::MapPlayerLimitsMirage_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Mimicry_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Snowblind_CTF = %min SPC %max;
$Host::MapPlayerLimitsShortFall_CTF = %min SPC %max;
$Host::MapPlayerLimitsIceRidge_nef_CTF = %min SPC %max;
$Host::MapPlayerLimitsDisjointed_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_MuddySwamp_CTF = %min SPC %max;
$Host::MapPlayerLimitsBlink_CTF = %min SPC %max;
$Host::MapPlayerLimitsHighAnxiety_CTF = %min SPC %max;

//Voteable but Not in Rotation
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = 10;
%max = 32;

$Host::MapPlayerLimitsSnowcone_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Drache_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_HawkingHeat_CTF = %min SPC %max;
$Host::MapPlayerLimitsJadeValley_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Sherman_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Silenus_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_FrozenHope_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_IceDagger_CTF = %min SPC %max;
$Host::MapPlayerLimitsS5_Reynard_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Cinereous_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_OsIris_CTF = %min SPC %max;
$Host::MapPlayerLimitsCoppersky_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Crevice_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_SubZero_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Titan_CTF = %min SPC %max;
$Host::MapPlayerLimitsConfusco_CTF = %min SPC %max;
$Host::MapPlayerLimitsFallout_CTF = %min SPC %max;
$Host::MapPlayerLimitsTheClocktower_CTF = %min SPC %max;
$Host::MapPlayerLimitsSoylentGreen_CTF = %min SPC %max;
$Host::MapPlayerLimitsSurreal_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_MidnightMayhemDeluxe_CTF = %min SPC %max;
$Host::MapPlayerLimitsNightdance_CTF = %min SPC %max;
$Host::MapPlayerLimitsRamparts_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Celerity_CTF = %min SPC %max;
$Host::MapPlayerLimitsBlastside_nef_CTF = %min SPC %max;
$Host::MapPlayerLimitsInfernus_CTF = %min SPC %max;
$Host::MapPlayerLimitsNatureMagic_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Damnation_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_DangerousCrossing_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_DeadlyBirdsSong_CTF = %min SPC %max;
$Host::MapPlayerLimitsVauban_CTF = %min SPC %max;

//Vehicle Maps
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = 12;
%max = 32;

$Host::MapPlayerLimitsHostileLoch_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_BeachBlitz_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Magnum_CTF = %min SPC %max;
$Host::MapPlayerLimitsLogans_Run_CTF = %min SPC %max;
$Host::MapPlayerLimitsRollercoaster_nef_CTF = %min SPC %max;
$Host::MapPlayerLimitsMoonDance_CTF = %min SPC %max;
$Host::MapPlayerLimitsRaindance_nef_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Magamatic_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_FrozenGlory_CTF = %min SPC %max;
$Host::MapPlayerLimitsLandingParty_CTF = %min SPC %max;
$Host::MapPlayerLimitsTitanV_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Crossfire_CTF = %min SPC %max;
$Host::MapPlayerLimitsWindyGap_CTF = %min SPC %max;
$Host::MapPlayerLimitsSurro_CTF = %min SPC %max;
$Host::MapPlayerLimitsHarvestDance_CTF = %min SPC %max;
$Host::MapPlayerLimitsSubZeroV_CTF = %min SPC %max;
$Host::MapPlayerLimitsThe_Calm_CTF = %min SPC %max;

//Vehicle Maps: Voteable, But Not in Rotation
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = 12;
%max = 32;

$Host::MapPlayerLimitsTWL2_RoughLand_CTF = %min SPC %max;
$Host::MapPlayerLimitsS8_Geothermal_CTF = %min SPC %max;
$Host::MapPlayerLimitsLakefront_CTF = %min SPC %max;
$Host::MapPlayerLimitsShockRidge_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_BlueMoon_CTF = %min SPC %max;
$Host::MapPlayerLimitsFullCircle_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Katabatic_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Starfallen_CTF = %min SPC %max;
$Host::MapPlayerLimitsConstructionYard_CTF = %min SPC %max;
$Host::MapPlayerLimitsAcidRain_CTF = %min SPC %max;
$Host::MapPlayerLimitsSandOcean_CTF = %min SPC %max;
$Host::MapPlayerLimitsStarIce_CTF = %min SPC %max;
$Host::MapPlayerLimitsks_braistv_CTF = %min SPC %max;
$Host::MapPlayerLimitsFilteredDust_CTF = %min SPC %max;
$Host::MapPlayerLimitsChoke_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Ruined_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Chokepoint_CTF = %min SPC %max;
$Host::MapPlayerLimitsGlade_CTF = %min SPC %max;

//BIG Vehicle Maps
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = 14;
%max = 32;

$Host::MapPlayerLimitsFenix_CTF = "18" SPC %max;
$Host::MapPlayerLimitsHillside_CTF = "18" SPC %max;
$Host::MapPlayerLimitsSangre_de_Grado_CTF = %min SPC %max;
$Host::MapPlayerLimitsSlapdash_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Bleed_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Harvester_CTF = %min SPC %max;
$Host::MapPlayerLimitsArchipelago_CTF = %min SPC %max;
$Host::MapPlayerLimitsPantheon_CTF = %min SPC %max;
$Host::MapPlayerLimitsCircleofstones_CTF = %min SPC %max;
$Host::MapPlayerLimitsBerylBasin_CTF = %min SPC %max;
$Host::MapPlayerLimitsTWL_Frozen_CTF = %min SPC %max;

//Not In Rotation - Not Voteable
/////////////////////////////////////////////////////////////////////

//$Host::MapPlayerLimitsSandstorm_CTF = "8 32";
//$Host::MapPlayerLimitsScarabrae_nef_CTF = "8 32";
//$Host::MapPlayerLimitsStarfallen_CTF = "8 32";
//$Host::MapPlayerLimitsStonehenge_nef_CTF = "8 32";
//$Host::MapPlayerLimitsExtractor_CTF = "8 32";
//$Host::MapPlayerLimitsAstersDescent_CTF = "8 32";
//$Host::MapPlayerLimitsAzoth_CTF = "8 32";
//$Host::MapPlayerLimitsBattleGrove_CTF = "8 32";
//$Host::MapPlayerLimitsDurango_CTF = "8 32";
//$Host::MapPlayerLimitsDustLust_CTF = "8 32";
//$Host::MapPlayerLimitsIceGulch_CTF = "8 32";
//$Host::MapPlayerLimitsMountainMist_CTF = "8 32";
//$Host::MapPlayerLimitsPeak_CTF = "8 32";
//$Host::MapPlayerLimitsPendulum_CTF = "8 32";
//$Host::MapPlayerLimitsS5_Misadventure_CTF = "8 32";
//$Host::MapPlayerLimitsS8_CentralDogma_CTF = "8 32";
//$Host::MapPlayerLimitsS8_Mountking_CTF = "8 32";
//$Host::MapPlayerLimitsS8_Zilch_CTF = "8 32";
//$Host::MapPlayerLimitsTWL2_CloakOfNight_CTF = "8 32";
//$Host::MapPlayerLimitsTWL2_Dissention_CTF = "8 32";
//$Host::MapPlayerLimitsTWL2_Drifts_CTF = "8 32";
//$Host::MapPlayerLimitsTWL2_Drorck_CTF = "8 32";
//$Host::MapPlayerLimitsTWL2_Norty_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Abaddon_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_BaNsHee_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Boss_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_NoShelter_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Clusterfuct_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Curtilage_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Deserted_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Frostclaw_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Horde_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Neve_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Pandemonium_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Ramparts_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_Sandstorm_CTF = "8 32";
//$Host::MapPlayerLimitsTWL_WoodyMyrk_CTF = "8 32";
//$Host::MapPlayerLimitsOctoberRust_CTF = "8 32";
//$Host::MapPlayerLimitsDevilsElbow_CTF = "8 32";
//$Host::MapPlayerLimitsCloudCity_CTF = "8 32";
//$Host::MapPlayerLimitsDamnation_CTF = "8 32";
//$Host::MapPlayerLimitsDeathBirdsFly_CTF = "8 32";
//$Host::MapPlayerLimitsDesiccator_CTF = "8 32";
//$Host::MapPlayerLimitsDustToDust_CTF = "8 32";
//$Host::MapPlayerLimitsKatabatic_CTF = "8 32";
//$Host::MapPlayerLimitsQuagmire_CTF = "8 32";
//$Host::MapPlayerLimitsRecalescence_CTF = "8 32";
//$Host::MapPlayerLimitsReversion_CTF = "8 32";
//$Host::MapPlayerLimitsRiverDance_CTF = "8 32";
//$Host::MapPlayerLimitsSanctuary_CTF = "8 32";
//$Host::MapPlayerLimitsThinIce_CTF = "8 32";
//$Host::MapPlayerLimitsTombstone_CTF = "8 32";
//$Host::MapPlayerLimitsBroadside_nef_CTF = "8 32";
//$Host::MapPlayerLimitsCamelland_CTF = "8 32";
//$Host::MapPlayerLimitsHighTrepidation_CTF = "8 32";
//$Host::MapPlayerLimitsSmallDesertofDeath_CTF = "8 32";
//$Host::MapPlayerLimitsAgorazscium_CTF = "8 32";
//$Host::MapPlayerLimitsBasinFury_CTF = "8 32";
//$Host::MapPlayerLimitsCadaver_CTF = "8 32";
//$Host::MapPlayerLimitsEivoItoxico_CTF = "8 32";
//$Host::MapPlayerLimitsEinfach_CTF = "8 32";
//$Host::MapPlayerLimitsPicnicTable_CTF = "8 32";
//$Host::MapPlayerLimitsHostility_CTF = "8 32";
//$Host::MapPlayerLimitsHighWire_CTF = "8 32";
//$Host::MapPlayerLimitsCloudBurst_CTF = "8 32";
//$Host::MapPlayerLimitsCloseCombat_CTF = "8 32";
//$Host::MapPlayerLimitsDesertofDeath_nef_CTF = "8 32";
//$Host::MapPlayerLimitsGorgon_CTF = "8 32";
//$Host::MapPlayerLimitsMagamatic_CTF = "8 32";
//$Host::MapPlayerLimitsSub-zero_CTF = "8 32";



//  _           _              _     _     _ _   
// | |         | |            | |   | |   (_) |  
// | |     __ _| | ___ __ __ _| |__ | |__  _| |_ 
// | |    / _` | |/ / '__/ _` | '_ \| '_ \| | __|
// | |___| (_| |   <| | | (_| | |_) | |_) | | |_ 
// |______\__,_|_|\_\_|  \__,_|_.__/|_.__/|_|\__|
/////////////////////////////////////////////////////////////////////

//In Rotation
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = -1;
%max = 32;

$Host::MapPlayerLimitsVaubanLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsMiniSunDried_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsSundance_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsTWL_BeachBlitzLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsDesertofDeathLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsRaindance_nefLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsSunDriedLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsSkinnyDipLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsSaddiesHill_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsHavenLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsLushLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsBoxLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsTitaniaLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsTibbawLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsInfernusLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsS8_GeothermalLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsCankerLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsDustRunLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsCrossfiredLak_LakRabbit = %min SPC %max;

//Voteable, But not in rotation
/////////////////////////////////////////////////////////////////////

$Host::MapPlayerLimitsTreasureIslandLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsSulfide_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsFrozenFuryLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsArrakis_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsEquinoxLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsPhasmaDustLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsGodsRiftLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsSolsDescentLak_LakRabbit = %min SPC %max;
$Host::MapPlayerLimitsCrater71Lak_LakRabbit = %min SPC %max;

//Not Voteable, Not in rotation
/////////////////////////////////////////////////////////////////////

//$Host::MapPlayerLimitsEscaladeLak_LakRabbit = "8 32";
//$Host::MapPlayerLimitsMagmaticLak_LakRabbit = "8 32";
//$Host::MapPlayerLimitsHillsOfSorrow_LakRabbit = "8 32";
//$Host::MapPlayerLimitsTWL2_MuddySwampLak_LakRabbit = "8 32";
//$Host::MapPlayerLimitsSandStormLak_LakRabbit = "8 32";
//$Host::MapPlayerLimitsBeggarsRunLak_LakRabbit = "8 32";
//$Host::MapPlayerLimitsDamnnationLak_LakRabbit = "8 32";



//  _      _____ _______ ______ 
// | |    / ____|__   __|  ____|
// | |   | |       | |  | |__   
// | |   | |       | |  |  __|  
// | |___| |____   | |  | |     
// |______\_____|  |_|  |_|     
/////////////////////////////////////////////////////////////////////

//In Rotation
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = -1;
%max = 32;

$Host::MapPlayerLimitsBastardForgeLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsFirestormLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsDangerousCrossingLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsSmallCrossingLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsDireLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsRoundTheMountainLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsCirclesEdgeLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTenebrousCTF_sctf = %min SPC %max;
$Host::MapPlayerLimitsTheFray_sctf = %min SPC %max;
$Host::MapPlayerLimitsSignalLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsStarFallLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_DamnationLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_Icedance_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_Mordacity_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_SilenusLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_CanyonCrusadeDeluxeLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_FrozenHopeLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_JaggedClawLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_HildebrandLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_SkylightLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_BeachBlitzLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_FeignLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_RollercoasterLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_StonehengeLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_WilderZoneLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsoasisintensity_sctf = %min SPC %max;
$Host::MapPlayerLimitsberlard_sctf = %min SPC %max;
$Host::MapPlayerLimitsRaindanceLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsSmallTimeLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsArenaDome_sctf = %min SPC %max;
$Host::MapPlayerLimitsBulwark_sctf = %min SPC %max;
$Host::MapPlayerLimitsDiscord_sctf = %min SPC %max;
$Host::MapPlayerLimitsJadeValley_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_MassiveLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsBlink_sctf = %min SPC %max;
$Host::MapPlayerLimitsHillTopHop_sctf = %min SPC %max;

//Voteable, But not in rotation
/////////////////////////////////////////////////////////////////////

$Host::MapPlayerLimitsHeadstone_sctf = %min SPC %max;
$Host::MapPlayerLimitsMirage_sctf = %min SPC %max;
$Host::MapPlayerLimitsBeggarsRunLT_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_HawkingHeat_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_Mimicry_sctf = %min SPC %max;
$Host::MapPlayerLimitsS5_Woodymyrk_sctf = %min SPC %max;
$Host::MapPlayerLimitsS8_Cardiac_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Celerity_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Crevice_sctf = %min SPC %max;
$Host::MapPlayerLimitsS8_Opus_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_MidnightMayhemDeluxe_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL2_Ocular_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_Cinereous_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_Deserted_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_DangerousCrossing_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_OsIris_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_Damnation_sctf = %min SPC %max;
$Host::MapPlayerLimitsTWL_Titan_sctf = %min SPC %max;

//Not Voteable, Not in rotation
/////////////////////////////////////////////////////////////////////

//$Host::MapPlayerLimitsSurrealLT_sctf = "8 32";
//$Host::MapPlayerLimitsCoppersky_sctf = "8 32";
//$Host::MapPlayerLimitsDuelersDelight_sctf = "8 32";
//$Host::MapPlayerLimitsSuperHappyBouncyFunTime_sctf = "8 32";
//$Host::MapPlayerLimitsPariahLT_sctf = "8 32";
//$Host::MapPlayerLimitsSmallMelee_sctf = "8 32";
//$Host::MapPlayerLimitsTitForTat_sctf = "8 32";
//$Host::MapPlayerLimitsCloseCombatLT_sctf = "8 32";
//$Host::MapPlayerLimitsPrismatic_sctf = "8 32";
//$Host::MapPlayerLimitsDamnation_sctf = "8 32";
//$Host::MapPlayerLimitsDustToDust_sctf = "8 32";
//$Host::MapPlayerLimitsMinotaur_sctf = "8 32";
//$Host::MapPlayerLimitsDesertofDeath_nef_sctf = "8 32";
//$Host::MapPlayerLimitsGorgon_sctf = "8 32";
//$Host::MapPlayerLimitsTitan_sctf = "8 32";
//$Host::MapPlayerLimitsMac_FlagArena_sctf = "8 32";
//$Host::MapPlayerLimitsExtractor_sctf = "8 32";
//$Host::MapPlayerLimitsAstersDescent_sctf = "8 32";
//$Host::MapPlayerLimitsAzoth_sctf = "8 32";
//$Host::MapPlayerLimitsDustLust_sctf = "8 32";
//$Host::MapPlayerLimitsDisjointed_sctf = "8 32";
//$Host::MapPlayerLimitsPeak_sctf = "8 32";
//$Host::MapPlayerLimitsSnowcone_sctf = "8 32";
//$Host::MapPlayerLimitsS5_Centaur_sctf = "8 32";
//$Host::MapPlayerLimitsS5_Drache_sctf = "8 32";
//$Host::MapPlayerLimitsS5_Misadventure_sctf = "8 32";
//$Host::MapPlayerLimitsS5_Reynard_sctf = "8 32";
//$Host::MapPlayerLimitsS5_Sherman_sctf = "8 32";
//$Host::MapPlayerLimitsS8_Geothermal_sctf = "8 32";
//$Host::MapPlayerLimitsS8_Zilch_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_Drifts_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_Drorck_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_FrozenGlory_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_IceDagger_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_MuddySwamp_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_Norty_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_RoughLand_sctf = "8 32";
//$Host::MapPlayerLimitsTWL2_Ruined_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_BaNsHee_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Boss_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Crossfire_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_NoShelter_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Clusterfuct_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Curtilage_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_DeadlyBirdsSong_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Frostclaw_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Magamatic_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Neve_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Pandemonium_sctf = "8 32";
//$Host::MapPlayerLimitsTWL_Ramparts_sctf = "8 32";
//$Host::MapPlayerLimitsDehSwamp_sctf = "8 32";
//$Host::MapPlayerLimitsHostileLoch_sctf = "8 32";
//$Host::MapPlayerLimitsDevilsElbow_sctf = "8 32";
//$Host::MapPlayerLimitsCamelland_sctf = "8 32";
//$Host::MapPlayerLimitsSmallDesertofDeath_sctf = "8 32";
//$Host::MapPlayerLimitsShortFall_sctf = "8 32";
//$Host::MapPlayerLimitsFallout_sctf = "8 32";
//$Host::MapPlayerLimitsSoylentGreen_sctf = "8 32";
//$Host::MapPlayerLimitsIsland_sctf = "8 32";
//$Host::MapPlayerLimitsHighOctane_sctf = "8 32";



//  _____             _   _                     _       _     
// |  __ \           | | | |                   | |     | |    
// | |  | | ___  __ _| |_| |__  _ __ ___   __ _| |_ ___| |__  
// | |  | |/ _ \/ _` | __| '_ \| '_ ` _ \ / _` | __/ __| '_ \ 
// | |__| |  __/ (_| | |_| | | | | | | | | (_| | || (__| | | |
// |_____/ \___|\__,_|\__|_| |_|_| |_| |_|\__,_|\__\___|_| |_|
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
%min = -1;
%max = 32;

$Host::MapPlayerLimitsRaspDM_DM = %min SPC %max;
$Host::MapPlayerLimitsEntombedDM_DM = %min SPC %max;
$Host::MapPlayerLimitsIceDomeDM_DM = %min SPC %max;
$Host::MapPlayerLimitsHoofToeDM_DM = %min SPC %max;
$Host::MapPlayerLimitsArenaDomeDM_DM = %min SPC %max;
$Host::MapPlayerLimitsVulcansWrathDM_DM = %min SPC %max;
$Host::MapPlayerLimitsRampartsDM_DM = %min SPC %max;
$Host::MapPlayerLimitsShrineDM_DM = %min SPC %max;
$Host::MapPlayerLimitsLiveBaitDM_DM = %min SPC %max;
$Host::MapPlayerLimitsFourSquareDM_DM = %min SPC %max;
$Host::MapPlayerLimitsBrigDM_DM = %min SPC %max;



//  _____             _ 
// |  __ \           | |
// | |  | |_   _  ___| |
// | |  | | | | |/ _ \ |
// | |__| | |_| |  __/ |
// |_____/ \__,_|\___|_|
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
//%min = -1;
//%max = 32;

//$Host::MapPlayerLimitsAgentsOfFortune_Duel = %min SPC %max;
//$Host::MapPlayerLimitsCasern_Cavite_Duel = %min SPC %max;
//$Host::MapPlayerLimitsEquinox_Duel = %min SPC %max;
//$Host::MapPlayerLimitsEscalade_Duel = %min SPC %max;
//$Host::MapPlayerLimitsFracas_Duel = %min SPC %max;
//$Host::MapPlayerLimitsInvictus_Duel = %min SPC %max;
//$Host::MapPlayerLimitsMyrkWood_Duel = %min SPC %max;
//$Host::MapPlayerLimitsOasis_Duel = %min SPC %max;
//$Host::MapPlayerLimitsPyroclasm_Duel = %min SPC %max;
//$Host::MapPlayerLimitsRasp_Duel = %min SPC %max;
//$Host::MapPlayerLimitsSunDried_Duel = %min SPC %max;
//$Host::MapPlayerLimitsTalus_Duel = %min SPC %max;
//$Host::MapPlayerLimitsUnderhill_Duel = %min SPC %max;
//$Host::MapPlayerLimitsWhiteout_Duel = %min SPC %max;
//$Host::MapPlayerLimitsTombstone_Duel = %min SPC %max;
//$Host::MapPlayerLimitsVaubanLak_Duel = %min SPC %max;
//$Host::MapPlayerLimitsDesertofDeathLak_Duel = %min SPC %max;



//   _____ _                 
//  / ____(_)                
// | (___  _  ___  __ _  ___ 
//  \___ \| |/ _ \/ _` |/ _ \
//  ____) | |  __/ (_| |  __/
// |_____/|_|\___|\__, |\___|
//                 __/ |     
//                |___/    
/////////////////////////////////////////////////////////////////////

//Set Values for this Group
//%min = -1;
//%max = 32;

//$Host::MapPlayerLimitsIsleofman_Siege = %min SPC %max;
//$Host::MapPlayerLimitsTrident_Siege = %min SPC %max;
//$Host::MapPlayerLimitsAlcatraz_Siege = %min SPC %max;

}

// Run our function
setmaps();