Build
dotnet build

Rodar
dotnet run

Publicar
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\portable