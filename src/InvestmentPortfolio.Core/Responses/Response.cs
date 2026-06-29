namespace InvestmentPortfolio.Core.Responses;

public class Response<TData>
{
    public TData? Data { get; set; }
    public int Code { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess => Code is >= 200 and <= 299;

    public Response() { }

    public Response(TData? data, int code, string? message = null)
    {
        Data = data;
        Code = code;
        Message = message;
    }
}