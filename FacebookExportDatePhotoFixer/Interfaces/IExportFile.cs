using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacebookExportDatePhotoFixer.Data.JSON;
using static FacebookExportDatePhotoFixer.Data.JSON.Conversation;

namespace FacebookExportDatePhotoFixer.Interfaces
{
    interface IExportFile
    {
        string Location { get; }
    }
}
