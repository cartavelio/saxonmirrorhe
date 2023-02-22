xquery version "1.0";


module namespace n = "saxon:bug";

declare variable $n:VAR := <element><a/></element>;

declare function n:get-a($myparam) as element()
{
	element {$myparam} { $n:VAR/a }
};