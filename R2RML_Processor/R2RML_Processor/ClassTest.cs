using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Storage;
using VDS.RDF.Query;

namespace R2RML_Processor
{
    public class ClassTest
    {
        public void Testings()
        {
            //*********************
            //******URI Nodes******
            //*********************
            Graph g = new Graph();
            g.BaseUri = UriFactory.Create("http://example.org/");
            IUriNode graphUri = g.CreateUriNode();
            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            //Create a URI Node using a QName
            //Need to define a Namespace first
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/namespace/"));
            IUriNode qname = g.CreateUriNode("ex:demo");



            //*********************
            //*****Blank Nodes*****
            //*********************
            //Create an anonymous Blank Node
            //Each call to this constructor generates a Blank Node with a new unique identifier within the Graph
            IBlankNode anon = g.CreateBlankNode();
            //Create a named Blank Node
            //Reusing the same ID results in the same Blank Node within the Graph
            //Note that if the ID refers to an automatically assigned ID that is already in use the returned
            //Blank Node will be given an alternative ID
            IBlankNode named = g.CreateBlankNode("ID");



            //*********************
            //****Literal Nodes****
            //*********************
            //Create a Plain Literal
            ILiteralNode plain = g.CreateLiteralNode("some value");
            //Create some Language Specified Literal
            ILiteralNode hello = g.CreateLiteralNode("hello", "en");
            ILiteralNode bonjour = g.CreateLiteralNode("bonjour", "fr");
            //Create some typed Literals
            //You'll need to be using the VDS.RDF.Parsing namespace to reference the constants used here
            ILiteralNode number = g.CreateLiteralNode("1", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            ILiteralNode boolean = g.CreateLiteralNode("true", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));

            //Create two Literal Nodes which both represent the Integer 1
            //You'll need to be using the VDS.RDF.Parsing namespace to reference the constants used here
            ILiteralNode one1 = g.CreateLiteralNode("1", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            ILiteralNode one2 = g.CreateLiteralNode("0001", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            //Are they equal
            bool equal = one1.Equals(one2);
            //Prints false since the string values are not equal
            MessageBox.Show(equal.ToString()); //Console.WriteLine(equal.ToString());
            //Use Loose Literal Equality Mode
            Options.LiteralEqualityMode = LiteralEqualityMode.Loose;
            //Are they equal
            equal = one1.Equals(one2);
            //Prints true since the typed values are equal
            MessageBox.Show(equal.ToString()); //Console.WriteLine(equal.ToString());


            //*********************
            //*******Triples*******
            //*********************
            //Create some Nodes
            /*IUriNode*/
            dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode createdBy = g.CreateUriNode(UriFactory.Create("http://example.org/createdBy"));
            ILiteralNode robVesse = g.CreateLiteralNode("Rob Vesse");
            //Assert this Triple
            Triple t = new Triple(dotNetRDF, createdBy, robVesse);
            g.Assert(t);
            g.Retract(t);


            //*********************
            //*****TripleStore*****
            //*********************

        } //endOf Testings

        public void HelloWorld(Form1 form)
        {
            Graph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://example.org/says"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
            ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");
            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));

            this.show(g.Triples, form);

            NTriplesWriter ntwriter = new NTriplesWriter();
            ntwriter.Save(g, "HelloWorld.nt");

            RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
            rdfxmlwriter.Save(g, "HelloWorld.rdf");

            TurtleWriter turtlewriter = new TurtleWriter();
            turtlewriter.Save(g, "HelloWorld.ttl");
        } //endOf HelloWorld

        private void show(BaseTripleCollection ts, Form1 form, string mode="new")
        {
            string result = "";
            foreach (Triple t in ts)
            {
                result += t.ToString() + "\n#\n";
            }
            if(mode == "add")
                form.rchbx.Text += result + "\n*\n";
            else
                form.rchbx.Text = result;
        } //endOf show

