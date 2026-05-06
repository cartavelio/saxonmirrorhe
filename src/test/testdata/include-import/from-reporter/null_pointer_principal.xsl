<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output omit-xml-declaration="true"/>

	<xsl:include href="null_pointer_included.xsl#abc"/>
	
	<xsl:template match="/">
		<out><xsl:text>Transformation completed successfully using principal stylesheet</xsl:text><xsl:apply-templates/></out>
	</xsl:template>
	
</xsl:stylesheet>