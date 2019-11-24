//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	Score hud stats system, gather data across x number of games to do math/stats									
//	This also has the added benefit of restoreing scores after leaving												
//	Script BY: DarkTiger																							
//  Note this system only works in online mode as it uses guid to keep track of people								
//  Version 1.0 - initial release																					
//  Version 2.0 - code refactor/optimizing/fixes																	
//  Version 3.0 - DM LCTF
//  Version 4.0 - code refactor/optimizing/fixes
//	 Version 5.0 - DuleMod Arena support + optimizing/fixes + extras																	
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//ChangeLog
//    4.0
//    *Removed most redudent/repeating code
//       *Menus - removed redudent menus
//       *Global arrays - reorganized to get rid of repeating field values
//       *Save Load - removed unused functions from the load/save fast methods
//       *Stats -  condensed the stats handling code by 50%
//    *Fixed bug with saveing weapon stats after a person has left
//    *Removed $dtStats::fullGames, as it was not necessary becuase $dtStats::fgPercentage gives the same level of contorl
//    *Null GUID protection, just to prevent garbage or null data
//
//    5.0
//    Rework functions to remove dependencies on other .vl2 this script now works in base, classic, and some variants of the two. 
//    Arena and Duel Mod Support added  
//    MA stats across all weapons/items
//    Mine Grenades Satchel Turrets Vehicles stats added
//    Stats menu clean up and rework, some stuff was not even working 
//    Remvoed ProjectileData::onCollision hook for stats collection, everything is now under clientDmg
//    Make processGameLink into one funciton for all game types
//    Added fail safe to getCNameToCID to prevent blank names 
//    Reduce decimal places and scale down large numbers in the menus 
//    Added $dtStats::debugEchos for debuging issues 
//    Fix issue with reset when player joins the game
//    Fix issue with back button bricking the score hud, as well as add a fail safe so this cant happen
//    Added to options to control whats displayed
//    Reworked history to remove redudent menus and to add the ablity to view weapon history, may expand on later
//    Cleaned up the field value array removed unused and unnecessary values
//    Total stats now reset onces a value has getting near the 32 bit int limit 
//    Added Live View so players can watch there stats in realtime with the added feature of being able to pin it and keep that window in place
//    Added Player/Armror kill stats
//
//    5.5 fixes
//    Rework hand grenade stats collection 
//    Moved commonly overridden functions in its own package and uses
//    DefaultGame activate/deactivatePackages to correctly postion them on top of gameType overides 
//    this fixes issues with lakRabbitGame overrides
//    Reworked some of the values on live screen to be more correct 
//    Moved resetDtStats to MissionDropReady so that liveStats work in lan game
//    
//-----------Settings------------
//disable stats system restart required;
$dtStats::Enable = 1; 
//Only self client can see his own stats, any stat, unless admin
$dtStats::viewSelf = 0; 
//number of games to gather a running average, i would not make this too big of a number as its a lot of data to load/save
$dtStats::MaxNumOfGames = 10;
//Value at witch total stats should reset should be less then 2,147,483,647 by a few 100k
$dtStats::ValMax = 2000000000;
//set to 1 for the averaging to skip over zeros for example 0 0 1 2 0 4 0  it would only add 1 2 4 and divide by 3
$dtStats::skipZeros = 1;

// Record stats if player is here for x percentage of the game
$dtStats::fgPercentage["CTFGame"] = 25;
//0 score based, 1 time based, 2 the closer one to finishing the game
$dtStats::fgPercentageType["CTFGame"] =2;

$dtStats::fgPercentage["LakRabbitGame"] = 25;
$dtStats::fgPercentageType["LakRabbitGame"] = 2;

$dtStats::fgPercentage["DMGame"] = 25;
$dtStats::fgPercentageType["DMGame"] = 2;
//LCTF
$dtStats::fgPercentage["SCtFGame"] =25;
$dtStats::fgPercentageType["SCtFGame"] = 2;

$dtStats::fgPercentage["ArenaGame"] =20; // $dtStats::fgPercentage["ArenaGame"]/RoundsLimit * 100
$dtStats::fgPercentage["DuelGame"] =0; // best to keep 0

$dtStats::returnToMenuTimer = (30*1000);// 30 sec min after not making an action reset

//Load/saving rates to prevent any server hitching
$dtStats::slowLoadTime = 250;
$dtStats::slowSaveTime = 64;

//Disables save system, and only show stats of current play session 
$dtStats::Basic = 0;
//Control whats displayed  
$dtStats::Live = 1;  
$dtStats::KD = 1;
$dtStats::Hist =1;
$dtStats::Vehicle = 0; 
$dtStats::Turret = 0; 
$dtStats::Armor = 0; 
$dtStats::Match = 0;
$dtStats::Weapon = 0;

//debug stuff
//Set to 1 when your makeing changes to the menu so you can see them  update live note the refresh rate is like 2-4 secs
//edit and exec("scripts/autoexec/stats.cs"); to see changes
$dtStats::enableRefresh = 0;
$dtStats::debugEchos = 1;// echos function calls
//$pref::NoClearConsole = 1;

// colors used
//00dcd4 Darker blue
//0befe7 Lighter blue
//00dc00 Green
//0099FF Blue
//FF9A00 Orange
//05edad Teal
//FF0000 Red
//dcdcdc White
//02d404 T2 Green
//fb3939 Lighter Red

//---------------------------------
//  Torque Markup Language - TML
//  Reference Tags
//---------------------------------

//<font:name:size>Sets the current font to the indicated name and size. Example: <font:Arial Bold:20>
//<tag:ID>Set a tag to which we can scroll a GuiScrollContentCtrl (parent control of the guiMLTextCtrl)
//<color:RRGGBBAA>Sets text color. Example: <color:dcdcdc> will display red text.
//<linkcolor:RRGGBBAA>Sets the color of a hyperlink.
//<linkcolorHL:RRGGBBAA>Sets the color of a hyperlink that is being clicked.
//<shadow:x:y>Add a shadow to the text, displaced by (x, y).
//<shadowcolor:RRGGBBAA>Sets the color of the text shadow.
//<bitmap:filePath>Displays the bitmap image of the given file. Note this is hard coded in t2 to only look in texticons in textures
//<spush>Saves the current text formatting so that temporary changes to formatting can be made. Used with <spop>.
//<spop>Restores the previously saved text formatting. Used with <spush>.
//<sbreak>Produces line breaks, similarly to <br>. However, while <br> keeps the current flow (for example, when flowing around the image), <sbreak> moves the cursor position to a new line in a more global manner (forces our text to stop flowing around the image, so text is drawn at a new line under the image).
//<just:left>Left justify the text.
//<just:right>Right justify the text.
//<just:center>Center the text.
//<a:URL>content</a>Inserts a hyperlink into the text. This can also be used to call a function class::onURL
//<lmargin:width>Sets the left margin.
//<lmargin%:width>Sets the left margin as a percentage of the full width.
//<rmargin:width>Sets the right margin.
//<clip:width>content</clip>Produces the content, but clipped to the given width.
//<div:bool>Use the profile's fillColorHL to draw a background for the text.
//<tab:##[,##[,##]]>Sets tab stops at the given locations.
//<br>Forced line break.
// This function allows you to reload all modified
///////////////////////////////////////////////////////////////////////////////
//                             		CTF
///////////////////////////////////////////////////////////////////////////////
$dtCTF = 0;
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "kills";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "deaths";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "suicides";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "teamKills";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "flagCaps";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "flagGrabs";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "carrierKills";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "flagReturns";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "score";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "scoreMidAir";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "scoreHeadshot";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "scoreRearshot";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "escortAssists";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "defenseScore";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "offenseScore";
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "flagDefends";

$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "winCount"; // in this script only
$dtStats::fieldValue[$dtCTF++,"CTFGame"] = "lossCount";
$dtStats::fieldCount["CTFGame"] = $dtCTF;
///////////////////////////////////////////////////////////////////////////////
//                            	 LakRabbit									 //
///////////////////////////////////////////////////////////////////////////////
//Game type values - out of LakRabbitGame.cs
$dtLAK =0;
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "score";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "kills";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "deaths";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "suicides";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "flagGrabs";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "flagTimeMS";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "morepoints";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "mas";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalSpeed";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalDistance";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalChainAccuracy";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalChainHits";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalSnipeHits";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalSnipes";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalShockHits";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "totalShocks";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "MidairflagGrabs";
$dtStats::fieldValue[$dtLAK++,"LakRabbitGame"] = "MidairflagGrabPoints";
$dtStats::fieldCount["LakRabbitGame"] = $dtLAK;
///////////////////////////////////////////////////////////////////////////////
//                            	 DMGame								   		 //
///////////////////////////////////////////////////////////////////////////////
$dtDMG = 0;
$dtStats::fieldValue[$dtDMG++,"DMGame"] = "score";
$dtStats::fieldValue[$dtDMG++,"DMGame"] = "kills";
$dtStats::fieldValue[$dtDMG++,"DMGame"] = "deaths";
$dtStats::fieldValue[$dtDMG++,"DMGame"] = "suicides";
$dtStats::fieldValue[$dtDMG++,"DMGame"] = "efficiency";
$dtStats::fieldCount["DMGame"] = $dtDMG;
///////////////////////////////////////////////////////////////////////////////
//                             		LCTF									 //
///////////////////////////////////////////////////////////////////////////////
$dtLCTF = 0;
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "kills";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "deaths";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "suicides";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "teamKills";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "flagCaps";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "flagGrabs";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "carrierKills";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "flagReturns";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "score";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "scoreMidAir";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "scoreHeadshot";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "scoreRearshot";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "escortAssists";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "defenseScore";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "offenseScore";
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "flagDefends";

$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "winCount";// in this script only
$dtStats::fieldValue[$dtLCTF++,"SCtFGame"] = "lossCount";
$dtStats::fieldCount["SCtFGame"] = $dtLCTF;
///////////////////////////////////////////////////////////////////////////////
//                            	 DuelGame								   		 //
///////////////////////////////////////////////////////////////////////////////
$dtDuelG = 0;
$dtStats::fieldValue[$dtDuelG++,"DuelGame"] = "score";
$dtStats::fieldValue[$dtDuelG++,"DuelGame"] = "kills";
$dtStats::fieldValue[$dtDuelG++,"DuelGame"] = "deaths";
$dtStats::fieldCount["DuelGame"] = $dtDuelG;
///////////////////////////////////////////////////////////////////////////////
//                            	 ArenaGame								   		 //
///////////////////////////////////////////////////////////////////////////////
$dtArenaG = 0;
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "kills";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "deaths";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "suicides";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "teamKills";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "snipeKills";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "roundsWon";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "roundsLost";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "assists";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "roundKills";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "hatTricks";
$dtStats::fieldValue[$dtArenaG++,"ArenaGame"] = "score";
$dtStats::fieldCount["ArenaGame"] = $dtArenaG;
///////////////////////////////////////////////////////////////////////////////
//                            	 HuntersGame								   		 //
///////////////////////////////////////////////////////////////////////////////
//$dtHunter = 0;
//$dtStats::fieldValue[$dtHunter++,"HuntersGame"] = "score";
//$dtStats::fieldValue[$dtHunter++,"HuntersGame"] = "suicides";
//$dtStats::fieldValue[$dtHunter++,"HuntersGame"] = "kills";
//$dtStats::fieldValue[$dtHunter++,"HuntersGame"] = "teamKills";
//$dtStats::fieldValue[$dtHunter++,"HuntersGame"] = "deaths";
//$dtStats::fieldValue[$dtHunter++,"HuntersGame"] = "flagPoints";
//$dtStats::fieldCount["HuntersGame"] = $dtHunter;
///////////////////////////////////////////////////////////////////////////////
//                              Weapon/Misc Stats
///////////////////////////////////////////////////////////////////////////////
//these are field values from this script
$dtWep = 0;
$dtStats::fieldValue[$dtWep++,"dtStats"] = "cgKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "cgDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "discKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "discDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mineKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mineDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "explosionKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "explosionDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "impactKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "impactDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "groundKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "groundDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "turretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "turretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "aaTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "aaTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "indoorDepTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "indoorDepTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "outdoorDepTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "outdoorDepTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "sentryTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "sentryTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "outOfBoundKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "outOfBoundDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "lavaKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "lavaDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shrikeBlasterKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shrikeBlasterDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "bellyTurretKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "bellyTurretDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "bomberBombsKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "bomberBombsDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "tankChaingunKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "tankChaingunDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "tankMortarKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "tankMortarDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "satchelChargeKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "satchelChargeDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mpbMissileKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mpbMissileDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "lightningKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "lightningDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "vehicleSpawnKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "vehicleSpawnDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "forceFieldPowerUpKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "forceFieldPowerUpDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "crashKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "crashDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "waterKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "waterDeaths";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "nexusCampingKills";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "nexusCampingDeaths";

//Damage Stats
$dtStats::fieldValue[$dtWep++,"dtStats"] = "cgDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "cgDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "discInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "discInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceIDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mineInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mineInDmgTaken";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "SatchelInDmg";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "SatchelInDmgTaken";

//rounds fired
$dtStats::fieldValue[$dtWep++,"dtStats"] = "cgShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "discShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeShotsFired";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mineShotsFired";

$dtStats::fieldValue[$dtWep++,"dtStats"] = "shotsFired";

$dtStats::fieldValue[$dtWep++,"dtStats"] = "cgDirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserDirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterDirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "elfDirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "discIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeInHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "missileIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "mineIndirectHits";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "SatchelInHits";

$dtStats::fieldValue[$dtWep++,"dtStats"] = "laserHeadShot";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "shockRearShot";
$dtStats::fieldValue[$dtWep++,"dtStats"] = "minePlusDisc";

   //miss
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "killStreak";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "assist";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "maxSpeed";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "avgSpeed";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "maxRV";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "comboPT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "comboCount";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "timeTL";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "distT";
   
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorL";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorM";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorH";
   
   
   if($dtStats::Armor){
 
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorMD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHD";
      
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLL";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLM";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLH";
      
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorML";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorMM";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorMH";
      
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHL";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHM";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHH";
      
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLLD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLMD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorLHD";
      
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorMLD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorMMD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorMHD";
      
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHLD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHMD";
      $dtStats::fieldValue[$dtWep++,"dtStats"] = "armorHHD";
   }

   //max distance
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "cgMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "discMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "laserMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "missileMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mineMax";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "shockMax";

   //weapon combos
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "cgCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "discCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "laserCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "missileCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mineCom";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "shockCom";
   
   //relative velocity
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "cgT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "discT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "laserT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "shockT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mineT";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "missileT";
   
   //acc
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "cgACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "discACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "laserACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "shockACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mineACC";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "missileACC";

   //midairs
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "cgMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "discMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "grenadeMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "laserMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mortarMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "shockLanceMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "plasmaMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "blasterMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hGrenadeMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "missileMA";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mineMA";

if($dtStats::Turret && !$dtStats::Basic){
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "AATurretDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "AATurretDirectHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "AATurretDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "IndoorDepTurretDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "IndoorDepTurretDirectHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "IndoorDepTurretDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "SentryTurretDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "SentryTurretDirectHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "SentryTurretDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "ShrikeBlasterDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "ShrikeBlasterDirectHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "ShrikeBlasterDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BellyTurretDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BellyTurretDirectHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BellyTurretDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankChaingunDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankChaingunDirectHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankChaingunDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "PlasmaTurretInDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "PlasmaTurretInHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "PlasmaTurretInDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MortarTurretInDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MortarTurretInHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MortarTurretInDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MissileTurretInDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MissileTurretInHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MissileTurretInDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "OutdoorDepTurretInDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "OutdoorDepTurretInHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "OutdoorDepTurretInDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BomberBombsInDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BomberBombsInHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BomberBombsInDmgTaken";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankMortarInDmg";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankMortarInHits";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankMortarInDmgTaken";
}

if($dtStats::Vehicle && !$dtStats::Basic){
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "wildRK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "assaultRK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mobileBaseRK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "scoutFlyerRK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "bomberFlyerRK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hapcFlyerRK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "wildRD";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "assaultRD";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mobileBaseRD";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "scoutFlyerRD";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "bomberFlyerRD";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hapcFlyerRD";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "wildCrash";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "assaultCrash";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mobileBaseCrash";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "scoutFlyerCrash";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "bomberFlyerCrash";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hapcFlyerCrash";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "wildEK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "assaultEK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "mobileBaseEK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "scoutFlyerEK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "bomberFlyerEK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "hapcFlyerEK";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "PlasmaTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "AATurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MortarTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "MissileTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "IndoorDepTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "OutdoorDepTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "SentryTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "ShrikeBlasterFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BellyTurretFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "BomberBombsFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankChaingunFired";
   $dtStats::fieldValue[$dtWep++,"dtStats"] = "TankMortarFired";
}

$dtStats::fieldCount["dtStats"] = $dtWep;
if(!$dtStats::Enable){return;} // abort exec
if(!isObject(statsGroup)){new SimGroup(statsGroup);}

package dtStats{

   function CTFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }
   function CTFGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function CTFGame::timeLimitReached(%game){
      dtStatsTimeLimitReached(%game);//common
      parent::timeLimitReached(%game);
   }
   function CTFGame::scoreLimitReached(%game){
      dtStatsScoreLimitReached(%game);//common
      parent::scoreLimitReached(%game);
   }
   function CTFGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function CTFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function CTFGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function CTFGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      CTFHud(%game, %client, %tag);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function LakRabbitGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function LakRabbitGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }
   function LakRabbitGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function LakRabbitGame::timeLimitReached(%game){
      dtStatsTimeLimitReached(%game);//common
      parent::timeLimitReached(%game);
   }
   function LakRabbitGame::scoreLimitReached(%game){
      dtStatsScoreLimitReached(%game);//common
      parent::scoreLimitReached(%game);
   }
   function LakRabbitGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function LakRabbitGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function LakRabbitGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      LakRabbitHud(%game, %client, %tag);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function DMGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function DMGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }
   function DMGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function DMGame::timeLimitReached(%game){
      dtStatsTimeLimitReached(%game);//common
      parent::timeLimitReached(%game);
   }
   function DMGame::scoreLimitReached(%game){
      dtStatsScoreLimitReached(%game);//common
      parent::scoreLimitReached(%game);
   }
   function DMGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function DMGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function DMGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      DMHud(%game, %client, %tag);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function SCtFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }
   function SCtFGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function SCtFGame::timeLimitReached(%game){
      dtStatsTimeLimitReached(%game);//common
      parent::timeLimitReached(%game);
   }
   function SCtFGame::scoreLimitReached(%game){
      dtStatsScoreLimitReached(%game);//common
      parent::scoreLimitReached(%game);
   }
   function SCtFGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function SCtFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function SCtFGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function SCtFGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      CTFHud(%game, %client, %tag);
   }
   /////////////////////////////////////////////////////////////////////////////////////
   function ArenaGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }
   function ArenaGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   //function ArenaGame::timeLimitReached(%game){ // not used
      //dtStatsTimeLimitReached(%game);//common
      //parent::timeLimitReached(%game);
   //}
   //function ArenaGame::scoreLimitReached(%game){// not used
      //dtStatsScoreLimitReached(%game);//common
      //parent::scoreLimitReached(%game);
   //}
   function ArenaGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function ArenaGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function ArenaGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }   
   function ArenaGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      ArenaHud(%game, %client, %tag);
   }
   /////////////////////////////////////////////////////////////////////////////
      function DuelGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      dtStatsMissionDropReady(%game, %client);//common
   }
   function DuelGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function DuelGame::timeLimitReached(%game){
      dtStatsTimeLimitReached(%game);//common
      parent::timeLimitReached(%game);
   }
   function DuelGame::scoreLimitReached(%game){
      dtStatsScoreLimitReached(%game);//common
      parent::scoreLimitReached(%game);
   }
   function DuelGame::gameOver( %game ){
      dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function DuelGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function DuelGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }   
   function DuelGame::updateScoreHud(%game, %client, %tag){
      DuelHud(%game, %client, %tag);
   }
///////////////////////////////////////////////////////////////////////////////

   function DefaultGame::missionLoadDone(%game){
      parent::missionLoadDone(%game);
          //check to see if we are running evo or not, if not then lets just enable these 
         if(!isFile("scripts/autoexec/evolution.cs")){
            $Host::EvoAveragePings = $Host::ShowIngamePlayerScores = 1;
         }
   }
   function serverCmdShowHud(%client, %tag){ // to refresh screen when client opens it up
      parent::serverCmdShowHud(%client, %tag);
      %tagName = getWord(%tag, 1);
      %tag = getWord(%tag, 0);
      if(%tag $= 'scoreScreen' && %client.viewStats){
         statsMenu(%client,Game.class);
      }
   }
   //function DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject){ 
      //clientDmgStats(%data,%position,%sourceObject,%targetObject, %damageType,%amount);
      //parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %sourceObject)
   //}
//////////////////////////////////////////////////////////////////////////////////
   function DefaultGame::activatePackages(%game){
         parent::activatePackages(%game);
         if(isActivePackage(dtStatsGame)){
             deactivatePackage(dtStatsGame);
             activatePackage(dtStatsGame);
         }
         else{
             activatePackage(dtStatsGame);
         }
   }
   function DefaultGame::deactivatePackages(%game){
         parent::deactivatePackages(%game);
      if(isActivePackage(dtStatsGame))
         deactivatePackage(dtStatsGame);
   }
