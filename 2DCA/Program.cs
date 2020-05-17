using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DCA
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine(FindAssets("assets"));
            Application.Run(new Form1());
        }

        static bool FindAssets(string dirName)
        {
            while (!File.Exists("2DCA.sln"))
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
                Directory.SetCurrentDirectory("..");
            }
            if (Directory.Exists(dirName))
            {
                Directory.SetCurrentDirectory(dirName);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
