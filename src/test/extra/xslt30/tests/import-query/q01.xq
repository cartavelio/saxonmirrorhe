xquery version "1.0";

module namespace n = "urn:q01";

declare variable $n:VAR := <element><a>23</a></element>;

declare function n:f($myparam) as element()
{
	element {$myparam} { $n:VAR/a }
};

declare %private variable $n:VAR2 := <element><a>23</a></element>;

declare %private function n:f2($myparam) as element()
{
	element {$myparam} { $n:VAR2/a }
};

declare %public function n:f3($myparam) as element()
{
	n:f($myparam)
};