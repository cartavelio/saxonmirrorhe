<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:el="http://www.saxonica.com/ns/doc/elements"
    xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
    xmlns:doc="http://www.saxonica.com/ns/doc" exclude-result-prefixes="xs el e xsi map doc"
    version="3.0">
    
    <!-- 
        2017-01-20 DL Added checks (and output messages) to compare lists of attributes defined in
        xsl-elements-src.xml and element-catalog.xml.
    -->

    <xsl:output method="xml" indent="yes"/>

    <xsl:variable name="catalog" select="doc(resolve-uri('element-catalog.xml', base-uri(/)))"
        doc:doc="The W3C Spec catalog that contains the syntax information for the elements in this section"/>
    <xsl:variable name="namespace" select="'http://www.w3.org/1999/XSL/Transform'"
        doc:doc="The namespace for these elements"/>

    <xsl:template match="article">
        <xsl:copy>
            <xsl:sequence select="@id|@title"/>
            <xsl:comment>Generated - <xsl:value-of select="current-dateTime()"/> from src/elements/xsl-elements-src.xml (the editable version)</xsl:comment>
            <xsl:apply-templates select="*" mode="#current"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="@*|*|text()">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" mode="#current"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="section">
        <xsl:copy>
            <xsl:sequence select="@id|@title"/>
            <!--<xsl:apply-templates select="el:xsl-element" mode="#current">
                <xsl:sort select="/name"/>
            </xsl:apply-templates>-->
            <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="el:xsl-element"
        xpath-default-namespace="http://www.saxonica.com/ns/doc/elements">
        <xsl-element xmlns="http://www.saxonica.com/ns/doc/elements">
            <xsl:variable name="atts"
                select="map:merge((attributes/attribute ! map:entry(xs:string(./@name),./(*|text()))))" as="map(xs:string,node()*)"/>
            <!--<xsl:message>xsl:<xsl:value-of select="name"/></xsl:message>-->
            <!--<xsl:message>attributes: <xsl:value-of select="map:keys($atts)"/></xsl:message>-->
            <name namespace="{$namespace}">
                <xsl:value-of select="name"/>
            </name>
            <xsl:variable name="element-in-catalog" select="$catalog//e:element-syntax[@name=current()/name]"/>
            
            <xsl:choose>
                <xsl:when test="empty($element-in-catalog)">
                    <xsl:message>xsl:<xsl:value-of select="name"/> element not found in catalog</xsl:message>
                    <!-- Special case for xsl:stream now renamed xsl:source-document -->
                    <xsl:if test="string(name) eq 'stream'">
                        <xsl:call-template name="element-syntax">
                            <xsl:with-param name="el-name" as="xs:string" select="'source-document'"/>
                            <xsl:with-param name="atts" as="map(xs:string,node()*)" select="$atts"/>
                            <xsl:with-param name="element-in-catalog" as="element()?"
                                select="$catalog//e:element-syntax[@name='source-document']"/>
                            <xsl:with-param name="current-element" as="element()" select="."/>
                        </xsl:call-template>
                    </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:call-template name="element-syntax">
                        <xsl:with-param name="el-name" as="xs:string" select="string(name)"/>
                        <xsl:with-param name="atts" as="map(xs:string,node()*)" select="$atts"/>
                        <xsl:with-param name="element-in-catalog" as="element()" select="$element-in-catalog"/>
                        <xsl:with-param name="current-element" as="element()" select="."/>
                    </xsl:call-template>
                </xsl:otherwise>
            </xsl:choose>
            
            <xsl:apply-templates select="*" mode="#current"/>
        </xsl-element>
    </xsl:template>

    <xsl:template match="el:name|el:attributes"/>
    
    <xsl:template name="element-syntax">
        <xsl:param name="el-name" as="xs:string"/>
        <xsl:param name="atts" as="map(xs:string,node()*)"/>
        <xsl:param name="element-in-catalog" as="element()"/>
        <xsl:param name="current-element" as="element()"/>
        <element-syntax xmlns="http://www.saxonica.com/ns/doc/elements" name="{$el-name}">
            <xsl:for-each select="$element-in-catalog/e:attribute">
                <!--<xsl:message>@<xsl:value-of select="@name"/></xsl:message>-->
                <attribute xmlns="http://www.saxonica.com/ns/doc/elements">
                    <xsl:apply-templates select="@*" mode="#current"/>
                    <desc>
                        <xsl:sequence select="$atts(@name)"/>
                        <xsl:if test="empty($atts(@name)) and not($el-name eq 'transform')">
                            <xsl:message>**** <xsl:value-of select="concat('xsl:',$el-name,'/@',@name)"/> requires description</xsl:message>
                        </xsl:if>
                    </desc>
                    <xsl:apply-templates select="*" mode="#current"/>
                </attribute>
            </xsl:for-each>
            <xsl:for-each select="map:keys($atts)">
                <xsl:sort/>
                <xsl:if test="empty($element-in-catalog/e:attribute[@name = current()])">
                    <xsl:choose>
                        <xsl:when test="starts-with(current(), 'saxon:')">
                            <xsl:message>Extension attribute <xsl:value-of select="concat('xsl:',$el-name,'/@',current())"/></xsl:message>
                            <xsl:variable name="data-type"
                                select="string($current-element/el:attributes/el:attribute[@name=current()]/@data-type)"/>
                            <attribute xmlns="http://www.saxonica.com/ns/doc/elements" name="{current()}">
                                <desc>
                                    <xsl:sequence select="$atts(current())"/>
                                    <xsl:if test="empty($atts(current()))">
                                        <xsl:message>**** <xsl:value-of select="concat('xsl:',$el-name,'/@',current())"/> requires description</xsl:message>
                                    </xsl:if>
                                </desc>
                                <xsl:choose>
                                    <xsl:when test="matches($data-type, '\s+')">
                                        <xsl:for-each select="tokenize($data-type, '\s+')">
                                            <constant value="{.}"/>
                                        </xsl:for-each>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <data-type name="{$current-element/el:attributes/el:attribute[@name=current()]/@data-type}"/>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </attribute>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:message>** <xsl:value-of select="concat('xsl:',$el-name,'/@',current())"/> not in element-catalog</xsl:message>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:if>
            </xsl:for-each>
            <xsl:apply-templates select="$element-in-catalog/(* except e:attribute)" mode="#current"/>
        </element-syntax>
    </xsl:template>

    <xsl:template match="e:*">
        <xsl:element name="{local-name(.)}" namespace="http://www.saxonica.com/ns/doc/elements">
            <xsl:apply-templates select="@*|*|text()" mode="#current"/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="e:element">
        <content xmlns="http://www.saxonica.com/ns/doc/elements">
            <xsl:apply-templates select="@*|*|text()" mode="#current"/>
        </content>
    </xsl:template>


</xsl:stylesheet>
