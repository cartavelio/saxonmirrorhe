package net.sf.saxon.option.cpp;

import java.util.List;
import org.graalvm.nativeimage.c.CContext;

public final class SaxonCHeaders implements CContext.Directives {
    @Override
    public List<String> getHeaderFiles() {
        return List.of("\"saxon_error.h\"");
    }

    @Override
    public List<String> getOptions() {
        //TODO This path is currently hard coded we need to change this in the build to be relative
        return List.of(
                "-I/Users/ond1/work/development/git/saxon-dev2/saxondev/libsaxonc/src/include/saxonc/"
        );
    }
}