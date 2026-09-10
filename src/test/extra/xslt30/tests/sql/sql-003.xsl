<!-- This stylesheet demonstrates the use of element extensibility with SAXON 
       It uses both the SQL extensions and saxon:assign -->
       
<!-- sql003: test sql:update and sql:query -->

<xsl:stylesheet
	xmlns:sql="http://saxon.sf.net/sql"
 	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
	xmlns:saxon="http://saxon.sf.net/"
 	extension-element-prefixes="saxon sql">

  <xsl:output method="xml" indent="yes"></xsl:output>

  <xsl:param name="driver" select="'org.postgresql.Driver'"/>
  <xsl:param name="database" select="'jdbc:postgresql://localhost/sample'"/>  
  <xsl:param name="user" select="'testeditor'"/>
  <xsl:param name="password" select="'sekrit'"/>

<xsl:template name="main" match="/">
    <xsl:if test="not(element-available('sql:connect'))">
        <xsl:message>sql:connect is not available</xsl:message>
    </xsl:if>
    <xsl:message>Connecting to <xsl:value-of select="$database"/>...</xsl:message>
    <xsl:variable name="connection" as="java:java.sql.Connection?" xmlns:java="http://saxon.sf.net/java-type">
        <sql:connect database="{$database}" user="{$user}" password="{$password}"
		    driver="{$driver}" xsl:extension-element-prefixes="sql">
	        <xsl:fallback>
	            <xsl:message terminate="yes">SQL extensions are not installed</xsl:message>
            </xsl:fallback>
        </sql:connect>
    </xsl:variable>
    <xsl:message>Connected...</xsl:message>
    <sql:update connection="$connection" table="book" where="author='Bonner'">
       <sql:column name="category" select="'F'"/>
    </sql:update>
    <xsl:variable name="results">
      <sql:query connection="$connection" table="book" column="category" column-tag="category"/>
    </xsl:variable>
    <out>
      <xsl:for-each-group select="$results//category" group-by=".">
        <xsl:sort select="current-grouping-key()"/>
        <c count="{count(current-group())}"><xsl:value-of select="current-grouping-key()"/></c>
      </xsl:for-each-group>
    </out>
</xsl:template>

</xsl:stylesheet>	
