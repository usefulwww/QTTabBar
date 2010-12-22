//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

#include <windows.h>
#include <msi.h>
#include <msiquery.h>
#include <shlobj.h>
#include <exdisp.h>
#include <Shlwapi.h>
#include <tchar.h>
#include <vector>

#define WIXAPI __stdcall

#pragma comment(lib, "msi.lib")
#pragma comment(lib, "shlwapi.lib")

UINT WM_SHOWHIDEBARS = RegisterWindowMessageA("QTTabBar_ShowHideBars");

struct PairHwndPath {
    HWND hwnd;
    TCHAR path[MAX_PATH];
};

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
	switch (ul_reason_for_call) {
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

void GetExplorerWindows(std::vector<PairHwndPath>& windows, BOOL needPaths) {
    IShellWindows *psw;
    if(SUCCEEDED(CoCreateInstance(CLSID_ShellWindows, NULL, CLSCTX_ALL, IID_IShellWindows, (void**)&psw))) {
        VARIANT v;
        V_VT(&v) = VT_I4;
        IDispatch* pdisp;
        for(V_I4(&v) = 0; psw->Item(v, &pdisp) == S_OK; V_I4(&v)++) {
            IWebBrowserApp *pwba;
            if(SUCCEEDED(pdisp->QueryInterface(IID_IWebBrowserApp, (void**)&pwba))) {
                PairHwndPath pair;
                if(SUCCEEDED(pwba->get_HWND((LONG_PTR*)&pair.hwnd))) {
                    IServiceProvider *psp;
                    if(needPaths && SUCCEEDED(pwba->QueryInterface(IID_IServiceProvider, (void**)&psp))) {
                        IShellBrowser *psb;
                        if(SUCCEEDED(psp->QueryService(SID_STopLevelBrowser, IID_IShellBrowser, (void**)&psb))) {
                            IShellView *psv;
                            if(SUCCEEDED(psb->QueryActiveShellView(&psv))) {
                                IFolderView *pfv;
                                if(SUCCEEDED(psv->QueryInterface(IID_IFolderView, (void**)&pfv))) {
                                    IPersistFolder2 *ppf2;
                                    if(SUCCEEDED(pfv->GetFolder(IID_IPersistFolder2, (void**)&ppf2))) {
                                        LPITEMIDLIST pidlFolder;
                                        if(SUCCEEDED(ppf2->GetCurFolder(&pidlFolder))) {
                                            if(!SHGetPathFromIDList(pidlFolder, pair.path)) {
                                                IShellFolder* psf;
                                                LPCITEMIDLIST pidlLast;
                                                if(SUCCEEDED(SHBindToParent(pidlFolder, IID_IShellFolder, (void**)&psf, &pidlLast))) {
                                                    STRRET strret;
                                                    if(SUCCEEDED(psf->GetDisplayNameOf(pidlLast, 0x8000, &strret))) {
                                                        StrRetToBuf(&strret, pidlLast, pair.path, MAX_PATH);
                                                    }
                                                    else {
                                                        pair.path[0] = 0;
                                                    }
                                                    psf->Release();
                                                }
                                            }
                                            CoTaskMemFree(pidlFolder);
                                        }
                                        ppf2->Release();
                                    }
                                    pfv->Release();
                                }
                                psv->Release();
                            }
                            psb->Release();
                        }
                        psp->Release();
                    }
                    windows.push_back(pair);
                }
                pwba->Release();
            }
            pdisp->Release();
        }
        psw->Release();
    }
}

UINT WIXAPI HideBars(MSIHANDLE hInstaller) {
    std::vector<PairHwndPath> windows;
    BOOL rollback = MsiGetMode(hInstaller, MSIRUNMODE_ROLLBACK);
    GetExplorerWindows(windows, false);
    for(UINT i = 0; i < windows.size(); ++i) {
        HWND hwnd = GetParent(windows[i].hwnd);
        if(hwnd == 0) hwnd = windows[i].hwnd;
        SendMessage(hwnd, WM_SHOWHIDEBARS, rollback ? 1 : 0, 0);
    }
    return ERROR_SUCCESS;
}

UINT WIXAPI CloseAndReopen(MSIHANDLE hInstaller) {
    std::vector<PairHwndPath> windows;
    BOOL rollback = MsiGetMode(hInstaller, MSIRUNMODE_ROLLBACK);
    GetExplorerWindows(windows, true);
    if(windows.size() == 0) return ERROR_SUCCESS;
    int length = 0;
    for(UINT i = 0; i < windows.size(); ++i) {
        HWND hwnd = GetParent(windows[i].hwnd);
        if(hwnd == 0) hwnd = windows[i].hwnd;
        SendMessage(hwnd, WM_CLOSE, 0, 0);
        int l = _tcslen(windows[i].path);
        if(l > 0 && i > 0) length += l + 1;
    }
    TCHAR* build = new TCHAR[length + 1];
    build[0] = 0;
    for(UINT i = 1; i < windows.size(); ++i) {
        UINT l = _tcslen(windows[i].path);
        _tcscat(build, windows[i].path);
        _tcscat(build, _T(";"));
    }
    HKEY key;
    REGSAM access = KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_WOW64_64KEY;
    if(RegOpenKeyEx(HKEY_CURRENT_USER, _T("Software\\Quizo\\QTTabBar\\"), 0, access, &key) == ERROR_SUCCESS) {
        RegSetValueEx(key, _T("TabsOnLastClosedWindow"), 0, REG_SZ, (LPBYTE)build, length + 1);
    }
    RegCloseKey(key);
    delete[] build;
    ShellExecute(NULL, NULL, windows[0].path, NULL, NULL, SW_SHOWNORMAL);
    return ERROR_SUCCESS;
}

