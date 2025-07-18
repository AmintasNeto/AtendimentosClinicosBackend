namespace BaseLibrary.Responses
{
    public record LoginResponse(bool Flag, string Message = null!, string Username = null!, string Role = null!, string Token = null!, string RefreshToken = null!, int UserId = -1);
}
