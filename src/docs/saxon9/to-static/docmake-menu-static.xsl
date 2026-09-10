<!-- Stylesheet to add the navigation menu for contents and documentation main page -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:variable name="img" select="'img'"/>

    <xsl:variable name="this-page" select="(//this-is)[1]"/>
    
    <xsl:variable name="path-prefix" select="'../documentation/'"/>

    <xsl:template match="*">
        <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>

    <xsl:variable name="is-IE" select="system-property('xsl:vendor')='Microsoft'"/>

    <xsl:template match="head">
        <head>
            <xsl:copy-of select="node()"/>
            <xsl:if test="not($is-IE)">
                <script language="javascript">
                    function resize_background(){
                    var docHeight = Math.max(body.scrollHeight, body.clientHeight, screen.zero(self.innerHeight)); 
                    document.getElementById('lhLightArea').style.height=docHeight; 
                    document.getElementById('lhDkBlueArea').style.height=docHeight; 
                    document.getElementById('rhDkBlueArea').style.height=docHeight; 
                    }
                </script>
            </xsl:if>
        </head>
    </xsl:template>

    <xsl:template match="this-is"/>

    <xsl:template match="body">
        <body>
            <xsl:if test="not($is-IE)">
                <xsl:attribute name="onload">resize_background()</xsl:attribute>
            </xsl:if>
            <xsl:variable name="fixed-or-absolute">
                <xsl:choose>
                    <xsl:when test="$is-IE">absolute</xsl:when>
                    <xsl:otherwise>fixed</xsl:otherwise>
                </xsl:choose>
            </xsl:variable>
            <xsl:copy-of select="@*"/>

            <div id="rhDkBlueArea"
                style="position:{$fixed-or-absolute}; width:123px; height:2000px; z-index:1; right: 0px; top: 0px;  border: 1px none #000000; background-color: #C1CEDE; layer-background-color: #C1CEDE; visibility: visible;"/>

            <div id="lhLightArea"
                style="position:{$fixed-or-absolute}; width:34px; height:2000px; z-index:1; left: 66px; top: 0px; border: 1px none #000000; background-color: #f6fffb; layer-background-color: #f6fffb; visibility: visible;"/>
            <div id="lhDkBlueArea"
                style="position:{$fixed-or-absolute}; width:66px; height:2000px; z-index:1; left: 0px; top: 0px; border: 1px none #000000; background-color: #C1CEDE; layer-background-color: #C1CEDE; visibility: visible;"/>
            <div id="LogoArea"
                style="position:absolute; width:340px; height:72px; z-index:3; right: 0px; top: 0px; border: 1px none #000000; visibility: visible;">
                <a href="http://www.saxonica.com/">

                    <img src="../img/saxonica_logo.gif" width="340" height="72" border="0"
                        alt="saxonica.com"/>
                </a>
            </div>
            <div id="MenuArea"
                style="position:absolute; width:157px; z-index:4; right: 33px; top: 114px; visibility: visible; background-color: #f6fffb; layer-background-color: #f6fffb; border: 1px solid #3D5B96; padding-top:9px; padding-left:15px;padding-right:11px; padding-bottom:9px;">
                <xsl:call-template name="MenuArea"/>
            </div>
            <div id="MainTextArea"
                style="position:absolute; height:100%; z-index:5; left: 130px; right: 260px; top: 110px; border: 1px none #000000; background-color: #E4EEF0; visibility: visible;">
                <xsl:apply-templates/>
            </div>
        </body>
    </xsl:template>


    <xsl:template name="MenuArea" match="div[@id='MenuArea']">

        <xsl:copy-of select="@*"/>

        <table border="0" cellspacing="0" cellpadding="1">
            <tr>
                <td class="title" colspan="2">
                    <p style="margin-top:0px; margin-bottom:0px; line-height=1.0em;"><a
                        href="{$path-prefix}documentation.html"><b>DOCUMENTATION</b></a><br/>
                        <!--<img src="{$path-prefix}{$img}/spacer.gif" width="120" height="10" border="0" alt=""/>-->&#xa0;</p>
                </td>
            </tr>
            <tr>
                <td colspan="2" style="border-bottom:solid 1px #3D5B96">
                    <p style="margin-top:0px; margin-bottom:0px; line-height=1.0em;">
                        <a href="{$path-prefix}contents-page.html">Full Contents</a>
                    </p>
                </td>
            </tr>
            <xsl:for-each select="document('../viewer_appJS/doc/contents.xml')/*/section">
                <tr>
                    <td colspan="2" style="border-bottom:solid 1px #3D5B96">
                        <p style="margin-top:0px; margin-bottom:0px; line-height=1.2em;">
                            <a href="{$path-prefix}{@name}/index.html">
                                <xsl:value-of select="@title"/>
                            </a>

                        </p>
                    </td>
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>

</xsl:stylesheet>
