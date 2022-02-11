import org.gradle.api.Plugin
import org.gradle.api.Project

public class ManageBranchesPlugin implements Plugin<Project> {
  void apply(Project project) {
    ProcessBuilder pb = new ProcessBuilder()
      .command("git", "rev-parse", "--abbrev-ref", "HEAD");
    Process proc = pb.start();
    proc.waitFor()

    BufferedReader reader = new BufferedReader(new InputStreamReader(proc.getInputStream()))
    String currentBranch = reader.readLine().trim()
    reader.close()

    String builtBranch = null
    try {
      String fn = "${project.buildDir}/built-branch"
      BufferedReader xbranch = new BufferedReader(new FileReader(new File(fn)))
      builtBranch = xbranch.readLine().trim()
    } catch (FileNotFoundException foe) {
      // nop
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
