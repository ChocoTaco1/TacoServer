// Get Random Maps Script
//
// Used by the Set Next Mission feature
//
// Random Set Next Mission maps
// Runs for SetNextMisssion (Random) and Map Repetition Checker
// Randomizes the maps available in the admin menu
//

// This file is present
$GetRandomMapsLoaded = true;

// getRandomMap - returns a random map without duplicates until all have been played.
// Script BY: DarkTiger				
// set this to 1 to save rng list to continue where the server left off in case of server reset 

$rngListSave = 1;
$rngDebug = 1;//show echos for testing 

function getRandomMap(){
   // builds valid map list  to pick from 
   for(%i = 0; %i < $HostMissionCount; %i++){
      %map = $HostMissionFile[%i];  
      %FFA = $Host::MapFFA[%map, $CurrentMissionType];
      %Cycle =$Host::MapCycle[%map, $CurrentMissionType];
      %bot = $BotEnabled[%i];
      if(%FFA && %Cycle){
         if($Host::BotsEnabled){
            if(%bot)
               %map[%c++] = %map;
         }
         else{
            %map[%c++] = %map;
         }
      }
   }
   %rng = getRSGN(1,%c,$CurrentMissionType); // use gameType as the id 
   if($rngDebug){error(%map[%rng] SPC %rng);}
   return %map[%rng];
}

function getRSGN(%min,%max,%id){
   // This funciton is kind of like a random sequence generator but its done on the fly
   // returns you a unique random number every time until max is reached 
   // id value is so it can be used in more then one place 
   // the id value can be function name that its used in or a number 
   // lastly it only for numbers between  -1000 to 1000 see down below;  
   
   if($rngListSave && isFile("rngLists/" @ %id @ ".cs") && !$rngIsLoaded[%id]){
      exec("rngLists/" @ %id @ ".cs");
      $rngIsLoaded[%id] = 1;
      if($rngDebug){error("Loading Map RNG List =" SPC "rngLists/" @ %id @ ".cs");}
   }
   
   if(%id $= ""){
      error("getRSG function call does not have an id value");
      return getRandom(%min,%max);
   }
   
   %c = %min - 1; // skip counter
   if(((%max - %min) - ($rng::Count[%id] - 1)) < 1) // reset if we cycled though all possable
   {
       $rng::Count[%id] = 1; // we dont reset to 0 becuae of the last number used 
       // change these numbers to expand range 
       for(%a = %min; %a <= %max; %a++)  // this resets a wide range incase min max change for what ever reasion
	   { 
         $rng::List[%id,%a] = 0; // reset number list back to 0 
       }
	   
       $rng::List[%id,$rng::Last[%id]]  = 1; // mark the last number used as in used after reset
   }
   
   %rng = getRandom(%min,%max - $rng::Count[%id] ); // find random number - the total number we have done 
   
   for(%i = %min; %i <= %max; %i++)  // loop cycle though all possable numbers
   {
      if($rng::List[%id,%i]) // skip over ones that we have all ready used
	  {
         continue;
      }
      %c++; // skip counter
      
	  if(%rng == %c) // onces the skip counter matches the rng number we have landed on a valid number that we havent used yet
	  {
         break;   // kill the loop 
      }
   }
   
   $rng::List[%id,%i] = 1;// this marks said number as used 
   $rng::Count[%id]++;// up are total used numbers 
   $rng::Last[%id] = %i; // for when the list resets it wont pick the same number twice
   
   if(%i > %max || %i < %min)
   { // fail safe 
      return %max;  
   }
   if($rngListSave){
      export( "$rng::*", "rngLists/" @ %id @ ".cs", false );
   }
   return %i; // return the one we stoped on 
}

// Return random maps list for SetNextMission
// Primarily used in admin menus
function SetNextMapGetRandoms( %client )
{
	%cyclecount = 1;
	for(%i = 0; %i < 8; %i++)
	{
		$SetNextMissionMapSlot[%cyclecount] = getRandomMap();
		%cyclecount++;
	}
}

// Reset SetNextMission every map change
package ResetSetNextMission
{

function DefaultGame::gameOver(%game)
{
	Parent::gameOver(%game);
	
	//Reset SetNextMission Restore
	$SetNextMissionRestore = "";
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(ResetSetNextMission))
    activatePackage(ResetSetNextMission);