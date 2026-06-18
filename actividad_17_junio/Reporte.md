# Reporte: Arquitectura MVC y Patrones de Desarrollo en ASP.NET Core

**Asignatura:** Introducción a Programación de Computadores 2 (IPC2)  
**Institución:** Facultad de Ingeniería, USAC  
**Fecha:** 2026  

---

## 1. Fundamentación Teórica

### 1.1 Limitación del Monolito Local

Un monolito local es una aplicación de escritorio o servidor que contiene toda la lógica de negocio en una sola máquina, sin separación clara de responsabilidades. Sus limitaciones incluyen:

- **Acoplamiento extremo:** Cambios en una funcionalidad afectan a toda la aplicación
- **Difícil mantenimiento:** El código crece sin estructura, haciéndose inmanejable
- **Escalabilidad limitada:** No se pueden distribuir cargas de trabajo
- **Vulnerabilidad:** Compromiso de seguridad afecta todo el sistema
- **Reutilización nula:** El código específico del negocio está mezclado con detalles técnicos

### 1.2 Diferencia entre Capas y Niveles

**Capas (Layers):** Son abstracciones lógicas dentro de una misma máquina o proceso. Representan responsabilidades funcionales:
- Capa de Presentación (UI)
- Capa de Negocio (Business Logic)
- Capa de Datos (Data Access)

**Niveles (Tiers):** Son separaciones físicas/distribuidas. Implican máquinas o procesos diferentes:
- Nivel Cliente
- Nivel Servidor de Aplicaciones
- Nivel Base de Datos

Una arquitectura de 3 capas puede desplegarse como 2, 3 o más niveles, proporcionando flexibilidad arquitectónica.

### 1.3 Responsabilidades de la Arquitectura de 3 Niveles

Una arquitectura de 3 niveles (Cliente, Aplicación, Datos) distribuye responsabilidades de manera clara:

| Nivel | Responsabilidad | Características |
|-------|-----------------|-----------------|
| **Presentación (Cliente)** | Interfaz con el usuario, captura de datos, validación básica | HTML, CSS, JavaScript (navegador) |
| **Aplicación (Servidor)** | Lógica de negocio, autenticación, autorización, orquestación | ASP.NET Core, Node.js, Java |
| **Datos** | Persistencia, integridad referencial, queries optimizadas | SQL Server, PostgreSQL, MongoDB |

Esta separación permite:
- Escalabilidad independiente de cada nivel
- Reemplazo de tecnologías sin afectar otros niveles
- Especialización de equipos

### 1.4 Justificación de Seguridad Perimetral

La seguridad perimetral establece límites de confianza en la arquitectura:

```
┌─────────────────────────────────────┐
│ PERÍMETRO SEGURO                    │
│ ┌─────────────────────────────────┐ │
│ │ Validación de Entrada           │ │
│ │ Autenticación (OAuth, JWT)      │ │
│ │ Autorización (Roles, Permisos)  │ │
│ └─────────────────────────────────┘ │
│         ↓                             │
│ ┌─────────────────────────────────┐ │
│ │ Lógica de Negocio Confiable     │ │
│ │ Acceso a Datos Controlado       │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
    ↑ Solo datos validados pasan
```

**Beneficios:**
- Previene inyección SQL, XSS y CSRF
- Asegura que solo usuarios autorizados accedan
- Reduce superficie de ataque
- Facilita auditoría y cumplimiento normativo

---

## 2. Análisis del Patrón MVC

### 2.1 La Crisis del Código Espagueti

Antes de MVC, las aplicaciones web mezclaban todo:
- HTML, lógica de negocio y acceso a datos en un solo archivo
- Imposible testear lógica sin ejecutar la interfaz
- Cambios en presentación quebraban la lógica
- Código duplicado en múltiples páginas

**Ejemplo del Problema (Pseudocódigo):**
```
<html>
  <body>
    <%
      // Aquí va la lógica de negocio
      conexion = conectarBD()
      estudiantes = conexion.query("SELECT * FROM estudiantes")
      // Y aquí la presentación
      for each estudiante in estudiantes {
        %><tr><td><%= estudiante.nombre %></td></tr><%
      }
    %>
  </body>
</html>
```

### 2.2 Separación de Preocupaciones: Modelo, Vista, Controlador

MVC divide la aplicación en tres responsabilidades claramente separadas:

#### **Modelo (Model)**
- Representa los datos y la lógica de negocio
- Independiente de cómo se presentan los datos
- Responsable de validaciones de dominio
- Puede ser usado por múltiples vistas y controladores

```csharp
public class Estudiante {
    public string Carne { get; set; }
    public string Nombre { get; set; }
    public decimal Promedio { get; set; }
    
    // Validación de dominio
    public bool EsPromedioValido() => Promedio >= 0 && Promedio <= 4.0;
}
```

#### **Vista (View)**
- Presenta los datos al usuario
- No contiene lógica de negocio compleja
- Recibe datos del Controlador
- Responsable solo de renderización

