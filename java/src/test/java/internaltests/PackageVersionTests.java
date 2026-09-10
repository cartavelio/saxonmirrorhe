package internaltests;

import net.sf.saxon.style.PackageVersion;
import net.sf.saxon.style.PackageVersionRanges;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test handling of XSLT package version range matches
 */
public class PackageVersionTests {

    private void testPackageVersion(String range, String version, boolean matches) {
        try {
            PackageVersionRanges ranges = new PackageVersionRanges(range);
            PackageVersion v = new PackageVersion(version);
            boolean match = ranges.contains(v);
            assertEquals(matches, match);
        } catch (XPathException e) {
            fail(e);
        }
    }

    private void testIncorrectPackageVersion(String range) {
        try {
            PackageVersionRanges ranges = new PackageVersionRanges(range);
            fail("undetected error");
        } catch (XPathException e) {
            // OK
        }
    }

    @Test
    public void testAny() {
        testPackageVersion("*", "12", true);
        testPackageVersion("*", "13-beta", true);
        testPackageVersion("*", "12.2.3", true);
    }

    @Test
    public void testAnyErrors() {
        testIncorrectPackageVersion("*, 1 to 10");
        testIncorrectPackageVersion("1 to 10, *");
        testIncorrectPackageVersion("1 to *");
    }

    @Test
    public void testSimple() {
        testPackageVersion("12", "12", true);
        testPackageVersion("12-beta", "12-beta", true);
        testPackageVersion("12", "13", false);
        testPackageVersion("12-beta", "12-alpha", false);
        testPackageVersion("12.2.3", "12.2.3", true);
        testPackageVersion("12.2.3", "12.2.4", false);
    }

    @Test
    public void testSimpleList() {
        testPackageVersion("12, 13, 14", "12", true);
        testPackageVersion("12, 14, 16", "13", false);
        testPackageVersion("12, 14.2-beta, 16", "14.2-beta", true);
        testPackageVersion("12.2.3 ,12.02.04, 12.2.6", "12.2.4", true);
        testPackageVersion("12.2.3 ,12.2.4, 12.2.6", "12.2.5", false);
    }

    @Test
    public void testSimpleRange() {
        testPackageVersion("12 to 14", "13", true);
        testPackageVersion("12 to 14", "15", false);
        testPackageVersion("12 to 14", "12", true);
        testPackageVersion("12 to 14", "14", true);
        testPackageVersion("12 to 14", "11", false);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.4", true);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.3", true);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.6", true);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.7", false);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.4-beta", true);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.6-beta", true);
        testPackageVersion("12.2.3 to 12.2.6", "12.2.3-beta", false);
    }

    @Test
    public void testRangeTo() {
        testPackageVersion("to 14", "13", true);
        testPackageVersion("to 14", "15", false);
        testPackageVersion("to 14", "12", true);
        testPackageVersion("to 14", "14", true);
        testPackageVersion("to 14", "11", true);
        testPackageVersion("to 14-beta", "14-beta", true);
        testPackageVersion("to 14-beta", "14", false);
        testPackageVersion("to 14-beta", "14-alpha", true);
        testPackageVersion("to 12.2.6", "11.1", true);
        testPackageVersion("to 12.2.6", "12.2.4", true);
        testPackageVersion("to 12.2.6", "12.2.3", true);
        testPackageVersion("to 12.2.6", "12.2.6", true);
        testPackageVersion("to 12.2.6", "12.2.7", false);
        testPackageVersion("to 12.2.6", "12.2.4-beta", true);
        testPackageVersion("to 12.2.6", "12.2.6-beta", true);
        testPackageVersion("to 12.2.6", "12.2.3-beta", true);
    }

    @Test
    public void testRangeFrom() {
        testPackageVersion("14+", "13", false);
        testPackageVersion("14+", "15", true);
        testPackageVersion("14+", "14", true);
        testPackageVersion("14+", "14.2", true);
        testPackageVersion("14+", "14-alpha", false);
        testPackageVersion("14+", "15-alpha", true);
        testPackageVersion("14-alpha+", "15-alpha", true);
        testPackageVersion("14-alpha+", "14", true);
        testPackageVersion("14.3+", "13", false);
        testPackageVersion("14.3+", "15", true);
        testPackageVersion("14.3+", "14.3", true);
        testPackageVersion("14.3+", "14.4", true);
        testPackageVersion("14.3+", "14.2", false);
        testPackageVersion("14.3+", "14.3-alpha", false);
        testPackageVersion("14.3+", "14.3.1-alpha", true);
    }

    @Test
    public void testPrefix() {
        testPackageVersion("12.*", "12", true);
        testPackageVersion("12.*", "12.1", true);
        testPackageVersion("12.*", "12.2.1", true);
        testPackageVersion("12.2.*", "12.2.1", true);
        testPackageVersion("12.2.*", "12.2.8.1", true);
        testPackageVersion("12.*", "13", false);
        testPackageVersion("12.*", "11.1", false);
        testPackageVersion("12.*", "12-alpha", true);
        testPackageVersion("12.2.*", "12-alpha", false);
    }

    @Test
    public void testToPrefix() {     // bug 6746
        testPackageVersion("to 12.*", "12", true);
        testPackageVersion("to 12.*", "12.1", true);
        testPackageVersion("to 12.*", "12.2.1", true);
        testPackageVersion("to 12.2.*", "12.2.1", true);
        testPackageVersion("to 12.2.*", "12.2.8.1", true);
        testPackageVersion("to 12.*", "13", false);
        testPackageVersion("to 12.*", "11.1", true);
        testPackageVersion("to 12.*", "12-alpha", true);
        testPackageVersion("to 12.2.*", "12-alpha", true);
    }

    @Test
    public void testFromToPrefix() {   // bug 6746
        testPackageVersion("11 to 12.*", "12", true);
        testPackageVersion("11 to 12.*", "12.1", true);
        testPackageVersion("11 to 12.*", "12.2.1", true);
        testPackageVersion("11 to 12.2.*", "12.2.1", true);
        testPackageVersion("11 to 12.2.*", "12.2.8.1", true);
        testPackageVersion("11 to 12.*", "13", false);
        testPackageVersion("11.3 to 12.*", "11.1", false);
        testPackageVersion("11.3 to 12.*", "12-alpha", true);
        testPackageVersion("11.3 to 12.2.*", "12-alpha", true);
    }

    @Test
    public void testComplexRanges() {
        testPackageVersion("11 to 12.*, 15 to 16.*", "12", true);
        testPackageVersion("11 to 12.*, 15 to 16.*", "14", false);
    }

}

