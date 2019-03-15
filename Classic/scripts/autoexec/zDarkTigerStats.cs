//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	Score Hud Stats System, Gather data across x number of games to do math/stats									//
//	This also has the added benefit of restoreing scores after leaving												//
//	Script BY: DarkTiger																							//
//	Prerequisites - Classic 1.5.2 - Evolution Admin Mod  - (zAdvancedStatsLogless.vl2 - for mine disc support)		//
//	Version 1.0 - initial release																					//
//	Version 2.0 - code refactor/optimizing/fixes																	//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//-----------Settings------------

$dtStats::viewSelf = 0; //Only self client can see his own stats, any stat, unless admin
//number of games to gather a running average, i would not make this too big of a number as its alot of data load/save
$dtStats::MaxNumOfGames = 10;
//set to 1 for the averaging to skip over zeros for example 0 0 1 2 0 4 0  it would only add 1 2 4 and divide by 3
$dtStats::skipZeros = 1;
$dtStats::Enable = 1; //a way to disable the stats system with out haveing to remove it
// Set to 1 for it to collect stats only on full games, the first game is ignored becuase its the game the player joined in at unless they meet the percentage requirement
//With it off it records all even after the player has left it will save
$dtStats::fullGames["CTFGame"] = 1;
//if they are here for 75% of the game, count it as a full game, this percentage is calc from time and score limit
$dtStats::fgPercentage["CTFGame"] = 25;
//0 score based, 1 time based, 2 the closer  one to finishing the game, 3 mix avg
$dtStats::fgPercentageType["CTFGame"] = 2;

$dtStats::fullGames["LakRabbitGame"] = 1;
$dtStats::fgPercentage["LakRabbitGame"] = 25;
$dtStats::fgPercentageType["LakRabbitGame"] = 2;

$dtStats::returnToMenuTimer = (30*1000)*1;// 1 min after not making an action reset
//Set to 1 when your makeing changes to the menu so you can see them  update live note the refresh rate is like 2-4 secs
//just make your edit and exec("scripts/autoexec/stats.cs"); to re exec it and it should apply
$dtStats::enableRefresh = 0;

//set to 1  for it saves the last game played between games - 2 files per player each game
//set to 0 for it to "only" save clients that have left this saves all data - $dtStats::MaxNumOfGames + 1 files for each player that has left
$dtStats::saveBetweenGames = 1;

//Turned this off for now to prevent any possable stutter
//$dtStats::saveOnLeave = 0; // code has been comented out see dtStatsClientLeaveGame

$dtStats::slowLoadTime = 500;// lets just load that slowly
$dtStats::slowSaveTime = 100;

//debug
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

///////////////////////////////////////////////////////////////////////////////
//                             CTF
///////////////////////////////////////////////////////////////////////////////
//Game type values out of CTFGame.cs
$dtStats::fieldValue[%ctf++,"CTFGame"] = "kills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "deaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "suicides";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "teamKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "flagCaps";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "flagGrabs";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "carrierKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "flagReturns";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "score";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "scoreMidAir";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "scoreHeadshot";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "minePlusDisc";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "scoreRearshot";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "escortAssists";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "defenseScore";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "offenseScore";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "flagDefends";

//Values in this script - keep this one as is,
//Then this can be copied from to setup other game types then one can trimed back the game the other game types

$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mineKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mineDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "explosionKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "explosionDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "impactKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "impactDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "groundKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "groundDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "turretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "turretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "aaTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "aaTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "indoorDepTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "indoorDepTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "outdoorDepTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "outdoorDepTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "sentryTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "sentryTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "outOfBoundKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "outOfBoundDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "lavaKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "lavaDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shrikeBlasterKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shrikeBlasterDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "bellyTurretKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "bellyTurretDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "bomberBombsKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "bomberBombsDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "tankChaingunKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "tankChaingunDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "tankMortarKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "tankMortarDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "satchelChargeKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "satchelChargeDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mpbMissileKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mpbMissileDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "lightningKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "lightningDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "vehicleSpawnKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "vehicleSpawnDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "forceFieldPowerUpKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "forceFieldPowerUpDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "crashKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "crashDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "waterKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "waterDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "nexusCampingKills";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "nexusCampingDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownKill";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownDeaths";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownDirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownInDmg";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownIndirectHits";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownInDmgTaken";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "cgShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "discShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "grenadeShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "laserShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "mortarShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "missileShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "shockLanceShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "plasmaShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "blasterShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "elfShotsFired";
$dtStats::fieldValue[%ctf++,"CTFGame"] = "unknownShotsFired";
$dtStats::fieldCount["CTFGame"] = %ctf;
///////////////////////////////////////////////////////////////////////////////
//                             LakRabbit
///////////////////////////////////////////////////////////////////////////////
//Game type values - out of LakRabbitGame.cs
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "score";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "kills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "deaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "suicides";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "flagGrabs";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "flagTimeMS";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "morepoints";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mas";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalSpeed";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalDistance";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalChainAccuracy";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalChainHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalSnipeHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalSnipes";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalShockHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "totalShocks";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "minePlusDisc";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "MidairflagGrabs";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "MidairflagGrabPoints";

//Values in this script
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mineKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mineDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "explosionKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "explosionDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "impactKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "impactDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "groundKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "groundDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "outOfBoundKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "outOfBoundDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "lavaKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "lavaDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "satchelChargeKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "satchelChargeDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "lightningKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "lightningDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "forceFieldPowerUpKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "forceFieldPowerUpDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "waterKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "waterDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "nexusCampingKills";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "nexusCampingDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownKill";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownDeaths";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownDirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownInDmg";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownIndirectHits";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownInDmgTaken";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "cgShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "discShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "grenadeShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "laserShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "mortarShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "missileShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "shockLanceShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "plasmaShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "blasterShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "elfShotsFired";
$dtStats::fieldValue[%lak++,"LakRabbitGame"] = "unknownShotsFired";
$dtStats::fieldCount["LakRabbitGame"] = %lak;

