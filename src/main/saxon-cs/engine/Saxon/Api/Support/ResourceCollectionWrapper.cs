using System.Collections;
using System.Collections.Generic;
using Saxon.Hej.expr;

namespace Saxon.Api.Support {
    public class ResourceCollectionWrapper : IEnumerable<IResource> {

        private readonly Saxon.Hej.lib.ResourceCollection resources;
        private readonly Saxon.Hej.expr.XPathContext context;

        public ResourceCollectionWrapper(Saxon.Hej.lib.ResourceCollection resources, Saxon.Hej.expr.XPathContext context) {
            this.resources = resources;
            this.context = context;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<IResource> GetEnumerator() {
            var collection = resources.getResources(context);
            while (collection.MoveNext()) {
                yield return new ResourceWrapper(collection.Current);
            }
        }

    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited