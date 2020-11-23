#!/bin/bash

JC=/usr/local/jet15.3/bin/jc

if [ "X$PROJECT" = "X" ]; then
    echo "PROJECT environment variable required."
    exit 1
fi

if [ ! -f "$JC" -o ! -x "$JC" ]; then
    echo "Excelsior Jet is not available."
    exit 1
fi

if [ ! -d "/saxonica" -o ! -f "/saxonica/build.gradle" ]; then
    echo "The repository must be mounted on the /saxonica volume."
    exit 1
fi

if [ ! -d "/saxonica/build/jet" ]; then
    echo "The build is not configured; build/jet does not exist."
    exit 1
fi

cd /saxonica/build/jet

if [ ! -f "$PROJECT" -a -f "${PROJECT}.prj" ]; then
    PROJECT="${PROJECT}.prj"
fi

if [ ! -f "$PROJECT" ]; then
    echo "No such project: $PROJECT"
    exit 1
fi

exec $JC =p $PROJECT
