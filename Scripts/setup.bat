ECHO Set Custom Variables
set blogUrl=https://medium.com/nateduff
set location=centralus
set group=dufftestgroup
set store=dufftestaccount
set function=SimpleAuth

ECHO Create Resource Group, Storage Account, and FunctionApp
call az group create --name %group% --location %location%
call az storage account create --name %store% --location %location% --resource-group %group% --sku "Standard_LRS" --kind "StorageV2"
call az functionapp create --name %function% --storage-account %store% --consumption-plan-location %location% --resource-group %group%

REM Get ConnectionString to configure the WebApp
FOR /F "tokens=*" %%g IN ('call az storage account show-connection-string -g %group% -n %store% --out tsv') do (SET connectionString=%%g)

ECHO Updating AppSettings for %function%
call az webapp config appsettings set -g %group% -n %function% --settings AzureWebJobDashboard=%connectionString%
call az webapp config appsettings set -g %group% -n %function% --settings AzureWebJobsStorage=%connectionString%
call az webapp config appsettings set -g %group% -n %function% --settings WEBSITE_CONTENTAZUREFILECONNECTIONSTRING=%connectionString%
call az webapp config appsettings set -g %group% -n %function% --settings FUNCTIONS_EXTENSION_VERSION="~2"
call az webapp config appsettings set -g %group% -n %function% --settings WEBSITE_CONTENTSHARE=%store%
call az webapp config appsettings set -g %group% -n %function% --settings WEBSITE_AUTH_PRESERVE_URL_FRAGMENT=true
call az webapp config appsettings set -g %group% -n %function% --settings BlogUrl=%blogUrl%

ECHO Creating required Directories
call az storage directory create -n "site" -s %store% --connection-string %connectionString%
call az storage directory create -n "wwwroot" -s "%store%/site" --connection-string %connectionString%