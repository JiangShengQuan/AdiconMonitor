@echo off  
  
::检测时间间隔，单位：秒  
set _interval=600
  
::需要守护的进程名称  
set _processName=监控数据.exe
  
::需要守护的进程启动命令  
set _processCmd=D:\数据监控\监控数据.exe
  
::需要守护的进程预估启动完毕所需时间，单位：秒  
set _processTimeout=3  
  
:LOOP  
set /a isAlive=false  
  
::通过进程名称检测  
tasklist | find /C "%_processName%" > temp.txt  
set /p num= < temp.txt  

  
if "%num%" == "0" (  
start %_processCmd% | echo %date% %time% 启动 %_processName%  
choice /D y /t %_processTimeout% > nul  
)  
  
if "%num%" NEQ "0" echo %date% %time% %_processName%已启动无需再次启动      

::ping -n %_interval% 127.1>nul  
choice /D y /t %_interval% >nul  
  
goto LOOP 