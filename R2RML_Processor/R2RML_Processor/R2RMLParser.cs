using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF;

namespace R2RML_Processor
{
    static class MyPluginExtensions
    {
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }
    }

    class R2RMLParser 
        : MyIRdfReader, ITraceableParser, ITraceableTokeniser
    {
        private bool _traceParsing = false;
        private bool _traceTokeniser = false;
        private TokenQueueMode _queueMode = TokenQueueMode.SynchronousBufferDuringParsing;
        private TurtleSyntax _syntax = TurtleSyntax.W3C;
        IToken nextToken;

         /// <summary>
        /// Creates a new Turtle Parser
        /// </summary>
        public R2RMLParser() { }

        /// <summary>
        /// Creates a new Turtle Parser
        /// </summary>
        /// <param name="syntax">Turtle Syntax</param>
        public R2RMLParser(TurtleSyntax syntax) 
        {
            this._syntax = syntax;
        }

        /// <summary>
        /// Creates a new Turtle Parser which uses the given Token Queue Mode
        /// </summary>
        /// <param name="queueMode">Queue Mode for Tokenising</param>
        public R2RMLParser(TokenQueueMode queueMode)
        {
            this._queueMode = queueMode;
        }

        /// <summary>
        /// Creates a new Turtle Parser which uses the given Token Queue Mode
        /// </summary>
        /// <param name="queueMode">Queue Mode for Tokenising</param>
        /// <param name="syntax">Turtle Syntax</param>
        public R2RMLParser(TokenQueueMode queueMode, TurtleSyntax syntax)
            : this(syntax)
        {
            this._queueMode = queueMode;
        }

        /// <summary>
        /// Gets/Sets whether Parsing Trace is written to the Console
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return this._traceParsing;
            }
            set
            {
                this._traceParsing = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser Trace is written to the Console
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._traceTokeniser;
            }
            set
            {
                this._traceTokeniser = value;
            }
        }

        /// <summary>
        /// Loads a Graph by reading Turtle syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph by reading Turtle syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph by reading Turtle syntax from the given file
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            this.Load(g, new StreamReader(filename, Encoding.UTF8));
        }

        /// <summary>
        /// Loads RDF by reading Turtle syntax from the given input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handle to use</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            //Issue a Warning if the Encoding of the Stream is not UTF-8
            if (!input.CurrentEncoding.Equals(Encoding.UTF8))
            {
#if !SILVERLIGHT
                this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
#else
                this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.GetType().Name + " - Please be aware that parsing errors may occur as a result");
