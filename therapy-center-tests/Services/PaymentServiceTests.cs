using Moq;
using Xunit;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Entities;
using TherapyCenter.DTO_s.Payment;

namespace TherapyCenter.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
        private readonly IPaymentService _paymentService;

        public PaymentServiceTests() => _paymentService = new PaymentService(_paymentRepoMock.Object);

        [Fact] public async Task RecordPaymentAsync_WithoutTransactionId_CreatesPending()
        {
            var request = new RecordPaymentRequest { AppointmentId = 1, Amount = 150, PaymentMethod = "Cash" };
            _paymentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => { p.PaymentId = 1; return p; });
            var result = await _paymentService.RecordPaymentAsync(request);
            Assert.Equal("Pending", result.Status);
            Assert.Null(result.PaidAt);
        }

        [Fact] public async Task RecordPaymentAsync_WithTransactionId_CreatesPaid()
        {
            var request = new RecordPaymentRequest { AppointmentId = 1, Amount = 200, PaymentMethod = "CreditCard", TransactionId = "TXN123" };
            _paymentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => { p.PaymentId = 1; return p; });
            var result = await _paymentService.RecordPaymentAsync(request);
            Assert.Equal("Paid", result.Status);
            Assert.Equal("TXN123", result.TransactionId);
            Assert.NotNull(result.PaidAt);
        }

        [Fact] public async Task MarkAsPaidAsync_UpdatesStatus()
        {
            var payment = new Payment { PaymentId = 1, AppointmentId = 1, Amount = 100, Status = "Pending" };
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(payment);
            _paymentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
            var result = await _paymentService.MarkAsPaidAsync(1, "TXN999");
            Assert.Equal("Paid", result.Status);
            Assert.Equal("TXN999", result.TransactionId);
        }

        [Fact] public async Task MarkAsPaidAsync_Nonexistent_Throws()
        {
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(999)).ReturnsAsync((Payment?)null);
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentService.MarkAsPaidAsync(999, null));
            Assert.Contains("Payment record not found", ex.Message);
        }

        [Fact] public async Task GetByAppointmentAsync_ReturnsPayment()
        {
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(new Payment { PaymentId = 1, Status = "Paid" });
            Assert.NotNull(await _paymentService.GetByAppointmentAsync(1));
        }

        [Fact] public async Task GetByAppointmentAsync_ReturnsNull()
        {
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(999)).ReturnsAsync((Payment?)null);
            Assert.Null(await _paymentService.GetByAppointmentAsync(999));
        }

        [Fact] public async Task GetByPatientAsync_ReturnsPayments()
        {
            _paymentRepoMock.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new List<Payment> { new() { PaymentId = 1 }, new() { PaymentId = 2 } });
            Assert.Equal(2, (await _paymentService.GetByPatientAsync(1)).Count());
        }

        [Fact] public async Task GetByPatientAsync_WhenNone_ReturnsEmpty()
        {
            _paymentRepoMock.Setup(x => x.GetByPatientIdAsync(999)).ReturnsAsync(new List<Payment>());
            Assert.Empty(await _paymentService.GetByPatientAsync(999));
        }
    }
}