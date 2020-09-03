//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	Stats system for classic and base 																				
//	Script BY: DarkTiger																							
// Version 1.0 - Initial release																					
// Version 2.0 - Code refactor / optimizing / fixes																	
// Version 3.0 - DM / LCTF added
// Version 4.0 - Code refactor / optimizing / fixes
// Version 5.0 - DuleMod and Arena support / optimizing / fixes / misc stuff		
//	Version 6.0 - Lan & Bot Support / Leaderboard / Stats Storage Overhaul / Optimization / Fixes 	
//	Version 7.0 - Code refactor / Heavy Optimization / Map Stats / Server Stats / Fixes / Misc other features  							
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
//    Moved commonly overridden functions in its own package and uses DefaultGame- 
//    -activate/deactivatePackages to correctly postion them on top of gameType 
//    -overrides this fixes issues with lakRabbitGame overrides
//    Reworked some of the values on live screen to be more correct 
//    Moved resetDtStats to MissionDropReady so that liveStats work in lan game
//    
//    6.0
//    Full lan/no guid support - function will gen a uid  and associate it with the players name
//    Overhall of the way we store stats - NOTE this breaks compatibility with older versions so delete serverStats folder 
//    Leader Board/Best Of System - this info is compled during non peak server hours  see $dtStats::buildSetTime 
//    Rename set/getFieldValue to set/getDynamicField to be less confuseing with the other field functions that deal with strings
//    Added setValueField function to handel the new way of stats storage 
//    Made skip zeros the the only method for running averages there for removed extra code  
//    Removed timeLimitReached and scoreLimitReached, not sure why i needed them in the first place everything runs threw gameover anyways  
//    Removed $dtStats::slowLoadTime its not used any more with the new system as theres only 2 files to load vs 11  at a given time 
//    History menu redone added a page system  to allow for larger then 10 game history    
//    System now self maintains files and will delete when out of date see $dtStats::expireMax
//    Removed AI checks and added in ai support for better testing
//    Fix some divide by zero issues useing conditional ternary operator example condition ? result1 : result2
//    Fix few dynamic fields that were named wrong resulting in stats just showing 0 
//    Score resetting is handeld in script to better handel end game saving
//    Added unused varable array uFV for short, this is only to reset the values we dont track directly or unused gametype values  
//    Renamed the arrays to keep them shorter, example fieldValue is now FV
//    Added max array to allow recording of varables we only want the max of example being longest sniper shot
//    Added an averaging array to be able to store current averages, keeps code complexity down on loaderboard stuff, may rework later 
//    setValueField will now default to 0 is value is "" 
//    Fixed mine disc code forgot to add %targetClient to the resetCode 
//    Replaced gameWinStat with postGameStats were custom stuff can added up or handled at the end of the game
//    Added Armor::onTrigger to better track stats on player, and to remove duplicate code 
//    Added a handfull of new varables to track, too many to list.
//    Surival time is now acurite also simplified the code  
//    Removed turrets stats other then kills death
//
//    6.5 Fixes
//    Misc fixes from 6.0 additions/changes 
//    Added option to view the other game types within the leaderboard stuff 
//    Bumped up distance for mortar midAirs so it's just outside its damage radius something strange is going on there
//    Added game type arrays for display and processing
//
//    6.6
//    Added game id to track individual games 
//    Removed misc turret stuff
//    Removed vehicle menus 
//    Misc fixes
//    
//    6.7
//    Escort fix moved
//    Stats now allways save no matter the condition, this should reduce some edge cases problems and to have a more complete data set
//    Added gamePCT becuase of the change to always save to track witch maps/games were cut short or joined in the middle of a match   
//    MapID Gen to track map specific stats 
//
//    7.0
//    Way to many changes to list so here are the major changes
//    Super heavy opmtiation and rework, new changes has improved some areas by 25-75% in terms of speed/impact on the server
//    Player Map Stats - like totals but done per map, this is also used to build map based leaderboards 
//    Server/Map Stats - track whats being played and whats being skipped as well as server health  
//    Added fail safe options load after and load slow  in case the amount of stat tracking grows too large
//    Ton of new stat values added 
//    Score hud UI mostly Reworked
// 
//    7.1
//    Combined save game and save total into one function 
//    Switched client leave for onDrop as client leave is tied to game type and does not work in between maps
//    changed vote override too serverCmdStartNewVote just so this script works in base and default classic
//    Few misc stats fixes 
//    Vote for map stat now gens an map id, so it will now show on the list
//    Renamed var for server hangs and host hangs, as they were not saving  
//    Removed a few stats that have no relevance 
//    Typo for ping avg var fix
//    Fixed gameID now saves and was moved to MissonLoadDone
//    Added mapGameID for parsers 
//    Fixed chat stats 
//    Added LeftID to better track what game the client left mainly for stat resetting 
//    Added deploy stats
//    Fixed issue were if a player left during the game over screen and it hasent saved yet, it would delete the stats before saving 
//    Score restore optimization 
//    
//    7.2
//    Map stats sorting and load optimization
//    Added version number global, and saved in player stats  
//    Added lastPlay, mapReconnects and mapDisconnectsScore for server stats 
//    Stats ui cleanup 
//    Fix for onDeploy was missing return %obj;
//    incGameStats now has $dtStats::mapStats to completely disable this feature if need be 
//    Fixed armor vs armor total
//    Added a global var to control build/sort speed of stats and set it back to 128
//    Few new stats
//    Disabled some mine disc stats as they were not accurate, may revisit later  
//    Fixed Flipflop stat
//
//    7.3 
//    Stat fixes to do copy paste mistakes  
//
//    7.35
//    Mine Disc Kill Stat 
//    Some extra lightning stats
//    Stat name and rename fixes
//    Fixed pixel margin on LMB and LB page
//    clientDmgStats optimization and cleanup
//    Score hud bug with viewing last month map stats 
//    Remove dtTurret stat unused 
//    
//-----------Settings------------
//Notes score ui width is 592
$dtStats::version = 7.35; 
//disable stats system restart required;
$dtStats::Enable = 1; 
//enable disable map stats
$dtStats::mapStats  = 1; 
//Only self client can see his own stats, any stat, unless admin
$dtStats::viewSelf = 0; 
//set max number of individual game to record
//Note only tested to 100 games, hard cap at 300
$dtStats::MaxNumOfGames = 100;
//number of games for running average 
$dtStats::avgCount = 10;

//how high the player has to be off the ground before it will count
$dtStats::midAirHeight = 10;

// 30 sec min after not making an action reset
$dtStats::returnToMenuTimer = (30*1000);

//sorting speed 
$dtStats::sortSpeed = 128;

//Load/saving rates to prevent any server hitching
$dtStats::slowSaveTime = 100;
$dtStats::loadSlowTime = 0;// dont use yet
//This will load player stats after their first game, to reduce any impact on the server.
$dtStats::loadAfter = 0;//keep 0 not finished 

//Control whats displayed  
$dtStats::Live = 1;  
$dtStats::KD = 0;// disabled 
$dtStats::Hist =1;
 
//Leaderboards stuff
//To rebuild the leaderboards manually type lStatsCycle(1) into the console;
//This time marks the end of day and to rebuild the leaderboards, best set this time when the server is normally empty or low numbers
$dtStats::buildSetTime = "8\t00\tam"; 
// top 15 players per cat;
$dtStats::topAmount = 15;
//Set 2 or more to enable, this also contorls how much history you want, best to keep this count low 
$dtStats::day = 0;//-365
$dtStats::week = 0;//~53
$dtStats::month = 3; //-12
$dtStats::quarter = 0;//-4
$dtStats::year = 0;// number of years

$dtStats::expireMax = 90;  
$dtStats::expireMin = 5;
// you gain extra days based on time played extra days = gameCount * expireFactor;
// example being 100 games * factor of 0.596 = will gain you 60 extra days but if its over the 90 day max it will be deleted
$dtStats::expireFactor["CTFGame"] = 0.596;
$dtStats::expireFactor["LakRabbitGame"] = 2;
$dtStats::expireFactor["DMGame"] = 6;
$dtStats::expireFactor["SCtFGame"] = 1.2;
$dtStats::expireFactor["ArenaGame"] = 2;
$dtStats::expireFactor["DuelGame"] = 10;

//File maintainers to deletes old files see $dtStats::expireMax
//deletes player stats files that are x amount days old, only works if $dtStats::sm  is enabled
$dtStats::sm  = 1;
//set to 1 to delete old leaderboards files
$dtStats::lsm  = 1;
$dtStats::lsmMap  = 1; 

//debug stuff 
$dtStats::enableRefresh = 0;
$dtStats::debugEchos = 1;// echos function calls
//$dtStats::returnToMenuTimer = (303*1000);
//$pref::NoClearConsole = 1;
//setLogMode(1);
//$AIDisableChat = 1;
//dbgSetParameters(6060,"password"); 
//$dtStats::serverReset = 1;// disable server reset

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



////////////////////////////////////////////////////////////////////////////////
//                           Supported Game Types
////////////////////////////////////////////////////////////////////////////////
//Array for processing stats
$dtStats::gameType[0] = "CTFGame";
$dtStats::gameType[1] = "LakRabbitGame";
$dtStats::gameType[2] = "DMGame";
$dtStats::gameType[3] = "SCtFGame";
$dtStats::gameType[4] = "ArenaGame"; 
$dtStats::gameType[5] = "DuelGame"; 
//$dtStats::gameType[6] = "SiegeGame";
$dtStats::gameTypeCount = 6;
//short hand name
$dtStats::gtNameShort["CTFGame"] = "CTF";
$dtStats::gtNameShort["LakRabbitGame"] = "LakRabbit";
$dtStats::gtNameShort["DMGame"] = "DM";
$dtStats::gtNameShort["SCtFGame"] = "LCTF";
$dtStats::gtNameShort["ArenaGame"] = "Arena"; 
$dtStats::gtNameShort["DuelGame"] = "Duel"; 
//$dtStats::gtNameShort["SiegeGame"] = "Siege"; 
//Display name 
$dtStats::gtNameLong["CTFGame"] = "Capture the Flag";
$dtStats::gtNameLong["LakRabbitGame"] = "LakRabbit";
$dtStats::gtNameLong["DMGame"] = "Deathmatch";
$dtStats::gtNameLong["SCtFGame"] = "Spawn CTF";
$dtStats::gtNameLong["ArenaGame"] = "Arena"; 
$dtStats::gtNameLong["DuelGame"] = "Duel MOD"; 
//$dtStats::gtNameLong["SiegeGame"] = "Siege"; 

//varTypes
$dtStats::varType[0] = "Game";//Game only stat
$dtStats::varType[1] = "TG";  //Total & Game stat 
$dtStats::varType[2] = "TTL"; //Total only stat
$dtStats::varType[3] = "Max"; //Largest value
$dtStats::varType[4] = "Min"; //Smallest value sorted inverse
$dtStats::varType[5] = "Avg"; //Average value
$dtStats::varType[6] = "AvgI";//Average value sorted inverse 
$dtStats::varTypeCount = 7;

function dtStatsResetGobals(){
   for(%v = 0; %v < $dtStats::varTypeCount; %v++){
      %varType = $dtStats::varType[%v];
      $dtStats::FC[%varType] = 0;
      for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
         %gameType = $dtStats::gameType[%i];
         $dtStats::uGFC[%gameType] = 0;
         $dtStats::FC[%gameType,%varType] =0;
         $dtStats::FCG[%gameType,%varType] =0;
      }
   }
   $dtStats::unusedCount = 0;
}dtStatsResetGobals();

///////////////////////////////////////////////////////////////////////////////
//                             		CTF
///////////////////////////////////////////////////////////////////////////////
//gametype values with in the gametype file CTFGame.cs
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "score";  
$dtStats::FVG[$dtStats::FCG["CTFGame","Avg"]++,"CTFGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["CTFGame","Max"]++,"CTFGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagCaps";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "carrierKills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagReturns";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "scoreMidAir";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "scoreHeadshot";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "scoreRearshot";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "escortAssists";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "defenseScore";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "offenseScore";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "flagDefends";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "SensorRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "TurretRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "StationRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "VStationRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "mpbtstationRepairs"; 
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "solarRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "sentryRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depSensorRepairs"; 
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depInvRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depTurretRepairs";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "tkDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "sensorDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "turretDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "iStationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "vstationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "mpbtstationDestroys"; 
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "solarDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "sentryDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depSensorDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depTurretDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "depStationDestroys";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "vehicleScore"; 
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "vehicleBonus"; 
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "genDefends";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "turretKills";
$dtStats::FVG[$dtStats::FCG["CTFGame","TG"]++,"CTFGame","TG"] = "mannedTurretKills";
/////////////////////////////////////////////////////////////////////////////
//gametype values in this script
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "winCount"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "lossCount";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "destruction";
$dtStats::FV[$dtStats::FC["CTFGame","Min"]++,"CTFGame","Min"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["CTFGame","AvgI"]++,"CTFGame","AvgI"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["CTFGame","Max"]++,"CTFGame","Max"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","Avg"]++,"CTFGame","Avg"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["CTFGame","Avg"]++,"CTFGame","Avg"] = "capEfficiency";
$dtStats::FV[$dtStats::FC["CTFGame","Avg"]++,"CTFGame","Avg"] = "winLostPct";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "wildRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "assaultRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "mobileBaseRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "scoutFlyerRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "bomberFlyerRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "hapcFlyerRK";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "wildRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "assaultRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "mobileBaseRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "scoutFlyerRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "bomberFlyerRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "hapcFlyerRD";
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "roadKills"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "roadDeaths"; 
$dtStats::FV[$dtStats::FC["CTFGame","Game"]++,"CTFGame","Game"] = "dtTeam"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "repairs"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "MotionSensorDep"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "PulseSensorDep"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "InventoryDep"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "TurretOutdoorDep"; 
$dtStats::FV[$dtStats::FC["CTFGame","TG"]++,"CTFGame","TG"] = "TurretIndoorDep"; 

/////////////////////////////////////////////////////////////////////////////
//Unused vars needed for stats back up
$dtStats::uGFV[$dtStats::uGFC["CTFGame"]++,"CTFGame"] = "returnPts";

///////////////////////////////////////////////////////////////////////////////
//                            	 LakRabbit								
///////////////////////////////////////////////////////////////////////////////
//Game type values - out of LakRabbitGame.cs
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","Avg"]++,"LakRabbitGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","Max"]++,"LakRabbitGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "morepoints";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "mas";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "MidairflagGrabs";
$dtStats::FVG[$dtStats::FCG["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "MidairflagGrabPoints";

$dtStats::FV[$dtStats::FC["LakRabbitGame","TG"]++,"LakRabbitGame","TG"] = "flagTimeMin";

$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "flagTimeMS";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalChainAccuracy";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalChainHits";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalSnipeHits";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalSnipes";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalSpeed";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalDistance";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalShockHits";
$dtStats::uGFV[$dtStats::uGFC["LakRabbitGame"]++,"LakRabbitGame"] = "totalShocks";
///////////////////////////////////////////////////////////////////////////////
//                            	 DMGame								   		
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["DMGame","Avg"]++,"DMGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["DMGame","Max"]++,"DMGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["DMGame","TG"]++,"DMGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["DMGame","Avg"]++,"DMGame","Avg"] = "efficiency";

$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "MidAir";
$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "Bonus";
$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "KillStreakBonus";
$dtStats::uGFV[$dtStats::uGFC["DMGame"]++,"DMGame"] = "killCounter";
///////////////////////////////////////////////////////////////////////////////
//                             		LCTF									
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["SCtFGame","Max"]++,"SCtFGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagCaps";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagGrabs";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "carrierKills";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagReturns";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "scoreMidAir";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "scoreHeadshot";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "scoreRearshot";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "escortAssists";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "defenseScore";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "offenseScore";
$dtStats::FVG[$dtStats::FCG["SCtFGame","TG"]++,"SCtFGame","TG"] = "flagDefends";

$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "winCount";// in this script only
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "lossCount";
$dtStats::FV[$dtStats::FC["SCtFGame","Min"]++,"SCtFGame","Min"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["SCtFGame","AvgI"]++,"SCtFGame","AvgI"] = "heldTimeSec";
$dtStats::FV[$dtStats::FC["SCtFGame","Max"]++,"SCtFGame","Max"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "grabSpeed";
$dtStats::FV[$dtStats::FC["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "capEfficiency";
$dtStats::FV[$dtStats::FC["SCtFGame","Avg"]++,"SCtFGame","Avg"] = "winLostPct";
$dtStats::FV[$dtStats::FC["SCtFGame","Game"]++,"SCtFGame","Game"] = "dtTeam"; 
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "destruction";
$dtStats::FV[$dtStats::FC["SCtFGame","TG"]++,"SCtFGame","TG"] = "repairs"; 
////////////////////////////Unused LCTF Vars/////////////////////////////////////
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "tkDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "genDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "sensorDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "turretDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "iStationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "vstationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "mpbtstationDestroys"; 
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "solarDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "sentryDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depSensorDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depTurretDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depStationDestroys";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "vehicleScore"; 
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "vehicleBonus"; 
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "genDefends";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "escortAssists";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "turretKills";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "mannedTurretKills";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "genRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "SensorRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "TurretRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "StationRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "VStationRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "mpbtstationRepairs"; 
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "solarRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "sentryRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depSensorRepairs"; 
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depInvRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "depTurretRepairs";
$dtStats::uGFV[$dtStats::uGFC["SCtFGame"]++,"SCtFGame"] = "returnPts";
///////////////////////////////////////////////////////////////////////////////
//                            	 DuelGame								   		
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["DuelGame","TG"]++,"DuelGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["DuelGame","Avg"]++,"DuelGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["DuelGame","Max"]++,"DuelGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["DuelGame","TG"]++,"DuelGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["DuelGame","TG"]++,"DuelGame","TG"] = "deaths";
///////////////////////////////////////////////////////////////////////////////
//                            	 ArenaGame								   		
///////////////////////////////////////////////////////////////////////////////
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "score";
$dtStats::FVG[$dtStats::FCG["ArenaGame","Avg"]++,"ArenaGame","Avg"] = "score";
$dtStats::FVG[$dtStats::FCG["ArenaGame","Max"]++,"ArenaGame","Max"] = "score";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "kills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "deaths";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "suicides";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "teamKills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "snipeKills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "roundsWon";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "roundsLost";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "assists";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "roundKills";
$dtStats::FVG[$dtStats::FCG["ArenaGame","TG"]++,"ArenaGame","TG"] = "hatTricks";
$dtStats::FV[$dtStats::FC["ArenaGame","Game"]++,"ArenaGame","Game"] = "dtTeam"; 
///////////////////////////////////////////////////////////////////////////////
//                            	 SiegeGame								   		
///////////////////////////////////////////////////////////////////////////////
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "score"; 
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "kills";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "deaths";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "suicides";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "objScore";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "teamKills";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "turretKills";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "offenseScore";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "defenseScore";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "tkDestroys";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "genDestroys";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "solarDestroys";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "flipFlopDefends";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "genDefends";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "genRepairs";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "solarRepairs";
//$dtStats::FVG[$dtStats::FCG["SiegeGame","TG"]++,"SiegeGame","TG"] = "outOfBounds";

///////////////////////////////////////////////////////////////////////////////
//                              Weapon/Misc Stats
///////////////////////////////////////////////////////////////////////////////
//these are field values from this script
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeaths";
//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfKills";
//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "explosionKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "explosionDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "impactKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "impactDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "groundKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "groundDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "aaTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "aaTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "indoorDepTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "indoorDepTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outdoorDepTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outdoorDepTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sentryTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sentryTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outOfBoundKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "outOfBoundDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lavaKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lavaDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shrikeBlasterKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shrikeBlasterDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bellyTurretKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bellyTurretDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bomberBombsKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "bomberBombsDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankChaingunKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankChaingunDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankMortarKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tankMortarDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "vehicleSpawnKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "vehicleSpawnDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "forceFieldPowerUpKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "forceFieldPowerUpDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "crashKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "crashDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nexusCampingKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nexusCampingDeaths";

//Damage Stats
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDmg";
//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDmg";

//rounds fired
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineShotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelShotsFired";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterHits";
//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "elfHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelHits";
//aoe hits
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDmgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDmgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDmgHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDmgHits";

//misc
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserHeadShot";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockRearShot";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "minePlusDisc";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "minePlusDiscKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shotsFired";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalMA"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalTime";  

$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitDist";  
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitHeight";  
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maHitSV";  

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killAir"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killGround"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathAir"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathGround"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killAirAir"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killAirGround"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killGroundAir"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killGroundGround"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathAirAir"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathGroundAir"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathAirGround"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathGroundGround"; 

//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "airTime"; 
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "airTime"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "groundTime";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "groundTime";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVKills"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVDeaths";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAkills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAHits";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lightningMAEVKills";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVHitWep";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "EVMAHit";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "totalWepDmg";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "timeTL";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "timeTL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "killStreak";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "killStreak";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "assist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "maxSpeed";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "avgSpeed";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "comboCount";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "distMov";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "weaponScore"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "firstKill"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "lastKill"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "deathKills";
//$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mineDiscPct";
//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDiscHit";
//$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mineDiscAcc";
//$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mineDiscAccMP";
//$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDiscShots";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "kdr"; 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "ctrlKKills";

// nongame
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "leavemissionareaCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "teamkillCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "switchteamCount"; 
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "flipflopCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "packpickupCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "weaponpickupCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "repairpackpickupCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "repairpackpickupEnemy";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "invyEatRepairPack";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "chatallCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "chatteamCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "voicebindsallCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "voicebindsteamCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "kickCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "obstimeoutkickCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "spawnobstimeoutCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "voteCount";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "lagSpikes";
$dtStats::FV[$dtStats::FC["TTL"]++,"TTL"] = "clientCrash";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorH";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLH";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorML";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMH";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHM";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHH";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLLD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLMD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorLHD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMLD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMMD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorMHD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHLD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHMD";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "armorHHD";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "doubleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tripleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quadrupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quintupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sextupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "septupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "octupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nonupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "decupleKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nuclearKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "multiKill";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "doubleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "tripleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quadrupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "quintupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "sextupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "septupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "octupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "nonupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "decupleChainKill";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "chainKill";

//weapon combos
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockCom";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelCom";

 //source kill velocity  - note no mine 
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] ="cgKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeKillSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileKillSV";

 //source hit velocity - note no mine 
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] ="cgHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeHitSV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileHitSV";

 //victim velocity
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] ="cgKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileKillVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelKillVV";


 //victim velocity
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] ="cgHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileHitVV";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelHitVV";

//midairs

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelMA";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarAoeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeAoeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discAoeMA";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaAoeMA";

//ma dist
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileMADist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineMADist";
   
   
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineHitDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelHitDist";

$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "cgKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "discKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "grenadeKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "hGrenadeKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "laserKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mortarKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "missileKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "plasmaKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "blasterKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "mineKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "satchelKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "shockKillDist";
$dtStats::FV[$dtStats::FC["Max"]++,"Max"] = "weaponHitDist";
   

//conditional see postGame 
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "cgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "discACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "grenadeACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "laserACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mortarACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "shockACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "plasmaACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "blasterACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "hGrenadeACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mineACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "satchelACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "missileACC";

$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "plasmaDmgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "discDmgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "grenadeDmgACC";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "mortarDmgACC";

$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "onTargetAcc";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "onTargetHMR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "onTargetHit";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "onTargetMis";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "onFire";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "onFire";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "onInput";
$dtStats::FV[$dtStats::FC["Avg"]++,"Avg"] = "onInput";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitHead";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenHead";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitHeadFront";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenHeadFront";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitHeadBack";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenHeadBack";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitHeadRight";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenHeadRight";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitHeadLeft";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenHeadLeft";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTorso";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenTorso";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTorsoFrontR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenTorsoFrontR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTorsoFrontL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenTorsoFrontL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTorsoBackR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenTorsoBackR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTorsoBackL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenTorsoBackL";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitLegs";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenLegs";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitLegFrontR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenLegFrontR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitLegFrontL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenLegFrontL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitLegBackR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenLegBackR";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitLegBackL";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hitTakenLegBackL";




$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKillAirAir"; // air to air kill
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKillAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKillAirAir";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeathAirAir"; // air to air death
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeathAirAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeathAirAir";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKillAirGround"; // air to ground kill
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKillAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKillAirGround";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeathAirGround";// air to ground death 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeathAirGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeathAirGround";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKillGroundAir"; // ground to air kill
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKillGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKillGroundAir";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeathGroundAir"; // ground to air death 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeathGroundAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeathGroundAir";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKillGroundGround"; // ground to ground kill
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKillGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKillGroundGround";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeathGroundGround"; // ground to ground death
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeathGroundGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeathGroundGround";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineScore";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelScore";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKillAir"; // air kills
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKillAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKillAir";


$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeathAir"; // air deaths 
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeathAir";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelDeathAir";



$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgKillGround"; // ground kills
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineKillGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "satchelKillGround";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "cgDeathGround"; // ground deaths
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "discDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "hGrenadeDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "grenadeDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "laserDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mortarDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "missileDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "shockDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "plasmaDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "blasterDeathGround";
$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "mineDeathGround";

$dtStats::FV[$dtStats::FC["TG"]++,"TG"] = "null";//rng number
////////////////////////////////////////////////////////////////////////////////
//Unused vars that are not tracked but used for other things

$dtStats::unused[$dtStats::unusedCount++] = "timeToLive";
$dtStats::unused[$dtStats::unusedCount++] = "at";
$dtStats::unused[$dtStats::unusedCount++] = "gt";
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
//MapStats Vars
////////////////////////////////////////////////////////////////////////////////
//NOTE DO NOT RECOUNT!!!!// NOTE DO NOT RECOUNT!!!!// NOTE DO NOT RECOUNT!!!!
//108 stats per game type is a hard limit, change at yor own risk 

//If you need to remove a stat its best to leave a gap in the numbers or replace it
//If you need to replace a stat simply rename the stat you want remvoed with the new stat do not change array numbers 
//If you want to add a new stat just add it at the end of the list and increasing the index number by 1 from the last one 
//If you do need to recount for whatever reason, it best to do it at the start of a month as recounting will zero out stats that were not in the same postion as last time

//1
$mapStats::mapVars[1,"CTFGame"] = "scoreTG";//note this starts at 1 for... reasions 
$mapStats::mapVars[2,"CTFGame"] = "defenseScoreTG"; 
$mapStats::mapVars[3,"CTFGame"] = "offenseScoreTG"; 
//2
$mapStats::mapVars[4,"CTFGame"] = "assistTG"; 
$mapStats::mapVars[5,"CTFGame"] = "killsTG"; 
$mapStats::mapVars[6,"CTFGame"] = "teamKillsTG"; 
//3
$mapStats::mapVars[7,"CTFGame"] = "flagCapsTG"; 
$mapStats::mapVars[8,"CTFGame"] = "flagGrabsTG"; 
$mapStats::mapVars[9,"CTFGame"] = "flagReturnsTG"; 
//4
$mapStats::mapVars[10,"CTFGame"] = "carrierKillsTG"; 
$mapStats::mapVars[11,"CTFGame"] = "escortAssistsTG"; 
$mapStats::mapVars[12,"CTFGame"] = "flagDefendsTG"; 
//5
$mapStats::mapVars[13,"CTFGame"] = "heldTimeSecMin"; 
$mapStats::mapVars[14,"CTFGame"] = "grabSpeedMax"; 
$mapStats::mapVars[15,"CTFGame"] = "capEfficiencyAvg"; 
//6
$mapStats::mapVars[16,"CTFGame"] = "heldTimeSecAvgi"; 
$mapStats::mapVars[17,"CTFGame"] = "grabSpeedAvg"; 
$mapStats::mapVars[18,"CTFGame"] = "capEfficiencyAvg"; 
//7
$mapStats::mapVars[19,"CTFGame"] = "destructionTG"; 
$mapStats::mapVars[20,"CTFGame"] = "repairsTG"; 
$mapStats::mapVars[21,"CTFGame"] = "genDefendsTG"; 
//8
$mapStats::mapVars[22,"CTFGame"] = "roadKillsTG"; 
$mapStats::mapVars[23,"CTFGame"] = "vehicleScoreTG"; 
$mapStats::mapVars[24,"CTFGame"] = "bomberBombsKillsTG"; 
//9
$mapStats::mapVars[25,"CTFGame"] = "discKillsTG"; 
$mapStats::mapVars[26,"CTFGame"] = "discMATG"; 
$mapStats::mapVars[27,"CTFGame"] = "minePlusDiscTG"; 
//10
$mapStats::mapVars[28,"CTFGame"] = "laserKillsTG"; 
$mapStats::mapVars[29,"CTFGame"] = "laserHeadShotTG"; 
$mapStats::mapVars[30,"CTFGame"] = "laserHitDistMax"; 
//11
$mapStats::mapVars[31,"CTFGame"] = "shockKillsTG"; 
$mapStats::mapVars[32,"CTFGame"] = "shockRearShotTG"; 
$mapStats::mapVars[33,"CTFGame"] = "shockMATG"; 
//12
$mapStats::mapVars[34,"CTFGame"] = "plasmaKillsTG"; 
$mapStats::mapVars[35,"CTFGame"] = "plasmaMATG"; 
$mapStats::mapVars[36,"CTFGame"] = "plasmaHitDistMax"; 
//13
$mapStats::mapVars[37,"CTFGame"] = "grenadeKillsTG"; 
$mapStats::mapVars[38,"CTFGame"] = "mortarKillsTG"; 
$mapStats::mapVars[39,"CTFGame"] = "missileKillsTG"; 
//12
$mapStats::mapVars[40,"CTFGame"] = "cgKillsTG"; 
$mapStats::mapVars[41,"CTFGame"] = "cgACCAvg"; 
$mapStats::mapVars[42,"CTFGame"] = "cgHitDistMax"; 
//13
$mapStats::mapVars[43,"CTFGame"] = "blasterKillsTG"; 
$mapStats::mapVars[44,"CTFGame"] = "blasterMATG"; 
$mapStats::mapVars[45,"CTFGame"] = "blasterHitDistMax"; 
//15
$mapStats::mapVars[46,"CTFGame"] = "mineKillsTG"; 
$mapStats::mapVars[47,"CTFGame"] = "hGrenadeKillsTG"; 
$mapStats::mapVars[48,"CTFGame"] = "satchelKillsTG"; 
//16
$mapStats::mapVars[49,"CTFGame"] = "discHitDistMax"; 
$mapStats::mapVars[50,"CTFGame"] = "discMADistMAx";
$mapStats::mapVars[51,"CTFGame"] = "totalTimeTG"; 
//17
$mapStats::mapVars[52,"CTFGame"] = "InventoryDepTG"; 
$mapStats::mapVars[53,"CTFGame"] = "TurretOutdoorDepTG"; 
$mapStats::mapVars[54,"CTFGame"] = "TurretIndoorDepTG"; 

$mapStats::mapVars[55,"CTFGame"] = "MotionSensorDepTG"; 
$mapStats::mapVars[56,"CTFGame"] = "PulseSensorDepTG"; 
$mapStats::mapVars[57,"CTFGame"] = "lagSpikesTG"; 
//19
$mapStats::mapVars[58,"CTFGame"] = "airTimeTG"; 
$mapStats::mapVars[59,"CTFGame"] = "groundTimeTG"; 
$mapStats::mapVars[60,"CTFGame"] = "distMovTG"; 

$mapStats::mapVars[61,"CTFGame"] = "repairpackpickupCountTG"; 
$mapStats::mapVars[62,"CTFGame"] = "repairpackpickupEnemyTG"; 
$mapStats::mapVars[63,"CTFGame"] = "invyEatRepairPackTG"; 
$mapStats::mapVarCount["CTFGame"] = 63; 
////////////////////////////////////////////////////////////////////////////////
//1
$mapStats::mapVars[1,"SCtFGame"] = "scoreTG"; 
$mapStats::mapVars[2,"SCtFGame"] = "defenseScoreTG"; 
$mapStats::mapVars[3,"SCtFGame"] = "offenseScoreTG"; 
//2
$mapStats::mapVars[4,"SCtFGame"] = "assistTG"; 
$mapStats::mapVars[5,"SCtFGame"] = "killsTG"; 
$mapStats::mapVars[6,"SCtFGame"] = "teamKillsTG"; 
//3
$mapStats::mapVars[7,"SCtFGame"] = "flagCapsTG"; 
$mapStats::mapVars[8,"SCtFGame"] = "flagGrabsTG"; 
$mapStats::mapVars[9,"SCtFGame"] = "flagReturnsTG"; 
//4
$mapStats::mapVars[10,"SCtFGame"] = "carrierKillsTG"; 
$mapStats::mapVars[11,"SCtFGame"] = "escortAssistsTG"; 
$mapStats::mapVars[12,"SCtFGame"] = "flagDefendsTG"; 
//5
$mapStats::mapVars[13,"SCtFGame"] = "heldTimeSecMin"; 
$mapStats::mapVars[14,"SCtFGame"] = "grabSpeedMax"; 
$mapStats::mapVars[15,"SCtFGame"] = "capEfficiencyAvg"; 
//6
$mapStats::mapVars[16,"SCtFGame"] = "heldTimeSecAvgi"; 
$mapStats::mapVars[17,"SCtFGame"] = "grabSpeedAvg"; 
$mapStats::mapVars[18,"SCtFGame"] = "capEfficiencyAvg"; 
//7
$mapStats::mapVars[19,"SCtFGame"] = "discKillsTG"; 
$mapStats::mapVars[20,"SCtFGame"] = "discMATG"; 
$mapStats::mapVars[21,"SCtFGame"] = "minePlusDiscTG"; 
//8
$mapStats::mapVars[22,"SCtFGame"] = "laserKillsTG"; 
$mapStats::mapVars[23,"SCtFGame"] = "laserHeadShotTG"; 
$mapStats::mapVars[24,"SCtFGame"] = "laserHitDistMax"; 
//9
$mapStats::mapVars[25,"SCtFGame"] = "shockKillsTG"; 
$mapStats::mapVars[26,"SCtFGame"] = "shockRearShotTG"; 
$mapStats::mapVars[27,"SCtFGame"] = "shockMATG"; 
//10
$mapStats::mapVars[28,"SCtFGame"] = "plasmaKillsTG"; 
$mapStats::mapVars[29,"SCtFGame"] = "plasmaMATG"; 
$mapStats::mapVars[30,"SCtFGame"] = "plasmaHitDistMax"; 
//11
$mapStats::mapVars[31,"SCtFGame"] = "grenadeKillsTG"; 
$mapStats::mapVars[32,"SCtFGame"] = "grenadeMATG"; 
$mapStats::mapVars[33,"SCtFGame"] = "grenadeHitDistMax";
//12
$mapStats::mapVars[34,"SCtFGame"] = "cgKillsTG"; 
$mapStats::mapVars[35,"SCtFGame"] = "cgACCAvg"; 
$mapStats::mapVars[36,"SCtFGame"] = "cgHitDistMax"; 
//13
$mapStats::mapVars[37,"SCtFGame"] = "blasterHitSVMax"; 
$mapStats::mapVars[38,"SCtFGame"] = "blasterDmgTG";
$mapStats::mapVars[39,"SCtFGame"] = "blasterComTG"; 
//14
$mapStats::mapVars[40,"SCtFGame"] = "mineKillsTG"; 
$mapStats::mapVars[41,"SCtFGame"] = "mineMATG"; 
$mapStats::mapVars[42,"SCtFGame"] = "mineHitDistMax"; 
//15
$mapStats::mapVars[43,"SCtFGame"] = "hGrenadeKillsTG"; 
$mapStats::mapVars[44,"SCtFGame"] = "hGrenadeMATG"; 
$mapStats::mapVars[45,"SCtFGame"] = "hGrenadeHitDistMax"; 
$mapStats::mapVarCount["SCtFGame"] = 45;
////////////////////////////////////////////////////////////////////////////////
//1
$mapStats::mapVars[1,"LakRabbitGame"] = "scoreTG";
$mapStats::mapVars[2,"LakRabbitGame"] = "killsTG";
$mapStats::mapVars[3,"LakRabbitGame"] = "assistTG";
//2
$mapStats::mapVars[4,"LakRabbitGame"] = "flagGrabsTG";
$mapStats::mapVars[5,"LakRabbitGame"] = "MidairflagGrabsTG";
$mapStats::mapVars[6,"LakRabbitGame"] = "flagTimeMinTG";
//3
$mapStats::mapVars[7,"LakRabbitGame"] = "discKillsTG"; 
$mapStats::mapVars[8,"LakRabbitGame"] = "discMATG"; 
$mapStats::mapVars[9,"LakRabbitGame"] = "discHitDistMax";
//4
$mapStats::mapVars[10,"LakRabbitGame"] = "discDmgTG"; 
$mapStats::mapVars[11,"LakRabbitGame"] = "discCom"; 
$mapStats::mapVars[12,"LakRabbitGame"] = "minePlusDiscTG";
//5
$mapStats::mapVars[13,"LakRabbitGame"] = "shockKillsTG"; 
$mapStats::mapVars[14,"LakRabbitGame"] = "shockMATG"; 
$mapStats::mapVars[15,"LakRabbitGame"] = "shockRearShotTG"; 
//6
$mapStats::mapVars[16,"LakRabbitGame"] = "shockHitSVMax"; 
$mapStats::mapVars[17,"LakRabbitGame"] = "shockDmgTG";
$mapStats::mapVars[18,"LakRabbitGame"] = "shockComTG"; 
//7
$mapStats::mapVars[19,"LakRabbitGame"] = "plasmaKillsTG"; 
$mapStats::mapVars[20,"LakRabbitGame"] = "plasmaMATG"; 
$mapStats::mapVars[21,"LakRabbitGame"] = "plasmaHitDistMax"; 
//8
$mapStats::mapVars[22,"LakRabbitGame"] = "plasmaHitSVMax"; 
$mapStats::mapVars[23,"LakRabbitGame"] = "plasmaDmgTG";
$mapStats::mapVars[24,"LakRabbitGame"] = "plasmaComTG"; 
//9
$mapStats::mapVars[25,"LakRabbitGame"] = "grenadeKillsTG"; 
$mapStats::mapVars[26,"LakRabbitGame"] = "grenadeMATG"; 
$mapStats::mapVars[27,"LakRabbitGame"] = "grenadeHitDistMax";
//10
$mapStats::mapVars[28,"LakRabbitGame"] = "grenadeHitSVMax"; 
$mapStats::mapVars[29,"LakRabbitGame"] = "grenadeDmgTG";
$mapStats::mapVars[30,"LakRabbitGame"] = "grenadeComTG"; 
//11
$mapStats::mapVars[31,"LakRabbitGame"] = "blasterKillsTG"; 
$mapStats::mapVars[32,"LakRabbitGame"] = "blasterMATG"; 
$mapStats::mapVars[33,"LakRabbitGame"] = "blasterHitDistMax";
//12
$mapStats::mapVars[34,"LakRabbitGame"] = "blasterHitSVMax"; 
$mapStats::mapVars[35,"LakRabbitGame"] = "blasterDmgTG";
$mapStats::mapVars[36,"LakRabbitGame"] = "blasterComTG"; 
//13
$mapStats::mapVars[37,"LakRabbitGame"] = "mineKillsTG"; 
$mapStats::mapVars[38,"LakRabbitGame"] = "mineMATG"; 
$mapStats::mapVars[39,"LakRabbitGame"] = "mineHitDistMax"; 
//14
$mapStats::mapVars[40,"LakRabbitGame"] = "mineHitVVMax"; 
$mapStats::mapVars[41,"LakRabbitGame"] = "mineDmgTG";
$mapStats::mapVars[42,"LakRabbitGame"] = "mineComTG"; 
//15
$mapStats::mapVars[43,"LakRabbitGame"] = "hGrenadeKillsTG"; 
$mapStats::mapVars[44,"LakRabbitGame"] = "hGrenadeMATG"; 
$mapStats::mapVars[45,"LakRabbitGame"] = "hGrenadeHitDistMax"; 
//16
$mapStats::mapVars[46,"LakRabbitGame"] = "hGrenadeHitSVMax"; 
$mapStats::mapVars[47,"LakRabbitGame"] = "hGrenadeDmgTG";
$mapStats::mapVars[48,"LakRabbitGame"] = "hGrenadeComTG"; 
$mapStats::mapVarCount["LakRabbitGame"] = 48;
////////////////////////////////////////////////////////////////////////////////
//1
$mapStats::mapVars[1,"DMGame"] = "scoreTG";
$mapStats::mapVars[2,"DMGame"] = "killsTG";
$mapStats::mapVars[3,"DMGame"] = "assistTG";
//2
$mapStats::mapVars[4,"DMGame"] = "efficiencyAvg";
$mapStats::mapVars[5,"DMGame"] = "timeTLAvg";
$mapStats::mapVars[6,"DMGame"] = "distMovTG";
//3
$mapStats::mapVars[7,"DMGame"] = "killAirTG";
$mapStats::mapVars[8,"DMGame"] = "killGroundTG";
$mapStats::mapVars[9,"DMGame"] = "EVKillsTG";
//4
$mapStats::mapVars[10,"DMGame"] = "firstKillTG";
$mapStats::mapVars[11,"DMGame"] = "lastKillTG";
$mapStats::mapVars[12,"DMGame"] = "deathKillsTG";
//5
$mapStats::mapVars[13,"DMGame"] = "doubleChainKillTG";
$mapStats::mapVars[14,"DMGame"] = "tripleChainKillTG";
$mapStats::mapVars[15,"DMGame"] = "quadrupleChainKillTG";
//6
$mapStats::mapVars[16,"DMGame"] = "killStreakMax";
$mapStats::mapVars[17,"DMGame"] = "comboCountTG";
$mapStats::mapVars[18,"DMGame"] = "kdrAvg";
//7
$mapStats::mapVars[19,"DMGame"] = "discKillsTG"; 
$mapStats::mapVars[20,"DMGame"] = "discMATG"; 
$mapStats::mapVars[21,"DMGame"] = "minePlusDiscTG"; 
//8
$mapStats::mapVars[22,"DMGame"] = "plasmaKillsTG"; 
$mapStats::mapVars[23,"DMGame"] = "plasmaMATG"; 
$mapStats::mapVars[24,"DMGame"] = "plasmaHitDistMax";
//9
$mapStats::mapVars[25,"DMGame"] = "grenadeKillsTG"; 
$mapStats::mapVars[26,"DMGame"] = "grenadeMATG"; 
$mapStats::mapVars[27,"DMGame"] = "grenadeHitDistMax";
//10
$mapStats::mapVars[28,"DMGame"] = "laserKillsTG"; 
$mapStats::mapVars[29,"DMGame"] = "laserHeadShotTG"; 
$mapStats::mapVars[30,"DMGame"] = "laserHitDistMax"; 
//11
$mapStats::mapVars[31,"DMGame"] = "shockKillsTG"; 
$mapStats::mapVars[32,"DMGame"] = "shockRearShotTG"; 
$mapStats::mapVars[33,"DMGame"] = "shockMATG";  
//12
$mapStats::mapVars[34,"DMGame"] = "mortarKillsTG"; 
$mapStats::mapVars[35,"DMGame"] = "mortarMATG"; 
$mapStats::mapVars[36,"DMGame"] = "mortarHitDistMax";
//13
$mapStats::mapVars[37,"DMGame"] = "cgKillsTG"; 
$mapStats::mapVars[38,"DMGame"] = "cgACCAvg"; 
$mapStats::mapVars[39,"DMGame"] = "cgHitDistMax"; 
//14
$mapStats::mapVars[40,"DMGame"] = "blasterKillsTG"; 
$mapStats::mapVars[41,"DMGame"] = "blasterMATG"; 
$mapStats::mapVars[42,"DMGame"] = "blasterHitDistMax"; 
//15
$mapStats::mapVars[43,"DMGame"] = "mineKillsTG"; 
$mapStats::mapVars[44,"DMGame"] = "mineMATG"; 
$mapStats::mapVars[45,"DMGame"] = "mineHitDistMax"; 
//16
$mapStats::mapVars[46,"DMGame"] = "hGrenadeKillsTG"; 
$mapStats::mapVars[47,"DMGame"] = "hGrenadeMATG"; 
$mapStats::mapVars[48,"DMGame"] = "hGrenadeHitDistMax"; 
$mapStats::mapVarCount["DMGame"] = 48;
////////////////////////////////////////////////////////////////////////////////
//1
$mapStats::mapVars[1,"DuelGame"] = "scoreTG";
$mapStats::mapVars[2,"DuelGame"] = "killsTG";
$mapStats::mapVars[3,"DuelGame"] = "deathsTG";
//2
$mapStats::mapVars[4,"DuelGame"] = "killAirTG";
$mapStats::mapVars[5,"DuelGame"] = "deathAirTG";
$mapStats::mapVars[6,"DuelGame"] = "airTimeAvg";
//3
$mapStats::mapVars[7,"DuelGame"] = "killGroundTG";
$mapStats::mapVars[8,"DuelGame"] = "deathGroundTG";
$mapStats::mapVars[9,"DuelGame"] = "groundTimeAvg";
//4
$mapStats::mapVars[10,"DuelGame"] = "kdrAvg";
$mapStats::mapVars[11,"DuelGame"] = "EVKillsTG";
$mapStats::mapVars[12,"DuelGame"] = "comboCountTG";
//5
$mapStats::mapVars[13,"DuelGame"] = "distMovTG";
$mapStats::mapVars[14,"DuelGame"] = "maxSpeedMax";
$mapStats::mapVars[15,"DuelGame"] = "timeTLAvg";
//6
$mapStats::mapVars[16,"DuelGame"] = "discKillsTG"; 
$mapStats::mapVars[17,"DuelGame"] = "discMATG"; 
$mapStats::mapVars[18,"DuelGame"] = "minePlusDiscTG"; 
//7
$mapStats::mapVars[19,"DuelGame"] = "plasmaKillsTG"; 
$mapStats::mapVars[20,"DuelGame"] = "plasmaMATG"; 
$mapStats::mapVars[21,"DuelGame"] = "plasmaHitDistMax";
//8
$mapStats::mapVars[22,"DuelGame"] = "grenadeKillsTG"; 
$mapStats::mapVars[23,"DuelGame"] = "grenadeMATG"; 
$mapStats::mapVars[24,"DuelGame"] = "grenadeHitDistMax";
//9
$mapStats::mapVars[25,"DuelGame"] = "laserKillsTG"; 
$mapStats::mapVars[26,"DuelGame"] = "laserHeadShotTG"; 
$mapStats::mapVars[27,"DuelGame"] = "laserHitDistMax"; 
//10
$mapStats::mapVars[28,"DuelGame"] = "shockKillsTG"; 
$mapStats::mapVars[29,"DuelGame"] = "shockRearShotTG"; 
$mapStats::mapVars[30,"DuelGame"] = "shockMATG";  
//11
$mapStats::mapVars[31,"DuelGame"] = "mortarKillsTG"; 
$mapStats::mapVars[32,"DuelGame"] = "mortarMATG"; 
$mapStats::mapVars[33,"DuelGame"] = "mortarHitDistMax";
//12
$mapStats::mapVars[34,"DuelGame"] = "cgKillsTG"; 
$mapStats::mapVars[35,"DuelGame"] = "cgACCAvg"; 
$mapStats::mapVars[36,"DuelGame"] = "cgHitDistMax"; 
//13
$mapStats::mapVars[37,"DuelGame"] = "blasterKillsTG"; 
$mapStats::mapVars[38,"DuelGame"] = "blasterMATG"; 
$mapStats::mapVars[39,"DuelGame"] = "blasterHitDistMax"; 
//14
$mapStats::mapVars[40,"DuelGame"] = "mineKillsTG"; 
$mapStats::mapVars[41,"DuelGame"] = "mineMATG"; 
$mapStats::mapVars[42,"DuelGame"] = "mineHitDistMax"; 
//15
$mapStats::mapVars[43,"DuelGame"] = "hGrenadeKillsTG"; 
$mapStats::mapVars[44,"DuelGame"] = "hGrenadeMATG"; 
$mapStats::mapVars[45,"DuelGame"] = "hGrenadeHitDistMax"; 
$mapStats::mapVarCount["DuelGame"] = 45;
////////////////////////////////////////////////////////////////////////////////
//1
$mapStats::mapVars[1,"ArenaGame"] = "scoreTG";
$mapStats::mapVars[2,"ArenaGame"] = "roundKillsTG";
$mapStats::mapVars[3,"ArenaGame"] = "assistTG";
//2
$mapStats::mapVars[4,"ArenaGame"] = "roundsWonTG";
$mapStats::mapVars[5,"ArenaGame"] = "teamKillsTG";
$mapStats::mapVars[61,"ArenaGame"] = "hatTricksTG";
//3
$mapStats::mapVars[7,"ArenaGame"] = "maxSpeedMax";
$mapStats::mapVars[8,"ArenaGame"] = "timeTLAvg";
$mapStats::mapVars[9,"ArenaGame"] = "distMovTG";
//4
$mapStats::mapVars[10,"ArenaGame"] = "killAirTG";
$mapStats::mapVars[11,"ArenaGame"] = "killGroundTG";
$mapStats::mapVars[12,"ArenaGame"] = "EVKillsTG";
//5
$mapStats::mapVars[13,"ArenaGame"] = "firstKillTG";
$mapStats::mapVars[14,"ArenaGame"] = "lastKillTG";
$mapStats::mapVars[15,"ArenaGame"] = "deathKillsTG";
//6
$mapStats::mapVars[16,"ArenaGame"] = "killStreakMax";
$mapStats::mapVars[17,"ArenaGame"] = "comboCountTG";
$mapStats::mapVars[18,"ArenaGame"] = "kdrAvg";
//7
$mapStats::mapVars[19,"ArenaGame"] = "discKillsTG"; 
$mapStats::mapVars[20,"ArenaGame"] = "discMATG"; 
$mapStats::mapVars[21,"ArenaGame"] = "minePlusDiscTG"; 
//8
$mapStats::mapVars[22,"ArenaGame"] = "plasmaKillsTG"; 
$mapStats::mapVars[23,"ArenaGame"] = "plasmaMATG"; 
$mapStats::mapVars[24,"ArenaGame"] = "plasmaHitDistMax";
//9
$mapStats::mapVars[25,"ArenaGame"] = "grenadeKillsTG"; 
$mapStats::mapVars[26,"ArenaGame"] = "grenadeMATG"; 
$mapStats::mapVars[27,"ArenaGame"] = "grenadeHitDistMax";
//10
$mapStats::mapVars[28,"ArenaGame"] = "laserKillsTG"; 
$mapStats::mapVars[29,"ArenaGame"] = "laserHeadShotTG"; 
$mapStats::mapVars[30,"ArenaGame"] = "laserHitDistMax"; 
//11
$mapStats::mapVars[31,"ArenaGame"] = "shockKillsTG"; 
$mapStats::mapVars[32,"ArenaGame"] = "shockRearShotTG"; 
$mapStats::mapVars[33,"ArenaGame"] = "shockMATG";  
//12
$mapStats::mapVars[34,"ArenaGame"] = "mortarKillsTG"; 
$mapStats::mapVars[35,"ArenaGame"] = "mortarMATG"; 
$mapStats::mapVars[36,"ArenaGame"] = "mortarHitDistMax";
//13
$mapStats::mapVars[37,"ArenaGame"] = "cgKillsTG"; 
$mapStats::mapVars[38,"ArenaGame"] = "cgACCAvg"; 
$mapStats::mapVars[39,"ArenaGame"] = "cgHitDistMax"; 
//14
$mapStats::mapVars[40,"ArenaGame"] = "blasterKillsTG"; 
$mapStats::mapVars[41,"ArenaGame"] = "blasterMATG"; 
$mapStats::mapVars[42,"ArenaGame"] = "blasterHitDistMax"; 
//15
$mapStats::mapVars[43,"ArenaGame"] = "mineKillsTG"; 
$mapStats::mapVars[44,"ArenaGame"] = "mineMATG"; 
$mapStats::mapVars[45,"ArenaGame"] = "mineHitDistMax"; 
//16
$mapStats::mapVars[46,"ArenaGame"] = "hGrenadeKillsTG"; 
$mapStats::mapVars[47,"ArenaGame"] = "hGrenadeMATG"; 
$mapStats::mapVars[48,"ArenaGame"] = "hGrenadeHitDistMax"; 
$mapStats::mapVarCount["ArenaGame"] = 48;
   

   
if(!$dtStats::Enable){return;} // abort exec
if(!isObject(statsGroup)){
   new SimGroup(statsGroup);
   RootGroup.add(statsGroup); 
   statsGroup.resetCount = -1;
   statsGroup.serverStart = 0;
   $dtStats::leftID++;
}

function dtAICON(%client){ 
   dtStatsMissionDropReady(Game.getId(), %client);
}
package dtStats{
   function AIConnection::startMission(%client){// ai support
      parent::startMission(%client);
      if($dtStats::Enable)
         schedule(25000,0,"dtAICON",%client);
   }
   function GameConnection::onDrop(%client, %reason){
      if($dtStats::Enable)
         dtStatsClientLeaveGame(%client);//common
      parent::onDrop(%client, %reason);
   }
   function CTFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
       if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function CTFGame::gameOver( %game ){
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   } 
   function CTFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if($dtStats::Enable)
         clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function CTFGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      if($dtStats::Enable)
         dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
      else
         parent::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function CTFGame::updateScoreHud(%game, %client, %tag){
      if($dtStats::Enable || %client.isSuperAdmin)
         CTFHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function LakRabbitGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if($dtStats::Enable)
         clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function LakRabbitGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function LakRabbitGame::gameOver( %game ){
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function LakRabbitGame::recalcScore(%game, %client){
      if($missionRunning){
         parent::recalcScore(%game, %client);
      }
   }
   function LakRabbitGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      if($dtStats::Enable)
         dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
      else
         parent::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function LakRabbitGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      if($dtStats::Enable || %client.isSuperAdmin)
         LakRabbitHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function DMGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if($dtStats::Enable)
         clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function DMGame::clientMissionDropReady(%game, %client){ // called when client has finished loading
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function DMGame::gameOver( %game ){
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function DMGame::recalcScore(%game, %client){
	  if(!$missionRunning){
         return;  
      }
      parent::recalcScore(%game, %client);
   }
   function DMGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      if($dtStats::Enable)
         dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
      else
         parent::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function DMGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      if($dtStats::Enable || %client.isSuperAdmin)
         DMHud(%game, %client, %tag); 
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function SCtFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function SCtFGame::gameOver( %game ){
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function SCtFGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if($dtStats::Enable)
         clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function SCtFGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      if($dtStats::Enable)
         dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
      else
         parent::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function SCtFGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      if($dtStats::Enable || %client.isSuperAdmin)
         CTFHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   /////////////////////////////////////////////////////////////////////////////////////
   function ArenaGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function ArenaGame::gameOver( %game ){
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function ArenaGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if($dtStats::Enable)
         clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function ArenaGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      if($dtStats::Enable)
         dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
      else
         parent::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }
   function ArenaGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      if($dtStats::Enable || %client.isSuperAdmin) 
         ArenaHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   /////////////////////////////////////////////////////////////////////////////
   function DuelGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function DuelGame::gameOver( %game ){
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
      parent::gameOver(%game);
   }
   function DuelGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      if($dtStats::Enable)
         clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation);//for stats collection
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
   }
   function DuelGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      if($dtStats::Enable)
         dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
      else
         parent::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5);
   }   
   function DuelGame::updateScoreHud(%game, %client, %tag){
      if($dtStats::Enable || %client.isSuperAdmin)
         DuelHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   ///////////////////////////////////////////////////////////////////////////////
   function DefaultGame::missionLoadDone(%game){
      parent::missionLoadDone(%game);
      if($dtStats::Enable){
         dtSaveServerVars();
         dtScanForRepair();
         $mapID::gameID = addNum($mapID::gameID,1); 
         if($dtStats::debugEchos)
            error("GAME ID" SPC $mapID::gameID SPC "//////////////////////////////");
         export( "$mapID::*", "serverStats/mapIDList.cs", false );
      }
   }
   function DefaultGame::forceObserver( %game, %client, %reason ){
      parent::forceObserver( %game, %client, %reason );
      if($dtStats::Enable){
         if(%reason $= "spawnTimeout"){
            %client.dtStats.spawnobstimeoutCount++; 
         }
         %client.gt = %client.at = 0;//air time ground time reset   
      }
   }
   function serverCmdShowHud(%client, %tag){ // to refresh screen when client opens it up
      parent::serverCmdShowHud(%client, %tag);
      if($dtStats::Enable){
         %tagName = getWord(%tag, 1);
         %tag = getWord(%tag, 0);
         if(%tag $= 'scoreScreen' && %client.viewStats){
            statsMenu(%client,Game.class);
         }
      }
   }
   function chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 ){
      parent::chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
	   if($dtStats::Enable){ 
         %sender.dtStats.chatallCount++;
	   }
    }
    function cannedChatMessageAll( %sender, %msgString, %name, %string, %keys ){
      parent::cannedChatMessageAll( %sender, %msgString, %name, %string, %keys );
      if($dtStats::Enable){ 
         %sender.dtStats.voicebindsallCount++;
      }
   }
	function chatMessageTeam( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 ){
      parent::chatMessageTeam( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
	   if($dtStats::Enable){
         %sender.dtStats.chatteamCount++;
	   }
    }
    function cannedChatMessageTeam( %sender, %team, %msgString, %name, %string, %keys ){
      parent::cannedChatMessageTeam( %sender, %team, %msgString, %name, %string, %keys );
      if($dtStats::Enable){ 
         %sender.dtStats.voicebindsteamCount++;
      }
   }
	function kick( %client, %admin, %guid ){
      if($dtStats::Enable)
         %client.dtStats.kickCount++;
      parent::kick( %client, %admin, %guid );
   }
   function cmdAutoKickObserver(%client, %key){ // Edit GG
      parent::cmdAutoKickObserver(%client, %key);
	   if($dtStats::Enable)
		   %client.dtStats.obstimeoutkickCount++;
   }
   
   function CTFGame::leaveMissionArea(%game, %playerData, %player){
	   parent::leaveMissionArea(%game, %playerData, %player);
	   if($dtStats::Enable)
	       %player.client.dtStats.leavemissionareaCount++;
   }
   function SCtFGame::leaveMissionArea(%game, %playerData, %player){
	   parent::leaveMissionArea(%game, %playerData, %player);
	   if($dtStats::Enable)
	       %player.client.dtStats.leavemissionareaCount++;
   }
   function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs, %respawned){ // z0dd - ZOD, 6/06/02. Don't send a message if player used respawn feature. Added %respawned
      parent::clientChangeTeam(%game, %client, %team, %fromObs, %respawned);
	   if($dtStats::Enable)
	       %client.dtStats.switchteamCount++;
   }
   function RepairPack::onThrow(%data,%obj,%shape){
      parent::onThrow(%data,%obj,%shape);
      if($dtStats::Enable){
           %obj.team = %shape.client.team;
           %player.dtRepairPickup = 0;
      }
   }
   function ItemData::onPickup(%this, %pack, %player, %amount){
      parent::onPickup(%this, %pack, %player, %amount);
      if($dtStats::Enable){
         %dtStats = %player.client.dtStats;
         if(%this.getname() $= "RepairPack"){
            if(%pack.team > 0 && %pack.team != %player.client.team)
               %dtStats.repairpackpickupEnemy++;
            %dtStats.repairpackpickupCount++;
            %player.dtRepairPickup = 1;
         }
         %dtStats.packpickupCount++;
		}
   }
   function stationTrigger::onLeaveTrigger(%data, %obj, %colObj){
      if($dtStats::Enable){
         if(isObject(%obj.station)){
            %name = %obj.station.getDataBlock().getName();
            if(%name $= "DeployedStationInventory" || %name $= "StationInventory"){
               if(%colObj.getMountedImage(2) > 0){
                  if(%colObj.getMountedImage(2).getName() !$= "RepairPackImage" && %colObj.dtRepairPickup){
                     %colObj.client.dtStats.invyEatRepairPack++;
                  }
               }
               %player.dtRepairPickup = 0;
            }
         }
      }
      parent::onLeaveTrigger(%data, %obj, %colObj);
   }
   function Weapon::onPickup(%this, %obj, %shape, %amount){
		parent::onPickup(%this, %obj, %shape, %amount);
	    if($dtStats::Enable)
	       %shape.client.dtStats.weaponpickupCount++;
   }
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
   // Flag Escort Fixes
   function CTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){ 
       parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
       if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
       %clAttacker.dmgdFlagTime = getSimTime();  
	}
	function CTFGame::testEscortAssist(%game, %victimID, %killerID){
	   if((getSimTime() - %victimID.dmgdFlagTime) < %game.TIME_CONSIDERED_FLAGCARRIER_THREAT && %killerID.player.holdingFlag $= "")
		  return true;
	   return false;
	}
	function SCtFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc){ 
	   parent::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
	   if ((%clVictim.player.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
		  %clAttacker.dmgdFlagTime = getSimTime();  
	}
	function SCtFGame::testEscortAssist(%game, %victimID, %killerID){
	   if((getSimTime() - %victimID.dmgdFlagTime) < %game.TIME_CONSIDERED_FLAGCARRIER_THREAT && %killerID.player.holdingFlag $= "")
		  return true;
	   return false;
	}
   function ProjectileData::onExplode(%data, %proj, %pos, %mod){
      if($dtStats::Enable)
         dtOnExplode(%data, %proj, %pos, %mod);
      parent::onExplode(%data, %proj, %pos, %mod);
   }
   //function MineDeployed::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType){
      //parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType);
      //if($DamageType::Disc && $dtStats::Enable){
         //%sourceObject.client.dtStats.mineDiscPct = (%sourceObject.client.dtStats.mineDiscHit++   / (%thrower.client.dtStats.mineShotsFired ? %thrower.client.dtStats.mineShotsFired : 1)) * 100;
         //%sourceObject.client.dtStats.mineDiscAcc = (%sourceObject.client.dtStats.mineDiscShots   / (%thrower.client.dtStats.mineShotsFired ? %thrower.client.dtStats.mineShotsFired : 1)) * 100;
         //%sourceObject.client.dtStats.mineDiscAccMP = (%sourceObject.client.dtStats.minePlusDisc   / (%thrower.client.dtStats.mineShotsFired ? %thrower.client.dtStats.mineShotsFired : 1)) * 100;
//
      //}
   //}
   function ShapeBaseImageData::onDeploy(%item, %plyr, %slot){
      %obj = parent::onDeploy(%item, %plyr, %slot);
      if($dtStats::Enable){
         %dtStats = %plyr.client.dtStats;
         %itemDB = %item.item;
         switch$(%itemDB){
            case "MotionSensorDeployable":
               %dtStats.MotionSensorDep++;
            case "PulseSensorDeployable":
               %dtStats.PulseSensorDep++;
            case "InventoryDeployable":
               %dtStats.InventoryDep++;
            case "TurretOutdoorDeployable":
               %dtStats.TurretOutdoorDep++;
            case "TurretIndoorDeployable":
               %dtStats.TurretIndoorDep++;
         }
      }
      return %obj;
   }
   //function TurretData::replaceCallback(%this, %turret, %engineer){
      //parent::replaceCallback(%this, %turret, %engineer);
      //if (%engineer.getMountedImage($BackPackSlot) != 0 && $dtStats::Enable){
         //%dtStats = %engineer.client.dtStats;
         //%barrel = %engineer.getMountedImage($BackPackSlot).turretBarrel;
         //switch$(%barrel){
            //case "ELFBarrelPack":
               //%dtStats.ELFBarrelDep++;
            //case "MortarBarrelPack":
               //%dtStats.MortarBarrelDep++;
            //case "PlasmaBarrelPack":
               //%dtStats.PlasmaBarrelDep++;
            //case "AABarrelPack":
               //%dtStats.AABarrelDep++;
            //case "MissileBarrelPack":
               //%dtStats.MissileBarrelDep++;
        //}
      //}
   //}
};
//helps with game types that override functions and dont use parent
// that way we get called first then the gametype can do whatever 
package dtStatsGame{
      function FlipFlop::playerTouch(%data, %flipflop, %player){
	      parent::playerTouch(%data, %flipflop, %player);
	      if($dtStats::Enable)
	         %player.client.dtStats.flipflopCount++;
      }
      function serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %teamSpecific, %msg){
	      parent::serverCmdStartNewVote(%client, %typeName, %arg1, %arg2, %arg3, %arg4, %teamSpecific, %msg);
	      if($dtStats::Enable){ 
	         %client.dtStats.voteCount++;
	         if(%typeName $= "VoteChangeMission"){
	            %mission = $HostMissionFile[%arg3];
               %missionType = $HostTypeName[%arg4] @ "Game";
               %map = cleanMapName(%mission);
               $dtServer::voteFor[%map,%missionType]++;
               getMapID(%map,%missionType,0,0);
	         }
	      }
      }
     function detonateGrenade(%obj){// from lakRabbitGame.cs for grenade tracking      
      if($dtStats::Enable){
         %obj.dtNade = 1;
         $dtObjExplode = %obj;
          %obj.sourceObject.client.dtShotSpeed = mFloor(vectorLen(%obj.sourceObject.getVelocity()) * 3.6);
      }
      parent::detonateGrenade(%obj);
   } 
   function MineDeployed::onThrow(%this, %mine, %thrower){
       parent::onThrow(%this, %mine, %thrower);
       if($dtStats::Enable){
          %thrower.client.lastMineThrow = getSimTime();
          %thrower.client.dtStats.mineShotsFired++; 
          %thrower.client.dtStats.shotsFired++;
          %thrower.client.dtStats.mineACC = (%thrower.client.dtStats.mineHits / %thrower.client.dtStats.mineShotsFired) * 100;
      }
   }
   function SatchelChargeTossed::onThrow(%this, %sat, %thrower){
      parent::onThrow(%this, %sat, %thrower);
      if($dtStats::Enable){
          %thrower.client.dtStats.satchelShotsFired++;
          %thrower.client.dtStats.shotsFired++;
          %thrower.client.dtStats.satchelACC = (%thrower.client.dtStats.satchelHits / %thrower.client.dtStats.satchelShotsFired) * 100;
      }
   }
   function GrenadeThrown::onThrow(%this, %gren,%thrower){
       parent::onThrow(%this, %gren);
       if($dtStats::Enable){
          %thrower.client.dtStats.hGrenadeShotsFired++;
          %thrower.client.dtStats.shotsFired++;
          %thrower.client.dtStats.hGrenadeACC = (%thrower.client.dtStats.hGrenadeHits / %thrower.client.dtStats.hGrenadeShotsFired) * 100;
       }
   }
   function ShapeBaseImageData::onFire(%data, %obj, %slot){
      %p = parent::onFire(%data, %obj, %slot);
      if($dtStats::Enable){
         if(isObject(%p)){
            clientShotsFired(%data.projectile, %obj, %p);
         }
      }
      return %p;
   }
   function Armor::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC){
      if($dtStats::Enable)
         clientDmgStats(%data,%position,%sourceObject,%targetObject, %damageType,%amount);
      parent::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %mineSC);
   }
   //0 Fire 1 ??? 2 jump 3 jet 4 gernade 5 mine
   function Armor::onTrigger(%data, %player, %triggerNum, %val){
      parent::onTrigger(%data, %player, %triggerNum, %val);
      if($dtStats::Enable){
         if(isObject(%player) && !%player.getObjectMount()){
            if(%val){//cut the amount of tiggers in half 
               %client = %player.client;
               %client.dtStats.onInput++;
//------------------------------------------------------------------------------               
               if(%triggerNum == 0){
                  %tPos = %player.getMuzzlePoint(0);
                  %hit = containerRayCast(%tPos, VectorAdd(%tPos, VectorScale(%player.getMuzzleVector(0), 5000)), $TypeMasks::PlayerObjectType, %player);              
                  if(%hit)
                     %client.dtStats.onTargetHit++; 
                  else
                     %client.dtStats.onTargetMis++;  
                  %client.dtStats.onFire++;
                  %client.dtStats.onTargetAcc =  (%client.dtStats.onTargetHit / (%client.dtStats.onFire ? %client.dtStats.onFire : 1)) * 100;
                  %client.dtStats.onTargetHMR=  (%client.dtStats.onTargetHit / (%client.dtStats.onTargetMis ? %client.dtStats.onTargetMis : 1)) * 100;
                  //error(%client.dtStats.onTargetAcc SPC %hit);
               }  
//------------------------------------------------------------------------------                   
               %speed = mFloor(vectorLen(%player.getVelocity()) * 3.6);
               if(%speed > %client.dtStats.maxSpeed){%client.dtStats.maxSpeed = %speed;}
               %client.dtStats.avgTSpeed += %speed; %client.dtStats.avgSpeedCount++;
               %client.dtStats.avgSpeed = %client.dtStats.avgTSpeed/%client.dtStats.avgSpeedCount;
               if(%client.dtStats.avgSpeedCount >= 500){%client.dtStats.avgSpeedCount=%client.dtStats.avgTSpeed=0;}   
//------------------------------------------------------------------------------                    
               %xypos = getWords(%player.getPosition(),0,1) SPC 0;
               if(%client.lp !$= ""){
                  %dis = mFloor(vectorDist(%client.lp,%xypos));
                  %client.dtStats.distMov = %client.dtStats.distMov + (%dis/1000);
               }
               %client.lp = %xypos;
//------------------------------------------------------------------------------                       
            }
            if (%triggerNum == 3){ //jet triggers 
               if(%val){
                  if(isEventPending(%player.jetTimeTest)){
                     cancel(%player.jetTimeTest);
                  }
                   %client.jetTrigCount++;
                  if(%client.ground){
                     if(%client.gt > 0){
                        %client.dtStats.groundTime += ((getSimTime() - %client.gt)/1000)/60;
                     }
                     %client.at =  getSimTime();
                  }
                  %client.ground = 0;
               }
               else{
                   if(!isEventPending(%player.jetTimeTest)){
                     %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
                     %rayStart = %player.getWorldBoxCenter();
                     %rayEnd = VectorAdd(%rayStart,"0 0" SPC (10000 * -1));
                     %raycast = ContainerRayCast(%rayStart, %rayEnd, %mask, %player);  
                     %groundPos = getWords(%raycast, 1, 3);
                     %dis = vectorDist(%player.getPosition(),%groundPos);
                     %zv = getWord(%player.getVelocity(),2);
                     %time = (((%zv + mSqrt(mPow((%zv),2) + 2 * mAbs(getGravity()) * %dis)) / mAbs(getGravity()))* 1000);
                     %player.jetTimeTest = schedule(%time,0,"chkGrounded",%player);
                  }  
               }
            }
         }
         else{
            %client.lp = "";
            %client.gt = %client.at = 0; 
         }
      }
   }
   function StaticShapeData::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType){
      if($dtStats::Enable)
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
      if($dtStats::Enable)
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
      if($dtStats::Enable)
         clientShotsFired(ShockLanceImage.projectile, %obj, %p);
      return %p;
   }
   function Armor::onMount(%this,%obj,%vehicle,%node){
      parent::onMount(%this,%obj,%vehicle,%node);
      if($dtStats::Enable){
         %obj.client.vehDBName = %vehicle.getDataBlock().getName();
         %obj.client.gt = %obj.client.at = 0;// resets fly/ground time
      }
   }
   ////////////////////////////////////////////////////////////////////////////////
   function CTFGame::playerDroppedFlag(%game, %player){
      if($dtStats::Enable){
         %flag = %player.holdingFlag;
         %game.dtTotalFlagTime[%flag] = 0;
      }
      parent::playerDroppedFlag(%game, %player);
   }
   function CTFGame::boundaryLoseFlag(%game, %player){
     if($dtStats::Enable){
         %flag = %player.holdingFlag;
         %game.dtTotalFlagTime[%flag] = 0;
     }
     parent::boundaryLoseFlag(%game, %player); 
   }
   function CTFGame::playerTouchEnemyFlag(%game, %player, %flag){
      if($dtStats::Enable){
         if(%flag.isHome){
            %game.dtTotalFlagTime[%flag] = getSimTime();
         }
         if(!%player.flagStatsWait){
            %grabspeed = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);
            if(%grabSpeed > %player.client.dtStats.grabSpeed){
               if($TeamRank[2,"count"] > 5 && $TeamRank[1,"count"] > 5)
                  %player.client.dtStats.grabSpeed  = %grabSpeed;
            }
         }  
      }
      parent::playerTouchEnemyFlag(%game, %player, %flag);
   }
   function CTFGame::flagCap(%game, %player){
      if($dtStats::Enable){
         %flag = %player.holdingFlag;
         if(%game.dtTotalFlagTime[%flag]){
            %heldTime = (getSimTime() - %game.dtTotalFlagTime[%flag])/1000;
            if(%heldTime < %player.client.dtStats.heldTimeSec || !%player.client.dtStats.heldTimeSec){
                  if($TeamRank[2,"count"] > 5 && $TeamRank[1,"count"] > 5)
                     %player.client.dtStats.heldTimeSec  = %heldTime;
            }
         }
      }
      parent::flagCap(%game, %player);
   }
/////////////////////////////////////////////////////////////////////////////   
   function SCtFGame::playerDroppedFlag(%game, %player){
      if($dtStats::Enable){
         %flag = %player.holdingFlag;
         %game.dtTotalFlagTime[%flag] = 0;
      }
      parent::playerDroppedFlag(%game, %player);
   }
   function SCtFGame::boundaryLoseFlag(%game, %player){
     if($dtStats::Enable){
         %flag = %player.holdingFlag;
         %game.dtTotalFlagTime[%flag] = 0;
     }
     parent::boundaryLoseFlag(%game, %player); 
   }
   function SCtFGame::playerTouchEnemyFlag(%game, %player, %flag){
      if($dtStats::Enable){
         if(%flag.isHome){
            %game.dtTotalFlagTime[%flag] = getSimTime();
         }
         if(!%player.flagStatsWait){
            %grabspeed = mFloor(VectorLen(setWord(%player.getVelocity(), 2, 0)) * 3.6);
            if(%grabSpeed > %player.client.dtStats.grabSpeed){
               if($TeamRank[2,"count"] > 5 && $TeamRank[1,"count"] > 5)
                  %player.client.dtStats.grabSpeed  = %grabSpeed;
            }
         }  
      }
      parent::playerTouchEnemyFlag(%game, %player, %flag);
   }
   function SCtFGame::flagCap(%game, %player){
      if($dtStats::Enable){
         %flag = %player.holdingFlag;
         if(%game.dtTotalFlagTime[%flag]){
            %heldTime = (getSimTime() - %game.dtTotalFlagTime[%flag])/1000;
            if(%heldTime < %player.client.dtStats.heldTimeSec || !%player.client.dtStats.heldTimeSec){
               if($TeamRank[2,"count"] > 5 && $TeamRank[1,"count"] > 5)
                  %player.client.dtStats.heldTimeSec  = %heldTime;
            }
         }
      }
      parent::flagCap(%game, %player);
   }
};

function chkGrounded(%player){
   if(isObject(%player)){
      %client =  %player.client;
      if(!%client.ground){
         if(%client.at > 0){
            %client.dtStats.airTime += ((getSimTime() - %client.at)/1000)/60;
         }
         %client.gt =  getSimTime();
      }
      %client.ground = 1;
      %player.jetTimeTest = 0;
   }
 // error(%client.airTime SPC %client.groundTime);
}
function dtScanForRepair(){
   InitContainerRadiusSearch("0 0 0",  9000, $TypeMasks::ItemObjectType);
   while ((%itemObj = containerSearchNext()) != 0){
      if(%itemObj.getDatablock().getName() $= "RepairPack"){
         %repairList[%c++] = %itemObj;
      }
   }
  for(%i = 1; %i <= %c; %i++){   
     %itemObj = %repairList[%i];
      InitContainerRadiusSearch("0 0 0",  9000, $TypeMasks::ItemObjectType | $TypeMasks::StationObjectType | $TypeMasks::SensorObjectType | $TypeMasks::GeneratorObjectType  | $TypeMasks::TurretObjectType);           //| $TypeMasks::PlayerObjectType
      %disMin = 0; 
      while ((%teamObj = containerSearchNext()) != 0){
         if(%teamObj.getType() & $TypeMasks::ItemObjectType && %teamObj.team == 0)
            continue;
         if(%teamObj.team > -1){
            %dis  = vectorDist(%itemObj.getPosition(),%teamObj.getPosition());
            if(%dis < %disMin || %disMin == 0){
               %disMin = %dis;
               %itemObj.team = %teamObj.team;
            }
         }
      }
  }
}

if($dtStats::Enable){
   activatePackage(dtStats);
}
////////////////////////////////////////////////////////////////////////////////
//							 Game Type Commons								  //
////////////////////////////////////////////////////////////////////////////////
function dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){  
   if($dtStats::debugEchos){error(%game SPC %client SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4 SPC %arg5);}
   if(%arg1 $= "S"){
      %client.viewClient = getCNameToCID(%arg3);
      if( %client.viewClient != 0){
         %client.viewStats = 1;// lock out score hud from updateing untill they are done
         %client.viewMenu = %arg2;
         %client.GlArg4 = %arg4;
         %client.GlArg5 = %arg5;
         if($dtStats::debugEchos){error("dtGameLink GUID = "  SPC %client.guid SPC %arg1 SPC %arg2  SPC %arg3 SPC %arg4 SPC %arg5);}  
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
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tS\tView\t%1>+</a>%1</clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %6<rmargin:540><just:right>%7',
                     %cl.name, %clScore, %clKills, %clWins, %clStyle, %clLosses, %clBonus );
            }
            // For observers, create an anchor around the player name so they can be observed
            else
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t%6>%1</a></clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %8<rmargin:540><just:right>%7',
                     %cl.name, %clScore, %clKills, %clWins, %clStyle, %cl, %clBonus, %clLosses );
            }
         }
         else{
               if(%cl == %client){
                  if ( %client.team != 0 && %client.isAlive )
                  {
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tS\tView\t%1>+</a>%1</clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %6<rmargin:540><just:right>%7',
                           %cl.name, %clScore, %clKills, %clWins, %clStyle, %clLosses, %clBonus );
                  }
                  // For observers, create an anchor around the player name so they can be observed
                  else
                  {
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '%5<tab:20, 450>\t<clip:200><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t%6>%1</a></clip><rmargin:250><just:right>%2<rmargin:340><just:right>%3<rmargin:450><just:right>%4 / %8<rmargin:540><just:right>%7',
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
            if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
               messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
               
            }
            else if(%cl == %client){
               messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
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
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tS\tView\t%1>+</a>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180><a:gamelink\tS\tView\t%3>+</a>%3</clip><just:right>%4',
                           %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                           %col1Style, %col2Style );
         }
         //else for observers, create an anchor around the player name so they can be observed
         else
         {
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\tS\tView\t%3>+</a><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
                           %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                           %col1Style, %col2Style, %col1Client, %col2Client );
         }
         %index++;
      }
      else{
         if(%client ==  %col1Client){
            if (%client.team != 0)
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tS\tView\t%1>+</a>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180>%3</clip><lmargin:515><just:left>%4',
                              %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                              %col1Style, %col2Style );
            }
            //else for observers, create an anchor around the player name so they can be observed
            else
            {
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
                              %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                              %col1Style, %col2Style, %col1Client, %col2Client );
            }
            %index++;
         }
         else if(%client == %col2Client){
               if (%client.team != 0)
               {
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180>%1</clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><clip:180><a:gamelink\tS\tView\t%3>+</a>%3</clip><lmargin:515><just:left>%4',
                                 %col1Client.name, formatDuelScore(%col1Client.score), %col2Client.name, formatDuelScore(%col2Client.score),
                                 %col1Style, %col2Style );
               }
               //else for observers, create an anchor around the player name so they can be observed
               else
               {
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\tS\tView\t%3>+</a><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
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
               messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<lmargin:10><just:left><clip:180><a:gamelink\t1%7>%1</a></clip><lmargin:200><just:left>%2<rmargin:305><just:right><color:ff0000>versus<rmargin:570>%6<lmargin:330><just:left><a:gamelink\tS\tView\t%3>+</a><a:gamelink\t1%8><clip:180>%3</clip></a><lmargin:515><just:left>%4',
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
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t1%3>%1</a></clip><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);
         else
            messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><color:0000dc><a:gamelink\tS\tView\t%1>+</a>%1</a></clip><color:00dcdc><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);

         %index++;
      }
      else{
         if(%client == %cl){
            if ((%cl != %client) && (%cl.team == 0) && (%client.team == 0) && %cl.Initialized)
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t1%3>%1</a></clip><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
                           %cl.name, %clientTimeStr, %cl, "none", formatDuelScore(%cl.score), %cl.kills, %cl.deaths);
            else
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><color:0000dc><a:gamelink\tS\tView\t%1>+ </a>%1</a></clip><color:00dcdc><just:right><rmargin:310><color:ffff00>%6<rmargin:370>%7<rmargin:450><color:ffffff>%5<color:00dcdc><rmargin:510>%4<rmargin:570>%2',
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
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%6',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %clBonus);
         }
         else if(%client.name $= %cl.name){
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%6',
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
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t%6> %1</a></clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%7',
            %cl.name, %clScore, %clKills, %clDeaths, %clStyle, %cl, %clBonus);
          }
         else if(%client.name $= %cl.name){
            messageClient( %client, 'SetLineHud', "", %tag, %index, '%5<tab:20, 450>\t<clip:115><a:gamelink\tS\tView\t%1>+</a><a:gamelink\t%6> %1</a></clip><rmargin:225><just:right>%2<rmargin:300><just:right>%3<rmargin:390><just:right>%4<rmargin:490>%7',
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
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
               %cl.name, %obsTimeStr );
            }
            else if(%client.name $= %cl.name){
               messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
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
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tS\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name !$= "" && %col2Client.name $= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= "" && %col2Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tS\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  
               }
               else{
                  if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tS\tView\t%4>+</a>
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tS\tView\t%4>+</a>
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tS\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tS\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col2Client.name, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  
               }
            }
            else{
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  if(%col1Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style );
               }
               else{
                  if(%col1Client.name $= %client.name)
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
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
                  if (%col2Style $= "<color:00dc00>")//<a:gamelink\tS\tView\t%1>+</a>
                  {
                     if(%col1Client.name !$= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
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
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
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
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
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
                  if (%col2Style $= "<color:00dc00>")//<a:gamelink\tS\tView\t%1>+</a><a:gamelink\tS\tView\t%4>+</a>
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     
                  }
                  else if (%col2Style $= "<color:dcdcdc>")//<a:gamelink\tS\tView\t%4>+</a>
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
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
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tS\tView\t%4>+</a>
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tS\tView\t%4>+</a>
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.name, %col1ClientScore, %col1ClientTime,
                     %col2Client.name, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tS\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
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
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.name, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
               }
               else{
                  if(%col1Client.name $= %client.name){
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tS\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
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
               %obsTimeStr = %game.formatTime(%obsTime, false);//<a:gamelink\tS\tView\t%3>+</a>
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr);
               }
               else if(%client.name $= %cl.name){
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%1>+</a> %1</clip><rmargin:260><just:right>%2',
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
         messageClient( %client, 'SetScoreHudSubheader', "",
         '<tab:15,314>\tPLAYERS (%1)<rmargin:265><clip:100><just:right>%4</clip>%3<rmargin:565><just:left>\tPLAYERS (%2)<clip:100><just:right>%5</clip>%3', $TeamRank[1, count], $TeamRank[2, count], (%ShowScores?'SCORE':''),%PingString[1],%PingString[2]);
         
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
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$= "" && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>  %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
               else{ //else for observers, create an anchor around the player name so they can be observed
                  //messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  
                  if(%team1Client.name !$= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$= "" && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
            }
            else{
               if(%client.team != 0){
                  if(%team1Client.name $= %client.name && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$=""  && %team2Client.name $= %client.name)
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= %client.name && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a> %1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name $= %client.name)
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a> %3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200>%1</clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200>%3</clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style);
                  
               }
               else{ //else for observers, create an anchor around the player name so they can be observed
                  //messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  if(%team1Client.name $= %client.name && %team2Client.name !$= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name !$= "" && %team2Client.name $= %client.name)
                     mssageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= %client.name && %team2Client.name $= "")
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\tS\tView\t%7>+</a><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
                  else if(%team1Client.name $= "" && %team2Client.name $= %client.name)
                     messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:0,300>\t<spush>%5<clip:200><a:gamelink\t%7> %1</a></clip><rmargin:260><just:right>%2<spop><rmargin:560><just:left>\t%6<clip:200><a:gamelink\tS\tView\t%8>+</a><a:gamelink\t%8> %3</a></clip><just:right>%4', %team1Client.name, %team1ClientScore, %team2Client.name, %team2ClientScore, %col1Style, %col2Style, %team1Client, %team2Client);
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
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
                  
               }
               else if(%cl == %client){
                  messageClient(%client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tS\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2', %cl.name, %obsTimeStr,%cl);
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
   if($HostGamePlayerCount > $dtServer::maxPlayers[cleanMapName($CurrentMission),%game.class])
      $dtServer::maxPlayers[cleanMapName($CurrentMission),%game.class] = $HostGamePlayerCount;
      
   %client.lp = "";//last position for distMove
   %client.lgame = %game.class;
   %foundOld = 0;
   %mrx = setGUIDName(%client);// make sure we  have a guid if not make one
   
   %authInfo = %client.getAuthInfo();
   %realName = getField( %authInfo, 0 );
   if(%realName !$= "")
      %name = %realName;
   else 
      %name =  stripChars( detag( getTaggedString( %client.name ) ), "\cp\co\c6\c7\c8\c9" );
      
   %crash = 0;
   if(clientCrash(%name) && !$dtStats::cs[%client]){
      $dtStats::cs[%client] = 1;
      $dtServer::clientCrash[cleanMapName($CurrentMission),%game.class]++;
      $dtServer::clientCrashCount++;
      %crash = 1;
   }
      
   if(!isObject(%client.dtStats)){
      for (%i = 0; %i < statsGroup.getCount(); %i++){ // check to see if my old data is still there
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.guid == %client.guid){
            %foundOld =1;
            %client.dtStats = %dtStats;
            %dtStats.client = %client;
            %dtStats.clientLeft = 0;
            %dtStats.markForDelete = 0;
            if(%dtStats.leftID == $dtStats::leftID)
               $dtServer::mapReconnects[cleanMapName($CurrentMission),%game.class]++;
            if(isGameRun() && %dtStats.leftID == $dtStats::leftID && %dtStats.score != 0)// make sure game is running and we are on the same map
               resGameStats(%client,%game.class); // restore stats;
            else
               resetDtStats(%dtStats,%game.class,1);
               
            if(%client.score != 0)
               messageClient(%client, 'MsgClient', '\crWelcome back %1. Your score has been restored.~wfx/misc/rolechange.wav', %client.name);
            break;
         }
      }
      if(!%foundOld){
         %dtStats = new scriptObject(); // object used stats storage
         statsGroup.add(%dtStats);
         %client.dtStats = %dtStats;
         %dtStats.client =%client;
         %dtStats.guid = %client.guid;
         %dtStats.clientLeft = 0;
         %dtStats.markForDelete = 0;
         %dtStats.name = %name;
      }
   }
   else 
     %dtStats = %client.dtStats; 
   if(%crash)
      %dtStats.clientCrash++;
   %dtStats.joinPCT = %game.getGamePct(); 
   if(isObject(%dtStats) && %dtStats.gameData[%game.class] != 1){ // game type change
      %dtStats.gameStats["totalGames","g",%game.class] = 0;
      %dtStats.gameStats["statsOverWrite","g",%game.class] = -1;
      %dtStats.gameStats["fullSet","g",%game.class] = 0;
      resetDtStats(%dtStats,%game.class,1);
      if(!$dtStats::loadAfter || !isGameRun())
         loadGameStats(%dtStats,%game.class);
      else
         %dtStats.gameData[%game.class]= 0;
   }
}
function dtStatsClientLeaveGame(%client){
   if(isGameRun()){// if they dc during game over dont count it
      $dtServer::mapDisconnects[cleanMapName($CurrentMission),Game.class]++;
      if(%client.score != 0)
         $dtServer::mapDisconnectsScore[cleanMapName($CurrentMission),Game.class]++;
   }
   //if($HostGamePlayerCount - $HostGameBotCount == 0)
   if(isObject(%client.dtStats)){
      %client.dtStats.clientLeft = 1;
      %client.dtStats.leftTime = getSimTime();
      %client.dtStats.leftID = $dtStats::leftID;
      if(isObject(Game) && isGameRun() && %client.score != 0)
         bakGameStats(%client,Game.class);//back up there current game in case they lost connection
   }
}
function dtStatsGameOver( %game ){
   $dtStats::serverHang = $dtStats::hostHang = 0;
   $dtStats::LastMissionDN = $MissionDisplayName; 
   $dtStats::LastMissionCM = $CurrentMission;
   $dtStats::LastGameType = %game.class;
   $dtStats::statsSave = 1;
   if(%game.getGamePct() > 50){
      $dtServer::playCount[cleanMapName($CurrentMission),%game.class]++;
      $dtServer::lastPlay[cleanMapName($CurrentMission),%game.class] = getDayNum() TAB getYear TAB formattimestring("mm/dd/yy hh:nn:a");
   }
   else
      $dtServer::skipCount[cleanMapName($CurrentMission),%game.class]++;

   statsGroup.firstKill = 0;
   if($dtStats::debugEchos){error("dtStatsGameOver");}
   if(%game.class $= "CTFGame" || %game.class $= "SCtFGame" || %game.class $= "ArenaGame"){
      statsGroup.team[1] = $teamScore[1];
      statsGroup.team[2] = $teamScore[2];  
   }
   %timeNext =0;
   %time = 2000;
   for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients data
      %dtStats = statsGroup.getObject(%i);
      if(%dtStats.clientLeft || !isObject(%dtStats.client)){ // find any that left during the match and
         %dtStats.markForDelete = 1;
         %game.postGameStats(%dtStats); 
         %time += $dtStats::slowSaveTime; // this will chain them
         schedule(%time ,0,"incGameStats",%dtStats,%game.class);
         if($dtStats::mapStats){
            %time += $dtStats::slowSaveTime; // this will chain them
            schedule(%time ,0,"saveMapStats",%dtStats,%game.class); 
         }
         %time += $dtStats::slowSaveTime; // this will chain them
         schedule(%time ,0,"saveGameTotalStats",%dtStats,%game.class); 
      }
      else if(isObject(%dtStats.client)){// make sure client is still a thing 
         %client = %dtStats.client;
         %client.viewMenu = %client.viewClient = %client.viewStats = 0;//reset hud
         %client.lastPage   = 1; %client.lgame = %game; 
         bakGameStats(%client,%game.class);// copy over game type values before they reset 
         %game.postGameStats(%dtStats); 
         %time += $dtStats::slowSaveTime; // this will chain them
         schedule(%time ,0,"incGameStats",%dtStats,%game.class); //resetDtStats after incGame
         if($dtStats::mapStats){
            %time += $dtStats::slowSaveTime; // this will chain them
            schedule(%time ,0,"saveMapStats",%dtStats,%game.class); 
         }
         %time += $dtStats::slowSaveTime;
         schedule(%time,0,"saveGameTotalStats",%dtStats,%game.class); //
      }
      else{
         error("Logic issue in dtStatsGameOver" SPC %dtStats SPC %client SPC %game.class);
         %dtStats.delete();
      }
   }
   %time += $dtStats::slowSaveTime;
   schedule(%time,0,"dtSaveDone");
}
function dtSaveDone(){
  $dtStats::statsSave = 0; 
  $dtStats::leftID++;
}

////////////////////////////////////////////////////////////////////////////////
//							Supporting Functions							  //
////////////////////////////////////////////////////////////////////////////////
function DefaultGame::postGameStats(%game,%dtStats){ //stats to add up at the end of the match 
   if(!isObject(%dtStats))
      return;
   %dtStats.null = getRandom(1,100);  
   %dtStats.kdr = (%dtStats.kills / (%dtStats.deaths ? %dtStats.deaths : 1));
   if(statsGroup.lastKill == %dtStats)
      %dtStats.lastKill = 1;
      
   %dtStats.dtTeam = %dtStats.team;// this may need to be lastteam
   if(%dtStats.clientLeft)
      %dtStats.totalTime = ((%dtStats.leftTime - %dtStats.joinTime)/1000)/60;
   else
      %dtStats.totalTime = ((getSimTime() - %dtStats.joinTime)/1000)/60;//convert it to min
   
   %dtStats.gamePCT = mFloor(%game.getGamePct() - %dtStats.joinPCT);
   
   %dtStats.cgScore         = (%dtStats.cgKill      + %dtStats.cgMA       + %dtStats.cgKillAir        + (%dtStats.cgKillDist/100)      + %dtStats.cgCom) / 100;
   %dtStats.discScore       = %dtStats.discKill     + %dtStats.discMA     + %dtStats.discKillAir      + (%dtStats.discKillDist/100)    + %dtStats.discCom;
   %dtStats.hGrenadeScore   = %dtStats.hGrenadeKill + %dtStats.hGrenadeMA + %dtStats.hGrenadeKillAir  + (%dtStats.hGrenadeKillDist/20) + %dtStats.hGrenadeCom;
   %dtStats.grenadeScore    = %dtStats.grenadeKill  + %dtStats.grenadeMA  + %dtStats.grenadeKillAir   + (%dtStats.grenadeKillDist/100) + %dtStats.grenadeCom;
   %dtStats.laserScore      = %dtStats.laserKill    + %dtStats.laserMA    + %dtStats.laserKillAir     + (%dtStats.laserKillDist/250)   + %dtStats.laserCom    + %dtStats.laserHeadShot;
   %dtStats.mortarScore     = %dtStats.mortarKill   + %dtStats.mortarMA   + %dtStats.mortarKillAir    + (%dtStats.mortarKillDist/50)   + %dtStats.mortarCom;
   %dtStats.missileScore    = (%dtStats.missileKill + %dtStats.missileMA  + %dtStats.missileKillAir   + (%dtStats.missileKillDist/500) + %dtStats.missileCom) / 10;
   %dtStats.shockScore      = %dtStats.shockKill    + %dtStats.shockMA    + %dtStats.shockKillAir     + (%dtStats.shockKillDist/2)     + %dtStats.shockCom    + %dtStats.shockRearShot;
   %dtStats.plasmaScore     = %dtStats.plasmaKill   + %dtStats.plasmaMA   + %dtStats.plasmaKillAir    + (%dtStats.plasmaKillDist/50)   + %dtStats.plasmaCom;
   %dtStats.blasterScore    = %dtStats.blasterKill  + %dtStats.blasterMA  + %dtStats.blasterKillAir   + (%dtStats.blasterKillDist/50)  + %dtStats.blasterCom;
   %dtStats.mineScore       = %dtStats.mineKill     + %dtStats.mineMA     + %dtStats.mineKillAir      + (%dtStats.mineKillDist/20)     + %dtStats.mineCom;
   %dtStats.satchelScore    = %dtStats.satchelKill  + %dtStats.satchelMA  + %dtStats.satchelKillAir   + (%dtStats.satchelKillDist/20)  + %dtStats.satchelCom;
   
   %dtStats.weaponScore =   %dtStats.cgScore + 
                           %dtStats.discScore + 
                           %dtStats.grenadeScore + 
                           %dtStats.laserScore + 
                           %dtStats.mortarScore + 
                           %dtStats.shockScore + 
                           %dtStats.plasmaScore + 
                           %dtStats.blasterScore + 
                           %dtStats.hGrenadeScore + 
                           %dtStats.missileScore + 
                           %dtStats.mineScore;
   
   %dtStats.totalMA = %dtStats.discMA + 
                     %dtStats.grenadeMA + 
                     %dtStats.laserMA + 
                     %dtStats.mortarMA + 
                     %dtStats.shockMA + 
                     %dtStats.plasmaMA + 
                     %dtStats.blasterMA + 
                     %dtStats.hGrenadeMA + 
                     %dtStats.mineMA;
                         
                           
   %dtStats.EVKills =    %dtStats.explosionKills +
                        %dtStats.groundKills +
                        %dtStats.outOfBoundKills +
                        %dtStats.lavaKills +
                        %dtStats.lightningKills +
                        %dtStats.vehicleSpawnKills +
                        %dtStats.forceFieldPowerUpKills +
                        %dtStats.nexusCampingKills;

   %dtStats.EVDeaths =   %dtStats.explosionDeaths +
                        %dtStats.groundDeaths +
                        %dtStats.outOfBoundDeaths +
                        %dtStats.lavaDeaths +
                        %dtStats.lightningDeaths +
                        %dtStats.vehicleSpawnDeaths +
                        %dtStats.forceFieldPowerUpDeaths +
                        %dtStats.nexusCampingDeaths;
                        
   %dtStats.totalWepDmg = %dtStats.cgDmg +
                         %dtStats.laserDmg +
                         %dtStats.blasterDmg +
                         %dtStats.elfDmg +
                         %dtStats.discDmg +
                         %dtStats.grenadeDmg +
                         %dtStats.hGrenadeDmg +
                         %dtStats.mortarDmg +
                         %dtStats.missileDmg +
                         %dtStats.plasmaDmg +
                         %dtStats.shockDmg +
                         %dtStats.mineDmg +
                         %dtStats.SatchelDmg;
                                     
                         
   if(%dtStats.cgShotsFired < 100)
      %dtStats.cgACC = 0;
   
   if(%dtStats.discShotsFired < 15){  
      %dtStats.discACC = 0;
      %dtStats.discDmgACC = 0;
   }
   
   if(%dtStats.grenadeShotsFired < 10){
      %dtStats.grenadeACC = 0;
      %dtStats.grenadeDmgACC = 0;
   }
   
   if(%dtStats.laserShotsFired < 5)
      %dtStats.laserACC = 0;
   
   if(%dtStats.mortarShotsFired < 10){
      %dtStats.mortarACC = 0;
      %dtStats.mortarDmgACC = 0;
   }
   
   if(%dtStats.shockShotsFired < 5)
      %dtStats.shockACC = 0;
   
   if(%dtStats.plasmaShotsFired < 10){
      %dtStats.plasmaACC = 0;
      %dtStats.plasmaDmgACC = 0;
   }
   
   if(%dtStats.blasterShotsFired < 15)
      %dtStats.blasterACC = 0;
   
   if(%dtStats.hGrenadeShotsFired < 6)
      %dtStats.hGrenadeACC = 0;
   
   //if(%dtStats.mineShotsFired < 10){
      //%dtStats.mineACC = 0;
      //%dtStats.mineDiscPct = 0;
      //%dtStats.mineDiscAccMP = 0;
      //
   //}
   
   //if(%dtStats.mineDiscShots < 5)
      //%dtStats.mineDiscAcc = 0;
      
   if(%dtStats.satchelShotsFired < 5)
      %dtStats.satchelACC = 0;
   
   if(%dtStats.missileShotsFired < 10)
      %dtStats.missileACC = 0;
                         
                         
   if(%game.class $= "CTFGame" || %game.class $= "SCtFGame"){
     %dtStats.destruction =   %dtStats.genDestroys + 
                              %dtStats.solarDestroys + 
                              %dtStats.sensorDestroys + 
                              %dtStats.turretDestroys + 
                              %dtStats.IStationDestroys +
                              %dtStats.aStationDestroys + 
                              %dtStats.VStationDestroys + 
                              %dtStats.sentryDestroys + 
                              %dtStats.depSensorDestroys + 
                              %dtStats.client.depTurretDestroys +
                              %dtStats.depStationDestroys + 
                              %dtStats.mpbtstationDestroys;
                              
   %dtStats.repairs =  %dtStats.genRepairs +
                           %dtStats.SensorRepairs +
                           %dtStats.TurretRepairs +
                           %dtStats.StationRepairs +
                           %dtStats.VStationRepairs +
                           %dtStats.mpbtstationRepairs + 
                           %dtStats.solarRepairs +
                           %dtStats.sentryRepairs +
                           %dtStats.depSensorRepairs + 
                           %dtStats.depInvRepairs +
                           %dtStats.depTurretRepairs;

      if(%dtStats.flagGrabs > 0)                     
         %dtStats.capEfficiency = %dtStats.flagCaps / %dtStats.flagGrabs;
      else 
         %dtStats.capEfficiency = 0;
                     
      if(statsGroup.team[1] == statsGroup.team[2]){
         %dtStats.winCount = 0;
         %dtStats.lossCount = 0;
      }
      else if(statsGroup.team[1] > statsGroup.team[2] && %dtStats.team == 1)
         %dtStats.winCount = 1;
      else if(statsGroup.team[2] > statsGroup.team[1]  && %dtStats.team == 2)
         %dtStats.winCount = 1;
      else if(%dtStats.team > 0)
         %dtStats.lossCount = 1;
         
      %winCount = getField(%dtStats.gameStats["winCountTG","t",%game.class],5) + %dtStats.winCount;
      %lostCount = getField(%dtStats.gameStats["lossCountTG","t",%game.class],5) + %dtStats.lossCount;   
      %lostCount = %lostCount ? %lostCount : 1;
      %winCount = %winCount ? %winCount : 0;
      %dtStats.winLostPct = (%winCount / %lostCount);
   }
   else if(%game.class $= "LakRabbitGame")
      %dtStats.flagTimeMin = (%dtStats.flagTimeMS / 1000)/60; 
}

function isGameRun(){// 
   return  (($MatchStarted + $missionRunning) == 2) ? 1 : 0;
}

function DefaultGame::getGamePct(%game){
   if(isGameRun()){
      %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
      %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
      return %timePct;
   }
   return 0;
}

function ArenaGame::getGamePct(%game){
   if(isGameRun()){
      // Verify that there is a roundlimit and that the team has met it
      if(%game.roundLimit != 0){
         if( $TeamScore[1] >= $TeamScore[2]){
            return ($TeamScore[1] / %game.roundLimit) * 100;
         }
        else if( $TeamScore[1] <= $TeamScore[2]){
           return ($TeamScore[2] / %game.roundLimit) * 100;
        }
      }
   }
   return 0;
}
function CTFGame::getGamePct(%game){
   if(isGameRun()){
      %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
      %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
      
      %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
      if(%scoreLimit $= "")
         %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
      
      if($TeamScore[1] > $TeamScore[2])
         %scorePct =  ($TeamScore[1] / %scoreLimit) * 100;
      else
         %scorePct =  ($TeamScore[2] / %scoreLimit) * 100;
      

      if(%scorePct > %timePct)
         return %scorePct;
      else
         return %timePct;
   }
   return 0;
}

function LakRabbitGame::getGamePct(%game){
   if(isGameRun()){
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
      
      if(%scorePct > %timePct)
         return %scorePct;
      else
         return %timePct;
   }
   return 0;
   
}
function DMGame::getGamePct(%game){
   if(isGameRun()){
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
      

      if(%scorePct > %timePct)
         return %scorePct;
      else
         return %timePct;
   }
   return 0;
}
function SCtFGame::getGamePct(%game){
   if(isGameRun()){
      %curTimeLeftMS =  mAbs((($missionStartTime - getSimTime())/60)/1000);
      %timePct =    (%curTimeLeftMS /  $Host::TimeLimit) * 100;
      
      %scoreLimit = MissionGroup.CTF_scoreLimit * %game.SCORE_PER_TEAM_FLAG_CAP;
      if(%scoreLimit $= "")
         %scoreLimit = 5 * %game.SCORE_PER_TEAM_FLAG_CAP;
      
      if($TeamScore[1] > $TeamScore[2])
         %scorePct =  ($TeamScore[1] / %scoreLimit) * 100;
      else
         %scorePct =  ($TeamScore[2] / %scoreLimit) * 100;
      

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
function secToMinSec(%sec){
   %min = mFloor(%sec / 60);
   %sec -= %min * 60;
   
   // pad it
   if(%min < 10)
      %min = "0" @ %min;
   if(%sec < 10)
      %sec = "0" @ %sec;

   return(%min @ ":" @ %sec);
}
function formatTime(%ms)
{
   %sec = mFloor(%ms / 1000);
   %min = mFloor(%sec / 60);
   %hour = mFloor(%min / 60);
   %days = mFloor(%hour / 24);
   %sec -= %min * 60;
   %min -= %hour * 60;
   %hour -= %days * 24;
   // pad it
   if(%day < 10)
      %day = "0" @ %day;
   if(%hour < 10)
      %hour = "0" @ %hour; 
   if(%min < 10)
      %min = "0" @ %min;
   if(%sec < 10)
      %sec = "0" @ %sec;

   return(%days @ ":" @ %hour @ ":" @ %min @ ":" @ %sec);
}
function clientCrash(%name){
   %len = strlen(%name);
   %dot = getSubStr(%name,%len-2,1);
   %num = getSubStr(%name,%len-1,1);
   if(%dot $= "." && %num > 0)
    return 1;  
   return 0;
}
function setDynamicField(%obj,%field,%value){
   if(isObject(%obj)){
      if(%value $= "")
         %value = 0;
      if(%field $= "")
         %field = "error";
      %format = %obj @ "." @ %field @ "=" @%value@ ";";
      eval(%format);//eww
   }
}
function getDynamicField(%obj,%field){
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
   if(%name !$= ""){
      if(isObject(%name) && %name.getClassName() $= "GameConnection" || %name.getClassName() $= "AIConnection"){
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
   }
   return 0;
}
function cleanName(%nm){
   %validChars = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
   %inValid = " !\"#$%&'()*+,-./:;<=>?@[\\]^_'{|}~\t\n\r";
   for(%a=0; %a < strlen(%nm); %a++){
      %c = getSubStr(%nm,%a,1);
      %vc = strpos(%validChars,%c);
      %iv = strpos(%inValid,%c);
      if(%vc !$= -1){
         %name = %name @ %c;
      }
      else if(%iv !$= -1){ // replace invlaid with number
         %name = %name @ %iv;
      }
   }
   return %name;
}
function cleanMapName(%nm){
   if($cleanMapName $= %nm || $cleanMap $= %nm)
      %name = $cleanMap;
   else{
      %validChars = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
      %inValid = " !_\"#$%&'()*+,-./:;<=>?@[\\]^'{|}~\t\n\r";
      for(%a=0; %a < strlen(%nm); %a++){
         %c = getSubStr(%nm,%a,1);
         %vc = strpos(%validChars,%c);
         %iv = strpos(%inValid,%c);
         if(%vc !$= -1){
            %name = %name @ %c;
         }
         else if(%iv !$= -1){ // replace invlaid with number
            %name = %name;
         }
      }
      $cleanMapName = %nm;
      $cleanMap = %name;
   }
   return %name;
}
function setGUIDName(%client){
   if(isFile("serverStats/genGUIDList.cs") && $genGUIDList != 1){
      exec("serverStats/genGUIDList.cs");
     $genGUIDList = 1; 
   }
   if(%client.guid){
      return 0;  
   }
   else{
      %name  = cleanName(getTaggedString(%client.name));
      if($guidGEN::ID[%name]){
         %client.guid = $guidGEN::ID[%name];
      }
      else{
         $guidGEN::ID[%name] = $guidGEN::Count--;
         export( "$guidGEN::*", "serverStats/genGUIDList.cs", false );
          %client.guid = $guidGEN::ID[%name];
      }
      return 1;
   }
}
function getMapID(%map,%game,%type,%clean){
   if(%clean) 
      %map = cleanMapName(%map);
   if(%game !$= "" && %map !$= ""){
      if($mapID::IDGame[%map,%game] && %type)
         return $mapID::IDGame[%map,%game];
      else if($mapID::ID[%map,%game])
         return $mapID::ID[%map,%game];
      else{
         $mapID::count++;
         $mapID::countGame[%game]++;
         
         $mapID::ID[%map,%game] = $mapID::count;
         $mapID::IDGame[%map,%game] = $mapID::countGame[%game];
         
         $mapID::IDNameGame[$mapID::countGame[%game],%game] = %map;
         $mapID::IDName[$mapID::count] = %map;
         
         export( "$mapID::*", "serverStats/mapIDList.cs", false );
         if(%type)
            return $mapID::IDGame[%map,%game];
         else
            return $mapID::ID[%map,%game];
      }
   }
   else 
      error("getMapID no %map or %game in function call");
}
function loadMapIdList(){
   if(isFile("serverStats/mapIDList.cs") && $genMapId != 1){
      $genMapId = 1; 
      exec("serverStats/mapIDList.cs");
   }  
}
function getDayNum(){
   %date = formattimestring("mm dd yy");
  %m = getWord(%date,0);%d = getWord(%date,1);%y = getWord(%date,2);
   %count = 0;
  if(%y % 4 < 1){%days[2] = "29";}else{%days[2] = "28";} // leap year
  %days[1] = "31";%days[3] = "31";
  %days[4] = "30"; %days[5] = "31"; %days[6] = "30";
  %days[7] = "31"; %days[8] = "31"; %days[9] = "30";
  %days[10] = "31"; %days[11] = "30"; %days[12] = "31";
  for(%i = 1; %i <= %m-1; %i++){
     %count += %days[%i];
  }
  return %count + %d;
}
function getWeekNum(){
    return mCeil(getDayNum() / 7);
}
function getMonthNum(){
    return formattimestring("mm") + 0;
}
function getQuarterNum(){
    return mCeil((formattimestring("mm"))/3);
}
function getYear(){
    return formattimestring("yy") +0;
}
function monthString(%num){
 %i[1] = "January";  %i[2] = "February";  %i[3] = "March";
 %i[4] = "April";    %i[5] = "May";       %i[6] = "June"; 
 %i[7] = "July";     %i[8] = "August";    %i[9] = "September";
 %i[10] = "October"; %i[11] = "November"; %i[12] = "December";
   return %i[%num];
}

////////////////////////////////////////////////////////////////////////////////
//							Load Save Management							  //
////////////////////////////////////////////////////////////////////////////////

function loadSlowStats(%file,%dtStats,%game,%total){
  if(!%file.isEOF()){
      %line = strreplace(%file.readline(),"%t","\t");
      %var = getField(%line,0);
      %dtStats.gameStats[%var,"g",%game] =  getFields(%line,1,getFieldCount(%line)-1);
      schedule($dtStats::loadSlowTime,0,"loadSlowStats",%file,%dtStats,%game,%total);
   }
   else{
      %dtStats.gameData[%game]= 1;
      %file.close();
      %file.delete(); 
   }
}
function loadTotalSlowStats(%file,%dtStats,%game){
  if(!%file.isEOF()){
      %line = strreplace(%file.readline(),"%t","\t");
      %var = getField(%line,0);
      if(%var !$= "playerName"){
         %d0 = getField(%line,1);%d1 = getField(%line,2);
         %w0 = getField(%line,3);%w1 = getField(%line,4);
         %m0 = getField(%line,5);%m1 = getField(%line,6);
         %q0 = getField(%line,7);%q1 = getField(%line,8);
         %y0 = getField(%line,9);%y1 = getField(%line,10);
      
         if(%day){ %d0 = %d1; %d1 = 0;} //if there was a change flip new with old and reset new
         if(%week){%w0 = %w1;%w1 = 0;}
         if(%month){%m0 = %m1;%m1 = 0;}
         if(%quarter){%q0 = %q1;%q1 = 0;}
         if(%year){ %y0 = %y1;%y1 = 0;}
            
         %dtStats.gameStats[%var,"t",%game] = %d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1;
      }
      schedule($dtStats::loadSlowTime,0,"loadTotalSlowStats",%file,%dtStats,%game,%total);
   }
   else{
      %file.close();
      %file.delete(); 
   }
}

function loadGameStats(%dtStats,%game){// called when client joins server.cs onConnect
   if($dtStats::debugEchos){error("loadGameStats GUID = "  SPC %dtStats.guid);} 
   if(%dtStats.guid !$= ""){
      loadGameTotalStats(%dtStats,%game);
      %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
      if(isFile(%filename)){
         %file = new FileObject();
         RootGroup.add(%file);
         %file.OpenForRead(%filename);
         if($dtStats::loadSlowTime > 0 && !$dtStats::loadAfter)// if load after is enabled dont load slow  
            loadSlowStats(%file,%dtStats,%game);  
         else{
            while( !%file.isEOF() ){
               %line = strreplace(%file.readline(),"%t","\t");
               %var = getField(%line,0);
               %dtStats.gameStats[%var,"g",%game] =  getFields(%line,1,getFieldCount(%line)-1);
            }
            %dtStats.gameData[%game]= 1;
            %file.close();
            %file.delete(); 
         }
      }
      else
       %dtStats.gameData[%game]= 1;
   }
}
function loadGameTotalStats(%dtStats,%game){
   if($dtStats::debugEchos){error("loadGameTotalStats GUID = "  SPC %dtStats.guid);}
   
   %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "t.cs";
   %d = $dtStats::curDay; %w = $dtStats::curWeek; %m = $dtStats::curMonth; %q = $dtStats::curQuarter; %y = $dtStats::curYear;   
   if(isFile(%filename)){
      %file = new FileObject();
      RootGroup.add(%file);
      %file.OpenForRead(%filename);
      
      %day  = %week = %month = %quarter = %year = 0;
      %dateLine = strreplace(%file.readline(),"%t","\t"); // first line should allways be the date line 
      if(getField(%dateLine,0) $= "days"){
         if(getField(%dateLine,2) != %d){%day = 1;} // see what has changed sence we last played
         if(getField(%dateLine,4) != %w){%week = 1;}
         if(getField(%dateLine,6) != %m){%month = 1;}
         if(getField(%dateLine,8) != %q){%quarter = 1;}
         if(getField(%dateLine,10) != %y){%year = 1;}
         
         %d0 = getField(%dateLine,1);%d1 = getField(%dateLine,2);
         %w0 = getField(%dateLine,3);%w1 = getField(%dateLine,4);
         %m0 = getField(%dateLine,5);%m1 = getField(%dateLine,6);
         %q0 = getField(%dateLine,7);%q1 = getField(%dateLine,8);
         %y0 = getField(%dateLine,9);%y1 = getField(%dateLine,10);
         
         if(%day){ %d0 = %d1; %d1 = %d;} //if there was a change flip new with old and reset new
         if(%week){%w0 = %w1;%w1 = %w;}
         if(%month){%m0 = %m1;%m1 = %m;}
         if(%quarter){%q0 = %q1;%q1 = %q;}
         if(%year){ %y0 = %y1; %y1 = %y;}
         %dtStats.gameStats["dwmqy","t",%game] =  %d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1; // update line 
      }
      if($dtStats::loadSlowTime > 0 && !$dtStats::loadAfter)// if load after is enabled dont load slow  
         loadTotalSlowStats(%file,%dtStats,%game); 
      else{
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var = getField(%line,0);
            if(%var !$= "playerName" && %var !$= "versionNum"){
               %d0 = getField(%line,1);%d1 = getField(%line,2);
               %w0 = getField(%line,3);%w1 = getField(%line,4);
               %m0 = getField(%line,5);%m1 = getField(%line,6);
               %q0 = getField(%line,7);%q1 = getField(%line,8);
               %y0 = getField(%line,9);%y1 = getField(%line,10);
               
               if(%day){ %d0 = %d1; %d1 = 0;} //if there was a change flip new with old and reset new
               if(%week){%w0 = %w1;%w1 = 0;}
               if(%month){%m0 = %m1;%m1 = 0;}
               if(%quarter){%q0 = %q1;%q1 = 0;}
               if(%year){ %y0 = %y1;%y1 = 0;}
         
               %dtStats.gameStats[%var,"t",%game] = %d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1;

            }
         }
         %file.close();
         %file.delete();
      }
   }
   else// must be new person so be sure to set the dates 
      %dtStats.gameStats["dwmqy","t",%game] =  %d TAB %d TAB %w TAB %w TAB %m TAB %m TAB %q TAB %q TAB %y TAB %y;  
}
function saveGameTotalStats(%dtStats,%game){
   if($dtStats::debugEchos){error("saveGameTotalStats GUID = "  SPC %dtStats.guid);}
      if(%dtStats.guid !$= ""){
         
         if(%dtStats.gameStats["statsOverWrite","g",%game] $= ""){%dtStats.gameStats["statsOverWrite","g",%game] = 0;}
         %fileTotal = new FileObject();
         RootGroup.add(%fileTotal);
         %fileNameTotal = "serverStats/stats/"@ %game @ "/" @ %dtStats.guid  @ "t.cs";  
         %fileTotal.OpenForWrite(%fileNameTotal);
         %fileTotal.writeLine("days" @ "%t" @ strreplace(%dtStats.gameStats["dwmqy","t",%game],"\t","%t"));                                
         %fileTotal.writeLine("gameCount" @ "%t" @ strreplace(%dtStats.gameStats["gameCount","t",%game],"\t","%t"));
         %fileTotal.writeLine("playerName" @ "%t" @  %dtStats.name);
         %fileTotal.writeLine("versionNum" @ "%t" @  $dtStats::version);
         
         %fileGame = new FileObject();
         RootGroup.add(%fileGame);
         %fileNameGame = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
         %fileGame.OpenForWrite(%fileNameGame);
         %fileGame.writeLine("playerName" @ "%t" @ trim(%dtStats.name));
         %fileGame.writeLine("statsOverWrite" @ "%t" @ %dtStats.gameStats["statsOverWrite","g",%game]);
         %fileGame.writeLine("totalGames" @ "%t" @  %dtStats.gameStats["totalGames","g",%game]);
         %fileGame.writeLine("fullSet" @ "%t" @  %dtStats.gameStats["fullSet","g",%game]);
         %fileGame.writeLine("dayStamp" @ "%t" @ strreplace(%dtStats.gameStats["dayStamp","g",%game],"\t","%t"));
         %fileGame.writeLine("weekStamp" @ "%t" @ strreplace(%dtStats.gameStats["weekStamp","g",%game],"\t","%t"));
         %fileGame.writeLine("monthStamp" @ "%t" @ strreplace(%dtStats.gameStats["monthStamp","g",%game],"\t","%t"));
         %fileGame.writeLine("quarterStamp" @ "%t" @ strreplace(%dtStats.gameStats["quarterStamp","g",%game],"\t","%t"));
         %fileGame.writeLine("yearStamp" @ "%t" @ strreplace(%dtStats.gameStats["yearStamp","g",%game],"\t","%t"));
         %fileGame.writeLine("dateStamp" @ "%t" @ strreplace(%dtStats.gameStats["dateStamp","g",%game],"\t","%t"));
         %fileGame.writeLine("timeDayMonth" @ "%t" @ strreplace(%dtStats.gameStats["timeDayMonth","g",%game],"\t","%t"));
         %fileGame.writeLine("map" @ "%t" @ strreplace(%dtStats.gameStats["map","g",%game],"\t","%t"));
         %fileGame.writeLine("mapID" @ "%t" @ strreplace(%dtStats.gameStats["mapID","g",%game],"\t","%t"));
         %fileGame.writeLine("mapGameID" @ "%t" @ strreplace(%dtStats.gameStats["mapGameID","g",%game],"\t","%t"));
         %fileGame.writeLine("gameID" @ "%t" @ strreplace(%dtStats.gameStats["gameID","g",%game],"\t","%t"));
         %fileGame.writeLine("gamePCT" @ "%t" @ strreplace(%dtStats.gameStats["gamePCT","g",%game],"\t","%t"));
         %fileGame.writeLine("versionNum" @ "%t" @ strreplace(%dtStats.gameStats["versionNum","g",%game],"\t","%t"));
         
         for(%q = 0; %q < $statsVars::count[%game]; %q++){
            %varNameType = $statsVars::varNameType[%q,%game];
            %varType =  $statsVars::varType[%varNameType,%game]; 
            if(%varType !$= "TTL"){
               %val = %dtStats.gameStats[%varNameType,"g",%game];
               %fileGame.writeLine(%varNameType @ "%t" @ strreplace(%val,"\t","%t"));
            }
            if(%varType !$= "Game"){
               %val = %dtStats.gameStats[%varNameType,"t",%game];
               %fileTotal.writeLine(%varNameType @ "%t" @ strreplace(%val,"\t","%t"));
            }
         }
         %fileTotal.close();
         %fileGame.close();
         %fileTotal.delete();
         %fileGame.delete();
      }
      if(%dtStats.markForDelete){
         if($dtStats::debugEchos){error("Client Left, Deleting Stat Object = "  SPC %dtStats SPC %dtStats.guid);}
         %dtStats.delete();
      }
}
function saveMapStats(%dtStats,%game){
   if($dtStats::debugEchos){error("saveMapStats GUID = "  SPC %dtStats.guid);}
      %filename = "serverStats/stats/"@ %game @ "/" @ %dtStats.guid  @ "m.cs";  
      %file = new FileObject();
      RootGroup.add(%file);
      %file.OpenForWrite(%filename);
      %file.writeLine("curDMY" @ "%t" @  $dtStats::curDay  @ "%t" @ $dtStats::curMonth @ "%t" @ $dtStats::curYear);
      %file.writeLine("playerName" @ "%t" @  %dtStats.name);
      %file.writeLine("varName" @ "%t" @   strreplace(%dtStats.mapStats["varName",%game],"\t","%t"));
      //%file.writeLine("versionNum" @ "%t" @  $dtStats::version);
      for(%q = 1; %q <= $mapID::countGame[%game]; %q++){
         %mapName = $mapID::IDNameGame[%q,%game];
         %mid = getMapID(%mapName,%game,0,0);
         %gid = getMapID(%mapName,%game,1,0);
         %mapNameID = %mapName @ "-" @ %mid @ "-" @ %gid;
         if(%varType !$= "Game"){
            %val = %dtStats.mapStats[%mapNameID,%game];
            if(getFieldCount(%val) == 0)
               %val = $dtStats::blank["m"];
            %file.writeLine(%mapNameID  @ "%t" @ strreplace(%val,"\t","%t"));
         }
      }
      %file.close();
      %file.delete();
}

function loadMapStats(%dtStats,%game){
   if($dtStats::debugEchos){error("loadMapStats GUID = "  SPC %dtStats.guid);} 
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "m.cs";
      if(isFile(%filename)){
         %file = new FileObject();
         RootGroup.add(%file);
         %file.OpenForRead(%filename);
         %date = strreplace(%file.readline(),"%t","\t");
         if($dtStats::curMonth == getField(%date,2)){// if not the same month then reset 
            while( !%file.isEOF() ){
               %line = strreplace(%file.readline(),"%t","\t");
               %var = getField(%line,0);
               %dtStats.mapStats[%var,%game] =  getFields(%line,1,getFieldCount(%line)-1);
            }
         }
         %dtStats.mapData[%game]= 1; 
         %file.close();
         %file.delete(); 
      }
      else
       %dtStats.mapData[%game] = 1;
   }
} 
function getMapIDName(%game){
   %map = cleanMapName($dtStats::LastMissionCM);
   %mid = getMapID(%map,%game,0,1);
   %gid = getMapID(%map,%game,1,1);
   %mapNameID = %map @ "-" @ %mid @ "-" @ %gid;  
   return %mapNameID;
} 
function incGameStats(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("incGameStats GUID = "  SPC %dtStats.guid);} 
   if(!%dtStats.gameData[%game]) // if not loaded load total stats so we can save 
      loadGameStats(%dtStats,%game);
   if(!%dtStats.mapData[%game] && $dtStats::mapStats)
      loadMapStats(%dtStats,%game);
      
   %c = %dtStats.gameStats["statsOverWrite","g",%game]++;
   if(%dtStats.gameStats["statsOverWrite","g",%game]  > $dtStats::MaxNumOfGames-1 || %dtStats.gameStats["statsOverWrite","g",%game]  > 99){
      %c = %dtStats.gameStats["statsOverWrite","g",%game] = 0;
      %dtStats.gameStats["fullSet","g",%game] = 1;
   }

   %dtStats.gameStats["totalGames","g",%game]++;

   %c1 = getField(%dtStats.gameStats["gameCount","t",%game],1);
   setValueField(%dtStats,"gameCount","t",%game,1,%c1++);
   %c7 = getField(%dtStats.gameStats["gameCount","t",%game],3);
   setValueField(%dtStats,"gameCount","t",%game,3,%c7++);
   %c30 = getField(%dtStats.gameStats["gameCount","t",%game],5);
   setValueField(%dtStats,"gameCount","t",%game,5,%c30++);
   %c90 = getField(%dtStats.gameStats["gameCount","t",%game],7);
   setValueField(%dtStats,"gameCount","t",%game,7,%c90++);
   %c365 = getField(%dtStats.gameStats["gameCount","t",%game],9);
   setValueField(%dtStats,"gameCount","t",%game,9,%c365++);
   
   setValueField(%dtStats,"dayStamp","g",%game,%c,$dtStats::curDay);   
   setValueField(%dtStats,"weekStamp","g",%game,%c,$dtStats::curWeek);  
   setValueField(%dtStats,"monthStamp","g",%game,%c,$dtStats::curMonth);  
   setValueField(%dtStats,"quarterStamp","g",%game,%c,$dtStats::curQuarter);  
   setValueField(%dtStats,"yearStamp","g",%game,%c,$dtStats::curYear);  
   setValueField(%dtStats,"dateStamp","g",%game,%c,formattimestring("yy-mm-dd hh:nn:ss"));
   setValueField(%dtStats,"timeDayMonth","g",%game,%c,formattimestring("hh:nn a, mm-dd"));
   setValueField(%dtStats,"map","g",%game,%c,$dtStats::LastMissionDN);
   setValueField(%dtStats,"mapID","g",%game,%c,getMapID($dtStats::LastMissionCM,%game,0,1));
   setValueField(%dtStats,"mapGameID","g",%game,%c,getMapID($dtStats::LastMissionCM,%game,1,1));
   setValueField(%dtStats,"gameID","g",%game,%c,$mapID::gameID);
   setValueField(%dtStats,"gamePCT","g",%game,%c,%dtStats.gamePCT);
   setValueField(%dtStats,"versionNum","g",%game,%c,$dtStats::version);
      
   for(%q = 0; %q < $statsVars::count[%game]; %q++){
      %varNameType = $statsVars::varNameType[%q,%game];
      %varName = $statsVars::varName[%q,%game]; 
      %varType =  $statsVars::varType[%varNameType,%game]; 
      switch$(%varType){
         case "Game":
            %val = getDynamicField(%dtStats,%varName);
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);
         case "TG":
            %val = getDynamicField(%dtStats,%varName);
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);
            
            %varID = $mapStats::mapVarIndex[%varNameType,%game]; 
            if(%varID > 0 && $dtStats::mapStats){
               %varID -= 1;
               %varSpot = getField(%dtStats.mapStats["varName",%game],%varID);
               if(%varSpot $= %varNameType)// make sure the spot is still named the same 
                   %mapVal = getField(%dtStats.mapStats[getMapIDName(%game),%game],%varID);
               else
                  %mapVal = 0;// reset to 0 the var was changed
               setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID,addNum(%mapVal,%val));
               setValueField(%dtStats,"varName","m",%game,%varID,%varNameType);
            }

            for(%x = 1; %x <= 9; %x+=2){
               %t = getField(%dtStats.gameStats[%varNameType,"t",%game],%x);
               setValueField(%dtStats,%varNameType,"t",%game,%x,addNum(%t,%val));
            }
         case "TTL":
            %val = getDynamicField(%dtStats,%varName);
            %varID = $mapStats::mapVarIndex[%varNameType,%game]; 
            if(%varID > 0 && $dtStats::mapStats){
               %varID -= 1;
               %varSpot = getField(%dtStats.mapStats["varName",%game],%varID);
               if(%varSpot $= %varNameType)// make sure the spot is still named the same 
                   %mapVal = getField(%dtStats.mapStats[getMapIDName(%game),%game],%varID);
               else
                  %mapVal = 0;// reset to 0 the var was changed
               setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID,addNum(%mapVal,%val));
               setValueField(%dtStats,"varName","m",%game,%varID,%varNameType);
            }
            for(%x = 1; %x <= 9; %x+=2){
               %t = getField(%dtStats.gameStats[%varNameType,"t",%game],%x);
               setValueField(%dtStats,%varNameType,"t",%game,%x,addNum(%t,%val));
            }
         case "Max":
            %val = getDynamicField(%dtStats,%varName);
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);
            %varID = $mapStats::mapVarIndex[%varNameType,%game]; 
            if(%varID > 0 && $dtStats::mapStats){
               %varID -= 1;
               %varSpot = getField(%dtStats.mapStats["varName",%game],%varID);
               if(%varSpot $= %varNameType)// make sure the spot is still named the same 
                   %mapVal = getField(%dtStats.mapStats[getMapIDName(%game),%game],%varID);
               else
                  %mapVal = 0;
               if(%val > %mapVal){setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID,%val);}
               else{         setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID,%mapVal);}
               setValueField(%dtStats,"varName","m",%game,%varID,%varNameType);
            }
            for(%x = 1; %x <= 9; %x+=2){
               %t =    getField(%dtStats.gameStats[%varNameType,"t",%game],%x);
               if(%val > %t){setValueField(%dtStats,%varNameType,"t",%game,%x,%val);}
               else{         setValueField(%dtStats,%varNameType,"t",%game,%x,%t);}
            }
         case "Min":
            %val = getDynamicField(%dtStats,%varName);
            
            setValueField(%dtStats,%varNameType,"g",%game,%c,%val);
            %varID = $mapStats::mapVarIndex[%varNameType,%game]; 
            if(%varID > 0 && $dtStats::mapStats){
               %varID -= 1;
               %varSpot = getField(%dtStats.mapStats["varName",%game],%varID);
               if(%varSpot $= %varNameType)// make sure the spot is still named the same 
                   %mapVal = getField(%dtStats.mapStats[getMapIDName(%game),%game],%varID);
               else
                  %mapVal = 0;
               if(%val < %mapVal && %val != 0 || !%mapVal){  setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID,%val);}
               else{                                         setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID,%mapVal);}
               setValueField(%dtStats,"varName","m",%game,%varID,%varNameType);
            }
            for(%x = 1; %x <= 9; %x+=2){  
               %t = getField(%dtStats.gameStats[%varNameType,"t",%game],%x);
               if(%val < %t && %val != 0 || !%t){  setValueField(%dtStats,%varNameType,"t",%game,%x,%val);}
               else{            setValueField(%dtStats,%varNameType,"t",%game,%x,%t);}
            }
         case "Avg" or "AvgI":
            %val = getDynamicField(%dtStats,%varName);
            %varID = $mapStats::mapVarIndex[%varNameType,%game]; 
            if(%varID > 0 && %val != 0 && $dtStats::mapStats){
               %varID -= 1;
               %varSpot = getField(%dtStats.mapStats["varName",%game],%varID);
               if(%varSpot $= %varNameType)// make sure the spot is still named the same 
                  %mapVal = strreplace(getField(%dtStats.mapStats[getMapIDName(%game),%game],%varID),"%a","\t");
               else
                  %mapVal = 0 TAB 0 TAB 0;
               %mapTotal = getField(%mapVal,1) + %val;
               if(%mapTotal<950000){
                  %mapCount = getField(%mapVal,2) + 1;
                  %mapAvg = %mapTotal/%mapCount;
               }
               else{
                  %mapTotal =  mFloor(%mapTotal * 0.9);  
                  %mapCount = mFloor((getField(%mapVal,2) + 1) * 0.9);
                  %mapAvg = %mapTotal/%mapCount;
               }
               setValueField(%dtStats,getMapIDName(%game),"m",%game,%varID, hasValue(%mapAvg) @ "%a" @ hasValue(%mapTotal) @ "%a" @ hasValue(%mapCount));
               setValueField(%dtStats,"varName","m",%game,%varID,%varNameType);
            }
            
            for(%x = 1; %x <= 9; %x+=2){ 
               %t = strreplace(getField(%dtStats.gameStats[%varNameType,"t",%game],%x),"%a","\t");
               if(%val != 0){
                  %total = getField(%t,1) + %val;
                  if(%total<950000){
                     %gameCount = getField(%t,2) + 1;
                     %avg = %total/%gameCount;
                  }
                  else{
                     %total =  mFloor(%total * 0.9);  
                     %gameCount = mFloor((getField(%t,2) + 1) * 0.9);
                     %avg = %total/%gameCount;
                  }
               }
               else{
                  %avg = getField(%t,0); 
                  %total = getField(%t,1);
                  %gameCount = getField(%t,2);
               }
               if(%x ==1)
                  setValueField(%dtStats,%varNameType,"g",%game,%c,%avg);// may not be needed
               setValueField(%dtStats,%varNameType,"t",%game,%x, hasValue(%avg) @ "%a" @ hasValue(%total) @ "%a" @ hasValue(%gameCount));
         }
      }
   }
   resetDtStats(%dtStats,%game,0); // reset to 0 for next game
}
function cropDec(%num){
   %length = strlen(%num);
   %dot = strPos(%num,".");
   if(%dot == -1)
      return %num @ "x";
   else
      return getSubStr(%num,0,%dot) @ "x";
}  
function addNum(%a,%b){
   if(strPos(%a,"x") == -1 && strPos(%b,"x") == -1){
      %ab = %a + %b;
      if(%ab < 999999){
         return %ab;
      }
   }
   
   if(strPos(%a,"x") == -1)
      %a = cropDec(%a);
   if(strPos(%b,"x") == -1)
      %b = cropDec(%b); 
      
   if(strPos(%b,"-") == 0){
      %bn = strreplace(%b,"-","");
      if(xlCompare(%a,%bn) $= "<"){
         return 0; 
      }
      else{
         %r = addSubX(%a,%bn);
         if(strPos(%r,"-") == 0)
            return 0;
         return %r;
      }
   }

   %n1 = strLen(%a);
   %n2 = strLen(%b);  
   %cc = (%n1 > %n2) ? %n1 : %n2;
   for(%x = 1; %x < %cc; %x++){
      %q = (%x < %n1) ? getSubStr(%a,(%n1 - %x)-1,1) : 0;
      %w = (%x < %n2) ? getSubStr(%b,(%n2 - %x)-1,1) : 0;
      %sum = %q+%w+%c;//18  = 9 + 9 + 0
      %newVal = (%sum % 10) @ %newVal;//8 = 18 % 10
      %c = mFloor(%sum/10); //1 = 18/10
   }
   return %c ? %c  @ %newVal : %newVal;
}
function addSubX(%a,%b){// auto flips so its subing form the largest basicly absolute value
   if(strPos(%a,"x") == -1 && strPos(%b,"x") == -1){
      %ab = %a - %b;
      if(%ab < 0){
         return 0;
      }
      return %ab;
   }
   if(strPos(%a,"x") == -1)
      %a = cropDec(%a);
   if(strPos(%b,"x") == -1)
      %b = cropDec(%b);
      
   %n1 = strLen(%a);
   %n2 = strLen(%b);  
   %cc = (%n1 > %n2) ? %n1 : %n2;
   %c = 0;
   for(%x = 1; %x < %cc; %x++){
      %q = (%x < %n1) ? getSubStr(%a,(%n1 - %x)-1,1) : 0;
      %w = (%x < %n2) ? getSubStr(%b,(%n2 - %x)-1,1) : 0;
      %sub = %q-%w-%c;
      if(%x == %cc-1 && %sub == 0)
         break;
      if(%sub >= 0){  
         %newVal = %sub  @ %newVal;
         %c = 0;  
      }
      else{  
         %newVal = %sub+10  @ %newVal;
         %c = 1;  
      }
   }
   return trimZeroLeft(%newVal);
}
function trimZeroLeft(%val){
   %ln = strLen(%val);
   for(%x = 0; %x < %ln; %x++){ 
      %num = getSubStr(%val,%x,1); 
      if(%num != 0)
       break;  
   }
   if(%x == %ln)
      return 0;
   return getSubStr(%val,%x,%ln);
}
function xlCompare(%a,%b){
   if(strPos(%a,"x") == -1 && strPos(%b,"x") == -1){
      if(%a > %b)
         return ">";
      else if(%a < %b)
         return "<";
      else if (%a $= %b)
         return "=";
   }
   
   if(strPos(%a,"x") == -1)
      %a = %a @ "x";
   if(strPos(%b,"x") == -1)
      %b = %b @ "x"; 
      
   %n1 = strLen(%a); 
   %n2 = strLen(%b); 
   if(%n1 > %n2)
      return ">"; 
   else if(%n1 < %n2)
      return "<";
   else{
      if(%a $= %b)
         return "=";   
      %g = %l = 0;
      for(%x = 0; %x < %n1-1; %x++){
         %q = getSubStr(%a,%x,1);
         %w = getSubStr(%b,%x,1); 
         if(%q > %w)
            return ">";
         else if(%q < %w) 
            return "<";
      }
   }
}
function getTimeDif(%time){
   %x = formattimestring("hh");
   %y = formattimestring("nn");
   %z = formattimestring("a");
   %a = getField(%time,0);
   %b = getField(%time,1);
   %c = getField(%time,2);
   if(%c $= "pm" && %a < 12)
      %a += 12; 
   else if(%c $= "am" && %a == 12)
      %a = 0;
   if(%z $= "pm" && %x < 12)
      %x += 12; 
   else if(%z $= "am" && %x == 12)
      %x = 0;

   %v = %a + (%b/60);
   %w = %x + (%y/60);
   %h = (%v >  %w) ? (%h = mabs(%v - %w)) : (24 - mabs(%v - %w));
   %min = %h - mfloor(%h);
   %ms = mfloor(%h) * ((60 * 1000)* 60); // 60 * 1000 1 min * 60  =  one hour
   %ms += mFloor((%min*60)+0.5) * (60 * 1000); // %min * 60 to convert back to mins , * 60kms for one min 
   return mFloor(%ms);
} 
function genBlanks(){ // optimization thing saves on haveing to do it with every setValueField
   $dtStats::blank["g"] = $dtStats::blank["t"] = "";  

   if($dtStats::MaxNumOfGames > 300){
      $dtStats::MaxNumOfGames  = 300; //cap it
   }
   $dtStats::blank["g"] = $dtStats::blank["t"] = $dtStats::blank["m"]  = 0;
   for(%i=0; %i < $dtStats::MaxNumOfGames-1; %i++){
      $dtStats::blank["g"] = $dtStats::blank["g"] TAB 0;  
   }  
   for(%i=0; %i < 108-1; %i++){
      $dtStats::blank["m"] = $dtStats::blank["m"] TAB 0; 
   }
   for(%i=0; %i < 8; %i++){
      $dtStats::blank["t"] = $dtStats::blank["t"] TAB 0;  
   } 
}
function setValueField(%dtStats,%var,%type,%game,%c,%val){
   if(%type $= "g"){
      %fc = getFieldCount(%dtStats.gameStats[%var,%type,%game]);
      if(%fc < 2){
         %dtStats.gameStats[%var,%type,%game] = $dtStats::blank["g"];  
      }
      else if( %fc > $dtStats::MaxNumOfGames){// trim it down as it as the MaxNumOfGames have gotten smaller 
         %dtStats.gameStats[%var,%type,%game] = getFields(%dtStats.gameStats[%var,%type,%game],0,$dtStats::MaxNumOfGames-1);
      }
      %dtStats.gameStats[%var,%type,%game] =   setField(%dtStats.gameStats[%var,%type,%game],%c, hasValue(%val));
   }
   else if(%type $= "t"){
      %fc = getFieldCount(%dtStats.gameStats[%var,%type,%game]);
      if(%fc < 2){
         %dtStats.gameStats[%var,%type,%game] = $dtStats::blank["t"];
      }
      %dtStats.gameStats[%var,%type,%game] =   setField(%dtStats.gameStats[%var,%type,%game],%c, hasValue(%val));
   }
   else if(%type $= "m"){
      %fc = getFieldCount(%dtStats.mapStats[%var,%game]);
      if(%fc < 2){
         %dtStats.mapStats[%var,%game] = $dtStats::blank["m"];
      }
      %dtStats.mapStats[%var,%game] =   setField(%dtStats.mapStats[%var,%game],%c, hasValue(%val));
   }
}

function hasValue(%val){//make sure we have at least something in the field spot
  if(%val $= ""){return 0;}
  return %val; 
}

function bakGameStats(%client,%game) {//back up clients current stats in case they come back
   if($dtStats::debugEchos){error("bakGameStats GUID = "  SPC %client.guid);}  
   %dtStats = %client.dtStats;
   %dtStats.team = (%client.team > 0) ? %client.team : %client.lastTeam;
   
   for(%v = 0; %v < $dtStats::varTypeCount; %v++){
      %varType = $dtStats::varType[%v];
      for(%i = 1; %i <= $dtStats::FCG[%game,%varType]; %i++){
         %var = $dtStats::FVG[%i,%game,%varType];
         copyDynamicField(%dtStats,%client,%var);
      }
   }
   for(%i = 1; %i <= $dtStats::uGFC[%game]; %i++){
      %var = $dtStats::uGFV[%i,%game];
      copyDynamicField(%dtStats,%client,%var);
   }
}
function resGameStats(%client,%game){// copy data back over to client
   if($dtStats::debugEchos){error("resGameStats GUID = "  SPC %client.guid);}
   %dtStats = %client.dtStats;
   for(%v = 0; %v < $dtStats::varTypeCount; %v++){
      %varType = $dtStats::varType[%v];
       for(%i = 1; %i <= $dtStats::FCG[%game,%varType]; %i++){
         %var = $dtStats::FVG[%i,%game,%varType];
         copyDynamicField(%client,%dtStats,%var);
      } 
   }
   for(%i = 1; %i <= $dtStats::uGFC[%game]; %i++){
      %var = $dtStats::uGFV[%i,%game];
      copyDynamicField(%client,%dtStats,%var);
   }
} 
function copyDynamicField(%obj,%obj2,%field){
   if(isObject(%obj) && isObject(%obj2)){
      %format = %obj @ "." @ %field @ "=" @ %obj2 @ "." @ %field @ ";";
      eval(%format);
   }
}
function resetChain(%game,%dtStats,%count,%last){
   //if($dtStats::debugEchos){error("resetChain" SPC %last SPC %count);} 
   for(%i = %last; %i < %count; %i++){  
      %var = $statsVars::varName[%i,%game];
      setDynamicField(%dtStats,%var,0);
   }
}
function resetDtStats(%dtStats,%game,%slow){
   if($dtStats::debugEchos){error("resetDtStats GUID = "  SPC %dtStats.guid);}
   if(isObject(%dtStats)){
      %dtStats.joinTime = getSimTime();
      if(%slow){// low server impact 
         %amount =  100;
         %count = mFloor($statsVars::count[%game]/%amount);
         %leftOver = $statsVars::count[%game] - (%count * %amount);
         for(%i = 0; %i < %count; %i++){
            %x  += %amount;
            schedule(32*%i,0,"resetChain",%game,%dtStats,%x,(%i * %amount));
         }
         schedule(32*(%i+1),0,"resetChain",%game,%dtStats,(%x+%leftOver),(%i * %amount));
      }
      else{
         for(%q = 0; %q < $statsVars::count[%game]; %q++){
            %var = $statsVars::varName[%q,%game];
            setDynamicField(%dtStats,%var,0);
         }
      }
      for(%i = 1; %i <= $dtStats::unusedCount; %i++){//script unused
         %var = $dtStats::unused[%i];
         setDynamicField(%dtStats,%var,0);
      }
   }
}
function buildVarList(){ 
   deleteVariables("$statsVars::*");
   for(%g = 0; %g < $dtStats::gameTypeCount; %g++){
      %game = $dtStats::gameType[%g];
      for(%i = 1; %i <= $mapStats::mapVarCount[%game]; %i++){
         %val = $mapStats::mapVars[%i,%game];
         if(%val !$= "")
            $mapStats::mapVarIndex[%val,%game]  = %i;
      }
   }
   
   for(%g = 0; %g < $dtStats::gameTypeCount; %g++){
      %game = $dtStats::gameType[%g];
      $statsVars::count[%game] = -1;
      for(%v = 0; %v < $dtStats::varTypeCount; %v++){
         %varType = $dtStats::varType[%v];
         for(%i = 1; %i <= $dtStats::FCG[%game,%varType]; %i++){// game types
            %var = $dtStats::FVG[%i,%game,%varType] @ %varType;
            if($statsVars::varType[%var,%game] $= ""){
               $statsVars::varType[%var,%game] = %varType;
               $statsVars::varNameType[$statsVars::count[%game]++,%game] = %var;
               $statsVars::varName[$statsVars::count[%game],%game] = $dtStats::FVG[%i,%game,%varType];
            }
            else{
               error("Error buildVarList duplicate var:" SPC %var ); 
            }
         }
         for(%i = 1; %i <= $dtStats::FC[%game,%varType]; %i++){// game type script
            %var = $dtStats::FV[%i,%game,%varType] @ %varType;
            if($statsVars::varType[%var,%game] $= ""){
               $statsVars::varType[%var,%game] = %varType;
               $statsVars::varNameType[$statsVars::count[%game]++,%game] = %var;
               $statsVars::varName[$statsVars::count[%game],%game] = $dtStats::FV[%i,%game,%varType];
            }
            else{
               error("Error buildVarList duplicate var:" SPC %var );   
            }
         }
         for(%i = 1; %i <= $dtStats::FC[%varType]; %i++){// script
            %var = $dtStats::FV[%i,%varType] @ %varType;
            if($statsVars::varType[%var,%game] $= ""){ 
               $statsVars::varType[%var,%game] = %varType;
               $statsVars::varNameType[$statsVars::count[%game]++,%game] = %var;
               $statsVars::varName[$statsVars::count[%game],%game] = $dtStats::FV[%i,%varType];
            }
            else{
               error("Error buildVarList duplicate var:" SPC %var );
            }
         }
      }
      $statsVars::count[%game] += 1;
   }
}
////////////////////////////////////////////////////////////////////////////////
//Stats Collecting
////////////////////////////////////////////////////////////////////////////////
function clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation){
   if(%damageType == $DamageType::Explosion || %damageType == $DamageType::Ground ||
      %damageType == $DamageType::OutOfBounds ||  %damageType == $DamageType::Lava ||
      %damageType == $DamageType::VehicleSpawn || %damageType == $DamageType::ForceFieldPowerup || 
      %damageType == $DamageType::Lightning  ||   %damageType == $DamageType::NexusCamping ||   
      %damageType == $DamageType::Suicide	){
      if((getSimTime() - %clVictim.lastHitTime) < 3000)
         %clKiller = %clVictim.lastHitBy;
         %clVictim.lastHitBy = 0;
   }
   else if(!isObject(%clKiller) && isObject(%implement)){
      if(%damageType == $DamageType::IndoorDepTurret || %damageLocation == $DamageType::OutdoorDepTurret){
         %clKiller = %implement.owner;
      }
      else
         %clKiller = %implement.getControllingClient();  
   }
   %clVictim.lastPlayer = %clVictim.player;
   %killerDT = %clKiller.dtStats;
   %victimDT = %clVictim.dtStats;
   %clVictim.lp = "";//last position for distMove
//------------------------------------------------------------------------------
   %victimDT.timeToLive += getSimTime() - %clVictim.spawnTime;
   %victimDT.timeTL = mFloor((%victimDT.timeToLive/(%clVictim.deaths+%clVictim.suicides ? %clVictim.deaths+%clVictim.suicides : 1))/1000);
//------------------------------------------------------------------------------ 
   if(%clKiller.team == %clVictim.team && %clKiller != %clVictim)
      %killerDT.teamkillCount++;
//------------------------------------------------------------------------------    
   if(%clKiller.team != %clVictim.team){
      if(isObject(%clKiller.player))
         %kPlayer = %clKiller.player;
      else if(isObject(%clKiller.lastPlayer)) 
         %kPlayer = %clKiller.lastPlayer;
      if(isObject(%clVictim.player) && isObject(%kPlayer) && %damageType != $DamageType::IndoorDepTurret && %damageLocation != $DamageType::OutdoorDepTurret){
         
//------------------------------------------------------------------------------
            %clKiller.k++; %clVictim.k = 0;
            if(%clVictim == %clKiller || %damageType == $DamageType::Suicide || %damageType == $DamageType::Lava || %damageType == $DamageType::OutOfBounds || %damageType == $DamageType::Ground || %damageType == $DamageType::Lightning){
              %clVictim.k = %clKiller.k = 0;
            }
            if(%killerDT.killStreak < %clKiller.k){
               %killerDT.killStreak = %clKiller.k;
            }
//------------------------------------------------------------------------------            
            if(%clVictim.player.hitBy[%clKiller]){
               %killerDT.assist--;
            }
//------------------------------------------------------------------------------            
            %isCombo = 0;
            if(%kPlayer.combo[%clVictim.player] > 1){
               %killerDT.comboCount++;
               %isCombo =1;
            }
//------------------------------------------------------------------------------
         if(!statsGroup.firstKill && isGameRun()){
            statsGroup.firstKill = 1;
            %killerDT.firstKill = 1;
         }
//------------------------------------------------------------------------------         
         statsGroup.lastKill = %killerDT;
//------------------------------------------------------------------------------         
         if(%kPlayer.getState() $= "Dead"){
            %killerDT.deathKills++;
         }
//------------------------------------------------------------------------------         
         if(getSimTime() - %clKiller.mKill < 256){
            %clKiller.mkCounter++;
            if(!isEventPending(%clKiller.mkID))
               %clKiller.mkID = schedule(256,0,"multiKillDelayer",%clKiller,%killerDT);
         }
         else{
            if(!isEventPending(%clKiller.mkID))
               %clKiller.mkCounter = 1;        
         }%clKiller.mKill =  getSimTime();
//------------------------------------------------------------------------------      
         if(getSimTime() - %clKiller.mCKill < 10000){
            %clKiller.chainCount++;
            if(!isEventPending(%clKiller.chainID))
               %clKiller.chainID = schedule(256,0,"chainKill",%clKiller,%killerDT);
         } 
         else{
            %clKiller.chainCount = 1;  
         }%clKiller.mCKill =  getSimTime();
         
//------------------------------------------------------------------------------         
         if(rayTest(%clVictim.player, $dtStats::midAirHeight)){%vcAir =1;}else{%vcAir =2;}
         if(rayTest(%kPlayer, $dtStats::midAirHeight)){%kcAir =1;}else{%kcAir =2;}
      
         switch$(%clVictim.player.getArmorSize()){
            case "Light":%killerDT.armorL++; %victimDT.armorLD++;
               switch$(%kPlayer.getArmorSize()){
                  case "Light": %killerDT.armorLL++; %victimDT.armorLLD++;
                  case "Medium":%killerDT.armorML++; %victimDT.armorLMD++;
                  case "Heavy": %killerDT.armorHL++; %victimDT.armorLHD++;
               }
            case "Medium": %killerDT.armorM++; %victimDT.armorMD++;
               switch$(%kPlayer.getArmorSize()){
                  case "Light": %killerDT.armorLM++; %victimDT.armorMLD++;
                  case "Medium":%killerDT.armorMM++; %victimDT.armorMMD++;
                  case "Heavy": %killerDT.armorHM++; %victimDT.armorMHD++;
               }
            case "Heavy":%killerDT.armorH++; %victimDT.armorHD++;
               switch$(%kPlayer.getArmorSize()){
                  case "Light": %killerDT.armorLH++; %victimDT.armorHLD++;
                  case "Medium":%killerDT.armorMH++; %victimDT.armorHMD++;
                  case "Heavy": %killerDT.armorHH++; %victimDT.armorHHD++;
               }
         } 
//------------------------------------------------------------------------------         
         %dis = vectorDist(%kPlayer.getPosition(),%clVictim.player.getPosition());
         %vD = vectorSub(%clVictim.player.getVelocity(),%kPlayer.getVelocity());
         %rvel = mFloor(vectorLen(%vD) * 3.6);
         %victimVel =  mFloor(vectorLen(%clVictim.player.getVelocity()) * 3.6);
      }
      else{
         %kcAir = %vcAir = 0; 
         %rvel = %dis = 0;
      }
//------------------------------------------------------------------------------   
      if(%clVictim.EVDamageType && %clVictim.EVDamageType != %damageType){ // they were hit by something befor they were killed
         %killerDT.EVKillsWep++;
         %victimDT.EVDeathsWep++;
         if(rayTest(%clVictim.player, $dtStats::midAirHeight)){
            if(%clVictim.EVDamageType == $DamageType::Lightning && (getSimTime() - %clVictim.EVDamagetime) < 3000){
               %killerDT.lightningMAkills++;
               %clKiller.dtMessage("Lightning MidAir Kill","fx/misc/MA2.wav",1);
            }
            else
               %killerDT.EVMA++;
         }
         %clVictim.EVDamageType = 0;
      }
//------------------------------------------------------------------------------      
      if(%kcAir == 1 && %vcAir == 1){%killerDT.killAir++;%victimDT.deathAir++;%killerDT.killAirAir++;%victimDT.deathAirAir++;}
      else if(%kcAir == 2 && %vcAir == 1){%killerDT.killAir++;%victimDT.deathAir++;%killerDT.killGroundAir++;%victimDT.deathGroundAir++; }
      else if(%kcAir == 1 && %vcAir == 2){%killerDT.killGround++;%victimDT.deathGround++;%killerDT.killAirGround++;%victimDT.deathAirGround++;}
      else if(%kcAir == 2 && %vcAir == 2){%killerDT.killGround++;%victimDT.deathGround++;%killerDT.killGroundGround++; %victimDT.deathGroundGround++; }
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %killerDT.cgKills++;
            %victimDT.cgDeaths++;
            if(%killerDT.cgKillDist < %dis){%killerDT.cgKillDist = %dis;}
            if(%killerDT.cgKillVV < %victimVel){%killerDT.cgKillVV = %victimVel;}
            if(%killerDT.cgKillSV <  %clKiller.dtShotSpeed){%killerDT.cgKillSV = %clKiller.dtShotSpeed;}
            
            if(%isCombo){%killerDT.cgCom++;}
            
            if(%kcAir == 1 && %vcAir == 1){%killerDT.cgKillAir++;%victimDT.cgDeathAir++;%killerDT.cgKillAirAir++;%victimDT.cgDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.cgKillAir++;%victimDT.cgDeathAir++;%killerDT.cgKillGroundAir++;%victimDT.cgDeathGroundAir++; }
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.cgKillGround++;%victimDT.cgDeathGround++;%killerDT.cgKillAirGround++;%victimDT.cgDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.cgKillGround++;%victimDT.cgDeathGround++;%killerDT.cgKillGroundGround++; %victimDT.cgDeathGroundGround++; }
         case $DamageType::Disc:
            %killerDT.discKills++;
            %victimDT.discDeaths++;
            if(%killerDT.discKillDist < %dis){%killerDT.discKillDist = %dis;}
            if(%killerDT.discKillVV < %victimVel){%killerDT.discKillVV = %victimVel;} 
            if(%killerDT.discKillSV <  %clKiller.dtShotSpeed){%killerDT.discKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.discCom++;} 
            if(%clVictim.md == 1 && (getSimTime() - %clVictim.mdTime1) < 256){%killerDT.minePlusDiscKill++;}
            
            if(%kcAir == 1 && %vcAir == 1){%killerDT.discKillAir++;%victimDT.discDeathAir++;%killerDT.discKillAirAir++;%victimDT.discDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.discKillAir++;%victimDT.discDeathAir++;%killerDT.discKillGroundAir++;%victimDT.discDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.discKillGround++;%victimDT.discDeathGround++;%killerDT.discKillAirGround++;%victimDT.discDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.discKillGround++;%victimDT.discDeathGround++;%killerDT.discKillGroundGround++; %victimDT.discDeathGroundGround++;}
         case $DamageType::Grenade:
            if($dtObjExplode.dtNade){
               %killerDT.hGrenadeKills++;
               %victimDT.hGrenadeDeaths++;
               if(%killerDT.hGrenadeKillDist < %dis){%killerDT.hGrenadeKillDist = %dis;}
               if(%killerDT.hGrenadeKillVV < %victimVel){%killerDT.hGrenadeKillVV = %victimVel;}
               if(%killerDT.hGrenadeKillSV <  %clKiller.dtShotSpeed){%killerDT.hGrenadeKillSV = %clKiller.dtShotSpeed;}
               if(%isCombo){%killerDT.hGrenadeCom++;}
               if(%kcAir == 1 && %vcAir == 1){%killerDT.hGrenadeKillAir++;%victimDT.hGrenadeDeathAir++;%killerDT.hGrenadeKillAirAir++;%victimDT.hGrenadeDeathAirAir++;}
               else if(%kcAir == 2 && %vcAir == 1){%killerDT.hGrenadeKillAir++;%victimDT.hGrenadeDeathAir++;%killerDT.hGrenadeKillGroundAir++;%victimDT.hGrenadeDeathGroundAir++;}
               else if(%kcAir == 1 && %vcAir == 2){%killerDT.hGrenadeKillGround++;%victimDT.hGrenadeDeathGround++;%killerDT.hGrenadeKillAirGround++;%victimDT.hGrenadeDeathAirGround++;}
               else if(%kcAir == 2 && %vcAir == 2){%killerDT.hGrenadeKillGround++;%victimDT.hGrenadeDeathGround++;%killerDT.hGrenadeKillGroundGround++; %victimDT.hGrenadeDeathGroundGround++;}
            }
            else{
               %killerDT.grenadeKills++;
               %victimDT.grenadeDeaths++;
               if(%killerDT.grenadeKillDist < %dis){%killerDT.grenadeKillDist = %dis;}
               if(%killerDT.grenadeKillVV < %victimVel){%killerDT.grenadeKillVV = %victimVel;}
               if(%killerDT.grenadeKillSV <  %clKiller.dtShotSpeed){%killerDT.grenadeKillSV = %clKiller.dtShotSpeed;}
               if(%isCombo){%killerDT.grenadeCom++;}
               if(%kcAir == 1 && %vcAir == 1){%killerDT.grenadeKillAir++;%victimDT.grenadeDeathAir++;%killerDT.grenadeKillAirAir++;%victimDT.grenadeDeathAirAir++;}
               else if(%kcAir == 2 && %vcAir == 1){%killerDT.grenadeKillAir++;%victimDT.grenadeDeathAir++;%killerDT.grenadeKillGroundAir++;%victimDT.grenadeDeathGroundAir++;}
               else if(%kcAir == 1 && %vcAir == 2){%killerDT.grenadeKillGround++;%victimDT.grenadeDeathGround++;%killerDT.grenadeKillAirGround++;%victimDT.grenadeDeathAirGround++;}
               else if(%kcAir == 2 && %vcAir == 2){%killerDT.grenadeKillGround++;%victimDT.grenadeDeathGround++;%killerDT.grenadeKillGroundGround++; %victimDT.grenadeDeathGroundGround++;}
            }
         case $DamageType::Laser:
            %killerDT.laserKills++;
            %victimDT.laserDeaths++;
            if(%killerDT.laserKillDist < %dis){%killerDT.laserKillDist = %dis;}
            if(%killerDT.laserKillVV < %victimVel){%killerDT.laserKillVV = %victimVel;}
            if(%killerDT.laserKillSV <  %clKiller.dtShotSpeed){%killerDT.laserKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.laserCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.laserKillAir++;%victimDT.laserDeathAir++;%killerDT.laserKillAirAir++;%victimDT.laserDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.laserKillAir++;%victimDT.laserDeathAir++;%killerDT.laserKillGroundAir++;%victimDT.laserDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.laserKillGround++;%victimDT.laserDeathGround++;%killerDT.laserKillAirGround++;%victimDT.laserDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.laserKillGround++;%victimDT.laserDeathGround++;%killerDT.laserKillGroundGround++; %victimDT.laserDeathGroundGround++;}
         case $DamageType::Mortar:
            %killerDT.mortarKills++;
            %victimDT.mortarDeaths++;
            if(%killerDT.mortarKillDist < %dis){%killerDT.mortarKillDist = %dis;}
            if(%killerDT.mortarKillVV < %victimVel){%killerDT.mortarKillVV = %victimVel;}
            if(%killerDT.mortarKillSV <  %clKiller.dtShotSpeed){%killerDT.mortarKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.mortarCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.mortarKillAir++;%victimDT.mortarDeathAir++;%killerDT.mortarKillAirAir++;%victimDT.mortarDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.mortarKillAir++;%victimDT.mortarDeathAir++;%killerDT.mortarKillGroundAir++;%victimDT.mortarDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.mortarKillGround++;%victimDT.mortarDeathGround++;%killerDT.mortarKillAirGround++;%victimDT.mortarDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.mortarKillGround++;%victimDT.mortarDeathGround++;%killerDT.mortarKillGroundGround++; %victimDT.mortarDeathGroundGround++;}
         case $DamageType::Missile:
            %killerDT.missileKills++;
            %victimDT.missileDeaths++;
            if(%killerDT.missileKillDist < %dis){%killerDT.missileKillDist = %dis;}
            if(%killerDT.missileKillVV < %victimVel){%killerDT.missileKillVV = %victimVel;}
            if(%killerDT.missileKillSV <  %clKiller.dtShotSpeed){%killerDT.missileKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.missileCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.missileKillAir++;%victimDT.missileDeathAir++;%killerDT.missileKillAirAir++;%victimDT.missileDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.missileKillAir++;%victimDT.missileDeathAir++;%killerDT.missileKillGroundAir++;%victimDT.missileDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.missileKillGround++;%victimDT.missileDeathGround++;%killerDT.missileKillAirGround++;%victimDT.missileDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.missileKillGround++;%victimDT.missileDeathGround++;%killerDT.missileKillGroundGround++; %victimDT.missileDeathGroundGround++;}
         case $DamageType::ShockLance:
            %killerDT.shockKills++;
            %victimDT.shockDeaths++;
            if(%killerDT.shockKillDist < %dis){%killerDT.shockKillDist = %dis;}
            if(%killerDT.shockKillVV < %victimVel){%killerDT.shockKillVV = %victimVel;}
            if(%killerDT.shockKillSV <  %clKiller.dtShotSpeed){%killerDT.shockKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.shockCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.shockKillAir++;%victimDT.shockDeathAir++;%killerDT.shockKillAirAir++;%victimDT.shockDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.shockKillAir++;%victimDT.shockDeathAir++;%killerDT.shockKillGroundAir++;%victimDT.shockDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.shockKillGround++;%victimDT.shockDeathGround++;%killerDT.shockKillAirGround++;%victimDT.shockDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.shockKillGround++;%victimDT.shockDeathGround++;%killerDT.shockKillGroundGround++; %victimDT.shockDeathGroundGround++;}
         case $DamageType::Plasma:
            %killerDT.plasmaKills++;
            %victimDT.plasmaDeaths++;
            if(%killerDT.plasmaKillDist < %dis){%killerDT.plasmaKillDist = %dis;}
            if(%killerDT.plasmaKillVV < %victimVel){%killerDT.plasmaKillVV = %victimVel;}
            if(%killerDT.plasmaKillSV <  %clKiller.dtShotSpeed){%killerDT.plasmaKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.plasmaCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.plasmaKillAir++;%victimDT.plasmaDeathAir++;%killerDT.plasmaKillAirAir++;%victimDT.plasmaDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.plasmaKillAir++;%victimDT.plasmaDeathAir++;%killerDT.plasmaKillGroundAir++;%victimDT.plasmaDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.plasmaKillGround++;%victimDT.plasmaDeathGround++;%killerDT.plasmaKillAirGround++;%victimDT.plasmaDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.plasmaKillGround++;%victimDT.plasmaDeathGround++;%killerDT.plasmaKillGroundGround++; %victimDT.plasmaDeathGroundGround++;}
         case $DamageType::Blaster:
            %killerDT.blasterKills++;
            %victimDT.blasterDeaths++;
            if(%killerDT.blasterKillDist < %dis){%killerDT.blasterKillDist = %dis;}
            if(%killerDT.blasterKillVV < %victimVel){%killerDT.blasterKillVV = %victimVel;}
            if(%killerDT.blasterKillSV <  %clKiller.dtShotSpeed){%killerDT.blasterKillSV = %clKiller.dtShotSpeed;}
            if(%isCombo){%killerDT.blasterCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.blasterKillAir++;%victimDT.blasterDeathAir++;%killerDT.blasterKillAirAir++;%victimDT.blasterDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.blasterKillAir++;%victimDT.blasterDeathAir++;%killerDT.blasterKillGroundAir++;%victimDT.blasterDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.blasterKillGround++;%victimDT.blasterDeathGround++;%killerDT.blasterKillAirGround++;%victimDT.blasterDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.blasterKillGround++;%victimDT.blasterDeathGround++;%killerDT.blasterKillGroundGround++; %victimDT.blasterDeathGroundGround++;}
         case $DamageType::ELF:
            %killerDT.elfKills++;
            %victimDT.elfDeaths++;
         case $DamageType::Mine:
            %killerDT.mineKills++;
            %victimDT.mineDeaths++;
            if(%killerDT.mineKillDist < %dis){%killerDT.mineKillDist = %dis;}
            if(%killerDT.mineKillVV < %victimVel){%killerDT.mineKillVV = %victimVel;}
            if(%isCombo){%killerDT.mineCom++;}
            if(%clVictim.md == 2 && (getSimTime() - %clVictim.mdTime2) < 256){%killerDT.minePlusDiscKill++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.mineKillAir++;%victimDT.mineDeathAir++;%killerDT.mineKillAirAir++;%victimDT.mineDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.mineKillAir++;%victimDT.mineDeathAir++;%killerDT.mineKillGroundAir++;%victimDT.mineDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.mineKillGround++;%victimDT.mineDeathGround++;%killerDT.mineKillAirGround++;%victimDT.mineDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.mineKillGround++;%victimDT.mineDeathGround++;%killerDT.mineKillGroundGround++; %victimDT.mineDeathGroundGround++;}
         case $DamageType::SatchelCharge:
            %killerDT.satchelKills++;
            %victimDT.satchelDeaths++;
            if(%killerDT.satchelKillDist < %dis){%killerDT.satchelKillDist = %dis;}
            if(%killerDT.satchelKillVV < %victimVel){%killerDT.satchelKillVV = %victimVel;}
            if(%isCombo){%killerDT.satchelCom++;}
            if(%kcAir == 1 && %vcAir == 1){%killerDT.satchelKillAir++;%victimDT.satchelDeathAir++;%killerDT.satchelKillAirAir++;%victimDT.satchelDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%killerDT.satchelKillAir++;%victimDT.satchelDeathAir++;%killerDT.satchelKillGroundAir++;%victimDT.satchelDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%killerDT.satchelKillGround++;%victimDT.satchelDeathGround++;%killerDT.satchelKillAirGround++;%victimDT.satchelDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%killerDT.satchelKillGround++;%victimDT.satchelDeathGround++;%killerDT.satchelKillGroundGround++; %victimDT.satchelDeathGroundGround++;}
         case $DamageType::Explosion:
            if(%clKiller){%killerDT.explosionKills++;}
            %victimDT.explosionDeaths++;
         case $DamageType::Impact:
            if(isObject(%clKiller.vehicleMounted)){
               %veh =   %clKiller.vehicleMounted.getDataBlock().getName();
               %killerDT.roadKills++;  %victimDT.roadDeaths++;
               switch$(%veh){
                  case "ScoutVehicle":     %killerDT.wildRK++;       %victimDT.wildRD++;
                  case "AssaultVehicle":   %killerDT.assaultRK++;    %victimDT.assaultRD++;
                  case "MobileBaseVehicle":%killerDT.mobileBaseRK++; %victimDT.mobileBaseRD++;
                  case "ScoutFlyer":       %killerDT.scoutFlyerRK++; %victimDT.scoutFlyerRD++;
                  case "BomberFlyer":      %killerDT.bomberFlyerRK++;%victimDT.bomberFlyerRD++;
                  case "HAPCFlyer":        %killerDT.hapcFlyerRK++;  %victimDT.hapcFlyerRD++;
               }
            }
            %killerDT.impactKills++;
            %victimDT.impactDeaths++;
         case $DamageType::Ground:
            if(%clKiller){%killerDT.groundKills++;}
            %victimDT.groundDeaths++;
         case $DamageType::PlasmaTurret:
            %killerDT.plasmaTurretKills++;
            %victimDT.plasmaTurretDeaths++;
         case $DamageType::AATurret:
            %killerDT.aaTurretKills++;
            %victimDT.aaTurretDeaths++;
         case $DamageType::ElfTurret:
            %killerDT.elfTurretKills++;
            %victimDT.elfTurretDeaths++;
         case $DamageType::MortarTurret:
            %killerDT.mortarTurretKills++;
            %victimDT.mortarTurretDeaths++;
         case $DamageType::MissileTurret:
            %killerDT.missileTurretKills++;
            %victimDT.missileTurretDeaths++;
         case $DamageType::IndoorDepTurret:
            %killerDT.indoorDepTurretKills++;
            %victimDT.indoorDepTurretDeaths++;
         case $DamageType::OutdoorDepTurret:
            %killerDT.outdoorDepTurretKills++;
            %victimDT.outdoorDepTurretDeaths++;
         case $DamageType::SentryTurret:
            %killerDT.sentryTurretKills++;
            %victimDT.sentryTurretDeaths++;
         case $DamageType::OutOfBounds:
            if(%clKiller){%killerDT.outOfBoundKills++;}
            %victimDT.outOfBoundDeaths++;
         case $DamageType::Lava:
            if(%clKiller){%killerDT.lavaKills++;}
            %victimDT.lavaDeaths++;
         case $DamageType::ShrikeBlaster:
            %killerDT.shrikeBlasterKills++;
            %victimDT.shrikeBlasterDeaths++;
         case $DamageType::BellyTurret:
            %killerDT.bellyTurretKills++;
            %victimDT.bellyTurretDeaths++;
         case $DamageType::BomberBombs:
            %killerDT.bomberBombsKills++;
            %victimDT.bomberBombsDeaths++;
         case $DamageType::TankChaingun:
            %killerDT.tankChaingunKills++;
            %victimDT.tankChaingunDeaths++;
         case $DamageType::TankMortar:
            %killerDT.tankMortarKills++;
            %victimDT.tankMortarDeaths++;
         case $DamageType::Lightning:
            if(%clKiller){
               %killerDT.lightningKills++;
               if(%vcAir == 1 && (getSimTime() - %clVictim.lastHitTime) < 3000 && %clVictim.lastHitMA){
                  %killerDT.lightningMAEVKills++;
                  %killerDT.lightningMAkills++;
                  %clKiller.dtMessage("Lightning MidAir EV Kill","fx/misc/MA2.wav",1);
               }
            }
            %victimDT.lightningDeaths++;
         case $DamageType::VehicleSpawn:
            if(%clKiller){%killerDT.vehicleSpawnKills++;}
            %victimDT.vehicleSpawnDeaths++;
         case $DamageType::ForceFieldPowerup:
            if(%clKiller){%killerDT.forceFieldPowerUpKills++;}
            %victimDT.forceFieldPowerUpDeaths++;
         case $DamageType::Crash:
            %killerDT.crashKills++;
            %victimDT.crashDeaths++;
         case $DamageType::NexusCamping:
            if(%clKiller){%killerDT.nexusCampingKills++;}
            %victimDT.nexusCampingDeaths++;
         case $DamageType::Suicide:
            if(%clKiller){%killerDT.ctrlKKills++;}
            //%victimDT.ctrlKKills++;
      }
   }
}

function multiKillDelayer(%clKiller,%killerDT){
   switch(%clKiller.mkCounter){
      case 2:
         %killerDT.doubleKill++;
      case 3:
         %killerDT.tripleKill++;
      case 4:
         %killerDT.quadrupleKill++;
      case 5:
         %killerDT.quintupleKill++;
      case 6:
         %killerDT.sextupleKill++;
      case 7:
         %killerDT.septupleKill++;
      case 8:
         %killerDT.octupleKill++;
      case 9:
         %killerDT.nonupleKill++;
      case 10:
         %killerDT.decupleKill++;
      default:
         if(%clKiller.mkCounter > 10)
         %killerDT.nuclearKill++;
   }
   %killerDT.multiKill++;
   %clKiller.mkCounter = 1;
}

function chainKill(%clKiller,%killerDT){
   switch(%clKiller.chainCount){
      case 2:
         %killerDT.doubleChainKill++;
      case 3:
         %killerDT.tripleChainKill++;
      case 4:
         %killerDT.quadrupleChainKill++;
      case 5:
         %killerDT.quintupleChainKill++;
      case 6:
         %killerDT.sextupleChainKill++;
      default:
         if(%clKiller.chainCount > 6)
         switch(%clKiller.chainCount){
            case 7:
               %killerDT.septupleChainKill++; 
            case 8:
               %killerDT.octupleChainKill++; 
            case 9:
               %killerDT.nonupleChainKill++; 
            case 10:
               %killerDT.decupleChainKill++;
         }
   }
   %killerDT.chainKill++;
} 
function GameConnection::dtMessage(%this,%message,%sfx,%bypass){
   if(!%this.isAIControlled()){
      %diff =  getSimTime() - %this.dtLastMessage;
      if(%sfx !$= "" && %bypass){
         %this.dtLastMessage = getSimTime();
         messageClient(%this,'MsgClient', "\c2" @ %message @ "~w" @ %sfx);
      }
      else if(%sfx !$= "" && %diff > 256){// limits sound spam
         %this.dtLastMessage = getSimTime();
         messageClient(%this,'MsgClient', "\c2" @ %message @ "~w" @ %sfx);
      }
      else
         messageClient(%this,'MsgClient', "\c2" @ %message);
      BottomPrint( %this, "\n" @ %message, 2, 3 ); 
   }
}

function rayTest(%targetObject,%dis){
   %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
   %rayStart = %targetObject.getWorldBoxCenter();
   %rayEnd = VectorAdd(%rayStart,"0 0" SPC ((%dis+1.15) * -1));
   %ground = !ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject);  
   return %ground; 
}
function rayTestDis(%targetObject){
   %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
   %rayStart = %targetObject.getWorldBoxCenter();
   %rayEnd = VectorAdd(%rayStart,"0 0" SPC -5000);
   %ray = ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject);  
   if(!%ray)
      return 0;
      %dis =  vectorDist(%rayStart,getWords(%ray,1,3)) - 1.15;
   return %dis; 
}
function vectorRayCast(%startPos,%vec,%dis){
   %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::PlayerObjectType;
   %endPos = VectorAdd(%startPos, VectorScale(VectorNormalize(%vec), %dis));
   %result = containerRayCast(%startPos, %endPos, %mask, %proj);  
   return %result;  
}

function  dtOnExplode(%data, %proj, %pos, %mod){
   %cl = %proj.sourceObject.client;
   if(isObject(%cl)){
      if(%proj.dtShotSpeed > 0)
         %cl.dtShotSpeed = %proj.dtShotSpeed; 
      else
         %cl.dtShotSpeed = mFloor(vectorLen(%proj.sourceObject.getVelocity()) * 3.6);
      %cl.lastExp = %data TAB %proj.initialPosition TAB %pos; 
   }
}
function testHit(%client){
   if(isObject(%client)){ 
      %field = %client.lastExp;
      %data = getField(%field,0); %sPos = getField(%field,1); %ePos = getField(%field,2);
      if(%data.hasDamageRadius){
         %mask = $TypeMasks::PlayerObjectType;
         %vec = vectorNormalize(vectorSub(%ePos,%sPos));// some how this vector works
         %ray = containerRayCast(%ePos, VectorAdd(%ePos, VectorScale(VectorNormalize(%vec), 5)), %mask, -1);  
         if(%ray){
            %dmgType = %data.radiusDamageType;
            //error(%dmgType);  
            return 1;
         }  
      }
   }
   return 0;
}
function clientDmgStats(%data,%position,%sourceObject, %targetObject, %damageType,%amount){
   if(%damageType == $DamageType::Explosion || %damageType == $DamageType::Ground ||
         %damageType == $DamageType::OutOfBounds ||  %damageType == $DamageType::Lava ||
         %damageType == $DamageType::VehicleSpawn || %damageType == $DamageType::ForceFieldPowerup || 
         %damageType == $DamageType::Lightning  ||   %damageType == $DamageType::NexusCamping){
         if(isObject(%targetObject)){   
            %targetObject.client.EVDamageType = %damageType;   
            %targetObject.client.EVDamagetime = getSimTime();
         }
         return;
   } 
//------------------------------------------------------------------------------
   if(%amount > 0 && %damageType > 0){
      if(isObject(%sourceObject)){ 
         %sourceClass = %sourceObject.getClassName();
         if(%sourceClass $= "Player"){
            %sourceClient = %sourceObject.client;
            %sourceDT = %sourceClient.dtStats;
            %aoeHIT = testHit(%sourceClient);
            %sv = mFloor(vectorLen(%sourceObject.getVelocity()) * 3.6);
         }
         else if(%sourceClass $= "Turret" || %sourceClass $= "FlyingVehicle" || %sourceClass $= "HoverVehicle" || %sourceClass $= "WheeledVehicle"){
            %sourceClient = %sourceObject.getControllingClient();
            %sourceDT = %sourceClient.dtStats;
            %aoeHIT = 0;
         }
         else{
            %aoeHIT = 0;
         }
      }
      if(isObject(%targetObject)){
         %targetClass  = %targetObject.getClassName();
         if(%targetClass $= "Player"){
            %targetClient = %targetObject.client;
            %targetDT = %targetClient.dtStats;
            %vv = mFloor(vectorLen(%targetObject.getVelocity()) * 3.6);  
            if(%sourceClass $= "Player" && %targetClient.team != %sourceClient.team && %sourceObject != %targetObject){
               %dis = vectorDist(%targetObject.getPosition(),%sourceObject.getPosition());
               %targetObject.dtPlayer = %targetObject.player;
               if(!%targetObject.combo[%sourceClient,%damageType]){
                  %targetObject.combo[%sourceClient,%damageType] = 1;
                  %sourceClient.player.combo[%targetObject]++;
               }
               
               if(!%targetObject.hitBy[%sourceClient]){
                  %sourceDT.assist++;
                  %targetObject.hitBy[%sourceClient] = 1; 
               }
               
               %targetClient.lastHitBy = %sourceClient;
               %targetClient.lastHitTime = getSimTime();
               
               if(%targetClient.EVDamageType && %targetClient.EVDamageType != %damageType){ // they were hit by something befor they were killed
                  %sourceDT.EVHitWep++;
                  if(rayTest(%targetClient.player, $dtStats::midAirHeight) && %damageType != $DamageType::Bullet){
                     if(%targetClient.EVDamageType == $DamageType::Lightning && (getSimTime() - %targetClient.EVDamagetime) < 3000){
                        %sourceDT.lightningMAHits++;
                        %sourceClient.dtMessage("Lightning MidAir Hit","fx/Bonuses/down_perppass3_bunnybump.wav",0);
                     }
                     else
                        %sourceDT.EVMAHit++;
                  }
                  if((getSimTime() - %targetClient.EVDamagetime) > 3000){
                     %targetClient.EVDamageType = 0;
                  }
               }
               
               %dmgL = %targetObject.getDamageLocation(%position);
               switch$(getWord(%dmgL,0)){
                  case "legs": %sourceDT.hitLegs++;%targetDT.hitTakenLegs++;
                      switch$(getWord(%dmgL,1)){
                        case "front_right":%sourceDT.hitLegFrontR++;%targetDT.hitTakenLegFrontR++;
                        case "front_Left":%sourceDT.hitLegFrontL++;%targetDT.hitTakenLegFrontL++;
                        case "back_right":%sourceDT.hitLegBackR++;%targetDT.hitTakenLegBackR++;
                        case "back_Left":%sourceDT.hitLegBackL++;%targetDT.hitTakenLegBackL++;
                     }
                  case "torso": %sourceDT.hitTorso++;%targetDT.hitTakenTorso++;
                      switch$(getWord(%dmgL,1)){
                        case "front_right":%sourceDT.hitTorsoFrontR++;%targetDT.hitTakenTorsoFrontR++;
                        case "front_Left":%sourceDT.hitTorsoFrontL++;%targetDT.hitTakenTorsoFrontL++;
                        case "back_right":%sourceDT.hitTorsoBackR++;%targetDT.hitTakenTorsoBackR++;
                        case "back_Left":%sourceDT.hitTorsoBackL++;%targetDT.hitTakenTorsoBackL++;
                     }
                  case "head":%sourceDT.hitHead++; %targetDT.hitTakenHead++;
                      switch$(getWord(%dmgL,1)){
                        case "middle_front":%sourceDT.hitHeadFront++;%targetDT.hitTakenHeadFront++;
                        case "middle_back":%sourceDT.hitHeadBack++;  %targetDT.hitTakenHeadBack++;
                        case "right_middle":%sourceDT.hitHeadRight++;%targetDT.hitTakenHeadRight++;
                        case "left_middle":%sourceDT.hitHeadLeft++;  %targetDT.hitTakenHeadLeft++;
                     }   
               }
               %rayTest = rayTestDis(%targetObject);
               if(%rayTest >= $dtStats::midAirHeight && %damageType != $DamageType::Bullet){
                  if(%sourceDT.maHitDist < %dis){%sourceDT.maHitDist = %dis;}
                  if(%sourceDT.maHitHeight < %rayTest){%sourceDT.maHitHeight = %rayTest;}
                  if(%sourceDT.maHitSV < %sv){%sourceDT.maHitSV = %sv;} 
                  %targetClient.lastHitMA = 1;   
               } 
               else{
                  %targetClient.lastHitMA = 0; 
               }
               switch$(%damageType){// list of all damage types to track see damageTypes.cs
                  case $DamageType::Blaster:
                     %sourceDT.blasterDmg += %amount;
                     %sourceDT.blasterHits++;
                     %sourceDT.blasterACC =  (%sourceDT.blasterHits / (%sourceDT.blasterShotsFired ? %sourceDT.blasterShotsFired : 1)) * 100;
                     if(%sourceDT.blasterHitDist < %dis){%sourceDT.blasterHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.blasterMADist < %dis){%sourceDT.blasterMADist = %dis;}
                        %sourceDT.blasterMA++;
                     } 
                     if(%sourceDT.blasterHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.blasterHitSV = %sourceObject.client.dtShotSpeed;} 
                     if(%sourceDT.blasterHitVV < %vv){%sourceDT.blasterHitVV = %vv;}         
                  case $DamageType::Plasma:
                     %sourceDT.plasmaDmg += %amount;
                     if(%aoeHIT){%sourceDT.plasmaHits++;%sourceDT.plasmaDmgHits++;}
                     else{%sourceDT.plasmaDmgHits++;}
                     %sourceDT.plasmaACC = (%sourceDT.plasmaHits / (%sourceDT.plasmaShotsFired ? %sourceDT.plasmaShotsFired : 1)) * 100;
                     %sourceDT.plasmaDmgACC = (%sourceDT.plasmaDmgHits / (%sourceDT.plasmaShotsFired ? %sourceDT.plasmaShotsFired : 1)) * 100;
                     if(%sourceDT.plasmaHitDist < %dis){%sourceDT.plasmaHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.plasmaMADist < %dis){%sourceDT.plasmaMADist = %dis;}
                        if(%aoeHIT){
                           %sourceDT.plasmaMA++;
                           %sourceDT.plasmaAoeMA++;
                        }
                        else
                           %sourceDT.plasmaAoeMA++;
                     }
                     if(%sourceDT.plasmaHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.plasmaHitSV = %sourceObject.client.dtShotSpeed;} 
                     if(%sourceDT.plasmaHitVV < %vv){%sourceDT.plasmaHitVV = %vv;} 
                  case $DamageType::Bullet:
                     %sourceDT.cgDmg += %amount;
                     %sourceDT.cgHits++;
                     
                     %sourceDT.cgACC = (%sourceDT.cgHits / (%sourceDT.cgShotsFired ? %sourceDT.cgShotsFired : 1)) * 100;
                     if(%sourceDT.cgHitDist < %dis){%sourceDT.cgHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.cgMADist < %dis){%sourceDT.cgMADist = %dis;}
                        %sourceDT.cgMA++;
                     }
                     if(%sourceDT.cgHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.cgHitSV = %sourceObject.client.dtShotSpeed;} 
                     if(%sourceDT.cgHitVV < %vv){%sourceDT.cgHitVV = %vv;}   
                  case $DamageType::Disc:
                     %sourceDT.discDmg += %amount;
                     if(%aoeHIT){%sourceDT.discHits++;%sourceDT.discDmgHits++;}
                     else{%sourceDT.discDmgHits++;} 
                     %sourceDT.discACC = (%sourceDT.discHits / (%sourceDT.discShotsFired ? %sourceDT.discShotsFired : 1)) * 100;
                     %sourceDT.discDmgACC = (%sourceDT.discDmgHits / (%sourceDT.discShotsFired ? %sourceDT.discShotsFired : 1)) * 100;
                     if(%sourceDT.discHitDist < %dis){%sourceDT.discHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%targetClient.md == 1 && (getSimTime() - %targetClient.mdTime1) < 256){%sourceDT.minePlusDisc++;}
                     %targetClient.md = 2;
                     %targetClient.mdTime2 = getSimTime(); 
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.discMADist < %dis){%sourceDT.discMADist = %dis;}
                        if(%aoeHIT){
                           %sourceDT.discMA++;
                           %sourceDT.discAoeMA++; 
                        }
                        else
                           %sourceDT.discAoeMA++; 
                     }   
                     if(%sourceDT.discHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.discHitSV = %sourceObject.client.dtShotSpeed;}   
                     if(%sourceDT.discHitVV < %vv){%sourceDT.discHitVV = %vv;}
                  case $DamageType::Grenade:
                     if($dtObjExplode.dtNade){
                        %sourceDT.hGrenadeDmg += %amount;
                        %sourceDT.hGrenadeHits++;
                        %sourceDT.hGrenadeACC = (%sourceDT.hGrenadeHits / (%sourceDT.hGrenadeShotsFired ? %sourceDT.hGrenadeShotsFired : 1)) * 100;
                        if(%sourceDT.hGrenadeHitDist < %dis){%sourceDT.hGrenadeHitDist = %dis;}
                        if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                        if(%rayTest >= $dtStats::midAirHeight){
                           if(%sourceDT.hGrenadeMADist < %dis){%sourceDT.hGrenadeMADist = %dis;}
                           %sourceDT.hGrenadeMA++;
                        }
                        if(%sourceDT.hGrenadeHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.hGrenadeHitSV = %sourceObject.client.dtShotSpeed;}  
                        if(%sourceDT.hGrenadeHitVV < %vv){%sourceDT.hGrenadeHitVV = %vv;}
                     }
                     else{
                        %sourceDT.grenadeDmg += %amount;
                        if(%aoeHIT){%sourceDT.grenadeHits++;%sourceDT.grenadeDmgHits++;}
                        else{%sourceDT.grenadeDmgHits++;} 
                        %sourceDT.grenadeACC = (%sourceDT.grenadeHits / (%sourceDT.grenadeShotsFired ? %sourceDT.grenadeShotsFired : 1)) * 100;
                        %sourceDT.grenadeDmgACC = (%sourceDT.grenadeDmgHits / (%sourceDT.grenadeShotsFired ? %sourceDT.grenadeShotsFired : 1)) * 100;
                        if(%sourceDT.grenadeHitDist < %dis){%sourceDT.grenadeHitDist = %dis;}
                        if(%rayTest >= $dtStats::midAirHeight){
                           if(%sourceDT.grenadeMADist < %dis){%sourceDT.grenadeMADist = %dis;}
                           if(%aoeHIT){
                              %sourceDT.grenadeMA++;
                              %sourceDT.grenadeAoeMA++;
                           }
                           else
                              %sourceDT.grenadeAoeMA++;
                        }
                        if(%sourceDT.grenadeHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.grenadeHitSV = %sourceObject.client.dtShotSpeed;}  
                        if(%sourceDT.grenadeHitVV < %vv){%sourceDT.grenadeHitVV = %vv;}
                     }   
                  case $DamageType::Laser:
                     if(%targetObject.getClassName() $= "Player"){
                        %damLoc = %targetObject.getDamageLocation(%position);
                        if(getWord(%damLoc,0) $= "head" && %sourceClient.team != %targetClient.team){
                           %sourceDT.laserHeadShot++;
                        }
                     }
                     %sourceDT.laserDmg += %amount;
                     %sourceDT.laserHits++;
                     %sourceDT.laserACC = (%sourceDT.laserHits / (%sourceDT.laserShotsFired ? %sourceDT.laserShotsFired : 1)) * 100;
                     if(%sourceDT.laserHitDist < %dis){%sourceDT.laserHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.laserMADist < %dis){%sourceDT.laserMADist = %dis;}
                        %sourceDT.laserMA++;
                     }
                     if(%sourceDT.laserHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.laserHitSV = %sourceObject.client.dtShotSpeed;} 
                     if(%sourceDT.laserHitVV < %vv){%sourceDT.laserHitVV = %vv;}
                  case $DamageType::Mortar:
                     %sourceDT.mortarDmg += %amount;
                     if(%aoeHIT){%sourceDT.mortarHits++;%sourceDT.mortarDmgHits++;}
                     else{%sourceDT.mortarDmgHits++;}
                     %sourceDT.mortarACC = (%sourceDT.mortarHits / (%sourceDT.mortarShotsFired ? %sourceDT.mortarShotsFired : 1)) * 100;
                     %sourceDT.mortarDmgACC = (%sourceDT.mortarDmgHits / (%sourceDT.mortarShotsFired ? %sourceDT.mortarShotsFired : 1)) * 100;
                     if(%sourceDT.mortarHitDist < %dis){%sourceDT.mortarHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.mortarMADist < %dis){%sourceDT.mortarMADist = %dis;}
                        if(%aoeHIT){
                           %sourceDT.mortarMA++;
                           %sourceDT.mortarAoeMA++;
                        }
                        else
                           %sourceDT.mortarAoeMA++;
                     }
                     if(%sourceDT.mortarHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.mortarHitSV = %sourceObject.client.dtShotSpeed;} 
                     if(%sourceDT.mortarHitVV < %vv){%sourceDT.mortarHitVV = %vv;}   
                  case $DamageType::Missile:
                     %sourceDT.missileDmg += %amount;
                     %sourceDT.missileHits++;
                     %sourceDT.missileACC = (%sourceDT.missileHits / (%sourceDT.missileShotsFired ? %sourceDT.missileShotsFired : 1)) * 100;
                     if(%sourceDT.missileHitDist < %dis){%sourceDT.missileHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.missileMADist < %dis){%sourceDT.missileMADist = %dis;}
                        %sourceDT.missileMA++;
                     }        
                     if(%sourceDT.missileHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.missileHitSV = %sourceObject.client.dtShotSpeed;}     
                     if(%sourceDT.missileHitVV < %vv){%sourceDT.missileHitVV = %vv;}
                  case $DamageType::ShockLance:
                     if(%targetClient.rearshot){
                        %sourceDT.shockRearShot++;
                        }
                     %sourceDT.shockDmg += %amount;
                     %sourceDT.shockHits++;
                     %sourceDT.shockACC = (%sourceDT.shockHits / (%sourceDT.shockShotsFired ? %sourceDT.shockShotsFired : 1)) * 100;
                     if(%sourceDT.shockHitDist < %dis){%sourceDT.shockHitDist = %dis;}
                     if(%sourceDT.weaponHitDist < %dis){%sourceDT.weaponHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){ 
                        if(%sourceDT.shockMADist < %dis){%sourceDT.shockMADist = %dis;}
                        %sourceDT.shockMA++;
                     }
                     if(%sourceDT.shockHitSV <  %sourceObject.client.dtShotSpeed){%sourceDT.shockHitSV = %sourceObject.client.dtShotSpeed;}
                     if(%sourceDT.shockHitVV < %vv){%sourceDT.shockHitVV = %vv;} 
                  case $DamageType::Mine:
                     %sourceDT.mineDmg += %amount;
                     %sourceDT.mineHits++;
                     %sourceDT.mineACC = (%sourceDT.mineHits / (%sourceDT.mineShotsFired ? %sourceDT.mineShotsFired : 1)) * 100;
                     if(%sourceDT.mineHitDist < %dis){%sourceDT.mineHitDist = %dis;}
                     if(%targetClient.md == 2 && (getSimTime() - %targetClient.mdTime2) < 256){%sourceDT.minePlusDisc++;}
                     %targetClient.md = 1;
                     %targetClient.mdTime1 = getSimTime(); 
                     if(%rayTest >= $dtStats::midAirHeight){
                        if(%sourceDT.mineMADist < %dis){%sourceDT.mineMADist = %dis;}
                        %sourceDT.mineMA++;
                     }
                     if(%sourceDT.mineHitVV < %vv){%sourceDT.mineHitVV = %vv;} 
                  case $DamageType::SatchelCharge:
                     %sourceDT.satchelDmg += %amount;
                     %sourceDT.satchelHits++;
                     %sourceDT.satchelACC = (%sourceDT.satchelHits / (%sourceDT.satchelShotsFired ? %sourceDT.satchelShotsFired : 1)) * 100;
                     if(%sourceDT.satchelHitDist < %dis){%sourceDT.satchelHitDist = %dis;}
                     if(%rayTest >= $dtStats::midAirHeight){%sourceDT.satchelMA++;}
                     if(%sourceDT.satchelHitVV < %vv){%sourceDT.satchelHitVV = %vv;} 
               }
            }
         }
         else if(%targetClass $= "Turret" || %targetClass $= "FlyingVehicle" || %targetClass $= "HoverVehicle" || %targetClass $= "WheeledVehicle"){
            %targetClient = %targetObject.getControllingClient();
            %targetDT = %targetClient.dtStats;
            if(%sourceClass $= "Player"){
               switch$(%damageType){// list of all damage types to track see damageTypes.cs
                  case $DamageType::Blaster:
                     %sourceDT.blasterHits++;
                     %sourceDT.blasterACC =  (%sourceDT.blasterHits / (%sourceDT.blasterShotsFired ? %sourceDT.blasterShotsFired : 1)) * 100;
                  case $DamageType::Plasma:
                     if(%aoeHIT){%sourceDT.plasmaHits++;%sourceDT.plasmaDmgHits++;}
                     else{%sourceDT.plasmaDmgHits++;}
                     %sourceDT.plasmaACC = (%sourceDT.plasmaHits / (%sourceDT.plasmaShotsFired ? %sourceDT.plasmaShotsFired : 1)) * 100;
                     %sourceDT.plasmaDmgACC = (%sourceDT.plasmaDmgHits / (%sourceDT.plasmaShotsFired ? %sourceDT.plasmaShotsFired : 1)) * 100;
                  case $DamageType::Bullet:
                     %sourceDT.cgHits++;
                     %sourceDT.cgACC = (%sourceDT.cgHits / (%sourceDT.cgShotsFired ? %sourceDT.cgShotsFired : 1)) * 100;
                  case $DamageType::Disc:
                     if(%aoeHIT){%sourceDT.discHits++;%sourceDT.discDmgHits++;}
                     else{%sourceDT.discDmgHits++;} 
                     %sourceDT.discACC = (%sourceDT.discHits / (%sourceDT.discShotsFired ? %sourceDT.discShotsFired : 1)) * 100;
                     %sourceDT.discDmgACC = (%sourceDT.discDmgHits / (%sourceDT.discShotsFired ? %sourceDT.discShotsFired : 1)) * 100;
                  case $DamageType::Grenade:
                     if($dtObjExplode.dtNade){
                        %sourceDT.hGrenadeHits++;
                        %sourceDT.hGrenadeACC = (%sourceDT.hGrenadeHits / (%sourceDT.hGrenadeShotsFired ? %sourceDT.hGrenadeShotsFired : 1)) * 100;
                     }
                     else{
                        if(%aoeHIT){%sourceDT.grenadeHits++;%sourceDT.grenadeDmgHits++;}
                        else{%sourceDT.grenadeDmgHits++;} 
                        %sourceDT.grenadeACC = (%sourceDT.grenadeHits / (%sourceDT.grenadeShotsFired ? %sourceDT.grenadeShotsFired : 1)) * 100;
                        %sourceDT.grenadeDmgACC = (%sourceDT.grenadeDmgHits / (%sourceDT.grenadeShotsFired ? %sourceDT.grenadeShotsFired : 1)) * 100;
                     }   
                  case $DamageType::Laser:
                     %sourceDT.laserHits++;
                     %sourceDT.laserACC = (%sourceDT.laserHits / (%sourceDT.laserShotsFired ? %sourceDT.laserShotsFired : 1)) * 100;
                  case $DamageType::Mortar:
                     if(%aoeHIT){%sourceDT.mortarHits++;%sourceDT.mortarDmgHits++;}
                     else{%sourceDT.mortarDmgHits++;}
                     %sourceDT.mortarACC = (%sourceDT.mortarHits / (%sourceDT.mortarShotsFired ? %sourceDT.mortarShotsFired : 1)) * 100;
                     %sourceDT.mortarDmgACC = (%sourceDT.mortarDmgHits / (%sourceDT.mortarShotsFired ? %sourceDT.mortarShotsFired : 1)) * 100;
                  case $DamageType::Missile:
                     %sourceDT.missileHits++;
                     %sourceDT.missileACC = (%sourceDT.missileHits / (%sourceDT.missileShotsFired ? %sourceDT.missileShotsFired : 1)) * 100;
                  case $DamageType::ShockLance:
                     %sourceDT.shockHits++;
                     %sourceDT.shockACC = (%sourceDT.shockHits / (%sourceDT.shockShotsFired ? %sourceDT.shockShotsFired : 1)) * 100;
                  case $DamageType::Mine:
                     %sourceDT.mineHits++;
                     %sourceDT.mineACC = (%sourceDT.mineHits / (%sourceDT.mineShotsFired ? %sourceDT.mineShotsFired : 1)) * 100;
                  case $DamageType::SatchelCharge:
                     %sourceDT.satchelHits++;
                     %sourceDT.satchelACC = (%sourceDT.satchelHits / (%sourceDT.satchelShotsFired ? %sourceDT.satchelShotsFired : 1)) * 100;
               }
            }
         }
      } 
   }
}
function clientShotsFired(%data, %sourceObject, %projectile){ // could do a fov check to see if we are trying to aim at a player 
   if(isObject(%projectile) && isObject(%sourceObject)){
      if(%projectile.sourceObject.getClassName() !$= "Player")
         %sourceClient = %projectile.sourceObject.getControllingClient();
      else
         %sourceClient = %sourceObject.client;
   }
      
   if(!isObject(%sourceClient.dtStats)){ return;}
   %dtStats = %sourceClient.dtStats;
   if(%data.directDamageType !$= ""){%damageType = %data.directDamageType;}
   else{%damageType =  %data.radiusDamageType;}
   %dtStats.shotsFired++;
   %sourceClient.dtShotSpeed = %projectile.dtShotSpeed = mFloor(vectorLen(%sourceObject.getVelocity()) * 3.6);
   switch$(%damageType){// list of all damage types to track see damageTypes.cs
      case $DamageType::Bullet:
         %dtStats.cgShotsFired++;
         %dtStats.cgACC = (%dtStats.cgHits / (%dtStats.cgShotsFired ? %dtStats.cgShotsFired : 1)) * 100;
      case $DamageType::Disc:
         //if(getSimTime() - %sourceClient.lastMineThrow < 5000)
            //%dtStats.mineDiscShots++;
         %dtStats.discShotsFired++;
         %dtStats.discACC = (%dtStats.discHits / (%dtStats.discShotsFired ? %dtStats.discShotsFired : 1)) * 100;
         %dtStats.discDmgACC = (%dtStats.discDmgHits / (%dtStats.discShotsFired ? %dtStats.discShotsFired : 1)) * 100;
      case $DamageType::Grenade:
         %dtStats.grenadeShotsFired++;
         %dtStats.grenadeACC = (%dtStats.grenadeHits / (%dtStats.grenadeShotsFired ? %dtStats.grenadeShotsFired : 1)) * 100;
         %dtStats.grenadeDmgACC = (%dtStats.grenadeDmgHits / (%dtStats.grenadeShotsFired ? %dtStats.grenadeShotsFired : 1)) * 100;
      case $DamageType::Laser:
         %dtStats.laserShotsFired++;
         %dtStats.laserACC = (%dtStats.laserHits / (%dtStats.laserShotsFired ? %dtStats.laserShotsFired : 1)) * 100;
      case $DamageType::Mortar:
         %dtStats.mortarShotsFired++;
         %dtStats.mortarACC = (%dtStats.mortarHits / (%dtStats.mortarShotsFired ? %dtStats.mortarShotsFired : 1)) * 100;
         %dtStats.mortarDmgACC = (%dtStats.mortarDmgHits / (%dtStats.mortarShotsFired ? %dtStats.mortarShotsFired : 1)) * 100;
      case $DamageType::Missile:
         %dtStats.missileShotsFired++;
         %dtStats.missileACC = (%dtStats.missileHits / (%dtStats.missileShotsFired ? %dtStats.missileShotsFired : 1)) * 100;
      case $DamageType::ShockLance:
         %dtStats.shockShotsFired++;
         %dtStats.shockACC = (%dtStats.shockHits / (%dtStats.shockShotsFired ? %dtStats.shockShotsFired : 1)) * 100;
      case $DamageType::Plasma:
         %dtStats.plasmaShotsFired++;
         %dtStats.plasmaACC = (%dtStats.plasmaHits / (%dtStats.plasmaShotsFired ? %dtStats.plasmaShotsFired : 1)) * 100;
          %dtStats.plasmaDmgACC = (%dtStats.plasmaDmgHits / (%dtStats.plasmaShotsFired ? %dtStats.plasmaShotsFired : 1)) * 100;
      case $DamageType::Blaster:
         %dtStats.blasterShotsFired++;
         %dtStats.blasterACC = (%dtStats.blasterHits / (%dtStats.blasterShotsFired ? %dtStats.blasterShotsFired : 1)) * 100;
      case $DamageType::ELF:
         %dtStats.elfShotsFired++;
   }
}
////////////////////////////////////////////////////////////////////////////////
//								Menu Stuff									  //
////////////////////////////////////////////////////////////////////////////////
function getGameData(%game,%client,%var,%type,%value){
   if(%type $= "game"){
      %total = getField(%client.dtStats.gameStats[%var,"g",%game],%value);
      if(%total !$= "")
         return mFloatLength(%total,2) + 0;
      else
         error("Error getGameData" SPC %game SPC %client SPC %var SPC %type SPC %value);  
   }
   else if(%type $= "total"){
      %total = getField(%client.dtStats.gameStats[%var,"t",%game],%value);
      if(strpos(%total,"%a") != -1){
        %total = getField(strreplace(%total,"%a","\t"),0);  
      }
      if(%total !$= "")
         return numReduce(%total,1);
      else
         error("Error getGameData" SPC %game SPC %client SPC %var SPC %type SPC %value);   
   }
   else if(%type $= "avg"){
      if(%client.dtStats.gameStats["totalGames","g",%game] != 0){
      %c = 0;
      %x = %client.dtStats.gameStats["statsOverWrite","g",%game];
      for(%i=0; %i < 16; %i++){
         %v = %x - %i;
         if(%v < 0)
            %v = $dtStats::MaxNumOfGames + %v;
         %num = getField(%client.dtStats.gameStats[%var,"g",%game],%v);
         if(%num $= ""){
            error("Error getGameData" SPC %game SPC %client SPC %var SPC %type SPC %value); 
            break;         
         }
         if(%num > 0 || %num < 0){
            %val += %num;
            %c++;
            if(%c >= %value)
               break;
         }
      }
      if(%c > 0)
         return numReduce(mCeil(%val / %c),1);
      }
   }
   return 0;
}
function getGameRunWinLossAvg(%client,%game){
      %winCount = getField(%vClient.dtStats.gameStats["winCount","t",%game],9);
      %lossCount =getField(%vClient.dtStats.gameStats["lossCount","t",%game],9);
      %total = %winCount + %lossCount;
      return (%winCount / %total) * 100 SPC (%lossCount / %total) * 100;
}

function numReduce(%num,%des){
   if(%num !$= ""){
      if(strPos(%num,"x") == -1){
         if(%num > 1000){
            %num =%num / 1000;
            %affix = "K";
            if(%num > 1000){
               %num = %num / 1000;
               %affix = "M";
                if(%num > 1000){
                  %num =  %num / 1000;
                  %affix = "G";
               }
            }
         }
         return mFloatLength(%num,%des)+0 @ %affix;
      }
      else{
         %n1 = strLen(%num);
         %c = 0;
         for(%x = 1; %x < %n1; %x++){
            %n = getSubStr(%num,(%n1 - %x)-1,1);
            %seg[%c] = %n @ %seg[%c]; 
            %l++;
            if(%x % 3 == 0){
               %c++; 
               %l = 0; 
            }
         }
         if(%l > 0)
            %c++;
            
         %end[2] = "K"; %end[3] = "M"; %end[4] = "G";  
         %end[5] = "T"; %end[6] = "P"; %end[7] = "E";  
         %end[8] = "Z"; %end[9] = "Y";     
         
         if(%c > 1 && %c < 10){
           %ln2 = strLen(%seg[%c-2]);
           if(%ln2 > 2)// trim it to kee it with in 7 char
             %seg[%c-2] = getSubStr(%seg[%c-2],0,2);
           if(%seg[%c-2] $= "0" || %seg[%c-2] $= "00")
              return  %seg[%c-1] @ %end[%c];
           else
              return  %seg[%c-1] @ "." @ %seg[%c-2] @ %end[%c];
         }
         else
            return %num;
      }
   }
   return 0; 
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
   
   %client.lastPage = 0; 
}
function clipStr(%str,%len){
   %slen = strLen(%str);
   if(%slen > %len){
      return getSubStr(%str,0,%len-2) @ "..";
   }
   return %str;
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
    %index = -1;
    
   %isTargetSelf = (%client == %vClient);
   %isAdmin = (%client.isAdmin || %client.isSuperAdmin);
   
   messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
  
   if(!isObject(%vClient)){// fail safe
      %menu = "Reset";
   }
   switch$(%menu){
      case "View":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @  getTaggedString(%vClient.name) @ "'s Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tReset\t%1>  Back</a>',%vClient);
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>  Main Options Menu");
         if($dtStats::Live)
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tLIVE\t%1>  + %2 Live Stats</a>',%vClient,$dtStats::gtNameShort[%game]);
         if($dtStats::Match)
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tCTFGame\t%1\t-1>  + %2 Match Stats</a>',%vClient,$dtStats::gtNameShort[%game]);
         if(%isTargetSelf || %isAdmin) {
            if($dtStats::KD)
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tKDA\t%1>  + %2 Yearly Totals</a>',%vClient,$dtStats::gtNameShort[%game]);
            
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            if($dtStats::Hist)
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tHISTORY\t%1\t1>  + Previous %2 Games</a>',%vClient,$dtStats::gtNameShort[%game]);
            if($dtStats::mapStats)
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tMAPLIST\t%1\t1\t%2-%3>  + %4 Map Leaderboards *Beta',%vClient,%game,$dtStats::curMonth,$dtStats::gtNameShort[%game]);   
            if($dtStats::day > 1)
                messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tLBOARDS\t%1\tday-%2\t0>  + %3 Daily Leaderboards *Beta</a>',%vClient,%game,$dtStats::gtNameShort[%game]);
            if($dtStats::week > 1)
                messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tLBOARDS\t%1\tweek-%2\t0>  + %3 Weekly Leaderboards *Beta</a>',%vClient,%game,$dtStats::gtNameShort[%game]);
            if($dtStats::month > 1)
                messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tLBOARDS\t%1\tmonth-%2\t0>  + %3 Monthly Leaderboards *Beta',%vClient,%game,$dtStats::gtNameShort[%game]);
            if($dtStats::quarter > 1)
                messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tLBOARDS\t%1\tquarter-%2\t0>  + %3 Quarterly Leaderboards *Beta</a>',%vClient,%game,$dtStats::gtNameShort[%game]);
            if($dtStats::year > 1)
                messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tS\tLBOARDS\t%1\tyear-%2\t0>  + %3 Yearly Leaderboards *Beta</a>',%vClient,%game,$dtStats::gtNameShort[%game]);
         }
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         if(%client.isSuperAdmin){
            %line = '<a:gamelink\tS\tSP\t%1\t1\t%2-%3>  + Server Admin Panel</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%game,1); 
         }
            
         for(%v = %index; %v < 13; %v++){messageClient( %client, 'SetLineHud', "", %tag, %index++, "");}
         if(%vClient.dtStats.gameStats["totalGames","g",%game.class] == 0)
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Stats update at the end of every map.');
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Based on the last" SPC %3 SPC "games.");
         //%line = '<just:center>Games Played = %3 Running Average = %1/%2 Overwrite Counter = %4';
         if(%vClient.dtStats.gameStats["totalGames","g",%game.class]> 1) {
            %line = '<just:center>Based on the last %1 games played.';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient.dtStats.gameStats["totalGames","g",%game.class]);
         }
         else{
            %line = '<just:center>Based on the last game played.';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         }
      case "SP":
         %opt1 = %client.GlArg4;
         %opt2 = %client.GlArg5;
         switch$(%opt1){
            case "buildStats":
               if(!$dtStats::building){
                  lStatsCycle(1);
                  %client.GlArg4 = 0; 
               }
               
            case "statsEnable":
               if(%opt2) 
                  $dtStats::Enable = 1; 
               else
                  $dtStats::Enable = 0; 
               %client.GlArg4 = 0;
            case "reset":
               $dtServer::serverHangTotal = 0;   
               $dtServer::serverHangMicro = 0;
               $dtServer::serverHangTime = 0; 
               $dtServer::serverHangLast = 0;
               $dtServer::hostHang = 0;
               $dtServer::hostTime = 0;   
               $dtServer::hostHangLast = 0;
               $dtServer::clientCrashCount = 0;
               %client.GlArg4 = 0; 
         } 
//------------------------------------------------------------------------------
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Server Panel");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
//------------------------------------------------------------------------------
         if($dtStats::Enable){
            %line = '<a:gamelink\tS\tSP\t%1\t%2\t%3>  + Disable Stats System</a> -  Note this will reset to default with server restart';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"statsEnable",0); 
         }
         else{
            %line = '<a:gamelink\tS\tSP\t%1\t%2\t%3>  + Enable Stats System</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"statsEnable",1);  
         }
//------------------------------------------------------------------------------
         if($dtStats::building){
            %line = '<color:00FF00> + Building Stats';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"buildStats",0); 
         }
         else{
            %time = (($dtServerVars::lastBuildTime !$= "") ? "<color:00FF00>" @ $dtServerVars::lastBuildTime : 0);
            %line = '<a:gamelink\tS\tSP\t%1\t%2\t%3>  + Force Build Stats</a> Last Build: %4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"buildStats",0,%time);  
         }
//------------------------------------------------------------------------------
         
         %line = '<a:gamelink\tS\tSV\t%1\t1\t%2-%3>  + Map Play Statistics</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%game,1); 
//------------------------------------------------------------------------------
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         %line = '<a:gamelink\tS\tSP\t%1\t%2\t%3>  + Reset Server Metrics</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"reset",0); 
         %a1 = ($dtServer::serverHangMap[cleanMapName($CurrentMission),%game] ? "<color:00FF00>" @ $dtServer::serverHangMap[cleanMapName($CurrentMission),%game] : 0); 
         %a2 = ($dtServer::serverHangTotal ? "<color:00FF00>" @ $dtServer::serverHangTotal : 0);
         %a3 = (($dtServer::serverHangLast !$= "") ? "<color:00FF00>" @ $dtServer::serverHangLast : 0);
         %a4 = ($dtServer::serverHangTime ? "<color:00FF00>" @ $dtServer::serverHangTime : 0);
 
         %b1 = ($dtServer::hostHangMap[cleanMapName($CurrentMission),%game] ? "<color:00FF00>" @ $dtServer::hostHangMap[cleanMapName($CurrentMission),%game] : 0); 
         %b2 = ($dtServer::hostHangTotal ? "<color:00FF00>" @ $dtServer::hostHangTotal : 0);
         %b3 = (($dtServer::hostHangLast !$= "") ? "<color:00FF00>" @ $dtServer::hostHangLast : 0);
         %b4 = ($dtServer::hostHangTime ? "<color:00FF00>" @ $dtServer::hostHangTime : 0);
         
         %c1 = ($dtStats::pingAvg ? $dtStats::pingAvg : 0);
         %cli = ($dtServer::clientCrashCount ? "<color:00FF00>" @ $dtServer::clientCrashCount : 0);
         %line = 'Server Ping Avg = %1 - Client Issues = %2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%c1@"ms",%cli); 
         
         %line = '<color:0befe7>Server Hangs - This Map = %1<color:0befe7> - All Time = %2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%a2); 
         %line = '<color:0befe7>Server Hangs - Time = %1<color:0befe7> - Delay Time =  %2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a3,%a4); 
         
         %line = '<color:0befe7>Host Hangs - This Map = %1<color:0befe7> - All Time = %2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%b1,%b2); 
         %line = '<color:0befe7>Host Hangs - Time = %1<color:0befe7> - Delay Time =  %2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%b3,%b4); 
         %id = new scriptObject(); %id.delete();
         %line = '<color:0befe7>ID Count - %1 out of 2147483647 %2%% Up Time - %3';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%id,mFloor((%id / 2147483647) * 100),formatTime(getSimTime())); 
         
         %max = 30;
         %limit = 5;
         %v = $dtServerVars::upTimeCount-1;
         %line = '<color:0befe7>Server Run Time History';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%limit);  
         for(%i = 0; %i < %max && %i < %limit ; %i++){
            %upTime = $dtServerVars::upTime[%v];
            if(%upTime !$= ""){
               %line = '<color:0befe7>%2: %1';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,strreplace(%upTime,"-"," "),%i+1);  
            }
            else
               break;
            if(%v-- == -1)
               %v = %max - 1;  
         }
      case "SV"://Server
         %vLPage = %client.GlArg4;
         %field5 = strreplace(%client.GlArg5,"-","\t");
         %client.lgame = %switch = getField(%field5,0);
         %client.cat = %cat = getField(%field5,1); 
         if(%vLPage == -1)
            %vLPage = %client.lastMapPage;  
         else
            %client.lastMapPage = %vLPage;

         %perPage = 14;// num of games listed per page
         if(%cat $= "R"){ 
            for(%i = 1; %i <= $mapID::countGame[%client.lgame]; %i++){
               %map = $mapID::IDNameGame[%i,%client.lgame];
               $dtServer::playCount[%map,%client.lgame] = 0;
               $dtServer::lastPlay[%map,%client.lgame] = 0;
               $dtServer::mapDisconnects[%map,%client.lgame] = 0;
               $dtServer::mapDisconnectsScore[%map,%client.lgame] = 0;
               $dtServer::mapReconnects[%map,%client.lgame] = 0;
               $dtServer::voteFor[%map,%client.lgame] = 0;
               $dtServer::skipCount[%map,%client.lgame] = 0;
               $dtServer::maxPlayers[%map,%client.lgame] = 0;
               $dtServer::clientCrash[%map,%client.lgame] = 0;
               $dtServer::serverHangMap[%map,%client.lgame] = 0;
               $dtServer::serverHangMapMicro[%map,%client.lgame] = 0;
               $dtServer::hostHangMap[%map,%client.lgame] = 0;
            }
            %client.cat = %cat = 1;
         }
         else if(%cat !$= "C"){
            if($dtStats::sortCat != %cat){ 
               for(%i = 1; %i <= $mapID::countGame[%client.lgame]; %i++){
                  %maxCount = %i; 
                  switch$(%cat){
                     case 1:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::playCount[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::playCount[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 1;
                     case 2:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::skipCount[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::skipCount[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 2;
                     case 3:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::voteFor[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::voteFor[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 3;
                     case 4:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::mapDisconnects[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::mapDisconnects[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                         $dtStats::sortCat = 4;
                     case 4.5:
                           for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                              %pc1 = $dtServer::mapReconnects[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                              %pc2 = $dtServer::mapReconnects[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                              if (%pc1 > %pc2)
                                 %maxCount = %j;  
                           }
                           %map1 = $mapID::IDNameGame[%i,%client.lgame];
                           %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                           $mapID::IDNameGame[%i,%client.lgame] = %map2;
                           $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                           $dtStats::sortCat = 4.5;
                     case 5:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::maxPlayers[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::maxPlayers[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 5;
                     case 6:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::hostHangMap[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::hostHangMap[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 6;
                     case 7:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::serverHangMap[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::serverHangMap[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 7;
                     default:
                        for (%j = %i+1; %j < $mapID::countGame[%client.lgame]; %j++){  
                           %pc1 = $dtServer::playCount[$mapID::IDNameGame[%j,%client.lgame],%client.lgame];
                           %pc2 = $dtServer::playCount[$mapID::IDNameGame[%maxCount,%client.lgame],%client.lgame];
                           if (%pc1 > %pc2)
                              %maxCount = %j;  
                        }
                        %map1 = $mapID::IDNameGame[%i,%client.lgame];
                        %map2 = $mapID::IDNameGame[%maxCount,%client.lgame];
                        $mapID::IDNameGame[%i,%client.lgame] = %map2;
                        $mapID::IDNameGame[%maxCount,%client.lgame] = %map1;
                        $dtStats::sortCat = 1;
                  }
               }
               error($dtStats::sortCat);
            }
            %client.GlArg5 = %client.lgame @ "-C";
         }
         
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Server Stats"); 
         %line = '<a:gamelink\tS\tSP\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a><just:right><a:gamelink\tS\tSV\t%1\t1\t%2-R><Reset Stats></a>';
         messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%client.lgame); 
      
         %line = '<tab:114,179,244,309,374,439,504><color:0befe7><font:univers condensed:18>%2</a>\t%3\t%4\t%5\t%6\t%7\t%8\t%9';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"Map Name",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "1" @ ">Play</a> ",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "2" @ ">Skips</a>",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "3" @ ">Votes</a> ",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "4" @ ">DC</a> / <a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "4.5" @ "> RC</a> ",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "5" @ ">Max-Plr</a> ",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "6" @ ">HostHangs</a> ",
         "<a:gamelink\tS\tSV\t" @ %vClient @ "\t1\t" @ %client.lgame @ "-" @ "7" @ ">ServerHang</a> ");
         for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z < $mapID::countGame[%client.lgame]; %z++){
            %map = $mapID::IDNameGame[%z+1,%client.lgame];
            %pc = $dtServer::playCount[%map,%client.lgame];
            %sc = $dtServer::skipCount[%map,%client.lgame]; 
            %vc = $dtServer::voteFor[%map,%client.lgame];
            %dc = $dtServer::mapDisconnects[%map,%client.lgame];
            %dcS = $dtServer::mapReconnects[%map,%client.lgame]; 
            %mp = $dtServer::maxPlayers[%map,%client.lgame];
            %cr = $dtServer::hostHangMap[%map,%client.lgame];
            %sh = $dtServer::serverHangMap[%map,%client.lgame];
            %v1 = %pc ? %pc : 0;  
            %v2 = %sc ? %sc : 0; 
            %v3 = %vc ? %vc : 0; 
            %v4 = %dc ? %dc : 0;  %v44 = %dcS ? %dcS : 0; 
            %v5 = %mp ? %mp : 0;  
            %v6 = %cr ? %cr : 0;  
            %v7 = %sh ? %sh : 0;  
            %line = '<tab:114,179,244,309,374,439,504><color:0befe7><font:univers condensed:18><clip:110>%1 %2</clip>\t<color:03d597>%3\t%4\t%5\t%6\t%7\t%8\t%9';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%z+1,%map,%v1,%v2,%v3,%v4 @ "/" @ %v44,%v5,%v6,%v7);
         }
         for(%i = %index; %i <  14; %i++)
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '');
         if($mapID::countGame[%client.lgame] > %perPage){
            if(%vLPage == 1){
               %line = '<color:0befe7><just:center><a:gamelink\tS\tSV\t%1\t%2\t%3-%4> Next Page --> </a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%vLPage + 1, %client.lgame, %cat);
            }
            else if(%vLPage * %perPage >= $mapID::countGame[%client.lgame]){
               %line = '<color:0befe7><just:center><a:gamelink\tS\tSV\t%1\t%2\t%3-%4> <-- Back Page</a>    <a:gamelink\tS\tSV\t%1\t1\t%3-%4><First Page></a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%vLPage - 1, %client.lgame, %cat);
            }
            else if(%vLPage > 1){
               %line = '<color:0befe7><just:center><a:gamelink\tS\tSV\t%1\t%2\t%4-%5> <-- Back Page <a:gamelink\tS\tSV\t%1\t%3\t%4-%5> Next Page --></a>    <a:gamelink\tS\tSV\t%1\t1\t%4-%5><First Page></a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%vLPage - 1,%vLPage + 1, %client.lgame, %cat);
            }
         }            
         %hasCount = 0;  %line = "";
         for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
            if($mapID::countGame[$dtStats::gameType[%i]] > 0 && $dtStats::gameType[%i] !$= %client.lgame){
               %hasCount++; 
               %line = %line @ "<a:gamelink\tS\tSV\t" @ %vClient @ "\t" @ 1 @ "\t" @ $dtStats::gameType[%i] @ "-" @ %cat @ ">[" @ $dtStats::gtNameShort[$dtStats::gameType[%i]]  @ "] </a>"; 
            }
         }
         if(%hasCount > 0)
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Switch Game Type" SPC %line);
 
      case "LakRabbitGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Stats\tGame\tRun Avg\tWeek Totals\tMonth Totals\tQuarter Totals\tYear Totals';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         
         %gameValue = getGameData(%game,%vClient,"killsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"killsTG","total",3);
         %month = getGameData(%game,%vClient,"killsTG","total",5);
         %quarter = getGameData(%game,%vClient,"killsTG","total",7);
         %year = getGameData(%game,%vClient,"killsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Kills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"deathsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"deathsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"deathsTG","total",3);
         %month = getGameData(%game,%vClient,"deathsTG","total",5);
         %quarter = getGameData(%game,%vClient,"deathsTG","total",7);
         %year = getGameData(%game,%vClient,"deathsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Deaths<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"scoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"scoreTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"scoreTG","total",3);
         %month = getGameData(%game,%vClient,"scoreTG","total",5);
         %quarter = getGameData(%game,%vClient,"scoreTG","total",7);
         %year = getGameData(%game,%vClient,"scoreTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Score<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"assistTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"assistTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"assistTG","total",3);
         %month = getGameData(%game,%vClient,"assistTG","total",5);
         %quarter = getGameData(%game,%vClient,"assistTG","total",7);
         %year = getGameData(%game,%vClient,"assistTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Assist<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"flagGrabsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagGrabsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"flagGrabsTG","total",3);
         %month = getGameData(%game,%vClient,"flagGrabsTG","total",5);
         %quarter = getGameData(%game,%vClient,"flagGrabsTG","total",7);
         %year = getGameData(%game,%vClient,"flagGrabsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Flag Grabs<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"flagTimeMinTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagTimeMinTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"flagTimeMinTG","total",3);
         %month = getGameData(%game,%vClient,"flagTimeMinTG","total",5);
         %quarter = getGameData(%game,%vClient,"flagTimeMinTG","total",7);
         %year = getGameData(%game,%vClient,"flagTimeMinTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Flag Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"MidairflagGrabsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"MidairflagGrabsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"MidairflagGrabsTG","total",3);
         %month = getGameData(%game,%vClient,"MidairflagGrabsTG","total",5);
         %quarter = getGameData(%game,%vClient,"MidairflagGrabsTG","total",7);
         %year = getGameData(%game,%vClient,"MidairflagGrabsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> MidAir Flag Grabs<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"airTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"airTimeTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"airTimeTG","total",3);
         %month = getGameData(%game,%vClient,"airTimeTG","total",5);
         %quarter = getGameData(%game,%vClient,"airTimeTG","total",7);
         %year = getGameData(%game,%vClient,"airTimeTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Air Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"groundTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"groundTimeTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"groundTimeTG","total",3);
         %month = getGameData(%game,%vClient,"groundTimeTG","total",5);
         %quarter = getGameData(%game,%vClient,"groundTimeTG","total",7);
         %year = getGameData(%game,%vClient,"groundTimeTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Ground Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"timeTLTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"timeTLTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"timeTLTG","total",3);
         %month = getGameData(%game,%vClient,"timeTLTG","total",5);
         %quarter = getGameData(%game,%vClient,"timeTLTG","total",7);
         %year = getGameData(%game,%vClient,"timeTLTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Survival Time Sec<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
      case "DMGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Stats\tGame\tRun Avg\tYear Totals\t\tArmor Vs Armor';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         
         %gameValue = getGameData(%game,%vClient,"killsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"killsTG","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Kills<color:03d597>\t%1\t%2\t%3\t| Kills\tLight\tMedium\tHeavy';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"deathsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"deathsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"deathsTG","total",9);
         %a1 = getGameData(%game,%vClient,"armorLLTG","game",%inc);
         %a2 = getGameData(%game,%vClient,"armorLMTG","game",%inc);
         %a3 = getGameData(%game,%vClient,"armorLHTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Deaths<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Light<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%a1,%a2,%a3);
         %gameValue = getGameData(%game,%vClient,"scoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"scoreTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"scoreTG","total",9);
         %b1 = getGameData(%game,%vClient,"armorMLTG","game",%inc);
         %b2 = getGameData(%game,%vClient,"armorMMTG","game",%inc);
         %b3 = getGameData(%game,%vClient,"armorMHTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Score<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Medium<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%b1,%b2,%b3);
         %gameValue = getGameData(%game,%vClient,"assistTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"assistTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"assistTG","total",9);
         %c1 = getGameData(%game,%vClient,"armorHLTG","game",%inc);
         %c2 = getGameData(%game,%vClient,"armorHMTG","game",%inc);
         %c3 = getGameData(%game,%vClient,"armorHHTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Assist<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Heavy<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%c1,%c2,%c3);
         %gameValue = getGameData(%game,%vClient,"efficiencyAvg","game",%inc);
         %avgValue = getGameData(%game,%vClient,"efficiencyAvg","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"efficiencyAvg","total",9);
         %d1 = %a1 + %b1 + %c3;
         %d2 = %a2 + %b2 + %c2;
         %d3 = %a3 + %b3 + %c3;
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Efficiency<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Total<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%d1,%d2,%d3);
         %gameValue = getGameData(%game,%vClient,"killStreakTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killStreakTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"killStreakTG","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Kill Streak<color:03d597>\t%1\t%2\t%3';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%a1,%a2,%a3);
         %gameValue = getGameData(%game,%vClient,"chainKillTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"chainKillTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"chainKillTG","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Chain Kills<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Deaths\tLight\tMedium\tHeavy';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"multiKillTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"multiKillTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"multiKillTG","total",9);
         %a1 = getGameData(%game,%vClient,"armorLLDTG","game",%inc);
         %a2 = getGameData(%game,%vClient,"armorLMDTG","game",%inc);
         %a3 = getGameData(%game,%vClient,"armorLHDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Multikills<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Light<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%a1,%a2,%a3);
         %gameValue = getGameData(%game,%vClient,"airTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"airTimeTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"airTimeTG","total",9);
         %b1 = getGameData(%game,%vClient,"armorMLDTG","game",%inc);
         %b2 = getGameData(%game,%vClient,"armorMMDTG","game",%inc);
         %b3 = getGameData(%game,%vClient,"armorMHDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Air Time<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Medium<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%b1,%b2,%b3);
         %gameValue = getGameData(%game,%vClient,"groundTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"groundTimeTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"groundTimeTG","total",9);
         %c1 = getGameData(%game,%vClient,"armorHLDTG","game",%inc);
         %c2 = getGameData(%game,%vClient,"armorHMDTG","game",%inc);
         %c3 = getGameData(%game,%vClient,"armorHHDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Ground Time<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Heavy<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%c1,%c2,%c3);
         %gameValue = getGameData(%game,%vClient,"timeTLTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"timeTLTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"timeTLTG","total",9);
         %d1 = %a1 + %b1 + %c3;
         %d2 = %a2 + %b2 + %c2;
         %d3 = %a3 + %b3 + %c3;
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Survival Time<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Total<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%d1,%d2,%d3);
         

      case "ArenaGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Stats\tGame\tRun Avg\tWeek Totals\tMonth Totals\tQuarter Totals\tYear Totals';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);

        
         %gameValue = getGameData(%game,%vClient,"killsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"killsTG","total",3);
         %month = getGameData(%game,%vClient,"killsTG","total",5);
         %quarter = getGameData(%game,%vClient,"killsTG","total",7);
         %year = getGameData(%game,%vClient,"killsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Kills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"deathsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"deathsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"deathsTG","total",3);
         %month = getGameData(%game,%vClient,"deathsTG","total",5);
         %quarter = getGameData(%game,%vClient,"deathsTG","total",7);
         %year = getGameData(%game,%vClient,"deathsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Deaths<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"scoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"scoreTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"scoreTG","total",3);
         %month = getGameData(%game,%vClient,"scoreTG","total",5);
         %quarter = getGameData(%game,%vClient,"scoreTG","total",7);
         %year = getGameData(%game,%vClient,"scoreTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Score<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"assistTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"assistTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"assistTG","total",3);
         %month = getGameData(%game,%vClient,"assistTG","total",5);
         %quarter = getGameData(%game,%vClient,"assistTG","total",7);
         %year = getGameData(%game,%vClient,"assistTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Assist<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"roundsWonTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"roundsWonTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"roundsWonTG","total",3);
         %month = getGameData(%game,%vClient,"roundsWonTG","total",5);
         %quarter = getGameData(%game,%vClient,"roundsWonTG","total",7);
         %year = getGameData(%game,%vClient,"roundsWonTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Rounds Won<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"roundsLostTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"roundsLostTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"roundsLostTG","total",3);
         %month = getGameData(%game,%vClient,"roundsLostTG","total",5);
         %quarter = getGameData(%game,%vClient,"roundsLostTG","total",7);
         %year = getGameData(%game,%vClient,"roundsLostTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Rounds Lost<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"roundKillsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"roundKillsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"roundKillsTG","total",3);
         %month = getGameData(%game,%vClient,"roundKillsTG","total",5);
         %quarter = getGameData(%game,%vClient,"roundKillsTG","total",7);
         %year = getGameData(%game,%vClient,"roundKillsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Round Kills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"hatTricksTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"hatTricksTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"hatTricksTG","total",3);
         %month = getGameData(%game,%vClient,"hatTricksTG","total",5);
         %quarter = getGameData(%game,%vClient,"hatTricksTG","total",7);
         %year = getGameData(%game,%vClient,"hatTricksTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Hat Tricks<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"airTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"airTimeTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"airTimeTG","total",3);
         %month = getGameData(%game,%vClient,"airTimeTG","total",5);
         %quarter = getGameData(%game,%vClient,"airTimeTG","total",7);
         %year = getGameData(%game,%vClient,"airTimeTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Air Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"groundTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"groundTimeTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"groundTimeTG","total",3);
         %month = getGameData(%game,%vClient,"groundTimeTG","total",5);
         %quarter = getGameData(%game,%vClient,"groundTimeTG","total",7);
         %year = getGameData(%game,%vClient,"groundTimeTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Ground Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"timeTLTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"timeTLTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"timeTLTG","total",3);
         %month = getGameData(%game,%vClient,"timeTLTG","total",5);
         %quarter = getGameData(%game,%vClient,"timeTLTG","total",7);
         %year = getGameData(%game,%vClient,"timeTLTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Survival Time Sec<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"multiKillTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"multiKillTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"multiKillTG","total",3);
         %month = getGameData(%game,%vClient,"multiKillTG","total",5);
         %quarter = getGameData(%game,%vClient,"multiKillTG","total",7);
         %year = getGameData(%game,%vClient,"multiKillTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Multikills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"chainKillTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"chainKillTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"chainKillTG","total",3);
         %month = getGameData(%game,%vClient,"chainKillTG","total",5);
         %quarter = getGameData(%game,%vClient,"chainKillTG","total",7);
         %year = getGameData(%game,%vClient,"chainKillTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Chain Kills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"killStreakTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killStreakTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"killStreakTG","total",3);
         %month = getGameData(%game,%vClient,"killStreakTG","total",5);
         %quarter = getGameData(%game,%vClient,"killStreakTG","total",7);
         %year = getGameData(%game,%vClient,"killStreakTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Kill Streak<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"firstKillTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"firstKillTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"firstKillTG","total",3);
         %month = getGameData(%game,%vClient,"firstKillTG","total",5);
         %quarter = getGameData(%game,%vClient,"firstKillTG","total",7);
         %year = getGameData(%game,%vClient,"firstKillTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> First Kill<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         
      case "DuelGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Stats\tGame\tRun Avg\tWeek Totals\tMonth Totals\tQuarter Totals\tYear Totals';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         
         %gameValue = getGameData(%game,%vClient,"killsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"killsTG","total",3);
         %month = getGameData(%game,%vClient,"killsTG","total",5);
         %quarter = getGameData(%game,%vClient,"killsTG","total",7);
         %year = getGameData(%game,%vClient,"killsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Kills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"deathsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"deathsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"deathsTG","total",3);
         %month = getGameData(%game,%vClient,"deathsTG","total",5);
         %quarter = getGameData(%game,%vClient,"deathsTG","total",7);
         %year = getGameData(%game,%vClient,"deathsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Deaths<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"scoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"scoreTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"scoreTG","total",3);
         %month = getGameData(%game,%vClient,"scoreTG","total",5);
         %quarter = getGameData(%game,%vClient,"scoreTG","total",7);
         %year = getGameData(%game,%vClient,"scoreTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Score<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"suicidesTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"suicidesTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"suicidesTG","total",3);
         %month = getGameData(%game,%vClient,"suicidesTG","total",5);
         %quarter = getGameData(%game,%vClient,"suicidesTG","total",7);
         %year = getGameData(%game,%vClient,"suicidesTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Suicides<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"distMovTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"distMovTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"distMovTG","total",3);
         %month = getGameData(%game,%vClient,"distMovTG","total",5);
         %quarter = getGameData(%game,%vClient,"distMovTG","total",7);
         %year = getGameData(%game,%vClient,"distMovTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Dist Moved<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"airTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"airTimeTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"airTimeTG","total",3);
         %month = getGameData(%game,%vClient,"airTimeTG","total",5);
         %quarter = getGameData(%game,%vClient,"airTimeTG","total",7);
         %year = getGameData(%game,%vClient,"airTimeTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Air Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"groundTimeTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"groundTimeTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"groundTimeTG","total",3);
         %month = getGameData(%game,%vClient,"groundTimeTG","total",5);
         %quarter = getGameData(%game,%vClient,"groundTimeTG","total",7);
         %year = getGameData(%game,%vClient,"groundTimeTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Ground Time Min<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"timeTLTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"timeTLTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"timeTLTG","total",3);
         %month = getGameData(%game,%vClient,"timeTLTG","total",5);
         %quarter = getGameData(%game,%vClient,"timeTLTG","total",7);
         %year = getGameData(%game,%vClient,"timeTLTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Survival Time Sec<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
                 
      case "CTFGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
            //%header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
            //%header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Stats\tGame\tRun Avg\tYear Totals\t\tArmor Vs Armor';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         %gameValue = getGameData(%game,%vClient,"winCountTG","game",%inc);
         %avgValue1 = getGameData(%game,%vClient,"winCountTG","avg",$dtStats::avgCount);
         %year1 = getGameData(%game,%vClient,"winCountTG","total",9);
         %avgValue2 = getGameData(%game,%vClient,"lossCountTG","avg",$dtStats::avgCount);
         %year2 = getGameData(%game,%vClient,"lossCountTG","total",9);
         %totalWinLoss = %avgValue1 +  %avgValue2;
         %avgValue = mFloor((%avgValue1 / %totalWinLoss)* 100) @ "%";
         %totalWinLoss = %year1 +  %year2;
         %year = mFloor((%year1 / %totalWinLoss)* 100) @ "%";
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Win / Lost<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Kills\tLight\tMedium\tHeavy';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"killsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"killsTG","total",9);
         %a1 = getGameData(%game,%vClient,"armorLLTG","game",%inc);
         %a2 = getGameData(%game,%vClient,"armorLMTG","game",%inc);
         %a3 = getGameData(%game,%vClient,"armorLHTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Kills<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Light<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%a1,%a2,%a3);
         %gameValue = getGameData(%game,%vClient,"deathsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"deathsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"deathsTG","total",9);
         %b1 = getGameData(%game,%vClient,"armorMLTG","game",%inc);
         %b2 = getGameData(%game,%vClient,"armorMMTG","game",%inc);
         %b3 = getGameData(%game,%vClient,"armorMHTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Deaths<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Medium<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%b1,%b2,%b3);
         %gameValue = getGameData(%game,%vClient,"scoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"scoreTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"scoreTG","total",9);
         %c1 = getGameData(%game,%vClient,"armorHLTG","game",%inc);
         %c2 = getGameData(%game,%vClient,"armorHMTG","game",%inc);
         %c3 = getGameData(%game,%vClient,"armorHHTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Score<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Heavy<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%c1,%c2,%c3);
         %gameValue = getGameData(%game,%vClient,"offenseScoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"offenseScoreTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"offenseScoreTG","total",9);
         %d1 = %a1 + %b1 + %c1;
         %d2 = %a2 + %b2 + %c2;
         %d3 = %a3 + %b3 + %c3;
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Offense Score<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Total<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%d1,%d2,%d3);
         %gameValue = getGameData(%game,%vClient,"defenseScoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"defenseScoreTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"defenseScoreTG","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Defense Score<color:03d597>\t%1\t%2\t%3';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"flagCapsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagCapsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"flagCapsTG","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Flag Caps<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Deaths\tLight\tMedium\tHeavy';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"flagGrabsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagGrabsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"flagGrabsTG","total",9);
         %a1 = getGameData(%game,%vClient,"armorLLDTG","game",%inc);
         %a2 = getGameData(%game,%vClient,"armorLMDTG","game",%inc);
         %a3 = getGameData(%game,%vClient,"armorLHDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Flag Grabs<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Light<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%a1,%a2,%a3);
         %gameValue = getGameData(%game,%vClient,"flagReturnsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagReturnsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"flagReturnsTG","total",9);
         %b1 = getGameData(%game,%vClient,"armorLMDTG","game",%inc);
         %b2 = getGameData(%game,%vClient,"armorMMDTG","game",%inc);
         %b3 = getGameData(%game,%vClient,"armorMHDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Flag Returns<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Medium<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%b1,%b2,%b3);
         %gameValue = getGameData(%game,%vClient,"carrierKillsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"carrierKillsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"carrierKillsTG","total",9);
         %c1 = getGameData(%game,%vClient,"armorLHDTG","game",%inc);
         %c2 = getGameData(%game,%vClient,"armorMHDTG","game",%inc);
         %c3 = getGameData(%game,%vClient,"armorHHDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Carrier Kills<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Heavy<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%c1,%c2,%c3);
         %gameValue = getGameData(%game,%vClient,"flagDefendsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagDefendsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"flagDefendsTG","total",9);
         %d1 = %a1 + %b1 + %c1;
         %d2 = %a2 + %b2 + %c2;
         %d3 = %a3 + %b3 + %c3;
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Flag Defends<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Total<color:03d597>\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%d1,%d2,%d3);
         %gameValue = getGameData(%game,%vClient,"escortAssistsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"escortAssistsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"escortAssistsTG","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Escort Assists<color:03d597>\t%1\t%2\t%3';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"heldTimeSecMin","game",%inc);
         %avgValue = getGameData(%game,%vClient,"heldTimeSecMin","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"heldTimeSecMin","total",9);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Capture Time<color:03d597>\t%1\t%2\t%3\t\t<color:0befe7>Vehicles Stats - Kills/Deaths';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year);
         %gameValue = getGameData(%game,%vClient,"grabSpeedMax","game",%inc);
         %avgValue = getGameData(%game,%vClient,"grabSpeedMax","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"grabSpeedMax","total",9);
         %v1 = getGameData(%game,%vClient,"wildRKTG","game",%inc) @ "/" @  getGameData(%game,%vClient,"wildRDTG","game",%inc);
         %v2 = getGameData(%game,%vClient,"mobileBaseRKTG","game",%inc) @ "/" @  getGameData(%game,%vClient,"mobileBaseRDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Grab Speed<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Wild Cat<color:03d597>\t%4\t<color:0befe7>| MPB<color:03d597>\t%5';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%v1,%v2);
         %gameValue = getGameData(%game,%vClient,"destructionTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"destructionTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"destructionTG","total",9);
         %v1 = getGameData(%game,%vClient,"scoutFlyerRKTG","game",%inc) + getGameData(%game,%vClient,"shrikeBlasterKillsTG","game",%inc) @ "/" @  
         getGameData(%game,%vClient,"scoutFlyerRDTG","game",%inc) + getGameData(%game,%vClient,"shrikeBlasterDeathsTG","game",%inc);
         %v2 = getGameData(%game,%vClient,"bomberFlyerRKTG","game",%inc) + getGameData(%game,%vClient,"bomberBombsKillsTG","game",%inc) + getGameData(%game,%vClient,"bellyTurretKillsTG","game",%inc) @ "/" @ 
         getGameData(%game,%vClient,"bomberFlyerRDTG","game",%inc) + getGameData(%game,%vClient,"bomberBombsDeathsTG","game",%inc) + getGameData(%game,%vClient,"bellyTurretDeathsTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Item Destruction<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Shrike<color:03d597>\t%4\t<color:0befe7>| Bomber<color:03d597>\t%5';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%v1,%v2);
         %gameValue = getGameData(%game,%vClient,"repairsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"repairsTG","avg",$dtStats::avgCount);
         %year = getGameData(%game,%vClient,"repairsTG","total",9);
         %v1 = getGameData(%game,%vClient,"assaultRKTG","game",%inc) + getGameData(%game,%vClient,"tankChaingunKillsTG","game",%inc)  + getGameData(%game,%vClient,"tankMortarKillsTG","game",%inc) @ "/" @ 
         getGameData(%game,%vClient,"assaultRDTG","game",%inc) + getGameData(%game,%vClient,"tankChaingunDeathsTG","game",%inc) + getGameData(%game,%vClient,"tankMortarDeathsTG","game",%inc) ;
         %v2 = getGameData(%game,%vClient,"hapcFlyerRKTG","game",%inc) @ "/" @  getGameData(%game,%vClient,"hapcFlyerRDTG","game",%inc);
         %line = '<tab:114,179,244,309,374,439,504><font:univers condensed:18><color:0befe7> Item Repairs<color:03d597>\t%1\t%2\t%3\t<color:0befe7>| Beowulf<color:03d597>\t%4\t<color:0befe7>| Havoc<color:03d597>\t%5';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%year,%v1,%v2);
         
      case "SCtFGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         }
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Stats\tGame\tRun Avg\tWeek\tMonth\tQuarter\tYear';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
            
         %gameValue = getGameData(%game,%vClient,"winCountTG","game",%inc);
         
         %avgValue1 = getGameData(%game,%vClient,"winCountTG","avg",$dtStats::avgCount);
         %avgValue2 = getGameData(%game,%vClient,"lossCountTG","avg",$dtStats::avgCount);
         %totalWinLoss = %avgValue1 +  %avgValue2;
         %avgValue = mFloor((%avgValue1 / %totalWinLoss)* 100) @ "%";
         
         %win = getGameData(%game,%vClient,"winCountTG","total",3);
         %loss = getGameData(%game,%vClient,"lossCountTG","total",3);
         %totalWinLoss = %win +  %loss;
         %week = mFloor((%win / %totalWinLoss)* 100) @ "%";
         
         %win = getGameData(%game,%vClient,"winCountTG","total",5);
         %loss = getGameData(%game,%vClient,"lossCountTG","total",5);
         %totalWinLoss = %win +  %loss;
         %month = mFloor((%win / %totalWinLoss)* 100) @ "%";
         
         %win = getGameData(%game,%vClient,"winCountTG","total",7);
         %loss = getGameData(%game,%vClient,"lossCountTG","total",7);
         %totalWinLoss = %win +  %loss;
         %quarter = mFloor((%win / %totalWinLoss)* 100) @ "%";
         
         %win = getGameData(%game,%vClient,"winCountTG","total",9);
         %loss = getGameData(%game,%vClient,"lossCountTG","total",9);
         %totalWinLoss = %win +  %loss;
         %year = mFloor((%win / %totalWinLoss)* 100) @ "%";
         
         
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Win / Lost<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"killsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"killsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"killsTG","total",3);
         %month = getGameData(%game,%vClient,"killsTG","total",5);
         %quarter = getGameData(%game,%vClient,"killsTG","total",7);
         %year = getGameData(%game,%vClient,"killsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Kills<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"deathsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"deathsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"deathsTG","total",3);
         %month = getGameData(%game,%vClient,"deathsTG","total",5);
         %quarter = getGameData(%game,%vClient,"deathsTG","total",7);
         %year = getGameData(%game,%vClient,"deathsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Deaths<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"scoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"scoreTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"scoreTG","total",3);
         %month = getGameData(%game,%vClient,"scoreTG","total",5);
         %quarter = getGameData(%game,%vClient,"scoreTG","total",7);
         %year = getGameData(%game,%vClient,"scoreTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Score<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"assistTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"assistTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"assistTG","total",3);
         %month = getGameData(%game,%vClient,"assistTG","total",5);
         %quarter = getGameData(%game,%vClient,"assistTG","total",7);
         %year = getGameData(%game,%vClient,"assistTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Assist<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"offenseScoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"offenseScoreTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"offenseScoreTG","total",3);
         %month = getGameData(%game,%vClient,"offenseScoreTG","total",5);
         %quarter = getGameData(%game,%vClient,"offenseScoreTG","total",7);
         %year = getGameData(%game,%vClient,"offenseScoreTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Offense Score<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"defenseScoreTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"defenseScoreTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"defenseScoreTG","total",3);
         %month = getGameData(%game,%vClient,"defenseScoreTG","total",5);
         %quarter = getGameData(%game,%vClient,"defenseScoreTG","total",7);
         %year = getGameData(%game,%vClient,"defenseScoreTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Defense Score<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"flagCapsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagCapsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"flagCapsTG","total",3);
         %month = getGameData(%game,%vClient,"flagCapsTG","total",5);
         %quarter = getGameData(%game,%vClient,"flagCapsTG","total",7);
         %year = getGameData(%game,%vClient,"flagCapsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Flag Caps<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"flagGrabsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagGrabsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"flagGrabsTG","total",3);
         %month = getGameData(%game,%vClient,"flagGrabsTG","total",5);
         %quarter = getGameData(%game,%vClient,"flagGrabsTG","total",7);
         %year = getGameData(%game,%vClient,"flagGrabsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Flag Grabs<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"flagReturnsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagReturnsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"flagReturnsTG","total",3);
         %month = getGameData(%game,%vClient,"flagReturnsTG","total",5);
         %quarter = getGameData(%game,%vClient,"flagReturnsTG","total",7);
         %year = getGameData(%game,%vClient,"flagReturnsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Flag Returns<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"flagDefendsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"flagDefendsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"flagDefendsTG","total",3);
         %month = getGameData(%game,%vClient,"flagDefendsTG","total",5);
         %quarter = getGameData(%game,%vClient,"flagDefendsTG","total",7);
         %year = getGameData(%game,%vClient,"flagDefendsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Flag Defends<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"v","game",%inc);
         %avgValue = getGameData(%game,%vClient,"escortAssistsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"escortAssistsTG","total",3);
         %month = getGameData(%game,%vClient,"escortAssistsTG","total",5);
         %quarter = getGameData(%game,%vClient,"escortAssistsTG","total",7);
         %year = getGameData(%game,%vClient,"escortAssistsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Escort Assists<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"heldTimeSecMin","game",%inc);
         %avgValue = getGameData(%game,%vClient,"heldTimeSecMin","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"heldTimeSecMin","total",3);
         %month = getGameData(%game,%vClient,"heldTimeSecMin","total",5);
         %quarter = getGameData(%game,%vClient,"heldTimeSecMin","total",7);
         %year = getGameData(%game,%vClient,"heldTimeSecMin","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Capture Time<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"grabSpeedMax","game",%inc);
         %avgValue = getGameData(%game,%vClient,"grabSpeedMax","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"grabSpeedMax","total",3);
         %month = getGameData(%game,%vClient,"grabSpeedMax","total",5);
         %quarter = getGameData(%game,%vClient,"grabSpeedMax","total",7);
         %year = getGameData(%game,%vClient,"grabSpeedMax","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Grab Speed<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"destructionTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"destructionTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"destructionTG","total",3);
         %month = getGameData(%game,%vClient,"destructionTG","total",5);
         %quarter = getGameData(%game,%vClient,"destructionTG","total",7);
         %year = getGameData(%game,%vClient,"destructionTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Item Destruction<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         %gameValue = getGameData(%game,%vClient,"repairsTG","game",%inc);
         %avgValue = getGameData(%game,%vClient,"repairsTG","avg",$dtStats::avgCount);
         %week = getGameData(%game,%vClient,"repairsTG","total",3);
         %month = getGameData(%game,%vClient,"repairsTG","total",5);
         %quarter = getGameData(%game,%vClient,"repairsTG","total",7);
         %year = getGameData(%game,%vClient,"repairsTG","total",9);
         %line = '<tab:114,179,244,324,404,484><font:univers condensed:18><color:0befe7> Item Repairs<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%gameValue,%avgValue,%week,%month,%quarter,%year);
         
      case "HISTORY":// Past Games
         %page = %client.GlArg4;
         if(%page == 0){ // back button was hit
            %page = %client.lastPage; // set it to the last one we were on 
         }
         if(%page $= ""){
            %page = 1;  
         }
         %client.lastPage = %page; // update with current page
         %perPage = 12;// num of games listed per page 
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s " @ $MissionTypeDisplayName @ " History");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Game history is set to" SPC $dtStats::MaxNumOfGames SPC "games.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>The oldest game will be overwritten.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         if(%vClient.dtStats.gameStats["fullSet","g",%game]){
            %x = ($dtStats::MaxNumOfGames-1) - %vClient.dtStats.gameStats["statsOverWrite","g",%game];//offset 
            for (%i = ($dtStats::MaxNumOfGames-1)-((%page - 1) * %perPage); %i > ($dtStats::MaxNumOfGames-1)-(%page  * %perPage) && %i >=0; %i--){  
               %v = %i - %x; //3 2 1 0
               if(%v < 0){ // invert 
                  %v = $dtStats::MaxNumOfGames + %v; //6 5 4  
               }   
               //echo(%v SPC ($dtStats::MaxNumOfGames-1)-((%page - 1) * %perPage));
               if(%i == 0){
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%v);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%v);
                  %line =  '<lmargin:10><color:0befe7>%4 - %2<color:02d404> - Overwritten<color:0befe7><lmargin:350><a:gamelink\tS\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tS\tWEAPON\t%1\t%3\tgame> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%timeDate,%v,%map,%game); 
               }
               else{
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%v);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%v);
                  %line = '<lmargin:10><color:0befe7>%4 - %2<lmargin:350><a:gamelink\tS\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tS\tWEAPON\t%1\t%3\tgame> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%timeDate,%v,%map,%game);  
               }
            }
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
		    if(%page == 1){
               %line = '<color:0befe7></a><lmargin:200><just:right><a:gamelink\tS\tHISTORY\t%1\t%2>Next</a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page + 1);
            }
            else if(%page * %perPage > $dtStats::MaxNumOfGames){
               %line = '<color:0befe7></a><lmargin:200><just:right><a:gamelink\tS\tHISTORY\t%1\t%2>Previous</a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1);
            }
            else if(%page > 1){
               %line = '<color:0befe7><lmargin:200><just:right><a:gamelink\tS\tHISTORY\t%1\t%2>Previous</a> | <a:gamelink\tS\tHISTORY\t%1\t%3>Next</a>';
                messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1,%page + 1);
            }
         }
         else{
            if(%vClient.dtStats.gameStats["statsOverWrite","g",%game] > 9){
               if(%page == 1){
                  %line = '<color:0befe7></a><lmargin:300><a:gamelink\tS\tHISTORY\t%1\t%2>Next</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page + 1);
               }
               else if(%page * %perPage > %vClient.dtStats.gameStats["statsOverWrite","g",%game]){
                  %line = '<color:0befe7></a><lmargin:300><a:gamelink\tS\tHISTORY\t%1\t%2>Previous</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1);
               }
               else if(%page > 1){
                  %line = '<color:0befe7><lmargin:250><a:gamelink\tS\tHISTORY\t%1\t%2>Previous</a> | <lmargin:300><a:gamelink\tS\tHISTORY\t%1\t%3>Next</a>';
                   messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1,%page + 1);
               }
               %gc = %vClient.dtStats.gameStats["statsOverWrite","g",%game];
               for(%z = (%page - 1) * %perPage; %z < %page * %perPage && %z <= %gc; %z++){
                  %v = %gc - %z;//temp fix just inverts it becuase.... im lazy 
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%v);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%v);
                  %line = '<lmargin:10><color:0befe7>%4 - %2<lmargin:350><a:gamelink\tS\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tS\tWEAPON\t%1\t%3\tgame> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%timeDate,%v,%map,%game);
               }
            }
            else{
                for(%z =%vClient.dtStats.gameStats["statsOverWrite","g",%game]; %z >= 0; %z--){
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%z);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%z);
                  %line = '<lmargin:10><color:0befe7>%4 - %2<lmargin:350><a:gamelink\tS\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tS\tWEAPON\t%1\t%3\tgame> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%timeDate,%z,%map,%game);
               }
            }
         }
      //case "KDA":
         //%inc = 9;// in case we want to be  able to switch 
         //messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Yearly Totals");
         //messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
         //%type = "total";
         //
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Weapons\tKills\tDeaths\tMidAirs\tCombos\tMax Dist\tSpeed\tAvg Acc\tDmg';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         //
         //%kills = getGameData(%game,%vClient,"blasterKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"blasterDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"blasterMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"blasterComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"blasterKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"blasterHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"blasterACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"blasterDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Blaster<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"plasmaKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"plasmaDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"plasmaAoeMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"plasmaComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"plasmaKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"plasmaHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"plasmaACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"plasmaDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Plasma Rifle<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"cgKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"cgDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"cgMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"cgComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"cgKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"cgHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"cgACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"cgDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Chaingun<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"discKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"discDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"discAoeMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"discComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"discKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"discHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"discACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"discDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Spinfusor<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"grenadeKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"grenadeDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"grenadeAoeMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"grenadeComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"grenadeKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"grenadeHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"grenadeACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"grenadeDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Grenade Launcher<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"laserKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"laserDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"laserMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"laserComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"laserKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"laserHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"laserACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"laserDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Laser Rifle<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"mortarKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"mortarDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"mortarAoeMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"mortarComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"mortarKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"mortarHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"mortarACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"mortarDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Fusion Mortar<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"missileKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"missileDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"missileMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"missileComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"missileKillDistMax",%type,%inc);
         //%speed = getGameData(%game,%vClient,"missileHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"missileACCAvg",%type,%inc);
         //%dmg = getGameData(%game,%vClient,"missileDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Missile Launcher<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"shockKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"shockDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"shockMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"shockComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"shockKillDistMax",%type,%inc);
         //%speed = getGameData(%game,%vClient,"shockHitSVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"shockACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"shockDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Shocklance<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"hGrenadeKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"hGrenadeDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"hGrenadeMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"hGrenadeComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"hGrenadeKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"hGrenadeHitVVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"hGrenadeACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"hGrenadeDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Hand Grenade<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"mineKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"mineDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"mineMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"mineComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"mineKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"mineHitVVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"mineACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"mineDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Mine<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         //
         //%kills = getGameData(%game,%vClient,"satchelKillsTG",%type,%inc);
         //%deaths = getGameData(%game,%vClient,"satchelDeathsTG",%type,%inc);
         //%ma = getGameData(%game,%vClient,"satchelMATG",%type,%inc);
         //%com = getGameData(%game,%vClient,"satchelComTG",%type,%inc);
         //%maxDist = getGameData(%game,%vClient,"satchelKillDistMax",%type,%inc);
         //%speed= getGameData(%game,%vClient,"satchelHitVVMax",%type,%inc);
         //%avgACC = getGameData(%game,%vClient,"satchelACCAvg",%type,%inc);
         //%dmg= getGameData(%game,%vClient,"satchelDmgTG",%type,%inc);
         //%line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Satchel Charge<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
                  //
     
      case "WEAPON":// Weapons
         %inc = %client.GlArg4;
         %type = %client.GlArg5;
         if(%type $= "game")
            %client.inc = %inc;
            
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient);
            switch$(%type){
               case "game":
                  %line = '<just:center><font:univers condensed:18><color:0befe7><a:gamelink\tS\tWEAPON\t%1\t%2\t%3>|Running Averages|</a> <a:gamelink\tS\tWEAPON\t%1\t%4\t%5>|Year Totals|</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,$dtStats::avgCount,"avg",9,"total");
               case "total":
                  %line = '<just:center><font:univers condensed:18><color:0befe7><a:gamelink\tS\tWEAPON\t%1\t%2\t%3>|Game Stats|</a> <a:gamelink\tS\tWEAPON\t%1\t%4\t%5>|Running Averages|</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%client.inc,"game",$dtStats::avgCount,"avg");
               case "avg":
                  %line = '<just:center><font:univers condensed:18><color:0befe7><a:gamelink\tS\tWEAPON\t%1\t%2\t%3>|Game Stats|</a> <a:gamelink\tS\tWEAPON\t%1\t%4\t%5>|Year Totals|</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%client.inc,"game",9,"total");
            }

         
         %a1 = getGameData(%game,%vClient,"minePlusDiscTG",%type,%inc);
         %b2 = getGameData(%game,%vClient,"killAirTG",%type,%inc);
         %c3 = getGameData(%game,%vClient,"killGroundTG",%type,%inc);
         %d4 = getGameData(%game,%vClient,"EVKillsTG",%type,%inc);
         %line = '<tab:0,145,290,435><font:univers condensed:18><color:0befe7> \tMine+Disc: <color:03d597>%1<color:0befe7> \tAir kills: <color:03d597>%2<color:0befe7> \tGround Kills: <color:03d597>%3<color:0befe7> \tEV Kills: <color:03d597>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         %a1 = getGameData(%game,%vClient,"laserHeadShotTG",%type,%inc);
         %b2 = getGameData(%game,%vClient,"shockRearShotTG",%type,%inc);
         %c3 = getGameData(%game,%vClient,"shotsFiredTG",%type,%inc);
         %d4 = getGameData(%game,%vClient,"elfShotsFiredTG",%type,%inc);
         %line = '<tab:0,145,290,435><font:univers condensed:18><color:0befe7> \tHeadShots:  <color:03d597>%1<color:0befe7>\tRearShots:  <color:03d597>%2<color:0befe7>\tShots Fired:  <color:03d597>%3<color:0befe7>\tELF Usage:  <color:03d597>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
      
        
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Weapons\tKills\tDeaths\tMidAirs\tCombos\tMax Dist\tSpeed\tAvg Acc\tDmg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         
         %kills = getGameData(%game,%vClient,"blasterKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"blasterDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"blasterMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"blasterComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"blasterKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"blasterHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"blasterACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"blasterDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Blaster<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"plasmaKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"plasmaDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"plasmaMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"plasmaComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"plasmaKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"plasmaHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"plasmaACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"plasmaDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Plasma Rifle<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"cgKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"cgDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"cgMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"cgComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"cgKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"cgHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"cgACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"cgDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Chaingun<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"discKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"discDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"discMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"discComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"discKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"discHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"discACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"discDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Spinfusor<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"grenadeKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"grenadeDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"grenadeMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"grenadeComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"grenadeKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"grenadeHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"grenadeACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"grenadeDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Grenade Launcher<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"laserKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"laserDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"laserMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"laserComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"laserKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"laserHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"laserACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"laserDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Laser Rifle<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"mortarKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"mortarDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"mortarMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"mortarComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"mortarKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"mortarHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"mortarACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"mortarDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Fusion Mortar<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"missileKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"missileDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"missileMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"missileComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"missileKillDistMax",%type,%inc);
         %speed = getGameData(%game,%vClient,"missileHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"missileACCAvg",%type,%inc);
         %dmg = getGameData(%game,%vClient,"missileDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Missile Launcher<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"shockKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"shockDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"shockMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"shockComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"shockKillDistMax",%type,%inc);
         %speed = getGameData(%game,%vClient,"shockHitSVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"shockACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"shockDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Shocklance<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"hGrenadeKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"hGrenadeDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"hGrenadeMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"hGrenadeComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"hGrenadeKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"hGrenadeHitVVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"hGrenadeACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"hGrenadeDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Hand Grenade<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"mineKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"mineDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"mineMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"mineComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"mineKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"mineHitVVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"mineACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"mineDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Mine<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
         
         %kills = getGameData(%game,%vClient,"satchelKillsTG",%type,%inc);
         %deaths = getGameData(%game,%vClient,"satchelDeathsTG",%type,%inc);
         %ma = getGameData(%game,%vClient,"satchelMATG",%type,%inc);
         %com = getGameData(%game,%vClient,"satchelComTG",%type,%inc);
         %maxDist = getGameData(%game,%vClient,"satchelKillDistMax",%type,%inc);
         %speed= getGameData(%game,%vClient,"satchelHitVVMax",%type,%inc);
         %avgACC = getGameData(%game,%vClient,"satchelACCAvg",%type,%inc);
         %dmg= getGameData(%game,%vClient,"satchelDmgTG",%type,%inc);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Satchel Charge<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%speed,%avgACC,%dmg);
                  
      case "LIVE":
         %inc = %client.GlArg4;
         %cycle = %client.GlArg5; 
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Live Stats");
         if(%inc $= "pin"){
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tS\tLIVE\t%1\t-1>Unpin Screen</a> - Games Played: %2',%vClient,%vClient.dtStats.gameStats["totalGames","g",%game]);
         }
         else{
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tS\tLIVE\t%1\tpin>Pin Screen</a> - Games Played: %2',%vClient,%vClient.dtStats.gameStats["totalGames","g",%game]);
         }
         //%i1=%i2=%i3=%i4=%i5=%i6=%i7=%i8=%i9=0;  
         //%line = '<color:0befe7>  PastGames<lmargin:100>%1<lmargin:150>%2<lmargin:200>%3<lmargin:250>%4<lmargin:300>%5<lmargin:350>%6<lmargin:400>%7<lmargin:450>%8<lmargin:500>%9';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7,%i8,%i9);  
         %dtVClient = %vClient.dtStats;
         %i1 = "<color:0befe7>Score:<color:03d597>" SPC %vClient.score; 
         %i2 = "<color:0befe7>Kills:<color:03d597>" SPC %vClient.kills;
         %i3 = "<color:0befe7>Deaths:<color:03d597>" SPC %vClient.deaths; 
         %i4 = "<color:0befe7>Assists:<color:03d597>" SPC %dtVClient.assist;
         %line = '<tab:0,145,290,435><font:univers condensed:18>\t%1\t%2\t%3\t%4'; 
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);         
         
         %i1 = "<color:0befe7>KDR:<color:03d597>" SPC kdr(%vClient.kills,%vClient.deaths) @ "%"; 
         %i2 = "<color:0befe7>KillStreak:<color:03d597>" SPC %dtVClient.killStreak;
         %i3 = "<color:0befe7>MineDisc:<color:03d597>" SPC %dtVClient.minePlusDisc;
         %i4 = %dtVClient.plasmaMA + %dtVClient.discMA + %dtVClient.mineMA + %dtVClient.grenadeMA + %dtVClient.hGrenadeMA + %dtVClient.mortarMA + %dtVClient.shockMA + %dtVClient.laserMA +
         %dtVClient.laserHeadShot + %dtVClient.shockRearShot + %dtVClient.comboPT + %dtVClient.assist +
         (%dtVClient.plasmaKillDist/500) + (%dtVClient.discKillDist/500) + (%dtVClient.mineKillDist/200) + (%dtVClient.grenadeKillDist/300) + (%dtVClient.hGrenadeKillDist/200) + (%dtVClient.mortarKillDist/200)+
         (%dtVClient.plasmaKillSV/100) + (%dtVClient.discKillSV/100) + (%dtVClient.mineKillVV/100) + (%dtVClient.grenadeKillSV/100) + (%dtVClient.hGrenadeKillVV/100) + (%dtVClient.mortarKillSV/100) + (%dtVClient.shockKillSV/50) + (%dtVClient.laserKillSV/100);
         %i4 = "<color:0befe7>Shot Rating:<color:03d597>" SPC mFloatLength(%i4/26,2) + 0; //
         %line = '<tab:0,145,290,435><font:univers condensed:18>\t%1\t%2\t%3\t%4';        
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);


         %dmg =  %dtVClient.blasterDmg + %dtVClient.plasmaDmg + %dtVClient.grenadeDmg + %dtVClient.hGrenadeDmg + %dtVClient.cgDmg + 
         %dtVClient.discDmg + %dtVClient.laserDmg + %dtVClient.mortarDmg + %dtVClient.missileDmg + %dtVClient.shockDmg + %dtVClient.mineDmg;
         %i1 = "<color:0befe7>Damage:<color:03d597>" SPC numReduce(%dmg,1);
         %i2 = "<color:0befe7>Speed:<color:03d597>" SPC  mFloatLength(%dtVClient.avgSpeed,1) + 0;
         %i3 = "<color:0befe7>Shots Fired:<color:03d597>" SPC numReduce(%dtVClient.shotsFired,2);
         %i4 = "<color:0befe7>Dist Moved KM:<color:03d597>" SPC numReduce(%dtVClient.distMov,1); 
         %line = '<tab:0,145,290,435><font:univers condensed:18>\t%1\t%2\t%3\t%4'; 
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);
         
         %i1 = "<color:0befe7>Lt Kills:<color:03d597>" SPC %dtVClient.armorL;
         %i2 = "<color:0befe7>Med Kills:<color:03d597>" SPC %dtVClient.armorM;
         %i3 = "<color:0befe7>Hvy Kills:<color:03d597>"SPC %dtVClient.armorH;
         %i4 = "<color:0befe7>Survival:<color:03d597>" SPC secToMinSec(%dtVClient.timeTL);   
         %line = '<tab:0,145,290,435><font:univers condensed:18>\t%1\t%2\t%3\t%4'; 
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);
         
      
        
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Weapons\tKills\tDeaths\tMidAirs\tCombos\tMax Dist\tAvg Acc\tSpeed\tDmg';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         
         %kills = %dtVClient.blasterKills;
         %deaths = %dtVClient.blasterDeaths;
         %ma = %dtVClient.blasterMA;
         %com = %dtVClient.blasterCom;
         %maxDist = numReduce(%dtVClient.blasterKillDist,1);
         %avgACC = numReduce(%dtVClient.blasterACC,1);
         %speed = numReduce(%dtVClient.blasterHitSV,1);
         %dmg = numReduce(%dtVClient.blasterDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Blaster<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.plasmaKills;
         %deaths = %dtVClient.plasmaDeaths;
         %ma = %dtVClient.plasmaMA;
         %com = %dtVClient.plasmaCom;
         %maxDist = numReduce(%dtVClient.plasmaKillDist,1);
         %avgACC = numReduce(%dtVClient.plasmaACC,1);
         %speed = numReduce(%dtVClient.plasmaHitSV,1);
         %dmg = numReduce(%dtVClient.plasmaDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Plasma Rifle<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.cgKills;
         %deaths = %dtVClient.cgDeaths;
         %ma = %dtVClient.cgMA;
         %com = %dtVClient.cgCom;
         %maxDist = numReduce(%dtVClient.cgKillDist,1);
         %avgACC = numReduce(%dtVClient.cgACC,1);
         %speed = numReduce(%dtVClient.cgHitSV,1);
         %dmg = numReduce(%dtVClient.cgDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Chaingun<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.discKills;
         %deaths = %dtVClient.discDeaths;
         %ma = %dtVClient.discMA;
         %com = %dtVClient.discCom;
         %maxDist = numReduce(%dtVClient.discKillDist,1);
         %avgACC = numReduce(%dtVClient.discACC,1);
         %speed = numReduce(%dtVClient.discHitSV,1);
         %dmg = numReduce(%dtVClient.discDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Spinfusor<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.grenadeKills;
         %deaths = %dtVClient.grenadeDeaths;
         %ma = %dtVClient.grenadeMA;
         %com = %dtVClient.grenadeCom;
         %maxDist = numReduce(%dtVClient.grenadeKillDist,1);
         %avgACC = numReduce(%dtVClient.grenadeDmgACC,1);
         %speed = numReduce(%dtVClient.grenadeHitSV,1);
         %dmg = numReduce(%dtVClient.grenadeDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Grenade Launcher<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.laserKills;
         %deaths = %dtVClient.laserDeaths;
         %ma = %dtVClient.laserMA;
         %com = %dtVClient.laserCom;
         %maxDist = numReduce(%dtVClient.laserKillDist,1);
         %avgACC = numReduce(%dtVClient.laserACC,1);
         %speed = numReduce(%dtVClient.laserHitSV,1);
         %dmg = numReduce(%dtVClient.laserDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Laser Rifle<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.mortarKills;
         %deaths = %dtVClient.mortarDeaths;
         %ma = %dtVClient.mortarMA;
         %com = %dtVClient.mortarCom;
         %maxDist = numReduce(%dtVClient.mortarKillDist,1);
         %avgACC = numReduce(%dtVClient.mortarDmgACC,1);
         %speed = numReduce(%dtVClient.mortarHitSV,1);
         %dmg = numReduce(%dtVClient.mortarDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Fusion Mortar<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.missileKills;
         %deaths = %dtVClient.missileDeaths;
         %ma = %dtVClient.missileMA;
         %com = %dtVClient.missileCom;
         %maxDist = numReduce(%dtVClient.missileKillDist,1);
         %avgACC = numReduce(%dtVClient.missileACC,1);
         %speed = numReduce(%dtVClient.missileHitSV,1);
         %dmg = numReduce(%dtVClient.missileDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Missile Launcher<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.shockKills;
         %deaths = %dtVClient.shockDeaths;
         %ma = %dtVClient.shockMA;
         %com = %dtVClient.shockCom;
         %maxDist = numReduce(%dtVClient.shockKillDist,1);
         %avgACC = numReduce(%dtVClient.shockACC,1);
         %speed = numReduce(%dtVClient.shockHitSV,1);
         %dmg = numReduce(%dtVClient.shockDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Shocklance<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.hGrenadeKills;
         %deaths = %dtVClient.hGrenadeDeaths;
         %ma = %dtVClient.hGrenadeMA;
         %com = %dtVClient.hGrenadeCom;
         %maxDist = numReduce(%dtVClient.hGrenadeKillDist,1);
         %avgACC = numReduce(%dtVClient.hGrenadeACC,1);
         %speed = numReduce(%dtVClient.hGrenadeHitVV,1);
         %dmg = numReduce(%dtVClient.hGrenadeDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Hand Grenade<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
         %kills = %dtVClient.mineKills;
         %deaths = %dtVClient.mineDeaths;
         %ma = %dtVClient.mineMA;
         %com = %dtVClient.mineCom;
         %maxDist = numReduce(%dtVClient.mineKillDist,1);
         %avgACC = numReduce(%dtVClient.mineACC,1);
         %speed = numReduce(%dtVClient.mineHitVV,1);
         %dmg = numReduce(%dtVClient.mineDmg,1);
         %line = '<tab:114,171,228,285,342,399,456,513><font:univers condensed:18><color:0befe7> Mine<color:03d597>\t%1\t%2\t%3\t%4\t%5\t%6\t%7\t%8';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%kills,%deaths,%ma,%com,%maxDist,%avgACC,%speed,%dmg);
         
      case "LBOARDS":
         %lType = getField(strreplace(%client.GlArg4,"-","\t"),0);
         %client.lgame = getField(strreplace(%client.GlArg4,"-","\t"),1);
         %page = %client.GlArg5;
         if(%client.lgame $= ""){
          %client.lgame = %game;  
         }
         if(!%page){
            %page = 1;  
         }
         if($lData::hasData[%lType,%client.lgame]){// see if have data
            %mon = getField($lData::mon[%lType, %client.lgame, %page],0);
            %year = getField($lData::mon[%lType, %client.lgame, %page],1);
            %client.curMon = %mon;
            %client.curYear = %year;
            %client.curLType = %lType;            
            %client.curPage = %page;
            switch$(%lType){
               case "day":        
                  %lTypeName = "Daily";
                  messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%1 Leaderboards For %2',%lTypeName,"Day:" @ %mon);
               case "week":      
                  %lTypeName = "Weekly";
                  messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%1 Leaderboards For %2',%lTypeName,"Week:" @ %mon);
               case "month":    
                  %lTypeName = "Monthly";
                  messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%1 Leaderboards For %2',%lTypeName,monthString(%mon));
               case "quarter":
                  %lTypeName = "Quarterly";
                  messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%1 Leaderboards For %2',%lTypeName,"Q:" @ %mon);
               case "year":
                  %lTypeName = "Yearly";
                  messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%1 Leaderboards For %2',%lTypeName,%year);
            }
            if(%client.isSuperAdmin)
               messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a><just:right><a:gamelink\tS\tVARLIST\t%1\t1>VarList </a>',%vClient,%lType);
            else
               messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,%lType);
            
            %header = '<tab:0,50,65,200,320,460>\t<color:0befe7> \t# \t%2\tScore\tWeapons\tScore';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%vClient,$dtStats::gtNameLong[%client.lgame]);
            
            for(%i = 0; %i < 10; %i++){
               %scoreName  = getField($lData::name["scoreTG",%client.lgame,%lType,%mon,%year],%i);
               %gameScore  = getField($lData::data["scoreTG",%client.lgame,%lType,%mon,%year],%i);
               %wepName  = getField($lData::name["weaponScoreTG",%client.lgame,%lType,%mon,%year],%i);
               %wepScore  = getField($lData::data["weaponScoreTG",%client.lgame,%lType,%mon,%year],%i);
               if(%gameScore){
                  %line = '<tab:0,50,65,200,320,460>\t<font:univers condensed:18> \t%3. \t<color:0befe7><clip:138>%1</clip><color:03d597>\t%4\t<color:0befe7><clip:138>%2</clip><color:03d597>\t%5';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%scoreName,%wepName,%i+1,%gameScore,mFloor(%wepScore+0.5));
               }
               else
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            }
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            
            %line = '<just:center><color:0befe7><a:gamelink\tS\tGLBOARDS\t%1>View More %2 Categories</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,$dtStats::gtNameLong[%client.lgame]);
            %line = '<just:center><color:0befe7><a:gamelink\tS\tWLBOARDS\t%1>View More Weapons Categories</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %line = '<just:center><color:0befe7><a:gamelink\tS\tMLBOARDS\t%1>View Miscellaneous Categories</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %hasCount = 0;  %line = "";
            for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
               if($lData::hasData[%lType,$dtStats::gameType[%i]] && $dtStats::gameType[%i] !$= %client.lgame){
                  %hasCount++; 
                  %line = %line @ "<a:gamelink\tS\tLBOARDS\t" @ %vClient @ "\t" @ %lType @ "-" @  $dtStats::gameType[%i] @ "\t0>[" @ $dtStats::gtNameShort[$dtStats::gameType[%i]]  @ "] </a>"; 
               }
            }
            //error(%client.lgame SPC %game SPC %hasCount );
            if(%hasCount > 0){
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Switch Game Type" SPC %line);
            }
            else{
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            }
            
            
			   //messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //error(%mon SPC %page SPC $lData::monCount[%client.lgame,%lType]);
            if($lData::monCount[%client.lgame,%lType] > 1){
               if(%page == 1){
                   %line = '<just:center>Click on category to view more<just:right><color:0befe7><a:gamelink\tS\tLBOARDS\t%1\t%2-%4\t%3>Previous</a>';
                   messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%lType,%page+1,%client.lgame); 
               }
               else if(%page >= $lData::monCount[%client.lgame,%lType]){
                  %line = '<just:center>Click on top category to view more<just:right><color:0befe7><a:gamelink\tS\tLBOARDS\t%1\t%2-%4\t%3>Next</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%lType,%page-1,%client.lgame);
               }
               else{
                  %line = '<just:center>Click on category to view more<just:right><a:gamelink\tS\tLBOARDS\t%1\t%2-%5\t%3>Next</a> | <color:0befe7><a:gamelink\tS\tLBOARDS\t%1\t%2-%5\t%4>Previous</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%lType,%page-1,%page+1,%client.lgame); 
               }
            }
         }
         else{//no data for selected game type
            %header = '<color:0befe7><just:center>No data at this time, check in 24 hours';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%i1,%i2,%i3,%i4++,%i5,%i6,%i7); 
            
            %hasCount = 0;  %line = "";
            for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
               if($lData::hasData[%lType,$dtStats::gameType[%i]] && $dtStats::gameType[%i] !$= %client.lgame){
                  %hasCount++; 
                  %line = %line @ "<a:gamelink\tS\tLBOARDS\t" @ %vClient @ "\t" @ %lType @ "-" @  $dtStats::gameType[%i] @ "\t0>" @ $dtStats::gtNameShort[$dtStats::gameType[%i]]  @ " </a>"; 
               }
            }  
            if(%hasCount > 0){
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>View other gametypes');
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>" @ %line);
            }
         }
      case "GLBOARDS":
         %lType = %client.curLType;
         %page = %client.curPage;
         %mon = getField($lData::mon[%lType, %client.lgame, %page],0);
         %year = getField($lData::mon[%lType, %client.lgame, %page],1);
         %client.backPage = "GLBOARDS";
         %NA = "N/A";
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ $dtStats::gtNameShort[%client.lgame] SPC "Greatest Hits");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType,%page,%client.lgame);
            //exec("scripts/autoexec/zDarktigerStats.cs");
         %clG = %client.lgame;
			switch$(%clG){
			   case "CTFGame":
               %var1 = "scoreTG";  %var1Title = "Score Total:";   %var1Name = "Score Total";    %var1TypeName = "Total";
               %var2 = "scoreAVG"; %var2Title = "Score Avg:";     %var2Name = "Score Average "; %var2TypeName = "Average";
               %var3 = "scoreMax"; %var3Title = "Highest Score:"; %var3Name = "Highest Score";  %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
 
               %var1 = "winLostPctAvg";  %var1Title = "Win Lost Ratio:";  %var1Name = "Win lost Ratio Average"; %var1TypeName = "Ratio";
               %var2 = "defenseScoreTG"; %var2Title = "Defense Score:"; %var2Name = "Defense Score";    %var2TypeName = "Total";
               %var3 = "offenseScoreTG"; %var3Title = "Offense Score:"; %var3Name = "Offense Score";    %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "flagCapsTG";    %var1Title = "Flag Caps:";    %var1Name = "Flag Caps";    %var1TypeName = "Total";
               %var2 = "flagGrabsTG";   %var2Title = "Flag Grabs:";   %var2Name = "Flag Grabs";   %var2TypeName = "Total";
               %var3 = "flagReturnsTG"; %var3Title = "Flag Returns:"; %var3Name = "Flag Returns"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
 
               %var1 = "carrierKillsTG";  %var1Title = "Carrier Kills:";  %var1Name = "Carrier Kills";  %var1TypeName = "Total";
               %var2 = "escortAssistsTG"; %var2Title = "Escort Assists:"; %var2Name = "Escort Assists"; %var2TypeName = "Total";
               %var3 = "flagDefendsTG";   %var3Title = "Flag Defends:";   %var3Name = "Flag Defends";   %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
     
               %var1 = "grabSpeedAvg";     %var1Title = "Grab Speed Avg:"; %var1Name = "Grab Speed Avg";            %var1TypeName = "Speed KM/H";
               %var2 = "heldTimeSecAvgI";  %var2Title = "Held Time Avg:";  %var2Name = "Held Time Avg";             %var2TypeName = "Seconds";
               %var3 = "capEfficiencyAvg"; %var3Title = "Cap Efficiency:"; %var3Name = "Cap Efficiency Caps/Grabs"; %var3TypeName = "Value";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "killsTG";     %var1Title = "Kills:";      %var1Name = "Kills";      %var1TypeName = "Total";
               %var2 = "assistTG";    %var2Title = "Assists:";    %var2Name = "Assists";    %var2TypeName = "Total";
               %var3 = "teamKillsTG"; %var3Title = "Team Kills:"; %var3Name = "Team Kills"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
 
               %var1 = "EVKillsTG";     %var1Title = "EV Kills:";    %var1Name = "Environmental Kills"; %var1TypeName = "Total";
               %var2 = "killStreakMax"; %var2Title = "Kill Streak:"; %var2Name = "Highest Kill Streak"; %var2TypeName = "Max";
               %var3 = "kdrAvg";        %var3Title = "KDR Avg:";     %var3Name = "Kill / Death Ratio";  %var3TypeName = "Value";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "airTimeTG";    %var1Title = "Air Time:";       %var1Name = "Total Air Time";    %var1TypeName = "Minutes";
               %var2 = "groundTimeTG"; %var2Title = "Ground Time:";    %var2Name = "Total Ground Time"; %var2TypeName = "Minutes";
               %var3 = "distMovTG";    %var3Title = "Distance Moved:"; %var3Name = "Distance Moved";    %var3TypeName = "Total KM";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
 
               %var1 = "killAirTG";    %var1Title = "Air kills:";     %var1Name = "Air Kills";     %var1TypeName = "Total";
               %var2 = "killGroundTG"; %var2Title = "Ground Kills:";  %var2Name = "Ground Kills";  %var2TypeName = "Total";
               %var3 = "totalMATG";    %var3Title = "Total MidAirs:"; %var3Name = "Total MidAirs"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
 
               %var1 = "multiKillTG";  %var1Title = "Multi Kills:"; %var1Name = "Multi Kills";      %var1TypeName = "Total";
               %var2 = "chainKillTG";  %var2Title = "Chain Kills:"; %var2Name = "Chain Kills";      %var2TypeName = "Total";
               %var3 = "deathKillsTG"; %var3Title = "Death Kills:"; %var3Name = "Kills While Dead"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "roadKillsTG";          %var1Title = "Road Kills:";   %var1Name = "Road Kills";           %var1TypeName = "Total";
               %var2 = "shrikeBlasterKillsTG"; %var2Title = "Shrike Kills:"; %var2Name = "Shrike Blaster Kills"; %var2TypeName = "Total";
               %var3 = "bomberBombsKillsTG";   %var3Title = "Bomber Kills:"; %var3Name = "Bomber Bomb Kills";    %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "destructionTG";            %var1Title = "Assets Destroyed:"; %var1Name = "Assets Destroyed";   %var1TypeName = "Total";
               %var2 = "repairsTG";                %var2Title = "Repairs:";          %var2Name = "Repairs";            %var2TypeName = "Total";
               %var3 = "repairpackpickupCountTTL"; %var3Title = "Repair Pack Grab:"; %var3Name = "Repair Pack Pickup"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "depStationDestroysTG"; %var1Title = "DepStation-Des:"; %var1Name = "Deployable Station Destroys"; %var1TypeName = "Total";
               %var2 = "depTurretDestroysTG";  %var2Title = "DepTurret-Des:";  %var2Name = "Deployable Turret Destroys"; %var2TypeName = "Total";
               %var3 = "depSensorDestroysTG";  %var3Title = "DepSensor-Des:";  %var3Name = "Deployable Sensor Destroys"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

               %var1 = "genDefendsTG";  %var1Title = "Gen Defends:";  %var1Name = "Generator Defends";  %var1TypeName = "Total";
               %var2 = "genDestroysTG"; %var2Title = "Gen Destroys:"; %var2Name = "Generator Destroys"; %var2TypeName = "Total";
               %var3 = "genRepairsTG";    %var3Title = "Gen Repairs:";  %var3Name = "Generator Repairs";  %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //15
               %var1 = "totalTimeTG"; %var1Title = "Server Time:";  %var1Name = "Total Server Time";      %var1TypeName = "Minutes";
               %var2 = "timeTLAvg";   %var2Title = "Avg Lifetime:"; %var2Name = "Average Lifetime"; %var2TypeName = "Seconds";
               %var3 = "maxSpeedMax"; %var3Title = "Max Speed:";    %var3Name = "Highest Speed";    %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
            case "SCtfGame":
               //1
               %var1 = "scoreTG";  %var1Title = "Score Total:";   %var1Name = "Score Total";    %var1TypeName = "Total";
               %var2 = "scoreAVG"; %var2Title = "Score Avg:";     %var2Name = "Score Average "; %var2TypeName = "Average";
               %var3 = "scoreMax"; %var3Title = "Highest Score:"; %var3Name = "Highest Score";  %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //2
               %var1 = "winLostPctAvg";  %var1Title = "Win Lost Ratio:"; %var1Name = "Win lost Average"; %var1TypeName = "Percentage";
               %var2 = "defenseScoreTG"; %var2Title = "Defense Score:";  %var2Name = "Defense Score";    %var2TypeName = "Total";
               %var3 = "offenseScoreTG"; %var3Title = "Offense Score:";  %var3Name = "Offense Score";    %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //3
               %var1 = "flagCapsTG";    %var1Title = "Flag Caps:";    %var1Name = "Flag Caps";    %var1TypeName = "Total";
               %var2 = "flagGrabsTG";   %var2Title = "Flag Grabs:";   %var2Name = "Flag Grabs";   %var2TypeName = "Total";
               %var3 = "flagReturnsTG"; %var3Title = "Flag Returns:"; %var3Name = "Flag Returns"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //4
               %var1 = "carrierKillsTG";  %var1Title = "Carrier Kills:";  %var1Name = "Carrier Kills";  %var1TypeName = "Total";
               %var2 = "escortAssistsTG"; %var2Title = "Escort Assists:"; %var2Name = "Escort Assists"; %var2TypeName = "Total";
               %var3 = "flagDefendsTG";   %var3Title = "Flag Defends:";   %var3Name = "Flag Defends";   %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //5
               %var1 = "grabSpeedAvg";     %var1Title = "Grab Speed Avg:"; %var1Name = "Grab Speed Avg"; %var1TypeName = "Speed KM/H";
               %var2 = "heldTimeSecAvgI";  %var2Title = "Held Time Avg:";  %var2Name = "Held Time Avg";  %var2TypeName = "Seconds";
               %var3 = "capEfficiencyAvg"; %var3Title = "Cap Efficiency:"; %var3Name = "Cap Efficiency"; %var3TypeName = "Percentage";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //6
               %var1 = "killsTG";     %var1Title = "Kills:";      %var1Name = "Kills";      %var1TypeName = "Total";
               %var2 = "assistTG";    %var2Title = "Assists:";    %var2Name = "Assists";    %var2TypeName = "Total";
               %var3 = "teamKillsTG"; %var3Title = "Team Kills:"; %var3Name = "Team Kills"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //7
               %var1 = "EVKillsTG";     %var1Title = "EV Kills:";    %var1Name = "Environmental Kills"; %var1TypeName = "Total";
               %var2 = "killStreakMax"; %var2Title = "Kill Streak:"; %var2Name = "Highest Kill Streak"; %var2TypeName = "Max";
               %var3 = "kdrAvg";        %var3Title = "KDR Avg:";     %var3Name = "Kill / Death Ratio";  %var3TypeName = "Value";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //8
               %var1 = "airTimeTG";    %var1Title = "Air Time:";       %var1Name = "Total Air Time";    %var1TypeName = "Minutes";
               %var2 = "groundTimeTG"; %var2Title = "Ground Time:";    %var2Name = "Total Ground Time"; %var2TypeName = "Minutes";
               %var3 = "distMovTG";    %var3Title = "Distance Moved:"; %var3Name = "Distance Moved";    %var3TypeName = "Total KM";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //9
               %var1 = "killAirTG";    %var1Title = "Air kills:";     %var1Name = "Air Kills";     %var1TypeName = "Total";
               %var2 = "killGroundTG"; %var2Title = "Ground Kills:";  %var2Name = "Ground Kills";  %var2TypeName = "Total";
               %var3 = "totalMATG";    %var3Title = "Total MidAirs:"; %var3Name = "Total MidAirs"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //10
               %var1 = "multiKillTG";  %var1Title = "Multi Kills:"; %var1Name = "Multi Kills";      %var1TypeName = "Total";
               %var2 = "chainKillTG";  %var2Title = "Chain Kills:"; %var2Name = "Chain Kills";      %var2TypeName = "Total";
               %var3 = "deathKillsTG"; %var3Title = "Death Kills:"; %var3Name = "Kills While Dead"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //11
               %var1 = "doubleKillTG";    %var1Title = "Double Kills:"; %var1Name = "Double Kills";   %var1TypeName = "Total";
               %var2 = "tripleKillTG";    %var2Title = "Triple Kill:";  %var2Name = "Triple Kill";    %var2TypeName = "Total";
               %var3 = "quadrupleKillTG"; %var3Title = "Quad kill:";    %var3Name = "Quadruple Kill"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //12
               %var1 = "doubleChainKillTG";    %var1Title = "Double Chain Kill:"; %var1Name = "Double Chain Kill";    %var1TypeName = "Total";
               %var2 = "tripleChainKillTG";    %var2Title = "Triple Chain Kill:"; %var2Name = "Triple Chain Kill";    %var2TypeName = "Total";
               %var3 = "quadrupleChainKillTG"; %var3Title = "Quad Chain Kill:";   %var3Name = "Quadruple Chain Kill"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //13
               %var1 = "totalTimeTG"; %var1Title = "Time Played:";  %var1Name = "Time Played";      %var1TypeName = "Minutes";
               %var2 = "timeTLAvg";   %var2Title = "Avg Lifetime:"; %var2Name = "Average Lifetime"; %var2TypeName = "Seconds";
               %var3 = "maxSpeedMax"; %var3Title = "Max Speed:";    %var3Name = "Highest Speed";    %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);

            case "LakRabbitGame":   
               //1            
               %var1 = "flagGrabsTG";       %var1Title = "Flag Grabs:";   %var1Name = "Flag Grabs";        %var1TypeName = "Total";
               %var2 = "flagTimeMinTG";     %var2Title = "Flag Time:";    %var2Name = "Flag Time";         %var2TypeName = "Minutes";
               %var3 = "MidairflagGrabsTG"; %var3Title = "MidAir Grabs:"; %var3Name = "MidAir Flag Grabs"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //2
               %var1 = "scoreTG";  %var1Title = "Score Total:";   %var1Name = "Score Total";    %var1TypeName = "Total";
               %var2 = "scoreAVG"; %var2Title = "Score Avg:";     %var2Name = "Score Average "; %var2TypeName = "Average";
               %var3 = "scoreMax"; %var3Title = "Highest Score:"; %var3Name = "Highest Score";  %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //3
               %var1 = "killsTG";     %var1Title = "Kills:";      %var1Name = "Kills";      %var1TypeName = "Total";
               %var2 = "assistTG";    %var2Title = "Assists:";    %var2Name = "Assists";    %var2TypeName = "Total";
               %var3 = "teamKillsTG"; %var3Title = "Team Kills:"; %var3Name = "Team Kills"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //4
               %var1 = "EVKillsTG";     %var1Title = "EV Kills:";    %var1Name = "Environmental Kills"; %var1TypeName = "Total";
               %var2 = "killStreakMax"; %var2Title = "Kill Streak:"; %var2Name = "Highest Kill Streak"; %var2TypeName = "Max";
               %var3 = "kdrAvg";        %var3Title = "KDR Avg:";     %var3Name = "Kill / Death Ratio";  %var3TypeName = "Value";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //5
               %var1 = "airTimeTG";    %var1Title = "Air Time:";       %var1Name = "Total Air Time";    %var1TypeName = "Minutes";
               %var2 = "groundTimeTG"; %var2Title = "Ground Time:";    %var2Name = "Total Ground Time"; %var2TypeName = "Minutes";
               %var3 = "distMovTG";    %var3Title = "Distance Moved:"; %var3Name = "Distance Moved";    %var3TypeName = "Total KM";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //6
               %var1 = "totalTimeTG"; %var1Title = "Time Played:";  %var1Name = "Time Played";      %var1TypeName = "Minutes";
               %var2 = "timeTLAvg";   %var2Title = "Avg Lifetime:"; %var2Name = "Average Lifetime"; %var2TypeName = "Seconds";
               %var3 = "maxSpeedMax"; %var3Title = "Max Speed:";    %var3Name = "Highest Speed";    %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //7
               %var1 = "killAirTG";    %var1Title = "Air kills:";     %var1Name = "Air Kills";     %var1TypeName = "Total";
               %var2 = "killGroundTG"; %var2Title = "Ground Kills:";  %var2Name = "Ground Kills";  %var2TypeName = "Total";
               %var3 = "totalMATG";    %var3Title = "Total MidAirs:"; %var3Name = "Total MidAirs"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //8
               %var1 = "multiKillTG";  %var1Title = "Multi Kills:"; %var1Name = "Multi Kills";      %var1TypeName = "Total";
               %var2 = "chainKillTG";  %var2Title = "Chain Kills:"; %var2Name = "Chain Kills";      %var2TypeName = "Total";
               %var3 = "deathKillsTG"; %var3Title = "Death Kills:"; %var3Name = "Kills While Dead"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //9
               %var1 = "doubleKillTG";    %var1Title = "Double Kills:"; %var1Name = "Double Kills";   %var1TypeName = "Total";
               %var2 = "tripleKillTG";    %var2Title = "Triple Kill:";  %var2Name = "Triple Kill";    %var2TypeName = "Total";
               %var3 = "quadrupleKillTG"; %var3Title = "Quad kill:";    %var3Name = "Quadruple Kill"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //10
               %var1 = "killAirTG";       %var1Title = "Air Kills:";      %var1Name = "Air Kills";             %var1TypeName = "Total";
               %var2 = "killAirGroundTG"; %var2Title = "Air To Ground:"; %var2Name = "Air To Ground Kills";    %var2TypeName = "Total";
               %var3 = "killAirAirTG";    %var3Title = "Air To Air:";    %var3Name = "Air To Air Kills";       %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //11
               %var1 = "killGroundTG";       %var1Title = "Ground Kills:";     %var1Name = "Ground Kills";           %var1TypeName = "Total";
               %var2 = "killGroundAirTG";    %var2Title = "Ground To Air:";    %var2Name = "Ground To Air Kills";    %var2TypeName = "Total";
               %var3 = "killGroundGroundTG"; %var3Title = "Ground To Ground:"; %var3Name = "Ground To Ground Kills"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //12
               %var1 = "maHitDistMax";   %var1Title = "MidAir Dist:";   %var1Name = "MidAir Max Distance"; %var1TypeName = "Meters";
               %var2 = "maHitHeightMax"; %var2Title = "MidAir Height:"; %var2Name = "MidAir Max Height";   %var2TypeName = "Meters";
               %var3 = "maHitSVMax";     %var3Title = "MidAir Speed:";  %var3Name = "MidAir Max Speed";    %var3TypeName = "KM/H";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //13
               %var1 = "totalTimeTG"; %var1Title = "Time Played:";  %var1Name = "Time Played";      %var1TypeName = "Minutes";
               %var2 = "timeTLAvg";   %var2Title = "Avg Lifetime:"; %var2Name = "Average Lifetime"; %var2TypeName = "Seconds";
               %var3 = "maxSpeedMax"; %var3Title = "Max Speed:";    %var3Name = "Highest Speed";    %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
            default:// the rest
               //1
               %var1 = "scoreTG";  %var1Title = "Score Total:";   %var1Name = "Score Total";    %var1TypeName = "Total";
               %var2 = "scoreAVG"; %var2Title = "Score Avg:";     %var2Name = "Score Average "; %var2TypeName = "Average";
               %var3 = "scoreMax"; %var3Title = "Highest Score:"; %var3Name = "Highest Score";  %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //2
               %var1 = "killsTG";     %var1Title = "Kills:";      %var1Name = "Kills";         %var1TypeName = "Total";
               %var2 = "assistTG";    %var2Title = "Assists:";    %var2Name = "Assists"; %var2TypeName = "Total";
               %var3 = "teamKillsTG"; %var3Title = "Team Kills:"; %var3Name = "Team Kills"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //3
               %var1 = "EVKillsTG";     %var1Title = "EV Kills:";    %var1Name = "Environmental Kills"; %var1TypeName = "Total";
               %var2 = "killStreakMax"; %var2Title = "Kill Streak:"; %var2Name = "Highest Kill Streak"; %var2TypeName = "Max";
               %var3 = "kdrAvg";        %var3Title = "KDR Avg:";     %var3Name = "Kill / Death Ratio";  %var3TypeName = "Value";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //4
               %var1 = "airTimeTG";    %var1Title = "Air Time:";       %var1Name = "Total Air Time";    %var1TypeName = "Minutes";
               %var2 = "groundTimeTG"; %var2Title = "Ground Time:";    %var2Name = "Total Ground Time"; %var2TypeName = "Minutes";
               %var3 = "distMovTG";    %var3Title = "Distance Moved:"; %var3Name = "Distance Moved";    %var3TypeName = "Total KM";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //5
               %var1 = "totalTimeTG"; %var1Title = "Time Played:";  %var1Name = "Time Played";      %var1TypeName = "Minutes";
               %var2 = "timeTLAvg";   %var2Title = "Avg Lifetime:"; %var2Name = "Average Lifetime"; %var2TypeName = "Seconds";
               %var3 = "maxSpeedMax"; %var3Title = "Max Speed:";    %var3Name = "Highest Speed";    %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //6
               %var1 = "killAirTG";    %var1Title = "Air kills:";     %var1Name = "Air Kills";     %var1TypeName = "Total";
               %var2 = "killGroundTG"; %var2Title = "Ground Kills:";  %var2Name = "Ground Kills";  %var2TypeName = "Total";
               %var3 = "totalMATG";    %var3Title = "Total MidAirs:"; %var3Name = "Total MidAirs"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //7
               %var1 = "multiKillTG";  %var1Title = "Multi Kills:"; %var1Name = "Multi Kills";      %var1TypeName = "Total";
               %var2 = "chainKillTG";  %var2Title = "Chain Kills:"; %var2Name = "Chain Kills";      %var2TypeName = "Total";
               %var3 = "deathKillsTG"; %var3Title = "Death Kills:"; %var3Name = "Kills While Dead"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //8
               %var1 = "doubleKillTG";    %var1Title = "Double Kills:"; %var1Name = "Double Kills";   %var1TypeName = "Total";
               %var2 = "tripleKillTG";    %var2Title = "Triple Kill:";  %var2Name = "Triple Kill";    %var2TypeName = "Total";
               %var3 = "quadrupleKillTG"; %var3Title = "Quad kill:";    %var3Name = "Quadruple Kill"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //9
               %var1 = "killAirTG";       %var1Title = "Air Kills:";     %var1Name = "Air Kills";           %var1TypeName = "Total";
               %var2 = "killAirGroundTG"; %var2Title = "Air To Ground:"; %var2Name = "Air To Ground Kills"; %var2TypeName = "Total";
               %var3 = "killAirAirTG";    %var3Title = "Air To Air:";    %var3Name = "Air To Air Kills";    %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //10
               %var1 = "killGroundTG";       %var1Title = "Ground Kills:";     %var1Name = "Ground Kills";           %var1TypeName = "Total";
               %var2 = "killGroundAirTG";    %var2Title = "Ground To Air:";    %var2Name = "Ground To Air Kills";    %var2TypeName = "Total";
               %var3 = "killGroundGroundTG"; %var3Title = "Ground To Ground:"; %var3Name = "Ground To Ground Kills"; %var3TypeName = "Total";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //11
               %var1 = "maHitDistMax";   %var1Title = "MidAir Distance:"; %var1Name = "MidAir Max Distance"; %var1TypeName = "Meters";
               %var2 = "maHitHeightMax"; %var2Title = "MidAir Height:";   %var2Name = "MidAir Max Height";   %var2TypeName = "Meters";
               %var3 = "maHitSVMax";     %var3Title = "MidAir Speed:";    %var3Name = "MidAir Max Speed";    %var3TypeName = "KM/H";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               //12
               %var1 = "totalTimeTG"; %var1Title = "Time Played:";  %var1Name = "Time Played";      %var1TypeName = "Minutes";
               %var2 = "timeTLAvg";   %var2Title = "Avg Lifetime:"; %var2Name = "Average Lifetime"; %var2TypeName = "Seconds";
               %var3 = "maxSpeedMax"; %var3Title = "Max Speed:";    %var3Name = "Highest Speed";    %var3TypeName = "Max";
               %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
			}
         for(%i = %index; %i < 15; %i++)
			   messageClient( %client, 'SetLineHud', "", %tag, %index++, '');
			messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Click on player name to view the top %1 players', $dtStats::topAmount);
      case "WLBOARDS":
         %lType = %client.curLType;
         %page = %client.curPage;
         %mon = getField($lData::mon[%lType, %client.lgame, %page],0);
         %year = getField($lData::mon[%lType, %client.lgame, %page],1);
         %client.backPage = "WLBOARDS";
         %NA = "N/A"; 
         messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Weapons Greatest Hits');
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType,%page,%client.lgame);
         
         %var1 = "minePlusDiscTG"; %var1Title = "Mine + Disc:";   %var1Name = "Mine Disc Hits";            %var1TypeName = "Total";
         %var2 = "discACCAvg";     %var2Title = "Spinfusor Acc:"; %var2Name = "Spinfusor Accuracy";        %var2TypeName = "Percentage";
         %var3 = "discMADistMax";  %var3Title = "Disc MA Dist:";  %var3Name = "Spinfusor MidAir Distance"; %var3TypeName = "Meters";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName; 
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
         
         %var1 = "weaponHitDistMax"; %var1Title = "Longest Shot:";   %var1Name = "Longest Shot";   %var1TypeName = "Meters";
         %var2 = "maxSpeedMax";      %var2Title = "Highest Speed:";  %var2Name = "Highest Speed";  %var2TypeName = "KM/H";
         %var3 = "satchelKillsTG";   %var3Title = "Satchel Kills:";   %var3Name = "Satchel Kills"; %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
         
         %var1 = "totalWepDmgTG";   %var1Title = "Tot Wep Damage:"; %var1Name = "Total Weapon Damage"; %var1TypeName = "Total";
         %var2 = "shotsFiredTG";    %var2Title = "Rounds Fired:";   %var2Name = "Rounds Fired";        %var2TypeName = "Total";
         %var3 = "elfShotsFiredTG"; %var3Title = "ELF Usage:";     %var3Name = "ELF Usage";            %var3TypeName = "Max";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
         
         %var1 = "shockRearShotTG"; %var1Title = "Rearshots:"; %var1Name = "Rear Shocklance";        %var1TypeName = "Total";
         %var2 = "laserHeadShotTG"; %var2Title = "Headshots:"; %var2Name = "Laser Rifle Head Shots"; %var2TypeName = "Total";
         %var3 = "comboCountTG";    %var3Title = "Combos:";    %var3Name = "Weapon Combos";          %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);


         %header = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> Weapon\tKills\tMidAirs\tDistance\tSpeed';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %wep = "Blaster";
         %var1 = "blasterKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "blasterMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "blasterHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "blasterHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
         
         %wep = "Plasma Rifle";
         %var1 = "plasmaKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "plasmaMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "plasmaHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "plasmaHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
          
         %wep = "Chaingun";
         %var1 = "cgKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "cgMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "cgHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "cgHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
         
         %wep = "Spinfusor";
         %var1 = "discKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "discMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "discHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "discHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
           
         %wep = "Grenade Launcher";
         %var1 = "grenadeKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "grenadeMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "grenadeHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "grenadeHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
         
         %wep = "Laser Rifle";
         %var1 = "laserKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "laserMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "laserHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "laserHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);

         %wep = "Fusion Mortar";
         %var1 = "mortarKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "mortarMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "mortarHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "mortarHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
         
         %wep = "Missile Launcher";
         %var1 = "missileKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "missileMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "missileHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "missileHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
          
         %wep = "Shocklance";
         %var1 = "shockKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "shockMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "shockHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "shockHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
         
         %wep = "Mine";
         %var1 = "mineKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "mineMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "mineHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "mineHitVVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
         
         %wep = "Hand Grenade";
         %var1 = "hGrenadeKillsTG";     %var1Name = %wep SPC "Kills";     %var1TypeName = "Total";
         %var2 = "hGrenadeMATG";        %var2Name = %wep SPC "MidAirs";   %var2TypeName = "Total";
         %var3 = "hGrenadeHitDistMax";  %var3Name = %wep SPC "Distance";  %var3TypeName = "Meters";
         %var4 = "hGrenadeHitSVMax";    %var4Name = %wep SPC "Hit Speed"; %var4TypeName = "KM/h";
         %nameTitle1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA;
         %nameTitle4 = getField($lData::data[%var4,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var4,%client.lgame,%lType,%mon,%year],0) : %NA;
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %client.statsFieldSet[%vsc4 = %f++] = %var4 TAB %var4Name TAB %var4TypeName;
         %client.linkSet[%l++] = %vsc1 TAB %vsc2 TAB %vsc3 TAB %vsc4;
         %line = '<tab:114,234,354,474><font:univers condensed:18><color:0befe7> %2<color:03d597>\t<a:gamelink\tS\tLB\t%1\t%7\t0>%3</a>\t<a:gamelink\tS\tLB\t%1\t%7\t1>%4</a>\t<a:gamelink\tS\tLB\t%1\t%7\t2>%5</a>\t<a:gamelink\tS\tLB\t%1\t%7\t3>%6</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%wep,%nameTitle1,%nameTitle2,%nameTitle3,%nameTitle4,%l);
		 
		   messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Click on player name to view the top %1 players', $dtStats::topAmount);
		   
      case "MLBOARDS": //misc
         %lType = %client.curLType;
         %page = %client.curPage;
         %mon = getField($lData::mon[%lType, %client.lgame, %page],0);
         %year = getField($lData::mon[%lType, %client.lgame, %page],1);
         %client.backPage = "MLBOARDS";
         %NA = "N/A";
		
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ $dtStats::gtNameShort[%client.lgame] SPC "Misc");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType,%page,%client.lgame);

         %var1 = "firstKillTG";  %var1Title = "First Kills:"; %var1Name = "First Kills";      %var1TypeName = "Total";
         %var2 = "lastKillTG";   %var2Title = "Last Kills:";  %var2Name = "Last Kills";       %var2TypeName = "Total";
         %var3 = "deathKillsTG"; %var3Title = "Death Kills:"; %var3Name = "Kills While Dead"; %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
         
         %var1 = "groundDeathsTG"; %var1Title = "Ground Deaths:"; %var1Name = "Ground Deaths"; %var1TypeName = "Total";
         %var2 = "groundKillsTG";  %var2Title = "Ground Kills:";  %var2Name = "Ground Kills";  %var2TypeName = "Total";
         %var3 = "lavaDeathsTG";   %var3Title = "Lava Deaths:";   %var3Name = "Lava Deaths";   %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);   
         
         %var1 = "chatallCountTTL";  %var1Title = "Chat All:";    %var1Name = "Chat All";    %var1TypeName = "Total";
         %var2 = "chatteamCountTTL"; %var2Title = "Chat Team:";   %var2Name = "Chat Team";   %var2TypeName = "Total";
         %var3 = "voteCountTTL";     %var3Title = "Vote Starts:"; %var3Name = "Vote Starts"; %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
			
			%var1 = "voicebindsallCountTTL";  %var1Title = "Voice Binds All:";  %var1Name = "Voice Binds All";  %var1TypeName = "Total";
         %var2 = "voicebindsteamCountTTL"; %var2Title = "Voice Binds Team:"; %var2Name = "Voice Binds Team"; %var2TypeName = "Total";
         %var3 = "kickCountTTL";           %var3Title = "Kick Count:";       %var3Name = "Kick Count";       %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
         
         %var1 = "leavemissionareaCountTTL"; %var1Title = "Leave Mission Area:"; %var1Name = "Leave Mission Area";   %var1TypeName = "Total";
         %var2 = "vehicleSpawnKillsTG";      %var2Title = "Veh Spawn Kills:";    %var2Name = "Vehicle Spawn Kills";  %var2TypeName = "Total";
         %var3 = "vehicleSpawnDeathsTG";     %var3Title = "Veh Spawn Deaths:";   %var3Name = "Vehicle Spawn Deaths"; %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
         
         %var1 = "switchteamCountTTL";     %var1Title = "Switch Teams:";  %var1Name = "Switch Teams";          %var1TypeName = "Total";
         %var2 = "teamkillCountTTL";       %var2Title = "Team Kills:";    %var2Name = "Team Kills";            %var2TypeName = "Average";
         %var3 = "obstimeoutkickCountTTL"; %var3Title = "Observer kick:"; %var3Name = "Observer Timeout Kick"; %var3TypeName = "Max";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);         
         
         %var1 = "explosionKillsTG";          %var1Title = "Explosion Kills:";   %var1Name = "Explosion Kills";            %var1TypeName = "Total";
         %var2 = "explosionDeathsTG";         %var2Title = "Explosion Deaths:";  %var2Name = "Explosion Deaths";           %var2TypeName = "Total";
         %var3 = "forceFieldPowerUpDeathsTG"; %var3Title = "Forcefield Deaths:"; %var3Name = "Forcefield Power Up Deaths"; %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
           
         %var1 = "spawnobstimeoutCountTTL"; %var1Title = "Spawn Timeouts:";  %var1Name = "Spawn Timeouts"; %var1TypeName = "Total";
         %var2 = "weaponpickupCountTTL";    %var2Title = "Weapon Pickups:";  %var2Name = "Weapon Pickups"; %var2TypeName = "Total";
         %var3 = "nullTG";                  %var3Title = "RNG Luck:";        %var3Name = "Random Number";  %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
         
         %var1 = "packpickupCountTTL"; %var1Title = "Pack Pickup:"; %var1Name = "Pack Pickup";     %var1TypeName = "Total";
         %var2 = "flipflopCountTTL";   %var2Title = "Switch Hits:"; %var2Name = "Switch Triggers"; %var2TypeName = "Total";
         %var3 = "lagSpikesTTL";       %var3Title = "Lag Spikes:";  %var3Name = "Lag Spikes";      %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);            
         
         %var1 = "repairpackpickupCountTTL"; %var1Title = "Repair Pickup:"; %var1Name = "Repair Pickup";        %var1TypeName = "Total";
         %var2 = "repairpackpickupEnemyTTL"; %var2Title = "Pickup Enemy:";  %var2Name = "Repair Pickup Enemy";  %var2TypeName = "Total";
         %var3 = "invyEatRepairPackTTL";     %var3Title = "Invy Eat:";      %var3Name = "Repair Packs Eaten";   %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         error(strLen(%nameTitle1));
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
         
         %var1 = "hitHeadTG";  %var1Title = "Head Hits:";  %var1Name = "Head Hits";  %var1TypeName = "Total";
         %var2 = "hitTorsoTG"; %var2Title = "Torso Hits:"; %var2Name = "Torso Hits"; %var2TypeName = "Total";
         %var3 = "hitLegsTG";  %var3Title = "Leg Hits:";   %var3Name = "Leg Hits";   %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
         //12
         %var1 = "lightningDeathsTG";  %var1Title = "Lightning Deaths:";  %var1Name = "Lightning Deaths";   %var1TypeName = "Total";
         %var2 = "lightningKillsTG";   %var2Title = "Lightning Kills:";   %var2Name = "Lightning Kills";    %var2TypeName = "Total";
         %var3 = "lightningMAkillsTG"; %var3Title = "Lightning MA Kills"; %var3Name = "Lightning MA Kills"; %var3TypeName = "Total";
         %i1 = getField($lData::data[%var1,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var1,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i2 = getField($lData::data[%var2,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var2,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %i3 = getField($lData::data[%var3,%client.lgame,%lType,%mon,%year],0) ? getField($lData::name[%var3,%client.lgame,%lType,%mon,%year],0) : %NA; 
         %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
         %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
         %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
         %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLB\t%1\t%2\t%6><clip:197>%3</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%7><clip:197>%4</clip></a>\t<a:gamelink\tS\tLB\t%1\t%2\t%8><clip:197>%5</clip></a>';
         %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
         %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
         %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,0,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
         
         for(%i = %index; %i < 15; %i++)
			   messageClient( %client, 'SetLineHud', "", %tag, %index++, '');
			messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Click on player name to view the top %1 players', $dtStats::topAmount);
      case "LB"://listBoards
         %lType = %client.curLType;
         %page = %client.curPage;
         %mon = getField($lData::mon[%lType, %client.lgame, %page],0);
         %year = getField($lData::mon[%lType, %client.lgame, %page],1);
         if(%client.GlArg4 != 0){ 
            %set = %client.linkSet[%client.GlArg4]; // find the array set 
            %fi = getField(%set,%client.GlArg5);//find the array postion 
            %fieldSet     = %client.statsFieldSet[%fi];
            %field      = getField(%fieldSet,0);
            %name       = getField(%fieldSet,1);
            %fieldName  = getField(%fieldSet,2);
         }
         else{
            %fieldSet     = %client.statsFieldSet[%client.GlArg5];
            %field      = getField(%fieldSet,0);
            %name       = getField(%fieldSet,1);
            %fieldName  = getField(%fieldSet,2);
         }
         messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%2 (Top %1 Players)',$dtStats::topAmount, %name);
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\t%3\t%1\t%2>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,%lType,%client.backPage);
        
         %header = '<tab:5,24,225><color:0befe7>\t#. \t%1\t%2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%name,%fieldName);
         for(%i = 0; %i < getFieldCount($lData::data[%field,%client.lgame,%lType,%mon,%year]) && %i < $dtStats::topAmount; %i++){
            %scoreName  = getField($lData::name[%field,%client.lgame,%lType,%mon,%year],%i);
            %gameScore  = getField($lData::data[%field,%client.lgame,%lType,%mon,%year],%i);
            if(%scoreName !$= "NA"){  
               %gameScore = (strPos(%gameScore,"x") == -1) ? (mFloatLength(%gameScore,2) + 0) : %gameScore;
               %line = '<tab:5,24,225><font:univers condensed:18><color:33CCCC> \t%1. \t%2\t<color:03d597>%3';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i+1,%scoreName,%gameScore);
            }
            else{
               if(%i == 0)
               %line = '<tab:24><color:0befe7>\tNo data for this stat at this time';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
              break;  
            }
         }
      case "LBM"://listBoardsMap
         %map  = %client.GlArg4;
         %GlArg4      = %client.statsFieldSet[%client.GlArg5];
         %field      = getField(%GlArg4,0);
         %name       = getField(%GlArg4,1);
         %fieldName  = getField(%GlArg4,2);
         messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%2 (Top %1 Players)',$dtStats::topAmount, %name);
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tMap\t%1\t%2\t0>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,%map);
        
         %header = '<tab:5,24,225><color:0befe7>\t#. \t%1\t%2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%name,%fieldName);
         for(%i = 0; %i < getFieldCount($lMapData::data[%map,%field,%client.lgame,%client.curMon]) && %i < $dtStats::topAmount; %i++){
            %scoreName  = getField($lMapData::name[%map,%field,%client.lgame,%client.curMon],%i);
            %gameScore  = getField($lMapData::data[%map,%field,%client.lgame,%client.curMon],%i);
            if(%scoreName !$= "NA"){  
               %gameScore = (strPos(%gameScore,"x") == -1) ? (mFloatLength(%gameScore,2) + 0) : %gameScore;
               %line = '<tab:5,24,225><font:univers condensed:18><color:33CCCC> \t%1. \t%2\t<color:03d597>%3';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i+1,%scoreName,%gameScore);
            }
             else{
               if(%i == 0)
               %line = '<tab:24><color:0befe7>\tNo data for this stat at this time';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
              break;  
            }
         } 
      case "Map"://listBoards
         %map = %client.GlArg4;
         messageClient( %client, 'SetScoreHudHeader', "", '<just:center>Map stats for %1 - %2',%map,monthString(%client.curMon));
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tMAPLIST\t%1\t-1\t%2-%3>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,%client.lgame,%client.curMon);
         %NA = "NA";
         %f = -1;
         %gm = %client.lgame;
         switch$(%gm){
            case "CTFGame":
               %var1 = "scoreTG";        %var1Title = "Score:";         %var1Name = "Score";         %var1TypeName = "Total";
               %var2 = "defenseScoreTG"; %var2Title = "Defense Score:"; %var2Name = "Defense Score"; %var2TypeName = "Total";
               %var3 = "offenseScoreTG"; %var3Title = "Offense Score:"; %var3Name = "Offense Score"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "assistTG";    %var1Title = "Assists:";  %var1Name = "Kill Assist";  %var1TypeName = "Total";
               %var2 = "killsTG";     %var2Title = "Kills:";      %var2Name = "Kills";      %var2TypeName = "Total";
               %var3 = "teamKillsTG"; %var3Title = "Team Kills:"; %var3Name = "Team Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "flagCapsTG";    %var1Title = "Flag Caps:";    %var1Name = "Flag Caps";    %var1TypeName = "Total";
               %var2 = "flagGrabsTG";   %var2Title = "Flag Grabs:";   %var2Name = "Flag Grabs";   %var2TypeName = "Total";
               %var3 = "flagReturnsTG"; %var3Title = "Flag Returns:"; %var3Name = "Flag Returns"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "carrierKillsTG";  %var1Title = "Carrier Kills:";  %var1Name = "Carrier Kills";       %var1TypeName = "Total";
               %var2 = "escortAssistsTG"; %var2Title = "Escort Assists:"; %var2Name = "Flag Escort Assists"; %var2TypeName = "Total";
               %var3 = "flagDefendsTG";   %var3Title = "Flag Defends:";   %var3Name = "Flag Returns";        %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "heldTimeSecMin";   %var1Title = "Flag Cap Time:";   %var1Name = "Flag Capture Time";       %var1TypeName = "Time In Secs";
               %var2 = "grabSpeedMax";     %var2Title = "Flag Grab Speed:"; %var2Name = "Flag Grab Speed";         %var2TypeName = "Highest Speed KM/H";
               %var3 = "capEfficiencyAvg"; %var3Title = "Flag Cap Eff:";    %var3Name = "Flag Capture Efficiency"; %var3TypeName = "Percentage";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
                            
               %var1 = "destructionTG"; %var1Title = "Assets Destroyed:"; %var1Name = "Assets Destroyed"; %var1TypeName = "Total";
               %var2 = "repairsTG";     %var2Title = "Repairs:";          %var2Name = "Repaired Base Items";  %var2TypeName = "Total";
               %var3 = "genDefendsTG";  %var3Title = "Gen Defends:";      %var3Name = "Generator Defends";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "roadKillsTG";        %var1Title = "Road Kills:";    %var1Name = "Road Kills";         %var1TypeName = "Total";
               %var2 = "vehicleScoreTG";     %var2Title = "Vehicle Score:"; %var2Name = "Vehicle Score";      %var2TypeName = "Total";
               %var3 = "bomberBombsKillsTG"; %var3Title = "Bomber Kills:";  %var3Name = "Bomber Bombs Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "discKillsTG";    %var1Title = "Disc Kills:";   %var1Name = "Spinfusor Kills";   %var1TypeName = "Total";
               %var2 = "discMATG";       %var2Title = "Disc MidAirs:"; %var2Name = "Spinfusor MidAirs"; %var2TypeName = "Total";
               %var3 = "minePlusDiscTG"; %var3Title = "Mine + Disc:";  %var3Name = "Mine + Disc Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "laserKillsTG";    %var1Title = "Laser Kills:";    %var1Name = "Laser Rifle Kills";        %var1TypeName = "Total";
               %var2 = "laserHeadShotTG"; %var2Title = "Head Shots:";     %var2Name = "Laser Rifle Head Shots";   %var2TypeName = "Total";
               %var3 = "laserHitDistMax"; %var3Title = "Laser Max Dist:"; %var3Name = "Laser Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "shockKillsTG";    %var1Title = "Shock Kills:";  %var1Name = "Shocklance Kills";     %var1TypeName = "Total";
               %var2 = "shockRearShotTG"; %var2Title = "Shock Rear:";   %var2Name = "Rear Shocklance Hits"; %var2TypeName = "Total";
               %var3 = "shockMATG";       %var3Title = "Shock MidAir:"; %var3Name = "Shocklance MidAIrs";   %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "plasmaKillsTG";    %var1Title = "Plasma Kills:";    %var1Name = "Plasma Rifle Kills";        %var1TypeName = "Total";
               %var2 = "plasmaMATG";       %var2Title = "Plasma MidAirs:";  %var2Name = "Plasma Rifle MidAirs";      %var2TypeName = "Total";
               %var3 = "plasmaHitDistMax"; %var3Title = "Plasma Max Dist:"; %var3Name = "Plasma Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "cgKillsTG";    %var1Title = "Chaingun Kills:";    %var1Name = "ChaingunKills";         %var1TypeName = "Total";
               %var2 = "cgACCAvg";     %var2Title = "Chaingun MidAirs:";  %var2Name = "Chaingun MidAirs";      %var2TypeName = "Total";
               %var3 = "cgHitDistMax"; %var3Title = "Chaingun Max Dist:"; %var3Name = "Chaingun Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "blasterKillsTG";    %var1Title = "Blaster Kills:";    %var1Name = "Blaster Kills";        %var1TypeName = "Total";
               %var2 = "blasterMATG";       %var2Title = "Blaster MidAirs:";  %var2Name = "Blaster MidAirs";      %var2TypeName = "Total";
               %var3 = "blasterHitDistMax"; %var3Title = "Blaster Max Dist:"; %var3Name = "Blaster Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "grenadeKillsTG"; %var1Title = "GrenadeL Kills:"; %var1Name = "Grenade Launcher Kills"; %var1TypeName = "Total";
               %var2 = "mortarKillsTG";  %var2Title = "Mortar Kills:";   %var2Name = "Fusion Mortar Kills";    %var2TypeName = "Total";
               %var3 = "missileKillsTG"; %var3Title = "Missile Kills:";  %var3Name = "Missile Launcher";       %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mineKillsTG";     %var1Title = "Mine Kills:";      %var1Name = "Mine Kills";           %var1TypeName = "Total";
               %var2 = "hGrenadeKillsTG"; %var2Title = "H-Grenade Kills:"; %var2Name = "Hand Grenade";         %var2TypeName = "Total";
               %var3 = "satchelKillsTG";  %var3Title = "Satchel Kills:";   %var3Name = "Satchel Charge Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
            case "SCtFGame":
               %var1 = "scoreTG";        %var1Title = "Score:";         %var1Name = "Score";         %var1TypeName = "Total";
               %var2 = "defenseScoreTG"; %var2Title = "Defense Score:"; %var2Name = "Defense Score"; %var2TypeName = "Total";
               %var3 = "offenseScoreTG"; %var3Title = "Offense Score:"; %var3Name = "Offense Score"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "assistTG";    %var1Title = "Kill Assists:"; %var1Name = "Kill Assists"; %var1TypeName = "Total";
               %var2 = "killsTG";     %var2Title = "Kills:";        %var2Name = "Kills";        %var2TypeName = "Total";
               %var3 = "teamKillsTG"; %var3Title = "Team Kills:";   %var3Name = "Team Kills";   %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "flagCapsTG";    %var1Title = "Flag Caps:";    %var1Name = "Flag Caps";    %var1TypeName = "Total";
               %var2 = "flagGrabsTG";   %var2Title = "Flag Grabs:";   %var2Name = "Flag Grabs";   %var2TypeName = "Total";
               %var3 = "flagReturnsTG"; %var3Title = "Flag Returns:"; %var3Name = "Flag Returns"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "carrierKillsTG";  %var1Title = "Carrier Kills:";  %var1Name = "Carrier Kills";       %var1TypeName = "Total";
               %var2 = "escortAssistsTG"; %var2Title = "Escort Assists:"; %var2Name = "Flag Escort Assists"; %var2TypeName = "Total";
               %var3 = "flagDefendsTG";   %var3Title = "Flag Defends:";   %var3Name = "Flag Returns";        %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "heldTimeSecMin";   %var1Title = "Flag Cap Time:";  %var1Name = "Flag Capture Time";        %var1TypeName = "Time In Secs";
               %var2 = "grabSpeedMax";     %var2Title = "Flag Grab Speed:"; %var2Name = "Flag Grab Speed";         %var2TypeName = "Highest Speed KM/H";
               %var3 = "capEfficiencyAvg"; %var3Title = "Flag Cap Eff:";    %var3Name = "Flag Capture Efficiency"; %var3TypeName = "Percentage";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "heldTimeSecAvgi";  %var1Title = "Cap Time Avg:";    %var1Name = "Flag Capture Time";       %var1TypeName = "Time In Secs";
               %var2 = "grabSpeedAvg";     %var2Title = "Grab Speed Avg:";  %var2Name = "Flag Grab Speed";         %var2TypeName = "Average KM/H";
               %var3 = "capEfficiencyAvg"; %var3Title = "Cap Eff Avg:";     %var3Name = "Flag Capture Efficiency"; %var3TypeName = "Percentage";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "discKillsTG";    %var1Title = "Disc Kills:";   %var1Name = "Spinfusor Kills";   %var1TypeName = "Total";
               %var2 = "discMATG";       %var2Title = "Disc MidAirs:"; %var2Name = "Spinfusor MidAirs"; %var2TypeName = "Total";
               %var3 = "minePlusDiscTG"; %var3Title = "Mine + Disc:";  %var3Name = "Mine + Disc Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "laserKillsTG";    %var1Title = "Laser Kills:";    %var1Name = "Laser Rifle Kills";        %var1TypeName = "Total";
               %var2 = "laserHeadShotTG"; %var2Title = "Head Shots:";     %var2Name = "Laser Rifle Head Shots";   %var2TypeName = "Total";
               %var3 = "laserHitDistMax"; %var3Title = "Laser Max Dist:"; %var3Name = "Laser Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "shockKillsTG";    %var1Title = "Shock Kills:";  %var1Name = "Shocklance Kills";     %var1TypeName = "Total";
               %var2 = "shockRearShotTG"; %var2Title = "Shock Rear:";   %var2Name = "Rear Shocklance Hits"; %var2TypeName = "Total";
               %var3 = "shockMATG";       %var3Title = "Shock MidAir:"; %var3Name = "Shocklance MidAIrs";   %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "plasmaKillsTG";    %var1Title = "Plasma Kills:";    %var1Name = "Plasma Rifle Kills";        %var1TypeName = "Total";
               %var2 = "plasmaMATG";       %var2Title = "Plasma MidAirs:";  %var2Name = "Plasma Rifle MidAirs";      %var2TypeName = "Total";
               %var3 = "plasmaHitDistMax"; %var3Title = "Plasma Max Dist:"; %var3Name = "Plasma Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "grenadeKillsTG";    %var1Title = "GrenadeL Kills:";    %var1Name = "Grenade Launcher Kills";        %var1TypeName = "Total";
               %var2 = "grenadeMATG";       %var2Title = "GrenadeL MidAirs:";  %var2Name = "Grenade Launcher MidAirs";      %var2TypeName = "Total";
               %var3 = "grenadeHitDistMax"; %var3Title = "GrenadeL Max Dist:"; %var3Name = "Grenade Launcher Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "cgKillsTG";    %var1Title = "Chaingun Kills:";    %var1Name = "ChaingunKills";         %var1TypeName = "Total";
               %var2 = "cgACCAvg";     %var2Title = "Chaingun MidAirs:";  %var2Name = "Chaingun MidAirs";      %var2TypeName = "Total";
               %var3 = "cgHitDistMax"; %var3Title = "Chaingun Max Dist:"; %var3Name = "Chaingun Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "blasterHitSVMax"; %var1Title = "Blaster Kills:";    %var1Name = "Blaster Kills";        %var1TypeName = "Total";
               %var2 = "blasterDmgTG";    %var2Title = "Blaster MidAirs:";  %var2Name = "Blaster MidAirs";      %var2TypeName = "Total";
               %var3 = "blasterComTG";    %var3Title = "Blaster Max Dist:"; %var3Name = "Blaster Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mineKillsTG";    %var1Title = "Mine Kills:";    %var1Name = "Mine Kills";        %var1TypeName = "Total";
               %var2 = "mineMATG";       %var2Title = "Mine MidAirs:";  %var2Name = "MineMidAirs";       %var2TypeName = "Total";
               %var3 = "mineHitDistMax"; %var3Title = "Mine Max Dist:"; %var3Name = "Mine Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "hGrenadeKillsTG";    %var1Title = "HGrenade Kills:";    %var1Name = "Hand Grenade Kills";        %var1TypeName = "Total";
               %var2 = "hGrenadeMATG";       %var2Title = "HGrenade MidAirs:";  %var1Name = "Hand Grenade MidAirs";      %var2TypeName = "Total";
               %var3 = "hGrenadeHitDistMax"; %var3Title = "HGrenade Max Dist:"; %var3Name = "Hand Grenade Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
            case "LakRabbitGame":  
               %var1 = "scoreTG";  %var1Title = "Score:";   %var1Name = "Score";   %var1TypeName = "Total";
               %var2 = "killsTG";  %var2Title = "Kills:";   %var2Name = "Kills";   %var2TypeName = "Total";
               %var3 = "assistTG"; %var3Title = "Assists:"; %var3Name = "Assists"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "flagGrabsTG";       %var1Title = "Flag Grabs:";        %var1Name = "Flag Grabs";        %var1TypeName = "Total";
               %var2 = "MidairflagGrabsTG"; %var2Title = "MidAir Flag Grabs:"; %var2Name = "MidAir Flag Grabs"; %var2TypeName = "Total";
               %var3 = "flagTimeMinTG";     %var3Title = "Flag Held Time:";    %var3Name = "Flag Held Time";    %var3TypeName = "Minutes";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "discKillsTG";    %var1Title = "Spinfusor Kills:";    %var1Name = "Spinfusor Kills";        %var1TypeName = "Total";
               %var2 = "discMATG";       %var2Title = "Spinfusor MidAirs:";  %var2Name = "Spinfusor MidAirs";      %var2TypeName = "Total";
               %var3 = "discHitDistMax"; %var3Title = "Spinfusor Max Dist:"; %var3Name = "Spinfusor Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "discDmgTG";      %var1Title = "Spinfusor Damage:"; %var1Name = "Spinfusor Damage"; %var1TypeName = "Total";
               %var2 = "discCom";        %var2Title = "Spinfusor Combos:"; %var2Name = "Spinfusor Combos"; %var2TypeName = "Total";
               %var3 = "minePlusDiscTG"; %var3Title = "Mine + Disc:";      %var3Name = "Mine + Disc Hits"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "shockKillsTG";    %var1Title = "Shock Kills:";    %var1Name = "Shocklance Kills";        %var1TypeName = "Total";
               %var2 = "shockMATG";       %var2Title = "Shock MidAirs:";  %var2Name = "Shocklance MidAirs";      %var2TypeName = "Total";
               %var3 = "shockRearShotTG"; %var3Title = "Shock Max Dist:"; %var3Name = "Shocklance Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "shockHitSVMax"; %var1Title = "Shock Speed:";  %var1Name = "Shocklance Max Speed"; %var1TypeName = "KM/H";
               %var2 = "shockDmgTG";    %var2Title = "Shock Damage:"; %var2Name = "Shocklance Damage";    %var2TypeName = "Total";
               %var3 = "shockComTG";    %var3Title = "Shock Combo:";  %var3Name = "Shocklance Combos";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
                
               %var1 = "plasmaKillsTG";    %var1Title = "Plasma Kills:";    %var1Name = "Plasma Kills";        %var1TypeName = "Total";
               %var2 = "plasmaMATG";       %var2Title = "Plasma MidAirs:";  %var2Name = "Plasma MidAirs";      %var2TypeName = "Total";
               %var3 = "plasmaHitDistMax"; %var3Title = "Plasma Max Dist:"; %var3Name = "Plasma Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "plasmaHitSVMax"; %var1Title = "Plasma Speed:";  %var1Name = "Plasma Max Speed"; %var1TypeName = "KM/H";
               %var2 = "plasmaDmgTG";    %var2Title = "Plasma Damage:"; %var2Name = "Plasma Damage";    %var2TypeName = "Total";
               %var3 = "plasmaComTG";    %var3Title = "Plasma Combos:"; %var3Name = "Plasma Combos";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "grenadeKillsTG";    %var1Title = "GrenadeL Kills:";    %var1Name = "Grenade Launcher Kills";        %var1TypeName = "Total";
               %var2 = "grenadeMATG";       %var2Title = "GrenadeL MidAirs:";  %var2Name = "Grenade Launcher MidAirs";      %var2TypeName = "Total";
               %var3 = "grenadeHitDistMax"; %var3Title = "GrenadeL Max Dist:"; %var3Name = "Grenade Launcher Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "grenadeHitSVMax"; %var1Title = "GrenadeL Speed:";  %var1Name = "Grenade Launcher Max Speed"; %var1TypeName = "KM/H";
               %var2 = "grenadeDmgTG";    %var2Title = "GrenadeL Damage:"; %var2Name = "Grenade Launcher Damage";    %var2TypeName = "Total";
               %var3 = "grenadeComTG";    %var3Title = "GrenadeL Combos:"; %var3Name = "Grenade Launcher Combos";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "blasterKillsTG";    %var1Title = "Blaster Kills:";    %var1Name = "Blaster Kills";        %var1TypeName = "Total";
               %var2 = "blasterMATG";       %var2Title = "Blaster MidAirs:";  %var2Name = "Blaster MidAirs";      %var2TypeName = "Total";
               %var3 = "blasterHitDistMax"; %var3Title = "Blaster Max Dist:"; %var3Name = "Blaster Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "blasterHitSVMax"; %var1Title = "Blaster Speed:";  %var1Name = "Blaster Max Speed"; %var1TypeName = "KM/H";
               %var2 = "blasterDmgTG";    %var2Title = "Blaster Damage:"; %var2Name = "Blaster Damage";    %var2TypeName = "Total";
               %var3 = "blasterComTG";    %var3Title = "Blaster Combos:"; %var3Name = "Blaster Combos";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "mineKillsTG";       %var1Title = "Mine Kills:";    %var1Name = "Mine Kills";        %var1TypeName = "Total";
               %var2 = "mineMATG";          %var2Title = "Mine MidAirs:";  %var2Name = "Mine MidAirs";      %var2TypeName = "Total";
               %var3 = "mineHitDistMax";    %var3Title = "Mine Max Dist:"; %var3Name = "Mine Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "mineHitVVMax"; %var1Title = "Mine Speed:";  %var1Name = "Mine Max Speed"; %var1TypeName = "KM/H";
               %var2 = "mineDmgTG";    %var2Title = "Mine Damage:"; %var2Name = "Mine Damage";    %var2TypeName = "Total";
               %var3 = "mineComTG";    %var3Title = "Mine Combos:"; %var3Name = "Mine Combos";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);   
               
               %var1 = "hGrenadeKillsTG";    %var1Title = "HGrenade Kills:";    %var1Name = "Hand Grenade Kills";        %var1TypeName = "Total";
               %var2 = "hGrenadeMATG";       %var2Title = "HGrenade MidAirs:";  %var2Name = "Hand Grenade MidAirs";      %var2TypeName = "Total";
               %var3 = "hGrenadeHitDistMax"; %var3Title = "HGrenade Max Dist:"; %var3Name = "Hand Grenade Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "hGrenadeHitSVMax"; %var1Title = "HGrenade Speed:";  %var1Name = "Hand Grenade Max Speed"; %var1TypeName = "KM/H";
               %var2 = "hGrenadeDmgTG";    %var2Title = "HGrenade Damage:"; %var2Name = "Hand Grenade Damage";    %var2TypeName = "Total";
               %var3 = "hGrenadeComTG";    %var3Title = "HGrenade Combos:"; %var3Name = "Hand Grenade Combos";    %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
            case "DMGame":
               %var1 = "scoreTG";  %var1Title = "Score:";   %var1Name = "Score";   %var1TypeName = "Total";
               %var2 = "killsTG";  %var2Title = "Kills:";   %var2Name = "Kills";   %var2TypeName = "Total";
               %var3 = "assistTG"; %var3Title = "Assists:"; %var3Name = "Assists"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "efficiencyAvg"; %var1Title = "Efficiency Avg:";    %var1Name = "Efficiency Avg"; %var1TypeName = "Value";
               %var2 = "timeTLAvg";     %var2Title = "Survival Time Avg:"; %var2Name = "Survival Time";  %var2TypeName = "Seconds";
               %var3 = "distMovTG";     %var3Title = "Distance Moved:";    %var3Name = "Distance Moved"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "killAirTG";    %var1Title = "Air Kills:";           %var1Name = "Air kills";           %var1TypeName = "Total";
               %var2 = "killGroundTG"; %var2Title = "Ground Kills:";        %var2Name = "Ground Kills";        %var2TypeName = "Total";
               %var3 = "EVKillsTG";    %var3Title = "Environmental Kills:"; %var3Name = "Environmental Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "firstKillTG";  %var1Title = "First Kills:";       %var1Name = "First kills";       %var1TypeName = "Total";
               %var2 = "lastKillTG";   %var2Title = "Last Kills:";        %var2Name = "Last Kills";        %var2TypeName = "Total";
               %var3 = "deathKillsTG"; %var3Title = "Kills After Death:"; %var3Name = "Kills After Death"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);  
               
               %var1 = "doubleChainKillTG";    %var1Title = "Double Kills:"; %var1Name = "Double Kills";   %var1TypeName = "Total";
               %var2 = "tripleChainKillTG";    %var2Title = "Triple Kills:"; %var2Name = "Triple Kills";   %var2TypeName = "Total";
               %var3 = "quadrupleChainKillTG"; %var3Title = "Quad kills:";   %var3Name = "Quadruple Kill"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "killStreakMax"; %var1Title = "Kill Streak:";   %var1Name = "Highest Kill Streak";  %var1TypeName = "Total";
               %var2 = "comboCountTG";  %var2Title = "Weapon Combos:"; %var1Name = "Weapon Combos";        %var2TypeName = "Total";
               %var3 = "kdrAvg";        %var3Title = "K/D Ratio Avg:"; %var3Name = "Kill Death Average"; %var3TypeName = "Percentage";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "discKillsTG";    %var1Title = "Spinfusor Kills:";   %var1Name = "Spinfusor Kills";   %var1TypeName = "Total";
               %var2 = "discMATG";       %var2Title = "Spinfusor MidAirs:"; %var2Name = "Spinfusor MidAirs"; %var2TypeName = "Total";
               %var3 = "minePlusDiscTG"; %var3Title = "Mine + Disc:";       %var3Name = "Mine + Disc Hits";  %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "plasmaKillsTG";    %var1Title = "Plasma Kills:";    %var1Name = "Plasma Rifle Kills";        %var1TypeName = "Total";
               %var2 = "plasmaMATG";       %var2Title = "Plasma MidAirs:";  %var2Name = "Plasma Rifle MidAirs";      %var2TypeName = "Total";
               %var3 = "plasmaHitDistMax"; %var3Title = "Plasma Max Dist:"; %var3Name = "Plasma Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "grenadeKillsTG";    %var1Title = "GrenadeL Kills:";    %var1Name = "Grenade Launcher Kills";        %var1TypeName = "Total";
               %var2 = "grenadeMATG";       %var2Title = "GrenadeL MidAirs:";  %var2Name = "Grenade Launcher MidAirs";      %var2TypeName = "Total";
               %var3 = "grenadeHitDistMax"; %var3Title = "GrenadeL Max Dist:"; %var3Name = "Grenade Launcher Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "laserKillsTG";    %var1Title = "Laser Kills:";    %var1Name = "Laser Rifle Kills";        %var1TypeName = "Total";
               %var2 = "laserHeadShotTG"; %var2Title = "Head Shots:";     %var2Name = "Laser Rifle Head Shots";   %var2TypeName = "Total";
               %var3 = "laserHitDistMax"; %var3Title = "Laser Max Dist:"; %var3Name = "Laser Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "shockKillsTG";    %var1Title = "Shock Kills:";  %var1Name = "Shocklance Kills";     %var1TypeName = "Total";
               %var2 = "shockRearShotTG"; %var2Title = "Shock Rear:";   %var2Name = "Rear Shocklance Hits"; %var2TypeName = "Total";
               %var3 = "shockMATG";       %var3Title = "Shock MidAir:"; %var3Name = "Shocklance MidAIrs";   %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mortarKillsTG";    %var1Title = "Mortar Kills:";    %var1Name = "Mortar Kills";        %var1TypeName = "Total";
               %var2 = "mortarMATG";       %var3Title = "Mortar MidAir:";   %var3Name = "Mortar MidAIrs";      %var3TypeName = "Total";
               %var3 = "mortarHitDistMax"; %var3Title = "Mortar Max Dist:"; %var3Name = "Mortar Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
                              
               %var1 = "cgKillsTG";    %var1Title = "Chaingun Kills:";    %var1Name = "ChaingunKills";         %var1TypeName = "Total";
               %var2 = "cgACCAvg";     %var2Title = "Chaingun MidAirs:";  %var2Name = "Chaingun MidAirs";      %var2TypeName = "Total";
               %var3 = "cgHitDistMax"; %var3Title = "Chaingun Max Dist:"; %var3Name = "Chaingun Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "blasterKillsTG"; %var1Title = "Blaster Kills:";    %var1Name = "Blaster Kills";        %var1TypeName = "Total";
               %var2 = "blasterMATG";    %var2Title = "Blaster MidAirs:";  %var2Name = "Blaster MidAirs";      %var2TypeName = "Total";
               %var3 = "blasterHitDistMax";    %var3Title = "Blaster Max Dist:"; %var3Name = "Blaster Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mineKillsTG";    %var1Title = "Mine Kills:";    %var1Name = "Mine Kills";        %var1TypeName = "Total";
               %var2 = "mineMATG";       %var2Title = "Mine MidAirs:";  %var2Name = "Mine MidAirs";      %var2TypeName = "Total";
               %var3 = "mineHitDistMax"; %var3Title = "Mine Max Dist:"; %var3Name = "Mine Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "hGrenadeKillsTG";       %var1Title = "HGrenade Kills:";    %var1Name = "Hand Grenade Kills";        %var1TypeName = "Total";
               %var2 = "hGrenadeMATG";          %var2Title = "HGrenade MidAirs:";  %var2Name = "Hand Grenade MidAirs";      %var2TypeName = "Total";
               %var3 = "hGrenadeHitDistMax"; %var3Title = "HGrenade Max Dist:"; %var3Name = "Hand Grenade Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
                
            case "DuelGame":
               %var1 = "scoreTG";  %var1Title = "Score:";  %var1Name = "Score";   %var1TypeName = "Total";
               %var2 = "killsTG";  %var2Title = "Kills:";  %var2Name = "Kills";   %var2TypeName = "Total";
               %var3 = "deathsTG"; %var3Title = "Deaths:"; %var3Name = "Deaths"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "killAirTG";  %var1Title = "Air Kills:";    %var1Name = "Air Kills";    %var1TypeName = "Total";
               %var2 = "deathAirTG"; %var2Title = "Air Deaths:";   %var2Name = "Air Deaths";   %var2TypeName = "Total";
               %var3 = "airTimeAvg"; %var3Title = "Air Time Avg:"; %var3Name = "Air Time Average"; %var3TypeName = "Average";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "killGroundTG";  %var1Title = "Ground Kills:";    %var1Name = "Ground Kills";        %var1TypeName = "Total";
               %var2 = "deathGroundTG"; %var2Title = "Ground Deaths:";   %var2Name = "Ground Deaths";       %var2TypeName = "Total";
               %var3 = "groundTimeAvg"; %var3Title = "Ground Time Avg:"; %var3Name = "Ground Time Average"; %var3TypeName = "Average";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "kdrAvg";       %var1Title = "K/D Ratio Avg:";       %var1Name = "Kill Death Average";  %var1TypeName = "Percentage";
               %var2 = "EVKillsTG";    %var2Title = "Environmental Kills:"; %var2Name = "Environmental Kills"; %var2TypeName = "Total";
               %var3 = "comboCountTG"; %var3Title = "Weapon Combos:";       %var3Name = "Weapon Combos"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "distMovTG";   %var1Title = "Distance Moved";     %var1Name = "Distance Moved";         %var1TypeName = "Percentage";
               %var2 = "maxSpeedMax"; %var2Title = "Highest  Speed:";    %var2Name = "Highest Speed";          %var2TypeName = "KM/H";
               %var3 = "timeTLAvg";   %var3Title = "Survival Time Avg:"; %var3Name = "Survival Time Average "; %var3TypeName = "seconds ";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "discDmgTG";      %var1Title = "Spinfusor Damage:"; %var1Name = "Spinfusor Damage"; %var1TypeName = "Total";
               %var2 = "discCom";        %var2Title = "Spinfusor Combos:"; %var2Name = "Spinfusor Combos"; %var2TypeName = "Total";
               %var3 = "minePlusDiscTG"; %var3Title = "Mine + Disc:";      %var3Name = "Mine + Disc Hits"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "plasmaKillsTG";    %var1Title = "Plasma Kills:";    %var1Name = "Plasma Rifle Kills";        %var1TypeName = "Total";
               %var2 = "plasmaMATG";       %var2Title = "Plasma MidAirs:";  %var2Name = "Plasma Rifle MidAirs";      %var2TypeName = "Total";
               %var3 = "plasmaHitDistMax"; %var3Title = "Plasma Max Dist:"; %var3Name = "Plasma Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "grenadeKillsTG";    %var1Title = "GrenadeL Kills:";    %var1Name = "Grenade Launcher Kills";        %var1TypeName = "Total";
               %var2 = "grenadeMATG";       %var2Title = "GrenadeL MidAirs:";  %var2Name = "Grenade Launcher MidAirs";      %var2TypeName = "Total";
               %var3 = "grenadeHitDistMax"; %var3Title = "GrenadeL Max Dist:"; %var3Name = "Grenade Launcher Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "laserKillsTG";    %var1Title = "Laser Kills:";    %var1Name = "Laser Rifle Kills";        %var1TypeName = "Total";
               %var2 = "laserHeadShotTG"; %var2Title = "Head Shots:";     %var2Name = "Laser Rifle Head Shots";   %var2TypeName = "Total";
               %var3 = "laserHitDistMax"; %var3Title = "Laser Max Dist:"; %var3Name = "Laser Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "shockKillsTG";    %var1Title = "Shock Kills:";  %var1Name = "Shocklance Kills";     %var1TypeName = "Total";
               %var2 = "shockRearShotTG"; %var2Title = "Shock Rear:";   %var2Name = "Rear Shocklance Hits"; %var2TypeName = "Total";
               %var3 = "shockMATG";       %var3Title = "Shock MidAir:"; %var3Name = "Shocklance MidAIrs";   %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mortarKillsTG";    %var1Title = "Mortar Kills:";    %var1Name = "Mortar Kills";        %var1TypeName = "Total";
               %var2 = "mortarMATG";       %var2Title = "Mortar MidAir:";   %var2Name = "Mortar MidAIrs";      %var2TypeName = "Total";
               %var3 = "mortarHitDistMax"; %var3Title = "Mortar Max Dist:"; %var3Name = "Mortar Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
                              
               %var1 = "cgKillsTG";    %var1Title = "Chaingun Kills:";    %var1Name = "ChaingunKills";         %var1TypeName = "Total";
               %var2 = "cgACCAvg";     %var2Title = "Chaingun MidAirs:";  %var2Name = "Chaingun MidAirs";      %var2TypeName = "Total";
               %var3 = "cgHitDistMax"; %var3Title = "Chaingun Max Dist:"; %var3Name = "Chaingun Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "blasterHitSVMax"; %var1Title = "Blaster Kills:";    %var1Name = "Blaster Kills";        %var1TypeName = "Total";
               %var2 = "blasterDmgTG";    %var2Title = "Blaster MidAirs:";  %var2Name = "Blaster MidAirs";      %var2TypeName = "Total";
               %var3 = "blasterComTG";    %var3Title = "Blaster Max Dist:"; %var3Name = "Blaster Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mineKillsTG";    %var1Title = "Mine Kills:";    %var1Name = "Mine Kills";        %var1TypeName = "Total";
               %var2 = "mineMATG";       %var2Title = "Mine MidAirs:";  %var2Name = "Mine MidAirs";      %var2TypeName = "Total";
               %var3 = "mineHitDistMax"; %var3Title = "Mine Max Dist:"; %var3Name = "Mine Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "hGrenadeKillsTG";    %var1Title = "HGrenade Kills:";    %var1Name = "Hand Grenade Kills";        %var1TypeName = "Total";
               %var2 = "hGrenadeMATG";       %var2Title = "HGrenade MidAirs:";  %var2Name = "Hand Grenade MidAirs";      %var2TypeName = "Total";
               %var3 = "hGrenadeHitDistMax"; %var3Title = "HGrenade Max Dist:"; %var3Name = "Hand Grenade Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
            case "ArenaGame":
               %var1 = "scoreTG";      %var1Title = "Score:";   %var1Name = "Score";   %var1TypeName = "Total";
               %var2 = "roundKillsTG"; %var2Title = "Kills:";   %var2Name = "Kills";   %var2TypeName = "Total";
               %var3 = "assistTG";     %var3Title = "Assists:"; %var3Name = "Assists"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "roundsWonTG"; %var1Title = "Rounds Won:"; %var1Name = "Rounds Won"; %var1TypeName = "Total";
               %var2 = "teamKillsTG"; %var2Title = "Team Kills:"; %var2Name = "Team Kills"; %var2TypeName = "Total";
               %var3 = "hatTricksTG"; %var3Title = "Hat Tricks:"; %var3Name = "Hat Tricks"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "maxSpeedMax"; %var1Title = "Highest  Speed:";    %var1Name = "Highest  Speed"; %var1TypeName = "KM/H";
               %var2 = "timeTLAvg";   %var2Title = "Survival Time Avg:"; %var2Name = "Survival Time";  %var2TypeName = "Seconds ";
               %var3 = "distMovTG";   %var3Title = "Distance Moved:";    %var3Name = "Distance Moved"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "killAirTG";    %var1Title = "Air Kills:";           %var1Name = "Air kills";           %var1TypeName = "Total";
               %var2 = "killGroundTG"; %var2Title = "Ground Kills:";        %var2Name = "Ground Kills";        %var2TypeName = "Total";
               %var3 = "EVKillsTG";    %var3Title = "Environmental Kills:"; %var3Name = "Environmental Kills"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "firstKillTG";  %var1Title = "First Kills:";       %var1Name = "First kills";       %var1TypeName = "Total";
               %var2 = "lastKillTG";   %var2Title = "Last Kills:";        %var2Name = "Last Kills";        %var2TypeName = "Total";
               %var3 = "deathKillsTG"; %var3Title = "Kills After Death:"; %var3Name = "Kills After Death"; %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "killStreakMax"; %var1Title = "Kill Streak:";   %var1Name = "Highest Kill Streak";  %var1TypeName = "Total";
               %var2 = "comboCountTG";  %var2Title = "Weapon Combos:"; %var1Name = "Weapon Combos";        %var2TypeName = "Total";
               %var3 = "kdrAvg";        %var3Title = "K/D Ratio Avg:"; %var3Name = "Kill Death Average"; %var3TypeName = "Percentage";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3); 
               
               %var1 = "discDmgTG";      %var1Title = "Spinfusor Damage:"; %var1Name = "Spinfusor Damage"; %var1TypeName = "Total";
               %var2 = "discCom";        %var2Title = "Spinfusor Combos:"; %var2Name = "Spinfusor Combos"; %var2TypeName = "Total";
               %var3 = "minePlusDiscTG"; %var3Title = "Mine + Disc:";      %var3Name = "Mine + Disc Hits"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "plasmaKillsTG";    %var1Title = "Plasma Kills:";    %var1Name = "Plasma Rifle Kills";        %var1TypeName = "Total";
               %var2 = "plasmaMATG";       %var2Title = "Plasma MidAirs:";  %var2Name = "Plasma Rifle MidAirs";      %var2TypeName = "Total";
               %var3 = "plasmaHitDistMax"; %var3Title = "Plasma Max Dist:"; %var3Name = "Plasma Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "grenadeKillsTG";    %var1Title = "GrenadeL Kills:";    %var1Name = "Grenade Launcher Kills";        %var1TypeName = "Total";
               %var2 = "grenadeMATG";       %var2Title = "GrenadeL MidAirs:";  %var2Name = "Grenade Launcher MidAirs";      %var2TypeName = "Total";
               %var3 = "grenadeHitDistMax"; %var3Title = "GrenadeL Max Dist:"; %var3Name = "Grenade Launcher Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "laserKillsTG";    %var1Title = "Laser Kills:";    %var1Name = "Laser Rifle Kills";        %var1TypeName = "Total";
               %var2 = "laserHeadShotTG"; %var2Title = "Head Shots:";     %var2Name = "Laser Rifle Head Shots";   %var2TypeName = "Total";
               %var3 = "laserHitDistMax"; %var3Title = "Laser Max Dist:"; %var3Name = "Laser Rifle Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "shockKillsTG";    %var1Title = "Shock Kills:";  %var1Name = "Shocklance Kills";     %var1TypeName = "Total";
               %var2 = "shockRearShotTG"; %var2Title = "Shock Rear:";   %var2Name = "Rear Shocklance Hits"; %var2TypeName = "Total";
               %var3 = "shockMATG";       %var3Title = "Shock MidAir:"; %var3Name = "Shocklance MidAIrs";   %var3TypeName = "Total";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mortarKillsTG";    %var1Title = "Mortar Kills:";    %var1Name = "Mortar Kills";        %var1TypeName = "Total";
               %var2 = "mortarMATG";       %var2Title = "Mortar MidAir:";   %var2Name = "Mortar MidAIrs";      %var2TypeName = "Total";
               %var3 = "mortarHitDistMax"; %var3Title = "Mortar Max Dist:"; %var3Name = "Mortar Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
                              
               %var1 = "cgKillsTG";    %var1Title = "Chaingun Kills:";    %var1Name = "ChaingunKills";         %var1TypeName = "Total";
               %var2 = "cgACCAvg";     %var2Title = "Chaingun MidAirs:";  %var2Name = "Chaingun MidAirs";      %var2TypeName = "Total";
               %var3 = "cgHitDistMax"; %var3Title = "Chaingun Max Dist:"; %var3Name = "Chaingun Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "blasterHitSVMax"; %var1Title = "Blaster Kills:";    %var1Name = "Blaster Kills";        %var1TypeName = "Total";
               %var2 = "blasterDmgTG";    %var2Title = "Blaster MidAirs:";  %var2Name = "Blaster MidAirs";      %var2TypeName = "Total";
               %var3 = "blasterComTG";    %var3Title = "Blaster Max Dist:"; %var3Name = "Blaster Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "mineKillsTG";    %var1Title = "Mine Kills:";    %var1Name = "Mine Kills";        %var1TypeName = "Total";
               %var2 = "mineMATG";       %var2Title = "Mine MidAirs:";  %var2Name = "Mine MidAirs";      %var2TypeName = "Total";
               %var3 = "mineHitDistMax"; %var3Title = "Mine Max Dist:"; %var3Name = "Mine Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
               
               %var1 = "hGrenadeKillsTG";    %var1Title = "HGrenade Kills:";    %var1Name = "Hand Grenade Kills";        %var1TypeName = "Total";
               %var2 = "hGrenadeMATG";       %var2Title = "HGrenade MidAirs:";  %var2Name = "Hand Grenade MidAirs";      %var2TypeName = "Total";
               %var3 = "hGrenadeHitDistMax"; %var3Title = "HGrenade Max Dist:"; %var3Name = "Hand Grenade Max Distance"; %var3TypeName = "Meters";
               %i1 = getField($lMapData::data[%map,%var1,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var1,%client.lgame,%client.curMon],0) : %NA; 
               %i2 = getField($lMapData::data[%map,%var2,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var2,%client.lgame,%client.curMon],0) : %NA; 
               %i3 = getField($lMapData::data[%map,%var3,%client.lgame,%client.curMon],0) ? getField($lMapData::name[%map,%var3,%client.lgame,%client.curMon],0) : %NA; 
               %client.statsFieldSet[%vsc1 = %f++] = %var1 TAB %var1Name TAB %var1TypeName;
               %client.statsFieldSet[%vsc2 = %f++] = %var2 TAB %var2Name TAB %var2TypeName;
               %client.statsFieldSet[%vsc3 = %f++] = %var3 TAB %var3Name TAB %var3TypeName;
               %line = '<tab:1,198,395><font:univers condensed:18>\t<a:gamelink\tS\tLBM\t%1\t%2\t%6>%3</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%7>%4</a>\t<a:gamelink\tS\tLBM\t%1\t%2\t%8>%5</a>';
               %nameTitle1 = "<color:0befe7>" @ %var1Title SPC "<color:03d597>" @ %i1;
               %nameTitle2 = "<color:0befe7>" @ %var2Title SPC "<color:03d597>" @ %i2;
               %nameTitle3 = "<color:0befe7>" @ %var3Title SPC "<color:03d597>" @ %i3;
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%nameTitle1,%nameTitle2,%nameTitle3,%vsc1,%vsc2,%vsc3);
            
         }
         
      case "MAPLIST":
         %vLPage = %client.GlArg4;
         %field5 = strreplace(%client.GlArg5,"-","\t");
         %client.lgame = getField(%field5,0);
         %curMon = ($lMapData::mapCount[%client.lgame,$dtStats::curMonth] > 0) ? 1 : 0;
         %lMon = $dtStats::curMonth - 1;
         if(%lMon < 1) %lMon = 12;
         %lastMon = ($lMapData::mapCount[%client.lgame,%lMon] > 0) ? 1 : 0;
      
         if(%curMon + %lastMon  == 2)
            %client.curMon  = getField(%field5,1);
         else if(%curMon)
            %client.curMon = $dtStats::curMonth;
         else if(%lastMon)          
            %client.curMon = %lMon;
            
         if(%vLPage == -1)
            %vLPage = %client.lastMapPage;  
         else
            %client.lastMapPage = %vLPage;

         %perPage = 14;// num of games listed per page
   
        // messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Map List" SPC monthString(%client.curMon)); 
         if(%curMon + %lastMon  == 2 && %client.curMon == $dtStats::curMonth){
            %line = '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> <just:right><a:gamelink\tS\tMAPLIST\t%1\t%2\t%3-%4> [View last month stats]</a>';
            messageClient( %client, 'SetScoreHudSubheader', "", %line,%vClient,1, %client.lgame, %lMon);    
         }
         else if(%curMon + %lastMon  == 2 && %client.curMon == %lMon){
            %line = '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> <just:right><a:gamelink\tS\tMAPLIST\t%1\t%2\t%3-%4> [View current month stats]</a>';
            messageClient( %client, 'SetScoreHudSubheader', "", %line,%vClient,1, %client.lgame, $dtStats::curMonth);    
         }
         else{
            %line = '<a:gamelink\tS\tView\t%1>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> ';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient); 
         }
            %switch = %client.lgame;
            switch$(%switch){
               case "CTFGame" or "SCtFGame":
                  %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18>%2</a>\t%3\t%4\t%5\t%6\t%7';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"Map Name","Score","Offense Score","Defense Score","Flag Time","Flag Grab Speed");
                  %gc = $lMapData::mapCount[%client.lgame,%client.curMon];
                  for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z < %gc; %z++){
                     %map = $lMapData::mapList[%z,%client.lgame,%client.curMon];
                     %v1 = getField($lMapData::name[%map,"scoreTG",%client.lgame,%client.curMon],0); 
                     %v2 = getField($lMapData::name[%map,"offenseScoreTG",%client.lgame,%client.curMon],0);  
                     %v3 = getField($lMapData::name[%map,"defenseScoreTG",%client.lgame,%client.curMon],0);   
                     %v4 = getField($lMapData::name[%map,"heldTimeSecMin",%client.lgame,%client.curMon],0); 
                     %v5 = getField($lMapData::name[%map,"grabSpeedMax",%client.lgame,%client.curMon],0); 
                     %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18><a:gamelink\tS\tMAP\t%1\t%2\t0><clip:110>%2</clip>\t<color:03d597><clip:90>%3</clip>\t<clip:90>%4</clip>\t<clip:90>%5</clip>\t<clip:90>%6</clip>\t<clip:90>%7</clip></a>';
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%v1,%v2,%v3,%v4,%v5);
                  }
               case "DMGame":
                  %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18>%2</a>\t%3\t%4\t%5\t%6\t%7';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"Map Name","Score","Kills","Assists","Kill Streak","Efficiency");
                  %gc = $lMapData::mapCount[%client.lgame,%client.curMon];
                  for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z < %gc; %z++){
                     %map = $lMapData::mapList[%z,%client.lgame,%client.curMon];
                     %v1 = getField($lMapData::name[%map,"scoreTG",%client.lgame,%client.curMon],0);  
                     %v2 = getField($lMapData::name[%map,"killsTG",%client.lgame,%client.curMon],0);  
                     %v3 = getField($lMapData::name[%map,"assistTG",%client.lgame,%client.curMon],0);  
                     %v4 = getField($lMapData::name[%map,"killStreakMax",%client.lgame,%client.curMon],0); 
                     %v5 = getField($lMapData::name[%map,"efficiencyAvg",%client.lgame,%client.curMon],0); 
                     %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18><a:gamelink\tS\tMAP\t%1\t%2\t0><clip:110>%2</clip>\t<color:03d597><clip:90>%3</clip>\t<clip:90>%4</clip>\t<clip:90>%5</clip>\t<clip:90>%6</clip>\t<clip:90>%7</clip></a>';
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%v1,%v2,%v3,%v4,%v5);
                  }
               case "LakRabbitGame":
                  %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18>%2</a>\t%3\t%4\t%5\t%6\t%7';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"Map Name","Score","Kills","Flag Grabs","MidAir Grabs","Flag Time");
                  %gc = $lMapData::mapCount[%client.lgame,%client.curMon];
                  for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z < %gc; %z++){
                     %map = $lMapData::mapList[%z,%client.lgame,%client.curMon];
                     %v1 = getField($lMapData::name[%map,"scoreTG",%client.lgame,%client.curMon],0);  
                     %v2 = getField($lMapData::name[%map,"killsTG",%client.lgame,%client.curMon],0);  
                     %v3 = getField($lMapData::name[%map,"flagGrabsTG",%client.lgame,%client.curMon],0);  
                     %v4 = getField($lMapData::name[%map,"MidairflagGrabsTG",%client.lgame,%client.curMon],0); 
                     %v5 = getField($lMapData::name[%map,"flagTimeMinTG",%client.lgame,%client.curMon],0); 
                     %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18><a:gamelink\tS\tMAP\t%1\t%2\t0><clip:110>%2</clip>\t<color:03d597><clip:90>%3</clip>\t<clip:90>%4</clip>\t<clip:90>%5</clip>\t<clip:90>%6</clip>\t<clip:90>%7</clip></a>';
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%v1,%v2,%v3,%v4,%v5);
                  }
               case "DuelGame":
                  %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18>%2</a>\t%3\t%4\t%5\t%6\t%7';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"Map Name","Score","Kills","Deaths","MidAir Kills","Ground Kills");
                  %gc = $lMapData::mapCount[%client.lgame,%client.curMon];
                  for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z < %gc; %z++){
                     %map = $lMapData::mapList[%z,%client.lgame,%client.curMon];
                     %v1 = getField($lMapData::name[%map,"scoreTG",%client.lgame,%client.curMon],0);  
                     %v2 = getField($lMapData::name[%map,"killsTG",%client.lgame,%client.curMon],0);  
                     %v3 = getField($lMapData::name[%map,"deathsTG",%client.lgame,%client.curMon],0);  
                     %v4 = getField($lMapData::name[%map,"killAirTG",%client.lgame,%client.curMon],0); 
                     %v5 = getField($lMapData::name[%map,"killGroundTG",%client.lgame,%client.curMon],0); 
                     %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18><a:gamelink\tS\tMAP\t%1\t%2\t0><clip:110>%2</clip>\t<color:03d597><clip:90>%3</clip>\t<clip:90>%4</clip>\t<clip:90>%5</clip>\t<clip:90>%6</clip>\t<clip:90>%7</clip></a>';
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%v1,%v2,%v3,%v4,%v5);
                  }
               case "ArenaGame":
                  %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18>%2</a>\t%3\t%4\t%5\t%6\t%7';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,"Map Name","Score","Kills","Assists","Rounds Won","Kill Streak");
                  %gc = $lMapData::mapCount[%client.lgame,%client.curMon];
                  for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z < %gc; %z++){
                     %map = $lMapData::mapList[%z,%client.lgame,%client.curMon];
                     %v1 = getField($lMapData::name[%map,"scoreTG",%client.lgame,%client.curMon],0);  
                     %v2 = getField($lMapData::name[%map,"roundKillsTG",%client.lgame,%client.curMon],0);  
                     %v3 = getField($lMapData::name[%map,"assistTG",%client.lgame,%client.curMon],0);  
                     %v4 = getField($lMapData::name[%map,"roundsWonTG",%client.lgame,%client.curMon],0); 
                     %v5 = getField($lMapData::name[%map,"killStreakMax",%client.lgame,%client.curMon],0); 
                     %line = '<tab:114,204,294,384,474><color:0befe7><font:univers condensed:18><a:gamelink\tS\tMAP\t%1\t%2\t0><clip:110>%2</clip>\t<color:03d597><clip:90>%3</clip>\t<clip:90>%4</clip>\t<clip:90>%5</clip>\t<clip:90>%6</clip>\t<clip:90>%7</clip></a>';
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%map,%v1,%v2,%v3,%v4,%v5);
                  }
            }

            for(%i = %index; %i < %perPage; %i++)
               messageClient( %client, 'SetLineHud', "", %tag, %index++, '');
         if(%curMon || %lastMon){
            if($lMapData::mapCount[%client.lgame,%client.curMon] > %perPage){
               if(%vLPage == 1){
                  %line = '<color:0befe7><just:center><a:gamelink\tS\tMAPLIST\t%1\t%2\t%3-%4> Next Page > </a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%vLPage + 1, %client.lgame, %client.curMon);
               }
               else if(%vLPage * %perPage > $lMapData::mapCount[%client.lgame,%client.curMon]){
                  %line = '<color:0befe7><just:center><a:gamelink\tS\tMAPLIST\t%1\t%2\t%3-%4> < Back Page</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%vLPage - 1, %client.lgame, %client.curMon);
               }
               else if(%vLPage > 1){
                  %line = '<color:0befe7><just:center><a:gamelink\tS\tMAPLIST\t%1\t%2\t%4-%5> < Back Page <a:gamelink\tS\tMAPLIST\t%1\t%3\t%4-%5> Next Page ></a>    <a:gamelink\tS\tMAPLIST\t%1\t1\t%4-%5><First></a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%vLPage - 1,%vLPage + 1, %client.lgame, %client.curMon);
               }
            }            
            %hasCount = 0;  %line = "";
            for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
               if($lMapData::mapCount[$dtStats::gameType[%i], %client.curMon] > 0 && $dtStats::gameType[%i] !$= %client.lgame){
                  %hasCount++; 
                  %line = %line @ "<a:gamelink\tS\tMAPLIST\t" @ %vClient @ "\t" @ 1 @ "\t" @ $dtStats::gameType[%i] @ "-" @ %client.curMon @ ">[" @ $dtStats::gtNameShort[$dtStats::gameType[%i]]  @ "] </a>"; 
               }
            }
            if(%hasCount > 0)
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Switch Game Type" SPC %line);
            //switch months 
         }
         else
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>No data at this time, check back in 24 hours");
      case "VARLIST":
         %vLPage = %client.GlArg4;
         %lType = %client.curLType;
         %pagex = %client.curPage;
         %mon = getField($lData::mon[%lType, %client.lgame, %pagex],0);
         %year = getField($lData::mon[%lType, %client.lgame, %pagex],1);
         %client.backPage = "VARLIST";
         %mon = %client.curMon;
         if(%vLPage == 0){ // back button was hit
            %vLPage = %client.GlArg4 = %client.varListPage; // set it to the last one we were on 
         }
         if(%vLPage $= "" || !%client.varListPage){
            %vLPage = 1;  
         }
         %client.varListPage =  %vLPage; // update with current page
         %perPage = 16;// num of games listed per page 
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Variable List"); 
        // messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tS\tLBOARDS\t%1\t%3>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType);
         if(%vLPage == 1){
            %line = '<a:gamelink\tS\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tS\tVARLIST\t%1\t%2> Next Page ></a>';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%vLPage + 1, %lType, %pagex,%client.lgame);
         }
         else if(%vLPage * %perPage > $statsVars::count[%client.lgame]){
            %line = '<a:gamelink\tS\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tS\tVARLIST\t%1\t%2> < Back Page</a>    <a:gamelink\tS\tVARLIST\t%1\t1><Reset></a>';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%vLPage - 1, %lType, %pagex,%client.lgame);
         }
         else if(%vLPage > 1){
            %line = '<a:gamelink\tS\tLBOARDS\t%1\t%4-%6\t%5>  Back</a>  -  <a:gamelink\tS\tReset\t%1>Return To Score Screen</a> -<a:gamelink\tS\tVARLIST\t%1\t%2\> < Back Page </a>|<a:gamelink\tS\tVARLIST\t%1\t%3> Next Page ></a>    <a:gamelink\tS\tVARLIST\t%1\t1><Reset></a>';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%vLPage - 1,%vLPage + 1, %lType, %pagex,%client.lgame);
         }
         %line = '<color:0befe7><lmargin:50>Variable Name<lmargin:250>Player Name';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
         %gc = $statsVars::count[%client.lgame];
         for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z <= %gc; %z++){
            %var = $statsVars::varNameType[%z,%client.lgame];
            %cat = $statsVars::varType[%var,%client.lgame];
            if(%cat !$= "Game"){// not sorted 
               %name = getField($lData::name[%var,%client.lgame,%lType,%mon,%year],0);  
               %client.statsFieldSet[%f++] = %var TAB %var TAB "Value";
               %line = '<color:0befe7><lmargin:50><a:gamelink\tS\tLB\t%1\t0\t%4>%2</a><lmargin:250><color:03d597>%3';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%var,%name,%f);
            }
            else{
               %line = '<color:0befe7><lmargin:50>%2<lmargin:250><color:03d597>%3';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%var,"NA");
            }
         }
      default://fail safe / reset
         %client.viewMenu   = 0;
         %client.viewClient = 0;
         %client.viewStats  = 0;
         %client.lastPage   = 1;
         %client.lgame = %game;
   }
}

////////////////////////////////////////////////////////////////////////////////
// LeaderBoards
////////////////////////////////////////////////////////////////////////////////
function lStatsCycle(%build){ // starts and manages the build/sort cycle
  if($dtStats::debugEchos){error("lStatsCycle" SPC $dtStats::build["day"] SPC $dtStats::week && !$dtStats::build["week"] SPC
  $dtStats::build["month"] SPC $dtStats::build["quarter"] SPC $dtStats::build["year"] SPC $dtStats::lCount);} 
   if(%build){//reset
      if(!$dtStats::statsSave){// make sure we are not inbetween missions and saveing 
         $dtStats::build["day"] = 0;
         $dtStats::build["week"] = 0;
         $dtStats::build["month"] = 0;
         $dtStats::build["quarter"] = 0;
         $dtStats::build["year"] = 0;
         $dtStats::lCount = 0;
         $dtStats::building = 1;
         $dtStats::hostTimeLimit = $Host::TimeLimit;
         if(isGameRun()){//if for some reason the game is running extend the time limit untill done
            Game.voteChangeTimeLimit(1,$Host::TimeLimit+60);
            messageAll( 'MsgStats', '\c3Stats build started, adjusting time limit temporarily');

         }
      }
      else{
         schedule(1000,0,"lStatsCycle",1);  
         return;
      }
   }
   if($dtStats::day > 0 && !$dtStats::build["day"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["day"] = 1; // mark as done 
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"day");
   }
   else if($dtStats::week > 0 && !$dtStats::build["week"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["week"] = 1; // mark as done 
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"week");
   }
   else if($dtStats::month > 0  && !$dtStats::build["month"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["month"] = 1; // mark as done 
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"month");
   }
   else if($dtStats::quarter > 0 && !$dtStats::build["quarter"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["quarter"] = 1; // mark as done 
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"quarter");
   }
   else if($dtStats::year > 0 && !$dtStats::build["year"]){
      %game = $dtStats::gameType[$dtStats::lCount];
      if($dtStats::lCount++ >= $dtStats::gameTypeCount){
         $dtStats::build["year"] = 1; // mark as done 
         $dtStats::lCount = 0; // reset
      }
      preLoadStats(%game,"year");
   }
   else{
      if($dtStats::debugEchos){error("leaderBoards finished building");} 
      //schedule(5000,0,"loadLeaderboards",1);// reset and reload leaderboards 
      mapStatsCycle(1);
   }
}
// only load one gameType/leaderboard at at time to reduce memory allocation 
function preLoadStats(%game,%lType){ //queue up files for processing
  if($dtStats::debugEchos){error("preLoadStats queuing up files for" SPC %game SPC %lType);}
   %folderPath = "serverStats/stats/" @ %game @ "/*t.cs";
   %count = getFileCount(%folderPath);
   if(!%count){
      lStatsCycle(0);
   }
   if(!isObject(serverStats)){new SimGroup(serverStats);RootGroup.add(serverStats);}
   else{serverStats.delete(); new SimGroup(serverStats);RootGroup.add(serverStats);}
   for (%i = 0; %i < %count; %i++){
      %file = findNextfile(%folderPath);
      schedule(%i * 32, 0,"loadStatsData",%file,%game,%lType,%i,%count);
   }
}
function markNewDay(){// updates are dates when the server is ready to cycle over to a new day
   $dtStats::curDay = getDayNum();
   $dtStats::curWeek = getWeekNum();
   $dtStats::curMonth = getMonthNum();
   $dtStats::curQuarter = getQuarterNum();
   $dtStats::curYear = getYear();
   if($dtStats::debugEchos){error("MarkNewDay =" SPC $dtStats::curDay SPC $dtStats::curWeek SPC $dtStats::curMonth SPC $dtStats::curQuarter SPC $dtStats::curYear);} 
}
// var old new old  new  old   new    old     new     old  new  
// var day day week week month month  quarter quarter year year  
// 0    1   2   3    4    5     6      7       8       9    10
function loadStatsData(%filepath,%game,%lType,%fileNum,%total){
   if($dtStats::debugEchos){error("loadStatsData" SPC %filePath SPC %fileNum SPC %total);}
   switch$(%lType){
      case "day":    %mon = $dtStats::curDay;     %fieldOld = 1;  %fieldNew = 2; 
      case "week":   %mon = $dtStats::curWeek;    %fieldOld = 3;  %fieldNew = 4; 
      case "month":  %mon = $dtStats::curMonth;   %fieldOld = 5;  %fieldNew = 6; 
      case "quarter":%mon = $dtStats::curQuarter; %fieldOld = 7;  %fieldNew = 8; 
      case "year":   %mon = $dtStats::curYear;    %fieldOld = 9;  %fieldNew = 10; 
      default:       %mon = getMonthNum();   %fieldOld = 5;  %fieldNew = 6; 
   }
   %file = new FileObject();
   RootGroup.add(%file);
   %file.OpenForRead(%filepath);
   %day = strreplace(%file.readline(),"%t","\t");
   if(getFieldCount(%day) >= 9) {
      %guid = getField(strreplace(getField(strreplace(%filepath,"/","\t"),3),"t","\t"),0);
      %gameCount = strreplace(%file.readline(),"%t","\t");
      %name = getField(strreplace(%file.readline(),"%t","\t"),1);
      %monOld = getField(%day,%fieldOld);
      %monNew = getField(%day,%fieldNew);// should allways be this one 
      %found = -1;
      if(%monNew == %mon){%found = %fieldNew;}
      else if(%monold == %mon){%found = %fieldOld;}
      if(%found > -1){
         %obj = new scriptObject();
         serverStats.add(%obj);
         %obj.name = %name;
         %obj.gameCount = getField(%gameCount,%found);
         %obj.guid =  %guid;
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var  = getField(%line,0);
            %obj.LStats[%var,%game] = getField(%line,%found);
         }     
      }
   }
   %file.close();
   %file.delete();
   if(%fileNum >= %total-1){
      if(serverStats.getCount()){// make sure we have data to sort 
         sortLStats(0,%game,%lType); 
      }
      else{
         if($dtStats::debugEchos){error("No Valid Data For" SPC %lType SPC %mon);}
         lStatsCycle(0);
      }
   }
}

function  sortLStats(%c,%game,%lType){
   if($dtStats::debugEchos){error("sortLStats" SPC %c SPC %game SPC %lType);}
   %var = $statsVars::varNameType[%c,%game];
   %cat = $statsVars::varType[%var,%game];
   if(%cat !$= "Game"){
      %sortCount = 0;
      if(!isObject(LFData)){
         switch$(%lType){
            case "day":    %mon = $dtStats::curDay;
            case "week":   %mon = $dtStats::curWeek;
            case "month":  %mon = $dtStats::curMonth;
            case "quarter":%mon = $dtStats::curQuarter;
            case "year":   %mon = $dtStats::curYear;
            default:       error("ltype is not set"); return;
         }
         //%fc = getFileCount("serverStats/LData/-CTFGame*.cs");
         new FileObject(LFData);
         RootGroup.add(LFData);
         LFData.openForWrite("serverStats/lData/" @ "-" @ %game @ "-" @ %mon @ "-" @ $dtStats::curYear @ "-" @ %lType @"-.cs"); 
      }

      %n = %var @ "%tname";// name list
      %s = %var @ "%tdata"; // data list 
      %g = %var @ "%tguid"; // data list 
      %statsCount = serverStats.getCount();
      if(%cat $= "AvgI" || %cat $= "Min"){
         %invCount = 0;
         for (%i = 0; %i < %statsCount; %i++){//selection sort 
            %maxCount = %i;  
            for (%j = %i+1; %j < %statsCount; %j++){
               if(%cat $= "AvgI"){
                  if (getField(strreplace(serverStats.getObject(%j).LStats[%var,%game],"%a","\t"),0) < getField(strreplace(serverStats.getObject(%maxCount).LStats[%var,%game],"%a","\t"),0))
                     %maxCount = %j;
               }
               else{
                  if (serverStats.getObject(%j).LStats[%var,%game] < serverStats.getObject(%maxCount).LStats[%var,%game])
                     %maxCount = %j; 
               }
            }
            %obj = serverStats.getObject(%maxCount);
            serverStats.bringToFront(%obj);// push the ones we have sorted to the front so we dont pass over them again 
            if(%cat $= "AvgI")
               %num = getField(strreplace(%obj.LStats[%var,%game],"%a","\t"),0);
            else
               %num = %obj.LStats[%var,%game];
            if(%num != 0){
               %invCount++;
               %n = %n @ "%t" @ %obj.name; 
               %s = %s @ "%t" @ %num;
               %g = %g @ "%t" @ %obj.guid;
            }
            if(%invCount >= $dtStats::topAmount){ 
                break;  
            }
         }
         if(!%invCount){
            %n = %n @ "%t" @ "NA"; 
            %s = %s @ "%t" @ 0;
            %g = %g @ "%t" @ 0;
         }
      }
      else{
         %invCount = 0;
         for (%i = 0; %i < %statsCount && %i < $dtStats::topAmount; %i++){//selection sort 
            %maxCount = %i;  
            for (%j = %i+1; %j < %statsCount; %j++){
               if(%cat $= "Avg"){
                  if (getField(strreplace(serverStats.getObject(%j).LStats[%var,%game],"%a","\t"),0) > getField(strreplace(serverStats.getObject(%maxCount).LStats[%var,%game],"%a","\t"),0))
                     %maxCount = %j;
               }
               else{
                  if (xlCompare(serverStats.getObject(%j).LStats[%var,%game] , serverStats.getObject(%maxCount).LStats[%var,%game]) $= ">")
                     %maxCount = %j;  
               }
            }
            %obj = serverStats.getObject(%maxCount);
            serverStats.bringToFront(%obj);// push the ones we have sorted to the front so we dont pass over them again 
            if(%cat $= "Avg")
               %num = getField(strreplace(%obj.LStats[%var,%game],"%a","\t"),0);
            else
               %num = %obj.LStats[%var,%game];
               
            if(%num != 0){
               %invCount++;
               %n = %n @ "%t" @ %obj.name; 
               %s = %s @ "%t" @ %num;
               %g = %g @ "%t" @ %obj.guid;
            }
         }
         if(!%invCount){
            %n = %n @ "%t" @ "NA"; 
            %s = %s @ "%t" @ 0;
            %g = %g @ "%t" @ 0;
         } 
      }
      LFData.writeLine(%n); 
      LFData.writeLine(%s);
      LFData.writeLine(%g);
   }
   
   if(%c++ < $statsVars::count[%game])
      schedule($dtStats::sortSpeed,0,"sortLStats",%c,%game,%lType);
   else{
      LFData.close();
      LFData.delete();
      lStatsCycle(0); // kick off the next one 
   }
}
function loadMapLeaderBoards(%reset){
   if(!$dtStats::mapStats)
      return;
   
   if($dtStats::debugEchos){error("loadMapLeaderBoards reset =" SPC %reset);}  
   
   if(%reset){deleteVariables("$lMapData::*");} 
   if(!$lMapData::load){$lMapData::load = 1;}
   else{return;}// exit  if we have all ready loaded  
   
   %file = new FileObject(); 
   RootGroup.add(%file);
   %folderPath = "serverStats/mlData/*.cs";
   %count = getFileCount(%folderPath);
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%folderPath);
      %game  = getField(strreplace(%filePath,"/","\t"),2);
      %fieldPath =strreplace(%filePath,"-","\t");
      %mon = getField(%fieldPath,1);
      %year = getField(%fieldPath,2);
      %map = getField(%fieldPath,3);
      
      if(!isFileExpired("mapData",%mon,%year)){// do to the amount of data we only load 2 months worth 
         %file.OpenForRead(%filepath);
         %break = 0;  
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var  = getField(%line,0);
            %stack  = getField(%line,1);
            if(%stack $= "name"){ 
               %name = getFields(%line,2,getFieldCount(%line)-1);
               if(%var $= "scoreTG"){// check first score to see if its worth loading 
                  if(getField(%name,0) $= "NA"){
                     %break = 1;
                     break;
                  }
               }
               $lMapData::name[%map,%var,%game,%mon] = %name; 
            }
            else if(%stack $= "data"){
               %data = getFields(%line,2,getFieldCount(%line)-1);
               $lMapData::data[%map,%var,%game,%mon] = %data; 
            }  
            else if(%stack $= "guid"){
               %guid = getFields(%line,2,getFieldCount(%line)-1);
               $lMapData::guid[%map,%var,%game,%mon] = %guid; 
            }
         }
         if(!%break){
            %c = $lMapData::mapCount[%game,%mon]++;
            $lMapData::mapList[%c-1,%game,%mon] = %map;  
         }
         %file.close();
      }
      else{
         if($dtStats::lsmMap){
            if($dtStats::debugEchos){error("Deleting old file" SPC %filepath);}
            schedule((%i+1)  * 1000,0,"deleteFile",%filepath);  
         }
      }
   }
   %file.close();
   %file.delete();
}schedule(2000,0,"loadMapLeaderBoards",0);

function loadLeaderboards(%reset){ // loads up leaderboards
   if($dtStats::debugEchos){error("loadLeaderboards reset =" SPC %reset);}  
   if(%reset){deleteVariables("$lData::*");} 
   if(!$lData::load){$lData::load = 1;}
   else{return;}// exit  if we have all ready loaded 
   markNewDay();//called when server starts and when build completes
   dtCleanUp(0);
   if(!isEventPending($dtStats::buildEvent))
      $dtStats::buildEvent = schedule(getTimeDif($dtStats::buildSetTime),0,"lStatsCycle",1);
   $dtStats::building = 0;
   if(isFile("serverStats/saveVars.cs"))
      exec("serverStats/saveVars.cs");
   %oldFileCount = 0; 
   %file = new FileObject(); 
   RootGroup.add(%file);
   %folderPath = "serverStats/LData/*.cs";
   %count = getFileCount(%folderPath);
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%folderPath);
      %fieldPath =strreplace(%filePath,"-","\t");
      %game = getField(%fieldPath,1);
      %mon = getField(%fieldPath,2); // 0 path / 1  game / 2 mon / 3 year / 4 type / 5 .cs
      %year = getField(%fieldPath,3); 
      %lType = getField(%fieldPath,4);
      //echo(isFileExpired(%lType,%mon,%year) SPC %lType SPC %mon SPC %year);
      if(!isFileExpired(%lType,%mon,%year)){   
         $lData::mon[%lType, %game, $lData::monCount[%game,%lType]++] = %mon TAB %year;
         if(!$lData::hasData[%lType,%game]){
           %sortArray[%sortCount++] = %lType TAB %game;   
         }
         $lData::hasData[%lType,%game] = 1;
         %file.OpenForRead(%filepath);  
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var  = getField(%line,0);
            %stack  = getField(%line,1);
            if(%stack $= "name"){
               %name = getFields(%line,2,getFieldCount(%line)-1);
               $lData::name[%var,%game,%lType,%mon,%year] = %name; 
            
            }
            else if(%stack $= "data"){
               %data = getFields(%line,2,getFieldCount(%line)-1);
               $lData::data[%var,%game,%lType,%mon,%year] = %data; 
            } 
            else if(%stack $= "guid"){
               %guid = getFields(%line,2,getFieldCount(%line)-1);
               $lData::guid[%var,%game,%lType,%mon,%year] = %guid; 
            }
         }
         %file.close();
      }
      else{// not valid any more delete;
         if($dtStats::lsm){
            if($dtStats::debugEchos){error("Deleting old file" SPC %filepath);}
            schedule((%i+1)  * 1000,0,"deleteFile",%filepath);  
         }
         else{
             %oldFileCount++;
         }
      }
   }
   %file.close();
   %file.delete();
   error("Found" SPC %oldFileCount SPC "Expired Leaderboard Files");
   if(%sortCount > 1){// sorts what the data we loaded by date as windows vs linux will return diffrent file orders  
      for(%i = 1; %i <= %sortCount; %i++){
         sortMon(getField(%sortArray[%i],0),getField(%sortArray[%i],1));
      }
   }
}schedule(5000,0,"loadLeaderboards",0);// delay this so supporting functions are exec first

function sortMon(%lType, %game){   
   %n = $lData::monCount[%game,%lType];
   if(%n > 1){//make sure we have enough elments worth sorting 
      for (%i = 1; %i <= %n-1; %i++){//sort by %ltype first   
         %m = %i;  
         for (%j = %i+1; %j <= %n; %j++)  
            if (getField($lData::mon[%lType, %game,%j],0) > getField($lData::mon[%lType, %game, %m],0))  
               %m = %j;  
               
            %low = $lData::mon[%lType, %game, %m];
            %high = $lData::mon[%lType, %game, %i]; 
            $lData::mon[%lType, %game, %m] = %high;
            $lData::mon[%lType, %game, %i] = %low;
      } 
      for (%i = 1; %i <= %n-1; %i++){// sort by year  
         %m = %i;  
         for (%j = %i+1; %j <= %n; %j++)  
            if (getField($lData::mon[%lType, %game, %j],1) > getField($lData::mon[%lType, %game, %m],1))  
               %m = %j;  
               
            %low = $lData::mon[%lType, %game, %m];
            %high = $lData::mon[%lType, %game, %i]; 
            $lData::mon[%lType, %game, %m] = %high;
            $lData::mon[%lType, %game, %i] = %low;
      } 
   }
   //debug
   //for (%i = 1; %i <= %n; %i++){echo($lData::mon[%lType, %game,%i] SPC %game);}
} 

function dtCleanUp(%force){
   %filename = "serverStats/stats/*t.cs";
   %count = getFileCount(%filename);
   %file = new FileObject();
   RootGroup.add(%file);
   %oldFileCount = 0;
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%filename);
      %file.OpenForRead(%filepath);  
      %game  = getField(strreplace(%filePath,"/","\t"),2);
      %dateLine = strreplace(%file.readline(),"%t","\t");
      %gameCountLine = strreplace(%file.readline(),"%t","\t");
      %day = getField(%dateLine,2);
      %year = getField(%dateLine,10);
      %file.close();
      //%d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1;
      %dayCount = isFileExpired("getCount",%day,%year);
      if(%dayCount > $dtStats::expireMin){ 
         %gcCM = getField(%gameCountLine,6);  
         %gcPM = getField(%gameCountLine,5);
         %gc =  (%gcCM > %gcPM) ? %gcCM : %gcPM;
         %extraDays = mCeil((%gc * $dtStats::expireFactor[%game]) + $dtStats::expireMin);
         //error(%extraDays SPC %dayCount);
         if(%dayCount > %extraDays || %dayCount > $dtStats::expireMax){
            if($dtStats::sm || %force){
               if($dtStats::debugEchos){error("Deleting old file" SPC %dayCount SPC %extraDays SPC %filepath);}
               if(isFile(%filepath)){
                  schedule(%v++ * 500,0,"deleteFile",%filepath);
                  %oldFileCount++;
               }
               %gPath = strreplace(%filepath,"t.cs","g.cs");
               %mPath = strreplace(%filepath,"t.cs","m.cs");
               if(isFile(%mPath)){
                  schedule(%v++ * 500,0,"deleteFile",%mPath);
                  %oldFileCount++;
               }
               if(isFile(%gPath)){
                  schedule(%v++ * 500,0,"deleteFile",%gPath);
                  %oldFileCount++;
               }
            }
            else{
               %oldFileCount++;
            }    
         }
      }
   }
   if($dtStats::sm || %force){
      error("Found" SPC %oldFileCount SPC "Expired Player Files");   
   }
   else{
      error("Found" SPC %oldFileCount SPC "Expired Player Files, Type dtCleanUp(1) to force clean and delete");
   }
   %file.delete();
}

function isFileExpired(%lType,%d,%year){
   switch$(%lType){
      case "expire":
         if($dtStats::expireMax > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            if(%days > $dtStats::expireMax){
               return 1;  
            }
            else{
               return 0;  
            }
         }
         else{
            return 1;  
         }
      case "getCount":
         if($dtStats::expireMax > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            return %days;
         }
         else{
            return -1;  
         }
      case "mapData":
         %dif = $dtStats::curYear - %year;
         %days += 12 * (%dif-1);
         %days += 13 - %d;
         %days += $dtStats::curMonth;
         //error(%days);
         if(%days > 2){
            return 1;  
         }
         else{
            return 0;  
         }
      case "day": 
         if($dtStats::day > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            if(%days > $dtStats::day){
               return 1;  
            }
            else{
               return 0;  
            }
         }
         else{
            return 1;  
         }
      case "week": 
         if($dtStats::week > 1){
            %dif = $dtStats::curYear - %year;
            %days += 53 * (%dif-1);
            %days += 54 - %d;
            %days += $dtStats::curWeek;
               if(%days > $dtStats::week){
                  return 1;  
               }
               else{
                  return 0;  
               }
            }
            else{
               return 1;  
            }
      case "month":
         if($dtStats::month > 1){
            %dif = $dtStats::curYear - %year;
            %days += 12 * (%dif-1);
            %days += 13 - %d;
            %days += $dtStats::curMonth;
            //error(%days);
            if(%days > $dtStats::month){
               return 1;  
            }
            else{
               return 0;  
            }
         }
         else{
            return 1;  
         } 
      case "quarter":
         if($dtStats::quarter > 1){
            %dif = $dtStats::curYear - %year;
            %days += 4 * (%dif-1);
            %days += 5 - %d;
            %days += $dtStats::curQuarter;
            if(%days > $dtStats::quarter){
               return 1;  
            }
            else{
               return 0;  
            }
         }
         else{
            return 1;  
         } 
      case "year": 
         %mon = $dtStats::curYear - %val;
         if(%mon <= $dtStats::year){
            return 0;  
         }
   }
   return 1; 
}
////////////////////////////////////////////////////////////////////////////////
// Map Stats
////////////////////////////////////////////////////////////////////////////////
function mapStatsCycle(%build){ // starts and manages the build/sort cycle
  if($dtStats::debugEchos){error("mapStatsCycle" SPC $dtStats::build["day"] SPC $dtStats::week && !$dtStats::build["week"] SPC
  $dtStats::build["month"] SPC $dtStats::build["quarter"] SPC $dtStats::build["year"] SPC $dtStats::lCount);} 
   if(%build){//reset
      $dtStats::mapBuild["month"] = 0;
      $dtStats::mapLCount = 0; 
   }
   if($dtStats::month > 0  && !$dtStats::mapBuild["month"]){
      %game = $dtStats::gameType[$dtStats::mapLCount];
      if($dtStats::mapLCount++ >= $dtStats::gameTypeCount){
         $dtStats::mapBuild["month"] = 1; // mark as done 
         $dtStats::mapLCount = 0; // reset
      }
      preLoadMapStats(%game,"month");
   }
   else{
      if($dtStats::debugEchos){error("map leader Boards finished building");} 
      schedule(1000,0,"loadMapLeaderBoards",1);
      schedule(5000,0,"loadLeaderboards",1);// reset and reload leaderboards 
      $dtServerVars::lastBuildTime = formattimestring("hh:nn:a mm-dd-yy");
      dtSaveServerVars();
      if(isObject(Game)){
         Game.voteChangeTimeLimit(1,$dtStats::hostTimeLimit);//put back to normal
         messageAll( 'MsgStats', '\c3Stats build complete, reverting time back to normal');
      }
         
   }
}

function preLoadMapStats(%game,%lType){ //queue up files for processing
  if($dtStats::debugEchos){error("preLoadMapStats queuing up files for" SPC %game SPC %lType);}
   %folderPath = "serverStats/stats/" @ %game @ "/*m.cs";
   %count = getFileCount(%folderPath);
   if(!%count){
      mapStatsCycle(0);
   }   
   if(!isObject(serverMapStats)){new SimGroup(serverMapStats);RootGroup.add(serverMapStats);}
   else{serverMapStats.delete(); new SimGroup(serverMapStats);RootGroup.add(serverMapStats);}
   for (%i = 0; %i < %count; %i++){
      %file = findNextfile(%folderPath);
      schedule(%i * 32, 0,"scanGameData",%file,%game,%lType,%i,%count);
   }
}

function scanGameData(%filepath,%game,%lType,%i,%count){
   if($dtStats::debugEchos){error("scanGameData" SPC %filePath SPC %fileNum SPC %total);}
   %file = new FileObject();
   RootGroup.add(%file);
   %file.OpenForRead(%filepath);
   //header stuff junk
   %date = getField(strreplace(%file.readline(),"%t","\t"),2);
   %name = getField(strreplace(%file.readline(),"%t","\t"),1);
   %var =  strreplace(%file.readline(),"%t","\t");
   
   %guid = getField(strreplace(getField(strreplace(%filepath,"/","\t"),3),"m","\t"),0);
   
   %mon = $dtStats::curMonth;  
   //------------------------------------------------------------------------------
   if(%mon == %date){// if we have valid games 
      %obj = new scriptObject(); // make an object to store it in 
      serverMapStats.add(%obj);
      %obj.varList = %var;
      %obj.guid = %guid;
      %obj.name = %name;
      while( !%file.isEOF() ){// load the rest of the file 
         %line = strreplace(%file.readline(),"%t","\t");
         %mapNameID  = getField(%line,0);
         %obj.mapStats[%mapNameID] = %line; // dump stats into temp var
      }
   }
      %file.close();// done with file lets close and delete 
      %file.delete();   


   if(%i >= %count-1){
      if(serverMapStats.getCount())// make sure we have data to sort 
         sortMapStats(1,%game,%lType,1); 
      else
        mapStatsCycle(0);
   }
}
function  sortMapStats(%varIndex,%game,%lType,%mapIndex){
   %map =  $mapID::IDNameGame[%mapIndex,%game];
   %mid = getMapID(%map,%game,0,0);
   %gid = getMapID(%map,%game,1,0);
   %mapNameID = %map @ "-" @ %mid @ "-" @ %gid;  
   if($dtStats::debugEchos){error("sortMmapStats" SPC %varIndex SPC %game SPC %lType SPC %mapNameID);}
   %var = $mapStats::mapVars[%varIndex,%game];
   %cat = $statsVars::varType[%var,%game];
   %sortCount = 0;
   if(!isObject(LMFData)){
      %mon = $dtStats::curMonth;
      new FileObject(LMFData);
      RootGroup.add(LMFData);
      LMFData.openForWrite("serverStats/mlData/" @ %game @ "/" @ "-" @ %mon @ "-" @ $dtStats::curYear @ "-" @ %map @"-.cs"); 
   }
   if(%var !$= ""){// make sure its not a skip
      %n = %var @ "%tname";// name list
      %s = %var @ "%tdata"; // data list 
      %g = %var @ "%tguid"; // data list 
      %statsCount = serverMapStats.getCount();
      if(%cat $= "AvgI" || %cat $= "Min"){
         %invCount = 0;
         for (%i = 0; %i < %statsCount; %i++){//selection sort 
            %maxCount = %i;  
            for (%j = %i+1; %j < %statsCount; %j++){
               %obj1 = serverMapStats.getObject(%j); %obj2 = serverMapStats.getObject(%maxCount);
               if(getField(%obj1.varList,%varIndex) $= getField(%obj2.varList,%varIndex)){//make sure the var matches up in case of change
                  if(%cat $= "AvgI"){
                     if (getField(strreplace(getField(%obj1.mapStats[%mapNameID],%varIndex),"%a","\t"),0) < getField(strreplace(getField(%obj2.mapStats[%mapNameID],%varIndex),"%a","\t"),0))
                        %maxCount = %j;
                  }
                  else{
                     if (getField(%obj1.mapStats[%mapNameID],%varIndex) < getField(%obj2.mapStats[%mapNameID],%varIndex))
                        %maxCount = %j; 
                  }
               }
            }
            %objMax = serverMapStats.getObject(%maxCount);
            serverMapStats.bringToFront(%objMax);// push the ones we have sorted to the front so we dont pass over them again 
            if(%cat $= "AvgI")
               %num = getField(strreplace(getField(%objMax.mapStats[%mapNameID],%varIndex),"%a","\t"),0);
            else
               %num = getField(%objMax.mapStats[%mapNameID],%varIndex);
            if(%num != 0){
               %invCount++;
               %n = %n @ "%t" @ %objMax.name; 
               %s = %s @ "%t" @ %num;
               %g = %g @ "%t" @ %objMax.guid;
            }
            if(%invCount >= $dtStats::topAmount){ 
                break;  
            }
         }
         if(!%invCount){
            %n = %n @ "%t" @ "NA"; 
            %s = %s @ "%t" @ 0;
            %g = %g @ "%t" @ 0;
         }
      }
      else{
         %invCount = 0;
         for (%i = 0; %i < %statsCount && %i < $dtStats::topAmount; %i++){//selection sort 
            %maxCount = %i;  
            for (%j = %i+1; %j < %statsCount; %j++){
               %obj1 = serverMapStats.getObject(%j); %obj2 = serverMapStats.getObject(%maxCount);
               if(getField(%obj1.varList,%varIndex) $= getField(%obj2.varList,%varIndex)){
                  if(%cat $= "Avg"){
                     if (getField(strreplace(getField(%obj1.mapStats[%mapNameID],%varIndex),"%a","\t"),0) > getField(strreplace(getField(%obj2.mapStats[%mapNameID],%varIndex),"%a","\t"),0))
                        %maxCount = %j;
                  }
                  else{
                     if (xlCompare(getField(%obj1.mapStats[%mapNameID],%varIndex) , getField(%obj2.mapStats[%mapNameID],%varIndex)) $= ">")
                        %maxCount = %j;  
                  }
               }
            }
            %objMax = serverMapStats.getObject(%maxCount);
            serverMapStats.bringToFront(%objMax);// push the ones we have sorted to the front so we dont pass over them again 
            if(%cat $= "Avg")
               %num = getField(strreplace(getField(%objMax.mapStats[%mapNameID],%varIndex),"%a","\t"),0);
            else
               %num = getField(%objMax.mapStats[%mapNameID],%varIndex);
               
            if(%num != 0){
               %invCount++;
               %n = %n @ "%t" @ %objMax.name; 
               %s = %s @ "%t" @ %num;
               %g = %g @ "%t" @ %objMax.guid;
            }
         }
         if(!%invCount){
            %n = %n @ "%t" @ "NA"; 
            %s = %s @ "%t" @ 0;
            %g = %g @ "%t" @ 0;
         } 
      }
      LMFData.writeLine(%n); 
      LMFData.writeLine(%s);
      LMFData.writeLine(%g);
   }
   
   if(%varIndex++ <= $mapStats::mapVarCount[%game])
      schedule($dtStats::sortSpeed,0,"sortmapStats",%varIndex,%game,%lType,%mapIndex);
   else if(%mapIndex++ <= $mapID::countGame[%game]){
      LMFData.close();
      LMFData.delete();
      schedule($dtStats::sortSpeed,0,"sortmapStats",1,%game,%lType,%mapIndex);
   }
   else{
      LMFData.close();
      LMFData.delete();
      mapStatsCycle(0); // kick off the next one 
   }
}

////////////////////////////////////////////////////////////////////////////////
//Server Stats
////////////////////////////////////////////////////////////////////////////////

function dtSaveServerVars(){
   $dtServerVars::lastSimTime = getSimTime();
   $dtServerVars::lastDate = formattimestring("mm/dd/yy hh:nn:a");
   $dtServerVars::lastMission = cleanMapName($CurrentMission);
   schedule(1,0,"export", "$dtServerVars::*", "serverStats/serverVars.cs", false );
   schedule(1000,0,"export", "$dtServer::serverHang*", "serverStats/serverHangs.cs", false );
   schedule(2000,0,"export", "$dtServer::hostHang*", "serverStats/hostHangs.cs", false );
   schedule(3000,0,"export", "$dtServer::playCount*", "serverStats/playCount.cs", false );
   schedule(3000,0,"export", "$dtServer::lastPlay*", "serverStats/lastPlay.cs", false );
   schedule(4000,0,"export", "$dtServer::mapDisconnects*", "serverStats/mapDisconnects.cs", false );
   schedule(5000,0,"export", "$dtServer::mapReconnects*", "serverStats/mapReconnects.cs", false );
   schedule(6000,0,"export", "$dtServer::voteFor*", "serverStats/voteFor.cs", false );
   schedule(7000,0,"export", "$dtServer::skipCount*", "serverStats/skipCount.cs", false );
   schedule(8000,0,"export", "$dtServer::maxPlayers*", "serverStats/maxPlayers.cs", false );
   schedule(9000,0,"export", "$dtServer::clientCrash*", "serverStats/clientCrash.cs", false );
}
$dtStats::prefTestTime = 512;// the lower the better tracking 
$dtStats::prefTestIdleTime = 10*1000;// if no one is playing just run slow
$dtStats::prefTolerance = 128;//this number is to account for base line preformance and differences between engine simTime and realtime  
$dtStats::prefLog = 0; // enable logging of server hangs
function prefTest(%time,%skip){
   %real  = getRealTime();
   %plCount = $HostGamePlayerCount - $HostGameBotCount;
   if(isGameRun() && !$dtStats::building && %plCount > 0){// only track during run time  
      %dif = (%real - %time) - $dtStats::prefTestTime; 
      //echo(%dif);
      if(%dif > $dtStats::prefTolerance && !%skip){ 
         %msg = "Server Hang Event" SPC formattimestring("hh:nn:a mm-dd-yy") SPC %dif SPC "ms";
         if($dtStats::debugEchos){error(%msg);}
         $dtServer::serverHangTotal++;
         $dtServer::serverHangMap[cleanMapName($CurrentMission),Game.class]++;
         $dtServer::serverHangLast = formattimestring("hh:nn:a mm-dd-yy");
         $dtServer::serverHangTime = %dif;
         LogPrefIssue(%msg SPC "Map:" SPC $CurrentMission SPC Game.class SPC "UpTime:" SPC formatTime(getSimTime()));
         %skip = 1;  
      }
      else
         %skip = 0; 
      dtPingAvg();
   }

   if($dtStats::prefEnable){
      if(isGameRun() && %plCount > 0)
         schedule($dtStats::prefTestTime, 0, "prefTest",%real,%skip);
      else 
         schedule($dtStats::prefTestIdleTime, 0, "prefTest",%real,1);
   }
}
function dtBuildEvent(){
   if(getTimeDif($dtStats::buildSetTime) < 900000){
      if(!isEventPending($dtStats::buildEvent))
         $dtStats::buildEvent = schedule(getTimeDif($dtStats::buildSetTime),0,"lStatsCycle",1);
   }  
}
function dtPingAvg(){
   %ping = %pc = 0;
   for(%i = 0; %i < ClientGroup.getCount(); %i++){
      %cl = ClientGroup.getObject(%i);
      if(!%cl.isAIControlled()){
         %ping = %cl.getPing();  
         if(%ping > 500)
            %cl.dtStats.lagSpikes++;
         %pc++;
         %pingT += %ping;  
      }
   }
   if(%pc > 0){
      $dtStats::pingAvg = %pingT / %pc;
   }
   if(%pc > 4){ 
      if($dtStats::pingAvg > 1000){//network issues 
         %msg = "Host Hang Event" SPC formattimestring("hh:nn:a mm-dd-yy") SPC $dtStats::pingAvg SPC "ms";
         if($dtStats::debugEchos){error(%msg);}
         $dtServer::hostHangMap[cleanMapName($CurrentMission),Game.class]++;
         $dtServer::hostHangTotal++;
         $dtServer::hostHangLast = formattimestring("hh:nn:a mm-dd-yy");
         $dtServer::hostHangTime = %pingT / %pc;
         LogPrefIssue(%msg SPC "Map:" SPC $CurrentMission SPC Game.class SPC "UpTime:" SPC formatTime(getSimTime()));
      }
   }
} 
function startMonitor(){
   if(!$dtStats::prefEnable){// if we are running dont start again 
      $dtStats::prefEnable =1;
      if($dtStats::prefTestTime < 128){$dtStats::prefTestTime = 128;}  
      prefTest(getRealTime(),1);
   }
} 

function LogPrefIssue(%msg) {
   if($dtStats::prefLog){
      %filename = "serverStats/logs/PrefLogs.txt";
      if(!IsFile(%filename)) {
         new fileobject(Clog);
         Clog.openforwrite(%filename);
         Clog.writeline(%msg);
         Clog.close();
         Clog.delete();
      }
      else{
         new fileobject(Clog);
         Clog.openforappend(%filename);
         Clog.writeline(%msg);
         Clog.close();
         Clog.delete();
      }
   }
}
function dtLoadServerVars(){// keep function at the bottom
   if($dtStats::Enable){
      if(!statsGroup.serverStart){
         statsGroup.serverStart = 1;
         $dtServerVars::upTimeCount = -1;   
         if(isFile("serverStats/serverVars.cs")){
            exec("serverStats/serverVars.cs");
            %date = $dtServerVars::lastDate;
            %upTime = formatTime($dtServerVars::lastSimTime); 
            %mis = $dtServerVars::lastMission;
            if($dtStats::debugEchos){schedule(6000,0,"error","last server uptime = " SPC %date @ "-" @ %upTime @ "-" @ %mis);}  
            $dtServerVars::upTime[$dtServerVars::upTimeCount] = %date @ "-" @ %upTime @ "-" @ %mis;
         }
         if($dtServerVars::upTimeCount++ >= 30){
            $dtServerVars::upTimeCount = 0;  
         }
         $dtServerVars::lastSimTime = getSimTime();
         $dtServerVars::lastDate =  formattimestring("mm/dd/yy hh:nn:a");
         export( "$dtServerVars::*", "serverStats/serverVars.cs", false );
         if(isFile("serverStats/serverHangs.cs"))
            exec("serverStats/serverHangs.cs");
         if(isFile("serverStats/hostHangs.cs")) 
            exec("serverStats/hostHangs.cs");
         if(isFile("serverStats/playCount.cs"))
            exec("serverStats/playCount.cs");
         if(isFile("serverStats/lastPlay.cs"))
            exec("serverStats/lastPlay.cs");
         if(isFile("serverStats/mapDisconnects.cs"))
            exec("serverStats/mapDisconnects.cs");
         if(isFile("serverStats/mapReconnects.cs"))
            exec("serverStats/mapReconnects.cs");
         if(isFile("serverStats/voteFor.cs"))
            exec("serverStats/voteFor.cs");
         if(isFile("serverStats/skipCount.cs"))
            exec("serverStats/skipCount.cs");
         if(isFile("serverStats/maxPlayers.cs"))
            exec("serverStats/maxPlayers.cs");
         if(isFile("serverStats/clientCrash.cs"))
            exec("serverStats/clientCrash.cs");
         genBlanks();
         buildVarList();
         startMonitor();
         loadMapIdList();
      }
   }
}dtLoadServerVars();

function testVarsRandomAll(%max){
   %game = Game.class;
   for(%q = 0; %q < $statsVars::count[%game]; %q++){
      %varNameType = $statsVars::varNameType[%q,%game];
      %varName = $statsVars::varName[%q,%game]; 
      for(%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         %val = getRandom(0,%max);
         setDynamicField(%client.dtStats,%varName,%val);
         setDynamicField(%client,%varName,%val);
      }
   } 
}