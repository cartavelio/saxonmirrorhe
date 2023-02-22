<?xml version="1.0"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	
	<xsl:param name="s" select="'s-default'" static='true'/>
	<xsl:param name="d" select="'d-default'" static='false'/>

	<xsl:template match="/">
		<xsl:variable name="orderedBills">
			<Bills>
				<xsl:for-each select="Bills/Bill">
					<xsl:sort select="@No"/>
					<xsl:copy-of select="."/>
				</xsl:for-each>
			</Bills>
		</xsl:variable>
		<Bills s="{$s}" d="{$d}">
			<xsl:for-each select="$orderedBills/Bills/Bill">
				<Bill>
					<xsl:for-each select="@*">
						<xsl:copy-of select="."/>
					</xsl:for-each>
					<xsl:attribute name="NewBill">
						<xsl:variable name="previousBill" select="preceding-sibling::Bill[position()=1]"/>
						<xsl:choose>
							<xsl:when test="not($previousBill) or $previousBill/@No != @No">1</xsl:when>
							<xsl:otherwise>0</xsl:otherwise>
						</xsl:choose>
					</xsl:attribute>
					<xsl:apply-templates/>
				</Bill>
			</xsl:for-each>
		</Bills>
	</xsl:template>
</xsl:stylesheet>