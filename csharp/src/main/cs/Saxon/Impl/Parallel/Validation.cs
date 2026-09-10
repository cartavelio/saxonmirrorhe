using System.Collections.Generic;
using System.Threading.Tasks;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.lib;
using Saxon.Api;
using Saxon.Eej.ee.s9api;
using Saxon.Eej.ee.validate;
using Processor = Saxon.Hej.s9api.Processor;
using XdmDestination = Saxon.Hej.s9api.XdmDestination;

namespace Saxon.Impl.Parallel {
    public class Validation {
        internal static void ValidateMultiple(
                Saxon.Hej.s9api.Processor processor, 
                Saxon.Hej.s9api.XsdSchema schema, 
                IEnumerable<Source> sources,
                InvalidityHandler invalidityHandler, 
                SchemaValidatorImpl.ReportList reportDocs,
                int threads) {
            ParallelOptions options = new() {
                MaxDegreeOfParallelism = threads
            };
            System.Threading.Tasks.Parallel.ForEach(sources, options, source => {
                Saxon.Hej.s9api.SchemaValidator validator = schema.newValidator();
                InvalidityHandler localHandler = invalidityHandler;
                if (invalidityHandler is InvalidityReportGenerator) {
                    InvalidityReportGeneratorEE singleReporter = 
                        (InvalidityReportGeneratorEE)processor.getUnderlyingConfiguration().createValidityReporter();
                    XdmDestination dest = new XdmDestination();
                    singleReporter.setDestination(dest);
                    validator.setInvalidityHandler(singleReporter);
                    localHandler = singleReporter;
                }
                else {
                    validator.setInvalidityHandler(invalidityHandler);
                }
                try {
                    validator.validate(source);
                }
                catch (SaxonApiException e) {
                    // ignore validation failure
                }

                if (localHandler is InvalidityReportGeneratorEE handler) {
                    reportDocs.add(((XdmDestination)handler.getDestination()).getXdmNode());
                }
            });
        }
    }
}

// Copyright (c) 2021 - 2026 Saxonica Limited