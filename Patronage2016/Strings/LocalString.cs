using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Patronage2016.Strings
{
    public class LocalString
    {
        private ResourceLoader location = new ResourceLoader();
        public string text = String.Empty;
        public LocalString(string txt)
        {
            text = location.GetString(txt);
        }
    }
}
