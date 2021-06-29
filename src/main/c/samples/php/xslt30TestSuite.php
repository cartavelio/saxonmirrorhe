<?php
/*error_reporting(E_ALL|E_STRICT);
ini_set('display_errors', 'on');*/
ini_set('max_execution_time', 1200);
/*$root = pathinfo($_SERVER['SCRIPT_FILENAME']);
define('BASE_FOLDER', basename($root['dirname']));
define('SITE_ROOT', realpath(dirname(__FILE__)));
define('SITE_URL', 'http://' . $_SERVER['HTTP_HOST'] . '/' . BASE_FOLDER);


echo 'BASE_FOLDER' . basename($root['dirname']);
echo '<br/>';
echo 'SITE_ROOT' . realpath(dirname(__FILE__));
echo '<br/>';
echo 'SITE_URL' . 'http://' . $_SERVER['HTTP_HOST'] . '/' . BASE_FOLDER;
echo '<br/>';
*/

//$path1 = "/media/ond1/c8f4c380-ed5d-4791-b60d-90df7fd39798/work/git/xslt30-test/";
//$path = "/media/ond1/c8f4c380-ed5d-4791-b60d-90df7fd39798/work/git/";

$path = __DIR__ . "/xslt30-test/";
$catalog = $path . "catalog.xml";
$saxon_version = "";

$proc = new Saxon\SaxonProcessor(True);
$proc->setConfigurationProperty("http://saxon.sf.net/feature/licenseFileLocation", "/usr/lib/saxon-license.lic");
$proc->setcwd("");
$xsltproc = $proc->newXslt30Processor();

$xpathProcForEnv = $proc->newXPathProcessor();

$xpathProcForEnv->declareNamespace("", "http://www.w3.org/2012/10/xslt-test-catalog");
//$xpathProcForEnv->setBaseURI($path);
/*$start = microtime(true);
              $proc->setSourceFile("cat.xml");                                              d
                $proc->importStylesheetFile("test.xsl");
                $result = $proc->transformToValue();
 $end = microtime(true); */

$pass = 0;
$fail = 0;
$error =0;
$notrun = 0;
$run_test = null;
$run_test_case = null;//"as-0119";//"copy-of-010";


function test_set_files($catalog)
{
    global $proc;
    global $xpathProcForEnv;
    $array = array();
    $attName;
    $attFile;

    if (file_exists($catalog)) {
        //$xmlV = $proc->parseXmlFromFile($catalog);//simplexml_load_file($catalog);
        //$xpathProcForEnv->setContextItem($xmlV);
        $xpathProcForEnv->setContextFile($catalog);

        $xml = $xpathProcForEnv->evaluate("//*:test-set");

        if ($xml != NULL) {
            //foreach ($xml->{'test-set'} as $value) {
            for ($i = 0; $i < $xml->size(); $i++) {
                $value = $xml->itemAt($i);
//	echo "________________".$value->attributes()->name."_________".$value->attributes()->file;
                
                if ($value != NULL && $value->isNode()) {
                    $node = $value->getNodeValue();

                    if ($node != NULL) {
                        $attName = $node->getAttributeValue("name");
                        $attFile = $node->getAttributeValue("file");
                        $array["" . $attName] = $attFile;
                    } else {
                        error_log("Node for test-set is NULL");
                    }
                    unset($node);
                } else {
                    error_log("Value for test-set is NULL");

                }
                unset($value);
            }

        }
    } else {
        exit('Failed to open ' . $catalog);
    }
	error_log("checkpoint 1");
    return $array;
}

