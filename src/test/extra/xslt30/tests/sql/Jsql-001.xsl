<!-- This stylesheet demonstrates the use of element extensibility with SAXON 
       It uses both the SQL extensions and saxon:assign -->

<!-- sql001, was saxon051 -->

<xsl:stylesheet xmlns:sql="http://saxon.sf.net/sql" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" version="3.0" xmlns:saxon="http://saxon.sf.net/"
    xmlns:doc="http://www.saxonica.com/ns/doc" xmlns:java="http://saxon.sf.net/java-type"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
    xmlns:err="http://www.w3.org/2005/xqt-errors"
    exclude-result-prefixes="xs java doc map err"
    extension-element-prefixes="saxon sql"
    expand-text="true">

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

    <xsl:variable name="count" select="0" saxon:assignable="yes"/>
    
    <xsl:variable name="connection" select="sql:DB-connect('yes')"/>

    <xsl:function name="sql:DB-connect" as="java:java.sql.Connection?">
        <xsl:param name="auto-commit" as="xs:string"/>
        <xsl:if test="not(element-available('sql:connect'))">
            <xsl:message>sql:connect is not available</xsl:message>
        </xsl:if>
        <xsl:message>Connecting to <xsl:value-of select="$database"/>...</xsl:message>
        <xsl:variable name="connection" as="java:java.sql.Connection?">
            <sql:connect database="{$database}" user="{$user}" password="{$password}"
                driver="{$driver}" xsl:extension-element-prefixes="sql" auto-commit="{$auto-commit}">
                <xsl:fallback>
                    <xsl:message terminate="yes">SQL extensions are not installed</xsl:message>
                </xsl:fallback>
            </sql:connect>
        </xsl:variable>
        <!--<xsl:variable name="connectionZZ" as="java:java.sql.Connection?"
            select="sql:connect($database,$driver,$user,$password,$auto-commit)"/>-->
        <xsl:message>Connected...</xsl:message>
        <xsl:sequence select="$connection"/>
    </xsl:function>

    <xsl:template name="DB-init"
        doc:doc="Initialise the database by dropping the table and setting up a new one">
        <xsl:param name="connection" tunnel="yes"/>
        <xsl:try>
            <xsl:message>Dropping table book ...</xsl:message>
            <sql:execute connection="$connection" statement="drop table book"/>
            <xsl:message>Dropped table book ...</xsl:message>
            <xsl:catch errors="*">
              <xsl:message>Failed to drop table book... </xsl:message>
            </xsl:catch>
        </xsl:try>
      <!--<xsl:try>
        <xsl:message>Dropping table BOOK ...</xsl:message>
        <sql:execute connection="$connection" statement="drop table BOOK"/>
        <xsl:message>Dropped table BOOK ...</xsl:message>
        <xsl:catch errors="*">
          <xsl:message>Failed to drop table BOOK ...</xsl:message>
        </xsl:catch>
      </xsl:try>-->
        <xsl:try>
          <xsl:message>Creating table book ...</xsl:message>
          <sql:execute connection="$connection"
            statement="create table book (title varchar(100), author varchar(100), category char(1), price decimal(10, 2), date date)"/>
          <xsl:message>Created table book ...</xsl:message>
          <xsl:catch errors="*">
            <xsl:message>Failed to create table book ... {$err:description}</xsl:message>
          </xsl:catch>
        </xsl:try>
    </xsl:template>

    <xsl:template name="DB-load"
        doc:doc="Make a new database that is loaded with data. The database is then CLOSED.">
        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <xsl:call-template name="DB-init">
            <xsl:with-param name="connection" select="$connection" tunnel="yes"/>
        </xsl:call-template>
        <xsl:apply-templates select="BOOKLIST">
            <xsl:with-param name="connection" select="$connection" tunnel="yes"/>
        </xsl:apply-templates>
        <sql:close connection="$connection"/>
    </xsl:template>

    <xsl:template name="test-001" doc:doc="Just setting up the database">
        <xsl:call-template name="DB-load"/>
        <out>Inserted <xsl:value-of select="$count"/> records.</out>
    </xsl:template>

    <xsl:template name="test-002" doc:doc="Retrieving a column">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="category" column-tag="category"
            />
        </xsl:variable>
        <sql:close connection="$connection"/>
      
      <xsl:message select="serialize($results, map{'method':'xml'})"/>

        <out rows="{count($results/*)}">
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>

    <!--<xsl:template name="test-002a" doc:doc="Retrieving a column through functions">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results"
            select="(sql:query($connection,'book','category',''), sql:close($connection))"/>

        <out rows="{count($results/*)}">
            <xsl:copy-of select="$results"/>
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>-->

    <!--<xsl:template name="test-002b" doc:doc="Retrieving a column through functions - missing where">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results"
            select="let $conn := $connection return
            (sql:query($conn,'book','category'),sql:close($conn))"/>

        <out rows="{count($results/*)}">
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>
-->
    <!--<xsl:template name="test-002c"
        doc:doc="Retrieving multiple columns through functions with patterns">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results"
            select="let $conn := sql:connect($database,$driver,$user,$password,'yes') return
            (sql:query($conn,'book','author,title','author LIKE ''%a%'''),sql:close($conn))"/>
        <out rows="{count($results)}">
            <xsl:sequence select="$results"/>
        </out>
    </xsl:template>-->

    <!--<xsl:template name="test-002d" doc:doc="Retrieving a column through functions - column sequence">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results"
            select="let $conn := sql:connect($database,$driver,$user,$password,'yes') return
            (sql:query($conn,'book',('author','category')),sql:close($conn))"/>

        <out entries="{count($results/*)}">
            <xsl:sequence select="$results"/>
        </out>
    </xsl:template>
