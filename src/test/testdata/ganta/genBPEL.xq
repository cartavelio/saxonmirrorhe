xquery version "1.0" encoding "utf-8";
module namespace avsf = "urn:activevos:avsf:xquery:function";

declare namespace abx = "http://www.activebpel.org/bpel/extension";
declare namespace aei = "http://www.activebpel.org/2009/02/bpel/extension/ignorable";
declare namespace bpel = "http://docs.oasis-open.org/wsbpel/2.0/process/executable";
declare namespace ext = "http://www.activebpel.org/2006/09/bpel/extension/query_handling";
declare namespace ext1 = "http://www.activebpel.org/2009/06/bpel/extension/links";
declare namespace screenFlow = "http://schemas.active-endpoints.com/wsdl/screenFlow/2010/10/screenFlow.wsdl";
declare namespace sf = "http://schemas.active-endpoints.com/appmodules/screenflow/2010/10/avosScreenflow.xsd";
declare namespace xsd = "http://www.w3.org/2001/XMLSchema";
declare namespace dict="http://schemas.active-endpoints.com/appmodules/repository/2010/10/avrepository.xsd";

(: Creates the screenflows id to be compatible with process name convention, id can sometimes contain special chars like colons, semicolons, spaces and ampersands. :)
declare function avsf:translateId( $str as xsd:string )
{
   replace($str, "\W", "_")
};

(: gets the activity name for the passed step, note this does not have to be unique in bpel. :)
declare function avsf:getActivityName( $step as node()* ) as xsd:string
{
  avsf:translateId( concat( "_", string($step/sf:title), "_", string($step/@id) ) ) 
};

(: Returns BPEL for targets of a step :)
declare function avsf:genTargets($step)
{
    <bpel:targets>
      {
        (: If start screen then check create a special target for it based on whether it is a subflow or the main start :)
        if (local-name($step) = "startScreen" and string($step/@sf_id) != "") then
          <bpel:target linkName="substart-{ $step/@sf_id/string() }"/> 
        else
          (: else Look at all actions for actions that point to this step :)
          for $action in $step/../sf:*/sf:actions/sf:action
          where $action/@targetScreen = $step/@id 
          return 
            <bpel:target linkName="{ $action/@id/string() }"/>
      }
    </bpel:targets>
};

(: Returns BPEL for sources of a step :)
declare function avsf:genSources($step)
{
   if (count($step/sf:actions/sf:action) > 0) then
      <bpel:sources>
      {
        for $action in $step/sf:actions/sf:action
        (: Create a condition to use for links if the link's target is a Jump To. This condition checks for endless loops. :)
        let $targetScreenType := local-name($step/../sf:*[@id=$action/@targetScreen])
        let $jumpToCondition := if ($targetScreenType = "jumpTo") then "and ($jumpToStepCounter <= 10)" else ""
        where not(contains($action/@id, "/"))
        return
          if (local-name($step) = "service") then
             <bpel:source linkName="{ $action/@id/string() }">
               <bpel:transitionCondition>
                  $screenFlowResponse/sf:selectedAction = "{ $action/text() }" { $jumpToCondition }
               </bpel:transitionCondition>
             </bpel:source>
          else if (local-name($step) = "subflow") then
             <bpel:source linkName="{ $action/@id/string() }">
               <bpel:transitionCondition>($subflowOutcome = "{ $action/text() }"{
                     (: catch all unknown outcomes in first action :)
                     if($action/@id = $action/../sf:action[1]/@id) then
                       " or $subflowOutcome = 'unknown'"
                     else
                       ""
               }) { $jumpToCondition }</bpel:transitionCondition>
             </bpel:source>
          else if (local-name($step) = "jumpTo") then
             <bpel:source linkName="{ $action/@id/string() }"/>
          else
             <bpel:source linkName="{ $action/@id/string() }">
               <bpel:transitionCondition>
                  $screenFlowResponse/sf:selectedAction = "{ $action/@id/string() }" { $jumpToCondition }
               </bpel:transitionCondition>
             </bpel:source>
      }
      </bpel:sources>
   (: Create the source end of link from end of subflow.  :)
   else if (count($step/sf:actions/sf:action) = 0 and $step/@sf_id != "") then
      <bpel:sources>
        <bpel:source linkName="endLink-{ $step/@id/string() }" /> 
      </bpel:sources>
   else
      ""
};



(: gets the next screen id string which is different for service name vs screen. :)
declare function avsf:getNextScreenId( $step as node()* ) as xsd:string
{
   if (local-name($step) = "service") then
      $step/sf:serviceName/text()
   else 
      $step/@id/string()
};


