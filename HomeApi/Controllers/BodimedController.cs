using DataLayer;
using DataLayer.Models;
using DataLayer.Models.DTOs;
using Extensions;
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
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        [HttpGet("allPatientsHistory")] // SEND ALL PATIENTs 
        public async Task<List<MedSestriPatientsDTO>> GetAllPatientsHistoryAsync()
        {
            var patients = await _context.MedSestriPatients
                .OrderByDescending(p => p.Date)
                .Include(p => p.PatientBloodTests)
                .ThenInclude(pt => pt.BloodTest)
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
                })
                .ToList()
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
                WriteLog.Log($"Patient {model.FullName} updated with new blood tests.");
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
                WriteLog.Log($"New patient {model.FullName} created with blood tests.");
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
            WriteLog.Log($"Patient {patient!.FullName} deleted.");
        }

        [HttpPost("createCatheterAppointment")]
        public async Task CreateCatheterAppointment([FromBody] MedSestriCatheter model)
        {
            await _context.MedSestriCatheters.AddAsync(model);
            await _context.SaveChangesAsync();
            WriteLog.Log($"New catheter appointment for {model.ClientName} on {model.Date} created.");
        }

        [HttpGet("getAllCatheterAppointments")]
        public async Task<List<MedSestriCatheter>> GetAllCatheterAppointments()
        {
            var result = await _context.MedSestriCatheters
                .Where(p => p.IsChecked == false)
                .OrderBy(d => d.Date)
                .ToListAsync();

            return result;
        }

        [HttpPut("checkCatheterAppointment")]
        public async Task CheckCatheterAppointment([FromBody] MedSestriCatheter model)
        {
            var entity = _context.MedSestriCatheters
                .FirstOrDefault(c => c.Id == model.Id);

            entity.IsChecked = true;
            await _context.SaveChangesAsync();
            WriteLog.Log($"Catheter appointment for {model.ClientName} on {model.Date} checked.");
        }

        [HttpPut("updateCatheterAppointment")]
        public async Task UpdateCatheterAppointment([FromBody] MedSestriCatheter model)
        {
            var entity = _context.MedSestriCatheters
                .FirstOrDefault(c => c.Id == model.Id);

            entity.ClientName = model.ClientName;
            entity.PhoneNumber = model.PhoneNumber;
            entity.Date = model.Date;
            entity.Address = model.Address;
            await _context.SaveChangesAsync();
        }
    }
}
