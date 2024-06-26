#how to get the list of users who has quot can view quot access

# Specify the site URL and the folder URL
$SiteURL = "https://capgeminifscoe.sharepoint.com/sites/allcompany"
$FolderURL = "/sites/allcompany/Shared Documents/PDFExtract"
$FolderURL = "/Shared Documents/PDFExtract"

# Get credentials and connect to SharePoint Online
$Cred = Get-Credential
Connect-PnPOnline -Url $SiteURL -Credentials $Cred

# Invoke the REST API endpoint
#$Response = Invoke-PnPSPRestMethod -Method Get -Url $SiteURL+ "_api/web/GetFileByServerRelativeUrl('$FolderURL')/ListItemAllFields/RoleAssignments"

$Response = Invoke-PnPSPRestMethod -Method Get -Url $SiteURL+ "_api/web/GetFileByServerRelativeUrl('$FolderURL')/ListItemAllFields/RoleAssignments"
$Response =Invoke-SPORestMethod -Method Get -Url $SiteURL+ "_api/web/GetFileByServerRelativeUrl('$FolderURL')/ListItemAllFields/RoleAssignments"

# Parse the JSON response
$Data = $Response | ConvertFrom-Json


#Define the Rest Method
$RestMethodURL = $SiteURL+'/_api/web/lists?$select=Title'
 
#Invoke Rest Call to Get All Lists
$Lists = Invoke-PnPSPRestMethod -Url $RestMethodURL
$Lists.value


#Read more: https://www.sharepointdiary.com/2018/04/call-sharepoint-online-rest-api-from-powershell.html#ixzz8cc5odI3u

# Loop through the role assignments
foreach ($RoleAssignment in $Data.value) {

# Get the user or group information
$Member = $RoleAssignment.Member
Write-Host "User/Group: $($Member.Title)"

# Get the role definitions
$RoleDefinitions = $RoleAssignment.RoleDefinitionBindings.results

# Check if the user or group has "Can view" access
if ($RoleDefinitions.Name -contains "View Only") {
Write-Host "Access: Can view"
}

# Check if the user or group has "Can edit" access
if ($RoleDefinitions.Name -contains "Edit") {
Write-Host "Access: Can edit"
}

Write-Host ""
}