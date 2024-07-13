

namespace Infrastructure.Actions.Unit.Tests
{
    public class EmployeeActionsTests
    {
        private readonly IEmployeeActions employeeActions;
        private readonly Mock<ITimeSummaryRepository> mockTimeSummary = new();
        private readonly Mock<IRepository<EmployeeDetails>> mockRepoEmployeeDetails = new();
        private readonly Mock<IRepository<CompanyEmployee>> mockRepoCompanyEmployees = new();
        private readonly Mock<IRepository<Company>> mockRepoCompanies = new();
        private readonly Mock<IDateTimeProvider> mockDateTimeProvider = new();
        private readonly Mock<IRepository<Loan>> mockRepoLoans = new();
        private readonly Mock<IRepository<Leave>> mockRepoLeaves = new();
        private readonly Mock<IRepository<History>> mockRepoHistories = new();
        private readonly Mock<IPasswordManager> mockPasswordManager = new();
        private readonly Mock<IValidator<ShiftValidationData, ShiftValidationResult>> mockValidator = new();
        private readonly Mock<ILogger<EmployeeActions>> mockLogger = new();

        public EmployeeActionsTests()
        {
            employeeActions = new EmployeeActions(
                mockTimeSummary.Object,
                mockRepoEmployeeDetails.Object,
                mockRepoCompanyEmployees.Object,
                mockRepoCompanies.Object,
                mockDateTimeProvider.Object,
                mockRepoLoans.Object,
                mockRepoLeaves.Object,
                mockRepoHistories.Object,
                mockPasswordManager.Object,
                mockValidator.Object,
                mockLogger.Object);
        }
        [Fact()]
        public async Task ClockIn_Should_Clock_In_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8,0),
                ShiftEndTime = new TimeOnly(8,0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate>{ rate },
                Shifts = new List<Shift>{ shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It","backend",company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            EmployeeTask employeeTask = new()
            {
                Rate = rate,
                Shift = shift,
                RateType = RateType.Standard,
                TaskName = "backend development"
            };
            var dateTimeProvide = mockDateTimeProvider.Object;
            companyEmployee.WeekDays[dateTimeProvide.DayOfWeekString(now)] = new List<EmployeeTask> { employeeTask };
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var clockInData = new ClockInAndOutData
            {
                QrCode = new CompanyQrCode { CompanyId = company.Id.ToString() }.Encode(),
                EmployeePosition = location,
                TimeZoneId = timezoneInfo.Id
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().BeNullOrWhiteSpace();
            result.Message.Should().Be("Clocked In");
            result.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact()]
        public async Task ClockIn_Should_Fail_No_TimeZoneInfo_in_Clockin_Data_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m,
                SaturdaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8,0),
                ShiftEndTime = new TimeOnly(8, 0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate> { rate },
                Shifts = new List<Shift> { shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It", "backend", company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            EmployeeTask employeeTask = new()
            {
                Rate = rate,
                Shift = shift,
                RateType = RateType.Standard,
                TaskName = "backend development"
            };
            var dateTimeProvide = mockDateTimeProvider.Object;
            companyEmployee.WeekDays[dateTimeProvide.DayOfWeekString(now)] = new List<EmployeeTask> { employeeTask };
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var clockInData = new ClockInAndOutData
            {
                QrCode = new CompanyQrCode { CompanyId = company.Id.ToString() }.Encode(),
                EmployeePosition = location
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().Be("Something is wrong with your request, use the supported tools to clock in!");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Fact()]
        public async Task ClockIn_Should_Fail_No_QrCode_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8, 0),
                ShiftEndTime = new TimeOnly(8, 0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate> { rate },
                Shifts = new List<Shift> { shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It", "backend", company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            EmployeeTask employeeTask = new()
            {
                Rate = rate,
                Shift = shift,
                RateType = RateType.Standard,
                TaskName = "backend development"
            };
            var dateTimeProvide = mockDateTimeProvider.Object;
            companyEmployee.WeekDays[dateTimeProvide.DayOfWeekString(now)] = new List<EmployeeTask> { employeeTask };
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var clockInData = new ClockInAndOutData
            {
                QrCode = string.Empty,
                EmployeePosition = location,
                TimeZoneId = timezoneInfo.Id
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().Be("Please use a valid company qr-code!");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Fact()]
        public async Task ClockIn_Should_Fail_Invalid_QrCode_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8, 0),
                ShiftEndTime = new TimeOnly(8, 0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate> { rate },
                Shifts = new List<Shift> { shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It", "backend", company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            EmployeeTask employeeTask = new()
            {
                Rate = rate,
                Shift = shift,
                RateType = RateType.Standard,
                TaskName = "backend development"
            };
            var dateTimeProvide = mockDateTimeProvider.Object;
            companyEmployee.WeekDays[dateTimeProvide.DayOfWeekString(now)] = new List<EmployeeTask> { employeeTask };
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var clockInData = new ClockInAndOutData
            {
                QrCode = new CompanyQrCode{ CompanyId = string.Empty }.Encode(),
                EmployeePosition = location,
                TimeZoneId = timezoneInfo.Id
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().Be("Invalid Qr-code!!");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Fact()]
        public async Task ClockIn_Should_Fail_Not_Within_Tolerance_Radius_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8, 0),
                ShiftEndTime = new TimeOnly(8, 0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate> { rate },
                Shifts = new List<Shift> { shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It", "backend", company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            EmployeeTask employeeTask = new()
            {
                Rate = rate,
                Shift = shift,
                RateType = RateType.Standard,
                TaskName = "backend development"
            };
            var dateTimeProvide = mockDateTimeProvider.Object;
            companyEmployee.WeekDays[dateTimeProvide.DayOfWeekString(now)] = new List<EmployeeTask> { employeeTask };
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var raduisTolerance = StellaConfig.CLOCK_IN_OUT_RADIUS_IN_METERS;

            var clockInData = new ClockInAndOutData
            {
                QrCode = new CompanyQrCode { CompanyId = company.Id.ToString() }.Encode(),
                EmployeePosition = location with { Latitude = location.Latitude + raduisTolerance * raduisTolerance, Longitude = location.Longitude + raduisTolerance * raduisTolerance },
                TimeZoneId = timezoneInfo.Id
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().Be($"You are too far away from work. You need to be within {raduisTolerance:f2} meters of your work location to clock in!");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Fact()]
        public async Task ClockIn_Should_Fail_Not_On_Timetable_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8, 0),
                ShiftEndTime = new TimeOnly(8, 0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate> { rate },
                Shifts = new List<Shift> { shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It", "backend", company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var raduisTolerance = StellaConfig.CLOCK_IN_OUT_RADIUS_IN_METERS;

            var clockInData = new ClockInAndOutData
            {
                QrCode = new CompanyQrCode { CompanyId = company.Id.ToString() }.Encode(),
                EmployeePosition = location,
                TimeZoneId = timezoneInfo.Id
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().Be("You are not on shift today. Check with your HR to be added.");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Fact()]
        public async Task ClockIn_Should_Fail_No_Task_For_Shift_Test()
        {
            var now = DateTime.Parse("2021-08-21T17:00:00.00Z");
            mockDateTimeProvider.SetupGet(x => x.Now)
                .Returns(now);
            var timezoneInfo = TimeZoneInfo.Local;

            var location = new Position()
            {
                Latitude = 10,
                Longitude = 10
            };

            var position = "backend";
            Rate rate = new()
            {
                NameOfPosition = position,
                OverTimeRate = 30m,
                PublicHolidaysRate = 35m,
                StandardDaysRate = 22m,
                SundaysRate = 35m
            };

            Shift shift = new()
            {
                ShiftStartTime = new TimeOnly(8, 0),
                ShiftEndTime = new TimeOnly(8, 0).Add(TimeSpan.FromHours(8)),
                Name = "AM"
            };

            var department = "It";

            var company = new Company
            {
                Address = new()
                {
                    Location = location
                },
                Rates = new List<Rate> { rate },
                Shifts = new List<Shift> { shift },
                Departments = new List<string> { department }
            };

            mockRepoCompanies.Setup(x => x.FindById(company.Id.ToString()))
                .Returns(Task.FromResult(company));

            var employeeId = "210821LI8Z7SL";
            var employee = new EmployeeDetails()
            {
                EmployeeId = employeeId
            };

            var companyEmployee = employee.CreateCompanyEmployeeFrom("It", "backend", company.Id);
            companyEmployee.Department = department;
            companyEmployee.Position = position;
            var dateTimeProvide = mockDateTimeProvider.Object;
            companyEmployee.WeekDays[dateTimeProvide.DayOfWeekString(now)] = new List<EmployeeTask>();
            employeeId = employeeId.ToLowerInvariant();
            mockRepoCompanyEmployees.Setup(y => y.FindOne(x => x.CompanyId == company.Id && x.EmployeeId.ToLowerInvariant() == employeeId))
                .Returns(Task.FromResult(companyEmployee));

            var timeSummary = new TimeSummary
            {
                EmployeeDetailsId = employee.Id,
                EmployeeId = employee.EmployeeId,
                CompanyId = company.Id,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };
            mockTimeSummary.Setup(x => x.TimeSummaryByEmployeeDetailsAndCompanyId(employee.Id.ToString(), company.Id.ToString()))
                .Returns(Task.FromResult(timeSummary));
            mockTimeSummary.Setup(x => x.Upsert(timeSummary.Id.ToString(), timeSummary))
                .Returns(Task.FromResult(timeSummary));

            var raduisTolerance = StellaConfig.CLOCK_IN_OUT_RADIUS_IN_METERS;

            var clockInData = new ClockInAndOutData
            {
                QrCode = new CompanyQrCode { CompanyId = company.Id.ToString() }.Encode(),
                EmployeePosition = location,
                TimeZoneId = timezoneInfo.Id
            };

            var result = await employeeActions.ClockIn(clockInData, employee.EmployeeId, employee.Id.ToString());

            result.Should().BeOfType<Response>();
            result.Error.Should().Be("You are not on shift today. Check with your HR to be added.");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }
}