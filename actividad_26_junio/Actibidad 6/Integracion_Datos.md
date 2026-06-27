# Integración de Datos

## Parte 1: Evaluación Conceptual y Buenas Prácticas

### 1. Formatos de Intercambio

| Formato | Ventajas | Desventajas |
|--------|----------|-------------|
| CSV | - Ligero y fácil de generar.
- Adecuado para datos tabulares simples.
- Ampliamente compatible con hojas de cálculo y ETL. | - No admite estructura jerárquica compleja.
- No contiene metadatos ni esquema.
- Sensible a comas y saltos de línea dentro de los campos. |
| XML | - Soporta estructuras anidadas y metadatos.
- Bueno para interoperabilidad entre sistemas empresariales.
- Validación con esquemas XSD. | - Más verboso y pesado en tamaño.
- Requiere un análisis más costoso en CPU.
- Puede ser más lento de procesar frente a formatos más simples. |

### 2. Diferenciación de Procesos

La serialización convierte un objeto en una representación de texto o bytes, mientras que la deserialización reconstruye el objeto desde esa representación. Con `System.Text.Json`, la serialización usa `JsonSerializer.Serialize(...)` y la deserialización usa `JsonSerializer.Deserialize<T>(...)`, donde el foco técnico es transformar entre instancias de C# y JSON. En este flujo, `JsonSerializerOptions` permite controlar el formato, como `PropertyNameCaseInsensitive = true` para hacer la lectura más flexible.

### 3. Antipatrón del Rendimiento

El error de rendimiento "N+1" ocurre cuando se procesan archivos masivos en ciclos que disparan operaciones independientes por cada registro, provocando múltiples accesos a recursos externos o a la base de datos. En el procesamiento de CSV, esto se soluciona con una estrategia de batching: se lee el archivo línea por línea, se acumulan los registros en un buffer intermedio y se hace una sola inserción o actualización masiva, reduciendo drásticamente la cantidad de llamadas y evitando la saturación de memoria y de I/O.

## Parte 2: Implementación Práctica en C#

### Desafío 1: Consumo de Endpoints y Deserialización

```csharp
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Carrera { get; set; }
    public string Correo { get; set; }
}

public class AlumnoService
{
    private readonly HttpClient _httpClient;

    public AlumnoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Alumno[]> ObtenerAlumnosAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync("https://api.usac.edu/v1/alumnos");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<Alumno[]>(json, options) ?? Array.Empty<Alumno>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Error al consultar el endpoint de alumnos.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error al deserializar la respuesta JSON.", ex);
        }
    }
}
```

### Desafío 2: Endpoint para Carga Masiva CSV

```csharp
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class AlumnoCsvDto
{
    public string Nombre { get; set; }
    public string Carrera { get; set; }
    public string Correo { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AlumnosController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AlumnosController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("carga-masiva-csv")]
    public async Task<IActionResult> CargarAlumnosCsv(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest("El archivo CSV es obligatorio.");
        }

        var registros = new List<Alumno>();

        using var stream = archivo.OpenReadStream();
        using var reader = new StreamReader(stream);

        string linea;
        while ((linea = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                continue;
            }

            var partes = linea.Split(',');
            if (partes.Length < 3)
            {
                continue;
            }

            registros.Add(new Alumno
            {
                Nombre = partes[0].Trim(),
                Carrera = partes[1].Trim(),
                Correo = partes[2].Trim()
            });
        }

        if (registros.Count > 0)
        {
            _dbContext.Alumnos.AddRange(registros);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new { registrosProcesados = registros.Count });
    }
}
```

## Parte 3: Referencias Bibliográficas

- Facultad de Ingeniería, USAC. (2026). Sesión 20: Integración de Datos. Consumo de APIs Externas y Carga Masiva (CSV/XML). Laboratorio del curso Introducción a la Programación y Computación 2. Guatemala.
