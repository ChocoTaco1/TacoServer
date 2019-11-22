// LateOveridesGame.cs
//
// Created to overrides that need to be executed later in the server loading process.
// Sometimes this is necessary.
//


// autoexec/GetTeamCounts.cs Overrides
// Proper Overrides
// Events that determine a TeamGetCounts update
package TeamCountsTriggers
{

function DefaultGame::clientJoinTeam( %game, %client, %team, %respawn )
{
	Parent::clientJoinTeam( %game, %client, %team, %respawn );

	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::clientChangeTeam(%game, %client, %team, %fromObs, %respawned)
{
   Parent::clientChangeTeam(%game, %client, %team, %fromObs, %respawned);
   
   //Trigger GetCounts
   ResetClientChangedTeams();
}

function DefaultGame::assignClientTeam(%game, %client, %respawn )
{
	Parent::assignClientTeam(%game, %client, %respawn );
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::onClientEnterObserverMode( %game, %client )
{
	Parent::onClientEnterObserverMode( %game, %client );
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::AIChangeTeam(%game, %client, %newTeam)
{
	Parent::AIChangeTeam(%game, %client, %newTeam);

	//Trigger GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::forceObserver(%game, %client, %reason)
{
	Parent::forceObserver(%game, %client, %reason);
   
	//Trigger GetCounts
	ResetClientChangedTeams();
}

function GameConnection::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch)
{
	Parent::onConnect(%client, %name, %raceGender, %skin, %voice, %voicePitch);
   
	//Reset GetCounts
	ResetClientChangedTeams();
}

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset GetCounts
	ResetClientChangedTeams();
}

function GameConnection::onDrop(%client, %reason)
{
	Parent::onDrop(%client, %reason);
	
	//Reset GetCounts
	ResetClientChangedTeams();
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TeamCountsTriggers))
    activatePackage(TeamCountsTriggers);
