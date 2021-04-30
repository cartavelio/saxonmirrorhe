<?php
/*error_reporting(E_ALL|E_STRICT);
ini_set('display_errors', 'on');*/
ini_set('max_execution_time', 300);
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


//$path = "/media/ond1/c8f4c380-ed5d-4791-b60d-90df7fd39798/work/git/xslt30-test/";
$path = __DIR__ . "/xslt30-test/";
$catalog = $path . "catalog.xml";
$saxon_version = "";

$proc = new Saxon\SaxonProcessor();
$proc->setcwd("");
$xsltproc = $proc->newXslt30Processor();
$xpathProcForEnv = $proc->newXPathProcessor();

/*$start = microtime(true);
              $proc->setSourceFile("cat.xml");
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
        $xmlV = $proc->parseXmlFromFile($catalog);//simplexml_load_file($catalog);
        $xpathProcForEnv->setContextItem($xmlV);
        //$xpathProcForEnv->setContextFile($catalog);
		error_log("checkpoint 0-1");
        $xml = $xpathProcForEnv->evaluate("//*:test-set");
		error_log("checkpoint 0-2");
        if ($xml != NULL) {
			error_log("checkpoint 0-3");
            //foreach ($xml->{'test-set'} as $value) {
            for ($i = 1; $i <= $xml->size(); $i++) {
                $value = $xml->itemAt($i);
//	echo "________________".$value->attributes()->name."_________".$value->attributes()->file;
                echo "itemAt";
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
    foreach ($xml->children() as $child) {
        $var1 = $child->getName();
        switch ($var1) {
            case "feature":
                return false;
            case "spec":
                if (strpos($child->attributes()->{'value'}, "XSLT30") !== false) {
                    return false;
                }
        }
    }
    return true;
}

function parse_environment($xml, $path)
{
    $array = array();
    foreach ($xml->{'environment'} as $value) {
        $envName = $value->attributes()->name;
        $source = $value->{'source'}[1];
        if ($source->attributes()->file and !($source->attributes()->validation)) {
            //  echo $path.$source->attributes()->file." ";
            $array["" . $envName] = $path . $source->attributes()->file;
        }
    }
    echo "<br/>";
    echo "<br/>";
    return $array;
}

function test_case($test_set)
{
    $env;
    $xml;
	error_log("checkpoint 3");
    $test_path = pathinfo($test_set)['dirname'] . "/";
    echo $test_path. "\n";
    return;
    if (file_exists($test_set)) {
        $xml = simplexml_load_file($test_set);
        $env = parse_environment($xml, $test_path);
		error_log("checkpoint 4");
        echo "<table border='1' style='width:60%'>
<tr>
  <th>Test Case Name </th>
  <th style='width:80%'>Result </th>
</tr>";

        $check = check_dependency($xml->dependencies);
        if ($check) {
            foreach ($xml->{'test-case'} as $value) {
                $testCaseName = $value->attributes()->name;
                echo "<tr><td>$testCaseName</td>";
                $result = "test";//run_test_case($value, $env, $test_path);
                echo "<td>" . $result . "</td>";
                echo "</tr>";
                //$test = $value->{'test'}->{'stylesheet'};
                //echo "____test:".$test_path."tests".$test->attributes()->file;
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
        //echo $test;
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

                            echo 'Error: ' . $executable->getErrorMessage() . " code:" . $proc->getErrorCode();
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
                    $proc->exceptionClear();
                    $init_template = $testcase->{'test'}->{'initial-template'};
                    if ($init_template) {
                        $proc->setProperty('it', strval($testcase->{'test'}->{'initial-template'}->attributes()->{'name'}));
                    }
                    $result = $proc->transformToString($test);
                    if ($result == NULL) {

                        $errCount = $proc->getExceptionCount();
                        $result = "";
                        if ($errCount > 0) {
                            $result = "<out>";
                            for ($i = 0; $i < $errCount; $i++) {
                                $errC = $proc->getErrorCode(intval(i));
                                if ($errC != NULL) {
                                    $result = $result . $errC . " ";
                                }
                                $result = $result . "</out>";
                            }
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
                    $proc->setParameter('', 'result', $resulti);
                    $proc->setParameter('', 'testDir', $testDirValue);
                    $proc->setProperty('it', 'main');
                    $outcome = $proc->transformToString("TestOutcome.xsl");
                    $proc->clear();
                    if ($outcome == NULL) {
                        echo "Assert:" . ($asserts->asXML());
                        $errCount = $proc->getExceptionCount();
                        if ($errCount > 0) {
                            $result = "<out>";
                            for ($i = 0; $i < $errCount; $i++) {
                                $errC = $proc->getErrorMessage(intval(i));
                                if ($errC != NULL) {
                                    $result = $result . $errC . " ";
                                }
                                $result = $result . "</out>";
                            }

                            echo "Error count: " . $errCount . ", " . ($result);
                        }
                        return "false";
                    }
                    $proc->clear();
                    if ($outcome == FALSE) {
                        $resulti = '|| Assertion: ' . ($asserts->asXML()) . '|||| result: ' . $resulti . ' ||| ';
                        return 'assert-Error: ' . $outcome;
                    }

                    return $outcome . '';

                }
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
    //test_case("xslt30-test/" . $test_set);
}
//}
echo "<br/>";
//$test1 = $proc->xsltApplyStylesheet("cat.xml","test.xsl");


//echo $proc->transformToString();
echo "This link is using Html: ";


?>
