using BackendServices;
using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Exceptions;
using BackendServices.Models;
using BackendServices.Models.RealTime;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PrePurchase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Actions.Admin.RealTime
{
    public class RealTimeDashBoardUpdate : IRealTimeDashBoardUpdate
    {
        private readonly IHubContext<DashboardUIHub> _dashboardHubContext;
        private readonly ITimeSummaryRepository _timeSummaries;
        private readonly IRepository<CompanyEmployee> _companyEmployees;
        private readonly ILogger<RealTimeDashBoardUpdate> _logger;

        public RealTimeDashBoardUpdate(IHubContext<DashboardUIHub> dashboardHubContext, IRepository<CompanyEmployee> companyEmployees, ITimeSummaryRepository timeSummaries, ILogger<RealTimeDashBoardUpdate> logger)
        {
            _dashboardHubContext = dashboardHubContext;
            _companyEmployees = companyEmployees;
            _timeSummaries = timeSummaries;
            _logger = logger;
        }

        public async Task<DashboardUIData> GetDashboardUIData(string companyId)
        {
            DashboardUIData dashboard = await FetchDashboardData(companyId); // Method to prepare dashboard data

            return dashboard;
        }

        public async void OnClockingAction(string companyId)
        {
            DashboardUIData dashboard = await FetchDashboardData(companyId); // Method to prepare dashboard data

            // Assuming "_hubContext" is properly initialized and injected
            await _dashboardHubContext.Clients.Group(companyId).SendAsync("dashboardUI", dashboard);
        }



        private async Task<DashboardUIData> FetchDashboardData(string companyId)
        {
            try
            {
                List<TimeSummary> times = await _timeSummaries.Find(x => x.CompanyId == ObjectId.Parse(companyId)).ToListAsync();
                List<CompanyEmployee> employees = await _companyEmployees.Find(x => x.CompanyId == ObjectId.Parse(companyId)).ToListAsync();

                Dictionary<(string EmployeeId, ObjectId CompanyId), CompanyEmployee> employeeDict = employees.ToDictionary(emp => (emp.EmployeeId, emp.CompanyId));

                List<EmployeeDataAndTimes> empList = times.Select(timeSummary => new EmployeeDataAndTimes
                {
                    ClockData = timeSummary.Clocks,
                    CompanyEmployee = employeeDict.TryGetValue((timeSummary.EmployeeId, timeSummary.CompanyId), out var companyEmployee)
                        ? companyEmployee
                        : null
                }).ToList();

                List<string> allDepartments = employees.Select(emp => emp.Department).Distinct().ToList();

                List<StaffOverview> employeeOnShift = allDepartments
                    .Select(department => new StaffOverview
                    {
                        DepartmentName = department,
                        TotalStaffOn = empList.Count(t => t.CompanyEmployee?.Department == department &&
                                                          t.ClockData != null &&
                                                          t.ClockData.Any(clock =>
                                                              clock.ClockIn.Date == DateTime.Today &&
                                                              clock.ClockOut == null)),
                        StaffNamesOn = empList.Where(t => t.CompanyEmployee?.Department == department &&
                                                          t.ClockData != null &&
                                                          t.ClockData.Any(clock =>
                                                              clock.ClockIn.Date == DateTime.Today &&
                                                              clock.ClockOut == null))
                                              .Select(t => $"{t.CompanyEmployee?.Name} {t.CompanyEmployee?.Surname}")
                                              .ToList()
                    })
                    .OrderByDescending(staff => staff.TotalStaffOn > 0) // Sort departments with staff on shift first
                    .ToList();

                List<StaffOverview> employeeOnBreak = allDepartments
                    .Select(department => new StaffOverview
                    {
                        DepartmentName = department,
                        TotalStaffOn = empList.Count(t => t.CompanyEmployee?.Department == department &&
                                                          t.ClockData != null &&
                                                          t.ClockData.Any(clock =>
                                                              clock.ClockIn.Date == DateTime.Today &&
                                                              clock.RestTimes != null &&
                                                              clock.RestTimes.Any(restTime =>
                                                                  restTime.StartTime.Date == DateTime.Today &&
                                                                  restTime.EndTime == null))),
                        StaffNamesOn = empList.Where(t => t.CompanyEmployee?.Department == department &&
                                                          t.ClockData != null &&
                                                          t.ClockData.Any(clock =>
                                                              clock.ClockIn.Date == DateTime.Today &&
                                                              clock.RestTimes != null &&
                                                              clock.RestTimes.Any(restTime =>
                                                                  restTime.StartTime.Date == DateTime.Today &&
                                                                  restTime.EndTime == null)))
                                              .Select(t => $"{t.CompanyEmployee?.Name} {t.CompanyEmployee?.Surname}")
                                              .ToList()
                    })
                    .OrderByDescending(staff => staff.TotalStaffOn > 0) // Sort departments with staff on break first
                    .ToList();

                string staffOnSitePercentage = "0.00";
                if (times.Any(x => x.Clocks.Any())) staffOnSitePercentage = (employeeOnShift.Count * 100.0 / times.Count(x => x.Clocks.Any())).ToString("0.00");

                string staffOnBreakPercentage = "0.00";
                if (employeeOnShift.Any()) staffOnBreakPercentage = (employeeOnBreak.Count * 100.0 / employeeOnShift.Count).ToString("0.00");

                decimal averageCost = empList.Sum(x => x.ClockData.Sum(clock => clock.Amount));

                DashboardUIData dashboard = new()
                {
                    StaffOnShift = employeeOnShift,
                    StaffOnBreak = employeeOnBreak,
                    StaffOnShiftPercentage = staffOnSitePercentage,
                    StaffOnBreakPercentage = staffOnBreakPercentage,
                    EmployeeDataAndTimes = empList,
                    StaffWorked = empList.Count(x => x.ClockData.Any()),
                    AverageCost = averageCost
                };

                return dashboard;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }


    }
}
