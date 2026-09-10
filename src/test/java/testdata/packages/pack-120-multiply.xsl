<?xml version="1.0" encoding="utf-8"?>
<package xmlns='http://ns.saxonica.com/xslt/export' xmlns:fn='http://www.w3.org/2005/xpath-functions' xmlns:java-type='http://saxon.sf.net/java-type' xmlns:vv='http://saxon.sf.net/generated-variable' xmlns:xs='http://www.w3.org/2001/XMLSchema' when='2019-05-13T16:37:23.509+01:00' id='0' name='http://example.org/pack-120.xsl' version='30' declaredModes='1' packageVersion='1' saxonVersion='10.0.0.1' target='HE' targetVersion='1' relocatable='false' implicit='false'>
 <co id='0' binds=''>
  <globalVariable name='Q{}opname' type='xs:string' line='8' module='file:/Users/mike/repo/tests/junit/testdata/packages/pack-120.xsl' visibility='PUBLIC' flags='s'>
   <str val='multiply'/>
  </globalVariable>
 </co>
 <co id='1' binds=''>
  <globalVariable name='Q{}operator' type='xs:string' line='9' module='file:/Users/mike/repo/tests/junit/testdata/packages/pack-120.xsl' visibility='PUBLIC' flags='s'>
   <str val='*'/>
  </globalVariable>
 </co>
 <co id='2' vis='PUBLIC' binds=''>
  <function name='Q{http://example.org/package}multiply' line='11' module='file:/Users/mike/repo/tests/junit/testdata/packages/pack-120.xsl' eval='16' flags='pU' as='xs:integer' slots='2'>
   <arg name='Q{}x' as='xs:integer'/>
   <arg name='Q{}y' as='xs:integer'/>
   <arith role='body' baseUri='file:/Users/mike/repo/tests/junit/testdata/packages/pack-120.xsl' ns='xsl=~ f=http://example.org/package xs=~' line='14' op='*' calc='i*i'>
    <varRef name='Q{}x' slot='0'/>
    <varRef name='Q{}y' slot='1'/>
   </arith>
  </function>
 </co>
 <co id='3' binds=''>
  <mode onNo='TC' flags='dWe' patternSlots='0'/>
 </co>
 <overridden/>
 <output>
  <property name='{http://saxon.sf.net/}stylesheet-version' value='30'/>
 </output>
 <decimalFormat/>
</package>
<?Σ 29acf150?>
