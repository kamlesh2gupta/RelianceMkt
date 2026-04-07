using Newtonsoft.Json;
using RelianceMkt.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class ApiHelper
{
    private static readonly HttpClient _client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI(string name, string mobile, string sapcode, string SapName)
    {
        var url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture" +
                  "?accessKey=u$re5470b019cbada9931f9d41d27261f60" +
                  "&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        var jsonBody = $@"
{{
  ""LeadDetails"": [
    {{ ""Attribute"": ""SearchBy"", ""Value"": ""Phone"" }},
    {{ ""Attribute"": ""__UseUserDefinedGuid__"", ""Value"": ""true"" }},
    {{ ""Attribute"": ""Source"", ""Value"": ""DigiMyin"" }},
    {{ ""Attribute"": ""FirstName"", ""Value"": ""{name}"" }},
    {{ ""Attribute"": ""Phone"", ""Value"": ""{mobile}"" }}
  ],
  ""Opportunity"": {{
    ""OpportunityEventCode"": 12000,
    ""OpportunityNote"": ""Opportunity"",
    ""OverwriteFields"": true,
    ""DoNotPostDuplicateActivity"": false,
    ""DoNotChangeOwner"": true,
    ""Fields"": [
      {{ ""SchemaName"": ""mx_Custom_2"", ""Value"": ""New Lead"" }},
      {{ ""SchemaName"": ""Status"", ""Value"": ""Open"" }},
      {{ ""SchemaName"": ""mx_Custom_11"", ""Value"": ""Digimyin"" }},
      {{ ""SchemaName"": ""mx_Custom_1"", ""Value"": ""{name}"" }},
      {{ ""SchemaName"": ""mx_Custom_35"", ""Value"": ""Mukhedkar Complex, Shivaji Putala, Vazirabad, Nanded, Maharashtra, India 431601"" }},
      {{
        ""SchemaName"": ""mx_Custom_36"",
        ""Value"": """",
        ""Fields"": [
          {{ ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""{sapcode}"" }},
          {{ ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""{SapName}"" }},
          {{ ""SchemaName"": ""mx_CustomObject_3"", ""Value"": ""777777"" }}
        ]
      }},
      {{ ""SchemaName"": ""mx_Custom_32"", ""Value"": ""431601"" }},
      {{ ""SchemaName"": ""mx_Custom_13"", ""Value"": ""1971-12-11"" }},
      {{ ""SchemaName"": ""mx_Custom_12"", ""Value"": ""New Business"" }},
      {{ ""SchemaName"": ""mx_Custom_43"", ""Value"": "" Indusind Nippon Life Super Suraksha"" }},
      {{ ""SchemaName"": ""mx_Custom_44"", ""Value"": ""Health Plan"" }},
      {{ ""SchemaName"": ""mx_Custom_45"", ""Value"": ""188"" }},
      {{ ""SchemaName"": ""mx_Custom_15"", ""Value"": ""20-50 Lacs"" }},
      {{ ""SchemaName"": ""mx_Custom_27"", ""Value"": ""API"" }},
      {{ ""SchemaName"": ""mx_Custom_54"", ""Value"": ""Individual"" }},
      {{
        ""SchemaName"": ""mx_Custom_46"",
        ""Value"": """",
        ""Fields"": [
          {{ ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""API"" }},
          {{ ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""DM"" }}
        ] 
      }}
    ]
  }}
}}";

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, content);
        var responseJson = await response.Content.ReadAsStringAsync();

        var obj = JsonConvert.DeserializeObject<LeadSquaredResponse>(responseJson);

        return (obj, responseJson);
    }

    public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI_SE(string name, string mobile, string sapcode, string SapName, string categoryName)
    {
        // var url = "https://karma.indusindnipponlife.com/v1.0/nlms/leads";
        var url = "https://karma.indusindnipponlife.com/v1.0/nlms/push-leads";

        //var url = "https://sa3dev.indusindnipponlife.com/v1.0/nlms/push-leads";

        string cleanedName = Regex.Replace(name ?? "", @"[^a-zA-Z\s]", "").Trim();
        string cleanMobile = Regex.Replace(mobile ?? "", @"\D", "");


        var jsonBody = $@"
                {{
                    ""userId"": ""{sapcode}"",
                    ""name"": ""{name}"",
                    ""phoneNo"": ""{mobile}"",
                    ""loginType"": ""Individual"",
                    ""leadType"": ""New Business"",
                    ""apiSource"": ""API"",
                    ""campaign"": ""Digimyin""
                }}";



        var request = new HttpRequestMessage(HttpMethod.Post, url);


        request.Headers.Add("x-api-key", "RNLIC10203040");
        request.Headers.Add("x-client-id", "22a0b4e5-940f-40a4-984d-6af1ac8e8c9e");

        //  request.Headers.TryAddWithoutValidation("x-token","eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjp7InVzZXJJZCI6IjcwMjc5MjM3IiwiZGV2aWNlSWQiOiIxMjM0NTY3ODk4NyJ9LCJpYXQiOjE3NzM4MTIwNzMsImV4cCI6MTc3Mzg0MDg3M30.qzSDs9Aul9AsazgaXWmybOTtlOtz5bCNEtajy8s43pnr4G72paPw-SqmDqpSxWnNZbRHNWOxzcDca_eJrJSl7mS7E424rK-ZXOtB1351e8kBmS3ivF6zLuWRVMnw9krAuwhbHPIcjYBYi4iIw5ooeYYHOjZ7jD-Aefja_UOpQ_KMknYDsuOPAZBwNRNZ_I0Ni-cXTCvzesz4VS4JZ0WzexBlLLOt47z2Gb8aTohrRApBniWUMF60WhCJk__HVTSD8PPw9l9bdgQHB4FmVWLLOkLMTvXIr442a_6qW--wHXwHgzZ0fhYEibkel3jaNg9-6rtI_j8WLhTwO1cJn9273w");

        request.Headers.Add("origin", "https://karma.indusindnipponlife.com");

        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);
        var responseJson = await response.Content.ReadAsStringAsync();

        LeadSquaredResponse apiResponse = new LeadSquaredResponse();

        if (response.IsSuccessStatusCode)
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(responseJson);

                apiResponse.Status = 1;
                apiResponse.RequestId = obj?.id ?? obj?.leadId ?? Guid.NewGuid().ToString();
            }
            catch
            {
                apiResponse.Status = 1;
                apiResponse.RequestId = Guid.NewGuid().ToString();
            }
        }
        else
        {
            apiResponse.Status = 0;
            apiResponse.RequestId = null;
            apiResponse.ExceptionMessage = responseJson;
        }

        return (apiResponse, responseJson);
    }
}