if(!$dtStats::Enable){return;} // abort exec
if(!isObject(statsGroup)){new SimGroup(statsGroup);}
// Just a note on the package and the functions its moding.
// The functions with in the package are mostly just my code additions and the parent order if there are other packages shouldent really matter for this
// The true overwrites that may be of issue if others exists are RadiusExplosion and  SniperRifleImage::onFire
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
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %damageLocation);//for stats collection
   }
   function LakRabbitGame::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation){
      parent::onClientKilled(%game, %clVictim, %clKiller, %damageType, %implement, %damageLocation);
      clientKillStats(%game,%clVictim, %clKiller, %damageType, %damageLocation);//for stats collection
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
   
   function CTFGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      //the default behavior when clicking on a game link is to start observing that client
      if(%arg1 $= "Stats"){
         %client.viewStats = 1;// lock out score hud from updateing untill they are done
         %client.viewMenu = %arg2;
         %client.viewClient = %arg3;
         %client.GlArg4 = %arg4;
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
      
      %targetClient = %arg1;
      if ((%client.team == 0) && isObject(%targetClient) && (%targetClient.team != 0))
      {
         %prevObsClient = %client.observeClient;
         
         // update the observer list for this client
         observerFollowUpdate( %client, %targetClient, %prevObsClient !$= "" );
         
         serverCmdObserveClient(%client, %targetClient);
         displayObserverHud(%client, %targetClient);
         
         //if (%targetClient != %prevObsClient)
         //{
            //messageClient(%targetClient, 'Observer', '\c1%1 is now observing you.', %client.name);
            //messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
         //}
      }
   }
   
   function CTFGame::updateScoreHud(%game, %client, %tag){// defaultGame/evo
      // error("CTFGame::updateScoreHud");
      if(%client.viewStats && $dtStats::enableRefresh){
         //echo("view stats");
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
   
   function LakRabbitGame::processGameLink(%game, %client, %arg1, %arg2, %arg3, %arg4, %arg5){
      //the default behavior when clicking on a game link is to start observing that client
      
      if(%arg1 $= "Stats"){
         %client.viewStats = 1;// lock out score hud from updateing untill they are done
         %client.viewMenu = %arg2;
         //echo(%arg3);
         %client.viewClient = getCNameToCID(%arg3);
         %client.GlArg4 = %arg4;
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
      
      %targetClient = %arg1;
      if ((%client.team == 0) && isObject(%targetClient) && (%targetClient.team != 0))
      {
         %prevObsClient = %client.observeClient;
         
         // update the observer list for this client
         observerFollowUpdate( %client, %targetClient, %prevObsClient !$= "" );
         
         serverCmdObserveClient(%client, %targetClient);
         displayObserverHud(%client, %targetClient);
         
         //if (%targetClient != %prevObsClient)
         //{
            //messageClient(%targetClient, 'Observer', '\c1%1 is now observing you.', %client.name);
            //messageClient(%prevObsClient, 'ObserverEnd', '\c1%1 is no longer observing you.', %client.name);
         //}
      }
   }
   function LakRabbitGame::updateScoreHud(%game, %client, %tag){
      // error("LakRabbitGame::updateScoreHud");
      if(%client.viewStats && $dtStats::enableRefresh){
         //echo("view stats");
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
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name !$= "" && %col2Client.name $= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= "" && %col2Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  
               }
               else{
                  if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tStats\tView\t%4>+</a>
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tStats\tView\t%4>+</a>
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left><a:gamelink\tStats\tView\t%4>+</a> %4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150>%1</clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310>%8<just:left>%4<rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col2Client.nameBase, %col2ClientScore, %col2ClientTime, %col1Style, %col2Style);
                  
               }
            }
            else{
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  if(%col1Client.name !$= "")
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style );
               }
               else{
                  if(%col1Client.name $= %client.name)
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> %1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style);
                  else
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200>%1</clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                  %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style );
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
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     
                  }
                  else if (%col2Style $= "<color:dcdcdc>")
                  {
                     if(%col1Client.name !$= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
                  else
                  {
                     if(%col1Client.name !$= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
               }
               else{/////////////////////////////////////////////////////////////////////
                  if (%col2Style $= "<color:00dc00>")//<a:gamelink\tStats\tView\t%1>+</a><a:gamelink\tStats\tView\t%4>+</a>
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     
                  }
                  else if (%col2Style $= "<color:dcdcdc>")//<a:gamelink\tStats\tView\t%4>+</a>
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><color:00dc00><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
                  else
                  {
                     if(%col1Client.name $= %client.name && %col2Client.name !$= "")//<a:gamelink\tStats\tView\t%4>+</a>
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name !$= "" && %col2Client.name $= %client.name)//<a:gamelink\tStats\tView\t%4>+</a>
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= %client.name && %col2Client.name $= "")
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else if(%col1Client.name $= "" && %col2Client.name $= %client.name)
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\tStats\tView\t%4>+</a> <a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                     else
                        messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:10><spush>%7\t<clip:150><a:gamelink\t%8>%1</a></clip><rmargin:205><just:right>%2<rmargin:270><just:right>%3<spop><rmargin:505><lmargin:310><just:left><clip:150><a:gamelink\t%9>%4</a></clip><rmargin:505><just:right>%5<rmargin:570><just:right>%6',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime,
                     %col2Client.nameBase, %col2ClientScore, %col2ClientTime,
                     %col1Style, %col1Client, %col2Client );
                  }
                  
               }
            }
            else{
               if(%client.isAdmin || %client.isSuperAdmin || !$dtStats::viewSelf){
                  if(%col1Client.name !$= ""){
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
               }
               else{
                  if(%col1Client.name $= %client.name){
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\tStats\tView\t%1>+</a> <a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
                  }
                  else{
                     messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20>%4\t<clip:200><a:gamelink\t%5>%1</a></clip><rmargin:280><just:right>%2<rmargin:375><just:right>%3',
                     %col1Client.nameBase, %col1ClientScore, %col1ClientTime, %col1Style, %col1Client );
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
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr,%cl.nameBase );
               }
               else if(%client.name $= %cl.name){
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150><a:gamelink\tStats\tView\t%3>+</a> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr,%cl.nameBase );
               }
               else{
                  messageClient( %client, 'SetLineHud', "", %tag, %index, '<tab:20, 310>\t<clip:150> %1</clip><rmargin:260><just:right>%2',
                  %cl.name, %obsTimeStr,%cl.nameBase );
               }
               
               %index++;
            }
         }
      }
      
      //clear the rest of Hud so we don't get old lines hanging around...
      messageClient( %client, 'ClearHud', "", %tag, %index );
   }
   function RadiusExplosion(%explosionSource, %position, %radius, %damage, %impulse, %sourceObject, %damageType)
   {
      // error("RadiusExplosion");
      InitContainerRadiusSearch(%position, %radius, $TypeMasks::PlayerObjectType      |
      $TypeMasks::VehicleObjectType     |
      $TypeMasks::StaticShapeObjectType |
      $TypeMasks::TurretObjectType      |
      $TypeMasks::ItemObjectType);
      
      %numTargets = 0;
      while ((%targetObject = containerSearchNext()) != 0)
      {
         %dist = containerSearchCurrRadDamageDist();
         
         if (%dist > %radius)
            continue;
         
         // z0dd - ZOD, 5/18/03. Changed to stop Force Field console spam
         // if (%targetObject.isMounted())
         if (!(%targetObject.getType() & $TypeMasks::ForceFieldObjectType) && %targetObject.isMounted())
         {
            %mount = %targetObject.getObjectMount();
            %found = -1;
            for (%i = 0; %i < %mount.getDataBlock().numMountPoints; %i++)
            {
               if (%mount.getMountNodeObject(%i) == %targetObject)
               {
                  %found = %i;
                  break;
               }
            }
            if (%found != -1)
            {
               if (%mount.getDataBlock().isProtectedMountPoint[%found])
               {
                  continue;
               }
            }
         }
         %targets[%numTargets]     = %targetObject;
         %targetDists[%numTargets] = %dist;
         %numTargets++;
      }
      
      for (%i = 0; %i < %numTargets; %i++)
      {
         %targetObject = %targets[%i];
         %dist = %targetDists[%i];
         if(isObject(%targetObject)) // z0dd - ZOD, 5/18/03 Console spam fix.
         {
            %coverage = calcExplosionCoverage(%position, %targetObject,
            ($TypeMasks::InteriorObjectType |
            $TypeMasks::TerrainObjectType |
            $TypeMasks::ForceFieldObjectType |
            $TypeMasks::VehicleObjectType));
            if (%coverage == 0)
               continue;
            
            //if ( $splashTest )
            %amount = (1.0 - ((%dist / %radius) * 0.88)) * %coverage * %damage;
            //else
            //%amount = (1.0 - (%dist / %radius)) * %coverage * %damage;
            
            //error( "damage: " @ %amount @ " at distance: " @ %dist @ " radius: " @ %radius @ " maxDamage: " @ %damage );
            
            %data = %targetObject.getDataBlock();
            %className = %data.className;
            
            if (%impulse && %data.shouldApplyImpulse(%targetObject))
            {
               %p = %targetObject.getWorldBoxCenter();
               %momVec = VectorSub(%p, %position);
               %momVec = VectorNormalize(%momVec);
               
               //------------------------------------------------------------------------------
               // z0dd - ZOD, 7/08/02. More kick when player damages self with disc or mortar.
               // Stronger DJs and mortar jumps without impacting others (mainly HoFs)
               if(%sourceObject == %targetObject)
               {
                  if (%damageType == $DamageType::Disc)
                  {
                     %impulse = 4475;
                  }
                  else if (%damageType == $DamageType::Mortar)
                  {
                     %impulse = 5750;
                  }
               }
               //------------------------------------------------------------------------------
               
               %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
               %doImpulse = true;
            }
            else if( %className $= FlyingVehicleData || %className $= HoverVehicleData ) // Removed WheeledVehicleData. z0dd - ZOD, 4/24/02. Do not allow impulse applied to MPB, conc MPB bug fix.
            {
               %p = %targetObject.getWorldBoxCenter();
               %momVec = VectorSub(%p, %position);
               %momVec = VectorNormalize(%momVec);
               %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
               
               if( getWord( %momVec, 2 ) < -0.5 )
                  %momVec = "0 0 1";
               
               // Add obj's velocity into the momentum vector
               %velocity = %targetObject.getVelocity();
               //%momVec = VectorNormalize( vectorAdd( %momVec, %velocity) );
               %doImpulse = true;
            }
            else
            {
               %momVec = "0 0 1";
               %doImpulse = false;
            }
            
            if(%amount > 0){
               %data.damageObject(%targetObject, %sourceObject, %position, %amount, %damageType, %momVec, %explosionSource.theClient, %explosionSource);
               clientIndirectDmgStats(Game.getId(),%data,%sourceObject,%targetObject, %damageType,%amount);
            }
            else if( %explosionSource.getDataBlock().getName() $= "ConcussionGrenadeThrown" && %data.getClassName() $= "PlayerData" )
            {
               %data.applyConcussion( %dist, %radius, %sourceObject, %targetObject );
               
               if(!$teamDamage && %sourceObject != %targetObject && %sourceObject.client.team == %targetObject.client.team)
               {
                  messageClient(%targetObject.client, 'msgTeamConcussionGrenade', '\c1You were hit by %1\'s concussion grenade.', getTaggedString(%sourceObject.client.name));
               }
            }
            //-------------------------------------------------------------------------------
            // z0dd - ZOD, 4/16/02. Tone done the how much bomber & HPC flip out when damaged
            if( %doImpulse )
            {
               %vehName = %targetObject.getDataBlock().getName();
               if ((%vehName $= "BomberFlyer") || (%vehName $= "HAPCFlyer"))
               {
                  %bomberimp = VectorScale(%impulseVec, 0.6);
                  %impulseVec = %bomberimp;
               }
               %targetObject.applyImpulse(%position, %impulseVec);
            }
            //if( %doImpulse )
            //   %targetObject.applyImpulse(%position, %impulseVec);
            //-------------------------------------------------------------------------------
         }
      }
   }
   function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal){
      //  error("ProjectileData::onCollision");
      parent::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal);
      clientDirectDmgStats(Game.getId(),%data,%projectile, %targetObject);
   }
   function ShapeBaseImageData::onFire(%data, %obj, %slot){
      // error("ShapeBaseImageData::onFire");
      %p = parent::onFire(%data, %obj, %slot);
      if(isObject(%p)){
         clientShotsFired(Game.getId(),%data.projectile, %p);
      }
      return %p;
   }
   function SniperRifleImage::onFire(%data,%obj,%slot){
      //error("SniperRifleImage::onFire");
      if(Game.class $= "LakRabbitGame"){
         return;
      }
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
      clientShotsFired(Game.getId(),%data.projectile, %p);
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
      // error("ShockLanceImage::onFire");
      %p = parent::onFire(%this, %obj, %slot);
      if(isObject(%p)){
         clientShotsFired(Game.getId(),%data.projectile, %p);
      }
      return %p;
   }
   
};
if($dtStats::Enable){
   activatePackage(dtStats);
}
////////////////////////////////////////////////////////////////////////////////
//Game Type Commons
////////////////////////////////////////////////////////////////////////////////
function dtStatsMissionDropReady(%game, %client){ // called when client has finished loading
   %foundOld =0;
   if(!%client.isAIControlled() && !isObject(%client.dtStats)){
      for (%i = 0; %i < statsGroup.getCount(); %i++){ // check to see if my old data is still there
         %dtStats = statsGroup.getObject(%i);
         if(%dtStats.guid == %client.guid){
            if(%dtStats.leftPCT < $dtStats::fgPercentage[%game.class] && $dtStats::fullGames[%game.class]){
               %client.dtStats.dtGameCounter = 0;// reset to 0 so this game does count this game
            }
            //error(%dtStats.guid SPC %client.guid);
            %client.dtStats = %dtStats;
            %dtStats.client = %client;
            %dtStats.name = %client.nameBase;
            %dtStats.clientLeft = 0;
            %dtStats.markForDelete = 0;
            %foundOld =1;
            resGameStats(%client,%game.class); // restore stats;
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
         %dtStats.name =%client.nameBase;
         %dtStats.clientLeft = 0;
         %dtStats.markForDelete = 0;
         %dtStats.lastGame[%game.class] = 0;
         loadGameStats(%client.dtStats,%game.class);
         %client.dtStats.gameData[%game.class] = 1;
         //loadGameStats(%client.dtStats,"LakRabbitGame);
         %client.dtStats.dtGameCounter = 0;// mark player as just joined after the first game over  they will record stats
         
         if(Game.getGamePct() < (100 - $dtStats::fgPercentage[%game.class]) && $dtStats::fullGames[%game.class]){// they will be here long enough to count as a full game
            %client.dtStats.dtGameCounter++;
         }
      }
   }
   else if(isObject(%client.dtStats) && %client.dtStats.gameData[%game.class] $= ""){ // game type change
      loadGameStats(%client.dtStats,%game.class);
      %client.dtStats.gameData[%game.class] = 1;
   }
}

function dtStatsClientLeaveGame(%game, %client){
   if(!%client.isAiControlled()){
      %client.dtStats.clientLeft = 1;
      bakGameStats(%client,%game.class);//back up there current game in case they lost connection
      %client.dtStats.leftPCT = %game.getGamePct();
      //if($dtStats::saveOnLeave){
         //saveGameStats(%client.dtStats,%game.class);
      //}
   }
}
function dtStatsTimeLimitReached(%game){
   %game.timeLimitHit = 1;
   if($dtStats::fullGames[%game.class]){
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.isAiControlled()){
            
            if( %client.dtStats.dtGameCounter > 0){ //we throw out the first game as we joined it in progress
               incGameStats(%client,%game.class); // setup for next game
            }
            %client.dtStats.dtGameCounter++;
         }
      }
   }
}
function dtStatsScoreLimitReached(%game){
   %game.scoreLimitHit = 1;
   if($dtStats::fullGames[%game.class]){ // same as time limit reached
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(!%client.isAiControlled()){
            if( %client.dtStats.dtGameCounter > 0){
               incGameStats(%client,%game.class);
            }
         }
         %client.dtStats.dtGameCounter++; // next game should be a full game
      }
   }
}
function dtStatsGameOver( %game ){
   //error("CTF::gameOver");
   %timeNext =0;
   for (%i = 0; %i < statsGroup.getCount(); %i++){// see if we have any old clients data
      %dtStats = statsGroup.getObject(%i);
      if(%dtStats.clientLeft){ // find any that left during the match and
         if($dtStats::fullGames[%game.class]){
            if(%dtStats.leftPCT > $dtStats::fgPercentage[%game.class]){ // if they where here for most of it and left at the end save it
               %dtStats.markForDelete = 1;
               incBakGameStats(%dtStats,%game.class);// dump the backup into are stats and save
               %time += %timeNext; // this will chain them
               %timeNext = $dtStats::slowSaveTime * ($dtStats::saveBetweenGames ? 1 : %dtStats.gameCount[%game]);
               schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
            }
            else{
               %dtStats.markForDelete = 1;
               %time += %timeNext; // this will chain them
               %timeNext = $dtStats::slowSaveTime * ($dtStats::saveBetweenGames ? 1 : %dtStats.gameCount[%game]);
               schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
            }
         }
         else{
            %dtStats.markForDelete = 1;
            incBakGameStats(%dtStats,%game.class);// dump the backup into are stats and save
            %time += %timeNext; // this will chain them
            %timeNext = $dtStats::slowSaveTime * ($dtStats::saveBetweenGames ? 1 : %dtStats.gameCount[%game]);
            schedule(%time ,0,"saveGameStats",%dtStats,%game.class); //
         }
      }
   }
   for (%z = 0; %z < ClientGroup.getCount(); %z++){
      %client = ClientGroup.getObject(%z);
      %client.viewMenu = 0; // reset hud
      %client.viewClient = 0;
      %client.viewStats = 0;
      if(!%client.isAiControlled() ){
         if(%game.scoreLimitHit != 1 && %game.timeLimitHit != 1){
            // if game was longer then x precent and admin changemaps then save
            if(%game.getGamePct() > $dtStats::fgPercentage[%game.class] && $dtStats::fullGames[%game.class] && %client.dtstats.dtGameCounter > 0){ // if we dont care about full games  setup next gamea and copy over stats
               incGameStats(%client,%game.class);
            }
            else if(!$dtStats::fullGames[%game.class]){
               incGameStats(%client,%game.class);
            }
            else{
               %client.dtStats.dtGameCounter++;
            }
         }
         if($dtStats::saveBetweenGames && %client.dtStats.lastGame[%game.class] > 0){// as it says
            %time += %timeNext; // this will chain them
            %timeNext = $dtStats::slowSaveTime * ($dtStats::saveBetweenGames ? 1 : %dtStats.gameCount[%game]);
            schedule(%time ,0,"saveGameStats",%client.dtStats,%game.class); //
         }
         resetDtStats(%client);
      }
   }
  // error($MissionDisplayName);
}

