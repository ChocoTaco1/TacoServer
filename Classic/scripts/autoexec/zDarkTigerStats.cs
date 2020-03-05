//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	Score hud stats system, gather data across x number of games to do math/stats									
//	This also has the added benefit of restoreing scores after leaving												
//	Script BY: DarkTiger																							
//  Version 1.0 - Initial release																					
//  Version 2.0 - Code refactor / optimizing/fixes																	
//  Version 3.0 - DM / LCTF added
//  Version 4.0 - Code refactor / optimizing / fixes
//	 Version 5.0 - DuleMod and Arena support / optimizing / fixes / misc stuff		
//	 Version 6.0 - Lan & Bot Support / Leaderboard / Stats Storage Overhaul / Optimization / Fixes 										
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
//    System now self maintains files and will delete when out of date see $dtStats::expire
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
//    7.0 ToDos
//       Replace or rework overallACC
//       Add option to load stats after players first game to reduce any sort of impact on the server
//       Remove or condense vehicle stats into one page kind of like the live screen and remove unused stuff
//       Armor stats to be reworked or removed undecided yet
//       Weapons,History to be reworked reduce the number of menus 
//       Match Stats to be reworked to show more intersting info possably rebuild like the live screen 
//       Clean up and optimize tracking functions, stuff was built to get things working not necessarily optimal/accurate 
//       Clean up or rework field array, looking at ways to add flexablity adding features like max avg  with out haveing to add to it 
//       Take look at reset code see if we can come up with a way to get rid or reduce the unused arrays   
//       Add few more things to the kill death screen from the new stuff that has been added and possably add history to it
//       With all the menu rework changes maybe look at making the main menu to be more intersting as well as all the other more optimal 

//-----------Settings------------

//disable stats system restart required;
$dtStats::Enable = 1; 
//Only self client can see his own stats, any stat, unless admin
$dtStats::viewSelf = 0; 
//set max number of individual game to record
//Note only tested to 100 games, hard cap at 300
$dtStats::MaxNumOfGames = 100;
//Value at witch total stats should cap out
//Note 32bit int cap is 2,147,483,647; so nothing byeond that
$dtStats::ValMax = 2000000000;
//This will load player stats after their first game, to reduce any impact on the server.
$dtStats::loadAfter = 0;//keep 0 not finished 
//enables self maintainer to deletes old files see $dtStats::expire
//Note may or may not cause issues
$dtStats::sm  = 1;
//deletes player stats files that are x amount days old, only works if $dtStats::sm  is enabled
$dtStats::expire = 60;  

//Record stats if player is here for x percentage of the game, set to 0 to rec every game
$dtStats::fgPercentage["CTFGame"] = 25;
//0 score based, 1 time based, 2 the closer one to finishing the game
$dtStats::fgPercentageType["CTFGame"] =2;

$dtStats::fgPercentage["LakRabbitGame"] = 25;
$dtStats::fgPercentageType["LakRabbitGame"] = 2;

$dtStats::fgPercentage["DMGame"] = 25;
$dtStats::fgPercentageType["DMGame"] = 2;

$dtStats::fgPercentage["SCtFGame"] = 25;
$dtStats::fgPercentageType["SCtFGame"] = 2;

// $dtStats::fgPercentage["ArenaGame"]/RoundsLimit * 100
$dtStats::fgPercentage["ArenaGame"] =20; 

//keep 0 as there is no measure of when a game is done
$dtStats::fgPercentage["DuelGame"] =0; 

// 30 sec min after not making an action reset
$dtStats::returnToMenuTimer = (30*1000);
//Load/saving rates to prevent any server hitching
$dtStats::slowSaveTime = 100;

//Disables save system, and only show stats of current play session 
$dtStats::Basic = 0;
//Control whats displayed  
$dtStats::Live = 1;  
$dtStats::KD = 1;
$dtStats::Hist =1;
$dtStats::Vehicle = 0; 
$dtStats::Armor = 0; 
$dtStats::Match = 0;
$dtStats::Weapon = 0;

//Leaderboards stuff
//To rebuild the leaderboards manually type lStatsCycle(1) into the console;
//This time marks the end of day and to rebuild the leaderboards, best set this time when the server is normally empty or low numbers
$dtStats::buildSetTime = "8\t00\tam"; 
// top 15 players per cat;
$dtStats::topAmount = 15;
//set to 1 to delete old leaderboards files
$dtStats::lsm  = 1;
//Set 2 or more to enable, this also contorls how much history you want, best to keep this count low 
$dtStats::day = 0;//-365
$dtStats::week = 0;//~53
$dtStats::month = 3; //-12
$dtStats::quarter = 0;//-4
$dtStats::year = 0;// number of years


//debug stuff
$dtStats::enableRefresh = 0;
$dtStats::debugEchos = 1;// echos function calls
//$pref::NoClearConsole = 1;
//setLogMode(1);
//$AIDisableChat = 1;

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
$dtStats::gameTypeCount = 6;
//short hand name
$dtStats::gtNameShort["CTFGame"] = "CTF";
$dtStats::gtNameShort["LakRabbitGame"] = "LakRabbit";
$dtStats::gtNameShort["DMGame"] = "DM";
$dtStats::gtNameShort["SCtFGame"] = "LCTF";
$dtStats::gtNameShort["ArenaGame"] = "Arena"; 
$dtStats::gtNameShort["DuelGame"] = "Duel"; 
//Display name 
$dtStats::gtNameLong["CTFGame"] = "Capture the Flag";
$dtStats::gtNameLong["LakRabbitGame"] = "LakRabbit";
$dtStats::gtNameLong["DMGame"] = "Deathmatch";
$dtStats::gtNameLong["SCtFGame"] = "Spawn CTF";
$dtStats::gtNameLong["ArenaGame"] = "Arena"; 
$dtStats::gtNameLong["DuelGame"] = "Duel MOD"; 


///////////////////////////////////////////////////////////////////////////////
//                             		CTF
///////////////////////////////////////////////////////////////////////////////
$dtStats::FC["CTFGame"] = 0;
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "kills";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "deaths";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "suicides";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "teamKills";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "flagCaps";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "flagGrabs";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "carrierKills";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "flagReturns";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "score";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "scoreMidAir";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "scoreHeadshot";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "scoreRearshot";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "escortAssists";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "defenseScore";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "offenseScore";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "flagDefends";

$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "winCount"; // in this script only
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "lossCount";
$dtStats::FV[$dtStats::FC["CTFGame"]++,"CTFGame"] = "destruction";
////////////////////////////Unused CTF Vars/////////////////////////////////////
$dtStats::uFC["CTFGame"] = 0;
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "tkDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "genDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "sensorDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "turretDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "iStationDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "vstationDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "mpbtstationDestroys"; 
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "solarDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "sentryDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "depSensorDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "depTurretDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "depStationDestroys";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "vehicleScore"; 
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "vehicleBonus"; 
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "genDefends";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "turretKills";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "mannedTurretKills";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "genRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "SensorRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "TurretRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "StationRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "VStationRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "mpbtstationRepairs"; 
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "solarRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "sentryRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "depSensorRepairs"; 
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "depInvRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "depTurretRepairs";
$dtStats::uFV[$dtStats::uFC["CTFGame"]++,"CTFGame"] = "returnPts";

///////////////////////////////////////////////////////////////////////////////
//                            	 LakRabbit								
///////////////////////////////////////////////////////////////////////////////
//Game type values - out of LakRabbitGame.cs
$dtStats::FC["LakRabbitGame"] = 0;
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "score";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "kills";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "deaths";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "suicides";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "flagGrabs";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "flagTimeMin";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "morepoints";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "mas";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalSpeed";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalDistance";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalChainAccuracy";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalChainHits";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalSnipeHits";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalSnipes";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalShockHits";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "totalShocks";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "MidairflagGrabs";
$dtStats::FV[$dtStats::FC["LakRabbitGame"]++,"LakRabbitGame"] = "MidairflagGrabPoints";
$dtStats::uFC["LakRabbitGame"] = 0;
$dtStats::uFV[$dtStats::uFC["LakRabbitGame"]++,"LakRabbitGame"] = "flagTimeMS";
///////////////////////////////////////////////////////////////////////////////
//                            	 DMGame								   		
///////////////////////////////////////////////////////////////////////////////
$dtStats::FC["DMGame"] = 0;
$dtStats::FV[$dtStats::FC["DMGame"]++,"DMGame"] = "score";
$dtStats::FV[$dtStats::FC["DMGame"]++,"DMGame"] = "kills";
$dtStats::FV[$dtStats::FC["DMGame"]++,"DMGame"] = "deaths";
$dtStats::FV[$dtStats::FC["DMGame"]++,"DMGame"] = "suicides";
$dtStats::FV[$dtStats::FC["DMGame"]++,"DMGame"] = "efficiency";

$dtStats::uFC["DMGame"] = 0;
$dtStats::uFV[$dtStats::uFC["DMGame"]++,"DMGame"] = "MidAir";
$dtStats::uFV[$dtStats::uFC["DMGame"]++,"DMGame"] = "Bonus";
$dtStats::uFV[$dtStats::uFC["DMGame"]++,"DMGame"] = "KillStreakBonus";
$dtStats::uFV[$dtStats::uFC["DMGame"]++,"DMGame"] = "killCounter";
///////////////////////////////////////////////////////////////////////////////
//                             		LCTF									
///////////////////////////////////////////////////////////////////////////////
$dtStats::FC["SCtFGame"] = 0;
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "kills";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "deaths";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "suicides";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "teamKills";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "flagCaps";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "flagGrabs";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "carrierKills";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "flagReturns";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "score";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "scoreMidAir";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "scoreHeadshot";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "scoreRearshot";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "escortAssists";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "defenseScore";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "offenseScore";
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "flagDefends";

$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "winCount";// in this script only
$dtStats::FV[$dtStats::FC["SCtFGame"]++,"SCtFGame"] = "lossCount";
////////////////////////////Unused LCTF Vars/////////////////////////////////////
$dtStats::uFC["SCtFGame"] = 0;
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "tkDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "genDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "sensorDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "turretDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "iStationDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "vstationDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "mpbtstationDestroys"; 
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "solarDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "sentryDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "depSensorDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "depTurretDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "depStationDestroys";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "vehicleScore"; 
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "vehicleBonus"; 
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "genDefends";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "escortAssists";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "turretKills";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "mannedTurretKills";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "genRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "SensorRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "TurretRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "StationRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "VStationRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "mpbtstationRepairs"; 
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "solarRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "sentryRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "depSensorRepairs"; 
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "depInvRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "depTurretRepairs";
$dtStats::uFV[$dtStats::uFC["SCtFGame"]++,"SCtFGame"] = "returnPts";
///////////////////////////////////////////////////////////////////////////////
//                            	 DuelGame								   		
///////////////////////////////////////////////////////////////////////////////
$dtStats::FC["DuelGame"]  = 0;
$dtStats::FV[$dtStats::FC["DuelGame"] ++,"DuelGame"] = "score";
$dtStats::FV[$dtStats::FC["DuelGame"] ++,"DuelGame"] = "kills";
$dtStats::FV[$dtStats::FC["DuelGame"] ++,"DuelGame"] = "deaths";
$dtStats::uFC["DuelGame"] = 0;
///////////////////////////////////////////////////////////////////////////////
//                            	 ArenaGame								   		
///////////////////////////////////////////////////////////////////////////////
$dtStats::FC["ArenaGame"] = 0;
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "kills";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "deaths";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "suicides";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "teamKills";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "snipeKills";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "roundsWon";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "roundsLost";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "assists";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "roundKills";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "hatTricks";
$dtStats::FV[$dtStats::FC["ArenaGame"]++,"ArenaGame"] = "score";
$dtStats::uFC["ArenaGame"] = 0;

///////////////////////////////////////////////////////////////////////////////
//                              Weapon/Misc Stats
///////////////////////////////////////////////////////////////////////////////
//these are field values from this script
$dtStats::FC["dtStats"] = 0;
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockLanceKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockLanceDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "explosionKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "explosionDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "impactKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "impactDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "groundKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "groundDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "turretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "turretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "aaTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "aaTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "indoorDepTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "indoorDepTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "outdoorDepTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "outdoorDepTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "sentryTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "sentryTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "outOfBoundKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "outOfBoundDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "lavaKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "lavaDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shrikeBlasterKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shrikeBlasterDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bellyTurretKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bellyTurretDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bomberBombsKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bomberBombsDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "tankChaingunKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "tankChaingunDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "tankMortarKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "tankMortarDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "satchelChargeKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "satchelChargeDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "lightningKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "lightningDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "vehicleSpawnKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "vehicleSpawnDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "forceFieldPowerUpKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "forceFieldPowerUpDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "crashKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "crashDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "waterKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "waterDeaths";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "nexusCampingKills";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "nexusCampingDeaths";

//Damage Stats
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockLanceInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockLanceIDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineInDmgTaken";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "SatchelInDmg";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "SatchelInDmgTaken";

//rounds fired
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockLanceShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeShotsFired";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineShotsFired";

$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgDirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserDirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterDirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "elfDirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeInHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockLanceIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineIndirectHits";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "SatchelInHits";

$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineHitMaxDist";
$dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockHitMaxDist";


   //misc
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserHeadShot";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockRearShot";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "minePlusDisc";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shotsFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "totalMA"; 
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "totalTime";  
   
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "killAir"; 
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "killGround"; 
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "deathAir"; 
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "deathGround"; 
      
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "inDirectHits";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "overallACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "airTime"; 
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "groundTime";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "EVKills"; 
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "EVDeaths";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "multiKills";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "chainKills";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "lightningMAkills";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "totalWepDmg";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "timeTL";


   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "killStreak";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "assist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "maxSpeed";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "avgSpeed";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "maxRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "comboPT";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "comboCount";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "distMov";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "weaponHitMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "weaponScore"; 
   
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorL";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorM";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorH";
   
   
  // if($dtStats::Armor){
 
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorMD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHD";
      
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLL";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLM";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLH";
      
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorML";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorMM";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorMH";
      
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHL";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHM";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHH";
      
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLLD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLMD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorLHD";
      
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorMLD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorMMD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorMHD";
      
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHLD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHMD";
      $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "armorHHD";
   //}

   //max kill distance
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineKillMaxDist";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockKillMaxDist";
   

   //weapon combos
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineCom";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockCom";
   
   //relative velocity
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineKillRV";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileKillRV";
   
   
   //acc
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineACC";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileACC";

   //midairs
   
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "cgMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "discMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "grenadeMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "laserMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mortarMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "shockMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "plasmaMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "blasterMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hGrenadeMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "missileMA";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mineMA";

if($dtStats::Vehicle){
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "ShrikeBlasterDmg";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "ShrikeBlasterDirectHits";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "ShrikeBlasterDmgTaken";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BellyTurretDmg";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BellyTurretDirectHits";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BellyTurretDmgTaken";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankChaingunDmg";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankChaingunDirectHits";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankChaingunDmgTaken";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BomberBombsInDmg";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BomberBombsInHits";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BomberBombsInDmgTaken";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankMortarInDmg";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankMortarInHits";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankMortarInDmgTaken";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "wildRK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "assaultRK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mobileBaseRK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "scoutFlyerRK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bomberFlyerRK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hapcFlyerRK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "wildRD";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "assaultRD";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mobileBaseRD";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "scoutFlyerRD";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bomberFlyerRD";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hapcFlyerRD";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "wildCrash";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "assaultCrash";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mobileBaseCrash";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "scoutFlyerCrash";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bomberFlyerCrash";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hapcFlyerCrash";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "wildEK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "assaultEK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "mobileBaseEK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "scoutFlyerEK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "bomberFlyerEK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "hapcFlyerEK";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "PlasmaTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "AATurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "MortarTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "MissileTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "IndoorDepTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "OutdoorDepTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "SentryTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "ShrikeBlasterFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BellyTurretFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "BomberBombsFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankChaingunFired";
   $dtStats::FV[$dtStats::FC["dtStats"]++,"dtStats"] = "TankMortarFired";
}
///////////////////////////////////////////////////////////////////
$dtStats::uFC["dtStats"] = 0; // not saved but used to calculate other stats that are saved
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillAir";


   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterDeathAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineDeathAir";
   
   

   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillGround";

   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterDeathGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineDeathGround";
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineDeathAirAir";
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaDeathAirAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterDeathAirGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineDeathAirGround";
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillAGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterDeathGroundAir";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineDeathGroundAir";
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillAGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterDeathGroundGround";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineDeathGroundGround";
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterScore";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineScore";
   
    //victim velocity
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "cgKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "discKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "grenadeKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "laserKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mortarKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "shockKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "plasmaKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "blasterKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "hGrenadeKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "mineKillVV";
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "missileKillVV";
   
   $dtStats::uFV[$dtStats::uFC["dtStats"]++,"dtStats"] = "ttl";
   
//////////////////////////////////////////////////////////////////////////////
//these are only saved in the total stats files 
   //keeps only the max value
   $dtStats::FC["max"] = 0; // value you want the max of \t the value you want it stored in 
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "cgKillMaxDist\tcgMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "discKillMaxDist\tdiscMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "grenadeKillMaxDist\tgrenadeMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "hGrenadeKillMaxDist\thGrenadeMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "laserKillMaxDist\tlaserMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "mortarKillMaxDist\tmortarMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "missileKillMaxDist\tmissileMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "plasmaKillMaxDist\tplasmaMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "blasterKillMaxDist\tblasterMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "mineKillMaxDist\tmineMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "shockKillMaxDist\tshockMaxDist";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "weaponHitMaxDist\tweaponHitMaxDistMax";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "score\tscoreMax";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "kills\tkillsMax";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "killStreak\tkillStreakMax";
   $dtStats::FV[$dtStats::FC["max"]++,"max"] = "maxSpeed\tmaxSpeedMax";
   

   //saves only the avg were total value does not make sence
   $dtStats::FC["avg"] = 0;//avg array  // value we want avg off \t new value we want to avg to be dumped into   
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "cgACC\tcgACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "discACC\tdiscACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "grenadeACC\tgrenadeACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "laserACC\tlaserACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "mortarACC\tmortarACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "shockACC\tshockACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "plasmaACC\tplasmaACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "blasterACC\tblasterACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "hGrenadeACC\thGrenadeACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "mineACC\tmineACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "missileACC\tmissileACCAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "score\tscoreAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "kills\tkillsAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "timeTL\ttimeTLAVG";
   $dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "overallACC\toverallACCAVG";
   //$dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "airTime\tairTimeAvg"; 
   //$dtStats::FV[$dtStats::FC["avg"]++,"avg"] = "groundTime\tgroundTimeAvg";
   
   
if(!$dtStats::Enable){return;} // abort exec
if(!isObject(statsGroup)){new SimGroup(statsGroup);}

