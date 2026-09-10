package net.sf.saxon.lib;

public class FeatureConfig {
    public Boolean configuredThroughFile;
    public String section;
    public String attribute;

    public FeatureConfig() {
        this.configuredThroughFile = false;
        this.section = null;
        this.attribute = null;
    }

    public FeatureConfig(String section, String attribute) {
        this.configuredThroughFile = true;
        this.section = section;
        this.attribute = attribute;
    }
}
