using System;
using System.Resources;
using System.Reflection;
using System.Globalization;  // Ajoutez cette ligne pour accéder à CultureInfo

namespace Sudoku.HumainHabituel
{
    public class Resources
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    ResourceManager temp = new ResourceManager("Sudoku.HumainHabituel.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        public static CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        public static string humain_py
        {
            get
            {
                return ResourceManager.GetString("humain.py", resourceCulture);
            }
        }
    }
}
