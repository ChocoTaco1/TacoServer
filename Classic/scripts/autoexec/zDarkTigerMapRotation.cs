//----------------DarkTiger's MapRotation----------------
//
// - To Further Randomize Map Rotation
// - Keep Track of maps so they arnt chosin again
//
// - Meant to be run with evo admin mod
// - Set $Host::ClassicRandomMissions to 2 to activate
// - $Host::ClassicRandomMissions = 2;
//



package DarkTigerMapRotation
{
	

function getNextMission( %misName, %misType )
{  
   // Find the index of the mission type:
   for( %TypeIdx = 0; %TypeIdx < $HostTypeCount; %TypeIdx++ )
   {
      if ($HostTypeName[%TypeIdx] $= %misType)
		break;
   }

   // Mission Type was not found. Return last mission an error message
   if(%TypeIdx == $HostTypeCount)
   {
      echo("Error: Could not find matching game type in function " @ "getNextMission( " @ %misName @", " @%misType @ " ).\n");
      return -1;
   }
   
   %EvoTempMissionListLength = 0;
   %EvoCurrentMissionIndex = -1;

   // Build a list of missions that are eligible to be cycled into
   for( %MisCounter = 0; %MisCounter < $HostMissionCount[%TypeIdx]; %MisCounter ++ )
   {
      %ThisMission = $HostMission[ %TypeIdx, %MisCounter ];
      %ThisMissionName = $HostMissionFile[ %ThisMission ];

      if ($Host::EvoCustomMapRotation && !$Host::MapCycle[%ThisMissionName, %misType]) 
	  {
		// If mission is not set to cycle, then jump to next
		continue;
	  }
      
      if($HostGameBotCount && (!$BotEnabled[$HostMission[%TypeIdx, %MisCounter]]))
	  {
		// If there are bots present and no bot support is on map, jump
		// to next
		continue;
	  }
      
      if ( %ThisMissionName $= %misName )
	  // This map is the current mission
	  {
		%EvoCurrentMissionIndex = %EvoTempMissionListLength;
	  }
		%EvoTempMissionList[ %EvoTempMissionListLength ] = %ThisMission;
		%EvoTempMissionListLength ++;
   }
   // If no maps are found, return last mission with error
   if(! %EvoTempMissionListLength )
   {
      echo("Error: No valid maps found in cycle." );
      return -1;
   }
   // Custom Map Rotation counts forwards
   if( $Host::EvoCustomMapRotation )
   {
      %NextMissionIndex = %EvoCurrentMissionIndex + 1;
   }
   else
   {
      // Standard Map Rotation counts backwards. For whatever reason
      if( %EvoCurrentMissionIndex <= 0 )
      {
         %NextMissionIndex = %EvoTempMissionListLength - 1;
      }
      else
      {
         %NextMissionIndex = %EvoCurrentMissionIndex - 1;
      }
   }

   if(( $Host::ClassicRandomMissions == 1) && ( %EvoTempMissionListLength > 2 ) )
   {
      %NextMissionIndex = %NextMissionIndex + GetRandom(0, %EvoTempMissionListLength - 2 );
   }
   else if(( $Host::ClassicRandomMissions  == 2) && ( %EvoTempMissionListLength > 2 ) ){
      %NextMissionIndex = %NextMissionIndex + getRSGN(0, %EvoTempMissionListLength - 2, "getNextMission1");
   }

   // Bring %NextMissionIndex back into the interval
   // between 0 and %Evo...Length
   if( %NextMissionIndex < 0 )
      %NextMissionIndex += %EvoTempMissionListLength;

   if( %NextMissionIndex >= %EvoTempMissionListLength )
      %NextMissionIndex -= %EvoTempMissionListLength;

   return %EvoTempMissionList[ %NextMissionIndex ];
}



// buildMissionList()
// Info: Build the mission list
function buildMissionList()
{  
   deleteVariables("$HostMission*");
   deleteVariables("$HostType*");
   
   $HostMissionCount = 0;
   $HostTypeCount = 0;
   // standard one
   if(!$Host::EvoCustomMapRotation)
   {
      %search = "missions/*.mis";
      %fobject = new FileObject();
      
	  for(%file = findFirstFile(%search); %file !$= ""; %file = findNextFile(%search))
      {
         // get the name
         %name = fileBase(%file);
         %idx = $HostMissionCount;
         
		 $HostMissionCount++;
         $HostMissionFile[%idx] = %name;
         $HostMissionName[%idx] = %name;
         
		 if(!%fobject.openForRead(%file))
            continue;
	  
         %typeList = "None";
		 
         while(!%fobject.isEOF())
         {
            %line = %fobject.readLine();
            if(getSubStr(%line, 0, 17) $= "// DisplayName = ")
            {
               // Override the mission name:
               $HostMissionName[%idx] = getSubStr(%line, 17, 1000);
            }
            else if(getSubStr(%line, 0, 18) $= "// MissionTypes = ")
            {
               %typeList = getSubStr(%line, 18, 1000);
               break;
            }
         }
		 
         %fobject.close();

         // Don't include single player missions:
         if(strstr(%typeList, "SinglePlayer") != -1)
            continue;
	  
         // Test to see if the mission is bot-enabled:
         %navFile = "terrains/" @ %name @ ".nav";
         $BotEnabled[%idx] = isFile(%navFile);
         for(%word = 0; (%misType = getWord(%typeList, %word)) !$= ""; %word++)
         {
            // z0dd - ZOD - Founder(founder@mechina.com): Append Tribe Practice to CTF missions
            if(%misType $= "CTF")
               %typeList = rtrim(%typeList) @ " PracticeCTF SCtF Hybrid";

            if(%misType $= "CnH")
               %typeList = rtrim(%typeList) @ " Conquest";

            // load TR2 gametype?
            if((%misType $= "TR2") && (!$Host::ClassicLoadTR2Gametype))
               continue;

            for(%i = 0; %i < $HostTypeCount; %i++)
               if($HostTypeName[%i] $= %misType)
                  break;

            if(%i == $HostTypeCount)
            {
               $HostTypeCount++;
               $HostTypeName[%i] = %misType;
               $HostMissionCount[%i] = 0;
            }
            // add the mission to the type
            $HostMission[%i, $HostMissionCount[%i]] = %idx;
            $HostMissionCount[%i]++;
         }
      }
	  
      getMissionTypeDisplayNames();
      %fobject.delete();
   }
   else
   {
      exec($Host::EvoCustomMapRotationFile);
      getMissionTypeDisplayNames();
   }
}

};




