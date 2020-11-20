using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R2RML_Processor
{
    class LogicalTable
    {
        string tableName;
        string sqlQuery;
        string sqlVersion;

        public LogicalTable()
        {
            tableName = "";
            sqlQuery = "";
            sqlVersion = rr.sqlVersion;
        }
    }
}
