<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test of saxon:threads on xsl:result-document with a document
     the test is based on xsl:For-each-group with attribute group-adjacent -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>


<xsl:template match="cities">
<!-- simple test of value-based grouping -->

  <xsl:for-each-group select="city" group-adjacent="@country">
    <xsl:result-document href="{current-grouping-key()}.out" saxon:asynchronous="yes" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <table>
    <tr>
        <th>Position</th>
        <th>Country</th>
        <th>City List</th>
        <th>Population</th>
     </tr>
    <tr>
      <td><xsl:value-of select="position()"/></td>
      <td><xsl:value-of select="@country"/></td>
      <td>
        <xsl:for-each select="current-group()">
          <xsl:value-of select="@name"/>
          <xsl:if test="position() != last()">, </xsl:if>
        </xsl:for-each>
      </td>
      <td><xsl:value-of select="sum(current-group()/@pop)"/></td>
    </tr>
    </table>
    </xsl:result-document>
  </xsl:for-each-group>


</xsl:template>
</xsl:stylesheet>
