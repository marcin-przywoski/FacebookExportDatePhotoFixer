using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FacebookExportDatePhotoFixer.Data.HTML;

namespace FacebookExportDatePhotoFixer.Interfaces
{
    interface IExport
    {
        string Location { get; set; }

        string Destination { get; set; }

        CultureInfo Language { get; set; }

        void GetLanguage();

        void GetExportFiles();

        void GetMessagesFromExportFiles();
        void ProcessExportFiles(CheckBox changeNameCheckbox);
        //int GetAmountOfMessagesInExport(List<HtmlFile>);
    }
}
