<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx map"
   extension-element-prefixes="saxon"
   expand-text="yes"
>
  <xsl:output method="json" indent="yes" saxon:property-order="email courses"/>
  
  <xsl:variable name="input" select="json-doc('courses.json')"/>

  <!-- Perform inversion of courses/students JSON structure: see
    http://www.saxonica.com/papers/xmlprague-2016mhk.pdf for problem
    description -->
  
  <xsl:template name="xsl:initial-template">
    <xsl:variable name="students" 
        as="tuple(faculty as xs:string, 
                  course as xs:string,
                  last as xs:string,
                  first as xs:string,
                  email as xs:string)*">
      <xsl:for-each select="$input?*">
        <xsl:variable name="faculty" select="?faculty"/>
        <xsl:for-each select="?courses?*">
          <xsl:variable name="course" select="?course"/>
          <xsl:for-each select="?students?*">
            <xsl:sequence select="map:merge((., map{'faculty': $faculty, 'course': $course}))"/>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:variable>
    <saxon:array>
      <xsl:for-each-group select="$students" group-by="?email">
        <xsl:sort select="?last"/>
        <xsl:sort select="?first"/>
         <saxon:array-member>
            <xsl:map>
              <xsl:map-entry key="'email'" select="current-grouping-key()"/>
              <xsl:map-entry key="'courses'" select="array{current-group()?course}"/>
            </xsl:map>
         </saxon:array-member>
      </xsl:for-each-group>
    </saxon:array>
  </xsl:template>  

</xsl:stylesheet>
