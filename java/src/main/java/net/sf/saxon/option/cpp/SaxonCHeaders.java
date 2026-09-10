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