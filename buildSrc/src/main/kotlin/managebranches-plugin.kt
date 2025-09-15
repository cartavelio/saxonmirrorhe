package com.saxonica.build

import ManageBranchesExtension
import com.saxonica.build.java.BranchInfo
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
        ManageBranchesPlugin.currentBranchName = BranchInfo.currentBranchName(project.projectDir)
      }
    }

    extension.currentBranch.convention(ManageBranchesPlugin.currentBranchName!!)
  }
}
