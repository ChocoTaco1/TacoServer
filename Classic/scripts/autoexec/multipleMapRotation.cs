//$Host::ClassicRotationFile = "prefs/mapRotation1.cs";

//Enable/Disable
//$Host::MultipleMapRotation = 0;

//How any mapRotation Files
//Naming scheme mapRotation1.cs, mapRotation2.cs, mapRotation3.cs, etc
//$Host::MultipleMapRotationCount = 3;

function multipleMapRotation()
{
	//Look for a progressing number
	%var = stripChars($Host::ClassicRotationFile, "prefs/mapRotation.cs");
	//echo("var: " @ %var);

	if(%var) //If number exists proceed
    {
        if(%var $= $Host::MultipleMapRotationCount)
            %var = 1;
        else
            %var = %var + 1;

        %mapRot = "prefs/mapRotation" @ %var @ ".cs";
        $Host::ClassicRotationFile = %mapRot;
    }

	schedule(10000,0,"multipleMapRotationEcho",0); //Echo at start
}

//Echo
function multipleMapRotationEcho()
{
    echo("Current MapRotation: " @ $Host::ClassicRotationFile);
}

//Run
if($Host::MultipleMapRotation)
	multipleMapRotation();
