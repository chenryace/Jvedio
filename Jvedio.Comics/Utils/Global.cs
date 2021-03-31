using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Comics
{
    public static class Global
    {
        public static Window GetWindowByName(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }
            return null;
        }
    }
}
