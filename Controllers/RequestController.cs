using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using carma.Models;
using carma.Persistence;
using carma.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace carma.Controllers
{

    [Route("api/request")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        public CarmaDbContext context { get; }
        private readonly IMapper mapper;
        public IConfiguration _config { get; }
        public RequestController(CarmaDbContext context, IMapper mapper, IConfiguration config)
        {
            this._config = config;
            this.mapper = mapper;
            this.context = context;

        }
        [HttpGet]
        public async Task<IEnumerable<RequestResource>> GetRequests(string? dateFrom = null, string? dateTo = null)
        {
            IQueryable<Request> query = context.Requests.Include(r => r.Vehicle);

            if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                query = query.Where(r => r.DateOfRequest >= fromDate);
            }

            if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                query = query.Where(r => r.DateOfRequest <= toDate);
            }

            var requests = await query.ToListAsync();

            return mapper.Map<IEnumerable<Request>, IEnumerable<RequestResource>>(requests);
        }
        [HttpGet("departmentSummary")]
        public async Task<IEnumerable<DepartmentSummaryResource>> GetDepartmentSummary(string? dateFrom = null, string? dateTo = null)
        {
            IQueryable<Request> query = context.Requests.Include(r => r.Vehicle);

            if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                query = query.Where(r => r.DateFrom >= fromDate);
            }

            if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                query = query.Where(r => r.DateTo <= toDate);
            }

            var requests = await query.ToListAsync();

            var departmentSummaries = requests
                .GroupBy(r => r.DepartmentId)
                .Select(group => new DepartmentSummaryResource
                {
                    departmentId = group.Key,
                    totalKm = group.Sum(r => r.TotalKm)
                })
                .ToList();

            return departmentSummaries;
        }
        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] SaveRequestResource requestResource)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is Invalid");

            // Validate start and end dates
            if (requestResource.DateFrom >= requestResource.DateTo)
                return BadRequest("End date must be after start date");

            var vehicle = await context.Vehicles.FindAsync(requestResource.VehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found");

            // Check if the vehicle is already out for the given time period
            var existingRequests = await context.Requests
      .Where(r =>
          r.VehicleId == requestResource.VehicleId &&
         (r.Status == "Potvrzeno" || r.Status == "Vydáno") &&
        (
        (r.DateFrom <= requestResource.DateTo && r.DateTo >= requestResource.DateTo) ||
        (r.DateFrom <= requestResource.DateFrom && r.DateTo >= requestResource.DateFrom)
        ))
      .ToListAsync();


            if (existingRequests.Any())
                return BadRequest("Pro tento čas je vozidlo již rezervováno");

            var request = mapper.Map<SaveRequestResource, Request>(requestResource);
            request.DateOfRequest = DateTime.Now;
            context.Requests.Add(request);
            await context.SaveChangesAsync();
            return Ok(request);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest([FromBody] SaveRequestResource requestResource, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model is Invalid");

            var requestFromDb = context.Requests.Find(id);
            if (requestFromDb == null)
                return NotFound();
            mapper.Map<SaveRequestResource, Request>(requestResource, requestFromDb);
            await context.SaveChangesAsync();
            return Ok(requestFromDb);
        }
        [HttpGet("getavaible/{vehicleId}")]
        public async Task<IActionResult> ReturnStatusDates(int vehicleId, string? dateFrom, string? dateTo)
        {
            if (dateFrom == null || dateTo == null)
            {
                dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
                dateTo = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
            }
            var dateFromParsed = DateTime.Parse(dateFrom);
            var dateToParsed = DateTime.Parse(dateTo);
            IQueryable<Request> query = context.Requests.Include(r => r.Vehicle);

            if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                query = query.Where(r => r.DateFrom >= fromDate);
            }

            if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                query = query.Where(r => r.DateTo <= toDate);
            }

            var requests = await query.ToListAsync();

            var reservedDays = requests
                .Where(r => r.VehicleId == vehicleId)
                .SelectMany(r => GetReservedDaysWithStatus(r.DateFrom, r.DateTo, "booked"))
                .ToList();

            var allDays = GetReservedDaysWithStatus(dateFromParsed, dateToParsed, "free");
            var allDaysList = new List<ReservationDay>();
            foreach (var day in allDays)
            {
                if (reservedDays.Any(r => r.date == day.date))
                {
                    allDaysList.Add(new ReservationDay { date = day.date, status = "booked" });
                }
                else
                {
                    allDaysList.Add(new ReservationDay { date = day.date, status = "free" });
                }
            }
            return Ok(allDaysList);
        }

        private IEnumerable<ReservationDay> GetReservedDaysWithStatus(DateTime startDate, DateTime endDate, string status)
        {
            List<ReservationDay> daysWithStatus = new List<ReservationDay>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                daysWithStatus.Add(new ReservationDay { date = date, status = status });
            }

            return daysWithStatus;
        }
    }
}