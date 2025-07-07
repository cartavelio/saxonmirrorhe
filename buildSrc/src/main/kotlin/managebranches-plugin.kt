package com.saxonica.build

import ManageBranchesExtension
import java.io.File
import java.io.BufferedReader
import java.io.FileReader
import java.io.InputStreamReader
import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.provider.Property
import org.gradle.kotlin.dsl.create

class ManageBranchesPlugin : Plugin<Project> {
  companion object {
    var currentBranchName: String? = null
  }    

  override fun apply(project: Project) {
    val extension = project.extensions.create<ManageBranchesExtension>("branches")

    // This is going to be configured for every subproject that uses it.
    // Make sure we only do the work once.
    synchronized(Companion) {
      if (ManageBranchesPlugin.currentBranchName == null) {
        // Where is git?
        var git: String? = null
        System.getenv("PATH").split(System.getProperty("path.separator")).forEach { dir ->
          if (git == null) {
            val fn = File(dir + "/git")
            if (fn.exists() && !fn.isDirectory() && fn.canExecute()) {
              git = fn.toString()
            } else {
              val wfn = File(dir + "\\git.exe")
              if (wfn.exists() && wfn.canExecute()) {
                git = wfn.toString()
              }
            }
          }
        }

        if (git == null) {
          throw GradleException("Cannot find git on PATH!?")
        }

        val pb = ProcessBuilder().command(git, "rev-parse", "--abbrev-ref", "HEAD")
        pb.directory(project.projectDir)
        val proc = pb.start()
        proc.waitFor()

        if (proc.exitValue() != 0) {
          throw GradleException("Git failed to return current branch!?")
        }

        ManageBranchesPlugin.currentBranchName = InputStreamReader(proc.inputStream).use { instream ->
          BufferedReader(instream).use { reader ->
            reader.readLine()
          }
        }
      }
    }

    extension.currentBranch.convention(ManageBranchesPlugin.currentBranchName!!)
  }
}
