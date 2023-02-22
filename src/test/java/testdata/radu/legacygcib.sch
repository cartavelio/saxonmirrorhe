<?xml version="1.0" encoding="iso-8859-1"?>
<iso:schema xmlns="http://purl.oclc.org/dsdl/schematron"
	xmlns:iso="http://purl.oclc.org/dsdl/schematron" xmlns:xlink="http://www.w3.org/1999/xlink"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" queryBinding="xslt2" schemaVersion="ISO19757-3">

	<iso:title>P2P Legacy GCIB XML Appliance Service Asertions</iso:title>

	<iso:pattern id="DHTMLx.Tree.Structure.Assertions">
		<iso:title>DHTMLx.Tree.Structure.Assertions</iso:title>

		<iso:rule context="headers">
			<iso:assert test="status.code=200"> The http status code should be equal to 200 for this test
				case. </iso:assert>
		</iso:rule>

	</iso:pattern>
</iso:schema>
