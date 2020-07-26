package TKwarn
{


// From Evo
function DefaultGame::testTeamKill(%game, %victimID, %killerID)
{
   %tk = Parent::testTeamKill(%game, %victimID, %killerID);
   if(!%tk)		
      return false; // is not a tk
  
   if($Host::TournamentMode || %killerID.isAdmin || %killerID.isAIcontrolled() || %victimID.isAIcontrolled())
      return true;

   // warn the player
   if((%killerID.teamkills == $Host::TKWarn1 - 1) && $Host::TKWarn1 != 0)
      centerprint(%killerID, "<font:Univers Bold:26><color:ff2222>You have teamkilled " @ %killerID.teamkills + 1 @ " players.\nCut it out!", 5, 3);
   // warn the player of his imminent kick
   else if((%killerID.teamkills == $Host::TKWarn2 - 1) && $Host::TKWarn2 != 0)
      centerprint(%killerID, "<font:Univers Bold:26><color:ff2222>You have teamkilled " @ %killerID.teamkills + 1 @ " players.\nWith " @ $Host::TKMax @ " teamkills, you will be kicked.", 5, 3);
   // kick the player
   else if((%killerID.teamkills >= $Host::TKMax - 1) && $Host::TKMax != 0)
   {
      Game.kickClientName = %killerID.name;
      kick(%killerID, false, %killerID.guid);
      adminLog( %killerID, " was autokicked for too many teamkills." );
   }
   return true;
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TKwarn))
    activatePackage(TKwarn);