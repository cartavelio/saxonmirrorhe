import schema namespace testns="http://myns" at "schema.xsd"; 

declare function local:test( $doc as element(*,testns:docType) ) {

let $objs := $doc/testns:object[@attr1 = "attr1"]
let $att1 := $objs[. instance of element(*,testns:concreteTypeAttr12)]

return
	data( ($att1/@attr2,1)[1] )
};


local:test( (validate{ . })/testns:doc)