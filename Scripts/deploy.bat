ECHO Set Custom Variables
set functionName=%~1
set group=dufftestgroup
set store=dufftestaccount

REM Get ConnectionString for updating the WebApp
FOR /F "tokens=*" %%g IN ('call az storage account show-connection-string -g %group% -n %store% --out tsv') do (SET connectionString=%%g)

ECHO Confirming required Directory and Uploading Files
call az storage directory create -n %functionName% -s "%store%/site/wwwroot" --connection-string %connectionString%
call az storage file upload-batch -d "%store%/site/wwwroot/%functionName%" -s "%Agent_ReleaseDirectory%\%DropFolder%\%functionName%" --connection-string %connectionString%