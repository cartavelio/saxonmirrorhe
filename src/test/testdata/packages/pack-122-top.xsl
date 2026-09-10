<?xml version="1.0" encoding="UTF-8"?>
<xsl:package name="http://package-122/" package-version="1.0" version="3.0"
    expand-text="yes" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="http://example.org/package">

    <xsl:use-package name="http://package-122/" package-version="1.0-add"/>
    
    <xsl:use-package name="http://package-122/" package-version="1.0-subtract"/>
    
    <xsl:use-package name="http://package-122/" package-version="1.0-multiply"/>


    <xsl:template name="xsl:initial-template" visibility='public'>
        <xsl:document>
            <out>
                <add>{f:add(10,2)}</add>
                <subtract>{f:subtract(10,2)}</subtract>
                <multiply>{f:multiply(10,2)}</multiply>
            </out>
        </xsl:document>
    </xsl:template>

</xsl:package>
