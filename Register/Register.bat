echo off

cd ..\QTTabBar\bin\%1
IF EXIST QTTabBar.dll (
    gacutil /if QTTabBar.dll
    regasm QTTabBar.dll
)

cd ..\..\..\QTPluginLib\bin\%1
IF EXIST QTPluginLib.dll (
    gacutil /if QTPluginLib.dll
)

cd ..\..\..\BandObjectLib\bin\%1
IF EXIST BandObjectLib.dll (
    gacutil /if BandObjectLib.dll
)
IF EXIST Interop.SHDocVw.dll (
    gacutil /if Interop.SHDocVw.dll
)

cd ..\..\..\Register