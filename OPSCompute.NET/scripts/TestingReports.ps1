$folderPath = (Get-Item -Path ".\").FullName

# CODE COVERAGE
.\scripts\XmlCoverageExport

# GENERATE HTML CODE REPORTS
.\scripts\GenerateCoverageReport

Read-Host "La ejecucion ha terminado"