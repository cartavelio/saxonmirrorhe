import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.internal.os.OperatingSystem;

class JarSignPluginExtension {
  String alias = "THIS-IS-NOT-THE-ALIAS"
  String password = "THIS-IS-NOT-THE-PASSWORD"
  String keystore = "THIS-IS-NOT-THE-KEYSTORE"
  ArrayList jarSigner = ["jarsigner"]
  ArrayList nugetSigner = ["nuget"]
  ArrayList exeSigner = ["signtool"]
  boolean signJars = false
  boolean signNugets = false
  boolean signExes = false
  OperatingSystem systemOs = OperatingSystem.current()

  public boolean getCanSignJars() {
    if (jarSigner[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || keystore.startsWith("THIS-IS-NOT"))
  }

  public boolean getCanSignNuget() {
    if (nugetSigner[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || keystore.startsWith("THIS-IS-NOT"))
  }

  public boolean getCanSignExes() {
    if (exeSigner[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || keystore.startsWith("THIS-IS-NOT"))
  }

  public boolean getMakeSignedJars() {
    if (signJars && !getCanSignJars()) {
      throw new GradleException("Build is not configured to sign jars.")
    }
    return signJars
  }    

  public boolean getMakeSignedNuget() {
    if (signNugets && !getCanSignNuget()) {
      throw new GradleException("Build is not configured to sign nupkg files.")
    }
    return signJars
  }    

  public boolean getMakeSignedExes() {
    if (signExes && !getCanSignExes()) {
      throw new GradleException("Build is not configured to sign executables.")
    }
    return signExes
  }    

  public ArrayList signJarCommand(String jarfile) {
    ArrayList command = jarSigner
    command += ["-storetype", "PKCS12",
                "-keystore", keystore,
                "-tsa", "http://rfc3161timestamp.globalsign.com/advanced"]
    return command += [jarfile, alias, "-storepass", password]
  }

  public ArrayList signNugetCommand(String nugetfile) {
    ArrayList command = nugetSigner
    command += ["sign", nugetfile,
	        "-CertificatePath", keystore,
	        "-Timestamper", "http://timestamp.digicert.com",
	        "-OutputDirectory", "build/distributions",
	        "-Verbosity", "detailed",
	        "-CertificatePassword", password]
    return command
  }

  public ArrayList signExeCommand(String exefile) {
    ArrayList command = exeSigner
    if (systemOs.isWindows()) {
      command += ["sign", "/fd", "sha1",
                  "/f", keystore,
                  "/t", "http://timestamp.digicert.com",
                  "/p", password,
                  exefile]
    } else {
      command += ["-s", "SAXONICA LIMITED", exefile]
    }
    return command
  }
}

public class JarSignPlugin implements Plugin<Project> {
  OperatingSystem systemOs = OperatingSystem.current()

  void apply(Project project) {
    def extension = project.extensions.create("jarsigner", JarSignPluginExtension)

    String prop = project.findProperty("saxonSignAlias") ?: System.getenv("SAXON_SIGN_ALIAS")
    if (prop != null) {
      extension.alias = prop
    }

    prop = project.findProperty("saxonSignPassword") ?: System.getenv("SAXON_SIGN_PASSWORD")
    if (prop != null) {
      extension.password = prop
    }

    prop = project.findProperty("saxonSignKeystore") ?: System.getenv("SAXON_SIGN_KEYSTORE")
    if (prop != null) {
      extension.keystore = prop
      File kstore = new File(prop)
      if (!kstore.exists()) {
        println("Warning: signing keystore file does not exist: ${prop}")
      }
    }

    if (project.findProperty("testSignJars") == "true") {
      extension.jarSigner = ["echo", "JARSIGNER"]
    }

    prop = project.findProperty("saxonSignJars") ?: System.getenv("SAXON_SIGN_JARS")
    extension.signJars = (prop == "true")

    if (project.findProperty("testSignExes") == "true") {
      extension.exeSigner = ["echo", "EXESIGNER"]
    }

    prop = project.findProperty("saxonSignExes") ?: System.getenv("SAXON_SIGN_EXES")

    extension.signExes = (prop == "true")

    if (systemOs.isWindows()) {
      prop = project.findProperty("windowsSignTool") ?: System.getenv("WINDOWS_SIGN_TOOL")
    } else if (systemOs.isMacOsX()) {
      prop = project.findProperty("macSignTool") ?: System.getenv("MAC_SIGN_TOOL")
    } else if (systemOs.isLinux()) {
      prop = null
    } else {
      // Unknown OS?
      prop = null
    }

    if (prop != null) {
      extension.exeSigner = [prop]
    }

    if (project.findProperty("testSignNuget") == "true") {
      extension.exeSigner = ["echo", "NUGET"]
    }

    prop = project.findProperty("saxonSignNugets") ?: System.getenv("SAXON_SIGN_NUGETS")

    extension.signNugets = (prop == "true")
  }
}
