public class UsuarioPasswordDto
{
    public int IdUsuario { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class UsuarioCambioPasswordDto
{
    public int IdUsuario { get; set; }
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}

public class UsuarioResetPasswordAdminDto
{
    public int IdUsuario { get; set; }
    public string PasswordNueva { get; set; } = string.Empty;
}