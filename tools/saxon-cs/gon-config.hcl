source = ["./build/cs/bin/Release/net5.0/osx-x64/publish/SaxonCS",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/SaxonCS.pdb",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/SaxonCS.xml",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libSystem.IO.Compression.Native.dylib",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libSystem.Native.dylib",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libSystem.Net.Security.Native.dylib",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libSystem.Security.Cryptography.Native.Apple.dylib",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libSystem.Security.Cryptography.Native.OpenSsl.dylib",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libclrjit.dylib",
          "./build/cs/bin/Release/net5.0/osx-x64/publish/libcoreclr.dylib"]

bundle_id = "com.saxonica.SaxonCS"

apple_id {
  username = "norm@saxonica.com"
  password = "@env:AC_PASSWORD"
}

sign {
  application_identity = "Developer ID Application: Saxonica Limited"
}

dmg {
  output_path = "saxoncs.dmg"
  volume_name = "SaxonCS"
}

zip {
  output_path = "saxoncs.zip"
}
