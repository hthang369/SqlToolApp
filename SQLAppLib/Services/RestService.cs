using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SQLAppLib.Services
{
    public class RestService : IDisposable
    {
        private HttpClient client;
        private const int MAX_RESPONSE_CONTENT_BUFFER_SIZE = 0x3e800;
        private string Token { get; set; }
        public string Url { private get; set; }
        public string Body { private get; set; }
        public string BodyType { private get; set; }
        public string Method { private get; set; }
        public double Timeout { private get; set; }
        public string ResponseType { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public RestService()
        {
            this.BodyType = "application/json";
            this.Timeout = 30.0;
            this.ResponseType = "json";
            this.client = new HttpClient();
            this.client.MaxResponseContentBufferSize = (long)0x3e800L;
        }

        public RestService(string url, string method, string body = "", string bodyType = "application/json", string responseType = "json")
        {
            this.Url = url;
            this.Body = body;
            this.BodyType = bodyType;
            this.Method = method;
            this.ResponseType = responseType;
            this.client = new HttpClient();
            this.client.MaxResponseContentBufferSize = (long)0x3e800L;
        }

        public RestService AddHeader(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                this.client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }
            return this;
        }

        public void Dispose()
        {
            if (this.client != null)
            {
                this.client.Dispose();
            }
        }

        private HttpResponseMessage get()
        {
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(this.Timeout));
            return this.client.GetAsync(this.Url, source.Token).Result;
        }

        private HttpResponseMessage post()
        {
            StringContent content = new StringContent(this.Body, Encoding.UTF8, this.BodyType);
            return this.client.PostAsync(this.Url, (HttpContent)content, new CancellationTokenSource(TimeSpan.FromSeconds(this.Timeout)).Token).Result;
        }

        private HttpResponseMessage put()
        {
            StringContent content = new StringContent(this.Body, Encoding.UTF8, this.BodyType);
            return this.client.PutAsync(this.Url, (HttpContent)content, new CancellationTokenSource(TimeSpan.FromSeconds(this.Timeout)).Token).Result;
        }

        private HttpResponseMessage delete()
        {
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(this.Timeout));
            return this.client.DeleteAsync(this.Url, source.Token).Result;
        }

        public string GetToken()
        {
            return this.client.DefaultRequestHeaders.Authorization.ToString();
        }

        private string processResponse(HttpResponseMessage data)
        {
            this.StatusCode = data.StatusCode;
            return data.Content.ReadAsStringAsync().Result;
        }

        private T processResponse<T>(HttpResponseMessage data) where T : new()
        {
            this.StatusCode = data.StatusCode;
            string result = data.Content.ReadAsStringAsync().Result;
            if ((this.ResponseType != "json") && (this.ResponseType != ""))
            {
                if (this.ResponseType != "xml")
                {
                    return default(T);
                }
                XmlDocument document1 = new XmlDocument();
                document1.LoadXml(result);
                JsonConvert.SerializeXmlNode((XmlNode)document1);
            }
            return JsonConvert.DeserializeObject<T>(result);
        }

        public Task<DataResult<string>> Run()
        {
            return Task.Run<DataResult<string>>(delegate {
                DataResult<string> result = new DataResult<string>();
                try
                {
                    this.Method = this.Method.ToLower();
                    MethodInfo method = base.GetType().GetMethod(this.Method, ((BindingFlags)BindingFlags.NonPublic) | (((BindingFlags)BindingFlags.Public) | ((BindingFlags)BindingFlags.Instance)));
                    HttpResponseMessage data = new HttpResponseMessage();
                    if (method == null)
                    {
                        result.Target = "";
                    }
                    else
                    {
                        data = (HttpResponseMessage)method.Invoke(this, (object[])null);
                        result.Target = this.processResponse(data);
                    }
                }
                catch (Exception exception1)
                {
                    result.AddError(exception1);
                }
                return result;
            });
        }

        public Task<DataResult<T>> Run<T>() where T : new()
        {
            return Task.Run<DataResult<T>>(delegate {
                DataResult<T> result = new DataResult<T>();
                try
                {
                    this.Method = this.Method.ToLower();
                    MethodInfo method = base.GetType().GetMethod(this.Method, ((BindingFlags)BindingFlags.NonPublic) | (((BindingFlags)BindingFlags.Public) | ((BindingFlags)BindingFlags.Instance)));
                    HttpResponseMessage data = new HttpResponseMessage();
                    if (method != null)
                    {
                        data = (HttpResponseMessage)method.Invoke(this, (object[])null);
                        result.Target = this.processResponse<T>(data);
                    }
                    else
                    {
                        T local = default(T);
                        result.Target = local;
                    }
                }
                catch (Exception exception1)
                {
                    result.AddError(exception1);
                }
                return result;
            });
        }

        public RestService SetToken(Scheme scheme, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme.ToString(), token);
            }
            return this;
        }

        public enum Scheme
        {
            Basic,
            Oauth,
            Bearer
        }
    }
}
