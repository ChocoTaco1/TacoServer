// anti NoFog Snipe by Red Shifter
// A far cry to the solution of noFog, but this'll stop the snipes
// This is a Server-Side Script

package antiNoFogSnipe {

function DefaultGame::missionLoadDone(%game) {

Parent::missionLoadDone(%game);

if (Sky.visibleDistance $= "" || Sky.visibleDistance == 0) {
// This script plays it safe. You better have a map that works.
error("WARNING! This map will not work with NoFog Snipe!");
BasicSniperShot.maxRifleRange = 1000;
}
else
BasicSniperShot.maxRifleRange = Sky.visibleDistance;
}

};
activatePackage(antiNoFogSnipe);
