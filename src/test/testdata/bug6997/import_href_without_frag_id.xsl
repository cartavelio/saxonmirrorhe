<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="other_stylesheet.xsl"/>
	<xsl:template match="/">
		<result><xsl:apply-templates /></result>
	</xsl:template>
</xsl:stylesheet>