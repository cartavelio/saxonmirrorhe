<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xpath-default-namespace="http://www.w3.org/XML/2004/xml-schema-test-suite/" xmlns:saxon="http://saxon.sf.net/">

<xsl:output method="xml" indent="yes"/>

<xsl:param name="in" select="doc('SaxonResults9.3.0.1.xml')"/>

<xsl:template match="/">
  <xsl:message terminate="yes">Use entry point summary, all-diffs, invalid-regex, or invalid-nist</xsl:message>
</xsl:template>

<xsl:template name="summary">
  <out>
    <summary>
      <xsl:for-each-group select="$in/testSuiteResults/testResult" group-by="concat(@validity, '~', @saxon:outcome)">
        <category result="{current-grouping-key()}" count="{count(current-group())}"/>
      </xsl:for-each-group>
    </summary>
    <breakdown>
      <xsl:for-each-group select="$in/testSuiteResults/testResult" group-by="@set">
        <testSet name="{@set}">
          <xsl:for-each-group select="current-group()" group-by="concat(@validity, '~', @saxon:outcome)">
            <category result="{current-grouping-key()}" count="{count(current-group())}"/>
          </xsl:for-each-group>                
        </testSet>
      </xsl:for-each-group>
    </breakdown>    
  </out>
</xsl:template>

<xsl:template name="all-diffs">
  <xsl:variable name="failures" 
       select="$in/testSuiteResults/testResult[@saxon:outcome='different' and not(@saxon:queried='true')]"/>
  <out on="{current-dateTime()}" failed="{count($failures)}" 
       queried="{count($in/testSuiteResults/testResult[@saxon:queried='true'])}">
    <xsl:for-each select="$failures">
      <test set="{@set}" group="{@group}" name="{@test}"/>
    </xsl:for-each>
  </out>
</xsl:template>  

<xsl:template name="invalid-regex">
  <out>
    <xsl:for-each select="$in/testSuiteResults/testResult[@validity='invalid' and @saxon:expect='valid' and @set='MS-Regex2006-07-15']">
      <test group="{@group}" name="{@test}"/>
    </xsl:for-each>
  </out>
</xsl:template>  

<xsl:template name="invalid-nist">
  <out>
    <xsl:for-each select="$in/testSuiteResults/testResult[@validity='invalid' and @saxon:expect='valid' and @set='NIST2004-01-14']">
      <test group="{@group}" name="{@test}"/>
    </xsl:for-each>
  </out>
</xsl:template>  

</xsl:stylesheet>