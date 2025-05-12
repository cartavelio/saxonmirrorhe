<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
   xmlns:f="http://f/"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:bin="http://expath.org/ns/binary"
   version="3.0" expand-text="yes">
   <xsl:template name="main">
      <xsl:text>    // START GENERATED CONTENT&#xa;</xsl:text>
      <xsl:for-each-group select="32 to 55295, 57344 to 65533, 65536 to 111411" group-adjacent="
         let $u := codepoints-to-string(.) => upper-case() => string-to-codepoints()
         return (if (count($u) eq 1) then $u - . else 2000000+.)">
         <xsl:choose>
            <xsl:when test="current-grouping-key() = 0"/>
            <xsl:when test="count(current-group()) gt 1">
               <xsl:text>    range({current-group()[1]}, {current-group()[last()]}, {current-grouping-key()});&#xa;</xsl:text>
            </xsl:when>
            <xsl:when test="string-length(upper-case(codepoints-to-string(.))) gt 1">
               <xsl:text>    special({.}, "{f:depict(upper-case(codepoints-to-string(.)))}");&#xa;</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:text>    single({current-group()[1]}, {current-grouping-key()});&#xa;</xsl:text>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:for-each-group>
      <xsl:text>    // END GENERATED CONTENT&#xa;</xsl:text>
   </xsl:template>
   <xsl:function name="f:depict" as="xs:string">
      <xsl:param name="s" as="xs:string"/>
      <xsl:sequence select="string-join(string-to-codepoints($s) ! f:escape(.), '')"/>
   </xsl:function>
   <xsl:function name="f:escape" as="xs:string">
      <xsl:param name="cp" as="xs:integer"/>
      <xsl:sequence select="if ($cp lt 127 or $cp gt 65535) then codepoints-to-string($cp) else ('\u' || xs:hexBinary(bin:pack-integer($cp, 2)))"/>
   </xsl:function>
</xsl:transform>