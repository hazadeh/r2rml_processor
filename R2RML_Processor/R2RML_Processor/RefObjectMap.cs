using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R2RML_Processor
{
    class RefObjectMap : TermMap
    {
        //public TermMap termMap;
        TriplesMap parentTriplesMap;
        List<Join> JoinConditions;
        string jointSQLquery;
    }
}
