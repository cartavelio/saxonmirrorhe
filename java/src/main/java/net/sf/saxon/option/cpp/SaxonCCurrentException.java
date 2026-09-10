// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import com.oracle.svm.core.Uninterruptible;

public class SaxonCCurrentException {
    private Throwable throwable = null;
    public static SaxonCCurrentException current = new SaxonCCurrentException();

    public SaxonCCurrentException() {}

    @Uninterruptible(reason = "exception handler")
    public void setThrowable(Throwable throwable) {
        this.throwable = throwable;
    }

    public Throwable getThrowable() {
        return this.throwable;
    }

    public static Throwable get(){
        return current.throwable;
    }

    public static void clear(){
        current.throwable = null;
    }

    @Uninterruptible(reason = "exception handler")
    public static SaxonCCurrentException getInstance(){
        return current;
    }


}
