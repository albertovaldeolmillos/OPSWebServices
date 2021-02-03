$folderPath = (Get-Item -Path ".\").FullName
$msBuild = "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MsBuild.exe"

$reportsPaths= "$folderPath\reports\UnitTest.xml"
$coveragePaths= "$folderPath\reports\coverage.xml"

SonarScanner.MSBuild.exe begin  /k:"OPSCompute" /n:"OPSCompute" /v:"1.0.0" /d:sonar.cs.xunit.reportsPaths="$reportsPaths" /d:sonar.cs.opencover.reportsPaths="$coveragePaths"
# BUILD 
& "$msBuild" /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"

# XUNIT TEST REPORTS
.\scripts\XmlTestingExport

# CODE COVERAGE
.\scripts\XmlCoverageExport

# GENERATE HTML CODE REPORTS
.\scripts\GenerateCoverageReport

SonarScanner.MSBuild.exe end

Read-Host "La ejecucion ha terminado"