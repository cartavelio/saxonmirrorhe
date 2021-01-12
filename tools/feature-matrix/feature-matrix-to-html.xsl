<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="http://local-functions/"
    exclude-result-prefixes="xs f" version="2.0">

    <xsl:output method="xhtml" indent="yes"/>
    
    <xsl:param name="major-version"/>

    <xsl:template match="featureMatrix">
        <xsl:processing-instruction name="xml-stylesheet">href="../make-menu.xsl" type="text/xsl"</xsl:processing-instruction>
        <xsl:variable name="version" select="@version"/>
        <xsl:variable name="documentation-version">
            <xsl:choose>
                <xsl:when test="@version = $major-version">                    
                    <xsl:value-of select="'documentation'"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="concat('documentation',@version)"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="page" select="concat('feature-matrix-',replace($version,'\.','-'))"/>
        <html>
            <head>
                <xsl:choose>
                    <xsl:when test="@version = $major-version">
                        <this-is section="products" page="{$page}" subpage=""/>                        
                    </xsl:when>
                    <xsl:otherwise>
                        <this-is section="products" page="{concat('feature-matrix-',replace($major-version,'\.','-'))}" subpage="{$page}"/>
                    </xsl:otherwise>
                </xsl:choose>
                <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
                <title>Saxonica: Saxon Product/Feature Matrix</title>
            </head>
            <body class="main">
                <div id="fullpage">
                    <h1>Saxon <xsl:value-of select="@version"/> Product comparison (Feature Matrix), Server
                        processing</h1>
                    <xsl:if test="@version ge '9.6'">
                        <p>
                            <i>For features in version 9.5 products, see the <a class="bodylink"
                                    href="feature-matrix-9-5.xml">Saxon 9.5 Feature Matrix</a>.</i>
                        </p>
                    </xsl:if>
                    <xsl:variable name="terms">
                        <xsl:choose>
                            <xsl:when test="./@version eq '9.5'">Basic, Adv (Advanced), SA (Schema
                                Aware)</xsl:when>
                            <xsl:otherwise>Basic, Adv (Advanced), Strm (Streaming), HOF
                                (Higher-Order Functions), SA (Schema Aware)</xsl:otherwise>
                        </xsl:choose>
                    </xsl:variable>
                    <p>The table below lists product features and indicates which features are
                        present in which product package. This may simply be indicated with a tick
                        or check-mark, or alternatively abbreviations are used to describe the level
                        of support for the feature. The terms used are: <xsl:value-of
                            select="$terms"/>.</p>
                    <p>The product packages referred to are:</p>
                    <ul>
                        <li><b>HE: </b>Saxon Home Edition (open source)</li>
                        <li><b>PE: </b>Saxon Professional Edition</li>
                        <li><b>EE: </b>Saxon Enterprise Edition</li>
                        <li><b>EE-T: </b>Saxon Enterprise Edition, Transformation package 
                            (restricted license allowing XSLT
                            and Schema processing only)</li>
                        <li><b>EE-Q: </b>Saxon Enterprise Edition, Query package
                            (restricted license allowing
                            XQuery and Schema processing only)</li>
                        <li><b>EE-V: </b>Saxon Enterprise Edition, Validation package
                            (restricted license allowing
                            Schema processing only)</li>
                    </ul>
                    <table class="feature-matrix">
                        <thead>
                            <tr>
                                <th>HE</th>
                                <th>PE</th>
                                <th>EE</th>
                                <th>EE-T</th>
                                <th>EE-Q</th>
                                <th>EE-V</th>
                                <th>Feature &amp; Description; link to documentation for more details</th>
                            </tr>
                        </thead>
                        <tbody>
                            <xsl:for-each select="feature">
                                <xsl:choose>
                                    <xsl:when test="./@class = 'heading'">
                                        <tr class="feature-heading">
                                            <td colspan="7" style="text-align:left;">
                                                <p>
                                                  <b>
                                                  <xsl:value-of select="shortName"/>
                                                  </b>
                                                </p>
                                            </td>
                                        </tr>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <tr>
                                            <td>
                                                <p>
                                                  <xsl:sequence
                                                      select="f:tick(products/product[tokenize(@editions, '\s') = 'HE'])"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                  <xsl:sequence
                                                      select="f:tick(products/product[tokenize(@editions, '\s') = 'PE'])"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                  <xsl:sequence
                                                      select="f:tick(products/product[tokenize(@editions, '\s') = 'EE'])"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                  <xsl:sequence
                                                      select="f:tick(products/product[tokenize(@editions, '\s') = 'EE-T'])"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                  <xsl:sequence
                                                      select="f:tick(products/product[tokenize(@editions, '\s') = 'EE-Q'])"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                  <xsl:sequence
                                                      select="f:tick(products/product[tokenize(@editions, '\s') = 'EE-V'])"/>
                                                </p>
                                            </td>
                                            <td class="feature-description">
                                                <table class="feature-title-details-link">
                                                  <tr>
                                                  <td style="text-align:left">
                                                  <b>
                                                  <xsl:value-of select="shortName"/>
                                                  <xsl:if test="ends-with(shortName, '3.0')"
                                                  >*</xsl:if>
                                                  </b>
                                                      <xsl:if test="spec">
                                                          <i><xsl:value-of select="concat(' (', spec/@level, ', ', spec/@date, ')')"/></i>
                                                      </xsl:if>                                                  
                                                  </td>
                                                  <xsl:if test="details">
                                                  <td style="text-align:right">
                                                  <p><xsl:for-each
                                                  select="details">
                                                  <a
                                                  href="{concat('../', $documentation-version, '/#!', @link)}"
                                                  class="bodylink {@subpages}">
                                                  <xsl:value-of select="@name"/>
                                                  </a>
                                                  <xsl:if test="not(position()=last())">
                                                  <xsl:text>, </xsl:text>
                                                  </xsl:if>
                                                  </xsl:for-each>
                                                  </p>
                                                  </td>
                                                  </xsl:if>
                                                  </tr>
                                                </table>
                                                <xsl:copy-of select="description/*"/>
                                            </td>
                                        </tr>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </xsl:for-each>
                        </tbody>
                        <tfoot>
                            <tr>
                                <th>HE</th>
                                <th>PE</th>
                                <th>EE</th>
                                <th>EE-T</th>
                                <th>EE-Q</th>
                                <th>EE-V</th>
                                <th/>
                            </tr>
                        </tfoot>
                    </table>
                    <p>* <i> NB: W3C specification version 3.0 was formerly known as version
                        2.1</i></p>
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:function name="f:tick">
        <xsl:param name="node" as="node()?"/>
        <xsl:choose>
            <xsl:when test="exists($node)">
                <xsl:choose>
                    <xsl:when test="$node/@level eq 'Inc'">
                        <img border="0" src="../img/tick-dark-blue-white-bkgd.png" width="25px"/>
                    </xsl:when>
                    <xsl:when test="$node/@level eq 'Open'">
                        <xsl:value-of select="'&#xa0;'"/>
                    </xsl:when>
                    <xsl:when test="exists($node/@level)">
                        <xsl:value-of select="$node/@level"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <img border="0" src="../img/tick-dark-blue-white-bkgd.png" width="25px"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

</xsl:stylesheet>
