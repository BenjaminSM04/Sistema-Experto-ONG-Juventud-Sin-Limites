# ?? Integraci�n de Identity con el Modelo de Dominio

## ?? Resumen de Cambios

Se ha integrado exitosamente **ASP.NET Core Identity** con el modelo de dominio personalizado, permitiendo usar las entidades `Usuario`, `Rol` y `UsuarioRol` directamente con el sistema de autenticaci�n.

---

## ? Cambios Principales

### 1. **Entidad Usuario**
```csharp
public class Usuario : IdentityUser<int>, IAuditable, ISoftDelete
```

**Beneficios:**
- ? Hereda toda la funcionalidad de Identity (hashing de passwords, lockout, 2FA, etc.)
- ? Mantiene los campos de auditor�a personalizados
- ? Soft delete integrado
- ? Relaci�n 1:1 con Persona (separaci�n de concerns)

**Campos heredados de IdentityUser<int>:**
- `Id` (int) - ID del usuario
- `UserName` - Nombre de usuario
- `Email` - Correo electr�nico
- `PasswordHash` - Hash de la contrase�a
- `AccessFailedCount` - Intentos fallidos de login
- `LockoutEnd` - Fecha de bloqueo
- `EmailConfirmed` - Email confirmado
- `PhoneNumber` - Tel�fono
- `TwoFactorEnabled` - 2FA habilitado
- Y m�s...

**Campos personalizados agregados:**
- `PersonaId` - FK a Persona
- `Estado` - Estado personalizado (Activo/Inactivo)
- Campos de auditor�a (IAuditable)
- Campos de soft delete (ISoftDelete)
- `RowVersion` - Control de concurrencia

---

### 2. **Entidad Rol**
```csharp
public class Rol : IdentityRole<int>, IAuditable, ISoftDelete
```

**Campos heredados de IdentityRole<int>:**
- `Id` (int) - ID del rol
- `Name` - Nombre del rol
- `NormalizedName` - Nombre normalizado
- `ConcurrencyStamp` - Control de concurrencia

**Campos personalizados:**
- `Descripcion` - Descripci�n del rol
- Campos de auditor�a
- Campos de soft delete
- `RowVersion`

---

### 3. **Entidad UsuarioRol**
```csharp
public class UsuarioRol : IdentityUserRole<int>, IAuditable, ISoftDelete
```

**Campos heredados:**
- `UserId` (int) - ID del usuario
- `RoleId` (int) - ID del rol

**Campos personalizados:**
- `AsignadoEn` - Fecha de asignaci�n
- Campos de auditor�a
- Campos de soft delete
- `RowVersion`

---

### 4. **ApplicationDbContext Actualizado**

```csharp
public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int, 
    IdentityUserClaim<int>,
    UsuarioRol,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
```

**Tablas de Identity creadas:**
- `Usuario` - Usuarios del sistema
- `Rol` - Roles del sistema  
- `UsuarioRol` - Relaci�n Usuario-Rol
- `UsuarioClaim` - Claims de usuarios
- `UsuarioLogin` - Logins externos (Google, Facebook, etc.)
- `RolClaim` - Claims de roles
- `UsuarioToken` - Tokens de usuarios

---

### 5. **Configuraci�n de Identity en Program.cs**

```csharp
builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<Rol>() // ? Roles personalizados
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();
```

---

## ?? Uso del Sistema

### Crear Usuario con Identity

```csharp
// Inyectar UserManager y RoleManager
@inject UserManager<Usuario> UserManager
@inject RoleManager<Rol> RoleManager

// Crear una persona primero
var persona = new Persona
{
    Nombres = "Juan",
  Apellidos = "P�rez",
    FechaNacimiento = new DateTime(1995, 5, 15),
    Telefono = "1234-5678"
};
await context.Personas.AddAsync(persona);
await context.SaveChangesAsync();

// Crear el usuario con Identity
var usuario = new Usuario
{
    PersonaId = persona.PersonaId,
  UserName = "juan.perez@ong.com",
    Email = "juan.perez@ong.com",
    EmailConfirmed = true,
    Estado = EstadoGeneral.Activo
};

// Crear con password (autom�ticamente hace el hash)
var result = await UserManager.CreateAsync(usuario, "Password123!");

if (result.Succeeded)
{
    // Asignar rol
    await UserManager.AddToRoleAsync(usuario, "Facilitador");
}
```

### Login

```csharp
@inject SignInManager<Usuario> SignInManager

// Login con email y password
var result = await SignInManager.PasswordSignInAsync(
    email,
    password,
    isPersistent: false,
    lockoutOnFailure: true
);

if (result.Succeeded)
{
 // Login exitoso
}
else if (result.RequiresTwoFactor)
{
    // Requiere 2FA
}
else if (result.IsLockedOut)
{
  // Usuario bloqueado
}
```

