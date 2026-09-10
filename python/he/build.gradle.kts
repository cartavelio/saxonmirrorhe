import com.saxonica.build.python.PytestRunner

plugins {
    id("saxon-build")
    id("com.saxonica.build.managebranches")
    id("saxonc-python")
}

saxoncPython {
    edition = "he"
}

tasks.named<PytestRunner>("pytest") {
    tests(layout.projectDirectory.dir("test"))
}
