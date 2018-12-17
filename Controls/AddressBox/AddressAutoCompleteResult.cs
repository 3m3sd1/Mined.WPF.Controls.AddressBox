using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mined.WPF.Controls
{
    public class AddressAutoCompleteResult
    {
        
        public class Prediction
        {
            public string Description { get; set; }
            public string Place_Id { get; set; }
        }

        public List<Prediction> Predictions { get; set; }
        public string Status { get; set; }
    }

    
}
