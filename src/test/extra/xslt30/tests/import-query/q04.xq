xquery version "1.0";

module namespace n4 = "urn:q04";

declare function n4:f($myparam as xs:integer) as xs:integer
{
	$myparam + 23
};