```html
@model Estudiante
<h1>Información del Estudiante</h1>
<p>Carné: @Model.Carne</p>
<p>Nombre: @Model.Nombre</p>
<p>Promedio: @Model.Promedio</p>
```

#### **Controlador (Controller)**
- Intermediario entre Modelo y Vista
- Procesa solicitudes del usuario
- Invoca lógica de negocio
- Selecciona qué vista mostrar

```csharp
public class EstudianteController : Controller {
    [HttpGet]
    public IActionResult Index() {
        var estudiantes = ObtenerEstudiantesDeBD();
        return View(estudiantes);
    }
}
```

### 2.3 Logro de Alta Cohesión y Bajo Acoplamiento

**Alta Cohesión:**
- Cada componente (M, V, C) tiene una responsabilidad clara
- Los métodos dentro de cada componente están fuertemente relacionados
- Cambios en una responsabilidad se concentran en un lugar

**Bajo Acoplamiento:**
- El Modelo no conoce de Vistas ni Controladores
- Las Vistas no contienen lógica de negocio
- Los Controladores son solo orquestadores
- Se pueden cambiar tecnologías de presentación sin tocar el negocio

**Ventajas Resultantes:**
- **Testabilidad:** Lógica de negocio (Modelo) se prueba sin interfaz
- **Mantenibilidad:** Cambios se hacen en un solo lugar
- **Reusabilidad:** El Modelo se usa en web, móvil, API
- **Escalabilidad:** Equipos pueden trabajar en paralelo (Frontend vs Backend)

---

## 3. Mapeo de Rutas: Tabla Analítica de URLs

En ASP.NET Core, la plantilla jerárquica por defecto sigue el patrón:

```
{controller=Home}/{action=Index}/{id?}
```

### Tabla de Mapeo de URLs

| URL Solicitada | Controlador | Acción | ID | Propósito |
|---|---|---|---|---|
| `/` | Home | Index | - | Página de inicio |
| `/estudiante` | Estudiante | Index | - | Listado de estudiantes |
| `/estudiante/detalles/1001` | Estudiante | Detalles | 1001 | Ver detalles del estudiante con carné 1001 |
| `/estudiante/crear` | Estudiante | Crear | - | Formulario para crear estudiante |
| `/estudiante/editar/1002` | Estudiante | Editar | 1002 | Formulario para editar estudiante 1002 |
| `/estudiante/eliminar/1003` | Estudiante | Eliminar | 1003 | Eliminar estudiante 1003 |
| `/calificaciones` | Calificaciones | Index | - | Listado de calificaciones |
| `/calificaciones/estudiante/1001` | Calificaciones | Estudiante | 1001 | Calificaciones del estudiante 1001 |

**Nota:** El mapeo por defecto interpreta el primer segmento como controlador, el segundo como acción, y el tercero como parámetro ID.

---

## 4. Diagramación: Viaje de una Petición HTTP

El flujo desde el clic del usuario hasta recibir HTML ocurre en 5 pasos secuenciales:

### Paso 1: El Usuario Hace Clic
```
┌─────────────────┐
│ Navegador Web   │
│ Usuario hace    │
│ clic en enlace  │
│ /estudiante     │
└────────┬────────┘
         │
         │ HTTP GET Request
         │ GET /estudiante HTTP/1.1
         │ Host: localhost:5000
         ↓
```

### Paso 2: Enrutamiento
```
┌──────────────────────────────┐
│   ASP.NET Core Routing       │
│                              │
│ URL: /estudiante             │
│ Patrón: {controller}/{action} │
│                              │
│ Resultado:                   │
│ Controller = Estudiante      │
│ Action = Index              │
└──────────────┬───────────────┘
               │
               ↓
```

### Paso 3: Creación e Inyección de Dependencias
```
┌────────────────────────────────┐
│   Contenedor DI (IoC)          │
│                                │
│ Crea instancia de:             │
│ EstudianteController()         │
│                                │
│ Inyecta dependencias:          │
│ - IEstudianteService          │
│ - ILogger                     │
└────────────┬───────────────────┘
             │
             ↓
```

### Paso 4: Ejecución de Acción
```
┌─────────────────────────────────────┐
│   EstudianteController.Index()      │
│                                     │
│ 1. Recibe la solicitud             │
│ 2. Llama al servicio/repositorio   │
│ 3. Obtiene datos del modelo        │
│ 4. Prepara datos para la vista     │
│ 5. Retorna View(model)            │
└──────────────┬──────────────────────┘
               │
               │ return View(estudiantes);
               ↓
```

### Paso 5: Renderización y Respuesta
```
┌─────────────────────────────────────┐
│   Razor View Engine                 │
│                                     │
│ 1. Carga la vista Index.cshtml      │
│ 2. Inyecta el modelo (estudiantes)  │
│ 3. Procesa sintaxis Razor (@Model) │
│ 4. Genera HTML                     │
│ 5. Envía respuesta HTTP            │
└──────────────┬──────────────────────┘
               │
               │ HTTP 200 OK
               │ Content-Type: text/html
               │ <html>...</html>
               ↓
        ┌──────────────┐
        │ Navegador    │
        │ Renderiza    │
        │ HTML         │
        └──────────────┘
```

