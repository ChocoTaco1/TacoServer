//Enable/Disable
$EnableMultipleMapRotation = 0;

//How any mapRotation Files
//Naming scheme mapRotation1.cs, mapRotation2.cs, mapRotation3.cs, etc
$mapRotationFilesCount = 3;

function multipleMapRotation() 
{
    if($EnableMultipleMapRotation)
    {
        %random = getrandom(1, $mapRotationFilesCount);
        %mapRot = "prefs/mapRotation" @ %random @ ".cs";
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