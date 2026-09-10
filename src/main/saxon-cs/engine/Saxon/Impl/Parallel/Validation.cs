using System.Collections.Generic;

using Saxon.Ejavax.xml.transform;
using Saxon.Hej.lib;
using Saxon.Api;
using Saxon.Eej.ee.s9api;
using Saxon.Eej.ee.validate;
using Processor = Saxon.Hej.s9api.Processor;
using XdmDestination = Saxon.Hej.s9api.XdmDestination;

namespace Saxon.Impl.Parallel {
    public class Validation {
        internal static void ValidateMultiple(Processor processor, IEnumerable<Source> sources,
                    InvalidityHandler invalidityHandler, SchemaValidatorImpl.ReportList reportDocs) {
            System.Threading.Tasks.Parallel.ForEach(sources, source => {
            //foreach (Source source in sources) {
                Saxon.Hej.s9api.SchemaValidator validator = processor.getSchemaManager().newSchemaValidator();
                InvalidityHandler localHandler = invalidityHandler;
                if (invalidityHandler is InvalidityReportGenerator) {
                    InvalidityReportGeneratorEE singleReporter = (InvalidityReportGeneratorEE)processor.getUnderlyingConfiguration().createValidityReporter();
                    XdmDestination dest = new XdmDestination();
                    singleReporter.setDestination(dest);
                    validator.setInvalidityHandler(singleReporter);
                    localHandler = singleReporter;
                }
                else {
                    validator.setInvalidityHandler(invalidityHandler);
                }
                //localHandler.StartReporting(source.getSystemId());
                try {
                    validator.validate(source);
                }
                catch (SaxonApiException e) {
                    // ignore validation failure
                }

                //localHandler.EndReporting();
                if (localHandler is InvalidityReportGeneratorEE handler) {
                    reportDocs.add(((XdmDestination)handler.getDestination()).getXdmNode());
                }
            });
        }
    }
}

// Copyright (c) 2021 Saxonica Limited