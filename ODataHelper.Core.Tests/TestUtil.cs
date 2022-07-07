namespace StringToExpression.Test
{
    public static class TestUtil
    {
        public static string Highlight(this StringSegment segment, string startHighlight = "[", string endHighlight = "]")
        {
            return segment.SourceString
                .Insert(segment.End, endHighlight)
                .Insert(segment.Start, startHighlight);
        }
    }
}