//////////////////////////////////////////////////////////////////////////////////
};
//helps with game types that override functions and dont use parent
// that way we get called first then the gametype can do whatever 
package dtStatsGame{
     function detonateGrenade(%obj){// from lakRabbitGame.cs for grenade tracking      
      %obj.dtNade = 1;
      $dtObjExplode = %obj;
      parent::detonateGrenade(%obj);
   } 
   function MineDeployed::onThrow(%this, %mine, %thrower){
       parent::onThrow(%this, %mine, %thrower);
       %thrower.client.mineShotsFired++;
       %thrower.client.shotsFired++;
       %thrower.client.mineACC = (%thrower.client.mineIndirectHits / %thrower.client.mineShotsFired) * 100;
   }
   function GrenadeThrown::onThrow(%this, %gren,%thrower){
       parent::onThrow(%this, %gren);
       %thrower.client.hGrenadeShotsFired++;
       %thrower.client.shotsFired++;
       %thrower.client.hGrenadeACC = (%thrower.client.hGrenadeInHits / %thrower.client.hGrenadeShotsFired) * 100;
   }
      function ShapeBaseImageData::onFire(%data, %obj, %slot){
      %p = parent::onFire(%data, %obj, %slot);
      if(isObject(%p)){
         clientShotsFired(%data.projectile, %obj, %p);
      }
      return %p;
   }
   function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC){
      clientDmgStats(%data,%position,%sourceObject,%targetObject, %damageType,%amount);
      parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
   }

   function StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType){
       clientDmgStats(%data,%position,%sourceObject,%targetObject, %damageType,%amount);
       parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
   }
   function SniperRifleImage::onFire(%data,%obj,%slot){
      if(!%obj.hasEnergyPack || %obj.getEnergyLevel() < %this.minEnergy) // z0dd - ZOD, 5/22/03. Check for energy too.
      {
         // siddown Junior, you can't use it
         serverPlay3D(SniperRifleDryFireSound, %obj.getTransform());
         return;
      }
      %pct = %obj.getEnergyLevel() / %obj.getDataBlock().maxEnergy;
      %p = new (%data.projectileType)() {
         dataBlock        = %data.projectile;
         initialDirection = %obj.getMuzzleVector(%slot);
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         damageFactor     = %pct * %pct;
         sourceSlot       = %slot;
      };
      clientShotsFired(%data.projectile, %obj, %p);
      %p.setEnergyPercentage(%pct);
      
      %obj.lastProjectile = %p;
      MissionCleanup.add(%p);
      serverPlay3D(SniperRifleFireSound, %obj.getTransform());
      
      // AI hook
      if(%obj.client)
         %obj.client.projectile = %p;
      
      %obj.setEnergyLevel(0);
      if($Host::ClassicLoadSniperChanges)
         %obj.decInventory(%data.ammo, 1);
   }
   function ShockLanceImage::onFire(%this, %obj, %slot){
      %p = parent::onFire(%this, %obj, %slot);
         clientShotsFired(ShockLanceImage.projectile, %obj, %p);
      return %p;
   }
   function Armor::onMount(%this,%obj,%vehicle,%node){
      parent::onMount(%this,%obj,%vehicle,%node);
      %obj.client.vehDBName = %vehicle.getDataBlock().getName();
   }
};
if($dtStats::Enable){
   activatePackage(dtStats);
}
////////////////////////////////////////////////////////////////////////////////
//							 Game Type Commons								  //
////////////////////////////////////////////////////////////////////////////////
function dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){   
    %client.viewClient = getCNameToCID(%arg3);
   if(%arg1 $= "Stats" && %client.viewClient != 0){
      %client.viewStats = 1;// lock out score hud from updateing untill they are done
      %client.viewMenu = %arg2;
      %client.GlArg4 = %arg4;
      if($dtStats::debugEchos){error("dtGameLink GUID = "  SPC %client.guid SPC %arg1 SPC %arg2  SPC %arg3 SPC %arg4);}  
      statsMenu(%client, %game.class);
      if(%arg2 !$= "Reset"){
         return;
      }
      else{
         messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
         %client.viewStats = 0;
         Game.updateScoreHud(%client, 'scoreScreen');
      }
   }
   if(%game.class $= "DuelGame"){
      switch (getSubStr(%arg1, 0, 1))
      {
         case 1:
            %targetClient = getSubStr(%arg1, 1, strlen(%arg1) - 1);

            if ((%targetClient.team != 0) || (%client.team != 0))
               {
               serverCmdHideHud(%client, 'scoreScreen');
               commandToClient(%client, 'setHudMode', 'Standard', "", 0);
               DefaultGame::processGameLink(%game, %client, %targetClient, %arg2, %arg3, %arg4, %arg5);
               }
            else
               %game.RequestDuel(%client, %TargetClient);
          case 2:
            %game.CancelDuelOffer(%client);
            serverCmdHideHud(%client, 'scoreScreen');
            commandToClient(%client, 'setHudMode', 'Standard', "", 0);
          case 3:
            %game.AcceptDuelOffer(%client.clientDuelRequestedBy);
            serverCmdHideHud(%client, 'scoreScreen');
            commandToClient(%client, 'setHudMode', 'Standard', "", 0);
          case 4:
            %game.DeclineDuelOffer(%client.clientDuelRequestedBy);
            serverCmdHideHud(%client, 'scoreScreen');
            commandToClient(%client, 'setHudMode', 'Standard', "", 0);
          default:
      }
   }
   else{
      %targetClient = %arg1;
      if ((%client.team == 0) && isObject(%targetClient) && (%targetClient.team != 0))
      {
         %prevObsClient = %client.observeClient;
         
         // update the observer list for this client
         observerFollowUpdate( %client, %targetClient, %prevObsClient !$= "" );
         
         serverCmdObserveClient(%client, %targetClient);
         displayObserverHud(%client, %targetClient);
         
         if (%targetClient != %prevObsClient)
         {
            messageClient(%targetClient, 'Observer', '\c1%1 is now observing you.', %client.name);
            messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
         }
      }
   }
}
function tsPlayerCountTeam( %teamindex )
{
  %count = 0;

  %lim = ClientGroup.getCount();

  for ( %i = 0; %i < %lim; %i++ )
  {
    %client = ClientGroup.getObject( %i );

    if ( %client.team == %teamindex )
      %count++;
  }

  return %count;
}
function ArenaHud(%game, %client, %tag){
   if(%client.viewStats && $dtStats::enableRefresh){
      statsMenu(%client, %game.class);
      return;
   }
   else if(%client.viewStats && !$dtStats::enableRefresh){
      return;
   }

   %ShowScores = ( $Host::TournamentMode || $Host::ShowIngamePlayerScores );
   

    // Clear the HUD
     messageClient( %client, 'ClearHud', "", %tag, 0 );

     // Clear the header
     messageClient( %client, 'SetScoreHudHeader', "", "" );

     // Send the subheader
     messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,205,310,390,500>\tTEAMS/PLAYERS\tSCORE\tKILLS\tWIN/LOSS\tBONUS');

     // Tracks the line of the score hud we're writing to
     %index = -1;


     // For every team in the game..

     for ( %iTeam = 1; %iTeam <= Game.numTeams; %iTeam++ )
     {
       // Send team name
       
       %teamPlayerCount = tsPlayerCountTeam( %iTeam );
       %teamPlayerCountPlural = %teamPlayerCount == 1 ? "" : "s";

       messageClient( %client, 'SetLineHud', "", %tag, %index++, '<tab:10, 310><spush><font:Univers Condensed:28>\t%1 (%2) <font:Univers Condensed:16>%3 Player%4<spop>', %game.getTeamName(%iTeam), $TeamScore[%iTeam], %teamPlayerCount, %teamPlayerCountPlural );
       messageClient( %client, 'SetLineHud', "", %tag, %index++, "");

       // Send team player list

       for ( %iPlayer = 0; %iPlayer < $TeamRank[%iTeam,count]; %iPlayer++ )
       {
         %cl = $TeamRank[%iTeam,%iPlayer];

         %clScore = %cl.score $= "" ? 0 : %cl.score;
         %clKills = %cl.kills $= "" ? 0 : %cl.kills;
         %clBonus = %cl.hatTricks $= "" ? 0 : %cl.hatTricks;
         %clWins = %cl.roundsWon $= "" ? 0 : %cl.roundsWon;
         %clLosses = %cl.roundsLost $= "" ? 0 : %cl.roundsLost;

         %score = %cl.score $= "" ? 0 : %cl.score;

         if ( %cl == %client )
           if ( %cl.isAlive )
             %clStyle = "<color:dcdcdc>";
           else
             %clStyle = "<color:dd7a7a>";
         else if ( %cl.isAlive )
           %clStyle = "";
         else
           %clStyle = "<color:f90202>";

         // For living players send a simple name
         if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
            if ( %client.team != 0 && %client.isAlive )
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tStats\tView\t%1>+</a>%1</clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %6<rmargin:540><just:right>%7',
                     %cl.name, %clScore, %clKills, %clWins, %clStyle, %clLosses, %clBonus );
            }
            // For observers, create an anchor around the player name so they can be observed
            else
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t%6>%1</a></clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %8<rmargin:540><just:right>%7',
                     %cl.name, %clScore, %clKills, %clWins, %clStyle, %cl, %clBonus, %clLosses );
            }
         }
         else{
               if(%cl == %client){
                  if ( %client.team != 0 && %client.isAlive )
                  {
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tStats\tView\t%1>+</a>%1</clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %6<rmargin:540><just:right>%7',
                           %cl.name, %clScore, %clKills, %clWins, %clStyle, %clLosses, %clBonus );
                  }
                  // For observers, create an anchor around the player name so they can be observed
                  else
                  {
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t%6>%1</a></clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %8<rmargin:540><just:right>%7',
                           %cl.name, %clScore, %clKills, %clWins, %clStyle, %cl, %clBonus, %clLosses );
                  }
               }
               else{
                  if ( %client.team != 0 && %client.isAlive )
                  {
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200>%1</clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %6<rmargin:540><just:right>%7',
                           %cl.name, %clScore, %clKills, %clWins, %clStyle, %clLosses, %clBonus );
                  }
                  // For observers, create an anchor around the player name so they can be observed
                  else
                  {
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\t%6>%1</a></clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %8<rmargin:540><just:right>%7',
                           %cl.name, %clScore, %clKills, %clWins, %clStyle, %cl, %clBonus, %clLosses );
                  }  
               }
         }
       }

       // Insert a blank line

       messageClient( %client, 'SetLineHud', "", %tag, %index++, "");

     }

   // Tack on the list of observers:
   %observerCount = 0;
   for(%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.team == 0)
         %observerCount++;
   }
   
   if(%observerCount > 0)
   {
      messageClient(%client, 'SetLineHud', "", %tag, %index, "");
      %index++;
      messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
      %index++;
      for(%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         //if this is an observer
         if(%cl.team == 0)
         {
            %obsTime = getSimTime() - %cl.observerStartTime;
            %obsTimeStr = %game.formatTime(%obsTime, false);
            if(%client.isAdmin ||%client.isSuperAdmin || !$dtStats::viewSelf){
               messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
               
            }
            else if(%cl == %client){
               messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
            }
            else{
               messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr);
            }
            %index++;
         }
      }
   }
   
   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient(%client, 'ClearHud', "", %tag, %index);
}

function DuelHud(%game, %client, %tag){
      if(%client.viewStats && $dtStats::enableRefresh){
         statsMenu(%client, %game.class);
         return;
      }
      else if(%client.viewStats && !$dtStats::enableRefresh){
         return;
      }
   // Clear the header:
   messageClient(%client, 'SetScoreHudHeader', "", "");
   messageClient(%client, 'SetScoreHudSubheader', "", '<lmargin:10><just:left>PLAYER<lmargin:185><just:left>ACCURACY<lmargin:330><just:left>PLAYER<lmargin:505><just:left>ACCURACY');

   for (%index = 0; %index < ClientGroup.getCount(); %index++)
      ClientGroup.getObject(%index).WasDisplayed = false;

   %index = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %col1Client = ClientGroup.getObject(%i);
      %col1ClientRank = "";
      %col1Style = "<color:00dcdc>";        

      if ((%col1Client.clientDuelingWith == 0) || (%col1Client.WasDisplayed) || !isObject(%col1Client.player))
         continue;

      if ( %col1Client == %client )
         %col1Style = "<color:dcdcdc>";
      
      %col2Client = %col1Client.clientDuelingWith;
      %col2Client.WasDisplayed = true;
      %col2ClientRank = "";
      %col2Style = "<color:00dcdc>";        

      if ( %col2Client == %client )
         %col2Style = "<color:dcdcdc>";        

      //if the client is not an observer, send the message
      if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
         if (%client.team != 0)
         {
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tStats\tView\t%1>+</a>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180><a:gamelink\tStats\tView\t%3>+</a>%3</clip><just:right>%4',
                           %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                           %col1Style, %col2Style );
         }
         //else for observers, create an anchor around the player name so they can be observed
         else
         {
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\tStats\tView\t%3>+</a><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
                           %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                           %col1Style, %col2Style, %col1Client, %col2Client );
         }
         %index++;
      }
      else{
         if(%client ==  %col1Client){
            if (%client.team != 0)
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tStats\tView\t%1>+</a>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180>%3</clip><lmargin:515><just:left>%4',
                              %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                              %col1Style, %col2Style );
            }
            //else for observers, create an anchor around the player name so they can be observed
            else
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
                              %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                              %col1Style, %col2Style, %col1Client, %col2Client );
            }
            %index++;
         }
         else if(%client == %col2Client){
               if (%client.team != 0)
               {
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180><a:gamelink\tStats\tView\t%3>+</a>%3</clip><lmargin:515><just:left>%4',
                                 %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                                 %col1Style, %col2Style );
               }
               //else for observers, create an anchor around the player name so they can be observed
               else
               {
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\tStats\tView\t%3>+</a><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
                                 %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                                 %col1Style, %col2Style, %col1Client, %col2Client );
               }
               %index++;
            
         }
         else{
            if (%client.team != 0)
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180>%3</clip><lmargin:515><just:left>%4',
                              %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                              %col1Style, %col2Style );
            }
            //else for observers, create an anchor around the player name so they can be observed
            else
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\tStats\tView\t%3>+</a><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
                              %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                              %col1Style, %col2Style, %col1Client, %col2Client );
            }
            %index++;
         }
      }
   }

   if (%index == 0)
   {
      messageClient(%client, 'SetLineHud', "", %tag, %index, "");
      %index++;
   }
   
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
      ClientGroup.getObject(%i).sorted = false;

   %clientSortCount = 0;
   while (true)
   {
      %maxScore = -1;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         if (!%cl.sorted)
         {
            if (%cl.score > %maxScore)
            {
               %clientSort[%clientSortCount] = %cl;
               %maxScore = %cl.score;
            }
         }
      }

      if (%maxScore == -1)
         break;
      else
      {
         %clientSort[%clientSortCount].sorted = true;
         %clientSortCount++;
      }
   }

   messageClient(%client, 'SetLineHud', "", %tag, %index, "<just:center>-----------------------------------------------------------------------------------------------------------------");
   %index++;
   messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><font:Univers Condensed:22>\tClick on a name to duel<rmargin:315><just:right>KILLS<rmargin:390>DEATHS<rmargin:440><just:right>ACC<rmargin:510><just:right>RANK<rmargin:570><just:right>TIME');
   %index++;

   for (%i = 0; %i < %clientSortCount; %i++)
   {
      %cl = %clientSort[%i];

      %clientTimeStr = "";
      if (%cl.observerStartTime != 0)
      {
         %clientTime = getSimTime() - %cl.observerStartTime;
         %clientTimeStr = %game.formatTime(%clientTime, false);
      }
      if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
         if ((%cl != %client) && (%cl.team == 0) && (%client.team == 0) && %cl.Initialized)
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t1%3>%1</a></clip><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);
         else
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><color:0000dc><a:gamelink\tStats\tView\t%1>+</a>%1</a></clip><color:00dcdc><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);

         %index++;
      }
      else{
         if(%client == %cl){
            if ((%cl != %client) && (%cl.team == 0) && (%client.team == 0) && %cl.Initialized)
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t1%3>%1</a></clip><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);
            else
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><color:0000dc><a:gamelink\tStats\tView\t%1>+ </a>%1</a></clip><color:00dcdc><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);

            %index++;
         }
         else{
            if ((%cl != %client) && (%cl.team == 0) && (%client.team == 0) && %cl.Initialized)
             messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\t1%3>%1</a></clip><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);
            else
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><color:0000dc>%1</a></clip><color:00dcdc><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);

            %index++; 
         }
      }
   }

   messageClient(%client, 'SetLineHud', "", %tag, %index, "");
   %index++;

   if (%client.clientRequestedDuelWith != 0)
   {
      messageClient( %client, 'SetLineHud', "", %tag, %index, 'Your duel request with %1 is pending (<a:gamelink\t2>CANCEL</a>).',
                     %client.clientRequestedDuelWith.name);
      %index++;
   }
   
   if (%client.clientDuelRequestedBy != 0)
   {
      messageClient( %client, 'SetLineHud', "", %tag, %index, 'Player %1 has requested a duel (<a:gamelink\t3>ACCEPT</a>, <a:gamelink\t4>DECLINE</a>).',
                     %client.clientDuelRequestedBy.name);
      %index++;
   }

   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient( %client, 'ClearHud', "", %tag, %index ); 
     
}
function DMHud(%game, %client, %tag){// note in this game type the score hud can only display 30 or so players 
      
    if(%client.viewStats && $dtStats::enableRefresh){
         statsMenu(%client, %game.class);
         return;
   }
   else if(%client.viewStats && !$dtStats::enableRefresh){
      return;
   }  
   // Clear the header:
   messageClient( %client, 'SetScoreHudHeader', "", "" );
   
   // Send the subheader:
   messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,200,280,360,465>\tPLAYER\tRATING\tKILLS\tDEATHS\tBONUS');
   
   for (%index = 0; %index < $TeamRank[0, count]; %index++)
   {
      //get the client info
      %cl = $TeamRank[0, %index];
      
      //get the score
      %clScore = %cl.score;
      
      %clKills = mFloatLength( %cl.kills, 0 );
      %clDeaths = mFloatLength( %cl.deaths + %cl.suicides, 0 );
	  %clBonus = mFloor((%cl.Bonus * %game.SCORE_PER_BONUS) + (%cl.MidAir * %game.SCORE_PER_MIDAIR) + (%cl.KillStreakBonus * %game.SCORE_PER_KILLSTREAKBONUS ));
      %clStyle = %cl == %client ? "<color:dcdcdc>" : ""; 
	  
	  //%BonusValue = %client.Bonus * %game.SCORE_PER_BONUS;
	  //%MidAirValue = %client.MidAir * %game.SCORE_PER_MIDAIR;
      //%KillStreakBonusValue = %client.KillStreakBonus * %game.SCORE_PER_KILLSTREAKBONUS;
      
      //if the client is not an observer, send the message
      if (%client.team != 0)
      { 
         if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){                             //  <tab:15,235,340,415,500>\%5\%1\%2\%3\tBG'
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%6',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %clBonus);
         }
         else if(%client.name $= %cl.name){
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%6',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %clBonus); 
         }
         else{
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115>%1</clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%6',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %clBonus); 
         }
      }
      //else for observers, create an anchor around the player name so they can be observed
      else
      {
          if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){   
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t%6> %1</a></clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%7',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %cl, %clBonus);
          }
         else if(%client.name $= %cl.name){
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tStats\tView\t%1>+</a><a:gamelink\t%6> %1</a></clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%7',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %cl, %clBonus);  
         }
          else{
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\t%6> %1</a></clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%7',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %cl, %clBonus); 
         }
      }
   }
   
   // Tack on the list of observers:
   %observerCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.team == 0)
         %observerCount++;
   }
   
   if (%observerCount > 0)
   {
      messageClient( %client, 'SetLineHud', "", %tag, %index, "");
      %index++;
      messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
      %index++;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         //if this is an observer
         if (%cl.team == 0)
         {
            %obsTime = getSimTime() - %cl.observerStartTime;
            %obsTimeStr = %game.formatTime(%obsTime, false);
            if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){  
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
               %cl.name, %obsTimeStr );
            }
            else if(%client.name $= %cl.name){
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
               %cl.name, %obsTimeStr );
            }
            else{
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150> %1</clip><rmargin:260><just:right>%2',
               %cl.name, %obsTimeStr ); 
            }
            %index++;
         }
      }
   }
   
   //clear the rest of Hud so we don't get old lines hanging around...
   messageClient( %client, 'ClearHud', "", %tag, %index );
}
function LakRabbitHud(%game, %client, %tag){
      if(%client.viewStats && $dtStats::enableRefresh){
         statsMenu(%client, %game.class);
         return;
      }
      else if(%client.viewStats && !$dtStats::enableRefresh){
         return;
      }
      
      //tricky stuff here...  use two columns if we have more than 15 clients...
      %numClients = $TeamRank[0, count];
      if ( %numClients > $ScoreHudMaxVisible )
         %numColumns = 2;
      
      // Clear the header:
      messageClient( %client, 'SetScoreHudHeader', "", "" );
      
      // Send subheader:
      if (%numColumns == 2)
         messageClient(%client, 'SetScoreHudSubheader', "", '<tab:5,155,225,305,455,525>\tPLAYER\tSCORE\tTIME\tPLAYER\tSCORE\tTIME');
      else
         messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,235,335>\tPLAYER\tSCORE\tTIME');
      
      //recalc the score for whoever is holding the flag
      if (isObject($AIRabbitFlag.carrier))
         %game.recalcScore($AIRabbitFlag.carrier.client);
      
      %countMax = %numClients;
      if ( %countMax > ( 2 * $ScoreHudMaxVisible ) )
      {
         if ( %countMax & 1 )
            %countMax++;
         %countMax = %countMax / 2;
      }
      else if ( %countMax > $ScoreHudMaxVisible )
         %countMax = $ScoreHudMaxVisible;
      
      for (%index = 0; %index < %countMax; %index++)
      {
         //get the client info
         %col1Client = $TeamRank[0, %index];
         %col1ClientScore = %col1Client.score $= "" ? 0 : %col1Client.score;
         %col1Style = "";
         
         if (isObject(%col1Client.player.holdingFlag))
         {
            %col1ClientTimeMS = %col1Client.flagTimeMS + getSimTime() - %col1Client.startTime;
            %col1Style = "<color:00dc00>";
         }
         else
         {
            %col1ClientTimeMS = %col1Client.flagTimeMS;
            if ( %col1Client == %client )
               %col1Style = "<color:dcdcdc>";
         }
         
         if (%col1ClientTimeMS <= 0)
            %col1ClientTime = "";
         else
         {
            %minutes = mFloor(%col1ClientTimeMS / (60 * 1000));
            if (%minutes <= 0)
               %minutes = "0";
            %seconds = mFloor(%col1ClientTimeMS / 1000) % 60;
            if (%seconds < 10)
               %seconds = "0" @ %seconds;
            
            %col1ClientTime = %minutes @ ":" @ %seconds;
         }
         
         //see if we have two columns
         if (%numColumns == 2)
         {
            %col2Client = "";
            %col2ClientScore = "";
            %col2ClientTime = "";
            %col2Style = "";
            
            //get the column 2 client info
            %col2Index = %index + %countMax;
            if (%col2Index < %numClients)
            {
               %col2Client = $TeamRank[0, %col2Index];
               %col2ClientScore = %col2Client.score $= "" ? 0 : %col2Client.score;
               
               if (isObject(%col2Client.player.holdingFlag))
               {
                  %col2ClientTimeMS = %col2Client.flagTimeMS + getSimTime() - %col2Client.startTime;
                  %col2Style = "<color:00dc00>";
               }
               else
               {
                  %col2ClientTimeMS = %col2Client.flagTimeMS;
                  if ( %col2Client == %client )
                     %col2Style = "<color:dcdcdc>";
               }
               
               if (%col2ClientTimeMS <= 0)
                  %col2ClientTime = "";
               else
               {
                  %minutes = mFloor(%col2ClientTimeMS / (60 * 1000));
                  if (%minutes <= 0)
                     %minutes = "0";
                  %seconds = mFloor(%col2ClientTimeMS / 1000) % 60;
                  if (%seconds < 10)
                     %seconds = "0" @ %seconds;
                  
                  %col2ClientTime = %minutes @ ":" @ %seconds;
               }
            }
         }
         
         //if the client is not an observer, send the message
         if (%client.team != 0)
         {
            if ( %numColumns == 2 ){
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  if(%col1Client.name !$= "" && %col2Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name !$= "" && %col2Client.name $= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= "" && %col2Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  
               }
               else{
                  if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tStats\tView\t%4>+</a>
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tStats\tView\t%4>+</a>
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  
               }
            }
            else{
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  if(%col1Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style );
               }
               else{
                  if(%col1Client.name $= %client.name)
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style );
               }
            }
         }
         //else for observers, create an anchor around the player name so they can be observed
         else
         {
            if ( %numColumns == 2 )
            {
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  //this is really crappy, but I need to save 1 tag - can only pass in up to %9, %10 doesn't work...
                  if (%col2Style $= "<color:00dc00>")//<a:gamelink\tStats\tView\t%1>+</a>
                  {
                     if(%col1Client.name !$= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     
                  }
                  else if (%col2Style $= "<color:dcdcdc>")
                  {
                     if(%col1Client.name !$= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
                  else
                  {
                     if(%col1Client.name !$= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
               }
               else{/////////////////////////////////////////////////////////////////////
                  if (%col2Style $= "<color:00dc00>")//<a:gamelink\tStats\tView\t%1>+</a><a:gamelink\tStats\tView\t%4>+</a>
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     
                  }
                  else if (%col2Style $= "<color:dcdcdc>")//<a:gamelink\tStats\tView\t%4>+</a>
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
                  else
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tStats\tView\t%4>+</a>
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tStats\tView\t%4>+</a>
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
                  
               }
            }
            else{
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  if(%col1Client.name !$= ""){
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
               }
               else{
                  if(%col1Client.name $= %client.name){
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
               }
            }
         }
      }
      
      // Tack on the list of observers:
      %observerCount = 0;
      for (%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         if (%cl.team == 0)
            %observerCount++;
      }
      
      if (%observerCount > 0) 
      {
         messageClient( %client, 'SetLineHud', "", %tag, %index, "");
         %index++;
         messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
         %index++;
         for (%i = 0; %i < ClientGroup.getCount(); %i++)
         {
            %cl = ClientGroup.getObject(%i);
            //if this is an observer
            if (%cl.team == 0)
            {
               %obsTime = getSimTime() - %cl.observerStartTime;
               %obsTimeStr = %game.formatTime(%obsTime, false);//<a:gamelink\tStats\tView\t%3>+</a>
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr);
               }
               else if(%client.name $= %cl.name){
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr);
               }
               else{
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr);
               }
               
               %index++;
            }
         }
      }
      
      //clear the rest of Hud so we don't get old lines hanging around...
      messageClient( %client, 'ClearHud', "", %tag, %index );
   }
 function CTFHud(%game, %client, %tag){// defaultGame/evo
      if(%client.viewStats && $dtStats::enableRefresh){
         statsMenu(%client, %game.class);
         return;
      }
      else if(%client.viewStats && !$dtStats::enableRefresh){
         return;
      }
      
      %ShowScores = ( $Host::TournamentMode || $Host::ShowIngamePlayerScores );
      
      if(Game.numTeams > 1)
      {
         // Send header:
         messageClient(%client, 'SetScoreHudHeader', "", '<tab:15,315>\t%1<rmargin:260><just:right>%2<rmargin:560><just:left>\t%3<just:right>%4', %game.getTeamName(1), $TeamScore[1], %game.getTeamName(2), $TeamScore[2]);
         
         if ( !$TeamRank[1, count] )
         {
            $TeamRank[1, count] = 0;
         }
         
         if ( !$TeamRank[2, count] )
         {
            $TeamRank[2, count] = 0;
         }
         
         if ( $Host::EvoAveragePings )
         {
            for ( %count = 0; %count <= Game.numteams; %count++ )
            {
               %Ping[%count] = 0;
               %PingSq[%count] = 0;
               %PingCount[%count] = 0;
            }
            
            for ( %ClientCount = ClientGroup.getCount() -1 ; %ClientCount >= 0;
               %ClientCount-- )
            {
               %ThisClient = ClientGroup.getObject( %ClientCount );
               %Team = %ThisClient.team;
               
               %PingVal = %ThisClient.getPing();
               
               %Ping[%Team] += %PingVal;
               %PingSq[%Team] += ( %PingVal * %PingVal );
               %PingCount[%Team] ++;
            }
            
            for ( %count = 0; %count <= %game.numteams; %count++ )
            {
               if ( %PingCount[%count] )
               {
                  %Ping[%count]   /= %PingCount[%count];
                  %PingSq[%count] /= %PingCount[%count];
                  
                  %PingSq[%count] = msqrt( %PingSq[%count] - ( %Ping[%count] * %Ping[%count] ) );
                  
                  %Ping[%count]   = mfloor( %Ping[%count] );
                  %PingSq[%count] = mfloor( %PingSq[%count] );
                  
                  %PingString[%count] = "<spush><font:Arial:14>P<font:Arial:12>ING: " @ %Ping[%count] @ " +/- " @ %PingSq[%count] @ "ms   <spop>";
               }
            }
         }
         messageClient( %client, 'SetScoreHudSubheader', "",
         '<tab:25,325>\tPLAYERS (%1)<rmargin:260><just:right>%4%3<rmargin:560><just:left>\tPLAYERS (%2)<just:right>%5%3', $TeamRank[1, count], $TeamRank[2, count], (%ShowScores?'SCORE':''),%PingString[1],%PingString[2]);
         
         %index = 0;
         while(true)
         {
            
            if(%index >= $TeamRank[1, count]+2 && %index >= $TeamRank[2, count]+2)
               break;
            
            //get the team1 client info
            %team1Client = "";
            %team1ClientScore = "";
            %col1Style = "";
            if(%index < $TeamRank[1, count])
            {
               %team1Client = $TeamRank[1, %index];
               
               if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores && %team1Client.score >= 0)
                  %team1ClientScore = 0;
               else
                  %team1ClientScore = %team1Client.score $= "" ? 0 : %team1Client.score;
               
               %col1Style = %team1Client == %client ? "<color:dcdcdc>" : "";
               
               if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
                  %team1playersTotalScore = 0;
               else
                  %team1playersTotalScore += %team1Client.score;
            }
            else if(%index == $teamRank[1, count] && $teamRank[1, count] != 0 && %game.class $= "CTFGame")
            {
               %team1ClientScore = "--------------";
            }
            else if(%index == $teamRank[1, count]+1 && $teamRank[1, count] != 0 && %game.class $= "CTFGame")
            {
               if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
                  %team1ClientScore = 0;
               else
                  %team1ClientScore = %team1playersTotalScore != 0 ? %team1playersTotalScore : 0;
            }
            
            //get the team2 client info
            %team2Client = "";
            %team2ClientScore = "";
            %col2Style = "";
            if(%index < $TeamRank[2, count])
            {
               %team2Client = $TeamRank[2, %index];
               
               if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores && %team2Client.score >= 0)
                  %team2ClientScore = 0;
               else
                  %team2ClientScore = %team2Client.score $= "" ? 0 : %team2Client.score;
               
               %col2Style = %team2Client == %client ? "<color:dcdcdc>" : "";
               
               if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
                  %team2playersTotalScore = 0;
               else
                  %team2playersTotalScore += %team2Client.score;
            }
            else if(%index == $teamRank[2, count] && $teamRank[2, count] != 0 && %game.class $= "CTFGame")
            {
               %team2ClientScore = "--------------";
            }
            else if(%index == $teamRank[2, count]+1 && $teamRank[2, count] != 0 && %game.class $= "CTFGame")
            {
               if(!$Host::TournamentMode && !$Host::ShowIngamePlayerScores)
                  %team2ClientScore = 0;
               else
                  %team2ClientScore = %team2playersTotalScore != 0 ? %team2playersTotalScore : 0;
            }
            
            if (!%ShowScores)
            {
               %team1ClientScore = '';
               %team2ClientScore = '';
            }
            
            if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
               if(%client.team != 0){ //if the client is not an observer, send the message
                  if(%team1Client.name !$= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$= "" && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>  %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
               else{ //else for observers, create an anchor around the player name so they can be observed
                  //messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  
                  if(%team1Client.name !$= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$= "" && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
            }
            else{
               if(%client.team != 0){
                  if(%team1Client.name $= %client.name && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$=""  && %team2Client.name $= %client.name)
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= %client.name && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name $= %client.name)
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
               else{ //else for observers, create an anchor around the player name so they can be observed
                  //messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  if(%team1Client.name $= %client.name && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$= "" && %team2Client.name $= %client.name)
                     mssageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= %client.name && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tStats\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name $= %client.name)
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tStats\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
               
            }
            
            %index++;
         }
      }
      else
      {
         //tricky stuff here...  use two columns if we have more than 15 clients...
         %numClients = $TeamRank[0, count];
         if(%numClients > $ScoreHudMaxVisible)
            %numColumns = 2;
         
         // Clear header:
         messageClient(%client, 'SetScoreHudHeader', "", "");
         
         // Send header:
         if(%numColumns == 2)
            messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15,315>\tPLAYER<rmargin:270><just:right>%1<rmargin:570><just:left>\tPLAYER<just:right>%1', (%ShowScores?'SCORE':''));
         else
            messageClient(%client, 'SetScoreHudSubheader', "", '<tab:15>\tPLAYER<rmargin:270><just:right>%1', (%ShowScores?'SCORE':''));
         
         %countMax = %numClients;
         if(%countMax > ( 2 * $ScoreHudMaxVisible ))
         {
            if(%countMax & 1)
               %countMax++;
            %countMax = %countMax / 2;
         }
         else if(%countMax > $ScoreHudMaxVisible)
            %countMax = $ScoreHudMaxVisible;
         
         for(%index = 0; %index < %countMax; %index++)
         {
            //get the client info
            %col1Client = $TeamRank[0, %index];
            %col1ClientScore = %col1Client.score $= "" ? 0 : %col1Client.score;
            %col1Style = %col1Client == %client ? "<color:dcdcdc>" : "";
            
            //see if we have two columns
            if(%numColumns == 2)
            {
               %col2Client = "";
               %col2ClientScore = "";
               %col2Style = "";
               
               //get the column 2 client info
               %col2Index = %index + %countMax;
               if(%col2Index < %numClients)
               {
                  %col2Client = $TeamRank[0, %col2Index];
                  %col2ClientScore = %col2Client.score $= "" ? 0 : %col2Client.score;
                  %col2Style = %col2Client == %client ? "<color:dcdcdc>" : "";
               }
            }
            
            if ( !%ShowScores )
            {
               %col1ClientScore = "";
               %col2ClientScore = "";
            }
            
            //if the client is not an observer, send the message
            if(%client.team != 0)
            {
               if(%numColumns == 2)
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25,325>\t<spush>%5<clip:195>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:195>%3</clip><just:right>%4', %col1Client.name, %col1ClientScore, %col2Client.name, %col2ClientScore, %col1Style, %col2Style);
               else
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25>\t%3<clip:195>%1</clip><rmargin:260><just:right>%2', %col1Client.name, %col1ClientScore, %col1Style);
            }
            //else for observers, create an anchor around the player name so they can be observed
            else
            {
               if(%numColumns == 2)
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:25,325>\t<spush>%5<clip:195><a:gamelink\t%7>%1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:195><a:gamelink\t%8>%3</a></clip><just:right>%4', %col1Client.name, %col1ClientScore, %col2Client.name, %col2ClientScore, %col1Style, %col2Style, %col1Client, %col2Client);
               else
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:25>\t%3<clip:195><a:gamelink\t%4>%1</a></clip><rmargin:260><just:right>%2', %col1Client.name, %col1ClientScore, %col1Style, %col1Client);
            }
         }
         
      }
      
      // Tack on the list of observers:
      %observerCount = 0;
      for(%i = 0; %i < ClientGroup.getCount(); %i++)
      {
         %cl = ClientGroup.getObject(%i);
         if(%cl.team == 0)
            %observerCount++;
      }
      
      if(%observerCount > 0)
      {
         messageClient(%client, 'SetLineHud', "", %tag, %index, "");
         %index++;
         messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:10, 310><spush><font:Univers Condensed:22>\tOBSERVERS (%1)<rmargin:260><just:right>TIME<spop>', %observerCount);
         %index++;
         for(%i = 0; %i < ClientGroup.getCount(); %i++)
         {
            %cl = ClientGroup.getObject(%i);
            //if this is an observer
            if(%cl.team == 0)
            {
               %obsTime = getSimTime() - %cl.observerStartTime;
               %obsTimeStr = %game.formatTime(%obsTime, false);
               if(%client.isAdmin ||%client.isSuperAdmin || !$dtStats::viewSelf){
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
                  
               }
               else if(%cl == %client){
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
               }
               else{
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150>%1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr);
               }
               %index++;
            }
         }
      }
      
      //clear the rest of Hud so we don't get old lines hanging around...
      messageClient(%client, 'ClearHud', "", %tag, %index);
   }
