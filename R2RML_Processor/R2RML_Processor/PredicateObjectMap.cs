using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R2RML_Processor
{
    class PredicateObjectMap 
    {
        public List<Pair<PredicateMap, ObjectMap>> predicate_objectPairs;
        public List<Pair<PredicateMap, RefObjectMap>> predicate_refObjectPairs;
        GraphMap graphMap;

        public void AddPair(PredicateMap predicateMap, ObjectMap objectMap)
        {
            predicate_objectPairs.Add(new Pair<PredicateMap, ObjectMap>(predicateMap, objectMap));
        }
        public void AddPair(PredicateMap predicateMap, RefObjectMap refObjectMap)
        {
            predicate_refObjectPairs.Add(new Pair<PredicateMap, RefObjectMap>(predicateMap, refObjectMap));
        }

    }

    public class Pair<F, S>
    {
        public Pair()
        {
        }

        public Pair(F first, S second)
        {
            this.First = first;
            this.Second = second;
        }

        public F First { get; set; }
        public S Second { get; set; }
    };
}
