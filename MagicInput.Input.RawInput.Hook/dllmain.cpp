// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdafx.h"
#include "dllmain.h"

#pragma comment(linker, "/section:shared,rws")
#pragma data_seg("shared")

static HWND g_hServerWnd = NULL;
static HHOOK g_hMouseHook = NULL;
static HHOOK g_hKeyboardHook = NULL;

#pragma data_seg()

static HINSTANCE g_hDllModule;

const int WM_APP_MOUSE = WM_APP + 1;
const int WM_APP_KEYBOARD = WM_APP + 2;
const int sendMessageMillisecondsTimeout = 150;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
			g_hDllModule = (HINSTANCE)hModule;

			break;
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
			break;
		case DLL_PROCESS_DETACH:
			break;
	}

	return TRUE;
}

LRESULT CALLBACK MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	DWORD_PTR result;

	if (nCode >= 0 && SendMessageTimeout(g_hServerWnd, WM_APP_MOUSE, wParam, lParam, SMTO_ABORTIFHUNG, sendMessageMillisecondsTimeout, &result) && result)
		return result;

	return CallNextHookEx(g_hMouseHook, nCode, wParam, lParam);
}

LRESULT CALLBACK KeyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	DWORD_PTR result;

	if (nCode >= 0 && SendMessageTimeout(g_hServerWnd, WM_APP_KEYBOARD, wParam, lParam, SMTO_ABORTIFHUNG, sendMessageMillisecondsTimeout, &result) && result)
		return result;

	return CallNextHookEx(g_hKeyboardHook, nCode, wParam, lParam);
}

extern "C" __declspec(dllexport) BOOL WINAPI LoadHook(HWND hServerWnd, BOOL installMouseHook, BOOL installKeyboardHook)
{
	if (!g_hServerWnd)
	{
		g_hServerWnd = hServerWnd;

		if (!g_hMouseHook && installMouseHook)
			g_hMouseHook = SetWindowsHookEx(WH_MOUSE, MouseHookProc, g_hDllModule, 0);

		if (!g_hKeyboardHook && installKeyboardHook)
			g_hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD, KeyboardHookProc, g_hDllModule, 0);

		return g_hMouseHook && g_hKeyboardHook;
	}

	return FALSE;
}

extern "C" __declspec(dllexport) BOOL WINAPI UnloadHook()
{
	if (g_hMouseHook && UnhookWindowsHookEx(g_hMouseHook))
		g_hMouseHook = NULL;

	if (g_hKeyboardHook && UnhookWindowsHookEx(g_hKeyboardHook))
		g_hKeyboardHook = NULL;

	if (!g_hMouseHook && !g_hKeyboardHook)
	{
		g_hServerWnd = NULL;

		return TRUE;
	}

	return FALSE;
}