function dtStatsMissionDropReady(%game, %client){ // called when client has finished loading
   if($dtStats::debugEchos){error("dtStatsMissionDropReady GUID = "  SPC %client.guid);}  
   %client.lp = "";
   if(%client.guid $= ""){ resetDtStats(%client,%game.class,1); return;}
   %foundOld = 0;
   if(!%client.isAIControlled() && !isObject(%client.dtStats))
   {
      for (%i = 0; %i < statsGroup.getCount(); %i++)
      { // check to see if my old data is still there
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.guid == %client.guid && %dtStats.markForDelete == 0)
         {
            if(%dtStats.leftPCT < $dtStats::fgPercentage[%game.class])
            {
               %client.dtStats.dtGameCounter = 0;
            }
            %client.dtStats = %dtStats;
            %dtStats.client = %client;
            %dtStats.name = %client.name;
            %dtStats.clientLeft = 0;
            %dtStats.markForDelete = 0;
            %foundOld =1;
            resGameStats(%client,%game.class); // restore stats;
            if(%client.score >= 1 || %client.score <= -1 )
            { //if(%num >= 1 || %num <= -1 ){
               messageClient(%client, 'MsgClient', '\crWelcome back %1. Your score has been restored.~wfx/misc/rolechange.wav', %client.name);
            }
            break;
         }
      }
      if(!%foundOld)
      {
         %dtStats = new scriptObject(); // object used stats storage
         statsGroup.add(%dtStats);
         %client.dtStats = %dtStats;
         %dtStats.gameCount[%game.class] = 0;
         %dtStats.totalNumGames[%game.class] = 0;
         %dtStats.totalGames[%game.class] = 0;
         %dtStats.statsOverWrite[%game.class] = 0;
         %dtStats.client =%client;
         %dtStats.guid = %client.guid;
         %dtStats.name =%client.name;
         %dtStats.clientLeft = 0;
         %dtStats.markForDelete = 0;
         %dtStats.lastGame[%game.class] = 0;
		 resetDtStats(%client,%game.class,1);
         loadGameStats(%client.dtStats,%game.class);
         %client.dtStats.gameData[%game.class] = 1;
         %client.dtStats.dtGameCounter = 0;
         
         if(Game.getGamePct() <= (100 - $dtStats::fgPercentage[%game.class])){// see if they will be here long enough to count as a full game
            %client.dtStats.dtGameCounter++;
         }
      }
   }
   else if(isObject(%client.dtStats) && %client.dtStats.gameData[%game.class] $= "")
   { // game type change
      %client.dtStats.gameCount[%game.class] = 0;
      %client.dtStats.totalNumGames[%game.class] = 0;
      %client.dtStats.totalGames[%game.class] = 0;
      %client.dtStats.statsOverWrite[%game.class] = 0;
      loadGameStats(%client.dtStats,%game.class);
      %client.dtStats.gameData[%game.class] = 1;
   }
}
function dtStatsClientLeaveGame(%game, %client){
   if(!%client.isAiControlled() && isObject(%client.dtStats)){
      if(%client.score < 1 && %client.score > -1 ){ // if(%num < 1 && %num > -1 ){
         if(isObject(%client.dtStats)){
            %client.dtStats.delete();
         }
         return;
      }
      %client.dtStats.clientLeft = 1;
      %game.gameWinStat(%client);
      bakGameStats(%client,%game.class);//back up there current game in case they lost connection
      %client.dtStats.leftPCT = %game.getGamePct();
   }
}

function dtStatsTimeLimitReached(%game){
   %game.timeLimitHit = 1;
   dtStatsLimitHit(%game);
}
function dtStatsScoreLimitReached(%game){
   %game.scoreLimitHit = 1;
   dtStatsLimitHit(%game);
}
function dtStatsLimitHit(%game){
   for (%i = 0; %i < ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      %game.gameWinStat(%client);
      if(!%client.isAiControlled() && isObject(%client.dtStats)){
         if( %client.dtStats.dtGameCounter > 0){ //we throw out the first game as we joined it in progress
            incGameStats(%client,%game.class); // setup for next game
         }
         %client.dtStats.dtGameCounter++;
      }
   }
}
function dtStatsGameOver( %game ){
   if($dtStats::debugEchos){error("dtStatsGameOver");}
   %timeNext =0;
   for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients data
      %dtStats = statsGroup.getObject(%i);
      if(%dtStats.clientLeft){ // find any that left during the match and
         if(%dtStats.leftPCT >= $dtStats::fgPercentage[%game.class]){ // if they where here for most of it and left at the end save it
            %dtStats.markForDelete = 1;
            incBakGameStats(%dtStats,%game.class);// dump the backup into are stats and save
            %time += %timeNext; // this will chain them
            %timeNext = $dtStats::slowSaveTime;
            schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
         }
         else{
            %dtStats.markForDelete = 1;
            %time += %timeNext; // this will chain them
            %timeNext = $dtStats::slowSaveTime;
            schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
         }
      }
   }
   for (%z = 0; %z < ClientGroup.getCount(); %z++){
      %client = ClientGroup.getObject(%z);
      %client.viewMenu = 0; // reset hud
      %client.viewClient = 0;
      %client.viewStats = 0;
      if(!%client.isAiControlled() && isObject(%client.dtStats)){
         if(%game.scoreLimitHit != 1 && %game.timeLimitHit != 1){
            %game.gameWinStat(%client);
            //make sure the game was long enough, in case admin changes maps 
            if(%game.getGamePct() >= $dtStats::fgPercentage[%game.class]  && %client.dtstats.dtGameCounter > 0){
               incGameStats(%client,%game.class); //resetDtStats after incGame
            }
            else{
               %client.dtStats.dtGameCounter++;
               resetDtStats(%client,%game.class,0);
            }
         }
         if(%client.dtStats.lastGame[%game.class] > 0){
            %time += %timeNext; // this will chain them
            %timeNext = $dtStats::slowSaveTime;
            schedule(%time ,0,"saveGameStats",%client.dtStats,%game.class); //
         }
      }
   }
}

////////////////////////////////////////////////////////////////////////////////
//							Supporting Functions							  //
////////////////////////////////////////////////////////////////////////////////

function DefaultGame::gameWinStat(%game,%client){
   if(%game.class $= "CTFGame" || %game.class $= "SCtFGame"){
      if($teamScore[1] == $teamScore[2]){
         %client.winCount = 0;
         %client.lossCount = 0;
      }
      else if($teamScore[1] > $teamScore[2] && %client.team == 1)
         %client.winCount = 1;
      else if($teamScore[2] > $teamScore[1]  && %client.team == 2)
         %client.winCount = 1;
      else
         %client.lossCount = 1;
   }
}
function DefaultGame::getGamePct(%game)
{
   if(!$MatchStarted){
      return 0;// if we are not running prob between games
   }
   %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   return %timePct;
   
}
function ArenaGame::getGamePct(%game)
{
   if(!$MatchStarted){
      return 0;// if we are not running prob between games
   }
     // Verify that there is a roundlimit and that the team has met it
     if(%game.roundLimit != 0){
        if( $TeamScore[1] >= $TeamScore[2]){
           return ($TeamScore[1] / %game.roundLimit) * 100;
        }
        else if( $TeamScore[1] <= $TeamScore[2]){
           return ($TeamScore[2] / %game.roundLimit) * 100;
        }
     }
   return 0;
}
function CTFGame::getGamePct(%game)
{
   if(!$MatchStarted){
      return 0;// if we are not running prob between games
   }
   %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
   
   if($TeamScore[1] > $TeamScore[2])
      %scorePct =  ($TeamScore[1] / %scoreLimit) * 100;
   else
      %scorePct =  ($TeamScore[2] / %scoreLimit) * 100;
   
   switch$($dtStats::fgPercentageType[%game.class]){
      case 0:
         return %scorePct;
      case 1:
         return %timePct;
      case 2:
         if(%scorePct > %timePct)
            return %scorePct;
         else
            return %timePct;
      default:
            return %timePct;
   }
}

function LakRabbitGame::getGamePct(%game)
{
   if(!$MatchStarted){
      return 0;// if we are not running prob between games
   }
   %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   
   %scoreLimit = MissionGroup.Rabbit_scoreLimit;
   if(%scoreLimit $= "")
      %scoreLimit = 2000;
   
   for (%i = 0; %i < ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if(%lScore < %client.score){
         %lScore = %client.score;
      }
   }
   %scorePct =  (%lScore / %scoreLimit) * 100;
   
   switch$($dtStats::fgPercentageType[%game.class]){
      case 0:
         return %scorePct;
      case 1:
         return %timePct;
      case 2:
         if(%scorePct > %timePct)
            return %scorePct;
         else
            return %timePct;
      default:
            return %timePct;
   }
   
}
function DMGame::getGamePct(%game)
{
   if(!$MatchStarted){
      return 0;// if we are not running prob between games
   }
   %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   
   %scoreLimit =  MissionGroup.DM_scoreLimit;
   if(%scoreLimit $= "")
      %scoreLimit = 25;
   
   for (%i = 0; %i < ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if(%lScore < %client.score){
         %lScore = %client.score;
      }
   }
   %scorePct =  (%lScore / %scoreLimit) * 100;
   
   switch$($dtStats::fgPercentageType[%game.class]){
      case 0:
         return %scorePct;
      case 1:
         return %timePct;
      case 2:
         if(%scorePct > %timePct)
            return %scorePct;
         else
            return %timePct;
      default:
          return %timePct;
 
   }
   
}
function SCtFGame::getGamePct(%game)
{
   if(!$MatchStarted){
      return 0;// if we are not running prob between games
   }
   %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
   %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
   
   %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
   if(%scoreLimit $= "")
      %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
   
   if($TeamScore[1] > $TeamScore[2])
      %scorePct =  ($TeamScore[1] / %scoreLimit) * 100;
   else
      %scorePct =  ($TeamScore[2] / %scoreLimit) * 100;
   
   switch$($dtStats::fgPercentageType[%game.class]){
      case 0:
         return %scorePct;
      case 1:
         return %timePct;
      case 2:
         if(%scorePct > %timePct)
            return %scorePct;
         else
            return %timePct;
      case 3:
         %mixPct =  (%scorePct + %timePct) / 2;
         return %mixPct;
         default:
         if(%scorePct > %timePct)
            return %scorePct;
         else
            return %timePct;
   }
}
function msToMinSec(%time)
{
   %sec = mFloor(%time / 1000);
   %min = mFloor(%sec / 60);
   %sec -= %min * 60;
   
   // pad it
   if(%min < 10)
      %min = "0" @ %min;
   if(%sec < 10)
      %sec = "0" @ %sec;

   return(%min @ ":" @ %sec);
}

function setFieldValue(%obj,%field,%value){
   if(isObject(%obj)){
      if(%value $= "")
         %value = 0;
      if(%field $= "")
         %field = "error";
      %format = %obj @ "." @ %field @ "=" @%value@ ";";
      eval(%format);//eww
   }
}
function getFieldValue(%obj,%field){
   if(isObject(%obj)){
      if(%field $= "")
         %field = "error";
      %format = "%result = " @ %obj @ "." @ %field @ ";";
      eval(%format);
      return %result;
   }
   else{
      return 0;
   }
}
function getCNameToCID(%name){
   
   if(isObject(%name) && %name.getClassName() $= "GameConnection"){
      return %name; // not a name its a client so return it
   }
   else{
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(getTaggedString(%client.name) $= %name){
            return %client;
         }
      }
   }
   return 0;
}

////////////////////////////////////////////////////////////////////////////////
//							Load Save Management							  //
////////////////////////////////////////////////////////////////////////////////
function loadGameStats(%dtStats,%game){// called when client joins server.cs onConnect
   if($dtStats::debugEchos){error("loadGameStats GUID = "  SPC %dtStats.guid);} 
   if(%dtStats.guid !$= "" && !$dtStats::Basic){
      loadGameTotalStats(%dtStats,%game);
      %filename = "serverStats/" @ %game @ "/" @ %dtStats.guid @ "/" @ 1 @ ".cs";
      if(isFile(%filename)){
         %file = new FileObject();
         %file.OpenForRead(%filename);
         while( !%file.isEOF() ){
            %line = %file.readline();
            %line = strreplace(%line,"%t","\t");
            %var = trim(getField(%line,0));
            %val = trim(getField(%line,1));
            %dtStats.gameStats[%var,1,%game] =  %val;
         }
         %file.close();
         %file.delete();
         if(%dtStats.gameCount[%game] > 1){
            schedule($dtStats::slowLoadTime,0,"loadGameSlow",%dtStats,2,%game);
         }
      }
   }
}
function loadGameTotalStats(%dtStats,%game){
   if($dtStats::debugEchos){error("loadGameTotalStats GUID = "  SPC %dtStats.guid);}
      %file = new FileObject();
      %filename = "serverStats/" @ %game @ "/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      if(isFile(%filename)){
         %file.OpenForRead(%filename);
         while( !%file.isEOF() ){
            %line = %file.readline();
            %line = strreplace(%line,"%t","\t");
            %var = trim(getField(%line,0));
            %val = trim(getField(%line,1));
            if(%var $= "gameCount"){
               if(%val > $dtStats::MaxNumOfGames){
                  %dtStats.gameCount[%game] = $dtStats::MaxNumOfGames;
               }
               else{
                  %dtStats.gameCount[%game] = %val;
               }
            }
            else if(%var $= "statsOverWrite"){
               %dtStats.statsOverWrite[%game] = %val;
            }
            else if(%var $= "totalNumGames"){
               %dtStats.totalNumGames[%game] = %val;
            }
            else if(%var $= "totalGames"){
               %dtStats.totalGames[%game] = %val;
            }
            else{
               if(%val > $dtStats::ValMax){//value is getting too big lets reset total stats
                  if($dtStats::debugEchos){error("Value Reset" SPC %var SPC "GUID = "  SPC %dtStats.guid);}
                  %dtStats.totalNumGames[%game] = 0;// reset 
                  for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++){
                     %var = $dtStats::fieldValue[%i,"dtStats"];
                      %dtStats.gameStats[%var,"t",%game] = 0;
                        
                  }
                  for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
                     %var = $dtStats::fieldValue[%i,%game];
                     %dtStats.gameStats[%var,"t",%game] = 0;
                  }
                  break;
               }
               else{
                  %dtStats.gameStats[%var,"t",%game] =  %val;
               }
            }
         }
         %file.close();
      }
   %file.delete();
}
function loadGameSlow(%dtStats,%i,%game){
   if(%dtStats.gameCount[%game] > 1){// load the rest
      if(%i <= %dtStats.gameCount[%game]){
         %filename = "serverStats/" @ %game @ "/" @ %dtStats.guid @ "/" @ %i @ ".cs";
         if(isFile(%filename)){
            %file = new FileObject();
            %file.OpenForRead(%filename);
            while( !%file.isEOF() ){
               %line = %file.readline();
               %line = strreplace(%line,"%t","\t");
               %var = trim(getField(%line,0));
               %val = trim(getField(%line,1));
               %dtStats.gameStats[%var,%i,%game] =  %val;
            }
            %file.close();
            %file.delete();
         }
         schedule($dtStats::slowLoadTime,0,"loadGameSlow",%dtStats,%i++,%game);
      }
   }
}

function saveGameStats(%dtStats,%game){
   if($dtStats::debugEchos){error("saveGameStats GUID = "  SPC %dtStats.guid);}
   if(%dtStats.lastGame[%game] > 0){
      if(%dtStats.guid !$= "" && !$dtStats::Basic){
         saveTotalStats(%dtStats,%game);
         %c = %dtStats.lastGame[%game];
         %file = new FileObject();
         %filename = "serverStats/" @ %game @ "/" @ %dtStats.guid @ "/" @ %c @ ".cs";
         
         %file.OpenForWrite(%filename);
         
         %file.writeLine("timeStamp" @ "%t" @ %dtStats.gameStats["timeStamp",%c,%game]);
         %file.writeLine("map" @ "%t" @ %dtStats.gameStats["map",%c,%game]);
         for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
            %val = $dtStats::fieldValue[%i,%game];
            %var = %dtStats.gameStats[%val,%c,%game];
            %file.writeLine(%val @ "%t" @ %var);
         }
         for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++){
            %val = $dtStats::fieldValue[%i,"dtStats"];
            %var = %dtStats.gameStats[%val,%c,%game];
            %file.writeLine(%val @ "%t" @ %var);
         }
         %file.close();
         %file.delete();
      }
      %dtStats.lastGame[%game] = 0;
      if(%dtStats.markForDelete){
         %dtStats.delete();
      }
   }
}
function saveTotalStats(%dtStats,%game){ // saved by the main save function
   if($dtStats::debugEchos){error("saveTotalStats GUID = "  SPC %dtStats.guid);}
   if(%dtStats.statsOverWrite[%game] $= ""){
      %dtStats.statsOverWrite[%game] = 0;
   }
      %filename = "serverStats/"@ %game @"/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      %file = new FileObject();
      %file.OpenForWrite(%filename);
      
      %file.writeLine("gameCount" @ "%t" @  %dtStats.gameCount[%game]);
      %file.writeLine("statsOverWrite" @ "%t" @ %dtStats.statsOverWrite[%game]);
      %file.writeLine("totalNumGames" @ "%t" @ %dtStats.totalNumGames[%game]);
      %file.writeLine("totalGames" @ "%t" @ %dtStats.totalGames[%game]);
      %file.writeLine("timeStamp" @ "%t" @ %dtStats.gameStats["timeStamp","t",%game]);
      for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
         %val = $dtStats::fieldValue[%i,%game];
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ %var);
      }
      for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++){
         %val = $dtStats::fieldValue[%i,"dtStats"];
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ %var);
      }
      %file.close();
      %file.delete();
}

