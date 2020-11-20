using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R2RML_Processor
{
    class ObjectMap : TermMap 
    {
        TermMap termMap;
        Uri dataType;
        string language;

        TriplesMap parentTriplesMap;
        List<Join> JoinConditions;
        string jointSQLquery;

    }
}
