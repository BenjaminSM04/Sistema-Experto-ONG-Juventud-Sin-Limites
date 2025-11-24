# Sistema Experto - ONG Juventud Sin Límites

![Logo ONG Juventud Sin Límites](wwwroot/images/logoInstitucion.png)

## 📋 Descripción

Sistema de gestión integral para la **ONG Juventud Sin Límites**, desarrollado con tecnologías modernas de .NET 8 y Blazor Server. Este sistema experto integra inteligencia artificial para la gestión de programas sociales, participantes, actividades y recursos, con un motor de inferencia que genera alertas y recomendaciones automatizadas.

## ✨ Características Principales

### 🎯 Gestión de Programas
- **4 Programas Principales**: Escuela de Vida (EDV), Academias, Juventud Segura y Bernabé
- Gestión completa de actividades y eventos
- Seguimiento de participantes por programa
- Control de asistencias
- Gestión de evidencias (fotos, actas, videos)
- Reportes y exportaciones en PDF

### 🤖 Motor de Inferencia IA
- Sistema experto basado en reglas
- Generación automática de alertas
- Análisis de patrones de asistencia
- Detección de riesgos en participantes
- Recomendaciones inteligentes

### 📊 Planes Operativos Anuales (POA)
- Creación y gestión de POAs dinámicos
- Plantillas configurables por programa
- Exportación a PDF con diseño profesional
- Seguimiento de cumplimiento de metas
- Métricas y KPIs automatizados

### 👥 Gestión de Participantes
- Registro completo de participantes
- Inscripción en múltiples actividades
- Historial de asistencias
- Estados de inscripción
- Gestión de datos personales

### 🔐 Seguridad Avanzada
- Autenticación con ASP.NET Core Identity
- Autenticación de dos factores (2FA) con TOTP
- Sistema de roles (Administrador, Coordinador, Usuario)
- Permisos granulares por programa
- Auditoría completa de acciones

### 📧 Sistema de Notificaciones
- Envío automático de credenciales por email
- Notificaciones de alertas
- Confirmación de email
- Recuperación de contraseña

### 📈 Dashboards y Reportes
- KPIs en tiempo real
- Gráficas interactivas (MudBlazor Charts)
- Reportes exportables en PDF
- Métricas de cumplimiento
- Estadísticas de programas

## 🛠️ Tecnologías Utilizadas

### Backend
- **.NET 8.0** - Framework principal
- **ASP.NET Core Blazor Server** - Interfaz de usuario interactiva
- **Entity Framework Core 8** - ORM para acceso a datos
- **SQL Server** - Base de datos relacional
- **ASP.NET Core Identity** - Autenticación y autorización

### Frontend
- **Blazor Server** - Renderizado del lado del servidor
- **MudBlazor 6.x** - Biblioteca de componentes UI
- **JavaScript Interop** - Interoperabilidad con JavaScript
- **CSS personalizado** - Diseño con paleta corporativa

### Seguridad
- **TOTP (Time-Based One-Time Password)** - 2FA
- **Data Protection API** - Encriptación de datos sensibles
- **HTTPS** - Comunicación segura
- **Anti-CSRF Tokens** - Protección contra ataques

### Servicios Externos
- **OpenWeatherMap API** - Información meteorológica
- **SMTP (Gmail)** - Envío de correos electrónicos
- **QuestPDF** - Generación de documentos PDF

## 🚀 Instalación y Configuración

### Prerrequisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB o Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Pasos de Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/BenjaminSM04/Sistema-Experto-ONG-Juventud-Sin-Limites.git
cd Sistema-Experto-ONG-Juventud-Sin-Limites
```

2. **Restaurar paquetes NuGet**
```bash
dotnet restore
```

3. **Configurar la base de datos**

Editar `appsettings.json` con la cadena de conexión:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SistemaExpertoONG;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

4. **Aplicar migraciones**
```bash
dotnet ef database update
```

5. **Configurar servicios externos (opcional)**

En `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "tu-email@gmail.com",
    "SenderPassword": "tu-app-password",
    "SenderName": "ONG Juventud Sin Límites"
  },
  "WeatherApi": {
    "ApiKey": "tu-api-key",
    "City": "Managua",
    "CountryCode": "NI"
  }
}
```

6. **Ejecutar la aplicación**
```bash
dotnet run
```

7. **Acceder al sistema**
- URL: `https://localhost:7232`
- Usuario administrador por defecto:
  - **Email**: admin@ong.com
  - **Contraseña**: Admin@123

## 📁 Estructura del Proyecto

