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
    public class PeopleController : ControllerBase
    {
        private readonly todoDBContext _context;

        public PeopleController(todoDBContext context)
        {
            _context = context;
        }

        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persons>>> GetPersons()
        {
            return await _context.PersonList.ToListAsync();
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Persons>> GetPerson(int id)
        {
            var person = await _context.PersonList.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        // PUT: api/People/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Persons person)
        {
            if (id != person.PersonId)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
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

        // POST: api/People
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Persons>> PostPerson(Persons person)
        {
            _context.PersonList.Add(person);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PersonExists(person.PersonId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPerson", new { id = person.PersonId }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _context.PersonList.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.PersonList.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PersonExists(int id)
        {
            return _context.PersonList.Any(e => e.PersonId == id);
        }
    }
}
