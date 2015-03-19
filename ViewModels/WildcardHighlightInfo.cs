using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels
{
    public class WildcardHighlightInfo : HighlightInfo
    {

        private List<HighlightInfo> _PartHighlights = new List<HighlightInfo>();

        public void AddPartMatch(HighlightInfo match)
        {
            if (match == null)
                return;

            if (!_PartHighlights.Any(cur => cur.StartIndex == match.StartIndex && cur.EndIndex == match.EndIndex))
                _PartHighlights.Add(match);
        }

        public IEnumerable<HighlightInfo> PartHighlights
        {
            get
            {
                return _PartHighlights.ToList();
            }
        }
    }
}