```
Sistema-Experto-ONG-Juventud-Sin-Limites/
├── Components/              # Componentes Blazor
│   ├── Account/            # Autenticación y cuenta
│   ├── Layout/             # Layouts de la aplicación
│   └── Pages/              # Páginas de la aplicación
│       ├── Admin/          # Panel de administración
│       ├── POA/            # Gestión de POAs
│       └── Programa/       # Gestión de programas
├── Data/                   # Contexto de base de datos
│   └── Migrations/         # Migraciones de EF Core
├── Domain/                 # Entidades del dominio
│   ├── Audit/              # Auditoría
│   ├── BI/                 # Business Intelligence
│   ├── Common/             # Entidades comunes
│   ├── Config/             # Configuración
│   ├── Motor/              # Motor de inferencia
│   ├── Operacion/          # Entidades operativas
│   ├── POA/                # Plan Operativo Anual
│   ├── Programas/          # Programas de la ONG
│   └── Security/           # Seguridad y usuarios
├── Infrastructure/         # Implementación de servicios
│   ├── Configurations/     # Configuración de EF Core
│   ├── Seed/               # Datos iniciales
│   ├── Services/           # Servicios de la aplicación
│   └── Validation/         # Validaciones
├── Tests/                  # Pruebas unitarias
└── wwwroot/                # Archivos estáticos
    ├── css/                # Estilos CSS
    ├── images/             # Imágenes y logos
    ├── js/                 # Scripts JavaScript
    └── uploads/            # Archivos subidos
```

## 🎨 Paleta de Colores Corporativa

- **Primario**: `#4D3935` (Marrón oscuro)
- **Secundario**: `#6D534F` (Marrón medio)
- **Acento Verde**: `#9FD996` (Verde claro)
- **Acento Verde Oscuro**: `#85C97C`
- **Acento Amarillo**: `#F7C484`
- **Acento Amarillo Oscuro**: `#F3C95A`
- **Fondo**: `#FEFEFD` (Blanco cálido)
- **Fondo Secundario**: `#F7F7F7` (Gris muy claro)

## 👥 Roles y Permisos

### Administrador
- Acceso completo al sistema
- Gestión de usuarios y roles
- Configuración del motor de inferencia
- Gestión de todos los programas
- Acceso a auditoría completa

### Coordinador
- Gestión de programas asignados
- Gestión de participantes
- Creación y seguimiento de POAs
- Visualización de alertas y reportes
- Gestión de actividades

### Usuario
- Visualización de programas asignados
- Registro de asistencias
- Visualización de participantes
- Consulta de reportes básicos

## 📊 Motor de Inferencia

El sistema incluye un motor de reglas que:

1. **Analiza patrones** de asistencia y comportamiento
2. **Genera alertas** automáticas basadas en:
   - Inasistencias consecutivas
   - Bajo rendimiento
   - Cambios de comportamiento
   - Incumplimiento de metas POA
3. **Clasifica riesgos** en participantes
4. **Recomienda acciones** preventivas
5. **Prioriza** intervenciones según severidad

### Reglas Predefinidas
- Detección de ausentismo (≥3 faltas consecutivas)
- Análisis de tendencias de asistencia
- Cumplimiento de POAs
- Participación activa en programas

## 📱 Características Responsive

El sistema está completamente optimizado para:
- 💻 Computadoras de escritorio
- 💼 Tablets
- 📱 Smartphones

## 🔒 Seguridad

- ✅ Autenticación de dos factores (2FA)
- ✅ Encriptación de contraseñas (BCrypt)
- ✅ Tokens CSRF
- ✅ Sanitización de HTML
- ✅ Validación de archivos subidos
- ✅ Auditoría de acciones
- ✅ Sesiones seguras
- ✅ HTTPS obligatorio en producción

## 📝 Licencia

Este proyecto es propiedad de **ONG Juventud Sin Límites** y es de uso interno exclusivo.

## 👨‍💻 Desarrolladores

**Benjamín Saenz**
**Grisel Aranda**
**Brisa Criales**

- GitHub: [@BenjaminSM04](https://github.com/BenjaminSM04)
- Proyecto desarrollado como sistema de gestión para la ONG Juventud Sin Límites

## 🤝 Contribuciones

Este es un proyecto privado para uso interno de la organización. Las contribuciones están limitadas a miembros autorizados.

## 📞 Soporte

Para soporte técnico o consultas sobre el sistema:
- Email: benjaminsaenz17@gmail.com
- Sistema de tickets interno

## 🔄 Historial de Versiones

### Versión 1.0.0 (Actual)
- ✅ Gestión completa de programas
- ✅ Motor de inferencia IA
- ✅ Sistema de POAs
- ✅ Autenticación 2FA
- ✅ Exportación de reportes PDF
- ✅ Gestión de participantes
- ✅ Dashboard interactivo
- ✅ Sistema de alertas

---

<p align="center">
  <strong>ONG Juventud Sin Límites</strong><br>
  Transformando vidas, construyendo futuro 🌟
</p>
