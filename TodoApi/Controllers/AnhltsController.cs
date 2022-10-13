using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnhltsController : ControllerBase
    {
        private readonly todoDBContext _context;

        public AnhltsController(todoDBContext context)
        {
            _context = context;
        }

        // GET: api/Anhlts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Anhlt>>> GetAnhlts()
        {
            return await _context.Anhlts.ToListAsync();
        }

        // GET: api/Anhlts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Anhlt>> GetAnhlt(int id)
        {
            var anhlt = await _context.Anhlts.FindAsync(id);

            if (anhlt == null)
            {
                return NotFound();
            }

            return anhlt;
        }

        // PUT: api/Anhlts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnhlt(int id, Anhlt anhlt)
        {
            if (id != anhlt.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(anhlt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnhltExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Anhlts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Anhlt>> PostAnhlt(Anhlt anhlt)
        {
            _context.Anhlts.Add(anhlt);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AnhltExists(anhlt.PersonId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAnhlt", new { id = anhlt.PersonId }, anhlt);
        }

        // DELETE: api/Anhlts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnhlt(int id)
        {
            var anhlt = await _context.Anhlts.FindAsync(id);
            if (anhlt == null)
            {
                return NotFound();
            }

            _context.Anhlts.Remove(anhlt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AnhltExists(int id)
        {
            return _context.Anhlts.Any(e => e.PersonId == id);
        }
    }
}
