using RestSharp;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Helpers;
public class RestClientWrapper : IRestClientWrapper
{
    private readonly RestClient client;

    public string? BaseUrl => this.client.Options.BaseUrl?.AbsoluteUri;

    public RestClientWrapper(string baseUrl, int maxTimeout = default)
    {

        this.client = maxTimeout == default ?
            new(new RestClientOptions { BaseUrl = new(baseUrl) }) :
            new(new RestClientOptions { BaseUrl = new(baseUrl), MaxTimeout = maxTimeout });
    }

    public RestResponse Delete(RestRequest request)
    {
        return this.client.Delete(request);
    }

    public async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request) => await this.ExecuteAsync<T>(request, CancellationToken.None);

    public async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken)
    {
        return await this.client.ExecuteAsync<T>(request, cancellationToken);
    }

    public RestResponse Get(RestRequest request)
    {
        return this.client.ExecuteGet(request);
    }

    public RestResponse Post(RestRequest request)
    {
        return this.client.ExecutePost(request);
    }

    public RestResponse Put(RestRequest request)
    {
        return this.client.ExecutePut(request);
    }
}