        public void Reading(Form1 form)
        {
            StringBuilder sb = new StringBuilder();
            var sw = new  System.IO.StringWriter(sb);

            Console.SetOut(sw); // redirect



            //*********************
            //****Graph Parsers****
            //*********************
            Graph g = new Graph();
            Graph h = new Graph();
            TurtleParser ttlparser = new TurtleParser();
            //ttlparser.TraceParsing = true;
            ttlparser.TraceTokeniser = true;
            //Load using a Filename
            ttlparser.Load(g, "HelloWorld.ttl");
            this.show(g.Triples, form, "add");
            //Load using a StreamReader
            ttlparser.Load(h, new StreamReader("HelloWorld.ttl"));
            this.show(h.Triples, form, "add");
      

            try
            {
                g = new Graph();
                NTriplesParser ntparser = new NTriplesParser();

                //Load using Filename
                ntparser.Load(g, "HelloWorld.nt");
                this.show(g.Triples, form, "add");
            }
            catch (RdfParseException parseEx)
            {
                MessageBox.Show("Parser Error: ", parseEx.Message);
            }
            catch (RdfException rdfEx)
            {
                MessageBox.Show("RDF Error: ", rdfEx.Message);
            }


            //*********************
            //*****Reading RDF*****
            //*********************
            FileLoader.Load(g, "HelloWorld.rdf");
            this.show(g.Triples, form, "add");
            //UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Barack_Obama")); //RdfException Or WebException
            this.show(g.Triples, form, "add");



            //EmbeddedResourceLoader.Load(g, "Your.Namespace.EmbeddedFile.n3, YourAssembly");



            //StringParser.Parse(g, "<http://example.org/a> <http://example.org/b> <http://example.org/c>.");

            Graph payam = new Graph();
            NTriplesParser parser = new NTriplesParser();
            parser.Load(payam, new StringReader("<http://example.org/a> <http://example.org/b> <http://example.org/c>."));
            this.show(payam.Triples, form, "add");



            //*********************
            //****Store Parsers****
            //*********************
            try
            {
                TripleStore store = new TripleStore();
                TriGParser trigparser = new TriGParser();
                //Load the Store
                trigparser.Load(store, "HelloWorld.rdf");

                form.rchbx.Text = "";
                foreach (Triple t in store.Triples)
                {
                    form.rchbx.Text += t.ToString() + "\n";
                }
            }
            catch (RdfException re)
            {
                MessageBox.Show(re.Message);
            }
            catch
            {
                MessageBox.Show(":-\"");
            }



            //*********************
            //**Advanced Parsing***
            //*********************
            //Create a Handler and use it for parsing
            CountHandler handler = new CountHandler();
            TurtleParser tparser = new TurtleParser();
            tparser.Load(handler, "HelloWorld.ttl");
            //Print the resulting count
            form.rchbx.Text += "#Advanced Parsing:Counting     " + handler.Count + " Triple(s)";
         
        } //endOf Reading

        public void Writing(Form1 form)
        {
            Graph g = new Graph();
            TurtleParser ttlparser = new TurtleParser();
            ttlparser.Load(g, "HelloWorld.ttl");



            //*********************
            //*****Basic Usage*****
            //*********************
            //Assume that the Graph to be saved has already been loaded into a variable g 
            RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
            //Save to a File
            rdfxmlwriter.Save(g, "Example.rdf");
            //Save to a Stream
            //rdfxmlwriter.Save(g, Console.StandardOut);
            form.rchbx.Text = "#Basic Usage\n";
            form.rchbx.Text += rdfxmlwriter.ToString() + "\n";



            //*********************
            //*Writing to Strings**
            //*********************
            form.rchbx.Text += "\n#Writing to string\n";
            //1. Assume that the Graph to be saved has already been loaded into a variable g RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
            String data = VDS.RDF.Writing.StringWriter.Write(g, rdfxmlwriter);
            form.rchbx.Text += data + "\n";

            //Assume that the Graph to be saved has already been loaded into a variable g RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
            System.IO.StringWriter sw = new System.IO.StringWriter();
            //Call the Save() method to write to the StringWriter
            rdfxmlwriter.Save(g, sw);
            //We can now retrieve the written RDF by using the ToString() method of the StringWriter
            data = sw.ToString();
            form.rchbx.Text += data + "\n";



            //*********************
            //***Advanced Usage****
            //*********************
//            public static void SaveGraph(IGraph g, IRdfWriter writer, String filename)
//{
//        //Set Pretty Print Mode on if supported
//        if (writer is IPrettyPrintingWriter) {
//                ((IPrettyPrintingWriter)writer).PrettyPrintMode = true;
//}
//        //Set High Speed Mode forbidden if supported
//        if (writer is IHighSpeedWriter) {
//                ((IHighSpeedWriter)writer).HighSpeedModePermitted = false;
//}
//        //Set Compression Level to High if supported
//        if (writer is ICompressingWriter) {
//                ((ICompressingWriter)writer).CompressionLevel = WriterCompressionLevel.High;
//}
//        //Save the Graph
//        writer.Save(g, filename);
//}



            //*********************
            //*****Formatters******
            //*********************
            form.rchbx.Text += "\n\n#Formatters\n";
            //Assumes that we already have a Graph in the variable g
            TurtleFormatter formatter = new TurtleFormatter();
            //Want to get only the triples defining types - assumes rdf: prefix is appropriately defined for this Graph
            //IUriNode rdfType = g.CreateUriNode("rdf:type");
            IUriNode rdfType = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org/"));
       
            //This prints only the Triples we find with the predicate rdf:type using NTriples formatting
            show(g.Triples, form, "add");
            foreach (Triple t in g.GetTriplesWithSubject(rdfType))
            {
                form.rchbx.Text += (t.ToString(formatter)) + "\n";
            }


            //*********************
            //****Basic Usage****
            //*********************



            //*********************
            //****Basic Usage****
            //*********************
        } //endOf Writing

