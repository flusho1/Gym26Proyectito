# Gym26 🏋️‍♂️

**Gym26** es una aplicación web de gestión de rutinas de entrenamiento diseñada para ser minimalista, rápida y optimizada para dispositivos móviles. Construida con tecnología .NET, busca ofrecer una experiencia de usuario fluida mediante una interfaz limpia y una gestión de datos eficiente.

---

## 🚀 Tecnologías Utilizadas

* **Framework:** .NET (Blazor Server)
* **Base de Datos:** PostgreSQL (alojado en Neon.tech)
* **Acceso a Datos:** Dapper (Micro-ORM de alto rendimiento)
* **Estilos:** CSS personalizado (Mobile-first, diseño minimalista)
* **Despliegue:** Render

---

## 🛠️ Características Principales

* **Gestión de Rutinas:** Creación, lectura y eliminación de registros de entrenamiento personalizados.
* **Diseño Responsivo:** Interfaz optimizada para el uso con una sola mano en dispositivos móviles.
* **Arquitectura Eficiente:** Uso de Dapper para consultas SQL directas, garantizando una comunicación rápida con la base de datos PostgreSQL.

---

## 📋 Estructura de la Base de Datos

La aplicación utiliza la siguiente estructura relacional para el seguimiento de rutinas:

| Columna | Tipo |
| :--- | :--- |
| `id` | SERIAL |
| `usuarioid` | INT |
| `ejercicioid` | INT |
| `series` | INT |
| `repeticiones` | INT |
| `pesokg` | DECIMAL |
| `fecharegistro` | TIMESTAMP |

---

## 📦 Instalación

1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/flusho1/gym26.git](https://github.com/flusho1/gym26.git)
   ```
   Configurar variables: Define tu Connection String de Neon.tech en appsettings.json.

Ejecutar:

 ```bash
dotnet run
 ```

📱 Notas para iOS
Para una mejor experiencia en iPhone, abre la aplicación en Safari y selecciona "Compartir" -> "Agregar al inicio" para instalarla como una web app.
