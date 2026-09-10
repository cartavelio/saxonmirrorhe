package com.saxonica.functions.extfn.EXPathArchive;

import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Base64BinaryValue;


/* A class to represent an entry in the archive - 
 * by using this we can abstract away from how the calls are being made,
 * be it with simple strings, maps or elements.
 * */
public class Entry {
    public Base64BinaryValue content;
    public String name;
    public String encoding;
    public Archive.CompressionType compression = Archive.CompressionType.DEFLATE;

    public Entry() {
    }

    public Entry(String name, Base64BinaryValue content) {
        this.name = name;
        this.content = content;
    }

    public Entry(Base64BinaryValue content) {
        this.content = content;
    }

    public Entry(String name) {
        this.name = name;
    }

    public Entry(Entry e) {
        this.name = e.name;
        this.encoding = e.encoding;
        this.compression = e.compression;
    }

    public Entry(Entry e, Base64BinaryValue content) {
        this.name = e.name;
        this.encoding = e.encoding;
        this.compression = e.compression;
        this.content = content;
    }

    // Copying an encoding from a default
    public Entry(Entry e, String name) {
        this.name = name;
        if (e != null) {
            this.encoding = e.encoding;
            this.compression = e.compression;
        }
    }

    @Override
    public boolean equals(Object o) {
        Entry e = (Entry) o;
        return e != null && name.equals(e.name);
    }

    public void setEntry(Base64BinaryValue content) {
        this.content = content;
    }

    public void setEncoding(String encoding) {
        this.encoding = encoding;
    }

    public void setCompression(Archive.CompressionType compression) {
        this.compression = compression;
    }

    public boolean isDir() {
        return name != null && name.endsWith("/");
    }

    public HashTrieMap makeMap(XPathContext context) throws XPathException {
        HashTrieMap map = new HashTrieMap();
        map = Archive.add(map, "name", name);
        if (content != null) {
            map = Archive.add(map, "content", content);
        }
        return map;
    }
}

// Copyright (c) 2018-2023 Saxonica Limited