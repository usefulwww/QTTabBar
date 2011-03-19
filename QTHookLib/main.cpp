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

#include <Windows.h>
#include <ShObjIdl.h>
#include <Shlobj.h>
#include "MinHook.h"

#if defined _M_X64
    #pragma comment(lib, "libMinHook.x64.lib")
#elif defined _M_IX86
    #pragma comment(lib, "libMinHook.x86.lib")
#endif

#define SFVM_LISTREFRESHED 17

extern "C" __declspec(dllexport) int Initialize();
extern "C" __declspec(dllexport) int Dispose();
extern "C" __declspec(dllexport) int InitShellBrowserHook(IShellBrowser* psb);

// Function pointer types
typedef HRESULT (WINAPI *COCREATEINSTANCE)(REFCLSID, LPUNKNOWN, DWORD, REFIID, LPVOID FAR*);
typedef HRESULT (WINAPI *REGISTERDRAGDROP)(HWND, LPDROPTARGET);
typedef HRESULT (WINAPI *SHCREATESHELLFOLDERVIEW)(const SFV_CREATE*, IShellView**);
typedef HRESULT (WINAPI *BROWSEOBJECT)(IShellBrowser*, PCUIDLIST_RELATIVE, UINT);
typedef HRESULT (WINAPI *CREATEVIEWWINDOW3)(IShellView3*, IShellBrowser*, IShellView*, SV3CVW3_FLAGS, FOLDERFLAGS, FOLDERFLAGS, FOLDERVIEWMODE, const SHELLVIEWID*, const RECT*, HWND*);
typedef HRESULT (WINAPI *MESSAGESFVCB)(IShellFolderViewCB*, UINT, WPARAM, LPARAM);

// Detour functions
HRESULT WINAPI DetourCoCreateInstance(REFCLSID rclsid, LPUNKNOWN pUnkOuter, DWORD dwClsContext, REFIID riid, LPVOID FAR* ppv);
HRESULT WINAPI DetourRegisterDragDrop(HWND hwnd, LPDROPTARGET pDropTarget);
HRESULT WINAPI DetourSHCreateShellFolderView(const SFV_CREATE* pcsfv, IShellView** ppsv);
HRESULT WINAPI DetourBrowseObject(IShellBrowser* _this, PCUIDLIST_RELATIVE pidl, UINT wFlags);
HRESULT WINAPI DetourCreateViewWindow3(IShellView3* _this, IShellBrowser* psbOwner, IShellView* psvPrev, SV3CVW3_FLAGS dwViewFlags, FOLDERFLAGS dwMask, FOLDERFLAGS dwFlags, FOLDERVIEWMODE fvMode, const SHELLVIEWID* pvid, const RECT* prcView, HWND* phwndView);
HRESULT WINAPI DetourMessageSFVCB(IShellFolderViewCB* _this, UINT uMsg, WPARAM wParam, LPARAM lParam);

// Pointers to original functions
COCREATEINSTANCE fpCoCreateInstance = NULL;
REGISTERDRAGDROP fpRegisterDragDrop = NULL;
SHCREATESHELLFOLDERVIEW fpSHCreateShellFolderView = NULL;
BROWSEOBJECT fpBrowseObject = NULL;
CREATEVIEWWINDOW3 fpCreateViewWindow3 = NULL;
MESSAGESFVCB fpMessageSFVCB = NULL;

// Messages
unsigned int WM_REGISTERDRAGDROP;
unsigned int WM_NEWTREECONTROL;
unsigned int WM_BROWSEOBJECT;
unsigned int WM_HEADERINALLVIEWS;
unsigned int WM_LISTREFRESHED;


BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_DETACH:
            return Dispose();

        case DLL_PROCESS_ATTACH:
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;
    }
    return true;
}

int Initialize() {

    volatile static long initialized;
    if(InterlockedIncrement(&initialized) != 1) {
        // Return if another thread has beaten us here.
        initialized = 1;
        return MH_OK;
    }

    // Register the messages.
    WM_REGISTERDRAGDROP = RegisterWindowMessageA("QTTabBar_RegisterDragDrop");
    WM_NEWTREECONTROL = RegisterWindowMessageA("QTTabBar_NewTreeControl");
    WM_BROWSEOBJECT = RegisterWindowMessageA("QTTabBar_BrowseObject");
    WM_HEADERINALLVIEWS = RegisterWindowMessageA("QTTabBar_HeaderInAllViews");
    WM_LISTREFRESHED = RegisterWindowMessageA("QTTabBar_ListRefreshed");

    // Initialize MinHook.
    MH_STATUS ret = MH_Initialize();
    if(ret != MH_OK) return ret;

    // Create and enable CoCreateInstance hook
    ret = MH_CreateHook(&CoCreateInstance, &DetourCoCreateInstance, reinterpret_cast<void**>(&fpCoCreateInstance));
    if(ret != MH_OK) return ret;
    ret = MH_EnableHook(&CoCreateInstance);
    if(ret != MH_OK) return ret;

    // Create and enable RegisterDragDrop hook
    ret = MH_CreateHook(&RegisterDragDrop, &DetourRegisterDragDrop, reinterpret_cast<void**>(&fpRegisterDragDrop));
    if(ret != MH_OK) return ret;
    ret = MH_EnableHook(&RegisterDragDrop);
    if(ret != MH_OK) return ret;

    // Create and enable SHCreateShellFolderView hook
    ret = MH_CreateHook(&SHCreateShellFolderView, &DetourSHCreateShellFolderView, reinterpret_cast<void**>(&fpSHCreateShellFolderView));
    if(ret != MH_OK) return ret;
    ret = MH_EnableHook(&SHCreateShellFolderView);
    if(ret != MH_OK) return ret;
    
    return ret;
}

