namespace ControlAcademicoMvc.Models
{
    /// <summary>
    /// Clase que representa un estudiante en el sistema de control académico.
    /// Esta clase forma parte de la capa de Modelo en la arquitectura MVC.
    /// </summary>
    public class Estudiante
    {
        /// <summary>
        /// Número de carné del estudiante.
        /// Identificador único en el sistema.
        /// </summary>
        public string Carne { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del estudiante.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Promedio académico del estudiante.
        /// Rango válido: 0.0 a 4.0
        /// </summary>
        public decimal Promedio { get; set; }

        /// <summary>
        /// Valida que el promedio esté dentro del rango permitido.
        /// </summary>
        /// <returns>true si el promedio es válido; false en caso contrario</returns>
        public bool EsPromedioValido()
        {
            return Promedio >= 0 && Promedio <= 4.0m;
        }
    }
}
