using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QCBur_dll
{
    public class AWSSimpleLambda
    {
        private readonly AmazonLambdaClient _lambdaClient;

        public AWSSimpleLambda(AWSCredentials credentials, RegionEndpoint region)
        {
            _lambdaClient = new AmazonLambdaClient(credentials, region);
        }

        public async Task<string> InvokeLambdaFunctionAsync(string functionName, string payload)
        {
            var request = new InvokeRequest
            {
                FunctionName = functionName,
                Payload = payload
            };

            try
            {
                var response = await _lambdaClient.InvokeAsync(request);
                using (var sr = new StreamReader(response.Payload))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            catch (AmazonLambdaException ex)
            {
                Console.WriteLine($"AmazonLambdaException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
            catch (AmazonServiceException ex)
            {
                Console.WriteLine($"AmazonServiceException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}
