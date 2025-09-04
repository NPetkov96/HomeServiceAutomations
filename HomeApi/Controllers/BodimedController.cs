using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BodimedController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public BodimedController(DataBaseContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<List<MedSestriBloodTest>> GetAllBloodTestsAsync()
        {
            Console.WriteLine("done");
            return await _context.MedSestriBloodTests.ToListAsync();
        }
    }
}
