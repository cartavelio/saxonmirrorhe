declare namespace map = "http://www.w3.org/2005/xpath-functions/map";
declare namespace output = "http://www.w3.org/2010/xslt-xquery-serialization";
declare option output:method "text";
let $map := json-doc('olson-to-windows-mapping.json')
for $key in map:keys($map)
order by $key
where $key ne "§§§ NOTE §§§"
return ('        { "'  || $key || '", "' || $map($key) || '" },&#xa;' )