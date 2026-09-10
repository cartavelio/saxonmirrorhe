package com.saxonica.ee.bytecode.util;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * GeneratedClassLoader interface. This interface provides access to method whose implementation depends on the chosen platform
 * (typically Java or .NET)
 */
public class GeneratedClassLoader extends ClassLoader {

    Map<String, Class> classMap = new ConcurrentHashMap<>();

    public GeneratedClassLoader(ClassLoader parentClassLoader) {
        super(parentClassLoader);
    }

    /**
     * Creates a class from the supplied byte array and registers it for later use.
     *
     * @param name      The name of the class to register
     * @param classFile The class file as a byte array
     * @since 9.6.0.3
     */


    public void registerClass(String name, byte[] classFile) {
        if (!classMap.containsKey(name)) {
            Class classi = defineClass(name, classFile, 0, classFile.length);
            classMap.put(name, classi);
        }
    }

    @Override
    protected Class<?> findClass(String name) throws ClassNotFoundException {
        if (classMap.containsKey(name)) {
            return classMap.get(name);
        } else {
            return super.findClass(name);
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited