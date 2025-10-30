# Script para resetear y aplicar Identity Integration

Write-Host "?? Integrando Identity con el modelo de dominio..." -ForegroundColor Cyan
Write-Host ""

# 1. Eliminar base de datos anterior
Write-Host "1?? Eliminando base de datos anterior..." -ForegroundColor Yellow
dotnet ef database drop --context ApplicationDbContext --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Base de datos eliminada" -ForegroundColor Green
} else {
    Write-Host "   ??  No se pudo eliminar la BD (puede que no existiera)" -ForegroundColor Yellow
}
Write-Host ""

# 2. Eliminar migraciones anteriores
Write-Host "2?? Eliminando migraciones anteriores..." -ForegroundColor Yellow
Remove-Item -Path "Data\Migrations\*" -Force -ErrorAction SilentlyContinue
if ($?) {
    Write-Host "   ? Migraciones eliminadas" -ForegroundColor Green
} else {
    Write-Host "   ??  No había migraciones previas" -ForegroundColor Yellow
}
Write-Host ""

# 3. Crear nueva migración con Identity
Write-Host "3?? Creando nueva migración con Identity integrado..." -ForegroundColor Yellow
dotnet ef migrations add IdentityIntegration --context ApplicationDbContext --output-dir Data/Migrations
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Migración creada exitosamente" -ForegroundColor Green
} else {
    Write-Host "   ? Error al crear migración" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 4. Aplicar migración
Write-Host "4?? Aplicando migración a la base de datos..." -ForegroundColor Yellow
dotnet ef database update --context ApplicationDbContext
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ? Base de datos actualizada exitosamente" -ForegroundColor Green
} else {
    Write-Host "   ? Error al aplicar migración" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 5. Resumen
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? INTEGRACIÓN DE IDENTITY COMPLETADA EXITOSAMENTE ?" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Tablas creadas:" -ForegroundColor Yellow
Write-Host "   • Usuario (con Identity integrado)" -ForegroundColor White
Write-Host "   • Rol (con Identity integrado)" -ForegroundColor White
Write-Host "   • UsuarioRol" -ForegroundColor White
Write-Host "   • UsuarioClaim, UsuarioLogin, UsuarioToken, RolClaim" -ForegroundColor White
Write-Host "   • + todas las tablas del dominio (47 en total)" -ForegroundColor White
Write-Host ""
Write-Host "?? Usuario admin por defecto:" -ForegroundColor Yellow
Write-Host "   Email:    admin@ong.com" -ForegroundColor Cyan
Write-Host "   Password: Admin@2025!" -ForegroundColor Cyan
Write-Host "   Rol:      Administrador" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? REQUISITOS DE CONTRASEÑAS:" -ForegroundColor Magenta
Write-Host "   • Mínimo 12 caracteres" -ForegroundColor White
Write-Host "   • Al menos 1 letra mayúscula" -ForegroundColor White
Write-Host "   • Al menos 1 letra minúscula" -ForegroundColor White
Write-Host "   • Al menos 1 número" -ForegroundColor White
Write-Host "   • Al menos 1 carácter especial (!@#$%^&*)" -ForegroundColor White
Write-Host ""
Write-Host "?? Consulta IDENTITY_INTEGRATION.md para más información" -ForegroundColor Magenta
Write-Host ""
Write-Host "?? Ya puedes ejecutar la aplicación con: dotnet run" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
