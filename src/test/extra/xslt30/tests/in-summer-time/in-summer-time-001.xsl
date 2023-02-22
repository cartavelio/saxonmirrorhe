<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs xsl">

  <!-- test saxon:in-summer-time -->
  
  <xsl:output indent="yes"/>
  
  <xsl:template name="main">
    <out>
      <a><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00Z'), 'gb')"/></a>
      <b><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00Z'), 'us')"/></b>
      <c><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00Z'), 'Europe/Paris')"/></c>
      <d><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00Z'), 'America/New_York')"/></d> 
      <e><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00'), 'Europe/Paris')"/></e>
      <f><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00'), 'America/New_York')"/></f>
      <g><xsl:value-of select="saxon:in-summer-time(xs:dateTime('1066-03-14T00:00:00'), 'America/New_York')"/></g>
      <h><xsl:value-of select="saxon:in-summer-time(xs:dateTime('2008-03-14T00:00:00'), 'Solar_System/The_Moon')"/></h>             
    </out>
  </xsl:template>
  


</xsl:stylesheet>