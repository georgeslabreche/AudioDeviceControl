@echo off
cls
tools\nant\bin\NAnt.exe -buildfile:build.xml %*
pause