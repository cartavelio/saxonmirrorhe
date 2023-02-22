<?xml version="1.0" encoding="utf-8"?>
<package xmlns='http://ns.saxonica.com/xslt/export' xmlns:fn='http://www.w3.org/2005/xpath-functions' xmlns:java-type='http://saxon.sf.net/java-type' xmlns:vv='http://saxon.sf.net/generated-variable' xmlns:xs='http://www.w3.org/2001/XMLSchema' when='2022-10-02T12:53:42.207266+01:00' id='0' version='30' packageVersion='1' saxonProduct='SAXON-EE' saxonVersion='12.1' buildDateTime='2022-10-02T12:53:42.208139+01:00' target='EE' targetVersion='1' relocatable='false' implicit='true'>
 <co id='0' binds='1'>
  <template name='Q{http://www.w3.org/1999/XSL/Transform}initial-template' flags='os' line='12' module='' slots='0'>
   <applyT role='body' defaultFunctionNS='http://www.w3.org/2005/xpath-functions' ns='test=https://saxonica.com/iss5701 xs=~ xsl=~ xml=~' line='13' flags='ti' bSlot='0'>
    <slash role='select' simple='1'>
     <treat as='N' diag='13|0|XTTE0510|'>
      <dot flags='a'/>
     </treat>
     <axis name='child' nodeTest='N'/>
    </slash>
   </applyT>
  </template>
 </co>
 <co id='1' binds='1'>
  <mode onNo='SC' flags='dW' patternSlots='0'>
   <templateRule prec='0' prio='-0.5' seq='0' rank='0' minImp='0' slots='0' matches='ND' flags='s' line='12' module=''>
    <p.nodeTest role='match' test='ND'/>
    <applyT role='action' defaultFunctionNS='http://www.w3.org/2005/xpath-functions' ns='test=https://saxonica.com/iss5701 xs=~ xsl=~ xml=~' line='13' flags='ti' bSlot='0'>
     <axis role='select' name='child' nodeTest='N'/>
    </applyT>
   </templateRule>
   <templateRule prec='0' prio='0.0' seq='1' rank='1' minImp='0' slots='0' matches='NE nQ{}p' flags='s' line='16' module=''>
    <p.nodeTest role='match' test='NE nQ{}p'/>
   </templateRule>
  </mode>
 </co>
 <overridden/>
 <output>
  <property name='method' value='xml'/>
  <property name='indent' value='no'/>
  <property name='encoding' value='utf-8'/>
  <property name='Q{http://saxon.sf.net/}stylesheet-version' value='30'/>
 </output>
 <decimalFormat/>
</package>
<?Σ 91cd0564?>
