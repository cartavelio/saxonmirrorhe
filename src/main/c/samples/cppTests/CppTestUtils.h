////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2021 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef CPPTESTS_CPPTESTUTILS_H
#define CPPTESTS_CPPTESTUTILS_H

#ifdef WINDOWS
#include <direct.h>
#define GetCurrentDir _getcwd
#else
#include <unistd.h>
#define GetCurrentDir getcwd
#endif
#include <list>


typedef struct
{
    int success;
    int failure;
    std::list<std::string> failureList;

}sResultCount;


class CppTestUtils {

public:
static int exists(const char *fname)
{
    FILE *file;
    file = fopen(fname, "r");
    if (file)
    {
        fclose(file);
        return 1;
    }
    return 0;
}

private:

};

#endif //CPPTESTS_CPPTESTUTILS_H
