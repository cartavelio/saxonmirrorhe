<!-- This stylesheet demonstrates the use of sql:execute -->

<!-- sql004, was saxon051 -->

<xsl:stylesheet
	xmlns:sql="http://saxon.sf.net/sql"
 	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
	xmlns:saxon="http://saxon.sf.net/"
 	extension-element-prefixes="saxon sql">

  <xsl:param name="driver" select="'org.postgresql.Driver'"/>
  <xsl:param name="database" select="'jdbc:postgresql://localhost/sample'"/>  
  <xsl:param name="user" select="'testeditor'"/>
  <xsl:param name="password" select="'sekrit'"/>

<!-- This stylesheet writes the book list to a SQL database -->

<xsl:variable name="count" select="0" saxon:assignable="yes"/>

<xsl:template match="BOOKLIST">
    <xsl:if test="not(element-available('sql:connect'))">
        <xsl:message>sql:connect is not available</xsl:message>
    </xsl:if>
    <xsl:message>Connecting to <xsl:value-of select="$database"/>...</xsl:message>
    <xsl:variable name="connection" as="java:java.sql.Connection?" xmlns:java="http://saxon.sf.net/java-type">
        <sql:connect database="{$database}" user="{$user}" password="{$password}"
		    driver="{$driver}" auto-commit="no"
		    xsl:extension-element-prefixes="sql">
	        <xsl:fallback>
	            <xsl:message terminate="yes">SQL extensions are not installed</xsl:message>
            </xsl:fallback>
        </sql:connect>
    </xsl:variable>
    <xsl:message>Connected...</xsl:message>
    <xsl:apply-templates select="BOOKS">
       <xsl:with-param name="connection" select="$connection" tunnel="yes"/>
    </xsl:apply-templates>
    <out>Inserted <xsl:value-of select="$count"/> records.</out>
</xsl:template>

<xsl:template match="BOOKS">
    <xsl:param name="connection" tunnel="yes"/>
    <xsl:for-each select="ITEM">
       <sql:execute connection="$connection"
            statement="INSERT INTO book (title, author, category) VALUES ('{TITLE}', '{AUTHOR}', '@CAT')"/>
	   <saxon:assign name="count" select="$count+1"/>
    </xsl:for-each>
    <sql:execute connection="$connection" statement="ROLLBACK WORK"/>
    <sql:close connection="$connection"/>
</xsl:template>

</xsl:stylesheet>	