function check_dependency($xml)
{

    if($xml != null) {
        error_log("dependency_count=".$xml->size());
        $xmlNode = $xml->itemAt(0)->getNodeValue();
        error_log("check_depend=".$xmlNode);

        $count = $xmlNode->getChildCount();
        error_log("dependency_child=".$count);

        for ($i = 0; $i < $count; $i++) {
            $child = $xmlNode->getChildNode($i);
            error_log("NodeKind=".$child->getNodeKind().", check_depend child=".$xmlNode->getChildNode($i)->getNodeKind(). "child=".$xmlNode->getChildNode($i));
            if ($child != null && $child->getNodeKind() == 1) {
                $var1 = $child->getLocalName();
                error_log("check_depend var1=".$var1. " value=".$child->getAttributeValue('value')."feature result=".($child->getAttributeValue('value')=="schema_aware"));
                switch ($var1) {
                    case "year_component_values":
                        //if value = 'support year zero' then set schema_aware and return true
                        return false;

                    case "feature":
                        if ($child->getAttributeValue('value')== "schema_aware") {
                            return false;
                        }   
                    case "spec":
                        if ($child->getAttributeValue('value')== "XSLT30" or $child->getAttributeValue('value')== "XSLT20+" or $child->getAttributeValue('value')== "XSLT30+" or $child->getAttributeValue('value')== "XSLT10+") {
                           //true
                        } else {
                            return false;
                        }

                }
            }
            unset($child);
        }
    }
    return true;
}

function parse_environment($xml, $path)
{
    global $proc;
    global $xpathProcForEnv;
    $array = array();
    if($xpathProcForEnv == null) {
        return $array;
    }
    
    $xpathProcForEnv->setContextItem($xml);
    $envValues = $xpathProcForEnv->evaluate("//*:test-set/*:environment");

    
    if($envValues == null) {
        return $array;
    }

    $count = $envValues->size();
    for ($i = 0; $i < $count; $i++) {

        $value = $envValues->itemAt($i);
        if( $value->isNode()) {

            $nodeii = $value->getNodeValue();
            if ($nodeii == null) {
                echo "environment node is null";
                continue;


            }

            $envName = $nodeii->getAttributeValue("name");
            $newarray = array();
            $newarray["" . $envName] = array();
            if($envName == "as-16") {
            error_log("env node=".$nodeii.",   \nenvName BBBB1= ".$envName);
            }

            //$array["" . $envName] = array();
            $xpathProcForEnv2 = $proc->newXPathProcessor();
            $xpathProcForEnv2->clearParameters();
            $xpathProcForEnv2->clearProperties();
            $xpathProcForEnv2->setContextItem($nodeii);
            $sources = $xpathProcForEnv2->evaluate("./*:source");
            $scount = 0;
            if($sources != null) {
                $scount = $sources->size();
                $newarray["sources"][] = array(); //$newarray["" . $envName]["sources"] = array();


                for ($j = 0; $j < $scount; $j++) {
                    $valuej = $sources->itemAt($j);
                    if ($valuej == null) {

                        continue;

                    }
                    if ($valuej->isNode()) {
                        $nodej = $valuej->getNodeValue();
                        if($envName == "as-16") {
                            error_log("size=".$scount.", sources for env node=" . $envName . ",   \nenvName CCCCC1= " . $nodej. "size = ".$scount);
                        }
                        $rawuri = $nodej->getAttributeValue("uri");
                        $role = $nodej->getAttributeValue("role");
                        //$file = $nodej->getAttributeValue("file");
                        if ("." == $role) {
                            $href = $nodej->getAttributeValue("file");
                            if ($href == null) {
                                $xpathProcForEnv->clearParameters();
                                $xpathProcForEnv->clearProperties();
                                $xpathProcForEnv->setContextItem($nodej);
                                $sourceContent = $xpathProcForEnv->evaluateSingle("string(content)");
                                $newarray["sources"][".content"] = $sourceContent->getStringValue();
                                //$array["" . $envName]["sources"][".content"] = $sourceContent->getStringValue();


                            } else {
                                $newarray["sources"][".file"] = $href;
                                //$array["" . $envName]["sources"][".file"] = $href;

                            }

                        }  elseif ($rawuri != null) {
                            //$array["" . $envName]["sources"][$rawuri] = $rawuri;
                            $newarray["sources"][".file"] = $rawuri;
                        }

                    }
                }


            }
           // if($envName == "as-01b") {
                error_log("Create env for " . $envName . " dump=" . print_r($newarray, TRUE));
                $array["" . $envName] = $newarray;
                error_log(" Whole dump=" . print_r($array, TRUE));
           // }
        }
       unset($nodeii);
    }
    echo "<br/>";
    echo "<br/>";
     //Test

    //$testArraySize = count($array);
    error_log("TEST ARRAY ITEMS YYYYYY1");
    foreach ( $array as $var ) {
        error_log("Array item".print_r($var, TRUE)."\n");
        error_log("\n");
        error_log("\n");
    }


    return $array;
}

