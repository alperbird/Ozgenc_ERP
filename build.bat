@echo off
setlocal

:: Masaüstü klasör yolları
set "RELEASE_DIR=%USERPROFILE%\Desktop\Release"
set "TOOLS_DIR=%USERPROFILE%\Desktop\Tools"

:: Klasörler yoksa oluştur
if not exist "%RELEASE_DIR%" (
    mkdir "%RELEASE_DIR%"
)
if not exist "%TOOLS_DIR%" (
    mkdir "%TOOLS_DIR%"
)

echo ========== EntityLayer build ==========
dotnet build EntityLayer/EntityLayer.csproj -c Release --output "%RELEASE_DIR%"

echo ========== DataAccessLayer build ==========
dotnet build DataAccessLayer/DataAccessLayer.csproj -c Release --output "%RELEASE_DIR%"

echo ========== BusinessLayer build ==========
dotnet build BusinessLayer/BusinessLayer.csproj -c Release --output "%RELEASE_DIR%"

echo ========== ApiLayer build ==========
dotnet build Ozgenc_ERP/ApiLayer.csproj -c Release --output "%RELEASE_DIR%"

echo ========== ERPClient build ==========
dotnet build ERPClient/ERPClient.csproj -c Release --output "%RELEASE_DIR%"


echo.
echo Tum projeler basariyla derlendi.
pause
