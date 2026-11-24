using System;

namespace TiendaApp.Modelo
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public string Turno { get; set; }      // solo cajeros (tabla cajero)
        public bool Activo { get; set; }
    }

    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Cajero = "Cajero";
        public static readonly string[] Todos = { Cajero, Administrador };
    }

    public static class Turnos
    {
        public static readonly string[] Todos = { "Mañana", "Tarde", "Noche" };

        public static bool EnHorario(string turno, DateTime ahora)
        {
            int h = ahora.Hour;
            switch (turno)
            {
                case "Mañana": return h >= 6 && h < 14;
                case "Tarde": return h >= 14 && h < 22;
                case "Noche": return h >= 22 || h < 6;
                default: return false;
            }
        }

        public static string Horario(string turno)
        {
            switch (turno)
            {
                case "Mañana": return "Horario: 6:00 a.m. - 2:00 p.m.";
                case "Tarde": return "Horario: 2:00 p.m. - 10:00 p.m.";
                case "Noche": return "Horario: 10:00 p.m. - 6:00 a.m.";
                default: return "";
            }
        }
    }

    /// <summary>Usuario que ha iniciado sesion.</summary>
    public static class Sesion
    {
        public static Usuario Actual { get; set; }

        public static bool EsAdministrador
        {
            get { return Actual != null && Actual.Rol == Roles.Administrador; }
        }
    }
}
