using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TestRevitAlbum.View.Common
{
    public static class ColorsProject
    {
        public static String Blue { get { return "#24A2E3"; } }
        public static String Yellow { get { return "#E5D819"; } }
        public static String Green { get { return "#7EE6D0"; } }
        public static String Orange { get { return "#E38E24"; } }

        private static List<String> ColorList = new List<String>();
        private static int count = 0;

        private static List<String> usedColors = new List<String>();

        public static void initColors()
        {
            ColorList = new List<String>() 
            {
                  Yellow, Orange, Blue, Green
            };
            usedColors = new List<String>();
        }

        public static String getColor()
        {
            var res = "";
            foreach (String color in ColorList)
            {
                if (usedColors.Count == 0 || !usedColors.Contains(color))
                {
                    res = color;
                    break;
                }
            }
            return res;
        }

        public static void usedColorGroup(String color)
        {
            var clr = ColorList.FirstOrDefault(x => x == color);
            if (clr != null) usedColors.Add(clr);  
        }

        public static void removeFromUsed(String color)
        {
            usedColors.Remove(color);
        }

        public static string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
