using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;

namespace TherapyCenter.Tests.Services
{
    public class SlotServiceTests
    {
        private readonly Mock<ISlotRepository> _slotRepoMock = new();
        private readonly ISlotService _slotService;

        public SlotServiceTests()
        {
            _slotService = new SlotService(_slotRepoMock.Object);
        }

        // ────────────────────────────────────────────────────────────────
        // GetAvailableAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAvailableAsync_ReturnsAvailableSlots()
        {
            // Arrange
            var date = new DateOnly(2026, 6, 15);
            var slots = new List<Slot>
            {
                new() { SlotId = 1, DoctorId = 1, Date = date, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsBooked = false },
                new() { SlotId = 2, DoctorId = 1, Date = date, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsBooked = false },
                new() { SlotId = 3, DoctorId = 1, Date = date, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 0), IsBooked = false }
            };

            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date)).ReturnsAsync(slots);

            // Act
            var result = await _slotService.GetAvailableAsync(1, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            _slotRepoMock.Verify(x => x.GetAvailableSlotsByDoctorAsync(1, date), Times.Once);
        }

        [Fact]
        public async Task GetAvailableAsync_WithNoSlots_ReturnsEmptyList()
        {
            // Arrange
            var date = new DateOnly(2026, 6, 15);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date)).ReturnsAsync(new List<Slot>());

            // Act
            var result = await _slotService.GetAvailableAsync(1, date);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableAsync_WithDifferentDoctor_ReturnsEmptyList()
        {
            // Arrange
            var date = new DateOnly(2026, 6, 15);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(999, date)).ReturnsAsync(new List<Slot>());

            // Act
            var result = await _slotService.GetAvailableAsync(999, date);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAvailableAsync_WithDifferentDate_ReturnsDifferentSlots()
        {
            // Arrange
            var date1 = new DateOnly(2026, 6, 15);
            var date2 = new DateOnly(2026, 6, 16);

            var slotsDate1 = new List<Slot>
            {
                new() { SlotId = 1, DoctorId = 1, Date = date1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsBooked = false }
            };

            var slotsDate2 = new List<Slot>
            {
                new() { SlotId = 2, DoctorId = 1, Date = date2, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(15, 0), IsBooked = false }
            };

            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date1)).ReturnsAsync(slotsDate1);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date2)).ReturnsAsync(slotsDate2);

            // Act
            var result1 = await _slotService.GetAvailableAsync(1, date1);
            var result2 = await _slotService.GetAvailableAsync(1, date2);

            // Assert
            Assert.Single(result1);
            Assert.Equal(new TimeOnly(9, 0), result1.First().StartTime);
            Assert.Single(result2);
            Assert.Equal(new TimeOnly(14, 0), result2.First().StartTime);
        }
    }
}