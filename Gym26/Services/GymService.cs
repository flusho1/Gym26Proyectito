using Dapper;
using Gym26.Models;
using System.Data;
using Npgsql;

namespace Gym26.Services
{
    public class GymService
    {
        private readonly IDbConnection _db;
        private readonly UserSession _session;

        public GymService(IDbConnection db, UserSession session)
        {
            _db = db;
            _session = session;
        }

        // --- MÉTODOS DE EJERCICIOS ---
        public async Task<IEnumerable<Ejercicio>> GetEjerciciosAsync()
        {
            return await _db.QueryAsync<Ejercicio>("SELECT id, nombre, grupomuscular, urlgif FROM ejercicios");
        }

        public async Task<bool> AgregarEjercicioAsync(Ejercicio nuevoEjercicio)
        {
            const string sql = "INSERT INTO ejercicios (nombre, grupomuscular) VALUES (@Nombre, @GrupoMuscular)";
            var result = await _db.ExecuteAsync(sql, nuevoEjercicio);
            return result > 0;
        }

        // --- MÉTODOS DE RUTINAS ---
        public async Task<IEnumerable<Rutina>> GetRutinasAsync()
        {
            const string sql = @"SELECT r.*, e.nombre AS nombreEjercicio 
                                 FROM rutinas r
                                 INNER JOIN ejercicios e ON r.ejercicioid = e.id
                                 WHERE r.usuarioid = @UsuarioId
                                 ORDER BY r.fecharegistro DESC";

            return await _db.QueryAsync<Rutina>(sql, new { UsuarioId = _session.UsuarioId });
        }

        public async Task AgregarRutinaAsync(Rutina rutina)
        {
            const string sql = @"
                INSERT INTO rutinas (usuarioid, ejercicioid, series, pesokg, repeticiones, fecharegistro) 
                VALUES (@UsuarioId, @EjercicioId, @Series, @PesoKG, @Repeticiones, CURRENT_TIMESTAMP)";

            rutina.UsuarioId = _session.UsuarioId;
            await _db.ExecuteAsync(sql, rutina);
        }

        public async Task<bool> EliminarRutinaAsync(int id)
        {
            // Usamos los nombres de columna exactos de tu tabla
            var sql = "DELETE FROM rutinas WHERE id = @Id AND usuarioid = @UsuarioId";

            var result = await _db.ExecuteAsync(sql, new
            {
                Id = id,
                UsuarioId = _session.UsuarioId
            });

            return result > 0;
        }

        public async Task<bool> EliminarEjercicioAsync(int id)
        {
            // Asegúrate de usar el nombre de la tabla correcto (ej. 'ejercicios') 
            // y el nombre de la columna id
            var sql = "DELETE FROM ejercicios WHERE id = @Id";

            var result = await _db.ExecuteAsync(sql, new { Id = id });

            return result > 0;
        }

        // --- MÉTODOS DE PLANTILLAS ---
        public async Task<List<Plantilla>> GetPlantillasAsync()
        {
            const string sql = @"SELECT * FROM plantillas 
                                 WHERE usuarioid = @UsuarioId OR usuarioid IS NULL";

            var result = await _db.QueryAsync<Plantilla>(sql, new { UsuarioId = _session.UsuarioId });
            return result.ToList();
        }

        public async Task<int> GuardarPlantillaCompletaAsync(string nombre, List<PlantillaDetalle> detalles)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using var transaction = _db.BeginTransaction();
            try
            {
                string sqlInsertPlantilla = "INSERT INTO plantillas (nombre, usuarioid) VALUES (@Nombre, @UsuarioId) RETURNING id;";
                int plantillaId = await _db.ExecuteScalarAsync<int>(sqlInsertPlantilla, new { Nombre = nombre, UsuarioId = _session.UsuarioId }, transaction);

                string sqlInsertDetalle = "INSERT INTO plantilla_detalle (plantillaid, ejercicioid, series, repeticiones) VALUES (@PlantillaId, @EjercicioId, @Series, @Repeticiones)";

                foreach (var detalle in detalles)
                {
                    detalle.PlantillaId = plantillaId;
                    await _db.ExecuteAsync(sqlInsertDetalle, detalle, transaction);
                }
                transaction.Commit();
                return plantillaId;
            }
            catch { transaction.Rollback(); throw; }
        }

