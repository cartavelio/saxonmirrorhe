<!DOCTYPE html SYSTEM "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>SaxonC API design use cases</title>
</head>
<body>
<?php

function userFunction($param2)
{

  echo("user function called\n");
  if(is_numeric($param2)){
    $result = $param2*2;
    return $result;
  }

}


$saxonProc1 = new Saxon\SaxonProcessor(true);
$xpath = $saxonProc1->newXPathProcessor();

$version = $saxonProc1->version();


echo '<b>PHP User Function Example</b><br/>';
echo 'Saxon Processor version: ' . $version;
echo '<br/>';


         $saxonProc1->registerUserFunction("http://www.example.com", "userFunction", "{ \"userFunction\": {".
                                                        "      \"as\": \"xs:integer?\"," .
                                                        "      \"param\": [" .
                                                        "        \"xs:integer\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $xpath = $saxonProc1->newXPathProcessor();
         $xpath->declareNamespace("dy", "http://www.example.com");

         try {
             $result = $xpath->evaluateSingle("dy:userFunction(3)");
             $value = $result->getAtomicValue()->getIntegerValue();
         } catch (Exception $e) {
             echo 'Caught exception: ', $e->getMessage(), "\n";
         }


?>
</body>
</html>
