xquery version "1.0";

module namespace n3 = "urn:q03";
import module namespace n4 = "urn:q04" at "q04.xq";

declare function n3:f($myparam as xs:integer) as xs:integer
{
	n4:f($myparam)
};

