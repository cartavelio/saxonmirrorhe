
declare namespace fo = "http://www.w3.org/1999/XSL/Format";

declare variable $XMLparm as document-node() external;

 
$XMLparm/(element()|processing-instruction()|comment()|text())
	

