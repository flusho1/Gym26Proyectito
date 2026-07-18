using Dapper;
using Gym26.Models;
using System.Data;
using Npgsql;

namespace Gym26.Services
{
    public class GymService
    {
        private readonly IDbConnection _db;
        private readonly UserSession _session; // Declaramos una sola vez

        public GymService(IDbConnection db, UserSession session)
        {
            _db = db;
            _session = session;
        }

        // --- MÉTODOS DE EJERCICIOS ---
        public async Task<IEnumerable<Ejercicio>> GetEjerciciosAsync()
        {
            return await _db.QueryAsync<Ejercicio>("SELECT Id, Nombre, GrupoMuscular, UrlGif FROM Ejercicios");
        }

        public async Task<bool> AgregarEjercicioAsync(Ejercicio nuevoEjercicio)
        {
            const string sql = "INSERT INTO Ejercicios (Nombre, GrupoMuscular) VALUES (@Nombre, @GrupoMuscular)";
            var result = await _db.ExecuteAsync(sql, nuevoEjercicio);
            return result > 0;
        }

        // --- MÉTODOS DE RUTINAS ---
        public async Task<IEnumerable<Rutina>> GetRutinasAsync()
        {
            const string sql = @"SELECT r.*, e.Nombre AS NombreEjercicio 
                         FROM Rutinas r
                         INNER JOIN Ejercicios e ON r.EjercicioId = e.Id
                         WHERE r.UsuarioId = @UsuarioId
                         ORDER BY r.FechaRegistro DESC";

            // Usamos _session.UsuarioId directamente
            return await _db.QueryAsync<Rutina>(sql, new { UsuarioId = _session.UsuarioId });
        }



        public async Task AgregarRutinaAsync(Rutina rutina)
        {
            const string sql = @"
                INSERT INTO Rutinas (UsuarioId, EjercicioId, Series, PesoKG, Repeticiones, FechaRegistro) 
                VALUES (@UsuarioId, @EjercicioId, @Series, @PesoKG, @Repeticiones, @FechaRegistro)";

            rutina.UsuarioId = _session.UsuarioId; 
            await _db.ExecuteAsync(sql, rutina);
        }

        public async Task<bool> EliminarRutinaAsync(int id)
        {
            // Solo borra si el ID de la rutina corresponde al usuario en sesión
            var sql = "DELETE FROM Rutinas WHERE Id = @Id AND UsuarioId = @UsuarioId";
            var result = await _db.ExecuteAsync(sql, new { Id = id, UsuarioId = _session.UsuarioId });
            return result > 0;
        }

        // --- MÉTODOS DE PLANTILLAS ---
        public async Task<List<Plantilla>> GetPlantillasAsync()
        {
            const string sql = @"SELECT * FROM Plantillas 
                         WHERE UsuarioId = @UsuarioId OR UsuarioId IS NULL";

            var result = await _db.QueryAsync<Plantilla>(sql, new { UsuarioId = _session.UsuarioId });
            return result.ToList();
        }

