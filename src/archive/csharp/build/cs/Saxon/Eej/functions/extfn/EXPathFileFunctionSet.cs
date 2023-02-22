using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class EXPathFileFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Eej.functions.extfn.EXPathFileFunctionSet THE_INSTANCE = new Saxon.Eej.functions.extfn.EXPathFileFunctionSet();
        public static Saxon.Eej.functions.extfn.EXPathFileFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private EXPathFileFunctionSet()  {
            init();
        }
        private void init() {
            register("append", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppend), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("append", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppend), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(2, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, null);
            register("append-binary", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppendBinary), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null);
            register("append-text", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppendText), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("append-text", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppendText), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("append-text-lines", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppendTextLines), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null);
            register("append-text-lines", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileAppendTextLines), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("base-dir", 0, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileBaseDir), Saxon.Hej.type.AnyItemType.getInstance(), OPT, 0);
            register("children", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileChildren), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("copy", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCopy), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("create-dir", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCreateDir), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("create-temp-dir", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCreateTempDir), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("create-temp-dir", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCreateTempDir), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("create-temp-file", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCreateTempFile), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("create-temp-file", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCreateTempFile), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("current-dir", 0, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileCurrentDir), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0);
            register("delete", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileDelete), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("delete", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileDelete), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, null);
            register("dir-separator", 0, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileDirSeparator), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0);
            register("exists", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileExists), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("is-dir", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileIsDir), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("is-file", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileIsFile), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("last-modified", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileLastModified), Saxon.Hej.type.BuiltInAtomicType.DATE_TIME_STAMP, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("line-separator", 0, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileLineSeparator), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0);
            register("list", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileList), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("list", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileList), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, null);
            register("list", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileList), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("move", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileMove), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("name", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileName), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("parent", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileParent), Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("path-separator", 0, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FilePathSeparator), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0);
            register("path-to-native", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FilePathToNative), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("path-to-uri", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FilePathToUri), Saxon.Hej.type.BuiltInAtomicType.ANY_URI, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("read-binary", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadBinary), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("read-binary", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadBinary), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("read-binary", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadBinary), Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("read-text", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadText), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("read-text", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadText), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("read-text-lines", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadTextLines), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("read-text-lines", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileReadTextLines), Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("resolve-path", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileResolvePath), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("size", 1, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileSize), Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("temp-dir", 0, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileTempDir), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, LATE);
            register("write", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWrite), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("write", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWrite), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(2, Saxon.Hej.pattern.NodeKindTest.ELEMENT, ONE, null);
            register("write-binary", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWriteBinary), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null);
            register("write-binary", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWriteBinary), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("write-text", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWriteText), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("write-text", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWriteText), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("write-text-lines", 2, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWriteTextLines), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null);
            register("write-text-lines", 3, typeof(Saxon.Eej.functions.extfn.EXPathFileFunctionSet.FileWriteTextLines), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, STAR, null).arg(2, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
        }
        public static readonly Saxon.Hej.value.BigDecimalValue VERSION = new Saxon.Hej.value.BigDecimalValue(Singulink.Numerics.BigDecimal.Parse("1.0"));
        public const string NAMESPACE = "http://expath.org/ns/file";
        public const string PREFIX = "file";
        public const string ERROR_NAMESPACE = NAMESPACE;
        public const string ERROR_PREFIX = PREFIX;
        private const string ERROR_PATH_NOT_EXIST = "not-found";
        private const string ERROR_PATH_EXISTS = "exists";
        private const string ERROR_PATH_NOT_DIRECTORY = "no-dir";
        private const string ERROR_PATH_IS_DIRECTORY = "is-dir";
        private const string ERROR_UNKNOWN_ENCODING = "unknown-encoding";
        public const string ERROR_INDEX_OUT_OF_BOUNDS = "out-of-range";
        public const string ERROR_IO = "io-error";
        public static readonly string NEWLINE = Saxon.Ejava.lang.System.getProperty("line.separator");
        public override string getNamespace() {
            return NAMESPACE;
        }
        public override string getConventionalPrefix() {
            return PREFIX;
        }
        public class FileBaseDir : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
                if (getRetainedStaticContext().getPackageData().isRelocatable()) {
                    return base.makeFunctionCall(arguments);
                } else {
                    try {
                        return Saxon.Hej.expr.Literal.makeLiteral(makeResult(getStaticBaseUriString()));
                    } catch (Saxon.Hej.trans.XPathException e) {
                        return new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(e));
                    }
                }
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                return makeResult(getStaticBaseUriString());
            }
            private static Saxon.Hej.om.GroundedValue makeResult(string baseURI) {
                if ((baseURI == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                string parent = _parent(baseURI);
                if ((parent == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                return Saxon.Hej.value.StringValue.makeStringValue(parent);
            }
        
        }
        public class FileAppend : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Hej.serialize.SerializationProperties @params;
                if ((arguments.Length>2)) {
                    Saxon.Hej.serialize.SerializationParamsHandler sph = new Saxon.Hej.serialize.SerializationParamsHandler();
                    sph.setSerializationParams((Saxon.Hej.om.NodeInfo)(arguments[2].head()));
                    @params = sph.getSerializationProperties();
                } else {
                    Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                    props.setProperty("method", "xml");
                    @params = new Saxon.Hej.serialize.SerializationProperties(props);
                }
                serialize(context, path, true, arguments[1], @params);
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileAppendBinary : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                try {
                    Saxon.Ejava.io.File file = toFile(path);
                    if (file.isDirectory()) {
                        error((("Path "+path)+" is a directory"), ERROR_PATH_IS_DIRECTORY);
                    }
                    System.IO.FileStream stream = makeFileAppendOutputStream(file);
                    Saxon.Hej.value.Base64BinaryValue contents = (Saxon.Hej.value.Base64BinaryValue)(arguments[1].head());
                    stream.Write(contents.getBinaryValue());
                    stream.Close();
                } catch (Saxon.Ejava.io.FileNotFoundException e) {
                    error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
                } catch (System.IO.IOException e) {
                    error(("Failed to write to file "+path), e, ERROR_IO);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        private static System.IO.FileStream makeFileAppendOutputStream(Saxon.Ejava.io.File file) {
            return file.makeAppendOutputStream();
        }
        public class FileAppendText : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                string contents = arguments[1].head().getStringValue();
                string encoding = "UTF-8";
                if ((arguments.Length>2)) {
                    encoding = arguments[2].head().getStringValue();
                }
                Saxon.Ejava.io.File file = null;
                System.IO.TextWriter writer;
                try {
                    file = toFile(path);
                    if (file.isDirectory()) {
                        error((("Path "+path)+" is a directory"), ERROR_PATH_IS_DIRECTORY);
                    }
                    writer = newOutputStreamWriter(makeFileAppendOutputStream(file), encoding);
                    writer.Write(contents);
                    writer.Close();
                } catch (/* substitute exception */ System.ArgumentException e) {
                    error(("Unsupported encoding "+encoding), ERROR_UNKNOWN_ENCODING);
                } catch (/* no matching annotation */ Saxon.Ejava.io.FileNotFoundException e) {
                    error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
                } catch (/* no matching annotation */ System.IO.IOException e) {
                    error(("Failed to write to file "+file.getAbsolutePath()), e, ERROR_IO);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        private static System.IO.TextWriter newOutputStreamWriter(System.IO.Stream stream, string encoding) {
            return Saxon.Impl.Helpers.IOUtils.newOutputStreamWriter(stream, encoding);
        }
        public class FileAppendTextLines : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                string encoding = "UTF-8";
                if ((arguments.Length>2)) {
                    encoding = arguments[2].head().getStringValue();
                }
                Saxon.Ejava.io.File file;
                try {
                    file = toFile(path);
                    if (file.isDirectory()) {
                        error((("Path "+path)+" is a directory"), ERROR_PATH_IS_DIRECTORY);
                    }
                    System.IO.TextWriter writer = newOutputStreamWriter(makeFileAppendOutputStream(file), encoding);
                    Saxon.Hej.om.SequenceIterator iter = arguments[1].iterate();
                    Saxon.Hej.om.Item item;
                    while (((item = iter.next()) != null)) {
                        writer.Write(item.getStringValue());
                        writer.Write(NEWLINE);
                    }
                } catch (Saxon.Ejava.io.UnsupportedEncodingException e) {
                    error(("Unsupported encoding "+encoding), ERROR_UNKNOWN_ENCODING);
                } catch (Saxon.Ejava.io.FileNotFoundException e) {
                    error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
                } catch (System.IO.IOException e) {
                    error(("Failed to write to file "+path), e, ERROR_IO);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileChildren : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                System.Collections.Generic.IList<Saxon.Hej.value.StringValue> result = new System.Collections.Generic.List<Saxon.Hej.value.StringValue>();
                string dir = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File fd = toExistingDirectory(dir);
                if (!(fd.isDirectory())) {
                    error((("File "+dir)+" is not a directory"), ERROR_PATH_NOT_DIRECTORY);
                }
                foreach (Saxon.Ejava.io.File file in fd.listFiles()) {
                    result.Add(new Saxon.Hej.value.StringValue((file.getAbsolutePath()+((file.isDirectory() ? Saxon.Ejava.io.File.separator : "")))));
                }
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
            }
        
        }
        public class FileCopy : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File from = toFile(arguments[0].head().getStringValue());
                Saxon.Ejava.io.File to = toFile(arguments[1].head().getStringValue());
                if ((from.exists() && from.isDirectory())) {
                    if (to.exists()) {
                        if (to.isDirectory()) {
                            to = new Saxon.Ejava.io.File(to, from.getName());
                            to.mkdir();
                            copyDirectory(from, to);
                        } else {
                            error("Source is a directory but target is a file", ERROR_PATH_EXISTS);
                        }
                    } else {
                        to.mkdir();
                        copyDirectory(from, to);
                    }
                } else {
                    if (to.exists()) {
                        if (to.isDirectory()) {
                            to = new Saxon.Ejava.io.File(to, from.getName());
                            copyFile(from, to);
                        } else {
                            copyFile(from, to);
                        }
                    } else {
                        copyFile(from, to);
                    }
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileCreateDir : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = toFile(path);
                if (file.exists()) {
                    if (!(file.isDirectory())) {
                        error((("Path "+path)+" already exists and is not a directory"), ERROR_PATH_EXISTS);
                    }
                } else {
                    bool result = file.mkdirs();
                    if (!(result)) {
                        error(("Failed to create directory "+path), ERROR_PATH_EXISTS);
                    }
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileCreateTempDir : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                try {
                    string prefix = arguments[0].head().getStringValue();
                    string suffix = arguments[1].head().getStringValue();
                    Saxon.Ejava.io.File test;
                    if ((arguments.Length == 2)) {
                        test = Saxon.Ejava.io.File.createTempFile(prefix, suffix);
                    } else {
                        string dir = arguments[2].head().getStringValue();
                        test = Saxon.Ejava.io.File.createTempFile(prefix, suffix, toFile(dir));
                    }
                    string path = test.getCanonicalPath();
                    test.delete();
                    Saxon.Ejava.io.File temp = new Saxon.Ejava.io.File(path);
                    temp.mkdirs();
                    deleteTempFile(context, temp);
                    return new Saxon.Hej.value.StringValue(temp.getCanonicalPath());
                } catch (System.IO.IOException e) {
                    handleIOError(e);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileCreateTempFile : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                try {
                    string prefix = arguments[0].head().getStringValue();
                    string suffix = arguments[1].head().getStringValue();
                    Saxon.Ejava.io.File temp;
                    if ((arguments.Length == 2)) {
                        temp = Saxon.Ejava.io.File.createTempFile(prefix, suffix);
                    } else {
                        string dir = arguments[2].head().getStringValue();
                        temp = Saxon.Ejava.io.File.createTempFile(prefix, suffix, toFile(dir));
                    }
                    deleteTempFile(context, temp);
                    return new Saxon.Hej.value.StringValue(temp.getCanonicalPath());
                } catch (System.IO.IOException e) {
                    handleIOError(e);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileCurrentDir : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toFile(".");
                return new Saxon.Hej.value.StringValue((file.getAbsolutePath()+((file.isDirectory() ? Saxon.Ejava.io.File.separator : ""))));
            }
        
        }
        public class FileDelete : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = toExistingFile(path);
                bool recurse = false;
                if ((arguments.Length == 2)) {
                    recurse = ((Saxon.Hej.value.BooleanValue)(arguments[1].head())).getBooleanValue();
                }
                if (!(recurse)) {
                    if ((file.isDirectory() && (file.list().Length != 0))) {
                        error(("Cannot delete non-empty directory "+file.getAbsolutePath()), ERROR_PATH_IS_DIRECTORY);
                    }
                    bool result = file.delete();
                    if (!(result)) {
                        error(("Failed to delete file "+file.getAbsolutePath()), ERROR_IO);
                    }
                } else {
                    try {
                        deleteRecursive(file);
                    } catch (System.IO.IOException e) {
                        error(("Failed to delete "+path), e, ERROR_IO);
                    }
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileDirSeparator : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.StringValue(Saxon.Ejava.io.File.separator);
            }
        
        }
        public class FileExists : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toFile(arguments[0].head().getStringValue());
                return Saxon.Hej.value.BooleanValue.get(file.exists());
            }
        
        }
        public class FileIsDir : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toFile(arguments[0].head().getStringValue());
                return Saxon.Hej.value.BooleanValue.get((file.exists() && file.isDirectory()));
            }
        
        }
        public class FileIsFile : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toFile(arguments[0].head().getStringValue());
                return Saxon.Hej.value.BooleanValue.get((file.exists() && file.isFile()));
            }
        
        }
        public class FileLastModified : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toExistingFile(arguments[0].head().getStringValue());
                long dt = file.lastModified();
                return Saxon.Hej.value.DateTimeValue.EPOCH.add(Saxon.Hej.value.DayTimeDurationValue.fromMilliseconds(dt));
            }
        
        }
        public class FileLineSeparator : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.StringValue(NEWLINE);
            }
        
        }
        public class FileList : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = toExistingDirectory(path);
                if (!(file.isDirectory())) {
                    error((("File "+path)+" is not a directory"), ERROR_PATH_NOT_DIRECTORY);
                }
                bool recursive = false;
                if ((arguments.Length>1)) {
                    recursive = ((Saxon.Hej.value.BooleanValue)(arguments[1].head())).getBooleanValue();
                }
                System.Collections.Generic.IList<Saxon.Hej.value.StringValue> result = new System.Collections.Generic.List<Saxon.Hej.value.StringValue>();
                Saxon.Ejava.io.FilenameFilter filter;
                if ((arguments.Length>2)) {
                    filter = Saxon.Hej.functions.URIQueryParameters.makeGlobFilter(arguments[2].head().getStringValue());
                } else {
                    filter = new Inner_FilenameFilter_d508e4927(this);
                }
                gatherDirectoryContents(file, "", recursive, filter, result);
                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
            }
            private static void gatherDirectoryContents(Saxon.Ejava.io.File dir, string prefix, bool recurse, Saxon.Ejava.io.FilenameFilter filter, System.Collections.Generic.IList<Saxon.Hej.value.StringValue> result) {
                Saxon.Hej.functions.URIQueryParameters.RegexFilter filter2 = ((filter is Saxon.Hej.functions.URIQueryParameters.RegexFilter) ? (Saxon.Hej.functions.URIQueryParameters.RegexFilter)(filter) : null);
                foreach (Saxon.Ejava.io.File file in dir.listFiles(filter)) {
                    if ((((filter2 == null) || !(file.isDirectory())) || filter2.matches(file.getName()))) {
                        string name = (prefix.Length==0 ? file.getName() : ((prefix+Saxon.Ejava.io.File.separator)+file.getName()));
                        if (file.isDirectory()) {
                            name += Saxon.Ejava.io.File.separator;
                        }
                        result.Add(new Saxon.Hej.value.StringValue(name));
                    }
                }
                if (recurse) {
                    foreach (Saxon.Ejava.io.File file in dir.listFiles()) {
                        if (file.isDirectory()) {
                            string name = (prefix.Length==0 ? file.getName() : ((prefix+Saxon.Ejava.io.File.separator)+file.getName()));
                            gatherDirectoryContents(file, name, recurse, filter, result);
                        }
                    }
                }
            }
        
            private class Inner_FilenameFilter_d508e4927 : Saxon.Ejava.io.FilenameFilter {
                private readonly FileList outerInstance;
                public Inner_FilenameFilter_d508e4927(FileList outerInstance) {
                    this.outerInstance = outerInstance;
                }
                public bool accept(Saxon.Ejava.io.File dir, string name) {
                    return true;
                }
            }

        }
        public class FileMove : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File from = toExistingFile(arguments[0].head().getStringValue());
                Saxon.Ejava.io.File to = toFile(arguments[1].head().getStringValue());
                if (from.isDirectory()) {
                    if (to.exists()) {
                        if (to.isDirectory()) {
                            from.renameTo(new Saxon.Ejava.io.File(to, from.getName()));
                        } else {
                            error("Target of directory move exists and is not a directory", ERROR_PATH_EXISTS);
                        }
                    } else {
                        to.mkdirs();
                        foreach (Saxon.Ejava.io.File f in from.listFiles()) {
                            f.renameTo(new Saxon.Ejava.io.File(to, f.getName()));
                        }
                    }
                } else {
                    if (to.isDirectory()) {
                        to = new Saxon.Ejava.io.File(to, from.getName());
                    }
                    if (to.exists()) {
                        to.delete();
                    }
                    bool result = from.renameTo(to);
                    if (!(result)) {
                        throw new Saxon.Hej.trans.XPathException((((("Moving "+from)+" to ")+to)+" failed "));
                    }
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileName : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                if ((path.Equals("/") || path.Equals(""))) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                }
                Saxon.Ejava.io.File file = toFile(path);
                return new Saxon.Hej.value.StringValue(file.getName());
            }
        
        }
        public class FileParent : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                if (path.Equals("/")) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Ejava.io.File file = toFile(path);
                try {
                    Saxon.Ejava.io.File parent = new Saxon.Ejava.io.File(file.getParent());
                    return new Saxon.Hej.value.StringValue((parent.getCanonicalPath()+Saxon.Ejava.io.File.separator));
                } catch (System.IO.IOException e) {
                    handleIOError(e);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FilePathSeparator : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.StringValue(System.IO.Path.PathSeparator.ToString());
            }
        
        }
        public class FilePathToNative : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = null;
                if (path.StartsWith("file:/")) {
                    try {
                        System.Uri uri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
                        file = new Saxon.Ejava.io.File(uri);
                        if (!((file.exists()))) {
                            error((("No file exists at "+path)), ERROR_PATH_NOT_EXIST);
                        }
                    } catch (System.UriFormatException err) {
                    }
                }
                if ((file == null)) {
                    file = toExistingFile(path);
                }
                try {
                    return new Saxon.Hej.value.StringValue(file.getCanonicalPath());
                } catch (System.IO.IOException e) {
                    handleIOError(e);
                }
                return null;
            }
        
        }
        public class FilePathToUri : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string filePath = arguments[0].head().getStringValue();
                System.Uri uri;
                if ((Saxon.Ejava.io.File.separator.Equals("\\") && ((filePath.StartsWith("\\\\") || filePath.StartsWith("//"))))) {
                    uri = new Saxon.Ejava.io.File(filePath).toPath().toUri();
                } else {
                    uri = Saxon.Impl.Helpers.UriUtils.Normalize(toFile(filePath).toURI());
                }
                return new Saxon.Hej.value.AnyURIValue(escapeCS(uri.ToString()));
            }
            private static string escapeCS(string s) {
                return System.Uri.EscapeUriString(s);
            }
        
        }
        public class FileReadBinary : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = checkReadSource(path);
                long offset = 0;
                if ((arguments.Length>1)) {
                    offset = ((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue();
                }
                long length = (file.length()-offset);
                if ((arguments.Length>2)) {
                    length = ((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue();
                }
                return readBinary(file, path, offset, length);
            }
            private static Saxon.Hej.value.Base64BinaryValue readBinary(Saxon.Ejava.io.File file, string path, long offset, long length) {
                try {
                    long fileLength = file.length();
                    if ((offset<0L)) {
                        error(("Negative offset in binary read:"+offset), ERROR_INDEX_OUT_OF_BOUNDS);
                    }
                    if ((length<0L)) {
                        error(("Negative length in binary read:"+length), ERROR_INDEX_OUT_OF_BOUNDS);
                    }
                    if (((offset+length)>fileLength)) {
                        error(("Length exceeds file length in binary read:"+((offset+length))), ERROR_INDEX_OUT_OF_BOUNDS);
                    }
                    byte[] content = new byte[(int)(length)];
                    System.IO.FileStream @in = System.IO.File.OpenRead(file.getPath());
                    if ((offset>0)) {
                        long skipped = @in.Seek(offset, System.IO.SeekOrigin.Current);
                        if ((skipped != offset)) {
                            error(((("Error skipping content at start of binary: "+skipped)+" bytes skipped, but requested ")+offset), ERROR_IO);
                        }
                    }
                    int bytes = @in.Read(content, 0, (int)(length));
                    if ((bytes != content.Length)) {
                        @in.Close();
                        error("Number of bytes read does not match file length", ERROR_IO);
                    }
                    @in.Close();
                    return new Saxon.Hej.value.Base64BinaryValue(content);
                } catch (System.IO.IOException e) {
                    error(("Failed to read file "+path), e, ERROR_IO);
                }
                return null;
            }
        
        }
        public class FileReadText : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = checkReadSource(path);
                string encoding = "UTF-8";
                if ((arguments.Length>1)) {
                    encoding = arguments[1].head().getStringValue();
                }
                System.IO.TextReader reader = getInputStreamReader(file, encoding);
                try {
                    string result = Saxon.Hej.functions.UnparsedTextFunction.readFile(context.getConfiguration().getValidCharacterChecker(), reader).ToString();
                    reader.Close();
                    return new Saxon.Hej.value.StringValue(result);
                } catch (Saxon.Hej.trans.XPathException e) {
                    try {
                        reader.Close();
                    } catch (System.IO.IOException e1) {
                        error(("Failed to close file "+file.getAbsolutePath()), e, ERROR_IO);
                    }
                    error(e.Message, ERROR_IO);
                } catch (System.IO.IOException e) {
                    try {
                        reader.Close();
                    } catch (System.IO.IOException e1) {
                        error(("Failed to close file "+file.getAbsolutePath()), e, ERROR_IO);
                    }
                    error(("Failed to read file "+path), e, ERROR_IO);
                }
                return null;
            }
        
        }
        public class FileReadTextLines : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = checkReadSource(path);
                string encoding = "UTF-8";
                if ((arguments.Length>1)) {
                    encoding = arguments[1].head().getStringValue();
                }
                return new Saxon.Hej.om.LazySequence(new Saxon.Eej.functions.extfn.EXPathFileFunctionSet.TextFileLinesIterator(file, encoding));
            }
        
        }
        public class FileResolvePath : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toFile(arguments[0].head().getStringValue());
                return new Saxon.Hej.value.StringValue((file.getAbsolutePath()+((file.isDirectory() ? Saxon.Ejava.io.File.separator : ""))));
            }
        
        }
        public class FileSize : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Ejava.io.File file = toExistingFile(arguments[0].head().getStringValue());
                return new Saxon.Hej.value.Int64Value((file.isDirectory() ? 0 : file.length()));
            }
        
        }
        public class FileTempDir : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.StringValue(Saxon.Ejava.lang.System.getProperty("java.io.tmpdir"));
            }
        
        }
        public class FileWrite : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Hej.om.Sequence contents = arguments[1];
                Saxon.Hej.serialize.SerializationProperties @params;
                if ((arguments.Length>2)) {
                    Saxon.Hej.serialize.SerializationParamsHandler sph = new Saxon.Hej.serialize.SerializationParamsHandler();
                    sph.setSerializationParams((Saxon.Hej.om.NodeInfo)(arguments[2].head()));
                    @params = sph.getSerializationProperties();
                } else {
                    @params = context.getConfiguration().obtainDefaultSerializationProperties();
                }
                serialize(context, path, false, contents, @params);
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileWriteBinary : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = checkWriteDestination(path);
                byte[] content = ((Saxon.Hej.value.Base64BinaryValue)(arguments[1].head())).getBinaryValue();
                if ((arguments.Length == 2)) {
                    try {
                        System.IO.FileStream stream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
                        stream.Write(content);
                        stream.Close();
                    } catch (System.IO.IOException e) {
                        error(("Failed to write to file "+path), e, ERROR_IO);
                    }
                } else {
                    try {
                        long offset = ((Saxon.Hej.value.NumericValue)(arguments[2].head())).longValue();
                        if ((offset<0L)) {
                            error(("Negative offset in binary write:"+offset), ERROR_INDEX_OUT_OF_BOUNDS);
                        }
                        int lb = content.Length;
                        byte[] data;
                        if ((file.exists() && (offset>0))) {
                            long fileLength = file.length();
                            if ((offset>fileLength)) {
                                error(("Offset exceeds file length in binary write:"+offset), ERROR_INDEX_OUT_OF_BOUNDS);
                            }
                            data = new byte[System.Math.Max((int)(fileLength), ((int)(offset)+lb))];
                            System.IO.FileStream @in = System.IO.File.OpenRead(file.getPath());
                            int bytes = @in.Read(data);
                            if ((bytes != fileLength)) {
                                @in.Close();
                                error("Number of bytes read does not match file length", ERROR_IO);
                            }
                            @in.Close();
                        } else {
                            data = new byte[((int)(offset)+lb)];
                        }
                        Saxon.Ejava.lang.System.arraycopy(content, 0, data, (int)(offset), lb);
                        System.IO.FileStream stream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
                        stream.Write(data);
                        stream.Close();
                    } catch (System.IO.IOException e) {
                        error(("Failed to write to file "+path), e, ERROR_IO);
                    }
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileWriteText : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = checkWriteDestination(path);
                string content = arguments[1].head().getStringValue();
                string encoding = "UTF-8";
                if ((arguments.Length>2)) {
                    encoding = arguments[2].head().getStringValue();
                }
                try {
                    System.IO.FileStream stream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
                    System.IO.TextWriter writer = newOutputStreamWriter(stream, encoding);
                    writer.Write(content);
                    writer.Close();
                } catch (Saxon.Ejava.io.UnsupportedEncodingException e) {
                    error(("Unsupported encoding "+encoding), ERROR_UNKNOWN_ENCODING);
                } catch (Saxon.Ejava.io.FileNotFoundException e) {
                    error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
                } catch (System.IO.IOException e) {
                    error(("Failed to write to file "+file.getAbsolutePath()), e, ERROR_IO);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class FileWriteTextLines : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string path = arguments[0].head().getStringValue();
                Saxon.Ejava.io.File file = checkWriteDestination(path);
                Saxon.Hej.om.SequenceIterator contents = arguments[1].iterate();
                string encoding = "UTF-8";
                if ((arguments.Length>2)) {
                    encoding = arguments[2].head().getStringValue();
                }
                try {
                    System.IO.FileStream stream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
                    System.IO.TextWriter writer = newOutputStreamWriter(stream, encoding);
                    Saxon.Hej.om.Item i;
                    while (((i = contents.next()) != null)) {
                        writer.Write(i.getStringValue());
                        writer.Write(NEWLINE);
                    }
                    writer.Close();
                } catch (Saxon.Ejava.io.UnsupportedEncodingException e) {
                    error(("Unsupported encoding "+encoding), ERROR_UNKNOWN_ENCODING);
                } catch (Saxon.Ejava.io.FileNotFoundException e) {
                    error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
                } catch (System.IO.IOException e) {
                    error(("Failed to write to file "+file.getAbsolutePath()), e, ERROR_IO);
                }
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        private static Saxon.Ejava.io.File toFile(string path) {
            Saxon.Ejava.io.File file;
            try {
                file = new Saxon.Ejava.io.File(path);
                if (file.isAbsolute()) {
                    return file;
                }
                bool maybeURI = path.StartsWith("file:");
                if (maybeURI) {
                    System.Uri uri = new System.Uri(System.Text.RegularExpressions.Regex.Replace(path, "\\\\", "/"), System.UriKind.RelativeOrAbsolute);
                    return new Saxon.Ejava.io.File(uri.LocalPath);
                } else {
                    string dir = Saxon.Ejava.lang.System.getProperty("expath.base.directory");
                    if ((dir != null)) {
                        return new Saxon.Ejava.io.File(dir, path).getCanonicalFile();
                    } else {
                        return file.getCanonicalFile();
                    }
                }
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(("URISyntax error in file path:"+path));
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(("IO error in file path:"+path));
            }
        }
        private static Saxon.Ejava.io.File toExistingFile(string path) {
            Saxon.Ejava.io.File file = toFile(path);
            if (!(file.exists())) {
                error(("No file exists at "+path), ERROR_PATH_NOT_EXIST);
            }
            return file;
        }
        private static Saxon.Ejava.io.File checkReadSource(string path) {
            Saxon.Ejava.io.File file = toExistingFile(path);
            if (file.isDirectory()) {
                error((("Path "+path)+" is a directory"), ERROR_PATH_IS_DIRECTORY);
            }
            if (!(file.exists())) {
                error(("File not found: "+path), ERROR_PATH_NOT_EXIST);
            }
            return file;
        }
        private static Saxon.Ejava.io.File checkWriteDestination(string path) {
            Saxon.Ejava.io.File file = toFile(path);
            if (file.isDirectory()) {
                error((("Path "+path)+" is a directory"), ERROR_PATH_IS_DIRECTORY);
            }
            if (!(file.getParentFile().exists())) {
                error(("Parent directory does not exist: "+path), ERROR_PATH_NOT_DIRECTORY);
            }
            return file;
        }
        private static Saxon.Ejava.io.File toExistingDirectory(string path) {
            Saxon.Ejava.io.File file = toFile(path);
            if (!(file.exists())) {
                error(("No directory exists at "+path), ERROR_PATH_NOT_DIRECTORY);
            }
            return file;
        }
        public static void error(string message, string code) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message);
            e.setErrorCodeQName(new Saxon.Hej.om.StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
            throw e;
        }
        public static void error(string message, System.Exception cause, string code) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message, cause);
            e.setErrorCodeQName(new Saxon.Hej.om.StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
            throw e;
        }
        private static void serialize(Saxon.Hej.expr.XPathContext context, string path, bool append, Saxon.Hej.om.Sequence contents, Saxon.Hej.serialize.SerializationProperties properties) {
            Saxon.Hej.lib.SerializerFactory factory = context.getConfiguration().getSerializerFactory();
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            pipe.setXPathContext(context);
            Saxon.Ejava.io.File file = toFile(path);
            if (file.isDirectory()) {
                error((("Path "+path)+" is a directory"), ERROR_PATH_IS_DIRECTORY);
            }
            System.IO.Stream stream = null;
            try {
                if (append) {
                    stream = makeFileAppendOutputStream(file);
                } else {
                    stream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
                }
            } catch (Saxon.Ejava.io.FileNotFoundException e) {
                error(("Failed to write to file "+path), e, ERROR_UNKNOWN_ENCODING);
            }
            Saxon.Ejavax.xml.transform.Result result = new Saxon.Ejavax.xml.transform.stream.StreamResult(stream);
            Saxon.Hej.@event.Receiver sr = factory.getReceiver(result, properties, pipe);
            sr.open();
            sr.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            Saxon.Hej.om.SequenceIterator contentIter = contents.iterate();
            Saxon.Hej.om.Item item;
            while (((item = contentIter.next()) != null)) {
                sr.append(item);
            }
            sr.endDocument();
            sr.close();
            try {
                if ((stream != null)) {
                    stream.Close();
                }
            } catch (System.IO.IOException e) {
            }
        }
        private static void copyDirectory(Saxon.Ejava.io.File from, Saxon.Ejava.io.File to) {
            foreach (Saxon.Ejava.io.File child in from.listFiles()) {
                Saxon.Ejava.io.File target = new Saxon.Ejava.io.File(to, child.getName());
                if (child.isDirectory()) {
                    if (target.exists()) {
                        if (target.isDirectory()) {
                            copyDirectory(child, target);
                        } else {
                            target.delete();
                            target.mkdir();
                            copyDirectory(child, target);
                        }
                    } else {
                        target.mkdir();
                        copyDirectory(child, target);
                    }
                } else {
                    if ((target.exists() && target.isDirectory())) {
                        error((((("Cannot copy "+child.getAbsolutePath())+" because directory ")+target.getAbsolutePath())+" is in the way"), ERROR_PATH_IS_DIRECTORY);
                    } else {
                        copyFile(child, target);
                    }
                }
            }
        }
        private static void copyFile(Saxon.Ejava.io.File sourceFile, Saxon.Ejava.io.File destFile) {
            System.IO.File.Copy(sourceFile.getPath(), destFile.getPath(), true);
        }
        private static void deleteTempFile(Saxon.Hej.expr.XPathContext context, Saxon.Ejava.io.File temp) {
            bool d = context.getConfiguration().getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.EXPATH_FILE_DELETE_TEMPORARY_FILES);
            if (d) {
                temp.deleteOnExit();
            }
        }
        private static void deleteRecursive(Saxon.Ejava.io.File file) {
            if (file.isDirectory()) {
                foreach (Saxon.Ejava.io.File child in file.listFiles()) {
                    deleteRecursive(child);
                }
            }
            bool ok = file.delete();
            if (!(ok)) {
                throw new System.IO.IOException(("Failed to delete "+file.getAbsolutePath()));
            }
        }
        private static void handleIOError(System.IO.IOException err) {
            Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(err);
            xe.setErrorCodeQName(new Saxon.Hej.om.StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, ERROR_IO));
            throw xe;
        }
        private class TextFileLinesIterator : Saxon.Hej.tree.iter.TextLinesIterator {
            internal Saxon.Ejava.io.File file;
            internal string encoding;
            public TextFileLinesIterator(Saxon.Ejava.io.File file, string encoding)  {
                this.encoding = encoding;
                this.file = file;
                reader = new Saxon.Impl.Helpers.LineNumberReader(getInputStreamReader(file, encoding));
                uri = file.toURI();
                checker = Saxon.Hej.z.IntPredicateLambda.of(ch => Saxon.Hej.serialize.charcode.XMLCharacterData.isValid11(ch));
            }
            public override Saxon.Hej.value.StringValue next() {
                Saxon.Hej.value.StringValue result = null;
                result = ((/*1430*/Saxon.Hej.value.StringValue)base.next());
                return result;
            }
        
        }
        private static System.IO.TextReader getInputStreamReader(Saxon.Ejava.io.File file, string encoding) {
            return file.getInputStreamReader(encoding);
        }
        public static string _parent(string path) {
            if (path.Equals("/")) {
                return null;
            }
            Saxon.Ejava.io.File file = toFile(path);
            try {
                Saxon.Ejava.io.File parent = new Saxon.Ejava.io.File(file.getParent());
                return (parent.getCanonicalPath()+Saxon.Ejava.io.File.separator);
            } catch (System.IO.IOException e) {
            }
            return null;
        }
    
        private class Inner_FilenameFilter_d508e4927 : Saxon.Ejava.io.FilenameFilter {
            private readonly FileList outerInstance;
            public Inner_FilenameFilter_d508e4927(FileList outerInstance) {
                this.outerInstance = outerInstance;
            }
            public bool accept(Saxon.Ejava.io.File dir, string name) {
                return true;
            }
        }

    }
}