using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGT_WPF
{
    public static class Paths
    {
        public static string MgtAppdataFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MGT\"; }
            private set { }
        }




    }
}
