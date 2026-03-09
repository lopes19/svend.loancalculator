namespace Svend.LoanCalculator.Api.Shared;

public static class Validate
{
    public static void GreaterThan(decimal value, decimal comparison, string message)
    {
        if (value <= comparison)
            throw new BusinessException(message);
    }

    public static void GreaterThan(int value, int comparison, string message)
    {
        if (value <= comparison)
            throw new BusinessException(message);
    }

    public static void LessOrEqualThan(int value, int comparison, string message)
    {
        if (value > comparison)
            throw new BusinessException(message);
    }
}