### Obtener Usuario Actual

```csharp
@inject UserManager<Usuario> UserManager
@inject AuthenticationStateProvider AuthState

var authState = await AuthState.GetAuthenticationStateAsync();
var user = await UserManager.GetUserAsync(authState.User);

// Acceder a datos personales
var persona = await context.Personas
    .FirstOrDefaultAsync(p => p.PersonaId == user.PersonaId);

Console.WriteLine($"{persona.Nombres} {persona.Apellidos}");
```

### Verificar Roles

```csharp
// En c�digo C#
var isAdmin = await UserManager.IsInRoleAsync(user, "Administrador");

// En Razor
@if (User.IsInRole("Administrador"))
{
    <AdminPanel />
}
```

### Cambiar Password

```csharp
var result = await UserManager.ChangePasswordAsync(
    user,
    currentPassword,
    newPassword
);
```

### Reset Password

```csharp
// Generar token
var token = await UserManager.GeneratePasswordResetTokenAsync(user);

// Enviar por email...

// Resetear
var result = await UserManager.ResetPasswordAsync(user, token, newPassword);
```

---

## ?? Usuario Administrador por Defecto

El seeder crea autom�ticamente:

**Email:** `admin@ong.com`  
**Password:** `Admin@123`  
**Rol:** Administrador

---

## ?? Ventajas de esta Integraci�n

### ? Seguridad
- Password hashing autom�tico con algoritmos robustos
- Lockout autom�tico despu�s de intentos fallidos
- Soporte para 2FA out-of-the-box
- Tokens seguros para reset de password
- Claims y policies integrados

### ? Funcionalidad
- Login/Logout manejado por Identity
- Recuperaci�n de contrase�a
- Confirmaci�n de email
- Logins externos (Google, Facebook, Microsoft, etc.)
- Gesti�n de claims personalizados
- Pol�ticas de autorizaci�n

### ? Mantenimiento
- Todo el modelo de dominio en un solo lugar
- Auditor�a autom�tica
- Soft delete integrado
- Concurrencia con RowVersion
- Extensible f�cilmente

### ? Compatibilidad
- Funciona con todos los componentes de Identity de Blazor
- Compatible con SignalR
- Compatible con APIs
- Soporte para cookies y JWT

---

## ?? Esquema de Relaciones

```
Persona (1) ????? (1) Usuario (hereda IdentityUser<int>)
           ?         ?
    ?   ??? (N) UsuarioRol (hereda IdentityUserRole<int>)
        ?     ?        ?
              ?         ?     ??? (1) Rol (hereda IdentityRole<int>)
         ?         ?
  ?         ??? (N) UsuarioPrograma
           ?        ?
        ?                  ??? (1) Programa
          ?
     ??? (1) Participante
         ?
         ??? (N) ActividadParticipante
```

---

## ?? Configuraciones Adicionales Disponibles

### En Program.cs:

```csharp
builder.Services.AddIdentityCore<Usuario>(options =>
{
    // Password
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    
    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
// User
    options.User.RequireUniqueEmail = true;
    
    // SignIn
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});
```

---

## ?? Migraci�n

Para aplicar los cambios a la base de datos:

```bash
# Eliminar migraciones anteriores si es necesario
dotnet ef migrations remove --context ApplicationDbContext

# Crear nueva migraci�n
dotnet ef migrations add IdentityIntegration --context ApplicationDbContext

# Aplicar a base de datos
dotnet ef database update --context ApplicationDbContext
```

---

## ?? Notas Importantes

1. **No usar ApplicationUser:** Todos los componentes ahora usan `Usuario` (alias global creado)

2. **Tablas de Identity:** Se renombraron para seguir la convenci�n de tu BD:
   - AspNetUsers ? Usuario
   - AspNetRoles ? Rol
   - AspNetUserRoles ? UsuarioRol
   - etc.

3. **IDs como int:** Todas las entidades usan `int` como PK en lugar del `string` por defecto de Identity

4. **Soft Delete en Identity:** Las tablas de Identity tambi�n tienen soft delete

5. **Persona vs Usuario:** Separa datos personales (Persona) de credenciales (Usuario)

---

## ?? Pr�ximos Pasos

1. ? Aplicar migraci�n
2. ? Personalizar componentes de UI de Identity seg�n necesidades
3. ? Implementar env�o real de emails (reemplazar IdentityNoOpEmailSender)
4. ? Configurar proveedores externos (Google, Facebook)
5. ? Agregar pol�ticas de autorizaci�n personalizadas
6. ? Implementar claims personalizados
7. ? Configurar 2FA si es necesario

---

**? Integraci�n completada exitosamente!**  
El sistema ahora usa Identity de forma nativa con tu modelo de dominio.

