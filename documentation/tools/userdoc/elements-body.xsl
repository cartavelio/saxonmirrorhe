<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:el="http://www.saxonica.com/ns/doc/elements"
    xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" exclude-result-prefixes="xs el e xsi"
    version="3.0"> 

    <xsl:output indent="no"/>

    <!-- Designed to transform src/elements/xsl-elements-expanded.xml; also used for:
        src/userdoc/extensions/instructions.xml (contains saxon-element elements)
        src/userdoc/extensions/serialization-params.xml (contains element-syntax elements)
        src/userdoc/extensions/sql-extension.xml (contains saxon-element elements) -->

    <xsl:variable name="implementation-doc"
                  select="doc('../../src/viewer_appJS/doc/implement.xml')"
                  as="document-node()"/>
    <xsl:variable name="id" select="article/@id|section[1]/@id"/>

    <xsl:template match="article|section[h1]">
        <xsl:copy>
            <xsl:sequence select="@id|@title|@class"/>
            <xsl:comment>Generated - <xsl:value-of select="current-dateTime()"/> 
                <xsl:if test="$id='xsl-elements'"> from src/elements/xsl-elements-expanded.xml, which is itself generated from src/userdoc/elements/xsl-elements.xml (the editable version)</xsl:if>
                <xsl:if test="$id='instructions'"> from src/userdoc/extensions/instructions.xml (the editable version)</xsl:if>    
                <xsl:if test="$id='serialization-parameters'"> from src/userdoc/extensions/serialization-params.xml (the editable version)</xsl:if> 
                <xsl:if test="$id='sql-extension'"> from src/userdoc/extensions/sql-extension.xml (the editable version)</xsl:if>
            </xsl:comment>
            <xsl:apply-templates select="*" mode="#current"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" mode="#current"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="el:*">
        <xsl:element name="{local-name()}">
            <xsl:apply-templates select="node()|@*" mode="#current"/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="el:xsl-element|el:saxon-element">
        <xsl:variable name="MyElement" select="."/>
        <xsl:variable name="elemName" select="el:name"/>
        <xsl:variable name="default.saxon"
            select="$implementation-doc/implement/default[@id = $id]/@saxon"/>
        <xsl:variable name="default.platform"
            select="$implementation-doc/implement/default[@id = $id]/@platform"/>
        <h1>
            <xsl:if test="self::el:xsl-element">
                <xsl:value-of select="concat('xsl:', $elemName)"/>
            </xsl:if>
            <xsl:if test="el:name/@namespace='http://saxon.sf.net/'">
                <xsl:value-of select="concat('saxon:', $elemName)"/>
            </xsl:if>
            <xsl:if test="el:name/@namespace='http://saxon.sf.net/sql'">
                <xsl:value-of select="concat('sql:', $elemName)"/>
            </xsl:if>
            <xsl:if test="el:name/@namespace='http://icl.com/saxon'">
                <xsl:value-of select="concat('saxon6:', $elemName)"/>
            </xsl:if>
        </h1>

        <xsl:apply-templates select="el:description" mode="#current"/>


        <xsl:if test="el:element-syntax/*[not(self::el:attribute)]">
            <xsl:apply-templates select="el:element-syntax" mode="model"/>
        </xsl:if>

        <xsl:if test="el:element-syntax/el:attribute">
            <h3 class="subtitle">Attributes</h3>
            <xsl:apply-templates select="el:element-syntax" mode="#current"/>
        </xsl:if>
        
        <h3 class="subtitle">Saxon availability</h3>
        <p>
            <xsl:value-of select="el:specs(el:in-spec)"/>
            <xsl:value-of select="el:edition((el:saxon-edition,$default.saxon,'default')[1])"/>
            <xsl:value-of select="el:platform((el:saxon-edition/@platform,$default.platform,'ALL')[1])"/>
        </p>

        <xsl:if test="el:status">
            <h3 class="subtitle">Notes on the Saxon implementation</h3>
            <xsl:apply-templates select="el:status" mode="#current"/>
        </xsl:if>
        <!-- <status-ok> elements are ignored -->

        <xsl:if test="el:details">
            <h3 class="subtitle">Details</h3>
            <xsl:apply-templates select="el:details" mode="#current"/>
        </xsl:if>

        <xsl:if test="el:examples">
            <h3 class="subtitle">Examples</h3>
            <xsl:apply-templates select="el:examples" mode="#current"/>
        </xsl:if>

        <xsl:if test="el:in-spec">
            <h3 class="subtitle">Links to W3C specifications</h3>
            <xsl:apply-templates select="el:in-spec" mode="#current"/>
        </xsl:if>

        <xsl:if test="el:see-also">
            <h3 class="subtitle">See also</h3>
            <xsl:apply-templates select="el:see-also" mode="#current"/>
        </xsl:if>

    </xsl:template>

    <xsl:template match="el:description">
        <xsl:apply-templates mode="#current"/>
    </xsl:template>

    <xsl:template match="el:element-syntax">
        <!-- Build table of attributes with descriptions -->
        <table class="element-att">
            <xsl:for-each select="el:attribute">
                <tr>
                    <td>
                        <p>
                            <code>
                                <xsl:apply-templates select="."/>
                            </code>
                        </p>
                    </td>
                    <td>
                        <p>
                            <code>
                                <xsl:apply-templates select="* except el:desc"/>
                            </code>
                        </p>
                    </td>
                    <td>
                        <p>
                            <xsl:apply-templates select="el:desc"/>
                        </p>
                    </td>
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>

    <xsl:template match="el:attribute">
        <xsl:choose>
            <xsl:when test="@deprecated='yes'">
                <xsl:text>[</xsl:text>
                <xsl:value-of select="@name"/>
                <xsl:text>]?</xsl:text>
            </xsl:when>
            <xsl:when test="@required='yes'">
                <strong>
                    <xsl:value-of select="@name"/>
                </strong>
            </xsl:when>
            <xsl:when test="parent::el:element-syntax[@name eq 'output-extras']">
                <xsl:value-of select="@name"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="@name"/>
                <xsl:text>?</xsl:text>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="el:data-type">
        <xsl:if test="position()>3"> | </xsl:if>
        <i>
            <xsl:value-of select="@name"/>
        </i>
    </xsl:template>

    <xsl:template match="el:constant">
        <xsl:if
            test="(position()>1 and not(parent::el:attribute-value-template)) or (position()>2 and parent::el:attribute-value-template)"
            > | </xsl:if>
        <xsl:text>"</xsl:text>
        <xsl:value-of select="@value"/>
        <xsl:text>"</xsl:text>
    </xsl:template>

    <xsl:template match="el:attribute-value-template">
        <xsl:text>{ </xsl:text>
        <xsl:apply-templates/>
        <xsl:text> }</xsl:text>
    </xsl:template>

    <xsl:template match="el:desc">
        <xsl:apply-templates mode="#current"/>
    </xsl:template>


    <xsl:template match="el:in-spec">
        <!-- Add relevant links to XSLT specifications, using in-spec elements -->
        <xsl:variable name="elemName" select="../el:name"/>
        <xsl:if test=". = 'xslt20'">
            <p>
                <a href="http://www.w3.org/TR/xslt20/#element-{$elemName}">XSLT 2.0
                    Specification</a>
            </p>
        </xsl:if>
        <xsl:if test=". = 'xslt30'">
            <p>
                <a href="http://www.w3.org/TR/xslt-30/#element-{$elemName}">XSLT 3.0
                    Specification</a>
            </p>
        </xsl:if>
        <xsl:if test=". = 'xslt40'">
            <p>
                <a href="https://qt4cg.org/branch/master/xslt-40/Overview.html#element-{$elemName}">XSLT 4.0
                    Specification</a>
            </p>
        </xsl:if>
    </xsl:template>

    <xsl:template match="el:status">
        <xsl:apply-templates mode="#current"/>
    </xsl:template>

    <xsl:template match="el:details">
        <xsl:apply-templates mode="#current"/>
    </xsl:template>

    <xsl:template match="el:examples">
        <!-- List of examples (enumerated if more than one).-->
        <xsl:choose>
            <xsl:when test="el:example">
                <xsl:choose>
                    <xsl:when test="count(el:example) = 1">
                        <xsl:for-each select="el:example">
                            <xsl:apply-templates mode="#current"/>
                        </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:iterate select="el:example">
                            <xsl:param name="i" select="1" as="xs:integer"/>
                            <xsl:variable name="new-i" select="$i + 1"/>
                            <h4>Example <xsl:value-of select="$i"/></h4>
                            <xsl:apply-templates mode="#current"/>
                            <xsl:next-iteration>
                                <xsl:with-param name="i" select="$new-i"/>
                            </xsl:next-iteration>
                        </xsl:iterate>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
                <xsl:apply-templates mode="#current"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="el:see-also">
        <!-- Give links to related xsl-elements, literal result element, functions, extensions -->
        <xsl:if test="@type = 'xsl-elements'">
            <xsl:choose>
                <xsl:when test="@name = 'lre'">
                    <p>
                        <a class="bodylink" href="/{@type}/{@name}">literal result elements
                        </a>
                    </p>
                </xsl:when>
                <xsl:otherwise>
                    <p>
                        <a class="bodylink" href="/{@type}/{@name}">
                            <xsl:value-of select="concat('xsl:', @name)"/>
                        </a>
                    </p>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
        <xsl:if test="@type = 'functions'">
            <xsl:choose>
                <xsl:when test="@namespace">
                    <p>
                        <a class="bodylink" href="/{@type}/{@namespace}/{@name}"><xsl:value-of
                                select="concat(el:namespace-prefix(@namespace),':', @name)"/>()</a>
                    </p>
                </xsl:when>
                <xsl:otherwise>
                    <p>
                        <a class="bodylink" href="/{@type}/fn/{@name}"><xsl:value-of
                                select="@name"/>()</a>
                    </p>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
        <xsl:if test="@type = 'extensions'">
            <p>
                <a class="bodylink" href="/{@type}/{@subtype}/{@name}">
                    <xsl:value-of select="concat(@namespace,':', @name)"/>
                </a>
            </p>
        </xsl:if>
        <xsl:if test="@type = 'sql-extension'">
            <p>
                <a class="bodylink" href="/{@type}/{@subtype}/{@name}">
                    <xsl:value-of select="concat('sql:', @name)"/>
                </a>
            </p>
        </xsl:if>
    </xsl:template>

    <!--    Model section-->

    <xsl:template match="el:element-syntax" mode="model">
        <p class="element-syntax">
            <xsl:apply-templates select="el:in-category"/>
            <xsl:apply-templates select="el:sequence|el:choice|el:model|el:content|el:text|el:empty"
                mode="#current"/>
            <xsl:apply-templates select="el:allowed-parents"/>
            <xsl:if test="not(el:attribute)">
                <xsl:call-template name="no-attributes"/>
            </xsl:if>
        </p>
    </xsl:template>

    <xsl:template match="el:in-category">
        <strong>Category: </strong>
        <xsl:value-of select="@name"/>
        <br/>
    </xsl:template>

    <xsl:template name="no-attributes">
        <br/>
        <strong>
            <i>Element has no attributes</i>
        </strong>
    </xsl:template>

    <xsl:template match="el:sequence|el:choice|el:model|el:content|el:text|el:empty" mode="model">
        <strong>Content: </strong>
        <xsl:apply-templates select="."/>
    </xsl:template>

    <xsl:template match="el:sequence|el:choice">
        <xsl:call-template name="group"/>
        <xsl:text>(</xsl:text>
        <xsl:apply-templates/>
        <xsl:text>)</xsl:text>
        <xsl:call-template name="repeat"/>
    </xsl:template>

    <xsl:template match="el:model">
        <xsl:call-template name="group"/>
        <i>
            <xsl:value-of select="@name"/>
        </i>
        <xsl:call-template name="repeat"/>
    </xsl:template>

    <xsl:template match="el:text">#PCDATA</xsl:template>

    <xsl:template match="el:empty">none</xsl:template>

    <xsl:template match="el:content">
        <xsl:call-template name="group"/>
        <xsl:choose>
            <xsl:when test="contains(@name, ':')">
                <xsl:value-of select="@name"/>
            </xsl:when>
            <xsl:otherwise>
                <a class="bodylink code" href="/xsl-elements/{@name}">
                    <xsl:value-of select="concat('xsl:', @name)"/>
                </a>
            </xsl:otherwise>
        </xsl:choose>
        <xsl:call-template name="repeat"/>
    </xsl:template>

    <xsl:template name="group">
        <xsl:if test="position()>2">
            <xsl:choose>
                <xsl:when test="parent::el:sequence">, </xsl:when>
                <xsl:when test="parent::el:choice"> | </xsl:when>
                <xsl:when test="parent::el:allowed-parents">; </xsl:when>
            </xsl:choose>
        </xsl:if>
    </xsl:template>

    <xsl:template name="repeat">
        <xsl:choose>
            <xsl:when test="@repeat='one-or-more'">
                <xsl:text>+</xsl:text>
            </xsl:when>
            <xsl:when test="@repeat='zero-or-more'">
                <xsl:text>*</xsl:text>
            </xsl:when>
            <xsl:when test="@repeat='zero-or-one'">
                <xsl:text>?</xsl:text>
            </xsl:when>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="el:allowed-parents">
        <br/>
        <strong>Permitted parent elements: </strong>
        <xsl:apply-templates/>
        <xsl:if test="not(*)">None</xsl:if>
    </xsl:template>

    <xsl:template match="el:parent">
        <xsl:call-template name="group"/>
        <a href="/xsl-elements/{@name}" class="bodylink code">
            <xsl:value-of select="concat('xsl:', @name)"/>
        </a>
    </xsl:template>

    <xsl:template match="el:parent-category[@name='sequence-constructor']">
        <xsl:if test="position()>2">; </xsl:if> any XSLT element whose content model is 
        <i>sequence-constructor</i>; any literal result element </xsl:template>
    

    <!-- Functions to determince the text within the 'Saxon availability' section.
        Spec versions in which the elements are defined. -->
    <xsl:function name="el:specs" as="xs:string*">
        <xsl:param name="in" as="element(el:in-spec)*"/>
        <!-- in-spec elements should be in order such that first one $in[1] is earliest spec, to ensure the correct message. -->
        <xsl:value-of select="$implementation-doc/implement/spec[@value = $in[1]]"/>
    </xsl:function>

    <!-- Saxon editons & versions. -->
    <xsl:function name="el:edition" as="xs:string*">
        <xsl:param name="available" as="xs:string"/>
        <xsl:variable name="available"
            select="for $t in tokenize($available, '\s+') return upper-case($t)" as="xs:string*"/>
        <xsl:for-each select="$available">
            <xsl:value-of select="$implementation-doc/implement/saxon[@version = current()]"/>
        </xsl:for-each>
    </xsl:function>
    
    <!-- Availability for different platforms. -->
    <xsl:function name="el:platform" as="xs:string*">
        <xsl:param name="available" as="xs:string"/>
        <xsl:variable name="availableSeq"
            select="for $t in tokenize($available, '\s+') return upper-case($t)" as="xs:string*"/>
        <xsl:variable name="platforms" as="xs:string">
            <xsl:choose>
                <xsl:when test="$availableSeq = 'J' and $availableSeq = 'C'">
                    <xsl:value-of select="'J_C'"/>
                </xsl:when>
                <xsl:when test="$availableSeq = 'J' and $availableSeq = 'CS'">
                    <xsl:value-of select="'J_CS'"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$available"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        
        <xsl:for-each select="$platforms">
            <xsl:value-of select="$implementation-doc/implement/saxon[@platform = current()]"/>
        </xsl:for-each>
    </xsl:function>
    
    <!-- Get namespace conventional prefix for functions. -->
    <xsl:function name="el:namespace-prefix" as="xs:string">
        <xsl:param name="ns" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$ns = 'fn'">fn</xsl:when>
            <xsl:when test="$ns = 'saxon'">saxon</xsl:when>
            <xsl:when test="$ns = 'map'">map</xsl:when>
            <xsl:when test="$ns = 'math'">math</xsl:when>
            <xsl:when test="$ns = 'expath-archive'">arch</xsl:when>
            <xsl:when test="$ns = 'expath-binary'">bin</xsl:when>
            <xsl:when test="$ns = 'expath-file'">file</xsl:when>
            <xsl:when test="$ns = 'expath-zip'">zip</xsl:when>
            <xsl:when test="$ns = 'exslt-common'">exsl</xsl:when>
            <xsl:when test="$ns = 'exslt-random'">random</xsl:when>
            <xsl:when test="$ns = 'exslt-math'">exslt-math</xsl:when>
            <xsl:when test="$ns = 'exslt-sets'">set</xsl:when>
            <xsl:when test="$ns = 'exslt-dates-and-times'">date</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$ns"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>


</xsl:stylesheet>
