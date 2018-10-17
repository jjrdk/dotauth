namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using SimpleIdentityServer.Core.Common.Models;

    internal class RepresentationComparer : IComparer<Representation>
    {
        private readonly Filter _filter;

        public RepresentationComparer(Filter filter)
        {
            _filter = filter;
        }

        public int Compare(Representation x, Representation y)
        {
            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var xAttrs = _filter.Evaluate(x);
            var yAttrs = _filter.Evaluate(y);
            if (xAttrs == null || !xAttrs.Any())
            {
                return -1;
            }

            if (yAttrs == null || !yAttrs.Any())
            {
                return 1;
            }

            var xAttr = xAttrs.First();
            var yAttr = yAttrs.First();
            return xAttr.CompareTo(yAttr);
        }
    }
}