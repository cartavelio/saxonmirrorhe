xquery version "1.0" encoding "utf-8";
module namespace myf = "urn:active-endpoints:bpep:function";

declare function myf:a($e as xs:string) as xs:string {
      concat($e, ' World')
};

declare function myf:b($e as xs:string) as xs:string {
      myf:a($e)
};

declare function myf:sayHello($e as xs:string) as xs:string {
      myf:b($e)
};

