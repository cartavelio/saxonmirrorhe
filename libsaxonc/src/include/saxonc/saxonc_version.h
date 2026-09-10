////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 - 2026 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License,
// v. 2.0. If a copy of the MPL was not distributed with this file, You can
// obtain one at http://mozilla.org/MPL/2.0/. This Source Code Form is
// "Incompatible With Secondary Licenses", as defined by the Mozilla Public
// License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "saxonc_export.h"

#ifndef SAXONC_VERSION_H
#define SAXONC_VERSION_H

#ifdef __cplusplus
#define EXTERN_SAXONC extern "C" {
#define EXTERN_SAXONC_END }
#else
#define EXTERN_SAXONC
#define EXTERN_SAXONC_END
#endif

EXTERN_SAXONC

SAXONC_EXPORT char*    get_saxonc_version();
SAXONC_EXPORT unsigned get_saxonc_version_major();
SAXONC_EXPORT unsigned get_saxonc_version_minor();
SAXONC_EXPORT unsigned get_saxonc_version_patch();
SAXONC_EXPORT unsigned get_saxonc_version_tweak();

EXTERN_SAXONC_END

#endif //SAXONC_VERSION_H
