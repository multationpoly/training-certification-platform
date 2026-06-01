using System.Net.Http.Json;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Services;

public class PublicCertificateClient
{
    private readonly HttpClient _http;
    public PublicCertificateClient(HttpClient http) => _http = http;
    public async Task<PublicCertificationLookupResultViewModel?> VerifyAsync(string traineeId, string reference)
    {
        var response = await _http.GetAsync($"api/public/certification?traineeId={Uri.EscapeDataString(traineeId)}&certificateRef={Uri.EscapeDataString(reference)}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResponseViewModel<PublicCertificationLookupResultViewModel>>();
        return payload?.Data;
    }
}
