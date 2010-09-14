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

typedef HRESULT (WINAPI *COCREATEINSTANCE)(REFCLSID, LPUNKNOWN, DWORD, REFIID, LPVOID FAR*);
typedef HRESULT (WINAPI *REGISTERDRAGDROP)(HWND, LPDROPTARGET);
typedef HRESULT (WINAPI *BROWSEOBJECT)(IShellBrowser*, PCUIDLIST_RELATIVE, UINT);

bool Initialize();
bool Dispose();
extern "C" __declspec(dllexport) bool InitShellBrowserHook(IShellBrowser* psb);
HRESULT WINAPI DetourCoCreateInstance(REFCLSID rclsid, LPUNKNOWN pUnkOuter, DWORD dwClsContext, REFIID riid, LPVOID FAR* ppv);
HRESULT WINAPI DetourRegisterDragDrop(HWND hwnd, LPDROPTARGET pDropTarget);
HRESULT WINAPI DetourBrowseObject(IShellBrowser* _this, PCUIDLIST_RELATIVE pidl, UINT wFlags);

unsigned int WM_REGISTERDRAGDROP;
unsigned int WM_NEWTREECONTROL;
unsigned int WM_BROWSEOBJECT;
COCREATEINSTANCE fpCoCreateInstance = NULL;
REGISTERDRAGDROP fpRegisterDragDrop = NULL;
BROWSEOBJECT CBaseBrowser_BrowseObject = NULL;
BROWSEOBJECT fpBrowseObject = NULL;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_ATTACH:
            return Initialize();

        case DLL_PROCESS_DETACH:
            return Dispose();

        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;
    }
    return true;
}

bool Initialize() {

    // Register the messages.
    WM_REGISTERDRAGDROP = RegisterWindowMessageA("QTTabBar_RegisterDragDrop");
    WM_NEWTREECONTROL = RegisterWindowMessageA("QTTabBar_NewTreeControl");
    WM_BROWSEOBJECT = RegisterWindowMessageA("QTTabBar_BrowseObject");

    // Initialize MinHook.
    if(MH_Initialize() != MH_OK) {
        return false;
    }

    // Create and enable CoCreateInstance hook
    if(MH_CreateHook(&CoCreateInstance, &DetourCoCreateInstance, reinterpret_cast<void**>(&fpCoCreateInstance)) != MH_OK) {
        return false;
    }
    if(MH_EnableHook(&CoCreateInstance) != MH_OK) {
        return false;
    }

    // Create and enable RegisterDragDrop hook
    if(MH_CreateHook(&RegisterDragDrop, &DetourRegisterDragDrop, reinterpret_cast<void**>(&fpRegisterDragDrop)) != MH_OK) {
        return false;
    }
    if(MH_EnableHook(&RegisterDragDrop) != MH_OK) {
        return false;
    }

    return true;
}

bool InitShellBrowserHook(IShellBrowser* psb) {

    // Grab the 11th entry in the VTable, which is BrowseObject
    void** vtable = *reinterpret_cast<void***>(psb);
    CBaseBrowser_BrowseObject = (BROWSEOBJECT)(vtable[11]);

    // Create and enable CBaseBrowser::BrowseObject hook
    if(MH_CreateHook(CBaseBrowser_BrowseObject, &DetourBrowseObject, reinterpret_cast<void**>(&fpBrowseObject)) != MH_OK) {
        return false;
    }
    if(MH_EnableHook(CBaseBrowser_BrowseObject) != MH_OK) {
        return false;
    }
    return true;
}

bool Dispose() {

    // Disable hooks
    if(MH_DisableHook(&CoCreateInstance) != MH_OK) {
        return false;
    }
    if(MH_DisableHook(&RegisterDragDrop) != MH_OK) {
        return false;
    }
    if(CBaseBrowser_BrowseObject != NULL && MH_DisableHook(CBaseBrowser_BrowseObject) != MH_OK) {
        return false;
    }

    // Uninitialize MinHook.
    if(MH_Uninitialize() != MH_OK) {
        return false;
    }

    return true;
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