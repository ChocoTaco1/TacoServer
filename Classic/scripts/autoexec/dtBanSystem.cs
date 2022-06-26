//$Host::dtBanlist = "prefs/dtBanlist.cs";
//$Host::KickBanTime = 20; is 20 Minutes
//$Host::BanTime = 43200; is One Month
//$Host::BanTime = 129600; is Three Months
//$Host::BanTime = 259200; is Six Months
//$Host::BanTime = 518400; is 1 year
//$Host::BanTime = 1000000; is Until you unban them (Forever)
//$Host::BanTime = BAN; is Until you unban them (Forever)

//$dtBanList::GUID3555379 = "DAY OF THE YEAR BANNED \t YEAR BANNED \t HOUR BANNED \t MINUTE BANNED \t TIME TO BE BANNED";
//$dtBanList::GUID3555379 = "4\t2021\t18\t31\t518400";

//TO UNBAN SOMEONE WITHOUT RESTARTING THE SERVER
//banList();in console
//unbanIndex(%index) %index is the number next to the players name from listBans();
//Example: unbanold(555555,"22.222.222.222"); put ip in quotes

package dtBan
{
function ClassicLoadBanlist()
{
	$ClassicPermaBans = 0;
	if(isFile($Host::dtBanlist))
		exec($Host::dtBanlist);
	$ClassicWhitelists = 0;
	exec($Host::ClassicWhitelist);
}

function BanList::add(%guid, %ipAddress, %time){
   if(%time > 999999){
      %time = "BAN";
   }
   %name = getClientBanName(%guid, %ipAddress);
   if (%guid > 0){
      $dtBanList::GUID[%guid] = dtBanMark() TAB %time TAB %name;
   }
   if (getSubStr(%ipAddress, 0, 3) $= "IP:"){
      %bareIP = getSubStr(%ipAddress, 3, strLen(%ipAddress));
      %bareIP = getSubStr(%bareIP, 0, strstr(%bareIP, ":"));
      %bareIP = strReplace(%bareIP, ".", "_"); // variable access bug workaround
      // add IP ban
      $dtBanList::IP[%bareIP] = dtBanMark() TAB %time TAB %name;
   }
   %found = 0;
   %eIndex = -1;
   for (%i = 0; %i <  100; %i++){
      if($dtBanList::NameList[%i] !$= ""){
         if(getField($dtBanList::NameList[%i], 0) $= %name){
            %found =1;
            if(%guid > 0)
               $dtBanList::NameList[%i] = setField($dtBanList::NameList[%i], 1, %guid);
            if(getSubStr(%ipAddress, 0, 3) $= "IP:")
               $dtBanList::NameList[%i] = setField($dtBanList::NameList[%i], 2, %bareIP);
            break;
         }
      }
      else if(%eIndex == -1){
         %eIndex = %i;    
      }
   }
   if(!%found){
      if($dtBanList::NameList[%eIndex] $= ""){
         $dtBanList::NameList[%eIndex] = %name TAB %guid TAB %bareIP;
      }
      else{
         error("Ban Index is not empty");
      }
   }
   saveBanList();
}

function banList_checkIP(%client){
   %ip = %client.getAddress();
   %ip = getSubStr(%ip, 3, strLen(%ip));
   %ip = getSubStr(%ip, 0, strstr(%ip, ":"));
   %ip = strReplace(%ip, ".", "_");

   %time = $dtBanList::IP[%ip];
   if(%time $= "BAN")
      return 1;
   if (getFieldCount(%time) > 0){
      %delta =  getBanCount(getField(%time,0), getField(%time,1),getField(%time,2),getField(%time,3));
      if (%delta < getField(%time,4))
         return 1;
      else{
         for (%i = 0; %i <  100; %i++){
            if($dtBanList::NameList[%i] !$= ""){
               if(getField($dtBanList::NameList[%i], 1) $= %guid){
                  unbanIndex(%i);
                  break;
               }
            }
         }
         $dtBanList::IP[%ip] =  "";
         saveBanList();
      }
   }
   return 0;
}

function banList_checkGUID(%guid){
   %time = $dtBanList::GUID[%guid];
   if(%time $= "BAN")
      return 1;
   if (getFieldCount(%time) > 0){
      %delta =  getBanCount(getField(%time,0), getField(%time,1),getField(%time,2),getField(%time,3));
      if (%delta < getField(%time,4))
         return 1;
      else{
         for (%i = 0; %i <  100; %i++){
            if($dtBanList::NameList[%i] !$= ""){
               if(getField($dtBanList::NameList[%i], 1) $= %guid){
                  unbanIndex(%i);
                  break;
               }
            }
         }
         $dtBanList::GUID[%guid] = "";
         saveBanList();
      }
   }
   return 0;
}

};

if (!isActivePackage(dtBan)){
	activatePackage(dtBan);
}

