using Microsoft.AspNetCore.Mvc;
using ControlAcademicoMvc.Models;

namespace ControlAcademicoMvc.Controllers
{
    /// <summary>
    /// Controlador responsable de manejar solicitudes relacionadas con estudiantes.
    /// Este controlador implementa el patrón MVC con responsabilidades claramente separadas.
    /// </summary>
    public class EstudianteController : Controller
    {
        /// <summary>
        /// Base de datos simulada en memoria.
        /// En una aplicación real, esto vendría de un repositorio o contexto Entity Framework.
        /// </summary>
        private static List<Estudiante> _estudiantes = new()
        {
            new Estudiante { Carne = "2020001", Nombre = "Juan García López", Promedio = 3.85m },
            new Estudiante { Carne = "2020002", Nombre = "María Rodríguez Pérez", Promedio = 3.92m },
            new Estudiante { Carne = "2020003", Nombre = "Carlos Martínez Sánchez", Promedio = 3.45m },
            new Estudiante { Carne = "2020004", Nombre = "Ana Hernández Ruiz", Promedio = 3.78m },
            new Estudiante { Carne = "2020005", Nombre = "Luis Díaz Torres", Promedio = 3.55m }
        };

        /// <summary>
        /// Acción GET que retorna el listado de todos los estudiantes.
        /// </summary>
        /// <returns>Vista Index con lista de estudiantes</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View(_estudiantes);
        }

        /// <summary>
        /// Acción GET que retorna los detalles de un estudiante específico.
        /// </summary>
        /// <param name="id">Carné del estudiante a consultar</param>
        /// <returns>Vista Details con datos del estudiante, o NotFound si no existe</returns>
        [HttpGet]
        public IActionResult Detalles(string id)
        {
            var estudiante = _estudiantes.FirstOrDefault(e => e.Carne == id);
            return estudiante == null ? NotFound() : View(estudiante);
        }

        /// <summary>
        /// Acción GET que retorna la vista de creación de nuevo estudiante.
        /// </summary>
        /// <returns>Vista Crear con formulario vacío</returns>
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        /// <summary>
        /// Acción POST que crea un nuevo estudiante.
        /// </summary>
        /// <param name="estudiante">Datos del estudiante a crear</param>
        /// <returns>Redirección a Index tras éxito</returns>
        [HttpPost]
        public IActionResult Crear(Estudiante estudiante)
        {
            if (ModelState.IsValid && estudiante.EsPromedioValido())
            {
                _estudiantes.Add(estudiante);
                return RedirectToAction(nameof(Index));
            }
            return View(estudiante);
        }

        /// <summary>
        /// Acción POST que elimina un estudiante.
        /// </summary>
        /// <param name="id">Carné del estudiante a eliminar</param>
        /// <returns>Redirección a Index tras éxito</returns>
        [HttpPost]
        public IActionResult Eliminar(string id)
        {
            var estudiante = _estudiantes.FirstOrDefault(e => e.Carne == id);
            if (estudiante != null)
            {
                _estudiantes.Remove(estudiante);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