function incGameStats(%client,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("incGameStats GUID = "  SPC %client.guid);}   
   %client.viewMenu = "Reset";
   if(%client.dtStats.gameCount[%game]  >= $dtStats::MaxNumOfGames){ // we have the max number allowed
      if(%client.dtStats.statsOverWrite[%game] < $dtStats::MaxNumOfGames){
         %c = %client.dtStats.statsOverWrite[%game]++;
      }
      else{
         %client.dtStats.statsOverWrite[%game] = 1; //reset
         %c = 1;
      }
   }
   else{
      %c = %client.dtStats.gameCount[%game]++; // number of games this player has played
   }
   %client.dtStats.lastGame[%game] = %c;
   %client.dtStats.gameStats["timeStamp",%c,%game] = formattimestring("hh:nn a, mm-dd");
   %client.dtStats.gameStats["map",%c,%game] = $MissionDisplayName;
   
   %client.dtStats.totalNumGames[%game]++;
   %client.dtStats.totalGames[%game]++;
   %client.dtStats.gameStats["timeStamp","t",%game] = formattimestring("hh:nn a, mm-dd");
   
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = getFieldValue(%client,%val);
      if(%val $= "flagTimeMS"){// convert to min
         %var = mfloor((%var / 1000) / 60);
      }
      %client.dtStats.gameStats[%val,%c,%game] = %var;
      %client.dtStats.gameStats[%val,"t",%game] += %var;
   }
   for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++){
      %val = $dtStats::fieldValue[%i,"dtStats"];
      %var = getFieldValue(%client,%val);
      %client.dtStats.gameStats[%val,%c,%game] = %var;
      %client.dtStats.gameStats[%val,"t",%game] += %var;
   }
   resetDtStats(%client,%game,0); // reset to 0 for next game
}

function incBakGameStats(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("incBakGameStats GUID = "  SPC %dtStats.guid);}    
   if(%dtStats.gameCount[%game]  >= $dtStats::MaxNumOfGames){ // we have the max number allowed
      if(%dtStats.statsOverWrite[%game] < $dtStats::MaxNumOfGames){
         %c = %dtStats.statsOverWrite[%game]++;
      }
      else{
         %dtStats.statsOverWrite[%game] = 1; //reset
         %c = 1;
      }
   }
   else{
      %c = %dtStats.gameCount[%game]++; // number of games this player has played
   }
   
   %dtStats.lastGame[%game] = %c;
   %dtStats.gameStats["timeStamp",%c,%game] = formattimestring("hh:nn a, mm-dd");
   %dtStats.gameStats["map",%c,%game] = $MissionDisplayName;
   
   %dtStats.totalNumGames[%game]++;
   %dtStats.totalGames[%game]++;
   
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = %dtStats.gameStats[%val,"b",%game];
      if(%val $= "flagTimeMS"){// convert to min
         %var = mfloor((%var / 1000) / 60);
      }
      %dtStats.gameStats[%val,%c,%game] = %var;
      %dtStats.gameStats[%val,"t",%game] += %var;
   }
   for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++){
      %val = $dtStats::fieldValue[%i,"dtStats"];
      %var = %dtStats.gameStats[%val,"b",%game];
      %dtStats.gameStats[%val,%c,%game] = %var;
      %dtStats.gameStats[%val,"t",%game] += %var;
   }
   
}

function bakGameStats(%client,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("bakGameStats GUID = "  SPC %client.guid);}   
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = getFieldValue(%client,%val);
      %client.dtStats.gameStats[%val,"b",%game] = %var;
   }
   for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++){
      %val = $dtStats::fieldValue[%i,"dtStats"];
      %var = getFieldValue(%client,%val);
      %client.dtStats.gameStats[%val,"b",%game] = %var;
   }
}

function resGameStats(%client,%game){// copy data back over to client
   if($dtStats::debugEchos){error("resGameStats GUID = "  SPC %client.guid);}
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++)
   {
      %val = $dtStats::fieldValue[%i,%game];
      %var = %client.dtStats.gameStats[%val,"b",%game];
      if(%val $= "winCount" || %val $= "lossCount")
      {
         %var = 0; // set to 0 becuase we came back and its not the end of the game
      }
      setFieldValue(%client,%val,%var);
   }
   for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++)
   {
      %val = $dtStats::fieldValue[%i,"dtStats"];
      %var = %client.dtStats.gameStats[%val,"b",%game];
      setFieldValue(%client,%val,%var);
   }
}

// resets stats that are used in this file
//the others are handled with in the gametype it self
function resetDtStats(%client,%game,%g){
   if($dtStats::debugEchos){error("resetDtStats GUID = "  SPC %client.guid);}
   for(%i = 1; %i <= $dtStats::fieldCount["dtStats"]; %i++)
   {
      %val = $dtStats::fieldValue[%i,"dtStats"];
      setFieldValue(%client,%val,0);
   }
   if(%g){
      for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++)
      {
         %val = $dtStats::fieldValue[%i,%game];
         setFieldValue(%client,%val,0);
      }
   }
   %client.winCount = 0;// this is isolated becuase only certain game types use this
   %client.lossCount = 0;
}
////////////////////////////////////////////////////////////////////////////////
//Stats Collecting
////////////////////////////////////////////////////////////////////////////////
function clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation){
   if(!isObject(%clKiller) && isObject(%implement)){
      %clKiller = %implement.getControllingClient();  
   }

   %clVictim.lp = "";
   %ttlAvg = 5;
   %clVictim.ttl[%clVictim.ttlC++]  = getSimTime() - %clVictim.spawnTime;
   if(%clVictim.ttlC >= %ttlAvg){
      for(%t = 1; %t <= %clVictim.ttlC; %t++){%time += %clVictim.ttl[%t];}
      %clVictim.timeTL = %time/%ttlAvg;
      %clVictim.ttlC = 0;
      %clVictim.ttlH = 1;
   }
   else{
      if(%clVictim.ttlH){
         for(%t = 1; %t <= %ttlAvg; %t++){%time += %clVictim.ttl[%t];}
         %clVictim.timeTL = %time/%ttlAvg; 
      }
      else{
         for(%t = 1; %t <= %clVictim.ttlC; %t++){%time += %clVictim.ttl[%t];}
         %clVictim.timeTL = %time/%clVictim.ttlC;
      }
   }

   if(%clKiller.team != %clVictim.team){
      
      if(isObject(%clKiller.player) && isObject(%clVictim.player)){
         %dis = vectorDist(%clKiller.player.getPosition(),%clVictim.player.getPosition());
         %vD = vectorSub(%clVictim.player.getVelocity(),%clKiller.player.getVelocity());
         %vel = vectorLen(%vD);
      }
      else{
        %vel = 0;
        %dis = 0; 
      }
      
      %clKiller.k++; 
		%clVictim.k = 0;
      if(%clVictim == %clKiller || %damageType == $DamageType::Suicide || %damageType == $DamageType::Lava || %damageType == $DamageType::OutOfBounds || %damageType == $DamageType::Ground || %damageType == $DamageType::Lightning){
		  %clVictim.k = %clKiller.k = 0;
		}
		if(%clKiller.killStreak < %clKiller.k){
		   %clKiller.killStreak = %clKiller.k;
		}
		
      if(%clVictim.player.hitBy[%clKiller]){
            %clKiller.assist--;// they are the killer there for remove it;
      }
      
      %isCombo = 0;
      if(%clKiller.player.combo[%clVictim.player] > 1){
         %clKiller.comboPT = %clKiller.player.combo[%clVictim.player];
         %clKiller.comboCount++;
         %isCombo =1;
      }
      if(isObject(%clVictim.player) && isObject(%clKiller.player)){// armor kill stats
         switch$(%clVictim.player.getArmorSize()){
            case "Light":%clKiller.armorL++; %clVictim.armorLD++;
               switch$(%clKiller.player.getArmorSize()){
                  case "Light": %clKiller.armorLL++; %clVictim.armorLLD++;
                  case "Medium":%clKiller.armorML++; %clVictim.armorLMD++;
                  case "Heavy": %clKiller.armorHL++; %clVictim.armorLHD++;
               }
            case "Medium": %clKiller.armorM++; %clVictim.armorMD++;
               switch$(%clKiller.player.getArmorSize()){
                  case "Light": %clKiller.armorLM++; %clVictim.armorMLD++;
                  case "Medium":%clKiller.armorMM++; %clVictim.armorMMD++;
                  case "Heavy": %clKiller.armorHM++; %clVictim.armorMHD++;
               }
            case "Heavy":%clKiller.armorH++; %clVictim.armorHD++;
               switch$(%clKiller.player.getArmorSize()){
                  case "Light": %clKiller.armorLH++; %clVictim.armorHLD++;
                  case "Medium":%clKiller.armorMH++; %clVictim.armorHMD++;
                  case "Heavy": %clKiller.armorHH++; %clVictim.armorHHD++;
               }
         } 
      }
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %clKiller.cgKills++;
            %clVictim.cgDeaths++;
            if(%clKiller.cgMax < %dis){%clKiller.cgMax = %dis;  }
            if(%clKiller.cgT < %vel){%clKiller.cgT = %vel;}  
            if(%isCombo){%clKiller.cgCom++;}
         case $DamageType::Disc:
            %clKiller.discKills++;
            %clVictim.discDeaths++;
            if(%clKiller.discMax < %dis){%clKiller.discMax = %dis;}
            if(%clKiller.discT < %vel){%clKiller.discT = %vel;} 
            if(%isCombo){%clKiller.discCom++;} 
         case $DamageType::Grenade:
            if($dtObjExplode.dtNade){
               %clKiller.hGrenadeKills++;
               %clVictim.hGrenadeDeaths++;
               if(%clKiller.hGrenadeMax < %dis){%clKiller.hGrenadeMax = %dis;}
               if(%clKiller.hGrenadeT < %vel){%clKiller.hGrenadeT = %vel;}  
               if(%isCombo){%clKiller.hGrenadeCom++;}
            }
            else{
               %clKiller.grenadeKills++;
               %clVictim.grenadeDeaths++;
               if(%clKiller.grenadeMax < %dis){%clKiller.grenadeMax = %dis;}
               if(%clKiller.grenadeT < %vel){%clKiller.grenadeT = %vel;} 
               if(%isCombo){%clKiller.grenadeCom++;}
            }
         case $DamageType::Laser:
            %clKiller.laserKills++;
            %clVictim.laserDeaths++;
            if(%damageLocation $= "head"){%clKiller.laserHeadShot++;}
            if(%clKiller.laserMax < %dis){%clKiller.laserMax = %dis;}
            if(%clKiller.laserT < %vel){%clKiller.laserT = %vel;}
            if(%isCombo){%clKiller.laserCom++;}
         case $DamageType::Mortar:
            %clKiller.mortarKills++;
            %clVictim.mortarDeaths++;
            if(%clKiller.mortarMax < %dis){%clKiller.mortarMax = %dis;}
            if(%clKiller.mortarT < %vel){%clKiller.mortarT = %vel;}
            if(%isCombo){%clKiller.mortarCom++;}
         case $DamageType::Missile:
            %clKiller.missileKills++;
            %clVictim.missileDeaths++;
            if(%clKiller.missileMax < %dis){%clKiller.missileMax = %dis;}
            if(%clKiller.missileT < %vel){%clKiller.missileT = %vel;}
         case $DamageType::ShockLance:
            %clKiller.shockLanceKills++;
            %clVictim.shockLanceDeaths++;
            if(%clKiller.shockMax < %dis){%clKiller.shockMax = %dis;}
            if(%clVictim.rearshot){%clKiller.shockRearShot++;}
            if(%clKiller.shockT < %vel){%clKiller.shockT = %vel;}
            if(%isCombo){%clKiller.shockLanceCom++;}
         case $DamageType::Plasma:
            %clKiller.plasmaKills++;
            %clVictim.plasmaDeaths++;
            if(%clKiller.plasmaMax < %dis){%clKiller.plasmaMax = %dis;}
            if(%clKiller.plasmaT < %vel){%clKiller.plasmaT = %vel;}
            if(%isCombo){%clKiller.plasmaCom++;}
         case $DamageType::Blaster:
            %clKiller.blasterKills++;
            %clVictim.blasterDeaths++;
            if(%clKiller.blasterMax < %dis){%clKiller.blasterMax = %dis;}
            if(%clKiller.blasterT < %vel){%clKiller.blasterT = %vel;}
            if(%isCombo){%clKiller.blasterCom++;}
         case $DamageType::ELF:
            %clKiller.elfKills++;
            %clVictim.elfDeaths++;
         case $DamageType::Mine:
            %clKiller.mineKills++;
            %clVictim.mineDeaths++;
            if(%clKiller.mineMax < %dis){%clKiller.mineMax = %dis;}
            if(%clKiller.mineT < %vel){%clKiller.mineT = %vel;}
            if(%isCombo){%clKiller.mineCom++;}
         case $DamageType::Explosion:
            %clKiller.explosionKills++;
            %clVictim.explosionDeaths++;
         case $DamageType::Impact:
            if(isObject(%clKiller.vehicleMounted)){
               %veh =   %clKiller.vehicleMounted.getDataBlock().getName();
               switch$(%veh){
                  case "ScoutVehicle":     %clKiller.wildRK++;       %clVictim.wildRD++;
                  case "AssaultVehicle":   %clKiller.assaultRK++;    %clVictim.assaultRD++;
                  case "MobileBaseVehicle":%clKiller.mobileBaseRK++; %clVictim.mobileBaseRD++;
                  case "ScoutFlyer":       %clKiller.scoutFlyerRK++; %clVictim.scoutFlyerRD++;
                  case "BomberFlyer":      %clKiller.bomberFlyerRK++;%clVictim.bomberFlyerRD++;
                  case "HAPCFlyer":        %clKiller.hapcFlyerRK++;  %clVictim.hapcFlyerRD++;
               }
            }
            else{
               if(isObject(%implement))
                  %veh = %implement.getDataBlock().getName();
               switch$(%veh){
                  case "ScoutVehicle":     %clVictim.wildEK++;
                  case "AssaultVehicle":   %clVictim.assaultEK++;
                  case "MobileBaseVehicle":%clVictim.mobileBaseEK++;
                  case "ScoutFlyer":       %clVictim.scoutFlyerEK++;
                  case "BomberFlyer":      %clVictim.bomberFlyerEK++;
                  case "HAPCFlyer":        %clVictim.hapcFlyerEK++;
               }
            }
            %clKiller.impactKills++;
            %clVictim.impactDeaths++;
         case $DamageType::Ground:
            %clKiller.groundKills++;
            %clVictim.groundDeaths++;
         case $DamageType::Turret:
            %clKiller.turretKills++;
            %clVictim.turretDeaths++;
         case $DamageType::PlasmaTurret:
            %clKiller.plasmaTurretKills++;
            %clVictim.plasmaTurretDeaths++;
         case $DamageType::AATurret:
            %clKiller.aaTurretKills++;
            %clVictim.aaTurretDeaths++;
         case $DamageType::ElfTurret:
            %clKiller.elfTurretKills++;
            %clVictim.elfTurretDeaths++;
         case $DamageType::MortarTurret:
            %clKiller.mortarTurretKills++;
            %clVictim.mortarTurretDeaths++;
         case $DamageType::MissileTurret:
            %clKiller.missileTurretKills++;
            %clVictim.missileTurretDeaths++;
         case $DamageType::IndoorDepTurret:
            %clKiller.indoorDepTurretKills++;
            %clVictim.indoorDepTurretDeaths++;
         case $DamageType::OutdoorDepTurret:
            %clKiller.outdoorDepTurretKills++;
            %clVictim.outdoorDepTurretDeaths++;
         case $DamageType::SentryTurret:
            %clKiller.sentryTurretKills++;
            %clVictim.sentryTurretDeaths++;
         case $DamageType::OutOfBounds:
            %clKiller.outOfBoundKills++;
            %clVictim.outOfBoundDeaths++;
         case $DamageType::Lava:
            %clKiller.lavaKills++;
            %clVictim.lavaDeaths++;
         case $DamageType::ShrikeBlaster:
            %clKiller.shrikeBlasterKills++;
            %clVictim.shrikeBlasterDeaths++;
         case $DamageType::BellyTurret:
            %clKiller.bellyTurretKills++;
            %clVictim.bellyTurretDeaths++;
         case $DamageType::BomberBombs:
            %clKiller.bomberBombsKills++;
            %clVictim.bomberBombsDeaths++;
         case $DamageType::TankChaingun:
            %clKiller.tankChaingunKills++;
            %clVictim.tankChaingunDeaths++;
         case $DamageType::TankMortar:
            %clKiller.tankMortarKills++;
            %clVictim.tankMortarDeaths++;
         case $DamageType::SatchelCharge:
            %clKiller.satchelChargeKills++;
            %clVictim.satchelChargeDeaths++;
         case $DamageType::MPBMissile:
            %clKiller.mpbMissileKills++;
            %clVictim.mpbMissileDeaths++;
         case $DamageType::Lightning:
            %clKiller.lightningKills++;
            %clVictim.lightningDeaths++;
         case $DamageType::VehicleSpawn:
            %clKiller.vehicleSpawnKills++;
            %clVictim.vehicleSpawnDeaths++;
         case $DamageType::ForceFieldPowerup:
            %clKiller.forceFieldPowerUpKills++;
            %clVictim.forceFieldPowerUpDeaths++;
         case $DamageType::Crash:
               %veh =  %clVictim.vehDBName;
            switch$(%veh){
               case "ScoutVehicle":     %clVictim.wildCrash++;
               case "AssaultVehicle":   %clVictim.assaultCrash++;
               case "MobileBaseVehicle":%clVictim.mobileBaseCrash++;
               case "ScoutFlyer":       %clVictim.scoutFlyerCrash++;
               case "BomberFlyer":      %clVictim.bomberFlyerCrash++;
               case "HAPCFlyer":        %clVictim.hapcFlyerCrash++;
            }
            %clKiller.crashKills++;
            %clVictim.crashDeaths++;
         case $DamageType::Water:
            %clKiller.waterKills++;
            %clVictim.waterDeaths++;
         case $DamageType::NexusCamping:
            %clKiller.nexusCampingKills++;
            %clVictim.nexusCampingDeaths++;
         }
      }
   }
