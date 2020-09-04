@echo off

set DIR=%~dp0

cd /d "%DIR%"

setlocal enabledelayedexpansion

for /r %%i in (*.proto) do (

set pbname=%%i

      set pbname=!pbname:~0,-5!pb

      protoc -I %DIR% --descriptor_set_out !pbname! %%i

)

 

if exist "./pb" rmdir /s /q "./pb"

mkdir "./pb"

move *.pb ./pb

 

echo "finished"