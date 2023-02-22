<!-- This stylesheet demonstrates the use of element extensibility with SAXON 
       It uses the SQL extensions -->
       
<!-- sql003: test sql:query with column tags obtained from the metadata -->

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

<xsl:template match="/">
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
    <sql:execute connection="$connection" statement="drop table book"/>    
    <sql:execute connection="$connection"
        statement="create table book (title varchar(100), author varchar(100), category varchar(20))"/>
    <xsl:for-each select="//ITEM">
       <sql:execute connection="$connection"
            statement="INSERT INTO book (title, author, category) VALUES ('{TITLE}', '{AUTHOR}', '{@CAT}')"/>
    </xsl:for-each>
    <out>
      <sql:query connection="$connection" table="book" column="*" column-tag="#auto"/>
    </out>
</xsl:template>

<xsl:output indent="yes"/>

</xsl:stylesheet>	
