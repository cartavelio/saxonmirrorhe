xquery version "1.0" encoding "utf-8";
module namespace avcf = "urn:active-endpoints:avc:xquery:function";
declare namespace com = "http://schemas.active-endpoints.com/avc/2009/07/avoscentral-common.xsd"; 
declare namespace api = "http://schemas.active-endpoints.com/avc/2009/07/avoscentral-api-wsdl.xsd";
declare namespace cfg = "http://schemas.active-endpoints.com/avc/2009/07/avoscentral-config.xsd";
declare namespace avc-int = "http://schemas.active-endpoints.com/avoscentral/2009/11/avcx";

declare function avcf:allowedRole($allowedRoles as node()*, $userRoles as node()*) as xs:boolean
{
   (: value-intersect of role list vs. allowed roles :)
   if ( exists($userRoles) ) then
      exists(distinct-values($allowedRoles[.=$userRoles]))
   else
      true()
};

(: build the taskRole for the allowed roles and the current filter :)
declare function avcf:buildTaskDefs($allowedRoles as node()*, $taskRoleFilter as node()) as node()*
{
   for $taskFilterCategory in $taskRoleFilter/cfg:taskFilterCategoryDef
   where avcf:allowedRole($allowedRoles, $taskFilterCategory/com:allowedRoles/com:role/text())
   return
      <api:taskFilterCategory id="{$taskFilterCategory/@id}" name="{$taskFilterCategory/@name}">
      {$taskFilterCategory/com:taskColumns}
      {         
         for $taskFilterDef in $taskFilterCategory/com:taskFilterDef
         return
            <com:taskFilterDef id="{$taskFilterDef/@id}" name="{$taskFilterDef/@name}">
               <com:filter>{ $taskFilterDef/com:filter/* }</com:filter>
               {
               if (exists($taskFilterDef/com:feedQueryString)) then
                  $taskFilterDef/com:feedQueryString
               else
                  ()
               }
            </com:taskFilterDef>
      }
      </api:taskFilterCategory>
};

(: build a set of defs for the supplied user roles and category :)
declare function avcf:buildRequestDef($allowedRoles as node()*, $requestCategory as node()) as node()*
{
      for $requestDef in $requestCategory/com:requestDef
         where avcf:allowedRole($allowedRoles, $requestDef/com:allowedRoles/com:role/text())
      return
         <com:requestDef id="{$requestDef/@id}" name="{$requestDef/@name}">
         <com:description>{ $requestDef/com:description/text() }</com:description>
         <com:formLocation>{ normalize-space($requestDef/com:formLocation/text()) }</com:formLocation>
         {
            if (exists($requestDef/com:formPreview)) then
               <com:formPreview>{ normalize-space($requestDef/com:formPreview/text()) }</com:formPreview>
            else
               ()
         }
         
         {
            if (exists($requestDef/com:version)) then
               <com:version>{ $requestDef/com:version/text() }</com:version>
            else
               ()
         }
      </com:requestDef>
};
