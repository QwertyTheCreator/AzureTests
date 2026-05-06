@echo off
setlocal

echo Removing old publish folder and deploy.zip...

if exist publish (
    rmdir /s /q publish
)

if exist deploy.zip (
    del /q deploy.zip
)

echo Publishing project...
dotnet publish -o publish

if errorlevel 1 (
    echo dotnet publish failed.
    exit /b 1
)

echo Creating deploy.zip...
cd publish

tar -a -c -f ../deploy.zip *

if errorlevel 1 (
    echo zip creation failed.
    exit /b 1
)

cd ..

echo Deploying to Azure...
az webapp deploy --src-path deploy.zip --resource-group rg-WebApp --name ahlazko-webapp --type zip

if errorlevel 1 (
    echo Azure deployment failed.
    exit /b 1
)

echo Done.
endlocal