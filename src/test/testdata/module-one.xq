module namespace m='http://www.example.com/module-one';

declare function m:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };

declare variable $m:ten as xs:integer := 10;