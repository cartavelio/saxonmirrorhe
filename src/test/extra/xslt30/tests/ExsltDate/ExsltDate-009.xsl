<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:difference -->

<xsl:template match="/">
<out>
YEAR DIFFS
'<xsl:value-of select="date:difference('2000Z', '2003Z')" />'
'<xsl:value-of select="date:difference('2003Z', '2000Z')" />'
'<xsl:value-of select="date:difference('2000Z', '2000Z')" />'
'<xsl:value-of select="date:difference('2000Z', '2001-01-02Z')" />'
'<xsl:value-of select="date:difference('2000Z', '2001-01-01T00:00:01Z')" />'
'<xsl:value-of select="date:difference('2000-01-02Z', '2001Z')" />'
YEARMONTH DIFFS
'<xsl:value-of select="date:difference('2000-01Z', '2001-02Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '2001-01Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '2000-01Z')" />'
'<xsl:value-of select="date:difference('2001-02Z', '2000-01Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '2000-02Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '1999-12Z')" />'
'<xsl:value-of select="date:difference('1999-12Z', '2000-01Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '2000-03-02Z')" />'
'<xsl:value-of select="date:difference('2000-01-02Z', '2000-04Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '2000-05-02T10:10:10Z')" />'
DATE DIFFS
  general
'<xsl:value-of select="date:difference('2000-01-01Z', '2000-01-02Z')" />'
'<xsl:value-of select="date:difference('2000-01-01', '2000-01-02')" />'
'<xsl:value-of select="date:difference('2000-01-01Z', '2000-01-01Z')" />'
'<xsl:value-of select="date:difference('2000-01-02Z', '2000-01-01Z')" />'
'<xsl:value-of select="date:difference('2000-01-01Z', '1999-12-01Z')" />'
  months lengths and leap years
'<xsl:value-of select="date:difference('2001-01-01Z', '2001-02-01Z')" />'
'<xsl:value-of select="date:difference('2001-04-01Z', '2001-05-01Z')" />'
'<xsl:value-of select="date:difference('2001-02-01Z', '2001-03-01Z')" />'
'<xsl:value-of select="date:difference('2004-02-01Z', '2004-03-01Z')" />'
'<xsl:value-of select="date:difference('1900-02-01Z', '1900-03-01Z')" />'
'<xsl:value-of select="date:difference('1999-12-31Z', '2000-01-01Z')" />'
  year periods
'<xsl:value-of select="date:difference('2001-06-15Z', '2002-06-15Z')" />'
'<xsl:value-of select="date:difference('2003-06-15Z', '2004-06-15Z')" />'
'<xsl:value-of select="date:difference('2003-06-15Z', '2013-06-15Z')" />'
  truncation
'<xsl:value-of select="date:difference('2000-01-01Z', '2000-01-02T23:59:59Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T23:59:59Z', '2000-01-02Z')" />'
DATETIME DIFFS
  general
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-01T00:00:01Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00', '2000-01-01T00:00:01')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:01Z', '2000-01-01T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-01T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-02T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-02T00:00:00Z')" />'
  fractional seconds
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-01T00:00:01.234Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00.123Z', '2000-01-01T00:00:01Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-01T00:00:00.012Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-01T00:00:00.001Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00.123Z', '2000-01-01T00:00:00.789Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00.789Z', '2000-01-01T00:00:00.123Z')" />'
  day boundary
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-02T23:59:59Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T23:59:59Z', '2000-01-02T00:00:00Z')" />'
  months lengths and leap years
'<xsl:value-of select="date:difference('2001-01-01T00:00:00Z', '2001-02-01T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2001-04-01T00:00:00Z', '2001-05-01T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2001-02-01T00:00:00Z', '2001-03-01T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2004-02-01T00:00:00Z', '2004-03-01T00:00:00Z')" />'
'<xsl:value-of select="date:difference('1900-02-01T00:00:00Z', '1900-03-01T00:00:00Z')" />'
  year periods
'<xsl:value-of select="date:difference('2001-06-15T00:00:00Z', '2002-06-15T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2003-06-15T00:00:00Z', '2004-06-15T00:00:00Z')" />'
'<xsl:value-of select="date:difference('2003-06-15T00:00:00Z', '2013-06-15T00:00:00Z')" />'
INVALID INPUT
'<xsl:value-of select="date:difference('200oZ', '2003Z')" />'
'<xsl:value-of select="date:difference('2000Z', '20o3Z')" />'
'<xsl:value-of select="date:difference('2000-o1Z', '2001-02Z')" />'
'<xsl:value-of select="date:difference('2000-01Z', '2001-13Z')" />'
'<xsl:value-of select="date:difference('2000-01-o1Z', '2000-01-02Z')" />'
'<xsl:value-of select="date:difference('2000-01-01Z', '2001-02-29Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:00Z', '2000-01-01T00:00:o1Z')" />'
'<xsl:value-of select="date:difference('2000-01-01T00:00:61Z', '2000-01-01T00:00:01Z')" />'
UNSUPPORTED DATATYPES
'<xsl:value-of select="date:difference('00:00:00Z', '00:00:01Z')" />'
'<xsl:value-of select="date:difference('13', '12')" />'
'<xsl:value-of select="date:difference('12-30', '12-31')" />'
</out>

</xsl:template>
</xsl:stylesheet>
