using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tura.RabbitMq
{
    public interface IHttpContextHelper
    {
        string GetAuthorizationHeaders();
    }
    public class HttpContextHelper : IHttpContextHelper
    {
        public Dictionary<string, StringValues> GetParameterValues;

        public IHttpContextAccessor _httpContextAccessor;

        public HttpContextHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    
        public string GetAuthorizationHeaders()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            string authorizationString = "";

            StringValues stringValue;
            if (request.Headers.TryGetValue("Authorization", out stringValue))
                authorizationString = stringValue;

            return authorizationString;

        }
    }
}
