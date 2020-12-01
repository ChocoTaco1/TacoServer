//$dtBanList::IP192_168_0_133 = "336\t2020\t365";
//336 day of the year banned Dec1
//year year banned 2020
//365 How long to ban (1 year)

//$Host::dtBanlist = "prefs/dtBanlist.cs";

if(isFile($Host::dtBanlist))
   exec($Host::dtBanlist);

package dtBan{
   function BanList::add(%guid, %ipAddress, %days){
      if (%guid > 0){
         $dtBanList::GUID[%guid] = dtBanDay() TAB getBanYear() TAB %days;
      }
      if (getSubStr(%ipAddress, 0, 3) $= "IP:"){
         // add IP ban
         %bareIP = getSubStr(%ipAddress, 3, strLen(%ipAddress));
         %bareIP = getSubStr(%bareIP, 0, strstr(%bareIP, ":"));
         %bareIP = strReplace(%bareIP, ".", "_"); // variable access bug workaround

         $dtBanList::IP[%bareIP] = dtBanDay() TAB getBanYear() TAB %days;
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
      if (%time !$= "" && %time !$= "UNBAN"){
         %delta =  getBanCount(getField(%time,0), getField(%time,1));
         if (%delta < getField(%time,2))
            return 1;
         else 
            $dtBanList::IP[%ip] =  "UNBAN";
      }
      return 0;
   }
   function banList_checkGUID(%guid){
      %time = $dtBanList::GUID[%guid];
      if (%time !$= "" && %time !$= "UNBAN"){
         %delta =  getBanCount(getField(%time,0), getField(%time,1));
         if (%delta < getField(%time,2))
            return 1;
         else
             $dtBanList::GUID[%guid] = "UNBAN";
      }
      return 0;
   }
};
if (!isActivePackage(dtBan)) 
   activatePackage(dtBan);
   
function  getBanCount(%d, %year){
   %dif = getBanYear() - %year;
   %days += 365 * (%dif-1);
   %days += 365 - %d;
   %days += dtBanDay();
   return %days;
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

function getBanYear(){
    return formattimestring("yy");
}
   