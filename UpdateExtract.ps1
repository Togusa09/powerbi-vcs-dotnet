Add-Type -assembly "system.io.compression.filesystem"

Remove-Item -Path New -Force -Recurse
[io.compression.zipfile]::ExtractToDirectory("C:\OwnSource\powerbi-vcs-dotnet\PowerBiVcs\bin\Debug\Files\Template2.zip", "C:\OwnSource\powerbi-vcs-dotnet\PowerBiVcs\bin\Debug\Files\New")
Copy-Item -Path "C:\OwnSource\powerbi-vcs-dotnet\PowerBiVcs\bin\Debug\TemplateVcs\*" -Destination "C:\OwnSource\powerbi-vcs\TemplateVcs_Own" -Force -Recurse