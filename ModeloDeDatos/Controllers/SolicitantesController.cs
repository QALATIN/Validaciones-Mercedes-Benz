using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloDeDatos.Models;
using ModeloDeDatos.Context;
namespace ModeloDeDatos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolicitantesController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;

        public SolicitantesController(IDBiometricsMercedezBenzContext context)
        {
            _context = context;
        }

        [HttpGet("{count}")]
        // GET: Solicitantes
        public async Task<ActionResult<IEnumerable<Solicitante>>> GetSolicitantes(int count)
        {
            //obtenemos los {count} solicitantes de la base de datos que aún no estén finalizados incluyendo el registro de validación
            var itemContext = _context.Solicitantes.Where(x => x.Estatus.ToLower() != "finalizada")
                                                    .OrderByDescending(x => x.FechaEnvio)
                                                    .Include(i => i.Validaciones)
                                                    .Select(x => x)
                                                    .Take(count);
            return await itemContext.ToListAsync();
        }

    }
}
