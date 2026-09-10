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

  return null;

}


function sumASequence($arr){
  echo("sumASequence called\n");
  if(is_array($arr)){
    return array_sum($arr);
  }

  throw new Exception('$arr argument is not an array type');

}

function implode_all($arr) {
    for ($i=0; $i<count($arr); $i++) {
        if (@is_array($arr[$i]))
            $arr[$i] = implode_all ($arr[$i]);
    }
    return implode(' ', $arr);
}

function twoSequenceParam($arr1, $arr2){
  echo("twoSequenceParam called\n");
  if(is_array($arr1) and is_array($arr2)){
    $out1 = implode_all($arr1);
    $out2 = implode_all($arr2);
    echo $out1."\n";
    echo $out2."\n";
    return True;
  } else {
    throw new Exception('$arr argument is not an array type');
  }
    return False;
}

function sequenceOfArraysParam($arr1, $arr2){
  echo("sequenceOfArraysParam called\n");
  if(is_array($arr1) and is_array($arr2)){
    $out1 = implode_all($arr1);
    $out2 = implode_all($arr2);
    echo $out1."\n";
    echo $out2."\n";
    return True;
  } else {
    throw new Exception('$arr argument is not an array type');
  }
    return False;

}

function sequenceOfArraysParamB($arr1){
  echo("sequenceOfArraysParamB called\n");
  if(is_array($arr1)){
    $out1 = implode_all($arr1);
    echo $out1."\n";
    return True;
  } else {
    throw new Exception('$arr argument is not an array type');
  }
    return False;

}

function sequenceOfMapParam($map1, $map2){
  echo("sequenceOfMapParam called\n");
  if(is_array($map1)){
    $out1 = implode_all($map1);
    echo $out1."\n";
    return True;
  } else {
    throw new Exception('$map argument is not an array type');
  }
    return False;
}

function attributeParam($input){
  echo("attributeParam called\n");
  return $input != NULL && ($input instanceof Saxon\XdmNode) && $input->getNodeKind() == 2;
}


function documentParam($input){
  echo("documentParam called\n");
  return $input != NULL && ($input instanceof Saxon\XdmNode) && $input->getNodeKind() == 9;
}

function elementParam($input){
  echo("elementParam called\n");
  return $input != NULL && ($input instanceof Saxon\XdmNode) && $input->getNodeKind() == 1;
}

function nodeParam($input){
  echo("nodeParam called\n");
  return $input != NULL && ($input instanceof Saxon\XdmNode) && $input->getNodeKind() >= 1 && $input->getNodeKind() <= 13;
}


function testSequence($saxonProc) {
         $saxonProc->registerUserFunction("http://www.example.com", "sumASequence", "{ \"sumASequence\": {".
                                                        "      \"as\": \"xs:integer?\"," .
                                                        "      \"param\": [" .
                                                        "        \"xs:integer*\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $xpath = $saxonProc->newXPathProcessor();


         $xpath->declareNamespace("dy", "http://www.example.com");

         try {
             $result = $xpath->evaluateSingle("dy:sumASequence((1,2,3,4,5))");
             if($result != null) {
                $value = $result->getAtomicValue()->getLongValue();
                echo 'Correct Result='.$value;
             }
         } catch (Exception $e) {
             echo 'Caught exception: ', $e->getMessage(), "\n";
         }

}

function testSimpleUserFunction($saxonProc1){
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
         if($result != null) {
            $value = $result->getAtomicValue()->getLongValue();
            echo 'Result='.$value;
         }
     } catch (Exception $e) {
         echo 'Caught exception: ', $e->getMessage(), "\n";
     }

}


