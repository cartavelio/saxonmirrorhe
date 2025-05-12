<?xml version="1.0" encoding="utf-8"?>
<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fc="http://www.saxonica.com/ns/doc/functions"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map" version="3.0"
    exclude-result-prefixes="xs fc map">
    
    <!-- As well as being used alongside 'functions-data.xsl' to generate the dynamic version of
        the Function Library, this stylesheet is also imported by 'to-static/c-functions.xsl' 
        to generate the static (HTML) version.
        So templates that can be used in both cases are found here, while the shared stylesheet
        functions (xsl:function) are defined in the imported stylesheet 'functions-functions.xsl'. -->
    
    <xsl:import href="functions-functions.xsl"/>
    
    <xsl:function name="fc:path-prefix" as="xs:string">
        <xsl:param name="node" as="element()"/>
        <xsl:sequence
            select="if ($node/(/table)) then '../' (: generated feature keys :)
                    else if ($node/ancestor::subpage) then '../../'
                    else if ($node/ancestor::page) then '../'
                    else ''"
        />
    </xsl:function>
    
    <!-- Further info for function namespace page -->
    <xsl:function name="fc:namespace-info">
        <xsl:param name="ns" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions'">It includes both the core
                functions defined in XPath, and the additional functions defined in the XSLT
                specification.</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/'">For general information about using these functions
                see <span class="link" data-href="/extensions/functions/saxon-extension-functions">Saxon
                    extension functions</span>.</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/map'">It includes the core
                functions for handling maps defined in XPath 3.1.</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/array'">It includes the core
                functions for handling arrays defined in XPath 3.1.</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/math'">It includes the core
                mathematical and trigonometric functions defined in XPath 3.0.</xsl:when>
            <xsl:when test="starts-with($ns, 'http://expath.org/')">For general information about using these
                functions see <span class="link" data-href="/extensions/functions/expath">EXPath
                    extensions</span>.</xsl:when>
            <xsl:when test="starts-with($ns, 'http://exslt.org/')">For general information about using these
                functions see <span class="link" data-href="/extensions/functions/exslt">EXSLT
                    extensions</span>.</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/sql'">For general information about using these
                functions see <span class="link" data-href="/sql-extension">Saxon SQL
                    extension</span>.</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="''"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <xsl:template match="node()|@*" mode="f">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" mode="f"/>
        </xsl:copy>
    </xsl:template>
    
    <!-- Signature formats. -->
    <xsl:template match="signatures" mode="f"
        xpath-default-namespace="http://www.saxonica.com/ns/doc/functions">
        <xsl:param name="output-spec-references" as="xs:boolean"/>
        
        <xsl:for-each select="proto">
            <xsl:variable name="MyProto" select="."/>
            
            <p class="fn-sig">
                <xsl:value-of
                    select="concat(../../name,'(',string-join(for $arg in arg return concat('$',$arg/@name, ' as ', $arg/@type), ', ') ,')', ' &#x2794;  ',@return-type)"
                />
            </p>
            <xsl:apply-templates select="description" mode="f"/>
            <xsl:if test="$output-spec-references">
                <aside>
                    <xsl:value-of select="fc:specs(in-spec)"/>
                </aside>
            </xsl:if>
            <table class="fn-prototype" style="margin-bottom:10px;">
                <tr>
                    <td width="470" align="left" colspan="4" style="border-top:solid 1px;">
                        <p>
                            <i>
                                <xsl:value-of select="if (exists(arg)) then 'Arguments' else 'There are no arguments'"/>
                            </i>
                        </p>
                    </td>
                </tr>
                <xsl:for-each select="arg">
                    <tr>
                        <td width="40">
                            <p>&#xa0;</p>
                        </td>
                        <td width="80" valign="top">
                            <p>$<xsl:value-of select="@name"/></p>
                        </td>
                        <td valign="top" width="150">
                            <p>
                                <xsl:value-of select="@type"/>
                            </p>
                        </td>
                        <td valign="top" width="200">
                            <p>
                                <xsl:value-of select="@desc"/>
                            </p>
                        </td>
                    </tr>
                </xsl:for-each>
                <tr>
                    <td colspan="2" style="border-top:solid 1px; border-bottom:solid 1px;">
                        <p>
                            <i>Result</i>
                        </p>
                    </td>
                    
                    <td style="border-top:solid 1px #3D5B96; border-bottom:solid 1px;" colspan="2">
                        <p>
                            <xsl:value-of select="$MyProto/@return-type"/>
                        </p>
                    </td>
                    
                </tr>
            </table>
            
        </xsl:for-each>
    </xsl:template>

    <xsl:template match="fc:*" mode="f">
        <xsl:element name="{local-name()}">
            <xsl:apply-templates select="node()|@*" mode="f"/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="fc:summary" mode="f">
        <p>
            <xsl:apply-templates mode="f"/>
        </p>
    </xsl:template>

    <xsl:template match="a|fc:a" mode="f">
        <xsl:choose>
            <xsl:when test="substring(@href, 1, 5) = ('file:','http:') or starts-with(@href, 'https:')">
                <a class="link" href="{@href}">
                    <xsl:value-of select="."/>
                </a>
            </xsl:when>
            <xsl:otherwise>
                <span class="link {@class}" data-href="{@href}">
                    <xsl:apply-templates select="node()" mode="secondary"/>
                </span>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="a[@class =('javalink','cslink')]|fc:a[@class =('javalink','cslink')]" mode="f">
        <span data-href="{@href}">
            <xsl:copy-of select="@* except @href"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>

    <xsl:template match="code|fc:code" mode="f">
        <code>
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates/>
        </code>
    </xsl:template>

    <xsl:template match="example|fc:example" priority="2" mode="f">
        <samp>
            <xsl:apply-templates/>
        </samp>
    </xsl:template>
    
    <!-- 'Notes on the Saxon implementation' section. -->
    <xsl:template match="fc:status" mode="f">
        <h3 class="subtitle">Notes on the Saxon implementation</h3>
        <xsl:apply-templates mode="f"/>
    </xsl:template>
    
    <xsl:template match="fc:details" mode="f">
        <h3 class="subtitle">Details</h3>
        <xsl:apply-templates mode="f"/>
    </xsl:template>
    
    <xsl:template match="fc:notes" mode="f">
        <h3 class="subtitle">Notes</h3>
        <xsl:apply-templates mode="f"/>
    </xsl:template>
    
    <xsl:template match="fc:description" mode="f">
        <xsl:apply-templates mode="f"/>
    </xsl:template>

</xsl:transform>
