package com.saxonica.build

import cc.ekblad.toml.*
import cc.ekblad.toml.configuration.ListMode
import cc.ekblad.toml.serialization.CollectionSyntax
import org.gradle.api.file.Directory
import org.gradle.api.file.RegularFile
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import java.nio.file.Path

class PyprojectToml(val sourceDir: Provider<Directory>, val versionProperty: Property<String>, val editionProperty: Property<String>) {
    val serializer = tomlSerializer {
        indentStep(4)
        inlineListMode(ListMode.Adaptive)
        preferListSyntax(CollectionSyntax.Table)
        preferTableSyntax(CollectionSyntax.Table)
    }

    data class Pyproject(
        val project: Project,
        val buildSystem: BuildSystem,
        val tool: Tool
    ) {
        data class BuildSystem(
            val requires: List<String>,
            val buildBackend: String,
        )

        data class Project(
            val name: String,
            val version: String,
            val requiresPython: String,
            var description: String,
            val authors: List<Author>,
            val readme: Map<String, String>,
            val urls: Map<String, String>,
        )

        data class Author(val name: String, val email: String)

        data class Tool(val saxonicaBuild: SaxonicaBuild)

        data class SaxonicaBuild(val requires: List<String>)

    }

    fun process(output: Provider<Directory>) {
        val edition = editionProperty.get()
        val version = versionProperty.get()
        val pyproject = mapper.decode<Pyproject>(pyprojectPath(sourceDir))
        val rewrittenProject = pyproject.project.copy(name = "saxonc${edition}", version = version, description = pyproject.project.description.replace("{{EDITION}}", edition.uppercase()).replace("{{version}}", version))
        val rewrittenToml = mapper.encodeToDocument(pyproject.copy(project = rewrittenProject))
        serializer.write(rewrittenToml, pyprojectPath(output))
    }

    companion object {
        val mapper = tomlMapper {
            mapping<Pyproject>(
                "build-system" to "buildSystem",
            )
            mapping<Pyproject.BuildSystem>(
                "build-backend" to "buildBackend",
            )
            mapping<Pyproject.Project>(
                "requires-python" to "requiresPython",
            )
            mapping<Pyproject.Tool>(
                "saxonica-build" to "saxonicaBuild",
            )
        }

        fun pyprojectPath(dir: Provider<Directory>): Path {
            return filePath(dir.map { it.file("pyproject.toml") })
        }

        fun filePath(file: Provider<RegularFile>): Path {
            return file.get().asFile.toPath()
        }

        fun buildSystemRequires(file: Provider<RegularFile>): List<String> {
            val pyproject = mapper.decode<Pyproject>(filePath(file))
            return pyproject.buildSystem.requires + pyproject.tool.saxonicaBuild.requires
        }
    }
}