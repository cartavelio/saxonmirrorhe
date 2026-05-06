package com.saxonica.build.cpp

import org.apache.tools.ant.filters.ReplaceTokens
import org.gradle.api.Action
import org.gradle.api.DefaultTask
import org.gradle.api.file.CopySpec
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileSystemOperations
import org.gradle.api.file.ProjectLayout
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.Optional
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.TaskAction
import org.gradle.kotlin.dsl.filter
import javax.inject.Inject

abstract class CMakeCompatibleSourceFolder @Inject constructor(private var fs: FileSystemOperations, private var layout: ProjectLayout) : DefaultTask() {
  @get:Input
  abstract val packageDirName: Property<String>

  @get:Internal
  abstract val packageTmpDir: DirectoryProperty

  @get:OutputDirectory
  val outputDir: Provider<Directory> = packageTmpDir.map { it.dir("tmp/${packageDirName.get()}") }

  @get:Input
  abstract val replaceTokens: MapProperty<String, String>

  @get:Internal
  abstract val copyBlock: Property<Action<CopySpec>>

  @TaskAction
  fun build() {
    fs.copy {
      into(packageTmpDir.map { it.dir("tmp/${packageDirName.get()}") })

      from(layout.projectDirectory.dir("../source-cmake-distro")) {
        include("CMakeLists.txt.in")
        rename { "CMakeLists.txt" }
        filter<ReplaceTokens>("tokens" to replaceTokens.get())
      }
      from(layout.projectDirectory.dir("../cmake")) {
        into("cmake")
        include("libsaxonc.cmake")
      }

      into("lib") {
        from(layout.projectDirectory.dir("../src/lib")) {
          exclude("*.c.in")
        }
      }

      from(layout.projectDirectory.dir("../src/include")) {
        into("include")
      }
      from(layout.projectDirectory.dir("src/include")) {
        into("include")
      }

      from(layout.projectDirectory.dir("../")) {
        include("README.md")
        filter<ReplaceTokens>("tokens" to replaceTokens.get())
      }

      copyBlock.get().execute(this)
    }
  }
}
