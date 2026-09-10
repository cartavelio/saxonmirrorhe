<?xml version="1.0" encoding="UTF-8"?>
<!--
1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!--
================================================================================================== 
This transformation goes through an entire part of SQL, locating Conformance Rules
and collecting them for Annex A; in doing so, it sorts by Feature ID, then by Subclause number.  
Rather clever, if I do say so myself... 
================================================================================================== 
-->

<!--
  Identity transformation so that everything not otherwise transformed is preserved
-->
<xsl:template match="*|@*|comment()|processing-instruction()|text()" priority="0">
  <xsl:copy>
    <xsl:apply-templates select="*|@*|comment()|processing-instruction()|text()"/>
  </xsl:copy>
</xsl:template>


<!--
================================================================================================== 
  Define the principle key for this transormation

  The 'generic-id' key finds elements with an id attribute
================================================================================================== 
-->
<xsl:key name="generic-id" match="*[@id]" use="@id"/>


<!--
================================================================================================== 
  The 'feature-code' key captures feature elements, indexed by the code attribute
================================================================================================== 
-->
<xsl:key name="feature-code" match="//ConformanceRules/olist/item/feature" use="@code"/>


<!--
================================================================================================== 
  The 'conformance-rules' key captures item elements in ConformanceRule elements,
  indexed by the id attribute of the ancestor clause/subClause* element
================================================================================================== 
-->
<xsl:key name="feature-code" match="//ConformanceRules/olist/item" use="../../../../@id"/>


<!--
================================================================================================== 
   This template matches the conformanceSummary element, which really should be used only in
   "Annex A", but that's not enforced by the DTD...so be nice!!
================================================================================================== 
-->
<xsl:template match="conformanceSummary" priority="1">
  <olist> 
    <xsl:for-each select="//ConformanceRules/olist/item/feature
                          [generate-id() =
                           generate-id(key('feature-code', @code)[1])]">
      <xsl:sort select="@code"/>
      <xsl:variable name="this-feature-code" select="@code"/>
      <xsl:variable name="this-feature-description" select="text()"/>
      <item>
        <xsl:text>Specifications for </xsl:text><xsl:copy-of select="."/><xsl:text>:</xsl:text>
        <olist>
          <xsl:for-each select="//ConformanceRules/olist/item
                              [feature/@code = $this-feature-code][1]">
            <xsl:variable name="this-section" select="../../../../@id"/>
            <item>
              <specref ref="{$this-section}"/><xsl:text>:</xsl:text>
              <olist>
                <xsl:for-each select="$this-section/../bodyMatter/ConformanceRules/olist/item
                                    [feature/@code=$this-feature-code]">
                  <xsl:choose>
                    <xsl:when test="@id">
                      <xsl:variable name="idAttr-id" select="generate-id(@id)"/>
                      <xsl:variable name="new-id-value">
                        <xsl:value-of select="./@id"/>
                        <xsl:text>_AnnexA</xsl:text>
                      </xsl:variable>
                      <item>
                        <xsl:copy-of select="@*[generate-id() != $idAttr-id]"/>
                        <xsl:attribute name="id">
                          <xsl:value-of select="$new-id-value"/>
                        </xsl:attribute>
                        <xsl:copy-of select="./node()"/>
                      </item>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:copy-of select="."/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:for-each>
              </olist>
            </item>
          </xsl:for-each>
        </olist>
      </item>
    </xsl:for-each>
  </olist>
</xsl:template>

</xsl:stylesheet>