-->


    <xsl:template name="test-003" doc:doc="Retrieving a column with query">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="category"
                where="author='Bonner'" column-tag="category"/>
        </xsl:variable>
        <sql:close connection="$connection"/>

        <out rows="{count($results/*)}">
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>

    <xsl:template name="test-003a" doc:doc="Retrieving a column with query with multiple columns">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="author,category"
                where="author='Bonner'"/>
        </xsl:variable>
        
        <out rows="{count($results/*)}">
            <!-- <results>
                <xsl:sequence select="$results"/>
            </results>-->
            <!-- Some SQL engines return the column names as requested, some normalize to upper case -->
            <xsl:for-each-group select="$results//(category|CATEGORY)" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
            <xsl:for-each-group select="$results//(author|AUTHOR)" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <a>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </a>
            </xsl:for-each-group>
        </out>
        <sql:close connection="$connection"/>
    </xsl:template>

    <xsl:template name="test-004" doc:doc="Simple update">
        <xsl:call-template name="DB-load"/>
        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <sql:update connection="$connection" table="book" where="author='Bonner'">
            <sql:column name="category" select="'F'"/>
        </sql:update>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="category"
                where="author='Bonner'" column-tag="category"/>
        </xsl:variable>
        <sql:close connection="$connection"/>
        <out rows="{count($results/*)}">
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>

    <xsl:template name="test-005" doc:doc="Simple delete">
        <xsl:call-template name="DB-load"/>
        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <sql:delete connection="$connection" table="book" where="author='Bonner'"/>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="category"
                where="author='Bonner'" column-tag="category"/>
        </xsl:variable>
        <sql:close connection="$connection"/>
        <out rows="{count($results/*/*)}">
            <xsl:copy-of select="$results"/>
        </out>
    </xsl:template>

    <xsl:template name="test-006" doc:doc="Simple execute">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="connection" select="sql:DB-connect('yes')"/>
        <xsl:for-each select="//ITEM">
            <sql:execute connection="$connection"
                statement="INSERT INTO book (title, author, category) VALUES ('{TITLE}', '{AUTHOR}', '{@CAT}')"
            />
        </xsl:for-each>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="category"
                where="author='Bonner'" column-tag="category"/>
        </xsl:variable>
        <sql:close connection="$connection"/>

        <out rows="{count($results/*)}">
            <!--<results>
                <xsl:sequence select="$results"/>
            </results>-->
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>


    <xsl:template name="test-007" doc:doc="Simple rollback">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="connection" select="sql:DB-connect('no')"/>
        <sql:execute connection="$connection" statement="COMMIT WORK"/>
        <xsl:for-each select="//ITEM">
            <sql:execute connection="$connection"
                statement="INSERT INTO book (title, author, category) VALUES ('{TITLE}', '{AUTHOR}', '{@CAT}')"
            />
        </xsl:for-each>
        <sql:execute connection="$connection" statement="ROLLBACK WORK"/>
        <xsl:variable name="results">
            <sql:query connection="$connection" table="book" column="category"
                where="author='Bonner'" column-tag="category"/>
        </xsl:variable>
        <sql:close connection="$connection"/>

        <out rows="{count($results/*)}">
            <xsl:for-each-group select="$results//category" group-by=".">
                <xsl:sort select="current-grouping-key()"/>
                <c>
                    <xsl:value-of select="normalize-space(current-grouping-key())"/>
                </c>
            </xsl:for-each-group>
        </out>
    </xsl:template>

    <!--<xsl:template name="test-010" doc:doc="Retrieving a column through functions, returning map">
        <xsl:call-template name="DB-load"/>

        <xsl:variable name="results"
            select="let $conn := sql:connect($database,$driver,$user,$password,'yes') return
            (sql:queryMap($conn,'SELECT * FROM book'),sql:close($conn))"
            as="map(*)*"/>

        <out rows="{count($results)}">
            <xsl:for-each select="$results">
                <row>
                    <xsl:variable name="this" select="."/>
                    <xsl:for-each select="map:keys(.)">
                        <xsl:element name="{.}">
                            <xsl:value-of select="$this(.)"/>
                        </xsl:element>
                    </xsl:for-each>
                </row>
            </xsl:for-each>
        </out>
    </xsl:template>
    
    <xsl:template name="test-011" doc:doc="Retrieving a column through functions, returning map">
        <xsl:call-template name="DB-load"/>
        
        <xsl:variable name="results"
            select="let $conn := sql:connect($database,$driver,$user,$password,'yes'),
            $query := sql:queryMap($conn,?) return
            ($query('SELECT * FROM book'),sql:close($conn))"
            as="map(*)*"/>
        
        <out rows="{count($results)}">
            <xsl:for-each select="$results">
                <row>
                    <xsl:variable name="this" select="."/>
                    <xsl:for-each select="map:keys(.)">
                        <xsl:element name="{.}">
                            <xsl:value-of select="$this(.)"/>
                        </xsl:element>
                    </xsl:for-each>
                </row>
            </xsl:for-each>
        </out>
    </xsl:template>
    <xsl:template name="test-012" doc:doc="Connection as a map; Retrieving all columns through functions, returning map">
        <xsl:call-template name="DB-load"/>
        
        <xsl:variable name="results"
            select="let $conn := sql:connectMap($database,$driver,$user,$password,'yes') return
            ($conn('query')('SELECT * FROM book'),$conn('close')())"
            />
        <xsl:message select="count($results)"/>
        <out rows="{count($results)}">
            <xsl:for-each select="$results">
                <row>
                    <xsl:variable name="this" select="."/>
                    <xsl:for-each select="map:keys(.)">
                        <xsl:element name="{.}">
                            <xsl:value-of select="$this(.)"/>
                        </xsl:element>
                    </xsl:for-each>
                </row>
            </xsl:for-each>
        </out>
    </xsl:template>
    <xsl:template name="test-013" doc:doc="Connection as a map; Retrieving selected columns through functions, returning map">
        <xsl:call-template name="DB-load"/>
        
        <xsl:variable name="results"
            select="let $conn := sql:connectMap($database,$driver,$user,$password,'yes') return
            ($conn('query')('SELECT author,title FROM book'),$conn('close')())"
        />
        <xsl:message select="count($results)"/>
        <out rows="{count($results)}">
            <xsl:for-each select="$results">
                <row>
                    <xsl:variable name="this" select="."/>
                    <xsl:for-each select="map:keys(.)">
                        <xsl:element name="{.}">
                            <xsl:value-of select="$this(.)"/>
                        </xsl:element>
                    </xsl:for-each>
                </row>
            </xsl:for-each>
        </out>
    </xsl:template>
-->


    <xsl:template match="BOOKLIST">
        <xsl:apply-templates select="BOOKS" mode="#current"/>
    </xsl:template>

    <!-- The sql:insert is avoided here as the categories are single character values -->
    <xsl:template match="BOOKS">
        <xsl:param name="connection" tunnel="yes"/>
        <xsl:for-each select="ITEM">
            <!--<sql:execute connection="$connection"
                statement="INSERT INTO book (title, author, category) VALUES ('{TITLE}', '{AUTHOR}', '{@CAT}')"/>-->
            <sql:insert table="book" connection="$connection">
                <sql:column name="title" select="TITLE"/>
                <sql:column name="author" select="AUTHOR"/>
                <sql:column name="category" select="@CAT"/>
                <sql:column name="price" select="PRICE"/>
                <sql:column name="date" select="DATE"/>
            </sql:insert>
            <saxon:assign name="count" select="$count+1"/>
        </xsl:for-each>
    </xsl:template>

</xsl:stylesheet>