function saveBanList(){
 if(!isEventPending($banEvent))
   $banEvent = schedule(1000,0,"export","$dtBanList*", $Host::dtBanlist);
}
function getClientBanName(%guid, %ip){
   %found = 0;
   for (%i = 0; %i <  ClientGroup.getCount(); %i++){
      %client = ClientGroup.getObject(%i);
      if(%guid > 0 && %client.guid $= %guid){
         %found = 1;
        break;
      }
      else if(%client.getAddress() $= %ip){
         %found = 1;
         break;
      }
   }
   if(%found){
      %authInfo = %client.getAuthInfo();
      %realName = getField( %authInfo, 0 );
      if(%realName !$= "")
         %name = %realName;
      else
         %name =  stripChars( detag( getTaggedString( %client.name ) ), "\cp\co\c6\c7\c8\c9\c0" );
      return trim(%name);
  }
  return 0;
}

function  getBanCount(%d, %year, %h, %n){
   if(%d && %year && %h && %n){
      %dif = formattimestring("yy") - %year;
      %days += 365 * (%dif-1);
      %days += 365 - %d;
      %days += dtBanDay();
      %ht = %nt = 0;
      if(formattimestring("H") > %h){
         %ht = formattimestring("H") - %h;
      }
      else if(formattimestring("H") < %h){
         %ht = 24 - %h;
         %ht = formattimestring("H")+ %ht;
      }
      if(formattimestring("n") > %n){
         %nt = formattimestring("n") - %n;
      }
      else if(formattimestring("n") < %n){
         %nt = 60 - %n;
         %nt = formattimestring("n") + %nt;
      }
      return mfloor((%days * 1440) +  (%ht*60) + %nt);
   }
   return 0;
   //return mfloor((%days * 1440) +  (%ht*60) + %nt) TAB (%days * 1440) TAB (%ht*60) TAB %nt;
}

function dtBanDay(){
   %date = formattimestring("mm dd yy");
   %m = getWord(%date,0);%d = getWord(%date,1);%y = getWord(%date,2);
   %count = 0;
   if(%y % 4 < 1){%days[2] = "29";}else{%days[2] = "28";} // leap year
   %days[1] = "31";%days[3] = "31";
   %days[4] = "30"; %days[5] = "31"; %days[6] = "30";
   %days[7] = "31"; %days[8] = "31"; %days[9] = "30";
   %days[10] = "31"; %days[11] = "30"; %days[12] = "31";
   for(%i = 1; %i <= %m-1; %i++){
      %count += %days[%i];
   }
   return %count + %d;
}

function dtBanMark(){
   %date = formattimestring("mm dd yy");
   %m = getWord(%date,0);%d = getWord(%date,1);%y = getWord(%date,2);
   %count = 0;
   if(%y % 4 < 1){%days[2] = "29";}else{%days[2] = "28";} // leap year
   %days[1] = "31";%days[3] = "31";
   %days[4] = "30"; %days[5] = "31"; %days[6] = "30";
   %days[7] = "31"; %days[8] = "31"; %days[9] = "30";
   %days[10] = "31"; %days[11] = "30"; %days[12] = "31";
   for(%i = 1; %i <= %m-1; %i++){
      %count += %days[%i];
   }
   return %count + %d TAB formattimestring("yy") TAB formattimestring("H") TAB formattimestring("n");
}

function banList(){
   %found = 0;
   for (%i = 0; %i <  100; %i++){
      %fieldList = $dtBanList::NameList[%i];
      if($dtBanList::NameList[%i] !$= ""){
         %found = 1;
         error("index:" @ %i SPC "Name:" @ getField(%fieldList,0) SPC  "GUID:" @ getField(%fieldList,1)  SPC "IP:" @  getField(%fieldList,2));
      }
   }
   if(%found){
       error("Use unbanIndex(%index); to unban user from the list ");
   }
   else{
      error("No bans, see" SPC $Host::dtBanlist SPC "for older entries");
   }
}

function unbanIndex(%index){
   if( $dtBanList::NameList[%index] !$= ""){
      %fieldList = $dtBanList::NameList[%index];
      %name = getField(%fieldList, 0);
      $dtBanList::NameList[%index] = "";
      error("Name" SPC getField(%fieldList,0) SPC "UNBANNED");
      %guid = getField(%fieldList,1);
      if($dtBanList::GUID[%guid] !$= ""){
        $dtBanList::GUID[%guid] = "";
        error("GUID" SPC %guid SPC "UNBANNED");
      }
      %ip = getField(%fieldList,2);
      if($dtBanList::IP[%ip] !$= ""){
        $dtBanList::IP[%ip] =  "";
        error("IP" SPC %ip SPC "UNBANNED");
      }
      saveBanList();
      return %name;
   }
   return -1;
}

//old method
function unbanold(%guid,%ip){
   %ip = strReplace(%ip, ".", "_");
   for (%i = 0; %i <  100; %i++){
      if($dtBanList::NameList[%i] !$= ""){
         if(getField($dtBanList::NameList[%i], 2) $= %ip || getField($dtBanList::NameList[%i], 1) $= %guid){
            unbanIndex(%i);
            return;
         }
      }
   }
   if($dtBanList::GUID[%guid] !$= ""){
     $dtBanList::GUID[%guid] = "";
     error("GUID" SPC %guid SPC "UNBANNED");
   }
   if($dtBanList::IP[%ip] !$= ""){
     $dtBanList::IP[%ip] =  "";
     error("IP" SPC %ip SPC "UNBANNED");
   }
   saveBanList();
}