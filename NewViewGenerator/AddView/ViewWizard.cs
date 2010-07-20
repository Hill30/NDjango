using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using NDjango;
using NDjango.Interfaces;
using NDjango.Designer;
using NDjango.Designer.Parsing;
using EnvDTE;

namespace NewViewGenerator
{
    public class ViewWizard: IVsSelectionEvents
    {
        public static IVsMonitorSelection SelectionService = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
        public static DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
        public static uint contextCookie;
        public static uint selectionCookie;
        public ViewWizard()
        {
            SelectionService.AdviseSelectionEvents(this, out selectionCookie);
            contextCookie = RegisterContext();
        }
        public void Update()
        {
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppHier,ppSC;
            object directory = "";
            if (ErrorHandler.Succeeded(SelectionService.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)))
            {
                try
                {
                    hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
                    hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectDir, out directory);
                    projectDir = directory.ToString();
                }
                finally
                {
                    Marshal.Release(ppHier);
                }
            }
            if (parser == null)
                parser = InitializeParser();
            if (templatesDir == null)
            {
                templatesDir = new TemplateDirectory();
                templatesDir.selectionTracker = SelectionService;
            }
        }
        #region private fields
        string projectDir;
        string projectName;
        string viewsFolderName;
        INode blockNameNode = null;
        ProjectItems viewsFolder;
        IVsHierarchy hierarchy;
        ITemplateManager parser;
        TemplateDirectory templatesDir;
        #endregion

        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }
        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
              return VSConstants.S_OK;
        }
        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld,IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld,IVsHierarchy pHierNew, uint itemidNew,IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {

            if (pHierNew != null)
            {
                string itemName;
                //pHierNew.GetProperty(itemidNew, (int)__VSHPROPID.VSHPROPID_Name, out itemName);
                pHierNew.GetCanonicalName(itemidNew, out itemName);
                bool activectx = itemName != null && (itemName.ToString().Contains("Views") ||itemName.ToString().Contains("views"));
                if (activectx)
                {
                    object temp;
                    hierarchy = pHierNew;
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT,(int)__VSHPROPID.VSHPROPID_ProjectDir, out temp);
                    projectDir = temp.ToString();
                    //root = projectFullName.Substring(0, projectFullName.LastIndexOf('\\') + 1);
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out temp);
                    projectName = temp.ToString();
                    viewsFolderName = itemName.ToString();
                }
                int factive = (activectx)? 1 : 0;
                SelectionService.SetCmdUIContext(contextCookie, factive);

            }
            return VSConstants.S_OK;

        }
        public void AddNewItemFromVsTemplate(string templateName, string language, string name)
        {
            if (name == null)
                throw new ArgumentException("name");
            int activeProject = GetActiveProject();
            ProjectItems parent = dte.Solution.Projects.Item(activeProject).ProjectItems;
            if (parent == null)
                throw new ArgumentException("project");

            EnvDTE80.Solution2 sol = dte.Solution as EnvDTE80.Solution2;
            string filename = sol.GetProjectItemTemplate(templateName, language);
            parent.AddFromTemplate(filename, name);
        }
        
        public void AddFromFile(string fileName,string itemName)
        {
            int rootLen = projectDir.Length;
            string folderName = viewsFolderName.Substring(rootLen + 1, viewsFolderName.Length - rootLen - 1);
            int index = GetActiveProject();
            if (index > 0)
            {
                viewsFolder = dte.Solution.Projects.Item(index).ProjectItems; ;//default ViewsFolder is  the root of the project
                SearchFolder(folderName, viewsFolder);//find the real folder the new view must be inserted to
                viewsFolder.AddFromTemplate(fileName, itemName);
                int i = 1;
                for (; i < viewsFolder.Count; i++)
                    if (viewsFolder.Item(i).Name == itemName)
                        break;
                //EnvDTE.Constants.vsViewKindCode = {7651A701-06E5-11D1-8EBD-00A0C90F26EA}
                viewsFolder.Item(i).Open("{7651A701-06E5-11D1-8EBD-00A0C90F26EA}").Visible = true;
                
            }
        }
        public List<Assembly> GetReferences()
        {
            Project project = dte.Solution.Projects.Item(1);
            List<Assembly> list = new List<Assembly>();
            if (project.Object is VSLangProj.VSProject)
            {
                VSLangProj.VSProject vsproject = (VSLangProj.VSProject)project.Object;
                foreach (VSLangProj.Reference reference in vsproject.References)
                {
                    try
                    {
                        if (reference.StrongName)
                            //System.Configuration, Version=2.0.0.0,
                            //Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A
                            list.Add(Assembly.Load(

                                reference.Identity +
                                ", Version=" + reference.Version +
                                ", Culture=" + (string.IsNullOrEmpty(reference.Culture) ?
                                "neutral" : reference.Culture) +
                                ", PublicKeyToken=" + reference.PublicKeyToken));
                        else
                            list.Add(Assembly.Load(reference.Path));
                    }
                    catch (System.IO.FileLoadException ex)
                    {
                    }

                }
            }
            else if (project.Object is VsWebSite.VSWebSite)
            {
                VsWebSite.VSWebSite vswebsite = (VsWebSite.VSWebSite)project.Object;
                foreach (VsWebSite.AssemblyReference reference in vswebsite.References)
                    list.Add(Assembly.Load(reference.StrongName));
            }
            return list;

        }

        public IEnumerable<string> GetTemplates(string root)
        {
            return templatesDir.GetTemplates(root);
        }
        public IEnumerable<string> Recent5Templates
        {
            get { return templatesDir.Recent5Templates; }
        }
        public void RegisterInserted(string inserted)
        {
            templatesDir.RegisterInserted(inserted);
        }
        
        /// <summary>
        /// Gets a list of block names recursively from base template using ASTNode - BlockNameNode 
        /// </summary>
        /// <param name="template">temporary template to parse or file path to that template</param>
        /// <returns></returns>
        public List<string> GetTemplateBlocks(string template)
        {
            var nodes = parser.GetTemplate(template, new NDjango.TypeResolver.DefaultTypeResolver()).Nodes;
            List<string> blocks = new List<string>();
            foreach (INode node in nodes)
            {
                if (node.NodeType == NodeType.BlockName)
                    break;
                else
                    for (int i = 0; i < node.Nodes.Count; i++ )
                        FindBlockNameNode(node.Nodes.Values.ElementAt(i));
            }
            if (blockNameNode != null)
            {
                var completion_provider = blockNameNode as ICompletionValuesProvider;
                if (completion_provider != null)
                blocks.AddRange(completion_provider.Values); 

            }
            return blocks;
        }
        /// <summary>
        /// In case when the solution has several project - necessary to find out the index of the active project
        /// </summary>
        /// <returns>index in Solution.Projects collection</returns>
        private int GetActiveProject()
        {
            int i = 1;
            foreach (Project p in dte.Solution.Projects)
                if (String.Compare(p.Name, projectName) == 0)
                    return i;
                else
                    i++;
            return -1;//if there is no project with specified name in the collection
        }
        /// <summary>
        /// Helper method to walk through the project tree and find folder with the specified name
        /// </summary>
        /// <param name="folder">name of the folder to search in the project hierarchy</param>
        /// <param name="parent">recursion step variable,node in the project hierarchy</param>
        private void SearchFolder(string folder, ProjectItems parent)
        {
            if (String.IsNullOrEmpty(folder))
                return;
            foreach (ProjectItem pi in parent)
            {
                if (folder.StartsWith(pi.Name, true, System.Globalization.CultureInfo.CurrentCulture))
                {
                    if (String.Compare(folder, pi.Name, true) != 0
                        && String.Compare(folder,pi.Name + "\\",true) != 0)
                    {
                        folder = folder.Remove(0, pi.Name.Length + 1);
                        if (folder.EndsWith("\\"))
                            folder = folder.Remove(folder.Length - 1, 1);
                        SearchFolder(folder, pi.ProjectItems);
                    }
                    else
                    {
                        viewsFolder =  pi.ProjectItems;
                    }
                }
            }

        }
        /// <summary>
        /// Creates custom UICONTEXT for MenuCommand visibility tracking.
        /// For example,it is possible to set the restriction to add new view only in Views folder and its subfolders
        /// </summary>
        /// <returns>cookie of the registered custom UICONTEXT</returns>
        private uint RegisterContext()
        {
            uint retVal;
            Guid uiContext = GuidList.UICONTEXT_ViewsSelected;
            SelectionService.GetCmdUIContextCookie(ref uiContext, out retVal);
            return retVal;

        }
        /// <summary>
        /// Helper method to walk through parsed template AST
        /// </summary>
        /// <param name="nodes">parameter for the next step of recursion</param>
        private void FindBlockNameNode(IEnumerable<INode> nodes)
        {
            foreach (INode subnode in nodes)
            {
                if (subnode.NodeType == NodeType.BlockName)
                {
                    blockNameNode = subnode;
                    break;
                }
                else
                    for (int i = 0; i < subnode.Nodes.Values.Count; i++)
                    {
                        FindBlockNameNode(subnode.Nodes.Values.ElementAt(i));
                    }
            }
        }
        private ITemplateManager InitializeParser()
        {

            TemplateLoader template_loader  = new TemplateLoader(projectDir);
            List<Tag> tags = new List<Tag>();
            List<Filter> filters = new List<Filter>();
            TemplateManagerProvider provider = new TemplateManagerProvider();
            return provider
                    .WithTags(tags)
                    .WithFilters(filters)
                    .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false)
                    .WithLoader(template_loader)
                    .GetNewManager();

        }

        

        

    }
}