function mdReset(%client){
 %client.md = 0;  
}
function rayTest(%targetObject,%dis){
   %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
   %rayStart = %targetObject.getWorldBoxCenter();
   %rayEnd = VectorAdd(%rayStart,"0 0" SPC (%dis * -1));
   %ground = !ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject);  
   return %ground; 
}
function clientDmgStats(%data,%pos,%sourceObject, %targetObject, %damageType,%amount){
   %t = %s = 0;
   if(isObject(%sourceObject)){      
      if(%sourceObject.getClassName() !$= "Player"){
         %client = %sourceObject.getControllingClient();
         %s = 1;
      }
      else{
         %client = %sourceObject.client;
         %s = 1;
         %pos = getWords(%sourceObject.getPosition(),0,1) SPC 0;
         if(%client.lp !$= ""){%client.distT += vectorDist(%client.lp,%pos);}
            %client.lp = %pos;
      }
   }
   else{
    return;  
   }
   if(isObject(%targetObject)){
      if(%targetObject.getClassName() !$= "Player"){
          %targetClient = %targetObject.getControllingClient();
          %t = 1;
      }
      else {
         %targetClient = %targetObject.client;
         %t = 1;
         %pos = getWords(%targetObject.getPosition(),0,1) SPC 0;
         if(%targetClient.lp !$= ""){%targetClient.distT += vectorDist(%targetClient.lp,%pos);}
            %targetClient.lp = %pos;
      }
   }
   if(%damageType > 0 && %sourceObject != %targetObject){
      if(%t && %s){
         if(%targetClient != %client && %targetClient.team != %client.team){
            if(!%targetObject.hitBy[%client]){
               %client.assist++;
               %targetObject.hitBy[%client] = 1; 
            }
            if(!%targetObject.combo[%client,%damageType]){
               %targetObject.combo[%client,%damageType] = 1;
               %client.player.combo[%targetObject]++;
            }
         }
         if(isObject(%client.player)){
            %speed = vectorLen(%client.player.getVelocity());
            %vD = vectorSub(%targetObject.getVelocity(),%client.player.getVelocity());
            %vel = vectorLen(%vD);
            if(%client.maxRV < %vel){%client.maxRV = %vel;} 
            if(%client.maxSpeed < %speed){%client.maxSpeed = %speed;}
            %targetClient.avgTSpeed += %vel; %targetClient.avgSpeedCount++;
            %targetClient.avgSpeed = %targetClient.avgTSpeed/%targetClient.avgSpeedCount;
            if(%targetClient.avgSpeedCount >= 50){%targetClient.avgSpeedCount=%targetClient.avgTSpeed=0;}
         }
      }
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Blaster:
            %client.blasterDmg += %amount;
            %client.blasterDirectHits++;
            %client.blasterACC = (%client.blasterDirectHits / %client.blasterShotsFired) * 100;
            if(%t){
               %targetClient.blasterDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){%client.blasterMA++;}
            }
         case $DamageType::Plasma:
            %client.plasmaInDmg += %amount;
            %client.plasmaIndirectHits++;
            %client.plasmaACC = (%client.plasmaIndirectHits / %client.plasmaShotsFired) * 100;
            if(%t){
               %targetClient.plasmaInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,PlasmaBolt.damageRadius+1)){%client.plasmaMA++;}
            }
         case $DamageType::Bullet:
            %client.cgDmg += %amount;
            %client.cgDirectHits++;
            %client.cgACC = (%client.cgDirectHits / %client.cgShotsFired) * 100;
            if(%t){
               %targetClient.cgDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){%client.cgMA++;}
            }
         case $DamageType::Disc:
            %client.discInDmg += %amount;
            %client.discIndirectHits++;
            %client.discACC = (%client.discIndirectHits / %client.discShotsFired) * 100;
            if(%t){
               %targetClient.discInDmgTaken += %amount;
               if(%targetClient.md == 1){%client.minePlusDisc++;}
               %targetClient.md = 2;
               schedule(300,0,"mdReset");
               if(%targetClient != %client && rayTest(%targetObject,DiscProjectile.damageRadius+1)){%client.discMA++;}   
            }
         case $DamageType::Grenade:
            if($dtObjExplode.dtNade){
               %client.hGrenadeInDmg += %amount;
               %client.hGrenadeInHits++;
               %client.hGrenadeACC = (%client.hGrenadeInHits / %client.hGrenadeShotsFired) * 100;
               if(%t){
                  %targetClient.hGrenadeInDmgTaken += %amount;
                  if(%targetClient != %client && rayTest(%targetObject,GrenadeThrown.damageRadius+1)){%client.hGrenadeMA++;}
               }
            }
            else{
               %client.grenadeInDmg += %amount;
               %client.grenadeIndirectHits++;
               %client.grenadeACC = (%client.grenadeIndirectHits / %client.grenadeShotsFired) * 100;
               if(%t){
                  %targetClient.grenadeInDmgTaken += %amount;
                  if(%targetClient != %client && rayTest(%targetObject,BasicGrenade.damageRadius+1)){ %client.grenadeMA++;}
               }
            }   
         case $DamageType::Laser:
            %client.laserDmg += %amount;
            %client.laserDirectHits++;
            %client.laserACC = (%client.laserDirectHits / %client.laserShotsFired) * 100;
            if(%t){
               %targetClient.laserDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){%client.laserMA++;}
         }
         case $DamageType::Mortar:
            %client.mortarInDmg += %amount;
            %client.mortarIndirectHits++;
            %client.mortarACC = (%client.mortarIndirectHits / %client.mortarShotsFired) * 100;
            if(%t){
               %targetClient.mortarInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,MortarShot.damageRadius+1)){%client.mortarMA++;}
            }
         case $DamageType::Missile:
            %client.missileInDmg += %amount;
            %client.missileIndirectHits++;
            %client.missileACC = (%client.missileIndirectHits / %client.missileShotsFired) * 100;
            if(%t){
               %targetClient.missileInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,ShoulderMissile.damageRadius+1)){%client.missileMA++;}            
            }
         case $DamageType::ShockLance:
            %client.shockLanceInDmg += %amount;
            %client.shockLanceIndirectHits++;
            %client.shockACC = (%client.shockLanceIndirectHits / %client.shockLanceShotsFired) * 100;
            if(%t){
               %targetClient.shockLanceInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){ %client.shockLanceMA++;}
            }
         case $DamageType::Mine:
            %client.mineInDmg += %amount;
            %client.mineIndirectHits++;
            %client.mineACC = (%client.mineIndirectHits / %client.mineShotsFired) * 100;
            if(%t){
               %targetClient.mineInDmgTaken += %amount;
               if(%targetClient.md == 2){
                  %client.minePlusDisc++;
               }
               %targetClient.md = 1;
               schedule(300,0,"mdReset");
               if(%targetClient != %client && rayTest(%targetObject,MineDeployed.damageRadius+1)){%client.mineMA++;}
            }
         case $DamageType::SatchelCharge:
            %client.SatchelInDmg += %amount;
            %client.SatchelInHits++;
            if(%t)
               %targetClient.SatchelInDmgTaken += %amount;
         case $DamageType::PlasmaTurret:
            %client.PlasmaTurretInDmg +=  %amount;
            %client.PlasmaTurretInHits++;
            if(%t)
               %targetClient.PlasmaTurretInDmgTaken += %amount;
         case $DamageType::MortarTurret:
            %client.MortarTurretInDmg +=  %amount;
            %client.MortarTurretInHits++;
            %targetClient.MortarTurretInDmgTaken += %amount;
         case $DamageType::MissileTurret:
            %client.MissileTurretInDmg +=  %amount;
            %client.MissileTurretInHits++;
            if(%t)
               %targetClient.MissileTurretInDmgTaken += %amount;
         case $DamageType::OutdoorDepTurret:
            %client.OutdoorDepTurretInDmg +=  %amount;
            %client.OutdoorDepTurretInHits++;
            if(%t)
               %targetClient.OutdoorDepTurretInDmgTaken += %amount;
         case $DamageType::BomberBombs:
            %client.BomberBombsInDmg +=  %amount;
            %client.BomberBombsInHits++;
            if(%t)
               %targetClient.BomberBombsInDmgTaken += %amount;
         case $DamageType::TankMortar:
            %client.TankMortarInDmg +=  %amount;
            %client.TankMortarInHits++;
            if(%t)
               %targetClient.TankMortarInDmgTaken += %amount;
         case $DamageType::MPBMissile:
            %client.MPBMissileInDmg +=  %amount;
            %client.MPBMissileInHits++;
            if(%t)
               %targetClient.MPBMissileInDmgTaken += %amount;
         case $DamageType::AATurret:
            %client.AATurretDmg += %amount;
            %client.AATurretDirectHits++;
            if(%t)
               %targetClient.AATurretDmgTaken += %amount;
         case $DamageType::IndoorDepTurret:
            %client.IndoorDepTurretDmg += %amount;
            %client.IndoorDepTurretDirectHits++;
            %targetClient.IndoorDepTurretDmgTaken += %amount;
         case $DamageType::SentryTurret:
            %client.SentryTurretDmg += %amount;
            %client.SentryTurretDirectHits++;
            if(%t)
               %targetClient.SentryTurretDmgTaken += %amount;
         case $DamageType::ShrikeBlaster:
            %client.ShrikeBlasterDmg += %amount;
            %client.ShrikeBlasterDirectHits++;
            if(%t)
               %targetClient.ShrikeBlasterDmgTaken += %amount;
         case $DamageType::BellyTurret:
            %client.BellyTurretDmg += %amount;
            %client.BellyTurretDirectHits++;
            if(%t)
               %targetClient.BellyTurretDmgTaken += %amount;
         case $DamageType::TankChaingun:
            %client.TankChaingunDmg += %amount;
            %client.TankChaingunDirectHits++;
            if(%t)
               %targetClient.TankChaingunDmgTaken += %amount;
      }
   }
}
function clientShotsFired(%data, %sourceObject, %projectile){ // could do a fov check to see if we are trying to aim at a player
   if(isObject(%projectile) && %projectile.sourceObject.getClassName() !$= "Player"){
      %client = %projectile.sourceObject.getControllingClient();
   }
   else{
      %client = %sourceObject.client;
   }
   if(!isObject(%client) || %client.isAiControlled()){ return;}
   
   %speed = vectorLen(%client.player.getVelocity());
   %client.avgTSpeed += %speed; %client.avgSpeedCount++;
   %client.avgSpeed = %client.avgTSpeed/%client.avgSpeedCount;
   if(%client.avgSpeedCount >= 50){%client.avgSpeedCount=%client.avgTSpeed=0;}   
   
   %pos = getWords(%sourceObject.getPosition(),0,1) SPC 0;
   if(%client.lp !$= ""){%client.distT += vectorDist(%client.lp,%pos);}
   %client.lp = %pos;
   
   if(%data.directDamageType !$= ""){%damageType = %data.directDamageType;}
   else{%damageType =  %data.radiusDamageType;}
   
   %client.shotsFired++;
   switch$(%damageType){// list of all damage types to track see damageTypes.cs
      case $DamageType::Bullet:
         %client.cgShotsFired++;
         %client.cgACC = (%client.cgDirectHits / %client.cgShotsFired) * 100;
      case $DamageType::Disc:
         %client.discShotsFired++;
         %client.discACC = (%client.discIndirectHits / %client.discShotsFired) * 100;
      case $DamageType::Grenade:
         %client.grenadeShotsFired++;
         %client.grenadeACC = (%client.grenadeIndirectHits / %client.grenadeShotsFired) * 100;
      case $DamageType::Laser:
         %client.laserShotsFired++;
         %client.laserACC = (%client.laserDirectHits / %client.laserShotsFired) * 100;
      case $DamageType::Mortar:
         %client.mortarShotsFired++;
         %client.mortarACC = (%client.mortarIndirectHits / %client.mortarShotsFired) * 100;
      case $DamageType::Missile:
         %client.missileShotsFired++;
         %client.missileACC = (%client.missileIndirectHits / %client.missileShotsFired) * 100;
      case $DamageType::ShockLance:
         %client.shockLanceShotsFired++;
         %client.shockACC = (%client.shockLanceIndirectHits / %client.shockLanceShotsFired) * 100;
      case $DamageType::Plasma:
         %client.plasmaShotsFired++;
         %client.plasmaACC = (%client.plasmaIndirectHits / %client.plasmaShotsFired) * 100;
      case $DamageType::Blaster:
         %client.blasterShotsFired++;
         %client.blasterACC = (%client.blasterDirectHits / %client.blasterShotsFired) * 100;
      case $DamageType::ELF:
         %client.elfShotsFired++;
      case $DamageType::PlasmaTurret:
         %client.PlasmaTurretFired++;
      case $DamageType::AATurret:
         %client.AATurretFired++;
      case $DamageType::MortarTurret:
         %client.MortarTurretFired++;
      case $DamageType::MissileTurret:
         %client.MissileTurretFired++;
      case $DamageType::IndoorDepTurret:
         %client.IndoorDepTurretFired++;
      case $DamageType::OutdoorDepTurret:
         %client.OutdoorDepTurretFired++;
      case $DamageType::SentryTurret:
         %client.SentryTurretFired++;
      case $DamageType::ShrikeBlaster:
         %client.ShrikeBlasterFired++;
      case $DamageType::BellyTurret:
         %client.BellyTurretFired++;
      case $DamageType::BomberBombs:
         %client.BomberBombsFired++;
      case $DamageType::TankChaingun:
         %client.TankChaingunFired++;
      case $DamageType::TankMortar:
         %client.TankMortarFired++;
      case $DamageType::MPBMissile:
         %client.MPBMissileFired++;
   }
}
////////////////////////////////////////////////////////////////////////////////
//								Menu Stuff									  //
////////////////////////////////////////////////////////////////////////////////
function getGameData(%client, %value,%game,%inc){
   if(%inc != -1 && %inc !$= ""){
      %total = %client.dtStats.gameStats[%value,%inc,%game];
      if(%total !$= ""){
         return mFloatLength(%total,2) + 0;
      }
      else{
         return 0;
      } 
   }
   %c = 0;
   if(%client.dtStats.gameCount[%game] != 0 && %client.dtStats.gameCount[%game] !$= ""){
      for(%i=1; %i <= %client.dtStats.gameCount[%game]; %i++){
         if(!$dtStats::skipZeros){
            %val += %client.dtStats.gameStats[%value,%i,%game];
         }
         else if(%client.dtStats.gameStats[%value,%i,%game] != 0 && %client.dtStats.gameStats[%value,%i,%game] !$= ""){
            %val += %client.dtStats.gameStats[%value,%i,%game];
            %c++;
         }
      }
      if(!$dtStats::skipZeros)
         return mCeil(%val / %client.dtStats.gameCount[%game]);
      else if(%c > 0)
         return mCeil(%val / %c);
      else
         return 0;
   }
   else{
      return 0;
   }
}
function getGameRunWinLossAvg(%client,%game){
   if(%client.dtStats.gameCount[%game] != 0 && %client.dtStats.gameCount[%game] !$= ""){
      for(%i=1; %i <= %client.dtStats.gameCount[%game]; %i++){
         %winCount += %client.dtStats.gameStats["winCount",%i,%game];
         %lossCount += %client.dtStats.gameStats["lossCount",%i,%game];
         %total = %winCount + %lossCount;
      }
      return (%winCount / %total) * 100 SPC (%lossCount / %total) * 100;
   }
}
function getGameTotalAvg(%vClient,%value,%game){
   if(%vClient.dtStats.gameStats[%value,"t",%game] !$= "" && %vClient.dtStats.totalNumGames[%game] > 0)
      %totalAvg = %vClient.dtStats.gameStats[%value,"t",%game] / %vClient.dtStats.totalNumGames[%game];
   else
      %totalAvg = 0;
   
   return mCeil(%totalAvg);
}
function numReduce(%num,%des){
   if(%num !$= ""){
      if(%num > 1000){
         %num = mFloatLength(%num / 1000,%des) + 0 @ "k";
         if(%num > 1000){
            %num = mFloatLength(%num / 1000,%des) + 0 @ "M";
             if(%num > 1000){
               %num =  mFloatLength(%num / 1000,%des) + 0 @ "G";
            }
         }
         return   %num;
      }
      else{
         return mFloatLength(%num,%des)+0;
      }
   } 
   return 0; 
}
function getGameTotal(%vClient,%value,%game){
   %total = %vClient.dtStats.gameStats[%value,"t",%game];
   if(%total !$= ""){
      if(%total > 1000){
         %total = mFloatLength(%total / 1000,1) + 0 @ "k";
         if(%total > 1000){
            %total = mFloatLength(%total / 1000,1) + 0 @ "M";
             if(%total > 1000){
               %total =  mFloatLength(%total / 1000,1) + 0 @ "G";
            }
         }
         return %total;
      }
      else{
         return mFloatLength(%total,1) + 0;
      }
   }
   else{
      return 0;
   }
}
function kdr(%x,%y) 
{
	 if(%x == 0)
       return 0; 
     else if(%y == 0)
        return 100;
	
	 if(%x >= %y)
		return 100 - mFloatLength((%y / %x) * 100,1) + 0;
	 else
		return mFloatLength((%x / %y) * 100,1) - 100 + 0;
}
function menuReset(%client){
   %client.viewMenu = 0;
   %client.viewClient = 0;
   %client.viewStats = 0;
   
}

