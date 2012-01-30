using System;
using System.Windows.Forms;

namespace NDjango.Designer.Misc
{
    /// <summary>
    /// Class containing static helper methods
    /// </summary>
    public static class otherStuff
    {
        public static void ShowErrorDialog(string caption, string msg) 
        {
            MessageBox.Show(
                msg,
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
