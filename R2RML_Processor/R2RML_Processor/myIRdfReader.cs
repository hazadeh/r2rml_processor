using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;

namespace R2RML_Processor
{
    public interface MyIRdfReader
    {
        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IGraph g, StreamReader input);

        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Input
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IGraph g, TextReader input);

        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax from a given File
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="filename">The Filename of the File to read from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the File</exception>
        void Load(IGraph g, String filename);

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IRdfHandler handler, StreamReader input);

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IRdfHandler handler, TextReader input);

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax from a given File
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">The Filename of the File to read from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IRdfHandler handler, String filename);

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        event RdfReaderWarning Warning;
    }
}

//namespace VDS.RDF.Parsing
//{
//    /// <summary>
//    /// Interface for Parsers that support Tokeniser Tracing
//    /// </summary>
//    public interface ITraceableTokeniser
//    {
//        /// <summary>
//        /// Gets/Sets whether Tokeniser Tracing is used
//        /// </summary>
//        bool TraceTokeniser
//        {
//            get;
//            set;
//        }
//    }

//    /// <summary>
//    /// Interface for Parsers that support Parser Tracing
//    /// </summary>
//    public interface ITraceableParser
//    {
//        /// <summary>
//        /// Gets/Sets whether Parser Tracing is used
//        /// </summary>
//        bool TraceParsing
//        {
//            get;
//            set;
//        }
//    }
//
//}
