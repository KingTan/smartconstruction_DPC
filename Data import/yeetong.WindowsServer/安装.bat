@ECHO OFF
echo ׼����װ����
pause
REM The following directory is for .NET 4.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
echo ��װ����...
echo ---------------------------------------------------
InstallUtil /i  E:\Debug\GOYO_RoutedProtocol_WS.exe
echo ---------------------------------------------------
echo ��װ����ɹ���
pause