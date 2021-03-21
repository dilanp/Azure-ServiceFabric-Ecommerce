#===========NOTE: ONLY TESTED ON POWERSHELL VERSION 5.1.19041.610===============

#===================================Building an app=============================

#In Environment variables, set the PATH for the location of MSBuild.exe of the latest Visual Studio version.
#E.g: C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin

#Launch PowerShell with admin privileges.

#Make sure the "msbuild" command runs without errors.
msbuild

#Move into the main "App folder" of the Service Fabric solution in Powershell.
cd D:\Development\Azure Projects\Ecommerce\Ecommerce

#Run the "msbuild" command.
msbuild

#Verify that all the deployment artefacts are created in the "\pkg\[Debug/Release]" folder.

#==================================Deploying an app==============================

#Launch PowerShell with admin privileges.
#Then, move into the "\pkg\[Debug/Release]" folder of the deployment.

#(1)Connect to the local Service Fabric Cluster.
Connect-ServiceFabricCluster

#Set the deployment artefact folder into a variable.
$packageLocation = "D:\Development\Azure Projects\Ecommerce\Ecommerce\pkg\Debug"

#(2)Test the deployment artefacts and make sure it returns "True".
Test-ServiceFabricApplicationPackage `
	-ApplicationPackagePath $packageLocation

#Import the ServiceFabricSDK PowerShell module.
Import-Module "$ENV:ProgramFiles\Microsoft SDKs\Service Fabric\Tools\PSModule\ServiceFabricSDK\ServiceFabricSDK.psm1"

#Load the cluster manifest into a variable.
$clusterManifest = Get-ServiceFabricClusterManifest

#Get the image store connection string from the cluster manifest.
$connectionString = Get-ImageStoreConnectionStringFromClusterManifest `
	-ClusterManifest $clusterManifest

#(3)Upload application to the image store.
Copy-ServiceFabricApplicationPackage `
	-ApplicationPackagePath $packageLocation `
	-ImageStoreConnectionString $connectionString `
	-ApplicationPackagePathInImageStore "EcommercePackage"
	
#(4)Register the application in Service Fabric Cluster.
Register-ServiceFabricApplicationType `
	-ApplicationPathInImageStore "EcommercePackage"
#Now, you should see the application appear on Service Fabric Explorer.

#(5)Finally, create an instance of the application.
New-ServiceFabricApplication `
	-ApplicationName "fabric:/MyEcommerceApp" `
	-ApplicationTypeName "EcommerceType" `
	-ApplicationTypeVersion "1.0.0"
#Now, you should see all the services running under the application!

#===================================Upgrading an app================================

#Launch PowerShell with admin privileges.
#Then, move into the "\pkg\[Debug/Release]" folder of the deployment.

#(1)Connect to the local Service Fabric Cluster.
Connect-ServiceFabricCluster

#Set the deployment artefact folder into a variable.
$packageLocation = "D:\Development\Azure Projects\Ecommerce\Ecommerce\pkg\Debug"

#(2)Test the deployment artefacts and make sure it returns "True".
Test-ServiceFabricApplicationPackage -ApplicationPackagePath $packageLocation

#Import the ServiceFabricSDK PowerShell module.
Import-Module "$ENV:ProgramFiles\Microsoft SDKs\Service Fabric\Tools\PSModule\ServiceFabricSDK\ServiceFabricSDK.psm1"

#Load the cluster manifest into a variable.
$clusterManifest = Get-ServiceFabricClusterManifest

#Get the image store connection string from the cluster manifest.
$connectionString = Get-ImageStoreConnectionStringFromClusterManifest `
	-ClusterManifest $clusterManifest

#(3)Upload application to the image store.
Copy-ServiceFabricApplicationPackage `
	-ApplicationPackagePath $packageLocation `
	-ImageStoreConnectionString $connectionString `
	-ApplicationPackagePathInImageStore "EcommercePackage"
	
#(4)Register the application in Service Fabric Cluster.
Register-ServiceFabricApplicationType `
	-ApplicationPathInImageStore "EcommercePackage"
#Now, you should see the application appear on Service Fabric Explorer.

#(5)Finally, start the upgrade.
Start-ServiceFabricApplicationUpgrade `
	-ApplicationName "fabric:/MyEcommerceApp" `
	-ApplicationTypeVersion "1.1.0" ` #Must be different to the current version
	-FailureAction Rollback ` #OR "Manual"
	-Monitored
	
#Get the status of the upgrade.
Get-ServiceFabricApplicationUpgrade `
	-ApplicationName "fabric:/MyEcommerceApp"

#====================================Removing an app===================================

#Launch PowerShell with admin privileges.

#Remove the application instance
Remove-ServiceFabricApplication `
	-ApplicationName "fabric:/MyEcommerceApp" `
	-Force
	
#Unregister the application type.
Unregister-ServiceFabricApplicationType `
	-ApplicationTypeName "EcommerceType" `
	-ApplicationTypeVersion "1.0.0" ` #Check for the correct version
	-Force
	
