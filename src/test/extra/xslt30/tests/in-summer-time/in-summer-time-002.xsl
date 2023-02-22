<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs xsl">

  <!-- test saxon:adjust-to-civil-time -->
  
  <xsl:output indent="yes"/>
  
  <xsl:template name="main">
    <out>
      <a><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-01-14T00:00:00Z'), 'Europe/London')"/></a>
      <b><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-01-14T00:00:00-05:00'), 'America/Los_Angeles')"/></b>
      <c><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-01-14T00:00:00Z'), 'Europe/Paris')"/></c>
      <d><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-01-14T00:00:00Z'), 'America/New_York')"/></d>
      <xsl:choose>
      <xsl:when test="implicit-timezone() eq xs:duration('PT0S')"> 
        <e><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-07-14T00:00:00'), 'Europe/Paris')"/></e>
        <f><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-07-14T00:00:00'), 'America/New_York')"/></f>
        <g><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('1066-07-14T00:00:00'), 'America/New_York')"/></g>
        <h><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-07-14T00:00:00'), 'Solar_System/The_Moon')"/></h>
      </xsl:when>
      <xsl:otherwise>
        <!-- assume British Summer Time, to get the same results as the previous case -->
        <e><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-07-14T01:00:00'), 'Europe/Paris')"/></e>
        <f><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-07-14T01:00:00'), 'America/New_York')"/></f>
        <g><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('1066-07-14T01:00:00'), 'America/New_York')"/></g>
        <h><xsl:value-of select="saxon:adjust-to-civil-time(xs:dateTime('2008-07-14T01:00:00'), 'Solar_System/The_Moon')"/></h>      
      </xsl:otherwise>
      </xsl:choose>             
    </out>
  </xsl:template>
  


</xsl:stylesheet>