import schema namespace simple="http://www.example.org/simple" at "xsd/simple.xsd";
declare variable $phoneNumber external;
let $currentPhoneNo as simple:PhoneNumber := data($phoneNumber) cast as simple:PhoneNumber
return $currentPhoneNo
