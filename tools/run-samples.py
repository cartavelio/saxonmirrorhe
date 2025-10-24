#!/bin/env python3

import re
import os
import sys
from abc import ABC, abstractmethod
import shutil
import subprocess
import click

try:
    from . import checksamples
except:
    import checksamples

ignore_errors = False

class Config:
    def __init__(self, edition, platform, csexec):
        self.platform = platform
        self.edition = edition
        self.script_path = os.path.realpath(__file__)
        self.root_path = os.path.dirname(os.path.dirname(self.script_path))
        self.version = None

        if platform == "java":
            if edition == "ee":
                self.build = os.path.join(self.root_path, "java/build")
            else:
                self.build = os.path.join(self.root_path, f"java/{edition}/build")
        elif platform == "csharp":
            self.saxoncs = f"{self.root_path}/csharp/{edition}/{csexec}"
            self.build = os.path.join(self.root_path, f"csharp/{edition}/build")
        elif platform == "saxonc":
            self.build = os.path.join(self.root_path, f"libsaxonc/{edition}/build")
        elif platform == "python":
            self.build = os.path.join(self.root_path, f"python/{edition}/build")
        else:
            raise ValueError(f"Unexpected platform: {platform}")

        self.output = os.path.join(self.build, "sample_results")
        if not os.path.isdir(self.output):
            os.makedirs(self.output)

        with(open(f"{self.root_path}/gradle.properties")) as prop:
            lines = prop.readlines()
            for line in lines:
                vermatch = re.search("saxonVersion\\s*=\\s*(.+)", line.strip())
                if vermatch:
                    self.version = vermatch.group(1)
                    break

        if self.version is None:
            print("Failed to find version?")
            sys.exit(1)

        if self.version.endswith(".0"):
            self.version = self.version[0:len(self.version)-2]

        cp = [f"{self.build}/sample-staging/saxon-{self.edition}-{self.version}.jar"]

        if edition == "ee":
            cp.append(f"{self.build}/sample-staging/saxon-sql-{self.version}.jar")

        for (dirpath, dirnames, filenames) in os.walk(f"{self.build}/stage/lib"):
            for name in filenames:
                if name.endswith(".jar"):
                    cp.append(f"{dirpath}/{name}")
        for (dirpath, dirnames, filenames) in os.walk(f"{self.root_path}/samples/lib"):
            for name in filenames:
                if name.endswith(".jar"):
                    cp.append(f"{dirpath}/{name}")
        cp.append(f"{self.build}/samples/run/classes")

        self.classpath = ":".join(cp)


class TestRunner(ABC):
    def run(self):
        if self.skip:
            print(f"Skipping {self.sample} for {self.config.edition}")
            return

        dispatch = {
            "java": self.runJava,
            "csharp": self.runCSharp,
            "saxonc": self.runC
        }

        if self.config.platform in dispatch:
            dispatch[self.config.platform]()
        else:
            print(f"Running {self.sample}")
            raise ValueError(f"Unexpected platform: {self.config.platform}")

        if self.checker and os.path.exists(self.output):
            try:
                with open(self.output, "r") as out:
                    self.checker(out.read())
                print(f"Running {self.sample} 👍")
            except Exception as ex:
                print(f"Running {self.sample} 👎")
                raise ex

    def args(self, arglist):
        # Crude, but it'll probably do
        if type(arglist) == list:
            self.arguments = self.arguments + arglist
        else:
            self.arguments.append(arglist)
        return self