        public async Task<int> GuardarPlantillaCompletaAsync(string nombre, List<PlantillaDetalle> detalles)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using var transaction = _db.BeginTransaction();
            try
            {
                // 1. Cambiamos SCOPE_IDENTITY() por RETURNING id para PostgreSQL
                // 2. Usamos 'plantillas' en minúsculas y 'usuarioid' para coincidir con la DB
                string sqlInsertPlantilla = @"
            INSERT INTO plantillas (nombre, usuarioid) 
            VALUES (@Nombre, @UsuarioId) 
            RETURNING id;";

                int plantillaId = await _db.ExecuteScalarAsync<int>(
                    sqlInsertPlantilla,
                    new { Nombre = nombre, UsuarioId = _session.UsuarioId },
                    transaction);

                // 3. Asegúrate de que la tabla se llame 'plantilla_detalle' y sus columnas no tengan guiones
                string sqlInsertDetalle = @"
            INSERT INTO plantilla_detalle (plantillaid, ejercicioid, series, repeticiones) 
            VALUES (@PlantillaId, @EjercicioId, @Series, @Repeticiones)";

                foreach (var detalle in detalles)
                {
                    detalle.PlantillaId = plantillaId;
                    await _db.ExecuteAsync(sqlInsertDetalle, detalle, transaction);
                }
                transaction.Commit();
                return plantillaId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task AplicarPlantillaAsync(int plantillaId)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using var transaction = _db.BeginTransaction();
            try
            {
                var detalles = await _db.QueryAsync<PlantillaDetalle>(
                    "SELECT * FROM PlantillaDetalles WHERE PlantillaId = @PlantillaId",
                    new { PlantillaId = plantillaId }, transaction);

                foreach (var item in detalles)
                {
                    await _db.ExecuteAsync(
        @"INSERT INTO Rutinas (UsuarioId, EjercicioId, Series, Repeticiones, FechaRegistro) 
          VALUES (@UsuarioId, @EjercicioId, @Series, @Repeticiones, GETDATE())",
        new { UsuarioId = _session.UsuarioId, item.EjercicioId, item.Series, item.Repeticiones },
        transaction);
                }
                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }

        public async Task<List<RecordPersonal>> GetRecordsPersonalesAsync()
        {
            const string sql = @"
        SELECT e.Nombre AS Ejercicio, MAX(r.PesoKG) AS MaxPeso
        FROM Rutinas r
        INNER JOIN Ejercicios e ON r.EjercicioId = e.Id
        WHERE r.UsuarioId = @UsuarioId
        GROUP BY e.Nombre
        ORDER BY MaxPeso DESC";

            return (await _db.QueryAsync<RecordPersonal>(sql, new { UsuarioId = _session.UsuarioId })).ToList();
        }

        // Guardar cambios en un ejercicio existente
        public async Task ActualizarEjercicioAsync(Ejercicio ejercicio)
        {
            await _db.ExecuteAsync(
                @"UPDATE Ejercicios 
          SET Nombre = @Nombre, 
              GrupoMuscular = @GrupoMuscular, 
              UrlGif = @UrlGif 
          WHERE Id = @Id",
                ejercicio);
        }

        public async Task<List<Rutina>> GetHistorialEjercicioAsync(int ejercicioId)
        {
            const string sql = @"
        SELECT FechaRegistro, PesoKG 
        FROM Rutinas 
        WHERE EjercicioId = @EjercicioId AND UsuarioId = @UsuarioId
        ORDER BY FechaRegistro ASC";

            return (await _db.QueryAsync<Rutina>(sql, new { EjercicioId = ejercicioId, UsuarioId = _session.UsuarioId })).ToList();
        }

        public async Task<Rutina?> GetUltimaMarcaAsync(int ejercicioId)
        {
            const string sql = @"
        SELECT TOP 1 * FROM Rutinas 
        WHERE EjercicioId = @EjercicioId AND UsuarioId = @UsuarioId
        ORDER BY FechaRegistro DESC";

            return await _db.QueryFirstOrDefaultAsync<Rutina>(sql, new { EjercicioId = ejercicioId, UsuarioId = _session.UsuarioId });
        }


        public async Task<IEnumerable<Usuario>> GetUsuariosAsync()
        {
            return await _db.QueryAsync<Usuario>("SELECT * FROM Usuarios");
        }

        public async Task<List<PlanRutina>> GetPlanesAsync()
        {
            string sql = "SELECT Id, Nombre AS NombreDia, Descripcion FROM Plantillas";

            var resultados = await _db.QueryAsync<PlanRutina>(sql);
            return resultados.ToList();
        }

        public async Task<List<PlantillaDetalle>> GetEjerciciosByPlanAsync(int planId)
        {
            return (await _db.QueryAsync<PlantillaDetalle>(
                "SELECT * FROM PlantillaDetalles WHERE PlantillaId = @planId",
                new { planId })).ToList();
        }


        public async Task<List<DateTime>> GetDiasEntrenadosAsync()
        {
            const string sql = @"SELECT DISTINCT CAST(FechaRegistro AS DATE) 
                         FROM Rutinas 
                         WHERE UsuarioId = @UsuarioId";

            var result = await _db.QueryAsync<DateTime>(sql, new { UsuarioId = _session.UsuarioId });
            return result.ToList();
        }
    }



}