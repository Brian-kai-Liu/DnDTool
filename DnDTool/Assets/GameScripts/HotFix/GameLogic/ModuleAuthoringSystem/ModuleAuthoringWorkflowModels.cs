namespace GameLogic
{
    internal sealed class ModuleAuthoringOperationResult
    {
        public bool Success { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public static ModuleAuthoringOperationResult Ok(string message = "")
        {
            return new ModuleAuthoringOperationResult
            {
                Success = true,
                Message = message ?? string.Empty,
            };
        }

        public static ModuleAuthoringOperationResult Fail(string message)
        {
            return new ModuleAuthoringOperationResult
            {
                Success = false,
                Message = message ?? string.Empty,
            };
        }
    }
}
