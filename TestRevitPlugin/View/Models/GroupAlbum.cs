using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TestRevitPlugin.View.Models;


namespace TestRevitAlbum.View.Models
{
    public class GroupAlbum
    {
        public string name {  get; set; }
        public List<Album> albums { get; set; } = new List<Album>();
    }
}
