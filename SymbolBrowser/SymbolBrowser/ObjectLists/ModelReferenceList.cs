namespace Microsoft.SymbolBrowser.ObjectLists
{
    public class ModelReferenceList : ResultList
    {
        public ModelReferenceList(string text, string fName)
            : base(text, fName, 0, LibraryNodeType.Classes)
        {
            // class list
        }
    }
}