        public void WorkingWithGraphs(Form1 form)
        {
            Graph g = new Graph();
            TurtleParser ttlparser = new TurtleParser();
            ttlparser.Load(g, "HelloWorld.ttl");

            //*********************
            //********Nodes********
            //*********************
            //Assuming we have some Graph g find all the URI Nodes 
            foreach (IUriNode u in g.Nodes.UriNodes())
            {
                //Write the URI to the Console
                form.rchbx.Text = "#Nodes\n" + (u.Uri.ToString()) + "\n";
            }


            //*********************
            //***Selecting Nodes***
            //*********************
            form.rchbx.Text += "\n#Selecting Nodes\n";
            //Assuming we have some Graph g
            //Selecting a Blank Node
            IBlankNode b = g.GetBlankNode("myNodeID"); if (b != null)
            {
                form.rchbx.Text += ("Blank Node with ID " + b.InternalID + " exists in the Graph" + "\n");

            }
            else
            {
                form.rchbx.Text += ("No Blank Node with the given ID existed in the Graph" + "\n");

            }
            
            //Selecting Literal Nodes
            //Plain Literal with the given Value
            ILiteralNode l = g.GetLiteralNode("Some Text"); //Literal with the given Value and Language Specifier
            ILiteralNode l2 = g.GetLiteralNode("Some Text", "en"); //Literal with the given Value and DataType
            ILiteralNode l3 = g.GetLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            
            //Selecting URI Nodes
            //By URI
            IUriNode u1 = g.GetUriNode(new Uri("http://www.dotnetrdf.org"));
            form.rchbx.Text += (u1.ToString() + " exists in the Graph" + "\n");
            //By QName
            IUriNode u2 = g.GetUriNode("rdf:select");



            //*********************
            //**Selecting Triples**
            //*********************
            form.rchbx.Text += "\n\n#Selecting Triples\n";
            //Assuming we have some Graph g
            //Get all Triples involving a given Node
            IUriNode select = g.CreateUriNode(new Uri("http://www.dotnetrdf.org"));
            IEnumerable<Triple> ts = g.GetTriples(select);
            form.rchbx.Text += ("uri:\n");
            foreach (Triple t in ts)
            {
                form.rchbx.Text += (t.ToString()+ "\n");
 
            }

            //Get all Triples which meet some criteria
            //Want to find everything that is rdf:type ex:Person 
            IUriNode rdfType = g.CreateUriNode("rdf:type"); 
            IUriNode person = g.CreateUriNode("rdf:Person");
            ts = g.GetTriplesWithPredicateObject(rdfType, person);
           
            //Get all Triples with a given Subject 
            //We're reusing the node we created earlier 
            ts = g.GetTriplesWithSubject(select);
            form.rchbx.Text += ("subject:\n");
            foreach (Triple t in ts)
            {
                form.rchbx.Text += (t.ToString() + "\n");

            }
            
            //Get all the Triples with a given Predicate
            ts = g.GetTriplesWithPredicate(rdfType);
            
            //Get all the Triples with a given Object
            ts = g.GetTriplesWithObject(person);




            //*********************
            //***Loading Graphs****
            //*********************
            form.rchbx.Text += "\n\n#Loading Graphs\n";
            //Create our Storage Provider - this example uses Virtuoso Universal Server
            //VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "username", "password");
            ////Load the Graph into an ordinary graph instance first
            //Graph g = new Graph();
            //virtuoso.LoadGraph(g, new Uri("http://example.org/"));
            ////Then place the Graph into a wrapper
            //StoreGraphPersistenceWrapper wrapper = new StoreGraphPersistenceWrapper(virtuoso, g);
            ////Now make changes to this Graph as desired...
            ////Remember to call Dispose() to ensure changes get persisted when you are done
            //wrapper.Dispose();

            //First define a SPARQL Endpoint for DBPedia
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"));
            //Next define our query
            //We're going to ask DBPedia to describe the first thing it finds which is a Person
            String query = "DESCRIBE ?person WHERE {?person a <http://dbpedia.org/ontology/Person>} LIMIT 1";
            //Get the result 
            IGraph gg = endpoint.QueryWithResultGraph(query);
            form.rchbx.Text += gg.Triples.Last().ToString() + "\n";
        } //endOf WorkingWithGraphs