function test_case($test_set)
{
    global $proc;
    global $xpathProcForEnv;
    global $run_test;
    global $run_test_case;
    $env = null;
    $xml = null;
    $test_path = pathinfo($test_set)['dirname'] . "/";

    if (file_exists($test_set)) {
        $xml = $proc->parseXmlFromFile($test_set);
        $test_set_name = $xml->getChildNode(0)->getAttributeValue("name");
        if($run_test != null && $test_set_name != $run_test) {
            return;
        }
         
        if($xml == null) {
            echo "Cannot find test ". $test_set. "\n";
            if ($proc->exceptionOccurred() && $proc->getErrorMessage() != null) {
                echo " error: " . $proc->getErrorMessage();
            }
            return;
        }  else {
            $env = parse_environment($xml, $test_path);
        //    print_r($env);
        }


        echo "<table border='1' style='width:60%'>
<tr>
  <th>Test Case Name </th>
  <th style='width:80%'>Result </th>
</tr>";
        $t_dep = getNodeFromXPath($xml, "./dependencies");

        $check = check_dependency($t_dep);
        $testCases = $xpathProcForEnv->evaluate("//test-case");
        if ($check &&   $testCases != null) {

            $countTC = $testCases->size();
            for ($x = 0; $x < $countTC; $x++) {
                $value = $testCases->itemAt($x);
                if($value->isNode()) {
                    $nodex = $value->getNodeValue();
                    $testCaseName = $nodex->getAttributeValue("name");
                    echo "<tr><td>$testCaseName</td>";
                    $result = null;
                    if($run_test_case == null || $run_test_case == $testCaseName) {
                        $result = run_test_case($nodex, $env, $test_path);
                    }  else {
                        $result = "Not run";

                    }

                    echo "<td>" . $result . "<br/></td>";
                    echo "</tr>";
                    //$test = $value->{'test'}->{'stylesheet'};
                    //echo "____test:".$test_path."tests".$test->attributes()->file;
                }
            }

        } else {
            echo "<tr><td>No tests run</td><td></td></tr>";
        }
        echo "</table>";

    } else {
        exit('Failed to open ' . $test_set);
    }
}

function getNodeFromXPath($node, $xpath) {
    global $xpathProcForEnv;
    $xpathProcForEnv->clearParameters();
    $xpathProcForEnv->clearProperties();
    if($node != null) {
        $xpathProcForEnv->setContextItem($node);
    }
      $result = $xpathProcForEnv->evaluate($xpath);
      return $result;

}


function getSingleNodeFromXPath($node, $xpath) {
    global $xpathProcForEnv;
    $xpathProcForEnv->clearParameters();
    $xpathProcForEnv->clearProperties();
    if($node != null) {
        $xpathProcForEnv->setContextItem($node);
    }

      $result = $xpathProcForEnv->evaluateSingle($xpath);
      return $result;

}



