using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.InstrumentModels
{
    public class SplitInstrument
    {
        public string name;
        public DateTime expirationDate;
        public List<InstrumentShared> instrumentList;
    }
}
