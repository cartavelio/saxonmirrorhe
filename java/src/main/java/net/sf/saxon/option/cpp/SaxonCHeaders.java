// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import org.graalvm.nativeimage.c.CContext;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.stream.Collectors;

public final class SaxonCHeaders implements CContext.Directives {
    private List<String> headerFileNames = List.of("saxon_error.h");

    @Override
    public List<String> getHeaderFiles() {
        return headerFileNames.stream().map(name -> "\"" + name + "\"").toList();
    }

    @Override
    public List<String> getOptions() {
        Path projectIncludePath = Paths.get(System.getProperty("libsaxonc-core.include_path"));
        Path cmakeIncludePath = Paths.get(System.getProperty("libsaxonc-core.cmake_include_path"));

        List<Path> fullPaths = List.of(projectIncludePath, cmakeIncludePath);
        for (String headerFileName : headerFileNames) {
            boolean found = false;
            for (Path path : fullPaths) {
                if(Files.exists(path.resolve(headerFileName))) {
                    found = true;
                }
            }
            if (!found) {
                String searchedPaths = String.join(", ", fullPaths.stream().map(p -> p.toString()).toList());
                throw new RuntimeException(headerFileName + " does not exist in " + searchedPaths);
            }
        }
        return fullPaths.stream().map( includePath -> "-I" + includePath).toList();
    }
}