#endif
            }

            this.Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Loads RDF by reading Turtle syntax from the given file using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handle to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            this.Load(handler, new StreamReader(filename, Encoding.UTF8));
        }

        /// <summary>
        /// Loads RDF by reading Turtle syntax from the given input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handle to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                TokenisingParserContext context = new TokenisingParserContext(handler, new TurtleTokeniser(input, this._syntax), this._queueMode, this._traceParsing, this._traceTokeniser);
                //context = new TokenisingParserContext(handler, new TurtleTokeniser(input, this._syntax), this._queueMode, this._traceParsing, this._traceTokeniser);
                this.Parse(context);
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //Catch is just here in case something goes wrong with closing the stream
                    //This error can be ignored
                }
            }
        }

        /// <summary>
        /// Internal method which does the parsing of the input
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(TokenisingParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                //Initialise Buffer and start parsing
                context.Tokens.InitialiseBuffer(10);

                /*IToken*/ nextToken = context.Tokens.Dequeue();
                if (nextToken.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, expected a BOF Token", nextToken);
                }

                do
                {
                    nextToken = context.Tokens.Peek();

                    switch (nextToken.TokenType)
                    {
                        case Token.AT:
                            this.TryParseDirective(context);
                            break;

                        case Token.COMMENT:
                            //Discard and ignore
                            context.Tokens.Dequeue();
                            break;

                        case Token.QNAME:
                        case Token.URI:
                            //Valid for delcration of a Triple Map
                            this.TryParseTriplesMap(context);
                            break;

                        case Token.LITERAL:
                        case Token.LITERALWITHDT:
                        case Token.LITERALWITHLANG:
                        case Token.LONGLITERAL:
                            //Literals not valid as Start of Triples
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, Literals are not valid as Subjects in Turtle", nextToken);

                        case Token.KEYWORDA:
                            //'a' Keyword only valid as Predicate
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, the 'a' Keyword is only valid as a Predicate in Turtle", nextToken);

                        case Token.EOF:
                            //OK - the loop will now terminate since we've seen the End of File
                            break;

                        default:
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered", nextToken);
                    }
                } while (nextToken.TokenType != Token.EOF);

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        /// <summary>
        /// Tries to parse Base/Prefix declarations
        /// </summary>
        /// <param name="context">Parse Context</param>
        private void TryParseDirective(TokenisingParserContext context)
        {
            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse a Base/Prefix Declaration");
            }

            //If we've been called an AT token has been encountered which we can discard
            context.Tokens.Dequeue();

            //Then we expect either a Base Directive/Prefix Directive
            IToken directive = context.Tokens.Dequeue();
            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                //Then expect a Uri for the Base Uri
                IToken u = context.Tokens.Dequeue();
                if (u.TokenType == Token.URI)
                {
                    //Set the Base Uri resolving against the current Base if any
                    try
                    {
                        Uri baseUri = UriFactory.Create(Tools.ResolveUri(u.Value, context.BaseUri.ToSafeString()));
                        context.BaseUri = baseUri;

                        if (!context.Handler.HandleBaseUri(baseUri)) ParserHelper.Stop();
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to set the Base URI to '" + u.Value + "' due to the following error:\n" + rdfEx.Message, u, rdfEx);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + u.GetType().ToString() + "' encountered, expected a URI after a Base Directive", u);
                }
            }
            else if (directive.TokenType == Token.PREFIXDIRECTIVE)
            {
                //Expect a Prefix then a Uri
                IToken pre = context.Tokens.Dequeue();
                if (pre.TokenType == Token.PREFIX)
                {
                    IToken ns = context.Tokens.Dequeue();
                    if (ns.TokenType == Token.URI)
                    {
                        //Register a Namespace resolving the Namespace Uri against the Base Uri
                        try
                        {
                            Uri nsUri = UriFactory.Create(Tools.ResolveUri(ns.Value, context.BaseUri.ToSafeString()));
                            String nsPrefix = (pre.Value.Length > 1) ? pre.Value.Substring(0, pre.Value.Length - 1) : String.Empty;
                            context.Namespaces.AddNamespace(nsPrefix, nsUri);
                            if (!context.Handler.HandleNamespace(nsPrefix, nsUri)) ParserHelper.Stop();
                        }
                        catch (RdfException rdfEx)
                        {
                            throw new RdfParseException("Unable to resolve the Namespace URI '" + ns.Value + "' due to the following error:\n" + rdfEx.Message, ns, rdfEx);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + ns.GetType().ToString() + "' encountered, expected a URI after a Prefix Directive", pre);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + pre.GetType().ToString() + "' encountered, expected a Prefix after a Prefix Directive", pre);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix Directive after an @ symbol", directive);
            }

            //All declarations are terminated with a Dot
            IToken terminator = context.Tokens.Dequeue();
            if (terminator.TokenType != Token.DOT)
            {
                throw ParserHelper.Error("Unexpected Token '" + terminator.GetType().ToString() + "' encountered, expected a Dot Line Terminator to terminate a Prefix/Base Directive", terminator);
            }
        }
        
        /// <summary>
        /// Tries to parse Triples
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseTriplesMap(TokenisingParserContext context)
        {
            /*IToken*/ nextToken = context.Tokens.Dequeue();
            string tripleMapID;
           
            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse Triple Map from the TriplesMap Token '" + nextToken.GetType().ToString() + "'");
            }

            switch (nextToken.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    tripleMapID = ParserHelper.TryResolveUri(context, nextToken).ToString();
                    break;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
            }

            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.KEYWORDA:
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.TriplesMap)
                    { break; }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                    }
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.SEMICOLON:
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            this.TryParseLogicalTable(context, tripleMapID);
        }

        private void TryParseLogicalTable(TokenisingParserContext context, string triplesMapID)
        {
            /*IToken*/ nextToken = context.Tokens.Dequeue();
            string tableName = "";
            string sqlQuery = "";
            string sqlVersion = "";

            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse LogicalTable from the LogicalTable Token '" + nextToken.GetType().ToString() + "'");
            }

            switch (nextToken.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.logicalTable)
                    {
                        break;
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                    }

                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            nextToken = context.Tokens.Dequeue();
            switch(nextToken.TokenType)
            {
                case Token.LEFTSQBRACKET:
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }


            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.tableName)
                    {
                        nextToken = context.Tokens.Dequeue();
                        if(nextToken.TokenType == Token.LITERAL)
                            {
                                tableName = nextToken.Value;
                                break;
                            }
                            else{
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                    }
                    else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.sqlQuery)
                    {
                        nextToken = context.Tokens.Dequeue();
                        if (nextToken.TokenType == Token.LITERAL || nextToken.TokenType == Token.LONGLITERAL)
                        {
                            sqlQuery = nextToken.Value;
                            break;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                    }
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            nextToken = context.Tokens.Peek();
            switch (nextToken.TokenType)
            {
                case Token.SEMICOLON:
                    context.Tokens.Dequeue();
                    nextToken = context.Tokens.Peek();
                    break;
            }

            switch (nextToken.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.sqlVersion)
                    {
                        context.Tokens.Dequeue();
                        nextToken = context.Tokens.Dequeue();
                        if (nextToken.TokenType == Token.QNAME || nextToken.TokenType == Token.URI)
                        {
                            sqlVersion = ParserHelper.TryResolveUri(context, nextToken).ToString();
                            nextToken = context.Tokens.Peek();
                            break;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                    } 
            }

            switch (nextToken.TokenType)
            {
                case Token.SEMICOLON:
                    context.Tokens.Dequeue();
                    break;
            }

            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.RIGHTSQBRACKET:
                    nextToken = context.Tokens.Dequeue();
                    if (nextToken.TokenType == Token.SEMICOLON)
                        break;
                    else
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString(), nextToken);

                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString(), nextToken);
            }

            this.TryParseSubjectMap(context, triplesMapID, tableName, sqlQuery, sqlVersion);
        }

        private void TryParseSubjectMap(TokenisingParserContext context, string triplesMapID, string tableName, string sqlQuery, string sqlVersion)
        {
            /*IToken*/ nextToken = context.Tokens.Dequeue();
            //SubjectMap subjectMap = new SubjectMap();
            SubjectMap subjectMap = new SubjectMap();

            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse Triple Map from the TriplesMap Token '" + nextToken.GetType().ToString() + "'");
            }

            switch (nextToken.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                    if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.subject)
                    {
                        nextToken = context.Tokens.Dequeue();
                        switch (nextToken.TokenType)
                        {
                            case Token.QNAME:
                            case Token.URI:
                                subjectMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                this.TryParsePredicateObjectMap(context, triplesMapID, tableName, sqlQuery, sqlVersion, subjectMap);
                                return;
                                //break;

                            default:
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        break;
                    }
                    else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.subjectMap)
                    {
                        break;
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                    }


                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
            }

            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.LEFTSQBRACKET:
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            int condition = 1;
            string type = "";
            while (condition != 0)
            {
                nextToken = context.Tokens.Dequeue();
                switch (nextToken.TokenType)
                {
                    case Token.RIGHTSQBRACKET:
                        if (condition == 1)
                        {
                            //subjectmap khalie
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                        else if (type == "")
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken); 
                        }
                        else
                        {
                            //end of map
                            condition = 0;
                            break;
                        }

                    case Token.SEMICOLON:
                        break;

                    case Token.QNAME:
                    case Token.URI:
                        if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.constant)
                        {
                            if (condition != 2)
                            {
                                nextToken = context.Tokens.Dequeue();
                                switch (nextToken.TokenType)
                                {
                                    case Token.QNAME:
                                    case Token.URI:
                                        subjectMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                        type = rr.constant;
                                        condition = 2;
                                        break;

                                    default:
                                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                                }
                                break;
                            }
                            else
                            {
                                //semantic error
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.column)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LITERAL:
                                    subjectMap.column = nextToken.Value;
                                    type = rr.column;
                                    condition = 2;
                                    break;
                                    
                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.template)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LITERAL:
                                    subjectMap.template = nextToken.Value;
                                    type = rr.template;
                                    condition = 2;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.termType)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.QNAME:
                                case Token.URI:
                                    string str = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                    if (str == rr.rrNameSpace + rr.IRI)
                                    {
                                        subjectMap.termType = rr.rrNameSpace + rr.IRI;
                                        break;
                                    }
                                    else if (str == rr.rrNameSpace + rr.BlankNode)
                                    {
                                        subjectMap.termType = rr.rrNameSpace + rr.BlankNode;
                                        break;
                                    }
                                    else { throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken); }

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.language)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.datatype)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);                            
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.inverseExpression)
                        {
                            if(type == "constant")
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LONGLITERAL:
                                case Token.LITERAL:
                                    subjectMap.inverseExpression = nextToken.Value;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.graph)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.QNAME:
                                case Token.URI:
                                    subjectMap.graphMap = new GraphMap();
                                    subjectMap.graphMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.graphMap)
                        {
                            try
                            {
                                subjectMap.graphMap = new GraphMap();
                                subjectMap.graphMap = TryParseGraphMap(context);
                            }
                            catch
                            {
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.clasS)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.QNAME:
                                case Token.IRI:
                                case Token.URI:
                                    subjectMap.clasS = new List<string>();
                                    subjectMap.clasS.Add(ParserHelper.TryResolveUri(context, nextToken).ToString());
                                    while (true)
                                    {
                                        nextToken = context.Tokens.Peek();
                                        if (nextToken.TokenType == Token.COMMA)
                                        {
                                            context.Tokens.Dequeue();
                                            nextToken = context.Tokens.Dequeue();
                                            switch (nextToken.TokenType)
                                            {
                                                case Token.QNAME:
                                                case Token.IRI:
                                                case Token.URI:
                                                    subjectMap.clasS.Add(ParserHelper.TryResolveUri(context, nextToken).ToString());
                                                    break;
                                               
                                                default:
                                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    break;
                                    
                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;

                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                }
            }
            nextToken = context.Tokens.Peek();
            switch (nextToken.TokenType)
            {
                case Token.SEMICOLON:
                    context.Tokens.Dequeue();
                    break;
            }
            this.TryParsePredicateObjectMap(context, triplesMapID, tableName, sqlQuery, sqlVersion, subjectMap);
            
        }

        GraphMap TryParseGraphMap(TokenisingParserContext context)
        {
            GraphMap graphMap = new GraphMap();

            int condition = 1;
            string type = "";
            while (condition != 0)
            {
                nextToken = context.Tokens.Dequeue();
                switch (nextToken.TokenType)
                {
                    case Token.RIGHTSQBRACKET:
                        if (condition == 1)
                        {
                            //subjectmap khalie
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                        else if (type == "")
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                        else
                        {
                            //end of map
                            condition = 0;
                            break;
                        }

                    case Token.SEMICOLON:
                        break;

                    case Token.QNAME:
                    case Token.URI:
                        if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.constant)
                        {
                            if (condition != 2)
                            {
                                nextToken = context.Tokens.Dequeue();
                                switch (nextToken.TokenType)
                                {
                                    case Token.QNAME:
                                    case Token.URI:
                                        graphMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                        type = rr.constant;
                                        condition = 2;
                                        break;

                                    default:
                                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                                }
                                break;
                            }
                            else
                            {
                                //semantic error
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.column)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LITERAL:
                                    graphMap.column = nextToken.Value;
                                    type = rr.column;
                                    condition = 2;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.template)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LITERAL:
                                    graphMap.template = nextToken.Value;
                                    type = rr.template;
                                    condition = 2;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.termType)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.QNAME:
                                case Token.URI:
                                    string str = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                    if (str == rr.rrNameSpace + rr.IRI)
                                    {
                                        graphMap.termType = rr.rrNameSpace + rr.IRI;
                                        break;
                                    }
                                    else { throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken); }

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.language)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.datatype)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.inverseExpression)
                        {
                            //shak daram inam dare ya na
                            if (type == "constant")
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);

                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LONGLITERAL:
                                case Token.LITERAL:
                                    graphMap.inverseExpression = nextToken.Value;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                       
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                }
            }

            return graphMap;
        }

        void TryParsePredicateObjectMap(TokenisingParserContext context, string triplesMapID, string tableName, string sqlQuery, string sqlVersion, SubjectMap subjectMap)
        {
            /*IToken*/
            nextToken = context.Tokens.Dequeue();
            PredicateObjectMap predicateObjectMap = new PredicateObjectMap();
            PredicateMap predicateMap = new PredicateMap();
            ObjectMap objectMap = new ObjectMap();
            RefObjectMap refObjectMap = new RefObjectMap();

            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse Triple Map from the TriplesMap Token '" + nextToken.GetType().ToString() + "'");
            }

            int condition = 1;  // 1->predicateObjectMap
                                // 2->predicateMap
                                // 3->objectMap
            while (condition != 0)
            {
                switch (nextToken.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.predicateObjectMap)
                        {
                            if (condition == 1)
                            {
                                break;
                            }
                            else
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);

                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }


                    default:
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                }

                nextToken = context.Tokens.Dequeue();
                switch (nextToken.TokenType)
                {
                    case Token.LEFTSQBRACKET:
                        break;
                    default:
                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                }

                nextToken = context.Tokens.Dequeue();
                switch (nextToken.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.predicate)
                        {
                            if (condition == 1)
                            {
                                nextToken = context.Tokens.Dequeue();
                                switch (nextToken.TokenType)
                                {
                                    case Token.QNAME:
                                    case Token.URI:
                                        predicateMap = new PredicateMap();
                                        predicateMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                        condition = 2;
                                        nextToken = context.Tokens.Peek();
                                        if (nextToken.TokenType == Token.SEMICOLON)
                                        {
                                            context.Tokens.Dequeue();
                                        }
                                        break;

                                    default:
                                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                                }
                                break;
                            }
                            else
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                       
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToSafeString() == rr.rrNameSpace + rr.predicateMap)
                        {
                            if (condition == 1)
                            {
                                predicateMap = TryParsePredicateMap(context);
                                condition = 2;
                                break;
                            }
                            else throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                } //predicateMap switch

                nextToken = context.Tokens.Dequeue();
                switch (nextToken.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.objecT)
                        {
                            if (condition == 2)
                            {
                                nextToken = context.Tokens.Dequeue();
                                switch (nextToken.TokenType)
                                {
                                    case Token.QNAME:
                                    case Token.URI:
                                    case Token.LITERAL:
                                        objectMap = new ObjectMap();
                                        objectMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                        condition = 3;
                                        break;

                                    default:
                                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                                }
                                break;
                            }
                            else
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToSafeString() == rr.rrNameSpace + rr.objectMap)
                        {
                            if (condition == 2)
                            {
                                objectMap = TryParseObjectMap(context);
                                predicateObjectMap.AddPair(predicateMap, objectMap);
                                condition = 3;
                                break;
                            }
                            else throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                } //objectMap switch


                nextToken = context.Tokens.Peek();
                switch(nextToken.TokenType)
                {
                    case Token.SEMICOLON:
                        if (condition == 3)
                        {
                            condition = 1;
                            context.Tokens.Dequeue();
                            break;
                        }
                        else throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                }
                
            } //end Of While
            //TriplesMap triplesMap = new TriplesMap(subjectMap, predicateObjectMap);
            return;
        }

        
        PredicateMap TryParsePredicateMap(TokenisingParserContext context)
        {
            PredicateMap predicateMap = new PredicateMap();
            nextToken = context.Tokens.Dequeue();
            switch (nextToken.TokenType)
            {
                case Token.LEFTSQBRACKET:
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
            }

            int condition = 1;
            string type = "";
            while (condition != 0)
            {
                nextToken = context.Tokens.Dequeue();
                switch (nextToken.TokenType)
                {
                    case Token.RIGHTSQBRACKET:
                        if (condition == 1)
                        {
                            //subjectmap khalie
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                        else if (type == "")
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                        else
                        {
                            //end of map
                            condition = 0;
                            break;
                        }

                    case Token.SEMICOLON:
                        break;

                    case Token.QNAME:
                    case Token.URI:
                        if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.constant)
                        {
                            if (condition != 2)
                            {
                                nextToken = context.Tokens.Dequeue();
                                switch (nextToken.TokenType)
                                {
                                    case Token.QNAME:
                                    case Token.URI:
                                        predicateMap.constant = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                        type = rr.constant;
                                        condition = 2;
                                        break;

                                    default:
                                        throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                                }
                                break;
                            }
                            else
                            {
                                //semantic error
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.column)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LITERAL:
                                    predicateMap.column = nextToken.Value;
                                    type = rr.column;
                                    condition = 2;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.template)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LITERAL:
                                    predicateMap.template = nextToken.Value;
                                    type = rr.template;
                                    condition = 2;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.termType)
                        {
                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.QNAME:
                                case Token.URI:
                                    string str = ParserHelper.TryResolveUri(context, nextToken).ToString();
                                    if (str == rr.rrNameSpace + rr.IRI)
                                    {
                                        predicateMap.termType = rr.rrNameSpace + rr.IRI;
                                        break;
                                    }
                                    else if (str == rr.rrNameSpace + rr.BlankNode)
                                    {
                                        predicateMap.termType = rr.rrNameSpace + rr.BlankNode;
                                        break;
                                    }
                                    else { throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken); }

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.language)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.datatype)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                        }
                        else if (ParserHelper.TryResolveUri(context, nextToken).ToString() == rr.rrNameSpace + rr.inverseExpression)
                        {
                            if (type == "constant")
                                throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);

                            nextToken = context.Tokens.Dequeue();
                            switch (nextToken.TokenType)
                            {
                                case Token.LONGLITERAL:
                                case Token.LITERAL:
                                    predicateMap.inverseExpression = nextToken.Value;
                                    break;

                                default:
                                    throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a TriplesMap", nextToken);
                            }
                            break;
                        }
                   
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + nextToken.GetType().ToString() + "' encountered, this Token is not valid as the TriplesMap", nextToken);
                        }
                }
            }
            nextToken = context.Tokens.Peek();
            switch (nextToken.TokenType)
            {
                case Token.SEMICOLON:
                    context.Tokens.Dequeue();
                    break;
            }
            return predicateMap;
        }

        ObjectMap TryParseObjectMap(TokenisingParserContext context)
        {
            ObjectMap objectMap = new ObjectMap();
            return objectMap;
        }
        RefObjectMap TryParseRefObjectMap(TokenisingParserContext context)
        {
            RefObjectMap refObjectMap = new RefObjectMap();
            return refObjectMap;
        }


        public event RdfReaderWarning Warning;

        private void RaiseWarning(String message)
        {
            RdfReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }
        public override string ToString()
        {
            return "Turtle";
        }
    }

}