class Transform(TestRunner):
    def __init__(self, config, sample_name, for_editions=[]):
        self.config = config
        self.sample = sample_name
        self.skip = len(for_editions) > 0 and config.edition not in for_editions
        self.xml = None
        self.xsl = None
        self.arguments = []
        self.output = os.path.join(config.output, f"{sample_name}.html")
        self.checker = None

    def source(self, filename):
        self.xml = filename
        return self

    def stylesheet(self, filename):
        self.xsl = filename
        return self

    def output_path(self, path):
        self.output = os.path.join(self.config.output, path)
        return self

    def check_with(self, checkmethod):
        self.checker = checkmethod
        return self

    def runJava(self):
        command = ["java", "-cp", f"{self.config.classpath}", "net.sf.saxon.Transform"]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xsl:
            command.append(f"-xsl:{self.xsl}")
        command.append(f"-o:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

    def runCSharp(self):
        command = [self.config.saxoncs, "transform" ]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xsl:
            command.append(f"-xsl:{self.xsl}")
        command.append(f"-o:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

    def runC(self):
        command = [f"build/Release/Release/Transform"]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xsl:
            command.append(f"-xsl:{self.xsl}")
        command.append(f"-o:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

                   
class Query(TestRunner):
    def __init__(self, config, sample_name, for_editions=[]):
        self.config = config
        self.sample = sample_name
        self.skip = len(for_editions) > 0 and config.edition not in for_editions
        self.xml = None
        self.xquery = None
        self.xqueryString = None
        self.arguments = []
        self.output = os.path.join(config.output, f"{sample_name}.html")
        self.checker = None

    def source(self, filename):
        self.xml = filename
        return self

    def query(self, filename):
        self.xquery = filename
        return self

    def queryString(self, expression):
        self.xqueryString = expression
        return self

    def output_path(self, path):
        self.output = os.path.join(self.config.output, path)

    def check_with(self, checkmethod):
        self.checker = checkmethod
        return self

    def runJava(self):
        command = ["java", "-cp", f"{self.config.classpath}", "net.sf.saxon.Query"]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xquery:
            command.append(f"-q:{self.xquery}")
        if self.xqueryString:
            command.append(f"-qs:{self.xqueryString}")
        command.append(f"-o:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)


    def runCSharp(self, args=None):
        command = [self.config.saxoncs, "query" ]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xquery:
            command.append(f"-q:{self.xquery}")
        if self.xqueryString:
            command.append(f"-qs:{self.xqueryString}")
        command.append(f"-o:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

    def runC(self, args=None):
        command = [f"build/Release/Release/Query"]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xquery:
            command.append(f"-q:{self.xquery}")
        if self.xqueryString:
            command.append(f"-qs:{self.xqueryString}")
        command.append(f"-o:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

class Validate(TestRunner):
    def __init__(self, config, sample_name, for_editions=["ee"]):
        self.config = config
        self.sample = sample_name
        self.skip = len(for_editions) > 0 and config.edition not in for_editions
        self.xml = None
        self.xsd = None
        self.arguments = []
        self.output = os.path.join(config.output, f"{sample_name}.xml")

    def source(self, filename):
        self.xml = filename
        return self

    def schema(self, filename):
        self.xsd = filename
        return self

    def run(self):
        if self.skip:
            print(f"Skipping {self.sample} for {self.config.edition}")
            return

        dispatch = {
            "java": self.runJava,
            "csharp": self.runCSharp,
            "saxonc": self.runC
        }

        if self.config.platform in dispatch:
            dispatch[self.config.platform]()
        else:
            raise ValueError(f"Unexpected platform: {self.config.platform}")

    def runJava(self):
        command = ["java", "-cp", f"{self.config.classpath}", "com.saxonica.Validate"]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xsd:
            command.append(f"-xsd:{self.xsd}")
        command.append(f"-report:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        print(f"Running {self.sample}")
        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

        command = ["java", "-cp", f"{self.config.classpath}", "com.saxonica.Validate"]
        command.append(f"-s:{self.output}")
        command.append(f"-xsd:build/samples/resources/schemas/validation-reports.xsd")

        print(f"Validating {self.sample} report")
        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

    def runCSharp(self):
        command = [self.config.saxoncs, "validate" ]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xsd:
            command.append(f"-xsd:{self.xsd}")
        command.append(f"-report:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        print(f"Running {self.sample}")
        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

        command = [self.config.saxoncs, "validate" ]
        command.append(f"-s:{self.output}")
        command.append(f"-xsd:build/samples/resources/schemas/validation-reports.xsd")

        print(f"Validating {self.sample} report")
        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

    def runC(self):
        command = [f"build/Release/Release/Validate"]
        if self.xml:
            command.append(f"-s:{self.xml}")
        if self.xsd:
            command.append(f"-xsd:{self.xsd}")
        command.append(f"-report:{self.output}")
        if len(self.arguments) > 0:
            command = command + self.arguments

        print(f"Running {self.sample}")
        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

        command = [f"build/Release/Release/Validate"]
        command.append(f"-s:{self.output}")
        command.append(f"-xsd:build/samples/resources/schemas/validation-reports.xsd")

        print(f"Validating {self.sample} report")
        cp = subprocess.run(command)
        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)

class JavaClass(TestRunner):
    def __init__(self, config, sample_name, for_editions=[]):
        self.config = config
        self.sample = sample_name
        self.skip = len(for_editions) > 0 and config.edition not in for_editions
        self.klassname = None
        self.arguments = []
        self.checker = None

    def classname(self, classname):
        self.klassname = classname
        return self

    def check_with(self, checkmethod):
        self.checker = checkmethod
        return self

    def run(self, args=None):
        if self.skip:
            print(f"Skipping {self.sample} for {self.config.edition}")
            return

        if not self.klassname:
            printf("Skipping {self.sample}; no class configured")
            return

        command = ["java",
                   "--add-exports=java.xml/com.sun.org.apache.xerces.internal.parsers=ALL-UNNAMED",
                   "--add-exports=java.xml/com.sun.org.apache.xerces.internal.util=ALL-UNNAMED",
                   "-cp", f"{self.config.classpath}", self.klassname]
        if len(self.arguments) > 0:
            command = command + self.arguments

        cwd = os.getcwd()
        os.chdir(f"{self.config.build}/samples/resources")

        print(f"Running {self.sample}")

        #print(command)

        cp = subprocess.run(command, capture_output=True, text=True)

        os.chdir(cwd)

        if len(cp.stdout) > 0:
            with(open(os.path.join(self.config.output, f"{self.sample}.txt"), "w")) as file:
                file.write(cp.stdout)
            if self.checker:
                try:
                    self.checker(os.path.join(self.config.output, f"{self.sample}.txt"))
                    print(f"👍")
                except RuntimeError as err:
                    print(f"👎")
                    print("\t", str(err))
                    if (not ignore_errors):
                        sys.exit(1)

        if len(cp.stderr) > 0:
            with(open(os.path.join(self.config.output, f"{self.sample}.err"), "w")) as file:
                file.write(cp.stderr)

        if cp.returncode != 0:
            print(f"Failed: {cp.returncode}")
            print(" ".join(command))
            sys.exit(1)


@click.command()
@click.option("--edition", default="he",
              type=click.Choice(["he", "pe", "ee"]),
              help="Saxon edition to run")
@click.option("--platform", default="java",
              type=click.Choice(["java", "csharp", "python", "saxonc"]),
              help="Saxon platform to run")
@click.option("--csexec",
              help="SaxonCS executable")
def run_samples(edition, platform, csexec):
    if platform == "csharp":
        if csexec is None:
            print("The --csexec option is required for csharp")
            sys.exit(1)
    else:
        if csexec is not None:
            print("The --csexec option can only be used for csharp")
            sys.exit(1)

    config = Config(edition, platform, csexec)

    print(f"Running {platform} tests for {edition}")

    transforms = []

    if platform == "java":
        if edition == "he":
            transforms.append(
                Transform(config, "books_with_config-HE")
                .source("build/samples/resources/data/books.xml")
                .stylesheet("build/samples/resources/styles/books.xsl")
                .args(["-config:build/samples/resources/config/config-HE.xml",
                       f"--traceListenerOutputFile:{config.output}/traceOutput_config.log"]))

        if edition == "ee":
            transforms.append(
                Transform(config, "books_with_config")
                .source("build/samples/resources/data/books.xml")
                .stylesheet("build/samples/resources/styles/books.xsl")
                .args(["-config:build/samples/resources/config/config.xml",
                       f"--traceListenerOutputFile:{config.output}/traceOutput_config.log"]))

    transforms += [
        Transform(config, "Bible_1")
        .source("build/samples/resources/bible/nt/nt.xml")
        .stylesheet("build/samples/resources/styles/bible.xsl")
        .output_path("biblehtml/index.html")
        .args(f"dir={config.output}/biblehtml"),

        Query(config, "Books_Query_1")
        .source("build/samples/resources/data/books.xml")
        .query("build/samples/resources/query/books.xq")
        .check_with(checksamples.check_books_query_1),

        Query(config, "Books_Query_2", ["pe", "ee"])
        .source("build/samples/resources/data/books.xml")
        .queryString("saxon:line-number((//*)[last()])")
        .args("-l:on")
        .check_with(checksamples.check_books_query_2),

        Transform(config, "Books_Transform_1")
        .source("build/samples/resources/data/books.xml")
        .stylesheet("build/samples/resources/styles/books.xsl")
        .check_with(checksamples.check_books_transform_1),

        Transform(config, "Books_Transform_2")
        .source("build/samples/resources/data/books.xml")
        .stylesheet("build/samples/resources/styles/books.xsl")
        .args("top-author='Thomas Hardy'")
        .check_with(checksamples.check_books_transform_2),

        Transform(config, "Books_Transform_3")
        .source("build/samples/resources/data/books.xml")
        .stylesheet("build/samples/resources/styles/books-csv.xsl")
        .check_with(checksamples.check_books_transform_3),

        Validate(config, "Books_Validate_1")
        .source("build/samples/resources/data/books.xml")
        .schema("build/samples/resources/data/books.xsd"),

        Transform(config, "Shakespeare_1")
        .source("build/samples/resources/data/othello.xml")
        .stylesheet("build/samples/resources/styles/play.xsl")
        .output_path("playhtml/play.html")
        .args(f"dir={config.output}/playhtml"),

        Transform(config, "Tour_Transform_1")
        .stylesheet("build/samples/resources/styles/tour.xsl")
        .args(["-it:main"])
        .check_with(checksamples.check_tour_transform_1),

        Transform(config, "Tour_Transform_2")
        .stylesheet("build/samples/resources/styles/tour.xsl")
        .args(["-it:main", "start=a6"])
        .check_with(checksamples.check_tour_transform_2),

        Transform(config, "Tour_Transform_3")
        .stylesheet("build/samples/resources/styles/tour.xsl")
        .args(["-it:main", "start=g2"]),

        Query(config, "Tour_Query_1")
        .query("build/samples/resources/query/tour.xq")
        .args(["start=e5"])
        .check_with(checksamples.check_tour_query_1),

        Query(config, "Tour_Query_2")
        .query("build/samples/resources/query/tour.xq")
        .args(["start=d2", "end=42"])
        .check_with(checksamples.check_tour_query_2),

        Query(config, "Tour_Query_3")
        .query("build/samples/resources/query/tour.xq")
        .args(["end=61"])
    ]

    if platform == "java":
        transforms.append(
            Validate(config, "Books_Validate_2")
            .source("build/samples/resources/data/books-invalid.xml")
            .schema("build/samples/resources/data/books.xsd")
            .args("-quit:off"))

    for xform in transforms:
        xform.run()

    if platform == "java":
        sample_classes = [
            JavaClass(config, "JAXPExamples")
            .classname("he.JAXPExamples")
            .args("all")
            .check_with(checksamples.check_jaxpexamples),

            JavaClass(config, "ApplyXPathJAXP_saxon")
            .classname("he.ApplyXPathJAXP")
            .args(["data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "saxon"])
            .check_with(checksamples.check_applyxpathjaxp_saxon),

            JavaClass(config, "ApplyXPathJAXP_dom")
            .classname("he.ApplyXPathJAXP")
            .args(["data/books.xml", "/BOOKLIST/BOOKS/ITEM[@CAT='MMP']/TITLE", "dom"])
            .check_with(checksamples.check_applyxpathjaxp_dom),

            JavaClass(config, "ExampleContentHandler")
            .classname("he.ExampleContentHandler")
            .args(["data/books.xml"])
            .check_with(checksamples.check_examplecontenthandler),

            JavaClass(config, "S9APIExamplesOM_ee", ["ee"])
            .classname("he.S9APIExamplesOM")
            .args(["all"])
            .check_with(checksamples.check_s9apiexamplesom_ee),

            JavaClass(config, "S9APIExamplesOM_pe", ["pe"])
            .classname("he.S9APIExamplesOM")
            .args(["all"])
            .check_with(checksamples.check_s9apiexamplesom_pe),

            JavaClass(config, "S9APIExamples_ee", ["ee"])
            .classname("he.S9APIExamples")
            .args(["all"])
            .check_with(checksamples.check_s9apiexamples_ee),

            JavaClass(config, "S9APIExamples_pe", ["pe"])
            .classname("he.S9APIExamples")
            .args(["nonschema"])
            .check_with(checksamples.check_s9apiexamples_pe),

            JavaClass(config, "S9APIExamples_he", ["he"])
            .classname("he.S9APIExamples")
            .args(["nonschema"])
            .check_with(checksamples.check_s9apiexamples_he),

            JavaClass(config, "JDOM2Example", ["ee"])
            .classname("ee.JDOM2Example")
            .args(["data/books.xml", "styles/books.xsl"])
            .check_with(checksamples.check_jdom2example),

            JavaClass(config, "PullExamples", ["ee"])
            .classname("ee.PullExamples")
            .check_with(checksamples.check_pullexamples),

            JavaClass(config, "QuickValidator", ["ee"])
            .classname("ee.QuickValidator")
            .args(["data/books.xml", "data/books.xsd"]),
            # produces no output

            JavaClass(config, "SchemaValidatorExample_1", ["ee"])
            .classname("ee.SchemaValidatorExample")
            .args(["data/books.xsd", "data/books.xml"]),
            # produces no output

            JavaClass(config, "SchemaValidatorExample_2", ["ee"])
            .classname("ee.SchemaValidatorExample")
            .args(["--", "data/books.xml"]),
            # produces no output

            JavaClass(config, "SchemaValidatorHandlerExample_1", ["ee"])
            .classname("ee.SchemaValidatorHandlerExample")
            .args(["data/books.xsd", "data/books.xml"])
            .check_with(checksamples.check_schemavalidatorhandler),

            JavaClass(config, "SchemaValidatorHandlerExample_2", ["ee"])
            .classname("ee.SchemaValidatorHandlerExample")
            .args(["--", "data/books.xml"])
            .check_with(checksamples.check_schemavalidatorhandler),

            JavaClass(config, "XPathExampleSA", ["ee"])
            .classname("ee.XPathExampleSA")
            .args(["data/books.xml", "data/books.xsd"])
            .check_with(checksamples.check_xpathexamplesa),

            JavaClass(config, "XQJExamples", ["ee"])
            .classname("ee.XQJExamples")
            .args(["all"])
            .check_with(checksamples.check_xqjexamples),
        ]

        for jclass in sample_classes:
            jclass.run()


if __name__ == "__main__":
    run_samples()
