import com.saxonica.build.php.PhpExec

plugins {
    id("saxonc-php")
}

saxoncPhp {
    edition = "ee"
}

//tasks.named<PhpExec>("phpunit") {
//    args(layout.projectDirectory.file("my/other/tests.php").asFile.toString())
//}