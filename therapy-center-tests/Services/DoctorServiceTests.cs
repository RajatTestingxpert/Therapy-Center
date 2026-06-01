using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;

namespace TherapyCenter.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
        private readonly Mock<ISlotRepository> _slotRepoMock = new();
        private readonly IDoctorService _doctorService;

        public DoctorServiceTests()
        {
            _doctorService = new DoctorService(_doctorRepoMock.Object, _slotRepoMock.Object);
        }

        private Doctor CreateDoctor(int id, string first, string last, string spec) => new()
        {
            DoctorId = id, UserId = id, Specialization = spec,
            User = new User { UserId = id, FirstName = first, LastName = last, Email = $"{first}@clinic.com", Role = "Doctor" }
        };

        [Fact] public async Task GetAllAsync_ReturnsAll()
        {
            _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Doctor> { CreateDoctor(1, "John", "Doe", "Physio"), CreateDoctor(2, "Jane", "Smith", "Speech") });
            Assert.Equal(2, (await _doctorService.GetAllAsync()).Count());
        }

        [Fact] public async Task GetAllAsync_WhenEmpty_ReturnsEmpty()
        {
            _doctorRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Doctor>());
            Assert.Empty(await _doctorService.GetAllAsync());
        }

        [Fact] public async Task GetByIdAsync_ReturnsDoctor()
        {
            _doctorRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreateDoctor(1, "John", "Doe", "Physio"));
            var result = await _doctorService.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("John Doe", result!.FullName);
        }

        [Fact] public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            _doctorRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);
            Assert.Null(await _doctorService.GetByIdAsync(999));
        }

        [Fact] public async Task GetAvailableSlotsAsync_ReturnsSlots()
        {
            var date = new DateOnly(2026, 6, 15);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date)).ReturnsAsync(new List<Slot> { new() { SlotId = 1, DoctorId = 1, Date = date, StartTime = new TimeOnly(9, 0) } });
            Assert.Single(await _doctorService.GetAvailableSlotsAsync(1, date));
        }

        [Fact] public async Task GetAvailableSlotsAsync_WhenNone_ReturnsEmpty()
        {
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, new DateOnly(2026, 6, 15))).ReturnsAsync(new List<Slot>());
            Assert.Empty(await _doctorService.GetAvailableSlotsAsync(1, new DateOnly(2026, 6, 15)));
        }
    }
}