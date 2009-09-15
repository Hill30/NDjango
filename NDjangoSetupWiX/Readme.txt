NDjango Requirements:
 *NET Framework 3.5 SP1
 *Visual Studio 2008
 *F# CTP v.1.9.6.16 (You can download it from:  http://download.microsoft.com/download/F/7/4/F74A3170-261C-4E8F-B1A8-2E352C61A89B/InstallFSharp.msi)

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