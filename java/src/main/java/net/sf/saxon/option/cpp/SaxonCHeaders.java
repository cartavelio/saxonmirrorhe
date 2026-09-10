package net.sf.saxon.option.cpp;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;

import org.graalvm.nativeimage.c.CContext;

public final class SaxonCHeaders implements CContext.Directives {
    private List<String> headerFileNames = List.of("saxonc/saxon_error.h");

    @Override
    public List<String> getHeaderFiles() {
        return headerFileNames.stream().map(name -> "\"" + name + "\"").toList();
    }

    @Override
    public List<String> getOptions() {
        //TODO This path is currently hard coded we need to change this in the build to be relative
        String include_path_str = System.getProperty("libsaxonc.include_path");
        Path include_path = Paths.get(include_path_str);
        for (String headerFileName : headerFileNames) {
            if (!Files.exists(include_path.resolve(headerFileName))) {
                throw new RuntimeException(headerFileName + " does not exist in " + include_path_str);
            }
        }
        return List.of(
                "-I" + include_path
        );
    }
}