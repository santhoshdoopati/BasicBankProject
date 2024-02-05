using Newtonsoft.Json;
using RestSharp;

namespace BasicBankProject.Modal
{
    public class RestHelpers
    {
        public RestHelpers() { 
        //Placeholder for Get token
        }
       
        public RestResponse MakeAPICall(string Url, string endpoint, Method method, object body, string token = "")
        {
            var client = new RestClient(Url);
            var request = new RestRequest(endpoint, method);
            if (token != "")
            {
                request.AddHeader("token", token);
            }
            if(body != null)
            {
                request.AddBody(body);
            }
            return client.Execute(request);
        }

        public T DeserializeResponse<T>(RestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public string SerializeRequest(object requestBody)
        {
            return JsonConvert.SerializeObject(requestBody);
        }

    }
}
