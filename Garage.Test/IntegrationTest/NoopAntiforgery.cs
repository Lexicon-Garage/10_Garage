using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Garage.Test.IntegrationTest
{
    public class NoopAntiforgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet(
                requestToken: "noop-token",
                cookieToken: "noop-cookie",
                formFieldName: "__RequestVerificationToken",
                headerName: "X-CSRF-TOKEN"
            );
        }

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet(
                requestToken: "noop-token",
                cookieToken: "noop-cookie",
                formFieldName: "__RequestVerificationToken",
                headerName: "X-CSRF-TOKEN"
            );
        }

        public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public void SetCookieTokenAndHeader(HttpContext httpContext)
        {
        }


        public Task ValidateRequestAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }
    }
}
