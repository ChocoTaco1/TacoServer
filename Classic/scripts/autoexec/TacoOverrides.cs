//Various Overrides
//

package TacoOverrides
{

//Issue with the start grenade throw was very soft and bumped it up a tad
function serverCmdEndThrowCount(%client, %data)
{
   if(%client.player.throwStart == 5)
      return;

   // ---------------------------------------------------------------
   // z0dd - ZOD, 8/6/02. New throw str features
   %throwStrength = (getSimTime() - %client.player.throwStart) / 150;
   if(%throwStrength > $maxThrowStr) 
      %throwStrength = $maxThrowStr; 
   else if(%throwStrength < 0.5)
      %throwStrength = 0.5;
   // ---------------------------------------------------------------
   
   %throwScale = %throwStrength / 2;
   %client.player.throwStrength = %throwScale;

   %client.player.throwStart = 5; //was 0
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(TacoOverrides))
    activatePackage(TacoOverrides);