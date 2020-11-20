using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF;
namespace R2RML_Processor
{
    class TermMap
    {
        public string constant = "";
        public string column = "";
        public string templateBase = "";
        public string templateValue = "";
        public string template
        {
            get
            {
                return string.Format("{0}{{{1}}}", templateBase, templateValue);
            }
            set
            {
                string[] splited = value.Split('{');
                templateBase = splited[0];
                splited = splited[1].Split('"');
                templateValue = splited[1];
            }
        }
        public string termType = "";
        public string inverseExpression = "";
       // public TermMap() { }
    }
}