function dtAICON(%client){ 
   dtStatsMissionDropReady(Game.getId(), %client);
}
package dtStats{
   function AIConnection::startMission(%client){// ai support
      parent::startMission(%client);
      if($dtStats::Enable)
         schedule(25000,0,"dtAICON",%client);
   }
   function CTFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
       if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function CTFGame::onClientLeaveGame(%game, %client){
      if($dtStats::Enable)
         dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function CTFGame::gameOver( %game ){
      parent::gameOver(%game);
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
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
   function CTFGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      if($dtStats::Enable)
         CTFHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   function CTFGame::resetScore(%game, %client){
     //to prevent the game type from resetting scores as we do it after the stats are saved    
      if(!$dtStats::Enable)
         parent::resetScore(%game, %client);
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
   function LakRabbitGame::onClientLeaveGame(%game, %client){
      if($dtStats::Enable)
         dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function LakRabbitGame::gameOver( %game ){
      parent::gameOver(%game);
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
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
      if($dtStats::Enable)
         LakRabbitHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   function LakRabbitGame::resetScore(%game, %client){
     //to prevent the game type from resetting scores as we do it after the stats are saved 
     if(!$dtStats::Enable)
         parent::resetScore(%game, %client);  
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
   function DMGame::onClientLeaveGame(%game, %client){
      if($dtStats::Enable)
         dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function DMGame::gameOver( %game ){
      parent::gameOver(%game);
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
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
      if($dtStats::Enable)
         DMHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   function DMGame::resetScore(%game, %client){
     //to prevent the game type from resetting scores as we do it after the stats are saved  
      if(!$dtStats::Enable)
         parent::resetScore(%game, %client);
   }
   ////////////////////////////////////////////////////////////////////////////////
   function SCtFGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function SCtFGame::onClientLeaveGame(%game, %client){
      if($dtStats::Enable)
         dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function SCtFGame::gameOver( %game ){
      parent::gameOver(%game);
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
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
      if($dtStats::Enable)
         CTFHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   function SCtFGame::resetScore(%game, %client){
     //to prevent the game type from resetting scores as we do it after the stats are saved  
      if(!$dtStats::Enable)
         parent::resetScore(%game, %client);
   }
   /////////////////////////////////////////////////////////////////////////////////////
   function ArenaGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function ArenaGame::onClientLeaveGame(%game, %client){
      dtStatsClientLeaveGame(%game, %client);//common
      if($dtStats::Enable)
         parent::onClientLeaveGame(%game, %client);
   }
   function ArenaGame::gameOver( %game ){
      parent::gameOver(%game);
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
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
      if($dtStats::Enable) 
         ArenaHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   function ArenaGame::resetScore(%game, %client){
   //to prevent the game type from resetting scores as we do it after the stats are saved  
     if(!$dtStats::Enable)
         parent::resetScore(%game, %client);
   }
   /////////////////////////////////////////////////////////////////////////////
   function DuelGame::clientMissionDropReady(%game, %client){
      parent::clientMissionDropReady(%game, %client);
      if($dtStats::Enable)
         dtStatsMissionDropReady(%game, %client);//common
   }
   function DuelGame::onClientLeaveGame(%game, %client){
      if($dtStats::Enable)
         dtStatsClientLeaveGame(%game, %client);//common
      parent::onClientLeaveGame(%game, %client);
   }
   function DuelGame::gameOver( %game ){
      parent::gameOver(%game);
      if($dtStats::Enable)
         dtStatsGameOver(%game);//common
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
      if($dtStats::Enable)
         DuelHud(%game, %client, %tag);
      else
         parent::updateScoreHud(%game, %client, %tag);
   }
   function DuelGame::resetScore(%game, %client){
    //to prevent the game type from resetting scores as we do it after the stats are saved  
      if(!$dtStats::Enable)
         parent::resetScore(%game, %client);
   }
///////////////////////////////////////////////////////////////////////////////
   function DefaultGame::forceObserver( %game, %client, %reason ){
      parent::forceObserver( %game, %client, %reason );
      if($dtStats::Enable){
         %client.gt = %client.at = 0;//air time ground time reset   
      }
   }
   function DefaultGame::missionLoadDone(%game){
      parent::missionLoadDone(%game);
          //check to see if we are running evo or not, if not then lets just enable these 
         if(!isFile("scripts/autoexec/evolution.cs")){
            $Host::EvoAveragePings = $Host::ShowIngamePlayerScores = 1;
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
      if($dtStats::Enable){
         %obj.dtNade = 1;
         $dtObjExplode = %obj;
      }
      parent::detonateGrenade(%obj);
   } 
   function MineDeployed::onThrow(%this, %mine, %thrower){
       parent::onThrow(%this, %mine, %thrower);
       if($dtStats::Enable){
          %thrower.client.mineShotsFired++;
          %thrower.client.shotsFired++;
          %thrower.client.mineACC = (%thrower.client.mineIndirectHits / %thrower.client.mineShotsFired) * 100;
      }
   }
   function GrenadeThrown::onThrow(%this, %gren,%thrower){
       parent::onThrow(%this, %gren);
       if($dtStats::Enable){
          %thrower.client.hGrenadeShotsFired++;
          %thrower.client.shotsFired++;
          %thrower.client.hGrenadeACC = (%thrower.client.hGrenadeInHits / %thrower.client.hGrenadeShotsFired) * 100;
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
      //echo(%triggerNum SPC %val);
      if($dtStats::Enable){
         if(isObject(%player) && !%player.getObjectMount()){
            if(%val){// cuts  the amount of tiggers by half
               %client = %player.client;
               %speed = mFloor(vectorLen(%player.getVelocity()) * 3.6);
               
               if(%client.maxSpeed < %speed){%client.maxSpeed = %speed;}
               %client.avgTSpeed += %speed; %client.avgSpeedCount++;
               %client.avgSpeed = %client.avgTSpeed/%client.avgSpeedCount;
               if(%client.avgSpeedCount >= 500){%client.avgSpeedCount=%client.avgTSpeed=0;}   
               
               //dist moved
               %xypos = getWords(%player.getPosition(),0,1) SPC 0;
               if(%client.lp !$= ""){%client.distMov = mFloor(%client.distMov + vectorDist(%client.lp,%xypos));}
                  %client.lp = %xypos;
            }
               
            if (%triggerNum == 3){ //jet triggers 
               if(%val){
                  if(isEventPending(%player.jetTimeTest)){
                     cancel(%player.jetTimeTest);
                  }
                   %client.jetTrigCount++;
                  if(%client.ground){
                     if(%client.gt > 0){
                        %client.groundTime += ((getSimTime() - %client.gt)/1000)/60;
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
                     //%player.testDis = %player.getPosition();
                     //%a = getVecAngle(%player.getForwardVector(),vectorNormalize(%player.getVelocity()),%zv);
                     //%hv = vectorLen(%player.getVelocity());
                     //%range = %hv * mCos(%a * 3.14159/180) * (%hv * mSin(%a * 3.14159/180) + mSqrt(mPow((%hv * mSin(%a * 3.14159/180)),2) + 2 * mAbs(getGravity()) * %dis)) / mAbs(getGravity());
                     %time = (((%zv + mSqrt(mPow((%zv),2) + 2 * mAbs(getGravity()) * %dis)) / mAbs(getGravity()))* 1000); // not perfect but close enough with out getting too crazy and facy
                    // error(%dis SPC %time SPC %range);
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
};

function chkGrounded(%player){
   if(isObject(%player)){
      %client =  %player.client;
      if(!%client.ground){
         if(%client.at > 0){
            %client.airTime += ((getSimTime() - %client.at)/1000)/60;
         }
         %client.gt =  getSimTime();
      }
      %client.ground = 1;
      %player.jetTimeTest = 0;
   }
 // error(%client.airTime SPC %client.groundTime);
}
if($dtStats::Enable){
   activatePackage(dtStats);
}
////////////////////////////////////////////////////////////////////////////////
//							 Game Type Commons								  //
////////////////////////////////////////////////////////////////////////////////
function dtGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){  
    error(%game SPC %client SPC %arg1 SPC %arg2 SPC %arg3 SPC %arg4 SPC %arg5);
    %client.viewClient = getCNameToCID(%arg3);
   if(%arg1 $= "Stats" && %client.viewClient != 0){
      %client.viewStats = 1;// lock out score hud from updateing untill they are done
      %client.viewMenu = %arg2;
      %client.GlArg4 = %arg4;
      %client.GlArg5 = %arg5;
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
   %client.lp = "";//last position for distMove
   %client.lgame = %game.class;
   %foundOld = 0;
   %mrx = setGUIDName(%client);// make sure we  have a guid if not make one
   if(!isObject(%client.dtStats)){
      for (%i = 0; %i < statsGroup.getCount(); %i++){ // check to see if my old data is still there
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.guid == %client.guid && %dtStats.markForDelete == 0)
         {
            if(Game.getGamePct() <= (100 - $dtStats::fgPercentage[%game.class])){// see if they will be here long enough to count as a full game
               %client.dtStats.dtGameCounter++;
            }
            %client.dtStats = %dtStats;
            %dtStats.client = %client;
            %dtStats.clientLeft = 0;
            %dtStats.markForDelete = 0;
            %client.joinTime = getSimTime();
            %foundOld =1;
            resGameStats(%client,%game.class); // restore stats;
            if(%client.score >= 1 || %client.score <= -1 ){ //if(%num >= 1 || %num <= -1 ){
               messageClient(%client, 'MsgClient', '\crWelcome back %1. Your score has been restored.~wfx/misc/rolechange.wav', %client.name);
            }
            break;
         }
      }
      if(!%foundOld){
         %dtStats = new scriptObject(); // object used stats storage
         statsGroup.add(%dtStats);
         %client.dtStats = %dtStats;
         %dtStats.gameStats["totalGames","g",%game.class] = 0;
         %dtStats.gameStats["statsOverWrite","g",%game.class] = -1;
         %dtStats.gameStats["fullSet","g",%game.class] = 0;
         %dtStats.client =%client;
         if(%mrx){%dtStats.mrxUID = 1;}
         else{%dtStats.mrxUID = 0;}
         %dtStats.guid = %client.guid;
         %dtStats.name =  stripChars( detag( getTaggedString( %client.name ) ), "\cp\co\c6\c7\c8\c9" );
         %dtStats.clientLeft = 0;
         %dtStats.markForDelete = 0;
         %client.joinTime = getSimTime();
		   resetDtStats(%client,%game.class,1);
		   if(!$dtStats::loadAfter){
            loadGameStats(%client.dtStats,%game.class);
		   }
		   else{
            %dtStats.isLoaded = 0;
		   }
         %client.dtStats.gameData[%game.class] = 1;
         %client.dtStats.dtGameCounter = 0;
         if(Game.getGamePct() <= (100 - $dtStats::fgPercentage[%game.class])){// see if they will be here long enough to count as a full game
            %client.dtStats.dtGameCounter++;
         }
      }
   }
   else if(isObject(%client.dtStats) && %client.dtStats.gameData[%game.class] != 1){ // game type change
      %client.dtStats.gameStats["totalGames","g",%game.class] = 0;
      %client.dtStats.gameStats["statsOverWrite","g",%game.class] = -1;
      %client.dtStats.gameStats["fullSet","g",%game.class] = 0;
      resetDtStats(%client,%game.class,1);
      loadGameStats(%client.dtStats,%game.class);
      %client.dtStats.gameData[%game.class] = 1;
   }
   else if(!%client.dtStats.isLoaded ){
       loadGameStats(%client.dtStats,%game.class);
   }
}
function dtStatsClientLeaveGame(%game, %client){
   if(isObject(%client.dtStats)){
      if(%client.score < 1 && %client.score > -1 ){ // if(%num < 1 && %num > -1 ){
         if(isObject(%client.dtStats)){
            %client.dtStats.delete();
         }
         return;
      }
      %client.dtStats.clientLeft = 1;
      %game.postGameStats(%client);
      bakGameStats(%client,%game.class);//back up there current game in case they lost connection
      %client.dtStats.leftPCT = %game.getGamePct();
   }
}
function dtStatsGameOver( %game ){
   $dtStats::LastMission = $MissionDisplayName;
   if($dtStats::debugEchos){error("dtStatsGameOver");}
   %timeNext =0;
   for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients data
      %dtStats = statsGroup.getObject(%i);
      if(%dtStats.clientLeft){ // find any that left during the match and
         if(%dtStats.leftPCT >= $dtStats::fgPercentage[%game.class]){ // if they where here for most of it and left at the end save it
            %dtStats.markForDelete = 1;
             %time += $dtStats::slowSaveTime; // this will chain them
             schedule(%time ,0,"incBakGameStats",%dtStats,%game.class);
            %time += $dtStats::slowSaveTime; // this will chain them
            schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
         }
         else{
            %dtStats.markForDelete = 1;
            %time +=  $dtStats::slowSaveTime; // this will chain them
            schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
         }
      }
   }
   for (%z = 0; %z < ClientGroup.getCount(); %z++){
      %client = ClientGroup.getObject(%z);
      %client.viewMenu = %client.viewClient = %client.viewStats = 0;//reset hud
      if(isObject(%client.dtStats)){
         %game.postGameStats(%client);
         //make sure the game was long enough, in case admin changes maps 
         if(%game.getGamePct() >= $dtStats::fgPercentage[%game.class]  && %client.dtstats.dtGameCounter > 0){
            %time += $dtStats::slowSaveTime; // this will chain them
            schedule(%time ,0,"incGameStats",%client.dtStats,%game.class); //resetDtStats after incGame
              %time += $dtStats::slowSaveTime;
            schedule(%time,0,"saveGameStats",%client.dtStats,%game.class); //
         }
         else{
            %client.dtStats.dtGameCounter++;
            resetDtStats(%client,%game.class,1);
         }
      }
   }
}

////////////////////////////////////////////////////////////////////////////////
//							Supporting Functions							  //
////////////////////////////////////////////////////////////////////////////////
function DefaultGame::postGameStats(%game,%client){ //stats to add up at the end of the match 

   %client.totalTime = ((getSimTime() - %client.joinTime)/1000)/60;//convert it to min
   
   %client.cgScore         = %client.cgKill       + %client.cgMA       + %client.cgKillAir        + (%client.cgKillMaxDist/100)      + %client.cgCom;
   %client.discScore       = %client.discKill     + %client.discMA     + %client.discKillAir      + (%client.discKillMaxDist/100)    + %client.discCom;
   %client.hGrenadeScore   = %client.hGrenadeKill + %client.hGrenadeMA + %client.hGrenadeKillAir  + (%client.hGrenadeKillMaxDist/20) + %client.hGrenadeCom;
   %client.grenadeScore    = %client.grenadeKill  + %client.grenadeMA  + %client.grenadeKillAir   + (%client.grenadeKillMaxDist/100) + %client.grenadeCom;
   %client.laserScore      = %client.laserKill    + %client.laserMA    + %client.laserKillAir     + (%client.laserKillMaxDist/250)   + %client.laserCom    + %client.laserHeadShot;
   %client.mortarScore     = %client.mortarKill   + %client.mortarMA   + %client.mortarKillAir    + (%client.mortarKillMaxDist/50)   + %client.mortarCom;
   %client.missileScore    = %client.missileKill  + %client.missileMA  + %client.missileKillAir   + (%client.missileKillMaxDist/500) + %client.missileCom;
   %client.shockScore      = %client.shockKill    + %client.shockMA    + %client.shockKillAir     + (%client.shockKillMaxDist/2)     + %client.shockCom    + %client.shockRearShot;
   %client.plasmaScore     = %client.plasmaKill   + %client.plasmaMA   + %client.plasmaKillAir    + (%client.plasmaKillMaxDist/50)   + %client.plasmaCom;
   %client.blasterScore    = %client.blasterKill  + %client.blasterMA  + %client.blasterKillAir   + (%client.blasterKillMaxDist/50)  + %client.blasterCom;
   %client.mineScore       = %client.mineKill     + %client.mineMA     + %client.mineKillAir      + (%client.mineKillMaxDist/20)     + %client.mineCom;
   
   %client.weaponScore =   %client.cgScore + 
                           %client.discScore + 
                           %client.grenadeScore + 
                           %client.laserScore + 
                           %client.mortarScore + 
                           %client.shockScore + 
                           %client.plasmaScore + 
                           %client.blasterScore + 
                           %client.hGrenadeScore + 
                           %client.missileScore + 
                           %client.mineScore;
   
   %client.totalMA = %client.cgMA + 
                     %client.discMA + 
                     %client.grenadeMA + 
                     %client.laserMA + 
                     %client.mortarMA + 
                     %client.shockMA + 
                     %client.plasmaMA + 
                     %client.blasterMA + 
                     %client.hGrenadeMA + 
                     %client.missileMA + 
                     %client.mineMA;
                         
   %client.killAir = %client.cgKillAir +
                     %client.discKillAir +
                     %client.hGrenadeKillAir +
                     %client.grenadeKillAir +
                     %client.laserKillAir +
                     %client.mortarKillAir +
                     %client.missileKillAir +
                     %client.shockKillAir +
                     %client.plasmaKillAir +
                     %client.blasterKillAir +
                     %client.mineKillAir;
                     
   %client.deathAir = %client.cgDeathAir +
                        %client.discDeathAir + 
                        %client.hGrenadeDeathAir +
                        %client.grenadeDeathAir +
                        %client.laserDeathAir +
                        %client.mortarDeathAir +
                        %client.missileDeathAir +
                        %client.shockDeathAir +
                        %client.plasmaDeathAir +
                        %client.blasterDeathAir +
                        %client.mineDeathAir;

   %client.killGround = %client.cgKillGround +
                        %client.discKillGround +
                        %client.hGrenadeKillGround +
                        %client.grenadeKillGround +
                        %client.laserKillGround +
                        %client.mortarKillGround +
                        %client.missileKillGround +
                        %client.shockKillGround +
                        %client.plasmaKillGround +
                        %client.blasterKillGround +
                        %client.mineKillGround;
                        
   %client.deathGround =   %client.cgDeathGround +
                           %client.discDeathGround +
                           %client.hGrenadeDeathGround +
                           %client.grenadeDeathGround +
                           %client.laserDeathGround +
                           %client.mortarDeathGround +
                           %client.missileDeathGround +
                           %client.shockDeathGround +
                           %client.plasmaDeathGround +
                           %client.blasterDeathGround +
                           %client.mineDeathGround;
                           
   %client.EVKills =    %client.explosionKills +
                        %client.groundKills +
                        %client.outOfBoundKills +
                        %client.lavaKills +
                        %client.lightningKills +
                        %client.vehicleSpawnKills +
                        %client.forceFieldPowerUpKills +
                        %client.nexusCampingKills;

   %client.EVDeaths =   %client.explosionDeaths +
                        %client.groundDeaths +
                        %client.outOfBoundDeaths +
                        %client.lavaDeaths +
                        %client.lightningDeaths +
                        %client.vehicleSpawnDeaths +
                        %client.forceFieldPowerUpDeaths +
                        %client.nexusCampingDeaths;
                        
   %client.totalWepDmg = %client.cgDmg +
                         %client.laserDmg +
                         %client.blasterDmg +
                         %client.elfDmg +
                         %client.discInDmg +
                         %client.grenadeInDmg +
                         %client.hGrenadeInDmg +
                         %client.mortarInDmg +
                         %client.missileInDmg +
                         %client.plasmaInDmg +
                         %client.shockLanceInDmg +
                         %client.mineInDmg +
                         %client.SatchelInDmg;
                         
                         
   if(%game.class $= "CTFGame" || %game.class $= "SCtFGame"){
     %client.destruction =    %client.genDestroys + 
                              %client.solarDestroys + 
                              %client.sensorDestroys + 
                              %client.turretDestroys + 
                              %client.IStationDestroys +
                              %client.aStationDestroys + 
                              %client.VStationDestroys + 
                              %client.sentryDestroys + 
                              %client.depSensorDestroys + 
                              %client.depTurretDestroys +
                              %client.depStationDestroys + 
                              %client.mpbtstationDestroys;
                              
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
   else if(%game.class $= "LakRabbitGame"){
      %client.flagTimeMin = mFloor((%client.flagTimeMS / 1000)/60); 
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
function loadGameStats(%dtStats,%game,%total){// called when client joins server.cs onConnect
   if($dtStats::debugEchos){error("loadGameStats GUID = "  SPC %dtStats.guid);} 
   %dtStats.isLoaded = 1;
   if(%dtStats.guid !$= "" && !$dtStats::Basic){
      loadGameTotalStats(%dtStats,%game);
      %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
      if(isFile(%filename)){
         %file = new FileObject();
         %file.OpenForRead(%filename);
         while( !%file.isEOF() ){
            %line = strreplace(%file.readline(),"%t","\t");
            %var = getField(%line,0);
            %dtStats.gameStats[%var,"g",%game] =  getFields(%line,1,getFieldCount(%line)-1);
         }
         %file.close();
         %file.delete();
      }
   }
}
function loadGameTotalStats(%dtStats,%game){
   if($dtStats::debugEchos){error("loadGameTotalStats GUID = "  SPC %dtStats.guid);}
   
   %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "t.cs";
   %d = $dtStats::curDay; %w = $dtStats::curWeek; %m = $dtStats::curMonth; %q = $dtStats::curQuarter; %y = $dtStats::curYear;   
   if(isFile(%filename)){
      %file = new FileObject();
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
      while( !%file.isEOF() ){
         %line = strreplace(%file.readline(),"%t","\t");
         %var = getField(%line,0);
         if(%var $= "playerName"){continue;}// skip vars
         else{
            
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
            
            if(%d1 > $dtStats::ValMax){%d1 = $dtStats::ValMax;error("ValMax Day Hit" SPC %var SPC %game SPC %dtStats.guid);}
            if(%w1 > $dtStats::ValMax){%w1 = $dtStats::ValMax;error("ValMax Week Hit" SPC %var SPC %game SPC %dtStats.guid);}
            if(%m1 > $dtStats::ValMax){%m1 = $dtStats::ValMax;error("ValMax Month Hit" SPC %var SPC %game SPC %dtStats.guid);}
            if(%q1 > $dtStats::ValMax){%q1 = $dtStats::ValMax;error("ValMax Quarter Hit" SPC %var SPC %game SPC %dtStats.guid);}
            if(%y1 > $dtStats::ValMax){%y1 = $dtStats::ValMax;error("ValMax Year Hit" SPC %var SPC %game SPC %dtStats.guid);}
            
            %dtStats.gameStats[%var,"t",%game] = %d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1;

         }
      }
      %file.close();
      %file.delete();
   }
   else{// must be new person so be sure to set the dates 
      %dtStats.gameStats["dwmqy","t",%game] =  %d TAB %d TAB %w TAB %w TAB %m TAB %m TAB %q TAB %q TAB %y TAB %y;  
   }
}


function saveGameStats(%dtStats,%game){
   if($dtStats::debugEchos){error("saveGameStats GUID = "  SPC %dtStats.guid);}

      if(%dtStats.guid !$= "" && !$dtStats::Basic){
         saveTotalStats(%dtStats,%game);
   
         %file = new FileObject();
         %filename = "serverStats/stats/" @ %game @ "/" @ %dtStats.guid  @ "g.cs";
         %file.OpenForWrite(%filename);
         %file.writeLine("playerName" @ "%t" @ trim(%dtStats.name));
         %file.writeLine("statsOverWrite" @ "%t" @ %dtStats.gameStats["statsOverWrite","g",%game]);
         %file.writeLine("totalGames" @ "%t" @  %dtStats.gameStats["totalGames","g",%game]);
         %file.writeLine("fullSet" @ "%t" @  %dtStats.gameStats["fullSet","g",%game]);
         %file.writeLine("dayStamp" @ "%t" @ strreplace(%dtStats.gameStats["dayStamp","g",%game],"\t","%t"));
         %file.writeLine("dateStamp" @ "%t" @ strreplace(%dtStats.gameStats["dateStamp","g",%game],"\t","%t"));
         %file.writeLine("timeDayMonth" @ "%t" @ strreplace(%dtStats.gameStats["timeDayMonth","g",%game],"\t","%t"));
         %file.writeLine("map" @ "%t" @ strreplace(%dtStats.gameStats["map","g",%game],"\t","%t"));
         for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
            %val = $dtStats::FV[%i,%game];
            %var = %dtStats.gameStats[%val,"g",%game];
            %file.writeLine(%val @ "%t" @ strreplace(%var,"\t","%t"));
         }
         for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
            %val = $dtStats::FV[%i,"dtStats"];
            %var = %dtStats.gameStats[%val,"g",%game];
            %file.writeLine(%val @ "%t" @ strreplace(%var,"\t","%t"));
         }

         %file.close();
         %file.delete();
      }
      if(%dtStats.markForDelete){
         %dtStats.delete();
      }
     // if($dtStats::saveDailies){saveDailyStats(%dtStats,%game);}
}
function saveTotalStats(%dtStats,%game){ // saved by the main save function
   if($dtStats::debugEchos){error("saveTotalStats GUID = "  SPC %dtStats.guid);}
   if(%dtStats.gameStats["statsOverWrite","g",%game] $= ""){%dtStats.gameStats["statsOverWrite","g",%game] = 0;}
      %filename = "serverStats/stats/"@ %game @ "/" @ %dtStats.guid  @ "t.cs";  
      %file = new FileObject();
      %file.OpenForWrite(%filename);
      %file.writeLine("days" @ "%t" @ strreplace(%dtStats.gameStats["dwmqy","t",%game],"\t","%t"));                                
      %file.writeLine("gameCount" @ "%t" @ strreplace(%dtStats.gameStats["gameCount","t",%game],"\t","%t"));
      %file.writeLine("playerName" @ "%t" @  %dtStats.name);

      for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
         %val = $dtStats::FV[%i,%game];
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ strreplace(%var,"\t","%t"));
      }
      for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
         %val = $dtStats::FV[%i,"dtStats"];
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ strreplace(%var,"\t","%t"));
      }
      for(%i = 1; %i <= $dtStats::FC["max"]; %i++){
         %val = getField($dtStats::FV[%i,"max"],1);
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ strreplace(%var,"\t","%t"));
      }
      for(%i = 1; %i <= $dtStats::FC["avg"]; %i++){
         %val = getField($dtStats::FV[%i,"avg"],1);
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ strreplace(%var,"\t","%t"));
      }
      %file.close();
      %file.delete();
}

function incGameStats(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("incGameStats GUID = "  SPC %dtStats.guid);} 
   if(!%dtStats.isLoaded) // if not loaded load total stats so we can save 
      loadGameStats(%dtStats,%game);
      
   %dtStats.client.viewMenu = "Reset";
   
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
   setValueField(%dtStats,"dateStamp","g",%game,%c,formattimestring("yy-mm-dd hh:nn:ss"));
   setValueField(%dtStats,"timeDayMonth","g",%game,%c,formattimestring("hh:nn a, mm-dd"));
   setValueField(%dtStats,"map","g",%game,%c,$dtStats::LastMission);
   for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
      %val = $dtStats::FV[%i,%game];
      %var = getDynamicField(%dtStats.client,%val);
      
      setValueField(%dtStats,%val,"g",%game,%c,%var);

      %t1 = getField(%dtStats.gameStats[%val,"t",%game],1);
      setValueField(%dtStats,%val,"t",%game,1,%var + %t1);
      %t7 = getField(%dtStats.gameStats[%val,"t",%game],3);
      setValueField(%dtStats,%val,"t",%game,3,%var + %t7);
      %t30 = getField(%dtStats.gameStats[%val,"t",%game],5);
      setValueField(%dtStats,%val,"t",%game,5,%var + %t30);
      %t90 = getField(%dtStats.gameStats[%val,"t",%game],7);
      setValueField(%dtStats,%val,"t",%game,7,%var + %t90);
      %t365 = getField(%dtStats.gameStats[%val,"t",%game],9);
      setValueField(%dtStats,%val,"t",%game,9,%var + %t365);
   }
   for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
      %val = $dtStats::FV[%i,"dtStats"];
      %var = getDynamicField(%dtStats.client,%val);
      setValueField(%dtStats,%val,"g",%game,%c,%var);
      
      %t1 = getField(%dtStats.gameStats[%val,"t",%game],1);
      setValueField(%dtStats,%val,"t",%game,1,%var + %t1);
      %t7 = getField(%dtStats.gameStats[%val,"t",%game],3);
      setValueField(%dtStats,%val,"t",%game,3,%var + %t7);
      %t30 = getField(%dtStats.gameStats[%val,"t",%game],5);
      setValueField(%dtStats,%val,"t",%game,5,%var + %t30);
      %t90 = getField(%dtStats.gameStats[%val,"t",%game],7);
      setValueField(%dtStats,%val,"t",%game,7,%var + %t90);
      %t365 = getField(%dtStats.gameStats[%val,"t",%game],9);
      setValueField(%dtStats,%val,"t",%game,9,%var + %t365);
   }
   for(%i = 1; %i <= $dtStats::FC["max"]; %i++){
      %newVal = getField($dtStats::FV[%i,"max"],1);
      
      %val = getField($dtStats::FV[%i,"max"],0);// grab the val we want max of
      %var = getDynamicField(%dtStats.client,%val);
      
      %t1 = getField(%dtStats.gameStats[%newVal,"t",%game],1);
      %t7 = getField(%dtStats.gameStats[%newVal,"t",%game],3);
      %t30 = getField(%dtStats.gameStats[%newVal,"t",%game],5);
      %t90 = getField(%dtStats.gameStats[%newVal,"t",%game],7);
      %t365 = getField(%dtStats.gameStats[%newVal,"t",%game],9);
      
      if(%var > %t1){  setValueField(%dtStats,%newVal,"t",%game,1,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,1,%t1);}
      if(%var > %t7){  setValueField(%dtStats,%newVal,"t",%game,3,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,3,%t7);}
      if(%var > %t30){ setValueField(%dtStats,%newVal,"t",%game,5,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,5,%t30);}
      if(%var > %t90){ setValueField(%dtStats,%newVal,"t",%game,7,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,7,%t90);}
      if(%var > %t365){setValueField(%dtStats,%newVal,"t",%game,9,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,9,%t365);}
   }
   for(%i = 1; %i <= $dtStats::FC["avg"]; %i++){
      %val = getField($dtStats::FV[%i,"avg"],0);
      %avgVal = getField($dtStats::FV[%i,"avg"],1);
      %var = getDynamicField(%dtStats.client,%val);
      
      %g1 = getField(%dtStats.gameStats["gameCount","t",%game],1); //game counts
      %g7 = getField(%dtStats.gameStats["gameCount","t",%game],3);     
      %g30 = getField(%dtStats.gameStats["gameCount","t",%game],5);     
      %g90 = getField(%dtStats.gameStats["gameCount","t",%game],7);     
      %g365 = getField(%dtStats.gameStats["gameCount","t",%game],9);          
      %g1 = (%g1 ? %g1 : 1);  %g7 = (%g7 ? %g7 : 1);  %g30 = (%g30 ? %g30 : 1); %g90 = (%g90 ? %g90 : 1); %g365 = (%g365 ? %g365 : 1);
      
      %t1 = getField(%dtStats.gameStats[%val,"t",%game],1);
      setValueField(%dtStats,%avgVal,"t",%game,1, %t1 / %g1);
      %t7 = getField(%dtStats.gameStats[%val,"t",%game],3);
      setValueField(%dtStats,%avgVal,"t",%game,3, %t7 / %g7);
      %t30 = getField(%dtStats.gameStats[%val,"t",%game],5);
      setValueField(%dtStats,%avgVal,"t",%game,5, %t30 / %g30);
      %t90 = getField(%dtStats.gameStats[%val,"t",%game],7);
      setValueField(%dtStats,%avgVal,"t",%game,7, %t90 / %g90);
      %t365 = getField(%dtStats.gameStats[%val,"t",%game],9);
      setValueField(%dtStats,%avgVal,"t",%game,9, %t365 / %g365);
   }

   resetDtStats(%dtStats.client,%game,1); // reset to 0 for next game
}

function getTimeDif(%time){
   %x = formattimestring("hh");
   %y = formattimestring("nn");
   %z = formattimestring("a");
   %a = getField(%time,0);
   %b = getField(%time,1);
   %c = getField(%time,2);
   if(%c $= "pm" && %a < 12){
      %a += 12; 
   }
   else if(%c $= "am" && %a == 12){
      %a = 0;
   }
   if(%z $= "pm" && %x < 12){
      %x += 12; 
   }
   else if(%z $= "am" && %z == 12){
      %a = 0;
   }

   %v = %a + (%b/60);
   %w = %x + (%y/60);
   if(%v >  %w){%h = mabs(%v - %w);}
   else{ %h =  24 - mabs(%v - %w);}
   //error(%h);
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
   $dtStats::blank["g"] = $dtStats::blank["t"] = 0;
   for(%i=0; %i < $dtStats::MaxNumOfGames-2; %i++){
      $dtStats::blank["g"] = $dtStats::blank["g"] TAB 0;  
   }  
   for(%i=0; %i < 8; %i++){
      $dtStats::blank["t"] = $dtStats::blank["t"] TAB 0;  
   } 
}genBlanks();

function setValueField(%dtStats,%val,%type,%game,%c,%var){
   if(%type $= "g"){
      %fc = getFieldCount(%dtStats.gameStats[%val,%type,%game]);
      if(%fc < 3){//new value was added so fill it with 0 values 
         %dtStats.gameStats[%val,%type,%game] = $dtStats::blank["g"];  
      }
      else if( %fc > $dtStats::MaxNumOfGames){// trim it down as it as the MaxNumOfGames have gotten smaller 
         %dtStats.gameStats[%val,%type,%game] = getFields(%dtStats.gameStats[%val,%type,%game],0,$dtStats::MaxNumOfGames-1);
      }
      %dtStats.gameStats[%val,%type,%game] =   setField(%dtStats.gameStats[%val,%type,%game],%c, hasValue(%var));
   }
   else if(%type $= "t"){
      %fc = getFieldCount(%dtStats.gameStats[%val,%type,%game]);
      if(%fc < 3){//new value was added so fill it with 0 values 
         %dtStats.gameStats[%val,%type,%game] = $dtStats::blank["t"];
      }
      %dtStats.gameStats[%val,%type,%game] =   setField(%dtStats.gameStats[%val,%type,%game],%c, hasValue(%var));
   }
   
}

function hasValue(%val){//make sure we have at least something in the field spot
  if(%val $= ""){return 0;}
  return %val; 
}

function incBakGameStats(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::debugEchos){error("incBakGameStats GUID = "  SPC %dtStats.guid);}    
   
   if(!%dtStats.isLoaded)  
      loadGameStats(%dtStats,%game);
      
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
   setValueField(%dtStats,"dateStamp","g",%game,%c,formattimestring("yy-mm-dd hh:nn:ss"));  
   setValueField(%dtStats,"timeDayMonth","g",%game,%c,formattimestring("hh:nn a, mm-dd"));
   setValueField(%dtStats,"map","g",%game,%c,$dtStats::LastMission);
   for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
      %val = $dtStats::FV[%i,%game];
      %var = %dtStats.gameStats[%val,"b",%game];

      setValueField(%dtStats,%val,"g",%game,%c,%var);
      
      %t1 = getField(%dtStats.gameStats[%val,"t",%game],1);
      setValueField(%dtStats,%val,"t",%game,1,%var + %t1);
      %t7 = getField(%dtStats.gameStats[%val,"t",%game],3);
      setValueField(%dtStats,%val,"t",%game,3,%var + %t7);
      %t30 = getField(%dtStats.gameStats[%val,"t",%game],5);
      setValueField(%dtStats,%val,"t",%game,5,%var + %t30);
      %t90 = getField(%dtStats.gameStats[%val,"t",%game],7);
      setValueField(%dtStats,%val,"t",%game,7,%var + %t90);
      %t365 = getField(%dtStats.gameStats[%val,"t",%game],9);
      setValueField(%dtStats,%val,"t",%game,9,%var + %t365);
   }
   for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
      %val = $dtStats::FV[%i,"dtStats"];
      %var = %dtStats.gameStats[%val,"b",%game];
      %dtStats.gameStats[%val,"g",%game] = setField(%dtStats.gameStats[%val,"g",%game],%c,%var);
   
      setValueField(%dtStats,%val,"g",%game,%c,%var);
      
      %t1 = getField(%dtStats.gameStats[%val,"t",%game],1);
      setValueField(%dtStats,%val,"t",%game,1,%var + %t1);
      %t7 = getField(%dtStats.gameStats[%val,"t",%game],3);
      setValueField(%dtStats,%val,"t",%game,3,%var + %t7);
      %t30 = getField(%dtStats.gameStats[%val,"t",%game],5);
       setValueField(%dtStats,%val,"t",%game,5,%var + %t30);
      %t90 = getField(%dtStats.gameStats[%val,"t",%game],7);
      setValueField(%dtStats,%val,"t",%game,7,%var + %t90);
      %t365 = getField(%dtStats.gameStats[%val,"t",%game],9);
      setValueField(%dtStats,%val,"t",%game,9,%var + %t365);
   }
   for(%i = 1; %i <= $dtStats::FC["max"]; %i++){
      %newVal = getField($dtStats::FV[%i,"max"],1);
      
      %val = getField($dtStats::FV[%i,"max"],0);
      %var = %dtStats.gameStats[%val,"b",%game];
      
      %t1 = getField(%dtStats.gameStats[%newVal,"t",%game],1);
      %t7 = getField(%dtStats.gameStats[%newVal,"t",%game],3);
      %t30 = getField(%dtStats.gameStats[%newVal,"t",%game],5);
      %t90 = getField(%dtStats.gameStats[%newVal,"t",%game],7);
      %t365 = getField(%dtStats.gameStats[%newVal,"t",%game],9);
      
      if(%var > %t1){  setValueField(%dtStats,%newVal,"t",%game,1,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,1,%t1);}
      if(%var > %t7){  setValueField(%dtStats,%newVal,"t",%game,3,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,3,%t7);}
      if(%var > %t30){ setValueField(%dtStats,%newVal,"t",%game,5,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,5,%t30);}
      if(%var > %t90){ setValueField(%dtStats,%newVal,"t",%game,7,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,7,%t90);}
      if(%var > %t365){setValueField(%dtStats,%newVal,"t",%game,9,%var);}
      else{            setValueField(%dtStats,%newVal,"t",%game,9,%t365);}
   }
   for(%i = 1; %i <= $dtStats::FC["avg"]; %i++){
      %val = getField($dtStats::FV[%i,"avg"],0);
      %avgVal = getField($dtStats::FV[%i,"avg"],1);
      %var = getDynamicField(%dtStats.client,%val);
   
      %g1 = getField(%dtStats.gameStats["gameCount","t",%game],1); 
      %g7 = getField(%dtStats.gameStats["gameCount","t",%game],3);     
      %g30 = getField(%dtStats.gameStats["gameCount","t",%game],5);     
      %g90 = getField(%dtStats.gameStats["gameCount","t",%game],7);     
      %g365 = getField(%dtStats.gameStats["gameCount","t",%game],9);          
      %g1 = (%g1 ? %gi : 1);  %g7 = (%g7 ? %g7 : 1);  %g30 = (%g30 ? %g30 : 1); %g90 = (%g90 ? %g90 : 1); %g365 = (%g365 ? %g365 : 1); 
      
      %t1 = getField(%dtStats.gameStats[%val,"t",%game],1);
      setValueField(%dtStats,%avgVal,"t",%game,1, %t1 / %g1);
      %t7 = getField(%dtStats.gameStats[%val,"t",%game],3);
      setValueField(%dtStats,%avgVal,"t",%game,3, %t7 / %g7);
      %t30 = getField(%dtStats.gameStats[%val,"t",%game],5);
      setValueField(%dtStats,%avgVal,"t",%game,5, %t30 / %g30);
      %t90 = getField(%dtStats.gameStats[%val,"t",%game],7);
      setValueField(%dtStats,%avgVal,"t",%game,7, %t90 / %g90);
      %t365 = getField(%dtStats.gameStats[%val,"t",%game],9);
      setValueField(%dtStats,%avgVal,"t",%game,9, %t365 / %g365);
   }
}

function bakGameStats(%client,%game) {//back up clients current stats in case they come back
   if($dtStats::debugEchos){error("bakGameStats GUID = "  SPC %client.guid);}   
   for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
      %val = $dtStats::FV[%i,%game];
      %var = getDynamicField(%client,%val);
      %client.dtStats.gameStats[%val,"b",%game] = %var;
   }
   for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
      %val = $dtStats::FV[%i,"dtStats"];
      %var = getDynamicField(%client,%val);
      %client.dtStats.gameStats[%val,"b",%game] = %var;
   }
}

function resGameStats(%client,%game){// copy data back over to client
   if($dtStats::debugEchos){error("resGameStats GUID = "  SPC %client.guid);}
   for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
      %val = $dtStats::FV[%i,%game];
      %var = %client.dtStats.gameStats[%val,"b",%game];
      if(%val $= "winCount" || %val $= "lossCount"){
         %var = 0; // set to 0 becuase we came back and its not the end of the game
      }
      setDynamicField(%client,%val,%var);
   }
   for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
      %val = $dtStats::FV[%i,"dtStats"];
      %var = %client.dtStats.gameStats[%val,"b",%game];
      setDynamicField(%client,%val,%var);
   }
}

// resets stats that are used in this file
//the others are handled with in the gametype it self
function resetDtStats(%client,%game,%g){
   if($dtStats::debugEchos){error("resetDtStats GUID = "  SPC %client.guid);}
   for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
      %val = $dtStats::FV[%i,"dtStats"];
      setDynamicField(%client,%val,0);
   }
   for(%i = 1; %i <= $dtStats::uFC["dtStats"]; %i++){
      %val = $dtStats::uFV[%i,"dtStats"];
      setDynamicField(%client,%val,0);
   }
   if(%g){
      for(%i = 1; %i <= $dtStats::FC[%game]; %i++){
         %val = $dtStats::FV[%i,%game];
         setDynamicField(%client,%val,0);
      }
      for(%i = 1; %i <= $dtStats::uFC[%game]; %i++){
         %val = $dtStats::uFV[%i,%game];
         setDynamicField(%client,%val,0);
      }
   }
   %client.at =  %client.gt = 0;
}

////////////////////////////////////////////////////////////////////////////////
//Stats Collecting
////////////////////////////////////////////////////////////////////////////////
function clientKillStats(%game,%clVictim, %clKiller, %damageType, %implement, %damageLocation){
   if(%damageType == $DamageType::Explosion || %damageType == $DamageType::Ground ||
      %damageType == $DamageType::OutOfBounds ||  %damageType == $DamageType::Lava ||
      %damageType == $DamageType::VehicleSpawn || %damageType == $DamageType::ForceFieldPowerup || 
      %damageType == $DamageType::Lightning  ||   %damageType == $DamageType::NexusCamping){
      %clKiller = %clVictim.lastHitBy;
   }
   else if(!isObject(%clKiller) && isObject(%implement)){
      %clKiller = %implement.getControllingClient();  
   }
   %clVictim.lp = "";//last position for distMove
   
   %clVictim.ttl += getSimTime() - %clVictim.spawnTime;
   %clVictim.timeTL = mFloor((%clVictim.ttl/(%clVictim.deaths+%clVictim.suicides ? %clVictim.deaths+%clVictim.suicides : 1))/1000);
   
   if(%clKiller.team != %clVictim.team){
      
      if(isObject(%clKiller.player) && isObject(%clVictim.player)){
         %dis = vectorDist(%clKiller.player.getPosition(),%clVictim.player.getPosition());
         %vD = vectorSub(%clVictim.player.getVelocity(),%clKiller.player.getVelocity());
         %rvel = vectorLen(%vD);
         %victimVel =  vectorLen(%clVictim.player.getVelocity());
      }
      else{
        %rvel = 0;
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
      
         if(rayTest(%clVictim.player, 5)){%vcAir =1;}else{%vcAir =2;}
         if(rayTest(%clKiller.player, 5)){%kcAir =1;}else{%kcAir =2;}
      
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
      else{
         %kcAir = %vcAir = 0;   
      }
      
      if(%clVictim.EVDamageType && %clVictim.EVDamageType != %damageType){ // they were hit by something befor they were killed
         %clKiller.EVKillsWep++;
         %clVictim.EVDeathsWep++;
         if(rayTest(%clVictim.player, 5)){
            if(%clVictim.EVDamageType == $DamageType::Lightning){
               %clKiller.lightningMAkills++;
            }
            else{
               %clKiller.EVMA++;
            }
         }
         %clVictim.EVDamageType = 0;
      }
      
      if(getSimTime() - %clKiller.mKill < 300){
         %clKiller.multiKills++;
      } %clKiller.mKill =  getSimTime();
      
      if(getSimTime() - %clKiller.mCKill < 5000){
         %clKiller.chainKills++;
      } %clKiller.mCKill =  getSimTime();
      
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %clKiller.cgKills++;
            %clVictim.cgDeaths++;
            if(%clKiller.cgKillMaxDist < %dis){%clKiller.cgKillMaxDist = %dis;}
            if(%clKiller.cgKillRV < %rvel){%clKiller.cgKillRV = %rvel;}
            if(%clKiller.cgKillVV < %victimVel){%clKiller.cgKillVV = %victimVel;}
            if(%isCombo){%clKiller.cgCom++;}
            
            if(%kcAir == 1 && %vcAir == 1){%clKiller.cgKillAir++;%clVictim.cgDeathAir++;%clKiller.cgKillAirAir++;%clVictim.cgDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.cgKillAir++;%clVictim.cgDeathAir++;%clKiller.cgKillGroundAir++;%clVictim.cgDeathGroundAir++; }
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.cgKillGround++;%clVictim.cgDeathGround++;%clKiller.cgKillAirGround++;%clVictim.cgDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.cgKillGround++;%clVictim.cgDeathGround++;%clKiller.cgKillGroundGround++; %clVictim.cgDeathGroundGround++; }
         case $DamageType::Disc:
            %clKiller.discKills++;
            %clVictim.discDeaths++;
            if(%clKiller.discKillMaxDist < %dis){%clKiller.discKillMaxDist = %dis;}
            if(%clKiller.discKillRV < %rvel){%clKiller.discKillRV = %rvel;}
            if(%clKiller.discKillVV < %victimVel){%clKiller.discKillVV = %victimVel;} 
            if(%isCombo){%clKiller.discCom++;} 
            
            if(%kcAir == 1 && %vcAir == 1){%clKiller.discKillAir++;%clVictim.discDeathAir++;%clKiller.discKillAirAir++;%clVictim.discDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.discKillAir++;%clVictim.discDeathAir++;%clKiller.discKillGroundAir++;%clVictim.discDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.discKillGround++;%clVictim.discDeathGround++;%clKiller.discKillAirGround++;%clVictim.discDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.discKillGround++;%clVictim.discDeathGround++;%clKiller.discKillGroundGround++; %clVictim.discDeathGroundGround++;}
         case $DamageType::Grenade:
            if($dtObjExplode.dtNade){
               %clKiller.hGrenadeKills++;
               %clVictim.hGrenadeDeaths++;
               if(%clKiller.hGrenadeKillMaxDist < %dis){%clKiller.hGrenadeKillMaxDist = %dis;}
               if(%clKiller.hGrenadeKillRV < %rvel){%clKiller.hGrenadeKillRV = %rvel;}  
               if(%clKiller.hGrenadeKillVV < %victimVel){%clKiller.hGrenadeKillVV = %victimVel;}
               if(%isCombo){%clKiller.hGrenadeCom++;}
               if(%kcAir == 1 && %vcAir == 1){%clKiller.hGrenadeKillAir++;%clVictim.hGrenadeDeathAir++;%clKiller.hGrenadeKillAirAir++;%clVictim.hGrenadeDeathAirAir++;}
               else if(%kcAir == 2 && %vcAir == 1){%clKiller.hGrenadeKillAir++;%clVictim.hGrenadeDeathAir++;%clKiller.hGrenadeKillGroundAir++;%clVictim.hGrenadeDeathGroundAir++;}
               else if(%kcAir == 1 && %vcAir == 2){%clKiller.hGrenadeKillGround++;%clVictim.hGrenadeDeathGround++;%clKiller.hGrenadeKillAirGround++;%clVictim.hGrenadeDeathAirGround++;}
               else if(%kcAir == 2 && %vcAir == 2){%clKiller.hGrenadeKillGround++;%clVictim.hGrenadeDeathGround++;%clKiller.hGrenadeKillGroundGround++; %clVictim.hGrenadeDeathGroundGround++;}
            }
            else{
               %clKiller.grenadeKills++;
               %clVictim.grenadeDeaths++;
               if(%clKiller.grenadeKillMaxDist < %dis){%clKiller.grenadeKillMaxDist = %dis;}
               if(%clKiller.grenadeKillRV < %rvel){%clKiller.grenadeKillRV = %rvel;} 
               if(%clKiller.grenadeKillVV < %victimVel){%clKiller.grenadeKillVV = %victimVel;}
               if(%isCombo){%clKiller.grenadeCom++;}
               if(%kcAir == 1 && %vcAir == 1){%clKiller.grenadeKillAir++;%clVictim.grenadeDeathAir++;%clKiller.grenadeKillAirAir++;%clVictim.grenadeDeathAirAir++;}
               else if(%kcAir == 2 && %vcAir == 1){%clKiller.grenadeKillAir++;%clVictim.grenadeDeathAir++;%clKiller.grenadeKillGroundAir++;%clVictim.grenadeDeathGroundAir++;}
               else if(%kcAir == 1 && %vcAir == 2){%clKiller.grenadeKillGround++;%clVictim.grenadeDeathGround++;%clKiller.grenadeKillAirGround++;%clVictim.grenadeDeathAirGround++;}
               else if(%kcAir == 2 && %vcAir == 2){%clKiller.grenadeKillGround++;%clVictim.grenadeDeathGround++;%clKiller.grenadeKillGroundGround++; %clVictim.grenadeDeathGroundGround++;}
            }
         case $DamageType::Laser:
            %clKiller.laserKills++;
            %clVictim.laserDeaths++;
            if(%clKiller.laserKillMaxDist < %dis){%clKiller.laserKillMaxDist = %dis;}
            if(%clKiller.laserKillRV < %rvel){%clKiller.laserKillRV = %rvel;}
            if(%clKiller.laserKillVV < %victimVel){%clKiller.laserKillVV = %victimVel;}
            if(%isCombo){%clKiller.laserCom++;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.laserKillAir++;%clVictim.laserDeathAir++;%clKiller.laserKillAirAir++;%clVictim.laserDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.laserKillAir++;%clVictim.laserDeathAir++;%clKiller.laserKillGroundAir++;%clVictim.laserDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.laserKillGround++;%clVictim.laserDeathGround++;%clKiller.laserKillAirGround++;%clVictim.laserDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.laserKillGround++;%clVictim.laserDeathGround++;%clKiller.laserKillGroundGround++; %clVictim.laserDeathGroundGround++;}
         case $DamageType::Mortar:
            %clKiller.mortarKills++;
            %clVictim.mortarDeaths++;
            if(%clKiller.mortarKillMaxDist < %dis){%clKiller.mortarKillMaxDist = %dis;}
            if(%clKiller.mortarKillRV < %rvel){%clKiller.mortarKillRV = %rvel;}
            if(%clKiller.mortarKillVV < %victimVel){%clKiller.mortarKillVV = %victimVel;}
            if(%isCombo){%clKiller.mortarCom++;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.mortarKillAir++;%clVictim.mortarDeathAir++;%clKiller.mortarKillAirAir++;%clVictim.mortarDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.mortarKillAir++;%clVictim.mortarDeathAir++;%clKiller.mortarKillGroundAir++;%clVictim.mortarDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.mortarKillGround++;%clVictim.mortarDeathGround++;%clKiller.mortarKillAirGround++;%clVictim.mortarDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.mortarKillGround++;%clVictim.mortarDeathGround++;%clKiller.mortarKillGroundGround++; %clVictim.mortarDeathGroundGround++;}
         case $DamageType::Missile:
            %clKiller.missileKills++;
            %clVictim.missileDeaths++;
            if(%clKiller.missileKillMaxDist < %dis){%clKiller.missileKillMaxDist = %dis;}
            if(%clKiller.missileKillRV < %rvel){%clKiller.missileKillRV = %rvel;}
            if(%clKiller.missileKillVV < %victimVel){%clKiller.missileKillVV = %victimVel;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.missileKillAir++;%clVictim.missileDeathAir++;%clKiller.missileKillAirAir++;%clVictim.missileDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.missileKillAir++;%clVictim.missileDeathAir++;%clKiller.missileKillGroundAir++;%clVictim.missileDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.missileKillGround++;%clVictim.missileDeathGround++;%clKiller.missileKillAirGround++;%clVictim.missileDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.missileKillGround++;%clVictim.missileDeathGround++;%clKiller.missileKillGroundGround++; %clVictim.missileDeathGroundGround++;}
         case $DamageType::ShockLance:
            %clKiller.shockLanceKills++;
            %clVictim.shockLanceDeaths++;
            if(%clKiller.shockKillMaxDist < %dis){%clKiller.shockKillMaxDist = %dis;}
            if(%clKiller.shockKillRV < %rvel){%clKiller.shockKillRV = %rvel;}
            if(%clKiller.shockKillVV < %victimVel){%clKiller.shockKillVV = %victimVel;}
            if(%isCombo){%clKiller.shockCom++;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.shockKillAir++;%clVictim.shockDeathAir++;%clKiller.shockKillAirAir++;%clVictim.shockDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.shockKillAir++;%clVictim.shockDeathAir++;%clKiller.shockKillGroundAir++;%clVictim.shockDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.shockKillGround++;%clVictim.shockDeathGround++;%clKiller.shockKillAirGround++;%clVictim.shockDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.shockKillGround++;%clVictim.shockDeathGround++;%clKiller.shockKillGroundGround++; %clVictim.shockDeathGroundGround++;}
         case $DamageType::Plasma:
            %clKiller.plasmaKills++;
            %clVictim.plasmaDeaths++;
            if(%clKiller.plasmaKillMaxDist < %dis){%clKiller.plasmaKillMaxDist = %dis;}
            if(%clKiller.plasmaKillRV < %rvel){%clKiller.plasmaKillRV = %rvel;}
            if(%clKiller.plasmaKillVV < %victimVel){%clKiller.plasmaKillVV = %victimVel;}
            if(%isCombo){%clKiller.plasmaCom++;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.plasmaKillAir++;%clVictim.plasmaDeathAir++;%clKiller.plasmaKillAirAir++;%clVictim.plasmaDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.plasmaKillAir++;%clVictim.plasmaDeathAir++;%clKiller.plasmaKillGroundAir++;%clVictim.plasmaDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.plasmaKillGround++;%clVictim.plasmaDeathGround++;%clKiller.plasmaKillAirGround++;%clVictim.plasmaDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.plasmaKillGround++;%clVictim.plasmaDeathGround++;%clKiller.plasmaKillGroundGround++; %clVictim.plasmaDeathGroundGround++;}
         case $DamageType::Blaster:
            %clKiller.blasterKills++;
            %clVictim.blasterDeaths++;
            if(%clKiller.blasterKillMaxDist < %dis){%clKiller.blasterKillMaxDist = %dis;}
            if(%clKiller.blasterKillRV < %rvel){%clKiller.blasterKillRV = %rvel;}
            if(%clKiller.blasterKillVV < %victimVel){%clKiller.blasterKillVV = %victimVel;}
            if(%isCombo){%clKiller.blasterCom++;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.blasterKillAir++;%clVictim.blasterDeathAir++;%clKiller.blasterKillAirAir++;%clVictim.blasterDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.blasterKillAir++;%clVictim.blasterDeathAir++;%clKiller.blasterKillGroundAir++;%clVictim.blasterDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.blasterKillGround++;%clVictim.blasterDeathGround++;%clKiller.blasterKillAirGround++;%clVictim.blasterDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.blasterKillGround++;%clVictim.blasterDeathGround++;%clKiller.blasterKillGroundGround++; %clVictim.blasterDeathGroundGround++;}
         case $DamageType::ELF:
            %clKiller.elfKills++;
            %clVictim.elfDeaths++;
         case $DamageType::Mine:
            %clKiller.mineKills++;
            %clVictim.mineDeaths++;
            if(%clKiller.mineKillMaxDist < %dis){%clKiller.mineKillMaxDist = %dis;}
            if(%clKiller.mineKillRV < %rvel){%clKiller.mineKillRV = %rvel;}
            if(%clKiller.mineKillVV < %victimVel){%clKiller.mineKillVV = %victimVel;}
            if(%isCombo){%clKiller.mineCom++;}
            if(%kcAir == 1 && %vcAir == 1){%clKiller.mineKillAir++;%clVictim.mineDeathAir++;%clKiller.mineKillAirAir++;%clVictim.mineDeathAirAir++;}
            else if(%kcAir == 2 && %vcAir == 1){%clKiller.mineKillAir++;%clVictim.mineDeathAir++;%clKiller.mineKillGroundAir++;%clVictim.mineDeathGroundAir++;}
            else if(%kcAir == 1 && %vcAir == 2){%clKiller.mineKillGround++;%clVictim.mineDeathGround++;%clKiller.mineKillAirGround++;%clVictim.mineDeathAirGround++;}
            else if(%kcAir == 2 && %vcAir == 2){%clKiller.mineKillGround++;%clVictim.mineDeathGround++;%clKiller.mineKillGroundGround++; %clVictim.mineDeathGroundGround++;}
         case $DamageType::Explosion:
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.explosionKills++;}
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
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.groundKills++;}
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
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.outOfBoundKills++;}
            %clVictim.outOfBoundDeaths++;
         case $DamageType::Lava:
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.lavaKills++;}
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
         case $DamageType::Lightning:
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.lightningKills++;}
            %clVictim.lightningDeaths++;
         case $DamageType::VehicleSpawn:
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.vehicleSpawnKills++;}
            %clVictim.vehicleSpawnDeaths++;
         case $DamageType::ForceFieldPowerup:
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.forceFieldPowerUpKills++;}
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
            if(%clKiller){%clVictim.lastHitBy = 0;%clKiller.nexusCampingKills++;}
            %clVictim.nexusCampingDeaths++;
         }
      }
   }
