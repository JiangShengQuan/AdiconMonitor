@echo off  
  
::���ʱ��������λ����  
set _interval=600
  
::��Ҫ�ػ��Ľ�������  
set _processName=�������.exe
  
::��Ҫ�ػ��Ľ�����������  
set _processCmd=D:\���ݼ��\�������.exe
  
::��Ҫ�ػ��Ľ���Ԥ�������������ʱ�䣬��λ����  
set _processTimeout=3  
  
:LOOP  
set /a isAlive=false  
  
::ͨ���������Ƽ��  
tasklist | find /C "%_processName%" > temp.txt  
set /p num= < temp.txt  

  
if "%num%" == "0" (  
start %_processCmd% | echo %date% %time% ���� %_processName%  
choice /D y /t %_processTimeout% > nul  
)  
  
if "%num%" NEQ "0" echo %date% %time% %_processName%�����������ٴ�����      

::ping -n %_interval% 127.1>nul  
choice /D y /t %_interval% >nul  
  
goto LOOP 