int InitShellBrowserHook(IShellBrowser* psb) {
    volatile static long initialized;
    if(InterlockedIncrement(&initialized) != 1) {
        // Return if another thread has beaten us here.
        initialized = 1;
        return MH_OK;
    }

    // Hook the 11th entry in this IShellBrowser's VTable, which is BrowseObject
    void** vtable = *reinterpret_cast<void***>(psb);
    MH_STATUS ret = MH_CreateHook(vtable[11], &DetourBrowseObject, reinterpret_cast<void**>(&fpBrowseObject));
    if(ret != MH_OK) return ret;
    return MH_EnableHook(vtable[11]);
}

int Dispose() {
    // Uninitialize MinHook.
    return MH_Uninitialize();
}

//////////////////////////////
// Detour Functions
//////////////////////////////

HRESULT WINAPI DetourCoCreateInstance(REFCLSID rclsid, LPUNKNOWN pUnkOuter, DWORD dwClsContext, REFIID riid, LPVOID FAR* ppv) {
    HRESULT ret = fpCoCreateInstance(rclsid, pUnkOuter, dwClsContext, riid, ppv);
    if(SUCCEEDED(ret) && (
            IsEqualIID(riid, __uuidof(INameSpaceTreeControl)) || 
            IsEqualIID(riid, __uuidof(INameSpaceTreeControl2)))) {
        PostThreadMessage(GetCurrentThreadId(), WM_NEWTREECONTROL, reinterpret_cast<WPARAM>(*ppv), NULL);
    }  
    return ret;
}

HRESULT WINAPI DetourRegisterDragDrop(IN HWND hwnd, IN LPDROPTARGET pDropTarget) {
    LPDROPTARGET* ppDropTarget = &pDropTarget;
    SendMessage(hwnd, WM_REGISTERDRAGDROP, reinterpret_cast<WPARAM>(ppDropTarget), NULL);
    return fpRegisterDragDrop(hwnd, *ppDropTarget);
}

// TODO: This should have some way of reporting success or failure.
HRESULT WINAPI DetourSHCreateShellFolderView(const SFV_CREATE* pcsfv, IShellView** ppsv) {
    // Hook the 3rd entry in this IShellFolderViewCB's VTable, which is MessageSFVCB
    void** vtable = *reinterpret_cast<void***>(pcsfv->psfvcb);
    IShellView* dummy;
    if(MH_CreateHook(vtable[3], &DetourMessageSFVCB, reinterpret_cast<void**>(&fpMessageSFVCB)) == MH_OK) {
        MH_EnableHook(vtable[3]);
    }
    HRESULT ret = fpSHCreateShellFolderView(pcsfv, ppsv);
    if(SUCCEEDED(ret) && SUCCEEDED((*ppsv)->QueryInterface(IID_CDefView, reinterpret_cast<void**>(&dummy)))) {
        dummy->Release();
        IShellView3* psv3;
        if(SUCCEEDED((*ppsv)->QueryInterface(__uuidof(IShellView3), reinterpret_cast<void**>(&psv3)))) {
            // Hook the 20th entry in this IShellView3's VTable, which is CreateFolderView3
            vtable = *reinterpret_cast<void***>(psv3);
            if(MH_CreateHook(vtable[20], &DetourCreateViewWindow3, reinterpret_cast<void**>(&fpCreateViewWindow3)) == MH_OK) {
                MH_EnableHook(vtable[20]);
            }
            psv3->Release();
        }

        // Disable this hook, no need for it anymore.
        MH_DisableHook(&SHCreateShellFolderView);
    }
    return ret;
}

HRESULT WINAPI DetourBrowseObject(IShellBrowser* _this, PCUIDLIST_RELATIVE pidl, UINT wFlags) {
    HWND hwnd;
    LRESULT result = 0;
    if(SUCCEEDED(_this->GetWindow(&hwnd))) {
        HWND parent = GetParent(hwnd);
        if(parent != 0) hwnd = parent;
        result = SendMessage(hwnd, WM_BROWSEOBJECT, reinterpret_cast<WPARAM>(&wFlags), reinterpret_cast<LPARAM>(pidl));
    } 
    return result == 0 ? fpBrowseObject(_this, pidl, wFlags) : S_FALSE;
}

HRESULT WINAPI DetourCreateViewWindow3(IShellView3* _this, IShellBrowser* psbOwner, IShellView* psvPrev, SV3CVW3_FLAGS dwViewFlags, FOLDERFLAGS dwMask, FOLDERFLAGS dwFlags, FOLDERVIEWMODE fvMode, const SHELLVIEWID* pvid, const RECT* prcView, HWND* phwndView) {
    HWND hwnd;
    LRESULT result = 0;
    if(psbOwner != NULL && SUCCEEDED(psbOwner->GetWindow(&hwnd))) {
        HWND parent = GetParent(hwnd);
        if(parent != 0) hwnd = parent;
        if(SendMessage(hwnd, WM_HEADERINALLVIEWS, 0, 0) != 0) {
            dwMask |= FWF_NOHEADERINALLVIEWS;
            dwFlags &= ~FWF_NOHEADERINALLVIEWS;
        }
    }
    return fpCreateViewWindow3(_this, psbOwner, psvPrev, dwViewFlags, dwMask, dwFlags, fvMode, pvid, prcView, phwndView);
}

HRESULT WINAPI DetourMessageSFVCB(IShellFolderViewCB* _this, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    if(uMsg == SFVM_LISTREFRESHED && wParam != 0) {
        PostThreadMessage(GetCurrentThreadId(), WM_LISTREFRESHED, NULL, NULL);
    }
    return fpMessageSFVCB(_this, uMsg, wParam, lParam);
}
