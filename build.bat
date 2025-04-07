@echo off
setlocal

:: Masaüstü klasör yolları
set "DEBUG_DIR=%USERPROFILE%\Desktop\Debug"
set "TOOLS_DIR=%USERPROFILE%\Desktop\Tools"

:: Klasörler yoksa oluştur
if not exist "%DEBUG_DIR%" (
    mkdir "%DEBUG_DIR%"
)
if not exist "%TOOLS_DIR%" (
    mkdir "%TOOLS_DIR%"
)

echo ========== EntityLayer build ==========
dotnet build EntityLayer/EntityLayer.csproj -c Debug --output "%DEBUG_DIR%"

echo ========== DataAccessLayer build ==========
dotnet build DataAccessLayer/DataAccessLayer.csproj -c Debug --output "%DEBUG_DIR%"

echo ========== BusinessLayer build ==========
dotnet build BusinessLayer/BusinessLayer.csproj -c Debug --output "%DEBUG_DIR%"

echo ========== ApiLayer build ==========
dotnet build Ozgenc_ERP/ApiLayer.csproj -c Debug --output "%DEBUG_DIR%"

echo ========== ERPClient build ==========
dotnet build ERPClient/ERPClient.csproj -c Debug --output "%DEBUG_DIR%"


echo.
echo Tum projeler basariyla derlendi.
pause
