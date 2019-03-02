// MiniVehicleStation
// Adapted from the original station by
// Tim 'Zear' Hammock
//
// Because the effects were so tightly bound to the model, I have
//    not yet figured out how to properly align them with a half-scale
//    version of the pad. Therefore, this version simply eliminates
//    the majority of the effects. Mebbe I'll figure it out later...
//
// To use:
//
// 1) Put this script in the scripts/autoexec directory (or zip into a vl2
//       with the path set that location and drop the vl2 into base).
// 2) Create a pad with the scale set to "0.5 0.5 0.5".
// 3) Add a dynamic field named "useMiniVFX" to the pad. Its value should be "1".
// 4) There is no step 4.
//         
// The actual station will be repositioned and a platform placed under
//    it, so that it is not hanging in midair (automatic - you don't do
//    this yourself).
//

package MiniVehicleStation
{

function createVehicle(%client, %station, %blockName, %team , %pos, %rot, %angle)
{
   if(%station.useMiniVFX != "1")
   {
      Parent::createVehicle(%client, %station, %blockName, %team , %pos, %rot, %angle);
      return;
   }

   %obj = %blockName.create(%team);   
   if(%obj)
   {
      if ( %blockName $= "MobileBaseVehicle" )
      {
         %station.station.teleporter.MPB = %obj;
         %obj.teleporter = %station.station.teleporter;
      }

      %station.ready = false;
      %obj.team = %team;
      %obj.useCreateHeight(true);
      %obj.schedule(5500, "useCreateHeight", false);
      %obj.getDataBlock().isMountable(%obj, false);
      %obj.getDataBlock().schedule(6500, "isMountable", %obj, true);
      
      %station.playThread($ActivateThread,"activate2");
      %station.playAudio($ActivateSound, ActivateVehiclePadSound);

      vehicleListAdd(%blockName, %obj);
      MissionCleanup.add(%obj);
                                  
      %turret = %obj.getMountNodeObject(10);
      if(%turret > 0)
      {
         %turret.setCloaked(true);
         %turret.schedule(4800, "setCloaked", false);
      }
      
      %obj.setCloaked(true);
      %obj.setTransform(%pos @ " " @ %rot @ " " @ %angle);
   
      %obj.schedule(3700, "playAudio", 0, VehicleAppearSound);
      %obj.schedule(4800, "setCloaked", false);
      
      if(%client.player.lastVehicle)
      {
         %client.player.lastVehicle.lastPilot = "";
         vehicleAbandonTimeOut(%client.player.lastVehicle);
         %client.player.lastVehicle = "";
      }   
      %client.player.lastVehicle = %obj;
      %obj.lastPilot = %client.player;
   }

   %obj.getDataBlock().schedule(5000, "mountDriver", %obj, %client.player);

   if(%obj.getTarget() != -1)
      setTargetSensorGroup(%obj.getTarget(), %client.getSensorGroup());
}

function StationVehiclePad::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
   
   if(%obj.useMiniVFX != "1")
      return;
   
   %orient = MatrixCreate(%obj.station.position, %obj.station.rotation);
   %offset = MatrixCreate("0 0 -1.6", "0 0 1 0");
   %finalT = MatrixMultiply(%orient, %offset);
   %obj.station.setTransform(%finalT);
   %obj.station.trigger.setTransform(%finalT);
   
   // necessary redundancy
   %orient = MatrixCreate(%obj.station.position, %obj.station.rotation);
   %finalT = MatrixMultiply(%orient, %offset);
   
   %newPlat = new TSStatic()
   {
      position  = "0 0 -100";
      rotation  = "0 0 1 0";
	   scale = "2.1322 2.09362 3.19835";
      shapeName = "bmiscf.dts";
   };
   %newPlat.setTransform(%finalT);
}

};

activatePackage(MiniVehicleStation);
