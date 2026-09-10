package net.sf.saxon.test;

import net.sf.saxon.s9api.Location;

public class NullLocation implements Location {
    @Override
    public String getSystemId() {
        return null;
    }
    @Override
    public String getPublicId() {
        return null;
    }
    @Override
    public int getLineNumber() {
        return -1;
    }
    @Override
    public int getColumnNumber() {
        return -1;
    }
    @Override
    public Location saveLocation() {
        return this;
    }
}
