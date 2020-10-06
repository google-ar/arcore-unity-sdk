Google ARCore Camera Utility Plugin Source Code
================================================
Copyright 2017 Google LLC

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

## Overview
The arcore_camera_unity.zip file contains source code of the libarcore_camera_utility.so
C library. This library is used to create the Unity plugin for reading image pixels from
ARCore GPU texture.

## Files Included
- include/camera_utility.h : header file of the C API interface.
- include/gl_utility.h : header file of GL utility functions.
- include/texture_reader.h : header file of C++ API interface.
- src/camera_utility.cc : implementation of the C API.
- src/gl_utility.cc : implementation of GL utility functions.
- src/texture_reader.cc : implementation of C++ API.

## How to Build
You can build the library by using Google Android NDK(https://developer.android.com/ndk/index.html).
Google Android NDK can be downloaded from this URL(https://developer.android.com/ndk/downloads/index.html).