        public void WorkingWithTriples(Form1 form)
        {
            TripleStore tstore = new TripleStore();
            tstore.LoadFromFile("HelloWorld.ttl");
            foreach (Triple t in tstore.Triples)
            {
                form.rchbx.Text += t.ToString() + "\n";
            }


            if (tstore.HasGraph(new Uri("http://example.org/")))
            {
                form.rchbx.Text += ("Graph exists");
            }
            else
            {
                form.rchbx.Text += ("Graph doesn't exist");
            }







            //First create a Triple Store
            TripleStore store = new TripleStore();
            //Next load a Graph from a File and add it to the Store
            Graph g = new Graph();
            TurtleParser ttlparser = new TurtleParser();
            ttlparser.Load(g, "HelloWorld.ttl"); 
            store.Add(g);
            //Next load a Graph from a URI into the Store
            store.AddFromUri(new Uri("http://dbpedia.org/resource/Barack_Obama"));
            //Load another Graph in from the same file
            //This will cause an error since the Graph will have the same Base URI //and you can't insert duplicate Graphs in a Triple Store
            Graph h = new Graph();
            ttlparser.Load(h, "HelloWorld.ttl");
            try
            {
                store.Add(h);
            }
            catch
            {
                //We get an error
            }
            //You can avoid this by using the second optional boolean parameter to specify behaviour //when a Graph already exists
            //We try loading the same Graph again but we tell it to merge if it exists in the Store store.Add(h, true);
            //Try and load an empty Graph that has no Base URI
            //This Graph is then treated as being the default unnamed Graph of the store 

           
            //Graph i = new Graph();
            //store.Add(i);

            //foreach (Triple t in store.Triples)
            //{
            //    form.rchbx.Text += t.ToString() + "\n";
            //}

            store.Remove(new Uri("http://dbpedia.org/resource/Barack_Obama"));

            foreach (Triple t in store.Triples)
            {
                form.rchbx.Text += t.ToString() + "\n";
            }





            //Execute a raw SPARQL Query
            //Should get a SparqlResultSet back from a SELECT query
            form.rchbx.Text += "\n\n****\n\n";
            Object results = store.ExecuteQuery("SELECT * WHERE {?s ?p ?o}"); 
            if (results is SparqlResultSet)
            {
                //Print out the Results
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult result in rset)
                {
                    form.rchbx.Text += (result.ToString()) + "\n";
                }
            }
            //Use the SparqlQueryParser to give us a SparqlQuery object
            //Should get an IGraph back from a CONSTRUCT query
            SparqlQueryParser sparqlparser = new SparqlQueryParser();
            SparqlQuery query = sparqlparser.ParseFromString("CONSTRUCT { ?s ?p ?o } WHERE {?s ?p ?o}");
            results = store.ExecuteQuery(query);
            if (results is IGraph) {
                //Print out the Results
                IGraph ig = (IGraph)results; 
                foreach (Triple t in g.Triples) {
                    form.rchbx.Text += "\n\n" + (t.ToString());
                    form.rchbx.Text += "\n" + ("Query took " + query.QueryExecutionTime + " milliseconds");
                }
            }







