//$Host::ClassicRotationFile = "prefs/mapRotation1.cs";

//Enable/Disable
$EnableMultipleMapRotation = 0;

//How any mapRotation Files
//Naming scheme mapRotation1.cs, mapRotation2.cs, mapRotation3.cs, etc
$mapRotationFilesCount = 3;

function multipleMapRotation() 
{
    if($EnableMultipleMapRotation)
    {
        %var = stripChars($Host::ClassicRotationFile, "prefs/mapRotation.cs");
        //echo("var: " @ %var);
        if(%var $= $mapRotationFilesCount)
            %var = 1;
        else
            %var = %var + 1;

        %mapRot = "prefs/mapRotation" @ %var @ ".cs";
        $Host::ClassicRotationFile = %mapRot;
        
        //Echo at start
        schedule(10000,0,"multipleMapRotationEcho",0);
    }
}

//Echo at start
function multipleMapRotationEcho()
{
    echo("Current MapRotation: " @ $Host::ClassicRotationFile);
}

//Run
multipleMapRotation();