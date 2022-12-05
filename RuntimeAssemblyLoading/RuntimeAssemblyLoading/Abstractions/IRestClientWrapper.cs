using RestSharp;

namespace RuntimeAssemblyLoading.Abstractions;
public interface IRestClientWrapper
{
    public string BaseUrl { get; }

    Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request) => this.ExecuteAsync<T>(request, CancellationToken.None);

    Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken);

    RestResponse Get(RestRequest request);
    RestResponse Post(RestRequest request);
    RestResponse Put(RestRequest request);
    RestResponse Delete(RestRequest request);
}

