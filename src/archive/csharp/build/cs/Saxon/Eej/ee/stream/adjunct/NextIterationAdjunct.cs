using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class NextIterationAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => new Saxon.Eej.ee.stream.feed.NextIterationFeed(getExpression(), @out, context, arg));
        }
    
    }
}