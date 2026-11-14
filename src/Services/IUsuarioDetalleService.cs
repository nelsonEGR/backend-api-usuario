using ApiUsuario.Dtos;

namespace ApiUsuario.Services;

public interface IUsuarioDetalleService
{
    Task AssignPasswordAsync(UsuarioPasswordDto dto, CancellationToken cancellationToken);

    Task ChangePasswordAsync(UsuarioCambioPasswordDto dto, CancellationToken cancellationToken);

    Task ResetPasswordAsync(UsuarioResetPasswordAdminDto dto, CancellationToken cancellationToken);

    Task UpdateLocationAsync(int idUsuario, UsuarioUbicacionDto dto, CancellationToken cancellationToken);
}
