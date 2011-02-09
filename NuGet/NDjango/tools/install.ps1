param($installPath, $toolsPath, $package, $project)

#$vsixId = "2780818b-02fb-4990-a051-7cee3fd09157"

#[void] [Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms")

#$title = "NDjango"
#$body = "toolsPath = '" + $toolsPath +"' installPath = '"  + $installPath + "' package = '" + $package + "' project = " + $project
#$body = "Do you want to install NDjango Template Editor?"

#$install = 
#    [Windows.Forms.MessageBox]::Show($body, $title, [Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Information)

$vsix_version = "1.0"

#$path_to_designer = $Env:HOMEDRIVE + $Env:HOMEPATH + "\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Hill30\NDjango Template Editor\" + $vsix_version
$path_to_designer = $Env:USERPROFILE + "\Local Settings\Application Data\Microsoft\VisualStudio\10.0\Extensions\Hill30\NDjango Template Editor\" + $vsix_version
$designer_exist = test-path -path $path_to_designer

If (!$designer_exist) {
	$DirInfo = New-Object System.IO.DirectoryInfo($Env:VS100COMNTOOLS)
	$path = [io.path]::Combine($DirInfo.Parent.FullName, "IDE")
	$path = [io.path]::Combine($path, "VSIXInstaller.exe")
	#[Array]$arguments = "/q", $toolsPath + "\NDjango.Designer.vsix"
	[Array]$arguments = $toolsPath + "\NDjango.Designer.vsix"
	
	&$path $arguments

#	$message_body = "You must restart Microsoft Visual Studio in order for the changes to take effect"
#	$temp = 
#	    [Windows.Forms.MessageBox]::Show($message_body, $title, [Windows.Forms.MessageBoxButtons]::Ok, [System.Windows.Forms.MessageBoxIcon]::Information)
}


$dirinfo = New-Object System.Io.Directoryinfo($Env:Vs100comntools)
$mvc3 = [Io.Path]::Combine($Dirinfo.Parent.Fullname, "Ide")
$mvc3 = $mvc3 + "\Itemtemplates\Csharp\Web\Mvc 3\Codetemplates\AddView"

$mvc3_exist = test-path -path $mvc3
If ($mvc3_exist) {

	if(!(test-path -Path ($mvc3 + "\DjangoViewEngine")))	{
		New-Item ($mvc3 + "\DjangoViewEngine") -itemtype directory | Out-Null
	}

	Copy-Item $toolspath\DjangoViewEngine\*.* -Destination ($mvc3 + "\DjangoViewEngine")
}

$proj = get-project
$app = $proj.ProjectItems.Item("global.asax").ProjectItems.Item("global.asax.cs").FileCodeModel.CodeElements | where-object {$_.Kind -eq 5}
$class = $app.Children.Item(1)
$result = $class.AddVariable("DjangoTemplateManager","NDjango.Interfaces.ITemplateManager",0,1)
#$result = $class.AddProperty("DjangoTemplateManager","DjangoTemplateManager","NDjango.Interfaces.ITemplateManager",0,1)