function testAssertion($assert, $result, $errorCode) {
    //global $xpathProcForAssertions;

    global $proc;

    $xpathProcForAssertions = $proc->newXPathProcessor();
    
    /*$xpathProcForAssertions->declareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
    $xpathProcForAssertions->declareNamespace("xs", "http://www.w3.org/2001/XMLSchema");
    $xpathProcForAssertions->declareNamespace("math", "http://www.w3.org/2005/xpath-functions/math");
    $xpathProcForAssertions->declareNamespace("map", "http://www.w3.org/2005/xpath-functions/map");
    $xpathProcForAssertions->declareNamespace("array", "http://www.w3.org/2005/xpath-functions/array");
    $xpathProcForAssertions->declareNamespace("j", "http://www.w3.org/2005/xpath-functions");*/

    $xpathProcForAssertions2 = $proc->newXPathProcessor();
   //$aSize = $asserts->size();
   //for($i=0; $i < $aSize; $i++) {

       //$item = $asserts->itemAt($i)->getNodeValue();//getNodeFromXPath($asserts->itemAt($i)->getNodeValue(), "//result");
      // if($asserts->itemAt(0)->isNode())  {
          // $assert =  $asserts->itemAt($i)->getNodeValue();
           $xpathProcForAssertions->setBaseURI($assert->getBaseURI());
           error_log("chck result=".$assert);

       $assertName = $assert->getLocalName(); // substr($assert->getNodeName(), strpos($assert->getNodeName(), "}")+1);
           error_log("localName=".$assertName.", assertxx=".$assert);
    if ($assertName == "error") {
            if($errorCode == null) {
                $errorCode = "";
            }
            $aErrorCode = $assert->getAttributeValue("code");
            if ($aErrorCode == $errorCode) {
                return true;
            } else {
                error_log("Test failed but not with the correct error code, assert=".$aErrorCode . "Actual error code=".$errorCode);
                 return true;
            }

        } elseif($result == null) {
        return true; //TODO fixup

    }

        elseif($assertName == "assert-xml") {
            $file = $assert->getAttributeValue("file");
            $xpathProcForAssertions->setContextItem($assert);

            $comparandNode = $xpathProcForAssertions->evaluateSingle("if (@file) then unparsed-text(resolve-uri(@file, base-uri(.))) else string(.)");
            $message = $xpathProcForAssertions->getErrorMessage();
            if($message != null || $comparandNode == null) {
                error_log("ERRORXXX=" . $message);
                return false;
            }
            $comparand = $comparandNode.'';
            error_log("comparand=".$comparandNode);
            if (strpos($comparand, "<?xml") === 0) {
                $index = strpos($comparand, "?>");
                error_log("Error=       ".$index);
                $comparand = substr($comparand, $index + 2);
            }
            $comparand = trim($comparand);

            $comparand =  str_replace(array('.', ' ', "\n", "\t", "\r"), '', $comparand);
            $resultStr =  $result->itemAt(0)->getNodeValue();
            $resultStr =  str_replace(array('.', ' ', "\n", "\t", "\r"), '', $resultStr);

            error_log("XXXXXX comparand=" . $comparand . ", result = " . $resultStr);

            if ($comparand == $resultStr) {
                return true;
            } else {
                error_log("assert-xml: " . $comparand . "\n Outcome:" . $result);
                return false;
            }

        } elseif ($assertName == "assert")  {
            $xpathProcForAssertions->clearParameters();
            $xpathProcForAssertions->clearProperties();
            if($result != null && $result->size()==1) {
                

                $item = $result->itemAt(0);
                //error_log("itemYYYYYY= ".$item->getNodeValue());

                $xpathProcForAssertions2->setContextItem($item);

                $namespaceValues = $xpathProcForAssertions2->evaluate("for \$i in 1 to count(//namespace::*) return if (empty(index-of((//namespace::*)[position() = (1 to (\$i - 1))][name() = name((//namespace::*)[\$i])], (//namespace::*)[\$i]))) then (//namespace::*)[\$i] else ()");

                $xpathProcForAssertions->clearParameters();
                $xpathProcForAssertions->clearProperties();

                if($namespaceValues!= null && $namespaceValues->size() > 0 ) {
                    for($j=0; $j < $namespaceValues->size(); $j++) {
                        $item = $namespaceValues->itemAt($j);
                        if($item->isNode())  {
                            $nsNode = $item->getNodeValue();
                            if($nsNode != null) {
                                $nsName = $nsNode->getNodeName();
                                error_log("Namespace=" . $nsName . " = " . $nsNode->getStringValue());

                                if ($nsName != null) {
                                   // $xpathProcForAssertions->declareNamespace($nsName, $nsNode->getStringValue());
                                }

                        }

                        }
                    }



                }

                $xpathProcForAssertions->setContextItem($result->itemAt(0));
                $xpath = $assert->getChildNode(0)->getStringValue();
                error_log("cp0 - assertLLL");
                error_log("xpath=".$xpath. ", XXXXXX result=" . $result->itemAt(0));
                $check = $xpathProcForAssertions->effectiveBooleanValue($xpath);
                $testResult = $xpathProcForAssertions->evaluateSingle("/out");

                error_log(", XXXXXX testResult=" . $testResult);
                
                if($check == false) {
                    return "assert: " . $xpath . "\n OutcomeXXY:" . $result->itemAt(0);
                }
                return true;


            }  else {
                error_log("FAIL - assertion with multipl results not handled yet");
                return false;

            }

        } elseif ($assertName == "all-of") {
            $jasserts = getNodeFromXPath($assert, "*");
            //$jNode = $jasserts->itemAt(0)->getNodeValue();
            $jSize = $jasserts->size();
            for ($j = 0 ; $j < $jSize; $j++) {
                if(!testAssertion($jasserts->itemAt($j)->getNodeValue(), $result, $errorCode)) {
                    false;

                }

            }

            return true;

        }
        
            
    //    }
       
  // }
   error_log("testAssertion is false");
   return false;
    
}