function statsMenu(%client,%game){
 //if($dtStats::debugEchos){error("statsMenu GUID = "  SPC %client.guid);}
   %menu = %client.viewMenu;
   if(%client.GlArg4 $= "pin"){
      cancel(%client.rtmt); 
   }
   else{
    cancel(%client.rtmt); 
    %client.rtmt = schedule($dtStats::returnToMenuTimer,0,"menuReset",%client);  
   }
   
   
   %vClient = %client.viewClient;
   %tag = 'scoreScreen';
   
   %isTargetSelf = (%client == %vClient);
   %isAdmin = (%client.isAdmin || %client.isSuperAdmin);
   
   messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
   %index = -1;
   if(!isObject(%vClient)){// fail safe
      %menu = "Reset";
   }
   switch$(%menu){
      case "View":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @  getTaggedString(%vClient.name) @ "'s Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tReset\t%1>  Back</a>',%vClient);
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>  Main Options Menu");
         switch$(%game){
            case "CTFGame":
               if($dtStats::Live)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLIVE\t%1>  + CTF Live Stats</a>',%vClient);
			   if($dtStats::Match)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFGame\t%1\t-1>  + CTF Match Stats</a>',%vClient);
               if(%isTargetSelf || %isAdmin) {
				  if($dtStats::Weapon)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tWEAPON\t%1\t-1>  + CTF Weapon Stats </a>',%vClient);
                  if($dtStats::Vehicle)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tVehicles\t%1>  + CTF Vehicle Stats</a>',%vClient);
                  if($dtStats::Turret)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tTurrets\t%1>  + CTF Turrets Stats</a>',%vClient); 
                  if($dtStats::Armor)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tARMOR\t%1>  + CTF Armor Stats</a>',%vClient); 
                  if($dtStats::KD)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tKDA\t%1>  + CTF Kills/Deaths</a>',%vClient);
                  
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1>  + Previous CTF Games</a>',%vClient);
               }
            case "LakRabbitGame":
               if($dtStats::Live)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLIVE\t%1>  + Lak Live Stats</a>',%vClient);
			   if($dtStats::Match)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLakRabbitGame\t%1\t-1>  + Lak Match Stats</a>',%vClient);
               if(%isTargetSelf || %isAdmin) {
				  if($dtStats::Weapon)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tWEAPON\t%1>  + Lak Weapon Stats</a>',%vClient);
                  if($dtStats::Armor)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tARMOR\t%1>  + Arena Armor Stats</a>',%vClient); 
                  
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1>  + Previous Lak Games</a>',%vClient);
               }
            case "DMGame":
               if($dtStats::Live)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLIVE\t%1>  + Deathmatch Live Stats</a>',%vClient); 
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tDMGame\t%1\t-1>  + Deathmatch Stats</a>',%vClient);
               if(%isTargetSelf || %isAdmin) {
				  if($dtStats::Weapon)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tWEAPON\t%1\t-1>  + Deathmatch Weapon Stats</a>',%vClient);
                  if($dtStats::Armor)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tARMOR\t%1>  + Deathmatch Armor Stats</a>',%vClient); 
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                   messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1>  + Previous Deathmatch Games</a>',%vClient);
               }            
            case "DuelGame":
               if($dtStats::Live)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLIVE\t%1>  + Duel Mod Live Stats</a>',%vClient);
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tDuelGame\t%1\t-1>  + Duel Mod Stats</a>',%vClient);
               if(%isTargetSelf || %isAdmin) {
				  if($dtStats::Weapon)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tWEAPON\t%1\t-1>  + Duel Mod Weapon Stats</a>',%vClient);
                  if($dtStats::Armor)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tARMOR\t%1>  + Duel Mod Armor Stats</a>',%vClient);
                  
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1>  + Previous Duel Mod Games</a>',%vClient);
               }               
            case "SCtFGame":// LCTF
               if($dtStats::Live)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLIVE\t%1>  + LCTF Live Stats</a>',%vClient);
               //messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tSCtFGame\t%1\t-1>  + LCTF Match Stats</a>',%vClient);
               if(%isTargetSelf || %isAdmin) {
				  if($dtStats::Weapon)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tWEAPON\t%1\t-1>  + LCTF Weapon Stats</a>',%vClient);
                  if($dtStats::Vehicle)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tVehicles\t%1>  + CTF Vehicle Stats</a>',%vClient);
                  if($dtStats::Turret)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tTurrets\t%1>  + CTF Turrets Stats</a>',%vClient); 
                  if($dtStats::KD)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tKDA\t%1>  + LCTF Kills/Deaths</a>',%vClient);
                  
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1>  + Previous LCTF Games</a>',%vClient);
               }
            case "ArenaGame":
               if($dtStats::Live)
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLIVE\t%1>  + Arena Live Stats</a>',%vClient);
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tArenaGame\t%1\t-1>  + Arena Stats</a>',%vClient);
               if(%isTargetSelf || %isAdmin) {
                  if($dtStats::Weapon)
					 messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tWEAPON\t%1\t-1>  + Arena Weapon Stats</a>',%vClient);
                  if($dtStats::Armor)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tARMOR\t%1>  + Arena Armor Stats</a>',%vClient); 
                 
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1>  + Previous Arena Games</a>',%vClient);
               }
         }
         %m = 14 - %index;
         for(%v = 0; %v < %m; %v++){messageClient( %client, 'SetLineHud', "", %tag, %index++, "");}
         
         if(%vClient.dtStats.gameCount[%game] == 0)
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Stats update at the end of every map.');
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Based on the last" SPC %3 SPC "games.");
         //%line = '<just:center>Games Played = %3 Running Average = %1/%2 Overwrite Counter = %4';
         if(%vClient.dtStats.gameCount[%game] > 1) {
            %line = '<just:center>Based on the last %1 games played.';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient.dtStats.totalNumGames[%game]);
         }
         else if(%vClient.dtStats.gameCount[%game] == 1) {
            %line = '<just:center>Based on the last game played.';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         }
           
      case "LakRabbitGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Suicides<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Suicides<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"suicides",%game,%inc),getGameTotal(%vClient,"suicides",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mas",%game,%inc),getGameTotal(%vClient,"mas",%game),getGameTotalAvg(%vClient,"mas",%game),%vClient.mas);
         %line1 = '<color:0befe7> Flag Grabs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagGrabs",%game,%inc),getGameTotal(%vClient,"flagGrabs",%game),getGameTotalAvg(%vClient,"flagGrabs",%game),%vClient.flagGrabs);
         %line1 = '<color:0befe7> Midair Flag Grabs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midair Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"MidairflagGrabs",%inc,%game,%inc),getGameTotal(%vClient,"MidairflagGrabs",%game),getGameTotalAvg(%vClient,"MidairflagGrabs",%game),%vClient.MidairflagGrabs);
         %line1 = '<color:0befe7> Midair Flag Grab Points<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midair Flag Grab Points<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"MidairflagGrabPoints",%inc,%game,%inc),getGameTotal(%vClient,"MidairflagGrabPoints",%game),getGameTotalAvg(%vClient,"MidairflagGrabPoints",%game),%vClient.MidairflagGrabPoints);
         %line1 = '<color:0befe7> Flag Time Minutes<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Time Minutes<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagTimeMS",%game,%inc),getGameTotal(%vClient,"flagTimeMS",%game),getGameTotalAvg(%vClient,"flagTimeMS",%game),%vClient.flagTimeMS);
         %line1 = '<color:0befe7> Bonus Points<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Bonus Points<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"morepoints",%game,%inc),getGameTotal(%vClient,"morepoints",%game),getGameTotalAvg(%vClient,"morepoints",%game),%vClient.morepoints);
         %line1 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minedisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Total Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Total Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"totalDistance",%game,%inc),getGameTotal(%vClient,"totalDistance",%game),getGameTotalAvg(%vClient,"totalDistance",%game),%vClient.totalDistance);
         %line1 = '<color:0befe7> Total Shock Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Total Shock Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"totalShockHits",%game,%inc),getGameTotal(%vClient,"totalShockHits",%game),getGameTotalAvg(%vClient,"totalShockHits",%game),%vClient.totalShockHits);
         %line1 = '<color:0befe7> Total Shocks<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Total Shocks<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"totalShocks",%game,%inc),getGameTotal(%vClient,"totalShocks",%game),getGameTotalAvg(%vClient,"totalShocks",%game),%vClient.totalShocks);
      case "DMGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "",%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Suicides<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Suicides<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"suicides",%game,%inc),getGameTotal(%vClient,"suicides",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         

      case "ArenaGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Suicides<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Suicides<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"suicides",%game,%inc),getGameTotal(%vClient,"suicides",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         %line1 = '<color:0befe7> Team Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Team Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"teamKills",%game,%inc),getGameTotal(%vClient,"teamKills",%game),getGameTotalAvg(%vClient,"teamKills",%game),%vClient.teamKills);
         %line1 = '<color:0befe7> Snipe Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Snipe Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"snipeKills",%game,%inc),getGameTotal(%vClient,"snipeKills",%game),getGameTotalAvg(%vClient,"roundsWon",%game),%vClient.roundsWon);
         %line1 = '<color:0befe7> Rounds Won<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Rounds Won<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"roundsWon",%game,%inc),getGameTotal(%vClient,"roundsWon",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         %line1 = '<color:0befe7> Rounds Lost<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Rounds Lost<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"roundsLost",%game,%inc),getGameTotal(%vClient,"roundsLost",%game),getGameTotalAvg(%vClient,"roundsLost",%game),%vClient.roundsLost);
         %line1 = '<color:0befe7> Assists<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Assists<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"assists",%game,%inc),getGameTotal(%vClient,"assists",%game),getGameTotalAvg(%vClient,"assists",%game),%vClient.assists);
         %line1 = '<color:0befe7> Round Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Round Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"roundKills",%game,%inc),getGameTotal(%vClient,"roundKills",%game),getGameTotalAvg(%vClient,"roundKills",%game),%vClient.roundKills);
         %line1 = '<color:0befe7> Hat Tricks<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hat Tricks<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hatTricks",%game,%inc),getGameTotal(%vClient,"hatTricks",%game),getGameTotalAvg(%vClient,"hatTricks",%game),%vClient.hatTricks);
         
      case "DuelGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
                 
      case "CTFGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Win %6<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Win %6<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         
         %wlPCT =  getGameRunWinLossAvg(%client,%game);
         %runAvg = mFloor(getWord(%wlPCT,0)) @ "%";
         
         %winTotal = getGameTotal(%vClient,"winCount",%game);
         %lossTotal = getGameTotal(%vClient,"lossCount",%game);
         %total  = %winTotal SPC "W /" SPC %lossTotal SPC "L";
         
         %totalWinLoss = %winTotal +  %lossTotal;
         %totalAvg = mFloor((%winTotal / %totalWinLoss)* 100) @ "%";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%runAvg,%total,%totalAvg,%vClient.winCount,"%");
         
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Mid-Air<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mid-Air<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreMidAir",%game,%inc),getGameTotal(%vClient,"scoreMidAir",%game),getGameTotalAvg(%vClient,"scoreMidAir",%game),%vClient.scoreMidAir);
         %line1 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minePlusDisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Flag Caps<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Caps<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagCaps",%game,%inc),getGameTotal(%vClient,"flagCaps",%game),getGameTotalAvg(%vClient,"flagCaps",%game),%vClient.flagCaps);
         %line1 = '<color:0befe7> Flag Grabs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagGrabs",%game,%inc),getGameTotal(%vClient,"flagGrabs",%game),getGameTotalAvg(%vClient,"flagGrabs",%game),%vClient.flagGrabs);
         %line1 = '<color:0befe7> Carrier Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Carrier Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"carrierKills",%game,%inc),getGameTotal(%vClient,"carrierKills",%game),getGameTotalAvg(%vClient,"carrierKills",%game),%vClient.carrierKills);
         %line1 = '<color:0befe7> Flag Returns<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Returns<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagReturns",%game,%inc),getGameTotal(%vClient,"flagReturns",%game),getGameTotalAvg(%vClient,"flagReturns",%game),%vClient.flagReturns);
         %line1 = '<color:0befe7> Flag Defends<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Defends<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagDefends",%game,%inc),getGameTotal(%vClient,"flagDefends",%game),getGameTotalAvg(%vClient,"flagDefends",%game),%vClient.flagDefends);
         %line1 = '<color:0befe7> Offense Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Offense Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"offenseScore",%game,%inc),getGameTotal(%vClient,"offenseScore",%game),getGameTotalAvg(%vClient,"offenseScore",%game),%vClient.offenseScore);
         %line1 = '<color:0befe7> Defense Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Defense Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"defenseScore",%game,%inc),getGameTotal(%vClient,"defenseScore",%game),getGameTotalAvg(%vClient,"defenseScore",%game),%vClient.defenseScore);
         %line1 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Backshots<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Backshots<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreRearshot",%game,%inc),getGameTotal(%vClient,"scoreRearshot",%game),getGameTotalAvg(%vClient,"scoreRearshot",%game),%vClient.scoreRearshot);
         %line1 = '<color:0befe7> Headshots<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Headshots<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreHeadshot",%game,%inc),getGameTotal(%vClient,"scoreHeadshot",%game),getGameTotalAvg(%vClient,"scoreHeadshot",%game),%vClient.scoreHeadshot);
      
      case "SCtFGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Win %6<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Win %6<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         
         %wlPCT =  getGameRunWinLossAvg(%client,%game);
         %runAvg = mFloor(getWord(%wlPCT,0)) @ "%";
         
         %winTotal = getGameTotal(%vClient,"winCount",%game);
         %lossTotal = getGameTotal(%vClient,"lossCount",%game);
         %total  = %winTotal SPC "W /" SPC %lossTotal SPC "L";
         
         %totalWinLoss = %winTotal +  %lossTotal;
         %totalAvg = mFloor((%winTotal / %totalWinLoss)* 100) @ "%";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%runAvg,%total,%totalAvg,%vClient.winCount,"%");
         
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Mid-Air<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mid-Air<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreMidAir",%game,%inc),getGameTotal(%vClient,"scoreMidAir",%game),getGameTotalAvg(%vClient,"scoreMidAir",%game),%vClient.scoreMidAir);
         %line1 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minePlusDisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Flag Caps<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Caps<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagCaps",%game,%inc),getGameTotal(%vClient,"flagCaps",%game),getGameTotalAvg(%vClient,"flagCaps",%game),%vClient.flagCaps);
         %line1 = '<color:0befe7> Flag Grabs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagGrabs",%game,%inc),getGameTotal(%vClient,"flagGrabs",%game),getGameTotalAvg(%vClient,"flagGrabs",%game),%vClient.flagGrabs);
         %line1 = '<color:0befe7> Carrier Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Carrier Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"carrierKills",%game,%inc),getGameTotal(%vClient,"carrierKills",%game),getGameTotalAvg(%vClient,"carrierKills",%game),%vClient.carrierKills);
         %line1 = '<color:0befe7> Flag Returns<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Returns<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagReturns",%game,%inc),getGameTotal(%vClient,"flagReturns",%game),getGameTotalAvg(%vClient,"flagReturns",%game),%vClient.flagReturns);
         %line1 = '<color:0befe7> Flag Defends<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Defends<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagDefends",%game,%inc),getGameTotal(%vClient,"flagDefends",%game),getGameTotalAvg(%vClient,"flagDefends",%game),%vClient.flagDefends);
         %line1 = '<color:0befe7> Offense Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Offense Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"offenseScore",%game,%inc),getGameTotal(%vClient,"offenseScore",%game),getGameTotalAvg(%vClient,"offenseScore",%game),%vClient.offenseScore);
         %line1 = '<color:0befe7> Defense<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Defense<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"defenseScore",%game,%inc),getGameTotal(%vClient,"defenseScore",%game),getGameTotalAvg(%vClient,"defenseScore",%game),%vClient.defenseScore);
         %line1 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.wildCrash);
         %line1 = '<color:0befe7> Backshots<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Backshots<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreRearshot",%game,%inc),getGameTotal(%vClient,"scoreRearshot",%game),getGameTotalAvg(%vClient,"scoreRearshot",%game),%vClient.scoreRearshot);
         %line1 = '<color:0befe7> Headshots<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Headshots<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreHeadshot",%game,%inc),getGameTotal(%vClient,"scoreHeadshot",%game),getGameTotalAvg(%vClient,"scoreHeadshot",%game),%vClient.scoreHeadshot);
      case "HISTORY":// Past Games
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s " @ $MissionTypeDisplayName @ " History");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Game history is set to" SPC $dtStats::MaxNumOfGames SPC "games.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>The oldest game will be overwritten.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         if(%vClient.dtStats.gameCount[%game] >= $dtStats::MaxNumOfGames){
            %in = %vClient.dtStats.statsOverWrite[%game] + 1;
            if(%in > $dtStats::MaxNumOfGames){
               %in = 1;
            }
            for(%z = %in - 1; %z > 0; %z--){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%z,%game];
               %map = %vClient.dtStats.gameStats["map",%z,%game];
               messageClient( %client, 'SetLineHud', "", %tag, %index++,' <color:0befe7>%4 - %2<lmargin:250><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:300><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>',%vClient,%timeDate,%z,%map,%game);
            }
            for(%b = %vClient.dtStats.gameCount[%game]; %b >= %in; %b--){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%b,%game];
               %map = %vClient.dtStats.gameStats["map",%b,%game];
               if(%b == %in){
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, ' <color:0befe7>%4 - %2<lmargin:250><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:300><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a><color:02d404> - Overwritten',%vClient,%timeDate,%b,%map,%game);
               }
               else{
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,' <color:0befe7>%4 - %2<lmargin:250><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:300><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>',%vClient,%timeDate,%b,%map,%game);
               }
            }
            
         }
         else{
            for(%z = %vClient.dtStats.gameCount[%game]; %z >= 1; %z--){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%z,%game];
               %map = %vClient.dtStats.gameStats["map",%z,%game];
               messageClient( %client, 'SetLineHud', "", %tag, %index++,' <color:0befe7>%4 - %2<lmargin:250><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:300><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>',%vClient,%timeDate,%z,%map,%game);
            }
         }
      case "KDA":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Kills/Deaths");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         %a1 = getGameTotal(%vClient,"cgKills",%game); %b2 = getGameTotal(%vClient,"cgDeaths",%game); %c3 = getGameTotal(%vClient,"discKills",%game);
         %d4 = getGameTotal(%vClient,"discDeaths",%game); %e5 = getGameTotal(%vClient,"grenadeKills",%game); %f6 = getGameTotal(%vClient,"grenadeDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Chaingun: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Spinfusor: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Grenade Launcher: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"laserKills",%game); %b2 = getGameTotal(%vClient,"laserDeaths",%game); %c3 = getGameTotal(%vClient,"mortarKills",%game);
         %d4 = getGameTotal(%vClient,"mortarDeaths",%game); %e5 = getGameTotal(%vClient,"shockLanceKills",%game); %f6 = getGameTotal(%vClient,"shockLanceDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Laser Rifle: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Fusion Mortar: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Shocklance: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"plasmaKills",%game); %b2 = getGameTotal(%vClient,"plasmaDeaths",%game); %c3 = getGameTotal(%vClient,"blasterKills",%game);
         %d4 = getGameTotal(%vClient,"blasterDeaths",%game); %e5 = getGameTotal(%vClient,"elfKills",%game); %f6 = getGameTotal(%vClient,"elfDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Plasma Rifle: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Blaster: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>ELF Projector: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, " -----------------------------------------------------------------------------------------------------------------");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         
         %a1 = getGameTotal(%vClient,"mineKills",%game); %b2 = getGameTotal(%vClient,"mineDeaths",%game); %c3 = getGameTotal(%vClient,"explosionKills",%game);
         %d4 = getGameTotal(%vClient,"explosionDeaths",%game); %e5 = getGameTotal(%vClient,"impactKills",%game); %f6 = getGameTotal(%vClient,"impactDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Mines: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Explosion: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Impact: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"groundKills"); %b2 = getGameTotal(%vClient,"groundDeaths"); %c3 = getGameTotal(%vClient,"turretKills");
         %d4 = getGameTotal(%vClient,"turretDeaths",%game); %e5 = getGameTotal(%vClient,"plasmaTurretKills",%game); %f6 = getGameTotal(%vClient,"plasmaTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Ground: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Turret: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Plasma Turret: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"aaTurretKills"); %b2 = getGameTotal(%vClient,"aaTurretDeaths"); %c3 = getGameTotal(%vClient,"elfTurretKills");
         %d4 = getGameTotal(%vClient,"elfTurretDeaths",%game); %e5 = getGameTotal(%vClient,"mortarTurretKills",%game); %f6 = getGameTotal(%vClient,"mortarTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  AA Turret: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>ELF Turret: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Mortar Turret: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"missileTurretKills",%game); %b2 = getGameTotal(%vClient,"missileTurretDeaths",%game); %c3 = getGameTotal(%vClient,"indoorDepTurretKills",%game);
         %d4 = getGameTotal(%vClient,"indoorDepTurretDeaths",%game); %e5 = getGameTotal(%vClient,"outdoorDepTurretKills",%game); %f6 = getGameTotal(%vClient,"outdoorDepTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Missile Turret: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Spider Camp Turret: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Land Spike Turret: <color:33CCCC>%5:%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         %a1 = getGameTotal(%vClient,"sentryTurretKills",%game); %b2 = getGameTotal(%vClient,"sentryTurretDeaths",%game); %c3 = getGameTotal(%vClient,"outOfBoundKills",%game);
         %d4 = getGameTotal(%vClient,"outOfBoundDeaths",%game); %e5 = getGameTotal(%vClient,"lavaKills",%game); %f6 = getGameTotal(%vClient,"lavaDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Sentry Turret: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Out Of Bounds: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Lava: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"shrikeBlasterKills",%game); %b2 = getGameTotal(%vClient,"shrikeBlasterDeaths",%game); %c3 = getGameTotal(%vClient,"bellyTurretKills",%game);
         %d4 = getGameTotal(%vClient,"bellyTurretDeaths",%game); %e5 = getGameTotal(%vClient,"bomberBombsKills",%game); %f6 = getGameTotal(%vClient,"bomberBombsDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Shrike Blaster: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Bomber Turret: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Bomber Bombs: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"tankChaingunKills",%game); %b2 = getGameTotal(%vClient,"tankChaingunDeaths",%game); %c3 = getGameTotal(%vClient,"tankMortarKills",%game);
         %d4 = getGameTotal(%vClient,"tankMortarDeaths",%game); %e5 = getGameTotal(%vClient,"nexusCampingKills",%game); %f6 = getGameTotal(%vClient,"nexusCampingDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Tank Chaingun: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Tank Mortar: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Nexus Camping: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         %a1 = getGameTotal(%vClient,"satchelChargeKills",%game); %b2 = getGameTotal(%vClient,"satchelChargeDeaths",%game); %c3 = getGameTotal(%vClient,"lightningKills",%game);
         %d4 = getGameTotal(%vClient,"lightningDeaths",%game); %e5 = getGameTotal(%vClient,"vehicleSpawnKills",%game); %f6 = getGameTotal(%vClient,"vehicleSpawnDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Satchel Charge: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Lightning: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Vehicle Spawn: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"forceFieldPowerUpKills",%game); %b2 = getGameTotal(%vClient,"forceFieldPowerUpDeaths",%game); %c3 = getGameTotal(%vClient,"crashKills",%game);
         %d4 = getGameTotal(%vClient,"crashDeaths",%game); %e5 = getGameTotal(%vClient,"waterKills",%game); %f6 = getGameTotal(%vClient,"waterDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Forcefield Power: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Crash: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Water: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"nexusCampingKills",%game); %b2 = getGameTotal(%vClient,"nexusCampingDeaths",%game); 
         //%line = '<font:univers condensed:18><color:0befe7>  Nexus Camping: <color:33CCCC>%1:%2';
        // messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2);
         
      case "WEAPON":// Weapons
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         }
         
         %line = '<color:00dcd4><a:gamelink\tStats\tBlaster\t%1\t%2>  + Blaster Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         %line = '<color:00dcd4><a:gamelink\tStats\tPlasmaRifle\t%1\t%2>  + Plasma Rifle Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         %line = '<color:00dcd4><a:gamelink\tStats\tChaingun\t%1\t%2>  + Chaingun Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient,%inc);
         %line = '<color:00dcd4><a:gamelink\tStats\tSpinfusor\t%1\t%2>  + Spinfusor Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         %line = '<color:00dcd4><a:gamelink\tStats\tGrenadeLauncher\t%1\t%2>  + Grenade Launcher Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         
         if(%game !$= "LakRabbitGame"){
            %line = '<color:00dcd4><a:gamelink\tStats\tLaserRifle\t%1\t%2>  + Laser Rifle Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
            %line = '<color:00dcd4><a:gamelink\tStats\tELF\t%1\t%2>  + ELF Projector Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         }
         
         %line = '<color:00dcd4><a:gamelink\tStats\tFusionMortar\t%1\t%2>  + Fusion Mortar Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         if(%game !$= "LakRabbitGame" && %game !$= "ArenaGame" ){
            %line = '<color:00dcd4><a:gamelink\tStats\tMissileLauncher\t%1\t%2>  + Missile Launcher Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         }
         
         %line = '<color:00dcd4><a:gamelink\tStats\tShocklance\t%1\t%2>  + Shocklance Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         %line = '<color:00dcd4><a:gamelink\tStats\tMine\t%1\t%2>  + Mine Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         %line = '<color:00dcd4><a:gamelink\tStats\tHandGrenade\t%1\t%2>  + Hand Grenade Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         
         if(%game !$= "LakRabbitGame" && %game !$= "ArenaGame" ){
            %line = '<color:00dcd4><a:gamelink\tStats\tSatchelCharge\t%1\t%2>  + Satchel Charge Stats</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%inc);
         }
      case "ARMOR":
         //%inc = %client.GlArg4; // leave this here in case we want history later
         %inc = -1;
         //if(%inc != -1){//History
            //messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            //messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            //%header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         //}
         //else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Armor Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:250>Live<lmargin:320>Run Avg<lmargin:390>Totals<lmargin:460>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         //}
         %line1 = '<color:0befe7> Scout Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorL",%game,%inc),getGameTotal(%vClient,"armorL",%game),getGameTotalAvg(%vClient,"armorL",%game),%vClient.armorL);
         %line1 = '<color:0befe7> Scout Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLD",%game,%inc),getGameTotal(%vClient,"armorLD",%game),getGameTotalAvg(%vClient,"armorLD",%game),%vClient.armorLD);
         %line1 = '<color:0befe7> Scout Vs Scout Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Vs Scout Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLL",%game,%inc),getGameTotal(%vClient,"armorLL",%game),getGameTotalAvg(%vClient,"armorLL",%game),%vClient.armorLL);
         %line1 = '<color:0befe7> Scout Vs Scout Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Vs Scout Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLLD",%game,%inc),getGameTotal(%vClient,"armorLLD",%game),getGameTotalAvg(%vClient,"armorLLD",%game),%vClient.armorLLD);         
         %line1 = '<color:0befe7> Scout Vs Assault Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Vs Assault Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLM",%game,%inc),getGameTotal(%vClient,"armorLM",%game),getGameTotalAvg(%vClient,"armorLM",%game),%vClient.armorLM);
         %line1 = '<color:0befe7> Scout Vs Assault Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Vs Assault Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLMD",%game,%inc),getGameTotal(%vClient,"armorLMD",%game),getGameTotalAvg(%vClient,"armorLMD",%game),%vClient.armorLMD);  
         %line1 = '<color:0befe7> Scout Vs Juggernaut Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Vs Juggernaut Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLH",%game,%inc),getGameTotal(%vClient,"armorLH",%game),getGameTotalAvg(%vClient,"armorLH",%game),%vClient.armorLH);
         %line1 = '<color:0befe7> Scout Vs Juggernaut Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Scout Vs Juggernaut Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorLHD",%game,%inc),getGameTotal(%vClient,"armorLHD",%game),getGameTotalAvg(%vClient,"armorLHD",%game),%vClient.armorLHD); 
         
         %line1 = '<color:0befe7> Assault Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorM",%game,%inc),getGameTotal(%vClient,"armorM",%game),getGameTotalAvg(%vClient,"armorM",%game),%vClient.armorM);
         %line1 = '<color:0befe7> Assault Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorMD",%game,%inc),getGameTotal(%vClient,"armorMD",%game),getGameTotalAvg(%vClient,"armorMD",%game),%vClient.armorMD);
         %line1 = '<color:0befe7> Assault Vs Scout Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Vs Scout Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorML",%game,%inc),getGameTotal(%vClient,"armorML",%game),getGameTotalAvg(%vClient,"armorML",%game),%vClient.armorML);
         %line1 = '<color:0befe7> Assault Vs Scout Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Vs Scout Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorMLD",%game,%inc),getGameTotal(%vClient,"armorMLD",%game),getGameTotalAvg(%vClient,"armorMLD",%game),%vClient.armorMLD);         
         %line1 = '<color:0befe7> Assault Vs Assault Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Vs Assault Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorMM",%game,%inc),getGameTotal(%vClient,"armorMM",%game),getGameTotalAvg(%vClient,"armorMM",%game),%vClient.armorMM);
         %line1 = '<color:0befe7> Assault Vs Assault Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Vs Assault Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorMMD",%game,%inc),getGameTotal(%vClient,"armorMMD",%game),getGameTotalAvg(%vClient,"armorMMD",%game),%vClient.armorMMD);  
         %line1 = '<color:0befe7> Assault Vs Juggernaut Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Vs Juggernaut Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorMH",%game,%inc),getGameTotal(%vClient,"armorMH",%game),getGameTotalAvg(%vClient,"armorMH",%game),%vClient.armorMH);
         %line1 = '<color:0befe7> Assault Vs Juggernaut Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Assault Vs Juggernaut Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorMHD",%game,%inc),getGameTotal(%vClient,"armorMHD",%game),getGameTotalAvg(%vClient,"armorMHD",%game),%vClient.armorMHD); 
         
         %line1 = '<color:0befe7> Juggernaut Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorH",%game,%inc),getGameTotal(%vClient,"armorH",%game),getGameTotalAvg(%vClient,"armorH",%game),%vClient.armorH);
         %line1 = '<color:0befe7> Juggernaut Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHD",%game,%inc),getGameTotal(%vClient,"armorHD",%game),getGameTotalAvg(%vClient,"armorHD",%game),%vClient.armorHD);
         %line1 = '<color:0befe7> Juggernaut Vs Scout Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Vs Scout Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHL",%game,%inc),getGameTotal(%vClient,"armorHL",%game),getGameTotalAvg(%vClient,"armorHL",%game),%vClient.armorHL);
         %line1 = '<color:0befe7> Juggernaut Vs Scout Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Vs Scout Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHLD",%game,%inc),getGameTotal(%vClient,"armorHLD",%game),getGameTotalAvg(%vClient,"armorHLD",%game),%vClient.armorHLD);         
         %line1 = '<color:0befe7> Juggernaut Vs Assault Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Vs Assault Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHM",%game,%inc),getGameTotal(%vClient,"armorHM",%game),getGameTotalAvg(%vClient,"armorHM",%game),%vClient.armorHM);
         %line1 = '<color:0befe7> Juggernaut Vs Assault Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Vs Assault Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHMD",%game,%inc),getGameTotal(%vClient,"armorHMD",%game),getGameTotalAvg(%vClient,"armorHMD",%game),%vClient.armorHMD);  
         %line1 = '<color:0befe7> Juggernaut Vs Juggernaut Kills<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Vs Juggernaut Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHH",%game,%inc),getGameTotal(%vClient,"armorHH",%game),getGameTotalAvg(%vClient,"armorHH",%game),%vClient.armorHH);
         %line1 = '<color:0befe7> Juggernaut Vs Juggernaut Deaths<color:00dcd4><lmargin:250>%5<lmargin:320>%2<lmargin:390>%3<lmargin:460>%4';
         %line2 = '<color:0befe7> Juggernaut Vs Juggernaut Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"armorHHD",%game,%inc),getGameTotal(%vClient,"armorHHD",%game),getGameTotalAvg(%vClient,"armorHHD",%game),%vClient.armorHHD);
      case "LIVE":
         %inc = %client.GlArg4;
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Live Stats");
         if(%inc $= "pin"){
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tStats\tLIVE\t%1\t-1>Unpin Screen</a> - Games Played: %2',%vClient,%vClient.dtStats.totalGames[%game]);
         }
         else{
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tStats\tLIVE\t%1\tpin>Pin Screen</a> - Games Played: %2',%vClient,%vClient.dtStats.totalGames[%game]);
         }
         //%i1=%i2=%i3=%i4=%i5=%i6=%i7=%i8=%i9=0;  
         //%line = '<color:0befe7>  PastGames<lmargin:100>%1<lmargin:150>%2<lmargin:200>%3<lmargin:250>%4<lmargin:300>%5<lmargin:350>%6<lmargin:400>%7<lmargin:450>%8<lmargin:500>%9';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7,%i8,%i9);  
         
         %i1 = "Score:" SPC %vClient.score; 
         %i2 = "Kills:" SPC %vClient.kills;
         %i3 = "Deaths:" SPC %vClient.deaths; 
         %i4 = "Assists:" SPC %vClient.assist;
         %line = '<color:0befe7>  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);         
         
         %i1 = "KDR:" SPC kdr(%vClient.kills,%vClient.deaths) @ "%"; 
         %i2 = "KillStreak:" SPC %vClient.killStreak;
         %i3 = "Combos:" SPC %vClient.comboCount;
         %i4 = %vClient.plasmaMA + %vClient.discMA + %vClient.mineMA + %vClient.grenadeMA + %vClient.hGrenadeMA + %vClient.mortarMA + %vClient.shockLanceMA + %vClient.laserMA +
         %vClient.laserHeadShot + %vClient.shockRearShot + %vClient.comboPT + %vClient.assist +
         (%vClient.plasmaMax/500) + (%vClient.discMax/500) + (%vClient.mineMax/200) + (%vClient.grenadeMax/300) + (%vClient.hGrenadeMax/200) + (%vClient.mortarMax/200)+
         (%vClient.plasmaT/100) + (%vClient.discT/100) + (%vClient.mineT/100) + (%vClient.grenadeT/100) + (%vClient.hGrenadeT/100) + (%vClient.mortarT/100) + (%vClient.shockT/50) + (%vClient.laserT/100);
         %i4 = "Shot Rating:" SPC mFloatLength(%i4/26,2) + 0; //
         %line = '<color:0befe7>  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';         
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);


         %dmg =  %vClient.blasterDmg + %vClient.plasmaInDmg + %vClient.grenadeInDmg + %vClient.hGrenadeInDmg + %vClient.cgDmg + 
         %vClient.discInDmg + %vClient.laserDmg + %vClient.mortarInDmg + %vClient.missileInDmg + %vClient.shockLanceInDmg + %vClient.mineInDmg;
         %i1 = "Damage:" SPC numReduce(%dmg,1);
         %i2 = "Speed:" SPC  mFloatLength(%vClient.avgSpeed,1) + 0;
         %i3 = "Shots Fired:" SPC numReduce(%vClient.shotsFired,2); //"RelSpeed:" SPC mFloatLength(%vClient.maxRV,1)+0;
         %i4 = "Dist Moved:" SPC numReduce(%vClient.distT,1); // %vClient.dtStats.totalGames[%game];
         %line = '<color:0befe7>  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);
         
         %i1 = "Lt Kills:" SPC %vClient.armorL;
         %i2 = "Med Kills:" SPC %vClient.armorM;
         %i3 = "Hvy Kills:"SPC %vClient.armorH;
         %i4 = "Survival:" SPC msToMinSec(%vClient.timeTL);   
         %line = '<color:0befe7>  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);

         messageClient( %client, 'SetLineHud', "", %tag, %index++, ""); 

         %header = '<color:0befe7>  Weapon<lmargin:140>K:D<lmargin:212>MidAirs<lmargin:284>Accuracy<lmargin:356>DmgAvg<lmargin:428>Speed<lmargin:500>MaxDis';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %i1=%i2=%i3=%i4=%i5=%i6=%i7=0;    
         %i1 = %vClient.blasterKills @ ":" @ %vClient.blasterDeaths;
         %i2 = %vClient.blasterMA;
         %i3 = mFloatLength(%vClient.blasterACC,1) + 0 @ "%";   
         %i4 = mFloatLength((%vClient.blasterDmg/%vClient.blasterShotsFired),2) + 0;
         %i5 = mFloatLength(%vClient.blasterT,1)+0;           
         %i6 = mCeil(%vClient.blasterMax) @ "m";
         %line = '<color:0befe7>  Blaster<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.plasmaKills @ ":" @ %vClient.plasmaDeaths;
         %i2 = %vClient.plasmaMA;   
         %i3 = mFloatLength(%vClient.plasmaACC,1) + 0 @ "%";
         %i4 = mFloatLength((%vClient.plasmaInDmg/%vClient.plasmaShotsFired),2) + 0; 
         %i5 = mFloatLength(%vClient.plasmaT,1)+0;          
         %i6 = mCeil(%vClient.plasmaMax) @ "m";
         %line = '<color:0befe7>  Plasma Rifle<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.cgKills @ ":" @ %vClient.cgDeaths;
         %i2 = %vClient.cgMA;
         %i3 = mFloatLength(%vClient.cgACC,1) + 0 @ "%";
         %i4 = mFloatLength((%vClient.cgDmg/%vClient.cgShotsFired),2) + 0;   
         %i5 = mFloatLength(%vClient.cgT,1)+0;           
         %i6 = mCeil(%vClient.cgMax) @ "m";     
         %line = '<color:0befe7>  Chaingun<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.discKills @ ":" @ %vClient.discDeaths;
         %i2 = %vClient.discMA; 
         %i3 = mFloatLength(%vClient.discACC,1) + 0 @ "%";
         %i4 = mFloatLength((%vClient.discInDmg/%vClient.discShotsFired),2) + 0;  
         %i5 = mFloatLength(%vClient.discT,1)+0;           
         %i6 = mCeil(%vClient.discMax) @ "m";
         %line = '<color:0befe7>  Spinfusor<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.grenadeKills @ ":" @ %vClient.grenadeDeaths;
         %i2 = %vClient.grenadeMA; 
         %i3 = mFloatLength(%vClient.grenadeACC,1) + 0 @ "%";
         %i4 = mFloatLength((%vClient.grenadeInDmg/%vClient.grenadeShotsFired),2) + 0;
         %i5 = mFloatLength(%vClient.grenadeT,1)+0;           
         %i6 = mCeil(%vClient.grenadeMax) @ "m";         
         %line = '<color:0befe7>  Grenade Launcher<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.laserKills @ ":" @ %vClient.laserDeaths;
         %i2 = %vClient.laserMA;
         %i3 = mFloatLength(%vClient.laserACC,1) + 0 @ "%";   
         %i4 = mFloatLength((%vClient.laserDmg/%vClient.laserShotsFired),2) + 0;
         %i5 = mFloatLength(%vClient.laserT,1)+0;           
         %i6 = mCeil(%vClient.laserMax) @ "m";         
         %line = '<color:0befe7>  Laser Rifle<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.mortarKills @ ":" @ %vClient.mortarDeaths;
         %i2 = %vClient.mortarMA;  
         %i3 = mFloatLength(%vClient.mortarACC,1) + 0 @ "%";
         %i4 = mFloatLength((%vClient.mortarInDmg/%vClient.mortarShotsFired),2) + 0;
         %i5 = mFloatLength(%vClient.mortarT,1)+0;           
         %i6 = mCeil(%vClient.mortarMax) @ "m";         
         %line = '<color:0befe7>  Fusion Mortar<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.missileKills @ ":" @ %vClient.missileDeaths;
         %i2 =  %vClient.missileMA;  
         %i3 = mFloatLength(%vClient.missileACC,1) + 0 @ "%";         
         %i4 = mFloatLength((%vClient.missileInDmg/%vClient.missileShotsFired),2) + 0;
         %i5 = mFloatLength(%vClient.missileShotsFired,1)+0;           
         %i6 = mCeil(%vClient.missileMax) @ "m";         
         %line = '<color:0befe7>  Missile Launcher<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.shockLanceKills @ ":" @ %vClient.shockLanceDeaths;
         %i2 = %vClient.shockLanceMA;
         %i3 = mFloatLength(%vClient.shockACC,1) + 0 @ "%";   
         %i4 = mFloatLength((%vClient.shocklanceInDmg/%vClient.shockLanceShotsFired),2) + 0;
         %i5 = mFloatLength(%vClient.shockT,1)+0;           
         %i6 =  mCeil(%vClient.shockMax) @ "m";         
         %line = '<color:0befe7>  Shocklance<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.mineKills @ ":" @ %vClient.mineDeaths;
         %i2 =  %vClient.mineMA;  
         %i3 = mFloatLength(%vClient.mineACC,1) + 0 @ "%";          
         %i4 = mFloatLength((%vClient.mineInDmg/%vClient.mineIndirectHits),2) + 0;
         %i5 = mFloatLength(%vClient.mineT,1)+0;        
         %i6 = mCeil(%vClient.mineMax) @ "m";         
         %line = '<color:0befe7>  Mine<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7); 
         %i1 = %vClient.hGrenadeKills @ ":" @ %vClient.hGrenadeDeaths;
         %i2 =  %vClient.hGrenadeMA;  
         %i3 = mFloatLength(%vClient.hGrenadeACC,1) + 0 @ "%";           
         %i4 = mFloatLength((%vClient.hGrenadeInDmg/%vClient.hGrenadeInHits),2) + 0;
         %i5 = mFloatLength(%vClient.hGrenadeT,1)+0;           
         %i6 = mCeil(%vClient.hGrenadeMax) @ "m";         
         %line = '<color:0befe7>  Hand Grenade<lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7); 
      case "Turrets":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Turret Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7>  Moving Avg<lmargin:120>Kills<lmargin:180>Deaths<lmargin:250>Dmg<lmargin:310>Dmg Taken<lmargin:400>Hits<lmargin:470>ShotsFired';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7>  Plasma Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaTurretKills",%game),getGameData(%vClient,"plasmaTurretDeaths",%game),getGameData(%vClient,"PlasmaTurretInDmg",%game),
         getGameData(%vClient,"PlasmaTurretInDmgTaken",%game),getGameData(%vClient,"PlasmaTurretInHits",%game),getGameData(%vClient,"PlasmaTurretFired",%game));
         %line = '<color:0befe7>  AA Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"aaTurretKills",%game),getGameData(%vClient,"aaTurretDeaths",%game),getGameData(%vClient,"AATurretDmg",%game),
         getGameData(%vClient,"AATurretDmgTaken",%game),getGameData(%vClient,"AATurretDirectHits",%game),getGameData(%vClient,"AATurretFired",%game));
         %line = '<color:0befe7>  Mortar Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarTurretKills",%game),getGameData(%vClient,"mortarTurretDeaths",%game),getGameData(%vClient,"MortarTurretInDmg",%game),
         getGameData(%vClient,"MortarTurretInDmgTaken",%game),getGameData(%vClient,"MortarTurretInHits",%game),getGameData(%vClient,"MortarTurretFired",%game));
         %line = '<color:0befe7>  Missile Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileTurretKills",%game),getGameData(%vClient,"missileTurretDeaths",%game),getGameData(%vClient,"MissileTurretInDmg",%game),
         getGameData(%vClient,"MissileTurretInDmgTaken",%game),getGameData(%vClient,"MissileTurretInHits",%game),getGameData(%vClient,"MissileTurretFired",%game));
         %line = '<color:0befe7>  Indoor Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"indoorDepTurretKills",%game),getGameData(%vClient,"indoorDepTurretDeaths",%game),getGameData(%vClient,"IndoorDepTurretDmg",%game),
         getGameData(%vClient,"IndoorDepTurretDmgTaken",%game),getGameData(%vClient,"IndoorDepTurretDirectHits",%game),getGameData(%vClient,"IndoorDepTurretFired",%game));
         %line = '<color:0befe7>  Outdoor Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"outdoorDepTurretKills",%game),getGameData(%vClient,"outdoorDepTurretDeaths",%game),getGameData(%vClient,"outdoorDepTurretInDmg",%game),
         getGameData(%vClient,"OutdoorDepTurretInDmgTaken",%game),getGameData(%vClient,"OutdoorDepTurretInHits",%game),getGameData(%vClient,"OutdoorDepTurretFired",%game));
         %line = '<color:0befe7>  Sentry Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"sentryTurretKills",%game),getGameData(%vClient,"sentryTurretDeaths",%game),getGameData(%vClient,"SentryTurretDmg",%game),
         getGameData(%vClient,"SentryTurretDmgTaken",%game),getGameData(%vClient,"SentryTurretDirectHits",%game),getGameData(%vClient,"SentryTurretFired",%game));
      
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         %header = '<color:0befe7>  Total Avg<lmargin:120>Kills<lmargin:180>Deaths<lmargin:250>Dmg<lmargin:310>Dmg Taken<lmargin:400>Hits<lmargin:470>ShotsFired';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7>  Plasma Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"plasmaTurretKills",%game),getGameTotalAvg(%vClient,"plasmaTurretDeaths",%game),getGameTotalAvg(%vClient,"PlasmaTurretInDmg",%game),
         getGameTotalAvg(%vClient,"PlasmaTurretInDmgTaken",%game),getGameTotalAvg(%vClient,"PlasmaTurretInHits",%game),getGameTotalAvg(%vClient,"PlasmaTurretFired",%game));
         %line = '<color:0befe7>  AA Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"aaTurretKills",%game),getGameTotalAvg(%vClient,"aaTurretDeaths",%game),getGameTotalAvg(%vClient,"AATurretDmg",%game),
         getGameTotalAvg(%vClient,"AATurretDmgTaken",%game),getGameTotalAvg(%vClient,"AATurretDirectHits",%game),getGameTotalAvg(%vClient,"AATurretFired",%game));
         %line = '<color:0befe7>  Mortar Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"mortarTurretKills",%game),getGameTotalAvg(%vClient,"mortarTurretDeaths",%game),getGameTotalAvg(%vClient,"MortarTurretInDmg",%game),
         getGameTotalAvg(%vClient,"MortarTurretInDmgTaken",%game),getGameTotalAvg(%vClient,"MortarTurretInHits",%game),getGameTotalAvg(%vClient,"MortarTurretFired",%game));
         %line = '<color:0befe7>  Missile Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"missileTurretKills",%game),getGameTotalAvg(%vClient,"missileTurretDeaths",%game),getGameTotalAvg(%vClient,"MissileTurretInDmg",%game),
         getGameTotalAvg(%vClient,"MissileTurretInDmgTaken",%game),getGameTotalAvg(%vClient,"MissileTurretInHits",%game),getGameTotalAvg(%vClient,"MissileTurretFired",%game));
         %line = '<color:0befe7>  Indoor Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"indoorDepTurretKills",%game),getGameTotalAvg(%vClient,"indoorDepTurretDeaths",%game),getGameTotalAvg(%vClient,"IndoorDepTurretDmg",%game),
         getGameTotalAvg(%vClient,"IndoorDepTurretDmgTaken",%game),getGameTotalAvg(%vClient,"IndoorDepTurretDirectHits",%game),getGameTotalAvg(%vClient,"IndoorDepTurretFired",%game));
         %line = '<color:0befe7>  Outdoor Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"outdoorDepTurretKills",%game),getGameTotalAvg(%vClient,"outdoorDepTurretDeaths",%game),getGameTotalAvg(%vClient,"outdoorDepTurretInDmg",%game),
         getGameTotalAvg(%vClient,"OutdoorDepTurretInDmgTaken",%game),getGameTotalAvg(%vClient,"OutdoorDepTurretInHits",%game),getGameTotalAvg(%vClient,"OutdoorDepTurretFired",%game));
         %line = '<color:0befe7>  Sentry Turret<color:00dcd4><lmargin:120>%2<lmargin:180>%3<lmargin:250>%4<lmargin:310>%5<lmargin:400>%6<lmargin:470>%7';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameTotalAvg(%vClient,"sentryTurretKills",%game),getGameTotalAvg(%vClient,"sentryTurretDeaths",%game),getGameTotalAvg(%vClient,"SentryTurretDmg",%game),
         getGameTotalAvg(%vClient,"SentryTurretDmgTaken",%game),getGameTotalAvg(%vClient,"SentryTurretDirectHits",%game),getGameTotalAvg(%vClient,"SentryTurretFired",%game));
      
      case "Vehicles":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Vehicle Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %line = '<color:00dcd4><a:gamelink\tStats\tWildCat\t%1>  + WildCat Grav Cycle Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
         %line = '<color:00dcd4><a:gamelink\tStats\tSHRIKE\t%1>  + Shrike Scout Flier</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
         %line = '<color:00dcd4><a:gamelink\tStats\tBEOWULF\t%1>  + Beowulf Assault Tank</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
         %line = '<color:00dcd4><a:gamelink\tStats\tTHUNDERSWORD\t%1>  + ThunderSword Bomber</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
         %line = '<color:00dcd4><a:gamelink\tStats\tHAVOC\t%1>  + Havoc Heavy Transport Flier</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
         %line = '<color:00dcd4><a:gamelink\tStats\tMPB\t%1>  + Jericho Forward Base(MPB)</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
      case "WildCat":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>WildCat Grav Cycle Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tVehicles\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7> Road Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"wildRK",%game,%inc),getGameTotal(%vClient,"wildRK",%game),getGameTotalAvg(%vClient,"wildRK",%game),%vClient.wildRK);
         %line = '<color:0befe7> Road Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"wildRD",%game,%inc),getGameTotal(%vClient,"wildRD",%game),getGameTotalAvg(%vClient,"wildRD",%game),%vClient.wildRD);
         %line = '<color:0befe7> Runaway Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"wildEK",%game,%inc),getGameTotal(%vClient,"wildEK",%game),getGameTotalAvg(%vClient,"wildEK",%game),%vClient.wildEK);
         %line = '<color:0befe7> Crashes<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"wildCrash",%game,%inc),getGameTotal(%vClient,"wildCrash",%game),getGameTotalAvg(%vClient,"wildCrash",%game),%vClient.wildCrash);
         
      case "SHRIKE":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shrike Scout Flier Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tVehicles\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7><lmargin:190>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7> Shrike Blaster Kills<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shrikeBlasterKills",%game,%inc),getGameTotal(%vClient,"shrikeBlasterKills",%game),getGameTotalAvg(%vClient,"shrikeBlasterKills",%game),%vClient.shrikeBlasterKills);
         %line = '<color:0befe7> Shrike Blaster Deaths<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shrikeBlasterDeaths",%game,%inc),getGameTotal(%vClient,"shrikeBlasterDeaths",%game),getGameTotalAvg(%vClient,"shrikeBlasterDeaths",%game),%vClient.shrikeBlasterDeaths);
         %line = '<color:0befe7> Shrike Blaster Damage<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"ShrikeBlasterDmg",%game,%inc),getGameTotal(%vClient,"ShrikeBlasterDmg",%game),getGameTotalAvg(%vClient,"ShrikeBlasterDmg",%game),mFloatLength(%vClient.ShrikeBlasterDmg,2)+0);
         %line = '<color:0befe7> Shrike Blaster Hits<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"ShrikeBlasterDirectHits",%game,%inc),getGameTotal(%vClient,"ShrikeBlasterDirectHits",%game),getGameTotalAvg(%vClient,"ShrikeBlasterDirectHits",%game),%vClient.ShrikeBlasterDirectHits);
         %line = '<color:0befe7> Shrike Blaster Dmg Taken<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"ShrikeBlasterDmgTaken",%game,%inc),getGameTotal(%vClient,"ShrikeBlasterDmgTaken",%game),getGameTotalAvg(%vClient,"ShrikeBlasterDmgTaken",%game),mFloatLength(%vClient.ShrikeBlasterDmgTaken,2)+0);
         %line = '<color:0befe7> Shrike Blaster  Fired<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"ShrikeBlasterFired",%game,%inc),getGameTotal(%vClient,"ShrikeBlasterFired",%game),getGameTotalAvg(%vClient,"ShrikeBlasterFired",%game),%vClient.ShrikeBlasterFired);
         %line = '<color:0befe7> Road Kills<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoutFlyerRK",%game,%inc),getGameTotal(%vClient,"scoutFlyerRK",%game),getGameTotalAvg(%vClient,"scoutFlyerRK",%game),%vClient.scoutFlyerRK);
         %line = '<color:0befe7> Road Deaths<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoutFlyerRD",%game,%inc),getGameTotal(%vClient,"scoutFlyerRD",%game),getGameTotalAvg(%vClient,"scoutFlyerRD",%game),%vClient.scoutFlyerRD);
         %line = '<color:0befe7> Runaway Deaths<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoutFlyerEK",%game,%inc),getGameTotal(%vClient,"scoutFlyerEK",%game),getGameTotalAvg(%vClient,"scoutFlyerEK",%game),%vClient.scoutFlyerEK);
         %line = '<color:0befe7> Crashes<color:00dcd4><lmargin:190>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoutFlyerCrash",%game,%inc),getGameTotal(%vClient,"scoutFlyerCrash",%game),getGameTotalAvg(%vClient,"scoutFlyerCrash",%game),%vClient.scoutFlyerCrash);
         
      case "BEOWULF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Beowulf Assault Tank Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tVehicles\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7> Tank Chaingun Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"tankChaingunKills",%game,%inc),getGameTotal(%vClient,"tankChaingunKills",%game),getGameTotalAvg(%vClient,"tankChaingunKills",%game),%vClient.tankChaingunKills);
         %line = '<color:0befe7> Tank Chaingun Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"tankChaingunDeaths",%game,%inc),getGameTotal(%vClient,"tankChaingunDeaths",%game),getGameTotalAvg(%vClient,"tankChaingunDeaths",%game),%vClient.tankChaingunDeaths);
         %line = '<color:0befe7> Chaingun Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankChaingunDmg",%game,%inc),getGameTotal(%vClient,"TankChaingunDmg",%game),getGameTotalAvg(%vClient,"TankChaingunDmg",%game),mFloatLength(%vClient.TankChainGunDmg,2)+0);
         %line = '<color:0befe7> Chaingun Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankChaingunDirectHits",%game,%inc),getGameTotal(%vClient,"TankChaingunDirectHits",%game),getGameTotalAvg(%vClient,"TankChaingunDirectHits",%game),%vClient.TankChaingunDirectHits);
         %line = '<color:0befe7> Chaingun Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankChaingunDmgTaken",%game,%inc),getGameTotal(%vClient,"TankChaingunDmgTaken",%game),getGameTotalAvg(%vClient,"TankChaingunDmgTaken",%game),mFloatLength(%vClient.TankChaingunDmgTaken,2)+0);
         %line = '<color:0befe7> Chaingun Rounds Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankChaingunFired",%game,%inc),getGameTotal(%vClient,"TankChaingunFired",%game),getGameTotalAvg(%vClient,"TankChaingunFired",%game),%vClient.TankChaingunFired);
         %line = '<color:0befe7> Mortar Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"tankMortarKills",%game,%inc),getGameTotal(%vClient,"tankMortarKills",%game),getGameTotalAvg(%vClient,"tankMortarKills",%game),%vClient.tankMortarKills);
         %line = '<color:0befe7> Mortar Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"tankMortarDeaths",%game,%inc),getGameTotal(%vClient,"tankMortarDeaths",%game),getGameTotalAvg(%vClient,"tankMortarDeaths",%game),%vClient.tankMortarDeaths);
         %line = '<color:0befe7> Splash Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankMortarInDmg",%game,%inc),getGameTotal(%vClient,"TankMortarInDmg",%game),getGameTotalAvg(%vClient,"TankMortarInDmg",%game),mFloatLength(%vClient.TankMortarInDmg,2)+0);
         %line = '<color:0befe7> Mortar Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankMortarInHits",%game,%inc),getGameTotal(%vClient,"TankMortarInHits",%game),getGameTotalAvg(%vClient,"TankMortarInHits",%game),%vClient.TankMortarInHits);
         %line = '<color:0befe7> Mortar Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankMortarInDmgTaken",%game,%inc),getGameTotal(%vClient,"TankMortarInDmgTaken",%game),getGameTotalAvg(%vClient,"TankMortarInDmgTaken",%game),mFloatLength(%vClient.TankMortarInDmgTaken,2)+0);
         %line = '<color:0befe7> Mortar Rounds Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"TankMortarFired",%game,%inc),getGameTotal(%vClient,"TankMortarFired",%game),getGameTotalAvg(%vClient,"TankMortarFired",%game),%vClient.TankMortarFired);
         %line = '<color:0befe7> Road Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"assaultRK",%game,%inc),getGameTotal(%vClient,"assaultRK",%game),getGameTotalAvg(%vClient,"assaultRK",%game),%vClient.assaultRK);
         %line = '<color:0befe7> Road Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"assaultRD",%game,%inc),getGameTotal(%vClient,"assaultRD",%game),getGameTotalAvg(%vClient,"assaultRD",%game),%vClient.assaultRD);
         %line = '<color:0befe7> Runaway Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"assaultEK",%game,%inc),getGameTotal(%vClient,"assaultEK",%game),getGameTotalAvg(%vClient,"assaultEK",%game),%vClient.assaultEK);
         %line = '<color:0befe7> Crashes<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"assaultCrash",%game,%inc),getGameTotal(%vClient,"assaultCrash",%game),getGameTotalAvg(%vClient,"assaultCrash",%game),%vClient.assaultCrash);
         
      case "THUNDERSWORD":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ThunderSword Bomber Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tVehicles\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7> BellyTurret Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bellyTurretKills",%game,%inc),getGameTotal(%vClient,"bellyTurretKills",%game),getGameTotalAvg(%vClient,"bellyTurretKills",%game),%vClient.bellyTurretKills);
         %line = '<color:0befe7> BellyTurret  Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bellyTurretDeaths",%game,%inc),getGameTotal(%vClient,"bellyTurretDeaths",%game),getGameTotalAvg(%vClient,"bellyTurretDeaths",%game),%vClient.bellyTurretDeaths);
         %line = '<color:0befe7> BellyTurret Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BellyTurretDmg",%game,%inc),getGameTotal(%vClient,"BellyTurretDmg",%game),getGameTotalAvg(%vClient,"BellyTurretDmg",%game),mFloatLength(%vClient.BellyTurretDmg,2)+0);
         %line = '<color:0befe7> BellyTurret Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BellyTurretDirectHits",%game,%inc),getGameTotal(%vClient,"BellyTurretDirectHits",%game),getGameTotalAvg(%vClient,"BellyTurretDirectHits",%game),%vClient.BellyTurretDirectHits);
         %line = '<color:0befe7> BellyTurret Dmg Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BellyTurretDmgTaken",%game,%inc),getGameTotal(%vClient,"BellyTurretDmgTaken",%game),getGameTotalAvg(%vClient,"BellyTurretDmgTaken",%game),mFloatLength(%vClient.BellyTurretDmgTaken,2)+0);
         %line = '<color:0befe7> BellyTurret Rounds Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BellyTurretFired",%game,%inc),getGameTotal(%vClient,"BellyTurretFired",%game),getGameTotalAvg(%vClient,"BellyTurretFired",%game),%vClient.BellyTurretFired);
         %line = '<color:0befe7> Bomb Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bomberBombsKills",%game,%inc),getGameTotal(%vClient,"bomberBombsKills",%game),getGameTotalAvg(%vClient,"bomberBombsKills",%game),%vClient.bomberBombsKills);
         %line = '<color:0befe7> Bomb Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bomberBombsDeaths",%game,%inc),getGameTotal(%vClient,"bomberBombsDeaths",%game),getGameTotalAvg(%vClient,"bomberBombsDeaths",%game),%vClient.bomberBombsDeaths);
         %line = '<color:0befe7> Bomb Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BomberBombsInDmg",%game,%inc),getGameTotal(%vClient,"BomberBombsInDmg",%game),getGameTotalAvg(%vClient,"BomberBombsInDmg",%game),mFloatLength(%vClient.BomberBombsInDmg,2)+0);
         %line = '<color:0befe7> Bomb Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BomberBombsInHits",%game,%inc),getGameTotal(%vClient,"BomberBombsInHits",%game),getGameTotalAvg(%vClient,"BomberBombsInHits",%game),%vClient.BomberBombsInHits);
         %line = '<color:0befe7> Bomb Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BomberBombsInDmgTaken",%game,%inc),getGameTotal(%vClient,"BomberBombsInDmgTaken",%game),getGameTotalAvg(%vClient,"BomberBombsInDmgTaken",%game),mFloatLength(%vClient.BomberBombsInDmgTaken,2)+0);
         %line = '<color:0befe7> Bomb Rounds Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"BomberBombsFired",%game,%inc),getGameTotal(%vClient,"BomberBombsFired",%game),getGameTotalAvg(%vClient,"BomberBombsFired",%game),%vClient.BomberBombsFired);
         %line = '<color:0befe7> Road Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bomberFlyerRK",%game,%inc),getGameTotal(%vClient,"bomberFlyerRK",%game),getGameTotalAvg(%vClient,"bomberFlyerRK",%game),%vClient.bomberFlyerRK);
         %line = '<color:0befe7> Road Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bomberFlyerRD",%game,%inc),getGameTotal(%vClient,"bomberFlyerRD",%game),getGameTotalAvg(%vClient,"bomberFlyerRD",%game),%vClient.bomberFlyerRD);
         %line = '<color:0befe7> Runaway Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bomberFlyerEK",%game,%inc),getGameTotal(%vClient,"bomberFlyerEK",%game),getGameTotalAvg(%vClient,"bomberFlyerEK",%game),%vClient.bomberFlyerEK);
         %line = '<color:0befe7> Crashes<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"bomberFlyerCrash",%game,%inc),getGameTotal(%vClient,"bomberFlyerCrash",%game),getGameTotalAvg(%vClient,"bomberFlyerCrash",%game),%vClient.bomberFlyerCrash);
      case "HAVOC":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Havoc Heavy Transport Flier Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tVehicles\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7> Road Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hapcFlyerRK",%game,%inc),getGameTotal(%vClient,"hapcFlyerRK",%game),getGameTotalAvg(%vClient,"hapcFlyerRK",%game),%vClient.hapcFlyerRK);
         %line = '<color:0befe7> Road Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hapcFlyerRD",%game,%inc),getGameTotal(%vClient,"hapcFlyerRD",%game),getGameTotalAvg(%vClient,"hapcFlyerRD",%game),%vClient.hapcFlyerRD);
         %line = '<color:0befe7> Runaway Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hapcFlyerEK",%game,%inc),getGameTotal(%vClient,"hapcFlyerEK",%game),getGameTotalAvg(%vClient,"hapcFlyerEK",%game),%vClient.hapcFlyerEK);
         %line = '<color:0befe7> Crashes<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hapcFlyerCrash",%game,%inc),getGameTotal(%vClient,"hapcFlyerCrash",%game),getGameTotalAvg(%vClient,"hapcFlyerCrash",%game),%vClient.hapcFlyerCrash);
         
      case "MPB":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Jericho Forward Base(MPB) Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tVehicles\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
         %header = '<color:0befe7><lmargin:180>Live<lmargin:250>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7> Road Kills<color:00dcd4><lmargin:180>%5<lmargin:250>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mobileBaseRK",%game,%inc),getGameTotal(%vClient,"mobileBaseRK",%game),getGameTotalAvg(%vClient,"mobileBaseRK",%game),%vClient.mobileBaseRK);
         %line = '<color:0befe7> Road Deaths<color:00dcd4><lmargin:180>%5<lmargin:250>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mobileBaseRD",%game,%inc),getGameTotal(%vClient,"mobileBaseRD",%game),getGameTotalAvg(%vClient,"mobileBaseRD",%game),%vClient.mobileBaseRD);
         %line = '<color:0befe7> Runaway Deaths<color:00dcd4><lmargin:180>%5<lmargin:250>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mobileBaseEK",%game,%inc),getGameTotal(%vClient,"mobileBaseEK",%game),getGameTotalAvg(%vClient,"mobileBaseEK",%game),%vClient.mobileBaseEK);
         %line = '<color:0befe7> Crashes<color:00dcd4><lmargin:180>%5<lmargin:250>%2<lmargin:370>%3<lmargin:470>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mobileBaseCrash",%game,%inc),getGameTotal(%vClient,"mobileBaseCrash",%game),getGameTotalAvg(%vClient,"mobileBaseCrash",%game),%vClient.mobileBaseCrash);
         
      case "Blaster":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterKills",%game,%inc),getGameTotal(%vClient,"blasterKills",%game),getGameTotalAvg(%vClient,"blasterKills",%game),%vClient.blasterKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDeaths",%game,%inc),getGameTotal(%vClient,"blasterDeaths",%game),getGameTotalAvg(%vClient,"blasterDeaths",%game),%vClient.blasterDeaths);
         %line1 = '<color:0befe7> Direct Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDmg",%game,%inc),getGameTotal(%vClient,"blasterDmg",%game),getGameTotalAvg(%vClient,"blasterDmg",%game),mFloatLength(%vClient.blasterDmg,2)+0);
         %line1 = '<color:0befe7> Direct Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDmgTaken",%game,%inc),getGameTotal(%vClient,"blasterDmgTaken",%game),getGameTotalAvg(%vClient,"blasterDmgTaken",%game),mFloatLength(%vClient.blasterDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDirectHits",%game,%inc),getGameTotal(%vClient,"blasterDirectHits",%game),getGameTotalAvg(%vClient,"blasterDirectHits",%game),%vClient.blasterDirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterShotsFired",%game,%inc),getGameTotal(%vClient,"blasterShotsFired",%game),getGameTotalAvg(%vClient,"blasterShotsFired",%game),%vClient.blasterShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterMax",%game,%inc),getGameTotal(%vClient,"blasterMax",%game),getGameTotalAvg(%vClient,"blasterMax",%game),mFloatLength(%vClient.blasterMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterT",%game,%inc),getGameTotal(%vClient,"blasterT",%game),getGameTotalAvg(%vClient,"blasterT",%game),mFloatLength(%vClient.blasterT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterCom",%game,%inc),getGameTotal(%vClient,"blasterCom",%game),getGameTotalAvg(%vClient,"blasterCom",%game),%vClient.blasterCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterMA",%game,%inc),getGameTotal(%vClient,"blasterMA",%game),getGameTotalAvg(%vClient,"blasterMA",%game),%vClient.blasterMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterACC",%game,%inc),getGameTotal(%vClient,"blasterACC",%game),getGameTotalAvg(%vClient,"blasterACC",%game),mFloatLength(%vClient.blasterACC,2)+0);
      case "Spinfusor":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
           messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discKills",%game,%inc),getGameTotal(%vClient,"discKills",%game),getGameTotalAvg(%vClient,"discKills",%game),%vClient.discKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discDeaths",%game,%inc),getGameTotal(%vClient,"discDeaths",%game),getGameTotalAvg(%vClient,"discDeaths",%game),%vClient.discDeaths);
         %line1 = '<color:0befe7> Damage Dealt <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discInDmg",%game,%inc),getGameTotal(%vClient,"discInDmg",%game),getGameTotalAvg(%vClient,"discInDmg",%game),mFloatLength(%vClient.discInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discInDmgTaken",%game,%inc),getGameTotal(%vClient,"discInDmgTaken",%game),getGameTotalAvg(%vClient,"discInDmgTaken",%game),mFloatLength(%vClient.discInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discIndirectHits",%game,%inc),getGameTotal(%vClient,"discIndirectHits",%game),getGameTotalAvg(%vClient,"discIndirectHits",%game),%vClient.discIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discShotsFired",%game,%inc),getGameTotal(%vClient,"discShotsFired",%game),getGameTotalAvg(%vClient,"discShotsFired",%game),%vClient.discShotsFired);
         %line1 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minePlusDisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discMax",%game,%inc),getGameTotal(%vClient,"discMax",%game),getGameTotalAvg(%vClient,"discMax",%game),mFloatLength(%vClient.discMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discT",%game,%inc),getGameTotal(%vClient,"discT",%game),getGameTotalAvg(%vClient,"discT",%game),mFloatLength(%vClient.discT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discCom",%game,%inc),getGameTotal(%vClient,"discCom",%game),getGameTotalAvg(%vClient,"discCom",%game),%vClient.discCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discMA",%game,%inc),getGameTotal(%vClient,"discMA",%game),getGameTotalAvg(%vClient,"discMA",%game),%vClient.discMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discACC",%game,%inc),getGameTotal(%vClient,"discACC",%game),getGameTotalAvg(%vClient,"discACC",%game),mFloatLength(%vClient.discACC,2)+0);
      case "Chaingun":
          %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgKills",%game,%inc),getGameTotal(%vClient,"cgKills",%game),getGameTotalAvg(%vClient,"cgKills",%game),%vClient.cgKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDeaths",%game,%inc),getGameTotal(%vClient,"cgDeaths",%game),getGameTotalAvg(%vClient,"cgDeaths",%game),%vClient.cgDeaths);
         %line1 = '<color:0befe7> Direct Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDmg",%game,%inc),getGameTotal(%vClient,"cgDmg",%game),getGameTotalAvg(%vClient,"cgDmg",%game),mFloatLength(%vClient.cgDmg,2)+0);
         %line1 = '<color:0befe7> Direct Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDmgTaken",%game,%inc),getGameTotal(%vClient,"cgDmgTaken",%game),getGameTotalAvg(%vClient,"cgDmgTaken",%game),mFloatLength(%vClient.cgDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDirectHits",%game,%inc),getGameTotal(%vClient,"cgDirectHits",%game),getGameTotalAvg(%vClient,"cgDirectHits",%game),%vClient.cgDirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgShotsFired",%game,%inc),getGameTotal(%vClient,"cgShotsFired",%game),getGameTotalAvg(%vClient,"cgShotsFired",%game),%vClient.cgShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgMax",%game,%inc),getGameTotal(%vClient,"cgMax",%game),getGameTotalAvg(%vClient,"cgMax",%game),mFloatLength(%vClient.cgMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgT",%game,%inc),getGameTotal(%vClient,"cgT",%game),getGameTotalAvg(%vClient,"cgT",%game),mFloatLength(%vClient.cgT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgCom",%game,%inc),getGameTotal(%vClient,"cgCom",%game),getGameTotalAvg(%vClient,"cgCom",%game),%vClient.cgCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgMA",%game,%inc),getGameTotal(%vClient,"cgMA",%game),getGameTotalAvg(%vClient,"cgMA",%game),%vClient.cgMA);  
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgACC",%game,%inc),getGameTotal(%vClient,"cgACC",%game),getGameTotalAvg(%vClient,"cgACC",%game),mFloatLength(%vClient.cgACC,2)+0);
      case "GrenadeLauncher":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeKills",%game,%inc),getGameTotal(%vClient,"grenadeKills",%game),getGameTotalAvg(%vClient,"grenadeKills",%game),%vClient.grenadeKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeDeaths",%game,%inc),getGameTotal(%vClient,"grenadeDeaths",%game),getGameTotalAvg(%vClient,"grenadeDeaths",%game),%vClient.grenadeDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeInDmg",%game,%inc),getGameTotal(%vClient,"grenadeInDmg",%game),getGameTotalAvg(%vClient,"grenadeInDmg",%game),mFloatLength(%vClient.grenadeInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeInDmgTaken",%game,%inc),getGameTotal(%vClient,"grenadeInDmgTaken",%game),getGameTotalAvg(%vClient,"grenadeInDmgTaken",%game),mFloatLength(%vClient.grenadeInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeIndirectHits",%game,%inc),getGameTotal(%vClient,"grenadeIndirectHits",%game),getGameTotalAvg(%vClient,"grenadeIndirectHits",%game),%vClient.grenadeIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeShotsFired",%game,%inc),getGameTotal(%vClient,"grenadeShotsFired",%game),getGameTotalAvg(%vClient,"grenadeShotsFired",%game),%vClient.grenadeShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeMax",%game,%inc),getGameTotal(%vClient,"grenadeMax",%game),getGameTotalAvg(%vClient,"grenadeMax",%game),mFloatLength(%vClient.grenadeMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeT",%game,%inc),getGameTotal(%vClient,"grenadeT",%game),getGameTotalAvg(%vClient,"grenadeT",%game),mFloatLength(%vClient.grenadeT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeCom",%game,%inc),getGameTotal(%vClient,"grenadeCom",%game),getGameTotalAvg(%vClient,"grenadeCom",%game),%vClient.grenadeCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeMA",%game,%inc),getGameTotal(%vClient,"grenadeMA",%game),getGameTotalAvg(%vClient,"grenadeMA",%game),%vClient.grenadeMA);      
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeACC",%game,%inc),getGameTotal(%vClient,"grenadeACC",%game),getGameTotalAvg(%vClient,"grenadeACC",%game),mFloatLength(%vClient.grenadeACC,2)+0);
      case "LaserRifle":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserKills",%game,%inc),getGameTotal(%vClient,"laserKills",%game),getGameTotalAvg(%vClient,"laserKills",%game),%vClient.laserKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDeaths",%game,%inc),getGameTotal(%vClient,"laserDeaths",%game),getGameTotalAvg(%vClient,"laserDeaths",%game),%vClient.laserDeaths);
         %line1 = '<color:0befe7> Direct Damage<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDmg",%game,%inc),getGameTotal(%vClient,"laserDmg",%game),getGameTotalAvg(%vClient,"laserDmg",%game),mFloatLength(%vClient.laserDmg,2)+0);
         %line1 = '<color:0befe7> Direct Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDmgTaken",%game,%inc),getGameTotal(%vClient,"laserDmgTaken",%game),getGameTotalAvg(%vClient,"laserDmgTaken",%game),mFloatLength(%vClient.laserDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDirectHits",%game,%inc),getGameTotal(%vClient,"laserDirectHits",%game),getGameTotalAvg(%vClient,"laserDirectHits",%game),%vClient.laserDirectHits);
         %line1 = '<color:0befe7> Shots Fired <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserShotsFired",%game,%inc),getGameTotal(%vClient,"laserShotsFired",%game),getGameTotalAvg(%vClient,"laserShotsFired",%game),%vClient.laserShotsFired);
         %line1 = '<color:0befe7> Headshots<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Headshots<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserHeadShot",%game,%inc),getGameTotal(%vClient,"laserHeadShot",%game),getGameTotalAvg(%vClient,"laserHeadShot",%game),%vClient.laserHeadShot);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserMax",%game,%inc),getGameTotal(%vClient,"laserMax",%game),getGameTotalAvg(%vClient,"laserMax",%game),mFloatLength(%vClient.laserMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserT",%game,%inc),getGameTotal(%vClient,"laserT",%game),getGameTotalAvg(%vClient,"laserT",%game),mFloatLength(%vClient.laserT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserCom",%game,%inc),getGameTotal(%vClient,"laserCom",%game),getGameTotalAvg(%vClient,"laserCom",%game),%vClient.laserCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserMA",%game,%inc),getGameTotal(%vClient,"laserMA",%game),getGameTotalAvg(%vClient,"laserMA",%game),%vClient.laserMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserACC",%game,%inc),getGameTotal(%vClient,"laserACC",%game),getGameTotalAvg(%vClient,"laserACC",%game),mFloatLength(%vClient.laserACC,2)+0);
      case "FusionMortar":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarKills",%game,%inc),getGameTotal(%vClient,"mortarKills",%game),getGameTotalAvg(%vClient,"mortarKills",%game),%vClient.mortarKills);
         %line1 = '<color:0befe7> Deaths <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarDeaths",%game,%inc),getGameTotal(%vClient,"mortarDeaths",%game),getGameTotalAvg(%vClient,"mortarDeaths",%game),%vClient.mortarDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarInDmg",%game,%inc),getGameTotal(%vClient,"mortarInDmg",%game),getGameTotalAvg(%vClient,"mortarInDmg",%game),mFloatLength(%vClient.mortarInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarInDmgTaken",%game,%inc),getGameTotal(%vClient,"mortarInDmgTaken",%game),getGameTotalAvg(%vClient,"mortarInDmgTaken",%game),mFloatLength(%vClient.mortarInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarIndirectHits",%game,%inc),getGameTotal(%vClient,"mortarIndirectHits",%game),getGameTotalAvg(%vClient,"mortarIndirectHits",%game),%vClient.mortarIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarShotsFired",%game,%inc),getGameTotal(%vClient,"mortarShotsFired",%game),getGameTotalAvg(%vClient,"mortarShotsFired",%game),%vClient.mortarShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarMax",%game,%inc),getGameTotal(%vClient,"mortarMax",%game),getGameTotalAvg(%vClient,"mortarMax",%game),mFloatLength(%vClient.mortarMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarT",%game,%inc),getGameTotal(%vClient,"mortarT",%game),getGameTotalAvg(%vClient,"mortarT",%game),mFloatLength(%vClient.mortarT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarCom",%game,%inc),getGameTotal(%vClient,"mortarCom",%game),getGameTotalAvg(%vClient,"mortarCom",%game),%vClient.mortarCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarMA",%game,%inc),getGameTotal(%vClient,"mortarMA",%game),getGameTotalAvg(%vClient,"mortarMA",%game),%vClient.mortarMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarACC",%game,%inc),getGameTotal(%vClient,"mortarACC",%game),getGameTotalAvg(%vClient,"mortarACC",%game),mFloatLength(%vClient.mortarACC,2)+0);
      case "MissileLauncher":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileKills",%game,%inc),getGameTotal(%vClient,"missileKills",%game),getGameTotalAvg(%vClient,"missileKills",%game),%vClient.missileKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileDeaths",%game,%inc),getGameTotal(%vClient,"missileDeaths",%game),getGameTotalAvg(%vClient,"missileDeaths",%game),%vClient.missileDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileInDmg",%game,%inc),getGameTotal(%vClient,"missileInDmg",%game),getGameTotalAvg(%vClient,"missileInDmg",%game),mFloatLength(%vClient.missileInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileInDmgTaken",%game,%inc),getGameTotal(%vClient,"missileInDmgTaken",%game),getGameTotalAvg(%vClient,"missileInDmgTaken",%game),mFloatLength(%vClient.missileInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileIndirectHits",%game,%inc),getGameTotal(%vClient,"missileIndirectHits",%game),getGameTotalAvg(%vClient,"missileIndirectHits",%game),mFloatLength(%vClient.missileIndirectHits,2)+0);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileShotsFired",%game,%inc),getGameTotal(%vClient,"missileShotsFired",%game),getGameTotalAvg(%vClient,"missileShotsFired",%game),%vClient.missileShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileMax",%game,%inc),getGameTotal(%vClient,"missileMax",%game),getGameTotalAvg(%vClient,"missileMax",%game),mFloatLength(%vClient.missileMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileT",%game,%inc),getGameTotal(%vClient,"missileT",%game),getGameTotalAvg(%vClient,"missileT",%game),mFloatLength(%vClient.missileT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileCom",%game,%inc),getGameTotal(%vClient,"missileCom",%game),getGameTotalAvg(%vClient,"missileCom",%game),%vClient.missileCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileMA",%game,%inc),getGameTotal(%vClient,"missileMA",%game),getGameTotalAvg(%vClient,"missileMA",%game),%vClient.missileMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileACC",%game,%inc),getGameTotal(%vClient,"missileACC",%game),getGameTotalAvg(%vClient,"missileACC",%game),mFloatLength(%vClient.missileACC,2)+0);
      case "Shocklance":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceKills",%game,%inc),getGameTotal(%vClient,"shockLanceKills",%game),getGameTotalAvg(%vClient,"shockLanceKills",%game),%vClient.shockLanceKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceDeaths",%game,%inc),getGameTotal(%vClient,"shockLanceDeaths",%game),getGameTotalAvg(%vClient,"shockLanceDeaths",%game),%vClient.shockLanceDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceInDmg",%game,%inc),getGameTotal(%vClient,"shockLanceInDmg",%game),getGameTotalAvg(%vClient,"shockLanceInDmg",%game),mFloatLength(%vClient.shockLanceInDmg,20)+0);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceInDmgTaken",%game,%inc),getGameTotal(%vClient,"shockLanceInDmgTaken",%game),getGameTotalAvg(%vClient,"shockLanceInDmgTaken",%game),mFloatLength(%vClient.shockLanceInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Direct Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceIndirectHits",%game,%inc),getGameTotal(%vClient,"shockLanceIndirectHits",%game),getGameTotalAvg(%vClient,"shockLanceIndirectHits",%game),%vClient.shockLanceIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceShotsFired",%game,%inc),getGameTotal(%vClient,"shockLanceShotsFired",%game),getGameTotalAvg(%vClient,"shockLanceShotsFired",%game),%vClient.shockLanceShotsFired);
         %line1 = '<color:0befe7> Backshots<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Backshots<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockRearShot",%game,%inc),getGameTotal(%vClient,"shockRearShot",%game),getGameTotalAvg(%vClient,"shockRearShot",%game),%vClient.shockRearShot);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockMax",%game,%inc),getGameTotal(%vClient,"shockMax",%game),getGameTotalAvg(%vClient,"shockMax",%game),mFloatLength(%vClient.shockMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockT",%game,%inc),getGameTotal(%vClient,"shockT",%game),getGameTotalAvg(%vClient,"shockT",%game),mFloatLength(%vClient.shockT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockCom",%game,%inc),getGameTotal(%vClient,"shockCom",%game),getGameTotalAvg(%vClient,"shockCom",%game),%vClient.shockCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockMA",%game,%inc),getGameTotal(%vClient,"shockMA",%game),getGameTotalAvg(%vClient,"shockMA",%game),%vClient.shockMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockACC",%game,%inc),getGameTotal(%vClient,"shockACC",%game),getGameTotalAvg(%vClient,"shockACC",%game),mFloatLength(%vClient.shockACC,2)+0);
      case "PlasmaRifle":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaKills",%game,%inc),getGameTotal(%vClient,"plasmaKills",%game),getGameTotalAvg(%vClient,"plasmaKills",%game),%vClient.plasmaKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaDeaths",%game,%inc),getGameTotal(%vClient,"plasmaDeaths",%game),getGameTotalAvg(%vClient,"plasmaDeaths",%game),%vClient.plasmaDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaInDmg",%game,%inc),getGameTotal(%vClient,"plasmaInDmg",%game),getGameTotalAvg(%vClient,"plasmaInDmg",%game),mFloatLength(%vClient.plasmaInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaInDmgTaken",%game,%inc),getGameTotal(%vClient,"plasmaInDmgTaken",%game),getGameTotalAvg(%vClient,"plasmaInDmgTaken",%game),mFloatLength(%vClient.plasmaInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaIndirectHits",%game,%inc),getGameTotal(%vClient,"plasmaIndirectHits",%game),getGameTotalAvg(%vClient,"plasmaIndirectHits",%game),mFloatLength(%vClient.plasmaIndirectHits,2)+0);
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaShotsFired"),getGameTotal(%vClient,"plasmaShotsFired",%game),getGameTotalAvg(%vClient,"plasmaShotsFired",%game),%vClient.plasmaShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaMax",%game,%inc),getGameTotal(%vClient,"plasmaMax",%game),getGameTotalAvg(%vClient,"plasmaMax",%game),mFloatLength(%vClient.plasmaMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaT",%game,%inc),getGameTotal(%vClient,"plasmaT",%game),getGameTotalAvg(%vClient,"plasmaT",%game),mFloatLength(%vClient.plasmaT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaCom",%game,%inc),getGameTotal(%vClient,"plasmaCom",%game),getGameTotalAvg(%vClient,"plasmaCom",%game),%vClient.plasmaCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaMA",%game,%inc),getGameTotal(%vClient,"plasmaMA",%game),getGameTotalAvg(%vClient,"plasmaMA",%game),%vClient.plasmaMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaACC",%game,%inc),getGameTotal(%vClient,"plasmaACC",%game),getGameTotalAvg(%vClient,"plasmaACC",%game),mFloatLength(%vClient.plasmaACC,2)+0);
      case "ELF":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"elfShotsFired",%game,%inc),getGameTotal(%vClient,"elfShotsFired",%game),getGameTotalAvg(%vClient,"elfShotsFired",%game),%vClient.elfShotsFired);
      case "Mine":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Mine Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Mine Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineKills",%game,%inc),getGameTotal(%vClient,"mineKills",%game),getGameTotalAvg(%vClient,"mineKills",%game),%vClient.mineKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineDeaths",%game,%inc),getGameTotal(%vClient,"mineDeaths",%game),getGameTotalAvg(%vClient,"mineDeaths",%game),%vClient.mineDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineInDmg",%game,%inc),getGameTotal(%vClient,"mineInDmg",%game),getGameTotalAvg(%vClient,"mineInDmg",%game),mFloatLength(%vClient.mineInDmg,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineIndirectHits",%game,%inc),getGameTotal(%vClient,"mineIndirectHits",%game),getGameTotalAvg(%vClient,"mineIndirectHits",%game),%vClient.mineIndirectHits);
         %line1 = '<color:0befe7> Mines Thrown<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mines Thrown<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineShotsFired",%game,%inc),getGameTotal(%vClient,"mineShotsFired",%game),getGameTotalAvg(%vClient,"mineShotsFired",%game),%vClient.mineShotsFired);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineInDmgTaken",%game,%inc),getGameTotal(%vClient,"mineInDmgTaken",%game),getGameTotalAvg(%vClient,"mineInDmgTaken",%game),mFloatLength(%vClient.mineInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineMax",%game,%inc),getGameTotal(%vClient,"mineMax",%game),getGameTotalAvg(%vClient,"mineMax",%game),mFloatLength(%vClient.mineMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineT",%game,%inc),getGameTotal(%vClient,"mineT",%game),getGameTotalAvg(%vClient,"mineT",%game),mFloatLength(%vClient.mineT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineCom",%game,%inc),getGameTotal(%vClient,"mineCom",%game),getGameTotalAvg(%vClient,"mineCom",%game),%vClient.mineCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineMA",%game,%inc),getGameTotal(%vClient,"mineMA",%game),getGameTotalAvg(%vClient,"mineMA",%game),%vClient.mineMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineACC",%game,%inc),getGameTotal(%vClient,"mineACC",%game),getGameTotalAvg(%vClient,"mineACC",%game),mFloatLength(%vClient.mineACC,2)+0);
      case "HandGrenade":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Hand Grenade Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Hand Grenade Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }

         %line1 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeKills",%game,%inc),getGameTotal(%vClient,"hGrenadeKills",%game),getGameTotalAvg(%vClient,"hGrenadeKills",%game),%vClient.hGrenadeKills);
         %line1 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeDeaths",%game,%inc),getGameTotal(%vClient,"hGrenadeDeaths",%game),getGameTotalAvg(%vClient,"hGrenadeDeaths",%game),%vClient.hGrenadeDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeInDmg",%game,%inc),getGameTotal(%vClient,"hGrenadeInDmg",%game),getGameTotalAvg(%vClient,"hGrenadeInDmg",%game),mFloatLength(%vClient.hGrenadeInDmg,2)+0);
         %line1 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeInHits",%game,%inc),getGameTotal(%vClient,"hGrenadeInHits",%game),getGameTotalAvg(%vClient,"hGrenadeInHits",%game),%vClient.hGrenadeInHits);
         %line1 = '<color:0befe7> Grenades Thrown<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Grenades Thrown<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeShotsFired",%game,%inc),getGameTotal(%vClient,"hGrenadeShotsFired",%game),getGameTotalAvg(%vClient,"hGrenadeShotsFired",%game),%vClient.hGrenadeShotsFired);
         %line1 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeInDmgTaken",%game,%inc),getGameTotal(%vClient,"hGrenadeInDmgTaken",%game),getGameTotalAvg(%vClient,"hGrenadeInDmgTaken",%game),mFloatLength(%vClient.hGrenadeInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeMax",%game,%inc),getGameTotal(%vClient,"hGrenadeMax",%game),getGameTotalAvg(%vClient,"hGrenadeMax",%game),mFloatLength(%vClient.hGrenadeMax,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeT",%game,%inc),getGameTotal(%vClient,"hGrenadeT",%game),getGameTotalAvg(%vClient,"hGrenadeT",%game),mFloatLength(%vClient.hGrenadeT,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeCom",%game,%inc),getGameTotal(%vClient,"hGrenadeCom",%game),getGameTotalAvg(%vClient,"hGrenadeCom",%game),%vClient.hGrenadeCom);
         %line1 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeMA",%game,%inc),getGameTotal(%vClient,"hGrenadeMA",%game),getGameTotalAvg(%vClient,"hGrenadeMA",%game),%vClient.hGrenadeMA);
         %line1 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeACC",%game,%inc),getGameTotal(%vClient,"hGrenadeACC",%game),getGameTotalAvg(%vClient,"hGrenadeACC",%game),mFloatLength(%vClient.hGrenadeACC,2)+0);
      case "SatchelCharge":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Satchel Charge Stats" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%inc);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Satchel Charge Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tWEAPON\t%1\t-1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"satchelChargeKills",%game,%inc),getGameTotal(%vClient,"satchelChargeKills",%game),getGameTotalAvg(%vClient,"satchelChargeKills",%game),%vClient.satchelChargeKills);
         %line1 = '<color:0befe7> Deaths <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"satchelChargeDeaths",%game,%inc),getGameTotal(%vClient,"satchelChargeDeaths",%game),getGameTotalAvg(%vClient,"satchelChargeDeaths",%game),%vClient.satchelChargeDeaths);
         %line1 = '<color:0befe7> Splash Damage <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Splash Damage <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"SatchelInDmg",%game,%inc),getGameTotal(%vClient,"SatchelInDmg",%game),getGameTotalAvg(%vClient,"SatchelInDmg",%game),mFloatLength(%vClient.SatchelInDmg,2)+0);
         %line1 = '<color:0befe7> Hits <color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits <color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"SatchelInHits",%game,%inc),getGameTotal(%vClient,"SatchelInHits",%game),getGameTotalAvg(%vClient,"SatchelInHits",%game),%vClient.SatchelInHits);
         %line1 = '<color:0befe7> Splash Damage Taken<color:00dcd4><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"SatchelInDmgTaken",%game,%inc),getGameTotal(%vClient,"SatchelInDmgTaken",%game),getGameTotalAvg(%vClient,"SatchelInDmgTaken",%game),mFloatLength(%vClient.SatchelInDmgTaken,2)+0);
      default://fail safe / reset
         %client.viewMenu = 0;
         %client.viewClient = 0;
         %client.viewStats = 0;
   }
}
