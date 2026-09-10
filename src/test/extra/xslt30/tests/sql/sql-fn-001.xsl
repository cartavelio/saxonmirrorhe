
<xsl:stylesheet xmlns:sql="http://saxon.sf.net/sql" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" version="3.0" xmlns:saxon="http://saxon.sf.net/"
    xmlns:doc="http://www.saxonica.com/ns/doc" xmlns:java="http://saxon.sf.net/java-type"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
    exclude-result-prefixes="xs java doc map"
    extension-element-prefixes="saxon sql">

    <!-- insert your database details here, or supply them in parameters -->
    <!--<xsl:param name="database" select="'jdbc:hsqldb:file:db/test'"/>  
    <xsl:param name="user" select="'mike'"/>
<xsl:param name="password" select="'saxonxxx'"/>
<xsl:param name="driver" select="'org.hsqldb.jdbc.JDBCDriver'"/>
    -->

    <!-- Note that there are problems with single character values when used in sql:column,
    hence there are some normalizations of strings, or using sql:execute instead -->

    <xsl:param name="database" static='yes' select="'jdbc:mysql://localhost/saxontest'"/>
    <xsl:param name="user" static='yes' select="'jwl'"/>
    <xsl:param name="password" static='yes' select="'john'"/>
    <xsl:param name="driver" select="'com.mysql.jdbc.Driver'"/>

    <!-- This stylesheet writes the book list to a SQL database -->
    
    <xsl:variable name="connection" select="sql:connect(map{
        'database':$database,
        'driver':$driver,
        'user':$user,
        'password':$password,
        'autoCommit':true()})"/>

 
    <xsl:template name="DB-init"
        doc:doc="Initialise the database by dropping the table and setting up a new one">
        <xsl:param name="connection" tunnel="yes"/>
        <xsl:try select="sql:execute($connection, 'drop table book')">
            <xsl:catch errors="*"/>
        </xsl:try>
        <xsl:try select="sql:execute($connection, 'create table book (title varchar(100), author varchar(100), CATEGORY char(1), price decimal(10, 2), date date)')">
            <xsl:catch errors="*"/>
        </xsl:try>
    </xsl:template>

    <xsl:template name="DB-load"
        doc:doc="Make a new database that is loaded with data. The database is then CLOSED.">
        <xsl:message>Into DB-load</xsl:message>
        <xsl:variable name="void" as="item()*">
            <xsl:call-template name="DB-init">
                <xsl:with-param name="connection" select="$connection" tunnel="yes"/>
            </xsl:call-template>
        </xsl:variable>
        <xsl:message>Done DB-init ... (<xsl:value-of select="count($void)"/>)</xsl:message>
        <xsl:variable name="void" as="item()*">
            <xsl:apply-templates select="BOOKLIST">
                <xsl:with-param name="connection" select="$connection" tunnel="yes"/>
            </xsl:apply-templates>
        </xsl:variable>
        <xsl:sequence select="$void"/>
    </xsl:template>

    <xsl:template name="test-001" doc:doc="Just setting up the database">
        <void><xsl:call-template name="DB-load"/></void>
        <saxon:do action="$connection?close()"/>
    </xsl:template>

    <xsl:template name="test-002" doc:doc="Retrieving a column">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results" as="map(*)*" select="sql:query($connection, 'book', '*')"/>
        
        <!--<xsl:message select="'RESULTS: ' || serialize($results, map{'method':'adaptive'})"/>-->

        <out rows="{count($results)}">
            <xsl:for-each-group select="$results?CATEGORY" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
        
        <saxon:do action="$connection?close()"/>

    </xsl:template>
    
    <xsl:template name="test-003" doc:doc="Retrieving rows with a WHERE clause">
        <xsl:call-template name="DB-load"/>
        
        <xsl:variable name="predicate">CATEGORY='S'</xsl:variable>      
        <xsl:variable name="results" as="map(*)*" select="sql:query($connection, 'book', 'TITLE', $predicate)"/>
        
        <xsl:message select="'RESULTS: ' || serialize($results, map{'method':'adaptive'})"/>
        
        <out rows="{count($results)}">
            <xsl:for-each select="$results?TITLE">
                <xsl:sort select="."/>
                <title>
                    <xsl:value-of select="."/>
                </title>
            </xsl:for-each>
        </out>
        
        <saxon:do action="$connection?close()"/>
        
    </xsl:template>

    <xsl:template name="test-004"
        doc:doc="Retrieving multiple columns through functions with patterns">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results"
            select="sql:query($connection,'book',('author','title'),'author LIKE ''%a%''')"/>
        <out rows="{count($results)}">[
            <xsl:value-of select="$results!serialize(., map{'method':'json'})" separator=",&#xa;"/>
        ]    
        </out>
        <saxon:do action="$connection?close()"/>
    </xsl:template>

    <xsl:template name="test-005" doc:doc="Simple update">
        <xsl:call-template name="DB-load"/>
 
        <xsl:variable name="predicate">author='Bonner'</xsl:variable>
        <xsl:sequence select="sql:update($connection, 'BOOK', $predicate,
            map{'CATEGORY': 'F'})"/>
        <xsl:variable name="results" select="sql:query($connection, 'book', '*')"/>       
        <out bonner-category="{$results[?AUTHOR='Bonner']?CATEGORY}"/>
        <saxon:do action="$connection?close()"/>
    </xsl:template>
    
    <xsl:template name="test-006" doc:doc="Retrieving rows using a prepared query">
        <xsl:call-template name="DB-load"/>
            
        <xsl:variable name="query" as="function(*)" select="sql:prepared-query($connection, 'SELECT date, price FROM book WHERE author = ?')"/>
        
        <out>
            <xsl:for-each select="('Danzig', 'Bonner')">
                <xsl:variable name="result" select="$query(.)"/>
                <result for="{.}">
                    <xsl:value-of select="serialize(array{$result}, map{'method':'adaptive'})"/>
                </result>
            </xsl:for-each>
        </out>
        
        <saxon:do action="$connection?commit(), $connection?close()"/>
        
    </xsl:template>
    
    <xsl:template name="test-007" doc:doc="Commit, delete, and rollback">
        <out>
            <xsl:call-template name="DB-load"/>
            <saxon:do action="$connection?setAutoCommit(false())"/>
            
            <xsl:variable name="result" select="sql:query($connection, 'book', '*')"/>
            <a count="{count($result)}">
                <xsl:value-of select="serialize(array{$result}, map{'method':'adaptive'})"/>
            </a>
            
            <xsl:variable name="predicate">author='Bonner'</xsl:variable>
            
            <xsl:variable name="result" select="sql:query($connection, 'book', '*', $predicate)"/>
            <b count="{count($result)}">
                <xsl:value-of select="serialize(array{$result}, map{'method':'adaptive'})"/>
            </b>
            
            <saxon:do action="sql:delete($connection, 'book', $predicate)"/>
            
            <xsl:variable name="result" select="sql:query($connection, 'book', '*')"/>
            <c count="{count($result)}">
                <xsl:value-of select="serialize(array{$result}, map{'method':'adaptive'})"/>
            </c>
            <saxon:do action="$connection?rollback_0()"/>
            <d>
                <xsl:value-of select="sql:query($connection, 'book', '*') => count()"/>
            </d>
            
            <saxon:do action="$connection?close()"/>
        </out>
        
    </xsl:template>
    
    <xsl:template name="test-008" doc:doc="Prepared statement execute">
        <xsl:call-template name="DB-init"/>
        
        <saxon:do action="sql:execute($connection, 'DELETE FROM book')"/>
        
        <xsl:variable name="add-book"
            select="$connection => sql:prepared-statement(
            'INSERT INTO book (title, author, category, price, date) VALUES (?, ?, ?, ?, ?)')"/>
        
        <xsl:for-each select="//ITEM">
            <saxon:do action="$add-book(TITLE, AUTHOR, @CAT, PRICE, DATE)"/>
        </xsl:for-each>
        
        <xsl:variable name="results" select="sql:query($connection, 'book', '*')"/>
        <out rows="{count($results)}"/>
 
    </xsl:template>
    

    <xsl:template match="BOOKLIST">
        <xsl:apply-templates select="BOOKS" mode="#current"/>
    </xsl:template>

    <xsl:template match="BOOKS">
        <xsl:param name="connection" tunnel="yes"/>
        <xsl:for-each select="ITEM">
            <saxon:do action="sql:insert($connection, 'book', 
                map{'title': TITLE,
                    'author': AUTHOR,
                    'CATEGORY': @CAT,
                    'price': PRICE,
                    'date': DATE})"/>
        </xsl:for-each>
    </xsl:template>

</xsl:stylesheet>
