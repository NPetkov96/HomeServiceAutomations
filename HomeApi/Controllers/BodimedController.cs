using DataLayer;
using DataLayer.Models;
using DataLayer.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BodimedController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public BodimedController(DataBaseContext context)
        {
            this._context = context;
        }

        [HttpGet("allBloodTests")] // SEND ALL BLOOD TESTs 
        public async Task<List<MedSestriBloodTest>> GetAllBloodTestsAsync()
        {
            return await _context.MedSestriBloodTests
                .OrderByDescending(x => x.HasPriority)
                .ToListAsync();
        }

        [HttpGet("allPatientsHistory")] // SEND ALL PATIENTs 
        public async Task<List<MedSestriPatientsDTO>> GetAllPatientsHistoryAsync()
        {
            var patients = await _context.MedSestriPatients
        .OrderByDescending(p => p.Date)
        .Include(p => p.PatientBloodTests)         // assuming PatientBloodTests is collection of linking entity
        .ThenInclude(pt => pt.BloodTest)           // then include the actual BloodTest
        .ToListAsync();

            var patientsDto = patients.Select(patient => new MedSestriPatientsDTO
            {
                FullName = patient.FullName,
                EGN = patient.EGN,
                PhoneNumber = patient.PhoneNumber,
                Date = patient.Date,
                Note = patient.Note,
                BloodTests = patient.PatientBloodTests.Select(pb => new MedSestriBloodTest
                {
                    Name = pb.BloodTest.Name,
                    BngPrice = pb.BloodTest.BngPrice,
                    EuroPrice = pb.BloodTest.EuroPrice
                }).ToList()
            }).ToList();

            return patientsDto;
        }

        [HttpPost("createPatient")] // CREATING NEW PATIENT
        public async Task CreatePatient([FromBody] MedSestriPatient model)
        {
            var oneHourAgo = DateTime.Now.AddHours(-1);

            var existingModel = await _context.MedSestriPatients
                .FirstOrDefaultAsync(p => p.Date >= oneHourAgo && (p.EGN == model.EGN || p.FullName == model.FullName));

            var newBloodTests = model.BloodTests
                .Select(bt => bt.Name).ToList();

            var selectedBloodTests = await _context.MedSestriBloodTests
                        .Where(bt => newBloodTests.Contains(bt.Name))
                        .ToListAsync();

            if (existingModel != null)
            {
                existingModel.FullName = model.FullName;
                existingModel.PhoneNumber = model.PhoneNumber;
                existingModel.EGN = model.EGN;
                existingModel.Date = model.Date;
                existingModel.Note = model.Note;

                var existingRelations = await _context.MedSestriPatientsBloodTests
                    .Where(x => x.PatientId == existingModel.Id)
                    .ToListAsync();

                _context.MedSestriPatientsBloodTests.RemoveRange(existingRelations);
                await _context.SaveChangesAsync();

                foreach (var test in selectedBloodTests)
                {
                    existingModel.PatientBloodTests.Add(new MedSestriPatientBloodTest
                    {
                        PatientId = existingModel.Id,
                        BloodTestId = test.Id
                    });
                }
            }
            else
            {
                var newPatient = new MedSestriPatient
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    EGN = model.EGN,
                    Date = model.Date,
                    Note = model.Note,
                    PatientBloodTests = selectedBloodTests
                        .Select(bt => new MedSestriPatientBloodTest
                        {
                            BloodTestId = bt.Id
                        })
                        .ToList()
                };

                await _context.MedSestriPatients.AddAsync(newPatient);
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost("deletePatient")] // DELETE PATIENT
        public async Task DeletePatient([FromBody] DateTime date)
        {
            var patient = await _context.MedSestriPatients
                .FirstOrDefaultAsync(p => p.Date == date);

            var existingRelations = await _context.MedSestriPatientsBloodTests
                .Where(x => x.PatientId == patient!.Id)
                .ToListAsync();

            _context.MedSestriPatientsBloodTests.RemoveRange(existingRelations);
            await _context.SaveChangesAsync();

            _context.MedSestriPatients.Remove(patient!);
            await _context.SaveChangesAsync();
        }
    }
}
