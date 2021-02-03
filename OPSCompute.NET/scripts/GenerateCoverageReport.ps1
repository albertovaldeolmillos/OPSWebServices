
$folderPath = (Get-Item -Path ".\").FullName
$reportGenerator = "$folderPath\packages\\ReportGenerator.3.1.2\tools\ReportGenerator.exe"

& "$reportGenerator"  "-reports:.\reports\coverage.xml" -targetdir:.\reports\coverage