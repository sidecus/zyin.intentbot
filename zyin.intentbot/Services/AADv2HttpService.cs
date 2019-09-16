namespace Zyin.IntentBot.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Base class for Bearer token based RESTful services using AADv2 tokens.
    /// </summary>
    public abstract class AADv2HttpService
    {
        /// <summary>
        /// HttpClient reference
        /// </summary>
        protected readonly HttpClient httpClient;

        /// <summary>
        /// logger object
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AADv2HttpService" /> class
        /// </summary>
        /// <param name="httpClient">Injected HttpClient for the required api</param>
        /// <param name="logger">logger for this class</param>
        public AADv2HttpService(HttpClient httpClient, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        /// <summary>
        /// user token to the target service
        /// </summary>
        public string AppToken { get; private set; }

        /// <summary>
        /// Initialize OBO token based on the source token
        /// </summary>
        /// <param name="appToken">source token to our own resource. It's used as user assertion to get target resource token</param>
        /// <returns>task</returns>
        public void InitializeTokenAsync(string appToken)
        {
            this.AppToken = appToken ?? throw new ArgumentNullException(nameof(appToken));
        }

        /// <summary>
        /// Make a HTTP get call
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="Func<HttpResponseMessage"></param>
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <typeparam name="T">api return type</typeparam>
        /// <returns>task of T</returns>
        protected async Task<T> GetHttpAsync<T>(
            string uri,
            Func<HttpResponseMessage, Task<T>> successCallback,
            Func<HttpResponseMessage, Task<T>> errorCallback = null)
        {
            return await this.MakeHttpCallAsync(
                HttpMethod.Get,
                uri,
                null,
                successCallback,
                errorCallback);
        }

        /// <summary>
        /// Make a HTTP post call
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="Func<HttpResponseMessage"></param>
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <typeparam name="T">api return type</typeparam>
        /// <returns>task of T</returns>
        protected async Task<T> PostHttpAsync<T>(
            string uri,
            HttpContent content,
            Func<HttpResponseMessage, Task<T>> successCallback,
            Func<HttpResponseMessage, Task<T>> errorCallback = null)
        {
            return await this.MakeHttpCallAsync(
                HttpMethod.Post,
                uri,
                content,
                successCallback,
                errorCallback);
        }

        /// <summary>
        /// Make a HTTP PUT call
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="Func<HttpResponseMessage"></param>
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <typeparam name="T">api return type</typeparam>
        /// <returns>task of T</returns>
        protected async Task<T> PutHttpAsync<T>(
            string uri,
            HttpContent content,
            Func<HttpResponseMessage, Task<T>> successCallback,
            Func<HttpResponseMessage, Task<T>> errorCallback = null)
        {
            return await this.MakeHttpCallAsync(
                HttpMethod.Put,
                uri,
                content,
                successCallback,
                errorCallback);
        }

        /// <summary>
        /// Make an api call
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <typeparam name="T">api return type</typeparam>
        /// <returns>task of T</returns>
        protected async Task<T> MakeHttpCallAsync<T>(
            HttpMethod method,
            string uri,
            HttpContent content,
            Func<HttpResponseMessage, Task<T>> successCallback,
            Func<HttpResponseMessage, Task<T>> errorCallback = null)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (successCallback == null)
            {
                throw new ArgumentNullException(nameof(successCallback));
            }

            if (string.IsNullOrWhiteSpace(this.AppToken))
            {
                throw new InvalidOperationException("user token has not been set yet");
            }

            // if error callback is not provided, use the default one.
            errorCallback = errorCallback ?? (response => this.ProcessErrorDefault<T>(response));
            
            // Make the call
            using (var request = this.CreateHttpRequestMessage(method, uri, content))
            {
                using (var response = await this.httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await successCallback(response);
                    }
                    else
                    {
                        this.logger.LogError($"Error calling api {uri} for {this.GetType().Name}. HttpStatusCode: {response.StatusCode}");
                        return await errorCallback(response);
                    }
                }
            }
        }

        /// <summary>
        /// Create a http request message with proper authorization header to talk with the given service
        /// </summary>
        /// <param name="method">http method</param>
        /// <param name="uri">request relative uri</param>
        /// <param name="content">http content if any</param>
        /// <returns>http request message</returns>
        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uri, HttpContent content = null)
        {
            if (method == HttpMethod.Get && content != null)
            {
                throw new InvalidOperationException("Http Get but there is content being sent");
            }

            // Create request and set authorization header on it
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.AppToken);
            request.Content = content;
            return request;
        }

        /// <summary>
        /// Default error handler - always throws.
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Task<T> ProcessErrorDefault<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Special handling for 403
                throw new ApplicationException("You don't have permission to access the resource.");
            }
            else
            {
                // This will throw. The return is just for compilation
                response.EnsureSuccessStatusCode();
                return Task.FromResult(default(T));
            }
        }
    }
}
