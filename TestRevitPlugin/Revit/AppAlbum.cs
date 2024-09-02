using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace TestRevitPlugin
{
    internal class AppAlbum : IExternalApplication
    {

        

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location,
                iconDirectoryPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\",
                tabName = "Тест";

            application.CreateRibbonTab(tabName);

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Первый");

            PushButtonData numericData = new PushButtonData(nameof(AlbumRevit), "Нумерация листов", assemblyLocation, typeof(AlbumRevit).FullName)
            {
                LargeImage = new BitmapImage(new Uri(iconDirectoryPath+"numeric.png"))
            };
            panel.AddItem(numericData);
            return Result.Succeeded;
        }
    }
}
