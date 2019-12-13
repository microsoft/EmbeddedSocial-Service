@echo off
set ProjectDir=%1
set ConfigName=%2

REM if-else-if is not supported by BAT. Use goto instead

if "%ConfigName%"=="sp-api" GOTO ssl-prod
if "%ConfigName%"=="sp-dev-alec" GOTO ssl
if "%ConfigName%"=="sp-dev-eduardo" GOTO ssl
if "%ConfigName%"=="sp-dev-landon" GOTO ssl
if "%ConfigName%"=="sp-dev-sharad" GOTO ssl
if "%ConfigName%"=="sp-dev-stefan" GOTO ssl
if "%ConfigName%"=="sp-dev-test1" GOTO ssl
if "%ConfigName%"=="sp-dev-vso" GOTO ssl
if "%ConfigName%"=="sp-ppe" GOTO ssl
if "%ConfigName%"=="sp-ppe-beihai" GOTO ssl-prod
if "%ConfigName%"=="sp-prod-beihai" GOTO ssl-prod
if "%ConfigName%"=="sp-test-perf" GOTO ssl-prod

REM Use HTTP
copy /Y %ProjectDir%ServiceDefinition.http.csdef %ProjectDir%ServiceDefinition.csdef
GOTO end

:ssl
REM Use SSL
copy /Y %ProjectDir%ServiceDefinition.ssl.csdef %ProjectDir%ServiceDefinition.csdef
GOTO end

:ssl-prod
REM Use SSL and large vm size
copy /Y %ProjectDir%ServiceDefinition.ssl-prod.csdef %ProjectDir%ServiceDefinition.csdef

:end
