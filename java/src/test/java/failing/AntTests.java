package failing;

import jaxptest.ConfigTest;
import org.junit.jupiter.api.Test;

import java.io.File;

/*
import org.apache.tools.ant.Project;
import org.apache.tools.ant.ProjectHelper;

 */

/**
 * Tests invocation of Saxon via Ant.
 * These tests are currently inactive. They have been used in the past to investigate Saxon problems
 * that could only be reproduced when Saxon was invoked from Ant.
 */
public class AntTests {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testTransformTask() {
        runAnt(configTest.getDataDir() + "ant/build1.xml");
    }

    //public void testValidateTask() {
//        runAnt("ant/build-xmlvalidate.xml");
//    }


    private void runAnt(String file) {
        File buildFile = new File(file);
        /*
        Project p = new Project();
        p.setUserProperty("ant.file", buildFile.getAbsolutePath());
        p.init();
        ProjectHelper helper = ProjectHelper.getProjectHelper();
        p.addReference("ant.projectHelper", helper);
        helper.parse(p, buildFile);
        p.executeTarget(p.getDefaultTarget());

         */

    }


//    public void testWalmsleyBug() {
//        runAnt("/Users/mike/bugs/2015/Walmsley/saxonbug/build.xml");
//    }

//    public void testXmlCatalog() {
    // removed because it depends too much on the machine setup, e.g CatalogManager.properties
//        runAnt(configTest.getDataDir() + "ant/buildWithCatalog.xml");
//    }
}

