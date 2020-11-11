param
(
    $config = 'Release'
)

dotnet new tool-manifest --force
dotnet tool install Cake.Tool --version 0.38.5
dotnet tool restore
dotnet cake full.cake --configuration=$config