////////////////////////////////////////////////////////////////////////////////
//Supporting Functions
////////////////////////////////////////////////////////////////////////////////

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
   
   switch$($dtStats::fgPercentageType["CTFGame"]){
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
   
   switch$($dtStats::fgPercentageType["LakRabbitGame"]){
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
         %mixPct =  (%scorePct +  %timePct) / 2;
         return %mixPct;
      default:
         if(%scorePct > %timePct)
            return %scorePct;
         else
            return %timePct;
   }
   
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
      return %name;
   }
   else{
      for (%i = 0; %i < ClientGroup.getCount(); %i++){
         %client = ClientGroup.getObject(%i);
         if(%client.nameBase $= %name){
            return %client;
         }
      }
   }
}
////////////////////////////////////////////////////////////////////////////////
//Load Save Management
////////////////////////////////////////////////////////////////////////////////
function loadGameStats(%dtStats,%game){// called when client joins server.cs onConnect
   if($dtStats::Enable  == 0){return;}
   loadGameTotalStats(%dtStats,%game);
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/" @ %game @ "/" @ %dtStats.guid @ "/" @ 1 @ ".cs";
   }
   else{
      return;
   }
   if(!isFile(%filename)){  resetDtStats(%dtStats.client); return;}// new player
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
function loadGameSlow(%dtStats,%i,%game){
   if(%dtStats.gameCount[%game] > 1){// load the rest
      if(%i <= %dtStats.gameCount[%game]){
         // error("slow Load" SPC %i);
         if(%dtStats.guid !$= ""){
            %filename = "serverStats/" @ %game @ "/" @ %dtStats.guid @ "/" @ %i @ ".cs";
         }
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
            %file.delete();
         }
         schedule($dtStats::slowLoadTime,0,"loadGameSlow",%dtStats,%i++,%game);
      }
      else{
         resetDtStats(%dtStats.client);
      }
   }
   
}

function saveGameStats(%dtStats,%game){ // called when client leaves server.cs onDrop
   if($dtStats::Enable  == 0){return;}
   if(%dtStats.lastGame[%game] > 0 && $dtStats::saveBetweenGames){
      saveTotalStats(%dtStats,%game);
      %c = %dtStats.lastGame[%game];
      if(%dtStats.guid !$= ""){
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
         
         %file.close();
         %file.delete();
      }
      %dtStats.lastGame[%game] = 0;
      if(%dtStats.markForDelete){
         %dtStats.delete();
      }
   }
   else{
      saveTotalStats(%dtStats,%game);
      saveStatsSlow(%dtStats,1,%game);
   }
}
function saveStatsSlow(%dtStats,%c,%game){ // called when client leaves server.cs onDrop
   
   //if(!isObject(%file)){ error("no object");}
   
   if(%c <= %dtStats.gameCount[%game]){
      //error("saveSlow" SPC %dtStats SPC %c SPC %dtStats.gameCount[%game] SPC %file);
      if(%dtStats.guid !$= ""){
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
         
         %file.close();
         %file.delete();
         schedule($dtStats::slowSaveTime,0,"saveStatsSlow",%dtStats,%c++,%game);
      }
   }
   else if(%dtStats.markForDelete){
      %dtStats.delete();
   }
   else{
     %dtStats.lastGame[%game] = 0;  
   }
}

function incGameStats(%client,%game) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
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
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = getFieldValue(%client,%val);
      if(%val $= "flagTimeMS"){// convert to min
         %var = mfloor((%var / 1000) / 60);
      }
      %client.dtStats.gameStats[%val,%c,%game] = %var;
   }
   addGameTotal(%client,%game); // add totals
   resetDtStats(%client); // reset to 0 for next game
}
function incBakGameStats(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
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
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = %dtStats.gameStats[%val,"b",%game];
      if(%val $= "flagTimeMS"){// convert to min
         %var = mfloor((%var / 1000) / 60);
      }
      %dtStats.gameStats[%val,%c,%game] = %var;
   }
   addGameBakTotal(%dtStats,%game); // add totals
}
function addGameBakTotal(%dtStats,%game) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.dtStats.totalNumGames[%game]++;
   %dtStats.gameStats["timeStamp",%c,%game] += formattimestring("hh:nn a, mm-dd");
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = %dtStats.gameStats[%val,"b",%game];
      if(%val $= "flagTimeMS"){// convert to min
         %var = mfloor((%var / 1000) / 60);
      }
      %dtStats.gameStats[%val,"t",%game] += %var;
   }
}
function bakGameStats(%client,%game) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = getFieldValue(%client,%val);
      %client.dtStats.gameStats[%val,"b",%game] = %var;
   }
}
function resGameStats(%client,%game) {// copy data back over to client
   if($dtStats::Enable  == 0){return;}
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = %client.dtStats.gameStats[%val,"b",%game];
      setFieldValue(%client,%val,%var);
   }
}

function addGameTotal(%client,%game) {// record that games stats and inc by one
   if($dtStats::Enable  == 0){return;}
   %client.dtStats.totalNumGames[%game]++;
   %client.dtStats.gameStats["timeStamp","t",%game] = formattimestring("hh:nn a, mm-dd");
   for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
      %val = $dtStats::fieldValue[%i,%game];
      %var = getFieldValue(%client,%val);
      if(%val $= "flagTimeMS"){// convert to min
         %var = mfloor((%var / 1000) / 60);
      }
      %client.dtStats.gameStats[%val,"t",%game] += %var;
   }
}
function saveTotalStats(%dtStats,%game){ // saved by the main save function
   if($dtStats::Enable  == 0){return;}
   if(%dtStats.statsOverWrite[%game] $= ""){
      %dtStats.statsOverWrite[%game] = 0;
   }
   if(%dtStats.guid !$= ""){
      %filename = "serverStats/"@ %game @"/" @ %dtStats.guid @ "/" @ "totalStats" @ ".cs";
      %file = new FileObject();
      %file.OpenForWrite(%filename);
      
      %file.writeLine("gameCount" @ "%t" @  %dtStats.gameCount[%game]);
      %file.writeLine("statsOverWrite" @ "%t" @ %dtStats.statsOverWrite[%game]);
      %file.writeLine("totalNumGames" @ "%t" @ %dtStats.totalNumGames[%game]);
      %file.writeLine("timeStamp" @ "%t" @ %dtStats.gameStats["timeStamp","t",%game]);
      for(%i = 1; %i <= $dtStats::fieldCount[%game]; %i++){
         %val = $dtStats::fieldValue[%i,%game];
         %var = %dtStats.gameStats[%val,"t",%game];
         %file.writeLine(%val @ "%t" @ %var);
      }
      %file.close();
      %file.delete();
   }
}

