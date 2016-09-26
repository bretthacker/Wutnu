using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Wutnu.Infrastructure
{
    public class ShortUrlConstraint : IRouteConstraint
    {

        private readonly int _maxLength;
        private string _parameterValue;

        public ShortUrlConstraint(int maxLength, string value)
        {
            _maxLength = maxLength;
            _parameterValue = value;
        }

        public bool Match
            (
                HttpContextBase httpContext, 
                Route route, 
                string parameterName, 
                RouteValueDictionary values, 
                RouteDirection routeDirection
            )
        {
            object value;
            if (values.TryGetValue(parameterName, out value) && value != null)
            {
                if (value.ToString() == _parameterValue)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
