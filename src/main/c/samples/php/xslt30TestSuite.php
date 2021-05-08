<?php
/*error_reporting(E_ALL|E_STRICT);
ini_set('display_errors', 'on');*/
/*ini_set('max_execution_time', 300);
$root = pathinfo($_SERVER['SCRIPT_FILENAME']);
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

$proc = new Saxon\SaxonProcessor();
$proc->setcwd("");
$xsltproc = $proc->newXslt30Processor();

$xpathProcForEnv = $proc->newXPathProcessor();
$xpathProcForEnv->declareNamespace("", "http://www.w3.org/2012/10/xslt-test-catalog");

/*$start = microtime(true);
              $proc->setSourceFile("cat.xml");                                              d
                $proc->importStylesheetFile("test.xsl");
                $result = $proc->transformToValue();
 $end = microtime(true); */


function test_set_files($catalog)
{
    global $proc;
    global $xpathProcForEnv;
    $array = array();
    $attName;
    $attFile;
    error_log("checkpoint 0, ".$catalog);
    if (file_exists($catalog)) {
        //$xmlV = $proc->parseXmlFromFile($catalog);//simplexml_load_file($catalog);
        //$xpathProcForEnv->setContextItem($xmlV);
        $xpathProcForEnv->setContextFile($catalog);

        $xml = $xpathProcForEnv->evaluate("//*:test-set");

        if ($xml != NULL) {
			error_log("checkpoint 0-3");
            //foreach ($xml->{'test-set'} as $value) {
            for ($i = 0; $i < $xml->size(); $i++) {
                $value = $xml->itemAt($i);
//	echo "________________".$value->attributes()->name."_________".$value->attributes()->file;
                
                if ($value != NULL && $value->isNode()) {
                    $node = $value->getNodeValue();

                    if ($node != NULL) {
                        error_log("item is node");
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
    $count = $xml->getChildCount();
    for ($i = 0; $i < $count; $i++){
        $child = $xml->getChildNode($i);
        if($child->getNodeKind() == 1) {
            $var1 = $child->getNodeName();
            switch ($var1) {
                case "feature":
                    return false;
                case "spec":
                    if (strpos($child->attributes()->{'value'}, "XSLT30") !== false) {
                        return false;
                    }
            }
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
    $envValues = $xpathProcForEnv->evaluate("//environment");

    if($envValues == null) {
        return $array;
    }

    $count = $envValues->size();
    for ($i = 0; $i < $count; $i++) {
        $value = $envValues->itemAt($i);
        if( $value->isNode()) {
            $node = $value->getNodeValue();
            if ($node == null) {
                echo "environment node is null";
                continue;


            }

            $envName = $node->getAttributeValue("name");
            $array["" . $envName] = array();
            $xpathProcForEnv->clearParameters();
            $xpathProcForEnv->clearProperties();
            $xpathProcForEnv->setContextItem($node);
            $sources = $xpathProcForEnv->evaluate("//*:source");
            $scount = $sources->size();
            $array["" . $envName]["sources"] = array();
            for ($j = 0; $j < $count; $j++) {
                $valuej = $sources->itemAt($j);
                if ($valuej == null) {

                    continue;

                }
                if($valuej->isNode()) {
                    $nodej = $valuej->getNodeValue();

                    $role = $nodej->getAttributeValue("uri");
                    $rawuri = $nodej->getAttributeValue("role");
                    if ($rawuri != null) {
                        $array["" . $envName]["sources"][$rawuri] = $path . $rawuri;

                    } elseif ("." == $role) {
                        $href = $nodej->getAttributeValue("file");
                        if ($href == null) {
                            $xpathProcForEnv->clearParameters();
                            $xpathProcForEnv->clearProperties();
                            $xpathProcForEnv->setContextItem($nodej);
                            $sourceContent = $xpathProcForEnv->evaluate("string(content)");
                            $array["" . $envName]["sources"][".content"] = $sourceContent->getStringValue();


                        } else {
                            $array["" . $envName]["sources"][".file"] = $path . $href;

                        }

                    }

                }
            }
        }
        
    }
    echo "<br/>";
    echo "<br/>";
    return $array;
}

function test_case($test_set)
{
    global $proc;
    global $xpathProcForEnv;
    $env = null;
    $xml;
    $test_path = pathinfo($test_set)['dirname'] . "/";
    echo "Check:    ".$test_set. "\n";

    if (file_exists($test_set)) {
        $xml = $proc->parseXmlFromFile($test_set);
        printf("checkpoint 0");
        if($xml == null) {
            echo "Cannot find test ". $test_set;
            return;
        }  else {
            printf("checkpoint 1");
            $env = parse_environment($xml, $test_path);
        }
        printf("checkpoint 2");
		error_log("checkpoint 4");
        echo "<table border='1' style='width:60%'>
<tr>
  <th>Test Case Name </th>
  <th style='width:80%'>Result </th>
</tr>";

        $check = check_dependency($xml);
        $testCases = $xpathProcForEnv->evaluate("//test-case");
        if ($check or $testCases != null) {


            $countTC = $testCases->size();
            for ($x = 0; $x < $countTC; $x++) {
                $value = $testCases->itemAt($x);
                if($value->isNode()) {
                    $nodex = $value->getNodeValue();
                    $testCaseName = $nodex->getAttributeValue("name");
                    echo "<tr><td>$testCaseName</td>";
                    $result = run_test_case($nodex, $env, $test_path);
                    echo "<td>" . var_dump($result) . "<br/></td>";
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

function run_test_case($testcase, $env, $path)
{
    global $proc;
    global $xsltproc;
    $errorTest = FALSE;
    $t_env = $testcase->{'environment'};
    $testDirValue = $proc->createAtomicValue(strval($path));
    $t_env_ref = $t_env->attributes()->ref;
    $checkDependency = check_dependency($testcase->dependencies);
    if ($checkDependency) {

        $test = $path . $testcase->{'test'}->{'stylesheet'}->attributes()->file;
        echo "Test case=".$test;
        if (file_exists($test)) {
            try {
                if ($t_env) {
                    echo "t_env" . $t_env . "\n    ";
                    print_r($env);
                    if ($env[$t_env_ref . ""]) {
                        $result = NULL;
                        //error_log("before apply source:".$env[$t_env_ref.""]);
                        //error_log("before apply stylesheet:".$test);
                        $executable = $xsltproc->compileFromFile($test);
                        $init_template = $testcase->{'test'}->{'initial-template'};
                        if ($executable == NULL) {
                            error_log("Executable is null on the following file: " . $test);
                            $result = NULL;
                        } else {

                            if ($init_template) {
                                //$proc->setProperty('it', strval($testcase->{'test'}->{'initial-template'}->attributes()->{'name'}));

                                $executable->setInitialMatchSelectionAsFile($env[$t_env_ref . ""]);

                                $result = $executable->callTemplateReturningString(strval($testcase->{'test'}->{'initial-template'}->attributes()->{'name'}));
                            } else {
                                $result = $executable->applyTemplatesReturningString($env[$t_env_ref . ""], $test);
                            }
                        }

                        if ($result == NULL) {

                            echo 'Error: ' . $executable->getErrorMessage() . " code:" . $executable->getErrorCode();
                            $result = "";


                        }

                        $asserts = $testcase->{'result'};
                        $xdmValue = $proc->parseXmlFromString(strval($asserts->asXML()));
                        /*if($testcase->attributes()->{'name'}=='import-0502b'){
                             echo "result:".$result;
                             echo "assert:".($asserts->asXML());
                            exit('import-0502b');
                        }*/
                        if ($xdmValue == NULL) {
                            error_log("NULL FOUND IN ASSERT");
                            if ($proc->exceptionOccurred()) {
                                error_log(" assert exception: " . $proc->getErrorMessage());
                                return "NO ASSERTION" . ($asserts->asXML());
                            }

                        }

                        error_log("before result : ");
                        $resulti = $proc->parseXmlFromString('' . $result);
                        if ($resulti == NULL) {
                            error_log(" assert exception: " . $proc->getErrorMessage());
                            return "NULL result";
                        }

                        $outcomeExecutable = $xsltproc->compileFromFile("TestOutcome.xsl");
                        $outcomeExecutable->setParameter('assertion', $xdmValue);
                        $outcomeExecutable->setParameter('result', $resulti);
                        $outcomeExecutable->setParameter('testDir', $testDirValue);
                        $outcome = $outcomeExecutable->callTemplateReturningString('main');

                        if ($outcome == NULL) {
                            error_log("Assert:" . ($asserts->asXML()));
                            echo "Assert:" . ($asserts->asXML());
                            echo "Assert:" . ($result);

                            error_log("Error Message" . $outcomeExecutable->getErrorMessage());
                            echo "<out>$outcomeExecutable->getErrorMessage()</out>";

                            return "false";
                        }
                        if ($errorTest) {
                            return 'assert-Error: ' . $outcome;
                        }
                        $proc->clear();
                        return $outcome;
                    } else {
                        return $t_env_ref . " not found";
                    }
                } else {
                    $xsltproc->exceptionClear();
                    $init_template = $testcase->{'test'}->{'initial-template'};
                    echo "Test data=".$test;
                    $executable = $xsltproc->compileFromFile($test);
                    if($executable == null) {
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



                    }
                    $asserts = $testcase->{'result'};
                    $xdmValue = $proc->parseString('' . $asserts->asXML());
                    /*if($testcase->attributes()->{'name'}=='import-0502b'){
                         echo "result:".$result;
                         echo "assert:".($asserts->asXML());
                        exit('import-0502b');
                    }*/
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


                            echo "Error count: "  . ($result);

                        return "false";
                    }
                    $proc->clear();
                    if ($outcome == FALSE) {
                        $resulti = '|| Assertion: ' . ($asserts->asXML()) . '|||| result: ' . $resulti . ' ||| ';
                        return 'assert-Error: ' . $outcome;
                    }

                    return $outcome . '';

                
            } catch (Exception $ex) {
                return "Fail - error: " . $ex->getMessage();
            }
            return "Fail";
        } else {
            return $test . " not found";
        }

    }

    return "Not Run";
}


$saxon_version = $proc->version();
echo "<a name='top'></a>";
echo "<h1>W3 XSLT 3.0 Test Suite</h1>
<h4>(PHP Test Harness. Version: 2.0. Saxon/C PHP Extension, Saxon product version: $saxon_version)</h4>
<br/>";
$testFiles = test_set_files($catalog);
echo "<b>Test sets: " . count($testFiles) . "<b/>";
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
//if($key == 'copy-of'){
    test_case($path. $test_set);
}
//}
echo "<br/>";
//$test1 = $proc->xsltApplyStylesheet("cat.xml","test.xsl");


//echo $proc->transformToString();
echo "This link is using Html: ";


?>
