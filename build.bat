@echo off
REM .NET项目构建和运行脚本

REM 设置中文编码
chcp 65001 >nul

REM 显示帮助信息
:help
cls
echo.--------------------------------------------------------------------------
echo.                        输入法项目构建脚本
 echo.--------------------------------------------------------------------------
echo.1. 构建项目 (build)
echo.2. 运行项目 (run)
echo.3. 清理项目 (clean)
echo.4. 构建并运行 (buildrun)
echo.5. 退出 (exit)
echo.--------------------------------------------------------------------------

echo.请输入操作编号或命令: 
set /p command=

echo.

REM 根据用户输入执行相应操作
if /i "%command%"=="1" goto build
if /i "%command%"=="2" goto run
if /i "%command%"=="3" goto clean
if /i "%command%"=="4" goto buildrun
if /i "%command%"=="5" goto exit
if /i "%command%"=="build" goto build
if /i "%command%"=="run" goto run
if /i "%command%"=="clean" goto clean
if /i "%command%"=="buildrun" goto buildrun
if /i "%command%"=="exit" goto exit

REM 无效输入
cls
echo.错误: 无效的命令或操作编号。
pause
goto help

REM 构建项目
:build
cls
echo.开始构建项目...
echo.
dotnet build fqwb.sln
if %errorlevel% neq 0 (
echo.
echo.构建失败！
pause
goto help
)
echo.
echo.构建成功！
pause
goto help

REM 运行项目
:run
cls
echo.开始运行项目...
echo.
dotnet run --project fqwb.csproj
if %errorlevel% neq 0 (
echo.
echo.运行失败！
pause
goto help
)
echo.
echo.程序已退出。
pause
goto help

REM 清理项目
:clean
cls
echo.开始清理项目...
echo.
dotnet clean fqwb.sln
if %errorlevel% neq 0 (
echo.
echo.清理失败！
pause
goto help
)
echo.
echo.清理成功！
pause
goto help

REM 构建并运行
:buildrun
cls
echo.开始构建项目...
echo.
dotnet build fqwb.sln
if %errorlevel% neq 0 (
echo.
echo.构建失败！
pause
goto help
)
echo.
echo.构建成功，开始运行项目...
echo.
dotnet run --project fqwb.csproj
echo.
echo.程序已退出。
pause
goto help

REM 退出脚本
:exit
cls
echo.感谢使用输入法项目构建脚本！
echo.再见！
pause