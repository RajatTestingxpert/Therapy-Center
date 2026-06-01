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

        public SlotServiceTests() => _slotService = new SlotService(_slotRepoMock.Object);

        [Fact]
        public async Task GetAvailableAsync_ReturnsAvailableSlots()
        {
            var date = new DateOnly(2026, 6, 15);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date)).ReturnsAsync(new List<Slot>
            {
                new() { SlotId = 1, DoctorId = 1, Date = date, StartTime = new TimeOnly(9, 0), IsBooked = false },
                new() { SlotId = 2, DoctorId = 1, Date = date, StartTime = new TimeOnly(10, 0), IsBooked = false }
            });
            Assert.Equal(2, (await _slotService.GetAvailableAsync(1, date)).Count());
        }

        [Fact]
        public async Task GetAvailableAsync_WithNoSlots_ReturnsEmpty()
        {
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, new DateOnly(2026, 6, 15))).ReturnsAsync(new List<Slot>());
            Assert.Empty(await _slotService.GetAvailableAsync(1, new DateOnly(2026, 6, 15)));
        }

        [Fact]
        public async Task GetAvailableAsync_WithDifferentDoctor_ReturnsEmpty()
        {
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(999, new DateOnly(2026, 6, 15))).ReturnsAsync(new List<Slot>());
            Assert.Empty(await _slotService.GetAvailableAsync(999, new DateOnly(2026, 6, 15)));
        }

        [Fact]
        public async Task GetAvailableAsync_DifferentDates_ReturnsDifferentSlots()
        {
            var date1 = new DateOnly(2026, 6, 15);
            var date2 = new DateOnly(2026, 6, 16);
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date1)).ReturnsAsync(new List<Slot> { new() { SlotId = 1, StartTime = new TimeOnly(9, 0) } });
            _slotRepoMock.Setup(x => x.GetAvailableSlotsByDoctorAsync(1, date2)).ReturnsAsync(new List<Slot> { new() { SlotId = 2, StartTime = new TimeOnly(14, 0) } });
            Assert.Equal(new TimeOnly(9, 0), (await _slotService.GetAvailableAsync(1, date1)).First().StartTime);
            Assert.Equal(new TimeOnly(14, 0), (await _slotService.GetAvailableAsync(1, date2)).First().StartTime);
        }
    }
}