            //            //Create a connection to 4store in this example
            //FourStoreConnector 4store = new FourStoreConnector("http://example.com:8080/"); PersistentTripleStore store = new PersistentTripleStore(4store);
            ////See whether a Graph exists in the store
            ////If the Graph exists in the underlying store this will cause it to be loaded into memory if (store.HasGraph(new Uri("http://example.org/someGraph")))
            //{
            //}
            ////Get the graph out of the in-memory view (note that if it changes in the underlying store in the meantime you will not see those changes)
            //Graph g = store.Graph(new Uri("http://example.org/someGraph")); //Do something with the Graph...
            ////If you were to add a Graph to the store this would be added to the in-memory state only initially
            //Graph toAdd = new Graph();
            //toAdd.LoadFromUri(new Uri("http://example.org/newGraph")); store.Add(toAdd);
            ////To ensure that the new graph is saved call Flush() store.Flush();
            ////You can also use this class to make queries/updates against the underlying store
            ////Note - If you've made changes to the in-memory state of the store making a query/update will throw an error unless you've
            //// persisted those changes
            //// Use Flush() or Discard() to ensure the state of the store is consistent for querying
            ////Make a Query against the Store
            ////Should get a SparqlResultSet back from a SELECT query
            //Object results = store.ExecuteQuery("SELECT * WHERE {?s ?p ?o}");
            //if (results is SparqlResultSet)
            //{
            //    //Print out the Results
            //    SparqlResultSet rset = (SparqlResultSet)results; foreach (SparqlResult result in rset)
            //    {
            //    }
            //}








            //Read a Store from the TriG file
            store = new TripleStore();
            TriGParser trigparser = new TriGParser(); 
            trigparser.Load(store, "input.trig");
            //Now we want to save to another TriG file
            TriGWriter trigwriter = new TriGWriter(); 
            trigwriter.Save(store, "output.trig");








        } //endOf WorkingWithTriples

        public void QueryWithSPARQL(Form1 form)
        {
            ////Enumerating via the Results property
            //foreach (SparqlResult result in rset.Results)
            //{
            //    Console.WriteLine(result.ToString()); //Enumerating directly
            //}
            //foreach (SparqlResult result in rset)
            //{
            //    Console.WriteLine(result.ToString());
            //}




            ////Assuming our result row is in a variable r //With Named Indexing
            //INode value = r["var"]; //With Indexing
            //INode value = r[0];
            ////With method
            //INode value = r.Value("var");




            TripleStore store = new TripleStore();
            store.LoadFromFile("HelloWorld.ttl");
            //Assume that we fill our Store with data from somewhere
            //Execute a raw SPARQL Query
            //Should get a SparqlResultSet back from a SELECT query
            Object results = store.ExecuteQuery("SELECT * WHERE {?s ?p ?o}"); if (results is SparqlResultSet)
            {
                //Print out the Results
                SparqlResultSet rset = (SparqlResultSet)results; foreach (SparqlResult result in rset)
                {
                    form.rchbx.Text += (result.ToString()) + "\n";
                }
            }
            //Use the SparqlQueryParser to give us a SparqlQuery object
            //Should get a Graph back from a CONSTRUCT query
            SparqlQueryParser sparqlparser = new SparqlQueryParser();
            SparqlQuery query = sparqlparser.ParseFromString("CONSTRUCT { ?s ?p ?o } WHERE {?s ?p ?o}"); results = store.ExecuteQuery(query);
            if (results is IGraph)
            {
                //Print out the Results
                IGraph g = (IGraph)results; foreach (Triple t in g.Triples)
                {
                    form.rchbx.Text += (t.ToString()) + "\n";
                    form.rchbx.Text += ("Query took " + query.QueryExecutionTime.ToString()) + "\n";
                }
            }







            //Define a remote endpoint
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
            //Make a SELECT query against the Endpoint
            SparqlResultSet resultset = endpoint.QueryWithResultSet("SELECT DISTINCT ?Concept WHERE {[] a ?Concept}"); 
            foreach (SparqlResult result in resultset)
            {
                Console.WriteLine(result.ToString());
            }
            //Make a DESCRIBE query against the Endpoint
            IGraph ig = endpoint.QueryWithResultGraph("DESCRIBE "); 
            foreach (Triple t in ig.Triples)
            {
                Console.WriteLine(t.ToString());
            }








//            //Get the Query processor
//ISparqlQueryProcessor processor = new LeviathanQueryProcessor(store);
////Use the SparqlQueryParser to give us a SparqlQuery object
////Should get a Graph back from a CONSTRUCT query
//SparqlQueryParser sparqlparser = new SparqlQueryParser();
//SparqlQuery query = sparqlparser.ParseFromString("CONSTRUCT { ?s ?p ?o } WHERE {?s ?p ?o}"); results = processor.ProcessQuery(query);
//if (results is IGraph)
//    //Print out the Results
//IGraph g = (IGraph)results;
//NTriplesFormatter formatter = new NTriplesFormatter(); foreach (Triple t in g.Triples)
//{
//Console.WriteLine(t.ToString(formatter)); Console.WriteLine("Query took " + query.QueryTime + " milliseconds");
//}
//}
        } //end of QueryWithSPARQL

