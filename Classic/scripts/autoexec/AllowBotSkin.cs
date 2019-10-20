// Allow Bot Skin Script
//
// Disables the bot skin check serverside
// In combination with disabling clientside bot skin checking
// Use of Bot skins can be achieved
//

package AllowBotSkin
{


function GMW_SkinPopup::fillList( %this, %raceGender )
{
   for ( %i = 0; %i < %this.size(); %i++ )
      %this.realSkin[%i] = "";

	%this.clear();
   %path = "textures/skins/";
   switch ( %raceGender )
   {
      case 0:  // Human Male
         %pattern = ".lmale.png";
      case 1:  // Human Female
         %pattern = ".lfemale.png";
      case 2:  // Bioderm
         %pattern = ".lbioderm.png";
   }

   %customSkins = GMW_SkinPrefPopup.getSelected();
   %count = 0;
   for ( %file = findFirstFile( %path @ "*" @ %pattern ); %file !$= ""; %file = findNextFile( %path @ "*" @ %pattern ) )
   {
      %skin = getSubStr( %file, strlen( %path ), strlen( %file ) - strlen( %path ) - strlen( %pattern ) );  // strip off the path and postfix

      // Make sure this is not a bot skin:
      //if ( %skin !$= "basebot" && %skin !$= "basebbot" )
      //{
         // See if this skin has an alias:
         %baseSkin = false;
         for ( %i = 0; %i < $SkinCount; %i++ )
         {
            if ( %skin $= $Skin[%i, code] ) 
            {
               %baseSkin = true;
               %skin = $Skin[%i, name];
               break;
            }
         }

         if ( %customSkins != %baseSkin )
         {
            if ( %baseSkin )
               %this.realSkin[%count] = $Skin[%i, code];
            %this.add( %skin, %count );
            %count++;
         }
      //}
   }
   
   %this.sort( true );
}

};

// Prevent package from being activated if it is already
if (!isActivePackage(AllowBotSkin))
    activatePackage(AllowBotSkin);
