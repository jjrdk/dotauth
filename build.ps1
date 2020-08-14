param
(
    $config = 'Release'
)

dotnet new tool-manifest
dotnet tool install Cake.Tool
dotnet tool restore
dotnet cake
