xquery version "3.1";

module namespace testfunc="http://example.com/testfunc";

declare function testfunc:sum(
        $a as xs:integer,
        $b as xs:integer
) as xs:integer {
        $a + $b
};