        public async Task AplicarPlantillaAsync(int plantillaId)
        {
            if (_db.State != ConnectionState.Open) _db.Open();
            using var transaction = _db.BeginTransaction();
            try
            {
                var detalles = await _db.QueryAsync<PlantillaDetalle>("SELECT * FROM plantilla_detalle WHERE plantillaid = @PlantillaId", new { PlantillaId = plantillaId }, transaction);

                foreach (var item in detalles)
                {
                    await _db.ExecuteAsync(@"INSERT INTO rutinas (usuarioid, ejercicioid, series, repeticiones, fecharegistro) 
                                             VALUES (@UsuarioId, @EjercicioId, @Series, @Repeticiones, CURRENT_TIMESTAMP)",
                                             new { UsuarioId = _session.UsuarioId, item.EjercicioId, item.Series, item.Repeticiones }, transaction);
                }
                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }

        public async Task<List<RecordPersonal>> GetRecordsPersonalesAsync()
        {
            const string sql = @"
                SELECT e.nombre AS ejercicio, MAX(r.pesokg) AS maxpeso
                FROM rutinas r
                INNER JOIN ejercicios e ON r.ejercicioid = e.id
                WHERE r.usuarioid = @UsuarioId
                GROUP BY e.nombre
                ORDER BY maxpeso DESC";

            return (await _db.QueryAsync<RecordPersonal>(sql, new { UsuarioId = _session.UsuarioId })).ToList();
        }

        public async Task ActualizarEjercicioAsync(Ejercicio ejercicio)
        {
            await _db.ExecuteAsync("UPDATE ejercicios SET nombre = @Nombre, grupomuscular = @GrupoMuscular, urlgif = @UrlGif WHERE id = @Id", ejercicio);
        }

        public async Task<List<Rutina>> GetHistorialEjercicioAsync(int ejercicioId)
        {
            const string sql = "SELECT fecharegistro, pesokg FROM rutinas WHERE ejercicioid = @EjercicioId AND usuarioid = @UsuarioId ORDER BY fecharegistro ASC";
            return (await _db.QueryAsync<Rutina>(sql, new { EjercicioId = ejercicioId, UsuarioId = _session.UsuarioId })).ToList();
        }

        public async Task<Rutina?> GetUltimaMarcaAsync(int ejercicioId)
        {
            const string sql = "SELECT * FROM rutinas WHERE ejercicioid = @EjercicioId AND usuarioid = @UsuarioId ORDER BY fecharegistro DESC LIMIT 1";
            return await _db.QueryFirstOrDefaultAsync<Rutina>(sql, new { EjercicioId = ejercicioId, UsuarioId = _session.UsuarioId });
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosAsync()
        {
            return await _db.QueryAsync<Usuario>("SELECT * FROM usuarios");
        }

        public async Task<List<PlanRutina>> GetPlanesAsync()
        {
            return (await _db.QueryAsync<PlanRutina>("SELECT id, nombre AS nombredia, descripcion FROM plantillas")).ToList();
        }

        public async Task<List<PlantillaDetalle>> GetEjerciciosByPlanAsync(int planId)
        {
            return (await _db.QueryAsync<PlantillaDetalle>("SELECT * FROM plantilla_detalle WHERE plantillaid = @planId", new { planId })).ToList();
        }

        public async Task<bool> EliminarPlanAsync(int id, int usuarioId)
        {
            var sql = "DELETE FROM plantillas WHERE id = @Id AND usuarioid = @UsuarioId";

            var result = await _db.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });

            return result > 0;
        }

        public async Task<List<DateTime>> GetDiasEntrenadosAsync()
        {
            const string sql = "SELECT DISTINCT fecharegistro::date FROM rutinas WHERE usuarioid = @UsuarioId";
            var result = await _db.QueryAsync<DateTime>(sql, new { UsuarioId = _session.UsuarioId });
            return result.ToList();
        }
    }
}