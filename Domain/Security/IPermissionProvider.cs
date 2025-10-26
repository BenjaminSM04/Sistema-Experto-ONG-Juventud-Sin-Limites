namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

/// <summary>
/// Define las características/permisos disponibles en el sistema
/// </summary>
public enum Feature
{
    // ========== VISUALIZACIÓN ==========
    /// <summary>Dashboard completo con toda la información (Solo Admin)</summary>
    Ver_Dashboard_Completo,
    
    /// <summary>Dashboard de un programa específico</summary>
    Ver_Dashboard_Programa,
    
    /// <summary>Dashboard de una actividad específica</summary>
    Ver_Dashboard_Actividad,
    
    /// <summary>Información de presupuesto (Solo Admin)</summary>
    Ver_Presupuesto,
    
    /// <summary>Información financiera completa (Solo Admin)</summary>
    Ver_Finanzas,
    
    /// <summary>Datos sensibles de participantes (Solo Admin)</summary>
    Ver_DatosSensibles,
    
    /// <summary>Registros de asistencia</summary>
    Ver_Asistencia,
    
    /// <summary>Información de participantes</summary>
 Ver_Participantes,
  
    /// <summary>Alertas del motor de inferencia</summary>
    Ver_Alertas,
    
    /// <summary>Métricas y KPIs</summary>
    Ver_Metricas,
 
    /// <summary>POA (Plan Operativo Anual)</summary>
    Ver_POA,
    
    // ========== GESTIÓN ==========
    /// <summary>Gestionar usuarios del sistema</summary>
    Gestionar_Usuarios,
    
    /// <summary>Gestionar roles y permisos</summary>
    Gestionar_Roles,
    
    /// <summary>Crear, editar, eliminar programas</summary>
    Gestionar_Programas,
    
    /// <summary>Crear, editar, eliminar actividades</summary>
    Gestionar_Actividades,
    
    /// <summary>Gestionar participantes</summary>
    Gestionar_Participantes,
    
    /// <summary>Gestionar POA</summary>
    Gestionar_POA,
    
    // ========== MOTOR DE INFERENCIA ==========
    /// <summary>Ejecutar el motor de inferencia</summary>
    Ejecutar_Motor,
    
    /// <summary>Configurar reglas y parámetros del motor</summary>
    Configurar_Motor,
    
    /// <summary>Ver logs de ejecución del motor</summary>
    Ver_Logs_Motor,
    
    /// <summary>Gestionar reglas de inferencia</summary>
    Gestionar_Reglas,

    // ========== REPORTES ==========
    /// <summary>Generar reportes completos con información sensible</summary>
    Generar_Reportes_Completos,
    
    /// <summary>Generar reportes de programa</summary>
Generar_Reportes_Programa,
    
    /// <summary>Exportar datos a Excel/PDF</summary>
    Exportar_Datos,
    
    // ========== ASISTENCIA ==========
    /// <summary>Registrar asistencia en actividades</summary>
  Registrar_Asistencia,
    
    /// <summary>Modificar registros de asistencia</summary>
 Modificar_Asistencia,
    
    // ========== EVIDENCIAS ==========
    /// <summary>Subir evidencias (fotos, videos, documentos)</summary>
    Subir_Evidencias,
    
    /// <summary>Eliminar evidencias</summary>
    Eliminar_Evidencias,
    
    // ========== METABASE ==========
    /// <summary>Acceso completo a dashboards de Metabase</summary>
    Acceso_Metabase_Admin,
    
    /// <summary>Acceso a dashboards de Metabase filtrados por programa</summary>
  Acceso_Metabase_Coordinador,
    
    /// <summary>Acceso de solo lectura a Metabase</summary>
    Acceso_Metabase_Lectura,
    
    // ========== AUDITORÍA ==========
/// <summary>Ver logs de auditoría del sistema</summary>
    Ver_Logs_Auditoria,
    
    /// <summary>Ver historial de cambios</summary>
    Ver_Historial_Cambios
}

/// <summary>
/// Proveedor de permisos del sistema
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// Verifica si el usuario actual tiene acceso a una característica
    /// </summary>
    /// <param name="feature">Característica a verificar</param>
    /// <param name="programaId">ID del programa (opcional, para permisos específicos de programa)</param>
    /// <param name="actividadId">ID de la actividad (opcional, para permisos específicos de actividad)</param>
    /// <returns>True si tiene acceso, False si no</returns>
    Task<bool> CanAccessAsync(Feature feature, int? programaId = null, int? actividadId = null);
    
    /// <summary>
    /// Obtiene todos los features a los que el usuario actual tiene acceso
    /// </summary>
    Task<List<Feature>> GetUserFeaturesAsync();
    
    /// <summary>
    /// Obtiene los IDs de programas a los que el usuario tiene acceso
    /// </summary>
    Task<List<int>> GetUserProgramsAsync();

    /// <summary>
  /// Verifica si el usuario es administrador
    /// </summary>
  bool IsAdministrador();
    
    /// <summary>
    /// Verifica si el usuario es coordinador de al menos un programa
    /// </summary>
    Task<bool> IsCoordinadorAsync();
    
    /// <summary>
    /// Verifica si el usuario es facilitador de al menos una actividad
    /// </summary>
    Task<bool> IsFacilitadorAsync();
}
