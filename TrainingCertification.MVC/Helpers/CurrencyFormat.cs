namespace TrainingCertification.MVC.Helpers;

public static class CurrencyFormat
{
    public static string Money(decimal value) => $"BHD {value:N2}";
}
