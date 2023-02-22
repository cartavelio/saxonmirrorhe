using System;

namespace Saxon.Api {
    internal class JPredicateImpl : java.util.function.Predicate
    {

        System.Predicate<QName> predicate;

        public JPredicateImpl(System.Predicate<QName> p) {
            predicate = p;

        }

        public Predicate and(Predicate value1, Predicate value2)
        {
            throw new NotImplementedException();
        }

        public Predicate negate(Predicate value)
        {
            throw new NotImplementedException();
        }

        public Predicate or(Predicate value1, Predicate value2)
        {
            throw new NotImplementedException();
        }

        public Predicate and(Predicate other)
        {
            throw new NotImplementedException();
        }

        public Predicate isEqual(object targetRef)
        {
            throw new NotImplementedException();
        }

        public Predicate negate()
        {
            throw new NotImplementedException();
        }

        public Predicate or(Predicate other)
        {
            throw new NotImplementedException();
        }

        public bool test(object t)
        {
            return predicate.Invoke(new QName((net.sf.saxon.s9api.QName)t));
        }
    }
}