@ECHO OFF  
echo ׼��ж�ط���  
pause  
REM The following directory is for .NET 4.0  
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
echo ж�ط���...  
echo ---------------------------------------------------  
InstallUtil /u    E:\Debug\RoutedProtocol_WS.exe
echo ---------------------------------------------------  
echo ��װж�سɹ���  
pause  
==========================================  