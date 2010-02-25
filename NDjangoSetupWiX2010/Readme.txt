Readme 2010:

NDjango Requirements:
 *Visual Studio 2010 RC /NET Framework 4.0

To install:

1. Uninstall all previous versions of VS2010 (if any)
2. Uninstall all previous versions of F# CTP (if any)
3. install VS2010 RC
4. Run setup

Because of the bug in the F# CTP installer it is important to completeley uninstall both VS2010 and F# CTP before installing new stuff see http://cs.hubfs.net/forums/thread/13145.aspx for more details

NDjango files to be installed:

1. NDjango.Core.dll
2. NDjangoFilters.NDjangoExtension.dll
3. NDjango.Designer.dll
4. NDjango templates

Release Notes:

See Readme.txt for installation instructions and a list of files included in the setup 

New in 0.9.7.0 *NDjango Editor for Microsoft Visual Studio 2010 RC.
 
 *Ndjango.Core and NDjango.Filters are now compiled with FSharp.Core version 4.0 and FSharp.PowerPack version 1.9.9.9.
 *NDjango Editor - code completion now filters the completion dropdown as you type
 *NDjango Editor - error messages are only sent to the error list, where they stay until the file is closed
 
New in 0.9.6.0 *NDjango Editor for Microsoft Visual Studio 2010 beta 2.
 *Ndjango.Core and NDjango.Filters are now bound to the new FSharp.Core version 4.0 and FSharp.PowerPack version 1.9.7.8.

New in 0.9.5.0
 *New NDjango Editor for Microsoft Visual Studio 2010 beta 1. 

New in 0.9.1.3
 *Integration project Bistro Integration is now bound to the Bistro.Core version 0.9.1.0 instead of 0.9.0.0
 *NDjango.Core and NDjango.Filters hasn't changed since 0.9.1.2

New in 0.9.1.2
 *Introduced tags and filters registration using StructureMap.dll
 *StructureMap.dll 2.5.3 included in the setup

New in 0.9.1.0
 *Improved performance - Severely reduced level of interlocking between threads in cross-thread operations.
 *Introduced SimpleTag to simplify creation of new tags:
 *Internal architecture has slightly changed - TemplateManagerProvider introduced instead of static methods. That change may affect those who implemented library-level integration. It does NOT affect those who use Bistro Integration or ASP.MVC Integration projects for integration with NDjango.
 *Extended diagnostic information on parsing errors - errors encountered can be now included in syntax tree and parsing process will not be interrupted.
 *Parsing process behaviour can be changed through Settings dictionary. Two new options added - to enable/disable dynamic templates update support and to enable/disable crash on error while parsing a template.
 *Fixed bug "Problem with view not updating after change" in ASP.MVC Integration.

New in 0.9.0.5
 *moved to new version of F# (1.9.6.16)
 *added new filters
 *some small bugs fixed

New in 0.9.0.4
 *Improved perfomance on big templates

New in 0.9.0.3:
 *Optimzed the ASTWalker - addressed stack overflow in the walker

New in 0.9.0.2:
 *Increased performance on big templates.

New in 0.9.0.1:
 *Setup now includes F# v.1.9.6.2 assemblies needed for NDjango(FSharp.Core.dll, FSharp.PowerPack.dll)
 *Template reader buffer bug fixed.
 *Removed GetTemplate method from ITemplateManager interface.

See www.ndjango.com for more details.