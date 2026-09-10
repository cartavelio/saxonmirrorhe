<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:saxon="http://saxon.sf.net/"
                extension-element-prefixes="saxon"
		version="2.0">

<!-- Tests use of saxon:evaluate() within a predicate -->
<!-- Bug 6.1/017 -->
<!-- Formerly test saxon056 -->

<!-- RUN as: ./saxon test.xml test.xsl sortby1=@category -->
<!-- test.xml can be empty file -->



<xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

<!-- primary sort fields -->

<!-- ******************************************************************************************************** -->
<!-- **************** change field below from "param" to "variable" and it works as intended on version 5.5.1 ************** -->
<!-- ******************************************************************************************************** -->

<xsl:param name="sortby1" select="'@category'"/>

<xsl:variable name="indata">
 <things>
  <thing category="forbin" name="SVC board vhdl code consolidation"/>
  <thing category="forbin" name="IOgen template incorporation"/>
  <thing category="notforbin" name="General m4-template maintenance"/>
 </things>
</xsl:variable>

<!-- main -->
<xsl:template name="main">
 <xsl:variable name="workaround" select="$indata/things/thing[saxon:evaluate($sortby1)='forbin']"
                saxon:explain="no" xmlns:saxon="http://saxon.sf.net/"/>
 <out>               
 <xsl:for-each select="$workaround">
  <td><xsl:value-of select="@name"/></td>
 </xsl:for-each>
 </out>  
</xsl:template>

</xsl:stylesheet>

