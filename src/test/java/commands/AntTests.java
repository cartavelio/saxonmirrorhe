package commands;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import org.apache.tools.ant.Project;
import org.apache.tools.ant.ProjectHelper;

import java.io.File;

/**
 * Tests invocation of Saxon via Ant
 */
public class AntTests extends TestCase {

    public void testTransformTask() {
        runAnt(ConfigTest.DATA_DIR + "ant/build1.xml");
    }

    //public void testValidateTask() {
//        runAnt("ant/build-xmlvalidate.xml");
//    }


    private void runAnt(String file) {
        File buildFile = new File(file);
        Project p = new Project();
        p.setUserProperty("ant.file", buildFile.getAbsolutePath());
        p.init();
        ProjectHelper helper = ProjectHelper.getProjectHelper();
        p.addReference("ant.projectHelper", helper);
        helper.parse(p, buildFile);
        p.executeTarget(p.getDefaultTarget());

    }

    public void testWalmsleyBug() {
        runAnt("/Users/mike/bugs/2015/Walmsley/saxonbug/build.xml");
    }

//    public void testXmlCatalog() {
    // removed because it depends too much on the machine setup, e.g CatalogManager.properties
//        runAnt(ConfigTest.DATA_DIR + "ant/buildWithCatalog.xml");
//    }
}

