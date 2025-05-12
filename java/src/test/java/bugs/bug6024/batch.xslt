<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:saxon="http://saxon.sf.net/"
                xmlns:shared="urn:shared-functions"
                exclude-result-prefixes="#all"
                extension-element-prefixes="saxon"
                version="2.0">
   <xsl:import href="shared-functions.xslt"/>
   <xsl:output method="text" omit-xml-declaration="yes"/>

   <xsl:template name="xsl:initial-template" match="/">
      <xsl:variable name="startTime" select="saxon:timestamp()"/>
      <xsl:sequence select="$startTime[shared:always-false()]"/>
      <xsl:sequence select="shared:log('INFO', 'Batch execution starting...')"/>
      <xsl:sequence select="shared:log('INFO', 'Batch execution succeeded')"/>
      <xsl:sequence select="shared:log-execution-time($startTime)"/>
   </xsl:template>

   <xsl:function name="shared:always-false" as="xs:boolean">
      <xsl:sequence select="current-date() lt xs:date('2000-01-01')"/>
   </xsl:function>

   <xsl:function name="shared:log">
      <xsl:param name="level" as="xs:string"/>
      <xsl:param name="message" as="item()*"/>
      <xsl:message select="serialize($message)"/>
   </xsl:function>

   <xsl:function name="shared:log-execution-time">
      <xsl:param name="startTime" as="xs:dateTime"/>
      <xsl:sequence select="shared:log('DEBUG', concat('Execution time: ', (saxon:timestamp() - $startTime) div xs:dayTimeDuration('PT1S'), 's'))"/>
   </xsl:function>

</xsl:stylesheet>