function loadGameTotalStats(%dtStats,%game){
   if($dtStats::Enable  == 0){return;}
   %file = new FileObject();
   if(%dtStats.guid !$= ""){
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
            else{
               if(%val > 2000000000){//
                  %val = 0;
               }
               %dtStats.gameStats[%var,"t",%game] =  %val;
            }
         }
         %file.close();
      }
   }
   %file.delete();
}
//prob could have segmented one of the arrays above for this  but ill leave it incase someone delets something
function resetDtStats(%client){
   if($dtStats::Enable  == 0){return;}
   %client.cgKills = 0;                %client.cgDeaths = 0;               %client.discKills = 0;
   %client.discDeaths = 0;             %client.grenadeKills = 0;           %client.grenadeDeaths = 0;
   %client.laserKills = 0;             %client.laserDeaths = 0;            %client.mortarKills = 0;
   %client.mortarDeaths = 0;           %client.missileKills = 0;           %client.missileDeaths = 0;
   %client.shockLanceKills = 0;        %client.shockLanceDeaths = 0;       %client.plasmaKills = 0;
   %client.plasmaDeaths = 0;           %client.blasterKills = 0;           %client.blasterDeaths = 0;
   %client.elfKills = 0;               %client.elfDeaths = 0;              %client.mineKills = 0;
   %client.mineDeaths = 0;             %client.explosionKills = 0;         %client.explosionDeaths = 0;
   %client.impactKills = 0;            %client.impactDeaths = 0;           %client.groundKills = 0;
   %client.groundDeaths = 0;           %client.turretKills = 0;            %client.turretDeaths = 0;
   %client.plasmaTurretKills = 0;      %client.plasmaTurretDeaths = 0;     %client.aaTurretKills = 0;
   %client.aaTurretDeaths = 0;         %client.elfTurretKills = 0;         %client.elfTurretDeaths = 0;
   %client.mortarTurretKills = 0;      %client.mortarTurretDeaths = 0;     %client.missileTurretKills = 0;
   %client.missileTurretDeaths = 0;    %client.indoorDepTurretKills = 0;   %client.indoorDepTurretDeaths = 0;
   %client.outdoorDepTurretKills = 0;  %client.outdoorDepTurretDeaths = 0; %client.sentryTurretKills = 0;
   %client.sentryTurretDeaths = 0;     %client.outOfBoundKills = 0;        %client.outOfBoundDeaths = 0;
   %client.lavaKills = 0;              %client.lavaDeaths = 0;             %client.shrikeBlasterKills = 0;
   %client.shrikeBlasterDeaths = 0;    %client.bellyTurretKills = 0;       %client.bellyTurretDeaths = 0;
   %client.bomberBombsKills = 0;       %client.bomberBombsDeaths = 0;      %client.tankChaingunKills = 0;
   %client.tankChaingunDeaths = 0;     %client.tankMortarKills = 0;        %client.tankMortarDeaths = 0;
   %client.satchelChargeKills = 0;     %client.satchelChargeDeaths = 0;    %client.mpbMissileKills = 0;
   %client.mpbMissileDeaths = 0;       %client.lightningKills = 0;         %client.lightningDeaths = 0;
   %client.vehicleSpawnKills = 0;      %client.vehicleSpawnDeaths = 0;     %client.forceFieldPowerUpKills = 0;
   %client.forceFieldPowerUpDeaths = 0;%client.crashKills = 0;             %client.crashDeaths = 0;
   %client.waterKills = 0;             %client.waterDeaths = 0;            %client.nexusCampingKills = 0;
   %client.nexusCampingDeaths = 0;     %client.unknownKill = 0;            %client.unknownDeaths = 0;
   %client.cgDmg = 0;                  %client.cgDirectHits = 0;           %client.cgDmgTaken = 0;
   %client.discDmg = 0;                %client.discDirectHits = 0;         %client.discDmgTaken = 0;
   %client.grenadeDmg = 0;             %client.grenadeDirectHits = 0;      %client.grenadeDmgTaken = 0;
   %client.laserDmg = 0;               %client.laserDirectHits = 0;        %client.laserDmgTaken = 0;
   %client.mortarDmg = 0;              %client.mortarDirectHits = 0;       %client.mortarDmgTaken = 0;
   %client.missileDmg = 0;             %client.missileDirectHits = 0;      %client.missileDmgTaken = 0;
   %client.shockLanceDmg = 0;          %client.shockLanceDirectHits = 0;   %client.shockLanceDmgTaken = 0;
   %client.plasmaDmg = 0;              %client.plasmaDirectHits = 0;       %client.plasmaDmgTaken = 0;
   %client.blasterDmg = 0;             %client.blasterDirectHits = 0;      %client.blasterDmgTaken = 0;
   %client.elfDmg = 0;                 %client.elfDirectHits = 0;          %client.elfDmgTaken = 0;
   %client.unknownDmg = 0;             %client.unknownDirectHits = 0;      %client.unknownDmgTaken = 0;
   %client.cgInDmg = 0;                %client.cgIndirectHits = 0;         %client.cgInDmgTaken = 0;
   %client.discInDmg = 0;              %client.discIndirectHits = 0;       %client.discInDmgTaken = 0;
   %client.grenadeInDmg = 0;           %client.grenadeIndirectHits = 0;    %client.grenadeInDmgTaken = 0;
   %client.laserInDmg = 0;             %client.laserIndirectHits = 0;      %client.laserInDmgTaken = 0;
   %client.mortarInDmg = 0;            %client.mortarIndirectHits = 0;     %client.mortarInDmgTaken = 0;
   %client.missileInDmg = 0;           %client.missileIndirectHits = 0;    %client.missileInDmgTaken = 0;
   %client.shockLanceInDmg = 0;        %client.shockLanceIndirectHits = 0; %client.shockLanceInDmgTaken = 0;
   %client.plasmaInDmg = 0;            %client.plasmaIndirectHits = 0;     %client.plasmaInDmgTaken = 0;
   %client.blasterInDmg = 0;           %client.blasterIndirectHits = 0;    %client.blasterInDmgTaken = 0;
   %client.elfInDmg = 0;               %client.elfIndirectHits = 0;        %client.elfInDmgTaken = 0;
   %client.unknownInDmg = 0;           %client.unknownIndirectHits = 0;    %client.unknownInDmgTaken = 0;
   %client.cgShotsFired = 0;           %client.discShotsFired = 0;         %client.grenadeShotsFired = 0;
   %client.laserShotsFired = 0;        %client.mortarShotsFired = 0;       %client.missileShotsFired = 0;
   %client.shockLanceShotsFired = 0;   %client.plasmaShotsFired = 0;       %client.blasterShotsFired = 0;
   %client.elfShotsFired = 0;          %client.minePlusDisc = 0;           %client.unknownShotsFired = 0;
   %client.MidairflagGrabs = 0;		   %client.MidairflagGrabPoints = 0;
}
////////////////////////////////////////////////////////////////////////////////
//Stats Collecting
////////////////////////////////////////////////////////////////////////////////
function clientKillStats(%game, %clVictim, %clKiller, %damageType, %damageLocation){
   error(%game SPC %clVictim SPC  %clKiller SPC  %damageType SPC  %damageLocation);
   if($dtStats::Enable  == 0){return;}
   if(%clKiller.team != %clVictim.team){
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %clKiller.cgKills++;
            %clVictim.cgDeaths++;
         case $DamageType::Disc:
            %clKiller.discKills++;
            %clVictim.discDeaths++;
         case $DamageType::Grenade:
            %clKiller.grenadeKills++;
            %clVictim.grenadeDeaths++;
         case $DamageType::Laser:
            %clKiller.laserKills++;
            %clVictim.laserDeaths++;
         case $DamageType::Mortar:
            %clKiller.mortarKills++;
            %clVictim.mortarDeaths++;
         case $DamageType::Missile:
            %clKiller.missileKills++;
            %clVictim.missileDeaths++;
         case $DamageType::ShockLance:
            %clKiller.shockLanceKills++;
            %clVictim.shockLanceDeaths++;
         case $DamageType::Plasma:
            %clKiller.plasmaKills++;
            %clVictim.plasmaDeaths++;
         case $DamageType::Blaster:
            %clKiller.blasterKills++;
            %clVictim.blasterDeaths++;
         case $DamageType::ELF:
            %clKiller.elfKills++;
            %clVictim.elfDeaths++;
         case $DamageType::Mine:
            %clKiller.mineKills++;
            %clVictim.mineDeaths++;
         case $DamageType::Explosion:
            %clKiller.explosionKills++;
            %clVictim.explosionDeaths++;
         case $DamageType::Impact:
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
            %clKiller.crashKills++;
            %clVictim.crashDeaths++;
         case $DamageType::Water:
            %clKiller.waterKills++;
            %clVictim.waterDeaths++;
         case $DamageType::NexusCamping:
            %clKiller.nexusCampingKills++;
            %clVictim.nexusCampingDeaths++;
         default:
            %clKiller.unknownKill++;
            %clVictim.unknownDeaths++;
      }
   }
}

function clientDirectDmgStats(%game, %data, %projectile, %targetObject){ // projectile
   if($dtStats::Enable  == 0){return;}
   // echo(isObject(%targetObject) SPC %targetObject.getClassName() SPC %targetObject.client.team SPC %projectile.sourceObject.client.team);
   if(isObject(%targetObject) && %targetObject.getClassName() $= "Player" && %targetObject.client.team != %projectile.sourceObject.client.team){
      if(%data.directDamageType !$= ""){
         %damageType = %data.directDamageType;
         %amount = %data.directDamage;
      }
      else{
         %damageType =  %data.radiusDamageType;
         %amount = %data.indirectDamage;// counts as full
      }
      %armorData = %targetObject.getDatablock();
      %damageScale = %armorData.damageScale[%damageType];
      if(%damageScale !$= "")
         %amount *= %damageScale;
      %client = %projectile.sourceObject.client;
      %targetClient = %targetObject.client;
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %client.cgDmg += %amount;
            %client.cgDirectHits++;
            %targetClient.cgDmgTaken += %amount;
         case $DamageType::Disc:
            %client.discDmg += %amount;
            %client.discDirectHits++;
            %targetClient.discDmgTaken += %amount;
         case $DamageType::Grenade:
            %client.grenadeDmg += %amount;
            %client.grenadeDirectHits++;
            %targetClient.grenadeDmgTaken += %amount;
         case $DamageType::Laser:
            %client.laserDmg += %amount;
            %client.laserDirectHits++;
            %targetClient.laserDmgTaken += %amount;
         case $DamageType::Mortar:
            %client.mortarDmg += %amount;
            %client.mortarDirectHits++;
            %targetClient.mortarDmgTaken += %amount;
         case $DamageType::Missile:
            %client.missileDmg += %amount;
            %client.missileDirectHits++;
            %targetClient.missileDmgTaken += %amount;
         case $DamageType::ShockLance:
            %client.shockLanceDmg += %amount;
            %client.shockLanceDirectHits++;
            %targetClient.shockLanceDmgTaken += %amount;
         case $DamageType::Plasma:
            %client.plasmaDmg += %amount;
            %client.plasmaDirectHits++;
            %targetClient.plasmaDmgTaken += %amount;
         case $DamageType::Blaster:
            %client.blasterDmg += %amount;
            %client.blasterDirectHits++;
            %targetClient.blasterDmgTaken += %amount;
         case $DamageType::ELF:
            %client.elfDmg += %amount;
            %client.elfDirectHits++;
            %targetClient.elfDmgTaken += %amount;
         default:
            %client.unknownDmg += %amount;
            %client.unknownDirectHits++;
            %targetClient.unknownDmgTaken += %amount;
      }
   }
}

function clientIndirectDmgStats(%game,%data,%sourceObject, %targetObject, %damageType,%amount){
   // echo(%data SPC %sourceObject SPC %targetObject SPC %damageType SPC %amount);
   //error(%damageType SPC %targetObject SPC %targetObject.client.mineDisc );
   //error(getObjectTypeMask(%targetObject));
   if($dtStats::Enable  == 0){return;}
   if(isObject(%targetObject) && %targetObject.getClassName() $= "Player" && %sourceObject.client.team != %targetObject.client.team){  // only care about pvp
      %damageScale = %data.damageScale[%damageType];
      if(%damageScale !$= ""){
         %amount *= %damageScale;
      }
      %client = %sourceObject.client;
      %targetClient = %targetObject.client;
      //echo(%damageType SPC %targetClient SPC %targetClient.mineDisc);
      switch$(%damageType){// list of all damage types to track see damageTypes.cs
         case $DamageType::Bullet:
            %client.cgInDmg += %amount;
            %client.cgIndirectHits++;
            %targetClient.cgInDmgTaken += %amount;
         case $DamageType::Disc:
            %client.discInDmg += %amount;
            %client.discIndirectHits++;
            %targetClient.discInDmgTaken += %amount;
            if(%targetClient.mineDisc){
               %client.minePlusDisc++;
            }
         case $DamageType::Mine:
            if(%targetClient.mineDisc){
               %client.minePlusDisc++;
            }
         case $DamageType::Grenade:
            %client.grenadeInDmg += %amount;
            %client.grenadeIndirectHits++;
            %targetClient.grenadeInDmgTaken += %amount;
         case $DamageType::Laser:
            %client.laserInDmg += %amount;
            %client.laserIndirectHits++;
            %targetClient.laserInDmgTaken += %amount;
         case $DamageType::Mortar:
            %client.mortarInDmg += %amount;
            %client.mortarIndirectHits++;
            %targetClient.mortarInDmgTaken += %amount;
         case $DamageType::Missile:
            %client.missileInDmg += %amount;
            %client.missileIndirectHits++;
            %targetClient.missileInDmgTaken += %amount;
         case $DamageType::ShockLance:
            %client.shockLanceInDmg += %amount;
            %client.shockLanceIndirectHits++;
            %targetClient.shockLanceInDmgTaken += %amount;
         case $DamageType::Plasma:
            %client.plasmaInDmg += %amount;
            %client.plasmaIndirectHits++;
            %targetClient.plasmaInDmgTaken += %amount;
         case $DamageType::Blaster:
            %client.blasterInDmg += %amount;
            %client.blasterIndirectHits++;
            %targetClient.blasterInDmgTaken += %amount;
         case $DamageType::ELF:
            %client.elfInDmg += %amount;
            %client.elfIndirectHits++;
            %targetClient.elfInDmgTaken += %amount;
         default:
            %client.unknownInDmg += %amount;
            %client.unknownIndirectHits++;
            %targetClient.unknownInDmgTaken += %amount;
      }
   }
}
function clientShotsFired(%game, %data, %projectile){ // could do a fov check to see if we are trying to aim at a player
   if($dtStats::Enable  == 0){return;}
   %client = %projectile.sourceObject.client;
   if(!isObject(%client) || %client.isAiControlled()){ return;}
   if(%data.directDamageType !$= ""){
      %damageType = %data.directDamageType;
   }
   else{
      %damageType =  %data.radiusDamageType;
   }
   // echo(%damageType);
   switch$(%damageType){// list of all damage types to track see damageTypes.cs
      case $DamageType::Bullet:
         %client.cgShotsFired++;
      case $DamageType::Disc:
         %client.discShotsFired++;
      case $DamageType::Grenade:
         %client.grenadeShotsFired++;
      case $DamageType::Laser:
         %client.laserShotsFired++;
      case $DamageType::Mortar:
         %client.mortarShotsFired++;
      case $DamageType::Missile:
         %client.missileShotsFired++;
      case $DamageType::ShockLance:
         %client.shockLanceShotsFired++;
      case $DamageType::Plasma:
         %client.plasmaShotsFired++;
      case $DamageType::Blaster:
         %client.blasterShotsFired++;
      case $DamageType::ELF:
         %client.elfShotsFired++;
      default:
         %client.unknownShotsFired++;
   }
}
////////////////////////////////////////////////////////////////////////////////
//Menu Stuff
////////////////////////////////////////////////////////////////////////////////
function getGameRunAvg(%client, %value,%game){
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
         return %val / %client.dtStats.gameCount[%game];
      else if(%c > 0)
         return %val / %c;
      else
         return 0;
   }
   else{
      return 0;
   }
}
function getGameTotalAvg(%vClient,%value,%game){
   //error(%vClient SPC %value);
   if(%vClient.dtStats.gameStats[%value,"t",%game] !$= "" && %vClient.dtStats.totalNumGames[%game] > 0)
      %totalAvg = %vClient.dtStats.gameStats[%value,"t",%game] / %vClient.dtStats.totalNumGames[%game];
   else
      %totalAvg = 0;
   
   return %totalAvg;
}
function getGameTotal(%vClient,%value,%game){
   %total = %vClient.dtStats.gameStats[%value,"t",%game];
   if(%total !$= ""){
      return %total;
   }
   else{
      return 0;
   }
}
function getGameDetails(%vClient,%value,%c,%game){
   %total = %vClient.dtStats.gameStats[%value,%c,%game];
   if(%total !$= ""){
      return %total;
   }
   else{
      return 0;
   }
}
function menuReset(%client){
   //error("menuReset");
   %client.viewMenu = 0;
   %client.viewClient = 0;
   %client.viewStats = 0;
   
}

