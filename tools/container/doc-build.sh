#!/bin/bash

. "bin/parse-build-args.sh"

parse_build_args "$@"

if [ "$ABORT" != "0" ]; then
    echo "Abort."
    exit 1
fi

SRC=saxondev-java-$BRANCH
CONTAINER=saxonica-docbuild
PLATFORM=linux/amd64
if [ "$ARCH" != "x64" ]; then
    PLATFORM="linux/$ARCH"
fi

if [ "$DEBUG" = "1" ]; then
    echo "Running in debug mode; starting a shell"
    RUN=/bin/bash
    ARGS=""
else
    echo "Building the $BRANCH branch"
    RUN=/root/build-saxon.sh
    ARGS="$@"

    VOLS="w3c-test-suites saxondev-$BRANCH saxondev-gradle"
    if [ "$SAXONC" = "1" -o "$DOXYGEN" = "1" -o "$PYDOC" = "1" ]; then
        VOLS="$VOLS graalvm22"
    fi

    for vol in $VOLS; do
        if [ `docker volume ls | grep local | grep $vol | wc -l` = 0 ]; then
            echo "Cannot find volume: $vol" 1>&2
            ABORT=1
        fi
    done
fi

if [ "$ABORT" != "0" ]; then
    echo "Invalid Docker configuration."
    exit 1
fi

for vol in saxondev-java-results $SRC; do
    if [ `docker volume ls | grep local | grep $vol | wc -l` = 0 ]; then
        docker volume create $vol
    fi
done

if [ "$SAXON_LICENSE_DIR" = "" ]; then
    echo "SAXON_LICENSE_DIR environment variable must be set"
    exit 1
fi

docker run --rm --platform $PLATFORM \
  -v $SAXON_LICENSE_DIR:/config \
  --mount source=w3c-test-suites,ro,target=/test-suites \
  --mount source=saxondev-main,ro,target=/repository \
  --mount source=$SRC,target=/src \
  --mount source=saxondev-java-results,target=/testdriver-results \
  --mount source=saxondev-gradle,target=/gradle \
  --mount source=graalvm22,target=/graal \
  -e GRADLE_USER_HOME=/gradle \
  -e SAXON_LICENSE_DIR=/config \
  -e W3C_QT3_TEST_SUITE_DIR=/test-suites/qt3tests \
  -e W3C_QT4_TEST_SUITE_DIR=/test-suites/qt4tests \
  -e W3C_XSD_TEST_SUITE_DIR=/test-suites/xsdtests \
  -e W3C_XSLT30_TEST_SUITE_DIR=/test-suites/xslt30-test \
  -e W3C_XSLT40_TEST_SUITE_DIR=/test-suites/xslt40-test \
  -e W3C_EXPATH_TEST_SUITE_DIR=/test-suites/expath-cg/tests/qt3 \
  -e SAXON_EXTRA_TEST_SUITE_DIR=/test-suites/extra-test-suites \
  $CONTAINER $RUN $ARGS

if [ "$OUTPUT" != "" ]; then
    docker run --mount source=$SRC,target=/src --name copy-helper -d busybox sleep 3600

    if [ "$SAXONJ" = "1" ]; then
        mkdir -p $OUTPUT/releases || exit 1
        pushd $OUTPUT >/dev/null 2>&1 || exit 1
        docker exec copy-helper /bin/sh -c 'cd /src/build && tar cf - releases' | tar vxf -
        popd >/dev/null 2>&1 || exit 1
    fi
    if [ "$SAXONC" = "1" ]; then
        mkdir -p $OUTPUT/c || exit 1
        pushd $OUTPUT >/dev/null 2>&1 || exit 1
        docker exec copy-helper /bin/sh -c 'cd /src/build && tar cf - c/libsaxon-*' | tar vxf -
        popd >/dev/null 2>&1 || exit 1
    fi
    if [ "$PYTHON" = "1" ]; then
        mkdir -p $OUTPUT || exit 1
        pushd $OUTPUT >/dev/null 2>&1 || exit 1
        if [ "$HE" = "1" ]; then
            docker exec copy-helper /bin/sh -c 'cd /src/build && tar cf - hec_pypi' | tar vxf -
        fi
        if [ "$PE" = "1" ]; then
            docker exec copy-helper /bin/sh -c 'cd /src/build && tar cf - pec_pypi' | tar vxf -
        fi
        if [ "$EE" = "1" ]; then
            docker exec copy-helper /bin/sh -c 'cd /src/build && tar cf - eec_pypi' | tar vxf -
        fi
        popd >/dev/null 2>&1 || exit 1
    fi
    if [ "$WHEELS" = "1" ]; then
        mkdir -p $OUTPUT || exit 1
        pushd $OUTPUT >/dev/null 2>&1 || exit 1
        docker cp copy-helper:/src/build/distributions/linux.wheels.tar.gz .
        popd >/dev/null 2>&1 || exit 1
    fi
    if [ "$PYDOC" = "1" ]; then
        mkdir -p $OUTPUT || exit 1
        pushd $OUTPUT >/dev/null 2>&1 || exit 1
        docker cp copy-helper:/src/build/doc_pypi/saxonc.html .
        popd >/dev/null 2>&1 || exit 1
    fi
    if [ "$DOXYGEN" = "1" ]; then
        mkdir -p $OUTPUT || exit 1
        pushd $OUTPUT >/dev/null 2>&1 || exit 1
        docker exec copy-helper /bin/sh -c 'cd /src/build/doc_pypi && tar cf - documentation' | tar vxf -
    fi

    docker stop copy-helper > /dev/null
    docker rm copy-helper > /dev/null
fi

