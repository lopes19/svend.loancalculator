namespace Svend.LoanCalculator.Api.Shared;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(List<string> messages) : base(string.Join("; ", messages)) { }
}