function mdReset(%client){
 %client.md = 0;  
}
function evReset(%client){
 %client.EVDamageType = 0;  
}
function hitByReset(%client){
 %client.lastHitBy = 0;  
}
function rayTest(%targetObject,%dis){
   %client =  %targetObject.client;
   %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType;
   %rayStart = %targetObject.getWorldBoxCenter();
   %rayEnd = VectorAdd(%rayStart,"0 0" SPC (%dis * -1));
   %ground = !ContainerRayCast(%rayStart, %rayEnd, %mask, %targetObject);  
   return %ground; 
}
function clientDmgStats(%data,%position,%sourceObject, %targetObject, %damageType,%amount){
   if(%damageType == $DamageType::Explosion || %damageType == $DamageType::Ground ||
      %damageType == $DamageType::OutOfBounds ||  %damageType == $DamageType::Lava ||
      %damageType == $DamageType::VehicleSpawn || %damageType == $DamageType::ForceFieldPowerup || 
      %damageType == $DamageType::Lightning  ||   %damageType == $DamageType::NexusCamping){
      if(!%targetObject.client.EVDamageType){
         schedule(5000,0,"evReset",%targetObject.client);
      }
      %targetObject.client.EVDamageType = %damageType;   
      %targetObject.client.EVDamageCount++;
      return;
   } 
   %t = %s = 0;
   if(isObject(%sourceObject)){      
      if(%sourceObject.getClassName() !$= "Player"){
         %client = %sourceObject.getControllingClient(); 
         %s = 1;
      }
      else{
         %client = %sourceObject.client;
         %s = 1;
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
      }
   }
   if(%damageType > 0 && %sourceObject != %targetObject){
      if(%t && %s){
         if(%targetClient != %client && %targetClient.team != %client.team){
            %targetClient.lastHitBy = %client;
            schedule(3000,0,"hitByReset",%targetClient);
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
            %rvel = vectorLen(vectorSub(%targetObject.getVelocity(),%client.player.getVelocity()));
            %dis = vectorDist(%targetObject.getPosition(),%client.player.getPosition());
            if(%client.maxRV < %rvel){%client.maxRV = %rvel;} 
         }
      }
      %client.overallACC  = (%client.inDirectHits++ / (%client.shotsFired ? %client.shotsFired : 1)) * 100;
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Blaster:
            %client.blasterDmg += %amount;
            %client.blasterDirectHits++;
            %client.blasterACC =  (%client.blasterDirectHits / (%client.blasterShotsFired ? %client.blasterShotsFired : 1)) * 100;
            if(%client.blasterHitMaxDist < %dis){%client.blasterHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.blasterDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){%client.blasterMA++;}
            }
            
         case $DamageType::Plasma:
            %client.plasmaInDmg += %amount;
            %client.plasmaIndirectHits++;
            %client.plasmaACC = (%client.plasmaIndirectHits / (%client.plasmaShotsFired ? %client.plasmaShotsFired : 1)) * 100;
            if(%client.plasmaHitMaxDist < %dis){%client.plasmaHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.plasmaInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,PlasmaBolt.damageRadius+1)){%client.plasmaMA++;}
            }
         case $DamageType::Bullet:
            %client.cgDmg += %amount;
            %client.cgDirectHits++;
            %client.cgACC = (%client.cgDirectHits / (%client.cgShotsFired ? %client.cgShotsFired : 1)) * 100;
            if(%client.cgHitMaxDist < %dis){%client.cgHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.cgDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){%client.cgMA++;}
            }
         case $DamageType::Disc:
            %client.discInDmg += %amount;
            %client.discIndirectHits++;
            %client.discACC = (%client.discIndirectHits / (%client.discShotsFired ? %client.discShotsFired : 1)) * 100;
            if(%client.discHitMaxDist < %dis){%client.discHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.discInDmgTaken += %amount;
               if(%targetClient.md == 1){%client.minePlusDisc++;}
               %targetClient.md = 2;
               schedule(300,0,"mdReset",%targetClient);//mineDisc
               if(%targetClient != %client && rayTest(%targetObject,DiscProjectile.damageRadius+1)){%client.discMA++;}   
            }
         case $DamageType::Grenade:
            if($dtObjExplode.dtNade){
               %client.hGrenadeInDmg += %amount;
               %client.hGrenadeInHits++;
               %client.hGrenadeACC = (%client.hGrenadeInHits / (%client.hGrenadeShotsFired ? %client.hGrenadeShotsFired : 1)) * 100;
               if(%client.hGrenadeHitMaxDist < %dis){%client.hGrenadeHitMaxDist = %dis;}
               if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
               if(%t){
                  %targetClient.hGrenadeInDmgTaken += %amount;
                  if(%targetClient != %client && rayTest(%targetObject,GrenadeThrown.damageRadius+1)){%client.hGrenadeMA++;}
               }
            }
            else{
               %client.grenadeInDmg += %amount;
               %client.grenadeIndirectHits++;
               %client.grenadeACC = (%client.grenadeIndirectHits / (%client.grenadeShotsFired ? %client.grenadeShotsFired : 1)) * 100;
               if(%client.grenadeHitMaxDist < %dis){%client.grenadeHitMaxDist = %dis;}
               if(%t){
                  %targetClient.grenadeInDmgTaken += %amount;
                  if(%targetClient != %client && rayTest(%targetObject,BasicGrenade.damageRadius+1)){ %client.grenadeMA++;}
               }
            }   
         case $DamageType::Laser:
            if(%targetObject.getClassName() $= "Player"){
               %damLoc = %targetObject.getDamageLocation(%position);
               if(getWord(%damLoc,0) $= "head"){%client.laserHeadShot++;}
            }
            %client.laserDmg += %amount;
            %client.laserDirectHits++;
            %client.laserACC = (%client.laserDirectHits / (%client.laserShotsFired ? %client.laserShotsFired : 1)) * 100;
            if(%client.laserHitMaxDist < %dis){%client.laserHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.laserDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){%client.laserMA++;}
            }
         case $DamageType::Mortar:
            %client.mortarInDmg += %amount;
            %client.mortarIndirectHits++;
            %client.mortarACC = (%client.mortarIndirectHits / (%client.mortarShotsFired ? %client.mortarShotsFired : 1)) * 100;
            if(%client.mortarHitMaxDist < %dis){%client.mortarHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.mortarInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,MortarShot.damageRadius+2)){%client.mortarMA++;}
            }
         case $DamageType::Missile:
            %client.missileInDmg += %amount;
            %client.missileIndirectHits++;
            %client.missileACC = (%client.missileIndirectHits / (%client.missileShotsFired ? %client.missileShotsFired : 1)) * 100;
            if(%client.missileHitMaxDist < %dis){%client.missileHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.missileInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,ShoulderMissile.damageRadius+1)){%client.missileMA++;}            
            }
         case $DamageType::ShockLance:
            if(%targetClient.rearshot){%client.shockRearShot++;}
            %client.shockLanceInDmg += %amount;
            %client.shockLanceIndirectHits++;
            %client.shockACC = (%client.shockLanceIndirectHits / (%client.shockLanceShotsFired ? %client.shockLanceShotsFired : 1)) * 100;
            if(%client.shockHitMaxDist < %dis){%client.shockHitMaxDist = %dis;}
            if(%client.weaponHitMaxDist < %dis){%client.weaponHitMaxDist = %dis;}
            if(%t){
               %targetClient.shockLanceInDmgTaken += %amount;
               if(%targetClient != %client && rayTest(%targetObject,5)){ %client.shockMA++;}
            }
         case $DamageType::Mine:
            %client.mineInDmg += %amount;
            %client.mineIndirectHits++;
            %client.mineACC = (%client.mineIndirectHits / (%client.mineShotsFired ? %client.mineShotsFired : 1)) * 100;
            if(%client.mineHitMaxDist < %dis){%client.mineHitMaxDist = %dis;}
            if(%t){
               %targetClient.mineInDmgTaken += %amount;
               if(%targetClient.md == 2){
                  %client.minePlusDisc++; //discMine
               }
               %targetClient.md = 1;
               schedule(300,0,"mdReset",%targetClient);
               if(%targetClient != %client && rayTest(%targetObject,MineDeployed.damageRadius+1)){%client.mineMA++;}
            }
         case $DamageType::SatchelCharge:
            %client.SatchelInDmg += %amount;
            %client.SatchelInHits++;
            if(%t)
               %targetClient.SatchelInDmgTaken += %amount;
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
   if(!isObject(%client)){ return;}
   
   if(%data.directDamageType !$= ""){%damageType = %data.directDamageType;}
   else{%damageType =  %data.radiusDamageType;}
   %client.shotsFired++;
   %client.overallACC  = (%client.inDirectHits / (%client.shotsFired ? %client.shotsFired : 1)) * 100;
   switch$(%damageType){// list of all damage types to track see damageTypes.cs
      case $DamageType::Bullet:
         %client.cgShotsFired++;
         %client.cgACC = (%client.cgDirectHits / (%client.cgShotsFired ? %client.cgShotsFired : 1)) * 100;
      case $DamageType::Disc:
         %client.discShotsFired++;
         %client.discACC = (%client.discIndirectHits / (%client.discShotsFired ? %client.discShotsFired : 1)) * 100;
      case $DamageType::Grenade:
         %client.grenadeShotsFired++;
         %client.grenadeACC = (%client.grenadeIndirectHits / (%client.grenadeShotsFired ? %client.grenadeShotsFired : 1)) * 100;
      case $DamageType::Laser:
         %client.laserShotsFired++;
         %client.laserACC = (%client.laserDirectHits / (%client.laserShotsFired ? %client.laserShotsFired : 1)) * 100;
      case $DamageType::Mortar:
         %client.mortarShotsFired++;
         %client.mortarACC = (%client.mortarIndirectHits / (%client.mortarShotsFired ? %client.mortarShotsFired : 1)) * 100;
      case $DamageType::Missile:
         %client.missileShotsFired++;
         %client.missileACC = (%client.missileIndirectHits / (%client.missileShotsFired ? %client.missileShotsFired : 1)) * 100;
      case $DamageType::ShockLance:
         %client.shockLanceShotsFired++;
         %client.shockACC = (%client.shockLanceIndirectHits / (%client.shockLanceShotsFired ? %client.shockLanceShotsFired : 1)) * 100;
      case $DamageType::Plasma:
         %client.plasmaShotsFired++;
         %client.plasmaACC = (%client.plasmaIndirectHits / (%client.plasmaShotsFired ? %client.plasmaShotsFired : 1)) * 100;
      case $DamageType::Blaster:
         %client.blasterShotsFired++;
         %client.blasterACC = (%client.blasterDirectHits / (%client.blasterShotsFired ? %client.blasterShotsFired : 1)) * 100;
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
      %total = getField(%client.dtStats.gameStats[%value,"g",%game],%inc);
      if(%total !$= ""){
         return mFloatLength(%total,2) + 0;
      }
      else{
         return 0;
      } 
   }
   %c = 0;
   if(%client.dtStats.gameStats["totalGames","g",%game] != 0){
      for(%i=0; %i < $dtStats::MaxNumOfGames; %i++){
         %num = getField(%client.dtStats.gameStats[%value,"g",%game],%i);
         if(%num > 0 || %num < 0){
            %val += %num;
            %c++;
         }
      }
      if(%c > 0)
         return mCeil(%val / %c);
      else
         return 0;
   }
   else{
      return 0;
   }
}
function getGameRunWinLossAvg(%client,%game){
      %winCount = getField(%vClient.dtStats.gameStats["winCount","t",%game],9);
      %lossCount =getField(%vClient.dtStats.gameStats["lossCount","t",%game],9);
      %total = %winCount + %lossCount;
      return (%winCount / %total) * 100 SPC (%lossCount / %total) * 100;
}

