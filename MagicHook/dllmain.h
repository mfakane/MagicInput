#include "stdafx.h"
#pragma once

extern "C" __declspec(dllexport) BOOL WINAPI LoadHook(HWND hServerWnd, BOOL installMouseHook, BOOL installKeyboardHook);
extern "C" __declspec(dllexport) BOOL WINAPI UnloadHook();
