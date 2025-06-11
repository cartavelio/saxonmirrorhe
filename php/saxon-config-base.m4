dnl config.m4 for extension saxon
SAXON_VERSION="12.6.0"

PHP_ARG_WITH([saxon],
  [for saxon support],
  [AS_HELP_STRING([--with-saxon],
    [Include saxon support])])

if test "$PHP_SAXON" != "no"; then
  dnl Write more examples of tests here...

  dnl --with-saxon -> check with-path
  SEARCH_PATH="/opt/saxonica /usr/local /usr" # If SaxonC has been installed
  SEARCH_FOR="/include/saxonc/SaxonProcessor.h"
  if test -r $PHP_SAXON/$SEARCH_FOR; then # path given as parameter
    SAXON_DIR=$PHP_SAXON
  else # search default path list
    AC_MSG_CHECKING([for Saxon files in default path])
    for i in $SEARCH_PATH ; do
      if test -r $i/$SEARCH_FOR; then
        SAXON_DIR=$i
        AC_MSG_RESULT(found in $i)
      fi
    done
  fi

  if test -z "$SAXON_DIR"; then
    AC_MSG_RESULT([not found])
    AC_MSG_ERROR([Please reinstall the SaxonC distribution])
  fi


  SAXON_SHARED_LIB_SEARCH="libsaxonc-core libsaxonc"
  if test "$SHLIB_SUFFIX_NAME" == "dylib" ; then
    SAXON_SHARED_LIB_SUFFIX="$SAXON_EDITION.$SAXON_VERSION.dylib"
  else
    SAXON_SHARED_LIB_SUFFIX="$SAXON_EDITION.so.$SAXON_VERSION"
  fi
  AC_MSG_CHECKING([for Saxon shared libs files in default path])
  for l in $SAXON_SHARED_LIB_SEARCH ; do
    if test ! -f "$SAXON_DIR/lib/$l-$SAXON_SHARED_LIB_SUFFIX" ; then
      AC_MSG_RESULT([$SAXON_DIR/lib/$l-$SAXON_SHARED_LIB_SUFFIX not found])
      AC_MSG_ERROR([Please make sure you have installed the SaxonC $SAXON_VERSION distribution])
    fi
  done
  CXXFLAGS="-std=c++14"
  PHP_REQUIRE_CXX()
  PHP_ADD_INCLUDE(["$SAXON_DIR/include"])
  PHP_SUBST(SAXON_SHARED_LIBADD)
  PHP_SUBST(PHP_SAXONC_API)

  dnl In case of no dependencies
  AC_DEFINE(HAVE_SAXON, 1, [ Have saxon support ])

  PHP_ADD_LIBRARY(stdc++, 1, SAXON_SHARED_LIBADD)
  PHP_ADD_LIBRARY(dl, 1, SAXON_SHARED_LIBADD)
  PHP_ADD_LIBRARY(saxonc-$SAXON_EDITION, 1, SAXON_SHARED_LIBADD)
  PHP_ADD_LIBRARY(saxonc-core-$SAXON_EDITION, 1, SAXON_SHARED_LIBADD)
  PHP_NEW_EXTENSION(saxon, ../src/php8_saxon.cpp, $ext_shared)
fi
