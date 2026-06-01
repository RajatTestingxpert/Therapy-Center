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

        public PaymentServiceTests()
        {
            _paymentService = new PaymentService(_paymentRepoMock.Object);
        }

        // ────────────────────────────────────────────────────────────────
        // RecordPaymentAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RecordPaymentAsync_WithoutTransactionId_CreatesPendingPayment()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                AppointmentId = 1,
                Amount = 150.00m,
                PaymentMethod = "Cash",
                TransactionId = null
            };

            _paymentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) =>
                {
                    p.PaymentId = 1;
                    return p;
                });

            // Act
            var result = await _paymentService.RecordPaymentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PaymentId);
            Assert.Equal(150.00m, result.Amount);
            Assert.Equal("Cash", result.PaymentMethod);
            Assert.Equal("Pending", result.Status);
            Assert.Null(result.PaidAt);
            _paymentRepoMock.Verify(x => x.CreateAsync(It.Is<Payment>(p =>
                p.Status == "Pending" && p.PaidAt == null)), Times.Once);
        }

        [Fact]
        public async Task RecordPaymentAsync_WithTransactionId_CreatesPaidPayment()
        {
            // Arrange
            var request = new RecordPaymentRequest
            {
                AppointmentId = 1,
                Amount = 200.00m,
                PaymentMethod = "CreditCard",
                TransactionId = "TXN123456"
            };

            _paymentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) =>
                {
                    p.PaymentId = 2;
                    return p;
                });

            // Act
            var result = await _paymentService.RecordPaymentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Paid", result.Status);
            Assert.Equal("TXN123456", result.TransactionId);
            Assert.NotNull(result.PaidAt);
            _paymentRepoMock.Verify(x => x.CreateAsync(It.Is<Payment>(p =>
                p.Status == "Paid" && p.TransactionId == "TXN123456")), Times.Once);
        }

        // ────────────────────────────────────────────────────────────────
        // MarkAsPaidAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task MarkAsPaidAsync_UpdatesPaymentStatus()
        {
            // Arrange
            var payment = new Payment
            {
                PaymentId = 1,
                AppointmentId = 1,
                Amount = 100.00m,
                Status = "Pending"
            };

            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(payment);
            _paymentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p);

            // Act
            var result = await _paymentService.MarkAsPaidAsync(1, "TXN999");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Paid", result.Status);
            Assert.Equal("TXN999", result.TransactionId);
            Assert.NotNull(result.PaidAt);
        }

        [Fact]
        public async Task MarkAsPaidAsync_WithNullTransactionId_StillMarksAsPaid()
        {
            // Arrange
            var payment = new Payment
            {
                PaymentId = 1,
                AppointmentId = 1,
                Amount = 100.00m,
                Status = "Pending"
            };

            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(payment);
            _paymentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p);

            // Act
            var result = await _paymentService.MarkAsPaidAsync(1, null);

            // Assert
            Assert.Equal("Paid", result.Status);
            Assert.Null(result.TransactionId);
            Assert.NotNull(result.PaidAt);
        }

        [Fact]
        public async Task MarkAsPaidAsync_WithNonexistentPayment_ThrowsKeyNotFound()
        {
            // Arrange
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(999)).ReturnsAsync((Payment?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _paymentService.MarkAsPaidAsync(999, null));
            Assert.Contains("Payment record not found", ex.Message);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByAppointmentAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByAppointmentAsync_ReturnsPayment_WhenExists()
        {
            // Arrange
            var payment = new Payment { PaymentId = 1, AppointmentId = 1, Amount = 100m, Status = "Paid" };
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(1)).ReturnsAsync(payment);

            // Act
            var result = await _paymentService.GetByAppointmentAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.PaymentId);
            Assert.Equal("Paid", result.Status);
        }

        [Fact]
        public async Task GetByAppointmentAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            _paymentRepoMock.Setup(x => x.GetByAppointmentIdAsync(999)).ReturnsAsync((Payment?)null);

            // Act
            var result = await _paymentService.GetByAppointmentAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ────────────────────────────────────────────────────────────────
        // GetByPatientAsync
        // ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByPatientAsync_ReturnsPayments()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new() { PaymentId = 1, AppointmentId = 1, Amount = 100m },
                new() { PaymentId = 2, AppointmentId = 2, Amount = 200m }
            };

            _paymentRepoMock.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(payments);

            // Act
            var result = await _paymentService.GetByPatientAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByPatientAsync_WhenNone_ReturnsEmptyList()
        {
            // Arrange
            _paymentRepoMock.Setup(x => x.GetByPatientIdAsync(999)).ReturnsAsync(new List<Payment>());

            // Act
            var result = await _paymentService.GetByPatientAsync(999);

            // Assert
            Assert.Empty(result);
        }
    }
}