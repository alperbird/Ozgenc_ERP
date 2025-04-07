# Ozgenc_ERP

build.bat dosyası proje içerisinde bulunan projeleri çıkartmayı sağlar. Sırasıyla:
EntityLayer-DAL-BusinessLayer-ApiLayer-ERPClient projelerini Release eder.

Dipnot: GeneratePdf projesi .Net Framework ile çalıştığı için onu proje dışına çıkartma yöntemi farklıdır. Bunun için:
1)Developer Command Prompt for VS komut satırını açın
2)Ardından şu komutu yazın:
msbuild "Your project filepath/GeneratePdf.csproj" /p:Configuration=Release /p:OutputPath="C:\Users\your username\Desktop\Tools"
Masaüstü'nde bulunan Tools klasörüne proje dosyasını çıkarır.
