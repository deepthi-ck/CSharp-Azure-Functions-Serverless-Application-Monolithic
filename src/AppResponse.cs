namespace FunctionsMonolith
{
    public sealed class AppResponse
    {
        public string Status { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Node { get; set; }
        public string Message { get; set; }
        public bool Found { get; set; }
        public static AppResponse Success(string key, string value, string node)
        {
            return new AppResponse { Status = "SUCCESS", Key = key, Value = value, Node = node, Found = true, Message = "ok" };
        }
        public static AppResponse NotFound(string key)
        {
            return new AppResponse { Status = "NOT_FOUND", Key = key, Found = false, Message = "not found" };
        }
        public static AppResponse Error(string message)
        {
            return new AppResponse { Status = "ERROR", Found = false, Message = message ?? string.Empty };
        }
    }
}
