xquery version "3.1";
module namespace test="http://example.com/ns/test";

declare function test:function1() as xs:string {
   "test:function1"
};

declare function test:function2($par as xs:string) as xs:string {
   "test:function2-" || $par
};

declare %private function test:private-function() as xs:string {
  "test:private-function"    
};

declare function test:function3() as xs:string {
  test:private-function() || "-" || "called"
};
