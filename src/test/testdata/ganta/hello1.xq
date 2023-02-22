xquery version "1.0" encoding "utf-8";
module namespace abc = "urn:active-endpoints:bpep:function1";

declare function abc:sayBye($e as xs:string) as xs:string {
      concat('Bye ', $e)
};