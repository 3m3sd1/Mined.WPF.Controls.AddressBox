using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mined.WPF.Controls
{
    public class PlaceDetailsResult
    {
        public class AddressComponent
        {
            public string Long_Name { get; set; }
            public string Short_Name { get; set; }
            public List<string> Types { get; set; }
        }
        public class ResultType
        {
            public List<AddressComponent> Address_Components { get; set; }    
        }

        public ResultType Result { get; set; }
        public string Status { get; set; }
    }
}
