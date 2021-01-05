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
   if (%guid > 0){
      $dtBanList::GUID[%guid] = dtBanMark() TAB %time;
   }
   if (getSubStr(%ipAddress, 0, 3) $= "IP:"){
      // add IP ban
      %bareIP = getSubStr(%ipAddress, 3, strLen(%ipAddress));
      %bareIP = getSubStr(%bareIP, 0, strstr(%bareIP, ":"));
      %bareIP = strReplace(%bareIP, ".", "_"); // variable access bug workaround

      $dtBanList::IP[%bareIP] = dtBanMark() TAB %time;
      //error("ban" SPC %bareIP SPC $dtBanList::IP[%bareIP]);
   }

   // write out the updated bans to the file
   export("$dtBanList*", $Host::dtBanlist);
}

function banList_checkIP(%client){
   %ip = %client.getAddress();
   %ip = getSubStr(%ip, 3, strLen(%ip));
   %ip = getSubStr(%ip, 0, strstr(%ip, ":"));
   %ip = strReplace(%ip, ".", "_");

   %time = $dtBanList::IP[%ip];
   if(%time $= "BAN")
      return 1;
   if (%time !$= "" && %time != 0){
      %delta =  getBanCount(getField(%time,0), getField(%time,1),getField(%time,2),getField(%time,3));
      if (%delta < getField(%time,4))
         return 1;
      else{
         $dtBanList::IP[%ip] =  "";
         schedule(1000,0,"export","$dtBanList*", $Host::dtBanlist);
         //export("$dtBanList*", "prefs/dtBanlist.cs");
      }
   }
   return 0;
}

function banList_checkGUID(%guid){
   %time = $dtBanList::GUID[%guid];
   if(%time $= "BAN")
      return 1;
   if (%time !$= "" && %time != 0){
      %delta =  getBanCount(getField(%time,0), getField(%time,1),getField(%time,2),getField(%time,3));
      if (%delta < getField(%time,4))
         return 1;
      else{
          $dtBanList::GUID[%guid] = "";
          schedule(500,0,"export","$dtBanList*", $Host::dtBanlist);
          //export("$dtBanList*", "prefs/dtBanlist.cs");
      }
   }
   return 0;
}

};

if (!isActivePackage(dtBan))
	activatePackage(dtBan);

function  getBanCount(%d, %year, %h, %n){
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
   //return mfloor((%days * 1440) +  (%ht*60) + %nt) TAB (%days * 1440) TAB (%ht*60) TAB %nt;
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

function unban(%guid,%ip){
   if($dtBanList::GUID[%guid] !$= ""){
     $dtBanList::GUID[%guid] = "";
     error("GUID" SPC %guid SPC "UNBANNED");
   }
   if($dtBanList::IP[%ip] !$= ""){
     $dtBanList::IP[%ip] =  "";
     error("IP" SPC %ip SPC "UNBANNED");
   }
   export("$dtBanList*", $Host::dtBanlist);
}
