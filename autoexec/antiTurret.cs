$TurretPlayerCount = 10; // amount of players needed on server for turrets

package antiTurret {

function TurretData::selectTarget(%this, %turret)
{
   if( !$Host::TournamentMode && $TotalTeamPlayerCount < $TurretPlayerCount) {
      %turret.clearTarget();
   }
   else {
      Parent::selectTarget(%this, %turret);
   }
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(antiTurret))
    activatePackage(antiTurret);