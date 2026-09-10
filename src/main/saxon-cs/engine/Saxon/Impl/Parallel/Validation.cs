using System.Collections.Generic;
using com.saxonica.ee.s9api;
using com.saxonica.ee.validate;
using javax.xml.transform;
using net.sf.saxon.lib;
using Saxon.Api;
using Processor = net.sf.saxon.s9api.Processor;
using XdmDestination = net.sf.saxon.s9api.XdmDestination;

namespace Saxon.Impl.Parallel {
    public class Validation {
        public static void ValidateMultiple(Processor processor, IEnumerable<Source> sources,
                    InvalidityHandler invalidityHandler, SchemaValidatorImpl.ReportList reportDocs) {
            System.Threading.Tasks.Parallel.ForEach(sources, source => {
            //foreach (Source source in sources) {
                net.sf.saxon.s9api.SchemaValidator validator = processor.getSchemaManager().newSchemaValidator();
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
                //localHandler.startReporting(source.getSystemId());
                try {
                    validator.validate(source);
                }
                catch (SaxonApiException e) {
                    // ignore validation failure
                }

                //localHandler.endReporting();
                if (localHandler is InvalidityReportGeneratorEE handler) {
                    reportDocs.add(((XdmDestination)handler.getDestination()).getXdmNode());
                }
            });
        }
    }
}