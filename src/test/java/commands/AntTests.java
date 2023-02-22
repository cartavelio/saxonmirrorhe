package commands;

import jaxptest.ConfigTest;
/*
import org.apache.tools.ant.Project;
import org.apache.tools.ant.ProjectHelper;

 */
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.io.File;

/**
 * Tests invocation of Saxon via Ant
 */
public class AntTests {

    @BeforeAll
    public static void setUp() {
        System.err.println("These tests require ant.jar and ant-launcher.jar to be on the classpath");
    }

    // These tests require ant.jar and ant-launcher.jar to be on the classpath

    @Test
    public void testTransformTask() {
        runAnt(ConfigTest.DATA_DIR + "ant/build1.xml");
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

    @Test
    public void testWalmsleyBug() {
        runAnt(ConfigTest.DATA_DIR + "ant/walmsley/saxonbug/build.xml");
    }

//    public void testXmlCatalog() {
    // removed because it depends too much on the machine setup, e.g CatalogManager.properties
//        runAnt(ConfigTest.DATA_DIR + "ant/buildWithCatalog.xml");
//    }
}

