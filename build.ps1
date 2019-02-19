param
(
    $config = 'Release'
)

dotnet tool install -g Cake.Tool
dotnet tool install -g dotnet-warp

dotnet cake build.cake
