using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRevitPlugin.View.Models
{
    public class Album
    {   
        public Album(String name) {
            this.AlbumName = name;
            this.AlbumSheets = new List<ViewSheet>();
        }

        public string AlbumName {  get; set; }
        public List<ViewSheet> AlbumSheets { get; set; }
    }
}
