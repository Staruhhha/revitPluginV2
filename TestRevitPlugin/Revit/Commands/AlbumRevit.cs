using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRevitPlugin
{
    [Transaction(TransactionMode.Manual)]
    public class AlbumRevit : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;
            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("OpenDialog");
                if (document.IsFamilyDocument)
                {
                    TaskDialog.Show("Ошибка", "Программа должна запускаться только из проекта.");
                    return Result.Failed;
                }
                MainWindow mainWindow = new MainWindow(uIDocument);
                mainWindow.Show();
                transaction.Commit();
                return Result.Succeeded;
            }
            
        }



    }
}
