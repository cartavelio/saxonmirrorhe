/**
 * <p>This package contains defines the two interfaces Watch and WatchMaker, and various
 * implementations of these interfaces.</p>
 * <p>These classes are used internally by Saxon-EE to support streamed execution. None of these
 * classes is intended for direct use by user applications.</p>
 * <p>A Watch is used on a streaming pipeline to recognize specific events, such as the start
 * of a selected element, and to take action when these events occur. A WatchMaker is a factory
 * class for creating a particular kind of Watch. The WatchManager is a class that sits on the Receiver
 * pipeline keeping track of which Watches are active, and notifying them of events so that they can
 * decide whether they are of interest.</p>
 */
package com.saxonica.ee.stream.watch;

// Copyright (c) 2010-2023 Saxonica Limited.