        public void r2rml(Form1 form)
        {
            Graph g = new Graph();
            //MyTurtleParser turtleParser = new MyTurtleParser();
            //turtleParser.TraceParsing = true;
            //turtleParser.Load(g, "r2rml.ttl");
            //form.rchbx.Text += "*" + turtleParser.ToString() + "\n";
            //show(g.Triples, form, "add");
            R2RMLParser r2rmlParser = new R2RMLParser();
            r2rmlParser.TraceParsing = true;
            r2rmlParser.Load(g, "r2rml.ttl");
            form.rchbx.Text += "*" + r2rmlParser.ToString() + "\n";
            show(g.Triples, form, "add");

            //String R2RML_URI = "http://www.w3.org/ns/r2rml";
            //String morphNS = "http://es.upm.fi.dia.oeg/morph#";	
            //String r2rml = "rr";
            //String morph = "morph";
	      

            //String queryString = "PREFIX " + r2rml + ": <" + g.BaseUri + "> \n" +
            //        "PREFIX " + morph + ": <" + morphNS + "> \n" +
            //        "SELECT ?tMap ?query ?table ?subjCol ?subjColOp ?subjType ?subject ?subjClass " +
            //        "?subjInverse ?subjTemplate WHERE { \n" +
            //        tMapVar + " a <" + TriplesMap.getURI() + "> ; \n" +
            //        r2rml + ":" + sqlQuery.getLocalName() + " ?query ; \n" +
            //        r2rml + ":" + subjectMap.getLocalName() + " ?subjMap . \n" +
            //        "OPTIONAL { " + tMapVar + " " + r2rml + ":" + tableName.getLocalName() + " ?table . } \n" +
            //        "OPTIONAL { ?subjMap " + r2rml + ":" + column.getLocalName() + " ?subjCol . } \n" +
            //        "OPTIONAL { ?subjMap " + morph + ":" + morphColumnOperation.getLocalName() + " ?subjColOp . } \n" +
            //        "OPTIONAL { ?subjMap " + r2rml + ":" + template.getLocalName() + " ?subjTemplate . } \n" +
            //        "OPTIONAL { ?subjMap " + r2rml + ":" + termType.getLocalName() + " ?subjType . } \n" +
            //        "OPTIONAL { ?subjMap " + r2rml + ":" + subject.getLocalName() + " ?subject . } \n" +
            //        "OPTIONAL { ?subjMap " + r2rml + ":" + classProperty.getLocalName() + " ?subjClass . } \n" +
            //    //"OPTIONAL { ?subjMap "+r2rml+":"+rrGraph.getLocalName() + " ?subjGraph . } \n"+				
            //    //"OPTIONAL { ?subjMap "+r2rml+":"+rrGraphColumn.getLocalName() + " ?subjGraphCol . } \n"+				
            //        "OPTIONAL { ?subjMap " + r2rml + ":" + inverseExpression.getLocalName() + " ?subjInverse . } " +
            //        "}";

        } //endOf r2rml

    } //endOf ClassTest
} //endOf namespace
