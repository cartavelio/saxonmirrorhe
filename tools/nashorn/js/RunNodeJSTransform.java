
package com.saxonica.js;

import jdk.nashorn.api.scripting.JSObject;
import jdk.nashorn.api.scripting.ScriptObjectMirror;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmValue;
import net.sf.saxon.trans.XPathException;

import javax.script.Invocable;
import javax.script.ScriptEngine;
import javax.script.ScriptEngineManager;
import javax.script.ScriptException;

public class RunNodeJSTransform extends RunJSTransform {

    public RunNodeJSTransform(String jsbase) throws ScriptException {
        super(jsbase);

    }

    public static void main(String[] args) throws Exception {
        String jsbase = args.length > 2 ? args[2] : System.getProperty("user.dir");
        RunNodeJSTransform t = new RunNodeJSTransform(jsbase);
        t.setStylesheetFileName(args[0]);
        t.setSourceFileName(args[1]);
        t.run();
    }

    @Override
    public void init(String jsbase) throws ScriptException {
        if (invocable == null) {
            if (jsbase == null) {
                jsbase = System.getProperty("user.dir");
            }
            ScriptEngine engine = new ScriptEngineManager().getEngineByName("nashorn");
            if (engine == null) {
                throw new RuntimeException("No nashorn");
            }
            jsbase = jsbase.replaceAll("\\\\","/");
            JSHelper.jsBase = "file://" + jsbase + "/src/js/src/";

            //engine.eval("load('/Users/mike/bin/closure-library-master/closure/goog/base.js');");
            //engine.eval("load('" + jsbase + "/src/js/nashorn/xmldom/sax.js');");
            engine.eval("load('" + jsbase + "/src/js/nashorn/xmldom/dom.js');");
            //engine.eval("load('" + jsbase + "/src/js/nashorn/xmldom/dom-parser.js');");
            engine.eval("load('" + jsbase + "/src/js/nashorn/xmldom/sax2.js');");
            engine.eval("load('" + jsbase + "/src/js/nashorn/NashornPlatform.js');");
            engine.eval("load('" + jsbase + "/releases/js/2.0/SaxonJS.min.js');");
            engine.eval("var SaxonJS = HostPlatform['entryPoint']");
            engine.eval("load('" + jsbase + "/src/js/nashorn/JSTestDriver.js');");


            invocable = (Invocable) engine;
            try {
                driver = (JSObject)engine.eval("JSTestDriver");
                invocable.invokeMethod(driver, "reset");
                invocable.invokeMethod(driver, "setJSBase", "file://" + jsbase + "/src/js/src/");
            } catch (NoSuchMethodException e) {
                throw new AssertionError(e);
            }
        }

    }

    @Override
    public XdmValue deliverPrincipalResult(Object principalResult, boolean nativeResult, ScriptObjectMirror options) {
        if (nativeResult) {
            try {
                return super.getPrincipalResult(options);
            } catch (XPathException e) {
                e.printStackTrace();
                return null;
            }

        } else {
            System.err.println("Result: " + principalResult);
            return new XdmAtomicValue(principalResult.toString());
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited

