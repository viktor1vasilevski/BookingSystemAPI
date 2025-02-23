using Main.Requests;
using Main.Responses;

namespace Main.Interfaces;

public interface IManagerService
{
    Task<ApiResponse<SearchRes>> SearchAsync(SearchReq request);
    ApiResponse<BookRes> Book(BookReq request);
    Task<ApiResponse<CheckStatusRes>> CheckStatusAsync(CheckStatusReq request);

}
