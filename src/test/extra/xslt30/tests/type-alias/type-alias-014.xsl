<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   exclude-result-prefixes="#all"
   expand-text="yes"
>
  <!-- Tuple type with duplicate field names-->
  
  <xsl:variable name="in" expand-text="no">
    { "name": "Michael Kay",
      "date of birth": "1951-10-11",
      "place of birth": "Hannover"
    }
  </xsl:variable>
  
  <saxon:item-type name="person" as="tuple(name, 'date of birth', 'place of birth', 'name')"/>
  
  <xsl:template match="~type(person)">
    <person>
      <name>{?name}</name>
      <dob>{?"date of birth"}</dob>
      <pob>{?"place of birth"}</pob>
    </person>
  </xsl:template>
  
 
  <xsl:template name="xsl:initial-template">
    <out>
      <xsl:apply-templates select="parse-json($in)"/>
    </out>
  </xsl:template>
  
 
</xsl:stylesheet>
