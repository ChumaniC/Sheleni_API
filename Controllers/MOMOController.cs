using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sheleni_API.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sheleni_API.Controllers
{
    [Route("api/momo")] // Base route for all MTN MoMo API endpoints
    [ApiController]
    public class MOMOController : ControllerBase
    {
        private readonly string _apiUrl = "https://sandbox.momodeveloper.mtn.com/v1_0"; 
        private readonly HttpClient _httpClient;
        private readonly string _accessToken = "c73400d2785048cc9567da9c7b792c0a";

        public MOMOController()
        {
            _httpClient = new HttpClient();
        }

        // Endpoint to create an API User
        [HttpPost("createApiUser")]
        public async Task<IActionResult> CreateApiUserAsync([FromBody] ApiUserRequestModel model)
        {
            try
            {
                // Generate a unique reference ID
                string referenceId = Guid.NewGuid().ToString();

                // Set the request headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-Reference-Id", referenceId);
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _accessToken);

                // Construct the API endpoint URL for creating API User
                string apiUrl = $"{_apiUrl}/apiuser";

                // Create the request body as a JSON object
                var requestContent = new
                {
                    providerCallbackHost = "string"
                };

                // Serialize the request body to JSON
                string requestBody = JsonConvert.SerializeObject(requestContent);
                byte[] byteData = Encoding.UTF8.GetBytes(requestBody);

                // Send an HTTP POST request to create API User
                HttpResponseMessage response;

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await _httpClient.PostAsync(apiUrl, content);
                }

                if (response.IsSuccessStatusCode)
                {
                    // API User created successfully
                    return Ok("API User created successfully.");
                }
                else
                {
                    // Handle other HTTP status codes
                    return BadRequest("Failed to create API User");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to get API user details
        [HttpGet("getApiUserDetails/{apiUserId}")]
        public async Task<IActionResult> GetApiUserDetailsAsync(string apiUserId)
        {
            try
            {
                // Set the request headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _accessToken);

                // Construct the API endpoint URL for getting API user details
                string apiUrl = $"{_apiUrl}/apiuser/{apiUserId}";

                // Send an HTTP GET request to get API user details
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a JSON string
                    string content = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response to get API user details
                    var userDetails = JsonConvert.DeserializeObject<dynamic>(content);

                    // You can extract specific details from the response as needed
                    // For example, if the response contains a 'providerCallbackHost' field:
                    // string providerCallbackHost = userDetails.providerCallbackHost;

                    return Ok(userDetails);
                }
                else
                {
                    // Handle other HTTP status codes
                    return BadRequest("Failed to get API user details");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        // Endpoint to create an API Key
        [HttpPost("createApiKey")]
        public async Task<IActionResult> CreateApiKeyAsync([FromBody] ApiKeyRequestModel model)
        {
            try
            {
                string apiUserId = model.ApiUserId;

                // Construct the API endpoint URL for creating API Key
                string apiUrl = $"{_apiUrl}/apiuser/{apiUserId}/apikey";

                // Send an HTTP POST request to create API Key
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string content = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response to get the API Key
                    var apiKeyResponse = JsonConvert.DeserializeObject<dynamic>(content);

                    // Extract the API Key
                    string apiKey = apiKeyResponse.apiKey;

                    // API Key created successfully
                    return Ok(apiKey);
                }
                else
                {
                    // Handle other HTTP status codes
                    return BadRequest("Failed to create API Key");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to get an access token
        [HttpPost("getAccessToken")]
        public async Task<IActionResult> GetAccessTokenAsync([FromBody] AccessTokenRequestModel model)
        {
            try
            {
                string apiKey = model.ApiKey;
                string apiUser = model.ApiUser;
                string apiSecret = model.ApiSecret;

                var clientId = apiKey;
                var clientSecret = apiUser + ":" + apiSecret;
                var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret));

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                });

                var response = await _httpClient.PostAsync("/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Parse the JSON response to extract the access token.
                    // It will typically have a field like "access_token" that contains the token.
                    // Return the access token for use in your API requests.
                    dynamic tokenResponse = JsonConvert.DeserializeObject(responseContent);
                    string accessToken = tokenResponse.access_token;
                    return Ok(accessToken);
                }
                else
                {
                    // Handle error cases here.
                    return BadRequest("Failed to get an access token");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to make a payment request
        [HttpPost("requestToPay")]
        public async Task<IActionResult> RequestToPayAsync([FromBody] PaymentRequestModel model)
        {
            try
            {
                string customerMobileNumber = model.CustomerMobileNumber;
                decimal purchaseAmount = model.PurchaseAmount;

                // Construct the API endpoint URL for Request to Pay
                string apiUrl = $"{_apiUrl}/requesttopay";

                // Create the request body
                var requestContent = new
                {
                    amount = purchaseAmount,
                    currency = "EUR", // Replace with the appropriate currency
                    externalId = Guid.NewGuid(), // Generate a unique external ID
                    payer = new
                    {
                        partyIdType = "MSISDN", // Mobile number is the party ID type
                        partyId = customerMobileNumber // Customer's mobile number
                    },
                    payerMessage = "Payment for purchase", // Customize as needed
                    payeeNote = "Thank you for your purchase" // Customize as needed
                };

                // Serialize the request body to JSON
                var requestBody = JsonConvert.SerializeObject(requestContent);

                // Create an HTTP request message with headers
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("Authorization", $"Bearer {_accessToken}");
                request.Headers.Add("X-Target-Environment", "sandbox"); // Replace with "production" for live environment
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

                // Send an HTTP POST request to the MTN MoMo API
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string content = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response to get transaction details
                    var transactionDetails = JsonConvert.DeserializeObject<dynamic>(content);

                    // Extract relevant information from the response as needed
                    string transactionId = transactionDetails.transactionId;
                    string status = transactionDetails.status;

                    // Handle the transaction details or return them as needed
                    return Ok($"Transaction ID: {transactionId}, Status: {status}");
                }
                else
                {
                    // Handle other HTTP status codes
                    return BadRequest("Failed to make a payment request");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
