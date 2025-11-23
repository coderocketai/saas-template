namespace CodeRocket.Common.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class ApplicationConstants
{
    public const string DefaultCulture = "ru-RU";
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    
    public static class DateFormats
    {
        public const string Default = "yyyy-MM-dd";
        public const string DateTime = "yyyy-MM-dd HH:mm:ss";
        public const string DateTimeWithTimeZone = "yyyy-MM-dd HH:mm:ss zzz";
    }
    
    public static class ValidationMessages
    {
        public const string RequiredField = "Поле обязательно для заполнения";
        public const string InvalidEmail = "Некорректный формат email";
        public const string InvalidLength = "Недопустимая длина поля";
    }
}