function testTwoSequence($saxonProc) {
         $saxonProc->registerUserFunction("http://www.example.com", "twoSequenceParam", "{ \"twoSequenceParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"xs:string*\", \"xs:integer*\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $xpath = $saxonProc->newXPathProcessor();


         $xpath->declareNamespace("ms", "http://www.example.com");

         try {
             $result = $xpath->evaluateSingle("ms:twoSequenceParam(('a','b','c'), (1,2,3))");
             if($result != null) {
                $value = $result->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }
         } catch (Exception $e) {
             echo 'Caught exception: ', $e->getMessage(), "\n";
         }

}

function commonSamplesForSequences($saxonProc) {
         $saxonProc->registerUserFunction("http://www.example.com", "sequenceOfArraysParam", "{ \"sequenceOfArraysParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"array(*)\", \"array(*)\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $saxonProc->registerUserFunction("http://www.example.com", "sequenceOfArraysParamB", "{ \"sequenceOfArraysParamB\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"array(*)*\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $saxonProc->registerUserFunction("http://www.example.com", "sequenceOfMapParam", "{ \"sequenceOfMapParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"map(*)\", \"map(*)\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $xpath = $saxonProc->newXPathProcessor();


         $xpath->declareNamespace("ms", "http://www.example.com");

         try {
             $result = $xpath->evaluateSingle("ms:sequenceOfArraysParam(array{1,2,3}, array{'a',2,3})");
             if($result != null) {
                $value = $result->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }

             $result1 = $xpath->evaluateSingle("ms:sequenceOfArraysParamB((array{1,2,3}, array{'a',2,3}))");
             if($result1 != null) {
                $value = $result1->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }

             $result2 = $xpath->evaluateSingle("ms:sequenceOfMapParam(map{'a':1,'b':2},map{'c':3,'d':'four'})");
             if($result1 != null) {
                $value = $result1->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }
         } catch (Exception $e) {
             echo 'Caught exception: ', $e->getMessage(), "\n";
         }

}

function handlingXdmNodes($saxonProc) {
         $saxonProc->registerUserFunction("http://www.example.com", "elementParam", "{ \"elementParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"element(*)\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $saxonProc->registerUserFunction("http://www.example.com", "documentParam", "{ \"documentParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"document-node()\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", False);

         $saxonProc->registerUserFunction("http://www.example.com", "nodeParam", "{ \"nodeParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"node()\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", False);

         $saxonProc->registerUserFunction("http://www.example.com", "attributeParam", "{ \"attributeParam\": {".
                                                        "      \"as\": \"xs:boolean\"," .
                                                        "      \"param\": [" .
                                                        "        \"attribute()\"" .
                                                        "      ],".
                                                        "      \"arity\": [1]" .
                                                        "    }".
                                                        "}", True);

         $xpath = $saxonProc->newXPathProcessor();


         $xpath->declareNamespace("ms", "http://www.example.com");

         try {
             $result = $xpath->evaluateSingle("ms:elementParam(parse-xml('<doc/>')/*)");
             if($result != null) {
                $value = $result->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }

             $result1 = $xpath->evaluateSingle("ms:documentParam(parse-xml('<doc/>'))");
             if($result1 != null) {
                $value = $result1->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }

             $result2 = $xpath->evaluateSingle("ms:nodeParam(parse-xml('<doc/>')/node())");
             if($result2 != null) {
                $value = $result2->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }

             $result3 = $xpath->evaluateSingle("ms:attributeParam(parse-xml('<doc a=\"1\"/>')/*/@a)");
             if($result3 != null) {
                $value = $result3->getAtomicValue()->getBooleanValue();
                echo 'Correct Result='.$value;
             }
         } catch (Exception $e) {
             echo 'Caught exception: ', $e->getMessage(), "\n";
         }

}



$saxonProc1 = new Saxon\SaxonProcessor(true);
$xpath = $saxonProc1->newXPathProcessor();

$version = $saxonProc1->version();


echo '<b>PHP User Function Example</b><br/>';
echo 'Saxon Processor version: ' . $version;
echo '<br/>';




testSimpleUserFunction($saxonProc1);

testSequence($saxonProc1);

testTwoSequence($saxonProc1);

commonSamplesForSequences($saxonProc1);

handlingXdmNodes($saxonProc1);


?>
</body>
</html>
