package commands;

import jaxptest.ConfigTest;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.io.File;

/**
 * Tests invocation of Saxon via Ant
 */
public class AntTests {
    private final ConfigTest configTest = new ConfigTest();
    
    public static void main(String[] args) {
        org.apache.tools.ant.launch.Launcher.main(args);
    }

    @BeforeAll
    public static void setUp() {
        System.err.println("These tests require ant.jar and ant-launcher.jar to be on the classpath");
    }

    // These tests require ant.jar and ant-launcher.jar to be on the classpath

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

    @Test
    public void testWalmsleyBug() {
        runAnt(configTest.getDataDir() + "ant/walmsley/saxonbug/build.xml");
    }

//    public void testXmlCatalog() {
    // removed because it depends too much on the machine setup, e.g CatalogManager.properties
//        runAnt(configTest.getDataDir() + "ant/buildWithCatalog.xml");
//    }
}

