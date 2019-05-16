using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MusicNotes.Design
{
    [ToolboxItemFilter("MusicNotes.MarqueeBorder", ToolboxItemFilterType.Require)]
    [ToolboxItemFilter("MusicNotes.MarqueeText", ToolboxItemFilterType.Require)]
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]

    public class MusicNotesControlDesign : WindowsFormsComponentEditor
    {
        public MusicNotesControlDesign()
        {
            Trace.WriteLine("MusicNotesControlDesign ctro");
        }
    }

}
