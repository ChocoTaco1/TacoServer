function getGroupObjectByName(%group, %name)
{
	%numObjects = %group.getCount();

	for(%i = 0; %i < %numObjects; %i++)
	{
		if (%group.getObject(%i).getName() $= %name)
			return %group.getObject(%i);
	}
}

function deleteObjectsFromMapByType(%type)
{
	%teamsGroup = getGroupObjectByName(MissionGroup, "Teams");
	if (!isObject(%teamsGroup))
	{
		return;
	}

	%team1Group = getGroupObjectByName(%teamsGroup, "Team1");
	if (!isObject(%team1Group))
	{
		return;
	}

	%team2Group = getGroupObjectByName(%teamsGroup, "Team2");
	if (!isObject(%team2Group))
	{
		return;
	}

	%team1Base0Group = getGroupObjectByName(%team1Group, "Base0");
	if (!isObject(%team1Base0Group))
	{
		return;
	}

	%team2Base0Group = getGroupObjectByName(%team2Group, "Base1");
	if (!isObject(%team2Base0Group))
	{
		return;
	}

	
	deleteObjectsFromGroupByType(%team1Base0Group, %type);
	deleteObjectsFromGroupByType(%team2Base0Group, %type);
}

function deleteObjectsFromGroupByType(%group, %type)
{
	%noObjectsLeft = 0;

	while (%noObjectsLeft == 0)
	{
		%i = 0;
		%noObjectsLeft = 1;
		%loop = 1;
     	%numObjects = %group.getCount();

		while ((%loop == 1) && (%i < %numObjects))
		{
         %obj = %group.getObject(%i);

         if (%obj.getClassName() $= "SimGroup")
            deleteObjectsFromGroupByType(%obj, %type);

         if (%obj.getClassName() $= %type)
			{
				%obj.delete();
				%loop = 0;
				%noObjectsLeft = 0;
			}
			else
				%i++;
		}
	}
}

function deleteNonSCtFObjectsFromMap()
{   
   deleteObjectsFromGroupByType(MissionGroup, "PhysicalZone");
   deleteObjectsFromGroupByType(MissionGroup, "Turret");
   deleteObjectsFromGroupByType(MissionGroup, "StaticShape");
   //deleteObjectsFromGroupByType(MissionGroup, "ForceFieldBare");
   deleteObjectsFromGroupByType(MissionGroup, "FlyingVehicle");
   deleteObjectsFromGroupByType(MissionGroup, "WheeledVehicle");
   deleteObjectsFromGroupByType(MissionGroup, "HoverVehicle");
   deleteObjectsFromGroupByType(MissionGroup, "Waypoint");
 
}