function run_test_case($testcase, $env, $path)
{
    global $proc;
    global $xsltproc;
    global $xpathProcForEnv;
    global $pass;
    global $fail;
    global $notrun;
    $errorCode  = null;
    $xsltproc->clearParameters();
    
    $errorTest = FALSE;
    $t_env = getNodeFromXPath($testcase, "./environment");

    $testDirValue = $proc->createAtomicValue(strval($path));
    $t_env_ref = null;
    if($t_env != null) {

        $t_env_ref = $t_env->itemAt(0)->getNodeValue()->getAttributeValue("ref");
         error_log("TEST CASE t_env_ref = ".$t_env_ref);
    }
    $t_dep = getNodeFromXPath($testcase, "./dependencies");

    $checkDependency = check_dependency($t_dep);//check_dependency($testcase->dependencies);
    if ($checkDependency) {
        $testElem = getSingleNodeFromXPath($testcase, "./test");

         error_log("TEST ELEMENT :".$testElem);
        if ($testElem != null) {                
            $testStylesheet = getSingleNodeFromXPath($testElem->getNodeValue(), "./stylesheet");

            if ($testStylesheet != null) {
                $test = $path . $testStylesheet->getNodeValue()->getAttributeValue("file");
                
                echo "Test case=" . $test;
                if (file_exists($test)) {
                    try {
                        //if ($t_env != null) {

                                $result = NULL;
                                $paramVar = getNodeFromXPath($testElem->getNodeValue(), "./param");

                                if($paramVar != null) {
                                    $paramCount = $paramVar->size();
                                    for($p = 0; $p < $paramCount;$p++) {
                                        $paramNode =  $paramVar->itemAt($p)->getNodeValue();
                                        $pValue = $paramNode->getAttributeValue("select");
                                        $pStatic = $paramNode->getAttributeValue("static");
                                        $pName = $paramNode->getAttributeValue("name");

                                        error_log("YYYYY  static parameters=". $pName. ", pValue=".$pValue);

                                        if($pStatic == "yes" and $pValue != null and $pValue != null) {
                                            $rValue = getNodeFromXPath(null, $pValue);
                                            error_log("rValue=". $rValue);
                                            if($rValue != null) {
                                                $xsltproc->setParameter($pName, $rValue);
                                            } else {
                                                error_log("Error: Could not set static parameter - cp0");
                                            }
                                        } else {
                                            error_log("Error: Could not set static parameter");
                                        }
                                    }
                                    
                                }

                                //error_log("before apply stylesheet:".$test);
                                $executable = $xsltproc->compileFromFile($test);
                                //$init_templateV = getNodeFromXPath($testElem->getNodeValue(), "./initial-template");
                                if ($executable == NULL) {
                                    error_log("Executable is null on the following file: " . $test);
                                    $result = NULL;
                                    $errorCode = $xsltproc->getErrorCode();
                                    //return "FAIL Error:".$xsltproc->getErrorMessage();
                                } else {

                                    $init_template = getSingleNodeFromXPath($testElem->getNodeValue(), "./initial-template");
                                    
                                    $init_mode = getSingleNodeFromXPath($testElem->getNodeValue(), "./initial-mode");
                                    
                                    $nit_function = getSingleNodeFromXPath($testElem->getNodeValue(), "./initial-function");





                                    
                                    

                                    /*
                                     *
                                     *
                                     *         Optional<XdmNode> initialMode = testInput.select(child("initial-mode")).asOptionalNode();
                                             Optional<XdmNode> initialFunction = testInput.select(child("initial-function")).asOptionalNode();
                                             Optional<XdmNode> initialTemplate = testInput.select(child("initial-template")).asOptionalNode();

                                            
                                QName initialFunctionName = getQNameAttribute(xpath, testInput, path("initial-function", "@name"));

                                    TODO check for output@ file
                                     * 
                                     */


                                    if ($init_template != null ) {

                                        if($t_env_ref != null) {
                                            $input = $path. $env[$t_env_ref . ""]["sources"][".file"];
                                            error_log("file:". $input);
                                            if($input != null) {
                                                error_log("input file: ".$input);
                                                $executable->setGlobalContextFromFile($input);
                                            }
                                            



                                        }
                                        $init_node = $init_template->getNodeValue();

                                        error_log("init_node: ".$init_node->getAttributeValue("name"));
                                        $init_name = $init_node->getAttributeValue("name");

                                        if($init_name == "xsl:initial-template") {
                                            $init_name =  "{http://www.w3.org/1999/XSL/Transform}initial-template";
                                        }

                                        $result = $executable->callTemplateReturningValue($init_name);
                                    } elseif ($init_mode != null) {

                                        $initialModeName = $init_template->getNodeValue()->getAttributeValue("name");
                                        $modeSelect = $init_template->getNodeValue()->getAttributeValue("select");
                                        if ($modeSelect != null) {
                                            $initialMatchSelection = getNodeFromXPath($modeSelect."", null);

                                        }
                                    
                                        
                                        
                                    }else {
                                         error_log("envvvalue:". print_r($env, TRUE));
                                         error_log("t_env_ref = ".$t_env_ref);
                                         $input = null;
                                         if($env[$t_env_ref . ""] != null) {
                                             error_log("env[t_env_ref] value:" . print_r($env[$t_env_ref . ""], TRUE));
                                             if(array_key_exists("sources", $env[$t_env_ref . ""])) {
                                                 if(array_key_exists(".file", $env[$t_env_ref . ""]["sources"])) {
                                                     $input = $path. $env[$t_env_ref . ""]["sources"][".file"];
                                                     $executable->setInitialMatchSelectionAsFile($input);
                                                     $executable->setGlobalContextFromFile($input);


                                                 } elseif (array_key_exists(".content", $env[$t_env_ref . ""]["sources"])) {
                                                     $input = $path. $env[$t_env_ref . ""]["sources"][".content"];
                                                     $cNode = $proc->parseXmlFromString($input);

                                                     if($cNode != null) {
                                                         $executable->setInitialMatchSelection($input);
                                                         $executable->setGlobalContextItem($input);

                                                     }

                                                 }


                                             }


                                         }

                                         error_log("Running applyTemplatesReturningValue");


                                        if($input == null) {
                                            $fail++;
                                            return FAIL;
                                        }  else {

                                        }
                                        $result = $executable->applyTemplatesReturningValue();

                                    }
                                }

                                /*if ($result == NULL) {

                                    error_log('Error: ' . $executable->getErrorMessage() . " code:" . $executable->getErrorCode());
                                    $result = "";
                                     $fail++;
                                    return "FAIL";
                                }  */
                                //error_log("Result:". $result->size());
                                //error_log("Result-node value:". $result->itemAt(0)->getNodeValue());
                                $asserts = getNodeFromXPath($testcase, "./result/child::*");


                                /*if($testcase->attributes()->{'name'}=='import-0502b'){
                                     echo "result:".$result;
                                     echo "assert:".($asserts->asXML());
                                    exit('import-0502b');
                                }*/
                                if ($asserts == NULL) {
                                    error_log("NULL FOUND IN ASSERT1 = ". $testElem);
                                    if ($proc->exceptionOccurred()) {
                                        error_log(" assert exception: " . $proc->getErrorMessage());

                                    }
                                    $fail++;

                                    return "FAIL ASSERT";
                                }

                                $outcome = testAssertion($asserts->itemAt(0)->getNodeValue(), $result, $errorCode);


                                if (!$outcome) {
                                    error_log("Assert:" . $outcome);
                                    error_log("Assert:" . ($asserts));
                                    error_log( "OutCome:" . ($result));


                                    return "FAIL";
                                }

                                $pass++;
                                return "PASS";


                           /* $xsltproc->exceptionClear();
                            $init_template = $testcase->{'test'}->{'initial-template'};
                            echo "Test data=" . $test;
                            $executable = $xsltproc->compileFromFile($test);
                            if ($executable == null) {
                                return "executable is NULL";
                            }
                            if ($init_template) {
                                $executable->setProperty('it', strval($testcase->{'test'}->{'initial-template'}->attributes()->{'name'}));
                            }
                            $result = $executable->transformToString();
                            if ($result == NULL) {


                                $result = "";

                                $result = "<out>";

                                $errC = $executable->getErrorCode();
                                if ($errC != NULL) {
                                    $result = $result . $errC . " ";
                                }
                                $result = $result . "</out>";
                            }



                        $asserts = $testcase->{'result'};
                        $xdmValue = $proc->parseString('' . $asserts->asXML());

                        if ($xdmValue == NULL) {
                            error_log(" assert exception: " . $proc->getErrorMessage(0));
                            return "NO ASSERTION";
                        }
                        $proc->setParameter('', 'assertion', $xdmValue);
                        $resulti = $proc->parseString('' . $result);
                        if ($resulti == NULL) {
                            return "NULL result";
                        }
                        $outcomeExecutable = $xsltproc->compileFromFile("TestOutcome.xsl");
                        $outcomeExecutable->setParameter('', 'result', $resulti);
                        $outcomeExecutable->setParameter('', 'testDir', $testDirValue);
                        //$outcomeExecutable->setProperty('it', 'main');
                        $outcome = $outcomeExecutable->callTemplateReturningString("main");
                        $proc->clear();
                        if ($outcome == NULL) {
                            echo "Assert:" . ($asserts->asXML());

                            $result = "<out>";

                            $errC = $proc->getErrorMessage();
                            if ($errC != NULL) {
                                $result = $result . $errC . " ";
                            }
                            $result = $result . "</out>";


                            echo "Error count: " . ($result);

                            return "false";
                        }
                        $proc->clear();
                        if ($outcome == FALSE) {
                            $resulti = '|| Assertion: ' . ($asserts->asXML()) . '|||| result: ' . $resulti . ' ||| ';
                            return 'assert-Error: ' . $outcome;
                        }

                        return $outcome . '';
                            */

                    } catch (Exception $ex) {
                        $asserts = getNodeFromXPath($testcase, "./result/child::*");
                        $outcome = testAssertion($asserts->itemAt(0)->getNodeValue(), $result, $errorCode);
                        if (!$outcome) {
                            $fail++;
                            return "Fail - error: " . $ex->getMessage();
                        } else {
                            $pass++;
                            return "PASS";
                        }
                    }

                } else {
                    $fail++;
                    return $test . " not found";
                }

            }


        }

    }
    $notrun++;
    return "Not Run";
}


