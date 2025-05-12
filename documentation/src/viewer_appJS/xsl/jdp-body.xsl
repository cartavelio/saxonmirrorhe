<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" version="3.0"
    xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT" 
    xmlns:f="urn:viewerapp.function"
    extension-element-prefixes="ixsl">
    
    <xsl:mode name="jdp" on-no-match="shallow-copy"/>

    <xsl:template match="a" mode="jdp">
        <xsl:copy>
            <xsl:attribute name="target" select="'_blank'"/>
            <xsl:apply-templates mode="jdp" select="node()|@* except @target"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="a[@class = 'javalink']" mode="jdp">
        <span class="javalink" data-href="{@href}">
            <xsl:value-of select="f:last-substring-after(. , 46)"/>
        </span>
    </xsl:template>
    
    <xsl:template match="a[@class = 'cslink']" mode="jdp">
        <span class="cslink" data-href="{@href}">
            <xsl:value-of select="f:last-substring-after(. , 46)"/>
        </span>
    </xsl:template>
    
    <xsl:template match="section" mode="jdp">
        <xsl:apply-templates mode="jdp"/>
    </xsl:template>

    <xsl:template match="section" mode="pkg-header">
        <h1>
            <xsl:value-of select="@id"/>
        </h1>
        <xsl:apply-templates mode="jdp" select="child::div/p[1]"/>
        <xsl:if test="count(child::div/p) gt 1">
            <p>
                <em>(More details at foot of page)</em>
            </p>
        </xsl:if>
    </xsl:template>

    <xsl:variable name="s-titles" select="('Interface', 'Delegate', 'Class', 'Enum', 'Exception')" as="xs:string+"/>

    <xsl:template match="document-node()" mode="summarise-pkg">
        <xsl:variable name="package" select="package"/>
        <xsl:variable name="classes" select="$package/class"/>
        <xsl:for-each select="1 to 5">
            <xsl:variable name="i" select="." as="xs:integer"/>
            <xsl:variable name="f-classes"
                select="if ($i eq 1) then ($package/interface | $classes[@interface='true'])
                else if ($i eq 2) then ($package/delegate)
                        else if ($i eq 3) then
                            $classes[not(
                                @interface='true'
                                or @superclass='Enum'
                                or ends-with(@superclass,'Exception'))]
                        else if ($i eq 4) then ($package/enum | $classes[@superclass='Enum'])
                        else $classes[ends-with(@superclass,'Exception')]"/>
            <xsl:if test="exists($f-classes)">
                <table border="1" style="width:100%">
                    <thead>
                        <tr>
                            <td colspan="2" style="text-align:center">
                                <h3>
                                    <xsl:value-of select="concat($s-titles[$i],' Summary')"/>
                                </h3>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <p>Name</p>
                            </td>
                            <td>
                                <p>Description</p>
                            </td>
                        </tr>
                    </thead>
                    <tbody>
                        <xsl:for-each select="$f-classes">
                            <tr>
                                <td>
                                    <xsl:sequence select="f:showType(., true(), true())"/>
                                </td>
                                <td>
                                    <p>
                                        <xsl:apply-templates select="(comment/attribute[@name='@deprecated'],
                                            comment/sentence)[1]" mode="jdc"/>
                                    </p>
                                </td>
                            </tr>
                        </xsl:for-each>
                    </tbody>
                </table>

                <p>&#160;</p>
            </xsl:if>
        </xsl:for-each>
    </xsl:template>
    
    <xsl:template match="article[@id='javadoc-packages']" mode="jdp">
        <h1>Saxon Java API Documentation</h1>
        <xsl:variable name="art" select="."/>
        <xsl:variable name="titles"
            select="('s9api Interfaces','Other Interfaces','External Interfaces','Other Packages')"
            as="xs:string+"/>

        <xsl:variable name="p1" select="'net.sf.saxon.s9api', 'net.sf.saxon.s9api.push',
            'net.sf.saxon.s9api.streams'"/>
        <xsl:variable name="p2"
            select="'com.saxonica.config','com.saxonica.jaxp','com.saxonica.schema',
'net.sf.saxon','net.sf.saxon.lib', 'net.sf.saxon.om',
'net.sf.saxon.query','net.sf.saxon.sxpath','net.sf.saxon.type',
'net.sf.saxon.type','net.sf.saxon.value','net.sf.saxon.xpath',
'net.sf.saxon.xqj'" />
        <xsl:variable name="p3" select="'javax.xml.xquery'"/>
        <xsl:variable name="p4" select="$art/section[not(@id = ($p1, $p2, $p3))]/@id"/>


        <xsl:for-each select="1 to 4">
            <xsl:variable name="i" select="." as="xs:integer"/>
            <table border="1" style="width:100%">
                <thead>
                    <tr>
                        <td colspan="2" style="text-align:center">
                            <h3>
                                <xsl:value-of select="$titles[$i]"/>
                            </h3>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>Package</p>
                        </td>
                        <td>
                            <p>Description</p>
                        </td>
                    </tr>
                </thead>
                <tbody>
                    <xsl:for-each
                        select="$art/section[@id = 
                        (if ($i eq 1) then $p1
                        else if ($i eq 2) then $p2
                        else if ($i eq 3) then $p3
                        else $p4)]">
                        <xsl:sort select="@id"/>
                        <tr>
                            <td>
                                <span class="javalink" data-href="{@id}">
                                    <xsl:value-of select="@id"/>
                                </span>
                            </td>

                            <td>
                                <xsl:apply-templates select="child::div/p[1]" mode="jdp"/>
                            </td>
                        </tr>
                    </xsl:for-each>
                </tbody>
            </table>

            <p>&#160;</p>
        </xsl:for-each>

    </xsl:template>
    
    <xsl:template match="article[@id='dotnetdoc-packages']" mode="jdp">
        <h1>Saxon .NET API Documentation</h1>
 
            <table border="1" style="width:100%">
                <thead>
                    <tr>
                        <td colspan="2" style="text-align:center">
                            <h3>Saxon.Api</h3>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>Package</p>
                        </td>
                        <td>
                            <p>Description</p>
                        </td>
                    </tr>
                </thead>
                <tbody>
                    <xsl:for-each select="section">
                        <tr>
                            <td>
                                <span class="cslink" data-href="{@id}">
                                    <xsl:value-of select="@id"/>
                                </span>
                            </td>
                            
                            <td>
                                <xsl:apply-templates select="child::div/p" mode="jdp"/>
                            </td>
                        </tr>
                    </xsl:for-each>
                </tbody>
            </table>
            
            <p>&#160;</p>
        
    </xsl:template>

</xsl:transform>