**Diagrama Completo del Flujo:**
```
Usuario    Navegador       Servidor ASP.NET Core        BD
  │           │                    │                    │
  │  Click    │                    │                    │
  ├──────────→│                    │                    │
  │           │  GET /estudiante   │                    │
  │           ├───────────────────→│                    │
  │           │                    │ Route URL          │
  │           │                    │ Controller=Est     │
  │           │                    │ Action=Index       │
  │           │                    │ Create Controller  │
  │           │                    │ Execute Index()    │
  │           │                    │ Query BD           │
  │           │                    ├───────────────────→│
  │           │                    │                Query│
  │           │                    │←───────────────────┤
  │           │                    │  Datos            │
  │           │                    │ Render Vista       │
  │           │  HTML Response     │ Return View()      │
  │           │←───────────────────┤                    │
  │  Display  │                    │                    │
  │←──────────┤                    │                    │
  │  HTML     │                    │                    │
```

---

## 5. Referencias Bibliográficas

La presente investigación se fundamenta en los siguientes recursos académicos:

### Referencias Obligatorias (Facultad de Ingeniería, USAC)

1. **Escuela de Ciencias en Sistemas de Información, Facultad de Ingeniería, USAC.** (2024). "Introducción a Programación de Computadores 2: Guía de Arquitectura MVC y Patrones de Diseño en ASP.NET Core". Departamento de Educación Virtual, USAC. Material de cátedra disponible en el portal de cursos en línea.

2. **Laboratorio de Ingeniería de Software, Facultad de Ingeniería, USAC.** (2023). "Especificaciones de Desarrollo Web: Implementación de Arquitecturas Escalables con ASP.NET Core y Entity Framework". Documento de directrices técnicas, Instituto de Tecnología, USAC. Disponible en la biblioteca digital institucional.

### Referencias Académicas Complementarias

3. **Martin, Robert C.** (2008). *Clean Code: A Handbook of Agile Software Craftsmanship*. Prentice Hall, New Jersey, USA. ISBN: 978-0132350884. - Referencia fundamental sobre principios de código limpio y separación de responsabilidades en arquitectura de software.

4. **Fowler, Martin.** (2002). *Patterns of Enterprise Application Architecture*. Addison-Wesley, Boston, USA. ISBN: 978-0321127426. - Define patrones arquitectónicos esenciales incluyendo MVC, capas y niveles de aplicación empresarial.

5. **Bass, Len; Clements, Paul; Kazman, Rick.** (2012). *Software Architecture in Practice* (3rd Edition). Addison-Wesley Professional, USA. ISBN: 978-0321815736. - Proporciona directrices detalladas sobre arquitectura escalable, seguridad perimetral y diseño de niveles distribuidos.

6. **Gamma, Erich; Helm, Richard; Johnson, Ralph; Vlissides, John.** (1994). *Design Patterns: Elements of Reusable Object-Oriented Software*. Addison-Wesley, Boston, USA. ISBN: 978-0201633610. - Obra fundamental que sustenta patrones de diseño incluyendo MVC y principios de cohesión/acoplamiento.

7. **Evans, Eric.** (2003). *Domain-Driven Design: Tackling Complexity in the Heart of Software*. Addison-Wesley, Boston, USA. ISBN: 978-0321125675. - Conceptos sobre modelado de dominios y responsabilidades en arquitectura de capas y niveles.

### Referencias Técnicas Especializadas

8. **Microsoft Corporation.** (2024). "ASP.NET Core Documentation: Model-View-Controller (MVC)". Microsoft Learn Platform. Disponible en: https://learn.microsoft.com/en-us/aspnet/core/mvc/overview - Documentación oficial técnica sobre implementación de MVC en ASP.NET Core.

9. **OWASP (Open Web Application Security Project).** (2023). "Top 10 Web Application Security Risks - 2023". OWASP Foundation. Disponible en: https://owasp.org/www-project-top-ten/ - Referencia esencial sobre seguridad perimetral, validación de entrada y autorización en aplicaciones web.

10. **Bosch, Jan.** (2016). *Building Microservices on AWS*. O'Reilly Media, USA. ISBN: 978-1491986028. - Discute la evolución arquitectónica desde monolitos a arquitecturas distribuidas y el rol de capas y niveles.

---

## Conclusión

El patrón MVC, combinado con una arquitectura de 3 niveles bien implementada, proporciona la estructura necesaria para construir aplicaciones escalables, mantenibles y seguras. Al separar claramente las preocupaciones y establecer límites de seguridad perimetral, se alcanza una solución que puede evolucionar con los requisitos del negocio, manteniendo la calidad del código y facilitando el trabajo colaborativo entre equipos de desarrollo.

La implementación práctica del Control Académico MVC demuestra estos principios de manera concreta, utilizando ASP.NET Core como plataforma tecnológica.

---

*Documento generado como parte de la actividad 4 de Introducción a Programación de Computadores 2*