function getGameTotalAvg(%vClient,%value,%game){
   if(getField(%vClient.dtStats.gameStats[%value,"t",%game],9) !$= "" && getField(%vClient.dtStats.gameStats["gameCount","t",%game],9) > 0)
      %totalAvg = getField(%vClient.dtStats.gameStats[%value,"t",%game],9) / getField(%vClient.dtStats.gameStats["gameCount","t",%game],9);
   else
      %totalAvg = 0;
   
   return mCeil(%totalAvg);
}
function numReduce(%num,%des){
   if(%num !$= ""){
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
   return 0; 
}
function getGameTotal(%vClient,%value,%game){
   %total = getField(%vClient.dtStats.gameStats[%value,"t",%game],9);
   if(%total !$= ""){
      return numReduce(%total,1);
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
   
   %client.lastPage = 0; 
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
                  if($dtStats::Armor)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tARMOR\t%1>  + CTF Armor Stats</a>',%vClient); 
                  if($dtStats::KD)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tKDA\t%1>  + CTF Kills/Deaths</a>',%vClient);
                  
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1\t1>  + Previous CTF Games</a>',%vClient);
                  if($dtStats::day > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tday-%2\t0>  + CTF Daily Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::week > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tweek-%2\t0>  + CTF Weekly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::month > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tmonth-%2\t0>  + CTF Monthly Leaderboards *Beta',%vClient,%game);
                  if($dtStats::quarter > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tquarter-%2\t0>  + CTF Quarterly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::year > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tyear-%2\t0>  + CTF Yearly Leaderboards *Beta</a>',%vClient,%game);
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
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1\t1>  + Previous Lak Games</a>',%vClient);
                  if($dtStats::day > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tday-%2\t0>  + LakRabbit Daily Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::week > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tweek-%2\t0>  + LakRabbit Weekly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::month > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tmonth-%2\t0>  + LakRabbit Monthly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::quarter > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tquarter-%2\t0>  + LakRabbit Quarterly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::year > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tyear-%2\t0>  + LakRabbit Yearly Leaderboards *Beta</a>',%vClient,%game);
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
                   messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1\t1>  + Previous Deathmatch Games</a>',%vClient);
                  if($dtStats::day > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tday-%2\t0>  + Deathmatch Daily Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::week > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tweek-%2\t0>  + Deathmatch Weekly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::month > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tmonth-%2\t0>  + Deathmatch Monthly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::quarter > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tquarter-%2\t0>  + Deathmatch Quarterly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::year > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tyear-%2\t0>  + Deathmatch Yearly Leaderboards *Beta</a>',%vClient,%game);
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
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1\t1>  + Previous Duel Mod Games</a>',%vClient);
                  if($dtStats::day > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tday-%2\t0>  + Duel Mod Daily Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::week > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tweek-%2\t0>  + Duel Mod Weekly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::month > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tmonth-%2\t0>  + Duel Mod Monthly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::quarter > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tquarter-%2\t0>  + Duel Mod Quarterly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::year > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tyear-%2\t0>  + Duel Mod Yearly Leaderboards *Beta</a>',%vClient,%game);
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
                  if($dtStats::KD)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tKDA\t%1>  + LCTF Kills/Deaths</a>',%vClient);
                  
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
                  if($dtStats::Hist)
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1\t1>  + Previous LCTF Games</a>',%vClient);
                  if($dtStats::day > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tday-%2\t0>  + LCTF Daily Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::week > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tweek-%2\t0>  + LCTF Weekly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::month > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tmonth-%2\t0>  + LCTF Monthly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::quarter > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tquarter-%2\t0>  + LCTF Quarterly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::year > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tyear-%2\t0>  + LCTF Yearly Leaderboards *Beta</a>',%vClient,%game);
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
                     messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tHISTORY\t%1\t1>  + Previous Arena Games</a>',%vClient);
                  if($dtStats::day > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tday-%2\t0>  + Arena Daily Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::week > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tweek-%2\t0>  + Arena Weekly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::month > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tmonth-%2\t0>  + Arena Monthly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::quarter > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tquarter-%2\t0>  + Arena Quarterly Leaderboards *Beta</a>',%vClient,%game);
                  if($dtStats::year > 1)
                      messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLBOARDS\t%1\tyear-%2\t0>  + Arena Yearly Leaderboards *Beta</a>',%vClient,%game);
               }
         }
         %m = 13 - %index;
         for(%v = 0; %v < %m; %v++){messageClient( %client, 'SetLineHud', "", %tag, %index++, "");}
         
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
           
      case "LakRabbitGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Suicides<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Suicides<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"suicides",%game,%inc),getGameTotal(%vClient,"suicides",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         %line1 = '<color:0befe7> Midairs<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mas",%game,%inc),getGameTotal(%vClient,"mas",%game),getGameTotalAvg(%vClient,"mas",%game),%vClient.mas);
         %line1 = '<color:0befe7> Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagGrabs",%game,%inc),getGameTotal(%vClient,"flagGrabs",%game),getGameTotalAvg(%vClient,"flagGrabs",%game),%vClient.flagGrabs);
         %line1 = '<color:0befe7> Midair Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midair Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"MidairflagGrabs",%inc,%game,%inc),getGameTotal(%vClient,"MidairflagGrabs",%game),getGameTotalAvg(%vClient,"MidairflagGrabs",%game),%vClient.MidairflagGrabs);
         %line1 = '<color:0befe7> Midair Flag Grab Points<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midair Flag Grab Points<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"MidairflagGrabPoints",%inc,%game,%inc),getGameTotal(%vClient,"MidairflagGrabPoints",%game),getGameTotalAvg(%vClient,"MidairflagGrabPoints",%game),%vClient.MidairflagGrabPoints);
         %line1 = '<color:0befe7> Flag Time Minutes<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Time Minutes<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagTimeMin",%game,%inc),getGameTotal(%vClient,"flagTimeMin",%game),getGameTotalAvg(%vClient,"flagTimeMin",%game),%vClient.flagTimeMin);
         %line1 = '<color:0befe7> Bonus Points<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Bonus Points<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"morepoints",%game,%inc),getGameTotal(%vClient,"morepoints",%game),getGameTotalAvg(%vClient,"morepoints",%game),%vClient.morepoints);
         %line1 = '<color:0befe7> Mine + Disc<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minedisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Total Distance<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Total Distance<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"totalDistance",%game,%inc),getGameTotal(%vClient,"totalDistance",%game),getGameTotalAvg(%vClient,"totalDistance",%game),%vClient.totalDistance);
         %line1 = '<color:0befe7> Total Shock Hits<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Total Shock Hits<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"totalShockHits",%game,%inc),getGameTotal(%vClient,"totalShockHits",%game),getGameTotalAvg(%vClient,"totalShockHits",%game),%vClient.totalShockHits);
         %line1 = '<color:0befe7> Total Shocks<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Total Shocks<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"totalShocks",%game,%inc),getGameTotal(%vClient,"totalShocks",%game),getGameTotalAvg(%vClient,"totalShocks",%game),%vClient.totalShocks);
      case "DMGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Suicides<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Suicides<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"suicides",%game,%inc),getGameTotal(%vClient,"suicides",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         

      case "ArenaGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Suicides<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Suicides<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"suicides",%game,%inc),getGameTotal(%vClient,"suicides",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         %line1 = '<color:0befe7> Team Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Team Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"teamKills",%game,%inc),getGameTotal(%vClient,"teamKills",%game),getGameTotalAvg(%vClient,"teamKills",%game),%vClient.teamKills);
         %line1 = '<color:0befe7> Snipe Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Snipe Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"snipeKills",%game,%inc),getGameTotal(%vClient,"snipeKills",%game),getGameTotalAvg(%vClient,"roundsWon",%game),%vClient.roundsWon);
         %line1 = '<color:0befe7> Rounds Won<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Rounds Won<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"roundsWon",%game,%inc),getGameTotal(%vClient,"roundsWon",%game),getGameTotalAvg(%vClient,"suicides",%game),%vClient.suicides);
         %line1 = '<color:0befe7> Rounds Lost<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Rounds Lost<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"roundsLost",%game,%inc),getGameTotal(%vClient,"roundsLost",%game),getGameTotalAvg(%vClient,"roundsLost",%game),%vClient.roundsLost);
         %line1 = '<color:0befe7> Assists<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Assists<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"assists",%game,%inc),getGameTotal(%vClient,"assists",%game),getGameTotalAvg(%vClient,"assists",%game),%vClient.assists);
         %line1 = '<color:0befe7> Round Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Round Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"roundKills",%game,%inc),getGameTotal(%vClient,"roundKills",%game),getGameTotalAvg(%vClient,"roundKills",%game),%vClient.roundKills);
         %line1 = '<color:0befe7> Hat Tricks<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hat Tricks<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hatTricks",%game,%inc),getGameTotal(%vClient,"hatTricks",%game),getGameTotalAvg(%vClient,"hatTricks",%game),%vClient.hatTricks);
         
      case "DuelGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Score<<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
                 
      case "CTFGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = "<color:0befe7><lmargin:180>Stats<lmargin:310>Totals<lmargin:440>Total Avg";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         else{//Default
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getTaggedString(%vClient.name) @ "'s Match Stats");
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
            %header = '<color:0befe7><lmargin:180>Live<lmargin:270>Moving Avg<lmargin:370>Totals<lmargin:470>Total Avg';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         }
         %line1 = '<color:0befe7> Win %6<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Win %6<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         
         %wlPCT =  getGameRunWinLossAvg(%client,%game);
         %runAvg = mFloor(getWord(%wlPCT,0)) @ "%";
         
         %winTotal = getGameTotal(%vClient,"winCount",%game);
         %lossTotal = getGameTotal(%vClient,"lossCount",%game);
         %total  = %winTotal SPC "W /" SPC %lossTotal SPC "L";
         
         %totalWinLoss = %winTotal +  %lossTotal;
         %totalAvg = mFloor((%winTotal / %totalWinLoss)* 100) @ "%";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%runAvg,%total,%totalAvg,%vClient.winCount,"%");
         
         %line1 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Mid-Air<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mid-Air<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreMidAir",%game,%inc),getGameTotal(%vClient,"scoreMidAir",%game),getGameTotalAvg(%vClient,"scoreMidAir",%game),%vClient.scoreMidAir);
         %line1 = '<color:0befe7> Mine + Disc<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minePlusDisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Flag Caps<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Caps<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagCaps",%game,%inc),getGameTotal(%vClient,"flagCaps",%game),getGameTotalAvg(%vClient,"flagCaps",%game),%vClient.flagCaps);
         %line1 = '<color:0befe7> Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagGrabs",%game,%inc),getGameTotal(%vClient,"flagGrabs",%game),getGameTotalAvg(%vClient,"flagGrabs",%game),%vClient.flagGrabs);
         %line1 = '<color:0befe7> Carrier Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Carrier Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"carrierKills",%game,%inc),getGameTotal(%vClient,"carrierKills",%game),getGameTotalAvg(%vClient,"carrierKills",%game),%vClient.carrierKills);
         %line1 = '<color:0befe7> Flag Returns<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Returns<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagReturns",%game,%inc),getGameTotal(%vClient,"flagReturns",%game),getGameTotalAvg(%vClient,"flagReturns",%game),%vClient.flagReturns);
         %line1 = '<color:0befe7> Flag Defends<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Defends<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagDefends",%game,%inc),getGameTotal(%vClient,"flagDefends",%game),getGameTotalAvg(%vClient,"flagDefends",%game),%vClient.flagDefends);
         %line1 = '<color:0befe7> Offense Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Offense Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"offenseScore",%game,%inc),getGameTotal(%vClient,"offenseScore",%game),getGameTotalAvg(%vClient,"offenseScore",%game),%vClient.offenseScore);
         %line1 = '<color:0befe7> Defense Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Defense Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"defenseScore",%game,%inc),getGameTotal(%vClient,"defenseScore",%game),getGameTotalAvg(%vClient,"defenseScore",%game),%vClient.defenseScore);
         %line1 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.score);
         %line1 = '<color:0befe7> Backshots<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Backshots<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreRearshot",%game,%inc),getGameTotal(%vClient,"scoreRearshot",%game),getGameTotalAvg(%vClient,"scoreRearshot",%game),%vClient.scoreRearshot);
         %line1 = '<color:0befe7> Headshots<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Headshots<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreHeadshot",%game,%inc),getGameTotal(%vClient,"scoreHeadshot",%game),getGameTotalAvg(%vClient,"scoreHeadshot",%game),%vClient.scoreHeadshot);
      
      case "SCtFGame":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
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
         
         %line1 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"kills",%game,%inc),getGameTotal(%vClient,"kills",%game),getGameTotalAvg(%vClient,"kills",%game),%vClient.kills);
         %line1 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"deaths",%game,%inc),getGameTotal(%vClient,"deaths",%game),getGameTotalAvg(%vClient,"deaths",%game),%vClient.deaths);
         %line1 = '<color:0befe7> Mid-Air<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mid-Air<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreMidAir",%game,%inc),getGameTotal(%vClient,"scoreMidAir",%game),getGameTotalAvg(%vClient,"scoreMidAir",%game),%vClient.scoreMidAir);
         %line1 = '<color:0befe7> Mine + Disc<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minePlusDisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Flag Caps<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Caps<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagCaps",%game,%inc),getGameTotal(%vClient,"flagCaps",%game),getGameTotalAvg(%vClient,"flagCaps",%game),%vClient.flagCaps);
         %line1 = '<color:0befe7> Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Grabs<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagGrabs",%game,%inc),getGameTotal(%vClient,"flagGrabs",%game),getGameTotalAvg(%vClient,"flagGrabs",%game),%vClient.flagGrabs);
         %line1 = '<color:0befe7> Carrier Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Carrier Kills<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"carrierKills",%game,%inc),getGameTotal(%vClient,"carrierKills",%game),getGameTotalAvg(%vClient,"carrierKills",%game),%vClient.carrierKills);
         %line1 = '<color:0befe7> Flag Returns<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Returns<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagReturns",%game,%inc),getGameTotal(%vClient,"flagReturns",%game),getGameTotalAvg(%vClient,"flagReturns",%game),%vClient.flagReturns);
         %line1 = '<color:0befe7> Flag Defends<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Flag Defends<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"flagDefends",%game,%inc),getGameTotal(%vClient,"flagDefends",%game),getGameTotalAvg(%vClient,"flagDefends",%game),%vClient.flagDefends);
         %line1 = '<color:0befe7> Offense Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Offense Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"offenseScore",%game,%inc),getGameTotal(%vClient,"offenseScore",%game),getGameTotalAvg(%vClient,"offenseScore",%game),%vClient.offenseScore);
         %line1 = '<color:0befe7> Defense<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Defense<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"defenseScore",%game,%inc),getGameTotal(%vClient,"defenseScore",%game),getGameTotalAvg(%vClient,"defenseScore",%game),%vClient.defenseScore);
         %line1 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Score<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"score",%game,%inc),getGameTotal(%vClient,"score",%game),getGameTotalAvg(%vClient,"score",%game),%vClient.wildCrash);
         %line1 = '<color:0befe7> Backshots<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Backshots<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreRearshot",%game,%inc),getGameTotal(%vClient,"scoreRearshot",%game),getGameTotalAvg(%vClient,"scoreRearshot",%game),%vClient.scoreRearshot);
         %line1 = '<color:0befe7> Headshots<font:univers condensed:18><color:33CCCC><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Headshots<font:univers condensed:18><color:33CCCC><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"scoreHeadshot",%game,%inc),getGameTotal(%vClient,"scoreHeadshot",%game),getGameTotalAvg(%vClient,"scoreHeadshot",%game),%vClient.scoreHeadshot);
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
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
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
                  %line =  '<lmargin:10><color:0befe7>%4 - %2<color:02d404> - Overwritten<color:0befe7><lmargin:350><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%timeDate,%v,%map,%game); 
               }
               else{
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%v);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%v);
                  %line = '<lmargin:10><color:0befe7>%4 - %2<lmargin:350><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%timeDate,%v,%map,%game);  
               }
            }
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
		    if(%page == 1){
               %line = '<color:0befe7></a><lmargin:200><just:right><a:gamelink\tStats\tHISTORY\t%1\t%2>Next</a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page + 1);
            }
            else if(%page * %perPage > $dtStats::MaxNumOfGames){
               %line = '<color:0befe7></a><lmargin:200><just:right><a:gamelink\tStats\tHISTORY\t%1\t%2>Previous</a>';
               messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1);
            }
            else if(%page > 1){
               %line = '<color:0befe7><lmargin:200><just:right><a:gamelink\tStats\tHISTORY\t%1\t%2>Previous</a> | <a:gamelink\tStats\tHISTORY\t%1\t%3>Next</a>';
                messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1,%page + 1);
            }
         }
         else{
            if(%vClient.dtStats.gameStats["statsOverWrite","g",%game] > 9){
               if(%page == 1){
                  %line = '<color:0befe7></a><lmargin:300><a:gamelink\tStats\tHISTORY\t%1\t%2>Next</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page + 1);
               }
               else if(%page * %perPage > %vClient.dtStats.gameStats["statsOverWrite","g",%game]){
                  %line = '<color:0befe7></a><lmargin:300><a:gamelink\tStats\tHISTORY\t%1\t%2>Previous</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1);
               }
               else if(%page > 1){
                  %line = '<color:0befe7><lmargin:250><a:gamelink\tStats\tHISTORY\t%1\t%2>Previous</a> | <lmargin:300><a:gamelink\tStats\tHISTORY\t%1\t%3>Next</a>';
                   messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%page - 1,%page + 1);
               }
               %gc = %vClient.dtStats.gameStats["statsOverWrite","g",%game];
               for(%z = (%page - 1) * %perPage; %z < %page * %perPage && %z <= %gc; %z++){
                  %v = %gc - %z;//temp fix just inverts it becuase.... im lazy 
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%v);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%v);
                  %line = '<lmargin:10><color:0befe7>%4 - %2<lmargin:350><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%timeDate,%v,%map,%game);
               }
            }
            else{
                for(%z =%vClient.dtStats.gameStats["statsOverWrite","g",%game]; %z >= 0; %z--){
                  %timeDate = getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%z);
                  %map = getField(%vClient.dtStats.gameStats["map","g",%game],%z);
                  %line = '<lmargin:10><color:0befe7>%4 - %2<lmargin:350><a:gamelink\tStats\t%5\t%1\t%3> + Match</a><lmargin:400><a:gamelink\tStats\tWEAPON\t%1\t%3> + Weapon</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,%line,%vClient,%timeDate,%z,%map,%game);
               }
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
         %a1 = getGameTotal(%vClient,"groundKills",%game); %b2 = getGameTotal(%vClient,"groundDeaths",%game); %c3 = getGameTotal(%vClient,"turretKills",%game);
         %d4 = getGameTotal(%vClient,"turretDeaths",%game); %e5 = getGameTotal(%vClient,"plasmaTurretKills",%game); %f6 = getGameTotal(%vClient,"plasmaTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7>  Ground: <color:33CCCC>%1:%2<color:0befe7><lmargin:200>Turret: <color:33CCCC>%3:%4<color:0befe7><lmargin:395>Plasma Turret: <color:33CCCC>%5:%6<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"aaTurretKills",%game); %b2 = getGameTotal(%vClient,"aaTurretDeaths",%game); %c3 = getGameTotal(%vClient,"elfTurretKills",%game);
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
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tHISTORY\t%1\t0>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient);
         
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
            //messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats" @ getField(%vClient.dtStats.gameStats["map","g",%game],%inc) SPC getField(%vClient.dtStats.gameStats["timeDayMonth","g",%game],%inc));
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
         %cycle = %client.GlArg5;
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Live Stats");
         if(%inc $= "pin"){
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tStats\tLIVE\t%1\t-1>Unpin Screen</a> - Games Played: %2',%vClient,%vClient.dtStats.gameStats["totalGames","g",%game]);
         }
         else{
            messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tStats\tLIVE\t%1\tpin>Pin Screen</a> - Games Played: %2',%vClient,%vClient.dtStats.gameStats["totalGames","g",%game]);
         }
         //%i1=%i2=%i3=%i4=%i5=%i6=%i7=%i8=%i9=0;  
         //%line = '<color:0befe7>  PastGames<lmargin:100>%1<lmargin:150>%2<lmargin:200>%3<lmargin:250>%4<lmargin:300>%5<lmargin:350>%6<lmargin:400>%7<lmargin:450>%8<lmargin:500>%9';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7,%i8,%i9);  
         
         %i1 = "<color:0befe7><font:univers condensed:18>Score:<color:03d597>" SPC %vClient.score; 
         %i2 = "<color:0befe7><font:univers condensed:18>Kills:<color:03d597>" SPC %vClient.kills;
         %i3 = "<color:0befe7><font:univers condensed:18>Deaths:<color:03d597>" SPC %vClient.deaths; 
         %i4 = "<color:0befe7><font:univers condensed:18>Assists:<color:03d597>" SPC %vClient.assist;
         %line = '  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);         
         
         %i1 = "<color:0befe7><font:univers condensed:18>KDR:<color:03d597>" SPC kdr(%vClient.kills,%vClient.deaths) @ "%"; 
         %i2 = "<color:0befe7><font:univers condensed:18>KillStreak:<color:03d597>" SPC %vClient.killStreak;
         %i3 = "<color:0befe7><font:univers condensed:18>MineDisc:<color:03d597>" SPC %vClient.minePlusDisc;
         %i4 = %vClient.plasmaMA + %vClient.discMA + %vClient.mineMA + %vClient.grenadeMA + %vClient.hGrenadeMA + %vClient.mortarMA + %vClient.shockMA + %vClient.laserMA +
         %vClient.laserHeadShot + %vClient.shockRearShot + %vClient.comboPT + %vClient.assist +
         (%vClient.plasmaKillMaxDist/500) + (%vClient.discKillMaxDist/500) + (%vClient.mineKillMaxDist/200) + (%vClient.grenadeKillMaxDist/300) + (%vClient.hGrenadeKillMaxDist/200) + (%vClient.mortarKillMaxDist/200)+
         (%vClient.plasmaKillRV/100) + (%vClient.discKillRV/100) + (%vClient.mineKillRV/100) + (%vClient.grenadeKillRV/100) + (%vClient.hGrenadeKillRV/100) + (%vClient.mortarKillRV/100) + (%vClient.shockKillRV/50) + (%vClient.laserKillRV/100);
         %i4 = "<color:0befe7><font:univers condensed:18>Shot Rating:<color:03d597>" SPC mFloatLength(%i4/26,2) + 0; //
         %line = '  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';         
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);


         %dmg =  %vClient.blasterDmg + %vClient.plasmaInDmg + %vClient.grenadeInDmg + %vClient.hGrenadeInDmg + %vClient.cgDmg + 
         %vClient.discInDmg + %vClient.laserDmg + %vClient.mortarInDmg + %vClient.missileInDmg + %vClient.shockLanceInDmg + %vClient.mineInDmg;
         %i1 = "<color:0befe7><font:univers condensed:18>Damage:<color:03d597>" SPC numReduce(%dmg,1);
         %i2 = "<color:0befe7><font:univers condensed:18>Speed:<color:03d597>" SPC  mFloatLength(%vClient.avgSpeed,1) + 0;
         %i3 = "<color:0befe7><font:univers condensed:18>Shots Fired:<color:03d597>" SPC numReduce(%vClient.shotsFired,2); //"RelSpeed:" SPC mFloatLength(%vClient.maxRV,1)+0;
         %i4 = "<color:0befe7><font:univers condensed:18>Dist Moved:<color:03d597>" SPC numReduce(%vClient.distMov,1); // %vClient.dtStats.gameStats["totalGames","g",%game];
         %line = '  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);
         
         %i1 = "<color:0befe7><font:univers condensed:18>Lt Kills:<color:03d597>" SPC %vClient.armorL;
         %i2 = "<color:0befe7><font:univers condensed:18>Med Kills:<color:03d597>" SPC %vClient.armorM;
         %i3 = "<color:0befe7><font:univers condensed:18>Hvy Kills:<color:03d597>"SPC %vClient.armorH;
         %i4 = "<color:0befe7><font:univers condensed:18>Survival:<color:03d597>" SPC secToMinSec(%vClient.timeTL);   
         %line = '  <lmargin:0>%1<lmargin:145>%2<lmargin:290>%3<lmargin:435>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4);

         messageClient( %client, 'SetLineHud', "", %tag, %index++, ""); 

         %header = '<color:0befe7>  Weapon<lmargin:140>K:D<lmargin:212>MidAirs<lmargin:284>Accuracy<lmargin:356>Combos<lmargin:428>Speed<lmargin:500>MaxDis';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %i1=%i2=%i3=%i4=%i5=%i6=%i7=0;    
         %i1 = %vClient.blasterKills @ ":" @ %vClient.blasterDeaths;
         %i2 = %vClient.blasterMA;
         %i3 = mFloatLength(%vClient.blasterACC,1) + 0 @ "%";   
         %i4 = %vClient.blasterCom;
         %i5 = mFloatLength(%vClient.blasterKillRV,1)+0;           
         %i6 = mCeil(%vClient.blasterKillMaxDist) @ "m";
         %line = '<color:0befe7>  Blaster<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.plasmaKills @ ":" @ %vClient.plasmaDeaths;
         %i2 = %vClient.plasmaMA;   
         %i3 = mFloatLength(%vClient.plasmaACC,1) + 0 @ "%";
         %i4 = %vClient.plasmaCom;  
         %i5 = mFloatLength(%vClient.plasmaKillRV,1)+0;          
         %i6 = mCeil(%vClient.plasmaKillMaxDist) @ "m";
         %line = '<color:0befe7>  Plasma Rifle<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.cgKills @ ":" @ %vClient.cgDeaths;
         %i2 = %vClient.cgMA;
         %i3 = mFloatLength(%vClient.cgACC,1) + 0 @ "%";
         %i4 = %vClient.cgCom;    
         %i5 = mFloatLength(%vClient.cgKillRV,1)+0;           
         %i6 = mCeil(%vClient.cgKillMaxDist) @ "m";     
         %line = '<color:0befe7>  Chaingun<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.discKills @ ":" @ %vClient.discDeaths;
         %i2 = %vClient.discMA; 
         %i3 = mFloatLength(%vClient.discACC,1) + 0 @ "%";
         %i4 =  %vClient.discCom;  
         %i5 = mFloatLength(%vClient.discKillRV,1)+0;           
         %i6 = mCeil(%vClient.discKillMaxDist) @ "m";
         %line = '<color:0befe7>  Spinfusor<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.grenadeKills @ ":" @ %vClient.grenadeDeaths;
         %i2 = %vClient.grenadeMA; 
         %i3 = mFloatLength(%vClient.grenadeACC,1) + 0 @ "%";
         %i4 = %vClient.grenadeCom; 
         %i5 = mFloatLength(%vClient.grenadeKillRV,1)+0;           
         %i6 = mCeil(%vClient.grenadeKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Grenade Launcher<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.laserKills @ ":" @ %vClient.laserDeaths;
         %i2 = %vClient.laserMA;
         %i3 = mFloatLength(%vClient.laserACC,1) + 0 @ "%";   
         %i4 = %vClient.laserCom;
         %i5 = mFloatLength(%vClient.laserKillRV,1)+0;           
         %i6 = mCeil(%vClient.laserKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Laser Rifle<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.mortarKills @ ":" @ %vClient.mortarDeaths;
         %i2 = %vClient.mortarMA;  
         %i3 = mFloatLength(%vClient.mortarACC,1) + 0 @ "%";
         %i4 = %vClient.mortarCom;
         %i5 = mFloatLength(%vClient.mortarKillRV,1)+0;           
         %i6 = mCeil(%vClient.mortarKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Fusion Mortar<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.missileKills @ ":" @ %vClient.missileDeaths;
         %i2 =  %vClient.missileMA;  
         %i3 = mFloatLength(%vClient.missileACC,1) + 0 @ "%";         
         %i4 = %vClient.missileCom;
         %i5 = mFloatLength(%vClient.missileShotsFired,1)+0;           
         %i6 = mCeil(%vClient.missileKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Missile Launcher<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.shockLanceKills @ ":" @ %vClient.shockLanceDeaths;
         %i2 = %vClient.shockMA;
         %i3 = mFloatLength(%vClient.shockACC,1) + 0 @ "%";   
         %i4 = %vClient.shockCom;
         %i5 = mFloatLength(%vClient.shockKillRV,1)+0;           
         %i6 =  mCeil(%vClient.shockKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Shocklance<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7);
         %i1 = %vClient.mineKills @ ":" @ %vClient.mineDeaths;
         %i2 =  %vClient.mineMA;  
         %i3 = mFloatLength(%vClient.mineACC,1) + 0 @ "%";          
         %i4 = %vClient.mineCom;
         %i5 = mFloatLength(%vClient.mineKillRV,1)+0;        
         %i6 = mCeil(%vClient.mineKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Mine<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7); 
         %i1 = %vClient.hGrenadeKills @ ":" @ %vClient.hGrenadeDeaths;
         %i2 =  %vClient.hGrenadeMA;  
         %i3 = mFloatLength(%vClient.hGrenadeACC,1) + 0 @ "%";           
         %i4 = %vClient.hGrenadeCom;
         %i5 = mFloatLength(%vClient.hGrenadeKillRV,1)+0;           
         %i6 = mCeil(%vClient.hGrenadeKillMaxDist) @ "m";         
         %line = '<color:0befe7>  Hand Grenade<color:33CCCC><font:univers condensed:18><lmargin:140>%1<lmargin:212>%2<lmargin:284>%3<lmargin:356>%4<lmargin:428>%5<lmargin:500>%6';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,%i4,%i5,%i6,%i7); 
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
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterKills",%game,%inc),getGameTotal(%vClient,"blasterKills",%game),getGameTotalAvg(%vClient,"blasterKills",%game),%vClient.blasterKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDeaths",%game,%inc),getGameTotal(%vClient,"blasterDeaths",%game),getGameTotalAvg(%vClient,"blasterDeaths",%game),%vClient.blasterDeaths);
         %line1 = '<color:0befe7> Direct Damage<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDmg",%game,%inc),getGameTotal(%vClient,"blasterDmg",%game),getGameTotalAvg(%vClient,"blasterDmg",%game),mFloatLength(%vClient.blasterDmg,2)+0);
         %line1 = '<color:0befe7> Direct Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDmgTaken",%game,%inc),getGameTotal(%vClient,"blasterDmgTaken",%game),getGameTotalAvg(%vClient,"blasterDmgTaken",%game),mFloatLength(%vClient.blasterDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterDirectHits",%game,%inc),getGameTotal(%vClient,"blasterDirectHits",%game),getGameTotalAvg(%vClient,"blasterDirectHits",%game),%vClient.blasterDirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterShotsFired",%game,%inc),getGameTotal(%vClient,"blasterShotsFired",%game),getGameTotalAvg(%vClient,"blasterShotsFired",%game),%vClient.blasterShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterKillMaxDist",%game,%inc),getGameTotal(%vClient,"blasterKillMaxDist",%game),getGameTotalAvg(%vClient,"blasterKillMaxDist",%game),mFloatLength(%vClient.blasterKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterKillRV",%game,%inc),getGameTotal(%vClient,"blasterKillRV",%game),getGameTotalAvg(%vClient,"blasterKillRV",%game),mFloatLength(%vClient.blasterKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterCom",%game,%inc),getGameTotal(%vClient,"blasterCom",%game),getGameTotalAvg(%vClient,"blasterCom",%game),%vClient.blasterCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterMA",%game,%inc),getGameTotal(%vClient,"blasterMA",%game),getGameTotalAvg(%vClient,"blasterMA",%game),%vClient.blasterMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"blasterACC",%game,%inc),getGameTotal(%vClient,"blasterACC",%game),getGameTotalAvg(%vClient,"blasterACC",%game),mFloatLength(%vClient.blasterACC,2)+0);
      case "Spinfusor":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discKills",%game,%inc),getGameTotal(%vClient,"discKills",%game),getGameTotalAvg(%vClient,"discKills",%game),%vClient.discKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discDeaths",%game,%inc),getGameTotal(%vClient,"discDeaths",%game),getGameTotalAvg(%vClient,"discDeaths",%game),%vClient.discDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discInDmg",%game,%inc),getGameTotal(%vClient,"discInDmg",%game),getGameTotalAvg(%vClient,"discInDmg",%game),mFloatLength(%vClient.discInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discInDmgTaken",%game,%inc),getGameTotal(%vClient,"discInDmgTaken",%game),getGameTotalAvg(%vClient,"discInDmgTaken",%game),mFloatLength(%vClient.discInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discIndirectHits",%game,%inc),getGameTotal(%vClient,"discIndirectHits",%game),getGameTotalAvg(%vClient,"discIndirectHits",%game),%vClient.discIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discShotsFired",%game,%inc),getGameTotal(%vClient,"discShotsFired",%game),getGameTotalAvg(%vClient,"discShotsFired",%game),%vClient.discShotsFired);
         %line1 = '<color:0befe7> Mine + Disc<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mine + Disc<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"minePlusDisc",%game,%inc),getGameTotal(%vClient,"minePlusDisc",%game),getGameTotalAvg(%vClient,"minePlusDisc",%game),%vClient.minePlusDisc);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discKillMaxDist",%game,%inc),getGameTotal(%vClient,"discKillMaxDist",%game),getGameTotalAvg(%vClient,"discKillMaxDist",%game),mFloatLength(%vClient.discKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discKillRV",%game,%inc),getGameTotal(%vClient,"discKillRV",%game),getGameTotalAvg(%vClient,"discKillRV",%game),mFloatLength(%vClient.discKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discCom",%game,%inc),getGameTotal(%vClient,"discCom",%game),getGameTotalAvg(%vClient,"discCom",%game),%vClient.discCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discMA",%game,%inc),getGameTotal(%vClient,"discMA",%game),getGameTotalAvg(%vClient,"discMA",%game),%vClient.discMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"discACC",%game,%inc),getGameTotal(%vClient,"discACC",%game),getGameTotalAvg(%vClient,"discACC",%game),mFloatLength(%vClient.discACC,2)+0);
      case "Chaingun":
          %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgKills",%game,%inc),getGameTotal(%vClient,"cgKills",%game),getGameTotalAvg(%vClient,"cgKills",%game),%vClient.cgKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDeaths",%game,%inc),getGameTotal(%vClient,"cgDeaths",%game),getGameTotalAvg(%vClient,"cgDeaths",%game),%vClient.cgDeaths);
         %line1 = '<color:0befe7> Direct Damage<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDmg",%game,%inc),getGameTotal(%vClient,"cgDmg",%game),getGameTotalAvg(%vClient,"cgDmg",%game),mFloatLength(%vClient.cgDmg,2)+0);
         %line1 = '<color:0befe7> Direct Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDmgTaken",%game,%inc),getGameTotal(%vClient,"cgDmgTaken",%game),getGameTotalAvg(%vClient,"cgDmgTaken",%game),mFloatLength(%vClient.cgDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgDirectHits",%game,%inc),getGameTotal(%vClient,"cgDirectHits",%game),getGameTotalAvg(%vClient,"cgDirectHits",%game),%vClient.cgDirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgShotsFired",%game,%inc),getGameTotal(%vClient,"cgShotsFired",%game),getGameTotalAvg(%vClient,"cgShotsFired",%game),%vClient.cgShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgKillMaxDist",%game,%inc),getGameTotal(%vClient,"cgKillMaxDist",%game),getGameTotalAvg(%vClient,"cgKillMaxDist",%game),mFloatLength(%vClient.cgKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgKillRV",%game,%inc),getGameTotal(%vClient,"cgKillRV",%game),getGameTotalAvg(%vClient,"cgKillRV",%game),mFloatLength(%vClient.cgKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgCom",%game,%inc),getGameTotal(%vClient,"cgCom",%game),getGameTotalAvg(%vClient,"cgCom",%game),%vClient.cgCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgMA",%game,%inc),getGameTotal(%vClient,"cgMA",%game),getGameTotalAvg(%vClient,"cgMA",%game),%vClient.cgMA);  
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"cgACC",%game,%inc),getGameTotal(%vClient,"cgACC",%game),getGameTotalAvg(%vClient,"cgACC",%game),mFloatLength(%vClient.cgACC,2)+0);
      case "GrenadeLauncher":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeKills",%game,%inc),getGameTotal(%vClient,"grenadeKills",%game),getGameTotalAvg(%vClient,"grenadeKills",%game),%vClient.grenadeKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeDeaths",%game,%inc),getGameTotal(%vClient,"grenadeDeaths",%game),getGameTotalAvg(%vClient,"grenadeDeaths",%game),%vClient.grenadeDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeInDmg",%game,%inc),getGameTotal(%vClient,"grenadeInDmg",%game),getGameTotalAvg(%vClient,"grenadeInDmg",%game),mFloatLength(%vClient.grenadeInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeInDmgTaken",%game,%inc),getGameTotal(%vClient,"grenadeInDmgTaken",%game),getGameTotalAvg(%vClient,"grenadeInDmgTaken",%game),mFloatLength(%vClient.grenadeInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeIndirectHits",%game,%inc),getGameTotal(%vClient,"grenadeIndirectHits",%game),getGameTotalAvg(%vClient,"grenadeIndirectHits",%game),%vClient.grenadeIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeShotsFired",%game,%inc),getGameTotal(%vClient,"grenadeShotsFired",%game),getGameTotalAvg(%vClient,"grenadeShotsFired",%game),%vClient.grenadeShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeKillMaxDist",%game,%inc),getGameTotal(%vClient,"grenadeKillMaxDist",%game),getGameTotalAvg(%vClient,"grenadeKillMaxDist",%game),mFloatLength(%vClient.grenadeKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeKillRV",%game,%inc),getGameTotal(%vClient,"grenadeKillRV",%game),getGameTotalAvg(%vClient,"grenadeKillRV",%game),mFloatLength(%vClient.grenadeKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeCom",%game,%inc),getGameTotal(%vClient,"grenadeCom",%game),getGameTotalAvg(%vClient,"grenadeCom",%game),%vClient.grenadeCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeMA",%game,%inc),getGameTotal(%vClient,"grenadeMA",%game),getGameTotalAvg(%vClient,"grenadeMA",%game),%vClient.grenadeMA);      
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"grenadeACC",%game,%inc),getGameTotal(%vClient,"grenadeACC",%game),getGameTotalAvg(%vClient,"grenadeACC",%game),mFloatLength(%vClient.grenadeACC,2)+0);
      case "LaserRifle":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserKills",%game,%inc),getGameTotal(%vClient,"laserKills",%game),getGameTotalAvg(%vClient,"laserKills",%game),%vClient.laserKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDeaths",%game,%inc),getGameTotal(%vClient,"laserDeaths",%game),getGameTotalAvg(%vClient,"laserDeaths",%game),%vClient.laserDeaths);
         %line1 = '<color:0befe7> Direct Damage<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDmg",%game,%inc),getGameTotal(%vClient,"laserDmg",%game),getGameTotalAvg(%vClient,"laserDmg",%game),mFloatLength(%vClient.laserDmg,2)+0);
         %line1 = '<color:0befe7> Direct Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDmgTaken",%game,%inc),getGameTotal(%vClient,"laserDmgTaken",%game),getGameTotalAvg(%vClient,"laserDmgTaken",%game),mFloatLength(%vClient.laserDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserDirectHits",%game,%inc),getGameTotal(%vClient,"laserDirectHits",%game),getGameTotalAvg(%vClient,"laserDirectHits",%game),%vClient.laserDirectHits);
         %line1 = '<color:0befe7> Shots Fired <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserShotsFired",%game,%inc),getGameTotal(%vClient,"laserShotsFired",%game),getGameTotalAvg(%vClient,"laserShotsFired",%game),%vClient.laserShotsFired);
         %line1 = '<color:0befe7> Headshots<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Headshots<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserHeadShot",%game,%inc),getGameTotal(%vClient,"laserHeadShot",%game),getGameTotalAvg(%vClient,"laserHeadShot",%game),%vClient.laserHeadShot);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserKillMaxDist",%game,%inc),getGameTotal(%vClient,"laserKillMaxDist",%game),getGameTotalAvg(%vClient,"laserKillMaxDist",%game),mFloatLength(%vClient.laserKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserKillRV",%game,%inc),getGameTotal(%vClient,"laserKillRV",%game),getGameTotalAvg(%vClient,"laserKillRV",%game),mFloatLength(%vClient.laserKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserCom",%game,%inc),getGameTotal(%vClient,"laserCom",%game),getGameTotalAvg(%vClient,"laserCom",%game),%vClient.laserCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserMA",%game,%inc),getGameTotal(%vClient,"laserMA",%game),getGameTotalAvg(%vClient,"laserMA",%game),%vClient.laserMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"laserACC",%game,%inc),getGameTotal(%vClient,"laserACC",%game),getGameTotalAvg(%vClient,"laserACC",%game),mFloatLength(%vClient.laserACC,2)+0);
      case "FusionMortar":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarKills",%game,%inc),getGameTotal(%vClient,"mortarKills",%game),getGameTotalAvg(%vClient,"mortarKills",%game),%vClient.mortarKills);
         %line1 = '<color:0befe7> Deaths <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarDeaths",%game,%inc),getGameTotal(%vClient,"mortarDeaths",%game),getGameTotalAvg(%vClient,"mortarDeaths",%game),%vClient.mortarDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarInDmg",%game,%inc),getGameTotal(%vClient,"mortarInDmg",%game),getGameTotalAvg(%vClient,"mortarInDmg",%game),mFloatLength(%vClient.mortarInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarInDmgTaken",%game,%inc),getGameTotal(%vClient,"mortarInDmgTaken",%game),getGameTotalAvg(%vClient,"mortarInDmgTaken",%game),mFloatLength(%vClient.mortarInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarIndirectHits",%game,%inc),getGameTotal(%vClient,"mortarIndirectHits",%game),getGameTotalAvg(%vClient,"mortarIndirectHits",%game),%vClient.mortarIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarShotsFired",%game,%inc),getGameTotal(%vClient,"mortarShotsFired",%game),getGameTotalAvg(%vClient,"mortarShotsFired",%game),%vClient.mortarShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarKillMaxDist",%game,%inc),getGameTotal(%vClient,"mortarKillMaxDist",%game),getGameTotalAvg(%vClient,"mortarKillMaxDist",%game),mFloatLength(%vClient.mortarKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarKillRV",%game,%inc),getGameTotal(%vClient,"mortarKillRV",%game),getGameTotalAvg(%vClient,"mortarKillRV",%game),mFloatLength(%vClient.mortarKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarCom",%game,%inc),getGameTotal(%vClient,"mortarCom",%game),getGameTotalAvg(%vClient,"mortarCom",%game),%vClient.mortarCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarMA",%game,%inc),getGameTotal(%vClient,"mortarMA",%game),getGameTotalAvg(%vClient,"mortarMA",%game),%vClient.mortarMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mortarACC",%game,%inc),getGameTotal(%vClient,"mortarACC",%game),getGameTotalAvg(%vClient,"mortarACC",%game),mFloatLength(%vClient.mortarACC,2)+0);
      case "MissileLauncher":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileKills",%game,%inc),getGameTotal(%vClient,"missileKills",%game),getGameTotalAvg(%vClient,"missileKills",%game),%vClient.missileKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileDeaths",%game,%inc),getGameTotal(%vClient,"missileDeaths",%game),getGameTotalAvg(%vClient,"missileDeaths",%game),%vClient.missileDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileInDmg",%game,%inc),getGameTotal(%vClient,"missileInDmg",%game),getGameTotalAvg(%vClient,"missileInDmg",%game),mFloatLength(%vClient.missileInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileInDmgTaken",%game,%inc),getGameTotal(%vClient,"missileInDmgTaken",%game),getGameTotalAvg(%vClient,"missileInDmgTaken",%game),mFloatLength(%vClient.missileInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileIndirectHits",%game,%inc),getGameTotal(%vClient,"missileIndirectHits",%game),getGameTotalAvg(%vClient,"missileIndirectHits",%game),mFloatLength(%vClient.missileIndirectHits,2)+0);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileShotsFired",%game,%inc),getGameTotal(%vClient,"missileShotsFired",%game),getGameTotalAvg(%vClient,"missileShotsFired",%game),%vClient.missileShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileKillMaxDist",%game,%inc),getGameTotal(%vClient,"missileKillMaxDist",%game),getGameTotalAvg(%vClient,"missileKillMaxDist",%game),mFloatLength(%vClient.missileKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileKillRV",%game,%inc),getGameTotal(%vClient,"missileKillRV",%game),getGameTotalAvg(%vClient,"missileKillRV",%game),mFloatLength(%vClient.missileKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileCom",%game,%inc),getGameTotal(%vClient,"missileCom",%game),getGameTotalAvg(%vClient,"missileCom",%game),%vClient.missileCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileMA",%game,%inc),getGameTotal(%vClient,"missileMA",%game),getGameTotalAvg(%vClient,"missileMA",%game),%vClient.missileMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"missileACC",%game,%inc),getGameTotal(%vClient,"missileACC",%game),getGameTotalAvg(%vClient,"missileACC",%game),mFloatLength(%vClient.missileACC,2)+0);
      case "Shocklance":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats");
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
         %line1 = '<color:0befe7> Kills <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceKills",%game,%inc),getGameTotal(%vClient,"shockLanceKills",%game),getGameTotalAvg(%vClient,"shockLanceKills",%game),%vClient.shockLanceKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceDeaths",%game,%inc),getGameTotal(%vClient,"shockLanceDeaths",%game),getGameTotalAvg(%vClient,"shockLanceDeaths",%game),%vClient.shockLanceDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceInDmg",%game,%inc),getGameTotal(%vClient,"shockLanceInDmg",%game),getGameTotalAvg(%vClient,"shockLanceInDmg",%game),mFloatLength(%vClient.shockLanceInDmg,20)+0);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceInDmgTaken",%game,%inc),getGameTotal(%vClient,"shockLanceInDmgTaken",%game),getGameTotalAvg(%vClient,"shockLanceInDmgTaken",%game),mFloatLength(%vClient.shockLanceInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Direct Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Direct Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceIndirectHits",%game,%inc),getGameTotal(%vClient,"shockLanceIndirectHits",%game),getGameTotalAvg(%vClient,"shockLanceIndirectHits",%game),%vClient.shockLanceIndirectHits);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockLanceShotsFired",%game,%inc),getGameTotal(%vClient,"shockLanceShotsFired",%game),getGameTotalAvg(%vClient,"shockLanceShotsFired",%game),%vClient.shockLanceShotsFired);
         %line1 = '<color:0befe7> Backshots<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Backshots<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockRearShot",%game,%inc),getGameTotal(%vClient,"shockRearShot",%game),getGameTotalAvg(%vClient,"shockRearShot",%game),%vClient.shockRearShot);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockKillMaxDist",%game,%inc),getGameTotal(%vClient,"shockKillMaxDist",%game),getGameTotalAvg(%vClient,"shockKillMaxDist",%game),mFloatLength(%vClient.shockKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockKillRV",%game,%inc),getGameTotal(%vClient,"shockKillRV",%game),getGameTotalAvg(%vClient,"shockKillRV",%game),mFloatLength(%vClient.shockKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockCom",%game,%inc),getGameTotal(%vClient,"shockCom",%game),getGameTotalAvg(%vClient,"shockCom",%game),%vClient.shockCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockMA",%game,%inc),getGameTotal(%vClient,"shockMA",%game),getGameTotalAvg(%vClient,"shockMA",%game),%vClient.shockMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"shockACC",%game,%inc),getGameTotal(%vClient,"shockACC",%game),getGameTotalAvg(%vClient,"shockACC",%game),mFloatLength(%vClient.shockACC,2)+0);
      case "PlasmaRifle":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats");
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
         %line1 = '<color:0befe7> Kills <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaKills",%game,%inc),getGameTotal(%vClient,"plasmaKills",%game),getGameTotalAvg(%vClient,"plasmaKills",%game),%vClient.plasmaKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaDeaths",%game,%inc),getGameTotal(%vClient,"plasmaDeaths",%game),getGameTotalAvg(%vClient,"plasmaDeaths",%game),%vClient.plasmaDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaInDmg",%game,%inc),getGameTotal(%vClient,"plasmaInDmg",%game),getGameTotalAvg(%vClient,"plasmaInDmg",%game),mFloatLength(%vClient.plasmaInDmg,2)+0);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaInDmgTaken",%game,%inc),getGameTotal(%vClient,"plasmaInDmgTaken",%game),getGameTotalAvg(%vClient,"plasmaInDmgTaken",%game),mFloatLength(%vClient.plasmaInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaIndirectHits",%game,%inc),getGameTotal(%vClient,"plasmaIndirectHits",%game),getGameTotalAvg(%vClient,"plasmaIndirectHits",%game),mFloatLength(%vClient.plasmaIndirectHits,2)+0);
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaShotsFired"),getGameTotal(%vClient,"plasmaShotsFired",%game),getGameTotalAvg(%vClient,"plasmaShotsFired",%game),%vClient.plasmaShotsFired);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaKillMaxDist",%game,%inc),getGameTotal(%vClient,"plasmaKillMaxDist",%game),getGameTotalAvg(%vClient,"plasmaKillMaxDist",%game),mFloatLength(%vClient.plasmaKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaKillRV",%game,%inc),getGameTotal(%vClient,"plasmaKillRV",%game),getGameTotalAvg(%vClient,"plasmaKillRV",%game),mFloatLength(%vClient.plasmaKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaCom",%game,%inc),getGameTotal(%vClient,"plasmaCom",%game),getGameTotalAvg(%vClient,"plasmaCom",%game),%vClient.plasmaCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaMA",%game,%inc),getGameTotal(%vClient,"plasmaMA",%game),getGameTotalAvg(%vClient,"plasmaMA",%game),%vClient.plasmaMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"plasmaACC",%game,%inc),getGameTotal(%vClient,"plasmaACC",%game),getGameTotalAvg(%vClient,"plasmaACC",%game),mFloatLength(%vClient.plasmaACC,2)+0);
      case "ELF":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats");
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
         %line1 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Shots Fired<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"elfShotsFired",%game,%inc),getGameTotal(%vClient,"elfShotsFired",%game),getGameTotalAvg(%vClient,"elfShotsFired",%game),%vClient.elfShotsFired);
      case "Mine":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Mine Stats");
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
         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineKills",%game,%inc),getGameTotal(%vClient,"mineKills",%game),getGameTotalAvg(%vClient,"mineKills",%game),%vClient.mineKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineDeaths",%game,%inc),getGameTotal(%vClient,"mineDeaths",%game),getGameTotalAvg(%vClient,"mineDeaths",%game),%vClient.mineDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineInDmg",%game,%inc),getGameTotal(%vClient,"mineInDmg",%game),getGameTotalAvg(%vClient,"mineInDmg",%game),mFloatLength(%vClient.mineInDmg,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineIndirectHits",%game,%inc),getGameTotal(%vClient,"mineIndirectHits",%game),getGameTotalAvg(%vClient,"mineIndirectHits",%game),%vClient.mineIndirectHits);
         %line1 = '<color:0befe7> Mines Thrown<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Mines Thrown<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineShotsFired",%game,%inc),getGameTotal(%vClient,"mineShotsFired",%game),getGameTotalAvg(%vClient,"mineShotsFired",%game),%vClient.mineShotsFired);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineInDmgTaken",%game,%inc),getGameTotal(%vClient,"mineInDmgTaken",%game),getGameTotalAvg(%vClient,"mineInDmgTaken",%game),mFloatLength(%vClient.mineInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineKillMaxDist",%game,%inc),getGameTotal(%vClient,"mineKillMaxDist",%game),getGameTotalAvg(%vClient,"mineKillMaxDist",%game),mFloatLength(%vClient.mineKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineKillRV",%game,%inc),getGameTotal(%vClient,"mineKillRV",%game),getGameTotalAvg(%vClient,"mineKillRV",%game),mFloatLength(%vClient.mineKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineCom",%game,%inc),getGameTotal(%vClient,"mineCom",%game),getGameTotalAvg(%vClient,"mineCom",%game),%vClient.mineCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineMA",%game,%inc),getGameTotal(%vClient,"mineMA",%game),getGameTotalAvg(%vClient,"mineMA",%game),%vClient.mineMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"mineACC",%game,%inc),getGameTotal(%vClient,"mineACC",%game),getGameTotalAvg(%vClient,"mineACC",%game),mFloatLength(%vClient.mineACC,2)+0);
      case "HandGrenade":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Hand Grenade Stats");
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

         %line1 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeKills",%game,%inc),getGameTotal(%vClient,"hGrenadeKills",%game),getGameTotalAvg(%vClient,"hGrenadeKills",%game),%vClient.hGrenadeKills);
         %line1 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeDeaths",%game,%inc),getGameTotal(%vClient,"hGrenadeDeaths",%game),getGameTotalAvg(%vClient,"hGrenadeDeaths",%game),%vClient.hGrenadeDeaths);
         %line1 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Dealt<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeInDmg",%game,%inc),getGameTotal(%vClient,"hGrenadeInDmg",%game),getGameTotalAvg(%vClient,"hGrenadeInDmg",%game),mFloatLength(%vClient.hGrenadeInDmg,2)+0);
         %line1 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeInHits",%game,%inc),getGameTotal(%vClient,"hGrenadeInHits",%game),getGameTotalAvg(%vClient,"hGrenadeInHits",%game),%vClient.hGrenadeInHits);
         %line1 = '<color:0befe7> Grenades Thrown<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Grenades Thrown<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeShotsFired",%game,%inc),getGameTotal(%vClient,"hGrenadeShotsFired",%game),getGameTotalAvg(%vClient,"hGrenadeShotsFired",%game),%vClient.hGrenadeShotsFired);
         %line1 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeInDmgTaken",%game,%inc),getGameTotal(%vClient,"hGrenadeInDmgTaken",%game),getGameTotalAvg(%vClient,"hGrenadeInDmgTaken",%game),mFloatLength(%vClient.hGrenadeInDmgTaken,2)+0);
         %line1 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Max Distance<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeKillMaxDist",%game,%inc),getGameTotal(%vClient,"hGrenadeKillMaxDist",%game),getGameTotalAvg(%vClient,"hGrenadeKillMaxDist",%game),mFloatLength(%vClient.hGrenadeKillMaxDist,2)+0);
         %line1 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Relative Velocity<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeKillRV",%game,%inc),getGameTotal(%vClient,"hGrenadeKillRV",%game),getGameTotalAvg(%vClient,"hGrenadeKillRV",%game),mFloatLength(%vClient.hGrenadeKillRV,2)+0);
         %line1 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Weapon Combos<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeCom",%game,%inc),getGameTotal(%vClient,"hGrenadeCom",%game),getGameTotalAvg(%vClient,"hGrenadeCom",%game),%vClient.hGrenadeCom);
         %line1 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Midairs<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeMA",%game,%inc),getGameTotal(%vClient,"hGrenadeMA",%game),getGameTotalAvg(%vClient,"hGrenadeMA",%game),%vClient.hGrenadeMA);
         %line1 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Accuracy<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"hGrenadeACC",%game,%inc),getGameTotal(%vClient,"hGrenadeACC",%game),getGameTotalAvg(%vClient,"hGrenadeACC",%game),mFloatLength(%vClient.hGrenadeACC,2)+0);
      case "SatchelCharge":
         %inc = %client.GlArg4;
         if(%inc != -1){//History
            messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Satchel Charge Stats");
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
         %line1 = '<color:0befe7> Kills <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Kills <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"satchelChargeKills",%game,%inc),getGameTotal(%vClient,"satchelChargeKills",%game),getGameTotalAvg(%vClient,"satchelChargeKills",%game),%vClient.satchelChargeKills);
         %line1 = '<color:0befe7> Deaths <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Deaths <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"satchelChargeDeaths",%game,%inc),getGameTotal(%vClient,"satchelChargeDeaths",%game),getGameTotalAvg(%vClient,"satchelChargeDeaths",%game),%vClient.satchelChargeDeaths);
         %line1 = '<color:0befe7> Splash Damage <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Splash Damage <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"SatchelInDmg",%game,%inc),getGameTotal(%vClient,"SatchelInDmg",%game),getGameTotalAvg(%vClient,"SatchelInDmg",%game),mFloatLength(%vClient.SatchelInDmg,2)+0);
         %line1 = '<color:0befe7> Hits <color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Hits <color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"SatchelInHits",%game,%inc),getGameTotal(%vClient,"SatchelInHits",%game),getGameTotalAvg(%vClient,"SatchelInHits",%game),%vClient.SatchelInHits);
         %line1 = '<color:0befe7> Splash Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%5<lmargin:270>%2<lmargin:370>%3<lmargin:470>%4';
         %line2 = '<color:0befe7> Splash Damage Taken<color:33CCCC><font:univers condensed:18><lmargin:180>%2<lmargin:310>%3<lmargin:440>%4';         
         %line = (%inc != -1) ? %line2 : %line1;
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameData(%vClient,"SatchelInDmgTaken",%game,%inc),getGameTotal(%vClient,"SatchelInDmgTaken",%game),getGameTotalAvg(%vClient,"SatchelInDmgTaken",%game),mFloatLength(%vClient.SatchelInDmgTaken,2)+0);
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
            if(%client.isSuperAdmin){
               messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a><just:right><a:gamelink\tStats\tVARLIST\t%1\t1>VarList </a>',%vClient,%lType);
            }
            else{
               messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%lType);
  
            }
            %header = '<color:0befe7> <lmargin:50># <lmargin:65>%2<lmargin:200>Score<lmargin:320>Weapons<lmargin:460>Score';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%vClient,$dtStats::gtNameLong[%client.lgame]);
            
            for(%i = 0; %i < 10; %i++){
               %scoreName  = getField($lData::name["score",%client.lgame,%lType,%mon,%year],%i);
               %gameScore  = getField($lData::data["score",%client.lgame,%lType,%mon,%year],%i);
               %wepName  = getField($lData::name["weaponScore",%client.lgame,%lType,%mon,%year],%i);
               %wepScore  = getField($lData::data["weaponScore",%client.lgame,%lType,%mon,%year],%i);
               if(%gameScore){
                  %line = '<font:univers condensed:18> <lmargin:50>%3. <lmargin:65><color:0befe7><clip:138>%1</clip><color:03d597><lmargin:200>%4<lmargin:320><color:0befe7><clip:138>%2</clip><color:03d597><lmargin:460>%5';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%scoreName,%wepName,%i+1,%gameScore,mFloor(%wepScore+0.5));
               }
               else{
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
               }
            }
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            
            %line = '<just:center><color:0befe7><a:gamelink\tStats\tGLBOARDS\t%1>View More %2 Categories</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,$dtStats::gtNameLong[%client.lgame]);
            %line = '<just:center><color:0befe7><a:gamelink\tStats\tWLBOARDS\t%1>View More Weapons Categories</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
            %hasCount = 0;  %line = "";
            for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
               if($lData::hasData[%lType,$dtStats::gameType[%i]] && $dtStats::gameType[%i] !$= %client.lgame){
                  %hasCount++; 
                  %line = %line @ "<a:gamelink\tStats\tLBOARDS\t" @ %vClient @ "\t" @ %lType @ "-" @  $dtStats::gameType[%i] @ "\t0>[" @ $dtStats::gtNameShort[$dtStats::gameType[%i]]  @ "] </a>"; 
               }
            }
            //error(%client.lgame SPC %game SPC %hasCount );
            if(%hasCount > 0){
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Switch Game Type" SPC %line);
            }
            else{
               messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            }
            
            
			   messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            //error(%mon SPC %page SPC $lData::monCount[%client.lgame,%lType]);
            if($lData::monCount[%client.lgame,%lType] > 1){
               if(%page == 1){
                   %line = '<just:center>Click on category to view more<just:right><color:0befe7><a:gamelink\tStats\tLBOARDS\t%1\t%2-%4\t%3>Previous</a>';
                   messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%lType,%page+1,%client.lgame); 
               }
               else if(%page >= $lData::monCount[%client.lgame,%lType]){
                  %line = '<just:center>Click on top category to view more<just:right><color:0befe7><a:gamelink\tStats\tLBOARDS\t%1\t%2-%4\t%3>Next</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%lType,%page-1,%client.lgame);
               }
               else{
                  %line = '<just:center>Click on category to view more<just:right><a:gamelink\tStats\tLBOARDS\t%1\t%2-%5\t%3>Next</a> | <color:0befe7><a:gamelink\tStats\tLBOARDS\t%1\t%2-%5\t%4>Previous</a>';
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%lType,%page-1,%page+1,%client.lgame); 
               }
            }
            else{
               %line = '<just:center>Click on category to view more';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
            }
         }
         else{//no data for selected game type
            %header = '<color:0befe7><just:center>No data at this time, check in 24 hours';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%i1,%i2,%i3,%i4++,%i5,%i6,%i7); 
            
            %hasCount = 0;  %line = "";
            for(%i = 0; %i < $dtStats::gameTypeCount; %i++){
               if($lData::hasData[%lType,$dtStats::gameType[%i]] && $dtStats::gameType[%i] !$= %client.lgame){
                  %hasCount++; 
                  %line = %line @ "<a:gamelink\tStats\tLBOARDS\t" @ %vClient @ "\t" @ %lType @ "-" @  $dtStats::gameType[%i] @ "\t0>" @ $dtStats::gtNameShort[$dtStats::gameType[%i]]  @ " </a>"; 
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
		 switch$(%client.lgame)
		 {
			case "CTFGame":
				%gametype = "CTF";
			case "SCtfGame":
				%gametype = "LCTF";
			case "LakRabbitGame":
				%gametype = "Lak";
			case "DMGame":
				%gametype = "DM";
			case "ArenaGame":
				%gametype = "Arena";
			case "DuelGame":
				%gametype = "Duel";
		 }
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %gametype SPC "Greatest Hits");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType,%page,%client.lgame);
            //exec("scripts/autoexec/zDarktigerStats.cs");
			if(%client.lgame $= "CTFGame" || %client.lgame $= "SCtFGame"){ 
			%line = "<color:0befe7><just:center>" @ %gametype SPC "Specific";
			messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);    

			%i1 = getField($lData::data["winCount",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["winCount",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["destruction",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["destruction",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Win Count: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Destruction Count: <color:03d597>%2</a>';
			messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"winCount-Win Count-Total","destruction-Destruction Count-Total",%vClient);

			%i1 = getField($lData::data["offenseScore",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["offenseScore",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["defenseScore",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["defenseScore",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Offense Score: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Defense Score: <color:03d597>%2</a>';
			messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"offenseScore-Offense Score-Total","defenseScore-Defense Score-Total",%vClient);
			
			%i1 = getField($lData::data["flagDefends",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["flagDefends",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["flagReturns",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["flagReturns",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Flag Defends: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Flag Returns: <color:03d597>%2</a>';
			messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"flagDefends-Flag Defends-Total","flagReturns-Flag Returns-Total",%vClient);
			
			%i1 = getField($lData::data["flagCaps",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["flagCaps",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["flagGrabs",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["flagGrabs",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Flags Caps: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Flag Grabs: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"flagCaps-Flag Caps-Total","flagGrabs-Flag Grabs-Total",%vClient);
            
			%i1 = getField($lData::data["carrierKills",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["carrierKills",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["escortAssists",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["escortAssists",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Carrier Kills: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Escort Assists: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"carrierKills-Carrier Kills-Total","escortAssists-Escort Assists-Total",%vClient);
         }
         else if(%client.lgame $= "LakRabbitGame"){  
            %line = "<color:0befe7><just:center>" @ %gametype SPC "Specific";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
			
			%i1 = getField($lData::data["flagGrabs",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["flagGrabs",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["flagTimeMin",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["flagTimeMin",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Flag Grabs: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Flag Time: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"flagGrabs-Flag Grabs-Total","flagTimeMin-Flag Time-Total Minutes",%vClient);

            %i1 = getField($lData::data["MidairflagGrabs",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["MidairflagGrabs",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["mas",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["mas",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Midair Flag Grabs: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Midairs Hits: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"MidairflagGrabs-Midair Flag Grabs-Total","mas-Midairs Hits-Total",%vClient);   
         }
            %line = "<color:0befe7><just:center>" @ %gametype SPC "Misc";
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line);
            
            %i1 = getField($lData::data["Kills",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["Kills",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["killStreakMax",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["killStreakMax",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Total Kills: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Kill Streak: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"kills-Total Kills-Total","killStreakMax-Kill Streak-Amount",%vClient);
           
            %i1 = getField($lData::data["scoreAVG",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["scoreAVG",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["scoreMax",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["scoreMax",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Highest Score: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Score Average: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"scoreAVG-Score Average-Amount","scoreMax-Highest Score-Amount",%vClient);
		        
            %i1 = getField($lData::data["EVKills",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["EVKills",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["overallACCAVG",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["overallACCAVG",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Environmental Kills: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Overall Accuracy: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"EVKills-Enviro Assisted Kills-Total","overallACCAVG-Overall Accuracy-Percentage",%vClient);
            
            %i1 = getField($lData::data["timeTLAVG",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["timeTLAVG",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["distMov",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["distMov",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Avg Survival Time: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Distance Traveled: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"timeTLAVG-Average Survival Time-Seconds","distMov-Distance Traveled-Amount In Meters",%vClient);
            
			%i1 = getField($lData::data["airTime",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["airTime",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["groundTime",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["groundTime",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Air Time: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Ground Time: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"airTime-Air Time-EST Minutes ","groundTime-Ground Time-EST Minutes",%vClient);
			
			%i1 = getField($lData::data["killAir",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["killAir",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["killGround",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["killGround",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Midair Kills: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Ground Kills: <color:03d597>%2</a>';
			messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"killAir-Midair Kills-Total","killGround-Ground Kills-Total",%vClient);
      
			%i1 = getField($lData::data["totalMA",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["totalMA",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["inDirectHits",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["inDirectHits",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Total Midairs: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Total Hits: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"totalMA-Total Midairs-Total","inDirectHits-Total Indirect/Direct Hits-Total",%vClient);
         
			%i1 = getField($lData::data["totalTime",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["totalTime",%client.lgame,%lType,%mon,%year],0) : %NA; 
			%i2 = getField($lData::data["multiKills",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["multiKills",%client.lgame,%lType,%mon,%year],0) : %NA;
			%line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Time Played: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Multi Kills: <color:03d597>%2</a>';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"totalTime-Time Played-Total Minutes","multiKills-Multi Kills-Total",%vClient);
			
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
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType,%page,%client.lgame);

		 %i1 = getField($lData::data["minePlusDisc",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["minePlusDisc",%client.lgame,%lType,%mon,%year],0) : %NA; 
		 %i2 = getField($lData::data["discACC",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["discACC",%client.lgame,%lType,%mon,%year],0) : %NA;
		 %line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Mine Disc: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Disc Accuracy: <color:03d597>%2</a>';
		 messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"minePlusDisc-Mine + Disc-Amount","discACCAVG-Spinfusor Accuracy-Percentage",%vClient);
		 
		 %i1 = getField($lData::data["weaponHitMaxDistMax",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["weaponHitMaxDistMax",%client.lgame,%lType,%mon,%year],0) : %NA; 
		 %i2 = getField($lData::data["maxSpeedMax",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["maxSpeedMax",%client.lgame,%lType,%mon,%year],0) : %NA;
		 %line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Longest Shot: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Highest Speed: <color:03d597>%2</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"weaponHitMaxDistMax-Longest Shot-Max Distance","maxSpeedMax-Highest Speed-Speed km/h",%vClient);

		 %i1 = getField($lData::data["discInDmg",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["discInDmg",%client.lgame,%lType,%mon,%year],0) : %NA; 
		 %i2 = getField($lData::data["shotsFired",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["shotsFired",%client.lgame,%lType,%mon,%year],0) : %NA;
		 %line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Most Damage: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Rounds Fired: <color:03d597>%2</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"discInDmg-Most Damage-Total","shotsFired-Most Rounds Fired-Total",%vClient);

		 %i1 = getField($lData::data["shockRearShot",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["shockRearShot",%client.lgame,%lType,%mon,%year],0) : %NA; 
		 %i2 = getField($lData::data["laserHeadShot",%client.lgame,%lType,%mon,%year],0) ? getField($lData::name["laserHeadShot",%client.lgame,%lType,%mon,%year],0) : %NA;
		 %line = '<font:univers condensed:18><lmargin:75><a:gamelink\tStats\tLB\t%5\t%3><color:0befe7>Rearshots: <color:03d597>%1</a><lmargin:350><a:gamelink\tStats\tLB\t%5\t%4><color:0befe7>Headshots: <color:03d597>%2</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,"shockRearShot-Rear Shocklance-Total","laserHeadShot-Laser Rifle Head Shots-Total",%vClient);
         
         %header = '<color:0befe7>  Weapon<lmargin:140>Kills<lmargin:290>MidAirs<lmargin:440>Combos';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %i1 = getField($lData::name["blasterKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["blasterMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["blasterCom",%client.lgame,%lType,%mon,%year],0); 
         %d1 = getField($lData::data["blasterKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["blasterMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["blasterCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Blaster<lmargin:140><a:gamelink\tStats\tLB\t%7\t%4><color:33CCCC><font:univers condensed:18>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"blasterKills-Blaster Kills-Total","blasterMA-Blaster MidAirs-Total","blasterCom-Blaster Combos-Total",%vClient);
         
         %i1 = getField($lData::name["plasmaKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["plasmaMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["plasmaCom",%client.lgame,%lType,%mon,%year],0); 
         %d1 = getField($lData::data["plasmaKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["plasmaMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["plasmaCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Plasma Rifle<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"plasmaKills-Plasma Rifle Kills-Total","plasmaMA-Plasma Rifle MidAirs-Total","plasmaCom-Plasma Rifle Combos-Total",%vClient);
          
         %i1 = getField($lData::name["cgKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["cgMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["cgCom",%client.lgame,%lType,%mon,%year],0);
         %d1 = getField($lData::data["cgKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["cgMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["cgCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;
         %line = '<color:0befe7>  Chaingun<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"cgKills-Chaingun Kills-Total","cgMA-Chaingun MidAirsTotal","cgCom-Chaingun Combos-Total",%vClient);
         
         %i1 = getField($lData::name["discKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["discMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["discCom",%client.lgame,%lType,%mon,%year],0);
         %d1 = getField($lData::data["discKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["discMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["discCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Spinfusor<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"discKills-Spinfusor Kills-Total","discMA-Spinfusor MidAirs-Total","discCom-Spinfusor Combos-Total",%vClient);
           
         %i1 = getField($lData::name["grenadeKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["grenadeMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["grenadeCom",%client.lgame,%lType,%mon,%year],0);  
         %d1 = getField($lData::data["grenadeKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["grenadeMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["grenadeCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Grenade Launcher<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"grenadeKills-Grenade Launcher Kills-Total","grenadeMA-Grenade Launcher MidAirs-Total","grenadeCom-Grenade Launcher Combos-Total",%vClient);
         
         %i1 = getField($lData::name["laserKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["laserMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["laserCom",%client.lgame,%lType,%mon,%year],0);   
         %d1 = getField($lData::data["laserKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["laserMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["laserCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA; 
         %line = '<color:0befe7>  Laser Rifle<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"laserKills-Laser Rifle Kills-Total","laserMA-Laser Rifle MidAirs-Total","laserCom-Laser Rifle Combos-Total",%vClient);
         
         %i1 = getField($lData::name["mortarKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["mortarMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["mortarCom",%client.lgame,%lType,%mon,%year],0);  
         %d1 = getField($lData::data["mortarKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["mortarMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["mortarCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA; 
         %line = '<color:0befe7>  Fusion Mortar<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"mortarKills-Fusion Mortar Kills-Total","mortarMA-Fusion Mortar MidAirs-Total","mortarCom-Fusion Mortar Combos-Total",%vClient);
         
         %i1 = getField($lData::name["missileKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["missileMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["missileCom",%client.lgame,%lType,%mon,%year],0);  
         %d1 = getField($lData::data["missileKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["missileMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["missileCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Missile Launcher<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"missileKills-Missile Launcher Kills-Total","missileMA-Missile Launcher MidAirs-Total","missileCom-Missile Launcher Combos-Total",%vClient); 
         
         %i1 = getField($lData::name["shockLanceKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["shockMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["shockCom",%client.lgame,%lType,%mon,%year],0); 
         %d1 = getField($lData::data["shockLanceKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["shockMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["shockCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Shocklance<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"shockLanceKills-Shocklance Kills-Total","shockMA-Shocklance MidAirs-Total","shockCom-Shocklance Combos-Total",%vClient); 
         
         %i1 = getField($lData::name["mineKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["mineMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["mineCom",%client.lgame,%lType,%mon,%year],0);  
         %d1 = getField($lData::data["mineKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["mineMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["mineCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA;  
         %line = '<color:0befe7>  Mine<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"mineKills-Mine Kills-Total","mineMA-Mine MidAirs-Total","mineCom-Mine Combos-Total",%vClient); 
         
         %i1 = getField($lData::name["hGrenadeKills",%client.lgame,%lType,%mon,%year],0);   
         %i2 = getField($lData::name["hGrenadeMA",%client.lgame,%lType,%mon,%year],0);   
         %i3 = getField($lData::name["hGrenadeCom",%client.lgame,%lType,%mon,%year],0);   
         %d1 = getField($lData::data["hGrenadeKills",%client.lgame,%lType,%mon,%year],0);         
         %d2 = getField($lData::data["hGrenadeMA",%client.lgame,%lType,%mon,%year],0);
         %d3 = getField($lData::data["hGrenadeCom",%client.lgame,%lType,%mon,%year],0);
         %i1 = %d1 ? %i1 : %NA; %i2 = %d2 ? %i2 : %NA; %i3 = %d3 ? %i3 : %NA; 
         %line = '<color:0befe7>  Hand Grenade<lmargin:140><font:univers condensed:18><color:33CCCC><a:gamelink\tStats\tLB\t%7\t%4>%1</a><lmargin:290><a:gamelink\tStats\tLB\t%7\t%5>%2</a><lmargin:440><a:gamelink\tStats\tLB\t%7\t%6>%3</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i1,%i2,%i3,"hGrenadeKills-Hand Grenade Kills-Total","hGrenadeMA-Hand Grenade MidAirs-Total","hGrenadeCom-Hand Grenade Combos-Total",%vClient); 
		 
		 messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Click on player name to view the top %1 players', $dtStats::topAmount);
      case "LB"://listBoards
         %lType = %client.curLType;
         %page = %client.curPage;
         %mon = getField($lData::mon[%lType, %client.lgame, %page],0);
         %year = getField($lData::mon[%lType, %client.lgame, %page],1);
         
         %GlArg4      = strreplace(%client.GlArg4,"-","\t");
         %field      = getField(%GlArg4,0);
         %name       = getField(%GlArg4,1);
         %fieldName  = getField(%GlArg4,2);
         messageClient( %client, 'SetScoreHudHeader', "", '<just:center>%2 (Top %1 Players)',$dtStats::topAmount, %name);
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\t%3\t%1\t%2>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,%lType,%client.backPage);
        
         %header = '<color:0befe7> <lmargin:50>#. <lmargin:75>%1<lmargin:250>%2';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header,%name,%fieldName);
         for(%i = 0; %i < $dtStats::topAmount; %i++){
            %scoreName  = getField($lData::name[%field,%client.lgame,%lType,%mon,%year],%i);
            %gameScore  = getField($lData::data[%field,%client.lgame,%lType,%mon,%year],%i);
            if(%gameScore){  
               %line = '<color:33CCCC><font:univers condensed:18> <lmargin:50>%1. <lmargin:75><clip:138>%2</clip><lmargin:250><color:03d597>%3';
               messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%i+1,%scoreName,mFloor(%gameScore + 0.5));
            }
         }
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
         %perPage = 15;// num of games listed per page 
         if(!$dtStats::varCount[%client.lgame]){
            $dtStats::varCount[%client.lgame] =  -1;   
            for(%i = 1; %i <= $dtStats::FC[%client.lgame]; %i++){
               %val = $dtStats::FV[%i,%client.lgame];
               $dtStats::varList[%client.lgame,$dtStats::varCount[%client.lgame]++] = %val;
            }
            for(%i = 1; %i <= $dtStats::FC["dtStats"]; %i++){
               %val = $dtStats::FV[%i,"dtStats"];
               $dtStats::varList[%client.lgame,$dtStats::varCount[%client.lgame]++] = %val;
            }
            for(%i = 1; %i <= $dtStats::FC["max"]; %i++){
               %val = getField($dtStats::FV[%i,"max"],1);
               $dtStats::varList[%client.lgame,$dtStats::varCount[%client.lgame]++] = %val;
            }
            for(%i = 1; %i <= $dtStats::FC["avg"]; %i++){
               %val = getField($dtStats::FV[%i,"avg"],1);
               $dtStats::varList[%client.lgame,$dtStats::varCount[%client.lgame]++] = %val;
            }
         }
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Variable List"); 
        // messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLBOARDS\t%1\t%3>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a>',%vClient,$dtStats::topAmount,%lType);
         if(%vLPage == 1){
            %line = '<a:gamelink\tStats\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tStats\tVARLIST\t%1\t%2> Next Page ></a>';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%vLPage + 1, %lType, %pagex,%client.lgame);
         }
         else if(%vLPage * %perPage > $dtStats::varCount[%client.lgame]){
            %line = '<a:gamelink\tStats\tLBOARDS\t%1\t%3-%5\t%4>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> - <a:gamelink\tStats\tVARLIST\t%1\t%2> < Back Page</a>    <a:gamelink\tStats\tVARLIST\t%1\t1><Reset></a>';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%vLPage - 1, %lType, %pagex,%client.lgame);
         }
         else if(%vLPage > 1){
            %line = '<a:gamelink\tStats\tLBOARDS\t%1\t%4-%6\t%5>  Back</a>  -  <a:gamelink\tStats\tReset\t%1>Return To Score Screen</a> -<a:gamelink\tStats\tVARLIST\t%1\t%2\> < Back Page </a>|<a:gamelink\tStats\tVARLIST\t%1\t%3> Next Page ></a>    <a:gamelink\tStats\tVARLIST\t%1\t1><Reset></a>';
            messageClient( %client, 'SetScoreHudSubheader', "",%line,%vClient,%vLPage - 1,%vLPage + 1, %lType, %pagex,%client.lgame);
         }
         %gc = $dtStats::varCount[%client.lgame];
         error(%vLPage);
         for(%z = (%vLPage - 1) * %perPage; %z < %vLPage * %perPage && %z <= %gc; %z++){
            %var = $dtStats::varList[%client.lgame,%z];
            %name = getField($lData::name[%var,%client.lgame,%lType,%mon,%year],0);  
            %line = '<color:0befe7><lmargin:50><a:gamelink\tStats\tLB\t%1\t%3\t0>%2</a><lmargin:250>%4';
            messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,%var,%var @"-"@ %var @ "-Value",%name);
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
////////////////////////////////////////////////////////////////////////////////
// LeaderBoards
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
function lStatsCycle(%build){ // starts and manages the build/sort cycle
  if($dtStats::debugEchos){error("lStatsCycle" SPC $dtStats::build["day"] SPC $dtStats::week && !$dtStats::build["week"] SPC
  $dtStats::build["month"] SPC $dtStats::build["quarter"] SPC $dtStats::build["year"] SPC $dtStats::lCount);} 
   if(%build){//reset
      $dtStats::build["day"] = 0;
      $dtStats::build["week"] = 0;
      $dtStats::build["month"] = 0;
      $dtStats::build["quarter"] = 0;
      $dtStats::build["year"] = 0;
      $dtStats::lCount = 0;
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
      schedule(5000,0,"loadLeaderboards",1);// reset and reload leaderboards 
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
   if(!isObject(serverStats)){new SimGroup(serverStats);}
   else{serverStats.delete(); new SimGroup(serverStats);}
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
      case "year":   %mon = $dtStats::curYear;       %fieldOld = 9;  %fieldNew = 10; 
      default:       %mon = getMonthNum();   %fieldOld = 5;  %fieldNew = 6; 
   }
   %file = new FileObject();
   %file.OpenForRead(%filepath);
   %day = strreplace(%file.readline(),"%t","\t");// read the first 3 lines to get are main data 
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
         sortLStats(1,%game,%game,%lType); 
      }
      else{
         if($dtStats::debugEchos){error("No Valid Data For" SPC %lType SPC %mon);}
         lStatsCycle(0);
      }
   }
}

function  sortLStats(%c,%cat,%game,%lType){
   if($dtStats::debugEchos){error("sortLStats" SPC %c SPC %cat SPC %game SPC %lType);}
   if(%cat $= "max" || %cat $= "avg"){%var = getField($dtStats::FV[%c,%cat],1);}
   else{%var = $dtStats::FV[%c,%cat];}
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
      LFData.openForWrite("serverStats/lData/" @ "-" @ %game @ "-" @ %mon @ "-" @ $dtStats::curYear @ "-" @ %lType @"-.cs"); 
   }
   %n = %var @ "%tname";// name list
   %s = %var @ "%tdata"; // data list 
   %g = %var @ "%tguid"; // data list 
   %statsCount = serverStats.getCount();
   for (%i = 0; %i < %statsCount && %i < $dtStats::topAmount; %i++){//selection sort 
      %maxCount = %i;  
      for (%j = %i+1; %j < %statsCount; %j++){  
         if (serverStats.getObject(%j).LStats[%var,%game] > serverStats.getObject(%maxCount).LStats[%var,%game])
            %maxCount = %j;  
      }
      %obj = serverStats.getObject(%maxCount);
      serverStats.bringToFront(%obj);// push the ones we have sorted to the front so we dont pass over them again 
      %n = %n @ "%t" @ %obj.name; 
      %s = %s @ "%t" @ %obj.LStats[%var,%game];
      %g = %g @ "%t" @ %obj.guid;
   } 
   
   LFData.writeLine(%n);
   LFData.writeLine(%s);
   LFData.writeLine(%g);
   
   if(%cat !$= "dtStats" &&  %cat !$= "max"  &&  %cat !$= "avg" && %c >= $dtStats::FC[%cat]){ // switch over to non game type stats
     %c = 0;  
     %cat = "dtStats";
   }
   else if(%cat $= "dtStats" && %c >= $dtStats::FC[%cat]){ 
     %c = 0;  
     %cat = "max";
   }
   else if(%cat $= "max" && %c >= $dtStats::FC[%cat]){ 
     %c = 0;  
     %cat = "avg";
   }
   if(%cat $= "avg"  && %c >= $dtStats::FC[%cat]){
      LFData.close();
      LFData.delete();
      lStatsCycle(0); // kick off the next one 
   }
   else{
      schedule(100,0,"sortLStats",%c++,%cat,%game,%lType);//keep at 100ms
   }
}

function loadLeaderboards(%reset){ // loads up leaderboards
   if($dtStats::debugEchos){error("loadLeaderboards reset =" SPC %reset);}  
   if(%reset){deleteVariables("$lData::*");} 
   if(!$lData::load){$lData::load = 1;}
   else{return;}// exit  if we have all ready loaded 
   if($dtStats::sm){
      dtCleanUp(0);
   }
   if(!isEventPending($dtStats::buildEvent)){
         $dtStats::buildEvent = schedule(getTimeDif($dtStats::buildSetTime),0,"lStatsCycle",1);
   }
   markNewDay();//called when server starts and when build completes
   %oldFileCount = 0; 
   %file = new FileObject(); 
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
            if(%stack $= "data"){
               %data = getFields(%line,2,getFieldCount(%line)-1);
               $lData::data[%var,%game,%lType,%mon,%year] = %data; 
            }
            else if(%stack $= "guid"){
               %guid = getFields(%line,2,getFieldCount(%line)-1);
               $lData::guid[%var,%game,%lType,%mon,%year] = %guid; 
            }
            else if(%stack $= "name"){
               %name = getFields(%line,2,getFieldCount(%line)-1);
               $lData::name[%var,%game,%lType,%mon,%year] = %name; 
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
   %oldFileCount = 0;
   for (%i = 0; %i < %count; %i++){
      %filepath = findNextfile(%filename);
      %file.OpenForRead(%filepath);  
      %dateLine = strreplace(%file.readline(),"%t","\t");
      %day = getField(%dateLine,2);
     // %month =  getField(%dateLine,6);
      %year = getField(%dateLine,10);
      %file.close();
      //%d0 TAB %d1 TAB %w0 TAB %w1 TAB %m0 TAB %m1 TAB %q0 TAB %q1 TAB %y0 TAB %y1;
      //echo(isFileExpired("expire",%day,%year) SPC %day SPC %year);
      if(isFileExpired("expire",%day,%year)){
         if($dtStats::sm || %force){
            if($dtStats::debugEchos){error("Deleting old file" SPC %filepath);}
            if(isFile(%filepath)){
               schedule(%v++ * 500,0,"deleteFile",%filepath);
            }
            %gPath = strreplace(%filepath,"t.cs","g.cs");
            if(isFile(%gPath)){
               schedule(%v++ * 500,0,"deleteFile",%gPath);
            }
         }
         else{
            %oldFileCount++;
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
         if($dtStats::expire > 1){
            %dif = $dtStats::curYear - %year;
            %days += 365 * (%dif-1);
            %days += 366 - %d;
            %days += $dtStats::curDay;
            if(%days > $dtStats::expire){
               return 1;  
            }
            else{
               return 0;  
            }
         }
         else{
            return 1;  
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