function statsMenu(%client,%game){
   if($dtStats::Enable  == 0){
      %client.viewMenu = 0;
      %client.viewClient = 0;
      %client.viewStats = 0;
      return;
   }
   %menu = %client.viewMenu;
   cancel(%client.rtmt); // if new action  then restart timer
   %client.rtmt = schedule($dtStats::returnToMenuTimer,0,"menuReset",%client);
   %vClient = %client.viewClient;
   %tag = 'scoreScreen';
   //error(%menu SPC %vClient);
   
   %isTargetSelf = (%client == %vClient);
   %isAdmin = (%client.isAdmin || %client.isSuperAdmin);
   
   messageClient( %client, 'ClearHud', "", 'scoreScreen', 0 );
   %index = -1;
   switch$(%menu)
   {
      case "View":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase@ "'s Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", "<a:gamelink\tStats\tReset>  Back</a>");
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>  Main Options Menu");
         if(%game $= "CTFGame"){
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTF\t%1>  + CTF Match Stats</a>',%vClient);
            if(%isTargetSelf || %isAdmin) {
			messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFW\t%1>  + CTF Weapon Stats</a>',%vClient);
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFA\t%1>  + CTF Kills/Deaths</a>',%vClient);
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tCTFH\t%1>  + Previous CTF Games</a>',%vClient);
			}
			else {
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");	
			}
         }
         if(%game $= "LakRabbitGame"){
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLak\t%1>  + Lak Match Stats</a>',%vClient);
            if(%isTargetSelf || %isAdmin) {
			messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLAKW\t%1>  + Lak Weapon Stats</a>',%vClient);
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
            messageClient( %client, 'SetLineHud', "", %tag, %index++, '<a:gamelink\tStats\tLAKH\t%1>  + Previous Lak Games</a>',%vClient);
			}
			else {
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
			}
            //messageClient( %client, 'SetLineHud', "", %tag, %index++, '(LakRabbit Games Played = %2) (LakRabbit Running Average %3/%4) (OW %5)',%vClient,%vClient.dtStats.lakTotalNumGames,%vClient.dtStats.lakGameCount,$dtStats::MaxNumOfGames,%vClient.dtStats.lakStatsOverWrite);
            messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         }
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, '<just:center>Updates are at the end of every map.');
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Based on the last" SPC $dtStats::MaxNumOfGames SPC "games.");
      case "LAKHIST":
         %inc = %client.GlArg4;
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKH\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Avg Per Game";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7><lmargin%:0>  Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"score",%inc,%game),getGameTotal(%vClient,"score",%game),mCeil(getGameTotalAvg(%vClient,"score",%game)));
         %line = '<color:0befe7>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"kills",%inc,%game),getGameTotal(%vClient,"kills",%game),mCeil(getGameTotalAvg(%vClient,"kills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"deaths",%inc,%game),getGameTotal(%vClient,"deaths",%game),mCeil(getGameTotalAvg(%vClient,"deaths",%game)));
         %line = '<color:0befe7>  Suicides<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"suicides",%inc,%game),getGameTotal(%vClient,"suicides",%game),mCeil(getGameTotalAvg(%vClient,"suicides",%game)));
         %line = '<color:0befe7>  Midairs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"mas",%inc,%game),getGameTotal(%vClient,"mas",%game),mCeil(getGameTotalAvg(%vClient,"mas",%game)));         
		 %line = '<color:0befe7>  Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"flagGrabs",%inc,%game),getGameTotal(%vClient,"flagGrabs",%game),mCeil(getGameTotalAvg(%vClient,"flagGrabs",%game)));
		 %line = '<color:0befe7>  Midair Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"MidairflagGrabs",%inc,%game),getGameTotal(%vClient,"MidairflagGrabs",%game),mCeil(getGameTotalAvg(%vClient,"MidairflagGrabs",%game)));
         %line = '<color:0befe7>  Midair Flag Grab Points<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"MidairflagGrabPoints",%inc,%game),getGameTotal(%vClient,"MidairflagGrabPoints",%game),mCeil(getGameTotalAvg(%vClient,"MidairflagGrabPoints",%game)));
         %line = '<color:0befe7>  Flag Time Minutes<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"flagTimeMS",%inc,%game),getGameTotal(%vClient,"flagTimeMS",%game),mCeil(getGameTotalAvg(%vClient,"flagTimeMS",%game)));
         %line = '<color:0befe7>  Bonus Points<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"morepoints",%inc,%game),getGameTotal(%vClient,"morepoints",%game),mCeil(getGameTotalAvg(%vClient,"morepoints",%game)));
         %line = '<color:0befe7>  Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"minePlusDisc",%inc,%game),getGameTotal(%vClient,"minePlusDisc",%game),mCeil(getGameTotalAvg(%vClient,"minePlusDisc",%game)));
         %line = '<color:0befe7>  Total Speed<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalSpeed",%inc),getGameTotal(%vClient,"totalSpeed",%game),mCeil(getGameTotalAvg(%vClient,"totalSpeed",%game)));
         %line = '<color:0befe7>  Total Distance<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalDistance",%inc,%game),getGameTotal(%vClient,"totalDistance",%game),mCeil(getGameTotalAvg(%vClient,"totalDistance",%game)));
         //%line = '<color:0befe7>Total Chain Accuracy<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalChainAccuracy",%inc,%game),getGameTotal(%vClient,"totalChainAccuracy",%game),mCeil(getGameTotalAvg(%vClient,"totalChainAccuracy",%game)));
         //%line = '<color:0befe7>Total Chain Hits Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalChainHits",%inc,%game),getGameTotal(%vClient,"totalChainHits",%game),mCeil(getGameTotalAvg(%vClient,"totalChainHits",%game)));
         //%line = '<color:0befe7>Total Snipe Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalSnipeHits",%inc,%game),getGameTotal(%vClient,"totalSnipeHits",%game),mCeil(getGameTotalAvg(%vClient,"totalSnipeHits",%game)));
         //%line = '<color:0befe7>Total Snipes<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalSnipes",%inc,%game),getGameTotal(%vClient,"totalSnipes",%game),mCeil(getGameTotalAvg(%vClient,"totalSnipes",%game)));
         %line = '<color:0befe7>  Total Shock Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalShockHits",%inc,%game),getGameTotal(%vClient,"totalShockHits",%game),mCeil(getGameTotalAvg(%vClient,"totalShockHits",%game)));
         %line = '<color:0befe7>  Total Shocks<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"totalShocks",%inc,%game),getGameTotal(%vClient,"totalShocks",%game),mCeil(getGameTotalAvg(%vClient,"totalShocks",%game)));
 
	  case "LAKW":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         //%header = "<color:0befe7>Weapons";
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tChaingunLAK\t%1> View Chaingun Stats</a><lmargin:230> <bitmap:%2>';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,$twbbitMap[getRandom(1,56)]);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tSpinfusorLAK\t%1>  + Spinfusor Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tGrenadeLauncherLAK\t%1>  + Grenade Launcher Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tLaserRifleLAK\t%1> View Laser Rifle Stats</a>';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tFusionMortarLAK\t%1>  + Fusion Mortar Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tMissileLauncherLAK\t%1> View Missile Launcher Stats</a>';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tShocklanceLAK\t%1>  + Shocklance Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tPlasmaRifleLAK\t%1>  + Plasma Rifle Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tBlasterLAK\t%1>  + Blaster Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         //%line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tELFLAK\t%1> View ELF Projector Stats</a>';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
      case "LAKH":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Lak History");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Game history is set to" SPC $dtStats::MaxNumOfGames SPC "games.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>The oldest game will be overwritten.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         if(%vClient.dtStats.gameCount[%game] >= $dtStats::MaxNumOfGames){
            %in = %vClient.dtStats.statsOverWrite[%game] + 1;
            if(%in > $dtStats::MaxNumOfGames){
               %in = 1;
            }
            for(%b = %in; %b <= %vClient.dtStats.gameCount[%game]; %b++){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%b,%game];
               %map = %vClient.dtStats.gameStats["map",%b,%game];
               if(%b == %in){
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3> + %4 - %2</a> <color:02d404><just:center>This game will be overwritten',%vClient,%timeDate,%b,%map);
               }
               else{
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3> + %4 - %2</a> ',%vClient,%timeDate,%b,%map);
               }
            }
            for(%z = 1; %z < %in; %z++){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%z,%game];
               %map = %vClient.dtStats.gameStats["map",%z,%game];
               messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3> + %4 - %2</a> ',%vClient,%timeDate,%z,%map);
            }
            
         }
         else{
            for(%z = 1; %z <= %vClient.dtStats.gameCount[%game]; %z++){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%z,%game];
               %map = %vClient.dtStats.gameStats["map",%z,%game];
               messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tLAKHIST\t%1\t%3> + %4 - %2</a> ',%vClient,%timeDate,%z,%map);
            }
         }
      case "Lak":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Match Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7><lmargin%:0>  Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"score",%game)),getGameTotal(%vClient,"score",%game),mCeil(getGameTotalAvg(%vClient,"score",%game)));
         %line = '<color:0befe7>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"kills",%game)),getGameTotal(%vClient,"kills",%game),mCeil(getGameTotalAvg(%vClient,"kills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"deaths",%game)),getGameTotal(%vClient,"deaths",%game),mCeil(getGameTotalAvg(%vClient,"deaths",%game)));
         %line = '<color:0befe7>  Suicides<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"suicides",%game)),getGameTotal(%vClient,"suicides",%game),mCeil(getGameTotalAvg(%vClient,"suicides",%game)));
         %line = '<color:0befe7>  Midairs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mas",%game)),getGameTotal(%vClient,"mas",%game),mCeil(getGameTotalAvg(%vClient,"mas",%game)));         
		 %line = '<color:0befe7>  Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"flagGrabs",%game)),getGameTotal(%vClient,"flagGrabs",%game),mCeil(getGameTotalAvg(%vClient,"flagGrabs",%game)));
		 %line = '<color:0befe7>  Midair Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"MidairflagGrabs",%inc,%game),getGameTotal(%vClient,"MidairflagGrabs",%game),mCeil(getGameTotalAvg(%vClient,"MidairflagGrabs",%game)));
         %line = '<color:0befe7>  Midair Flag Grab Points<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"MidairflagGrabPoints",%inc,%game),getGameTotal(%vClient,"MidairflagGrabPoints",%game),mCeil(getGameTotalAvg(%vClient,"MidairflagGrabPoints",%game)));         
         %line = '<color:0befe7>  Flag Time Minutes<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"flagTimeMS",%game)),getGameTotal(%vClient,"flagTimeMS",%game),mCeil(getGameTotalAvg(%vClient,"flagTimeMS",%game)));
         %line = '<color:0befe7>  Bonus Points<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"morepoints",%game)),getGameTotal(%vClient,"morepoints",%game),mCeil(getGameTotalAvg(%vClient,"morepoints",%game)));
		 %line = '<color:0befe7>  Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"minedisc",%game)),getGameTotal(%vClient,"minePlusDisc",%game),mCeil(getGameTotalAvg(%vClient,"minePlusDisc",%game)));
         %line = '<color:0befe7>  Total Speed<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalSpeed",%game)),getGameTotal(%vClient,"totalSpeed",%game),mCeil(getGameTotalAvg(%vClient,"totalSpeed",%game)));
         %line = '<color:0befe7>  Total Distance<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalDistance",%game)),getGameTotal(%vClient,"totalDistance",%game),mCeil(getGameTotalAvg(%vClient,"totalDistance",%game)));
         //%line = '<color:0befe7>Total Chain Accuracy<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalChainAccuracy",%game)),getGameTotal(%vClient,"totalChainAccuracy",%game),mCeil(getGameTotalAvg(%vClient,"totalChainAccuracy",%game)));
         //%line = '<color:0befe7>Total Chain Hits Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalChainHits",%game)),getGameTotal(%vClient,"totalChainHits",%game),mCeil(getGameTotalAvg(%vClient,"totalChainHits",%game)));
         //%line = '<color:0befe7>Total Snipe Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalSnipeHits",%game)),getGameTotal(%vClient,"totalSnipeHits",%game),mCeil(getGameTotalAvg(%vClient,"totalSnipeHits",%game)));
         //%line = '<color:0befe7>Total Snipes<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalSnipes",%game)),getGameTotal(%vClient,"totalSnipes",%game),mCeil(getGameTotalAvg(%vClient,"totalSnipes",%game)));
         %line = '<color:0befe7>  Total Shock Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalShockHits",%game)),getGameTotal(%vClient,"totalShockHits",%game),mCeil(getGameTotalAvg(%vClient,"totalShockHits",%game)));
         %line = '<color:0befe7>  Total Shocks<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"totalShocks",%game)),getGameTotal(%vClient,"totalShocks",%game),mCeil(getGameTotalAvg(%vClient,"totalShocks",%game)));

      case "CTFA":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Kills/Deaths");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         %a1 = getGameTotal(%vClient,"cgKills",%game); %b2 = getGameTotal(%vClient,"cgDeaths",%game); %c3 = getGameTotal(%vClient,"discKills",%game);
         %d4 = getGameTotal(%vClient,"discDeaths",%game); %e5 = getGameTotal(%vClient,"grenadeKills",%game); %f6 = getGameTotal(%vClient,"grenadeDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Chaingun: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Spinfusor: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Grenade Launcher: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"laserKills",%game); %b2 = getGameTotal(%vClient,"laserDeaths",%game); %c3 = getGameTotal(%vClient,"mortarKills",%game);
         %d4 = getGameTotal(%vClient,"mortarDeaths",%game); %e5 = getGameTotal(%vClient,"shockLanceKills",%game); %f6 = getGameTotal(%vClient,"shockLanceDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Laser Rifle: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Fusion Mortar: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Shocklance: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"plasmaKills",%game); %b2 = getGameTotal(%vClient,"plasmaDeaths",%game); %c3 = getGameTotal(%vClient,"blasterKills",%game);
         %d4 = getGameTotal(%vClient,"blasterDeaths",%game); %e5 = getGameTotal(%vClient,"elfKills",%game); %f6 = getGameTotal(%vClient,"elfDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Plasma Rifle: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Blaster: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>ELF Projector: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, " -----------------------------------------------------------------------------------------------------------------");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
		 messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
		 
         %a1 = getGameTotal(%vClient,"mineKills",%game); %b2 = getGameTotal(%vClient,"mineDeaths",%game); %c3 = getGameTotal(%vClient,"explosionKills",%game);
         %d4 = getGameTotal(%vClient,"explosionDeaths",%game); %e5 = getGameTotal(%vClient,"impactKills",%game); %f6 = getGameTotal(%vClient,"impactDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Mines: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Explosion: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Impact: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"groundKills"); %b2 = getGameTotal(%vClient,"groundDeaths"); %c3 = getGameTotal(%vClient,"turretKills");
         %d4 = getGameTotal(%vClient,"turretDeaths",%game); %e5 = getGameTotal(%vClient,"plasmaTurretKills",%game); %f6 = getGameTotal(%vClient,"plasmaTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Ground: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Plasma Turret: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"aaTurretKills"); %b2 = getGameTotal(%vClient,"aaTurretDeaths"); %c3 = getGameTotal(%vClient,"elfTurretKills");
         %d4 = getGameTotal(%vClient,"elfTurretDeaths",%game); %e5 = getGameTotal(%vClient,"mortarTurretKills",%game); %f6 = getGameTotal(%vClient,"mortarTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  AA Turret: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>ELF Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Mortar Turret: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"missileTurretKills",%game); %b2 = getGameTotal(%vClient,"missileTurretDeaths",%game); %c3 = getGameTotal(%vClient,"indoorDepTurretKills",%game);
         %d4 = getGameTotal(%vClient,"indoorDepTurretDeaths",%game); %e5 = getGameTotal(%vClient,"outdoorDepTurretKills",%game); %f6 = getGameTotal(%vClient,"outdoorDepTurretDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Missile Turret: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Spider Camp Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Land Spike Turret: <color:02d404>%5k/%6d';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         %a1 = getGameTotal(%vClient,"sentryTurretKills",%game); %b2 = getGameTotal(%vClient,"sentryTurretDeaths",%game); %c3 = getGameTotal(%vClient,"outOfBoundKills",%game);
         %d4 = getGameTotal(%vClient,"outOfBoundDeaths",%game); %e5 = getGameTotal(%vClient,"lavaKills",%game); %f6 = getGameTotal(%vClient,"lavaDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Sentry Turret: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Out Of Bounds: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Lava: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"shrikeBlasterKills",%game); %b2 = getGameTotal(%vClient,"shrikeBlasterDeaths",%game); %c3 = getGameTotal(%vClient,"bellyTurretKills",%game);
         %d4 = getGameTotal(%vClient,"bellyTurretDeaths",%game); %e5 = getGameTotal(%vClient,"bomberBombsKills",%game); %f6 = getGameTotal(%vClient,"bomberBombsDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Shrike Blaster: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Bomber Turret: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Bomber Bombs: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"tankChaingunKills",%game); %b2 = getGameTotal(%vClient,"tankChaingunDeaths",%game); %c3 = getGameTotal(%vClient,"tankMortarKills",%game);
         %d4 = getGameTotal(%vClient,"tankMortarDeaths",%game); %e5 = getGameTotal(%vClient,"mpbMissileKills",%game); %f6 = getGameTotal(%vClient,"mpbMissileDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Tank Chaingun: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Tank Mortar: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>MPB Missile: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
         %a1 = getGameTotal(%vClient,"satchelChargeKills",%game); %b2 = getGameTotal(%vClient,"satchelChargeDeaths",%game); %c3 = getGameTotal(%vClient,"lightningKills",%game);
         %d4 = getGameTotal(%vClient,"lightningDeaths",%game); %e5 = getGameTotal(%vClient,"vehicleSpawnKills",%game); %f6 = getGameTotal(%vClient,"vehicleSpawnDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Satchel Charge: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Lightning: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Vehicle Spawn: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"forceFieldPowerUpKills",%game); %b2 = getGameTotal(%vClient,"forceFieldPowerUpDeaths",%game); %c3 = getGameTotal(%vClient,"crashKills",%game);
         %d4 = getGameTotal(%vClient,"crashDeaths",%game); %e5 = getGameTotal(%vClient,"waterKills",%game); %f6 = getGameTotal(%vClient,"waterDeaths",%game);
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Forcefield Power: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Crash: <color:02d404>%3k/%4d<color:0befe7><lmargin:380>Water: <color:02d404>%5k/%6d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         %a1 = getGameTotal(%vClient,"nexusCampingKills",%game); %b2 = getGameTotal(%vClient,"nexusCampingDeaths",%game); %c3 = getGameTotal(%vClient,"unknownKill",%game);
         %d4 = getGameTotal(%vClient,"unknownDeaths",%game); %e5 = 0; %f6 = 0;
         %line = '<font:univers condensed:18><color:0befe7><lmargin:0>  Nexus Camping: <color:02d404>%1k/%2d<color:0befe7><lmargin:175>Unknown??: <color:02d404>%3k/%4d<color:0befe7>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%a1,%b2,%c3,%d4,%e5,%f6);
         
      case "CTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s Match Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"kills",%game)),getGameTotal(%vClient,"kills",%game),mCeil(getGameTotalAvg(%vClient,"kills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"deaths",%game)),getGameTotal(%vClient,"deaths",%game),mCeil(getGameTotalAvg(%vClient,"deaths",%game)));
         %line = '<color:0befe7>  Mid-Air<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreMidAir",%game)),getGameTotal(%vClient,"scoreMidAir",%game),mCeil(getGameTotalAvg(%vClient,"scoreMidAir",%game)));
         %line = '<color:0befe7>  Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"minePlusDisc",%game)),getGameTotal(%vClient,"minePlusDisc",%game),mCeil(getGameTotalAvg(%vClient,"minePlusDisc",%game)));
         %line = '<color:0befe7>  Flag Caps<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"flagCaps",%game)),getGameTotal(%vClient,"flagCaps",%game),mCeil(getGameTotalAvg(%vClient,"flagCaps",%game)));
         %line = '<color:0befe7>  Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"flagGrabs",%game)),getGameTotal(%vClient,"flagGrabs",%game),mCeil(getGameTotalAvg(%vClient,"flagGrabs",%game)));
         %line = '<color:0befe7>  Carrier Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"carrierKills",%game)),getGameTotal(%vClient,"carrierKills",%game),mCeil(getGameTotalAvg(%vClient,"carrierKills",%game)));
         %line = '<color:0befe7>  Flag Returns<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"flagReturns",%game)),getGameTotal(%vClient,"flagReturns",%game),mCeil(getGameTotalAvg(%vClient,"flagReturns",%game)));
         %line = '<color:0befe7>  Escort Assists<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"escortAssists",%game)),getGameTotal(%vClient,"escortAssists",%game),mCeil(getGameTotalAvg(%vClient,"escortAssists",%game)));
         %line = '<color:0befe7>  Flag Defends<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"flagDefends",%game)),getGameTotal(%vClient,"flagDefends",%game),mCeil(getGameTotalAvg(%vClient,"flagDefends",%game)));
         %line = '<color:0befe7>  Offense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"offenseScore",%game)),getGameTotal(%vClient,"offenseScore",%game),mCeil(getGameTotalAvg(%vClient,"offenseScore",%game)));
         %line = '<color:0befe7>  Defense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"defenseScore",%game)),getGameTotal(%vClient,"defenseScore",%game),mCeil(getGameTotalAvg(%vClient,"defenseScore",%game)));
         %line = '<color:0befe7>  Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"score",%game)),getGameTotal(%vClient,"score",%game),mCeil(getGameTotalAvg(%vClient,"score",%game)));
         %line = '<color:0befe7>  Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreRearshot",%game)),getGameTotal(%vClient,"scoreRearshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreRearshot",%game)));
         %line = '<color:0befe7>  Headshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreHeadshot",%game)),getGameTotal(%vClient,"scoreHeadshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreHeadshot",%game)));
      case "CTFW":// Weapons
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Weapon Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         //%header = "<color:0befe7>Weapons";
         //messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tChaingunCTF\t%1>  + Chaingun Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line, %vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tSpinfusorCTF\t%1>  + Spinfusor Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tGrenadeLauncherCTF\t%1>  + Grenade Launcher Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tLaserRifleCTF\t%1>  + Laser Rifle Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tFusionMortarCTF\t%1>  + Fusion Mortar Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tMissileLauncherCTF\t%1>  + Missile Launcher Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tShocklanceCTF\t%1>  + Shocklance Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tPlasmaRifleCTF\t%1>  + Plasma Rifle Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tBlasterCTF\t%1>  + Blaster Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         %line = '<spush><color:00dcd4><lmargin%:0><a:gamelink\tStats\tELFCTF\t%1>  + ELF Projector Stats</a>';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient);
         
      case "CTFH":// Past Games
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.namebase @ "'s CTF History");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tView\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>Game history is set to" SPC $dtStats::MaxNumOfGames SPC "games.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "<just:center>The oldest game will be overwritten.");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         messageClient( %client, 'SetLineHud', "", %tag, %index++, "");
         if(%vClient.dtStats.gameCount[%game] >= $dtStats::MaxNumOfGames){
            %in = %vClient.dtStats.statsOverWrite[%game] + 1;
            if(%in > $dtStats::MaxNumOfGames){
               %in = 1;
            }
            for(%b = %in; %b <= %vClient.dtStats.gameCount[%game]; %b++){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%b,%game];
               %map = %vClient.dtStats.gameStats["map",%b,%game];
               if(%b == %in){
                  messageClient( %client, 'SetLineHud', "", %tag, %index++, '<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3> + %4 - %2</a> <color:02d404><just:center>This game will be overwritten',%vClient,%timeDate,%b,%map);
               }
               else{
                  messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3> + %4 - %2</a> ',%vClient,%timeDate,%b,%map);
               }
            }
            for(%z = 1; %z < %in; %z++){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%z,%game];
               %map = %vClient.dtStats.gameStats["map",%z,%game];
               messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3> + %4 - %2</a> ',%vClient,%timeDate,%z,%map);
            }
            
         }
         else{
            for(%z = 1; %z <= %vClient.dtStats.gameCount[%game]; %z++){
               %timeDate = %vClient.dtStats.gameStats["timeStamp",%z,%game];
               %map = %vClient.dtStats.gameStats["map",%z,%game];
               messageClient( %client, 'SetLineHud', "", %tag, %index++,'<color:0befe7><a:gamelink\tStats\tCTFHist\t%1\t%3> + %4 - %2</a> ',%vClient,%timeDate,%z,%map);
            }
         }
      case "CTFHist":
         %inc = %client.GlArg4;
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>" @ %vClient.dtStats.gameStats["map",%inc,%game] SPC %vClient.dtStats.gameStats["timeStamp",%inc,%game]);
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFH\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Avg Per Game";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"kills",%inc,%game),getGameTotal(%vClient,"kills",%game),mCeil(getGameTotalAvg(%vClient,"kills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"deaths",%inc,%game),getGameTotal(%vClient,"deaths",%game),mCeil(getGameTotalAvg(%vClient,"deaths",%game)));
         %line = '<color:0befe7>  Mid-Air<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"scoreMidAir",%inc,%game),getGameTotal(%vClient,"scoreMidAir",%game),mCeil(getGameTotalAvg(%vClient,"scoreMidAir",%game)));
         %line = '<color:0befe7>  Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"minePlusDisc",%inc,%game),getGameTotal(%vClient,"minePlusDisc",%game),mCeil(getGameTotalAvg(%vClient,"minePlusDisc",%game)));
         %line = '<color:0befe7>  Flag Caps<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"flagCaps",%inc,%game),getGameTotal(%vClient,"flagCaps",%game),mCeil(getGameTotalAvg(%vClient,"flagCaps",%game)));
         %line = '<color:0befe7>  Flag Grabs<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"flagGrabs",%inc,%game),getGameTotal(%vClient,"flagGrabs",%game),mCeil(getGameTotalAvg(%vClient,"flagGrabs",%game)));
         %line = '<color:0befe7>  Carrier Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"carrierKills",%inc,%game),getGameTotal(%vClient,"carrierKills",%game),mCeil(getGameTotalAvg(%vClient,"carrierKills",%game)));
         %line = '<color:0befe7>  Flag Returns<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"flagReturns",%inc,%game),getGameTotal(%vClient,"flagReturns",%game),mCeil(getGameTotalAvg(%vClient,"flagReturns",%game)));
         %line = '<color:0befe7>  Escort Assists<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"escortAssists",%inc,%game),getGameTotal(%vClient,"escortAssists",%game),mCeil(getGameTotalAvg(%vClient,"escortAssists",%game)));
         %line = '<color:0befe7>  Flag Defends<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"flagDefends",%inc,%game),getGameTotal(%vClient,"flagDefends",%game),mCeil(getGameTotalAvg(%vClient,"flagDefends",%game)));
         %line = '<color:0befe7>  Offense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"offenseScore",%inc,%game),getGameTotal(%vClient,"offenseScore",%game),mCeil(getGameTotalAvg(%vClient,"offenseScore",%game)));
         %line = '<color:0befe7>  Defense Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"defenseScore",%inc,%game),getGameTotal(%vClient,"defenseScore",%game),mCeil(getGameTotalAvg(%vClient,"defenseScore",%game)));
         %line = '<color:0befe7>  Score<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"score",%inc,%game),getGameTotal(%vClient,"score",%game),mCeil(getGameTotalAvg(%vClient,"score",%game)));
         %line = '<color:0befe7>  Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"scoreRearshot",%inc,%game),getGameTotal(%vClient,"scoreRearshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreRearshot",%game)));
         %line = '<color:0befe7>  Headshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,getGameDetails(%vClient,"scoreHeadshot",%inc,%game),getGameTotal(%vClient,"scoreHeadshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreHeadshot",%game)));
      case "BlasterCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterKills",%game)),getGameTotal(%vClient,"blasterKills",%game),mCeil(getGameTotalAvg(%vClient,"blasterKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDeaths",%game)),getGameTotal(%vClient,"blasterDeaths",%game),mCeil(getGameTotalAvg(%vClient,"blasterDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDmg",%game)),getGameTotal(%vClient,"blasterDmg",%game),mCeil(getGameTotalAvg(%vClient,"blasterDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDmgTaken",%game)),getGameTotal(%vClient,"blasterDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"blasterDmgTaken",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDirectHits",%game)),getGameTotal(%vClient,"blasterDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"blasterDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterShotsFired",%game)),getGameTotal(%vClient,"blasterShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"blasterShotsFired",%game)));
      case "SpinfusorCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discKills",%game)),getGameTotal(%vClient,"discKills",%game),mCeil(getGameTotalAvg(%vClient,"discKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDeaths",%game)),getGameTotal(%vClient,"discDeaths",%game),mCeil(getGameTotalAvg(%vClient,"discDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDmg",%game)),getGameTotal(%vClient,"discDmg",%game),mCeil(getGameTotalAvg(%vClient,"discDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDmgTaken",%game)),getGameTotal(%vClient,"discDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"discDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discInDmg",%game)),getGameTotal(%vClient,"discInDmg",%game),mCeil(getGameTotalAvg(%vClient,"discInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discInDmgTaken",%game)),getGameTotal(%vClient,"discInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"discInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discIndirectHits",%game)),getGameTotal(%vClient,"discIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"discIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDirectHits",%game)),getGameTotal(%vClient,"discDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"discDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discShotsFired",%game)),getGameTotal(%vClient,"discShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"discShotsFired",%game)));
         %line = '<color:0befe7>  Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"minePlusDisc",%game)),getGameTotal(%vClient,"minePlusDisc",%game),mCeil(getGameTotalAvg(%vClient,"minePlusDisc",%game)));
      case "ChaingunCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgKills",%game)),getGameTotal(%vClient,"cgKills",%game),mCeil(getGameTotalAvg(%vClient,"cgKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDeaths",%game)),getGameTotal(%vClient,"cgDeaths",%game),mCeil(getGameTotalAvg(%vClient,"cgDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDmg",%game)),getGameTotal(%vClient,"cgDmg",%game),mCeil(getGameTotalAvg(%vClient,"cgDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDmgTaken",%game)),getGameTotal(%vClient,"cgDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"cgDmgTaken",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDirectHits",%game)),getGameTotal(%vClient,"cgDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"cgDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgShotsFired",%game)),getGameTotal(%vClient,"cgShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"cgShotsFired",%game)));
      case "GrenadeLauncherCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeKills",%game)),getGameTotal(%vClient,"grenadeKills",%game),mCeil(getGameTotalAvg(%vClient,"grenadeKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDeaths",%game)),getGameTotal(%vClient,"grenadeDeaths",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDmg",%game)),getGameTotal(%vClient,"grenadeDmg",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDmgTaken",%game)),getGameTotal(%vClient,"grenadeDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeInDmg",%game)),getGameTotal(%vClient,"grenadeInDmg",%game),mCeil(getGameTotalAvg(%vClient,"grenadeInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeInDmgTaken",%game)),getGameTotal(%vClient,"grenadeInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"grenadeInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeIndirectHits",%game)),getGameTotal(%vClient,"grenadeIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"grenadeIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDirectHits",%game)),getGameTotal(%vClient,"grenadeDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeShotsFired",%game)),getGameTotal(%vClient,"grenadeShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"grenadeShotsFired",%game)));
      case "LaserRifleCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserKills",%game)),getGameTotal(%vClient,"laserKills",%game),mCeil(getGameTotalAvg(%vClient,"laserKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDeaths",%game)),getGameTotal(%vClient,"laserDeaths",%game),mCeil(getGameTotalAvg(%vClient,"laserDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDmg",%game)),getGameTotal(%vClient,"laserDmg",%game),mCeil(getGameTotalAvg(%vClient,"laserDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDmgTaken",%game)),getGameTotal(%vClient,"laserDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"laserDmgTaken",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDirectHits",%game)),getGameTotal(%vClient,"laserDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"laserDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserShotsFired",%game)),getGameTotal(%vClient,"laserShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"laserShotsFired",%game)));
         %line = '<color:0befe7>  Head Shots <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreHeadshot",%game)),getGameTotal(%vClient,"scoreHeadshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreHeadshot",%game)));
         
      case "FusionMortarCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarKills",%game)),getGameTotal(%vClient,"mortarKills",%game),mCeil(getGameTotalAvg(%vClient,"mortarKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDeaths",%game)),getGameTotal(%vClient,"mortarDeaths",%game),mCeil(getGameTotalAvg(%vClient,"mortarDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDmg",%game)),getGameTotal(%vClient,"mortarDmg",%game),mCeil(getGameTotalAvg(%vClient,"mortarDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDmgTaken",%game)),getGameTotal(%vClient,"mortarDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"mortarDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarInDmg",%game)),getGameTotal(%vClient,"mortarInDmg",%game),mCeil(getGameTotalAvg(%vClient,"mortarInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarInDmgTaken",%game)),getGameTotal(%vClient,"mortarInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"mortarInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarIndirectHits",%game)),getGameTotal(%vClient,"mortarIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"mortarIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDirectHits",%game)),getGameTotal(%vClient,"mortarDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"mortarDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarShotsFired",%game)),getGameTotal(%vClient,"mortarShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"mortarShotsFired",%game)));
      case "MissileLauncherCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileKills",%game)),getGameTotal(%vClient,"missileKills",%game),mCeil(getGameTotalAvg(%vClient,"missileKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDeaths",%game)),getGameTotal(%vClient,"missileDeaths",%game),mCeil(getGameTotalAvg(%vClient,"missileDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDmg",%game)),getGameTotal(%vClient,"missileDmg",%game),mCeil(getGameTotalAvg(%vClient,"missileDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDmgTaken",%game)),getGameTotal(%vClient,"missileDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"missileDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileInDmg",%game)),getGameTotal(%vClient,"missileInDmg",%game),mCeil(getGameTotalAvg(%vClient,"missileInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileInDmgTaken",%game)),getGameTotal(%vClient,"missileInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"missileInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileIndirectHits",%game)),getGameTotal(%vClient,"missileIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"missileIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDirectHits",%game)),getGameTotal(%vClient,"missileDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"missileDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileShotsFired",%game)),getGameTotal(%vClient,"missileShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"missileShotsFired",%game)));
      case "ShocklanceCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceKills",%game)),getGameTotal(%vClient,"shockLanceKills",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDeaths",%game)),getGameTotal(%vClient,"shockLanceDeaths",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDmg",%game)),getGameTotal(%vClient,"shockLanceDmg",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDmgTaken",%game)),getGameTotal(%vClient,"shockLanceDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDmgTaken",%game)));
         
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDirectHits",%game)),getGameTotal(%vClient,"shockLanceDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceShotsFired",%game)),getGameTotal(%vClient,"shockLanceShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceShotsFired",%game)));
         %line = '<color:0befe7>  Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreRearshot",%game)),getGameTotal(%vClient,"scoreRearshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreRearshot",%game)));
      case "PlasmaRifleCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaKills",%game)),getGameTotal(%vClient,"plasmaKills",%game),mCeil(getGameTotalAvg(%vClient,"plasmaKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDeaths",%game)),getGameTotal(%vClient,"plasmaDeaths",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDmg",%game)),getGameTotal(%vClient,"plasmaDmg",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDmgTaken",%game)),getGameTotal(%vClient,"plasmaDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaInDmg",%game)),getGameTotal(%vClient,"plasmaInDmg",%game),mCeil(getGameTotalAvg(%vClient,"plasmaInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaInDmgTaken",%game)),getGameTotal(%vClient,"plasmaInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"plasmaInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaIndirectHits",%game)),getGameTotal(%vClient,"plasmaIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"plasmaIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDirectHits",%game)),getGameTotal(%vClient,"plasmaDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaShotsFired")),getGameTotal(%vClient,"plasmaShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"plasmaShotsFired",%game)));
      case "ELFCTF":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tCTFW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"elfShotsFired",%game)),getGameTotal(%vClient,"elfShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"elfShotsFired",%game)));
         
      case "BlasterLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Blaster Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterKills",%game)),getGameTotal(%vClient,"blasterKills",%game),mCeil(getGameTotalAvg(%vClient,"blasterKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDeaths",%game)),getGameTotal(%vClient,"blasterDeaths",%game),mCeil(getGameTotalAvg(%vClient,"blasterDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDmg",%game)),getGameTotal(%vClient,"blasterDmg",%game),mCeil(getGameTotalAvg(%vClient,"blasterDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDmgTaken",%game)),getGameTotal(%vClient,"blasterDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"blasterDmgTaken",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterDirectHits",%game)),getGameTotal(%vClient,"blasterDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"blasterDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"blasterShotsFired",%game)),getGameTotal(%vClient,"blasterShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"blasterShotsFired",%game)));
         
      case "SpinfusorLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Spinfusor Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discKills",%game)),getGameTotal(%vClient,"discKills",%game),mCeil(getGameTotalAvg(%vClient,"discKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDeaths",%game)),getGameTotal(%vClient,"discDeaths",%game),mCeil(getGameTotalAvg(%vClient,"discDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDmg",%game)),getGameTotal(%vClient,"discDmg",%game),mCeil(getGameTotalAvg(%vClient,"discDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDmgTaken",%game)),getGameTotal(%vClient,"discDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"discDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discInDmg",%game)),getGameTotal(%vClient,"discInDmg",%game),mCeil(getGameTotalAvg(%vClient,"discInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discInDmgTaken",%game)),getGameTotal(%vClient,"discInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"discInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discIndirectHits",%game)),getGameTotal(%vClient,"discIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"discIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discDirectHits",%game)),getGameTotal(%vClient,"discDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"discDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"discShotsFired",%game)),getGameTotal(%vClient,"discShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"discShotsFired",%game)));
         %line = '<color:0befe7>  Mine + Disc<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"minePlusDisc",%game)),getGameTotal(%vClient,"minePlusDisc",%game),mCeil(getGameTotalAvg(%vClient,"minePlusDisc",%game)));
         
      case "ChaingunLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Chaingun Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgKills",%game)),getGameTotal(%vClient,"cgKills",%game),mCeil(getGameTotalAvg(%vClient,"cgKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDeaths",%game)),getGameTotal(%vClient,"cgDeaths",%game),mCeil(getGameTotalAvg(%vClient,"cgDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDmg",%game)),getGameTotal(%vClient,"cgDmg",%game),mCeil(getGameTotalAvg(%vClient,"cgDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDmgTaken",%game)),getGameTotal(%vClient,"cgDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"cgDmgTaken",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgDirectHits",%game)),getGameTotal(%vClient,"cgDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"cgDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"cgShotsFired",%game)),getGameTotal(%vClient,"cgShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"cgShotsFired",%game)));
         
      case "GrenadeLauncherLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Grenade Launcher Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeKills",%game)),getGameTotal(%vClient,"grenadeKills",%game),mCeil(getGameTotalAvg(%vClient,"grenadeKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDeaths",%game)),getGameTotal(%vClient,"grenadeDeaths",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDmg",%game)),getGameTotal(%vClient,"grenadeDmg",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDmgTaken",%game)),getGameTotal(%vClient,"grenadeDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeInDmg",%game)),getGameTotal(%vClient,"grenadeInDmg",%game),mCeil(getGameTotalAvg(%vClient,"grenadeInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeInDmgTaken",%game)),getGameTotal(%vClient,"grenadeInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"grenadeInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeIndirectHits",%game)),getGameTotal(%vClient,"grenadeIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"grenadeIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeDirectHits",%game)),getGameTotal(%vClient,"grenadeDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"grenadeDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"grenadeShotsFired",%game)),getGameTotal(%vClient,"grenadeShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"grenadeShotsFired",%game)));
         
      case "LaserRifleLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Laser Rifle Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserKills",%game)),getGameTotal(%vClient,"laserKills",%game),mCeil(getGameTotalAvg(%vClient,"laserKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDeaths",%game)),getGameTotal(%vClient,"laserDeaths",%game),mCeil(getGameTotalAvg(%vClient,"laserDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDmg",%game)),getGameTotal(%vClient,"laserDmg",%game),mCeil(getGameTotalAvg(%vClient,"laserDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDmgTaken",%game)),getGameTotal(%vClient,"laserDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"laserDmgTaken",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserDirectHits",%game)),getGameTotal(%vClient,"laserDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"laserDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"laserShotsFired",%game)),getGameTotal(%vClient,"laserShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"laserShotsFired",%game)));
         %line = '<color:0befe7>  Head Shots <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreHeadshot",%game)),getGameTotal(%vClient,"scoreHeadshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreHeadshot",%game)));
         
      case "FusionMortarLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Fusion Mortar Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarKills",%game)),getGameTotal(%vClient,"mortarKills",%game),mCeil(getGameTotalAvg(%vClient,"mortarKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDeaths",%game)),getGameTotal(%vClient,"mortarDeaths",%game),mCeil(getGameTotalAvg(%vClient,"mortarDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDmg",%game)),getGameTotal(%vClient,"mortarDmg",%game),mCeil(getGameTotalAvg(%vClient,"mortarDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDmgTaken",%game)),getGameTotal(%vClient,"mortarDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"mortarDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarInDmg",%game)),getGameTotal(%vClient,"mortarInDmg",%game),mCeil(getGameTotalAvg(%vClient,"mortarInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarInDmgTaken",%game)),getGameTotal(%vClient,"mortarInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"mortarInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarIndirectHits",%game)),getGameTotal(%vClient,"mortarIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"mortarIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarDirectHits",%game)),getGameTotal(%vClient,"mortarDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"mortarDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"mortarShotsFired",%game)),getGameTotal(%vClient,"mortarShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"mortarShotsFired",%game)));
         
      case "MissileLauncherLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Missile Launcher Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileKills",%game)),getGameTotal(%vClient,"missileKills",%game),mCeil(getGameTotalAvg(%vClient,"missileKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDeaths",%game)),getGameTotal(%vClient,"missileDeaths",%game),mCeil(getGameTotalAvg(%vClient,"missileDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDmg",%game)),getGameTotal(%vClient,"missileDmg",%game),mCeil(getGameTotalAvg(%vClient,"missileDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDmgTaken",%game)),getGameTotal(%vClient,"missileDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"missileDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileInDmg",%game)),getGameTotal(%vClient,"missileInDmg",%game),mCeil(getGameTotalAvg(%vClient,"missileInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileInDmgTaken",%game)),getGameTotal(%vClient,"missileInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"missileInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileIndirectHits",%game)),getGameTotal(%vClient,"missileIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"missileIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileDirectHits",%game)),getGameTotal(%vClient,"missileDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"missileDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"missileShotsFired",%game)),getGameTotal(%vClient,"missileShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"missileShotsFired",%game)));
         
      case "ShocklanceLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Shocklance Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceKills",%game)),getGameTotal(%vClient,"shockLanceKills",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDeaths",%game)),getGameTotal(%vClient,"shockLanceDeaths",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDmg",%game)),getGameTotal(%vClient,"shockLanceDmg",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDmgTaken",%game)),getGameTotal(%vClient,"shockLanceDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDmgTaken",%game)));
         
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceDirectHits",%game)),getGameTotal(%vClient,"shockLanceDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"shockLanceShotsFired",%game)),getGameTotal(%vClient,"shockLanceShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"shockLanceShotsFired",%game)));
         %line = '<color:0befe7>  Rearshot<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"scoreRearshot",%game)),getGameTotal(%vClient,"scoreRearshot",%game),mCeil(getGameTotalAvg(%vClient,"scoreRearshot",%game)));
         
      case "PlasmaRifleLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>Plasma Rifle Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7><lmargin%:0>  Kills<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaKills",%game)),getGameTotal(%vClient,"plasmaKills",%game),mCeil(getGameTotalAvg(%vClient,"plasmaKills",%game)));
         %line = '<color:0befe7>  Deaths<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDeaths",%game)),getGameTotal(%vClient,"plasmaDeaths",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDeaths",%game)));
         %line = '<color:0befe7>  Direct Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDmg",%game)),getGameTotal(%vClient,"plasmaDmg",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDmg",%game)));
         %line = '<color:0befe7>  Direct Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDmgTaken",%game)),getGameTotal(%vClient,"plasmaDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDmgTaken",%game)));
         %line = '<color:0befe7>  Splash Damage<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaInDmg",%game)),getGameTotal(%vClient,"plasmaInDmg",%game),mCeil(getGameTotalAvg(%vClient,"plasmaInDmg",%game)));
         %line = '<color:0befe7>  Splash Damage Taken<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaInDmgTaken",%game)),getGameTotal(%vClient,"plasmaInDmgTaken",%game),mCeil(getGameTotalAvg(%vClient,"plasmaInDmgTaken",%game)));
         
         %line = '<color:0befe7>  Indirect Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaIndirectHits",%game)),getGameTotal(%vClient,"plasmaIndirectHits",%game),mCeil(getGameTotalAvg(%vClient,"plasmaIndirectHits",%game)));
         %line = '<color:0befe7>  Direct Hits<color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaDirectHits",%game)),getGameTotal(%vClient,"plasmaDirectHits",%game),mCeil(getGameTotalAvg(%vClient,"plasmaDirectHits",%game)));
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"plasmaShotsFired",%game)),getGameTotal(%vClient,"plasmaShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"plasmaShotsFired",%game)));
         
      case "ELFLAK":
         messageClient( %client, 'SetScoreHudHeader', "", "<just:center>ELF Projector Stats");
         messageClient( %client, 'SetScoreHudSubheader', "", '<a:gamelink\tStats\tLAKW\t%1>  Back</a>  -  <a:gamelink\tStats\tReset>Return To Score Screen</a>',%vClient);
         
         %header = "<color:0befe7><lmargin:0>       <lmargin:175> Stats<lmargin:330>Totals<lmargin:450>Totals Avg";
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %header);
         
         %line = '<color:0befe7>  Shots Fired <color:00dcd4><lmargin:180>%2<lmargin:330>%3<lmargin:450>%4';
         messageClient( %client, 'SetLineHud', "", %tag, %index++, %line,%vClient,mCeil(getGameRunAvg(%vClient,"elfShotsFired",%game)),getGameTotal(%vClient,"elfShotsFired",%game),mCeil(getGameTotalAvg(%vClient,"elfShotsFired",%game)));
         
      default://faill safe / reset
         %client.viewMenu = 0;
         %client.viewClient = 0;
         %client.viewStats = 0;
   }
}