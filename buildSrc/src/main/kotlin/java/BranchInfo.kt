package com.saxonica.build.java

import org.gradle.api.GradleException
import java.io.BufferedReader
import java.io.File
import java.io.InputStreamReader

class BranchInfo {
  companion object {
    private var _currentBranchName: String? = null
    private var _currentCommitHash: String? = null

    private var _git: String? = null
    private val git: String
      get() {
        if (_git == null) {
          System.getenv("PATH").split(File.pathSeparator).forEach { dir ->
            if (_git == null) {
              val fn = File(dir + "/git")
              if (fn.exists() && !fn.isDirectory() && fn.canExecute()) {
                _git = fn.toString()
              } else {
                val wfn = File(dir + "\\git.exe")
                if (wfn.exists() && wfn.canExecute()) {
                  _git = wfn.toString()
                }
              }
            }
          }
        }

        if (_git == null) {
          throw GradleException("Cannot find git on PATH!?")
        }

        return _git!!
      }

    fun currentBranchName(projectDir: File): String {
      synchronized(Companion) {
        if (_currentBranchName != null) {
          return _currentBranchName!!
        }

        val pb = ProcessBuilder().command(git, "rev-parse", "--abbrev-ref", "HEAD")
        pb.directory(projectDir)
        val proc = pb.start()
        proc.waitFor()

        if (proc.exitValue() != 0) {
          throw GradleException("Git failed to return current branch!?")
        }

        InputStreamReader(proc.inputStream).use { instream ->
          BufferedReader(instream).use { reader ->
            _currentBranchName = reader.readLine()
          }
        }

        return _currentBranchName!!.trim()
      }
    }

    fun currentCommitHash(projectDir: File): String {
      synchronized(Companion) {
        if (_currentCommitHash != null) {
          return _currentCommitHash!!
        }

        val pb = ProcessBuilder().command(git, "rev-parse", "--short", "HEAD")
        pb.directory(projectDir)
        val proc = pb.start()
        proc.waitFor()

        if (proc.exitValue() != 0) {
          throw GradleException("Git failed to return current commit hash!?")
        }

        InputStreamReader(proc.inputStream).use { instream ->
          BufferedReader(instream).use { reader ->
            _currentCommitHash = reader.readLine()
          }
        }

        return _currentCommitHash!!.trim()
      }
    }
  }
}

