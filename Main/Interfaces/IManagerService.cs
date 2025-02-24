using Main.Requests;
using Main.Responses;

namespace Main.Interfaces;

public interface IManagerService
{
    Task<ApiResponse<SearchResponse>> SearchAsync(SearchRequest request);
    ApiResponse<BookResponse> Book(BookRequest request);
    Task<ApiResponse<CheckStatusResponse>> CheckStatusAsync(CheckStatusRequest request);

}
