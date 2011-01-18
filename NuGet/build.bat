copy ..\NDjangoDesigner\bin\Debug\ASPMVCIntegration.NDjangoExtension40.dll NDjango\lib\Net40\
copy ..\NDjangoDesigner\bin\Debug\ASPMVCSampleLibrary.NDjangoExtension40.dll NDjango\lib\Net40\
copy ..\NDjangoDesigner\bin\Debug\NDjango.Core40.dll NDjango\lib\Net40\
copy ..\NDjangoDesigner\bin\Debug\NDjangoFilters.NDjangoExtension40.dll NDjango\lib\Net40\
copy ..\NDjangoDesigner\bin\Debug\ASPMVCIntegration.NDjangoExtension35.dll NDjango\lib\Net35\
copy ..\NDjangoDesigner\bin\Debug\ASPMVCSampleLibrary.NDjangoExtension35.dll NDjango\lib\Net35\
copy ..\NDjangoDesigner\bin\Debug\NDjango.Core35.dll NDjango\lib\Net35\
copy ..\NDjangoDesigner\bin\Debug\NDjangoFilters.NDjangoExtension35.dll NDjango\lib\Net35\
copy ..\NDjangoDesigner\bin\Debug\NDjango.Designer.vsix NDjango\tools\

nuget pack NDjango.nuspec -BasePath NDjango 1>log 2>log_error