function getRSGN(%min,%max,%id)
{
   // This funciton is kind of like a random sequence generator but its done on the fly
   // returns you a unique random number every time until max is reached 
   // id value is so it can be used in more then one place 
   // the id value can be function name that its used in or a number 
   // lastly it only for numbers between  -1000 to 1000 see down below;  
   
   if(%id $= "")
   {
      error("getRSG function call does not have an id value");
      return %max;
   }
   
   %c = %min - 1; // skip counter
   //$rngCount[%id] - 1) is to account for 0 
   if(((%max - %min) - ($rngCount[%id] - 1)) < 1) // reset if we cycled though all possable
   {
       $rngCount[%id] = 1; // we dont reset to 0 becuae of the last number used 
       // change these numbers to expand range 
       for(%a = -1000; %a <= 1000; %a++)  // this resets a wide range incase min max change for what ever reasion
	   { 
         $rngList[%id,%a] = 0; // reset number list back to 0 
       }
	   
       $rngList[%id,$rngLast[%id]]  = 1; // mark teh last number used as in used after reset
       //error("reset"); //debug
   }
   
   %rng = getRandom(%min,%max - $rngCount[%id] ); // find random number - the total number we have done 
   
   for(%i = %min; %i <= %max; %i++)  // loop cycle though all possable numbers
   {
      if($RngList[%id,%i]) // skip over ones that we have all ready used
	  {
         continue;
      }
      %c++; // skip counter
      
	  if(%rng == %c) // onces the skip counter matches the rng number we have landed on a valid number that we havent used yet
	  {
         break;   // kill the loop 
      }
   }
   
   $rngList[%id,%i] = 1;// this marks said number as used 
   $rngCount[%id]++;// up are total used numbers 
   $rngLast[%id] = %i; // for when the list resets it wont pick the same number twice
   
   if(%i > %max || %i < %min)
   { // fail safe 
      return %max;  
   }
   
   return %i; // return the one we stoped on 
}




// Info: Add a map to the mission list (source: bwadmin)
function addRotationMap(%file, %type, %ffa, %cycle)
{

   if ( %cycle $= "" )
   {
      %cycle = %ffa;
   }
   
   if(isFile("missions/" @ %file @ ".mis"))
   {
      if(%type $= "TR2" && !$Host::ClassicLoadTR2Gametype) // load TR2 gametype?
		return;
      
      %fobject = new FileObject();
      
      for(%n = 0; %n < $HostMissionCount; %n++)
		if($HostMissionFile[%n] $= %file)
	      break;

      if(%n == $HostMissionCount)
	  {
		$HostMissionCount++;
		$HostMissionFile[%n] = %file;
		$HostMissionName[%n] = %file;
	  
		if(%fobject.openForRead("missions/" @ %file @ ".mis"))
		{
			while(!%fobject.isEOF())
			{
				%line = %fobject.readLine();
				
				if(getSubStr(%line, 0, 17) $= "// DisplayName = ")	
					// Override the mission name:
					$HostMissionName[%n] = getSubStr(%line, 17, 1000);
			}
			
	        %fobject.close();
	    }
	  
	    %navFile = "terrains/" @ %file @ ".nav";
	    $BotEnabled[%n] = isFile(%navFile);
	  }
      
      for(%i = 0; %i < $HostTypeCount; %i++)
		
	  if($HostTypeName[%i] $= %type)
		break;

      if(%i == $HostTypeCount)
	  {
		$HostTypeCount++;
		$HostTypeName[%i] = %type;
		$HostMissionCount[%i] = 0;
	  }
      
      // add the mission to the type
      $HostMission[%i, $HostMissionCount[%i]] = %n;
      $HostMissionCount[%i]++;
      
      if(%ffa !$= "")
		$Host::MapFFA[%file, %type] = %ffa;
      
      if(%cycle !$= "")
		$Host::MapCycle[%file, %type] = %cycle;
      
      %fobject.delete();
   }
}


// Prevent package from being activated if it is already
if (!isActivePackage(DarkTigerMapRotation))
	activatePackage(DarkTigerMapRotation);