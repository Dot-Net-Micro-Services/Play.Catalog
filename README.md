# Play.Catalog
Catalog Microservices

## Create and publish package
```powershell
$version="1.0.3"
$owner="Dot-Net-Micro-Services"
$gh_pat="[PAT HERE]"

dotnet pack src\Play.Catalog.Contracts\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Catalog -o ..\packages

dotnet nuget push ..\packages\Play.Catalog.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image
```powershell
$env:GH_OWNER="Dot-Net-Micro-Services"
$env:GH_PAT="[PAT HERE]"
$acrname="playeconomyacrdev"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$acrname.azurecr.io/play.catalog:$version" .
```

## Run the docker image
```powershell
$cosmosDbConnectionString="[CONNECTION STRING HERE]"
$serviceBusConnectionString="[CONNECTION STRING HERE]"
docker run -it --rm -p 5000:5000 --name catalog 
-e MongoDbSettings__ConnectionString=$cosmosDbConnectionString
-e ServiceBusSettings__ConnectionString=$serviceBusConnectionString
-e ServiceSettings__MessageBroker="SERVICEBUS"
play.catalog:$version
```

## Publish the docker image
```powershell
az acr login --name $acrname
docker push "$acrname.azurecr.io/play.catalog:$version"
```