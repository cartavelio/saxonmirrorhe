#include <stdio.h> /* defines FILENAME_MAX */
#if defined(__APPLE__) || defined __linux__
#include <unistd.h>
#define GetCurrentDir getcwd
#else
#include <direct.h>
#define GetCurrentDir _getcwd
#endif

#include "saxonc/SaxonCProcessor.h"

int exists(const char *fname) {
  FILE *file;
  if ((file = fopen(fname, "r"))) {
    fclose(file);
    return 1;
  }
  return 0;
}

void path_join(char* result, const char* segment1, const char* segment2) {
  int path_len = strlen(segment1) + strlen(segment2) + 1;
  snprintf(result, path_len, "%s%s", segment1, segment2);
}

int main(int argc, char *argv[]) {
  const char * const data_dir = argc > 1 ? argv[1] : "";

  char cwd[FILENAME_MAX]; // create string buffer to hold path
  GetCurrentDir(cwd, FILENAME_MAX);
  printf("CWD = %s\n", cwd);
  int cap = 10;
  sxnc_parameter *parameters;
  int parLen = 0, parCap;
  parCap = cap;
  sxnc_property *properties;
  int propLen = 0;
  parCap = cap;
  sxnc_environment *environi;
  sxnc_processor *processor;

  initSaxonc(&environi, &processor, &parameters, &properties, parCap, parCap);

  create_graalvm_isolate(environi);

  int checkProc = c_createSaxonProcessor(environi, processor, 0);

  if (!checkProc) {
    const char *errorMessage = c_getErrorMessage(environi);
    if (errorMessage) {
      printf("Error message: %s \n", errorMessage);
    }
    return -1;
  }

  const char *verCh = getProductVariantAndVersion(environi, processor);
  printf("XSLT Tests\n\nSaxon version: %s \n", verCh);

  char cat_xml_file[FILENAME_MAX];
  path_join(cat_xml_file, data_dir, "/cat.xml");
  char test_xsl_file[FILENAME_MAX];
  path_join(test_xsl_file, data_dir, "/test.xsl");

  const char *result =
      xsltApplyStylesheet(environi, processor, cwd, cat_xml_file,
                          test_xsl_file, 0, 0, 0, 0);

  xsltSaveResultToFile(environi, processor, cwd, cat_xml_file,
                       test_xsl_file, "outputFile.xml", 0, 0, 0, 0);

  if (!result) {
    printf("result is null \n");
    const char *errorMessage = c_getErrorMessage(environi);
    if (errorMessage) {
      printf("Error message %s \n", errorMessage);
    }
  } else {
    printf("Test 1 Pass: %s", result);
  }

  if (exists("outputFile.xml")) {
    printf("Test 2 - Pass: outputFile.xml exists \n");
  } else {
    printf("Test 2 - Fail: outputFile.xml not found \n");
  }

  fflush(stdout);

  graal_tear_down(environi->thread);
  return 0;
}
