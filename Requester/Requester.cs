using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RequesterLib
{
    public class Requester
    {
        private const string DefaultMediaType = "application/x-www-form-urlencoded";

        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly CookieContainer _cookieContainer = new();

        private Requester()
        {
            var httpClientHandler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer
            };

            _httpClientHandler = httpClientHandler;
            _httpClient = new HttpClient(httpClientHandler);
        }

        public void AddDefaultHeader(string name, string value) => _httpClient.DefaultRequestHeaders.Add(name, value);

        public void AddCookie(Cookie cookie) => _cookieContainer.Add(cookie);

        public void AddCookie(string domain, string name, string value, string path = "/") =>
            AddCookie(new Cookie(name, value, path, domain));

        public void AddCookies(params (string, string, string)[] pairs) =>
            pairs.ToList().ForEach(pair => AddCookie(pair.Item1, pair.Item2, pair.Item3));

        public void UpdateCookie(string domain, string name, string value)
        {
            foreach (Cookie cookie in _cookieContainer.GetCookies(new Uri(domain)))
                if (cookie.Name.Equals(name))
                    cookie.Value = value;
        }

        public bool HasCookie(string domain, string name)
        {
            foreach (Cookie cookie in _cookieContainer.GetCookies(new Uri(domain)))
                if (cookie.Name.Equals(name))
                    return true;

            return false;
        }

        public void ExpireCookie(string domain, string name)
        {
            foreach (Cookie cookie in _cookieContainer.GetCookies(new Uri(domain)))
                if (cookie.Name.Equals(name))
                    cookie.Expired = true;
        }

        public void ExpireCookies(string domain, params string[] names)
        {
            var nameSet = new HashSet<string>(names);
            foreach (Cookie cookie in _cookieContainer.GetCookies(new Uri(domain)))
                if (nameSet.Contains(cookie.Name))
                    cookie.Expired = true;
        }

        public void ExpireCookies(string domain, Predicate<Cookie> shouldExpire)
        {
            foreach (Cookie cookie in _cookieContainer.GetCookies(new Uri(domain)))
                if (shouldExpire.Invoke(cookie))
                    cookie.Expired = true;
        }

        public void SetProxy(IWebProxy proxy) => _httpClientHandler.Proxy = proxy;

        public void SetProxy(string host, int port) => _httpClientHandler.Proxy = new WebProxy(host, port);

        public IWebProxy GetProxy() => _httpClientHandler.Proxy;

        public async Task<HttpResponseMessage> GetAsync(string url) => await _httpClient.GetAsync(url);

        public async Task<string> GetStringAsync(string url) => await _httpClient.GetStringAsync(url);

        public async Task<JsonElement> GetJsonElementAsync(string url) =>
            (await JsonDocument.ParseAsync(await _httpClient.GetStreamAsync(url))).RootElement;

        public async Task<T> GetFromJsonAsync<T>(string url) => await _httpClient.GetFromJsonAsync<T>(url);

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content) =>
            await _httpClient.PostAsync(url, content);

        public async Task<HttpResponseMessage> PostStringContentAsync(string url, string content,
            Encoding encoding = default,
            string mediaType = DefaultMediaType) =>
            await _httpClient.PostAsync(url, new StringContent(content, encoding, mediaType));

        public async Task<JsonElement> PostStringContentAndGetJsonElementAsync(string url, string content,
            JsonSerializerOptions serializerOptions = null, CancellationToken cancellationToken = default,
            Encoding encoding = default, string mediaType = DefaultMediaType)
        {
            var responseMessage = await _httpClient.PostAsync(url, new StringContent(content, encoding, mediaType),
                cancellationToken);
            return (await JsonDocument.ParseAsync(await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken)).RootElement;
        }

        public async Task<T> PostStringContentAndGetFromJsonAsync<T>(string url, string content,
            JsonSerializerOptions serializerOptions = null, CancellationToken cancellationToken = default,
            Encoding encoding = default, string mediaType = DefaultMediaType)
        {
            var responseMessage = await _httpClient.PostAsync(url, new StringContent(content, encoding, mediaType),
                cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(
                await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T obj,
            JsonSerializerOptions serializerOptions = null, CancellationToken cancellationToken = default) =>
            await _httpClient.PostAsJsonAsync(url, obj, serializerOptions, cancellationToken);

        public async Task<JsonElement> PostAsJsonAndGetJsonElementAsync<T>(string url, T obj,
            JsonSerializerOptions serializerOptions = null, CancellationToken cancellationToken = default)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync(url, obj, serializerOptions, cancellationToken);

            return (await JsonDocument.ParseAsync(await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken)).RootElement;
        }

        public async Task<R> PostAsJsonAndGetFromJsonAsync<T, R>(string url, T obj,
            JsonSerializerOptions serializerOptions = null, CancellationToken cancellationToken = default)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync(url, obj, serializerOptions, cancellationToken);
            return await JsonSerializer.DeserializeAsync<R>(
                await responseMessage.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage) =>
            await _httpClient.SendAsync(requestMessage);

        public CookieContainer GetCookieContainer() => _cookieContainer;

        public class Builder
        {
            private readonly Requester _requester;

            public Builder()
            {
                _requester = new Requester();
            }

            public Builder WithDefaultHeader(string name, string value)
            {
                _requester.AddDefaultHeader(name, value);
                return this;
            }

            public Builder WithCookie(Cookie cookie)
            {
                _requester.AddCookie(cookie);
                return this;
            }

            public Builder WithCookie(string domain, string name, string value, string path = "/")
            {
                _requester.AddCookie(domain, name, value, path);
                return this;
            }

            public Builder WithCookies(params (string, string, string)[] pairs)
            {
                _requester.AddCookies(pairs);
                return this;
            }

            public Builder WithProxy(IWebProxy proxy)
            {
                _requester.SetProxy(proxy);
                return this;
            }

            public Builder WithProxy(string host, int port)
            {
                _requester.SetProxy(host, port);
                return this;
            }

            public Requester Build() => _requester;
        }
    }
}