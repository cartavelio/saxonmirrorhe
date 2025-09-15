package com.saxonica.build.cpp

import SaxonBuildExtension
import SaxonCCoreExtension
import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.tasks.TaskAction

abstract class ReleaseTask: DefaultTask() {
  @TaskAction
  fun makeRelease() {
    val saxonC = project.extensions.findByType(SaxonCCoreExtension::class.java)
    if (saxonC !is SaxonCCoreExtension) {
      throw GradleException("Could not find 'saxonc' extension")
    }

    val saxonBuild = project.extensions.findByType(SaxonBuildExtension::class.java)
    if (saxonBuild !is SaxonBuildExtension) {
      throw GradleException("Could not find 'saxonbuild' extension")
    }

    val platform = saxonBuild.systemOsName.get()
    val arch = saxonBuild.systemOsArch.get()
    val version = saxonBuild.version.get()

    val edition = saxonC.edition.get()
    val releaseName = "libsaxon-${edition.uppercase()}C-${platform}-${arch}-v${version}"
    val releaseDir = project.layout.buildDirectory.dir("releases/${releaseName}")

    val layout = project.layout
    val configMap = mapOf("mac" to mapOf("libdir" to "libs/darwin",
                                         "libpath" to "../libs/darwin",
                                         "libname" to "libsaxon-${edition}c-*.dylib"),
                          "windows" to mapOf("libdir" to "libs/win",
                                             "libpath" to "..\\\\libs\\\\win",
                                             "libname" to "libsaxon-${edition}c-*.*"),
                          "linux" to mapOf("libdir" to "libs/nix",
                                           "libpath" to "../libs/nix",
                                           "libname" to "libsaxon-${edition}c-*.so"))

    val libdir = configMap[saxonBuild.systemOsName.get()]!!.get("libdir")!!
    //val libpath = configMap[saxonBuild.systemOsName.get()]!!.get("libpath")!!
    val libname = configMap[saxonBuild.systemOsName.get()]!!.get("libname")!!

    project.copy {
      from(layout.buildDirectory.dir("c"))
      into(layout.buildDirectory.dir("releases/${releaseName}/${libdir}"))
      include(libname)
    }

    project.copy {
      from(layout.projectDirectory.dir("../src/main/c"))
      into(releaseDir)
      include("README.md")
      filter { line ->
        line.replace("@@VERSION", saxonBuild.version.get())
            .replace("@@edition@@", edition)
            .replace("@@EDITION@@", edition.uppercase())
      }
    }

    project.copy {
      from(layout.buildDirectory.dir("c"))
      into(layout.buildDirectory.dir("releases/${releaseName}/Saxon.C.API/graalvm"))
      include("graal*.h")
      include("libsaxon-${edition}c*.h")
    }

    project.copy {
      from(layout.projectDirectory.dir("../src/main/c/pypi"))
      into(layout.buildDirectory.dir("releases/${releaseName}/pypi"))
      filter { line ->
        line.replace("@@VERSION@@", saxonBuild.version.get())
            .replace("@@edition@@", edition)
            .replace("@@EDITION@@", edition.uppercase())
      }
    }

    project.copy {
      from(layout.projectDirectory.dir("../src/main/c/samples"))
      into(layout.buildDirectory.dir("releases/${releaseName}/samples"))
      filter { line ->
        line.replace("@@VERSION@@", saxonBuild.version.get())
            .replace("@@edition@@", edition)
            .replace("@@EDITION@@", edition.uppercase())
      }
    }

    project.copy {
      from(layout.projectDirectory.dir("../src/main/c/command"))
      into(layout.buildDirectory.dir("releases/${releaseName}/command"))
      if (edition != "ee") {
        exclude("Validate.c")
      }
      filter { line ->
        line.replace("@@VERSION@@", saxonBuild.version.get())
            .replace("@@edition@@", edition)
            .replace("@@EDITION@@", edition.uppercase())
            .replace("@@libdir@@", libdir)
      }
    }

    project.copy {
      from(layout.projectDirectory.dir("../src/main/c/Saxon.C.API"))
      into(layout.buildDirectory.dir("releases/${releaseName}/Saxon.C.API"))
      include("SaxonProcessor.cpp")
      include("SaxonCGlue.c")
      include("SaxonCProcessor.c")
      include("SaxonCXPath.c")
      include("XdmValue.cpp")
      include("XdmItem.cpp")
      include("XdmNode.cpp")
      include("XdmAtomicValue.cpp")
      include("XdmFunctionItem.cpp")
      include("XdmMap.cpp")
      include("XdmArray.cpp")
      include("DocumentBuilder.cpp")
      include("Xslt30Processor.cpp")
      include("XsltExecutable.cpp")
      include("XQueryProcessor.cpp")
      include("XPathProcessor.cpp")
      include("SchemaValidator.cpp")
      include("SaxonApiException.cpp")
      include("CythonExceptionHandler.cpp")
      include("SaxonProcessor.h")
      include("SaxonCGlue.h")
      include("SaxonCProcessor.h")
      include("SaxonCXPath.h")
      include("XdmValue.h")
      include("XdmItem.h")
      include("XdmNode.h")
      include("XdmAtomicValue.h")
      include("XdmFunctionItem.h")
      include("XdmMap.h")
      include("XdmArray.h")
      include("DocumentBuilder.h")
      include("Xslt30Processor.h")
      include("XsltExecutable.h")
      include("XQueryProcessor.h")
      include("XPathProcessor.h")
      include("CythonExceptionHandler.h")
      include("SchemaValidator.h")
      include("SaxonApiException.h")
      include("php8_DocumentBuilder.cpp")
      include("php8_Xslt30Processor.cpp")
      include("php8_saxon_arginfo.h")
      include("php8_SchemaValidator.cpp")
      include("php8_XsltExecutable.cpp")
      include("php8_saxon.cpp")
      include("php8_XPathProcessor.cpp")
      include("php8_saxon.h")
      include("php8_XQueryProcessor.cpp")
      include("config.m4")
      include("config.w32")
      filter { line ->
        line.replace("@@VERSION@@", saxonBuild.version.get())
            .replace("@@edition@@", edition)
            .replace("@@EDITION@@", edition.uppercase())
      }
    }

    project.copy {
      from(layout.projectDirectory.dir("../../src/docs/notices"))
      into(layout.buildDirectory.dir("releases/${releaseName}/notices"))
      include("JAMESCLARK.txt")
      include("JLINE2.txt")
      include("LICENSE.txt")
    }
  }
}
