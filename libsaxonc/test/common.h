//
// Created by Matthew Patterson on 21/01/2026.
//

#include <string>

#ifndef SAXONC_COMMON_H
#define SAXONC_COMMON_H

#ifdef _WIN32
const std::string path_sep = "\\";
#else
const std::string path_sep = "/";
#endif

#endif //SAXONC_COMMON_H