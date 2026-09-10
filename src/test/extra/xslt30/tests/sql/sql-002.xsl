<!-- This stylesheet demonstrates the use of element extensibility with SAXON 
       It uses both the SQL extensions and saxon:assign -->
       
<!-- variant of sql001 to test bug clearance - sql:column being moved out of a for-each loop -->       

<!-- sql002 -->

<xsl:stylesheet
	xmlns:sql="http://saxon.sf.net/sql"
 	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
	xmlns:saxon="http://saxon.sf.net/"
 	extension-element-prefixes="saxon sql">

<!-- insert your database details here, or supply them in parameters -->
    <!--<xsl:param name="database" select="'jdbc:hsqldb:file:db/test'"/>  
    <xsl:param name="user" select="'mike'"/>
<xsl:param name="password" select="'saxonxxx'"/>
<xsl:param name="driver" select="'org.hsqldb.jdbc.JDBCDriver'"/>
    -->
    <xsl:param name="database" select="'jdbc:mysql://localhost/saxontest'"/>  
    <xsl:param name="user" select="'jwl'"/>
    <xsl:param name="password" select="'john'"/>
    <xsl:param name="driver" select="'com.mysql.jdbc.Driver'"/>

<!-- This stylesheet writes the book list to a SQL database -->

<xsl:variable name="count" select="0" saxon:assignable="yes"/>

<xsl:template match="BOOKLIST">
    <xsl:if test="not(element-available('sql:connect'))">
        <xsl:message>sql:connect is not available</xsl:message>
    </xsl:if>
    <xsl:message>Connecting to <xsl:value-of select="$database"/>...</xsl:message>
    <xsl:variable name="connection" as="java:java.sql.Connection?" xmlns:java="http://saxon.sf.net/java-type">
        <sql:connect database="{$database}" user="{$user}" password="{$password}"
		    driver="{$driver}" xsl:extension-element-prefixes="sql">
	        <!--<xsl:fallback>
	            <xsl:message terminate="yes">SQL extensions are not installed</xsl:message>
            </xsl:fallback>-->
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
    	<sql:insert table="book" connection="$connection">
	    	<sql:column name="title" select="TITLE"/>
        <sql:column name="author" select="AUTHOR"/>
        <sql:column name="category" select="'M'"/>
    	</sql:insert>
	<saxon:assign name="count" select="$count+1"/>
    </xsl:for-each>
    <sql:close connection="$connection"/>
</xsl:template>

</xsl:stylesheet>	