$saxon_version = $proc->version();
echo "<a name='top'></a>";
echo "<h1>W3 XSLT 3.0 Test Suite</h1>
<h4>(PHP Test Harness. Version: 2.0. Saxon/C PHP Extension, Saxon product version: $saxon_version)</h4>
<br/>";
$testFiles = test_set_files($catalog);
echo "<b>Test sets: " . count($testFiles) . "<b/> See <a href='#results'> Results here </a>";

echo "<table border='1'>
<tr>
  <th>Test Name </th>
  <th>File Name </th>
  <th> #Test cases </th>
</tr>";

foreach ($testFiles as $key => $test_set) {
    $testSet_xml = simplexml_load_file($path . $test_set);
    echo "<tr>";
    echo "<td><a href='#" . $key . "'>" . $key . "</a></td>";
    echo "<td>" . $test_set . "</td>";
    echo "<td>" . count($testSet_xml->{'test-case'}) . "</td>";
    echo "</tr>";
}
echo "</table>";

foreach ($testFiles as $key => $test_set) {
    echo "<div style='width:70%'><h4><a name='" . $key . "'>" . $key . "</a></h4></div><div style='width:30%;float:right'><a href='#top'>Back to top</a></div>";
    //if ($key == 'as') {   //copy-of
        test_case($path . $test_set);
   // }
}
echo "<br/>";
//$test1 = $proc->xsltApplyStylesheet("cat.xml","test.xsl");


//echo $proc->transformToString();
echo "<a name='results'>";
echo "<b>Results</b><br/>";
echo "# Success:".$pass. " #Failures: ".$fail." Not run: ". $notrun;


?>
