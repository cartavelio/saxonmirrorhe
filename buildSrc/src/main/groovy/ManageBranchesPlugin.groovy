import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project

public class ManageBranchesPlugin implements Plugin<Project> {
  void apply(Project project) {
    // Where is git?
    String git = null
    System.getenv("PATH").split(System.getProperty("path.separator")).each { dir ->
      if (git == null) {
        def fn = new File(dir + "/git")
        if (fn.exists() && !fn.isDirectory() && fn.canExecute()) {
          git = fn.toString()
        } else {
          fn = new File(dir + "\\git.exe")
          if (fn.exists() && fn.canExecute()) {
            git = fn.toString()
          }
        }
      }
    }

    if (git == null) {
      throw new GradleException("Cannot find git on PATH!?")
    }

    ProcessBuilder pb = new ProcessBuilder()
      .command(git, "rev-parse", "--abbrev-ref", "HEAD");
    pb.directory(project.projectDir)
    Process proc = pb.start();
    proc.waitFor()

    if (proc.exitValue() != 0) {
      throw new GradleException("Git failed to return current branch!?")
    }

    BufferedReader reader = new BufferedReader(new InputStreamReader(proc.getInputStream()))
    String currentBranch = reader.readLine()
    reader.close()

    String builtBranch = null
    try {
      String fn = "${project.buildDir}/built-branch"
      BufferedReader xbranch = new BufferedReader(new FileReader(new File(fn)))
      builtBranch = xbranch.readLine()
    } catch (FileNotFoundException foe) {
      // nop
    }

    if (currentBranch != builtBranch) {
      println("Manage branches: current=${currentBranch}, built=${builtBranch}")
    }

    if (currentBranch != builtBranch) {
      if (builtBranch != null) {
        println("Switching branches from '${builtBranch}' to '${currentBranch}'...")
        deleteRecursively(new File("${project.buildDir}/src"))
        deleteRecursively(new File("${project.buildDir}/preprocess"))
        deleteRecursively(new File("${project.buildDir}/filtered"))
        deleteRecursively(new File("${project.buildDir}/classes"))
        deleteRecursively(new File("${project.projectDir}/src/main/java/net/sf/saxon/lib/Feature.java"))
        deleteRecursively(new File("${project.projectDir}/src/main/java/net/sf/saxon/lib/FeatureCode.java"))
        deleteRecursively(new File("${project.projectDir}/src/main/java/net/sf/saxon/lib/FeatureKeys.java"))
        deleteRecursively(new File("${project.projectDir}/src/main/csharp/api/Saxon.Api/Feature.cs"))
        deleteRecursively(new File("${project.projectDir}/src/main/csharp/api/Saxon.Api/FeatureKeys.cs"))
      }
    }
  }

  private void deleteRecursively(File file) {
    if (file.exists()) {
      if (file.isDirectory()) {
        for (File fspec : file.listFiles()) {
          if (fspec.isDirectory()) {
            deleteRecursively(fspec);
          } else {
            fspec.delete();
          }
        }
      }
      file.delete();
    }
  }
}
